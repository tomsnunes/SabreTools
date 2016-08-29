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
		/// <summary>
		/// Copy a file to an output archive
		/// </summary>
		/// <param name="input">Input filename to be moved</param>
		/// <param name="output">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		public static void WriteToArchive(string input, string output, File rom)
		{
			string archiveFileName = Path.Combine(output, rom.Machine + ".zip");

			System.IO.Compression.ZipArchive outarchive = null;
			try
			{
				if (!System.IO.File.Exists(archiveFileName))
				{
					outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
				}
				else
				{
					outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update);
				}

				if (System.IO.File.Exists(input))
				{
					if (outarchive.Mode == ZipArchiveMode.Create || outarchive.GetEntry(rom.Name) == null)
					{
						outarchive.CreateEntryFromFile(input, rom.Name, CompressionLevel.Optimal);
					}
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						if (outarchive.Mode == ZipArchiveMode.Create || outarchive.GetEntry(file) == null)
						{
							outarchive.CreateEntryFromFile(file, file, CompressionLevel.Optimal);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				outarchive?.Dispose();
			}
		}

		/// <summary>
		/// Copy a file to an output archive using SharpCompress
		/// </summary>
		/// <param name="input">Input filename to be moved</param>
		/// <param name="output">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		public static void WriteToManagedArchive(string input, string output, File rom)
		{
			string archiveFileName = Path.Combine(output, rom.Machine + ".zip");

			// Delete an empty file first
			if (System.IO.File.Exists(archiveFileName) && new FileInfo(archiveFileName).Length == 0)
			{
				System.IO.File.Delete(archiveFileName);
			}

			// Get if the file should be written out
			bool newfile = System.IO.File.Exists(archiveFileName) && new FileInfo(archiveFileName).Length != 0;

			using (SharpCompress.Archive.Zip.ZipArchive archive = (newfile
				? ArchiveFactory.Open(archiveFileName, Options.LookForHeader) as SharpCompress.Archive.Zip.ZipArchive
				: ArchiveFactory.Create(ArchiveType.Zip) as SharpCompress.Archive.Zip.ZipArchive))
			{
				try
				{
					if (System.IO.File.Exists(input))
					{
						archive.AddEntry(rom.Name, input);
					}
					else if (Directory.Exists(input))
					{
						archive.AddAllFromDirectory(input, "*", SearchOption.AllDirectories);
					}

					archive.SaveTo(archiveFileName + ".tmp", CompressionType.Deflate);
				}
				catch (Exception)
				{
					// Don't log archive write errors
				}
			}

			if (System.IO.File.Exists(archiveFileName + ".tmp"))
			{
				System.IO.File.Delete(archiveFileName);
				System.IO.File.Move(archiveFileName + ".tmp", archiveFileName);
			}
		}

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
					sza = SevenZipArchive.Open(System.IO.File.OpenRead(input));
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

					using (FileStream itemstream = System.IO.File.OpenRead(input))
					{
						using (FileStream outstream = System.IO.File.Create(Path.Combine(tempdir, Path.GetFileNameWithoutExtension(input))))
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
					reader = ReaderFactory.Open(System.IO.File.OpenRead(input));
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
				reader = ReaderFactory.Open(System.IO.File.OpenRead(input));

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

					using(FileStream itemstream = System.IO.File.OpenRead(input))
					{
						using (FileStream outstream = System.IO.File.Create(Path.Combine(tempdir, Path.GetFileNameWithoutExtension(input))))
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

		/// <summary>
		/// Attempt to copy a file between archives
		/// </summary>
		/// <param name="inputArchive">Source archive name</param>
		/// <param name="outputArchive">Destination archive name</param>
		/// <param name="sourceEntryName">Input entry name</param>
		/// <param name="destEntryName">Output entry name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the copy was a success, false otherwise</returns>
		public static bool CopyFileBetweenArchives(string inputArchive, string outputArchive,
			string sourceEntryName, string destEntryName, Logger logger)
		{
			bool success = false;

			// First get the archive types
			ArchiveType? iat = GetCurrentArchiveType(inputArchive, logger);
			ArchiveType? oat = (System.IO.File.Exists(outputArchive) ? GetCurrentArchiveType(outputArchive, logger) : ArchiveType.Zip);

			// If we got back null (or the output is not a Zipfile), then it's not an archive, so we we return
			if (iat == null || (oat == null || oat != ArchiveType.Zip) || inputArchive == outputArchive)
			{
				return success;
			}

			IReader reader = null;
			System.IO.Compression.ZipArchive outarchive = null;
			try
			{
				reader = ReaderFactory.Open(System.IO.File.OpenRead(inputArchive));

				if (iat == ArchiveType.Zip || iat == ArchiveType.SevenZip || iat == ArchiveType.Rar)
				{
					while (reader.MoveToNextEntry())
					{
						logger.Log("Current entry name: '" + reader.Entry.Key + "'");
						if (reader.Entry != null && reader.Entry.Key.Contains(sourceEntryName))
						{
							if (!System.IO.File.Exists(outputArchive))
							{
								outarchive = ZipFile.Open(outputArchive, ZipArchiveMode.Create);
							}
							else
							{
								outarchive = ZipFile.Open(outputArchive, ZipArchiveMode.Update);
							}

							if (outarchive.Mode == ZipArchiveMode.Create || outarchive.GetEntry(destEntryName) == null)
							{
								System.IO.Compression.ZipArchiveEntry iae = outarchive.CreateEntry(destEntryName, CompressionLevel.Optimal) as System.IO.Compression.ZipArchiveEntry;

								using (Stream iaestream = iae.Open())
								{
									reader.WriteEntryTo(iaestream);
								}
							}
							success = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				success = false;
			}
			finally
			{
				reader?.Dispose();
				outarchive?.Dispose();
			}

			return success;
		}

		/// <summary>
		/// Attempt to copy a file between archives using SharpCompress
		/// </summary>
		/// <param name="inputArchive">Source archive name</param>
		/// <param name="outputArchive">Destination archive name</param>
		/// <param name="sourceEntryName">Input entry name</param>
		/// <param name="destEntryName">Output entry name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the copy was a success, false otherwise</returns>
		public static bool CopyFileBetweenManagedArchives(string inputArchive, string outputArchive,
			string sourceEntryName, string destEntryName, Logger logger)
		{
			bool success = false;

			// First get the archive types
			ArchiveType? iat = GetCurrentArchiveType(inputArchive, logger);
			ArchiveType? oat = (System.IO.File.Exists(outputArchive) ? GetCurrentArchiveType(outputArchive, logger) : ArchiveType.Zip);

			// If we got back null (or the output is not a Zipfile), then it's not an archive, so we we return
			if (iat == null || (oat == null || oat != ArchiveType.Zip) || inputArchive == outputArchive)
			{
				return success;
			}

			try
			{
				using (IReader reader = ReaderFactory.Open(System.IO.File.OpenRead(inputArchive)))
				{
					if (iat == ArchiveType.Zip || iat == ArchiveType.SevenZip || iat == ArchiveType.Rar)
					{
						while (reader.MoveToNextEntry())
						{
							logger.Log("Current entry name: '" + reader.Entry.Key + "'");
							if (reader.Entry != null && reader.Entry.Key.Contains(sourceEntryName))
							{
								// Get if the file should be written out
								bool newfile = System.IO.File.Exists(outputArchive) && new FileInfo(outputArchive).Length != 0;

								using (SharpCompress.Archive.Zip.ZipArchive archive = (newfile
									? ArchiveFactory.Open(outputArchive, Options.LookForHeader) as SharpCompress.Archive.Zip.ZipArchive
									: ArchiveFactory.Create(ArchiveType.Zip) as SharpCompress.Archive.Zip.ZipArchive))
								{
									try
									{
										Stream tempstream = new MemoryStream();
										reader.WriteEntryTo(tempstream);
										archive.AddEntry(destEntryName, tempstream);

										archive.SaveTo(outputArchive + ".tmp", CompressionType.Deflate);
									}
									catch (Exception)
									{
										// Don't log archive write errors
									}
								}

								if (System.IO.File.Exists(outputArchive + ".tmp"))
								{
									System.IO.File.Delete(outputArchive);
									System.IO.File.Move(outputArchive + ".tmp", outputArchive);
								}

								success = true;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				success = false;
			}

			return success;
		}

		/// <summary>
		/// Generate a list of RomData objects from the header values in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>List of RomData objects representing the found data</returns>
		public static List<File> GetArchiveFileInfo(string input, Logger logger)
		{
			List<File> roms = new List<File>();
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
				File possibleTgz = GetTorrentGZFileInfo(input, logger);

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
					using (BinaryReader br = new BinaryReader(System.IO.File.OpenRead(input)))
					{
						br.BaseStream.Seek(-8, SeekOrigin.End);
						byte[] headercrc = br.ReadBytes(4);
						crc = BitConverter.ToString(headercrc.Reverse().ToArray()).Replace("-", string.Empty).ToLowerInvariant();
						byte[] headersize = br.ReadBytes(4);
						size = BitConverter.ToInt32(headersize.Reverse().ToArray(), 0);
					}
				}

				reader = ReaderFactory.Open(System.IO.File.OpenRead(input));

				if (at != ArchiveType.Tar)
				{
					while (reader.MoveToNextEntry())
					{
						if (reader.Entry != null && !reader.Entry.IsDirectory)
						{
							logger.Log("Entry found: '" + reader.Entry.Key + "': "
								+ (size == 0 ? reader.Entry.Size : size) + ", "
								+ (crc == "" ? reader.Entry.Crc.ToString("X").ToLowerInvariant() : crc));

							roms.Add(new File
							{
								Type = ItemType.Rom,
								Name = reader.Entry.Key,
								Machine = new Machine
								{
									Name = gamename,
								},
								HashData = new HashData
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
		public static File GetTorrentGZFileInfo(string input, Logger logger)
		{
			string datum = Path.GetFileName(input).ToLowerInvariant();
			long filesize = new FileInfo(input).Length;
 
			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{40}\.gz"))
			{
				logger.Warning("Non SHA-1 filename found, skipping: '" + datum + "'");
				return new File();
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				logger.Warning("Possibly corrupt file '" + input + "' with size " + Style.GetBytesReadable(filesize));
				return new File();
			}

			// Get the Romba-specific header data
			byte[] header; // Get preamble header for checking
			byte[] headermd5; // MD5
			byte[] headercrc; // CRC
			byte[] headersz; // Int64 size
			using (BinaryReader br = new BinaryReader(System.IO.File.OpenRead(input)))
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
				correct &= (header[i] == Constants.TorrentGZHeader[i]);
			}
			if (!correct)
			{
				return new File();
			}

			// Now convert the data and get the right position
			string gzmd5 = BitConverter.ToString(headermd5).Replace("-", string.Empty);
			string gzcrc = BitConverter.ToString(headercrc).Replace("-", string.Empty);
			long extractedsize = (long)BitConverter.ToUInt64(headersz.Reverse().ToArray(), 0);

			File rom = new File
			{
				Type = ItemType.Rom,
				Machine = new Machine
				{
					Name = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				},
				Name = Path.GetFileNameWithoutExtension(input).ToLowerInvariant(),
				HashData = new HashData
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
			if (!System.IO.File.Exists(input))
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
			File rom = RomTools.GetSingleFileInfo(input);

			// If it doesn't exist, create the output file and then write
			string outfile = Path.Combine(outdir, rom.HashData.SHA1 + ".gz");
			using (FileStream inputstream = new FileStream(input, FileMode.Open))
			using (GZipStream output = new GZipStream(System.IO.File.Open(outfile, FileMode.Create, FileAccess.Write), CompressionMode.Compress))
			{
				inputstream.CopyTo(output);
			}

			// Now that it's ready, inject the header info
			using (BinaryWriter sw = new BinaryWriter(new MemoryStream()))
			{
				using (BinaryReader br = new BinaryReader(System.IO.File.OpenRead(outfile)))
				{
					// Write standard header and TGZ info
					byte[] data = Constants.TorrentGZHeader
									.Concat(StringToByteArray(rom.HashData.MD5)) // MD5
									.Concat(StringToByteArray(rom.HashData.CRC)) // CRC
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

				using (BinaryWriter bw = new BinaryWriter(System.IO.File.Open(outfile, FileMode.Create)))
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
					System.IO.File.Move(outfile, Path.Combine(outdir, Path.GetFileName(outfile)));
				}
				catch (Exception ex)
				{
					logger.Warning(ex.ToString());
					System.IO.File.Delete(outfile);
				}
			}

			return true;
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
			if (ext != ".7z" && ext != ".gz" && ext != ".lzma" && ext != ".rar"
				&& ext != ".rev" && ext != ".r00" && ext != ".r01" && ext != ".tar"
				&& ext != ".tgz" && ext != ".tlz" && ext != ".zip" && ext != ".zipx")
			{
				return outtype;
			}

			// Read the first bytes of the file and get the magic number
			try
			{
				byte[] magic = new byte[8];
				using (BinaryReader br = new BinaryReader(System.IO.File.OpenRead(input)))
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

			ArchiveType? archiveType = ArchiveTools.GetCurrentArchiveType(input, logger);
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
		/// http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
		/// </summary>
		public static byte[] StringToByteArray(String hex)
		{
			int NumberChars = hex.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for (int i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}
	}
}
