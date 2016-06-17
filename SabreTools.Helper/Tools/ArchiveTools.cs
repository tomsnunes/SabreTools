using SharpCompress.Archive;
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
		/// Copy a file either to an output archive or to an output folder
		/// </summary>
		/// <param name="input">Input filename to be moved</param>
		/// <param name="output">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		public static void WriteToArchive(string input, string output, Rom rom)
		{
			string archiveFileName = Path.Combine(output, rom.Game + ".zip");

			ZipArchive outarchive = null;
			try
			{
				if (!File.Exists(archiveFileName))
				{
					outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
				}
				else
				{
					outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update);
				}

				if (File.Exists(input))
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
			try
			{
				reader = ReaderFactory.Open(File.OpenRead(input));
				logger.Log("Found archive of type: " + at);

				if ((at == ArchiveType.Zip && zip != ArchiveScanLevel.External) ||
					(at == ArchiveType.SevenZip && sevenzip != ArchiveScanLevel.External) ||
					(at == ArchiveType.Rar && rar != ArchiveScanLevel.External))
				{
					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					// Extract all files to the temp directory
					reader.WriteAllToDirectory(tempdir, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
					encounteredErrors = false;
				}
				else if (at == ArchiveType.GZip && gz != ArchiveScanLevel.External)
				{
					// Close the original archive handle
					reader.Dispose();

					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					using (FileStream itemstream = File.OpenRead(input))
					{
						using (FileStream outstream = File.Create(tempdir + Path.GetFileNameWithoutExtension(input)))
						{
							using (GZipStream gzstream = new GZipStream(itemstream, CompressionMode.Decompress))
							{
								gzstream.CopyTo(outstream);
							}
						}
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
			catch (Exception ex)
			{
				// Don't log file open errors
				encounteredErrors = true;
			}
			finally
			{
				reader?.Dispose();
			}

			return !encounteredErrors;
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
							outfile = Path.Combine(tempdir, reader.Entry.Key);
							reader.WriteEntryToFile(outfile, ExtractOptions.Overwrite);
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
		/// <param name="inputarc">Source archive name</param>
		/// <param name="outputarc">Destination archive name</param>
		/// <param name="inentryname">Input entry name</param>
		/// <param name="outentryname">Output entry name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the copy was a success, false otherwise</returns>
		public static bool CopyFileBetweenArchives(string inputarc, string outputarc,
			string inentryname, string outentryname, Logger logger)
		{
			bool success = false;

			// First get the archive types
			ArchiveType? iat = GetCurrentArchiveType(inputarc, logger);
			ArchiveType? oat = (File.Exists(outputarc) ? GetCurrentArchiveType(outputarc, logger) : ArchiveType.Zip);

			// If we got back null (or the output is not a Zipfile), then it's not an archive, so we we return
			if (iat == null || (oat == null || oat != ArchiveType.Zip) || inputarc == outputarc)
			{
				return success;
			}

			IReader reader = null;
			ZipArchive outarchive = null;
			try
			{
				reader = ReaderFactory.Open(File.OpenRead(inputarc));

				if (iat == ArchiveType.Zip || iat == ArchiveType.SevenZip || iat == ArchiveType.Rar)
				{
					while (reader.MoveToNextEntry())
					{
						logger.Log("Current entry name: '" + reader.Entry.Key + "'");
						if (reader.Entry != null && reader.Entry.Key.Contains(inentryname))
						{
							if (!File.Exists(outputarc))
							{
								outarchive = ZipFile.Open(outputarc, ZipArchiveMode.Create);
							}
							else
							{
								outarchive = ZipFile.Open(outputarc, ZipArchiveMode.Update);
							}

							if (outarchive.Mode == ZipArchiveMode.Create || outarchive.GetEntry(outentryname) == null)
							{
								IArchiveEntry iae = outarchive.CreateEntry(outentryname, CompressionLevel.Optimal) as IArchiveEntry;
								using (Stream iaestream = iae.OpenEntryStream())
								using (Stream readerstream = (reader.Entry as IArchiveEntry).OpenEntryStream())
								{
									readerstream.CopyTo(iaestream);
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

			IReader reader = null;
			try
			{
				reader = ReaderFactory.Open(File.OpenRead(input));
				logger.Log("Found archive of type: " + at);

				if (at != ArchiveType.Tar)
				{
					while (reader.MoveToNextEntry())
					{
						if (reader.Entry != null && !reader.Entry.IsDirectory)
						{
							logger.Log("Entry found: '" + reader.Entry.Key + "': " + reader.Entry.Size + ", " + reader.Entry.Crc.ToString("X").ToLowerInvariant());

							roms.Add(new Rom
							{
								Type = "rom",
								Name = reader.Entry.Key,
								Game = gamename,
								Size = reader.Entry.Size,
								CRC = reader.Entry.Crc.ToString("X").ToLowerInvariant(),
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
			if (filesize < 32 /* bytes */)
			{
				logger.Warning("Possibly corrupt file '" + input + "' with size " + Style.GetBytesReadable(filesize));
				return new Rom();
			}

			// Get the Romba-specific header data
			byte[] header;
			byte[] footer;
			using (FileStream itemstream = File.OpenRead(input))
			{
				using (BinaryReader br = new BinaryReader(itemstream))
				{
					header = br.ReadBytes(32);
					br.BaseStream.Seek(-4, SeekOrigin.End);
					footer = br.ReadBytes(4);
				}
			}

			// Now convert the data and get the right positions
			string headerstring = BitConverter.ToString(header).Replace("-", string.Empty);
			string gzmd5 = headerstring.Substring(24, 32);
			string gzcrc = headerstring.Substring(56, 8);
			string gzsize = BitConverter.ToString(footer.Reverse().ToArray()).Replace("-", string.Empty);
			long extractedsize = Convert.ToInt64(gzsize, 16);

			// Only try to add if the file size is greater than 750 MiB
			if (filesize >= (750 * Constants.MibiByte))
			{
				// ISIZE is mod 4GiB, so we add that if the ISIZE is smaller than the filesize and greater than 1% different
				bool shouldfollowup = false;
				if (extractedsize < filesize && (100 * extractedsize / filesize) < 99 /* percent */)
				{
					logger.Log("mancalc - Filename: '" + Path.GetFullPath(input) + "'\nExtracted file size: " +
						extractedsize + ", " + Style.GetBytesReadable(extractedsize) + "\nArchive file size: " + filesize + ", " + Style.GetBytesReadable(filesize));
				}
				while (extractedsize < filesize && (100 * extractedsize / filesize) < 99 /* percent */)
				{
					extractedsize += (4 * Constants.GibiByte);
					shouldfollowup = true;
				}
				if (shouldfollowup)
				{
					logger.Log("Filename: '" + Path.GetFullPath(input) + "'\nFinal file size: " + extractedsize + ", " + Style.GetBytesReadable(extractedsize) +
					"\nExtracted CRC: " + gzcrc + "\nExtracted MD5: " + gzmd5 + "\nSHA-1: " + Path.GetFileNameWithoutExtension(input));
				}
			}

			Rom rom = new Rom
			{
				Type = "rom",
				Game = Path.GetFileNameWithoutExtension(input),
				Name = Path.GetFileNameWithoutExtension(input),
				Size = extractedsize,
				CRC = gzcrc,
				MD5 = gzmd5,
				SHA1 = Path.GetFileNameWithoutExtension(input),
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

			IReader reader = null;
			try
			{
				reader = ReaderFactory.Open(File.OpenRead(input));
				outtype = reader.ArchiveType;
			}
			catch (Exception)
			{
				// Don't log archive open errors
			}
			finally
			{
				reader?.Dispose();
			}

			return outtype;
		}
	}
}
