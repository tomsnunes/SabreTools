using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Helper.Data;
using SabreTools.Helper.Skippers;
using SabreTools.Helper.Tools;
using SharpCompress.Common;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
#endif

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Rebuilding and Verifying [MODULAR DONE, FOR NOW]

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="set">True to enable set building output, false otherwise</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildToOutput(List<string> inputs, string outDir, string tempDir, bool set, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst, int maxDegreeOfParallelism, Logger logger)
		{
			#region Perform setup

			// If the DAT is not populated and inverse is not set, inform the user and quit
			if (Count == 0 && !inverse)
			{
				logger.User("No entries were found to rebuild, exiting...");
				return false;
			}

			// Check that the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				outDir = Path.GetFullPath(outDir);
			}

			// Check the temp directory
			if (String.IsNullOrEmpty(tempDir))
			{
				tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			}

			// Then create or clean the temp directory
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			else
			{
				FileTools.CleanDirectory(tempDir);
			}

			// Preload the Skipper list
			int listcount = Skipper.List.Count;

			#endregion

			bool success = true;

			// Now choose the correct rebuilder
			if (set)
			{
				success = RebuildToOutputWithSets(inputs, outDir, tempDir, quickScan, date, delete, inverse,
					outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, maxDegreeOfParallelism, logger);
			}
			else
			{
				success = RebuildToOutputWithoutSets(inputs, outDir, tempDir, quickScan, date, delete, inverse,
					outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, maxDegreeOfParallelism, logger);
			}	

			return success;
		}

		/// <summary>
		/// Rebuild sets using CMP-style linear rebuilding
		/// </summary>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		private bool RebuildToOutputWithoutSets(List<string> inputs, string outDir, string tempDir, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst, int maxDegreeOfParallelism, Logger logger)
		{
			bool success = true;

			#region Rebuild from sources in order

			switch (outputFormat)
			{
				case OutputFormat.Folder:
					logger.User("Rebuilding all files to directory");
					break;
				case OutputFormat.TapeArchive:
					logger.User("Rebuilding all files to TAR");
					break;
				case OutputFormat.Torrent7Zip:
					logger.User("Rebuilding all files to Torrent7Z");
					break;
				case OutputFormat.TorrentGzip:
					logger.User("Rebuilding all files to TorrentGZ");
					break;
				case OutputFormat.TorrentLrzip:
					logger.User("Rebuilding all files to TorrentLRZ");
					break;
				case OutputFormat.TorrentRar:
					logger.User("Rebuilding all files to TorrentRAR");
					break;
				case OutputFormat.TorrentXZ:
					logger.User("Rebuilding all files to TorrentXZ");
					break;
				case OutputFormat.TorrentZip:
					logger.User("Rebuilding all files to TorrentZip");
					break;
			}
			DateTime start = DateTime.Now;

			// Now loop through all of the files in all of the inputs
			foreach (string input in inputs)
			{
				// If the input is a file
				if (File.Exists(input))
				{
					logger.User("Checking file: '" + input + "'");
					RebuildToOutputWithoutSetsHelper(input, outDir, tempDir, quickScan, date, delete, inverse,
						outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, maxDegreeOfParallelism, logger);
				}

				// If the input is a directory
				else if (Directory.Exists(input))
				{
					logger.User("Checking directory: '" + input + "'");
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						logger.User("Checking file: '" + file + "'");
						RebuildToOutputWithoutSetsHelper(file, outDir, tempDir, quickScan, date, delete, inverse,
							outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, maxDegreeOfParallelism, logger);
					}
				}
			}

			logger.User("Rebuilding complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			#endregion

			return success;
		}

		/// <summary>
		/// Attempt to add a file to the output if it matches
		/// </summary>
		/// <param name="file">Name of the file to process</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		private void RebuildToOutputWithoutSetsHelper(string file, string outDir, string tempDir, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst, int maxDegreeOfParallelism, Logger logger)
		{
			// If we somehow have a null filename, return
			if (file == null)
			{
				return;
			}

			// Define the temporary directory
			string tempSubDir = Path.GetFullPath(Path.Combine(tempDir, Path.GetRandomFileName())) + Path.DirectorySeparatorChar;

			// Set the deletion variables
			bool usedExternally = false;
			bool usedInternally = false;

			// Get the required scanning level for the file
			bool shouldExternalProcess = false;
			bool shouldInternalProcess = false;
			ArchiveTools.GetInternalExternalProcess(file, archiveScanLevel, logger, out shouldExternalProcess, out shouldInternalProcess);

			// If we're supposed to scan the file externally
			if (shouldExternalProcess)
			{
				Rom rom = FileTools.GetFileInfo(file, logger, noMD5: quickScan, noSHA1: quickScan, header: headerToCheckAgainst);
				usedExternally = RebuildToOutputWithoutSetsIndividual(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
					romba, updateDat, false /* isZip */, headerToCheckAgainst, logger);
			}

			// If we're supposed to scan the file internally
			if (shouldInternalProcess)
			{
				// If quickscan is set, do so
				if (quickScan)
				{
					List<Rom> extracted = ArchiveTools.GetArchiveFileInfo(file, logger);
					usedInternally = true;

					foreach (Rom rom in extracted)
					{
						usedInternally &= RebuildToOutputWithoutSetsIndividual(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
							romba, updateDat, true /* isZip */, headerToCheckAgainst, logger);
					}
				}
				// Otherwise, attempt to extract the files to the temporary directory
				else
				{
					bool encounteredErrors = ArchiveTools.ExtractArchive(file, tempSubDir, archiveScanLevel, logger);

					// If the file was an archive and was extracted successfully, check it
					if (!encounteredErrors)
					{
						logger.Verbose(Path.GetFileName(file) + " treated like an archive");
						List<string> extracted = Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories).ToList();
						foreach (string entry in extracted)
						{
							Rom rom = FileTools.GetFileInfo(entry, logger, noMD5: quickScan, noSHA1: quickScan, header: headerToCheckAgainst);
							RebuildToOutputWithoutSetsIndividual(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
								romba, updateDat, false /* isZip */, headerToCheckAgainst, logger);

							// Now we want to remove the file from the temp directory
							try
							{
								File.Delete(entry);
							}
							catch { }
						}

						// If the temp directory is empty, we assume that everything inside was used
						if (Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories).Count() == 0)
						{
							usedInternally = true;
						}
					}
					// Otherwise, just get the info on the file itself
					else if (File.Exists(file))
					{
						Rom rom = FileTools.GetFileInfo(file, logger, noMD5: quickScan, noSHA1: quickScan, header: headerToCheckAgainst);
						usedExternally = RebuildToOutputWithoutSetsIndividual(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
							romba, updateDat, false /* isZip */, headerToCheckAgainst, logger);
					}
				}
			}

			// If we are supposed to delete the file, do so
			if (delete && (usedExternally || usedInternally))
			{
				try
				{
					File.Delete(file);
				}
				catch { }
			}

			// Now delete the temp directory
			try
			{
				Directory.Delete(tempSubDir, true);
			}
			catch { }
		}

		/// <summary>
		/// Find duplicates and rebuild individual files to output
		/// </summary>
		/// <param name="rom">Information for the current file to rebuild from</param>
		/// <param name="file">Name of the file to process</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="isZip">True if the input file is an archive, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the file was able to be rebuilt, false otherwise</returns>
		private bool RebuildToOutputWithoutSetsIndividual(Rom rom, string file, string outDir, string tempDir, bool date,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, bool isZip, string headerToCheckAgainst, Logger logger)
		{
			// Set the output value
			bool rebuilt = false;

			// Find if the file has duplicates in the DAT
			bool hasDuplicates = rom.HasDuplicates(this, logger);

			// If it has duplicates and we're not filtering or we have no duplicates and we're filtering, rebuild it
			if (hasDuplicates ^ inverse)
			{
				// Get the list of duplicates to rebuild to
				List<DatItem> dupes = rom.GetDuplicates(this, logger, remove: updateDat);

				// If we don't have any duplicates, continue
				if (dupes.Count == 0)
				{
					return rebuilt;
				}

				// If we have an archive input, get the real name of the file to use
				if (isZip)
				{
					// Otherwise, extract the file to the temp folder
					file = ArchiveTools.ExtractItem(file, rom.Name, tempDir, logger);
				}				

				// If we couldn't extract the file, then continue,
				if (String.IsNullOrEmpty(file))
				{
					return rebuilt;
				}

				// Now loop through the list and rebuild accordingly
				foreach (Rom item in dupes)
				{
					rebuilt = true;

					switch (outputFormat)
					{
						case OutputFormat.Folder:
							string outfile = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(item.Machine.Name), item.Name);

							// Make sure the output folder is created
							Directory.CreateDirectory(Path.GetDirectoryName(outfile));

							// Now copy the file over
							try
							{
								File.Copy(file, outfile);
								if (date && !String.IsNullOrEmpty(item.Date))
								{
									File.SetCreationTime(outfile, DateTime.Parse(item.Date));
								}

								rebuilt &= true;
							}
							catch
							{
								rebuilt &= false;
							}

							break;
						case OutputFormat.TapeArchive:
							rebuilt &= ArchiveTools.WriteTAR(file, outDir, item, logger, date: date);
							break;
						case OutputFormat.Torrent7Zip:
							break;
						case OutputFormat.TorrentGzip:
							rebuilt &= ArchiveTools.WriteTorrentGZ(file, outDir, romba, logger);
							break;
						case OutputFormat.TorrentLrzip:
							break;
						case OutputFormat.TorrentRar:
							break;
						case OutputFormat.TorrentXZ:
							break;
						case OutputFormat.TorrentZip:
							rebuilt &= ArchiveTools.WriteTorrentZip(file, outDir, item, logger, date: date);
							break;
					}

					// And now clear the temp folder to get rid of any transient files
					try
					{
						Directory.Delete(tempDir, true);
					}
					catch { }
				}
			}

			return rebuilt;
		}

		/// <summary>
		/// Rebuild sets using RV-style set rebuilding
		/// </summary>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		private bool RebuildToOutputWithSets(List<string> inputs, string outDir, string tempDir, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst, int maxDegreeOfParallelism, Logger logger)
		{
			bool success = true;
			DatFile matched = new DatFile();
			List<string> files = new List<string>();

			#region Retrieve a list of all files

			logger.User("Retrieving list all files from input");
			DateTime start = DateTime.Now;

			// Create a list of just files from inputs
			Parallel.ForEach(inputs,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, },
				input => {
					if (File.Exists(input))
					{
						logger.Verbose("File found: '" + input + "'");
						files.Add(Path.GetFullPath(input));
					}
					else if (Directory.Exists(input))
					{
						logger.Verbose("Directory found: '" + input + "'");
						Parallel.ForEach(Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories),
							new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, },
							file =>
							{
								logger.Verbose("File found: '" + file + "'");
								files.Add(Path.GetFullPath(file));
							});
					}
					else
					{
						logger.Error("'" + input + "' is not a file or directory!");
					}
				});
			logger.User("Retrieving complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			#endregion

			DatFile current = new DatFile();
			Dictionary<string, SkipperRule> fileToSkipperRule = new Dictionary<string, SkipperRule>();

			#region Create a dat from input files

			logger.User("Getting hash information for all input files");
			start = DateTime.Now;

			// Now that we have a list of just files, we get a DAT from the input files
			Parallel.ForEach(files,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				file =>
				{
					// If we somehow have a null filename, return
					if (file == null)
					{
						return;
					}

					// Define the temporary directory
					string tempSubDir = Path.GetFullPath(Path.Combine(tempDir, Path.GetRandomFileName())) + Path.DirectorySeparatorChar;

					// Get the required scanning level for the file
					bool shouldExternalProcess = false;
					bool shouldInternalProcess = false;
					ArchiveTools.GetInternalExternalProcess(file, archiveScanLevel, logger, out shouldExternalProcess, out shouldInternalProcess);

					// If we're supposed to scan the file externally
					if (shouldExternalProcess)
					{
						Rom rom = FileTools.GetFileInfo(file, logger, noMD5: quickScan, noSHA1: quickScan, header: headerToCheckAgainst);
						rom.Name = Path.GetFullPath(file);
						current.Add(rom.Size + "-" + rom.CRC, rom);

						// If we had a header, we want the full file information too
						if (headerToCheckAgainst != null)
						{
							rom = FileTools.GetFileInfo(file, logger, noMD5: quickScan, noSHA1: quickScan);
							rom.Name = Path.GetFullPath(file);
							current.Add(rom.Size + "-" + rom.CRC, rom);
						}
					}

					// If we're supposed to scan the file internally
					if (shouldInternalProcess)
					{
						// If quickscan is set, do so
						if (quickScan)
						{
							List<Rom> extracted = ArchiveTools.GetArchiveFileInfo(file, logger);

							foreach (Rom rom in extracted)
							{
								Rom newrom = rom;
								newrom.Machine = new Machine(Path.GetFullPath(file), "");
								current.Add(rom.Size + "-" + rom.CRC, newrom);
							}
						}
						// Otherwise, attempt to extract the files to the temporary directory
						else
						{
							bool encounteredErrors = ArchiveTools.ExtractArchive(file, tempSubDir, archiveScanLevel, logger);

							// If the file was an archive and was extracted successfully, check it
							if (!encounteredErrors)
							{
								logger.Verbose(Path.GetFileName(file) + " treated like an archive");
								List<string> extracted = Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories).ToList();
								Parallel.ForEach(extracted,
									new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
									entry =>
									{
										Rom rom = FileTools.GetFileInfo(entry, logger, noMD5: quickScan, noSHA1: quickScan, header: headerToCheckAgainst);
										rom.Machine = new Machine(Path.GetFullPath(file), "");
										current.Add(rom.Size + "-" + rom.CRC, rom);

										// If we had a header, we want the full file information too
										if (headerToCheckAgainst != null)
										{
											rom = FileTools.GetFileInfo(file, logger, noMD5: quickScan, noSHA1: quickScan);
											rom.Machine = new Machine(Path.GetFullPath(file), "");
											current.Add(rom.Size + "-" + rom.CRC, rom);
										}
									});
							}
							// Otherwise, just get the info on the file itself
							else if (File.Exists(file))
							{
								Rom rom = FileTools.GetFileInfo(file, logger, noMD5: quickScan, noSHA1: quickScan, header: headerToCheckAgainst);
								rom.Name = Path.GetFullPath(file);
								current.Add(rom.Size + "-" + rom.CRC, rom);
							}
						}
					}

					// Now delete the temp directory
					try
					{
						Directory.Delete(tempSubDir, true);
					}
					catch { }
				});

			logger.User("Getting hash information complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			#endregion

			// Create a mapping from destination file to source file
			Dictionary<DatItem, DatItem> toFromMap = new Dictionary<DatItem, DatItem>();

			#region Find all required files for rebuild

			logger.User("Determining files to rebuild");
			start = DateTime.Now;

			// Order the DATs by hash first to make things easier
			logger.User("Sorting input DAT...");
			BucketByCRC(false, logger, output: false);
			logger.User("Sorting found files...");
			current.BucketByCRC(false, logger, output: false);

			// Now loop over and find all files that need to be rebuilt
			List<string> keys = current.Keys.ToList();
			Parallel.ForEach(keys,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				key =>
				{
					// If we are using the DAT as a filter, treat the files one way
					if (inverse)
					{
						// Check for duplicates
						List<DatItem> datItems = current[key];
						foreach (Rom rom in datItems)
						{
							// If the rom has duplicates, we skip it
							if (rom.HasDuplicates(this, logger))
							{
								return;
							}

							// Otherwise, map the file to itself
							try
							{
								Rom newrom = new Rom
								{
									Name = rom.Name.Remove(0, Path.GetDirectoryName(rom.Name).Length),
									Size = rom.Size,
									CRC = rom.CRC,
									MD5 = rom.MD5,
									SHA1 = rom.SHA1,

									Machine = new Machine
									{
										Name = Path.GetFileNameWithoutExtension(rom.Machine.Name),
									},
								};
								newrom.Name = newrom.Name.Remove(0, (newrom.Name.StartsWith("\\") || newrom.Name.StartsWith("/") ? 1 : 0));

								lock (toFromMap)
								{
									toFromMap.Add(newrom, rom);
								}
							}
							catch { }
						}
					}

					// Otherwise, treat it like a standard rebuild
					else
					{
						// If the input DAT doesn't have the key, then nothing from the current DAT are there
						if (!ContainsKey(key))
						{
							return;
						}

						// Otherwise, we try to find duplicates
						List<DatItem> datItems = current[key];
						foreach (Rom rom in datItems)
						{
							List<DatItem> found = rom.GetDuplicates(this, logger, false);

							// Now add all of the duplicates mapped to the current file
							foreach (Rom mid in found)
							{
								try
								{
									lock (toFromMap)
									{
										toFromMap.Add(mid, rom);
									}
								}
								catch { }
							}
						}
					}
				});

			logger.User("Determining complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			#endregion

			// Now bucket the list of keys by game so that we can rebuild properly
			SortedDictionary<string, List<DatItem>> keysGroupedByGame = BucketListByGame(toFromMap.Keys.ToList(), false, true, logger, output: false);

			#region Rebuild games in order

			switch (outputFormat)
			{
				case OutputFormat.Folder:
					logger.User("Rebuilding all files to directory");
					break;
				case OutputFormat.TapeArchive:
					logger.User("Rebuilding all files to TAR");
					break;
				case OutputFormat.Torrent7Zip:
					logger.User("Rebuilding all files to Torrent7Z");
					break;
				case OutputFormat.TorrentGzip:
					logger.User("Rebuilding all files to TorrentGZ");
					break;
				case OutputFormat.TorrentLrzip:
					logger.User("Rebuilding all files to TorrentLRZ");
					break;
				case OutputFormat.TorrentRar:
					logger.User("Rebuilding all files to TorrentRAR");
					break;
				case OutputFormat.TorrentXZ:
					logger.User("Rebuilding all files to TorrentXZ");
					break;
				case OutputFormat.TorrentZip:
					logger.User("Rebuilding all files to TorrentZip");
					break;
			}
			start = DateTime.Now;

			// Now loop through the keys and create the correct output items
			List<string> games = keysGroupedByGame.Keys.ToList();
			Parallel.ForEach(games,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				game =>
				{
					// Define the temporary directory
					string tempSubDir = Path.GetFullPath(Path.Combine(tempDir, Path.GetRandomFileName())) + Path.DirectorySeparatorChar;

					// Create an empty list for getting paths for rebuilding
					List<string> pathsToFiles = new List<string>();

					// Loop through all of the matched items in the game
					List<DatItem> itemsInGame = keysGroupedByGame[game];
					List<Rom> romsInGame = new List<Rom>();
					foreach (Rom rom in itemsInGame)
					{
						// Get the rom that's mapped to this item
						Rom source = (Rom)toFromMap[rom];

						// If we have an empty rom or machine, there was an issue
						if (source == null || source.Machine == null || source.Machine.Name == null)
						{
							continue;
						}

						// If the file is in an archive, we need to treat it specially
						string machinename = source.Machine.Name.ToLowerInvariant();
						if (machinename.EndsWith(".7z")
							|| machinename.EndsWith(".gz")
							|| machinename.EndsWith(".rar")
							|| machinename.EndsWith(".zip"))
						{
							string tempPath = ArchiveTools.ExtractItem(source.Machine.Name, Path.GetFileName(source.Name), tempSubDir, logger);
							pathsToFiles.Add(tempPath);
						}

						// Otherwise, we want to just add the full path
						else
						{
							pathsToFiles.Add(source.Name);
						}

						// If the size doesn't match, then we add the CRC as a postfix to the file
						Rom fi = FileTools.GetFileInfo(pathsToFiles.Last(), logger);
						if (fi.Size != source.Size)
						{
							rom.Name = Path.GetDirectoryName(rom.Name)
										+ (String.IsNullOrEmpty(Path.GetDirectoryName(rom.Name)) ? "" : Path.DirectorySeparatorChar.ToString())
										+ Path.GetFileNameWithoutExtension(rom.Name)
										+ " (" + fi.CRC + ")"
										+ Path.GetExtension(rom.Name);
							rom.CRC = fi.CRC;
							rom.Size = fi.Size;
						}

						// Now add the rom to the output list
						romsInGame.Add(rom);
					}

					// And now rebuild accordingly
					switch (outputFormat)
					{
						case OutputFormat.Folder:
							for (int i = 0; i < romsInGame.Count; i++)
							{
								string infile = pathsToFiles[i];
								Rom outrom = romsInGame[i];
								string outfile = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(outrom.Machine.Name), outrom.Name);

								// Make sure the output folder is created
								Directory.CreateDirectory(Path.GetDirectoryName(outfile));

								// Now copy the file over
								try
								{
									File.Copy(infile, outfile);
								}
								catch { }
							}
							break;
						case OutputFormat.TapeArchive:
							ArchiveTools.WriteTAR(pathsToFiles, outDir, romsInGame, logger);
							break;
						case OutputFormat.Torrent7Zip:
							break;
						case OutputFormat.TorrentGzip:
							for (int i = 0; i < itemsInGame.Count; i++)
							{
								string infile = pathsToFiles[i];
								Rom outrom = romsInGame[i];
								outrom.Machine.Name = Style.RemovePathUnsafeCharacters(outrom.Machine.Name);
								ArchiveTools.WriteTorrentGZ(infile, outDir, romba, logger);
							}
							break;
						case OutputFormat.TorrentLrzip:
							break;
						case OutputFormat.TorrentRar:
							break;
						case OutputFormat.TorrentXZ:
							break;
						case OutputFormat.TorrentZip:
							ArchiveTools.WriteTorrentZip(pathsToFiles, outDir, romsInGame, logger);
							break;
					}

					// And now clear the temp folder to get rid of any transient files
					try
					{
						Directory.Delete(tempSubDir, true);
					}
					catch { }
				});

			logger.User("Rebuilding complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			#endregion

			return success;
		}

		/// <summary>
		/// Process the DAT and verify the output directory
		/// </summary>
		/// <param name="datFile">DAT to use to verify the directory</param>
		/// <param name="inputs">List of input directories to compare against</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyDirectory(List<string> inputs, string tempDir, string headerToCheckAgainst, Logger logger)
		{
			// First create or clean the temp directory
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			else
			{
				FileTools.CleanDirectory(tempDir);
			}

			bool success = true;

			/*
			We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
			*/

			// Then, loop through and check each of the inputs
			logger.User("Processing files:\n");
			foreach (string input in inputs)
			{
				PopulateFromDir(input, false /* noMD5 */, false /* noSHA1 */, true /* bare */, false /* archivesAsFiles */,
					true /* enableGzip */, false /* addBlanks */, false /* addDate */, tempDir /* tempDir */, false /* copyFiles */,
					headerToCheckAgainst, 4 /* maxDegreeOfParallelism */, logger);
			}

			// Setup the fixdat
			DatFile matched = new DatFile(this);
			matched.Reset();
			matched.FileName = "fixDat_" + matched.FileName;
			matched.Name = "fixDat_" + matched.Name;
			matched.Description = "fixDat_" + matched.Description;
			matched.DatFormat = DatFormat.Logiqx;

			// Now that all files are parsed, get only files found in directory
			bool found = false;
			foreach (string key in Keys)
			{
				List<DatItem> roms = this[key];
				List<DatItem> newroms = DatItem.Merge(roms, logger);
				foreach (Rom rom in newroms)
				{
					if (rom.SourceID == 99)
					{
						found = true;
						matched.Add(rom.Size + "-" + rom.CRC, rom);
					}
				}
			}

			// Now output the fixdat to the main folder
			if (found)
			{
				matched.WriteToFile("", logger, stats: true);
			}
			else
			{
				logger.User("No fixDat needed");
			}

			return success;
		}

		#endregion
	}
}
