using DamienG.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SabreTools.Helper
{
	public class RomTools
	{
		/// <summary>
		/// Retrieve file information for a single file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="noMD5">True if MD5 hashes should not be calculated, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should not be calcluated, false otherwise</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		/// <remarks>Add read-offset for hash info</remarks>
		public static Rom GetSingleFileInfo(string input, bool noMD5 = false, bool noSHA1 = false, long offset = 0)
		{
			Rom rom = new Rom
			{
				Name = Path.GetFileName(input),
				Type = "rom",
				Size = (new FileInfo(input)).Length,
				CRC = string.Empty,
				MD5 = string.Empty,
				SHA1 = string.Empty,
			};

			try
			{
				using (Crc32 crc = new Crc32())
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
						crc.TransformBlock(buffer, 0, read, buffer, 0);
						if (!noMD5)
						{
							md5.TransformBlock(buffer, 0, read, buffer, 0);
						}
						if (!noSHA1)
						{
							sha1.TransformBlock(buffer, 0, read, buffer, 0);
						}
					}

					crc.TransformFinalBlock(buffer, 0, 0);
					rom.CRC = BitConverter.ToString(crc.Hash).Replace("-", "").ToLowerInvariant();

					if (!noMD5)
					{
						md5.TransformFinalBlock(buffer, 0, 0);
						rom.MD5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
					}
					if (!noSHA1)
					{
						sha1.TransformFinalBlock(buffer, 0, 0);
						rom.SHA1 = BitConverter.ToString(sha1.Hash).Replace("-", "").ToLowerInvariant();
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
		/// Get the header type for the input file
		/// </summary>
		/// <param name="input">Input file to parse for header</param>
		/// <param name="hs">Passed back size of the header</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The detected HeaderType</returns>
		public static HeaderType GetFileHeaderType(string input, out int hs, Logger logger)
		{
			// Open the file in read mode
			BinaryReader br = new BinaryReader(File.OpenRead(input));

			// Extract the first 1024 bytes of the file
			byte[] hbin = br.ReadBytes(1024);
			string header = BitConverter.ToString(hbin).Replace("-", string.Empty);
			br.Dispose();

			// Determine the type of the file from the header, if possible
			HeaderType type = HeaderType.None;

			// Loop over the header types and see if there's a match
			hs = -1;
			foreach (HeaderType test in Enum.GetValues(typeof(HeaderType)))
			{
				Dictionary<string, int> tempDict = new Dictionary<string, int>();

				// Try populating the dictionary from the master list
				try
				{
					tempDict = Remapping.HeaderMaps[test.ToString()];
				}
				catch
				{
					logger.Warning("The mapping for '" + test.ToString() + "' cannot be found!");
					continue;
				}

				// Loop over the dictionary and see if there are matches
				foreach (KeyValuePair<string, int> entry in tempDict)
				{
					if (Regex.IsMatch(header, entry.Key))
					{
						type = test;
						hs = entry.Value;
						break;
					}
				}

				// If we found something, break out
				if (type != HeaderType.None)
				{
					break;
				}
			}

			return type;
		}

		/// <summary>
		/// Merge an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="inroms">List of RomData objects representing the roms to be merged</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>A List of RomData objects representing the merged roms</returns>
		public static List<Rom> Merge(List<Rom> inroms, Logger logger)
		{
			// Check for null or blank roms first
			if (inroms == null || inroms.Count == 0)
			{
				return new List<Rom>();
			}

			// Create output list
			List<Rom> outroms = new List<Rom>();

			// Then deduplicate them by checking to see if data matches previous saved roms
			foreach (Rom rom in inroms)
			{
				// If it's a nodump, add and skip
				if (rom.Nodump)
				{
					outroms.Add(rom);
					continue;
				}

				// If it's the first rom in the list, don't touch it
				if (outroms.Count != 0)
				{
					// Check if the rom is a duplicate
					DupeType dupetype = DupeType.None;
					Rom savedrom = new Rom();
					int pos = -1;
					for (int i = 0; i < outroms.Count; i++)
					{
						Rom lastrom = outroms[i];

						// Get the duplicate status
						dupetype = GetDuplicateStatus(rom, lastrom);

						// If it's a duplicate, skip adding it to the output but add any missing information
						if (dupetype != DupeType.None)
						{
							savedrom = lastrom;
							pos = i;

							savedrom.CRC = (String.IsNullOrEmpty(savedrom.CRC) && !String.IsNullOrEmpty(rom.CRC) ? rom.CRC : savedrom.CRC);
							savedrom.MD5 = (String.IsNullOrEmpty(savedrom.MD5) && !String.IsNullOrEmpty(rom.MD5) ? rom.MD5 : savedrom.MD5);
							savedrom.SHA1 = (String.IsNullOrEmpty(savedrom.SHA1) && !String.IsNullOrEmpty(rom.SHA1) ? rom.SHA1 : savedrom.SHA1);
							savedrom.Dupe = dupetype;

							// If the current system has a lower ID than the previous, set the system accordingly
							if (rom.Metadata.SystemID < savedrom.Metadata.SystemID)
							{
								savedrom.Metadata.SystemID = rom.Metadata.SystemID;
								savedrom.Metadata.System = rom.Metadata.System;
								savedrom.Game = rom.Game;
								savedrom.Name = rom.Name;
							}

							// If the current source has a lower ID than the previous, set the source accordingly
							if (rom.Metadata.SourceID < savedrom.Metadata.SourceID)
							{
								savedrom.Metadata.SourceID = rom.Metadata.SourceID;
								savedrom.Metadata.Source = rom.Metadata.Source;
								savedrom.Game = rom.Game;
								savedrom.Name = rom.Name;
							}

							break;
						}
					}

					// If no duplicate is found, add it to the list
					if (dupetype == DupeType.None)
					{
						outroms.Add(rom);
					}
					// Otherwise, if a new rom information is found, add that
					else
					{
						outroms.RemoveAt(pos);
						outroms.Insert(pos, savedrom);
					}
				}
				else
				{
					outroms.Add(rom);
				}
			}

			// Then return the result
			return outroms;
		}

		/// <summary>
		/// List all duplicates found in a DAT based on a rom
		/// </summary>
		/// <param name="lastrom">Rom to use as a base</param>
		/// <param name="datdata">DAT to match against</param>
		/// <returns>List of matched RomData objects</returns>
		public static List<Rom> GetDuplicates(Rom lastrom, Dat datdata)
		{
			List<Rom> output = new List<Rom>();

			// Check for an empty rom list first
			if (datdata.Roms == null || datdata.Roms.Count == 0)
			{
				return output;
			}

			// Try to find duplicates
			foreach (List<Rom> roms in datdata.Roms.Values)
			{
				foreach (Rom rom in roms)
				{
					if (IsDuplicate(rom, lastrom))
					{
						output.Add(rom);
					}
				}
			}

			return output;
		}

		/// <summary>
		/// Determine if a file is a duplicate using partial matching logic
		/// </summary>
		/// <param name="rom">Rom to check for duplicate status</param>
		/// <param name="lastrom">Rom to use as a baseline</param>
		/// <returns>True if the roms are duplicates, false otherwise</returns>
		public static bool IsDuplicate(Rom rom, Rom lastrom)
		{
			bool dupefound = false;

			// If either is a nodump, it's never a match
			if (rom.Nodump || lastrom.Nodump)
			{
				return dupefound;
			}

			if (rom.Type == "rom" && lastrom.Type == "rom")
			{
				dupefound = ((rom.Size == lastrom.Size) &&
					((String.IsNullOrEmpty(rom.CRC) || String.IsNullOrEmpty(lastrom.CRC)) || rom.CRC == lastrom.CRC) &&
					((String.IsNullOrEmpty(rom.MD5) || String.IsNullOrEmpty(lastrom.MD5)) || rom.MD5 == lastrom.MD5) &&
					((String.IsNullOrEmpty(rom.SHA1) || String.IsNullOrEmpty(lastrom.SHA1)) || rom.SHA1 == lastrom.SHA1)
				);
			}
			else if (rom.Type == "disk" && lastrom.Type == "disk")
			{
				dupefound = (((String.IsNullOrEmpty(rom.MD5) || String.IsNullOrEmpty(lastrom.MD5)) || rom.MD5 == lastrom.MD5) &&
					((String.IsNullOrEmpty(rom.SHA1) || String.IsNullOrEmpty(lastrom.SHA1)) || rom.SHA1 == lastrom.SHA1)
				);
			}

			return dupefound;
		}

		/// <summary>
		/// Return the duplicate status of two roms
		/// </summary>
		/// <param name="rom">Current rom to check</param>
		/// <param name="lastrom">Last rom to check against</param>
		/// <returns>The DupeType corresponding to the relationship between the two</returns>
		public static DupeType GetDuplicateStatus(Rom rom, Rom lastrom)
		{
			DupeType output = DupeType.None;

			// If we don't have a duplicate at all, return none
			if (!IsDuplicate(rom, lastrom))
			{
				return output;
			}

			// If the duplicate is external already or should be, set it
			if (lastrom.Dupe >= DupeType.ExternalHash || lastrom.Metadata.SystemID != rom.Metadata.SystemID || lastrom.Metadata.SourceID != rom.Metadata.SourceID)
			{
				if (lastrom.Game == rom.Game && lastrom.Name == rom.Name)
				{
					output = DupeType.ExternalAll;
				}
				else
				{
					output = DupeType.ExternalHash;
				}
			}

			// Otherwise, it's considered an internal dupe
			else
			{
				if (lastrom.Game == rom.Game && lastrom.Name == rom.Name)
				{
					output = DupeType.InternalAll;
				}
				else
				{
					output = DupeType.InternalHash;
				}
			}

			return output;
		}

		/// <summary>
		/// Sort a list of RomData objects by SystemID, SourceID, Game, and Name (in order)
		/// </summary>
		/// <param name="roms">List of RomData objects representing the roms to be sorted</param>
		/// <param name="norename">True if files are not renamed, false otherwise</param>
		/// <returns>True if it sorted correctly, false otherwise</returns>
		public static bool Sort(List<Rom> roms, bool norename)
		{
			roms.Sort(delegate (Rom x, Rom y)
			{
				if (x.Metadata.SystemID == y.Metadata.SystemID)
				{
					if (x.Metadata.SourceID == y.Metadata.SourceID)
					{
						if (x.Game == y.Game)
						{
							return String.Compare(x.Name, y.Name);
						}
						return String.Compare(x.Game, y.Game);
					}
					return (norename ? String.Compare(x.Game, y.Game) : x.Metadata.SourceID - y.Metadata.SourceID);
				}
				return (norename ? String.Compare(x.Game, y.Game) : x.Metadata.SystemID - y.Metadata.SystemID);
			});
			return true;
		}
	}
}
