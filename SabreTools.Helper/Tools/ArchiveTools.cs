using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using SabreTools.Helper.Data;
using SabreTools.Helper.Dats;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = System.IO.BinaryWriter;
using EndOfStreamException = System.IO.EndOfStreamException;
using FileAccess = System.IO.FileAccess;
using FileMode = System.IO.FileMode;
using FileShare = System.IO.FileShare;
using FileStream = System.IO.FileStream;
using MemoryStream = System.IO.MemoryStream;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
#endif
using Ionic.Zlib;
using ROMVault2.SupportedFiles.Zip;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SabreTools.Helper.Tools
{
	public static class ArchiveTools
	{
		private const int _bufferSize = 4096 * 128;

		#region Extraction

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
				// 7-zip
				if (at == ArchiveType.SevenZip && (archiveScanLevel & ArchiveScanLevel.SevenZipInternal) != 0)
				{
					logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					// Extract all files to the temp directory
					SevenZipArchive sza = SevenZipArchive.Open(File.OpenRead(input));
					foreach (SevenZipArchiveEntry entry in sza.Entries)
					{
						entry.WriteToDirectory(tempDir, new ExtractionOptions{ PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
					}
					encounteredErrors = false;
					sza.Dispose();
				}

				// GZip
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

				// RAR
				else if (at == ArchiveType.Rar && (archiveScanLevel & ArchiveScanLevel.RarInternal) != 0)
				{
					logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					// Extract all files to the temp directory
					RarArchive ra = RarArchive.Open(input);
					foreach (RarArchiveEntry entry in ra.Entries)
					{
						entry.WriteToDirectory(tempDir, new ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
					}
					encounteredErrors = false;
					ra.Dispose();
				}

				// TAR
				else if (at == ArchiveType.Tar && (archiveScanLevel & ArchiveScanLevel.TarInternal) != 0)
				{
					logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempDir);

					// Extract all files to the temp directory
					TarArchive ta = TarArchive.Open(input);
					foreach (TarArchiveEntry entry in ta.Entries)
					{
						entry.WriteToDirectory(tempDir, new ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
					}
					encounteredErrors = false;
					ta.Dispose();
				}

				// Zip
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

						// Create the rest of the path, if needed
						if (!String.IsNullOrEmpty(Path.GetDirectoryName(zf.Entries[i].FileName)))
						{
							Directory.CreateDirectory(Path.Combine(tempDir, Path.GetDirectoryName(zf.Entries[i].FileName)));
						}

						// If the entry ends with a directory separator, continue to the next item, if any
						if (zf.Entries[i].FileName.EndsWith(Path.DirectorySeparatorChar.ToString())
							|| zf.Entries[i].FileName.EndsWith(Path.AltDirectorySeparatorChar.ToString())
							|| zf.Entries[i].FileName.EndsWith(Path.PathSeparator.ToString()))
						{
							continue;
						}

						FileStream writeStream = File.OpenWrite(Path.Combine(tempDir, zf.Entries[i].FileName));

						byte[] ibuffer = new byte[_bufferSize];
						int ilen;
						while ((ilen = readStream.Read(ibuffer, 0, _bufferSize)) > 0)
						{
							writeStream.Write(ibuffer, 0, ilen);
							writeStream.Flush();
						}

						zr = zf.CloseReadStream();
						writeStream.Dispose();
					}
					encounteredErrors = false;
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
		public static string ExtractItem(string input, string entryName, string tempDir, Logger logger)
		{
			string realEntry = "";
			Stream ms = ExtractStream(input, entryName, out realEntry, logger);

			// If we got out a null or empty entry, then we don't have a stream
			if (String.IsNullOrEmpty(realEntry))
			{
				ms.Dispose();
				return null;
			}

			realEntry = Path.Combine(Path.GetFullPath(tempDir), realEntry);
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
		/// Attempt to extract a stream from an archive
		/// </summary>
		/// <param name="input">Name of the archive to be extracted</param>
		/// <param name="entryName">Name of the entry to be extracted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Name of the extracted file, null on error</returns>
		public static Stream ExtractStream(string input, string entryName, out string realEntry, Logger logger)
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

			try
			{
				switch (at)
				{
					case ArchiveType.SevenZip:
						SevenZipArchive sza = SevenZipArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false, });
						foreach (SevenZipArchiveEntry entry in sza.Entries)
						{
							logger.Verbose("Current entry name: '" + entry.Key + "'");
							if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
							{
								realEntry = entry.Key;
								entry.WriteTo(st);
								break;
							}
						}
						sza.Dispose();
						break;

					case ArchiveType.GZip:
						// Decompress the input stream
						realEntry = Path.GetFileNameWithoutExtension(input);
						GZipStream gzstream = new GZipStream(File.OpenRead(input), CompressionMode.Decompress);
						gzstream.CopyTo(st);

						// Dispose of the stream
						gzstream.Dispose();
						break;

					case ArchiveType.Rar:
						RarArchive ra = RarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false, });
						foreach (RarArchiveEntry entry in ra.Entries)
						{
							logger.Verbose("Current entry name: '" + entry.Key + "'");
							if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
							{
								realEntry = entry.Key;
								entry.WriteTo(st);
								break;
							}
						}
						ra.Dispose();
						break;

					case ArchiveType.Tar:
						TarArchive ta = TarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false, });
						foreach (TarArchiveEntry entry in ta.Entries)
						{
							logger.Verbose("Current entry name: '" + entry.Key + "'");
							if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
							{
								realEntry = entry.Key;
								entry.WriteTo(st);
								break;
							}
						}
						ta.Dispose();
						break;

					case ArchiveType.Zip:
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
						break;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				st = null;
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

				switch (at)
				{
					case ArchiveType.SevenZip:
						SevenZipArchive sza = SevenZipArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false });
						foreach (SevenZipArchiveEntry entry in sza.Entries)
						{
							if (entry != null && !entry.IsDirectory)
							{
								logger.Verbose("Entry found: '" + entry.Key + "': "
									+ (size == 0 ? entry.Size : size) + ", "
									+ (crc == "" ? entry.Crc.ToString("X").ToLowerInvariant() : crc));

								roms.Add(new Rom
								{
									Type = ItemType.Rom,
									Name = entry.Key,
									Size = (size == 0 ? entry.Size : size),
									CRC = (crc == "" ? entry.Crc.ToString("X").ToLowerInvariant() : crc),

									Machine = new Machine
									{
										Name = gamename,
									},
								});
							}
						}
						break;

					case ArchiveType.GZip:// Get the CRC and size from the file
						BinaryReader br = new BinaryReader(File.OpenRead(input));
						br.BaseStream.Seek(-8, SeekOrigin.End);
						byte[] headercrc = br.ReadBytes(4);
						crc = BitConverter.ToString(headercrc.Reverse().ToArray()).Replace("-", string.Empty).ToLowerInvariant();
						byte[] headersize = br.ReadBytes(4);
						size = BitConverter.ToInt32(headersize.Reverse().ToArray(), 0);
						br.Dispose();
						break;

					case ArchiveType.Rar:
						RarArchive ra = RarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false });
						foreach (RarArchiveEntry entry in ra.Entries)
						{
							if (entry != null && !entry.IsDirectory)
							{
								logger.Verbose("Entry found: '" + entry.Key + "': "
									+ (size == 0 ? entry.Size : size) + ", "
									+ (crc == "" ? entry.Crc.ToString("X").ToLowerInvariant() : crc));

								roms.Add(new Rom
								{
									Type = ItemType.Rom,
									Name = entry.Key,
									Size = (size == 0 ? entry.Size : size),
									CRC = (crc == "" ? entry.Crc.ToString("X").ToLowerInvariant() : crc),

									Machine = new Machine
									{
										Name = gamename,
									},
								});
							}
						}
						break;

					case ArchiveType.Tar:
						TarArchive ta = TarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false });
						foreach (TarArchiveEntry entry in ta.Entries)
						{
							if (entry != null && !entry.IsDirectory)
							{
								logger.Verbose("Entry found: '" + entry.Key + "': "
									+ (size == 0 ? entry.Size : size) + ", "
									+ (crc == "" ? entry.Crc.ToString("X").ToLowerInvariant() : crc));

								roms.Add(new Rom
								{
									Type = ItemType.Rom,
									Name = entry.Key,
									Size = (size == 0 ? entry.Size : size),
									CRC = (crc == "" ? entry.Crc.ToString("X").ToLowerInvariant() : crc),

									Machine = new Machine
									{
										Name = gamename,
									},
								});
							}
						}
						break;

					case ArchiveType.Zip:
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
								Size = newsize,
								CRC = newcrc,

								Machine = new Machine
								{
									Name = gamename,
								},
							});
						}
						break;
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
				Name = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				Size = extractedsize,
				CRC = gzcrc.ToLowerInvariant(),
				MD5 = gzmd5.ToLowerInvariant(),
				SHA1 = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),

				Machine = new Machine
				{
					Name = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
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

			// If the file is null, then we have no archive type
			if (input == null)
			{
				return outtype;
			}

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
		/// (UNIMPLEMENTED) Read the information from an input 7z file
		/// </summary>
		/// <param name="input">Name of the input file to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <remarks>
		/// http://cpansearch.perl.org/src/BJOERN/Compress-Deflate7-1.0/7zip/DOC/7zFormat.txt</remarks>
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
		/// Write an input file to a tape archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTAR(string inputFile, string outDir, Rom rom, Logger logger, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteTAR(inputFiles, outDir, roms, logger, date: date);
		}

		/// <summary>
		/// Write a set of input files to a tape archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTAR(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger, bool date = false)
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
			string archiveFileName = Path.Combine(outDir, roms[0].Machine.Name + (roms[0].Machine.Name.EndsWith(".tar") ? "" : ".tar"));

			// Set internal variables
			Stream tarstream = new MemoryStream();
			TarArchive tarchive = TarArchive.Create();

			try
			{
				// If the full output path doesn't exist, create it
				if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
				}

				// We can only overwrite at this point, sorry
				tarstream = File.Open(archiveFileName, FileMode.Create, FileAccess.Write);

				// Map all inputs to index
				Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
				for (int i = 0; i < inputFiles.Count; i++)
				{
					inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), i);
				}

				// Sort the keys in TZIP order
				List<string> keys = inputIndexMap.Keys.ToList();
				keys.Sort(ZipFile.TorrentZipStringCompare);

				// Now add each of the files
				foreach (string key in keys)
				{
					int index = inputIndexMap[key];
					tarchive.AddEntry(key, inputFiles[index]);
				}

				tarchive.SaveTo(tarstream, new WriterOptions(CompressionType.None));
				success = true;
			}
			catch (Exception ex)
			{
				success = false;
				logger.Error(ex.ToString());
			}
			finally
			{
				tarstream?.Dispose();
				tarchive?.Dispose();
			}

			return success;
		}

		/// <summary>
		/// Write an input file to a torrent7z archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrent7Zip(string inputFile, string outDir, Rom rom, Logger logger, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteTorrent7Zip(inputFiles, outDir, roms, logger, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrent7z archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrent7Zip(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger, bool date = false)
		{
			return false;
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
				FileStream outputStream = File.Open(outfile, FileMode.Create, FileAccess.Write);

				// Open the output file for writing
				BinaryWriter sw = new BinaryWriter(outputStream);

				// Write standard header and TGZ info
				byte[] data = Constants.TorrentGZHeader
								.Concat(Style.StringToByteArray(rom.MD5)) // MD5
								.Concat(Style.StringToByteArray(rom.CRC)) // CRC
							.ToArray();
				sw.Write(data);
				sw.Write((ulong)rom.Size); // Long size (Unsigned, Mirrored)

				// Now create a deflatestream from the input file
				DeflateStream ds = new DeflateStream(outputStream, CompressionMode.Compress, CompressionLevel.BestCompression, true);

				// Copy the input stream to the output
				byte[] ibuffer = new byte[_bufferSize];
				int ilen;
				while ((ilen = inputStream.Read(ibuffer, 0, _bufferSize)) > 0)
				{
					ds.Write(ibuffer, 0, ilen);
					ds.Flush();
				}
				ds.Dispose();

				// Now write the standard footer
				sw.Write(Style.StringToByteArray(rom.CRC).Reverse().ToArray());
				sw.Write((uint)rom.Size);

				// Dispose of everything
				sw.Dispose();
				outputStream.Dispose();
				inputStream.Dispose();
			}

			// If we're in romba mode, create the subfolder and move the file
			if (romba)
			{
				MoveToRombaFolder(rom, outDir, outfile, logger);
			}

			return true;
		}

		/// <summary>
		/// Write an input file to a torrentlrzip archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentLRZ(string inputFile, string outDir, Rom rom, Logger logger, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteTorrentLRZ(inputFiles, outDir, roms, logger, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrentlrzip archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentLRZ(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger, bool date = false)
		{
			return false;
		}

		/// <summary>
		/// Write an input file to a torrentrar archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentRAR(string inputFile, string outDir, Rom rom, Logger logger, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteTorrentRAR(inputFiles, outDir, roms, logger, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrentrar archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentRAR(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger, bool date = false)
		{
			return false;
		}

		/// <summary>
		/// Write an input file to a torrentxz archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentXZ(string inputFile, string outDir, Rom rom, Logger logger, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteTorrentXZ(inputFiles, outDir, roms, logger, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrentxz archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentXZ(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger, bool date = false)
		{
			return false;
		}

		/// <summary>
		/// Write an input file to a torrentzip archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentZip(string inputFile, string outDir, Rom rom, Logger logger, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>();
			inputFiles.Add(inputFile);
			List<Rom> roms = new List<Rom>();
			roms.Add(rom);

			return WriteTorrentZip(inputFiles, outDir, roms, logger, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrentzip archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentZip(List<string> inputFiles, string outDir, List<Rom> roms, Logger logger, bool date = false)
		{
			bool success = false;
			string tempFile = Path.GetTempFileName();

			// If either list of roms is null or empty, return
			if (inputFiles == null || roms == null || inputFiles.Count == 0 || roms.Count == 0)
			{
				return success;
			}

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
			string archiveFileName = Path.Combine(outDir, roms[0].Machine.Name + (roms[0].Machine.Name.EndsWith(".zip") ? "" : ".zip"));

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
