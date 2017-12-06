using System;
using System.Collections.Generic;
using System.Linq;

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

			// Feature flags
			bool datFromDir = false,
				extract = false,
				restore = false,
				sort = false,
				split = false,
				stats = false,
				update = false,
				verify = false;

			// User flags
			bool addBlankFilesForEmptyFolder = false,
				addFileDates = false,
				archivesAsFiles = false,
				basedat = false,
				chdsAsFiles = false,
				cleanGameNames = false,
				copyFiles = false,
				datPrefix = false,
				delete = false,
				depot = false,
				descAsName = false,
				excludeOf = false,
				hashOnly = false,
				inplace = false,
				inverse = false,
				nostore = false,
				quickScan = false,
				quotes = false,
				remext = false,
				removeDateFromAutomaticName = false,
				removeUnicode = false,
				romba = false,
				sceneDateStrip = false,
				showBaddumpColumn = false,
				showNodumpColumn = false,
				shortname = false,
				superdat = false,
				skip = false,
				updateDat = false,
				usegame = true;
			DatFormat datFormat = 0x0;
			DedupeType dedup = DedupeType.None;
			Hash omitFromScan = Hash.DeepHashes; // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
			Hash stripHash = 0x0;
			OutputFormat outputFormat = OutputFormat.Folder;
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
				outDir = null,
				postfix = "",
				prefix = "",
				repext = "",
				rootdir = null,
				tempDir = "",
				url = null,
				version = null;
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
					// Top-level features
					case "Help":
						// No-op as this should be caught
						break;
					case "DATFromDir":
						datFromDir = true;
						break;
					case "Extract":
						extract = true;
						break;
					case "Restore":
						restore = true;
						break;
					case "Script":
						// No-op as this should be caught
						break;
					case "Split":
						split = true;
						break;
					case "Sort":
						sort = true;
						break;
					case "Stats":
						stats = true;
						break;
					case "Update":
						update = true;
						break;
					case "Verify":
						verify = true;
						break;

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
					case "bare":
						removeDateFromAutomaticName = true;
						break;
					case "base":
						basedat = true;
						break;
					case "baddump-col":
						showBaddumpColumn = true;
						break;
					case "base-name":
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
					case "desc-name":
						descAsName = true;
						break;
					case "dedup":
						dedup = DedupeType.Full;
						break;
					case "delete":
						delete = true;
						break;
					case "depot":
						depot = true;
						break;
					case "dat-fullnonmerged":
						splitType = SplitType.FullNonMerged;
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
					case "-dat-merged":
						splitType = SplitType.Merged;
						break;
					case "dat-devnonmerged":
						splitType = SplitType.DeviceNonMerged;
						break;
					case "dat-nonmerged":
						splitType = SplitType.NonMerged;
						break;
					case "dat-split":
						splitType = SplitType.Split;
						break;
					case "ext":
						splittingMode |= SplittingMode.Extension;
						break;
					case "files":
						archivesAsFiles = true;
						break;
					case "game-dedup":
						dedup = DedupeType.Game;
						break;
					case "game-prefix":
						datPrefix = true;
						break;
					case "hash-only":
						hashOnly = true;
						break;
					case "html":
						statDatFormat |= StatReportFormat.HTML;
						break;
					case "hash":
						splittingMode |= SplittingMode.Hash;
						break;
					case "ignore-chd":
						chdsAsFiles = true;
						break;
					case "inverse":
						inverse = true;
						break;
					case "inplace":
						inplace = true;
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
					case "not-run":
						filter.Runnable = false;
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
					case "-no-store-header":
						nostore = true;
						break;
					case "output-all":
						datFormat |= DatFormat.ALL;
						break;
					case "output-am":
						datFormat |= DatFormat.AttractMode;
						break;
					case "output-cmp":
						datFormat |= DatFormat.ClrMamePro;
						break;
					case "output-csv":
						datFormat |= DatFormat.CSV;
						break;
					case "output-dc":
						datFormat |= DatFormat.DOSCenter;
						break;
					case "of-as-game":
						filter.IncludeOfInGame = true;
						break;
					case "output-lr":
						datFormat |= DatFormat.Listroms;
						break;
					case "output-miss":
						datFormat |= DatFormat.MissFile;
						break;
					case "output-md5":
						datFormat |= DatFormat.RedumpMD5;
						break;
					case "output-ol":
						datFormat |= DatFormat.OfflineList;
						break;
					case "output-rc":
						datFormat |= DatFormat.RomCenter;
						break;
					case "output-sd":
						datFormat |= DatFormat.SabreDat;
						break;
					case "output-sfv":
						datFormat |= DatFormat.RedumpSFV;
						break;
					case "output-sha1":
						datFormat |= DatFormat.RedumpSHA1;
						break;
					case "output-sha256":
						datFormat |= DatFormat.RedumpSHA256;
						break;
					case "output-sha384":
						datFormat |= DatFormat.RedumpSHA384;
						break;
					case "output-sha512":
						datFormat |= DatFormat.RedumpSHA512;
						break;
					case "output-sl":
						datFormat |= DatFormat.SoftwareList;
						break;
					case "output-tsv":
						datFormat |= DatFormat.TSV;
						break;
					case "output-xml":
						datFormat |= DatFormat.Logiqx;
						break;
					case "quotes":
						quotes = true;
						break;
					case "quick":
						quickScan = true;
						break;
					case "roms":
						usegame = false;
						break;
					case "reverse-base-name":
						updateMode |= UpdateMode.ReverseBaseReplace;
						break;
					case "rev-cascade":
						updateMode |= UpdateMode.DiffReverseCascade;
						break;
					case "rem-md5":
						stripHash |= Hash.MD5;
						break;
					case "rem-ext":
						remext = true;
						break;
					case "romba":
						romba = true;
						break;
					case "rem-sha1":
						stripHash |= Hash.SHA1;
						break;
					case "rem-sha256":
						stripHash |= Hash.SHA256;
						break;
					case "rem-sha384":
						stripHash |= Hash.SHA384;
						break;
					case "rem-sha512":
						stripHash |= Hash.SHA512;
						break;
					case "rem-uni":
						removeUnicode = true;
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
					case "superdat":
						superdat = true;
						break;
					case "scene-date-strip":
						sceneDateStrip = true;
						break;
					case "skip":
						skip = true;
						break;
					case "single":
						filter.Single = true;
						break;
					case "skiparc":
						skipFileType = SkipFileType.Archive;
						break;
					case "skipfile":
						skipFileType = SkipFileType.File;
						break;
					case "t7z":
						outputFormat = OutputFormat.Torrent7Zip;
						break;
					case "tar":
						outputFormat = OutputFormat.TapeArchive;
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
					case "type":
						splittingMode |= SplittingMode.Type;
						break;
					case "tsv":
						statDatFormat |= StatReportFormat.TSV;
						break;
					case "txz":
						outputFormat = OutputFormat.TorrentXZ;
						break;
					case "text":
						statDatFormat |= StatReportFormat.Textfile;
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
					case "exclude-of":
						excludeOf = true;
						break;

					// User inputs
					case "7z":
						sevenzip = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
					case "-add-ext":
						addext = (string)feat.Value.GetValue();
						break;
					case "author":
						author = (string)feat.Value.GetValue();
						break;
					case "base-dat":
						basePaths.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "category":
						category = (string)feat.Value.GetValue();
						break;
					case "comment":
						comment = (string)feat.Value.GetValue();
						break;
					case "crc":
						filter.CRCs.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "date":
						date = (string)feat.Value.GetValue();
						break;
					case "dat":
						if (!File.Exists((string)feat.Value.GetValue()) && !Directory.Exists((string)feat.Value.GetValue()))
						{
							Globals.Logger.Error("Must be a valid file or folder of DATs: {0}", (string)feat.Value.GetValue());
							Globals.Logger.Close();
							return;
						}
						datfiles.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "desc":
						description = (string)feat.Value.GetValue();
						break;
					case "email":
						email = (string)feat.Value.GetValue();
						break;
					case "exta":
						exta.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "extb":
						extb.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "filename":
						filename = (string)feat.Value.GetValue();
						break;
					case "forcemerge":
						forcemerge = (string)feat.Value.GetValue();
						break;
					case "forcend":
						forcend = (string)feat.Value.GetValue();
						break;
					case "forcepack":
						forcepack = (string)feat.Value.GetValue();
						break;
					case "game-name":
						filter.GameNames.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "game-type":
						filter.MachineTypes |= Utilities.GetMachineType((string)feat.Value.GetValue());
						break;
					case "gz":
						gz = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
					case "header":
						header = (string)feat.Value.GetValue();
						break;
					case "homepage":
						homepage = (string)feat.Value.GetValue();
						break;
					case "status":
						filter.ItemStatuses |= Utilities.GetItemStatus((string)feat.Value.GetValue());
						break;
					case "md5":
						filter.MD5s.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "mt":
						Globals.MaxThreads = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : Globals.MaxThreads;
						break;
					case "name":
						name = (string)feat.Value.GetValue();
						break;
					case "not-crc":
						filter.NotCRCs.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-game":
						filter.NotGameNames.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "not-gtype":
						filter.NotMachineTypes |= Utilities.GetMachineType((string)feat.Value.GetValue());
						break;
					case "not-status":
						filter.NotItemStatuses |= Utilities.GetItemStatus((string)feat.Value.GetValue());
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
						postfix = (string)feat.Value.GetValue();
						break;
					case "prefix":
						prefix = (string)feat.Value.GetValue();
						break;
					case "root":
						rootdir = (string)feat.Value.GetValue();
						break;
					case "rar":
						rar = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
					case "root-dir":
						filter.Root = (string)feat.Value.GetValue();
						break;
					case "rep-ext":
						repext = (string)feat.Value.GetValue();
						break;
					case "rom-name":
						filter.RomNames.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "rom-type":
						filter.RomTypes.AddRange((List<string>)feat.Value.GetValue());
						break;
					case "equal":
						filter.SizeEqualTo = Utilities.GetSizeFromString((string)feat.Value.GetValue());
						break;
					case "greater":
						filter.SizeGreaterThanOrEqual = Utilities.GetSizeFromString((string)feat.Value.GetValue());
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
					case "less":
						filter.SizeLessThanOrEqual = Utilities.GetSizeFromString((string)feat.Value.GetValue());
						break;
					case "temp":
						tempDir = (string)feat.Value.GetValue();
						break;
					case "url":
						url = (string)feat.Value.GetValue();
						break;
					case "version":
						version = (string)feat.Value.GetValue();
						break;
					case "zip":
						zip = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
						break;
				}
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0
				&& (datFromDir || extract || restore || split || stats || update || verify))
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
				InitDatFromDir(inputs, filename, name, description, category, version, author, email, homepage, url, comment,
					forcepack, excludeOf, sceneDateStrip, datFormat, romba, superdat, omitFromScan, removeDateFromAutomaticName, archivesAsFiles,
					skipFileType, addBlankFilesForEmptyFolder, addFileDates, tempDir, outDir, copyFiles, header, chdsAsFiles);
			}

			// If we're in header extract and remove mode
			else if (extract)
			{
				InitExtractRemoveHeader(inputs, outDir, nostore);
			}

			// If we're in header restore mode
			else if (restore)
			{
				InitReplaceHeader(inputs, outDir);
			}

			// If we're using the sorter
			else if (sort)
			{
				InitSort(datfiles, inputs, outDir, depot, quickScan, addFileDates, delete, inverse,
					outputFormat, romba, sevenzip, gz, rar, zip, updateDat, header, splitType, chdsAsFiles);
			}

			// Split a DAT by the split type
			else if (split)
			{
				InitSplit(inputs, outDir, inplace, datFormat, splittingMode, exta, extb, shortname, basedat);
			}

			// Get statistics on input files
			else if (stats)
			{
				InitStats(inputs, filename, outDir, filter.Single, showBaddumpColumn, showNodumpColumn, statDatFormat);
			}

			// Convert, update, merge, diff, and filter a DAT or folder of DATs
			else if (update)
			{
				InitUpdate(inputs, basePaths, filename, name, description, rootdir, category, version, date, author, email, homepage, url, comment, header,
					superdat, forcemerge, forcend, forcepack, excludeOf, sceneDateStrip, datFormat, usegame, prefix, postfix, quotes, repext, addext, remext,
					datPrefix, romba, updateMode, inplace, skip, removeDateFromAutomaticName, filter, splitType, outDir, cleanGameNames, removeUnicode,
					descAsName, dedup, stripHash);
			}

			// If we're using the verifier
			else if (verify)
			{
				InitVerify(datfiles, inputs, depot, hashOnly, quickScan, header, splitType, chdsAsFiles);
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
