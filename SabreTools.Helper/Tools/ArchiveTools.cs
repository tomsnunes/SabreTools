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
using FileStream = System.IO.FileStream;
using MemoryStream = System.IO.MemoryStream;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
#endif
using Ionic.Zlib;
using ROMVault2.SupportedFiles.Zip;
using SevenZip;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SabreTools.Helper.Tools
{
	/*
	 * TODO: Full archive support for: RAR, LRZip, ZPAQ?, Zstd?, LZ4?
	 * Torrent 7-zip: https://sourceforge.net/p/t7z/code/HEAD/tree/
	 * LRZIP: https://github.com/ckolivas/lrzip
	 * ZPAQ: https://github.com/zpaq/zpaq - In progress as external DLL
	 * Zstd: https://github.com/skbkontur/ZstdNet
	 * LZ4: https://github.com/lz4/lz4
	 */
	public static class ArchiveTools
	{
		private const int _bufferSize = 4096 * 128;

		#region Archive-to-Archive handling

		/// <summary>
		/// Transfer a single file from one archive to another
		/// </summary>
		/// <param name="inputArchive">Input archive name</param>
		/// <param name="inputEntry">Input entry name</param>
		/// <param name="outputDir">Output directory</param>
		/// <param name="outputEntry">Output Rom information</param>
		/// <param name="date">True if dates are preserved, false otherwise (default)</param>
		/// <returns>True if the transfer was a success, false otherwise</returns>
		public static bool Transfer(string inputArchive, string inputEntry, string outputDir, Rom outputEntry, bool date = false)
		{
			// Verify inputs
			// Create list versions
			// return Transfer(multiple)
			return false;
		}

		/// <summary>
		/// Transfer multiple files from one archive to another
		/// </summary>
		/// <param name="inputArchives">Input archive names</param>
		/// <param name="inputEntries">Input entry names</param>
		/// <param name="outputDir">Output directory</param>
		/// <param name="outputEntries">Output Rom informations</param>
		/// <param name="date">True if dates are preserved, false otherwise (default)</param>
		/// <returns>True if the transfesr were a success, false otherwise</returns>
		public static bool Transfer(List<string> inputArchives, List<string> inputEntries, string outputDir, List<Rom> outputEntries, bool date = false)
		{
			// Verify inputs
			// For each item, extract to stream, write to archive
			return false;
		}

		#endregion

		#region Extraction

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="outDir">Output directory for archive extraction</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string outDir, ArchiveScanLevel archiveScanLevel)
		{
			bool encounteredErrors = true;

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input);

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
					Globals.Logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(outDir);

					// Extract all files to the temp directory
					SevenZipArchive sza = SevenZipArchive.Open(FileTools.TryOpenRead(input));
					foreach (SevenZipArchiveEntry entry in sza.Entries)
					{
						entry.WriteToDirectory(outDir, new ExtractionOptions{ PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
					}
					encounteredErrors = false;
					sza.Dispose();
				}

				// GZip
				else if (at == ArchiveType.GZip && (archiveScanLevel & ArchiveScanLevel.GZipInternal) != 0)
				{
					Globals.Logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(outDir);

					// Decompress the input stream
					FileStream outstream = FileTools.TryCreate(Path.Combine(outDir, Path.GetFileNameWithoutExtension(input)));
					GZipStream gzstream = new GZipStream(FileTools.TryOpenRead(input), Ionic.Zlib.CompressionMode.Decompress);
					gzstream.CopyTo(outstream);

					// Dispose of the streams
					outstream.Dispose();
					gzstream.Dispose();

					encounteredErrors = false;
				}

				// RAR
				else if (at == ArchiveType.Rar && (archiveScanLevel & ArchiveScanLevel.RarInternal) != 0)
				{
					Globals.Logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(outDir);

					// Extract all files to the temp directory
					RarArchive ra = RarArchive.Open(input);
					foreach (RarArchiveEntry entry in ra.Entries)
					{
						entry.WriteToDirectory(outDir, new ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
					}
					encounteredErrors = false;
					ra.Dispose();
				}

				// TAR
				else if (at == ArchiveType.Tar && (archiveScanLevel & ArchiveScanLevel.TarInternal) != 0)
				{
					Globals.Logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(outDir);

					// Extract all files to the temp directory
					TarArchive ta = TarArchive.Open(input);
					foreach (TarArchiveEntry entry in ta.Entries)
					{
						entry.WriteToDirectory(outDir, new ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
					}
					encounteredErrors = false;
					ta.Dispose();
				}

				// Zip
				else if (at == ArchiveType.Zip && (archiveScanLevel & ArchiveScanLevel.ZipInternal) != 0)
				{
					Globals.Logger.Verbose("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(outDir);

					// Extract all files to the temp directory
					ZipFile zf = new ZipFile();
					ZipReturn zr = zf.Open(input, new FileInfo(input).LastWriteTime.Ticks, true);
					if (zr != ZipReturn.ZipGood)
					{
						throw new Exception(ZipFile.ZipErrorMessageText(zr));
					}

					for (int i = 0; i < zf.EntriesCount && zr == ZipReturn.ZipGood; i++)
					{
						// Open the read stream
						zr = zf.OpenReadStream(i, false, out Stream readStream, out ulong streamsize, out SabreTools.Helper.Data.CompressionMethod cm, out uint lastMod);

						// Create the rest of the path, if needed
						if (!String.IsNullOrEmpty(Path.GetDirectoryName(zf.Entries[i].FileName)))
						{
							Directory.CreateDirectory(Path.Combine(outDir, Path.GetDirectoryName(zf.Entries[i].FileName)));
						}

						// If the entry ends with a directory separator, continue to the next item, if any
						if (zf.Entries[i].FileName.EndsWith(Path.DirectorySeparatorChar.ToString())
							|| zf.Entries[i].FileName.EndsWith(Path.AltDirectorySeparatorChar.ToString())
							|| zf.Entries[i].FileName.EndsWith(Path.PathSeparator.ToString()))
						{
							continue;
						}

						FileStream writeStream = FileTools.TryCreate(Path.Combine(outDir, zf.Entries[i].FileName));

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
					zf.Close();
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
		/// <returns>Name of the extracted file, null on error</returns>
		public static string ExtractItem(string input, string entryName, string tempDir)
		{
			string realEntry = "";

			// Set the real entry name
			realEntry = "";

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return realEntry;
			}

			try
			{
				switch (at)
				{
					case ArchiveType.SevenZip:
						SevenZipArchive sza = SevenZipArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false, });
						foreach (SevenZipArchiveEntry entry in sza.Entries)
						{
							if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
							{
								realEntry = entry.Key;

								// Get the output path
								realEntry = Path.Combine(Path.GetFullPath(tempDir), realEntry);
								if (!Directory.Exists(Path.GetDirectoryName(realEntry)))
								{
									Directory.CreateDirectory(Path.GetDirectoryName(realEntry));
								}

								// Write the file out
								entry.WriteToFile(realEntry);
								break;
							}
						}
						sza.Dispose();
						break;

					case ArchiveType.GZip:
						// Decompress the input stream
						realEntry = Path.GetFileNameWithoutExtension(input);
						GZipStream gzstream = new GZipStream(FileTools.TryOpenRead(input), Ionic.Zlib.CompressionMode.Decompress);

						// Get the output path
						realEntry = Path.Combine(Path.GetFullPath(tempDir), realEntry);
						if (!Directory.Exists(Path.GetDirectoryName(realEntry)))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(realEntry));
						}

						// Write the file out
						FileStream gzfileout = FileTools.TryCreate(realEntry);
						byte[] gbuffer = new byte[_bufferSize];
						int glen;
						while ((glen = gzstream.Read(gbuffer, 0, _bufferSize)) > 0)
						{

							gzfileout.Write(gbuffer, 0, glen);
							gzfileout.Flush();
						}

						// Dispose of the streams
						gzstream.Dispose();
						gzfileout.Dispose();
						break;

					case ArchiveType.Rar:
						RarArchive ra = RarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false, });
						foreach (RarArchiveEntry entry in ra.Entries)
						{
							if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
							{
								realEntry = entry.Key;

								// Get the output path
								realEntry = Path.Combine(Path.GetFullPath(tempDir), realEntry);
								if (!Directory.Exists(Path.GetDirectoryName(realEntry)))
								{
									Directory.CreateDirectory(Path.GetDirectoryName(realEntry));
								}

								// Write the file out
								entry.WriteToFile(realEntry);
								break;
							}
						}
						ra.Dispose();
						break;

					case ArchiveType.Tar:
						TarArchive ta = TarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false, });
						foreach (TarArchiveEntry entry in ta.Entries)
						{
							if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
							{
								realEntry = entry.Key;

								// Get the output path
								realEntry = Path.Combine(Path.GetFullPath(tempDir), realEntry);
								if (!Directory.Exists(Path.GetDirectoryName(realEntry)))
								{
									Directory.CreateDirectory(Path.GetDirectoryName(realEntry));
								}

								// Write the file out
								entry.WriteToFile(realEntry);
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
							if (zf.Entries[i].FileName.Contains(entryName))
							{
								realEntry = zf.Entries[i].FileName;

								// Open the read stream
								zr = zf.OpenReadStream(i, false, out Stream readStream, out ulong streamsize, out SabreTools.Helper.Data.CompressionMethod cm, out uint lastMod);

								// Get the output path
								realEntry = Path.Combine(Path.GetFullPath(tempDir), realEntry);
								if (!Directory.Exists(Path.GetDirectoryName(realEntry)))
								{
									Directory.CreateDirectory(Path.GetDirectoryName(realEntry));
								}

								// Write the file out
								FileStream zipfileout = FileTools.TryCreate(realEntry);
								byte[] zbuffer = new byte[_bufferSize];
								int zlen;
								while ((zlen = readStream.Read(zbuffer, 0, _bufferSize)) > 0)
								{
									zipfileout.Write(zbuffer, 0, zlen);
									zipfileout.Flush();
								}

								zr = zf.CloseReadStream();
								zipfileout.Dispose();
							}
						}

						zf.Dispose();
						break;
				}
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				realEntry = "";
			}

			return realEntry;
		}

		#endregion

		#region Information

		/// <summary>
		/// Generate a list of RomData objects from the header values in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <param name="date">True if entry dates should be included, false otherwise (default)</param>
		/// <returns>List of RomData objects representing the found data</returns>
		public static List<Rom> GetArchiveFileInfo(string input, bool date = false)
		{
			return GetExtendedArchiveFileInfo(input, Hash.SecureHashes, date: date);
		}

		/// <summary>
		/// Generate a list of empty folders in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <returns>List of empty folders in the archive</returns>
		public static List<string> GetEmptyFoldersInArchive(string input)
		{
			List<string> empties = new List<string>();
			string gamename = Path.GetFileNameWithoutExtension(input);

			// First, we check that there is anything being passed as the input
			if (String.IsNullOrEmpty(input))
			{
				return empties;
			}

			// Next, get the archive type
			ArchiveType? at = GetCurrentArchiveType(input);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return empties;
			}

			IReader reader = null;
			try
			{
				Globals.Logger.Verbose("Found archive of type: " + at);

				switch (at)
				{
					case ArchiveType.SevenZip:
						SevenZipArchive sza = SevenZipArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false });
						List<SevenZipArchiveEntry> sevenZipEntries = sza.Entries.OrderBy(e => e.Key, new NaturalSort.NaturalReversedComparer()).ToList();
						string lastSevenZipEntry = null;
						foreach (SevenZipArchiveEntry entry in sevenZipEntries)
						{
							if (entry != null)
							{
								// If the current is a superset of last, we skip it
								if (lastSevenZipEntry != null && lastSevenZipEntry.StartsWith(entry.Key))
								{
									// No-op
								}
								// If the entry is a directory, we add it
								else if (entry.IsDirectory)
								{
									empties.Add(entry.Key);
									lastSevenZipEntry = entry.Key;
								}
							}
						}
						break;

					case ArchiveType.GZip:
						// GZip files don't contain directories
						break;

					case ArchiveType.Rar:
						RarArchive ra = RarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false });
						List<RarArchiveEntry> rarEntries = ra.Entries.OrderBy(e => e.Key, new NaturalSort.NaturalReversedComparer()).ToList();
						string lastRarEntry = null;
						foreach (RarArchiveEntry entry in rarEntries)
						{
							if (entry != null)
							{
								// If the current is a superset of last, we skip it
								if (lastRarEntry != null && lastRarEntry.StartsWith(entry.Key))
								{
									// No-op
								}
								// If the entry is a directory, we add it
								else if (entry.IsDirectory)
								{
									empties.Add(entry.Key);
									lastRarEntry = entry.Key;
								}
							}
						}
						break;

					case ArchiveType.Tar:
						TarArchive ta = TarArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false });
						List<TarArchiveEntry> tarEntries = ta.Entries.OrderBy(e => e.Key, new NaturalSort.NaturalReversedComparer()).ToList();
						string lastTarEntry = null;
						foreach (TarArchiveEntry entry in tarEntries)
						{
							if (entry != null)
							{
								// If the current is a superset of last, we skip it
								if (lastTarEntry != null && lastTarEntry.StartsWith(entry.Key))
								{
									// No-op
								}
								// If the entry is a directory, we add it
								else if (entry.IsDirectory)
								{
									empties.Add(entry.Key);
									lastTarEntry = entry.Key;
								}
							}
						}
						break;

					case ArchiveType.Zip:
						SharpCompress.Archives.Zip.ZipArchive za = SharpCompress.Archives.Zip.ZipArchive.Open(input, new ReaderOptions { LeaveStreamOpen = false });
						List<SharpCompress.Archives.Zip.ZipArchiveEntry> zipEntries = za.Entries.OrderBy(e => e.Key, new NaturalSort.NaturalReversedComparer()).ToList();
						string lastZipEntry = null;
						foreach (SharpCompress.Archives.Zip.ZipArchiveEntry entry in zipEntries)
						{
							if (entry != null)
							{
								// If the current is a superset of last, we skip it
								if (lastZipEntry != null && lastZipEntry.StartsWith(entry.Key))
								{
									// No-op
								}
								// If the entry is a directory, we add it
								else
								{
									if (entry.IsDirectory)
									{
										empties.Add(entry.Key);
									}
									lastZipEntry = entry.Key;
								}
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
			}
			finally
			{
				reader?.Dispose();
			}

			return empties;
		}

		/// <summary>
		/// Generate a list of RomData objects from the header values in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <param name="omitFromScan">Hash representing the hashes that should be skipped</param>
		/// <param name="date">True if entry dates should be included, false otherwise (default)</param>
		/// <returns>List of RomData objects representing the found data</returns>
		/// <remarks>TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually</remarks>
		public static List<Rom> GetExtendedArchiveFileInfo(string input, Hash omitFromScan = Hash.DeepHashes, bool date = false)
		{
			List<Rom> found = new List<Rom>();
			string gamename = Path.GetFileNameWithoutExtension(input);

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return null;
			}

			// If we got back GZip, try to get TGZ info first
			else if (at == ArchiveType.GZip)
			{
				Rom possibleTgz = GetTorrentGZFileInfo(input);

				// If it was, then add it to the outputs and continue
				if (possibleTgz != null && possibleTgz.Name != null)
				{
					found.Add(possibleTgz);
					return found;
				}
			}

			try
			{
				Globals.Logger.Verbose("Found archive of type: " + at);

				switch (at)
				{
					// 7-zip
					case ArchiveType.SevenZip:
						SevenZipArchive sza = SevenZipArchive.Open(FileTools.TryOpenRead(input));
						foreach (SevenZipArchiveEntry entry in sza.Entries.Where(e => e != null && !e.IsDirectory))
						{
							// If secure hashes are disabled, do a quickscan
							if (omitFromScan == Hash.SecureHashes)
							{
								found.Add(new Rom
								{
									Type = ItemType.Rom,
									Name = entry.Key,
									Size = entry.Size,
									CRC = entry.Crc.ToString("X").ToLowerInvariant(),
									Date = (date && entry.LastModifiedTime != null ? entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss") : null),

									Machine = new Machine
									{
										Name = gamename,
									},
								});
							}
							// Otherwise, extract to a stream
							else
							{
								// Create and populate the entry stream
								MemoryStream entryStream = new MemoryStream();
								entry.WriteTo(entryStream);

								// Get and add the extended Rom information
								Rom sevenZipEntryRom = FileTools.GetStreamInfo(entryStream, entryStream.Length, omitFromScan: omitFromScan);
								sevenZipEntryRom.Name = entry.Key;
								sevenZipEntryRom.Machine = new Machine()
								{
									Name = gamename,
								};
								sevenZipEntryRom.Date = (date && entry.LastModifiedTime != null ? entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss") : null);
								found.Add(sevenZipEntryRom);
							}
						}

						// Dispose of the archive
						sza.Dispose();
						break;

					// GZip
					case ArchiveType.GZip:
						// If secure hashes are disabled, do a quickscan
						if (omitFromScan == Hash.SecureHashes)
						{
							Rom tempRom = new Rom(gamename, gamename, omitFromScan);
							BinaryReader br = new BinaryReader(FileTools.TryOpenRead(input));
							br.BaseStream.Seek(-8, SeekOrigin.End);
							byte[] headercrc = br.ReadBytes(4);
							tempRom.CRC = BitConverter.ToString(headercrc.Reverse().ToArray()).Replace("-", string.Empty).ToLowerInvariant();
							byte[] headersize = br.ReadBytes(4);
							tempRom.Size = BitConverter.ToInt32(headersize.Reverse().ToArray(), 0);
							br.Dispose();

							found.Add(tempRom);
						}
						// Otherwise, extract to a stream
						else
						{
							// Create and populate the entry stream
							MemoryStream outstream = new MemoryStream();
							GZipStream gzstream = new GZipStream(FileTools.TryOpenRead(input), Ionic.Zlib.CompressionMode.Decompress);
							gzstream.CopyTo(outstream);

							// Get and add the extended Rom information
							Rom gzipEntryRom = FileTools.GetStreamInfo(outstream, outstream.Length, omitFromScan: omitFromScan);
							gzipEntryRom.Name = gzstream.FileName;
							gzipEntryRom.Machine = new Machine()
							{
								Name = gamename,
							};
							gzipEntryRom.Date = (date && gzstream.LastModified != null ? gzstream.LastModified?.ToString("yyyy/MM/dd hh:mm:ss") : null);
							found.Add(gzipEntryRom);

							// Dispose of the archive
							gzstream.Dispose();
						}
						break;

					// RAR
					case ArchiveType.Rar:
						RarArchive ra = RarArchive.Open(FileTools.TryOpenRead(input));
						foreach (RarArchiveEntry entry in ra.Entries.Where(e => e != null && !e.IsDirectory))
						{
							// If secure hashes are disabled, do a quickscan
							if (omitFromScan == Hash.SecureHashes)
							{
								found.Add(new Rom
								{
									Type = ItemType.Rom,
									Name = entry.Key,
									Size = entry.Size,
									CRC = entry.Crc.ToString("X").ToLowerInvariant(),
									Date = (date && entry.LastModifiedTime != null ? entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss") : null),

									Machine = new Machine
									{
										Name = gamename,
									},
								});
							}
							// Otherwise, extract to a stream
							else
							{
								// Create and populate the entry stream
								MemoryStream entryStream = new MemoryStream();
								entry.WriteTo(entryStream);

								// Get and add the extended Rom information
								Rom sevenZipEntryRom = FileTools.GetStreamInfo(entryStream, entryStream.Length, omitFromScan: omitFromScan);
								sevenZipEntryRom.Name = entry.Key;
								sevenZipEntryRom.Machine = new Machine()
								{
									Name = gamename,
								};
								sevenZipEntryRom.Date = entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss");
								found.Add(sevenZipEntryRom);
							}
						}

						// Dispose of the archive
						ra.Dispose();
						break;

					// TAR
					case ArchiveType.Tar:
						TarArchive ta = TarArchive.Open(FileTools.TryOpenRead(input));
						foreach (TarArchiveEntry entry in ta.Entries.Where(e => e != null && !e.IsDirectory))
						{
							// If secure hashes are disabled, do a quickscan
							if (omitFromScan == Hash.SecureHashes)
							{
								found.Add(new Rom
								{
									Type = ItemType.Rom,
									Name = entry.Key,
									Size = entry.Size,
									CRC = entry.Crc.ToString("X").ToLowerInvariant(),
									Date = (date && entry.LastModifiedTime != null ? entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss") : null),

									Machine = new Machine
									{
										Name = gamename,
									},
								});
							}
							// Otherwise, extract to a stream
							else
							{
								// Create and populate the entry stream
								MemoryStream entryStream = new MemoryStream();
								entry.WriteTo(entryStream);

								// Get and add the extended Rom information
								Rom sevenZipEntryRom = FileTools.GetStreamInfo(entryStream, entryStream.Length, omitFromScan: omitFromScan);
								sevenZipEntryRom.Name = entry.Key;
								sevenZipEntryRom.Machine = new Machine()
								{
									Name = gamename,
								};
								sevenZipEntryRom.Date = entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss");
								found.Add(sevenZipEntryRom);
							}
						}

						// Dispose of the archive
						ta.Dispose();
						break;

					// Zip
					case ArchiveType.Zip:
						ZipFile zf = new ZipFile();
						ZipReturn zr = zf.Open(input, new FileInfo(input).LastWriteTime.Ticks, true);
						if (zr != ZipReturn.ZipGood)
						{
							throw new Exception(ZipFile.ZipErrorMessageText(zr));
						}

						for (int i = 0; i < zf.EntriesCount && zr == ZipReturn.ZipGood; i++)
						{
							// Open the read stream
							zr = zf.OpenReadStream(i, false, out Stream readStream, out ulong streamsize, out SabreTools.Helper.Data.CompressionMethod cm, out uint lastMod);

							// If the entry ends with a directory separator, continue to the next item, if any
							if (zf.Entries[i].FileName.EndsWith(Path.DirectorySeparatorChar.ToString())
								|| zf.Entries[i].FileName.EndsWith(Path.AltDirectorySeparatorChar.ToString())
								|| zf.Entries[i].FileName.EndsWith(Path.PathSeparator.ToString()))
							{
								continue;
							}

							// If secure hashes are disabled, do a quickscan
							if (omitFromScan == Hash.SecureHashes)
							{
								string newname = zf.Entries[i].FileName;
								long newsize = (long)zf.Entries[i].UncompressedSize;
								string newcrc = BitConverter.ToString(zf.Entries[i].CRC.Reverse().ToArray(), 0, zf.Entries[i].CRC.Length).Replace("-", string.Empty).ToLowerInvariant();
								string convertedDate = Style.ConvertMsDosTimeFormatToDateTime(zf.Entries[i].LastMod).ToString("yyyy/MM/dd hh:mm:ss");

								found.Add(new Rom
								{
									Type = ItemType.Rom,
									Name = newname,
									Size = newsize,
									CRC = newcrc,
									Date = (date ? convertedDate : null),

									Machine = new Machine
									{
										Name = gamename,
									},
								});
							}
							// Otherwise, extract to a stream
							else
							{
								MemoryStream entryStream = new MemoryStream();

								byte[] ibuffer = new byte[_bufferSize];
								int ilen;
								while ((ilen = readStream.Read(ibuffer, 0, _bufferSize)) > 0)
								{
									entryStream.Write(ibuffer, 0, ilen);
									entryStream.Flush();
								}
								zr = zf.CloseReadStream();

								// Get and add the extended Rom information
								Rom zipEntryRom = FileTools.GetStreamInfo(entryStream, entryStream.Length, omitFromScan: omitFromScan);
								zipEntryRom.Name = zf.Entries[i].FileName;
								zipEntryRom.Machine = new Machine()
								{
									Name = gamename,
								};
								string convertedDate = Style.ConvertMsDosTimeFormatToDateTime(zf.Entries[i].LastMod).ToString("yyyy/MM/dd hh:mm:ss");
								zipEntryRom.Date = (date ? convertedDate : null);
								found.Add(zipEntryRom);
							}
						}
						

						// Dispose of the archive
						zf.Close();
						break;
				}
			}
			catch (Exception)
			{
				// Don't log file open errors
				return null;
			}

			return found;
		}

		/// <summary>
		/// Retrieve file information for a single torrent GZ file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetTorrentGZFileInfo(string input)
		{
			// Check for the file existing first
			if (!File.Exists(input))
			{
				return null;
			}

			string datum = Path.GetFileName(input).ToLowerInvariant();
			long filesize = new FileInfo(input).Length;

			// If we have the romba depot files, just skip them gracefully
			if (datum == ".romba_size" || datum == ".romba_size.backup")
			{
				Globals.Logger.Verbose("Romba depot file found, skipping: " + input);
				return null;
			}

			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Constants.SHA1Length + @"}\.gz")) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
			{
				Globals.Logger.Warning("Non SHA-1 filename found, skipping: '" + Path.GetFullPath(input) + "'");
				return null;
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				Globals.Logger.Warning("Possibly corrupt file '" + Path.GetFullPath(input) + "' with size " + Style.GetBytesReadable(filesize));
				return null;
			}

			// Get the Romba-specific header data
			byte[] header; // Get preamble header for checking
			byte[] headermd5; // MD5
			byte[] headercrc; // CRC
			ulong headersz; // Int64 size
			BinaryReader br = new BinaryReader(FileTools.TryOpenRead(input));
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
				SHA1 = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(), // TODO: When updating to SHA-256, this needs to update to SHA256

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
		/// <returns>ArchiveType of inputted file (null on error)</returns>
		public static ArchiveType? GetCurrentArchiveType(string input)
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
				BinaryReader br = new BinaryReader(FileTools.TryOpenRead(input));
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
		/// <param name="shouldExternalProcess">Output parameter determining if file should be processed externally</param>
		/// <param name="shouldInternalProcess">Output parameter determining if file should be processed internally</param>
		public static void GetInternalExternalProcess(string input, ArchiveScanLevel archiveScanLevel,
			out bool shouldExternalProcess, out bool shouldInternalProcess)
		{
			shouldExternalProcess = true;
			shouldInternalProcess = true;

			ArchiveType? archiveType = GetCurrentArchiveType(input);
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
		/// (INCOMPLETE) Retrieve file information for a RAR file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		public static void GetRarFileInfo(string input)
		{
			if (!File.Exists(input))
			{
				return;
			}

			BinaryReader br = new BinaryReader(FileTools.TryOpenRead(input));

			// Check for the signature first (Skipping the SFX Module)
			byte[] signature = br.ReadBytes(8);
			int startpos = 0;
			while (startpos < Constants.MibiByte && BitConverter.ToString(signature, 0, 7) != Constants.RarSig && BitConverter.ToString(signature) != Constants.RarFiveSig)
			{
				startpos++;
				br.BaseStream.Position = startpos;
				signature = br.ReadBytes(8);
			}
			if (BitConverter.ToString(signature, 0, 7) != Constants.RarSig && BitConverter.ToString(signature) != Constants.RarFiveSig)
			{
				return;
			}

			CoreRarArchive cra = new CoreRarArchive();
			if (startpos > 0)
			{
				br.BaseStream.Position = 0;
				cra.SFX = br.ReadBytes(startpos);
			}

			// Get all archive header information
			cra.HeaderCRC32 = br.ReadUInt32();
			cra.HeaderSize = br.ReadUInt32();
			uint headerType = br.ReadUInt32();

			// Special encryption information
			bool hasEncryptionHeader = false;

			// If it's encrypted
			if (headerType == (uint)RarHeaderType.ArchiveEncryption)
			{
				hasEncryptionHeader = true;
				cra.EncryptionHeaderCRC32 = cra.HeaderCRC32;
				cra.EncryptionHeaderSize = cra.HeaderSize;
				cra.EncryptionHeaderFlags = (RarHeaderFlags)br.ReadUInt32();
				cra.EncryptionVersion = br.ReadUInt32();
				cra.EncryptionFlags = br.ReadUInt32();
				cra.KDFCount = br.ReadByte();
				cra.Salt = br.ReadBytes(16);
				cra.CheckValue = br.ReadBytes(12);

				cra.HeaderCRC32 = br.ReadUInt32();
				cra.HeaderSize = br.ReadUInt32();
				headerType = br.ReadUInt32();
			}

			cra.HeaderFlags = (RarHeaderFlags)br.ReadUInt32();
			if ((cra.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0)
			{
				cra.ExtraAreaSize = br.ReadUInt32();
			}
			cra.ArchiveFlags = (RarArchiveFlags)br.ReadUInt32();
			if ((cra.ArchiveFlags & RarArchiveFlags.VolumeNumberField) != 0)
			{
				cra.VolumeNumber = br.ReadUInt32();
			}
			if (((cra.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0) && cra.ExtraAreaSize != 0)
			{
				cra.ExtraArea = br.ReadBytes((int)cra.ExtraAreaSize);
			}

			// Archive Comment Service Header

			// Now for file headers
			for (;;)
			{
				CoreRarArchiveEntry crae = new CoreRarArchiveEntry();
				crae.HeaderCRC32 = br.ReadUInt32();
				crae.HeaderSize = br.ReadUInt32();
				crae.HeaderType = (RarHeaderType)br.ReadUInt32();

				if (crae.HeaderType == RarHeaderType.EndOfArchive)
				{
					break;
				}

				crae.HeaderFlags = (RarHeaderFlags)br.ReadUInt32();
				if ((crae.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0)
				{
					crae.ExtraAreaSize = br.ReadUInt32();
				}
				if ((crae.HeaderFlags & RarHeaderFlags.DataAreaPresent) != 0)
				{
					crae.DataAreaSize = br.ReadUInt32();
				}
				crae.FileFlags = (RarFileFlags)br.ReadUInt32();
				crae.UnpackedSize = br.ReadUInt32();
				if ((crae.FileFlags & RarFileFlags.UnpackedSizeUnknown) != 0)
				{
					crae.UnpackedSize = 0;
				}
				crae.Attributes = br.ReadUInt32();
				crae.mtime = br.ReadUInt32();
				crae.DataCRC32 = br.ReadUInt32();
				crae.CompressionInformation = br.ReadUInt32();
				crae.HostOS = br.ReadUInt32();
				crae.NameLength = br.ReadUInt32();
				crae.Name = br.ReadBytes((int)crae.NameLength);
				if ((crae.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0)
				{
					uint extraSize = br.ReadUInt32();
					switch (br.ReadUInt32()) // Extra Area Type
					{
						case 0x01: // File encryption information
							crae.EncryptionSize = extraSize;
							crae.EncryptionFlags = (RarEncryptionFlags)br.ReadUInt32();
							crae.KDFCount = br.ReadByte();
							crae.Salt = br.ReadBytes(16);
							crae.IV = br.ReadBytes(16);
							crae.CheckValue = br.ReadBytes(12);
							break;

						case 0x02: // File data hash
							crae.HashSize = extraSize;
							crae.HashType = br.ReadUInt32();
							crae.HashData = br.ReadBytes(32);
							break;

						case 0x03: // High precision file time
							crae.TimeSize = extraSize;
							crae.TimeFlags = (RarTimeFlags)br.ReadUInt32();
							if ((crae.TimeFlags & RarTimeFlags.TimeInUnixFormat) != 0)
							{
								if ((crae.TimeFlags & RarTimeFlags.ModificationTimePresent) != 0)
								{
									crae.TimeMtime64 = br.ReadUInt64();
								}
								if ((crae.TimeFlags & RarTimeFlags.CreationTimePresent) != 0)
								{
									crae.TimeCtime64 = br.ReadUInt64();
								}
								if ((crae.TimeFlags & RarTimeFlags.LastAccessTimePresent) != 0)
								{
									crae.TimeLtime64 = br.ReadUInt64();
								}
							}
							else
							{
								if ((crae.TimeFlags & RarTimeFlags.ModificationTimePresent) != 0)
								{
									crae.TimeMtime = br.ReadUInt32();
								}
								if ((crae.TimeFlags & RarTimeFlags.CreationTimePresent) != 0)
								{
									crae.TimeCtime = br.ReadUInt32();
								}
								if ((crae.TimeFlags & RarTimeFlags.LastAccessTimePresent) != 0)
								{
									crae.TimeLtime = br.ReadUInt32();
								}
							}
							break;

						case 0x04: // File version number
							crae.VersionSize = extraSize;
							/* crae.VersionFlags = */ br.ReadUInt32();
							crae.VersionNumber = br.ReadUInt32();
							break;

						case 0x05: // File system redirection
							crae.RedirectionSize = extraSize;
							crae.RedirectionType = (RarRedirectionType)br.ReadUInt32();
							crae.RedirectionFlags = br.ReadUInt32();
							crae.RedirectionNameLength = br.ReadUInt32();
							crae.RedirectionName = br.ReadBytes((int)crae.RedirectionNameLength);
							break;

						case 0x06: // Unix owner and group information
							crae.UnixOwnerSize = extraSize;
							crae.UnixOwnerFlags = (RarUnixOwnerRecordFlags)br.ReadUInt32();
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.UserNameStringIsPresent) != 0)
							{
								crae.UnixOwnerUserNameLength = br.ReadUInt32();
								crae.UnixOwnerUserName = br.ReadBytes((int)crae.UnixOwnerUserNameLength);
							}
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.GroupNameStringIsPresent) != 0)
							{
								crae.UnixOwnerGroupNameLength = br.ReadUInt32();
								crae.UnixOwnerGroupName = br.ReadBytes((int)crae.UnixOwnerGroupNameLength);
							}
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.NumericUserIdIsPresent) != 0)
							{
								crae.UnixOwnerUserId = br.ReadUInt32();
							}
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.NumericGroupIdIsPresent) != 0)
							{
								crae.UnixOwnerGroupId = br.ReadUInt32();
							}
							break;

						case 0x07: // Service header data array

							break;
					}
				}
				if ((crae.HeaderFlags & RarHeaderFlags.DataAreaPresent) != 0)
				{
					crae.DataArea = br.ReadBytes((int)crae.DataAreaSize);
				}
			}
		}

		/// <summary>
		/// (INCOMPLETE) Get the T7Z status of the file
		/// </summary>
		/// <param name="filename">Name of the file to check</param>
		/// <returns>0 if the file isn't 7z, 1 if the file is t7z, 2 if the file is 7z</returns>
		public static int IsT7z(string filename)
		{
			int ist7z = 0;

			if (File.Exists(filename))
			{
				try
				{
					Stream fread = FileTools.TryOpenRead(filename);
					uint ar, offs = 0;
					fread.Seek(0, SeekOrigin.Begin);
					byte[] buffer = new byte[128];
					ar = (uint)fread.Read(buffer, 0, 4 + Constants.Torrent7ZipSignature.Length + 4);
					if (ar < (4 + Constants.Torrent7ZipSignature.Length + 4))
					{
						if (ar >= Constants.Torrent7ZipSignature.Length + 4)
						{
							ar -= (uint)(Constants.Torrent7ZipSignature.Length + 4);
						}
						if (ar <= Constants.Torrent7ZipHeader.Length)
						{
							ar = (uint)Constants.Torrent7ZipHeader.Length;
						}
						// memset(buffer+offs+ar,0,crcsz-ar)
					}

					fread.Dispose();
				}
				catch
				{
					Globals.Logger.Warning("File '" + filename + "' could not be opened");
					ist7z = 0;
				}
			}

			return ist7z;
		}

		#endregion

		#region Writing

		/// <summary>
		/// Write an input file to a tape archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTAR(string inputFile, string outDir, Rom rom, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>() { inputFile };
			List<Rom> roms = new List<Rom>() { rom };

			return WriteTAR(inputFiles, outDir, roms, date: date);
		}

		/// <summary>
		/// Write a set of input files to a tape archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTAR(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false)
		{
			bool success = false;
			string tempFile = Path.Combine(Path.GetTempPath(), "tmp" + Guid.NewGuid().ToString());

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
			string archiveFileName = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(roms[0].Machine.Name) + (roms[0].Machine.Name.EndsWith(".tar") ? "" : ".tar"));

			// Set internal variables
			TarArchive oldTarFile = TarArchive.Create();
			TarArchive tarFile = TarArchive.Create();

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

						// Copy the input stream to the output
						tarFile.AddEntry(roms[index].Name, inputFiles[index]);
					}
				}

				// Otherwise, sort the input files and write out in the correct order
				else
				{
					// Open the old archive for reading
					oldTarFile = TarArchive.Open(archiveFileName);

					// Get a list of all current entries
					List<string> entries = oldTarFile.Entries.Select(i => i.Key).ToList();

					// Map all inputs to index
					Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
					for (int i = 0; i < inputFiles.Count; i++)
					{
						// If the old one contains the new file, then just skip out
						if (entries.Contains(roms[i].Name.Replace('\\', '/')))
						{
							continue;
						}

						inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), -(i + 1));
					}

					// Then add all of the old entries to it too
					for (int i = 0; i < entries.Count; i++)
					{
						inputIndexMap.Add(entries[i], i);
					}

					// If the number of entries is the same as the old archive, skip out
					if (inputIndexMap.Keys.Count <= entries.Count)
					{
						success = true;
						return success;
					}

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
							// Copy the input file to the output
							tarFile.AddEntry(roms[-index - 1].Name, inputFiles[-index - 1]);
						}

						// Otherwise, copy the file from the old archive
						else
						{
							// Get the stream from the original archive
							string tempEntry = Path.Combine(Path.GetTempPath(), "tmp" + Guid.NewGuid().ToString());
							oldTarFile.Entries.Where(e => e.Key == key).ToList()[0].WriteToFile(tempEntry);

							// Copy the input stream to the output
							tarFile.AddEntry(key, tempEntry);
						}
					}
				}

				// Close the output tar file
				tarFile.SaveTo(tempFile, new WriterOptions(CompressionType.None));

				success = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				success = false;
			}
			finally
			{
				tarFile.Dispose();
				oldTarFile.Dispose();
			}

			// If the old file exists, delete it and replace
			if (File.Exists(archiveFileName))
			{
				FileTools.TryDeleteFile(archiveFileName);
			}
			File.Move(tempFile, archiveFileName);

			return true;
		}

		/// <summary>
		/// Write an input file to a torrent7z archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrent7Zip(string inputFile, string outDir, Rom rom, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>() { inputFile };
			List<Rom> roms = new List<Rom>() { rom };

			return WriteTorrent7Zip(inputFiles, outDir, roms, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrent7z archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrent7Zip(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false)
		{
			bool success = false;
			string tempFile = Path.Combine(outDir, "tmp" + Guid.NewGuid().ToString());

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
			string archiveFileName = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(roms[0].Machine.Name) + (roms[0].Machine.Name.EndsWith(".7z") ? "" : ".7z"));

			// Set internal variables
			SevenZipBase.SetLibraryPath("7za.dll");
			SevenZipExtractor oldZipFile;
			SevenZipCompressor zipFile;

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
					zipFile = new SevenZipCompressor()
					{
						ArchiveFormat = OutArchiveFormat.SevenZip,
						CompressionLevel = SevenZip.CompressionLevel.Normal,
					};

					// Map all inputs to index
					Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
					for (int i = 0; i < inputFiles.Count; i++)
					{
						inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), i);
					}

					// Sort the keys in TZIP order
					List<string> keys = inputIndexMap.Keys.ToList();
					keys.Sort(ZipFile.TorrentZipStringCompare);

					// Create the temp directory
					string tempPath = Path.Combine(Path.GetTempPath(), new Guid().ToString());
					if (!Directory.Exists(tempPath))
					{
						Directory.CreateDirectory(tempPath);
					}

					// Now add all of the files in order
					foreach (string key in keys)
					{
						string newkey = Path.Combine(tempPath, key);

						File.Move(inputFiles[inputIndexMap[key]], newkey);
						zipFile.CompressFiles(tempFile, newkey);
						File.Move(newkey, inputFiles[inputIndexMap[key]]);
					}

					FileTools.CleanDirectory(tempPath);
					FileTools.TryDeleteDirectory(tempPath);
				}

				// Otherwise, sort the input files and write out in the correct order
				else
				{
					// Open the old archive for reading
					Stream oldZipFileStream = FileTools.TryOpenRead(archiveFileName);
					oldZipFile = new SevenZipExtractor(oldZipFileStream);

					// Map all inputs to index
					Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
					for (int i = 0; i < inputFiles.Count; i++)
					{
						// If the old one contains the new file, then just skip out
						if (oldZipFile.ArchiveFileNames.Contains(roms[i].Name.Replace('\\', '/')))
						{
							continue;
						}

						inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), -(i + 1));
					}

					// Then add all of the old entries to it too
					for (int i = 0; i < oldZipFile.FilesCount; i++)
					{
						inputIndexMap.Add(oldZipFile.ArchiveFileNames[i], i);
					}

					// If the number of entries is the same as the old archive, skip out
					if (inputIndexMap.Keys.Count <= oldZipFile.FilesCount)
					{
						success = true;
						return success;
					}

					// Otherwise, process the old zipfile
					zipFile = new SevenZipCompressor()
					{
						ArchiveFormat = OutArchiveFormat.SevenZip,
						CompressionLevel = SevenZip.CompressionLevel.Normal,
					};
					Stream zipFileStream = FileTools.TryOpenWrite(tempFile);

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
							zipFile.CompressFiles(zipFileStream, inputFiles[-index - 1]);
						}

						// Otherwise, copy the file from the old archive
						else
						{
							Stream oldZipFileEntryStream = FileTools.TryOpenReadWrite(inputFiles[index]);
							oldZipFile.ExtractFile(index, oldZipFileEntryStream);
							zipFile.CompressFiles(zipFileStream, inputFiles[index]);

							oldZipFileEntryStream.Dispose();
							FileTools.TryDeleteFile(inputFiles[index]);
						}
					}

					zipFileStream.Dispose();
					oldZipFile.Dispose();
				}

				success = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				success = false;
			}

			// If the old file exists, delete it and replace
			if (File.Exists(archiveFileName))
			{
				FileTools.TryDeleteFile(archiveFileName);
			}
			File.Move(tempFile, archiveFileName);

			// Now make the file T7Z
			// TODO: Add ACTUAL T7Z compatible code

			BinaryWriter bw = new BinaryWriter(FileTools.TryOpenReadWrite(archiveFileName));
			bw.Seek(0, SeekOrigin.Begin);
			bw.Write(Constants.Torrent7ZipHeader);
			bw.Seek(0, SeekOrigin.End);

			oldZipFile = new SevenZipExtractor(FileTools.TryOpenReadWrite(archiveFileName));

			// Get the correct signature to use (Default 0, Unicode 1, SingleFile 2, StripFileNames 4)
			byte[] tempsig = Constants.Torrent7ZipSignature;
			if (oldZipFile.FilesCount > 1)
			{
				tempsig[16] = 0x2;
			}
			else
			{
				tempsig[16] = 0;
			}

			bw.Write(tempsig);
			bw.Dispose();

			return true;
		}

		/// <summary>
		/// Write an input file to a torrent GZ file
		/// </summary>
		/// <param name="input">File to write from</param>
		/// <param name="outDir">Directory to write archive to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the write was a success, false otherwise</returns>
		/// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
		public static bool WriteTorrentGZ(string input, string outDir, bool romba)
		{
			// Check that the input file exists
			if (!File.Exists(input))
			{
				Globals.Logger.Warning("File " + input + " does not exist!");
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
			Rom rom = FileTools.GetFileInfo(input);

			// Get the output file name
			string outfile = null;

			// If we have a romba output, add the romba path
			if (romba)
			{
				outfile = Path.Combine(outDir, Style.GetRombaPath(rom.SHA1)); // TODO: When updating to SHA-256, this needs to update to SHA256

				// Check to see if the folder needs to be created
				if (!Directory.Exists(Path.GetDirectoryName(outfile)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(outfile));
				}
			}
			// Otherwise, we're just rebuilding to the main directory
			else
			{
				outfile = Path.Combine(outDir, rom.SHA1 + ".gz"); // TODO: When updating to SHA-256, this needs to update to SHA256
			}

			// If the output file exists, don't try to write again
			if (!File.Exists(outfile))
			{
				// Compress the input stream
				FileStream inputStream = FileTools.TryOpenRead(input);
				FileStream outputStream = FileTools.TryCreate(outfile);

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
				DeflateStream ds = new DeflateStream(outputStream, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestCompression, true);

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

			return true;
		}

		/// <summary>
		/// Write an input file to a torrentlrzip archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentLRZ(string inputFile, string outDir, Rom rom, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>() { inputFile };
			List<Rom> roms = new List<Rom>() { rom };

			return WriteTorrentLRZ(inputFiles, outDir, roms, date: date);
		}

		/// <summary>
		/// (UNIMPLEMENTED) Write a set of input files to a torrentlrzip archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentLRZ(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false)
		{
			return false;
		}

		/// <summary>
		/// Write an input file to a torrentrar archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentRAR(string inputFile, string outDir, Rom rom, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>() { inputFile };
			List<Rom> roms = new List<Rom>() { rom };

			return WriteTorrentRAR(inputFiles, outDir, roms, date: date);
		}

		/// <summary>
		/// (UNIMPLEMENTED) Write a set of input files to a torrentrar archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentRAR(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false)
		{
			return false;
		}

		/// <summary>
		/// Write an input file to a torrentxz archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentXZ(string inputFile, string outDir, Rom rom, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>() { inputFile };
			List<Rom> roms = new List<Rom>() { rom };

			return WriteTorrentXZ(inputFiles, outDir, roms, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrentxz archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentXZ(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false)
		{
			bool success = false;
			string tempFile = Path.Combine(outDir, "tmp" + Guid.NewGuid().ToString());

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
			string archiveFileName = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(roms[0].Machine.Name) + (roms[0].Machine.Name.EndsWith(".7z") ? "" : ".7z"));

			// Set internal variables
			SevenZipBase.SetLibraryPath("7za.dll");
			SevenZipExtractor oldZipFile;
			SevenZipCompressor zipFile;

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
					zipFile = new SevenZipCompressor()
					{
						ArchiveFormat = OutArchiveFormat.XZ,
						CompressionLevel = SevenZip.CompressionLevel.Normal,
					};

					// Map all inputs to index
					Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
					for (int i = 0; i < inputFiles.Count; i++)
					{
						inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), i);
					}

					// Sort the keys in TZIP order
					List<string> keys = inputIndexMap.Keys.ToList();
					keys.Sort(ZipFile.TorrentZipStringCompare);

					// Create the temp directory
					string tempPath = Path.Combine(Path.GetTempPath(), new Guid().ToString());
					if (!Directory.Exists(tempPath))
					{
						Directory.CreateDirectory(tempPath);
					}

					// Now add all of the files in order
					foreach (string key in keys)
					{
						string newkey = Path.Combine(tempPath, key);

						File.Move(inputFiles[inputIndexMap[key]], newkey);
						zipFile.CompressFiles(tempFile, newkey);
						File.Move(newkey, inputFiles[inputIndexMap[key]]);
					}

					FileTools.CleanDirectory(tempPath);
					FileTools.TryDeleteDirectory(tempPath);
				}

				// Otherwise, sort the input files and write out in the correct order
				else
				{
					// Open the old archive for reading
					Stream oldZipFileStream = FileTools.TryOpenRead(archiveFileName);
					oldZipFile = new SevenZipExtractor(oldZipFileStream);

					// Map all inputs to index
					Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
					for (int i = 0; i < inputFiles.Count; i++)
					{
						// If the old one contains the new file, then just skip out
						if (oldZipFile.ArchiveFileNames.Contains(roms[i].Name.Replace('\\', '/')))
						{
							continue;
						}

						inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), -(i + 1));
					}

					// Then add all of the old entries to it too
					for (int i = 0; i < oldZipFile.FilesCount; i++)
					{
						inputIndexMap.Add(oldZipFile.ArchiveFileNames[i], i);
					}

					// If the number of entries is the same as the old archive, skip out
					if (inputIndexMap.Keys.Count <= oldZipFile.FilesCount)
					{
						success = true;
						return success;
					}

					// Otherwise, process the old zipfile
					zipFile = new SevenZipCompressor()
					{
						ArchiveFormat = OutArchiveFormat.XZ,
						CompressionLevel = SevenZip.CompressionLevel.Normal,
					};
					Stream zipFileStream = FileTools.TryOpenWrite(tempFile);

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
							zipFile.CompressFiles(zipFileStream, inputFiles[-index - 1]);
						}

						// Otherwise, copy the file from the old archive
						else
						{
							Stream oldZipFileEntryStream = FileTools.TryCreate(inputFiles[index]);
							oldZipFile.ExtractFile(index, oldZipFileEntryStream);
							zipFile.CompressFiles(zipFileStream, inputFiles[index]);

							oldZipFileEntryStream.Dispose();
							FileTools.TryDeleteFile(inputFiles[index]);
						}
					}

					zipFileStream.Dispose();
					oldZipFile.Dispose();
				}

				success = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				success = false;
			}

			// If the old file exists, delete it and replace
			if (File.Exists(archiveFileName))
			{
				FileTools.TryDeleteFile(archiveFileName);
			}
			File.Move(tempFile, archiveFileName);

			// Now make the file TXZ
			// TODO: Add ACTUAL TXZ compatible code (based on T7z)

			BinaryWriter bw = new BinaryWriter(FileTools.TryOpenReadWrite(archiveFileName));
			bw.Seek(0, SeekOrigin.Begin);
			bw.Write(Constants.Torrent7ZipHeader);
			bw.Seek(0, SeekOrigin.End);

			oldZipFile = new SevenZipExtractor(FileTools.TryOpenReadWrite(archiveFileName));

			// Get the correct signature to use (Default 0, Unicode 1, SingleFile 2, StripFileNames 4)
			byte[] tempsig = Constants.Torrent7ZipSignature;
			if (oldZipFile.FilesCount > 1)
			{
				tempsig[16] = 0x2;
			}
			else
			{
				tempsig[16] = 0;
			}

			bw.Write(tempsig);
			bw.Dispose();

			return true;
		}

		/// <summary>
		/// Write an input file to a torrentzip archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentZip(string inputFile, string outDir, Rom rom, bool date = false)
		{
			// Wrap the individual inputs into lists
			List<string> inputFiles = new List<string>() { inputFile };
			List<Rom> roms = new List<Rom>() { rom };

			return WriteTorrentZip(inputFiles, outDir, roms, date: date);
		}

		/// <summary>
		/// Write a set of input files to a torrentzip archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFile">Input filenames to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">List of Rom representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public static bool WriteTorrentZip(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false)
		{
			bool success = false;
			string tempFile = Path.Combine(outDir, "tmp" + Guid.NewGuid().ToString());

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
			string archiveFileName = Path.Combine(outDir, Style.RemovePathUnsafeCharacters(roms[0].Machine.Name) + (roms[0].Machine.Name.EndsWith(".zip") ? "" : ".zip"));

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
						Stream freadStream = FileTools.TryOpenRead(inputFiles[index]);
						ulong istreamSize = (ulong)(new FileInfo(inputFiles[index]).Length);

						DateTime dt = DateTime.Now;
						if (date && !String.IsNullOrEmpty(roms[index].Date) && DateTime.TryParse(roms[index].Date.Replace('\\', '/'), out dt))
						{
							uint msDosDateTime = Style.ConvertDateTimeToMsDosTimeFormat(dt);
							zipFile.OpenWriteStream(false, false, roms[index].Name.Replace('\\', '/'), istreamSize,
								SabreTools.Helper.Data.CompressionMethod.Deflated, out writeStream, lastMod: msDosDateTime);
						}
						else
						{
							zipFile.OpenWriteStream(false, true, roms[index].Name.Replace('\\', '/'), istreamSize, SabreTools.Helper.Data.CompressionMethod.Deflated, out writeStream);
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
							Stream freadStream = FileTools.TryOpenRead(inputFiles[-index - 1]);
							ulong istreamSize = (ulong)(new FileInfo(inputFiles[-index - 1]).Length);

							DateTime dt = DateTime.Now;
							if (date && !String.IsNullOrEmpty(roms[-index - 1].Date) && DateTime.TryParse(roms[-index - 1].Date.Replace('\\', '/'), out dt))
							{
								uint msDosDateTime = Style.ConvertDateTimeToMsDosTimeFormat(dt);
								zipFile.OpenWriteStream(false, false, roms[-index - 1].Name.Replace('\\', '/'), istreamSize,
									SabreTools.Helper.Data.CompressionMethod.Deflated, out writeStream, lastMod: msDosDateTime);
							}
							else
							{
								zipFile.OpenWriteStream(false, true, roms[-index - 1].Name.Replace('\\', '/'), istreamSize, SabreTools.Helper.Data.CompressionMethod.Deflated, out writeStream);
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
							oldZipFile.OpenReadStream(index, false, out Stream zreadStream, out ulong istreamSize, out SabreTools.Helper.Data.CompressionMethod icompressionMethod, out uint lastMod);
							zipFile.OpenWriteStream(false, lastMod == Constants.TorrentZipFileDateTime, oldZipFile.Filename(index),
								istreamSize, SabreTools.Helper.Data.CompressionMethod.Deflated, out writeStream, lastMod: lastMod);

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
				FileTools.TryDeleteFile(archiveFileName);
			}
			File.Move(tempFile, archiveFileName);

			return true;
		}

		#endregion
	}
}
