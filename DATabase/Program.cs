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

		static void Main(string[] args)
		{
			// Perform initial setup and verification
			DBTools.EnsureDatabase(_dbName, _connectionString);
			Remapping.CreateRemappings();
			Console.Clear();

			/*
			// Show runtime header
			Console.WriteLine(
@"+-------------------------------------------------------------------+
|                                                                   |
|                        DATabase 0.0.4.1                           |
|                                                                   |
|                 by Matt Nadareski (darksabre76)                   |
|                                                                   |
+-------------------------------------------------------------------+
");
			*/

			// If there's not enough arguments, show the help screen
			if (args.Length == 0)
			{
				Help();
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
				// Check to see if the second argument is a file that exists
				if (input != "" && File.Exists(input))
				{
					Console.WriteLine(input);
					Import imp = new Import(input, _connectionString);
					imp.ImportData();
				}
				// Check to see if the second argument is a directory that exists
				else if (input != "" && Directory.Exists(input))
				{
					foreach (string filename in Directory.GetFiles(input, "*", SearchOption.TopDirectoryOnly))
					{
						Console.WriteLine(filename);
						Import imp = new Import(filename, _connectionString);
						imp.ImportData();
					}
				}
				// Otherwise, show help
				else
				{
					Help();
					return;
				}
			}

			// Generate a DAT
			else if (generate)
			{
				Generate gen = new Generate(systems, sources, _connectionString, norename, old);
				gen.Export();
			}

			// List all available sources
			else if (listsrc)
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

							Console.WriteLine("Available Sources (id <= name):");
							while (sldr.Read())
							{
								Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1));
							}
						}
					}
				}
			}

			// List all available systems
			else if (listsys)
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

							Console.WriteLine("Available Systems (id <= name):");
							while (sldr.Read())
							{
								Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1) + " - " + sldr.GetString(2));
							}
						}
					}
				}
			}

			// Convert RV DAT to XML DAT
			else if (convert)
			{
				if (File.Exists(input))
				{
					Console.WriteLine("Converting " + input);
					XElement conv = Converters.RomVaultToXML(File.ReadAllLines(input));
					FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(input) + ".new.xml");
					StreamWriter sw = new StreamWriter(fs);
					sw.Write(conv);
					sw.Close();
					fs.Close();
					Console.WriteLine("Converted file: " + Path.GetFileNameWithoutExtension(input) + ".new.xml");
				}
				// Otherwise, show help
				else
				{
					Help();
					return;
				}
			}
			return;
		}

		private static void Help ()
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
		}
	}
}
