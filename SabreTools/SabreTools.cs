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

		private static Logger _logger;

		/// <summary>
		/// Start menu or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			_logger = new Logger(true, "sabretools.log");
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
				addBlanks = false,
				addDate = false,
				archivesAsFiles = false,
				bare = false,
				clean = false,
				datfromdir = false,
				datprefix = false,
				dedup = false,
				enableGzip = false,
				extsplit = false,
				forceunpack = false,
				generate = false,
				genall = false,
				hashsplit = false,
				headerer = false,
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
				quotes = false,
				rem = false,
				remext = false,
				restore = false,
				romba = false,
				single = false,
				softlist = false,
				stats = false,
				superdat = false,
				trim = false,
				typesplit = false,
				skip = false,
				update = false,
				usegame = true;
			bool? cascade = null,
				nodump = null,
				tsv = null;
			DiffMode diffMode = 0x0;
			int maxParallelism = 4;
			long sgt = -1,
				slt = -1,
				seq = -1;
			OutputFormat outputFormat = 0x0;
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
				outDir = "",
				postfix = "",
				prefix = "",
				repext = "",
				romname = "",
				romtype = "",
				root = "",
				rootdir = "",
				sha1 = "",
				sources = "",
				systems = "",
				tempDir = "",
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
					case "-ab":
					case "--add-blank":
						addBlanks = true;
						break;
					case "-ad":
					case "--add-date":
						addDate = true;
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
						outputFormat |= OutputFormat.ClrMamePro;
						break;
					case "-cm":
					case "--convert-miss":
						outputFormat |= OutputFormat.MissFile;
						break;
					case "-cr":
					case "--convert-rc":
						outputFormat |= OutputFormat.RomCenter;
						break;
					case "-cs":
					case "--convert-sd":
						outputFormat |= OutputFormat.SabreDat;
						break;
					case "-csv":
					case "--csv":
						tsv = false;
						break;
					case "-cx":
					case "--convert-xml":
						outputFormat |= OutputFormat.Xml;
						break;
					case "-clean":
					case "--clean":
						clean = true;
						break;
					case "-d":
					case "--d2d":
					case "--dfd":
						datfromdir = true;
						break;
					case "-dd":
					case "--dedup":
						dedup = true;
						break;
					case "-di":
					case "--diff":
						diffMode |= DiffMode.All;
						break;
					case "-did":
					case "--diff-du":
						diffMode |= DiffMode.Dupes;
						break;
					case "-dii":
					case "--diff-in":
						diffMode |= DiffMode.Individuals;
						break;
					case "-din":
					case "--diff-nd":
						diffMode |= DiffMode.NoDupes;
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
					case "-hd":
					case "--headerer":
						headerer = true;
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
						outputFormat |= OutputFormat.ClrMamePro;
						break;
					case "-om":
					case "--output-miss":
						outputFormat |= OutputFormat.MissFile;
						break;
					case "-omd5":
					case "--output-md5":
						outputFormat |= OutputFormat.RedumpMD5;
						break;
					case "-or":
					case "--output-rc":
						outputFormat |= OutputFormat.RomCenter;
						break;
					case "-os":
					case "--output-sd":
						outputFormat |= OutputFormat.SabreDat;
						break;
					case "-osfv":
					case "--output-sfv":
						outputFormat |= OutputFormat.RedumpSFV;
						break;
					case "-osha1":
					case "--output-sha1":
						outputFormat |= OutputFormat.RedumpSHA1;
						break;
					case "-ox":
					case "--output-xml":
						outputFormat |= OutputFormat.Xml;
						break;
					case "-q":
					case "--quotes":
						quotes = true;
						break;
					case "-r":
					case "--roms":
						usegame = false;
						break;
					case "-rc":
					case "--rev-cascade":
						cascade = false;
						break;
					case "-re":
					case "--restore":
						restore = true;
						break;
					case "-rm":
					case "--remove":
						rem = true;
						break;
					case "-rme":
					case "--rem-ext":
						remext = true;
						break;
					case "-ro":
					case "--romba":
						romba = true;
						break;
					case "-sd":
					case "--superdat":
						superdat = true;
						break;
					case "-sf":
					case "--skip":
						skip = true;
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
					case "-trim":
						trim = true;
						break;
					case "-ts":
					case "--type-split":
						typesplit = true;
						break;
					case "-tsv":
					case "--tsv":
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
						else if (temparg.StartsWith("-mt=") || temparg.StartsWith("--mt="))
						{
							Int32.TryParse(temparg.Split('=')[1], out maxParallelism);
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
							outDir = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-post=") || temparg.StartsWith("--postfix="))
						{
							postfix = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-pre=") || temparg.StartsWith("--prefix="))
						{
							prefix = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-r=") || temparg.StartsWith("--root="))
						{
							rootdir = temparg.Split('=')[1];
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
							seq = GetSizeFromString(temparg.Split('=')[1]);
						}
						else if (temparg.StartsWith("-sgt=") || temparg.StartsWith("--greater="))
						{
							sgt = GetSizeFromString(temparg.Split('=')[1]);
						}
						else if (temparg.StartsWith("-sha1=") || temparg.StartsWith("--sha1="))
						{
							sha1 = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-slt=") || temparg.StartsWith("--less="))
						{
							slt = GetSizeFromString(temparg.Split('=')[1]);
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
							tempDir = temparg.Split('=')[1];
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
							Console.WriteLine();
							_logger.Error("Invalid input detected: " + arg);
							_logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// If more than one switch is enabled, show the help screen
			if (!(add ^ datfromdir ^ extsplit ^ generate ^ genall ^ hashsplit ^ headerer ^ import ^ listsrc ^
				listsys ^ (merge || diffMode != 0 || update || outputFormat != 0 || tsv != null|| trim) ^ rem ^ stats ^ typesplit))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help();
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (datfromdir || extsplit || hashsplit || headerer
				|| (merge || diffMode != 0 || update || outputFormat != 0 || tsv != null) || stats || trim || typesplit))
			{
				_logger.Error("This feature requires at least one input");
				Build.Help();
				_logger.Close();
				return;
			}

			// Now take care of each mode in succesion

			// Add a source or system
			if (add)
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

			// Create a DAT from a directory or set of directories
			else if (datfromdir)
			{
				InitDatFromDir(inputs, filename, name, description, category, version, author, forceunpack, outputFormat,
					romba, superdat, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, addBlanks, addDate, tempDir, maxParallelism);
			}

			// Split a DAT by extension
			else if (extsplit)
			{
				InitExtSplit(inputs, exta, extb, outDir);
			}

			// Generate all DATs
			else if (genall)
			{
				InitImport(ignore);
				InitGenerateAll(norename, old);
			}

			// Generate a DAT
			else if (generate)
			{
				InitImport(ignore);
				InitGenerate(systems, norename, old);
			}

			// Split a DAT by available hashes
			else if (hashsplit)
			{
				InitHashSplit(inputs, outDir);
			}

			// If we're in headerer mode
			else if (headerer)
			{
				InitHeaderer(inputs, restore, outDir, _logger);
			}

			// Import a file or folder
			else if (import)
			{
				InitImport(ignore);
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

			// Get statistics on input files
			else if (stats)
			{
				InitStats(inputs, single);
			}

			// Split a DAT by item type
			else if (typesplit)
			{
				InitTypeSplit(inputs, outDir);
			}

			// Convert, update, merge, diff, and filter a DAT or folder of DATs
			else if (update || tsv != null || outputFormat != 0 || merge || diffMode != 0)
			{
				InitUpdate(inputs, filename, name, description, rootdir, category, version, date, author, email, homepage, url, comment, header,
					superdat, forcemerge, forcend, forcepack, outputFormat, usegame, prefix,
					postfix, quotes, repext, addext, remext, datprefix, romba, tsv, merge, diffMode, cascade, inplace, skip, bare, gamename, romname,
					romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, outDir, clean, softlist, dedup, maxParallelism);
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
