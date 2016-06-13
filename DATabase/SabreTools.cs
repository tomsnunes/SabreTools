using System;
using System.Collections.Generic;
using System.IO;

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
	public partial class SabreTools
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

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				/*
				// If there are no arguments, show the menu
				if (!Console.IsOutputRedirected)
				{
					ShowMainMenu();
				}
				else
				{
					Build.Help();
				}
				*/

				Build.Help();
				_logger.Close();
				return;
			}

			// Set all default values
			bool help = false,
				add = false,
				archivesAsFiles = false,
				bare = false,
				cascade = false,
				clean = false,
				datfromdir = false,
				datprefix = false,
				dedup = false,
				diff = false,
				disableForce = false,
				enableGzip = false,
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
				noMD5 = false,
				norename = false,
				noSHA1 = false,
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
			bool? nodump = null;
			long sgt = -1,
				slt = -1,
				seq = -1;
			string addext = "",
				author = "",
				category = "",
				comment = "",
				crc = "",
				date = "",
				description = "",
				email = "",
				exta = "",
				extb = "",
				filename = "",
				forcemerge = "",
				forcend = "",
				forcepack = "",
				gamename = "",
				header = "",
				homepage = "",
				name = "",
				manu = "",
				md5 = "",
				outdir = "",
				postfix = "",
				prefix = "",
				repext = "",
				romname = "",
				romtype = "",
				sha1 = "",
				sources = "",
				systems = "",
				root = "",
				tempdir = "",
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
					case "-cc":
					case "--convert-cmp":
						outputCMP = true;
						break;
					case "-cm":
					case "--convert-miss":
						outputMiss = true;
						break;
					case "-cr":
					case "--convert-rc":
						outputRC = true;
						break;
					case "-cs":
					case "--convert-sd":
						outputSD = true;
						break;
					case "-cx":
					case "--convert-xml":
						outputXML = true;
						break;
					case "-clean":
					case "--clean":
						clean = true;
						break;
					case "-d":
					case "--dfd":
						datfromdir = true;
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
					case "-f":
					case "--files":
						archivesAsFiles = true;
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
						datprefix = true;
						break;
					case "-gz":
					case "--gz-files":
						enableGzip = true;
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
					case "-nd":
					case "--nodump":
						nodump = true;
						break;
					case "-nm":
					case "--noMD5":
						noMD5 = true;
						break;
					case "-nnd":
					case "--not-nodump":
						nodump = false;
						break;
					case "-nr":
					case "--no-rename":
						norename = true;
						break;
					case "-ns":
					case "--noSHA1":
						noSHA1 = true;
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
						else if (arg.StartsWith("-crc=") || arg.StartsWith("--crc="))
						{
							crc = arg.Split('=')[1];
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
						else if (arg.StartsWith("-gn=") || arg.StartsWith("--game-name="))
						{
							gamename = arg.Split('=')[1];
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
						else if (arg.StartsWith("-md5=") || arg.StartsWith("--md5="))
						{
							md5 = arg.Split('=')[1];
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
						else if (arg.StartsWith("-rd=") || arg.StartsWith("--root-dir="))
						{
							root = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-re=") || arg.StartsWith("--rep-ext="))
						{
							repext = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-rn=") || arg.StartsWith("--rom-name="))
						{
							romname = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-rt=") || arg.StartsWith("--rom-type="))
						{
							romtype = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-seq=") || arg.StartsWith("--equal="))
						{
							if (!Int64.TryParse(arg.Split('=')[1], out seq))
							{
								seq = -1;
							}
						}
						else if (arg.StartsWith("-sgt=") || arg.StartsWith("--greater="))
						{
							if (!Int64.TryParse(arg.Split('=')[1], out sgt))
							{
								sgt = -1;
							}
						}
						else if (arg.StartsWith("-sha1=") || arg.StartsWith("--sha1="))
						{
							sha1 = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-slt=") || arg.StartsWith("--less="))
						{
							if (!Int64.TryParse(arg.Split('=')[1], out slt))
							{
								slt = -1;
							}
						}
						else if (arg.StartsWith("-source=") && sources == "")
						{
							sources = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-system=") && systems == "")
						{
							systems = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-t=") || arg.StartsWith("--temp="))
						{
							tempdir = arg.Split('=')[1];
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

			// If help is set, show the help screen
			if (help)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// If more than one switch is enabled, show the help screen
			if (!(add ^ datfromdir ^ extsplit ^ generate ^ genall ^ hashsplit ^ import ^ listsrc ^ listsys ^ (merge || diff) ^
				(update || outputCMP || outputRC || outputSD || outputXML || outputMiss || romba) ^ rem ^ stats ^ trim))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help();
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (update || (outputMiss || romba) || outputCMP || outputRC || outputSD
				|| outputXML || extsplit || hashsplit || datfromdir || (merge || diff) || stats || trim))
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

			// Convert, update, and filter a DAT or folder of DATs
			else if (update || outputCMP || outputMiss || outputRC || outputSD || outputXML || romba)
			{
				foreach (string input in inputs)
				{
					InitUpdate(input, filename, name, description, category, version, date, author, email, homepage, url, comment, header,
						superdat, forcemerge, forcend, forcepack, outputCMP, outputMiss, outputRC, outputSD, outputXML, usegame, prefix,
						postfix, quotes, repext, addext, datprefix, romba, tsv, gamename, romname, romtype, sgt, slt, seq, crc, md5,
						sha1, nodump, outdir, clean, dedup);
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
				InitExtSplit(inputs, exta, extb, outdir);
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

			// Create a DAT from a directory or set of directories
			else if (datfromdir)
			{
				InitDatFromDir(inputs, filename, name, description, category, version, author, forceunpack, old, romba, superdat, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, tempdir);
			}

			// If nothing is set, show the help
			else
			{
				Build.Help();
			}

			_logger.Close();
			return;
		}
	}
}
