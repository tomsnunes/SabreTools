using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the DATabase application
	/// </summary>
	public partial class SabreTools
	{
		// Private required variables
		private static Logger _logger;

		/// <summary>
		/// Start menu or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			_logger = new Logger(true, "sabretools.log");

			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}
			Build.Start("SabreTools");
			DatabaseTools.EnsureDatabase(Constants.HeadererDbSchema, Constants.HeadererFileName, Constants.HeadererConnectionString);

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
				Build.Help();
				_logger.Close();
				return;
			}

			// Set all default values
			bool help = false,

				// Feature flags
				datFromDir = false,
				headerer = false,
				splitByExt = false,
				splitByHash = false,
				splitByType = false,
				stats = false,
				update = false,

				// Other flags
				addBlankFilesForEmptyFolder = false,
				addFileDates = false,
				cleanGameNames = false,
				copyFiles = false,
				datPrefix = false,
				dedup = false,
				enableGzip = false,
				excludeOf = false,
				inplace = false,
				merge = false,
				noMD5 = false,
				noSHA1 = false,
				parseArchivesAsFiles = false,
				quotes = false,
				rem = false,
				remext = false,
				removeDateFromAutomaticName = false,
				restore = false,
				romba = false,
				showBaddumpColumn = false,
				showNodumpColumn = false,
				single = false,
				softlist = false,
				superdat = false,
				trim = false,
				skip = false,
				usegame = true;
			DiffMode diffMode = 0x0;
			int maxParallelism = 4;
			long sgt = -1,
				slt = -1,
				seq = -1;
			OutputFormat outputFormat = 0x0;
			StatOutputFormat statOutputFormat = StatOutputFormat.None;
			
			// DAT fields
			string
				author = null,
				category = null,
				comment = null,
				date = null,
				description = null,
				email = null,
				filename = null,
				forcemerge = "",
				forcend = "",
				forcepack = "",
				header = null,
				homepage = null,
				name = null,
				rootdir = null,
				url = null,
				version = null,

				// Filter fields
				crc = "",
				gamename = "",
				md5 = "",
				romname = "",
				romtype = "",
				root = "",
				sha1 = "",
				status = "",

				// Missfile fields
				addext = "",
				postfix = "",
				prefix = "",
				repext = "",

				// Misc fields
				exta = null,
				extb = null,
				outDir = "",

				tempDir = "";
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
					case "-ab":
					case "--add-blank":
						addBlankFilesForEmptyFolder = true;
						break;
					case "-ad":
					case "--add-date":
						addFileDates = true;
						break;
					case "-b":
					case "--bare":
						removeDateFromAutomaticName = true;
						break;
					case "-bc":
					case "--baddump-col":
						showBaddumpColumn = true;
						break;
					case "-c":
					case "--cascade":
						diffMode |= DiffMode.Cascade;
						break;
					case "-cf":
					case "--copy-files":
						copyFiles = true;
						break;
					case "-csv":
					case "--csv":
						statOutputFormat = StatOutputFormat.CSV;
						break;
					case "-clean":
					case "--clean":
						cleanGameNames = true;
						break;
					case "-d":
					case "--d2d":
					case "--dfd":
						datFromDir = true;
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
						splitByExt = true;
						break;
					case "-f":
					case "--files":
						parseArchivesAsFiles = true;
						break;
					case "-gp":
					case "--game-prefix":
						datPrefix = true;
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
						splitByHash = true;
						break;
					case "-html":
					case "--html":
						statOutputFormat = StatOutputFormat.HTML;
						break;
					case "-ip":
					case "--inplace":
						inplace = true;
						break;
					case "-m":
					case "--merge":
						merge = true;
						break;
					case "-nc":
					case "--nodump-col":
						showNodumpColumn = true;
						break;
					case "-nm":
					case "--noMD5":
						noMD5 = true;
						break;
					case "-ns":
					case "--noSHA1":
						noSHA1 = true;
						break;
					case "-oa":
					case "--output-all":
						outputFormat |= OutputFormat.ALL;
						break;
					case "-oc":
					case "--output-cmp":
						outputFormat |= OutputFormat.ClrMamePro;
						break;
					case "-ocsv":
					case "--output-csv":
						outputFormat |= OutputFormat.CSV;
						break;
					case "-od":
					case "--output-dc":
						outputFormat |= OutputFormat.DOSCenter;
						break;
					case "-om":
					case "--output-miss":
						outputFormat |= OutputFormat.MissFile;
						break;
					case "-omd5":
					case "--output-md5":
						outputFormat |= OutputFormat.RedumpMD5;
						break;
					case "-ool":
					case "--output-ol":
						outputFormat |= OutputFormat.OfflineList;
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
					case "-osl":
					case "--output-sl":
						outputFormat |= OutputFormat.SoftwareList;
						break;
					case "-otsv":
					case "--output-tsv":
						outputFormat |= OutputFormat.TSV;
						break;
					case "-ox":
					case "--output-xml":
						outputFormat |= OutputFormat.Logiqx;
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
						diffMode |= DiffMode.ReverseCascade;
						break;
					case "-re":
					case "--restore":
						restore = true;
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
					case "--trim":
						trim = true;
						break;
					case "-ts":
					case "--type-split":
						splitByType = true;
						break;
					case "-tsv":
					case "--tsv":
						statOutputFormat = StatOutputFormat.TSV;
						break;
					case "-ud":
					case "--update":
						update = true;
						break;
					case "-xof":
					case "--exclude-of":
						excludeOf = true;
						break;
					default:
						string temparg = arg.Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-ae=") || temparg.StartsWith("--add-ext="))
						{
							addext = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-au=") || temparg.StartsWith("--author="))
						{
							author = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-ca=") || temparg.StartsWith("--category="))
						{
							category = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-co=") || temparg.StartsWith("--comment="))
						{
							comment = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-crc=") || temparg.StartsWith("--crc="))
						{
							crc = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-da=") || temparg.StartsWith("--date="))
						{
							date = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-de=") || temparg.StartsWith("--desc="))
						{
							description = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-em=") || temparg.StartsWith("--email="))
						{
							email = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-exta="))
						{
							exta = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-extb="))
						{
							extb = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-f=") || temparg.StartsWith("--filename="))
						{
							filename = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-fm=") || temparg.StartsWith("--forcemerge="))
						{
							forcemerge = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-fn=") || temparg.StartsWith("--forcend="))
						{
							forcend = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-fp=") || temparg.StartsWith("--forcepack="))
						{
							forcepack = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-gn=") || temparg.StartsWith("--game-name="))
						{
							gamename = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-h=") || temparg.StartsWith("--header="))
						{
							header = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-hp=") || temparg.StartsWith("--homepage="))
						{
							homepage = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-input="))
						{
							inputs.Add(temparg.Split('=')[1] ?? "");
						}
						else if (temparg.StartsWith("-is=") || temparg.StartsWith("--status="))
						{
							status = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-md5=") || temparg.StartsWith("--md5="))
						{
							md5 = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-mt=") || temparg.StartsWith("--mt="))
						{
							Int32.TryParse(temparg.Split('=')[1] ?? "", out maxParallelism);
						}
						else if (temparg.StartsWith("-n=") || temparg.StartsWith("--name="))
						{
							name = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-out=") || temparg.StartsWith("--out="))
						{
							outDir = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-post=") || temparg.StartsWith("--postfix="))
						{
							postfix = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-pre=") || temparg.StartsWith("--prefix="))
						{
							prefix = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-r=") || temparg.StartsWith("--root="))
						{
							rootdir = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-rd=") || temparg.StartsWith("--root-dir="))
						{
							root = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-re=") || temparg.StartsWith("--rep-ext="))
						{
							repext = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-rn=") || temparg.StartsWith("--rom-name="))
						{
							romname = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-rt=") || temparg.StartsWith("--rom-type="))
						{
							romtype = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-seq=") || temparg.StartsWith("--equal="))
						{
							seq = GetSizeFromString(temparg.Split('=')[1] ?? "");
						}
						else if (temparg.StartsWith("-sgt=") || temparg.StartsWith("--greater="))
						{
							sgt = GetSizeFromString(temparg.Split('=')[1] ?? "");
						}
						else if (temparg.StartsWith("-sha1=") || temparg.StartsWith("--sha1="))
						{
							sha1 = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-slt=") || temparg.StartsWith("--less="))
						{
							slt = GetSizeFromString(temparg.Split('=')[1] ?? "");
						}
						else if (temparg.StartsWith("-t=") || temparg.StartsWith("--temp="))
						{
							tempDir = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-u=") || temparg.StartsWith("-url=") || temparg.StartsWith("--url="))
						{
							url = temparg.Split('=')[1] ?? "";
						}
						else if (temparg.StartsWith("-v=") || temparg.StartsWith("--version="))
						{
							version = temparg.Split('=')[1] ?? "";
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
			if (!(splitByExt ^ splitByHash ^ headerer ^ (datFromDir || merge || diffMode != 0 || update
				|| outputFormat != 0 || trim) ^ rem ^ stats ^ splitByType))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help();
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (datFromDir || splitByExt || splitByHash || headerer
				|| (merge || diffMode != 0 || update || outputFormat != 0) || stats || trim || splitByType))
			{
				_logger.Error("This feature requires at least one input");
				Build.Help();
				_logger.Close();
				return;
			}

			// Now take care of each mode in succesion

			// Create a DAT from a directory or set of directories
			if (datFromDir)
			{
				InitDatFromDir(inputs,
					filename,
					name,
					description,
					category,
					version,
					author,
					forcepack,
					excludeOf,
					outputFormat,
					romba,
					superdat,
					noMD5,
					noSHA1,
					removeDateFromAutomaticName,
					parseArchivesAsFiles,
					enableGzip,
					addBlankFilesForEmptyFolder,
					addFileDates,
					tempDir,
					copyFiles,
					header,
					maxParallelism);
			}

			// Split a DAT by extension
			else if (splitByExt)
			{
				InitExtSplit(inputs, exta, extb, outDir);
			}

			// Split a DAT by available hashes
			else if (splitByHash)
			{
				InitHashSplit(inputs, outDir);
			}

			// If we're in headerer mode
			else if (headerer)
			{
				InitHeaderer(inputs, restore, outDir);
			}

			// Get statistics on input files
			else if (stats)
			{
				InitStats(inputs, filename, single, showBaddumpColumn, showNodumpColumn, statOutputFormat);
			}

			// Split a DAT by item type
			else if (splitByType)
			{
				InitTypeSplit(inputs, outDir);
			}

			// Convert, update, merge, diff, and filter a DAT or folder of DATs
			else if (update || outputFormat != 0 || merge || diffMode != 0)
			{
				InitUpdate(inputs, filename, name, description, rootdir, category, version, date, author, email, homepage, url, comment, header,
					superdat, forcemerge, forcend, forcepack, excludeOf, outputFormat, usegame, prefix,
					postfix, quotes, repext, addext, remext, datPrefix, romba, merge, diffMode, inplace, skip, removeDateFromAutomaticName, gamename, romname,
					romtype, sgt, slt, seq, crc, md5, sha1, status, trim, single, root, outDir, cleanGameNames, softlist, dedup, maxParallelism);
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
