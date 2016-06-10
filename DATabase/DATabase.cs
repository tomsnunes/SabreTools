﻿using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.IO.Compression;

using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the DATabase application
	/// </summary>
	/// <remarks>
	/// The following features are missing from DATabaseTwo with respect to the original DATabase:
	/// - Source merging
	/// - Custom DATs based on a system and a source
	/// - Multi-source and multi-system DATs
	/// 
	/// The following features need to (want to) be implemented in DATabaseTwo for further stability
	/// - Import updating file locations and names when SHA-1 hashes are matched
	/// - True duplicate DATs being removed from the import folder (SHA-1 matches)
	/// - Generate All only generating DATs that have been recently updated
	///		+ This requires implementing a "last updated" data point for all DATs and tracking for "last generate" somewhere
	/// - Impelement a ToSort folder for DATs that will place DATs in the correct subfolder on Import
	/// </remarks>
	public partial class DATabase
	{
		// Private required variables
		private static string _datroot = "DATS";
		private static string _outroot = "Output";
		private static string _dbName = "dats.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";

		private static Logger _logger;

		/// <summary>
		/// Start menu or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			_logger = new Logger(true, "database.log");
			_logger.Start();

			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}
			Setup();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				_logger.Close();
				return;
			}

			// If there's no arguments, show the menu
			if (args.Length == 0)
			{
				ShowMainMenu();
				_logger.Close();
				return;
			}

			// Set all default values
			bool help = false,
				add = false,
				bare = false,
				cascade = false,
				clean = false,
				dedup = false,
				diff = false,
				gamename = false,
				disableForce = false,
				extsplit = false,
				forceunpack = false,
				generate = false,
				genall = false,
				hashsplit = false,
				ignore = false,
				import = false,
				inplace = false,
				listsrc = false,
				listsys = false,
				merge = false,
				norename = false,
				old = false,
				outputCMP = false,
				outputMiss = false,
				outputRC = false,
				outputSD = false,
				outputXML = false,
				quotes = false,
				rem = false,
				romba = false,
				single = false,
				stats = false,
				superdat = false,
				trim = false,
				tsv = false,
				skip = false,
				update = false,
				usegame = true;
			string addext = "",
				author = "",
				category = "",
				comment = "",
				date = "",
				description = "",
				email = "",
				exta = "",
				extb = "",
				filename = "",
				forcemerge = "",
				forcend = "",
				forcepack = "",
				header = "",
				homepage = "",
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
					case "-c":
					case "--cascade":
						cascade = true;
						break;
					case "-clean":
					case "--clean":
						clean = true;
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
					case "-hs":
					case "--hash-split":
						hashsplit = true;
						break;
					case "-i":
					case "--import":
						import = true;
						break;
					case "-ig":
					case "--ignore":
						ignore = true;
						break;
					case "-ip":
					case "--inplace":
						inplace = true;
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
					case "-oc":
					case "--output-cmp":
						outputCMP = true;
						break;
					case "-om":
					case "--output-miss":
						outputMiss = true;
						break;
					case "-or":
					case "--output-rc":
						outputRC = true;
						break;
					case "-os":
					case "--output-sd":
						outputSD = true;
						break;
					case "-ox":
					case "--output-xml":
						outputXML = true;
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
					case "-ro":
					case "--romba":
						romba = true;
						break;
					case "-sd":
					case "--superdat":
						superdat = true;
						break;
					case "-si":
					case "--single":
						single = true;
						break;
					case "-st":
					case "--stats":
						stats = true;
						break;
					case "--skip":
						skip = true;
						break;
					case "-tm":
					case "--trim-merge":
						trim = true;
						break;
					case "-tsv":
					case " --tsv":
						tsv = true;
						break;
					case "-u":
					case "--unzip":
						forceunpack = true;
						break;
					case "-ud":
					case "--update":
						update = true;
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
						else if (arg.StartsWith("-ca=") || arg.StartsWith("--category="))
						{
							category = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-co=") || arg.StartsWith("--comment="))
						{
							comment = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-da=") || arg.StartsWith("--date="))
						{
							date = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-de=") || arg.StartsWith("--desc="))
						{
							description = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-em=") || arg.StartsWith("--email="))
						{
							email = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-exta="))
						{
							exta = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-extb="))
						{
							extb = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-f=") || arg.StartsWith("--filename="))
						{
							filename = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-fm=") || arg.StartsWith("--forcemerge="))
						{
							forcemerge = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-fn=") || arg.StartsWith("--forcend="))
						{
							forcend = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-fp=") || arg.StartsWith("--forcepack="))
						{
							forcepack = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-h=") || arg.StartsWith("--header="))
						{
							header = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-hp=") || arg.StartsWith("--homepage="))
						{
							homepage = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-input="))
						{
							inputs.Add(arg.Split('=')[1].Replace("\"", ""));
						}
						else if (arg.StartsWith("-manu=") && manu == "")
						{
							manu = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-n=") || arg.StartsWith("--name="))
						{
							name = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-out=") && outdir == "")
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
						else if (arg.StartsWith("-source=") && sources == "")
						{
							sources = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-system=") && systems == "")
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
						else if (arg.StartsWith("-u=") || arg.StartsWith("--url="))
						{
							url = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-url=") && url == "")
						{
							url = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-v=") || arg.StartsWith("--version="))
						{
							version = arg.Split('=')[1];
						}
						else if (File.Exists(arg.Replace("\"", "")) || Directory.Exists(arg.Replace("\"", "")))
						{
							inputs.Add(arg);
						}
						else
						{
							_logger.Error("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							_logger.Close();
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
			if (help || !(add ^ extsplit ^ generate ^ genall ^ hashsplit ^ import ^ listsrc ^ listsys ^ (merge || diff) ^
				(update || outputCMP || outputRC || outputSD || outputXML || outputMiss || romba) ^ rem ^ stats ^ trim))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help();
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (update || (outputMiss || romba) || outputCMP || outputRC || outputSD
				|| outputXML || extsplit || hashsplit || (merge || diff) || stats || trim))
			{
				_logger.Error("This feature requires at least one input");
				Build.Help();
				_logger.Close();
				return;
			}

			// Now take care of each mode in succesion

			// Import a file or folder
			if (import)
			{
				InitImport(ignore);
			}

			// Generate a DAT
			else if (generate)
			{
				InitImport(ignore);
				InitGenerate(systems, norename, old);
			}

			// Generate all DATs
			else if (genall)
			{
				InitImport(ignore);
				InitGenerateAll(norename, old);
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

			// Convert or update a DAT or folder of DATs
			else if (update || outputCMP || outputMiss || outputRC || outputSD || outputXML || romba)
			{
				foreach (string input in inputs)
				{
					InitUpdate(input, filename, name, description, category, version, date, author, email, homepage, url, comment, header,
						superdat, forcemerge, forcend, forcepack, outputCMP, outputMiss, outputRC, outputSD, outputXML, usegame, prefix,
						postfix, quotes, repext, addext, gamename, romba, tsv, outdir, clean);
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
			else if (merge || diff)
			{
				InitMergeDiff(inputs, name, description, category, version, author, diff, dedup, bare, forceunpack, old, superdat, cascade, inplace, outdir, clean);
			}

			// Split a DAT by available hashes
			else if (hashsplit)
			{
				InitHashSplit(inputs, outdir);
			}

			// Get statistics on input files
			else if (stats)
			{
				InitStats(inputs, single);
			}

			// If nothing is set, show the help
			else
			{
				Build.Help();
			}

			_logger.Close();
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
    2) Check for new or changed DATs
    3) Generate System DATs
    4) DAT file tools
    5) List all available sources
    6) List all available systems
    7) Add and remove systems and sources
    8) Show credits
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

    1) Convert or clean DAT or folder of DATs
    2) Convert DAT to missfile
    3) Trim all entries in DAT and merge into a single game
    4) Merge, diff, and/or dedup 2 or more DAT files
    5) Split DAT using 2 extensions
    6) Split DATs by best available hash values
    7) Get statistics on a DAT or folder of DATs
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						ConvertMenu();
						break;
					case "2":
						ConvertMissMenu();
						break;
					case "3":
						TrimMergeMenu();
						break;
					case "4":
						MergeDiffMenu();
						break;
					case "5":
						ExtSplitMenu();
						break;
					case "6":
						HashSplitMenu();
						break;
					case "7":
						StatsMenu();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based any to any conversion menu
		/// </summary>
		private static void ConvertMenu()
		{
			string selection = "", input = "", outdir = "";
			OutputFormat outputFormat = OutputFormat.Xml;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"DAT CONVERSION MENU
===========================
Make a selection:

    1) File or folder to convert" + (input == "" ? "" : ":\n" + input) + @"
    2) New output folder" + (outdir == "" ? " (blank means source directory)" : ":\n" + outdir) + @"
    3) Current output type: " + (outputFormat.ToString()) + @"
    4) Process file or folder
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						Console.Clear();
						Console.Write("Please enter a file or folder name: ");
						input = Console.ReadLine();
						break;
					case "2":
						Console.Clear();
						Console.Write("Please enter a folder name: ");
						outdir = Console.ReadLine();
						break;
					case "3":
						string subsel = "";
						while (subsel == "")
						{
							Console.Clear();
							Console.WriteLine(@"Possible output formats:
    1) Xml
    2) ClrMamePro
    3) RomCenter
    4) SabreDAT
");
							Console.Write("Please enter your selection: ");
							subsel = Console.ReadLine();

							switch (subsel)
							{
								case "1":
									outputFormat = OutputFormat.Xml;
									break;
								case "2":
									outputFormat = OutputFormat.ClrMamePro;
									break;
								case "3":
									outputFormat = OutputFormat.RomCenter;
									break;
								case "4":
									outputFormat = OutputFormat.SabreDat;
									break;
								default:
									subsel = "";
									break;
							}
						}
						break;
					case "4":
						Console.Clear();
						InitConvert(input, outputFormat, outdir, false);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						input = ""; outdir = "";
						outputFormat = OutputFormat.Xml;
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based DAT to missfile conversion menu
		/// </summary>
		private static void ConvertMissMenu()
		{
			string selection = "", input = "", prefix = "", postfix = "", addext = "", repext = "";
			bool usegame = true, quotes = false, gamename = false, romba = false, tsv = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"DAT -> MISS CONVERT MENU
===========================
Make a selection:

    1) File to convert" + (input != "" ? ":\n\t" + input : "") + @"
    2) " + (usegame ? "Output roms instead of games" : "Output games instead of roms") + @"
    3) " + (romba ? "Disable Romba-style output naming" : "Enable Romba-style output naming (overrides previous)") + @"
    4) Prefix to add to each line" + (prefix != "" ? ":\n\t" + prefix : "") + @"
    5) Postfix to add to each line" + (postfix != "" ? ":\n\t" + postfix : "") + @"
    6) " + (quotes ? "Don't add quotes around each item" : "Add quotes around each item") + @"
    7) Replace all extensions with another" + (repext != "" ? ":\t" + repext : "") + @"
    8) Add extensions to each item" + (addext != "" ? ":\n\t" + addext : "") + @"
" + (!usegame ? "    9) " + (gamename ? "Don't add game name before every item" : "Add game name before every item") + "\n" : "") +
@"    10) " + (romba ? "Don't output items in Romba format" : "Output items in Romba format") + @"
    12) " + (tsv ? "Don't output items in TSV format" : "Output items in TSV format") + @"
    12) Begin conversion
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
						romba = !romba;
						break;
					case "4":
						Console.Clear();
						Console.Write("Please enter the prefix: ");
						prefix = Console.ReadLine();
						break;
					case "5":
						Console.Clear();
						Console.Write("Please enter the postfix: ");
						postfix = Console.ReadLine();
						break;
					case "6":
						quotes = !quotes;
						break;
					case "7":
						Console.Clear();
						Console.Write("Please enter the replacement extension: ");
						repext = Console.ReadLine();
						break;
					case "8":
						Console.Clear();
						Console.Write("Please enter the additional extension: ");
						addext = Console.ReadLine();
						break;
					case "9":
						gamename = !gamename;
						break;
					case "10":
						romba = !romba;
						break;
					case "11":
						tsv = !tsv;
						break;
					case "12":
						Console.Clear();
						InitConvertMiss(input, usegame, prefix, postfix, quotes, repext, addext, gamename, romba, tsv);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						input = ""; prefix = ""; postfix = ""; addext = ""; repext = "";
						usegame = true; quotes = false; gamename = false;
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
    2) Set the root directory for trimming calculation" + (root != "" ? ":\n\t" + root : "") + @"
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
						selection = ""; input = ""; root = "";
						forceunpack = true; rename = true;
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
			bool dedup = false, diff = false, bare = false, forceunpack = false, old = false, superdat = false, cascade = false, inplace = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"MERGE AND DIFF MENU
===========================
Make a selection:

    1) Add a file or folder to process
    2) Internal DAT name" + (name != "" ? ":\t" + name : "") + @"
    3) External DAT name/description" + (desc != "" ? ":\t" + desc : "") + @"
    4) Category" + (cat != "" ? ":\t" + cat : "") + @"
    5) Version" + (version != "" ? ":\t" + version : "") + @"
    6) Author" + (author != "" ? ":\t" + author : "") + @"
    7) " + (dedup ? "Don't dedup files in output" : "Dedup files in output") + @"    8) " + (diff ? "Only merge the input files" : "Diff the input files") + @"
    9) " + (bare ? "Don't append the date to the name" : "Append the date to the name") + @"
    10) " + (forceunpack ? "Remove 'forcepacking=\"unzip\"' from output" : "Add 'forcepacking=\"unzip\"' to output") + @"
    11) " + (old ? "Enable XML output" : "Enable ClrMamePro output") + @"
    12) " + (superdat ? "Disable SuperDAT output" : "Enable SuperDAT output") + @"
    13) " + (cascade ? "Disable cascaded diffing (only if diff enabled)" : "Enable cascaded diffing (only if diff enabled)") + @"
    14) " + (inplace ? "Disable inplace diffing (only if cascade enabled)" : "Enable inplace diffing (only if cascade enabled)") + @"
    15) Process the DAT(s)
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
						superdat = !superdat;
						break;
					case "13":
						cascade = !cascade;
						break;
					case "14":
						inplace = !inplace;
						break;
					case "15":
						Console.Clear();
						List<string> inputs = new List<string>(input.Split(';'));
						InitMergeDiff(inputs, name, desc, cat, version, author, diff, dedup, bare, forceunpack, old, superdat, cascade, inplace);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						selection = ""; input = ""; name = ""; desc = ""; cat = ""; version = ""; author = "";
						dedup = false; diff = false; bare = false; forceunpack = false; old = false;
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
						outdir = Console.ReadLine();
						break;
					case "5":
						Console.Clear();
						InitExtSplit(input, exta, extb, outdir);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						input = ""; exta = ""; extb = ""; outdir = "";
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based HashSplit menu
		/// </summary>
		private static void HashSplitMenu()
		{
			string selection = "", input = "", outdir = "";
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"HASH SPLIT MENU
===========================
Make a selection:

    1) File or folder to split" + (input != "" ? ":\n\t" + input : "") + @"
    2) Output directory" + (outdir != "" ? ":\n\t" + outdir : "") + @"
    3) Split the file
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
						Console.Write("Please enter the output directory: ");
						outdir = Console.ReadLine();
						break;
					case "3":
						Console.Clear();
						List<string> inputs = new List<string>();
						inputs.Add(input);
						InitHashSplit(inputs, outdir);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						input = ""; outdir = "";
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based Stats menu
		/// </summary>
		private static void StatsMenu()
		{
			string selection = "", input = "";
			bool single = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabase");
				Console.WriteLine(@"STATISTICS MENU
===========================
Make a selection:

    1) File or folder to get stats on" + (input != "" ? ":\n\t" + input : "") + @"
    2) " + (single ? "Don't show individual DAT statistics" : "Show individual DAT statistics") + @"
    3) Get stats on the file(s)
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
						single = !single;
						break;
					case "3":
						Console.Clear();
						List<string> inputs = new List<string>();
						inputs.Add(input);
						InitStats(inputs, single);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						input = "";
						single = false;
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
						manufacturer = ""; system = ""; name = ""; url = "";
						break;
					case "2":
						Console.Clear();
						ListSources();
						Console.Write("Please enter the source: ");
						InitRemoveSource(Console.ReadLine());
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						manufacturer = ""; system = ""; name = ""; url = "";
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
						manufacturer = ""; system = ""; name = ""; url = "";
						break;
					case "4":
						Console.Clear();
						ListSystems();
						Console.Write("Please enter the system: ");
						InitRemoveSystem(Console.ReadLine());
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						manufacturer = ""; system = ""; name = ""; url = "";
						break;
				}
			}
			return;
		}

		#endregion

		#region Init Methods

		/// <summary>
		/// Wrap importing and updating DATs
		/// </summary>
		/// <param name="ignore"></param>
		private static void InitImport(bool ignore)
		{
			IImport imp = new ImportTwo(_datroot, _connectionString, _logger, ignore);
			imp.UpdateDatabase();
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
				TrimMerge sg = new TrimMerge(input, root, rename, force, _logger);
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
		/// <param name="superdat">True if DATs should be merged in SuperDAT style, false otherwise</param>
		/// <param name="cascade">True if the outputted diffs should be cascaded, false otherwise</param>
		/// <param name="inplace">True if cascaded diffs overwrite the source files, false otherwise</param>
		/// <param name="outdir">Output directory for the files (blank is default)</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		private static void InitMergeDiff(List<string> inputs, string name, string desc, string cat, string version, string author,
			bool diff, bool dedup, bool bare, bool forceunpack, bool old, bool superdat, bool cascade, bool inplace, string outdir = "", bool clean = false)
		{
			// Make sure there are no folders in inputs
			List<string> newInputs = new List<string>();
			foreach (string input in inputs)
			{
				if (Directory.Exists(input.Replace("\"", "")))
				{
					foreach (string file in Directory.EnumerateFiles(input.Replace("\"", ""), "*", SearchOption.AllDirectories))
					{
						try
						{
							newInputs.Add(Path.GetFullPath(file) + "¬" + Path.GetFullPath(input.Replace("\"", "")));
						}
						catch (PathTooLongException)
						{
							_logger.Warning("The path for " + file + " was too long");
						}
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}
					}
				}
				else if (File.Exists(input.Replace("\"", "")))
				{
					try
					{
						newInputs.Add(Path.GetFullPath(input.Replace("\"", "")) + "¬" + Path.GetDirectoryName(Path.GetFullPath(input.Replace("\"", ""))));
					}
					catch (PathTooLongException)
					{
						_logger.Warning("The path for " + input.Replace("\"", "") + " was too long");
					}
					catch (Exception ex)
					{
						_logger.Error(ex.ToString());
					}
				}
			}

			MergeDiff md = new MergeDiff(newInputs, name, desc, cat, version, author, diff, dedup, bare, forceunpack, old, superdat, cascade, inplace, outdir, clean, _logger);
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
					_logger.Warning("Two extensions are needed to split a DAT!");
					return;
				}
				ExtSplit es = new ExtSplit(input, exta, extb, outdir, _logger);
				es.Split();
				return;
			}
			else
			{
				_logger.Log("I'm sorry but " + input + "doesn't exist!");
			}
		}

		/// <summary>
		/// Wrap splitting a DAT by best available hashes
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outdir">Output directory for the split files</param>
		private static void InitHashSplit(List<string> inputs, string outdir)
		{
			// Strip any quotations from the names
			outdir = outdir.Replace("\"", "");

			// Verify the input files
			foreach (string input in inputs)
			{
				if (!File.Exists(input.Replace("\"", "")) && !Directory.Exists(input.Replace("\"", "")))
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// If so, run the program
			HashSplit hs = new HashSplit(inputs, outdir, _logger);
			hs.Split();
		}

		/// <summary>
		/// Wrap getting statistics on a DAT or folder of DATs
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="single">True to show individual DAT statistics, false otherwise</param>
		private static void InitStats(List<string> inputs, bool single)
		{
			List<string> newinputs = new List<string>();

			foreach (string input in inputs)
			{
				if (File.Exists(input.Replace("\"", "")))
				{
					newinputs.Add(input.Replace("\"", ""));
				}
				if (Directory.Exists(input.Replace("\"", "")))
				{
					foreach (string file in Directory.GetFiles(input.Replace("\"", ""), "*", SearchOption.AllDirectories))
					{
						newinputs.Add(file.Replace("\"", ""));
					}
				}
			}

			Logger statlog = new Logger(true, "stats.txt");
			statlog.Start();
			Stats stats = new Stats(newinputs, single, statlog);
			stats.Process();
			statlog.Close();
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
				_logger.Log("Source " + name + " added!");
			}
			else
			{
				_logger.Error("Source " + name + " could not be added!");
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
					_logger.Log("Source '" + srcid + "' removed!");
				}
				else
				{
					_logger.Error("Source with id '" + srcid + "' could not be removed.");
				}
			}
			else
			{
				_logger.Error("Invalid input");
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
				_logger.Log("System " + manufacturer + " - " + system + " added!");
			}
			else
			{
				_logger.Error("System " + manufacturer + " - " + system + " could not be added!");
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
					_logger.Log("System '" + sysid + "' removed!");
				}
				else
				{
					_logger.Error("System with id '" + sysid + "' could not be removed.");
				}
			}
			else
			{
				_logger.Error("Invalid input");
			}
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Perform initial setup for the program
		/// </summary>
		private static void Setup()
		{
			Remapping.CreateRemappings();
			Build.Start("DATabase");

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

		/// <summary>
		/// List sources in the database
		/// </summary>
		/// <remarks>This does not have an analogue in DATabaseTwo</remarks>
		private static void ListSources()
		{
			string query = @"
SELECT DISTINCT source.id, source.name, source.url
FROM source
ORDER BY source.name";
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
							_logger.Warning("No sources found! Please add a system and then try again.");
							return;
						}

						Console.WriteLine("Available Sources (id <= name):\n");
						while (sldr.Read())
						{
							Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1) + (!String.IsNullOrEmpty(sldr.GetString(2)) ? " (" + sldr.GetString(2) + ")" : ""));
						}
					}
				}
			}
			return;
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
	}
}
