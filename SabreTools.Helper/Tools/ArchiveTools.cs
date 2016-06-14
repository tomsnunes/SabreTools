using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Writer;
using System;
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
		/// <param name="archiveType">Type of archive to attempt to write to</param>
		/// <param name="rom">RomData representing the new information</param>
		public static void WriteArchiveOrFile(string input, string output, ArchiveType archiveType, RomData rom)
		{
			string archiveFileName = output + Path.DirectorySeparatorChar + rom.Game + ".zip";
			string singleFileName = output + Path.DirectorySeparatorChar + rom.Game + Path.DirectorySeparatorChar + rom.Name;

			IWriter outarchive = null;
			FileStream fs = null;
			try
			{
				fs = File.OpenWrite(archiveFileName);
				outarchive = WriterFactory.Open(fs, ArchiveType.Zip, CompressionType.Deflate);
				outarchive.Write(rom.Name, input);
			}
			catch
			{
				if (!File.Exists(singleFileName))
				{
					File.Copy(input, singleFileName);
				}
			}
			finally
			{
				outarchive?.Dispose();
				fs?.Close();
				fs?.Dispose();
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
			IArchive archive = null;
			try
			{
				archive = ArchiveFactory.Open(input);
				ArchiveType at = archive.Type;
				logger.Log("Found archive of type: " + at);

				if ((at == ArchiveType.Zip && zip != ArchiveScanLevel.External) ||
					(at == ArchiveType.SevenZip && sevenzip != ArchiveScanLevel.External) ||
					(at == ArchiveType.Rar && rar != ArchiveScanLevel.External))
				{
					// Create the temp directory
					DirectoryInfo di = Directory.CreateDirectory(tempdir);

					// Extract all files to the temp directory
					IReader reader = archive.ExtractAllEntries();
					reader.WriteAllToDirectory(tempdir, ExtractOptions.ExtractFullPath);
					encounteredErrors = false;
				}
				else if (at == ArchiveType.GZip && gz != ArchiveScanLevel.External)
				{
					// Close the original archive handle
					archive.Dispose();

					// Create the temp directory
					DirectoryInfo di = Directory.CreateDirectory(tempdir);

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
				archive.Dispose();
			}
			catch (InvalidOperationException)
			{
				encounteredErrors = true;
				if (archive != null)
				{
					archive.Dispose();
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				encounteredErrors = true;
				if (archive != null)
				{
					archive.Dispose();
				}
			}

			return !encounteredErrors;
		}

		/// <summary>
		/// Retrieve file information for a single torrent GZ file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static RomData GetTorrentGZFileInfo(string input, Logger logger)
		{
			string datum = Path.GetFileName(input).ToLowerInvariant();
			long filesize = new FileInfo(input).Length;

			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{40}\.gz"))
			{
				logger.Warning("Non SHA-1 filename found, skipping: '" + datum + "'");
				return new RomData();
			}

			// Check if the file is at least the minimum length
			if (filesize < 32 /* bytes */)
			{
				logger.Warning("Possibly corrupt file '" + input + "' with size " + Style.GetBytesReadable(filesize));
				return new RomData();
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

			RomData rom = new RomData
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
	}
}
