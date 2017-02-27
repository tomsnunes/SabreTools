using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Helper.Data;
using SabreTools.Helper.Dats;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
#endif

namespace SabreTools
{
	public partial class SabreTools
	{
		#region Init Methods

		/// <summary>
		/// Wrap creating a DAT file from files or a directory in parallel
		/// </summary>
		/// <param name="inputs">List of input filenames</param>
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="category">New category</param>
		/// <param name="version">New version</param>
		/// <param name="author">New author</param>
		/// <param name="forcepack">String representing the forcepacking flag</param>
		/// <param name="excludeOf">True if cloneof, romof, and sampleof fields should be omitted from output, false otherwise</param>
		/// <param name="datFormat">DatFormat to be used for outputting the DAT</param>
		/// <param name="romba">True to enable reading a directory like a Romba depot, false otherwise</param>
		/// <param name="superdat">True to enable SuperDAT-style reading, false otherwise</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="removeDateFromAutomaticName">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="parseArchivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="addBlankFilesForEmptyFolder">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addFileDates">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is default temp directory)</param>
		/// <param name="outDir">Name of the directory to output the DAT to (blank is the current directory)</param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		private static void InitDatFromDir(List<string> inputs,
			string filename,
			string name,
			string description,
			string category,
			string version,
			string author,
			string forcepack,
			bool excludeOf,
			DatFormat datFormat,
			bool romba,
			bool superdat,
			Hash omitFromScan,
			bool removeDateFromAutomaticName,
			bool parseArchivesAsFiles,
			bool enableGzip,
			bool addBlankFilesForEmptyFolder,
			bool addFileDates,
			string tempDir,
			string outDir,
			bool copyFiles,
			string headerToCheckAgainst,
			int maxDegreeOfParallelism)
		{
			ForcePacking fp = ForcePacking.None;
			switch (forcepack?.ToLowerInvariant())
			{
				case "none":
				default:
					fp = ForcePacking.None;
					break;
				case "zip":
					fp = ForcePacking.Zip;
					break;
				case "unzip":
					fp = ForcePacking.Unzip;
					break;
			}

			// Create a new DATFromDir object and process the inputs
			DatFile basedat = new DatFile
			{
				FileName = filename,
				Name = name,
				Description = description,
				Category = category,
				Version = version,
				Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Author = author,
				ForcePacking = fp,
				DatFormat = (datFormat == 0 ? DatFormat.Logiqx : datFormat),
				Romba = romba,
				ExcludeOf = excludeOf,
				Type = (superdat ? "SuperDAT" : ""),
			};

			// Clean the temp directory
			tempDir = (String.IsNullOrEmpty(tempDir) ? Path.GetTempPath() : tempDir);

			// For each input directory, create a DAT
			foreach (string path in inputs)
			{
				if (Directory.Exists(path) || File.Exists(path))
				{
					// Clone the base Dat for information
					DatFile datdata = new DatFile(basedat);

					string basePath = Path.GetFullPath(path);
					bool success = datdata.PopulateFromDir(basePath, omitFromScan, removeDateFromAutomaticName, parseArchivesAsFiles, enableGzip,
						addBlankFilesForEmptyFolder, addFileDates, tempDir, copyFiles, headerToCheckAgainst, maxDegreeOfParallelism, _logger);

					// If it was a success, write the DAT out
					if (success)
					{
						datdata.WriteToFile(outDir, _logger);
					}

					// Otherwise, show the help
					else
					{
						Console.WriteLine();
						_help.OutputIndividualFeature("DATFromDir");
					}
				}
			}
		}

		/// <summary>
		/// Wrap extracting headers
		/// </summary>
		/// <param name="inputs">Input file or folder names</param>
		/// <param name="outDir">Output directory to write new files to, blank defaults to rom folder</param>
		private static void InitExtractRemoveHeader(List<string> inputs, string outDir)
		{
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					FileTools.DetectSkipperAndTransform(input, outDir, _logger);
				}
				else if (Directory.Exists(input))
				{
					foreach (string sub in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						FileTools.DetectSkipperAndTransform(sub, outDir, _logger);
					}
				}
			}
		}

		/// <summary>
		/// Wrap splitting a DAT by 2 extensions
		/// </summary>
		/// <param name="inputs">Input files or folders to be split</param>
		/// <param name="exta">First extension to split on</param>
		/// <param name="extb">Second extension to split on</param>
		/// <param name="outDir">Output directory for the split files</param>
		private static void InitExtSplit(List<string> inputs, List<string> exta, List<string> extb, string outDir)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0, _logger);
					datFile.SplitByExt(outDir, Path.GetDirectoryName(input), exta, extb, _logger);
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0, _logger);
						datFile.SplitByExt(outDir, (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar), exta, extb, _logger);
					}
				}
				else
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					_help.OutputIndividualFeature("Extension Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap splitting a DAT by best available hashes
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outDir">Output directory for the split files</param>
		private static void InitHashSplit(List<string> inputs, string outDir)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0, _logger);
					datFile.SplitByHash(outDir, Path.GetDirectoryName(input), _logger);
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0, _logger);
						datFile.SplitByHash(outDir, (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar), _logger);
					}
				}
				else
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					_help.OutputIndividualFeature("Hash Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap replacing headers
		/// </summary>
		/// <param name="inputs">Input file or folder names</param>
		/// <param name="outDir">Output directory to write new files to, blank defaults to rom folder</param>
		private static void InitReplaceHeader(List<string> inputs, string outDir)
		{
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					FileTools.RestoreHeader(input, outDir, _logger);
				}
				else if (Directory.Exists(input))
				{
					foreach (string sub in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						FileTools.RestoreHeader(sub, outDir, _logger);
					}
				}
			}
		}

		/// <summary>
		/// Wrap splitting a SuperDAT by lowest available level
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outDir">Output directory for the split files</param>
		/// <param name="shortname">True if short filenames should be used, false otherwise</param>
		/// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise</param>
		private static void InitLevelSplit(List<string> inputs, string outDir, bool shortname, bool basedat)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0, _logger, keep: true);
					datFile.SplitByLevel(outDir, Path.GetDirectoryName(input), shortname, basedat, _logger);
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0, _logger, keep: true);
						datFile.SplitByLevel(outDir, (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar), shortname, basedat, _logger);
					}
				}
				else
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					_help.OutputIndividualFeature("Level Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap sorting files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		private static void InitSort(List<string> datfiles, List<string> inputs, string outDir, string tempDir, bool quickScan, bool date, bool delete,
			bool inverse, OutputFormat outputFormat, bool romba, int sevenzip, int gz, int rar, int zip, bool updateDat, string headerToCheckAgainst,
			SplitType splitType, int maxDegreeOfParallelism)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

			DateTime start = DateTime.Now;
			_logger.User("Populating internal DAT...");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, new Filter(), splitType, false /* trim */, false /* single */, null /* root */, _logger,
					keep: true, useTags: true);
			}
			_logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			datdata.RebuildGeneric(inputs, outDir, tempDir, quickScan, date, delete, inverse, outputFormat, romba, asl,
				updateDat, headerToCheckAgainst, maxDegreeOfParallelism, _logger);
		}

		/// <summary>
		/// Wrap sorting files from a depot using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		private static void InitSortDepot(List<string> datfiles, List<string> inputs, string outDir, string tempDir, bool date, bool delete,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, string headerToCheckAgainst, SplitType splitType, int maxDegreeOfParallelism)
		{
			DateTime start = DateTime.Now;
			_logger.User("Populating internal DAT...");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, new Filter(), splitType, false /* trim */, false /* single */, null /* root */, _logger,
					keep: true, useTags: true);
			}
			_logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			datdata.RebuildDepot(inputs, outDir, tempDir, date, delete, inverse, outputFormat, romba,
				updateDat, headerToCheckAgainst, maxDegreeOfParallelism, _logger);
		}

		/// <summary>
		/// Wrap getting statistics on a DAT or folder of DATs
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="filename">Name of the file to output to, blank for default</param>
		/// <param name="outDir">Output directory for the report files</param>
		/// <param name="single">True to show individual DAT statistics, false otherwise</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		/// <param name="statDatFormat">Set the statistics output format to use</param>
		private static void InitStats(List<string> inputs, string filename, string outDir, bool single, bool baddumpCol, bool nodumpCol, StatDatFormat statDatFormat)
		{
			DatFile.OutputStats(inputs, filename, outDir, single, baddumpCol, nodumpCol, statDatFormat, _logger);
		}

		/// <summary>
		/// Wrap splitting a DAT by item type
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outDir">Output directory for the split files</param>
		private static void InitTypeSplit(List<string> inputs, string outDir)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0, _logger);
					datFile.SplitByType(outDir, Path.GetFullPath(Path.GetDirectoryName(input)), _logger);
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0, _logger);
						datFile.SplitByType(outDir, Path.GetFullPath((input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar)), _logger);
					}
				}
				else
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					_help.OutputIndividualFeature("Type Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap converting and updating DAT file from any format to any format
		/// </summary>
		/// <param name="inputs">List of input filenames</param>
		/// /* Normal DAT header info */
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="rootdir">New rootdir</param>
		/// <param name="category">New category</param>
		/// <param name="version">New version</param>
		/// <param name="date">New date</param>
		/// <param name="author">New author</param>
		/// <param name="email">New email</param>
		/// <param name="homepage">New homepage</param>
		/// <param name="url">New URL</param>
		/// <param name="comment">New comment</param>
		/// <param name="header">New header</param>
		/// <param name="superdat">True to set SuperDAT type, false otherwise</param>
		/// <param name="forcemerge">None, Split, Full</param>
		/// <param name="forcend">None, Obsolete, Required, Ignore</param>
		/// <param name="forcepack">None, Zip, Unzip</param>
		/// <param name="excludeOf">True if cloneof, romof, and sampleof fields should be omitted from output, false otherwise</param>
		/// <param name="datFormat">Non-zero flag for output format, zero otherwise for default</param>
		/// /* Missfile-specific DAT info */
		/// <param name="usegame">True if games are to be used in output, false if roms are</param>
		/// <param name="prefix">Generic prefix to be added to each line</param>
		/// <param name="postfix">Generic postfix to be added to each line</param>
		/// <param name="quotes">Add quotes to each item</param>
		/// <param name="repext">Replace all extensions with another</param>
		/// <param name="addext">Add an extension to all items</param>
		/// <param name="remext">Remove all extensions</param>
		/// <param name="datprefix">Add the dat name as a directory prefix</param>
		/// <param name="romba">Output files in romba format</param>
		/// /* Merging and Diffing info */
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diffMode">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// /* Filtering info */
		/// <param name="filter">Pre-populated filter object for DAT filtering</param>
		/// /* Trimming info */
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// /* Output DAT info */
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
		/// <param name="dedup">True to dedupe the roms in the DAT, false otherwise (default)</param>
		/// <param name="stripHash">StripHash that represents the hash(es) that you want to remove from the output</param>
		/// /* Multithreading info */
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		private static void InitUpdate(List<string> inputs,
			/* Normal DAT header info */
			string filename,
			string name,
			string description,
			string rootdir,
			string category,
			string version,
			string date,
			string author,
			string email,
			string homepage,
			string url,
			string comment,
			string header,
			bool superdat,
			string forcemerge,
			string forcend,
			string forcepack,
			bool excludeOf,
			DatFormat datFormat,

			/* Missfile-specific DAT info */
			bool usegame,
			string prefix,
			string postfix,
			bool quotes,
			string repext,
			string addext,
			bool remext,
			bool datprefix,
			bool romba,

			/* Merging and Diffing info */
			bool merge,
			DiffMode diffMode,
			bool inplace,
			bool skip,
			bool bare,

			/* Filtering info */
			Filter filter,

			/* Trimming info */
			SplitType splitType,
			bool trim,
			bool single,
			string root,

			/* Output DAT info */
			string outDir,
			bool clean,
			bool descAsName,
			bool dedup,
			Hash stripHash,

			/* Multithreading info */
			int maxDegreeOfParallelism)
		{
			// Set the special flags
			ForceMerging fm = ForceMerging.None;
			if (!String.IsNullOrEmpty(forcemerge))
			{
				switch (forcemerge.ToLowerInvariant())
				{
					case "none":
						fm = ForceMerging.None;
						break;
					case "split":
						fm = ForceMerging.Split;
						break;
					case "full":
						fm = ForceMerging.Full;
						break;
					default:
						_logger.Warning(forcemerge + " is not a valid merge flag");
						break;
				}
			}

			ForceNodump fn = ForceNodump.None;
			if (!String.IsNullOrEmpty(forcend))
			{
				switch (forcend.ToLowerInvariant())
				{
					case "none":
						fn = ForceNodump.None;
						break;
					case "obsolete":
						fn = ForceNodump.Obsolete;
						break;
					case "required":
						fn = ForceNodump.Required;
						break;
					case "ignore":
						fn = ForceNodump.Ignore;
						break;
					default:
						_logger.Warning(forcend + " is not a valid nodump flag");
						break;
				}
			}

			ForcePacking fp = ForcePacking.None;
			if (!String.IsNullOrEmpty(forcepack))
			{
				switch (forcepack.ToLowerInvariant())
				{
					case "none":
						fp = ForcePacking.None;
						break;
					case "zip":
						fp = ForcePacking.Zip;
						break;
					case "unzip":
						fp = ForcePacking.Unzip;
						break;
					default:
						_logger.Warning(forcepack + " is not a valid packing flag");
						break;
				}
			}

			// Normalize the extensions
			addext = (addext == "" || addext.StartsWith(".") ? addext : "." + addext);
			repext = (repext == "" || repext.StartsWith(".") ? repext : "." + repext);

			// If we're in merge or diff mode and the names aren't set, set defaults
			if (merge || diffMode != 0)
			{
				// Get the values that will be used
				if (date == "")
				{
					date = DateTime.Now.ToString("yyyy-MM-dd");
				}
				if (name == "")
				{
					name = (diffMode != 0 ? "DiffDAT" : "MergeDAT") + (superdat ? "-SuperDAT" : "") + (dedup ? "-deduped" : "");
				}
				if (description == "")
				{
					description = (diffMode != 0 ? "DiffDAT" : "MergeDAT") + (superdat ? "-SuperDAT" : "") + (dedup ? " - deduped" : "");
					if (!bare)
					{
						description += " (" + date + ")";
					}
				}
				if (category == "" && diffMode != 0)
				{
					category = "DiffDAT";
				}
				if (author == "")
				{
					author = "SabreTools";
				}
			}

			// Populate the DatData object
			DatFile userInputDat = new DatFile
			{
				FileName = filename,
				Name = name,
				Description = description,
				RootDir = rootdir,
				Category = category,
				Version = version,
				Date = date,
				Author = author,
				Email = email,
				Homepage = homepage,
				Url = url,
				Comment = comment,
				Header = header,
				Type = (superdat ? "SuperDAT" : null),
				ForceMerging = fm,
				ForceNodump = fn,
				ForcePacking = fp,
				MergeRoms = dedup,
				ExcludeOf = excludeOf,
				DatFormat = datFormat,
				StripHash = stripHash,

				UseGame = usegame,
				Prefix = prefix,
				Postfix = postfix,
				Quotes = quotes,
				RepExt = repext,
				AddExt = addext,
				RemExt = remext,
				GameName = datprefix,
				Romba = romba,
			};
			
			userInputDat.DetermineUpdateType(inputs, outDir, merge, diffMode, inplace, skip, bare, clean, descAsName,
				filter, splitType, trim, single, root, maxDegreeOfParallelism, _logger);
		}

		/// <summary>
		/// Wrap verifying files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">Input directories to compare against</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		private static void InitVerify(List<string> datfiles, List<string> inputs, string tempDir,
			bool hashOnly, bool quickScan, string headerToCheckAgainst, SplitType splitType)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(1, 1, 1, 1);

			DateTime start = DateTime.Now;
			_logger.User("Populating internal DAT...");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, new Filter(), splitType, false /* trim */, false /* single */, null /* root */, _logger,
					keep: true, useTags: true);
			}
			_logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			datdata.VerifyGeneric(inputs, tempDir, hashOnly, quickScan, headerToCheckAgainst, _logger);
		}

		/// <summary>
		/// Wrap verifying files from a depot using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">Input directories to compare against</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		private static void InitVerifyDepot(List<string> datfiles, List<string> inputs, string tempDir,
			string headerToCheckAgainst, SplitType splitType)
		{
			DateTime start = DateTime.Now;
			_logger.User("Populating internal DAT...");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, new Filter(), splitType, false /* trim */, false /* single */, null /* root */, _logger,
					keep: true, useTags: true);
			}
			_logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			datdata.VerifyDepot(inputs, tempDir, headerToCheckAgainst, _logger);
		}

		#endregion
	}
}
