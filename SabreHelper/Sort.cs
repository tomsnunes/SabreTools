using System;
using System.Collections.Generic;

namespace SabreTools.Helper
{
	public class Sort
	{
		public static List<RomData> Merge(List<RomData> inroms, bool presorted = false)
		{
			List<RomData> outroms = new List<RomData>();

			// First sort the roms by size, crc, sysid, srcid, md5, and sha1 (in order), if not sorted already
			if (!presorted)
			{
				inroms.Sort(delegate (RomData x, RomData y)
				{
					if (x.Size == y.Size)
					{
						if (x.CRC == y.CRC)
						{
							if (x.SystemID == y.SystemID)
							{
								if (x.SourceID == y.SourceID)
								{
									if (x.MD5 == y.MD5)
									{
										return String.Compare(x.SHA1, y.SHA1);
									}
									return String.Compare(x.MD5, y.MD5);
								}
								return x.SourceID - y.SourceID;
							}
							return x.SystemID - y.SystemID;
						}
						return String.Compare(x.CRC, y.CRC);
					}
					return (int)(x.Size - y.Size);
				});
			}

			// Then, deduplicate them by checking to see if data matches
			foreach (RomData rom in inroms)
			{
				// If it's the first rom in the list, don't touch it
				if (outroms.Count != 0)
				{
					// Check if the rom is a duplicate
					RomData last = outroms[outroms.Count - 1];

					bool shouldcont = false;
					if (rom.Type == "rom" && last.Type == "rom")
					{
						shouldcont = ((rom.Size != -1 && rom.Size == last.Size) && (
								(rom.CRC != "" && last.CRC != "" && rom.CRC == last.CRC) ||
								(rom.MD5 != "" && last.MD5 != "" && rom.MD5 == last.MD5) ||
								(rom.SHA1 != "" && last.SHA1 != "" && rom.SHA1 == last.SHA1)
								)
							);
					}
					else if (rom.Type == "disk" && last.Type == "disk")
					{
						shouldcont = ((rom.MD5 != "" && last.MD5 != "" && rom.MD5 == last.MD5) ||
								(rom.SHA1 != "" && last.SHA1 != "" && rom.SHA1 == last.SHA1)
							);
					}

					// If it's a duplicate, skip adding it to the output but add any missing information
					if (shouldcont)
					{
						last.CRC = (last.CRC == "" && rom.CRC != "" ? rom.CRC : last.CRC);
						last.MD5 = (last.MD5 == "" && rom.MD5 != "" ? rom.MD5 : last.MD5);
						last.SHA1 = (last.SHA1 == "" && rom.SHA1 != "" ? rom.SHA1 : last.SHA1);

						outroms.RemoveAt(inroms.Count - 1);
						outroms.Insert(inroms.Count, last);

						continue;
					}
				}
			}

			// Then return the result
			return outroms;
		}

		/// <summary>
		/// Sort a list of RomData objects by SystemID, SourceID, Game, and Name (in order)
		/// </summary>
		/// <param name="roms">List of RomData objects representing the roms to be sorted</param>
		/// <param name="norename">True if files are not renamed, false otherwise</param>
		public static void RomSort(List<RomData> roms, bool norename)
		{
			roms.Sort(delegate (RomData x, RomData y)
			{
				if (x.SystemID == y.SystemID)
				{
					if (x.SourceID == y.SourceID)
					{
						if (x.Game == y.Game)
						{
							return String.Compare(x.Name, y.Name);
						}
						return String.Compare(x.Game, y.Game);
					}
					return (norename ? String.Compare(x.Game, y.Game) : x.SourceID - y.SourceID);
				}
				return (norename ? String.Compare(x.Game, y.Game) : x.SystemID - y.SystemID);
			});
		}
	}
}
