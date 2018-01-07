using System;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

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
			_help = SabreTools.RetrieveHelp();

			// Get the location of the script tag, if it exists
			int scriptLocation = (new List<string>(args)).IndexOf("--script");

			// If output is being redirected or we are in script mode, don't allow clear screens
			if (!Console.IsOutputRedirected && scriptLocation == -1)
			{
				Console.Clear();
				Build.PrepareConsole("SabreTools");
			}

			// Now we remove the script tag because it messes things up
			if (scriptLocation > -1)
			{
				List<string> newargs = new List<string>(args);
				newargs.RemoveAt(scriptLocation);
				args = newargs.ToArray();
			}

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

			// User flags
			bool addBlankFilesForEmptyFolder = false,
				addFileDates = false,
				archivesAsFiles = false,
				basedat = false,
				chdsAsFiles = false,
				cleanGameNames = false,
				copyFiles = false,
				delete = false,
				depot = false,
				descAsName = false,
				hashOnly = false,
				inplace = false,
				inverse = false,
				nostore = false,
				quickScan = false,
				removeDateFromAutomaticName = false,
				removeUnicode = false,
				showBaddumpColumn = false,
				showNodumpColumn = false,
				shortname = false,
				skip = false,
				updateDat = false;
			Hash omitFromScan = Hash.DeepHashes; // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
			OutputFormat outputFormat = OutputFormat.Folder;
			ReplaceMode replaceMode = ReplaceMode.None;
			SkipFileType skipFileType = SkipFileType.None;
			SplittingMode splittingMode = SplittingMode.None;
			SplitType splitType = SplitType.None;
			StatReportFormat statDatFormat = StatReportFormat.None;
			UpdateMode updateMode = UpdateMode.None;

			// User inputs
			int gz = 1,
				rar = 1,
				sevenzip = 1,
				zip = 1;
			string outDir = null,
				tempDir = "";
			DatHeader datHeader = new DatHeader();
			Filter filter = new Filter();
			List<string> basePaths = new List<string>();
			List<string> datfiles = new List<string>();
			List<string> exta = new List<string>();
			List<string> extb = new List<string>();
			List<string> inputs = new List<string>();

			// Get the first argument as a feature flag
			string feature = args[0];

			// Verify that the flag is valid
			if (!_help.TopLevelFlag(feature))
			{
				Globals.Logger.User("'{0}' is not valid feature flag", feature);
				_help.OutputIndividualFeature(feature);
				Globals.Logger.Close();
				return;
			}

			// Now get the proper name for the feature
			feature = _help.GetFeatureName(feature);

			// If we had the help feature first
			if (feature == "Help")
			{
				// If we had something else after help
				if (args.Length > 1)
				{
					_help.OutputIndividualFeature(args[1]);
					Globals.Logger.Close();
					return;
				}
				// Otherwise, show generic help
				else
				{
					_help.OutputGenericHelp();
					Globals.Logger.Close();
					return;
				}
			}

			// Now verify that all other flags are valid
			for (int i = 1; i < args.Length; i++)
			{
				// Verify that the current flag is proper for the feature
				if (!_help[feature].ValidateInput(args[i]))
				{
					Globals.Logger.Error("Invalid input detected: {0}", args[i]);
					_help.OutputIndividualFeature(feature);
					Globals.Logger.Close();
					return;
				}

				// Special precautions for files and directories
				if (File.Exists(args[i]) || Directory.Exists(args[i]))
				{
					inputs.Add(args[i]);
				}
			}

			// Now loop through all inputs
			Dictionary<string, Feature> features = _help.GetEnabledFeatures();
			foreach (KeyValuePair<string, Feature> feat in features)
			{
				// Check all of the flag names and translate to arguments
				switch (feat.Key)
				{
					// User flags
					case "add-blank":
						addBlankFilesForEmptyFolder = true;
						break;
					case "add-date":
						addFileDates = true;
						break;
					case "against":
						updateMode |= UpdateMode.DiffAgainst;
						break;
					case "all-stats":
						statDatFormat = StatReportFormat.All;
						break;
					case "baddump-col":
						showBaddumpColumn = true;
						break;
					case "bare":
						removeDateFromAutomaticName = true;
						break;
					case "base":
						basedat = true;
						break;
					case "base-replace":
						updateMode |= UpdateMode.BaseReplace;
						break;
					case "cascade":
						updateMode |= UpdateMode.DiffCascade;
						break;
					case "copy-files":
						copyFiles = true;
						break;
					case "clean":
						cleanGameNames = true;
						break;
					case "csv":
						statDatFormat |= StatReportFormat.CSV;
						break;
					case "dat-devnonmerged":
						splitType = SplitType.DeviceNonMerged;
						break;
					case "dat-fullnonmerged":
						splitType = SplitType.FullNonMerged;
						break;
					case "dat-merged":
						splitType = SplitType.Merged;
						break;
					case "dat-nonmerged":
						splitType = SplitType.NonMerged;
						break;
					case "dat-split":
						splitType = SplitType.Split;
						break;
					case "dedup":
						datHeader.DedupeRoms = DedupeType.Full;
						break;
					case "delete":
						delete = true;
						break;
					case "depot":
						depot = true;
						break;
					case "desc-name":
						descAsName = true;
						break;
					case "diff":
						updateMode |= UpdateMode.AllDiffs;
						break;
					case "diff-du":
						updateMode |= UpdateMode.DiffDupesOnly;
						break;
					case "diff-in":
						updateMode |= UpdateMode.DiffIndividualsOnly;
						break;
					case "diff-nd":
						updateMode |= UpdateMode.DiffNoDupesOnly;
						break;
					case "exclude-of":
						datHeader.ExcludeOf = true;
						break;
					case "ext":
						splittingMode |= SplittingMode.Extension;
						break;
					case "files":
						archivesAsFiles = true;
						break;
					case "game-dedup":
						datHeader.DedupeRoms = DedupeType.Game;
						break;
					case "game-prefix":
						datHeader.GameName = true;
						break;
					case "hash":
						splittingMode |= SplittingMode.Hash;
						break; 
					case "hash-only":
						hashOnly = true;
						break;
					case "html":
						statDatFormat |= StatReportFormat.HTML;
						break;
					case "ignore-chd":
						chdsAsFiles = true;
						break;
					case "inplace":
						inplace = true;
						break;
					case "inverse":
						inverse = true;
						break;
					case "level":
						splittingMode |= SplittingMode.Level;
						break;
					case "merge":
						updateMode |= UpdateMode.Merge;
						break;
					case "nodump-col":
						showNodumpColumn = true;
						break;
					case "noMD5":
						omitFromScan |= Hash.MD5;
						break;
					case "noSHA1":
						omitFromScan |= Hash.SHA1;
						break;
					case "noSHA256":
						omitFromScan &= ~Hash.SHA256; // This needs to be inverted later
						break;
					case "noSHA384":
						omitFromScan &= ~Hash.SHA384; // This needs to be inverted later
						break;
					case "noSHA512":
						omitFromScan &= ~Hash.SHA512; // This needs to be inverted later
						break;
					case "not-run":
						filter.Runnable = false;
						break;
					case "no-store-header":
						nostore = true;
						break;
					case "of-as-game":
						filter.IncludeOfInGame = true;
						break;
					case "output-all":
						datHeader.DatFormat |= DatFormat.ALL;
						break;
					case "output-am":
						datHeader.DatFormat |= DatFormat.AttractMode;
						break;
					case "output-cmp":
						datHeader.DatFormat |= DatFormat.ClrMamePro;
						break;
					case "output-csv":
						datHeader.DatFormat |= DatFormat.CSV;
						break;
					case "output-dc":
						datHeader.DatFormat |= DatFormat.DOSCenter;
						break;
					case "output-lr":
						datHeader.DatFormat |= DatFormat.Listroms;
						break;
					case "output-miss":
						datHeader.DatFormat |= DatFormat.MissFile;
						break;
					case "output-md5":
						datHeader.DatFormat |= DatFormat.RedumpMD5;
						break;
					case "output-ol":
						datHeader.DatFormat |= DatFormat.OfflineList;
						break;
					case "output-rc":
						datHeader.DatFormat |= DatFormat.RomCenter;
						break;
					case "output-sd":
						datHeader.DatFormat |= DatFormat.SabreDat;
						break;
					case "output-sfv":
						datHeader.DatFormat |= DatFormat.RedumpSFV;
						break;
					case "output-sha1":
						datHeader.DatFormat |= DatFormat.RedumpSHA1;
						break;
					case "output-sha256":
						datHeader.DatFormat |= DatFormat.RedumpSHA256;
						break;
					case "output-sha384":
						datHeader.DatFormat |= DatFormat.RedumpSHA384;
						break;
					case "output-sha512":
						datHeader.DatFormat |= DatFormat.RedumpSHA512;
						break;
					case "output-sl":
						datHeader.DatFormat |= DatFormat.SoftwareList;
						break;
					case "output-tsv":
						datHeader.DatFormat |= DatFormat.TSV;
						break;
					case "output-xml":
						datHeader.DatFormat |= DatFormat.Logiqx;
						break;
					case "quick":
						quickScan = true;
						break;
					case "quotes":
						datHeader.Quotes = true;
						break;
					case "rem-md5":
						datHeader.StripHash |= Hash.MD5;
						break;
					case "rem-sha1":
						datHeader.StripHash |= Hash.SHA1;
						break;
					case "rem-sha256":
						datHeader.StripHash |= Hash.SHA256;
						break;
					case "rem-sha384":
						datHeader.StripHash |= Hash.SHA384;
						break;
					case "rem-sha512":
						datHeader.StripHash |= Hash.SHA512;
						break;
					case "rem-uni":
						removeUnicode = true;
						break;
					case "reverse-base-name":
						updateMode |= UpdateMode.ReverseBaseReplace;
						break;
					case "rev-cascade":
						updateMode |= UpdateMode.DiffReverseCascade;
						break;
					case "rem-ext":
						datHeader.RemoveExtension = true;
						break;
					case "romba":
						datHeader.Romba = true;
						break;
					case "roms":
						datHeader.UseRomName = true;
						break;
					case "runnable":
						filter.Runnable = true;
						break;
					case "short":
						shortname = true;
						break;
					case "scan-all":
						sevenzip = 0;
						gz = 0;
						rar = 0;
						zip = 0;
						break;
					case "scene-date-strip":
						datHeader.SceneDateStrip = true;
						break;
					case "skip":
						skip = true;
						break;
					case "skiparc":
						skipFileType = SkipFileType.Archive;
						break;
					case "skipfile":
						skipFileType = SkipFileType.File;
						break;
					case "single":
						filter.Single = true;
						break;
					case "superdat":
						datHeader.Type = "SuperDAT";
						break;
					case "t7z":
						outputFormat = OutputFormat.Torrent7Zip;
						break;
					case "tar":
						outputFormat = OutputFormat.TapeArchive;
						break;
					case "text":
						statDatFormat |= StatReportFormat.Textfile;
						break;
					case "tgz":
						outputFormat = OutputFormat.TorrentGzip;
						break;
					case "tlrz":
						outputFormat = OutputFormat.TorrentLRZip;
						break;
					case "tlz4":
						outputFormat = OutputFormat.TorrentLZ4;
						break;
					case "trar":
						outputFormat = OutputFormat.TorrentRar;
						break;
					case "trim":
						filter.Trim = true;
						break;
					case "tsv":
						statDatFormat |= StatReportFormat.TSV;
						break;
					case "type":
						splittingMode |= SplittingMode.Type;
						break;
					case "txz":
						outputFormat = OutputFormat.TorrentXZ;
						break;
					case "tzip":
						outputFormat = OutputFormat.TorrentZip;
						break;
					case "tzpaq":
						outputFormat = OutputFormat.TorrentZPAQ;
						break;
					case "tzstd":
						outputFormat = OutputFormat.TorrentZstd;
						break;
					case "update-dat":
						updateDat = true;
						break;
					case "update-desc":
						replaceMode |= ReplaceMode.Description;
						break;
					case "update-hashes":
						replaceMode |= ReplaceMode.Hash;
						break;
					case "update-manu":
						replaceMode |= ReplaceMode.Manufacturer;
						break;
					case "update-names":
						replaceMode |= ReplaceMode.ItemName;
						break;
					case "update-year":
						replaceMode |= ReplaceMode.Year;
						break;

					// User inputs
					case "7z":
						sevenzip = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
					case "add-ext":
						datHeader.AddExtension = (string)feat.Value.GetValue();
						break;
					case "author":
						datHeader.Author = (string)feat.Value.GetValue();
						break;
					case "base-dat":
						basePaths.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "category":
						datHeader.Category = (string)feat.Value.GetValue();
						break;
					case "comment":
						datHeader.Comment = (string)feat.Value.GetValue();
						break;
					case "crc":
						filter.CRCs.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "dat":
						datfiles.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "date":
						datHeader.Date = (string)feat.Value.GetValue();
						break;
					case "desc":
						datHeader.Description = (string)feat.Value.GetValue();
						break;
					case "email":
						datHeader.Email = (string)feat.Value.GetValue();
						break;
					case "equal":
						filter.SizeEqualTo = Utilities.GetSizeFromString((string)feat.Value.GetValue());
						break;
					case "exta":
						exta.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "extb":
						extb.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "filename":
						datHeader.FileName = (string)feat.Value.GetValue();
						break;
					case "forcemerge":
						datHeader.ForceMerging = Utilities.GetForceMerging((string)feat.Value.GetValue());
						break;
					case "forcend":
						datHeader.ForceNodump = Utilities.GetForceNodump((string)feat.Value.GetValue());
						break;
					case "forcepack":
						datHeader.ForcePacking = Utilities.GetForcePacking((string)feat.Value.GetValue());
						break;
					case "game-name":
						filter.GameNames.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "game-type":
						foreach (string mach in (List<string>)feat.Value.GetValue())
						{
							filter.MachineTypes |= Utilities.GetMachineType(mach);
						}
						break;
					case "greater":
						filter.SizeGreaterThanOrEqual = Utilities.GetSizeFromString((string)feat.Value.GetValue());
						break;
					case "gz":
						gz = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
					case "header":
						datHeader.Header = (string)feat.Value.GetValue();
						break;
					case "homepage":
						datHeader.Homepage = (string)feat.Value.GetValue();
						break;
					case "less":
						filter.SizeLessThanOrEqual = Utilities.GetSizeFromString((string)feat.Value.GetValue());
						break;
					case "md5":
						filter.MD5s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "mt":
						int val = (int)feat.Value.GetValue();
						if (val != Int32.MinValue)
						{
							Globals.MaxThreads = val;
						}
						break;
					case "name":
						datHeader.Name = (string)feat.Value.GetValue();
						break;
					case "not-crc":
						filter.NotCRCs.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-game":
						filter.NotGameNames.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-gtype":
						foreach (string nmach in (List<string>)feat.Value.GetValue())
						{
							filter.NotMachineTypes |= Utilities.GetMachineType(nmach);
						}
						break;
					case "not-status":
						foreach (string nstat in (List<string>)feat.Value.GetValue())
						{
							filter.NotItemStatuses |= Utilities.GetItemStatus(nstat);
						}
						break;
					case "not-md5":
						filter.NotMD5s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-rom":
						filter.NotRomNames.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-type":
						filter.NotRomTypes.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-sha1":
						filter.NotSHA1s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-sha256":
						filter.NotSHA256s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-sha384":
						filter.NotSHA384s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-sha512":
						filter.NotSHA512s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "out":
						outDir = (string)feat.Value.GetValue();
						break;
					case "postfix":
						datHeader.Postfix = (string)feat.Value.GetValue();
						break;
					case "prefix":
						datHeader.Prefix = (string)feat.Value.GetValue();
						break;
					case "rar":
						rar = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
					case "rep-ext":
						datHeader.ReplaceExtension = (string)feat.Value.GetValue();
						break;
					case "root":
						datHeader.RootDir = (string)feat.Value.GetValue();
						break;
					case "rom-name":
						filter.RomNames.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "rom-type":
						filter.RomTypes.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "root-dir":
						filter.Root = (string)feat.Value.GetValue();
						break;
					case "sha1":
						filter.SHA1s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "sha256":
						filter.SHA256s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "sha384":
						filter.SHA384s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "sha512":
						filter.SHA512s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "status":
						foreach (string stat in (List<string>)feat.Value.GetValue())
						{
							filter.ItemStatuses |= Utilities.GetItemStatus(stat);
						}
						break;
					case "temp":
						tempDir = (string)feat.Value.GetValue();
						break;
					case "url":
						datHeader.Url = (string)feat.Value.GetValue();
						break;
					case "version":
						datHeader.Version = (string)feat.Value.GetValue();
						break;
					case "zip":
						zip = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
				}
			}

			// Now take care of each mode in succesion
			switch (feature)
			{
				case "Help":
					// No-op as this should be caught
					break;
				// Create a DAT from a directory or set of directories
				case "DATFromDir":
					VerifyInputs(inputs, feature);
					InitDatFromDir(inputs, datHeader, omitFromScan, removeDateFromAutomaticName, archivesAsFiles, chdsAsFiles,
						skipFileType, addBlankFilesForEmptyFolder, addFileDates, tempDir, outDir, copyFiles);
					break;
				// If we're in header extract and remove mode
				case "Extract":
					VerifyInputs(inputs, feature);
					InitExtractRemoveHeader(inputs, outDir, nostore);
					break;
				// If we're in header restore mode
				case "Restore":
					VerifyInputs(inputs, feature);
					InitReplaceHeader(inputs, outDir);
					break;
				case "Script":
					// No-op as this should be caught
					break;
				// If we're using the sorter
				case "Sort":
					InitSort(datfiles, inputs, outDir, depot, quickScan, addFileDates, delete, inverse,
						outputFormat, datHeader.Romba, sevenzip, gz, rar, zip, updateDat, datHeader.Header, splitType, chdsAsFiles);
					break;
				// Split a DAT by the split type
				case "Split":
					VerifyInputs(inputs, feature);
					InitSplit(inputs, outDir, inplace, datHeader.DatFormat, splittingMode, exta, extb, shortname, basedat);
					break;
				// Get statistics on input files
				case "Stats":
					VerifyInputs(inputs, feature);
					InitStats(inputs, datHeader.FileName, outDir, filter.Single, showBaddumpColumn, showNodumpColumn, statDatFormat);
					break;
				// Convert, update, merge, diff, and filter a DAT or folder of DATs
				case "Update":
					VerifyInputs(inputs, feature);
					InitUpdate(inputs, basePaths, datHeader, updateMode, inplace, skip, removeDateFromAutomaticName, filter,
						splitType, outDir, cleanGameNames, removeUnicode, descAsName, replaceMode);
					break;
				// If we're using the verifier
				case "Verify":
					VerifyInputs(inputs, feature);
					InitVerify(datfiles, inputs, depot, hashOnly, quickScan, datHeader.Header, splitType, chdsAsFiles);
					break;
				// If nothing is set, show the help
				default:
					_help.OutputGenericHelp();
					break;
			}

			Globals.Logger.Close();
			return;
		}

		private static void VerifyInputs(List<string> inputs, string feature)
		{
			if (inputs.Count == 0)
			{
				Globals.Logger.Error("This feature requires at least one input");
				_help.OutputIndividualFeature(feature);
				Environment.Exit(0);
			}
		}
	}
}
