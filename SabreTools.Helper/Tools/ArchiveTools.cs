using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Common;
using SharpCompress.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SabreTools.Helper
{
	public class ArchiveTools
	{
		private const int _bufferSize = 4096 * 128;

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
		public static bool CopyFileBetweenArchives(string inputArchive, string outDir, string sourceEntryName, Rom destEntry, Logger logger)
		{
			string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			string realName = ExtractSingleItemFromArchive(inputArchive, sourceEntryName, temp, logger);
			bool success = WriteToArchive(realName, outDir, destEntry, logger);
			Directory.Delete(temp, true);
			return success;
		}

		/// <summary>
		/// Convert a compressed DeflateStream to a compressed GZipStream
		/// </summary>
		/// <param name="input">DeflateStream to convert</param>
		/// <returns>Converted GZipStream</returns>
		public static Stream DeflateStreamToGZipStream(Stream input)
		{
			DeflateStream ds = new DeflateStream(input, CompressionMode.Decompress);
			GZipStream gz = new GZipStream(ds, CompressionMode.Compress);
			return gz;
		}

		/// <summary>
		/// Convert a compressed GZipStream to a compressed DeflateStream
		/// </summary>
		/// <param name="input">GZipStream to convert</param>
		/// <returns>Converted DeflateStream</returns>
		public static Stream GZipStreamToDeflateStream(Stream input)
		{
			GZipStream gz = new GZipStream(input, CompressionMode.Decompress);
			DeflateStream ds = new DeflateStream(gz, CompressionMode.Compress);
			return ds;
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
			return ExtractArchive(input, tempDir, GetArchiveScanLevelFromNumbers(0, 2, 2, 0), logger);
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempDir, ArchiveScanLevel archiveScanLevel, Logger logger)
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
				if (at == ArchiveType.SevenZip && (archiveScanLevel & ArchiveScanLevel.SevenZipInternal) != 0)
				{
					logger.Verbose("Found archive of type: " + at);

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
				else if (at == ArchiveType.GZip && (archiveScanLevel & ArchiveScanLevel.GZipInternal) != 0)
				{
					logger.Verbose("Found archive of type: " + at);

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
				else if (at == ArchiveType.Zip && (archiveScanLevel & ArchiveScanLevel.ZipInternal) != 0)
				{
					logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					// Extract all files to the temp directory
					ZipFile zf = new ZipFile();
					ZipReturn zr = zf.Open(input, new FileInfo(input).LastWriteTime.Ticks, true);
					if (zr != ZipReturn.ZipGood)
					{
						throw new Exception(ZipFile.ZipErrorMessageText(zr));
					}

					for (int i = 0; i < zf.EntriesCount && zr == ZipReturn.ZipGood; i++)
					{
						// Set defaults before writing out
						Stream readStream;
						ulong streamsize = 0;
						CompressionMethod cm = CompressionMethod.Stored;
						uint lastMod = 0;

						zr = zf.OpenReadStream(i, false, out readStream, out streamsize, out cm, out lastMod);

						FileStream writeStream = File.OpenWrite(Path.Combine(tempDir, zf.Entries[i].FileName));

						byte[] ibuffer = new byte[_bufferSize];
						int ilen;
						while ((ilen = readStream.Read(ibuffer, 0, _bufferSize)) > 0)
						{
							writeStream.Write(ibuffer, 0, ilen);
							writeStream.Flush();
						}

						encounteredErrors = false;
						zr = zf.CloseReadStream();
						writeStream.Dispose();
					}
				}
				else if (at == ArchiveType.Rar && (archiveScanLevel & ArchiveScanLevel.RarInternal) != 0)
				{
					logger.Verbose("Found archive of type: " + at);

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
		/// <param name="entryName">Name of the entry to be extracted</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Name of the extracted file, null on error</returns>
		public static string ExtractSingleItemFromArchive(string input, string entryName, string tempDir, Logger logger)
		{
			string realEntry = "";
			Stream ms = ExtractSingleStreamFromArchive(input, entryName, out realEntry, logger);

			realEntry = Path.GetFullPath(Path.Combine(tempDir, realEntry));
			if (!Directory.Exists(Path.GetDirectoryName(realEntry)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(realEntry));
			}

			FileStream fs = File.Open(realEntry, FileMode.Create, FileAccess.Write);

			byte[] ibuffer = new byte[_bufferSize];
			int ilen;
			while ((ilen = ms.Read(ibuffer, 0, _bufferSize)) > 0)
			{
				fs.Write(ibuffer, 0, ilen);
				fs.Flush();
			}

			// Dispose of the streams
			ms.Dispose();
			fs.Dispose();

			return realEntry;
		}

		/// <summary>
		/// Attempt to extract a file from an archive
		/// </summary>
		/// <param name="input">Name of the archive to be extracted</param>
		/// <param name="entryName">Name of the entry to be extracted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Name of the extracted file, null on error</returns>
		public static Stream ExtractSingleStreamFromArchive(string input, string entryName, out string realEntry, Logger logger)
		{
			// Set the real entry name
			realEntry = "";

			// Get a writable stream to return
			Stream st = new MemoryStream();

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return st;
			}

			IReader reader = null;
			try
			{
				if (at == ArchiveType.Zip)
				{
					ZipFile zf = new ZipFile();
					ZipReturn zr = zf.Open(input, new FileInfo(input).LastWriteTime.Ticks, true);
					if (zr != ZipReturn.ZipGood)
					{
						throw new Exception(ZipFile.ZipErrorMessageText(zr));
					}

					for (int i = 0; i < zf.EntriesCount && zr == ZipReturn.ZipGood; i++)
					{
						logger.Verbose("Current entry name: '" + zf.Entries[i].FileName + "'");
						if (zf.Entries[i].FileName.Contains(entryName))
						{
							realEntry = zf.Entries[i].FileName;

							// Set defaults before writing out
							Stream readStream;
							ulong streamsize = 0;
							CompressionMethod cm = CompressionMethod.Stored;
							uint lastMod = 0;

							zr = zf.OpenReadStream(i, false, out readStream, out streamsize, out cm, out lastMod);

							byte[] ibuffer = new byte[_bufferSize];
							int ilen;
							while ((ilen = readStream.Read(ibuffer, 0, _bufferSize)) > 0)
							{
								st.Write(ibuffer, 0, ilen);
								st.Flush();
							}

							zr = zf.CloseReadStream();
						}
					}
				}
				else if (at == ArchiveType.SevenZip || at == ArchiveType.Rar)
				{
					reader = ReaderFactory.Open(File.OpenRead(input));
					while (reader.MoveToNextEntry())
					{
						logger.Verbose("Current entry name: '" + reader.Entry.Key + "'");
						if (reader.Entry != null && reader.Entry.Key.Contains(entryName))
						{
							realEntry  = reader.Entry.Key;
							reader.WriteEntryTo(st);
						}
					}
				}
				else if (at == ArchiveType.GZip)
				{
					// Decompress the input stream
					realEntry = Path.GetFileNameWithoutExtension(input);
					GZipStream gzstream = new GZipStream(File.OpenRead(input), CompressionMode.Decompress);
					gzstream.CopyTo(st);

					// Dispose of the stream
					gzstream.Dispose();
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				st = null;
			}
			finally
			{
				reader?.Dispose();
			}

			st.Position = 0;
			return st;
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
				logger.Verbose("Found archive of type: " + at);
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
				else if (at == ArchiveType.Zip)
				{
					ZipFile zf = new ZipFile();
					ZipReturn zr = zf.Open(input, new FileInfo(input).LastWriteTime.Ticks, true);
					if (zr != ZipReturn.ZipGood)
					{
						throw new Exception(ZipFile.ZipErrorMessageText(zr));
					}

					for (int i = 0; i < zf.EntriesCount && zr == ZipReturn.ZipGood; i++)
					{
						string newname = zf.Entries[i].FileName;
						long newsize = (size == 0 ? (long)zf.Entries[i].UncompressedSize : size);
						string newcrc = BitConverter.ToString(zf.Entries[i].CRC.Reverse().ToArray(), 0, zf.Entries[i].CRC.Length).Replace("-", string.Empty).ToLowerInvariant();

						logger.Verbose("Entry found: '" + newname + "': " + newsize + ", " + newcrc);

						roms.Add(new Rom
						{
							Type = ItemType.Rom,
							Name = newname,
							MachineName = gamename,
							Size = newsize,
							CRC = newcrc,
						});
					}
				}
				else if (at != ArchiveType.Tar)
				{
					reader = ReaderFactory.Open(File.OpenRead(input));
					while (reader.MoveToNextEntry())
					{
						if (reader.Entry != null && !reader.Entry.IsDirectory)
						{
							logger.Verbose("Entry found: '" + reader.Entry.Key + "': "
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
				return null;
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				logger.Warning("Possibly corrupt file '" + input + "' with size " + Style.GetBytesReadable(filesize));
				return null;
			}

			// Get the Romba-specific header data
			byte[] header; // Get preamble header for checking
			byte[] headermd5; // MD5
			byte[] headercrc; // CRC
			ulong headersz; // Int64 size
			BinaryReader br = new BinaryReader(File.OpenRead(input));
			header = br.ReadBytes(12);
			headermd5 = br.ReadBytes(16);
			headercrc = br.ReadBytes(4);
			headersz = br.ReadUInt64();
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
				return null;
			}

			// Now convert the data and get the right position
			string gzmd5 = BitConverter.ToString(headermd5).Replace("-", string.Empty);
			string gzcrc = BitConverter.ToString(headercrc).Replace("-", string.Empty);
			long extractedsize = (long)headersz;

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
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="shouldExternalProcess">Output parameter determining if file should be processed externally</param>
		/// <param name="shouldInternalProcess">Output parameter determining if file should be processed internally</param>
		public static void GetInternalExternalProcess(string input, ArchiveScanLevel archiveScanLevel,
			Logger logger, out bool shouldExternalProcess, out bool shouldInternalProcess)
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
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipInternal) != 0);
					break;
				case ArchiveType.Rar:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarInternal) != 0);
					break;
				case ArchiveType.SevenZip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipInternal) != 0);
					break;
				case ArchiveType.Zip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipInternal) != 0);
					break;
			}
		}

		/// <summary>
		/// Get the archive scan level based on the inputs
		/// </summary>
		/// <param name="sevenzip">User-defined scan level for 7z archives</param>
		/// <param name="gzip">User-defined scan level for GZ archives</param>
		/// <param name="rar">User-defined scan level for RAR archives</param>
		/// <param name="zip">User-defined scan level for Zip archives</param>
		/// <returns>ArchiveScanLevel representing the levels</returns>
		public static ArchiveScanLevel GetArchiveScanLevelFromNumbers(int sevenzip, int gzip, int rar, int zip)
		{
			ArchiveScanLevel archiveScanLevel = 0x0000;

			// 7z
			sevenzip = (sevenzip < 0 || sevenzip > 2 ? 0 : sevenzip);
			switch (sevenzip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.SevenZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.SevenZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.SevenZipExternal;
					break;
			}

			// GZip
			gzip = (gzip < 0 || gzip > 2 ? 0 : gzip);
			switch (gzip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.GZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.GZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.GZipExternal;
					break;
			}

			// RAR
			rar = (rar < 0 || rar > 2 ? 0 : rar);
			switch (rar)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.RarBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.RarInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.RarExternal;
					break;
			}

			// Zip
			zip = (zip < 0 || zip > 2 ? 0 : zip);
			switch (zip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.ZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.ZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.ZipExternal;
					break;
			}

			return archiveScanLevel;
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
		/// Copy a file to an output torrentzip archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteToArchive(string inputFile, string outDir, Rom rom, Logger logger, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteToArchive(inputFiles, outDir, roms, logger, date: date);
		}

		/// <summary>
		/// Copy a set of files to an output torrentzip archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteToArchive(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger, bool date = false)
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
						inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), i);
					}

					// Sort the keys in TZIP order
					List<string> keys = inputIndexMap.Keys.ToList();
					keys.Sort(ZipFile.TorrentZipStringCompare);

					// Now add all of the files in order
					foreach (string key in keys)
					{
						// Get the index mapped to the key
						int index = inputIndexMap[key];

						// Open the input file for reading
						Stream freadStream = File.Open(inputFiles[index], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
						ulong istreamSize = (ulong)(new FileInfo(inputFiles[index]).Length);

						DateTime dt = DateTime.Now;
						if (date && !String.IsNullOrEmpty(roms[index].Date) && DateTime.TryParse(roms[index].Date.Replace('\\', '/'), out dt))
						{
							uint msDosDateTime = Style.ConvertDateTimeToMsDosTimeFormat(dt);
							zipFile.OpenWriteStream(false, false, roms[index].Name.Replace('\\', '/'), istreamSize,
								CompressionMethod.Deflated, out writeStream, lastMod: msDosDateTime);
						}
						else
						{
							zipFile.OpenWriteStream(false, true, roms[index].Name.Replace('\\', '/'), istreamSize, CompressionMethod.Deflated, out writeStream);
						}

						// Copy the input stream to the output
						byte[] ibuffer = new byte[_bufferSize];
						int ilen;
						while ((ilen = freadStream.Read(ibuffer, 0, _bufferSize)) > 0)
						{
							writeStream.Write(ibuffer, 0, ilen);
							writeStream.Flush();
						}
						freadStream.Dispose();
						zipFile.CloseWriteStream(Convert.ToUInt32(roms[index].CRC, 16));
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
						if (oldZipFile.Contains(roms[i].Name.Replace('\\', '/')))
						{
							continue;
						}

						inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), -(i + 1));
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
						// Get the index mapped to the key
						int index = inputIndexMap[key];

						// If we have the input file, add it now
						if (index < 0)
						{
							// Open the input file for reading
							Stream freadStream = File.Open(inputFiles[-index - 1], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
							ulong istreamSize = (ulong)(new FileInfo(inputFiles[-index - 1]).Length);

							DateTime dt = DateTime.Now;
							if (date && !String.IsNullOrEmpty(roms[-index - 1].Date) && DateTime.TryParse(roms[-index - 1].Date.Replace('\\', '/'), out dt))
							{
								uint msDosDateTime = Style.ConvertDateTimeToMsDosTimeFormat(dt);
								zipFile.OpenWriteStream(false, false, roms[-index - 1].Name.Replace('\\', '/'), istreamSize,
									CompressionMethod.Deflated, out writeStream, lastMod: msDosDateTime);
							}
							else
							{
								zipFile.OpenWriteStream(false, true, roms[-index - 1].Name.Replace('\\', '/'), istreamSize, CompressionMethod.Deflated, out writeStream);
							}

							// Copy the input stream to the output
							byte[] ibuffer = new byte[_bufferSize];
							int ilen;
							while ((ilen = freadStream.Read(ibuffer, 0, _bufferSize)) > 0)
							{
								writeStream.Write(ibuffer, 0, ilen);
								writeStream.Flush();
							}
							freadStream.Dispose();
							zipFile.CloseWriteStream(Convert.ToUInt32(roms[-index - 1].CRC, 16));
						}

						// Otherwise, copy the file from the old archive
						else
						{
							// Instantiate the streams
							CompressionMethod icompressionMethod = CompressionMethod.Stored;
							uint lastMod = 0;
							ulong istreamSize = 0;
							Stream zreadStream;
							oldZipFile.OpenReadStream(index, false, out zreadStream, out istreamSize, out icompressionMethod, out lastMod);
							zipFile.OpenWriteStream(false, lastMod == Constants.TorrentZipFileDateTime, oldZipFile.Filename(index),
								istreamSize, CompressionMethod.Deflated, out writeStream, lastMod: lastMod);

							// Copy the input stream to the output
							byte[] ibuffer = new byte[_bufferSize];
							int ilen;
							while ((ilen = zreadStream.Read(ibuffer, 0, _bufferSize)) > 0)
							{
								writeStream.Write(ibuffer, 0, ilen);
								writeStream.Flush();
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
			Rom rom = FileTools.GetFileInfo(input, logger);

			// Get the output file name
			string outfile = Path.Combine(outDir, rom.SHA1 + ".gz");

			// If the output file exists, don't try to write again
			if (!File.Exists(outfile))
			{
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
							.ToArray();
				sw.Write(data);
				sw.Write((ulong)rom.Size); // Long size (Unsigned, Mirrored)

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
			}

			// If we're in romba mode, create the subfolder and move the file
			if (romba)
			{
				MoveToRombaFolder(rom, outDir, outfile, logger);
			}

			return true;
		}

		/// <summary>
		/// Get the romba path for a file based on the rom's SHA-1
		/// </summary>
		/// <param name="rom">Rom to get the sha1 from</param>
		/// <param name="baseOutDir">Base output folder</param>
		/// <returns>Formatted path string to use</returns>
		public static string GetRombaPath(Rom rom, string baseOutDir)
		{
			string subfolder = Path.Combine(rom.SHA1.Substring(0, 2), rom.SHA1.Substring(2, 2), rom.SHA1.Substring(4, 2), rom.SHA1.Substring(6, 2));
			return Path.Combine(baseOutDir, subfolder);
		}

		/// <summary>
		/// Move a file to a named, Romba-style subdirectory
		/// </summary>
		/// <param name="rom">Rom to get the sha1 from</param>
		/// <param name="baseOutDir">Base output folder</param>
		/// <param name="filename">Name of the file to be moved</param>
		/// <param name="logger">Logger object for file and console output</param>
		public static void MoveToRombaFolder(Rom rom, string baseOutDir, string filename, Logger logger)
		{
			string outDir = GetRombaPath(rom, baseOutDir);
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			try
			{
				File.Move(filename, Path.Combine(outDir, Path.GetFileName(filename)));
			}
			catch (Exception ex)
			{
				logger.Warning(ex.ToString());
				File.Delete(filename);
			}
		}

		#endregion
	}
}
