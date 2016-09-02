using System.Collections.Generic;

namespace SabreTools.Helper
{
	public class RomToolsHash
	{
		#region HashData-based sorting and merging

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

		/*
		/// <summary>
		/// List all duplicates found in a DAT based on a rom
		/// </summary>
		/// <param name="lastrom">Hash to use as a base</param>
		/// <param name="datdata">DAT to match against</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="remove">True to remove matched roms from the input, false otherwise (default)</param>
		/// <returns>List of matched HashData objects</returns>
		public static List<HashData> GetDuplicates(HashData lastrom, DatData datdata, Logger logger, bool remove = false)
		{
			List<HashData> output = new List<HashData>();

			// Check for an empty rom list first
			if (datdata.Hashes == null || datdata.Hashes.Count == 0)
			{
				return output;
			}

			// Try to find duplicates
			for (int i = 0; i < datdata.Hashes.Count; i++)
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
		*/

		/// <summary>
		/// Determine if a file is a duplicate using partial matching logic
		/// </summary>
		/// <param name="hash">Hash to check for duplicate status</param>
		/// <param name="lastHash">Hash to use as a baseline</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>True if the hashes are duplicates, false otherwise</returns>
		public static bool IsDuplicate(HashData hash, int hashIndex, HashData lastHash, int lastHashIndex, Logger logger)
		{
			bool dupefound = hash.Equals(lastHash);

			// More wonderful SHA-1 logging that has to be done
			if (hash.SHA1 == lastHash.SHA1 && hash.Size != lastHash.Size)
			{
				logger.User("SHA-1 mismatch - Hash: " + hash.SHA1);
			}

			return dupefound;
		}

		/// <summary>
		/// Return the duplicate status of two hashes
		/// </summary>
		/// <param name="hash">Current hash to check</param>
		/// <param name="lasthash">Last hash to check against</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>The DupeType corresponding to the relationship between the two</returns>
		public static DupeType GetDuplicateStatus(HashData hash, HashData lasthash, Logger logger)
		{
			DupeType output = DupeType.None;

			/*
			// If we don't have a duplicate at all, return none
			if (!IsDuplicate(hash, lasthash, logger))
			{
				return output;
			}
			*/

			// If the duplicate is external already or should be, set it
			if (lasthash.Roms[0].DupeType >= DupeType.ExternalHash || lasthash.Roms[0].Machine.SystemID != hash.Roms[0].Machine.SystemID || lasthash.Roms[0].Machine.SourceID != hash.Roms[0].Machine.SourceID)
			{
				if (lasthash.Roms[0].Machine.Name == hash.Roms[0].Machine.Name && lasthash.Roms[0].Name == hash.Roms[0].Name)
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
				if (lasthash.Roms[0].Machine.Name == hash.Roms[0].Machine.Name && lasthash.Roms[0].Name == hash.Roms[0].Name)
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

		#endregion
	}
}
