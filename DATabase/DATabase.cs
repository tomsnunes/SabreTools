using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Xml;
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

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				logger.Close();
				return;
			}

			// If there's no arguments, show the menu
			if (args.Length == 0)
			{
				ShowMainMenu();
				logger.Close();
				return;
			}

			// Set all default values
			bool help = false,
				add = false,
				convertRV = false,
				convertXml = false,
				disableForce = false,
				generate = false,
				genall = false,
				import = false,
				log = false,
				listsrc = false,
				listsys = false,
				norename = false,
				old = false,
				rem = false,
				single = false,
				skip = false;
			string manu = "",
				outdir = "",
				sources = "",
				systems = "",
				root = "",
				url = "";
			List<string> inputs = new List<string>();

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
					case "-a":
					case "--add":
						add = true;
						break;
					case "-cr":
					case "--convert-rv":
						convertRV = true;
						break;
					case "-cx":
					case "--convert-xml":
						convertXml = true;
						break;
					case "-df":
					case "--disable-force":
						disableForce = false;
						break;
					case "-g":
					case "--generate":
						generate = true;
						break;
					case "-ga":
					case "--generate-all":
						genall = true;
						break;
					case "-i":
					case "--import":
						import = true;
						break;
					case "-l":
					case "--log":
						log = true;
						break;
					case "-lso":
					case "--list-sources":
						listsrc = true;
						break;
					case "-lsy":
					case "--list-systems":
						listsys = true;
						break;
					case "-nr":
					case "--no-rename":
						norename = true;
						break;
					case "-old":
					case "--romvault":
						old = true;
						break;
					case "-r":
					case "--remove":
						rem = true;
						break;
					case "-sg":
					case "--single-game":
						single = true;
						break;
					case "--skip":
						skip = true;
						break;
					default:
						if (arg.StartsWith("input="))
						{
							inputs.Add(arg.Split('=')[1]);
						}
						else if (arg.StartsWith("manu=") && manu == "")
						{
							manu = arg.Split('=')[1];
						}
						else if (arg.StartsWith("out=") && outdir == "")
						{
							outdir = arg.Split('=')[1];
						}
						else if (arg.StartsWith("source=") && sources == "")
						{
							sources = arg.Split('=')[1];
						}
						else if (arg.StartsWith("system=") && systems == "")
						{
							systems = arg.Split('=')[1];
						}
						else if(arg.StartsWith("-rd=") || arg.StartsWith("--root-dir="))
						{
							root = arg.Split('=')[1];
						}
						else if (arg.StartsWith("url=") && url == "")
						{
							url = arg.Split('=')[1];
						}
						else
						{
							inputs.Add(arg);
						}
						break;
				}
			}

			// If skip is set, it's being called from the UI so we just exit
			if (skip)
			{
				return;
			}

			// If more than one switch is enabled or help is set, show the help screen
			if (help || !(add ^ convertRV ^ convertXml ^ generate ^ genall ^ import ^ listsrc ^ listsys ^ rem ^ single))
			{
				Build.Help();
				logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (convertRV || convertXml || import || single))
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Update the logger with the new value
			logger.ToFile = log;

			// Now take care of each mode in succesion

			// Import a file or folder
			if (import)
			{
				foreach (string input in inputs)
				{
					InitImport(input);
				}
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

			// Convert XML DAT to RV DAT
			else if (convertRV)
			{
				foreach (string input in inputs)
				{
					InitConvertRV(input);
				}
			}

			// Convert RV DAT to XML DAT
			else if (convertXml)
			{
				foreach (string input in inputs)
				{
					InitConvertXML(input);
				}
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
					Build.Help();
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
					Build.Help();
				}
			}

			// Consolodate and trim DAT
			else if (single)
			{
				foreach (string input in inputs)
				{
					InitSingleGame(input, root, norename, disableForce);
				}
			}

			logger.Close();
			return;
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
				Build.Start("DATabase");
				Console.WriteLine(@"MAIN MENU
===========================
Make a selection:

    1) Show command line usage
    2) Import a DAT file or folder
    3) Generate DAT files
    4) DAT file tools
    5) List all available sources
    6) List all available systems
    7) Add and remove systems and sources
    8) " + (logger.ToFile ? "Disable Logging" : "Enable Logging") + @"
    9) Show credits
    X) Exit Program
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();

				switch (selection)
				{
					case "1":
						Build.Help();
						break;
					case "2":
						ImportMenu();
						break;
					case "3":
						GenerateMenu();
						break;
					case "4":
						DatToolsMenu();
						break;
					case "5":
						Console.Clear();
						Build.Start("DATabase");
						ListSources();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "6":
						Console.Clear();
						Build.Start("DATabase");
						ListSystems();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "7":
						AddRemoveMenu();
						break;
					case "8":
						logger.ToFile = !logger.ToFile;
						break;
					case "9":
						Console.Clear();
						Build.Credits();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
			Console.Clear();
			Console.WriteLine("Thank you for using DATabase!");
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
				Build.Start("DATabase");
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

			// Drag and drop means quotes; we don't want quotes
			filename = filename.Replace("\"", "");

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
				logger.Error("I'm sorry but " + filename + " doesn't exist!");
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
				Build.Start("DATabase");
				Console.WriteLine(@"GENERATE MENU
===========================
Make a selection:

    1) " + (norename ? "Enable game renaming" : "Disable game renaming") + @"
    2) " + (old ? "Enable XML output" : "Enable RomVault output") + @"
    3) List of systems to generate from" + (systems != "" ? ": " + systems : "") + @"
    4) List of sources to generate from" + (sources != "" ? ": " + sources : "") + @"
    5) Enter an output folder" + (outdir != "" ? ":\n\t" + outdir : "") + @"
    6) Generate the DAT file
    7) Generate all available DAT files
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
					case "7":
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
		/// Wrap generating all standard DATs from the database
		/// </summary>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in RomVault format (default false)</param>
		private static void InitGenerateAll(string outdir, bool norename, bool old)
		{
			string actualdir = (outdir == "" ? Environment.CurrentDirectory + "/" : outdir + "/");
			outdir = actualdir + "/temp/";

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

			// Zip up all of the files that were generated
			logger.Log("Creating zip archive");
			ZipArchive zip = ZipFile.Open(actualdir + "dats-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip", ZipArchiveMode.Create);
			foreach (String filename in Directory.EnumerateFiles(outdir))
			{
				if (filename.EndsWith(".xml") || filename.EndsWith(".dat"))
				{
					string internalFolder = (filename.Contains("ALL (Merged") ? "" :
						filename.Contains("Merged") ? "merged-system/" :
							filename.Contains("ALL") ? "merged-source/" : "custom/");
					zip.CreateEntryFromFile(filename, internalFolder + Path.GetFileName(filename), CompressionLevel.Optimal);
				}
			}
			zip.Dispose();
			logger.Log("Zip archive created!");

			// Remove all of the DATs from the folder
			Directory.Delete(outdir, true);

			return;
		}

		/// <summary>
		/// Show the text-based DAT tools menu
		/// </summary>
		/// <remarks>
		/// At an unspecified future date, this will also include the following currently-separate programs:
		/// - DatSplit
		/// - SingleGame
		/// - MergeDAT
		/// - DATFromDir
		/// - DatToMiss
		/// </remarks>
		private static void DatToolsMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"DAT TOOLS MENU
===========================
Make a selection:

    1) Convert XML DAT to RV
    2) Convert RV DAT to XML
    3) Merge all entries into a single game and trim
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						ConvertRVMenu();
						break;
					case "2":
						ConvertXMLMenu();
						break;
					case "3":
						SingleGameMenu();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based XML to RV conversion menu
		/// </summary>
		private static void ConvertRVMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"XML -> RV CONVERT MENU
===========================
Enter the name of a DAT file to convert from XML to RV
or 'b' to go back to the previous menu:
");
				selection = Console.ReadLine();
				if (selection.ToLowerInvariant() != "b")
				{
					Console.Clear();
					InitConvertRV(selection);
					Console.Write("\nPress any key to continue...");
					Console.ReadKey();
				}
			}
			return;
		}

		/// <summary>
		/// Wrap converting DAT file from XML to RomVault
		/// </summary>
		/// <param name="filename"></param>
		private static void InitConvertRV(string filename)
		{
			if (File.Exists(filename))
			{
				Console.WriteLine("Converting " + filename);
				XmlDocument doc = new XmlDocument();
				try
				{
					doc.LoadXml(File.ReadAllText(filename));
					string conv = Converters.XMLToRomVault(doc);
					FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(filename) + ".new.dat");
					StreamWriter sw = new StreamWriter(fs);
					sw.Write(conv);
					sw.Close();
					fs.Close();
					Console.WriteLine("Converted file: " + Path.GetFileNameWithoutExtension(filename) + ".new.dat");
				}
				catch (XmlException)
				{
					logger.Warning("The file " + filename + " could not be parsed as an XML file");
				}
			}
			else
			{
				Console.WriteLine("I'm sorry but " + filename + " doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// Show the text-based RV to XML conversion menu
		/// </summary>
		private static void ConvertXMLMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"RV -> XML CONVERT MENU
===========================
Enter the name of a DAT file to convert from RV to XML
or 'b' to go back to the previous menu:
");
				selection = Console.ReadLine();
				if (selection.ToLowerInvariant() != "b")
				{
					Console.Clear();
					InitConvertXML(selection);
					Console.Write("\nPress any key to continue...");
					Console.ReadKey();
				}
			}
			return;
		}

		/// <summary>
		/// Wrap converting DAT file from RomVault to XML
		/// </summary>
		/// <param name="filename"></param>
		private static void InitConvertXML(string filename)
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

		private static void SingleGameMenu()
		{

		}

		/// <summary>
		/// Wrap converting a DAT to single-game mode
		/// </summary>
		/// <param name="input">Input file or folder to be converted</param>
		/// <param name="root">Root directory to base path lengths on</param>
		/// <param name="norename">True is games should not be renamed</param>
		/// <param name="disableForce">True if forcepacking="unzip" should be omitted</param>
		private static void InitSingleGame(string input, string root, bool norename, bool disableForce)
		{
			if (File.Exists(input) || Directory.Exists(input))
			{
				SingleGame sg = new SingleGame(input, root, norename, disableForce);
				sg.Process();
				return;
			}
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
				Build.Start("DATabase");
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
