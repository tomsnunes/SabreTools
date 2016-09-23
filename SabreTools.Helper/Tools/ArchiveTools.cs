using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Common;
using SharpCompress.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace SabreTools.Helper
{
	public class ArchiveTools
	{
		#region Archive-to-Archive Handling

		/// <summary>
		/// Attempt to copy a file between archives
		/// </summary>
		/// <param name="inputArchive">Source archive name</param>
		/// <param name="outDir">Destination archive name</param>
		/// <param name="sourceEntryName">Input entry name</param>
		/// <param name="destEntryName">Output entry name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the copy was a success, false otherwise</returns>
		public static bool CopyFileBetweenArchives(string inputArchive, string outDir,
			string sourceEntryName, Rom destEntry, Logger logger)
		{
			string tempfile = ExtractSingleItemFromArchive(inputArchive, sourceEntryName, Path.GetTempPath(), logger);
			return WriteToArchive(tempfile, outDir, destEntry);
		}

		#endregion

		#region Extraction

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempDir, Logger logger)
		{
			return ExtractArchive(input, tempDir, ArchiveScanLevel.Both, ArchiveScanLevel.External, ArchiveScanLevel.External, ArchiveScanLevel.Both, logger);
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempDir, int sevenzip, int gz, int rar, int zip, Logger logger)
		{
			return ExtractArchive(input, tempDir, (ArchiveScanLevel)sevenzip, (ArchiveScanLevel)gz, (ArchiveScanLevel)rar, (ArchiveScanLevel)zip, logger);
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Archive handling level for 7z</param>
		/// <param name="gz">Archive handling level for GZip</param>
		/// <param name="rar">Archive handling level for RAR</param>
		/// <param name="zip">Archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempDir, ArchiveScanLevel sevenzip,
			ArchiveScanLevel gz, ArchiveScanLevel rar, ArchiveScanLevel zip, Logger logger)
		{
			bool encounteredErrors = true;

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return encounteredErrors;
			}

			try
			{
				if (at == ArchiveType.SevenZip && sevenzip != ArchiveScanLevel.External)
				{
					logger.Log("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					// Extract all files to the temp directory
					SevenZipArchive sza = SevenZipArchive.Open(File.OpenRead(input));
					foreach (IArchiveEntry iae in sza.Entries)
					{
						iae.WriteToDirectory(tempDir, ExtractOptions.PreserveFileTime | ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
					}
					encounteredErrors = false;
					sza.Dispose();

				}
				else if (at == ArchiveType.GZip && gz != ArchiveScanLevel.External)
				{
					logger.Log("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					// Decompress the input stream
					FileStream outstream = File.Create(Path.Combine(tempDir, Path.GetFileNameWithoutExtension(input)));
					GZipStream gzstream = new GZipStream(File.OpenRead(input), CompressionMode.Decompress);
					gzstream.CopyTo(outstream);

					// Dispose of the streams
					outstream.Dispose();
					gzstream.Dispose();

					encounteredErrors = false;
				}
				else if ((at == ArchiveType.Zip && zip != ArchiveScanLevel.External)
					|| (at == ArchiveType.Rar && rar != ArchiveScanLevel.External))
				{
					logger.Log("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					// Extract all files to the temp directory
					IReader reader = ReaderFactory.Open(File.OpenRead(input));
					bool succeeded = reader.MoveToNextEntry();
					while (succeeded)
					{
						reader.WriteEntryToDirectory(tempDir, ExtractOptions.PreserveFileTime | ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
						succeeded = reader.MoveToNextEntry();
					}
					encounteredErrors = false;
					reader.Dispose();
				}
			}
			catch (EndOfStreamException)
			{
				// Catch this but don't count it as an error because SharpCompress is unsafe
			}
			catch (InvalidOperationException)
			{
				encounteredErrors = true;
			}
			catch (Exception)
			{
				// Don't log file open errors
				encounteredErrors = true;
			}

			return encounteredErrors;
		}

		/// <summary>
		/// Attempt to extract a file from an archive
		/// </summary>
		/// <param name="input">Name of the archive to be extracted</param>
		/// <param name="entryname">Name of the entry to be extracted</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Name of the extracted file, null on error</returns>
		public static string ExtractSingleItemFromArchive(string input, string entryname, string tempDir, Logger logger)
		{
			string outfile = null;

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return outfile;
			}

			IReader reader = null;
			try
			{
				if (at == ArchiveType.Zip || at == ArchiveType.SevenZip || at == ArchiveType.Rar)
				{
					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					reader = ReaderFactory.Open(File.OpenRead(input));
					while (reader.MoveToNextEntry())
					{
						logger.Log("Current entry name: '" + reader.Entry.Key + "'");
						if (reader.Entry != null && reader.Entry.Key.Contains(entryname))
						{
							outfile = Path.GetFullPath(Path.Combine(tempDir, reader.Entry.Key));
							if (!Directory.Exists(Path.GetDirectoryName(outfile)))
							{
								Directory.CreateDirectory(Path.GetDirectoryName(outfile));
							}
							reader.WriteEntryToFile(outfile, ExtractOptions.Overwrite);
						}
					}
				}
				else if (at == ArchiveType.GZip)
				{
					// Decompress the input stream
					FileStream outstream = File.Create(Path.Combine(tempDir, Path.GetFileNameWithoutExtension(input)));
					GZipStream gzstream = new GZipStream(File.OpenRead(input), CompressionMode.Decompress);
					gzstream.CopyTo(outstream);
					outfile = Path.GetFullPath(Path.Combine(tempDir, Path.GetFileNameWithoutExtension(input)));

					// Dispose of the streams
					outstream.Dispose();
					gzstream.Dispose();
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				outfile = null;
			}
			finally
			{
				reader?.Dispose();
			}

			return outfile;
		}

		#endregion

		#region Information

		/// <summary>
		/// Generate a list of RomData objects from the header values in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>List of RomData objects representing the found data</returns>
		public static List<Rom> GetArchiveFileInfo(string input, Logger logger)
		{
			List<Rom> roms = new List<Rom>();
			string gamename = Path.GetFileNameWithoutExtension(input);

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return roms;
			}

			// If we got back GZip, try to get TGZ info first
			else if (at == ArchiveType.GZip)
			{
				Rom possibleTgz = GetTorrentGZFileInfo(input, logger);

				// If it was, then add it to the outputs and continue
				if (possibleTgz.Name != null)
				{
					roms.Add(possibleTgz);
					return roms;
				}
			}

			IReader reader = null;
			try
			{
				logger.Log("Found archive of type: " + at);
				long size = 0;
				string crc = "";

				// If we have a gzip file, get the crc directly
				if (at == ArchiveType.GZip)
				{
					// Get the CRC and size from the file
					BinaryReader br = new BinaryReader(File.OpenRead(input));
					br.BaseStream.Seek(-8, SeekOrigin.End);
					byte[] headercrc = br.ReadBytes(4);
					crc = BitConverter.ToString(headercrc.Reverse().ToArray()).Replace("-", string.Empty).ToLowerInvariant();
					byte[] headersize = br.ReadBytes(4);
					size = BitConverter.ToInt32(headersize.Reverse().ToArray(), 0);
					br.Dispose();
				}

				if (at != ArchiveType.Tar)
				{
					reader = ReaderFactory.Open(File.OpenRead(input));
					while (reader.MoveToNextEntry())
					{
						if (reader.Entry != null && !reader.Entry.IsDirectory)
						{
							logger.Log("Entry found: '" + reader.Entry.Key + "': "
								+ (size == 0 ? reader.Entry.Size : size) + ", "
								+ (crc == "" ? reader.Entry.Crc.ToString("X").ToLowerInvariant() : crc));

							roms.Add(new Rom
							{
								Type = ItemType.Rom,
								Name = reader.Entry.Key,
								MachineName = gamename,
								Size = (size == 0 ? reader.Entry.Size : size),
								CRC = (crc == "" ? reader.Entry.Crc.ToString("X").ToLowerInvariant() : crc),
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
			}
			finally
			{
				reader?.Dispose();
			}

			return roms;
		}

		/// <summary>
		/// Retrieve file information for a single torrent GZ file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetTorrentGZFileInfo(string input, Logger logger)
		{
			string datum = Path.GetFileName(input).ToLowerInvariant();
			long filesize = new FileInfo(input).Length;

			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{40}\.gz"))
			{
				logger.Warning("Non SHA-1 filename found, skipping: '" + datum + "'");
				return new Rom();
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				logger.Warning("Possibly corrupt file '" + input + "' with size " + Style.GetBytesReadable(filesize));
				return new Rom();
			}

			// Get the Romba-specific header data
			byte[] header; // Get preamble header for checking
			byte[] headermd5; // MD5
			byte[] headercrc; // CRC
			byte[] headersz; // Int64 size
			BinaryReader br = new BinaryReader(File.OpenRead(input));
			header = br.ReadBytes(12);
			headermd5 = br.ReadBytes(16);
			headercrc = br.ReadBytes(4);
			headersz = br.ReadBytes(8);
			br.Dispose();

			// If the header is not correct, return a blank rom
			bool correct = true;
			for (int i = 0; i < header.Length; i++)
			{
				// This is a temp fix to ignore the modification time and OS until romba can be fixed
				if (i == 4 || i == 5 || i == 6 || i == 7 || i == 9)
				{
					continue;
				}
				correct &= (header[i] == Constants.TorrentGZHeader[i]);
			}
			if (!correct)
			{
				return new Rom();
			}

			// Now convert the data and get the right position
			string gzmd5 = BitConverter.ToString(headermd5).Replace("-", string.Empty);
			string gzcrc = BitConverter.ToString(headercrc).Replace("-", string.Empty);
			long extractedsize = BitConverter.ToInt64(headersz, 0);

			Rom rom = new Rom
			{
				Type = ItemType.Rom,
				MachineName = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				Name = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				Size = extractedsize,
				CRC = gzcrc.ToLowerInvariant(),
				MD5 = gzmd5.ToLowerInvariant(),
				SHA1 = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
			};

			return rom;
		}

		/// <summary>
		/// Returns the archive type of an input file
		/// </summary>
		/// <param name="input">Input file to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>ArchiveType of inputted file (null on error)</returns>
		public static ArchiveType? GetCurrentArchiveType(string input, Logger logger)
		{
			ArchiveType? outtype = null;

			// First line of defense is going to be the extension, for better or worse
			string ext = Path.GetExtension(input).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}

			if (ext != "7z" && ext != "gz" && ext != "lzma" && ext != "rar"
				&& ext != "rev" && ext != "r00" && ext != "r01" && ext != "tar"
				&& ext != "tgz" && ext != "tlz" && ext != "zip" && ext != "zipx")
			{
				return outtype;
			}

			// Read the first bytes of the file and get the magic number
			try
			{
				byte[] magic = new byte[8];
				BinaryReader br = new BinaryReader(File.OpenRead(input));
				magic = br.ReadBytes(8);
				br.Dispose();

				// Convert it to an uppercase string
				string mstr = string.Empty;
				for (int i = 0; i < magic.Length; i++)
				{
					mstr += BitConverter.ToString(new byte[] { magic[i] });
				}
				mstr = mstr.ToUpperInvariant();

				// Now try to match it to a known signature
				if (mstr.StartsWith(Constants.SevenZipSig))
				{
					outtype = ArchiveType.SevenZip;
				}
				else if (mstr.StartsWith(Constants.GzSig))
				{
					outtype = ArchiveType.GZip;
				}
				else if (mstr.StartsWith(Constants.RarSig) || mstr.StartsWith(Constants.RarFiveSig))
				{
					outtype = ArchiveType.Rar;
				}
				else if (mstr.StartsWith(Constants.TarSig) || mstr.StartsWith(Constants.TarZeroSig))
				{
					outtype = ArchiveType.Tar;
				}
				else if (mstr.StartsWith(Constants.ZipSig) || mstr.StartsWith(Constants.ZipSigEmpty) || mstr.StartsWith(Constants.ZipSigSpanned))
				{
					outtype = ArchiveType.Zip;
				}
			}
			catch (Exception)
			{
				// Don't log file open errors
			}

			return outtype;
		}

		/// <summary>
		/// Get if the current file should be scanned internally and externally
		/// </summary>
		/// <param name="input">Name of the input file to check</param>
		/// <param name="sevenzip">User-defined scan level for 7z archives</param>
		/// <param name="gzip">User-defined scan level for GZ archives</param>
		/// <param name="rar">User-defined scan level for RAR archives</param>
		/// <param name="zip">User-defined scan level for Zip archives</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="shouldExternalProcess">Output parameter determining if file should be processed externally</param>
		/// <param name="shouldInternalProcess">Output parameter determining if file should be processed internally</param>
		public static void GetInternalExternalProcess(string input, ArchiveScanLevel sevenzip, ArchiveScanLevel gzip,
			ArchiveScanLevel rar, ArchiveScanLevel zip, Logger logger, out bool shouldExternalProcess, out bool shouldInternalProcess)
		{
			shouldExternalProcess = true;
			shouldInternalProcess = true;

			ArchiveType? archiveType = GetCurrentArchiveType(input, logger);
			switch (archiveType)
			{
				case null:
					shouldExternalProcess = true;
					shouldInternalProcess = false;
					break;
				case ArchiveType.GZip:
					shouldExternalProcess = (gzip != ArchiveScanLevel.Internal);
					shouldInternalProcess = (gzip != ArchiveScanLevel.External);
					break;
				case ArchiveType.Rar:
					shouldExternalProcess = (rar != ArchiveScanLevel.Internal);
					shouldInternalProcess = (rar != ArchiveScanLevel.External);
					break;
				case ArchiveType.SevenZip:
					shouldExternalProcess = (sevenzip != ArchiveScanLevel.Internal);
					shouldInternalProcess = (sevenzip != ArchiveScanLevel.External);
					break;
				case ArchiveType.Zip:
					shouldExternalProcess = (zip != ArchiveScanLevel.Internal);
					shouldInternalProcess = (zip != ArchiveScanLevel.External);
					break;
			}
		}

		/// <summary>
		/// Read the information from an input 7z file
		/// </summary>
		/// <param name="input">Name of the input file to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <remarks>http://cpansearch.perl.org/src/BJOERN/Compress-Deflate7-1.0/7zip/DOC/7zFormat.txt</remarks>
		public static void GetSevenZipFileInfo(string input, Logger logger)
		{
			BinaryReader br = new BinaryReader(File.OpenRead(input));
			br.ReadBytes(6); // BYTE kSignature[6] = {'7', 'z', 0xBC, 0xAF, 0x27, 0x1C};
			logger.User("ArchiveVersion (Major.Minor): " + br.ReadByte() + "." + br.ReadByte());
			logger.User("StartHeaderCRC: " + br.ReadUInt32());
			logger.User("StartHeader (NextHeaderOffset, NextHeaderSize, NextHeaderCRC)" + br.ReadUInt64() + ", " + br.ReadUInt64() + ", " + br.ReadUInt32());
		}

		#endregion

		#region Writing

		/// <summary>
		/// Copy a file to an output archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteToArchive(string inputFile, string outDir, Rom rom)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteToArchive(inputFiles, outDir, roms);
		}

		/// <summary>
		/// Copy a set of files to an output archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFiles">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="roms">List of Rom representing the new information</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteToArchive(List<string> inputFiles, string outDir, List<Rom> roms)
		{
			bool success = false;

			// If the number of inputs is less than the number of available roms, return
			if (inputFiles.Count < roms.Count)
			{
				return success;
			}

			// If one of the files doesn't exist, return
			foreach (string file in inputFiles)
			{
				if (!File.Exists(file))
				{
					return success;
				}
			}

			// Get the output archive name from the first rebuild rom
			string archiveFileName = Path.Combine(outDir, roms[0].MachineName + ".zip");

			// First, open the archive
			ZipArchive outarchive = null;
			try
			{
				// If the full output path doesn't exist, create it
				if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
				}

				// If the archive doesn't exist, create it
				if (!File.Exists(archiveFileName))
				{
					outarchive = System.IO.Compression.ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
					outarchive.Dispose();
				}

				// Open the archive for writing
				outarchive = System.IO.Compression.ZipFile.Open(archiveFileName, ZipArchiveMode.Update);

				// Now loop through and add all files
				for (int i = 0; i < inputFiles.Count; i++)
				{
					string inputFile = inputFiles[i];
					Rom rom = roms[i];

					// If the archive doesn't already contain the entry, add it
					if (outarchive.GetEntry(rom.Name) == null)
					{
						outarchive.CreateEntryFromFile(inputFile, rom.Name, CompressionLevel.Optimal);
					}

					// If there's a Date attached to the rom, change the entry to that Date
					if (!string.IsNullOrEmpty(rom.Date))
					{
						DateTimeOffset dto = DateTimeOffset.Now;
						if (DateTimeOffset.TryParse(rom.Date, out dto))
						{
							outarchive.GetEntry(rom.Name).LastWriteTime = dto;
						}
					}
				}

				// Dispose of the streams
				outarchive.Dispose();

				success = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				success = false;
			}
			finally
			{
				outarchive?.Dispose();
			}

			return success;
		}

		/// <summary>
		/// Copy a file to an output torrentzip archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentZip(string inputFile, string outDir, Rom rom, Logger logger)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteTorrentZip(inputFiles, outDir, roms, logger);
		}

		/// <summary>
		/// Copy a set of files to an output torrentzip archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentZip(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger)
		{
			bool success = false;
			string tempFile = Path.GetTempFileName();

			// If the number of inputs is less than the number of available roms, return
			if (inputFiles.Count < roms.Count)
			{
				return success;
			}

			// If one of the files doesn't exist, return
			foreach (string file in inputFiles)
			{
				if (!File.Exists(file))
				{
					return success;
				}
			}

			// Get the output archive name from the first rebuild rom
			string archiveFileName = Path.Combine(outDir, roms[0].MachineName + ".zip");

			// Set internal variables
			Stream writeStream = null;
			ZipFile oldZipFile = new ZipFile();
			ZipFile zipFile = new ZipFile();
			ZipReturn zipReturn = ZipReturn.ZipGood;

			try
			{
				// If the full output path doesn't exist, create it
				if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
				}

				// If the archive doesn't exist, create it and put the single file
				if (!File.Exists(archiveFileName))
				{
					zipReturn = zipFile.Create(tempFile);

					// Map all inputs to index
					Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
					for (int i = 0; i < inputFiles.Count; i++)
					{
						inputIndexMap.Add(inputFiles[i], i);
					}

					// Sort the keys in TZIP order
					List<string> keys = inputIndexMap.Keys.ToList();
					keys.Sort(ZipFile.TorrentZipStringCompare);

					// Now add all of the files in order
					foreach (string key in keys)
					{
						string inputFile = key;
						Rom rom = roms[inputIndexMap[key]];

						// Open the input file for reading
						Stream fs = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

						ulong streamSize = (ulong)(new FileInfo(inputFile).Length);
						zipReturn = zipFile.OpenWriteStream(false, true, rom.Name, streamSize, CompressionMethod.Deflated, out writeStream);

						// Copy the input stream to the output
						byte[] buffer = new byte[8 * 1024];
						int len;
						while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
						{
							writeStream.Write(buffer, 0, len);
						}
						writeStream.Flush();
						zipFile.CloseWriteStream(Convert.ToUInt32(rom.CRC, 16));

						//Dispose of the file stream
						fs.Dispose();
					}
				}

				// Otherwise, sort the input files and write out in the correct order
				else
				{
					// Open the old archive for reading
					oldZipFile.Open(archiveFileName, new FileInfo(archiveFileName).LastWriteTime.Ticks, true);

					// Map all inputs to index
					Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
					for (int i = 0; i < inputFiles.Count; i++)
					{
						// If the old one contains the new file, then just skip out
						if (oldZipFile.Contains(roms[i].Name))
						{
							continue;
						}

						inputIndexMap.Add(inputFiles[i], -(i + 1));
					}

					// Then add all of the old entries to it too
					for (int i = 0; i < oldZipFile.EntriesCount; i++)
					{
						inputIndexMap.Add(oldZipFile.Filename(i), i);
					}

					// If the number of entries is the same as the old archive, skip out
					if (inputIndexMap.Keys.Count <= oldZipFile.EntriesCount)
					{
						success = true;
						return success;
					}

					// Otherwise, process the old zipfile
					zipFile.Create(tempFile);

					// Get the order for the entries with the new file
					List<string> keys = inputIndexMap.Keys.ToList();
					keys.Sort(ZipFile.TorrentZipStringCompare);

					// Copy over all files to the new archive
					foreach (string key in keys)
					{
						// Get the index mapped to they key
						int index = inputIndexMap[key];

						// If we have the input file, add it now
						if (index < 0)
						{
							// Open the input file for reading
							Stream freadStream = File.Open(key, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
							ulong istreamSize = (ulong)(new FileInfo(key).Length);
							zipFile.OpenWriteStream(false, true, roms[-index - 1].Name, istreamSize, CompressionMethod.Deflated, out writeStream);

							// Copy the input stream to the output
							byte[] ibuffer = new byte[8 * 1024];
							int ilen;
							while ((ilen = freadStream.Read(ibuffer, 0, ibuffer.Length)) > 0)
							{
								writeStream.Write(ibuffer, 0, ilen);
							}
							freadStream.Dispose();
							zipFile.CloseWriteStream(Convert.ToUInt32(roms[-index - 1].CRC, 16));
						}

						// Otherwise, copy the file from the old archive
						else
						{
							// Instantiate the streams
							CompressionMethod icompressionMethod = CompressionMethod.Stored;
							ulong istreamSize = 0;
							Stream zreadStream;
							oldZipFile.OpenReadStream(index, false, out zreadStream, out istreamSize, out icompressionMethod);
							zipFile.OpenWriteStream(false, true, oldZipFile.Filename(index), istreamSize, CompressionMethod.Deflated, out writeStream);

							// Copy the input stream to the output
							byte[] ibuffer = new byte[8 * 1024];
							int ilen;
							while ((ilen = zreadStream.Read(ibuffer, 0, ibuffer.Length)) > 0)
							{
								writeStream.Write(ibuffer, 0, ilen);
							}
							zipFile.CloseWriteStream(BitConverter.ToUInt32(oldZipFile.CRC32(index), 0));
						}
					}
				}

				// Close the output zip file
				zipFile.Close();

				success = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				success = false;
			}
			finally
			{
				zipFile.Dispose();
				oldZipFile.Dispose();
			}

			// If the old file exists, delete it and replace
			if (File.Exists(archiveFileName))
			{
				File.Delete(archiveFileName);
			}
			File.Move(tempFile, archiveFileName);

			return true;
		}

		/// <summary>
		/// Write an input file to a torrent GZ file
		/// </summary>
		/// <param name="input">File to write from</param>
		/// <param name="outDir">Directory to write archive to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the write was a success, false otherwise</returns>
		/// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
		public static bool WriteTorrentGZ(string input, string outDir, bool romba, Logger logger)
		{
			// Check that the input file exists
			if (!File.Exists(input))
			{
				logger.Warning("File " + input + " does not exist!");
				return false;
			}
			input = Path.GetFullPath(input);

			// Make sure the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}
			outDir = Path.GetFullPath(outDir);

			// Now get the Rom info for the file so we have hashes and size
			Rom rom = FileTools.GetSingleFileInfo(input);

			// If it doesn't exist, create the output file and then write
			string outfile = Path.Combine(outDir, rom.SHA1 + ".gz");

			// Compress the input stream
			FileStream inputStream = File.OpenRead(input);
			GZipStream outputStream = new GZipStream(File.Open(outfile, FileMode.Create, FileAccess.Write), CompressionMode.Compress);
			inputStream.CopyTo(outputStream);

			// Dispose of the streams
			inputStream.Dispose();
			outputStream.Dispose();

			// Now that it's ready, inject the header info
			BinaryWriter sw = new BinaryWriter(new MemoryStream());
			BinaryReader br = new BinaryReader(File.OpenRead(outfile));

			// Write standard header and TGZ info
			byte[] data = Constants.TorrentGZHeader
							.Concat(Style.StringToByteArray(rom.MD5)) // MD5
							.Concat(Style.StringToByteArray(rom.CRC)) // CRC
							.Concat(BitConverter.GetBytes(rom.Size).Reverse().ToArray()) // Long size (Mirrored)
						.ToArray();
			sw.Write(data);

			// Finally, copy the rest of the data from the original file
			br.BaseStream.Seek(10, SeekOrigin.Begin);
			int i = 10;
			while (br.BaseStream.Position < br.BaseStream.Length)
			{
				sw.Write(br.ReadByte());
				i++;
			}

			// Dispose of the stream
			br.Dispose();

			// Now write the new file over the original
			BinaryWriter bw = new BinaryWriter(File.Open(outfile, FileMode.Create));
			sw.BaseStream.Seek(0, SeekOrigin.Begin);
			bw.BaseStream.Seek(0, SeekOrigin.Begin);
			sw.BaseStream.CopyTo(bw.BaseStream);

			// Dispose of the streams
			bw.Dispose();
			sw.Dispose();

			// If we're in romba mode, create the subfolder and move the file
			if (romba)
			{
				string subfolder = Path.Combine(rom.SHA1.Substring(0, 2), rom.SHA1.Substring(2, 2), rom.SHA1.Substring(4, 2), rom.SHA1.Substring(6, 2));
				outDir = Path.Combine(outDir, subfolder);
				if (!Directory.Exists(outDir))
				{
					Directory.CreateDirectory(outDir);
				}

				try
				{
					File.Move(outfile, Path.Combine(outDir, Path.GetFileName(outfile)));
				}
				catch (Exception ex)
				{
					logger.Warning(ex.ToString());
					File.Delete(outfile);
				}
			}

			return true;
		}

		#endregion
	}
}
