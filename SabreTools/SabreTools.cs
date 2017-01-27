using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Helper;
using SabreTools.Helper.Data;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

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

			// If output is being redirected or we are in script mode, don't allow clear screens
			if (!Console.IsOutputRedirected && !args.Contains("--script"))
			{
				Console.Clear();
			}
			Build.Start("SabreTools");

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Help("Credits");
				_logger.Close();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help("SabreTools");
				_logger.Close();
				return;
			}

			// Feature flags
			bool help = false,
				datFromDir = false,
				extract = false,
				restore = false,
				sort = false, // SimpleSort
				splitByExt = false,
				splitByHash = false,
				splitByLevel = false,
				splitByType = false,
				stats = false,
				update = false,
				verify = false; // SimpleSort

			// User flags
			bool addBlankFilesForEmptyFolder = false,
				addFileDates = false,
				basedat = false,
				cleanGameNames = false,
				copyFiles = false,
				datPrefix = false,
				dedup = false,
				delete = false, // SimpleSort
				enableGzip = false,
				excludeOf = false,
				hashOnly = false,
				inplace = false,
				inverse = false, // SimpleSort
				merge = false,
				noMD5 = false,
				noSHA1 = false,
				parseArchivesAsFiles = false,
				quickScan = false, // SimpleSort
				quotes = false,
				remext = false,
				removeDateFromAutomaticName = false,
				romba = false,
				showBaddumpColumn = false,
				showNodumpColumn = false,
				shortname = false,
				single = false,
				softlist = false,
				superdat = false,
				trim = false,
				skip = false,
				updateDat = false, // SimpleSort
				usegame = true;
			bool? runnable = null;
			DatFormat datFormat = 0x0;
			DiffMode diffMode = 0x0;
			OutputFormat outputFormat = OutputFormat.Folder;
			SplitType splitType = SplitType.None;
			StatDatFormat statDatFormat = 0x0;

			// User inputs
			int gz = 2, // SimpleSort
				maxParallelism = 4,
				rar = 2, // SimpleSort
				sevenzip = 1, // SimpleSort
				zip = 1; // SimpleSort
			long sgt = -1,
				slt = -1,
				seq = -1;
			string addext = String.Empty,
				author = null,
				category = null,
				comment = null,
				date = null,
				description = null,
				email = null,
				exta = null,
				extb = null,
				filename = null,
				forcemerge = String.Empty,
				forcend = String.Empty,
				forcepack = String.Empty,
				header = null,
				homepage = null,
				name = null,
				outDir = String.Empty,
				postfix = String.Empty,
				prefix = String.Empty,
				repext = String.Empty,
				root = String.Empty,
				rootdir = null,
				tempDir = String.Empty,
				url = null,
				version = null;
			List<string> crc = new List<string>();
			List<string> datfiles = new List<string>(); // SimpleSort
			//List<string> exta = new List<string>();
			//List<string> extb = new List<string>();
			List<string> gamename = new List<string>();
			List<string> gametype = new List<string>();
			List<string> inputs = new List<string>();
			List<string> md5 = new List<string>();
			List<string> notcrc = new List<string>();
			List<string> notgamename = new List<string>();
			List<string> notgametype = new List<string>();
			List<string> notmd5 = new List<string>();
			List<string> notromname = new List<string>();
			List<string> notromtype = new List<string>();
			List<string> notsha1 = new List<string>();
			List<string> notstatus = new List<string>();
			List<string> romname = new List<string>();
			List<string> romtype = new List<string>();
			List<string> sha1 = new List<string>();
			List<string> status = new List<string>();

			// Determine which switches are enabled (with values if necessary)
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					// Feature flags
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-d":
					case "--d2d":
					case "--dfd":
						datFromDir = true;
						break;
					case "-es":
					case "--ext-split":
						splitByExt = true;
						break;
					case "-ex":
					case "--extract":
						extract = true;
						break;
					case "-hs":
					case "--hash-split":
						splitByHash = true;
						break;
					case "-ls":
					case "--lvl-split":
						splitByLevel = true;
						break;
					case "-re":
					case "--restore":
						restore = true;
						break;
					case "--script":
						// No-op for script mode, allowing for retaining the screen
						break;
					case "-ss":
					case "--sort":
						sort = true;
						break;
					case "-st":
					case "--stats":
						stats = true;
						break;
					case "-ts":
					case "--type-split":
						splitByType = true;
						break;
					case "-ve":
					case "--verify":
						verify = true;
						break;

					// User flags
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
					case "-ba":
					case "--base":
						basedat = true;
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
					case "-clean":
					case "--clean":
						cleanGameNames = true;
						break;
					case "-csv":
					case "--csv":
						statDatFormat |= StatDatFormat.CSV;
						break;
					case "-dd":
					case "--dedup":
						dedup = true;
						break;
					case "-del":
					case "--delete":
						delete = true;
						break;
					case "-df":
					case "--dat-fullnonmerged":
						splitType = SplitType.FullNonMerged;
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
					case "-dm":
					case "--dat-merged":
						splitType = SplitType.Merged;
						break;
					case "-dnm":
					case "--dat-nonmerged":
						splitType = SplitType.NonMerged;
						break;
					case "-ds":
					case "--dat-split":
						splitType = SplitType.Split;
						break;
					case "-f":
					case "--files":
						parseArchivesAsFiles = true;
						break;
					case "-gp":
					case "--game-prefix":
						datPrefix = true;
						break;
					case "-gzf":
					case "--gz-files":
						enableGzip = true;
						break;
					case "-ho":
					case "--hash-only":
						hashOnly = true;
						break;
					case "-html":
					case "--html":
						statDatFormat |= StatDatFormat.HTML;
						break;
					case "-in":
					case "--inverse":
						inverse = true;
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
					case "-nrun":
					case "--not-run":
						runnable = false;
						break;
					case "-ns":
					case "--noSHA1":
						noSHA1 = true;
						break;
					case "-oa":
					case "--output-all":
						datFormat |= DatFormat.ALL;
						break;
					case "-oam":
					case "--output-am":
						datFormat |= DatFormat.AttractMode;
						break;
					case "-oc":
					case "--output-cmp":
						datFormat |= DatFormat.ClrMamePro;
						break;
					case "-ocsv":
					case "--output-csv":
						datFormat |= DatFormat.CSV;
						break;
					case "-od":
					case "--output-dc":
						datFormat |= DatFormat.DOSCenter;
						break;
					case "-om":
					case "--output-miss":
						datFormat |= DatFormat.MissFile;
						break;
					case "-omd5":
					case "--output-md5":
						datFormat |= DatFormat.RedumpMD5;
						break;
					case "-ool":
					case "--output-ol":
						datFormat |= DatFormat.OfflineList;
						break;
					case "-or":
					case "--output-rc":
						datFormat |= DatFormat.RomCenter;
						break;
					case "-os":
					case "--output-sd":
						datFormat |= DatFormat.SabreDat;
						break;
					case "-osfv":
					case "--output-sfv":
						datFormat |= DatFormat.RedumpSFV;
						break;
					case "-osha1":
					case "--output-sha1":
						datFormat |= DatFormat.RedumpSHA1;
						break;
					case "-osl":
					case "--output-sl":
						datFormat |= DatFormat.SoftwareList;
						break;
					case "-otsv":
					case "--output-tsv":
						datFormat |= DatFormat.TSV;
						break;
					case "-ox":
					case "--output-xml":
						datFormat |= DatFormat.Logiqx;
						break;
					case "-q":
					case "--quotes":
						quotes = true;
						break;
					case "-qs":
					case "--quick":
						quickScan = true;
						break;
					case "-r":
					case "--roms":
						usegame = false;
						break;
					case "-rc":
					case "--rev-cascade":
						diffMode |= DiffMode.ReverseCascade;
						break;
					case "-rme":
					case "--rem-ext":
						remext = true;
						break;
					case "-ro":
					case "--romba":
						romba = true;
						break;
					case "-run":
					case "--runnable":
						runnable = true;
						break;
					case "-s":
					case "--short":
						shortname = true;
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
					case "-t7z":
					case "--t7z":
						outputFormat = OutputFormat.Torrent7Zip;
						break;
					case "-tar":
					case "--tar":
						outputFormat = OutputFormat.TapeArchive;
						break;
					case "-tgz":
					case "--tgz":
						outputFormat = OutputFormat.TorrentGzip;
						break;
					case "-tlrz":
					case "--tlrz":
						outputFormat = OutputFormat.TorrentLrzip;
						break;
					case "-trar":
					case "--trar":
						outputFormat = OutputFormat.TorrentRar;
						break;
					case "-trim":
					case "--trim":
						trim = true;
						break;
					case "-tsv":
					case "--tsv":
						statDatFormat |= StatDatFormat.TSV;
						break;
					case "-txz":
					case "--txz":
						outputFormat = OutputFormat.TorrentXZ;
						break;
					case "-tzip":
					case "--tzip":
						outputFormat = OutputFormat.TorrentZip;
						break;
					case "-ud":
					case "--update":
						update = true;
						break;
					case "-upd":
					case "--updated-dat":
						updateDat = true;
						break;
					case "-xof":
					case "--exclude-of":
						excludeOf = true;
						break;

					// User inputs
					case "-7z":
					case "--7z":
						if (!Int32.TryParse(args[++i], out sevenzip))
						{
							sevenzip = 1;
						}
						break;
					case "-ae":
					case "--add-ext":
						addext = args[++i];
						break;
					case "-au":
					case "--author":
						author = args[++i];
						break;
					case "-ca":
					case "--category=":
						category = args[++i];
						break;
					case "-co":
					case "--comment":
						comment = args[++i];
						break;
					case "-crc":
					case "--crc":
						crc.Add(args[++i]);
						break;
					case "-da":
					case "--date":
						i++;
						date = args[++i];
						break;
					case "-dat":
					case "--dat":
						if (!File.Exists(args[++i]))
						{
							_logger.Error("DAT must be a valid file: " + args[i]);
							_logger.Close();
							return;
						}
						datfiles.Add(args[++i]);
						break;
					case "-de":
					case "--desc":
						description = args[++i];
						break;
					case "-em":
					case "--email":
						email = args[++i];
						break;
					case "-exta":
					case "--exta":
						exta = args[++i];
						break;
					case "-extb":
					case "--extb":
						extb = args[++i];
						break;
					case "-fi":
					case "--filename":
						filename = args[++i];
						break;
					case "-fm":
					case "--forcemerge":
						forcemerge = args[++i];
						break;
					case "-fn":
					case "--forcend":
						forcend = args[++i];
						break;
					case "-fp":
					case "--forcepack":
						forcepack = args[++i];
						break;
					case "-gn":
					case "--game-name":
						gamename.Add(args[++i]);
						break;
					case "-gt":
					case "--game-type":
						gametype.Add(args[++i]);
						break;
					case "-gz":
					case "--gz":
						if (!Int32.TryParse(args[++i], out gz))
						{
							gz = 2;
						}
						break;
					case "-he":
					case "--header":
						header = args[++i];
						break;
					case "-hp":
					case "--homepage":
						homepage = args[++i];
						break;
					case "-is":
					case "--status":
						status.Add(args[++i]);
						break;
					case "-md5":
					case "--md5":
						md5.Add(args[++i]);
						break;
					case "-mt":
					case "--mt":
						Int32.TryParse(args[++i], out maxParallelism);
						break;
					case "-n":
					case "--name":
						name = args[++i];
						break;
					case "-ncrc":
					case "--not-crc":
						notcrc.Add(args[++i]);
						break;
					case "-ngn":
					case "--not-game":
						notgamename.Add(args[++i]);
						break;
					case "-ngt":
					case "--not-gtype":
						notgametype.Add(args[++i]);
						break;
					case "-nis":
					case "--not-status":
						notstatus.Add(args[++i]);
						break;
					case "-nmd5":
					case "--not-md5":
						notmd5.Add(args[++i]);
						break;
					case "-nrn":
					case "--not-rom":
						notromname.Add(args[++i]);
						break;
					case "-nrt":
					case "--not-type":
						notromtype.Add(args[++i]);
						break;
					case "-nsha1":
					case "--not-sha1":
						notsha1.Add(args[++i]);
						break;
					case "-out":
					case "--out":
						outDir = args[++i];
						break;
					case "-post":
					case "--postfix":
						postfix = args[++i];
						break;
					case "-pre":
					case "--prefix":
						prefix = args[++i];
						break;
					case "-rar":
					case "--rar":
						if (!Int32.TryParse(args[++i], out rar))
						{
							rar = 2;
						}
						break;
					case "-rd":
					case "--root-dir":
						root = args[++i];
						break;
					case "-rep":
					case "--rep-ext":
						repext = args[++i];
						break;
					case "-rn":
					case "--rom-name":
						romname.Add(args[++i]);
						break;
					case "-root":
					case "--root":
						rootdir = args[++i];
						break;
					case "-rt":
					case "--rom-type":
						romtype.Add(args[++i]);
						break;
					case "-seq":
					case "--equal":
						seq = GetSizeFromString(args[++i]);
						break;
					case "-sgt":
					case "--greater":
						sgt = GetSizeFromString(args[++i]);
						break;
					case "-sha1":
					case "--sha1":
						sha1.Add(args[++i]);
						break;
					case "-slt":
					case "--less":
						slt = GetSizeFromString(args[++i]);
						break;
					case "-t":
					case "--temp":
						tempDir = args[++i];
						break;
					case "-u":
					case "-url":
					case "--url":
						url = args[++i];
						break;
					case "-v":
					case "--version":
						version = args[++i];
						break;
					case "-zip":
					case "--zip":
						if (!Int32.TryParse(args[++i], out zip))
						{
							zip = 1;
						}
						break;
					default:
						string temparg = args[i].Replace("\"", String.Empty).Replace("file://", String.Empty);

						if (temparg.StartsWith("-") && temparg.Contains("="))
						{
							// Split the argument
							string[] split = temparg.Split('=');
							if (split[1] == null)
							{
								split[1] = String.Empty;
							}

							switch (split[0])
							{
								case "-7z":
								case "--7z":
									if (!Int32.TryParse(split[1], out sevenzip))
									{
										sevenzip = 1;
									}
									break;
								case "-ae":
								case "--add-ext":
									addext = split[1];
									break;
								case "-au":
								case "--author":
									author = split[1];
									break;
								case "-ca":
								case "--category=":
									category = split[1];
									break;
								case "-co":
								case "--comment":
									comment = split[1];
									break;
								case "-crc":
								case "--crc":
									crc.Add(split[1]);
									break;
								case "-da":
								case "--date":
									date = split[1];
									break;
								case "-dat":
								case "--dat":
									if (!File.Exists(split[1]))
									{
										_logger.Error("DAT must be a valid file: " + split[1]);
										_logger.Close();
										return;
									}
									datfiles.Add(split[1]);
									break;
								case "-de":
								case "--desc":
									description = split[1];
									break;
								case "-em":
								case "--email":
									email = split[1];
									break;
								case "-exta":
								case "--exta":
									exta = split[1];
									break;
								case "-extb":
								case "--extb":
									extb = split[1];
									break;
								case "-f":
								case "--filename":
									filename = split[1];
									break;
								case "-fm":
								case "--forcemerge":
									forcemerge = split[1];
									break;
								case "-fn":
								case "--forcend":
									forcend = split[1];
									break;
								case "-fp":
								case "--forcepack":
									forcepack = split[1];
									break;
								case "-gn":
								case "--game-name":
									gamename.Add(split[1]);
									break;
								case "-gt":
								case "--game-type":
									gametype.Add(split[1]);
									break;
								case "-gz":
								case "--gz":
									if (!Int32.TryParse(split[1], out gz))
									{
										gz = 2;
									}
									break;
								case "-h":
								case "--header":
									header = split[1];
									break;
								case "-hp":
								case "--homepage":
									homepage = split[1];
									break;
								case "-is":
								case "--status":
									status.Add(split[1]);
									break;
								case "-md5":
								case "--md5":
									md5.Add(split[1]);
									break;
								case "-mt":
								case "--mt":
									Int32.TryParse(split[1], out maxParallelism);
									break;
								case "-n":
								case "--name":
									name = split[1];
									break;
								case "-ncrc":
								case "--not-crc":
									notcrc.Add(split[1]);
									break;
								case "-ngn":
								case "--not-game":
									notgamename.Add(split[1]);
									break;
								case "-ngt":
								case "--not-gtype":
									notgametype.Add(split[1]);
									break;
								case "-nis":
								case "--not-status":
									notstatus.Add(split[1]);
									break;
								case "-nmd5":
								case "--not-md5":
									notmd5.Add(split[1]);
									break;
								case "-nrn":
								case "--not-rom":
									notromname.Add(split[1]);
									break;
								case "-nrt":
								case "--not-type":
									notromtype.Add(split[1]);
									break;
								case "-nsha1":
								case "--not-sha1":
									notsha1.Add(split[1]);
									break;
								case "-out":
								case "--out":
									outDir = split[1];
									break;
								case "-post":
								case "--postfix":
									postfix = split[1];
									break;
								case "-pre":
								case "--prefix":
									prefix = split[1];
									break;
								case "-r":
								case "--root":
									rootdir = split[1];
									break;
								case "-rar":
								case "--rar":
									if (!Int32.TryParse(split[1], out rar))
									{
										rar = 2;
									}
									break;
								case "-rd":
								case "--root-dir":
									root = split[1];
									break;
								case "-rep":
								case "--rep-ext":
									repext = split[1];
									break;
								case "-rn":
								case "--rom-name":
									romname.Add(split[1]);
									break;
								case "-rt":
								case "--rom-type":
									romtype.Add(split[1]);
									break;
								case "-seq":
								case "--equal":
									seq = GetSizeFromString(split[1]);
									break;
								case "-sgt":
								case "--greater":
									sgt = GetSizeFromString(split[1]);
									break;
								case "-sha1":
								case "--sha1":
									sha1.Add(split[1]);
									break;
								case "-slt":
								case "--less":
									slt = GetSizeFromString(split[1]);
									break;
								case "-t":
								case "--temp":
									tempDir = split[1];
									break;
								case "-u":
								case "-url":
								case "--url":
									url = split[1];
									break;
								case "-v":
								case "--version":
									version = split[1];
									break;
								case "-zip":
								case "--zip":
									if (!Int32.TryParse(split[1], out zip))
									{
										zip = 1;
									}
									break;
								default:
									if (File.Exists(temparg) || Directory.Exists(temparg))
									{
										inputs.Add(temparg);
									}
									else
									{
										_logger.Error("Invalid input detected: " + args[i]);
										_logger.Close();
										return;
									}
									break;
							}
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
						}
						else
						{
							_logger.Error("Invalid input detected: " + args[i]);
							_logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help("SabreTools");
				_logger.Close();
				return;
			}

			// If none of the feature flags is enabled, show the help screen
			if (!(datFromDir | extract | restore | sort | splitByExt | splitByHash | splitByLevel | splitByType | stats | update | verify))
			{
				_logger.Error("At least one feature switch must be enabled");
				_logger.Close();
				return;
			}

			// If more than one switch is enabled, show the help screen
			if (!(datFromDir ^ extract ^ restore ^ sort ^ splitByExt ^ splitByHash ^ splitByLevel ^ splitByType ^ stats ^ update ^ verify))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0
				&& (datFromDir || extract || restore || splitByExt || splitByHash || splitByLevel || splitByType || stats || update))
			{
				_logger.Error("This feature requires at least one input");
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
					datFormat,
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
					outDir,
					copyFiles,
					header,
					maxParallelism);
			}

			// If we're in header extract and remove mode
			else if (extract)
			{
				InitExtractRemoveHeader(inputs, outDir);
			}

			// If we're in header restore mode
			else if (restore)
			{
				InitReplaceHeader(inputs, outDir);
			}

			// If we're using the sorter
			else if (sort)
			{
				InitSort(datfiles, inputs, outDir, tempDir, quickScan, addFileDates, delete, inverse,
					outputFormat, romba, sevenzip, gz, rar, zip, updateDat, header, maxParallelism);
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

			// Split a SuperDAT by lowest available level
			else if (splitByLevel)
			{
				InitLevelSplit(inputs, outDir, shortname, basedat);
			}

			// Split a DAT by item type
			else if (splitByType)
			{
				InitTypeSplit(inputs, outDir);
			}

			// Get statistics on input files
			else if (stats)
			{
				InitStats(inputs, filename, outDir, single, showBaddumpColumn, showNodumpColumn, statDatFormat);
			}

			// Convert, update, merge, diff, and filter a DAT or folder of DATs
			else if (update)
			{
				InitUpdate(inputs, filename, name, description, rootdir, category, version, date, author, email, homepage, url, comment, header,
					superdat, forcemerge, forcend, forcepack, excludeOf, datFormat, usegame, prefix,
					postfix, quotes, repext, addext, remext, datPrefix, romba, merge, diffMode, inplace, skip, removeDateFromAutomaticName,
					gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, status, gametype,
					notgamename, notromname, notromtype, notcrc, notmd5, notsha1, notstatus, notgametype, runnable,
					splitType, trim, single, root, outDir, cleanGameNames, softlist, dedup, maxParallelism);
			}

			// If we're using the verifier
			else if (verify)
			{
				InitVerify(datfiles, inputs, tempDir, hashOnly, quickScan, header);
			}

			// If nothing is set, show the help
			else
			{
				Build.Help("SabreTools");
			}

			_logger.Close();
			return;
		}
	}
}
