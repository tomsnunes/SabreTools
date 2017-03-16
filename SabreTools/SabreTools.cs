using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Helper;
using SabreTools.Helper.Data;
using SabreTools.Helper.Dats;
using SabreTools.Helper.Help;

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
	/// <remarks>
	/// TODO: Wrap Parallel.ForEach loops in try/catch
	/// TODO: Find more foreach loops that are good candidates for Parallel.ForEach
	/// TODO: Look into async read/write to make things quicker. Ask edc for help?
	/// </remarks>
	public partial class SabreTools
	{
		// Private required variables
		private static Help _help;

		/// <summary>
		/// Start menu or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			Globals.Logger = new Logger(true, "sabretools.log");

			// Create a new Help object for this program
			_help = RetrieveHelp();

			// Get the location of the script tag, if it exists
			int scriptLocation = (new List<string>(args)).IndexOf("--script");

			// If output is being redirected or we are in script mode, don't allow clear screens
			if (!Console.IsOutputRedirected && scriptLocation == -1)
			{
				Console.Clear();
			}

			// Now we remove the script tag because it messes things up
			if (scriptLocation > -1)
			{
				List<string> newargs = new List<string>(args);
				newargs.RemoveAt(scriptLocation);
				args = newargs.ToArray();
			}

			Build.Start("SabreTools");

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				_help.OutputCredits();
				Globals.Logger.Close();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				_help.OutputGenericHelp();
				Globals.Logger.Close();
				return;
			}

			// Feature flags
			bool datFromDir = false,
				extract = false,
				restore = false,
				sort = false,
				sortDepot = false,
				splitByExt = false,
				splitByHash = false,
				splitByLevel = false,
				splitByType = false,
				stats = false,
				update = false,
				verify = false,
				verifyDepot = false;

			// User flags
			bool addBlankFilesForEmptyFolder = false,
				addFileDates = false,
				basedat = false,
				cleanGameNames = false,
				copyFiles = false,
				datPrefix = false,
				dedup = false,
				delete = false,
				descAsName = false,
				enableGzip = false,
				excludeOf = false,
				hashOnly = false,
				inplace = false,
				inverse = false,
				merge = false,
				oneGameOneRegion = false,
				parseArchivesAsFiles = false,
				quickScan = false,
				quotes = false,
				remext = false,
				removeDateFromAutomaticName = false,
				romba = false,
				showBaddumpColumn = false,
				showNodumpColumn = false,
				shortname = false,
				single = false,
				superdat = false,
				trim = false,
				skip = false,
				updateDat = false,
				usegame = true;
			DatFormat datFormat = 0x0;
			DiffMode diffMode = 0x0;
			Hash omitFromScan = Hash.SHA256 | Hash.SHA384 | Hash.SHA512; // Should be set to 0x0 later
			Hash stripHash = 0x0;
			OutputFormat outputFormat = OutputFormat.Folder;
			SplitType splitType = SplitType.None;
			StatDatFormat statDatFormat = 0x0;

			// User inputs
			int gz = 2,
				rar = 2,
				sevenzip = 1,
				zip = 1;
			string addext = "",
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
				outDir = "",
				postfix = "",
				prefix = "",
				repext = "",
				root = "",
				rootdir = null,
				tempDir = "",
				url = null,
				version = null;
			Filter filter = new Filter();
			List<string> datfiles = new List<string>();
			List<string> exta = new List<string>();
			List<string> extb = new List<string>();
			List<string> inputs = new List<string>();
			List<string> regions = new List<string>();

			// Get the first argument as a feature flag
			string feature = args[0];

			// Verify that the flag is valid
			if (!_help.TopLevelFlag(feature))
			{
				Globals.Logger.User("\"" + feature + "\" is not valid feature flag");
				_help.OutputIndividualFeature(feature);
				Globals.Logger.Close();
				return;
			}

			// Check the first argument for being a feature flag
			switch (feature)
			{
				case "-?":
				case "-h":
				case "--help":
					if (1 < args.Length)
					{
						_help.OutputIndividualFeature(args[1]);
					}
					else
					{
						_help.OutputGenericHelp();
					}
					Globals.Logger.Close();
					return;
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
				case "-ssd":
				case "--sort-depot":
					sortDepot = true;
					break;
				case "-st":
				case "--stats":
					stats = true;
					break;
				case "-ts":
				case "--type-split":
					splitByType = true;
					break;
				case "-ud":
				case "--update":
					update = true;
					break;
				case "-ve":
				case "--verify":
					verify = true;
					break;
				case "-ved":
				case "--verify-depot":
					verifyDepot = true;
					break;

				// If we don't have a valid flag, feed it through the help system
				default:
					_help.OutputIndividualFeature(feature);
					Globals.Logger.Close();
					return;
			}

			// Now get the proper name for the feature
			feature = _help.GetFeatureName(feature);

			// Determine which switches are enabled (with values if necessary)
			for (int i = 1; i < args.Length; i++)
			{
				// Verify that the current flag is proper for the feature
				if (!_help[feature].ValidateInput(args[i]))
				{
					Globals.Logger.Error("Invalid input detected: " + args[i]);
					_help.OutputIndividualFeature(feature);
					Globals.Logger.Close();
					return;
				}

				switch (args[i])
				{
					// User flags
					case "-1g1r":
					case "--1g1r":
						oneGameOneRegion = true;
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
					case "-dan":
					case "--desc-name":
						descAsName = true;
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
						omitFromScan |= Hash.MD5;
						break;
					case "-nrun":
					case "--not-run":
						filter.Runnable = false;
						break;
					case "-ns":
					case "--noSHA1":
						omitFromScan |= Hash.SHA1;
						break;
					case "-ns256":
					case "--noSHA256":
						omitFromScan &= ~Hash.SHA256; // This needs to be inverted later
						break;
					case "-ns384":
					case "--noSHA384":
						omitFromScan &= ~Hash.SHA384; // This needs to be inverted later
						break;
					case "-ns512":
					case "--noSHA512":
						omitFromScan &= ~Hash.SHA512; // This needs to be inverted later
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
					case "-ofg":
					case "--of-as-game":
						filter.IncludeOfInGame = true;
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
					case "-osha256":
					case "--output-sha256":
						datFormat |= DatFormat.RedumpSHA256;
						break;
					case "-osha384":
					case "--output-sha384":
						datFormat |= DatFormat.RedumpSHA384;
						break;
					case "-osha512":
					case "--output-sha512":
						datFormat |= DatFormat.RedumpSHA512;
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
					case "-rmd5":
					case "--rem-md5":
						stripHash |= Hash.MD5;
						break;
					case "-rme":
					case "--rem-ext":
						remext = true;
						break;
					case "-ro":
					case "--romba":
						romba = true;
						break;
					case "-rsha1":
					case "--rem-sha1":
						stripHash |= Hash.SHA1;
						break;
					case "-rsha256":
					case "--rem-sha256":
						stripHash |= Hash.SHA256;
						break;
					case "-rsha384":
					case "--rem-sha384":
						stripHash |= Hash.SHA384;
						break;
					case "-rsha512":
					case "--rem-sha512":
						stripHash |= Hash.SHA512;
						break;
					case "-run":
					case "--runnable":
						filter.Runnable = true;
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
					case "-upd":
					case "--update-dat":
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
						filter.CRCs.Add(args[++i]);
						break;
					case "-da":
					case "--date":
						i++;
						date = args[++i];
						break;
					case "-dat":
					case "--dat":
						i++;
						if (!File.Exists(args[i]) && !Directory.Exists(args[i]))
						{
							Globals.Logger.Error("Must be a valid file or folder of DATs: " + args[i]);
							Globals.Logger.Close();
							return;
						}
						datfiles.Add(args[i]);
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
						exta.Add(args[++i]);
						break;
					case "-extb":
					case "--extb":
						extb.Add(args[++i]);
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
						filter.GameNames.Add(args[++i]);
						break;
					case "-gt":
					case "--game-type":
						filter.MachineTypes |= Filter.GetMachineTypeFromString(args[++i]);
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
						filter.ItemStatuses |= Filter.GetStatusFromString(args[++i]);
						break;
					case "-md5":
					case "--md5":
						filter.MD5s.Add(args[++i]);
						break;
					case "-mt":
					case "--mt":
						if (Int32.TryParse(args[++i], out int mdop))
						{
							Globals.MaxDegreeOfParallelism = mdop;
						}
						else
						{
							Globals.MaxDegreeOfParallelism = 4;
						}
						break;
					case "-n":
					case "--name":
						name = args[++i];
						break;
					case "-ncrc":
					case "--not-crc":
						filter.NotCRCs.Add(args[++i]);
						break;
					case "-ngn":
					case "--not-game":
						filter.NotGameNames.Add(args[++i]);
						break;
					case "-ngt":
					case "--not-gtype":
						filter.NotMachineTypes |= Filter.GetMachineTypeFromString(args[++i]);
						break;
					case "-nis":
					case "--not-status":
						filter.NotItemStatuses |= Filter.GetStatusFromString(args[++i]);
						break;
					case "-nmd5":
					case "--not-md5":
						filter.NotMD5s.Add(args[++i]);
						break;
					case "-nrn":
					case "--not-rom":
						filter.NotRomNames.Add(args[++i]);
						break;
					case "-nrt":
					case "--not-type":
						filter.NotRomTypes.Add(args[++i]);
						break;
					case "-nsha1":
					case "--not-sha1":
						filter.NotSHA1s.Add(args[++i]);
						break;
					case "-nsha256":
					case "--not-sha256":
						filter.NotSHA256s.Add(args[++i]);
						break;
					case "-nsha384":
					case "--not-sha384":
						filter.NotSHA384s.Add(args[++i]);
						break;
					case "-nsha512":
					case "--not-sha512":
						filter.NotSHA512s.Add(args[++i]);
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
					case "-reg":
					case "--region":
						regions.Add(args[++i]);
						break;
					case "-rep":
					case "--rep-ext":
						repext = args[++i];
						break;
					case "-rn":
					case "--rom-name":
						filter.RomNames.Add(args[++i]);
						break;
					case "-root":
					case "--root":
						rootdir = args[++i];
						break;
					case "-rt":
					case "--rom-type":
						filter.RomTypes.Add(args[++i]);
						break;
					case "-sa":
					case "--scan-all":
						sevenzip = 0;
						gz = 0;
						rar = 0;
						zip = 0;
						break;
					case "-seq":
					case "--equal":
						filter.SizeEqualTo = GetSizeFromString(args[++i]);
						break;
					case "-sgt":
					case "--greater":
						filter.SizeGreaterThanOrEqual = GetSizeFromString(args[++i]);
						break;
					case "-sha1":
					case "--sha1":
						filter.SHA1s.Add(args[++i]);
						break;
					case "-sha256":
					case "--sha256":
						filter.SHA256s.Add(args[++i]);
						break;
					case "-sha384":
					case "--sha384":
						filter.SHA384s.Add(args[++i]);
						break;
					case "-sha512":
					case "--sha512":
						filter.SHA512s.Add(args[++i]);
						break;
					case "-slt":
					case "--less":
						filter.SizeLessThanOrEqual = GetSizeFromString(args[++i]);
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
						string temparg = args[i].Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-") && temparg.Contains("="))
						{
							// Split the argument
							string[] split = temparg.Split('=');
							
							// If we have a null second argument, we set it to be a blank
							if (split[1] == null)
							{
								split[1] = "";
							}
							// If we have more than 2 items in the split, we want to combine the other parts again
							else if (split.Length > 2)
							{
								split[1] = string.Join("=", split.Skip(1));
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
									filter.CRCs.Add(split[1]);
									break;
								case "-da":
								case "--date":
									date = split[1];
									break;
								case "-dat":
								case "--dat":
									if (!File.Exists(split[1]) && !Directory.Exists(split[1]))
									{
										Globals.Logger.Error("Must be a valid file or folder of DATs: " + split[1]);
										Globals.Logger.Close();
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
									exta.Add(split[1]);
									break;
								case "-extb":
								case "--extb":
									extb.Add(split[1]);
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
									filter.GameNames.Add(split[1]);
									break;
								case "-gt":
								case "--game-type":
									filter.MachineTypes |= Filter.GetMachineTypeFromString(split[1]);
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
									filter.ItemStatuses |= Filter.GetStatusFromString(split[1]);
									break;
								case "-md5":
								case "--md5":
									filter.MD5s.Add(split[1]);
									break;
								case "-mt":
								case "--mt":
									if (Int32.TryParse(split[1], out int odop))
									{
										Globals.MaxDegreeOfParallelism = odop;
									}
									else
									{
										Globals.MaxDegreeOfParallelism = 4;
									}
									break;
								case "-n":
								case "--name":
									name = split[1];
									break;
								case "-ncrc":
								case "--not-crc":
									filter.NotCRCs.Add(split[i]);
									break;
								case "-ngn":
								case "--not-game":
									filter.NotGameNames.Add(split[1]);
									break;
								case "-ngt":
								case "--not-gtype":
									filter.NotMachineTypes |= Filter.GetMachineTypeFromString(split[1]);
									break;
								case "-nis":
								case "--not-status":
									filter.NotItemStatuses |= Filter.GetStatusFromString(split[1]);
									break;
								case "-nmd5":
								case "--not-md5":
									filter.NotMD5s.Add(split[1]);
									break;
								case "-nrn":
								case "--not-rom":
									filter.NotRomNames.Add(split[1]);
									break;
								case "-nrt":
								case "--not-type":
									filter.NotRomTypes.Add(split[1]);
									break;
								case "-nsha1":
								case "--not-sha1":
									filter.NotSHA1s.Add(split[1]);
									break;
								case "-nsha256":
								case "--not-sha256":
									filter.NotSHA256s.Add(split[1]);
									break;
								case "-nsha384":
								case "--not-sha384":
									filter.NotSHA384s.Add(split[1]);
									break;
								case "-nsha512":
								case "--not-sha512":
									filter.NotSHA512s.Add(split[1]);
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
								case "-reg":
								case "--region":
									regions.Add(split[1]);
									break;
								case "-rep":
								case "--rep-ext":
									repext = split[1];
									break;
								case "-rn":
								case "--rom-name":
									filter.RomNames.Add(split[1]);
									break;
								case "-rt":
								case "--rom-type":
									filter.RomTypes.Add(split[1]);
									break;
								case "-seq":
								case "--equal":
									filter.SizeEqualTo = GetSizeFromString(split[1]);
									break;
								case "-sgt":
								case "--greater":
									filter.SizeGreaterThanOrEqual = GetSizeFromString(split[1]);
									break;
								case "-sha1":
								case "--sha1":
									filter.SHA1s.Add(split[1]);
									break;
								case "-sha256":
								case "--sha256":
									filter.SHA256s.Add(split[1]);
									break;
								case "-sha384":
								case "--sha384":
									filter.SHA384s.Add(split[1]);
									break;
								case "-sha512":
								case "--sha512":
									filter.SHA512s.Add(split[1]);
									break;
								case "-slt":
								case "--less":
									filter.SizeLessThanOrEqual = GetSizeFromString(split[1]);
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
										Globals.Logger.Error("Invalid input detected: " + args[i]);
										Globals.Logger.Close();
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
							Globals.Logger.Error("Invalid input detected: " + args[i]);
							Globals.Logger.Close();
							return;
						}
						break;
				}
			}

			// If none of the feature flags is enabled, show the help screen
			if (!(datFromDir | extract | restore | sort | sortDepot | splitByExt | splitByHash | splitByLevel | splitByType | stats | update | verify | verifyDepot))
			{
				Globals.Logger.Error("At least one feature switch must be enabled");
				_help.OutputGenericHelp();
				Globals.Logger.Close();
				return;
			}

			// If more than one switch is enabled, show the help screen
			if (!(datFromDir ^ extract ^ restore ^ sort ^ sortDepot ^ splitByExt ^ splitByHash ^ splitByLevel ^ splitByType ^ stats ^ update ^ verify ^ verifyDepot))
			{
				Globals.Logger.Error("Only one feature switch is allowed at a time");
				_help.OutputGenericHelp();
				Globals.Logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0
				&& (datFromDir || extract || restore || splitByExt || splitByHash || splitByLevel || splitByType || stats || update || verify || verifyDepot))
			{
				Globals.Logger.Error("This feature requires at least one input");
				_help.OutputIndividualFeature(feature);
				Globals.Logger.Close();
				return;
			}

			// Now take care of each mode in succesion

			// Create a DAT from a directory or set of directories
			if (datFromDir)
			{
				InitDatFromDir(inputs, filename, name, description, category, version, author, forcepack, excludeOf,
					datFormat, romba, superdat, omitFromScan, removeDateFromAutomaticName, parseArchivesAsFiles, enableGzip,
					addBlankFilesForEmptyFolder, addFileDates, tempDir, outDir, copyFiles, header);
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
					outputFormat, romba, sevenzip, gz, rar, zip, updateDat, header, splitType);
			}

			// If we're using the sorter from depot
			else if (sortDepot)
			{
				InitSortDepot(datfiles, inputs, outDir, tempDir, addFileDates, delete, inverse,
					outputFormat, romba, updateDat, header, splitType);
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
					superdat, forcemerge, forcend, forcepack, excludeOf, datFormat, usegame, prefix, postfix, quotes, repext, addext, remext,
					datPrefix, romba, merge, diffMode, inplace, skip, removeDateFromAutomaticName, filter, oneGameOneRegion, regions,
					splitType, trim, single, root, outDir, cleanGameNames, descAsName, dedup, stripHash);
			}

			// If we're using the verifier
			else if (verify)
			{
				InitVerify(datfiles, inputs, tempDir, hashOnly, quickScan, header, splitType);
			}

			// If we're using the depot verifier
			else if (verifyDepot)
			{
				InitVerifyDepot(datfiles, inputs, tempDir, header, splitType);
			}

			// If nothing is set, show the help
			else
			{
				_help.OutputGenericHelp();
			}

			Globals.Logger.Close();
			return;
		}
	}
}
