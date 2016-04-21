using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
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
				bare = false,
				convertMiss = false,
				convertCMP = false,
				convertXml = false,
				dedup = false,
				diff = false,
				gamename = false,
				disableForce = false,
				extsplit = false,
				forceunpack = false,
				generate = false,
				genall = false,
				import = false,
				log = false,
				listsrc = false,
				listsys = false,
				merge = false,
				norename = false,
				old = false,
				quotes = false,
				rem = false,
				trim = false,
				skip = false,
				usegame = true;
			string addext = "",
				author = "",
				cat = "",
				desc = "",
				exta = "",
				extb = "",
				name = "",
				manu = "",
				outdir = "",
				postfix = "",
				prefix = "",
				repext = "",
				sources = "",
				systems = "",
				root = "",
				url = "",
				version = "";
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
					case "-b":
					case "--bare":
						bare = true;
						break;
					case "-cc":
					case "--convert-cmp":
						convertCMP = true;
						break;
					case "-cm":
					case "--convert-miss":
						convertMiss = true;
						break;
					case "-cx":
					case "--convert-xml":
						convertXml = true;
						break;
					case "-dd":
					case "--dedup":
						dedup = true;
						break;
					case "-df":
					case "--disable-force":
						disableForce = true;
						break;
					case "-di":
					case "--diff":
						diff = true;
						break;
					case "-es":
					case "--ext-split":
						extsplit = true;
						break;
					case "-g":
					case "--generate":
						generate = true;
						break;
					case "-ga":
					case "--generate-all":
						genall = true;
						break;
					case "-gp":
					case "--game-prefix":
						gamename = true;
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
					case "-m":
					case "--merge":
						merge = true;
						break;
					case "-nr":
					case "--no-rename":
						norename = true;
						break;
					case "-o":
					case "--old":
						old = true;
						break;
					case "-q":
					case "--quotes":
						quotes = true;
						break;
					case "-r":
					case "--roms":
						usegame = false;
						break;
					case "-rm":
					case "--remove":
						rem = true;
						break;
					case "--skip":
						skip = true;
						break;
					case "-tm":
					case "--trim-merge":
						trim = true;
						break;
					case "-u":
					case "--unzip":
						forceunpack = true;
						break;
					default:
						if (arg.StartsWith("-ae=") || arg.StartsWith("--add-ext="))
						{
							addext = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-au=") || arg.StartsWith("--author="))
						{
							author = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-c=") || arg.StartsWith("--cat="))
						{
							cat = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-d=") || arg.StartsWith("--desc="))
						{
							desc = arg.Split('=')[1];
						}
						else if (arg.StartsWith("exta="))
						{
							exta = arg.Split('=')[1];
						}
						else if (arg.StartsWith("extb"))
						{
							extb = arg.Split('=')[1];
						}
						else if (arg.StartsWith("input="))
						{
							inputs.Add(arg.Split('=')[1]);
						}
						else if (arg.StartsWith("manu=") && manu == "")
						{
							manu = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-n=") || arg.StartsWith("--name="))
						{
							name = arg.Split('=')[1];
						}
						else if (arg.StartsWith("out=") && outdir == "")
						{
							outdir = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-post=") || arg.StartsWith("--postfix="))
						{
							postfix = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-pre=") || arg.StartsWith("--prefix="))
						{
							prefix = arg.Split('=')[1];
						}
						else if (arg.StartsWith("source=") && sources == "")
						{
							sources = arg.Split('=')[1];
						}
						else if (arg.StartsWith("system=") && systems == "")
						{
							systems = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-rd=") || arg.StartsWith("--root-dir="))
						{
							root = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-re=") || arg.StartsWith("--rep-ext="))
						{
							repext = arg.Split('=')[1];
						}
						else if (arg.StartsWith("url=") && url == "")
						{
							url = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-v=") || arg.StartsWith("--version="))
						{
							version = arg.Split('=')[1];
						}
						else if (File.Exists(arg) || Directory.Exists(arg))
						{
							inputs.Add(arg);
						}
						else
						{
							logger.Warning("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							logger.Close();
							return;
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
			if (help || !(add ^ convertMiss ^ convertCMP ^ convertXml ^ extsplit ^ generate ^ genall ^ import ^ listsrc ^ listsys ^ merge ^ rem ^ trim))
			{
				Build.Help();
				logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (convertMiss || convertCMP || convertXml || extsplit || import || merge || trim))
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

			// Convert DAT to missfile
			else if (convertMiss)
			{
				foreach (string input in inputs)
				{
					InitConvertMiss(input, usegame, prefix, postfix, quotes, repext, addext, gamename);
				}
			}

			// Convert XML DAT to CMP DAT
			else if (convertCMP)
			{
				foreach (string input in inputs)
				{
					InitConvertCMP(input);
				}
			}

			// Convert CMP DAT to XML DAT
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
			else if (trim)
			{
				foreach (string input in inputs)
				{
					InitTrimMerge(input, root, !norename, !disableForce);
				}
			}

			// Split a DAT by extension
			else if (extsplit)
			{
				foreach (string input in inputs)
				{
					InitExtSplit(input, exta, extb, outdir);
				}
			}

			// Merge, diff, and dedupe at least 2 DATs
			else if (merge)
			{
				InitMergeDiff(inputs, name, desc, cat, version, author, diff, dedup, bare, forceunpack, old);
			}

			logger.Close();
			return;
		}

		#region Menus

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
				Console.WriteLine(@"IMPORT MENU
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
    2) " + (old ? "Enable XML output" : "Enable ClrMamePro output") + @"
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
		/// Show the text-based DAT tools menu
		/// </summary>
		/// <remarks>
		/// At an unspecified future date, this will also include the following currently-separate programs:
		/// - DATFromDir
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

    1) Convert XML DAT to CMP
    2) Convert CMP DAT to XML
    3) Convert DAT to missfile
    4) Trim all entries in DAT and merge into a single game
    5) Merge, diff, and/or dedup 2 or more DAT files
    6) Split DAT using 2 extensions
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						ConvertCMPMenu();
						break;
					case "2":
						ConvertXMLMenu();
						break;
					case "3":
						ConvertMissMenu();
						break;
					case "4":
						TrimMergeMenu();
						break;
					case "5":
						MergeDiffMenu();
						break;
					case "6":
						ExtSplitMenu();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based XML to CMP conversion menu
		/// </summary>
		private static void ConvertCMPMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"XML -> CMP CONVERT MENU
===========================
Enter the name of a DAT file to convert from XML to CMP
or 'b' to go back to the previous menu:
");
				selection = Console.ReadLine();
				if (selection.ToLowerInvariant() != "b")
				{
					Console.Clear();
					InitConvertCMP(selection);
					Console.Write("\nPress any key to continue...");
					Console.ReadKey();
				}
			}
			return;
		}

		/// <summary>
		/// Show the text-based CMP to XML conversion menu
		/// </summary>
		private static void ConvertXMLMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"CMP -> XML CONVERT MENU
===========================
Enter the name of a DAT file to convert from CMP to XML
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
		/// Show the text-based DAT to missfile conversion menu
		/// </summary>
		private static void ConvertMissMenu()
		{
			string selection = "", input = "", prefix = "", postfix = "", addext = "", repext = "";
			bool usegame = true, quotes = false, gamename = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"DAT -> MISS CONVERT MENU
===========================
Make a selection:

    1) File to convert" + (input != "" ? ":\n\t" + input : "") + @"
    2) " + (usegame ? "Output roms instead of games" : "Output games instead of roms") + @"
    3) Prefix to add to each line" + (prefix != "" ? ":\n\t" + prefix : "") + @"
    4) Postfix to add to each line" + (postfix != "" ? ":\n\t" + postfix : "") + @"
    5) " + (quotes ? "Don't add quotes around each item" : "Add quotes around each item") + @"
    6) Replace all extensions with another" + (repext != "" ? ":\t" + repext : "") + @"
    7) Add extensions to each item" + (addext != "" ? ":\n\t" + addext : "") + @"
" + (!usegame ? "    8) " + (gamename ? "Don't add game name before every item" : "Add game name before every item") + "\n" : "") +
@"    9) Begin conversion
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						Console.Clear();
						Console.Write("Please enter the file name: ");
						input = Console.ReadLine();
						break;
					case "2":
						usegame = !usegame;
						break;
					case "3":
						Console.Clear();
						Console.Write("Please enter the prefix: ");
						prefix = Console.ReadLine();
						break;
					case "4":
						Console.Clear();
						Console.Write("Please enter the postfix: ");
						postfix = Console.ReadLine();
						break;
					case "5":
						quotes = !quotes;
						break;
					case "6":
						Console.Clear();
						Console.Write("Please enter the replacement extension: ");
						repext = Console.ReadLine();
						break;
					case "7":
						Console.Clear();
						Console.Write("Please enter the additional extension: ");
						addext = Console.ReadLine();
						break;
					case "8":
						gamename = !gamename;
						break;
					case "9":
						Console.Clear();
						InitConvertMiss(input, usegame, prefix, postfix, quotes, repext, addext, gamename);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based TrimMerge menu
		/// </summary>
		private static void TrimMergeMenu()
		{
			string selection = "", input = "", root = "";
			bool forceunpack = true, rename = true;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"DAT TRIM MENU
===========================
Make a selection:

    1) File or folder to process" + (input != "" ? ":\n\t" + input : "") + @"
    2) Root folder for reference" + (root != "" ? ":\n\t" + root : "") + @"
    3) " + (forceunpack ? "Remove 'forcepacking=\"unzip\"' from output" : "Add 'forcepacking=\"unzip\"' to output") + @"
    4) " + (rename ? "Keep all game names" : "Rename all games to '!'") + @"
    5) Process the file or folder
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						Console.Clear();
						Console.Write("Please enter the file or folder name: ");
						input = Console.ReadLine();
						break;
					case "2":
						Console.Clear();
						Console.Write("Please enter the root folder name: ");
						root = Console.ReadLine();
						break;
					case "3":
						forceunpack = !forceunpack;
						break;
					case "4":
						rename = !rename;
						break;
					case "5":
						Console.Clear();
						InitTrimMerge(input, root, rename, forceunpack);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based MergeDiff menu
		/// </summary>
		private static void MergeDiffMenu()
		{
			string selection = "", input = "", name = "", desc = "", cat = "", version = "", author = "";
			bool dedup = false, diff = false, bare = false, forceunpack = false, old = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"DAT TRIM MENU
===========================
Make a selection:

    1) Add a file or folder to process
    2) Internal DAT name" + (name != "" ? ":\t" + name : "") + @"
    3) External DAT name/description" + (desc != "" ? ":\t" + desc : "") + @"
    4) Category" + (cat != "" ? ":\t" + cat : "") + @"
    5) Version" + (version != "" ? ":\t" + version : "") + @"
    6) Author" + (author != "" ? ":\t" + author : "") + @"
    7) " + (dedup ? "Don't dedup files in output" : "Dedup files in output") + @"
    8) " + (diff ? "Only merge the input files" : "Diff the input files") + @"
    9) " + (bare ? "Don't append the date to the name" : "Append the date to the name") + @"
    10) " + (forceunpack ? "Remove 'forcepacking=\"unzip\"' from output" : "Add 'forcepacking=\"unzip\"' to output") + @"
    11) " + (old ? "Enable XML output" : "Enable ClrMamePro output") + @"
    12) Merge the DATs
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						Console.Clear();
						Console.Write("Please enter a file or folder name: ");
						input += (input == "" ? "" : ";") + Console.ReadLine();
						break;
					case "2":
						Console.Clear();
						Console.Write("Please enter a name: ");
						name = Console.ReadLine();
						break;
					case "3":
						Console.Clear();
						Console.Write("Please enter a description: ");
						desc = Console.ReadLine();
						break;
					case "4":
						Console.Clear();
						Console.Write("Please enter a category: ");
						cat = Console.ReadLine();
						break;
					case "5":
						Console.Clear();
						Console.Write("Please enter a version: ");
						version = Console.ReadLine();
						break;
					case "6":
						Console.Clear();
						Console.Write("Please enter an author: ");
						author = Console.ReadLine();
						break;
					case "7":
						dedup = !dedup;
						break;
					case "8":
						diff = !diff;
						break;
					case "9":
						bare = !bare;
						break;
					case "10":
						forceunpack = !forceunpack;
						break;
					case "11":
						old = !old;
						break;
					case "12":
						Console.Clear();
						List<string> inputs = new List<string>(input.Split(';'));
						InitMergeDiff(inputs, name, desc, cat, version, author, diff, dedup, bare, forceunpack, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based ExtSplit menu
		/// </summary>
		private static void ExtSplitMenu()
		{
			string selection = "", input = "", exta = "", extb = "", outdir = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"EXTENSION SPLIT MENU
===========================
Make a selection:

    1) File to split" + (input != "" ? ":\n\t" + input : "") + @"
    2) First file extension" + (exta != "" ? ":\t" + exta : "") + @"
    3) Second file extension" + (extb != "" ? ":\t" + extb : "") + @"
    4) Output directory" + (outdir != "" ? ":\n\t" + outdir : "") + @"
    5) Split the file
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						Console.Clear();
						Console.Write("Please enter the file name: ");
						input = Console.ReadLine();
						break;
					case "2":
						Console.Clear();
						Console.Write("Please enter the first extension: ");
						exta = Console.ReadLine();
						break;
					case "3":
						Console.Clear();
						Console.Write("Please enter the second extension: ");
						extb = Console.ReadLine();
						break;
					case "4":
						Console.Clear();
						Console.Write("Please enter the output directory: ");
						exta = Console.ReadLine();
						break;
					case "5":
						Console.Clear();
						InitExtSplit(input, exta, extb, outdir);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
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

		#endregion

		#region Init Methods

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
				bool success = imp.ImportData();
				logger.Log(filename + (success ? "" : " not") + " imported!");
			}
			// Check to see if the second argument is a directory that exists
			else if (filename != "" && Directory.Exists(filename))
			{
				foreach (string file in Directory.GetFiles(filename, "*", SearchOption.AllDirectories))
				{
					logger.Log("Beginning import of " + file);
					Import imp = new Import(file, _connectionString, logger);
					bool success = imp.ImportData();
					logger.Log(file + (success ? "" : " not") + " imported!");
				}
			}
			else
			{
				logger.Error("I'm sorry but " + filename + " doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// Wrap generating a DAT from the database
		/// </summary>
		/// <param name="systems">Comma-separated list of systems to be included in the DAT (blank means all)</param>
		/// <param name="sources">Comma-separated list of sources to be included in the DAT (blank means all)</param>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in ClrMamePro format (default false)</param>
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
		/// <param name="old">True if the output file should be in ClrMamePro format (default false)</param>
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

							using (SqliteCommand sslc = new SqliteCommand(squery, dbc))
							{
								using (SqliteDataReader ssldr = sslc.ExecuteReader())
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

				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
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
		/// Wrap converting DAT file from XML to ClrMamePro
		/// </summary>
		/// <param name="filename"></param>
		private static void InitConvertCMP(string filename)
		{
			if (File.Exists(filename))
			{
				logger.Log("Converting " + filename);
				XmlDocument doc = new XmlDocument();
				try
				{
					doc.LoadXml(File.ReadAllText(filename));
					string conv = Converters.XMLToClrMamePro(doc);
					FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(filename) + ".new.dat");
					StreamWriter sw = new StreamWriter(fs);
					sw.Write(conv);
					sw.Close();
					fs.Close();
					logger.Log("Converted file: " + Path.GetFileNameWithoutExtension(filename) + ".new.dat");
				}
				catch (XmlException)
				{
					logger.Warning("The file " + filename + " could not be parsed as an XML file");
				}
			}
			else
			{
				logger.Error("I'm sorry but " + filename + " doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// Wrap converting DAT file from ClrMamePro to XML
		/// </summary>
		/// <param name="filename"></param>
		private static void InitConvertXML(string filename)
		{
			if (File.Exists(filename))
			{
				logger.Log("Converting " + filename);
				XElement conv = Converters.ClrMameProToXML(File.ReadAllLines(filename));
				FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(filename) + ".new.xml");
				StreamWriter sw = new StreamWriter(fs);
				sw.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
					"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n");
				sw.Write(conv);
				sw.Close();
				fs.Close();
				logger.Log("Converted file: " + Path.GetFileNameWithoutExtension(filename) + ".new.xml");
			}
			else
			{
				logger.Error("I'm sorry but " + filename + "doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// Wrap converting a DAT to missfile
		/// </summary>
		/// <param name="input">File to be converted</param>
		/// <param name="usegame">True if games are to be used in output, false if roms are</param>
		/// <param name="prefix">Generic prefix to be added to each line</param>
		/// <param name="postfix">Generic postfix to be added to each line</param>
		/// <param name="quotes">Add quotes to each item</param>
		/// <param name="repext">Replace all extensions with another</param>
		/// <param name="addext">Add an extension to all items</param>
		/// <param name="gamename">Add the dat name as a directory prefix</param>
		private static void InitConvertMiss(string input, bool usegame, string prefix, string postfix, bool quotes, string repext, string addext, bool gamename)
		{
			// Strip any quotations from the name
			input = input.Replace("\"", "");

			if (input != "" && File.Exists(input))
			{
				// Get the full input name
				input = Path.GetFullPath(input);

				// Get the output name
				string name = Path.GetFileNameWithoutExtension(input) + "-miss.txt";

				// Read in the roms from the DAT and then write them to the file
				logger.Log("Converting " + input);
				Output.WriteToText(name, Path.GetDirectoryName(input), RomManipulation.Parse(input, 0, 0, logger), logger, usegame, prefix, postfix, addext, repext, quotes, gamename);
				logger.Log(input + " converted to: " + name);
				return;
			}
			else
			{
				logger.Error("I'm sorry but " + input + "doesn't exist!");
			}
		}

		/// <summary>
		/// Wrap trimming and merging a single DAT
		/// </summary>
		/// <param name="input">Input file or folder to be converted</param>
		/// <param name="root">Root directory to base path lengths on</param>
		/// <param name="rename">True is games should not be renamed</param>
		/// <param name="force">True if forcepacking="unzip" should be included</param>
		private static void InitTrimMerge(string input, string root, bool rename, bool force)
		{
			// Strip any quotations from the name
			input = input.Replace("\"", "");

			if (input != "" && (File.Exists(input) || Directory.Exists(input)))
			{
				TrimMerge sg = new TrimMerge(input, root, rename, force, logger);
				sg.Process();
				return;
			}
		}

		/// <summary>
		/// Wrap merging, diffing, and deduping 2 or mor DATs
		/// </summary>
		/// <param name="inputs">A List of Strings representing the DATs or DAT folders to be merged</param>
		/// <param name="name">Internal name of the DAT</param>
		/// <param name="desc">Description and external name of the DAT</param>
		/// <param name="cat">Category for the DAT</param>
		/// <param name="version">Version of the DAT</param>
		/// <param name="author">Author of the DAT</param>
		/// <param name="diff">True if a DiffDat of all inputs is wanted, false otherwise</param>
		/// <param name="dedup">True if the outputted file should remove duplicates, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="forceunpack">True if the forcepacking="unzip" tag is to be added, false otherwise</param>
		/// <param name="old">True if a old-style DAT should be output, false otherwise</param>
		private static void InitMergeDiff(List<string> inputs, string name, string desc, string cat, string version, string author, bool diff, bool dedup, bool bare, bool forceunpack, bool old)
		{
			// Make sure there are no folders in inputs
			List<string> newInputs = new List<string>();
			foreach (string input in inputs)
			{
				if (Directory.Exists(input.Replace("\"", "")))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						newInputs.Add(Path.GetFullPath(file));
					}
				}
				else if (File.Exists(input.Replace("\"", "")))
				{
					newInputs.Add(Path.GetFullPath(input.Replace("\"", "")));
				}
			}

			MergeDiff md = new MergeDiff(newInputs, name, desc, cat, version, author, diff, dedup, bare, forceunpack, old, logger);
			md.Process();
		}

		/// <summary>
		/// Wrap splitting a DAT by 2 extensions
		/// </summary>
		/// <param name="input">Input file or folder to be split</param>
		/// <param name="exta">First extension to split on</param>
		/// <param name="extb">Second extension to split on</param>
		/// <param name="outdir">Output directory for the split files</param>
		private static void InitExtSplit(string input, string exta, string extb, string outdir)
		{
			// Strip any quotations from the names
			input = input.Replace("\"", "");
			exta = exta.Replace("\"", "");
			extb = extb.Replace("\"", "");
			outdir = outdir.Replace("\"", "");

			if (input != "" && File.Exists(input))
			{
				if (exta == "" || extb == "")
				{
					logger.Warning("Two extensions are needed to split a DAT!");
					return;
				}
				ExtSplit es = new ExtSplit(input, exta, extb, outdir, logger);
				es.Split();
				return;
			}
			else
			{
				logger.Log("I'm sorry but " + input + "doesn't exist!");
			}
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

		#endregion

		#region Listing Methods

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

		#endregion
	}
}
