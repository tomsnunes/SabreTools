using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Instance Methods

		#region Bucketing [MODULAR DONE]

		/// <summary>
		/// Take the arbitrarily sorted Files Dictionary and convert to one sorted by CRC
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void BucketByCRC(bool mergeroms, Logger logger, bool output = true)
		{
			// If we already have the right sorting, trust it
			if (_sortedBy == SortedBy.CRC)
			{
				return;
			}

			// Set the sorted type
			_sortedBy = SortedBy.CRC;

			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (Files == null || Files.Count == 0)
			{
				Files = sortable;
			}

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by CRC");

			// Process each all of the roms
			List<string> keys = Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = Files[key];

				// If we're merging the roms, do so
				if (mergeroms)
				{
					roms = DatItem.Merge(roms, logger);
				}

				// Now add each of the roms to their respective games
				foreach (DatItem rom in roms)
				{
					count++;
					string newkey = (rom.Type == ItemType.Rom ? ((Rom)rom).CRC : Constants.CRCZero);

					if (!sortable.ContainsKey(newkey))
					{
						sortable.Add(newkey, new List<DatItem>());
					}
					sortable[newkey].Add(rom);
				}
			}

			// Now go through and sort all of the lists
			keys = sortable.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> sortedlist = sortable[key];
				DatItem.Sort(ref sortedlist, false);
				sortable[key] = sortedlist;
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			// Now assign the dictionary back
			Files = sortable;
		}

		/// <summary>
		/// Take the arbitrarily sorted Files Dictionary and convert to one sorted by Game
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <param name="lower">True if the game should be lowercased (default), false otherwise</param>
		public void BucketByGame(bool mergeroms, bool norename, Logger logger, bool output = true, bool lower = true)
		{
			// If we already have the right sorting, trust it
			if (_sortedBy == SortedBy.Game)
			{
				return;
			}

			// Set the sorted type
			_sortedBy = SortedBy.Game;

			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (Files == null || Files.Count == 0)
			{
				Files = sortable;
			}

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by game");

			// Process each all of the roms
			List<string> keys = Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = Files[key];

				// If we're merging the roms, do so
				if (mergeroms)
				{
					roms = DatItem.Merge(roms, logger);
				}

				// Now add each of the roms to their respective games
				foreach (DatItem rom in roms)
				{
					count++;
					string newkey = (norename ? ""
							: rom.SystemID.ToString().PadLeft(10, '0')
								+ "-"
								+ rom.SourceID.ToString().PadLeft(10, '0') + "-")
						+ (String.IsNullOrEmpty(rom.Machine.Name)
								? "Default"
								: rom.Machine.Name);
					if (lower)
					{
						newkey = newkey.ToLowerInvariant();
					}

					newkey = HttpUtility.HtmlEncode(newkey);

					if (!sortable.ContainsKey(newkey))
					{
						sortable.Add(newkey, new List<DatItem>());
					}
					sortable[newkey].Add(rom);
				}
			}

			// Now go through and sort all of the lists
			keys = sortable.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> sortedlist = sortable[key];
				DatItem.Sort(ref sortedlist, norename);
				sortable[key] = sortedlist;
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			// Now assign the dictionary back
			Files = sortable;
		}

		/// <summary>
		/// Take the arbitrarily sorted Files Dictionary and convert to one sorted by MD5
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void BucketByMD5(bool mergeroms, Logger logger, bool output = true)
		{
			// If we already have the right sorting, trust it
			if (_sortedBy == SortedBy.MD5)
			{
				return;
			}

			// Set the sorted type
			_sortedBy = SortedBy.MD5;

			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (Files == null || Files.Count == 0)
			{
				Files = sortable;
			}

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by MD5");

			// Process each all of the roms
			List<string> keys = Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = Files[key];

				// If we're merging the roms, do so
				if (mergeroms)
				{
					roms = DatItem.Merge(roms, logger);
				}

				// Now add each of the roms to their respective games
				foreach (DatItem rom in roms)
				{
					count++;
					string newkey = (rom.Type == ItemType.Rom
						? ((Rom)rom).MD5
						: (rom.Type == ItemType.Disk
							? ((Disk)rom).MD5
							: Constants.MD5Zero));

					if (!sortable.ContainsKey(newkey))
					{
						sortable.Add(newkey, new List<DatItem>());
					}
					sortable[newkey].Add(rom);
				}
			}

			// Now go through and sort all of the lists
			keys = sortable.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> sortedlist = sortable[key];
				DatItem.Sort(ref sortedlist, false);
				sortable[key] = sortedlist;
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			// Now assign the dictionary back
			Files = sortable;
		}

		/// <summary>
		/// Take the arbitrarily sorted Files Dictionary and convert to one sorted by SHA1
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void BucketBySHA1(bool mergeroms, Logger logger, bool output = true)
		{
			// If we already have the right sorting, trust it
			if (_sortedBy == SortedBy.SHA1)
			{
				return;
			}

			// Set the sorted type
			_sortedBy = SortedBy.SHA1;

			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (Files == null || Files.Count == 0)
			{
				Files = sortable;
			}

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by SHA-1");

			// Process each all of the roms
			List<string> keys = Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = Files[key];

				// If we're merging the roms, do so
				if (mergeroms)
				{
					roms = DatItem.Merge(roms, logger);
				}

				// Now add each of the roms to their respective games
				foreach (DatItem rom in roms)
				{
					count++;
					string newkey = (rom.Type == ItemType.Rom
						? ((Rom)rom).SHA1
						: (rom.Type == ItemType.Disk
							? ((Disk)rom).SHA1
							: Constants.MD5Zero));

					if (!sortable.ContainsKey(newkey))
					{
						sortable.Add(newkey, new List<DatItem>());
					}
					sortable[newkey].Add(rom);
				}
			}

			// Now go through and sort all of the lists
			keys = sortable.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> sortedlist = sortable[key];
				DatItem.Sort(ref sortedlist, false);
				sortable[key] = sortedlist;
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			// Now assign the dictionary back
			Files = sortable;
		}

		/// <summary>
		/// Take the arbitrarily sorted Files Dictionary and convert to one sorted by Size
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void BucketBySize(bool mergeroms, Logger logger, bool output = true)
		{
			// If we already have the right sorting, trust it
			if (_sortedBy == SortedBy.Size)
			{
				return;
			}

			// Set the sorted type
			_sortedBy = SortedBy.Size;

			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (Files == null || Files.Count == 0)
			{
				Files = sortable;
			}

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by size");

			// Process each all of the roms
			List<string> keys = Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = Files[key];

				// If we're merging the roms, do so
				if (mergeroms)
				{
					roms = DatItem.Merge(roms, logger);
				}

				// Now add each of the roms to their respective games
				foreach (DatItem rom in roms)
				{
					count++;
					string newkey = (rom.Type == ItemType.Rom ? ((Rom)rom).Size.ToString() : "-1");

					if (!sortable.ContainsKey(newkey))
					{
						sortable.Add(newkey, new List<DatItem>());
					}
					sortable[newkey].Add(rom);
				}
			}

			// Now go through and sort all of the lists
			keys = sortable.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> sortedlist = sortable[key];
				DatItem.Sort(ref sortedlist, false);
				sortable[key] = sortedlist;
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			// Now assign the dictionary back
			Files = sortable;
		}

		#endregion

		#endregion // Instance Methods

		#region Static Methods

		#region Bucketing [MODULAR DONE]

		/// <summary>
		/// Take an arbitrarily ordered List and return a Dictionary sorted by Game
		/// </summary>
		/// <param name="list">Input unsorted list</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by game name</returns>
		public static SortedDictionary<string, List<DatItem>> BucketListByGame(List<DatItem> list, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms for output");

			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (list == null || list.Count == 0)
			{
				return sortable;
			}

			// If we're merging the roms, do so
			if (mergeroms)
			{
				list = DatItem.Merge(list, logger);
			}

			// Now add each of the roms to their respective games
			foreach (DatItem rom in list)
			{
				if (rom == null)
				{
					continue;
				}

				count++;
				string newkey = (norename ? ""
						: rom.SystemID.ToString().PadLeft(10, '0')
							+ "-"
							+ rom.SourceID.ToString().PadLeft(10, '0') + "-")
					+ (rom.Machine == null || String.IsNullOrEmpty(rom.Machine.Name)
							? "Default"
							: rom.Machine.Name.ToLowerInvariant());
				newkey = HttpUtility.HtmlEncode(newkey);

				if (!sortable.ContainsKey(newkey))
				{
					sortable.Add(newkey, new List<DatItem>());
				}
				sortable[newkey].Add(rom);
			}

			return sortable;
		}

		#endregion

		#endregion // Static Methods
	}
}
