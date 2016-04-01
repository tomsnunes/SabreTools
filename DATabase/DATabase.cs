using System;
using System.Data.SQLite;
using System.IO;
using System.Xml.Linq;

using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the DATabase application
	/// </summary>
	class DATabase
	{
		private static Logger logger;
		private static string _dbName = "DATabase.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";
		private static string _version = "0.2.6.1";
		private static string _header =
@"+-----------------------------------------------------------------------------+
|                             DATabase " + _version + @"                                |
|                                                                             |
|                 Programming: Matt Nadareski (darksabre76)                   |
|                            Testing: @tractivo                               |
+-----------------------------------------------------------------------------+
";

		/// <summary>
		/// Start menu or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			logger = new Logger(false, "database.log");
			logger.Start();
			DBTools.EnsureDatabase(_dbName, _connectionString);
			Remapping.CreateRemappings();

			Console.Clear();
			Console.SetBufferSize(Console.BufferWidth, 999);
			Console.Title = "DATabase " + _version;

			// If there's no arguments, show the menu
			if (args.Length == 0)
			{
				ShowMainMenu();
				logger.Close();
				return;
			}

			// Determine which switches are enabled (with values if necessary)
			bool help = false, import = false, generate = false, convert = false,
				listsys = false, listsrc = false, norename = false, old = false,
				log = false, genall = false, add = false, rem = false, skip = false;
			string systems = "", sources = "", input = "", manu = "", url = "", outdir = "";
			foreach (string arg in args)
			{
				// Main functions
				help = help || (arg == "-h" || arg == "-?" || arg == "--help");
				import = import || (arg == "-i" || arg == "--import");
				generate = generate || (arg == "-g" || arg == "--generate");
				genall = genall || (arg == "-ga" || arg == "--generate-all");
				convert = convert || (arg == "-c" || arg == "--convert");
				listsys = listsys || (arg == "-lsy" || arg == "--list-systems");
				listsrc = listsrc || (arg == "-lso" || arg == "--list-sources");
				add = add || (arg == "-a" || arg == "--add");
				rem = rem || (arg == "-r" || arg == "--remove");

				// Switches
				log = log || (arg == "-l" || arg == "--log");
				old = old || (arg == "-old" || arg == "--romvault");
				norename = norename || (arg == "-nr" || arg == "--no-rename");
				skip = skip || (arg == "--skip");
				
				// User input strings
				systems = (arg.StartsWith("system=") && systems == "" ? arg.Split('=')[1] : systems);
				sources = (arg.StartsWith("source=") && sources == "" ? arg.Split('=')[1] : sources);
				outdir = (arg.StartsWith("out=") && outdir == "" ? arg.Split('=')[1] : outdir);
				manu = (arg.StartsWith("manu=") && manu == "" ? arg.Split('=')[1] : manu);
				url = (arg.StartsWith("url=") && url == "" ? arg.Split('=')[1] : url);

				// Take care of the two distinct input name possibilites; prioritize the input tag
				input = (arg.StartsWith("input=") && input == "" ? arg.Split('=')[1] : input);
				input = (!arg.StartsWith("-") && 
					!arg.StartsWith("source=") &&
					!arg.StartsWith("system=") &&
					!arg.StartsWith("out=") &&
					!arg.StartsWith("manu=") &&
					!arg.StartsWith("url=") &&
					!arg.StartsWith("input=") &&
					input == "" ? arg : input);
			}

			// If skip is set, it's being called from the UI so we just exit
			if (skip)
			{
				return;
			}

			// If more than one switch is enabled or help is set, show the help screen
			if (help || !(import ^ generate ^ listsys ^ listsrc ^ genall ^ add ^ rem))
			{
				Help();
				logger.Close();
				return;
			}

			// Update the logger with the new value
			logger.ToFile = log;

			// Now take care of each mode in succesion

			// Import a file or folder
			if (import)
			{
				InitImport(input);
			}

			// Generate a DAT
			else if (generate)
			{
				InitGenerate(systems, sources, outdir, norename, old);
			}

			// Generate all DATs
			else if (genall)
			{
				InitGenerateAll(outdir, norename, old);
			}

			// List all available sources
			else if (listsrc)
			{
				ListSources();
			}

			// List all available systems
			else if (listsys)
			{
				ListSystems();
			}

			// Convert RV DAT to XML DAT
			else if (convert)
			{
				InitConvert(input);
			}

			// Add a source or system
			else if (add)
			{
				if (manu != "" && systems != "")
				{
					InitAddSystem(manu, systems);
				}
				else if (sources != "" && url != "")
				{
					InitAddSource(manu, systems);
				}
				else
				{
					Help();
				}
			} 
			
			// Remove a source or system
			else if (rem)
			{
				if (systems != "")
				{
					InitRemoveSystem(systems);
				}
				else if (sources != "")
				{
					InitRemoveSource(sources);
				}
				else
				{
					Help();
				}
			}

			logger.Close();
			return;
		}

		/// <summary>
		/// Print the program header
		/// </summary>
		private static void PrintHeader()
		{
			ConsoleColor formertext = Console.ForegroundColor;
			ConsoleColor formerback = Console.BackgroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.WriteLine(_header);
			Console.ForegroundColor = formertext;
			Console.BackgroundColor = formerback;
		}

		/// <summary>
		/// Show the text-based main menu
		/// </summary>
		private static void ShowMainMenu()
		{
			Console.Clear();
			string selection = "";
			while (selection.ToLowerInvariant() != "x")
			{
				Console.Clear();
				PrintHeader();
				Console.WriteLine(@"MAIN MENU
===========================
Make a selection:

    1) Show command line usage
    2) Import a DAT file or folder
    3) Generate a DAT file
    4) Generate all DAT files
    5) Convert a DAT file from RV to XML
    6) List all available sources
    7) List all available systems
    8) Add and Remove from database
    9) " + (logger.ToFile ? "Disable Logging" : "Enable Logging") + @"
    X) Exit Program
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();

				switch (selection)
				{
					case "1":
						Help();
						break;
					case "2":
						ImportMenu();
						break;
					case "3":
						GenerateMenu();
						break;
					case "4":
						GenerateAllMenu();
						break;
					case "5":
						ConvertMenu();
						break;
					case "6":
						Console.Clear();
						PrintHeader();
						ListSources();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "7":
						Console.Clear();
						PrintHeader();
						ListSystems();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "8":
						AddRemoveMenu();
						break;
					case "9":
						logger.ToFile = !logger.ToFile;
						break;
				}
			}
			Console.Clear();
			Console.WriteLine("Thank you for using DATabase!");
		}

		/// <summary>
		/// Show the help dialog
		/// </summary>
		private static void Help()
		{
			Console.Clear();
			Console.Write(@"
DATabase - Import and Generate DAT files
-----------------------------------------
Usage: DATabase [option] [filename|dirname|<system=sy,...> <source=so,...>]

Options:
  -h, -?, --help	Show this help
  -i, --import		Start tool in import mode
			  A filename or folder is required to run
  -g, --generate	Start tool in generate mode
			  system=sy,...		List of system IDs
			  source=so,...		List of source IDs
			  out=dir			Output directory
			  -nr, --no-rename	Don't auto-rename games
			  -old, --romvault	Produce a DAT in RV format
  -ga, --generate-all	Start tool in generate all mode
  -lso, --list-sources	List all sources (id <= name)
  -lsy, --list-systems	List all systems (id <= name)
  -c, --convert		Convert a RV DAT to XML
			  A filename or folder is required to run
  -l, --log		Enable logging of program output
");

			Console.Write("\nPress any key to continue...");
			Console.ReadKey();
			Console.Write(@"
Database Options:
  -a, --add		Add a new system or source to the database
			  manu=mn		Manufacturer name (system only)
			  system=sy		System name (system only)
			  source=sr		Source name (source only)
			  url=ul		URL (source only)
  -r, --remove	Remove a system or source from the database
			  system=sy		System ID
			  source=so			Source ID

Filenames and directories can't start with '-', 'system=', or 'source='
unless prefixed by 'input='");
			Console.Write("\nPress any key to continue...");
			Console.ReadKey();
			return;
		}

		/// <summary>
		/// Show the text-based import menu
		/// </summary>
		private static void ImportMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				PrintHeader();
				Console.WriteLine( @"IMPORT MENU
===========================
Enter the name of a DAT file or folder containing DAT files
or 'b' to go back to the previous menu:");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				if (selection.ToLowerInvariant() != "b")
				{
					InitImport(selection);
					Console.Write("\nPress any key to continue...");
					Console.ReadKey();
				}
			}
			return;
		}

		/// <summary>
		/// Wrap importing a file or folder into the database
		/// </summary>
		/// <param name="filename">File or folder to be imported</param>
		private static void InitImport(string filename)
		{
			Console.Clear();

			// Check to see if the second argument is a file that exists
			if (filename != "" && File.Exists(filename))
			{
				logger.Log("Beginning import of " + filename);
				Import imp = new Import(filename, _connectionString, logger);
				imp.ImportData();
				logger.Log(filename + " imported!");
			}
			// Check to see if the second argument is a directory that exists
			else if (filename != "" && Directory.Exists(filename))
			{
				foreach (string file in Directory.GetFiles(filename, "*", SearchOption.AllDirectories))
				{
					logger.Log("Beginning import of " + file);
					Import imp = new Import(file, _connectionString, logger);
					imp.ImportData();
					logger.Log(file + " imported!");
				}
			}
			else
			{
				logger.Error("I'm sorry but " + filename + "doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// Show the text-based generate menu
		/// </summary>
		private static void GenerateMenu()
		{
			string selection = "", systems = "", sources = "", outdir = "";
			bool norename = false, old = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				PrintHeader();
				Console.WriteLine(@"GENERATE MENU
===========================
Make a selection:

    1) " + (norename ? "Enable game renaming" : "Disable game renaming") + @"
    2) " + (old ? "Enable XML output" : "Enable RomVault output") + @"
    3) Enter a list of systems to generate from
    4) Enter a list of sources to generate from
    5) Enter an output folder
    6) Generate the DAT file
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
						Console.Write("Please enter the systems separated by commas: ");
						systems = Console.ReadLine();
						break;
					case "4":
						Console.Clear();
						ListSources();
						Console.Write("Please enter the sources separated by commas: ");
						sources = Console.ReadLine();
						break;
					case "5":
						Console.Clear();
						Console.Write("Please enter a folder name: ");
						outdir = Console.ReadLine();
						break;
					case "6":
						Console.Clear();
						InitGenerate(systems, sources, outdir, norename, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
			return;
		}

		/// <summary>
		/// Wrap generating a DAT from the database
		/// </summary>
		/// <param name="systems">Comma-separated list of systems to be included in the DAT (blank means all)</param>
		/// <param name="sources">Comma-separated list of sources to be included in the DAT (blank means all)</param>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in RomVault format (default false)</param>
		private static void InitGenerate(string systems, string sources, string outdir, bool norename, bool old)
		{
			Generate gen = new Generate(systems, sources, outdir, _connectionString, logger, norename, old);
			gen.Export();
			return;
		}

		/// <summary>
		/// Show the text-based generate all menu
		/// </summary>
		private static void GenerateAllMenu()
		{
			string selection = "", outdir = "";
			bool norename = false, old = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				PrintHeader();
				Console.WriteLine(@"GENERATE ALL MENU
===========================
Make a selection:

    1) " + (norename ? "Enable game renaming" : "Disable game renaming") + @"
    2) " + (old ? "Enable XML output" : "Enable RomVault output") + @"
    3) Enter an output folder
    4) Generate all DAT files
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
						Console.Write("Please enter a folder name: ");
						outdir = Console.ReadLine();
						break;
					case "4":
						Console.Clear();
						InitGenerateAll(outdir, norename, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
			return;
		}

		/// <summary>
		/// Wrap generating all standard DATs from the database
		/// </summary>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in RomVault format (default false)</param>
		private static void InitGenerateAll(string outdir, bool norename, bool old)
		{
			// Generate system-merged
			string query = @"SELECT DISTINCT systems.id
		FROM systems
		JOIN games
			ON systems.id=games.system
		ORDER BY systems.manufacturer, systems.system";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							logger.Error("No systems found! Please add a source and then try again.");
							return;
						}

						while (sldr.Read())
						{
							InitGenerate(sldr.GetInt32(0).ToString(), "", outdir, norename, old);

							// Generate custom
							string squery = @"SELECT DISTINCT sources.id
		FROM systems
		JOIN games
			ON systems.id=games.system
		JOIN sources
			ON games.source=sources.id
		WHERE systems.id=" + sldr.GetInt32(0).ToString() + @"
        ORDER BY sources.name";

							using (SQLiteCommand sslc = new SQLiteCommand(squery, dbc))
							{
								using (SQLiteDataReader ssldr = sslc.ExecuteReader())
								{
									// If nothing is found, tell the user and exit
									if (!ssldr.HasRows)
									{
										logger.Error("No sources found! Please add a source and then try again.");
										return;
									}

									while (ssldr.Read())
									{
										InitGenerate(sldr.GetInt32(0).ToString(), ssldr.GetInt32(0).ToString(), outdir, norename, old);
									}
								}
							}
						}
					}
				}

				// Generate source-merged
				query = @"SELECT DISTINCT sources.id, sources.name
		FROM sources
		JOIN games
			ON sources.id=games.source
		ORDER BY sources.name";

				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							logger.Error("No sources found! Please add a source and then try again.");
							return;
						}

						while (sldr.Read())
						{
							InitGenerate("", sldr.GetInt32(0).ToString(), outdir, norename, old);
						}
					}
				}
			}

			// Generate MEGAMERGED
			InitGenerate("", "", outdir, norename, old);
			return;
		}

		/// <summary>
		/// Show the text-based conversion menu
		/// </summary>
		private static void ConvertMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				PrintHeader();
				Console.WriteLine(@"CONVERT MENU
===========================
Enter the name of a DAT file to convert from RV to XML
or 'b' to go back to the previous menu:
");
				selection = Console.ReadLine();
				if (selection.ToLowerInvariant() != "b")
				{
					Console.Clear();
					InitConvert(selection);
					Console.Write("\nPress any key to continue...");
					Console.ReadKey();
				}
			}
			return;
		}

		/// <summary>
		/// Wrap converting DAT file from RomValut to XML
		/// </summary>
		/// <param name="filename"></param>
		private static void InitConvert(string filename)
		{
			if (File.Exists(filename))
			{
				Console.WriteLine("Converting " + filename);
				XElement conv = Converters.RomVaultToXML(File.ReadAllLines(filename));
				FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(filename) + ".new.xml");
				StreamWriter sw = new StreamWriter(fs);
				sw.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
					"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n");
				sw.Write(conv);
				sw.Close();
				fs.Close();
				Console.WriteLine("Converted file: " + Path.GetFileNameWithoutExtension(filename) + ".new.xml");
			}
			else
			{
				Console.WriteLine("I'm sorry but " + filename + "doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// List sources in the database
		/// </summary>
		/// <param name="all">True to list all sources regardless if there is a game associated or not</param>
		private static void ListSources(bool all = false)
		{
			string query = @"
SELECT DISTINCT sources.id, sources.name
FROM sources " + (!all ? "JOIN games on sources.id=games.source" : "") + @"
ORDER BY sources.name COLLATE NOCASE";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							logger.Warning("No sources found! Please add a source and then try again.");
							return;
						}

						Console.WriteLine("Available Sources (id <= name):\n");
						while (sldr.Read())
						{
							Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1));
						}
					}
				}
			}
			return;
		}

		/// <summary>
		/// List systems in the database
		/// </summary>
		/// <param name="all">True to list all systems regardless if there is a game associated or not</param>
		private static void ListSystems(bool all = false)
		{
			string query = @"
SELECT DISTINCT systems.id, systems.manufacturer, systems.system
FROM systems " + (!all ? "JOIN games ON systems.id=games.system" : "") + @"
ORDER BY systems.manufacturer, systems.system";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							logger.Warning("No systems found! Please add a system and then try again.");
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

		/// <summary>
		/// Show the text-based add and remove menu
		/// </summary>
		private static void AddRemoveMenu()
		{
			string selection = "", manufacturer = "", system = "", name = "", url = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				PrintHeader();
				Console.WriteLine(@"ADD AND REMOVE MENU
===========================
Make a selection:

    1) Add a source
    2) Remove a source
    3) Add a system
    4) Remove a system
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						Console.Clear();
						Console.Write("Please enter the source name: ");
						name = Console.ReadLine();
						Console.Write("\nPlease enter the source URL: ");
						url = Console.ReadLine();
						InitAddSource(name, url);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "2":
						Console.Clear();
						ListSources(true);
						Console.Write("Please enter the source: ");
						InitRemoveSource(Console.ReadLine());
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "3":
						Console.Clear();
						Console.Write("Please enter the manufacturer: ");
						manufacturer = Console.ReadLine();
						Console.Write("\nPlease enter the system: ");
						system = Console.ReadLine();
						InitAddSystem(manufacturer, system);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "4":
						Console.Clear();
						ListSystems(true);
						Console.Write("Please enter the system: ");
						InitRemoveSystem(Console.ReadLine());
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
			return;
		}

		/// <summary>
		/// Wrap adding a new source to the database
		/// </summary>
		/// <param name="name">Source name</param>
		/// <param name="url">Source URL(s)</param>
		private static void InitAddSource(string name, string url)
		{
			if (DBTools.AddSource(name, url, _connectionString))
			{
				logger.Log("Source " + name + " added!");
			}
			else
			{
				logger.Error("Source " + name + " could not be added!");
			}
		}

		/// <summary>
		/// Wrap removing an existing source from the database
		/// </summary>
		/// <param name="id">Source ID to be removed from the database</param>
		private static void InitRemoveSource(string sourceid)
		{
			int srcid = -1;
			if (Int32.TryParse(sourceid, out srcid))
			{
				if (DBTools.RemoveSource(srcid, _connectionString))
				{
					logger.Log("Source '" + srcid + "' removed!");
				}
				else
				{
					logger.Error("Source with id '" + srcid + "' could not be removed.");
                }
			}
			else
			{
				logger.Error("Invalid input");
			}
		}

		/// <summary>
		/// Wrap adding a new system to the database
		/// </summary>
		/// <param name="manufacturer">Manufacturer name</param>
		/// <param name="system">System name</param>
		private static void InitAddSystem(string manufacturer, string system)
		{
			if (DBTools.AddSystem(manufacturer, system, _connectionString))
			{
				logger.Log("System " + manufacturer + " - " + system + " added!");
			}
			else
			{
				logger.Error("System " + manufacturer + " - " + system + " could not be added!");
			}
		}

		/// <summary>
		/// Wrap removing an existing system from the database
		/// </summary>
		/// <param name="id">System ID to be removed from the database</param>
		private static void InitRemoveSystem(string systemid)
		{
			int sysid = -1;
			if (Int32.TryParse(systemid, out sysid))
			{
				if (DBTools.RemoveSystem(sysid, _connectionString))
				{
					logger.Log("System '" + sysid + "' removed!");
				}
				else
				{
					logger.Error("System with id '" + sysid + "' could not be removed.");
                }
			}
			else
			{
				logger.Error("Invalid input");
			}
		}
	}
}
