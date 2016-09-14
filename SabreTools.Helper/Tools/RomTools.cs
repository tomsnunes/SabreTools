using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SabreTools.Helper
{
	public class RomTools
	{
		#region Rom-based sorting and merging

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
			try
			{
				roms.Sort(delegate (Rom x, Rom y)
				{
					if (x.Metadata.SystemID == y.Metadata.SystemID)
					{
						if (x.Metadata.SourceID == y.Metadata.SourceID)
						{
							if (x.Machine.Name == y.Machine.Name)
							{
								if ((x.Type == ItemType.Rom || x.Type == ItemType.Disk) && (y.Type == ItemType.Rom || y.Type == ItemType.Disk))
								{
									if (Path.GetDirectoryName(x.Name) == Path.GetDirectoryName(y.Name))
									{
										return Style.CompareNumeric(Path.GetFileName(x.Name), Path.GetFileName(y.Name));
									}
									return Style.CompareNumeric(Path.GetDirectoryName(x.Name), Path.GetDirectoryName(y.Name));
								}
								else if ((x.Type == ItemType.Rom || x.Type == ItemType.Disk) && (y.Type != ItemType.Rom && y.Type != ItemType.Disk))
								{
									return -1;
								}
								else if ((x.Type != ItemType.Rom && x.Type != ItemType.Disk) && (y.Type == ItemType.Rom || y.Type == ItemType.Disk))
								{
									return 1;
								}
								else
								{
									if (Path.GetDirectoryName(x.Name) == Path.GetDirectoryName(y.Name))
									{
										return Style.CompareNumeric(Path.GetFileName(x.Name), Path.GetFileName(y.Name));
									}
									return Style.CompareNumeric(Path.GetDirectoryName(x.Name), Path.GetDirectoryName(y.Name));
								}
							}
							return Style.CompareNumeric(x.Machine.Name, y.Machine.Name);
						}
						return (norename ? Style.CompareNumeric(x.Machine.Name, y.Machine.Name) : x.Metadata.SourceID - y.Metadata.SourceID);
					}
					return (norename ? Style.CompareNumeric(x.Machine.Name, y.Machine.Name) : x.Metadata.SystemID - y.Metadata.SystemID);
				});
				return true;
			}
			catch (Exception)
			{
				// Absorb the error
				return false;
			}
		}

		#endregion
	}
}
