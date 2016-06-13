using System;
using System.Collections.Generic;

using SabreTools.Helper;

namespace SabreTools
{
	public partial class SabreTools
	{
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
						InitUpdate(input, "", "", "", "", "", "", "", "", "", "", "", "", false, "", "", "",
							(outputFormat == OutputFormat.ClrMamePro), (outputFormat == OutputFormat.MissFile), (outputFormat == OutputFormat.RomCenter),
							(outputFormat == OutputFormat.SabreDat), (outputFormat == OutputFormat.Xml), false, "", "", false, "", "", false, false, false,
							"", "", "", -1, -1, -1, "", "", "", null, outdir, false, false);
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
						InitUpdate(input, "", "", "", "", "", "", "", "", "", "", "", "", false, "", "", "",
							false, true, false, false, false, usegame, prefix, postfix, quotes, repext, addext,
							gamename, romba, tsv, "", "", "", -1, -1, -1, "", "", "", null, "", false, false);
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
						List<string> inputs = new List<string>();
						inputs.Add(input);
						InitExtSplit(inputs, exta, extb, outdir);
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
	}
}
