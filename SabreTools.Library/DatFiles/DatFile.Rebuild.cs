using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.Items;
using SabreTools.Library.Skippers;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using MemoryStream = System.IO.MemoryStream;
using SearchOption = System.IO.SearchOption;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
#endif

namespace SabreTools.Library.DatFiles
{
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
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildDepot(List<string> inputs, string outDir, string tempDir, bool date, bool delete,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, string headerToCheckAgainst)
		{
			#region Perform setup

			// If the DAT is not populated and inverse is not set, inform the user and quit
			if (Count == 0 && !inverse)
			{
				Globals.Logger.User("No entries were found to rebuild, exiting...");
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

			string format = "";
			switch (outputFormat)
			{
				case OutputFormat.Folder:
					format = "directory";
					break;
				case OutputFormat.TapeArchive:
					format = "TAR";
					break;
				case OutputFormat.Torrent7Zip:
					format = "Torrent7Z";
					break;
				case OutputFormat.TorrentGzip:
					format = "TorrentGZ";
					break;
				case OutputFormat.TorrentLrzip:
					format = "TorrentLRZ";
					break;
				case OutputFormat.TorrentRar:
					format = "TorrentRAR";
					break;
				case OutputFormat.TorrentXZ:
					format = "TorrentXZ";
					break;
				case OutputFormat.TorrentZip:
					format = "TorrentZip";
					break;
			}

			InternalStopwatch watch = new InternalStopwatch("Rebuilding all files to {0}", format);

			// Now loop through and get only directories from the input paths
			List<string> directories = new List<string>();
			Parallel.ForEach(inputs, Globals.ParallelOptions, input =>
			{
				// Add to the list if the input is a directory
				if (Directory.Exists(input))
				{
					Globals.Logger.Verbose("Adding depot: {0}", input);
					lock (directories)
					{
						directories.Add(input);
					}
				}
			});

			// If we don't have any directories, we want to exit
			if (directories.Count == 0)
			{
				return success;
			}

			// Now that we have a list of depots, we want to sort the input DAT by SHA-1
			BucketBy(SortedBy.SHA1, DedupeType.None);

			// Then we want to loop through each of the hashes and see if we can rebuild
			List<string> hashes = Keys.ToList();
			foreach (string hash in hashes)
			{
				// Pre-empt any issues that could arise from string length
				if (hash.Length != Constants.SHA1Length)
				{
					continue;
				}

				Globals.Logger.User("Checking hash '{0}'", hash);

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
				Rom fileinfo = ArchiveTools.GetTorrentGZFileInfo(foundpath);

				// If the file information is null, then we continue
				if (fileinfo == null)
				{
					continue;
				}

				// Otherwise, we rebuild that file to all locations that we need to
				RebuildIndividualFile(fileinfo, foundpath, outDir, tempDir, date, inverse, outputFormat, romba,
					updateDat, false /* isZip */, headerToCheckAgainst);
			}

			watch.Stop();

			#endregion

			// If we're updating the DAT, output to the rebuild directory
			if (updateDat)
			{
				FileName = "fixDAT_" + FileName;
				Name = "fixDAT_" + Name;
				Description = "fixDAT_" + Description;
				WriteToFile(outDir);
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
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildGeneric(List<string> inputs, string outDir, string tempDir, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst)
		{
			#region Perform setup

			// If the DAT is not populated and inverse is not set, inform the user and quit
			if (Count == 0 && !inverse)
			{
				Globals.Logger.User("No entries were found to rebuild, exiting...");
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

			string format = "";
			switch (outputFormat)
			{
				case OutputFormat.Folder:
					format = "directory";
					break;
				case OutputFormat.TapeArchive:
					format = "TAR";
					break;
				case OutputFormat.Torrent7Zip:
					format = "Torrent7Z";
					break;
				case OutputFormat.TorrentGzip:
					format = "TorrentGZ";
					break;
				case OutputFormat.TorrentLrzip:
					format = "TorrentLRZ";
					break;
				case OutputFormat.TorrentRar:
					format = "TorrentRAR";
					break;
				case OutputFormat.TorrentXZ:
					format = "TorrentXZ";
					break;
				case OutputFormat.TorrentZip:
					format = "TorrentZip";
					break;
			}

			InternalStopwatch watch = new InternalStopwatch("Rebuilding all files to {0}", format);

			// Now loop through all of the files in all of the inputs
			foreach (string input in inputs)
			{
				// If the input is a file
				if (File.Exists(input))
				{
					Globals.Logger.User("Checking file: {0}", input);
					RebuildGenericHelper(input, outDir, tempDir, quickScan, date, delete, inverse,
						outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst);
				}

				// If the input is a directory
				else if (Directory.Exists(input))
				{
					Globals.Logger.Verbose("Checking directory: {0}", input);
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						Globals.Logger.User("Checking file: {0}", file);
						RebuildGenericHelper(file, outDir, tempDir, quickScan, date, delete, inverse,
							outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst);
					}
				}
			}

			watch.Stop();

			#endregion

			// If we're updating the DAT, output to the rebuild directory
			if (updateDat)
			{
				FileName = "fixDAT_" + FileName;
				Name = "fixDAT_" + Name;
				Description = "fixDAT_" + Description;
				WriteToFile(outDir);
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
		private void RebuildGenericHelper(string file, string outDir, string tempDir, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst)
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
            ArchiveTools.GetInternalExternalProcess(file, archiveScanLevel, out bool shouldExternalProcess, out bool shouldInternalProcess);

			// If we're supposed to scan the file externally
			if (shouldExternalProcess)
			{
				// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
				Rom rom = FileTools.GetFileInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), header: headerToCheckAgainst);
				usedExternally = RebuildIndividualFile(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
					romba, updateDat, null /* isZip */, headerToCheckAgainst);
			}

			// If we're supposed to scan the file internally
			if (shouldInternalProcess)
			{
				// Create an empty list of Roms for archive entries
				List<Rom> entries = new List<Rom>();
				usedInternally = true;

				// Get the TGZ status for later
				bool isTorrentGzip = (ArchiveTools.GetTorrentGZFileInfo(file) != null);

				// If we're in quickscan, use the header information
				if (quickScan)
				{
					entries = ArchiveTools.GetArchiveFileInfo(file, date: date);
				}
				// Otherwise get the deeper information
				else
				{
					// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
					entries = ArchiveTools.GetExtendedArchiveFileInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), date: date);
				}

				// If the entries list is null, we encountered an error and should scan exteranlly
				if (entries == null && File.Exists(file))
				{
					// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
					Rom rom = FileTools.GetFileInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes));
					usedExternally = RebuildIndividualFile(rom, file, outDir, tempSubDir, date, inverse, outputFormat,
						romba, updateDat, null /* isZip */, headerToCheckAgainst);
				}
				// Otherwise, loop through the entries and try to match
				else
				{
					foreach (Rom entry in entries)
					{
						usedInternally &= RebuildIndividualFile(entry, file, outDir, tempSubDir, date, inverse, outputFormat,
							romba, updateDat, !isTorrentGzip /* isZip */, headerToCheckAgainst);
					}
				}
			}

			// If we are supposed to delete the file, do so
			if (delete && (usedExternally || usedInternally))
			{
				FileTools.TryDeleteFile(file);
			}

			// Now delete the temp directory
			FileTools.TryDeleteDirectory(tempSubDir);
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
		/// <param name="isZip">True if the input file is an archive, false if the file is TGZ, null otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <returns>True if the file was able to be rebuilt, false otherwise</returns>
		private bool RebuildIndividualFile(Rom rom, string file, string outDir, string tempDir, bool date,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, bool? isZip, string headerToCheckAgainst)
		{
			// Set the output value
			bool rebuilt = false;

			// Find if the file has duplicates in the DAT
			bool hasDuplicates = rom.HasDuplicates(this);

			// If it has duplicates and we're not filtering, rebuild it
			if (hasDuplicates && !inverse)
			{
				// Get the list of duplicates to rebuild to
				List<DatItem> dupes = rom.GetDuplicates(this, remove: updateDat);

				// If we don't have any duplicates, continue
				if (dupes.Count == 0)
				{
					return rebuilt;
				}

				// If we have a very specifc TGZ->TGZ case, just copy it accordingly
				if (isZip == false && ArchiveTools.GetTorrentGZFileInfo(file) != null && outputFormat == OutputFormat.TorrentGzip)
				{
					// Get the proper output path
					if (romba)
					{
						outDir = Path.Combine(outDir, Style.GetRombaPath(rom.SHA1));
					}
					else
					{
						outDir = Path.Combine(outDir, rom.SHA1 + ".gz");
					}

					// Make sure the output folder is created
					Directory.CreateDirectory(Path.GetDirectoryName(outDir));

					// Now copy the file over
					try
					{
						File.Copy(file, outDir);
						rebuilt &= true;
					}
					catch
					{
						rebuilt = false;
					}

					return rebuilt;
				}

				// Get a generic stream for the file
				Stream fileStream = new MemoryStream();

				// If we have a zipfile, extract the stream to memory
				if (isZip != null)
				{
					string realName = null;
					(fileStream, realName) = ArchiveTools.ExtractStream(file, rom.Name);
				}
				// Otherwise, just open the filestream
				else
				{
					fileStream = FileTools.TryOpenRead(file);
				}

				// If the stream is null, then continue
				if (fileStream == null)
				{
					return rebuilt;
				}

				// Seek to the beginning of the stream
				fileStream.Seek(0, SeekOrigin.Begin);

				Globals.Logger.User("Matches found for '{0}', rebuilding accordingly...", Style.GetFileName(rom.Name));
				rebuilt = true;

				// Now loop through the list and rebuild accordingly
				foreach (Rom item in dupes)
				{
					switch (outputFormat)
					{
						case OutputFormat.Folder:
							rebuilt &= ArchiveTools.WriteFile(fileStream, outDir, item, date: date, overwrite: true);
							break;
						case OutputFormat.TapeArchive:
							rebuilt &= ArchiveTools.WriteTAR(fileStream, outDir, item, date: date);
							break;
						case OutputFormat.Torrent7Zip:
							rebuilt &= ArchiveTools.WriteTorrent7Zip(fileStream, outDir, item, date: date);
							break;
						case OutputFormat.TorrentGzip:
							rebuilt &= ArchiveTools.WriteTorrentGZ(fileStream, outDir, romba);
							break;
						case OutputFormat.TorrentLrzip:
							break;
						case OutputFormat.TorrentRar:
							break;
						case OutputFormat.TorrentXZ:
							rebuilt &= ArchiveTools.WriteTorrentXZ(fileStream, outDir, item, date: date);
							break;
						case OutputFormat.TorrentZip:
							rebuilt &= ArchiveTools.WriteTorrentZip(fileStream, outDir, item, date: date);
							break;
					}
				}

				// Close the input stream
				fileStream?.Dispose();
			}

			// If we have no duplicates and we're filtering, rebuild it
			else if (!hasDuplicates && inverse)
			{
				string machinename = null;

				// If we have a very specifc TGZ->TGZ case, just copy it accordingly
				if (isZip == false && ArchiveTools.GetTorrentGZFileInfo(file) != null && outputFormat == OutputFormat.TorrentGzip)
				{
					// Get the proper output path
					if (romba)
					{
						outDir = Path.Combine(outDir, Style.GetRombaPath(rom.SHA1));
					}
					else
					{
						outDir = Path.Combine(outDir, rom.SHA1 + ".gz");
					}

					// Make sure the output folder is created
					Directory.CreateDirectory(Path.GetDirectoryName(outDir));

					// Now copy the file over
					try
					{
						File.Copy(file, outDir);
						rebuilt &= true;
					}
					catch
					{
						rebuilt = false;
					}

					return rebuilt;
				}

				// Get a generic stream for the file
				Stream fileStream = new MemoryStream();

				// If we have a zipfile, extract the stream to memory
				if (isZip != null)
				{
					string realName = null;
					(fileStream, realName) = ArchiveTools.ExtractStream(file, rom.Name);
				}
				// Otherwise, just open the filestream
				else
				{
					fileStream = FileTools.TryOpenRead(file);
				}

				// If the stream is null, then continue
				if (fileStream == null)
				{
					return rebuilt;
				}

				// Get the item from the current file
				Rom item = FileTools.GetStreamInfo(fileStream, fileStream.Length, keepReadOpen: true);
				item.MachineName = Style.GetFileNameWithoutExtension(item.Name);
				item.MachineDescription = Style.GetFileNameWithoutExtension(item.Name);

				// If we are coming from an archive, set the correct machine name
				if (machinename != null)
				{
					item.MachineName = machinename;
					item.MachineDescription = machinename;
				}

				Globals.Logger.User("No matches found for '{0}', rebuilding accordingly from inverse flag...", Style.GetFileName(rom.Name));

				// Now rebuild to the output file
				switch (outputFormat)
				{
					case OutputFormat.Folder:
						string outfile = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(item.MachineName), item.Name);

						// Make sure the output folder is created
						Directory.CreateDirectory(Path.GetDirectoryName(outfile));

						// Now copy the file over
						try
						{
							FileStream writeStream = FileTools.TryCreate(outfile);

							// Copy the input stream to the output
							int bufferSize = 4096 * 128;
							byte[] ibuffer = new byte[bufferSize];
							int ilen;
							while ((ilen = fileStream.Read(ibuffer, 0, bufferSize)) > 0)
							{
								writeStream.Write(ibuffer, 0, ilen);
								writeStream.Flush();
							}
							writeStream.Dispose();

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
						rebuilt &= ArchiveTools.WriteTAR(fileStream, outDir, item, date: date);
						break;
					case OutputFormat.Torrent7Zip:
						rebuilt &= ArchiveTools.WriteTorrent7Zip(fileStream, outDir, item, date: date);
						break;
					case OutputFormat.TorrentGzip:
						rebuilt &= ArchiveTools.WriteTorrentGZ(fileStream, outDir, romba);
						break;
					case OutputFormat.TorrentLrzip:
						break;
					case OutputFormat.TorrentRar:
						break;
					case OutputFormat.TorrentXZ:
						rebuilt &= ArchiveTools.WriteTorrentXZ(fileStream, outDir, item, date: date);
						break;
					case OutputFormat.TorrentZip:
						rebuilt &= ArchiveTools.WriteTorrentZip(fileStream, outDir, item, date: date);
						break;
				}

				// Close the input stream
				fileStream?.Dispose();
			}

			// Now we want to take care of headers, if applicable
			if (headerToCheckAgainst != null)
			{
				// Get a generic stream for the file
				Stream fileStream = new MemoryStream();

				// If we have a zipfile, extract the stream to memory
				if (isZip != null)
				{
					string realName = null;
					(fileStream, realName) = ArchiveTools.ExtractStream(file, rom.Name);
				}
				// Otherwise, just open the filestream
				else
				{
					fileStream = FileTools.TryOpenRead(file);
				}

				// If the stream is null, then continue
				if (fileStream == null)
				{
					return rebuilt;
				}

				// Check to see if we have a matching header first
				SkipperRule rule = Skipper.GetMatchingRule(fileStream, Path.GetFileNameWithoutExtension(headerToCheckAgainst));

				// If there's a match, create the new file to write
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// If the file could be transformed correctly
					MemoryStream transformStream = new MemoryStream();
					if (rule.TransformStream(fileStream, transformStream, keepReadOpen: true, keepWriteOpen: true))
					{
						// Get the file informations that we will be using
						Rom headerless = FileTools.GetStreamInfo(transformStream, transformStream.Length, keepReadOpen: true);

						// Find if the file has duplicates in the DAT
						hasDuplicates = headerless.HasDuplicates(this);

						// If it has duplicates and we're not filtering, rebuild it
						if (hasDuplicates && !inverse)
						{
							// Get the list of duplicates to rebuild to
							List<DatItem> dupes = headerless.GetDuplicates(this, remove: updateDat);

							// If we don't have any duplicates, continue
							if (dupes.Count == 0)
							{
								return rebuilt;
							}

							Globals.Logger.User("Headerless matches found for '{0}', rebuilding accordingly...", Style.GetFileName(rom.Name));
							rebuilt = true;

							// Now loop through the list and rebuild accordingly
							foreach (Rom item in dupes)
							{
								// Create a headered item to use as well
								rom.CopyMachineInformation(item);
								rom.Name += "_" + rom.CRC;

								// If either copy succeeds, then we want to set rebuilt to true
								bool eitherSuccess = false;
								switch (outputFormat)
								{
									case OutputFormat.Folder:
										eitherSuccess |= ArchiveTools.WriteFile(transformStream, outDir, item, date: date, overwrite: true);
										eitherSuccess |= ArchiveTools.WriteFile(fileStream, outDir, rom, date: date, overwrite: true);
										break;
									case OutputFormat.TapeArchive:
										eitherSuccess |=  ArchiveTools.WriteTAR(transformStream, outDir, item, date: date);
										eitherSuccess |=  ArchiveTools.WriteTAR(fileStream, outDir, rom, date: date);
										break;
									case OutputFormat.Torrent7Zip:
										eitherSuccess |=  ArchiveTools.WriteTorrent7Zip(transformStream, outDir, item, date: date);
										eitherSuccess |=  ArchiveTools.WriteTorrent7Zip(fileStream, outDir, rom, date: date);
										break;
									case OutputFormat.TorrentGzip:
										eitherSuccess |=  ArchiveTools.WriteTorrentGZ(transformStream, outDir, romba);
										eitherSuccess |=  ArchiveTools.WriteTorrentGZ(fileStream, outDir, romba);
										break;
									case OutputFormat.TorrentLrzip:
										break;
									case OutputFormat.TorrentRar:
										break;
									case OutputFormat.TorrentXZ:
										eitherSuccess |=  ArchiveTools.WriteTorrentXZ(transformStream, outDir, item, date: date);
										eitherSuccess |=  ArchiveTools.WriteTorrentXZ(fileStream, outDir, rom, date: date);
										break;
									case OutputFormat.TorrentZip:
										eitherSuccess |=  ArchiveTools.WriteTorrentZip(transformStream, outDir, item, date: date);
										eitherSuccess |=  ArchiveTools.WriteTorrentZip(fileStream, outDir, rom, date: date);
										break;
								}

								// Now add the success of either rebuild
								rebuilt &= eitherSuccess;
							}
						}
					}

					// Dispose of the stream
					transformStream?.Dispose();
				}

				// Dispose of the stream
				fileStream?.Dispose();
			}

			// And now clear the temp folder to get rid of any transient files if we unzipped
			if (isZip == true)
			{
				FileTools.TryDeleteDirectory(tempDir);
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
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyDepot(List<string> inputs, string tempDir, string headerToCheckAgainst)
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

			InternalStopwatch watch = new InternalStopwatch("Verifying all from supplied depots");

			// Now loop through and get only directories from the input paths
			List<string> directories = new List<string>();
			foreach (string input in inputs)
			{
				// Add to the list if the input is a directory
				if (Directory.Exists(input))
				{
					Globals.Logger.Verbose("Adding depot: {0}", input);
					directories.Add(input);
				}
			}

			// If we don't have any directories, we want to exit
			if (directories.Count == 0)
			{
				return success;
			}

			// Now that we have a list of depots, we want to sort the input DAT by SHA-1
			BucketBy(SortedBy.SHA1, DedupeType.None);

			// Then we want to loop through each of the hashes and see if we can rebuild
			List<string> hashes = Keys.ToList();
			foreach (string hash in hashes)
			{
				// Pre-empt any issues that could arise from string length
				if (hash.Length != Constants.SHA1Length)
				{
					continue;
				}

				Globals.Logger.User("Checking hash '{0}'", hash);

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
				Rom fileinfo = ArchiveTools.GetTorrentGZFileInfo(foundpath);

				// If the file information is null, then we continue
				if (fileinfo == null)
				{
					continue;
				}

				// Now we want to remove all duplicates from the DAT
				fileinfo.GetDuplicates(this, remove: true);
			}

			watch.Stop();

			// If there are any entries in the DAT, output to the rebuild directory
			FileName = "fixDAT_" + FileName;
			Name = "fixDAT_" + Name;
			Description = "fixDAT_" + Description;
			WriteToFile(null);

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
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyGeneric(List<string> inputs, string tempDir, bool hashOnly, bool quickScan, string headerToCheckAgainst)
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
			Globals.Logger.User("Processing files:\n");
			foreach (string input in inputs)
			{
				// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
				PopulateFromDir(input, (quickScan ? Hash.SecureHashes : Hash.DeepHashes) /* omitFromScan */, true /* bare */, false /* archivesAsFiles */,
					true /* enableGzip */, SkipFileType.None, false /* addBlanks */, false /* addDate */, tempDir /* tempDir */, false /* copyFiles */,
					headerToCheckAgainst);
			}

			// Setup the fixdat
			DatFile matched = new DatFile(this);
			matched.ResetDictionary();
			matched.FileName = "fixDat_" + matched.FileName;
			matched.Name = "fixDat_" + matched.Name;
			matched.Description = "fixDat_" + matched.Description;
			matched.DatFormat = DatFormat.Logiqx;

			// If we are checking hashes only, essentially diff the inputs
			if (hashOnly)
			{
				// First we need to sort and dedupe by hash to get duplicates
				BucketBy(SortedBy.CRC, DedupeType.Full);

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
					List<DatItem> newroms = DatItem.Merge(roms);
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
			success &= matched.WriteToFile("", stats: true);

			return success;
		}

		#endregion
	}
}
