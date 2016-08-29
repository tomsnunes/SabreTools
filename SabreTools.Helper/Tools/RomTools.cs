using DamienG.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			// Add safeguard if file doesn't exist
			if (!File.Exists(input))
			{
				return new Rom();
			}

			Rom rom = new Rom
			{
				Name = Path.GetFileName(input),
				Type = ItemType.Rom,
				HashData = new Hash
				{
					Size = (new FileInfo(input)).Length,
					CRC = string.Empty,
					MD5 = string.Empty,
					SHA1 = string.Empty,
				}
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
					rom.HashData.CRC = BitConverter.ToString(crc.Hash).Replace("-", "").ToLowerInvariant();

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
						dupetype = GetDuplicateStatus(rom, lastrom, logger);

						// If it's a duplicate, skip adding it to the output but add any missing information
						if (dupetype != DupeType.None)
						{
							savedrom = lastrom;
							pos = i;

							savedrom.HashData.CRC = (String.IsNullOrEmpty(savedrom.HashData.CRC) && !String.IsNullOrEmpty(rom.HashData.CRC) ? rom.HashData.CRC : savedrom.HashData.CRC);
							savedrom.HashData.MD5 = (String.IsNullOrEmpty(savedrom.HashData.MD5) && !String.IsNullOrEmpty(rom.HashData.MD5) ? rom.HashData.MD5 : savedrom.HashData.MD5);
							savedrom.HashData.SHA1 = (String.IsNullOrEmpty(savedrom.HashData.SHA1) && !String.IsNullOrEmpty(rom.HashData.SHA1) ? rom.HashData.SHA1 : savedrom.HashData.SHA1);
							savedrom.Dupe = dupetype;

							// If the current system has a lower ID than the previous, set the system accordingly
							if (rom.Metadata.SystemID < savedrom.Metadata.SystemID)
							{
								savedrom.Metadata.SystemID = rom.Metadata.SystemID;
								savedrom.Metadata.System = rom.Metadata.System;
								savedrom.Machine.Name = rom.Machine.Name;
								savedrom.Name = rom.Name;
							}

							// If the current source has a lower ID than the previous, set the source accordingly
							if (rom.Metadata.SourceID < savedrom.Metadata.SourceID)
							{
								savedrom.Metadata.SourceID = rom.Metadata.SourceID;
								savedrom.Metadata.Source = rom.Metadata.Source;
								savedrom.Machine.Name = rom.Machine.Name;
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
		/// Merge an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="inhashes">List of HashData objects representing the roms to be merged</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>A List of HashData objects representing the merged roms</returns>
		public static List<HashData> Merge(List<HashData> inhashes, Logger logger)
		{
			// Create output list
			List<HashData> outroms = new List<HashData>();

			// Check for null or blank roms first
			if (inhashes == null || inhashes.Count == 0)
			{
				return outroms;
			}

			// Then deduplicate them by checking to see if data matches previous saved roms
			foreach (HashData hash in inhashes)
			{
				// If it's a nodump, add and skip
				if (hash.Roms[0].Nodump)
				{
					outroms.Add(hash);
					continue;
				}

				// If it's the first rom in the list, don't touch it
				if (outroms.Count != 0)
				{
					// Check if the rom is a duplicate
					DupeType dupetype = DupeType.None;
					HashData savedHash = new HashData();
					int pos = -1;
					for (int i = 0; i < outroms.Count; i++)
					{
						HashData lastrom = outroms[i];
						RomData savedRom = savedHash.Roms[0];
						MachineData savedMachine = savedRom.Machine;

						// Get the duplicate status
						dupetype = GetDuplicateStatus(hash, lastrom, logger);

						// If it's a duplicate, skip adding it to the output but add any missing information
						if (dupetype != DupeType.None)
						{
							savedHash = lastrom;
							pos = i;

							savedHash.CRC = (savedHash.CRC == null && hash.CRC != null ? hash.CRC : savedHash.CRC);
							savedHash.MD5 = (savedHash.MD5 == null && hash.MD5 != null ? hash.MD5 : savedHash.MD5);
							savedHash.SHA1 = (savedHash.SHA1 == null && hash.SHA1 != null ? hash.SHA1 : savedHash.SHA1);
							savedRom.DupeType = dupetype;

							// If the current system has a lower ID than the previous, set the system accordingly
							if (hash.Roms[0].Machine.SystemID < savedMachine.SystemID)
							{
								savedMachine.SystemID = hash.Roms[0].Machine.SystemID;
								savedMachine.System = hash.Roms[0].Machine.System;
								savedMachine.Name = hash.Roms[0].Machine.Name;
								savedRom.Name = hash.Roms[0].Name;
							}

							// If the current source has a lower ID than the previous, set the source accordingly
							if (hash.Roms[0].Machine.SourceID < savedMachine.SourceID)
							{
								savedMachine.SourceID = hash.Roms[0].Machine.SourceID;
								savedMachine.Source = hash.Roms[0].Machine.Source;
								savedMachine.Name = hash.Roms[0].Machine.Name;
								savedRom.Name = hash.Roms[0].Name;
							}

							break;
						}
					}

					// If no duplicate is found, add it to the list
					if (dupetype == DupeType.None)
					{
						outroms.Add(hash);
					}
					// Otherwise, if a new rom information is found, add that
					else
					{
						outroms.RemoveAt(pos);
						outroms.Insert(pos, savedHash);
					}
				}
				else
				{
					outroms.Add(hash);
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
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="remove">True to remove matched roms from the input, false otherwise (default)</param>
		/// <returns>List of matched RomData objects</returns>
		public static List<Rom> GetDuplicates(Rom lastrom, Dat datdata, Logger logger, bool remove = false)
		{
			List<Rom> output = new List<Rom>();

			// Check for an empty rom list first
			if (datdata.Files == null || datdata.Files.Count == 0)
			{
				return output;
			}

			// Try to find duplicates
			List<string> keys = datdata.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = datdata.Files[key];
				List<Rom> left = new List<Rom>();
				foreach (Rom rom in roms)
				{
					if (IsDuplicate(rom, lastrom, logger))
					{
						output.Add(rom);
					}
					else
					{
						left.Add(rom);
					}
				}

				// If we're in removal mode, replace the list with the new one
				if (remove)
				{
					datdata.Files[key] = left;
				}
			}

			return output;
		}

		/// <summary>
		/// Determine if a file is a duplicate using partial matching logic
		/// </summary>
		/// <param name="rom">Rom to check for duplicate status</param>
		/// <param name="lastrom">Rom to use as a baseline</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>True if the roms are duplicates, false otherwise</returns>
		public static bool IsDuplicate(Rom rom, Rom lastrom, Logger logger)
		{
			bool dupefound = rom.Equals(lastrom);

			// More wonderful SHA-1 logging that has to be done
			if (rom.HashData.SHA1 == lastrom.HashData.SHA1 && rom.HashData.Size != lastrom.HashData.Size)
			{
				logger.User("SHA-1 mismatch - Hash: " + rom.HashData.SHA1);
			}

			return dupefound;
		}

		/// <summary>
		/// Return the duplicate status of two roms
		/// </summary>
		/// <param name="rom">Current rom to check</param>
		/// <param name="lastrom">Last rom to check against</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>The DupeType corresponding to the relationship between the two</returns>
		public static DupeType GetDuplicateStatus(Rom rom, Rom lastrom, Logger logger)
		{
			DupeType output = DupeType.None;

			// If we don't have a duplicate at all, return none
			if (!IsDuplicate(rom, lastrom, logger))
			{
				return output;
			}

			// If the duplicate is external already or should be, set it
			if (lastrom.Dupe >= DupeType.ExternalHash || lastrom.Metadata.SystemID != rom.Metadata.SystemID || lastrom.Metadata.SourceID != rom.Metadata.SourceID)
			{
				if (lastrom.Machine.Name == rom.Machine.Name && lastrom.Name == rom.Name)
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
				if (lastrom.Machine.Name == rom.Machine.Name && lastrom.Name == rom.Name)
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
						if (x.Machine.Name == y.Machine.Name)
						{
							return String.Compare(x.Name, y.Name);
						}
						return String.Compare(x.Machine.Name, y.Machine.Name);
					}
					return (norename ? String.Compare(x.Machine.Name, y.Machine.Name) : x.Metadata.SourceID - y.Metadata.SourceID);
				}
				return (norename ? String.Compare(x.Machine.Name, y.Machine.Name) : x.Metadata.SystemID - y.Metadata.SystemID);
			});
			return true;
		}

		/// <summary>
		/// Clean a hash string and pad to the correct size
		/// </summary>
		/// <param name="hash">Hash string to sanitize</param>
		/// <param name="padding">Amount of characters to pad to</param>
		/// <returns>Cleaned string</returns>
		public static string CleanHashData(string hash, int padding)
		{
			// First get the hash to the correct length
			hash = (String.IsNullOrEmpty(hash) ? "" : hash.Trim());
			hash = (hash.StartsWith("0x") ? hash.Remove(0, 2) : hash);
			hash = (hash == "-" ? "" : hash);
			hash = (String.IsNullOrEmpty(hash) ? "" : hash.PadLeft(padding, '0'));
			hash = hash.ToLowerInvariant();

			// Then make sure that it has the correct characters
			if (!Regex.IsMatch(hash, "[0-9a-f]{" + padding + "}"))
			{
				hash = "";
			}

			return hash;
		}

		/// <summary>
		/// Clean a hash byte array and pad to the correct size
		/// </summary>
		/// <param name="hash">Hash byte array to sanitize</param>
		/// <param name="padding">Amount of bytes to pad to</param>
		/// <returns>Cleaned byte array</returns>
		public static byte[] CleanHashData(byte[] hash, int padding)
		{
			// If we have a null hash or a <=0 padding, return the hash
			if (hash == null || padding <= 0)
			{
				return hash;
			}

			// If we have a hash longer than the padding, trim and return
			if (hash.Length > padding)
			{
				return hash.Take(padding).ToArray();
			}

			// If we have a hash of the correct length, return
			if (hash.Length == padding)
			{
				return hash;
			}

			// Otherwise get the output byte array of the correct length
			byte[] newhash = new byte[padding];

			// Then write the proper number of empty bytes
			int padNeeded = padding - hash.Length;
			int index = 0;
			for (index = 0; index < padNeeded; index++)
			{
				newhash[index] = 0x00;
			}

			// Now add the original hash
			for (int i = 0; i < hash.Length; i++)
			{
				newhash[index + i] = hash[index];
			}

			return newhash;
		}
	}
}
