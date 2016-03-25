using System;
using System.Data.SQLite;
using System.IO;
using System.Xml.Linq;

using DATabase.Helper;

namespace DATabase
{
	class Program
	{
		private static string _dbName = "DATabase.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";
		private static string _header =
@"+-------------------------------------------------------------------+
|                        DATabase 0.0.5.0                           |
|                                                                   |
|                 by Matt Nadareski (darksabre76)                   |
+-------------------------------------------------------------------+";

		static void Main(string[] args)
		{
			// Perform initial setup and verification
			DBTools.EnsureDatabase(_dbName, _connectionString);
			Remapping.CreateRemappings();
			Console.Clear();

			// If there's no arguments, show the menu
			if (args.Length == 0)
			{
				ShowMainMenu();
				return;
			}

			// Determine which switches are enabled (with values if necessary)
			bool help = false, import = false, generate = false, convert = false,
				listsys = false, listsrc = false, norename = false, old = false;
			string systems = "", sources = "", input = "";
			foreach (string arg in args)
			{
				help = help || (arg == "-h" || arg == "-?" || arg == "--help");
				import = import || (arg == "-i" || arg == "--import");
				generate = generate || (arg == "-g" || arg == "--generate");
				convert = convert || (arg == "-c" || arg == "--convert");
				listsys = listsys || (arg == "-lsy" || arg == "--list-systems");
				listsrc = listsrc || (arg == "-lso" || arg == "--list-sources");
				norename = norename || (arg == "-nr" || arg == "--no-rename");
				old = old || (arg == "-old" || arg == "--romvault");
				systems = (arg.StartsWith("system=") && systems == "" ? arg.Split('=')[1] : systems);
				sources = (arg.StartsWith("source=") && sources == "" ? arg.Split('=')[1] : sources);

				// Take care of the two distinct input name possibilites; prioritize the input tag
				input = (arg.StartsWith("input=") && input == "" ? arg.Split('=')[1] : input);
				input = (!arg.StartsWith("-") && !arg.StartsWith("source=") && !arg.StartsWith("system=") && !arg.StartsWith("input=") && input == "" ? arg : input);
			}

			// If more than one switch is enabled or help is set, show the help screen
			if (help || !(import ^ generate ^ listsys ^ listsrc))
			{
				Help();
				return;
			}

			// Now take care of each mode in succesion

			// Import a file or folder
			if (import)
			{
				InitImport(input);
			}

			// Generate a DAT
			else if (generate)
			{
				InitGenerate(systems, sources, norename, old);
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
			return;
		}

		private static void ShowMainMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "x")
			{
				Console.Clear();
				Console.WriteLine(_header + @"
MAIN MENU
===========================
Make a selection:

    1) Show help
    2) Import a DAT file or folder
    3) Generate a DAT file
    4) Convert a DAT file from RV to XML
    5) List all available sources
    6) List all available systems
    X) Exit
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
						ConvertMenu();
						break;
					case "5":
						ListSources();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "6":
						ListSystems();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
			Console.Clear();
			Console.WriteLine("Thank you for using DATabase!");
		}

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
			  -nr, --no-rename	Don't auto-rename games
			  -old, --romvault	Produce a DAT in RV format
  -lso, --list-sources	List all sources (id <= name)
  -lsy, --list-systems	List all systems (id <= name)
  -c, --convert		Convert a RV DAT to XML
			  A filename or folder is required to run

Filenames and directories can't start with '-', 'system=', or 'source='
unless prefixed by 'input='
");

			Console.Write("\nPress any key to continue...");
			Console.ReadKey();
			return;
		}

		private static void ImportMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Console.WriteLine(_header + @"
IMPORT MENU
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

		private static void InitImport(string filename)
		{
			Console.Clear();

			// Check to see if the second argument is a file that exists
			if (filename != "" && File.Exists(filename))
			{
				Console.WriteLine("Beginning import of " + filename);
				Import imp = new Import(filename, _connectionString);
				imp.ImportData();
				Console.WriteLine(filename + " imported!");
			}
			// Check to see if the second argument is a directory that exists
			else if (filename != "" && Directory.Exists(filename))
			{
				foreach (string file in Directory.GetFiles(filename, "*", SearchOption.TopDirectoryOnly))
				{
					Console.WriteLine("Beginning import of " + file);
					Import imp = new Import(file, _connectionString);
					imp.ImportData();
					Console.WriteLine(file + " imported!");
				}
			}
			else
			{
				Console.WriteLine("I'm sorry but " + filename + "doesn't exist!");
			}
			return;
		}

		private static void GenerateMenu()
		{
			string selection = "", systems = "", sources = "";
			bool norename = false, old = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Console.WriteLine(_header + @"
GENERATE MENU
===========================
Make a selection:

    1) " + (norename ? "Enable game renaming" : "Disable game renaming") + @"
    2) " + (old ? "Enable XML output" : "Enable RomVault output") + @"
    3) Enter a list of systems to generate from
    4) Enter a list of sources to generate from
    5) Generate the DAT file
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
						InitGenerate(systems, sources, norename, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
			return;
		}

		private static void InitGenerate(string systems, string sources, bool norename, bool old)
		{
			Generate gen = new Generate(systems, sources, _connectionString, norename, old);
			gen.Export();
			return;
		}

		private static void ConvertMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Console.WriteLine(_header + @"
CONVERT MENU
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

		private static void InitConvert(string filename)
		{
			if (File.Exists(filename))
			{
				Console.WriteLine("Converting " + filename);
				XElement conv = Converters.RomVaultToXML(File.ReadAllLines(filename));
				FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(filename) + ".new.xml");
				StreamWriter sw = new StreamWriter(fs);
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

		private static void ListSources()
		{
			string query = @"
SELECT DISTINCT sources.id, sources.name
FROM sources JOIN games on sources.id=games.source
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
							Console.WriteLine("Error: No sources found! Please add a source and then try again.");
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

		private static void ListSystems()
		{
			string query = @"
SELECT DISTINCT systems.id, systems.manufacturer, systems.system
FROM systems JOIN games ON systems.id=games.system
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
							Console.WriteLine("Error: No systems found! Please add a system and then try again.");
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
	}
}
