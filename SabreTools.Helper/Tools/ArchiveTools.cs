using System;
using System.IO;
using System.IO.Compression;

using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Writer;

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
	}
}
