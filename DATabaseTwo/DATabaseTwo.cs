using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class DATabaseTwo
	{
		// Private required variables
		private static string _datroot = "DATS";
		private static string _outroot = "Output";
		private static string _dbName = "dats.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";
		private static Logger _logger;

		public static void Main(string[] args)
		{
			Console.Clear();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			_logger = new Logger(true, "database2.log");
			_logger.Start();

			// Perform initial setup
			Setup();

			// If there's no arguments, show the menu
			if (args.Length == 0)
			{
				_logger.ToFile = true;
				ShowMainMenu();
				_logger.Close();
				return;
			}

			// Set all default values
			bool help = false,
				gen = false,
				genall = false,
				ignore = false,
				listsys = false,
				norename = false,
				old = false;
			string system = "";

			// Determine which switches are enabled (with values if necessary)
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-g":
					case "--generate":
						gen = true;
						break;
					case "-ga":
					case "--generate-all":
						genall = true;
						break;
					case "-i":
					case "--ignore":
						ignore = true;
						break;
					case "-lsy":
					case "--list-systems":
						listsys = true;
						break;
					case "-nr":
					case "--no-rename":
						norename = true;
						break;
					case "-o":
					case "--old":
						old = true;
						break;
					default:
						if (arg.StartsWith("-sys=") || arg.StartsWith("--system="))
						{
							system = arg.Split('=')[1];
						}
						else
						{
							_logger.Warning("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							_logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set or system is blank, show the help screen
			if (help || (system == "" && !listsys))
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// If we want a list of systems
			if (listsys)
			{
				ListSystems();
			}

			// If we want to generate all DATs
			else if (genall)
			{
				InitImport(ignore);
				InitGenerateAll(norename, old);
			}

			// If we want to generate a DAT
			else if (gen)
			{
				InitImport(ignore);
				InitGenerate(system, norename, old);
			}

			_logger.Close();
		}

		#region Menus

		/// <summary>
		/// Show the text-based main menu
		/// </summary>
		private static void ShowMainMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "x")
			{
				Console.Clear();
				Build.Start("DATabaseTwo");
				Console.WriteLine(@"MAIN MENU
===========================
Make a selection:

    1) Show command line usage
    2) Check for new or changed DATs
    3) Generate System DATs
    4) List all available systems
    5) Show credits
    X) Exit Program
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();

				switch (selection)
				{
					case "1":
						Console.Clear();
						Build.Help();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "2":
						ImportMenu();
						break;
					case "3":
						GenerateMenu();
						break;
					case "4":
						Console.Clear();
						Build.Start("DATabaseTwo");
						ListSystems();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "5":
						Console.Clear();
						Build.Credits();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based import menu
		/// </summary>
		private static void ImportMenu()
		{
			string selection = "";
			bool ignore = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabaseTwo");
				Console.WriteLine(@"IMPORT MENU
===========================
Make a selection:

    1) " + (ignore ? "Enable new source prompt" : "Disable new source prompt") + @"
    2) Begin import process
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						ignore = !ignore;
						break;
					case "2":
						Console.Clear();
						InitImport(ignore);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						ignore = false;
						break;
				}
			}
			return;
		}

		/// <summary>
		/// Show the text-based generate menu
		/// </summary>
		private static void GenerateMenu()
		{
			string selection = "", system = "";
			bool norename = false, old = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabaseTwo");
				Console.WriteLine(@"GENERATE MENU
===========================
Make a selection:

    1) " + (norename ? "Enable game renaming" : "Disable game renaming") + @"
    2) " + (old ? "Enable XML output" : "Enable ClrMamePro output") + @"
    3) System ID to generate from" + (system != "" ? ": " + system : "") + @"
    4) Generate the DAT file for the specified system
    5) Generate all DAT files
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						norename = !norename;
						break;
					case "2":
						old = !old;
						break;
					case "3":
						Console.Clear();
						ListSystems();
						Console.Write("Please enter the System ID: ");
						system = Console.ReadLine();
						break;
					case "4":
						Console.Clear();
						InitGenerate(system, norename, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						system = "";
						norename = false; old = false;
						break;
					case "5":
						Console.Clear();
						InitGenerateAll(norename, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						system = "";
						norename = false; old = false;
						break;
				}
			}
			return;
		}

		#endregion

		#region Function Methods

		/// <summary>
		/// Wrap importing and updating DATs
		/// </summary>
		/// <param name="ignore"></param>
		private static void InitImport(bool ignore)
		{
			Import imp = new Import(_datroot, _connectionString, _logger, ignore);
			imp.ImportData();
		}

		/// <summary>
		/// Wrap generating a DAT from the library
		/// </summary>
		/// <param name="system">System ID to be used in the DAT (blank means all)</param>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in ClrMamePro format (default false)</param>
		private static void InitGenerate(string systemid, bool norename, bool old)
		{
			Generate gen = new Generate(systemid, "" /* sourceid */, _datroot, _outroot, _connectionString, _logger, norename, old);
			gen.Export();
		}

		/// <summary>
		/// Wrap generating all standard DATs from the library
		/// </summary>
		private static void InitGenerateAll(bool norename, bool old)
		{
			List<string> systems = new List<string>();
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				string query = "SELECT id FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							_logger.Warning("No systems found! Please add a system and then try again.");
							return;
						}

						while (sldr.Read())
						{
							systems.Add(sldr.GetInt32(0).ToString());
						}
					}
				}

				// Loop through the inputs
				foreach (string system in systems)
				{
					InitGenerate(system, norename, old);
				}
			}
		}

		/// <summary>
		/// List systems in the database
		/// </summary>
		private static void ListSystems()
		{
			string query = @"
SELECT DISTINCT system.id, system.manufacturer, system.name
FROM system
ORDER BY system.manufacturer, system.name";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							_logger.Warning("No systems found! Please add a system and then try again.");
							return;
						}

						Console.WriteLine("Available Systems (id <= name):\n");
						while (sldr.Read())
						{
							Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1) + " - " + sldr.GetString(2));
						}
					}
				}
			}
			return;
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Perform initial setup for the program
		/// </summary>
		private static void Setup()
		{
			Remapping.CreateRemappings();
			Build.Start("DATabaseTwo");

			// Perform initial database and folder setup
			if (!Directory.Exists(_datroot))
			{
				Directory.CreateDirectory(_datroot);
			}
			if (!Directory.Exists(_outroot))
			{
				Directory.CreateDirectory(_outroot);
			}
			DBTools.EnsureDatabase(_dbName, _connectionString);

			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				string query = "SELECT * FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							int systemid = sldr.GetInt32(0);
							string system = _datroot + Path.DirectorySeparatorChar + sldr.GetString(1) + " - " + sldr.GetString(2);
							system = system.Trim();

							if (!Directory.Exists(system))
							{
								Directory.CreateDirectory(system);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
