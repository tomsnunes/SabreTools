using OCRC;
using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Common;
using SharpCompress.Reader;
using System;
//using Ionic.Zlib;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SabreTools.Helper
{
	public class FileTools
	{
		#region Archive Writing

		/// <summary>
		/// Copy a file to an output archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outputDirectory">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteToArchive(string inputFile, string outputDirectory, Rom rom)
		{
			bool success = false;

			// If the input file doesn't exist, return
			if (!File.Exists(inputFile))
			{
				return success;
			}

			string archiveFileName = Path.Combine(outputDirectory, rom.Machine.Name + ".zip");

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
					outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
					outarchive.Dispose();
				}

				// Open the archive for writing
				using (outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update))
				{
					// If the archive doesn't already contain the entry, add it
					if (outarchive.GetEntry(rom.Name) == null)
					{
						outarchive.CreateEntryFromFile(inputFile, rom.Name, CompressionLevel.Optimal);
					}

					// If there's a Date attached to the rom, change the entry to that Date
					if (!String.IsNullOrEmpty(rom.Date))
					{
						DateTimeOffset dto = DateTimeOffset.Now;
						if (DateTimeOffset.TryParse(rom.Date, out dto))
						{
							outarchive.GetEntry(rom.Name).LastWriteTime = dto;
						}
					}
				}

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
		/// Copy a file to an output archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outputDirectory">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteToArchiveIonic(string inputFile, string outputDirectory, Rom rom)
		{
			

			return true;
		}

		/// <summary>
		/// Write an input file to a torrent GZ file
		/// </summary>
		/// <param name="input">File to write from</param>
		/// <param name="outdir">Directory to write archive to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the write was a success, false otherwise</returns>
		/// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
		public static bool WriteTorrentGZ(string input, string outdir, bool romba, Logger logger)
		{
			// Check that the input file exists
			if (!File.Exists(input))
			{
				logger.Warning("File " + input + " does not exist!");
				return false;
			}
			input = Path.GetFullPath(input);

			// Make sure the output directory exists
			if (!Directory.Exists(outdir))
			{
				Directory.CreateDirectory(outdir);
			}
			outdir = Path.GetFullPath(outdir);

			// Now get the Rom info for the file so we have hashes and size
			Rom rom = FileTools.GetSingleFileInfo(input);

			// If it doesn't exist, create the output file and then write
			string outfile = Path.Combine(outdir, rom.HashData.SHA1 + ".gz");
			using (FileStream inputstream = new FileStream(input, FileMode.Open))
			using (GZipStream output = new GZipStream(File.Open(outfile, FileMode.Create, FileAccess.Write), CompressionMode.Compress))
			{
				inputstream.CopyTo(output);
			}

			// Now that it's ready, inject the header info
			using (BinaryWriter sw = new BinaryWriter(new MemoryStream()))
			{
				using (BinaryReader br = new BinaryReader(File.OpenRead(outfile)))
				{
					// Write standard header and TGZ info
					byte[] data = Constants.TorrentGZHeader
									.Concat(Style.StringToByteArray(rom.HashData.MD5)) // MD5
									.Concat(Style.StringToByteArray(rom.HashData.CRC)) // CRC
									.Concat(BitConverter.GetBytes(rom.HashData.Size).Reverse().ToArray()) // Long size (Mirrored)
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
				}

				using (BinaryWriter bw = new BinaryWriter(File.Open(outfile, FileMode.Create)))
				{
					// Now write the new file over the original
					sw.BaseStream.Seek(0, SeekOrigin.Begin);
					bw.BaseStream.Seek(0, SeekOrigin.Begin);
					sw.BaseStream.CopyTo(bw.BaseStream);
				}
			}

			// If we're in romba mode, create the subfolder and move the file
			if (romba)
			{
				string subfolder = Path.Combine(rom.HashData.SHA1.Substring(0, 2), rom.HashData.SHA1.Substring(2, 2), rom.HashData.SHA1.Substring(4, 2), rom.HashData.SHA1.Substring(6, 2));
				outdir = Path.Combine(outdir, subfolder);
				if (!Directory.Exists(outdir))
				{
					Directory.CreateDirectory(outdir);
				}

				try
				{
					File.Move(outfile, Path.Combine(outdir, Path.GetFileName(outfile)));
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

		#region Archive Extraction

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempdir, Logger logger)
		{
			return ExtractArchive(input, tempdir, ArchiveScanLevel.Both, ArchiveScanLevel.External, ArchiveScanLevel.External, ArchiveScanLevel.Both, logger);
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempdir, int sevenzip,
			int gz, int rar, int zip, Logger logger)
		{
			return ExtractArchive(input, tempdir, (ArchiveScanLevel)sevenzip, (ArchiveScanLevel)gz, (ArchiveScanLevel)rar, (ArchiveScanLevel)zip, logger);
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Archive handling level for 7z</param>
		/// <param name="gz">Archive handling level for GZip</param>
		/// <param name="rar">Archive handling level for RAR</param>
		/// <param name="zip">Archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempdir, ArchiveScanLevel sevenzip,
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

			IReader reader = null;
			SevenZipArchive sza = null;
			try
			{
				if (at == ArchiveType.SevenZip && sevenzip != ArchiveScanLevel.External)
				{
					sza = SevenZipArchive.Open(File.OpenRead(input));
					logger.Log("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					// Extract all files to the temp directory
					sza.WriteToDirectory(tempdir, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
					encounteredErrors = false;
				}
				else if (at == ArchiveType.GZip && gz != ArchiveScanLevel.External)
				{
					logger.Log("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					using (FileStream itemstream = File.OpenRead(input))
					{
						using (FileStream outstream = File.Create(Path.Combine(tempdir, Path.GetFileNameWithoutExtension(input))))
						{
							using (GZipStream gzstream = new GZipStream(itemstream, CompressionMode.Decompress))
							{
								gzstream.CopyTo(outstream);
							}
						}
					}
					encounteredErrors = false;
				}
				else
				{
					reader = ReaderFactory.Open(File.OpenRead(input));
					logger.Log("Found archive of type: " + at);

					if ((at == ArchiveType.Zip && zip != ArchiveScanLevel.External) ||
						(at == ArchiveType.Rar && rar != ArchiveScanLevel.External))
					{
						// Create the temp directory
						Directory.CreateDirectory(tempdir);

						// Extract all files to the temp directory
						reader.WriteAllToDirectory(tempdir, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
						encounteredErrors = false;
					}
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
			finally
			{
				reader?.Dispose();
				sza?.Dispose();
			}

			return encounteredErrors;
		}

		/// <summary>
		/// Attempt to extract a file from an archive
		/// </summary>
		/// <param name="input">Name of the archive to be extracted</param>
		/// <param name="entryname">Name of the entry to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Name of the extracted file, null on error</returns>
		public static string ExtractSingleItemFromArchive(string input, string entryname,
			string tempdir, Logger logger)
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
				reader = ReaderFactory.Open(File.OpenRead(input));

				if (at == ArchiveType.Zip || at == ArchiveType.SevenZip || at == ArchiveType.Rar)
				{
					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					while (reader.MoveToNextEntry())
					{
						logger.Log("Current entry name: '" + reader.Entry.Key + "'");
						if (reader.Entry != null && reader.Entry.Key.Contains(entryname))
						{
							outfile = Path.GetFullPath(Path.Combine(tempdir, reader.Entry.Key));
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
					// Dispose the original reader
					reader.Dispose();

					using(FileStream itemstream = File.OpenRead(input))
					{
						using (FileStream outstream = File.Create(Path.Combine(tempdir, Path.GetFileNameWithoutExtension(input))))
						{
							using (GZipStream gzstream = new GZipStream(itemstream, CompressionMode.Decompress))
							{
								gzstream.CopyTo(outstream);
								outfile = Path.GetFullPath(Path.Combine(tempdir, Path.GetFileNameWithoutExtension(input)));
							}
						}
					}
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

		#region Archive-to-Archive Handling

		/// <summary>
		/// Attempt to copy a file between archives
		/// </summary>
		/// <param name="inputArchive">Source archive name</param>
		/// <param name="outputDirectory">Destination archive name</param>
		/// <param name="sourceEntryName">Input entry name</param>
		/// <param name="destEntryName">Output entry name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the copy was a success, false otherwise</returns>
		public static bool CopyFileBetweenArchives(string inputArchive, string outputDirectory,
			string sourceEntryName, Rom destEntry, Logger logger)
		{
			string tempfile = ExtractSingleItemFromArchive(inputArchive, sourceEntryName, Path.GetTempPath(), logger);
			return WriteToArchive(tempfile, outputDirectory, destEntry);
		}

		#endregion

		#region File Information

		/// <summary>
		/// Retrieve file information for a single file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="noMD5">True if MD5 hashes should not be calculated, false otherwise (default)</param>
		/// <param name="noSHA1">True if SHA-1 hashes should not be calcluated, false otherwise (default)</param>
		/// <param name="offset">Set a >0 number for getting hash for part of the file, 0 otherwise (default)</param>
		/// <param name="date">True if the file Date should be included, false otherwise (default)</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		/// <remarks>Add read-offset for hash info</remarks>
		public static Rom GetSingleFileInfo(string input, bool noMD5 = false, bool noSHA1 = false, long offset = 0, bool date = false)
		{
			// Add safeguard if file doesn't exist
			if (!File.Exists(input))
			{
				return new Rom();
			}

			FileInfo temp = new FileInfo(input);
			Rom rom = new Rom
			{
				Name = Path.GetFileName(input),
				Type = ItemType.Rom,
				HashData = new Hash
				{
					Size = temp.Length,
					CRC = string.Empty,
					MD5 = string.Empty,
					SHA1 = string.Empty,
				},
				Date = (date ? temp.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") : ""),
			};

			try
			{
				using (OptimizedCRC crc = new OptimizedCRC())
				using (MD5 md5 = MD5.Create())
				using (SHA1 sha1 = SHA1.Create())
				using (FileStream fs = File.OpenRead(input))
				{
					// Seek to the starting position, if one is set
					if (offset > 0)
					{
						fs.Seek(offset, SeekOrigin.Begin);
					}

					byte[] buffer = new byte[1024];
					int read;
					while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
					{
						crc.Update(buffer, 0, read);
						if (!noMD5)
						{
							md5.TransformBlock(buffer, 0, read, buffer, 0);
						}
						if (!noSHA1)
						{
							sha1.TransformBlock(buffer, 0, read, buffer, 0);
						}
					}

					crc.Update(buffer, 0, 0);
					rom.HashData.CRC = crc.Value.ToString("X8").ToLowerInvariant();

					if (!noMD5)
					{
						md5.TransformFinalBlock(buffer, 0, 0);
						rom.HashData.MD5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
					}
					if (!noSHA1)
					{
						sha1.TransformFinalBlock(buffer, 0, 0);
						rom.HashData.SHA1 = BitConverter.ToString(sha1.Hash).Replace("-", "").ToLowerInvariant();
					}
				}
			}
			catch (IOException)
			{
				return new Rom();
			}

			return rom;
		}

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
					using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
					{
						br.BaseStream.Seek(-8, SeekOrigin.End);
						byte[] headercrc = br.ReadBytes(4);
						crc = BitConverter.ToString(headercrc.Reverse().ToArray()).Replace("-", string.Empty).ToLowerInvariant();
						byte[] headersize = br.ReadBytes(4);
						size = BitConverter.ToInt32(headersize.Reverse().ToArray(), 0);
					}
				}

				reader = ReaderFactory.Open(File.OpenRead(input));

				if (at != ArchiveType.Tar)
				{
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
								Machine = new Machine
								{
									Name = gamename,
								},
								HashData = new Hash
								{
									Size = (size == 0 ? reader.Entry.Size : size),
									CRC = (crc == "" ? reader.Entry.Crc.ToString("X").ToLowerInvariant() : crc),
								},
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
			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			{
				header = br.ReadBytes(12);
				headermd5 = br.ReadBytes(16);
				headercrc = br.ReadBytes(4);
				headersz = br.ReadBytes(8);
			}

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
				Machine = new Machine
				{
					Name = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				},
				Name = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				HashData = new Hash
				{
					Size = extractedsize,
					CRC = gzcrc.ToLowerInvariant(),
					MD5 = gzmd5.ToLowerInvariant(),
					SHA1 = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				},
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
				using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
				{
					magic = br.ReadBytes(8);
				}

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

			ArchiveType? archiveType = FileTools.GetCurrentArchiveType(input, logger);
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
		/// Read the information from an input zip file and does not populate data
		/// </summary>
		/// <param name="input">Name of the input file to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <remarks>
		/// This does not do any handling for Zip64 currently
		/// https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT
		/// </remarks>
		public static ZipArchiveStruct GetZipFileInfo(string input, Logger logger)
		{
			// Create the zip archive struct to hold all of the information
			ZipArchiveStruct zas = new ZipArchiveStruct
			{
				FileName = Path.GetFileNameWithoutExtension(input),
				Entries = new List<ZipArchiveEntryStruct>(),
			};

			int position = -1;

			// Seek backwards to find the EOCD pattern
			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			{
				int sig = 101010256;
				int read = 0;
				int index = -5;
				while (sig != read && (-index) < new FileInfo(input).Length)
				{
					br.BaseStream.Seek(index, SeekOrigin.End);
					read = br.ReadInt32();
					index--;
				}

				// If we found the signature, then set the correct position
				if (sig == read)
				{
					position = (int)br.BaseStream.Position - 4;
				}
			}

			// If we found the EOCD, get all of the information out of that area
			if (position != -1)
			{
				zas.EOCDOffset = (int)position;
				using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
				{
					br.BaseStream.Seek(position, SeekOrigin.Begin);
					br.ReadInt32(); // end of central dir signature
					br.ReadInt16(); // number of this disk
					br.ReadInt16(); // number of the disk with the start of the central directory
					br.ReadInt16(); // total number of entries in the central directory on this disk
					br.ReadInt16(); // total number of entries in the central directory
					br.ReadInt32(); // size of the central directory
					position = br.ReadInt32(); // offset of start of central directory with respect to the starting disk number
					int commentlength = br.ReadInt16();
					zas.Comment = Style.ConvertHexToAscii(BitConverter.ToString(br.ReadBytes(commentlength)));
				}
			}

			// If we found the SOCD, get all of the information out of that area
			if (position != -1 && position != zas.EOCDOffset)
			{
				zas.SOCDOffset = position;
				using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
				{
					int temp = 0;
					while (temp != 84233040 /* digital signature */ && temp != 101010256 /* eocd */)
					{
						ZipArchiveEntryStruct zaes = new ZipArchiveEntryStruct();

						br.BaseStream.Seek(position, SeekOrigin.Begin);
						br.ReadInt32(); // central file header signature
						zaes.VersionMadeBy = (ArchiveVersion)br.ReadInt16();
						zaes.VersionNeeded = (ArchiveVersion)br.ReadInt16();
						zaes.GeneralPurposeBitFlag = (GeneralPurposeBitFlag)br.ReadInt16();
						zaes.CompressionMethod = (CompressionMethod)br.ReadInt16();
						zaes.LastModFileTime = br.ReadUInt16();
						zaes.LastModFileDate = br.ReadUInt16();
						zaes.CRC = br.ReadUInt32();
						zaes.CompressedSize = br.ReadUInt32();
						zaes.UncompressedSize = br.ReadUInt32();
						int fileNameLength = br.ReadInt16();
						int extraFieldLength = br.ReadInt16();
						int fileCommentLength = br.ReadInt16();
						br.ReadInt16(); // disk number start
						zaes.InternalFileAttributes = (InternalFileAttributes)br.ReadInt16();
						zaes.ExternalFileAttributes = br.ReadInt32();
						zaes.RelativeOffset = br.ReadInt32();
						zaes.FileName = br.ReadBytes(fileNameLength);
						zaes.ExtraField = br.ReadBytes(extraFieldLength);
						zaes.Comment = br.ReadBytes(fileCommentLength);
						zaes.Data = new byte[zaes.CompressedSize];
						position += 46 + fileNameLength + extraFieldLength + fileCommentLength;
						temp = br.ReadInt32();
						zas.Entries.Add(zaes);
					}
				}
			}

			// Finally, get a hash of the entire central directory (between SOCD and EOCD)
			if (zas.SOCDOffset > 0 && zas.EOCDOffset > 0)
			{
				using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
				{
					br.BaseStream.Seek(zas.SOCDOffset, SeekOrigin.Begin);
					byte[] cd = br.ReadBytes(zas.EOCDOffset - zas.SOCDOffset);

					OptimizedCRC ocrc = new OptimizedCRC();
					ocrc.Update(cd, 0, cd.Length);
					zas.CentralDirectoryCRC = ocrc.Value;
				}
			}

			return zas;
		}

		/// <summary>
		/// Read the information from an input 7z file
		/// </summary>
		/// <param name="input">Name of the input file to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <remarks>http://cpansearch.perl.org/src/BJOERN/Compress-Deflate7-1.0/7zip/DOC/7zFormat.txt</remarks>
		public static void GetSevenZipFIleInfo(string input, Logger logger)
		{
			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			{
				br.ReadBytes(6); // BYTE kSignature[6] = {'7', 'z', 0xBC, 0xAF, 0x27, 0x1C};
				logger.User("ArchiveVersion (Major.Minor): " + br.ReadByte() + "." + br.ReadByte());
				logger.User("StartHeaderCRC: " + br.ReadUInt32());
				logger.User("StartHeader (NextHeaderOffset, NextHeaderSize, NextHeaderCRC)" + br.ReadUInt64() + ", " + br.ReadUInt64() + ", " + br.ReadUInt32());
			}
		}

		#endregion

		#region File Manipulation

		/// <summary>
		/// Remove an arbitrary number of bytes from the inputted file
		/// </summary>
		/// <param name="input">File to be cropped</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToRemoveFromHead">Bytes to be removed from head of file</param>
		/// <param name="bytesToRemoveFromTail">Bytes to be removed from tail of file</param>
		public static void RemoveBytesFromFile(string input, string output, long bytesToRemoveFromHead, long bytesToRemoveFromTail)
		{
			// If any of the inputs are invalid, skip
			if (!File.Exists(input) || new FileInfo(input).Length <= (bytesToRemoveFromHead + bytesToRemoveFromTail))
			{
				return;
			}

			// Read the input file and write to the fail
			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(output)))
			{
				int bufferSize = 1024;
				long adjustedLength = br.BaseStream.Length - bytesToRemoveFromTail;

				// Seek to the correct position
				br.BaseStream.Seek((bytesToRemoveFromHead < 0 ? 0 : bytesToRemoveFromHead), SeekOrigin.Begin);

				// Now read the file in chunks and write out
				byte[] buffer = new byte[bufferSize];
				while (br.BaseStream.Position <= (adjustedLength - bufferSize))
				{
					buffer = br.ReadBytes(bufferSize);
					bw.Write(buffer);
				}

				// For the final chunk, if any, write out only that number of bytes
				int length = (int)(adjustedLength - br.BaseStream.Position);
				buffer = new byte[length];
				buffer = br.ReadBytes(length);
				bw.Write(buffer);
			}
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">String representing bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">String representing bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, string bytesToAddToHead, string bytesToAddToTail)
		{
			// Source: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
			byte[] bytesToAddToHeadArray = new byte[bytesToAddToHead.Length / 2];
			for (int i = 0; i < bytesToAddToHead.Length; i += 2)
			{
				bytesToAddToHeadArray[i / 2] = Convert.ToByte(bytesToAddToHead.Substring(i, 2), 16);
			}
			byte[] bytesToAddToTailArray = new byte[bytesToAddToTail.Length / 2];
			for (int i = 0; i < bytesToAddToTail.Length; i += 2)
			{
				bytesToAddToTailArray[i / 2] = Convert.ToByte(bytesToAddToTail.Substring(i, 2), 16);
			}

			AppendBytesToFile(input, output, bytesToAddToHeadArray, bytesToAddToTailArray);
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">Bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">Bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
		{
			// If any of the inputs are invalid, skip
			if (!File.Exists(input))
			{
				return;
			}

			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(output)))
			{
				if (bytesToAddToHead.Count() > 0)
				{
					bw.Write(bytesToAddToHead);
				}

				int bufferSize = 1024;

				// Now read the file in chunks and write out
				byte[] buffer = new byte[bufferSize];
				while (br.BaseStream.Position <= (br.BaseStream.Length - bufferSize))
				{
					buffer = br.ReadBytes(bufferSize);
					bw.Write(buffer);
				}

				// For the final chunk, if any, write out only that number of bytes
				int length = (int)(br.BaseStream.Length - br.BaseStream.Position);
				buffer = new byte[length];
				buffer = br.ReadBytes(length);
				bw.Write(buffer);

				if (bytesToAddToTail.Count() > 0)
				{
					bw.Write(bytesToAddToTail);
				}
			}
		}

		/// <summary>
		/// Copy a file to a new location, creating directories as needed
		/// </summary>
		/// <param name="input">Input filename</param>
		/// <param name="output">Output filename</param>
		public static void CopyFileToNewLocation(string input, string output)
		{
			if (File.Exists(input) && !File.Exists(output))
			{
				if (!Directory.Exists(Path.GetDirectoryName(output)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(output));
				}
				File.Copy(input, output);
			}
		}

		/// <summary>
		/// Cleans out the temporary directory
		/// </summary>
		/// <param name="dirname">Name of the directory to clean out</param>
		public static void CleanDirectory(string dirname)
		{
			foreach (string file in Directory.EnumerateFiles(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					File.Delete(file);
				}
				catch { }
			}
			foreach (string dir in Directory.EnumerateDirectories(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					Directory.Delete(dir, true);
				}
				catch { }
			}
		}

		#endregion

		#region TorrentZip

		/// <summary>
		/// Write an existing zip archive construct to a torrent zip file
		/// </summary>
		/// <param name="zae">ZipArchiveStruct representing the zipfile with compressed data</param>
		/// <param name="output">Name of the file to write out to</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <remarks>
		/// Things that need to be done:
		///		Get rid of redundant directories (ones that are implied by files inside them)
		/// </remarks>
		public static void WriteTorrentZip(ZipArchiveStruct zae, string output, Logger logger)
		{
			// First, rearrange entries by name
			List<ZipArchiveEntryStruct> entries = zae.Entries;

			using (BinaryWriter bw = new BinaryWriter(File.Open(output, FileMode.Create)))
			{
				List<long> offsets = new List<long>();

				// First, write out entries
				foreach (ZipArchiveEntryStruct zaes in zae.Entries)
				{
					offsets.Add(bw.BaseStream.Position);
					bw.Write(Constants.LocalFileHeaderSignature);
					bw.Write((ushort)20);
					bw.Write((ushort)GeneralPurposeBitFlag.DeflatingMaximumCompression);
					bw.Write((ushort)CompressionMethod.Deflated);
					bw.Write((ushort)48128);
					bw.Write((ushort)8600);
					bw.Write(zaes.CRC);
					bw.Write(zaes.CompressedSize);
					bw.Write(zaes.UncompressedSize);
					bw.Write((ushort)zaes.FileName.Length);
					bw.Write((ushort)0);
					bw.Write(zaes.FileName);
					bw.Write(zaes.Data);
					if ((zaes.GeneralPurposeBitFlag & GeneralPurposeBitFlag.ZeroedCRCAndSize) != 0)
					{
						bw.Write(zaes.CRC);
						bw.Write(zaes.CompressedSize);
						bw.Write(zaes.UncompressedSize);
					}
				}

				// Then write out the central directory
				int index = 0;
				zae.SOCDOffset = (int)bw.BaseStream.Position;
				foreach (ZipArchiveEntryStruct zaes in zae.Entries)
				{
					bw.Write(Constants.CentralDirectoryHeaderSignature);
					bw.Write((ushort)ArchiveVersion.MSDOSandOS2);
					bw.Write((ushort)20);
					bw.Write((ushort)GeneralPurposeBitFlag.DeflatingMaximumCompression);
					bw.Write((ushort)CompressionMethod.Deflated);
					bw.Write((ushort)48128);
					bw.Write((ushort)8600);
					bw.Write(zaes.CRC);
					bw.Write(zaes.CompressedSize);
					bw.Write(zaes.UncompressedSize);
					bw.Write((short)zaes.FileName.Length);
					bw.Write((short)0);
					bw.Write((short)0);
					bw.Write((short)0);
					bw.Write((short)0);
					bw.Write(0);
					bw.Write((int)offsets[index]);
					bw.Write(zaes.FileName);
					bw.Write(zaes.ExtraField);
					bw.Write(zaes.Comment);
					index++;
				}
				zae.EOCDOffset = (int)bw.BaseStream.Position;

				// Finally, write out the end record
				bw.Write(new byte[] { 0x50, 0x4b, 0x05, 0x06 }); // end of central dir signature
				bw.Write((short)0);
				bw.Write((short)0);
				bw.Write((short)zae.Entries.Count);
				bw.Write((short)zae.Entries.Count);
				bw.Write(zae.EOCDOffset - zae.SOCDOffset);
				bw.Write(zae.SOCDOffset);
				bw.Write((short)22);
				bw.Write("TORRENTZIPPED-".ToCharArray());
			}

			using (BinaryReader br = new BinaryReader(File.OpenRead(output)))
			{
				br.BaseStream.Seek(zae.SOCDOffset, SeekOrigin.Begin);
				byte[] cd = br.ReadBytes(zae.EOCDOffset - zae.SOCDOffset);

				OptimizedCRC ocrc = new OptimizedCRC();
				ocrc.Update(cd, 0, cd.Length);
				zae.CentralDirectoryCRC = ocrc.Value;
			}

			using (BinaryWriter bw = new BinaryWriter(File.Open(output, FileMode.Append)))
			{
				bw.Write(zae.CentralDirectoryCRC.ToString("X8").ToCharArray());
			}
		}

		/// <summary>
		/// Reorder all of the files in the archive based on lowercase filename
		/// </summary>
		/// <param name="inputArchive">Source archive name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the operation succeeded, false otherwise</returns>
		public static bool TorrentZipArchive(string inputArchive, Logger logger)
		{
			bool success = false;

			// If the input file doesn't exist, return
			if (!File.Exists(inputArchive))
			{
				return success;
			}

			// Make sure the file is a zip file to begin with
			if (GetCurrentArchiveType(inputArchive, logger) != ArchiveType.Zip)
			{
				return success;
			}

			ZipArchiveStruct zas = GetZipFileInfo(inputArchive, logger);

			for (int i = 0; i < zas.Entries.Count; i++)
			{
				zas.Entries[i] = PopulateEntry(zas.Entries[i], inputArchive);
				zas.Entries[i] = DecompressEntry(zas.Entries[i], inputArchive);
				zas.Entries[i] = CompressEntry(zas.Entries[i]);
			}

			WriteTorrentZip(zas, inputArchive + ".new", logger);
			Console.ReadLine();

			return success;
		}

		/// <summary>
		/// Convert uncompressed data in entry to compressed data
		/// </summary>
		/// <remarks>Does not seem to create TZIP compatible streams</remarks>
		private static ZipArchiveEntryStruct CompressEntry(ZipArchiveEntryStruct zaes)
		{
			byte[] uncompressedData = zaes.Data;
			using (MemoryStream cms = new MemoryStream())
			using (MemoryStream ums = new MemoryStream(uncompressedData))
			using (DeflateStream ds = new DeflateStream(cms, CompressionMode.Compress))
			{
				ums.CopyTo(ds);
				ds.Flush();
				zaes.Data = cms.ToArray();
			}
			return zaes;
		}

		/// <summary>
		/// Populate an archive entry struct with uncompressed data from an input zip archive
		/// </summary>
		private static ZipArchiveEntryStruct DecompressEntry(ZipArchiveEntryStruct zaes, string input)
		{
			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			{
				br.BaseStream.Seek(zaes.RelativeOffset + 30 + zaes.FileName.Length, SeekOrigin.Begin);
				byte[] compressedData = br.ReadBytes((int)zaes.CompressedSize);
				using (MemoryStream ums = new MemoryStream())
				using (MemoryStream cms = new MemoryStream(compressedData))
				using (DeflateStream ds = new DeflateStream(cms, CompressionMode.Decompress))
				{
					ds.CopyTo(ums);
					ums.Flush();
					zaes.Data = ums.ToArray();
				}
			}
			return zaes;
		}

		/// <summary>
		/// Populate an archive entry struct with data from an input zip archive
		/// </summary>
		private static ZipArchiveEntryStruct PopulateEntry(ZipArchiveEntryStruct zaes, string input)
		{
			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			{
				br.BaseStream.Seek(zaes.RelativeOffset + 30 + zaes.FileName.Length, SeekOrigin.Begin);
				zaes.Data = br.ReadBytes((int)zaes.CompressedSize);
			}
			return zaes;
		}

		#endregion
	}
}
