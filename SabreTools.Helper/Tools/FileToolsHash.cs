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
	public class FileToolsHash
	{
		#region Archive Writing

		// All things in this region are direct ports and do not take advantage of the multiple rom per hash that comes with the new system

		/// <summary>
		/// Copy a file to an output archive
		/// </summary>
		/// <param name="input">Input filename to be moved</param>
		/// <param name="output">Output directory to build to</param>
		/// <param name="hash">RomData representing the new information</param>
		/// <remarks>This uses the new system that is not implemented anywhere yet</remarks>
		public static void WriteToArchive(string input, string output, RomData rom)
		{
			string archiveFileName = Path.Combine(output, rom.Machine + ".zip");

			ZipArchive outarchive = null;
			try
			{
				if (!File.Exists(archiveFileName))
				{
					outarchive = System.IO.Compression.ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
				}
				else
				{
					outarchive = System.IO.Compression.ZipFile.Open(archiveFileName, ZipArchiveMode.Update);
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
		/// Copy a file to an output archive using SharpCompress
		/// </summary>
		/// <param name="input">Input filename to be moved</param>
		/// <param name="output">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		/// <remarks>This uses the new system that is not implemented anywhere yet</remarks>
		public static void WriteToManagedArchive(string input, string output, RomData rom)
		{
			string archiveFileName = Path.Combine(output, rom.Machine + ".zip");

			// Delete an empty file first
			if (File.Exists(archiveFileName) && new FileInfo(archiveFileName).Length == 0)
			{
				File.Delete(archiveFileName);
			}

			// Get if the file should be written out
			bool newfile = File.Exists(archiveFileName) && new FileInfo(archiveFileName).Length != 0;

			using (SharpCompress.Archive.Zip.ZipArchive archive = (newfile
				? ArchiveFactory.Open(archiveFileName, Options.LookForHeader) as SharpCompress.Archive.Zip.ZipArchive
				: ArchiveFactory.Create(ArchiveType.Zip) as SharpCompress.Archive.Zip.ZipArchive))
			{
				try
				{
					if (File.Exists(input))
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

			if (File.Exists(archiveFileName + ".tmp"))
			{
				File.Delete(archiveFileName);
				File.Move(archiveFileName + ".tmp", archiveFileName);
			}
		}

		#endregion

		#region File Information

		/// <summary>
		/// Generate a list of HashData objects from the header values in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>List of HashData objects representing the found data</returns>
		/// <remarks>This uses the new system that is not implemented anywhere yet</remarks>
		public static List<HashData> GetArchiveFileHashes(string input, Logger logger)
		{
			List<HashData> hashes = new List<HashData>();
			string gamename = Path.GetFileNameWithoutExtension(input);

			// First get the archive type
			ArchiveType? at = FileTools.GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return hashes;
			}

			// If we got back GZip, try to get TGZ info first
			else if (at == ArchiveType.GZip)
			{
				HashData possibleTgz = GetTorrentGZFileHash(input, logger);

				// If it was, then add it to the outputs and continue
				if (possibleTgz.Size != -1)
				{
					hashes.Add(possibleTgz);
					return hashes;
				}
			}

			IReader reader = null;
			try
			{
				logger.Log("Found archive of type: " + at);
				long size = 0;
				byte[] crc = null;

				// If we have a gzip file, get the crc directly
				if (at == ArchiveType.GZip)
				{
					// Get the CRC and size from the file
					using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
					{
						br.BaseStream.Seek(-8, SeekOrigin.End);
						crc = br.ReadBytes(4).Reverse().ToArray();
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
								+ (crc == null ? BitConverter.GetBytes(reader.Entry.Crc) : crc));

							RomData temprom = new RomData
							{
								Type = ItemType.Rom,
								Name = reader.Entry.Key,
								Machine = new MachineData
								{
									Name = gamename,
									Description = gamename,
								},
							};
							HashData temphash = new HashData
							{
								Size = (size == 0 ? reader.Entry.Size : size),
								CRC = (crc == null ? BitConverter.GetBytes(reader.Entry.Crc) : crc),
								MD5 = null,
								SHA1 = null,
								Roms = new List<RomData>(),
							};
							temphash.Roms.Add(temprom);
							hashes.Add(temphash);
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

			return hashes;
		}

		/// <summary>
		/// Retrieve file information for a single torrent GZ file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Populated HashData object if success, empty one on error</returns>
		/// <remarks>This uses the new system that is not implemented anywhere yet</remarks>
		public static HashData GetTorrentGZFileHash(string input, Logger logger)
		{
			string datum = Path.GetFileName(input).ToLowerInvariant();
			string sha1 = Path.GetFileNameWithoutExtension(input).ToLowerInvariant();
			long filesize = new FileInfo(input).Length;

			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{40}\.gz"))
			{
				logger.Warning("Non SHA-1 filename found, skipping: '" + datum + "'");
				return new HashData();
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				logger.Warning("Possibly corrupt file '" + input + "' with size " + Style.GetBytesReadable(filesize));
				return new HashData();
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
				correct &= (header[i] == Constants.TorrentGZHeader[i]);
			}
			if (!correct)
			{
				return new HashData();
			}

			// Now convert the size and get the right position
			long extractedsize = (long)BitConverter.ToUInt64(headersz.Reverse().ToArray(), 0);

			RomData temprom = new RomData
			{
				Type = ItemType.Rom,
				Name = sha1,
				Machine = new MachineData
				{
					Name = sha1,
					Description = sha1,
				},
			};
			HashData temphash = new HashData
			{
				Size = extractedsize,
				CRC = headercrc,
				MD5 = headermd5,
				SHA1 = Style.StringToByteArray(sha1),
				Roms = new List<RomData>(),
			};
			temphash.Roms.Add(temprom);

			return temphash;
		}

		#endregion
	}
}
