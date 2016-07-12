using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;

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
				enableGzip = false,
				extsplit = false,
				fake = false,
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
				offlineMerge = false,
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
				softlist = false,
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
				currentAllMerged = "",
				currentMissingMerged = "",
				currentNewMerged = "",
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
					case "-fk":
					case "--fake":
						fake = true;
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
					case "-ol":
					case "--offmerge":
						offlineMerge = true;
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
					case "-sl":
					case "--softlist":
						softlist = true;
						break;
					case "-st":
					case "--stats":
						stats = true;
						break;
					case "--skip":
						skip = true;
						break;
					case "-trim":
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
						string temparg = arg.Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-ae=") || temparg.StartsWith("--add-ext="))
						{
							addext = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-au=") || temparg.StartsWith("--author="))
						{
							author = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-ca=") || temparg.StartsWith("--category="))
						{
							category = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-co=") || temparg.StartsWith("--comment="))
						{
							comment = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-com=") || temparg.StartsWith("--com="))
						{
							currentAllMerged = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-crc=") || temparg.StartsWith("--crc="))
						{
							crc = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-da=") || temparg.StartsWith("--date="))
						{
							date = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-de=") || temparg.StartsWith("--desc="))
						{
							description = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-em=") || temparg.StartsWith("--email="))
						{
							email = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-exta="))
						{
							exta = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-extb="))
						{
							extb = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-f=") || temparg.StartsWith("--filename="))
						{
							filename = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-fix=") || temparg.StartsWith("--fix="))
						{
							currentMissingMerged = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-fm=") || temparg.StartsWith("--forcemerge="))
						{
							forcemerge = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-fn=") || temparg.StartsWith("--forcend="))
						{
							forcend = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-fp=") || temparg.StartsWith("--forcepack="))
						{
							forcepack = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-gn=") || temparg.StartsWith("--game-name="))
						{
							gamename = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-h=") || temparg.StartsWith("--header="))
						{
							header = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-hp=") || temparg.StartsWith("--homepage="))
						{
							homepage = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-input="))
						{
							inputs.Add(temparg.Split('=')[1]);
						}
						else if (temparg.StartsWith("-manu=") && manu == "")
						{
							manu = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-md5=") || temparg.StartsWith("--md5="))
						{
							md5 = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-n=") || temparg.StartsWith("--name="))
						{
							name = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-new=") || temparg.StartsWith("--new="))
						{
							currentNewMerged = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-out=") || temparg.StartsWith("--out="))
						{
							outdir = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-post=") || temparg.StartsWith("--postfix="))
						{
							postfix = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-pre=") || temparg.StartsWith("--prefix="))
						{
							prefix = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-rd=") || temparg.StartsWith("--root-dir="))
						{
							root = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-re=") || temparg.StartsWith("--rep-ext="))
						{
							repext = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-rn=") || temparg.StartsWith("--rom-name="))
						{
							romname = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-rt=") || temparg.StartsWith("--rom-type="))
						{
							romtype = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-seq=") || temparg.StartsWith("--equal="))
						{
							if (!Int64.TryParse(temparg.Split('=')[1], out seq))
							{
								seq = -1;
							}
						}
						else if (temparg.StartsWith("-sgt=") || temparg.StartsWith("--greater="))
						{
							if (!Int64.TryParse(temparg.Split('=')[1], out sgt))
							{
								sgt = -1;
							}
						}
						else if (temparg.StartsWith("-sha1=") || temparg.StartsWith("--sha1="))
						{
							sha1 = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-slt=") || temparg.StartsWith("--less="))
						{
							if (!Int64.TryParse(temparg.Split('=')[1], out slt))
							{
								slt = -1;
							}
						}
						else if (temparg.StartsWith("-source=") && sources == "")
						{
							sources = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-system=") && systems == "")
						{
							systems = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-t=") || temparg.StartsWith("--temp="))
						{
							tempdir = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-u=") || temparg.StartsWith("--url="))
						{
							url = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-url=") && url == "")
						{
							url = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-v=") || temparg.StartsWith("--version="))
						{
							version = temparg.Split('=')[1];
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
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
			if (!(add ^ datfromdir ^ extsplit ^ generate ^ genall ^ hashsplit ^ import ^ listsrc ^ listsys ^
				(merge || diff || update || outputCMP || outputRC || outputSD || outputXML || outputMiss || trim) ^
				offlineMerge ^ rem ^ stats))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help();
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (update || outputMiss || outputCMP || outputRC || outputSD
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

			// Convert, update, merge, diff, and filter a DAT or folder of DATs
			else if (update || outputCMP || outputMiss || outputRC || outputSD || outputXML || merge || diff)
			{
				InitUpdate(inputs, filename, name, description, category, version, date, author, email, homepage, url, comment, header,
					superdat, forcemerge, forcend, forcepack, outputCMP, outputMiss, outputRC, outputSD, outputXML, usegame, prefix,
					postfix, quotes, repext, addext, datprefix, romba, tsv, merge, diff, cascade, inplace, bare, gamename, romname,
					romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, outdir, clean, softlist, dedup);
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

			// Split a DAT by extension
			else if (extsplit)
			{
				InitExtSplit(inputs, exta, extb, outdir);
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

			// If we want to run Offline merging mode
			else if (offlineMerge)
			{
				if (!(currentAllMerged == "" && currentMissingMerged == "" && currentNewMerged == ""))
				{
					InitOfflineMerge(currentAllMerged, currentMissingMerged, currentNewMerged, fake);
				}
				else
				{
					_logger.User("All inputs were empty! At least one input is required...");
					Build.Help();
					_logger.Close();
					return;
				}
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
