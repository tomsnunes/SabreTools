using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Helper.Data;
using SabreTools.Helper.Skippers;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
#endif

namespace SabreTools.Helper.Dats
{
	/*
	 * TODO: Delete flags - Remove files from archive if only some are used (rebuild to TZip)
	 */
	public partial class DatFile
	{
		#region Rebuilding and Verifying [MODULAR DONE, FOR NOW]

		/// <summary>
		/// Process the DAT and find all matches in input files and folders assuming they're a depot
		/// </summary>
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
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildDepot(List<string> inputs, string outDir, string tempDir, bool date, bool delete,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, string headerToCheckAgainst,
			int maxDegreeOfParallelism, Logger logger)
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

			// Now we want to get forcepack flag if it's not overridden
			if (outputFormat == OutputFormat.Folder && ForcePacking != ForcePacking.None)
			{
				switch (ForcePacking)
				{
					case ForcePacking.Zip:
						outputFormat = OutputFormat.TorrentZip;
						break;
					case ForcePacking.Unzip:
						outputFormat = OutputFormat.Folder;
						break;
				}
			}

			// Preload the Skipper list
			int listcount = Skipper.List.Count;

			#endregion

			bool success = true;

			#region Rebuild from depots in order

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

			// Now loop through and get only directories from the input paths
			List<string> directories = new List<string>();
			foreach (string input in inputs)
			{
				// Add to the list if the input is a directory
				if (Directory.Exists(input))
				{
					logger.Verbose("Adding depot: '" + input + "'");
					directories.Add(input);
				}
			}

			// If we don't have any directories, we want to exit
			if (directories.Count == 0)
			{
				return success;
			}

			// Now that we have a list of depots, we want to sort the input DAT by SHA-1
			BucketBy(SortedBy.SHA1, false /* mergeroms */, logger, output: false);

			// Then we want to loop through each of the hashes and see if we can rebuild
			List<string> hashes = Keys.ToList();
			foreach (string hash in hashes)
			{
				// Pre-empt any issues that could arise from string length
				if (hash.Length != Constants.SHA1Length)
				{
					continue;
				}

				logger.User("Checking hash '" + hash + "'");

				// Get the extension path for the hash
				string subpath = Style.GetRombaPath(hash);

				// Find the first depot that includes the hash
				string foundpath = null;
				foreach (string directory in directories)
				{
					if (File.Exists(Path.Combine(directory, subpath)))
					{
						foundpath = Path.Combine(directory, subpath);
						break;
					}
				}

				// If we didn't find a path, then we continue
				if (foundpath == null)
				{
					continue;
				}

				// If we have a path, we want to try to get the rom information
				Rom fileinfo = ArchiveTools.GetTorrentGZFileInfo(foundpath, logger);

				// If the file information is null, then we continue
				if (fileinfo == null)
				{
					continue;
				}

				// Otherwise, we rebuild that file to all locations that we need to
				RebuildIndividualFile(fileinfo, foundpath, outDir, tempDir, date, inverse, outputFormat, romba, updateDat, true /*isZip*/, headerToCheckAgainst, logger);
			}

			logger.User("Rebuilding complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			#endregion

			// If we're updating the DAT, output to the rebuild directory
			if (updateDat)
			{
				_fileName = "fixDAT_" + _fileName;
				_name = "fixDAT_" + _name;
				_description = "fixDAT_" + _description;
				WriteToFile(outDir, logger);
			}

			return success;
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
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
		public bool RebuildGeneric(List<string> inputs, string outDir, string tempDir, bool quickScan, bool date,
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

			// Now we want to get forcepack flag if it's not overridden
			if (outputFormat == OutputFormat.Folder && ForcePacking != ForcePacking.None)
			{
				switch (ForcePacking)
				{
					case ForcePacking.Zip:
						outputFormat = OutputFormat.TorrentZip;
						break;
					case ForcePacking.Unzip:
						outputFormat = OutputFormat.Folder;
						break;
				}
			}

			// Preload the Skipper list
			int listcount = Skipper.List.Count;

			#endregion

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
					RebuildGenericHelper(input, outDir, tempDir, quickScan, date, delete, inverse,
						outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, maxDegreeOfParallelism, logger);
				}

				// If the input is a directory
				else if (Directory.Exists(input))
				{
					logger.Verbose("Checking directory: '" + input + "'");
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						logger.User("Checking file: '" + file + "'");
						RebuildGenericHelper(file, outDir, tempDir, quickScan, date, delete, inverse,
							outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, maxDegreeOfParallelism, logger);
					}
				}
			}

			logger.User("Rebuilding complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			#endregion

			// If we're updating the DAT, output to the rebuild directory
			if (updateDat)
			{
				_fileName = "fixDAT_" + _fileName;
				_name = "fixDAT_" + _name;
				_description = "fixDAT_" + _description;
				WriteToFile(outDir, logger);
			}

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
		private void RebuildGenericHelper(string file, string outDir, string tempDir, bool quickScan, bool date,
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
            ArchiveTools.GetInternalExternalProcess(file, archiveScanLevel, logger, out bool shouldExternalProcess, out bool shouldInternalProcess);

			// If we're supposed to scan the file externally
			if (shouldExternalProcess)
			{
				// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
				Rom rom = FileTools.GetFileInfo(file, logger, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), header: headerToCheckAgainst);
				usedExternally = RebuildIndividualFile(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
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
						usedInternally &= RebuildIndividualFile(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
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
						usedInternally = true;

						logger.Verbose(Path.GetFileName(file) + " treated like an archive");
						List<string> extracted = Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories).ToList();
						foreach (string entry in extracted)
						{
							// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
							Rom rom = FileTools.GetFileInfo(entry, logger, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes));
							usedInternally &= RebuildIndividualFile(rom, entry, outDir, tempSubDir, date, inverse, outputFormat,
								romba, updateDat, false /* isZip */, headerToCheckAgainst, logger);
						}
					}
					// Otherwise, just get the info on the file itself
					else if (File.Exists(file))
					{
						// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
						Rom rom = FileTools.GetFileInfo(file, logger, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes));
						usedExternally = RebuildIndividualFile(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
							romba, updateDat, false /* isZip */, headerToCheckAgainst, logger);
					}
				}
			}

			// If we are supposed to delete the file, do so
			if (delete && (usedExternally || usedInternally))
			{
				try
				{
					logger.Verbose("Attempting to delete input file '" + file + "'");
					File.Delete(file);
					logger.Verbose("File '" + file + "' deleted");
				}
				catch (Exception ex)
				{
					logger.Error("An error occurred while trying to delete '" + file + "' " + ex.ToString());
				}
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
		/// <remarks>
		/// TODO: If going from a TGZ file to a TGZ file, don't extract, just copy
		/// </remarks>
		private bool RebuildIndividualFile(Rom rom, string file, string outDir, string tempDir, bool date,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, bool isZip, string headerToCheckAgainst, Logger logger)
		{
			// Set the output value
			bool rebuilt = false;

			// Find if the file has duplicates in the DAT
			bool hasDuplicates = rom.HasDuplicates(this, logger);

			// If it has duplicates and we're not filtering, rebuild it
			if (hasDuplicates && !inverse)
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

				logger.User("Matches found for '" + Style.GetFileName(file) + "', rebuilding accordingly...");
				rebuilt = true;

				// Now loop through the list and rebuild accordingly
				foreach (Rom item in dupes)
				{
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
								rebuilt = false;
							}

							break;
						case OutputFormat.TapeArchive:
							rebuilt &= ArchiveTools.WriteTAR(file, outDir, item, logger, date: date);
							break;
						case OutputFormat.Torrent7Zip:
							rebuilt &= ArchiveTools.WriteTorrent7Zip(file, outDir, item, logger, date: date);
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
				}
			}

			// If we have no duplicates and we're filtering, rebuild it
			else if (!hasDuplicates && inverse)
			{
				string machinename = null;

				// If we have an archive input, get the real name of the file to use
				if (isZip)
				{
					// Otherwise, extract the file to the temp folder
					machinename = Style.GetFileNameWithoutExtension(file);
					file = ArchiveTools.ExtractItem(file, rom.Name, tempDir, logger);
				}

				// If we couldn't extract the file, then continue,
				if (String.IsNullOrEmpty(file))
				{
					return rebuilt;
				}

				// Get the item from the current file
				Rom item = FileTools.GetFileInfo(file, logger);
				item.Machine = new Machine()
				{
					Name = Style.GetFileNameWithoutExtension(item.Name),
					Description = Style.GetFileNameWithoutExtension(item.Name),
				};

				// If we are coming from an archive, set the correct machine name
				if (machinename != null)
				{
					item.Machine.Name = machinename;
					item.Machine.Description = machinename;
				}

				logger.User("No matches found for '" + Style.GetFileName(file) + "', rebuilding accordingly from inverse flag...");

				// Now rebuild to the output file
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
						rebuilt &= ArchiveTools.WriteTorrent7Zip(file, outDir, item, logger, date: date);
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
			}

			// Now we want to take care of headers, if applicable
			if (headerToCheckAgainst != null)
			{
				// Check to see if we have a matching header first
				SkipperRule rule = Skipper.GetMatchingRule(file, Path.GetFileNameWithoutExtension(headerToCheckAgainst), logger);

				// If there's a match, create the new file to write
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// If the file could be transformed correctly
					if (rule.TransformFile(file, file + ".new", logger))
					{
						// Get the file informations that we will be using
						Rom headerless = FileTools.GetFileInfo(file + ".new", logger);

						// Find if the file has duplicates in the DAT
						hasDuplicates = headerless.HasDuplicates(this, logger);

						// If it has duplicates and we're not filtering, rebuild it
						if (hasDuplicates && !inverse)
						{
							// Get the list of duplicates to rebuild to
							List<DatItem> dupes = headerless.GetDuplicates(this, logger, remove: updateDat);

							// If we don't have any duplicates, continue
							if (dupes.Count == 0)
							{
								return rebuilt;
							}

							logger.User("Headerless matches found for '" + Style.GetFileName(file) + "', rebuilding accordingly...");
							rebuilt = true;

							// Now loop through the list and rebuild accordingly
							foreach (Rom item in dupes)
							{
								// Create a headered item to use as well
								rom.Machine = item.Machine;
								rom.Name += "_" + rom.CRC;

								switch (outputFormat)
								{
									case OutputFormat.Folder:
										string outfile = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(item.Machine.Name), item.Name);
										string headeredOutfile = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(rom.Machine.Name), rom.Name);

										// Make sure the output folder is created
										Directory.CreateDirectory(Path.GetDirectoryName(outfile));

										// If either copy succeeds, then we want to set rebuilt to true
										bool eitherSuccess = false;

										// Now copy the files over
										try
										{
											File.Copy(file + ".new", outfile);
											if (date && !String.IsNullOrEmpty(item.Date))
											{
												File.SetCreationTime(outfile, DateTime.Parse(item.Date));
											}

											eitherSuccess |= true;
										}
										catch { }
										try
										{
											File.Copy(file, headeredOutfile);
											if (date && !String.IsNullOrEmpty(rom.Date))
											{
												File.SetCreationTime(outfile, DateTime.Parse(rom.Date));
											}

											eitherSuccess |= true;
										}
										catch { }

										// Now add the success of either rebuild
										rebuilt &= eitherSuccess;

										break;
									case OutputFormat.TapeArchive:
										rebuilt &= ArchiveTools.WriteTAR(file + ".new", outDir, item, logger, date: date);
										rebuilt &= ArchiveTools.WriteTAR(file, outDir, rom, logger, date: date);
										break;
									case OutputFormat.Torrent7Zip:
										rebuilt &= ArchiveTools.WriteTorrent7Zip(file + ".new", outDir, item, logger, date: date);
										rebuilt &= ArchiveTools.WriteTorrent7Zip(file, outDir, rom, logger, date: date);
										break;
									case OutputFormat.TorrentGzip:
										rebuilt &= ArchiveTools.WriteTorrentGZ(file + ".new", outDir, romba, logger);
										rebuilt &= ArchiveTools.WriteTorrentGZ(file, outDir, romba, logger);
										break;
									case OutputFormat.TorrentLrzip:
										break;
									case OutputFormat.TorrentRar:
										break;
									case OutputFormat.TorrentXZ:
										break;
									case OutputFormat.TorrentZip:
										rebuilt &= ArchiveTools.WriteTorrentZip(file + ".new", outDir, item, logger, date: date);
										rebuilt &= ArchiveTools.WriteTorrentZip(file, outDir, rom, logger, date: date);
										break;
								}
							}
						}
					}
				}
			}

			// And now clear the temp folder to get rid of any transient files if we unzipped
			if (isZip)
			{
				try
				{
					Directory.Delete(tempDir, true);
				}
				catch { }
			}

			return rebuilt;
		}

		/// <summary>
		/// Process the DAT and verify from the depots
		/// </summary>
		/// <param name="inputs">List of input directories to compare against</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyDepot(List<string> inputs, string tempDir, string headerToCheckAgainst, Logger logger)
		{
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

			bool success = true;

			logger.User("Verifying all from supplied depots");
			DateTime start = DateTime.Now;

			// Now loop through and get only directories from the input paths
			List<string> directories = new List<string>();
			foreach (string input in inputs)
			{
				// Add to the list if the input is a directory
				if (Directory.Exists(input))
				{
					logger.Verbose("Adding depot: '" + input + "'");
					directories.Add(input);
				}
			}

			// If we don't have any directories, we want to exit
			if (directories.Count == 0)
			{
				return success;
			}

			// Now that we have a list of depots, we want to sort the input DAT by SHA-1
			BucketBy(SortedBy.SHA1, false /* mergeroms */, logger, output: false);

			// Then we want to loop through each of the hashes and see if we can rebuild
			List<string> hashes = Keys.ToList();
			foreach (string hash in hashes)
			{
				// Pre-empt any issues that could arise from string length
				if (hash.Length != Constants.SHA1Length)
				{
					continue;
				}

				logger.User("Checking hash '" + hash + "'");

				// Get the extension path for the hash
				string subpath = Style.GetRombaPath(hash);

				// Find the first depot that includes the hash
				string foundpath = null;
				foreach (string directory in directories)
				{
					if (File.Exists(Path.Combine(directory, subpath)))
					{
						foundpath = Path.Combine(directory, subpath);
						break;
					}
				}

				// If we didn't find a path, then we continue
				if (foundpath == null)
				{
					continue;
				}

				// If we have a path, we want to try to get the rom information
				Rom fileinfo = ArchiveTools.GetTorrentGZFileInfo(foundpath, logger);

				// If the file information is null, then we continue
				if (fileinfo == null)
				{
					continue;
				}

				// Now we want to remove all duplicates from the DAT
				fileinfo.GetDuplicates(this, logger, remove: true);
			}

			logger.User("Verifying complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// If there are any entries in the DAT, output to the rebuild directory
			_fileName = "fixDAT_" + _fileName;
			_name = "fixDAT_" + _name;
			_description = "fixDAT_" + _description;
			WriteToFile(null, logger);

			return success;
		}

		/// <summary>
		/// Process the DAT and verify the output directory
		/// </summary>
		/// <param name="inputs">List of input directories to compare against</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyGeneric(List<string> inputs, string tempDir, bool hashOnly, bool quickScan, string headerToCheckAgainst, Logger logger)
		{
			// Check the temp directory exists
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

			// TODO: We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
			bool success = true;

			// Then, loop through and check each of the inputs
			logger.User("Processing files:\n");
			foreach (string input in inputs)
			{
				// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
				PopulateFromDir(input, (quickScan ? Hash.SecureHashes : Hash.DeepHashes) /* omitFromScan */, true /* bare */, false /* archivesAsFiles */,
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

			// If we are checking hashes only, essentially diff the inputs
			if (hashOnly)
			{
				// First we need to sort by hash to get duplicates
				BucketBy(SortedBy.SHA1, false /* mergeroms */, logger, output: false);

				// Then follow the same tactics as before
				foreach (string key in Keys)
				{
					List<DatItem> roms = this[key];
					foreach (DatItem rom in roms)
					{
						if (rom.SourceID == 99)
						{
							if (rom.Type == ItemType.Disk || rom.Type == ItemType.Rom)
							{
								matched.Add(((Disk)rom).SHA1, rom);
							}
						}
					}
				}
			}
			// If we are checking full names, get only files found in directory
			else
			{
				foreach (string key in Keys)
				{
					List<DatItem> roms = this[key];
					List<DatItem> newroms = DatItem.Merge(roms, logger);
					foreach (Rom rom in newroms)
					{
						if (rom.SourceID == 99)
						{
							matched.Add(rom.Size + "-" + rom.CRC, rom);
						}
					}
				}
			}

			// Now output the fixdat to the main folder
			success &= matched.WriteToFile("", logger, stats: true);

			return success;
		}

		#endregion
	}
}
