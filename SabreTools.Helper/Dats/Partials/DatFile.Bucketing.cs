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

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by CRC");

			// Process each all of the roms
			List<string> keys = Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = this[key];

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
			_files = sortable;
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

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by game");

			// Process each all of the roms
			List<string> keys = Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = this[key];

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
			_files = sortable;
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

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by MD5");

			// Process each all of the roms
			List<string> keys = Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = this[key];

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
			_files = sortable;
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

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by SHA-1");

			// Process each all of the roms
			List<string> keys = Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = this[key];

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
			_files = sortable;
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

			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms by size");

			// Process each all of the roms
			List<string> keys = Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = this[key];

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
			_files = sortable;
		}

		/// <summary>
		/// Use cloneof tags to create merged sets and remove the tags plus using the device_ref tags to get full sets
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void CreateFullyMergedSets(bool mergeroms, Logger logger, bool output = true)
		{
			// For sake of ease, the first thing we want to do is sort by game
			BucketByGame(mergeroms, false, logger, output);

			// Now we want to loop through all of the games and set the correct information
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// First, we try to add all device items first
				/*
					Here, we require that device_ref tags got read first, and then we can add them accordingly.
					As of right now, those tags are NOT being read into the machine. We'd have to do this in
					order to get this working correctly. At least this is a good placeholder for now.
				*/

				// Determine if the game has a parent or not
				string parent = null;
				if (this[game][0].Machine.CloneOf != null)
				{
					parent = this[game][0].Machine.CloneOf;
				}
				else if (this[game][0].Machine.RomOf != null)
				{
					parent = this[game][0].Machine.RomOf;
				}

				// If there is no parent, then we continue
				if (parent == null)
				{
					continue;
				}

				// If the parent doesn't exist, then we continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// Otherwise, move the items from the current game to a subfolder of the parent game
				Machine parentMachine = this[parent][0].Machine;
				List<DatItem> items = this[game];
				foreach (DatItem item in items)
				{
					item.Name = item.Machine.Name + "\\" + item.Name;
					item.Machine = parentMachine;
				}

				// Finally, remove the old game so it's not picked up by the writer
				Remove(game);
			}
		}

		/// <summary>
		/// Use cloneof tags to create merged sets and remove the tags
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void CreateMergedSets(bool mergeroms, Logger logger, bool output = true)
		{
			// For sake of ease, the first thing we want to do is sort by game
			BucketByGame(mergeroms, false, logger, output);

			// Now we want to loop through all of the games and set the correct information
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// Determine if the game has a parent or not
				string parent = null;
				if (this[game][0].Machine.CloneOf != null)
				{
					parent = this[game][0].Machine.CloneOf;
				}
				else if (this[game][0].Machine.RomOf != null)
				{
					parent = this[game][0].Machine.RomOf;
				}

				// If there is no parent, then we continue
				if (parent == null)
				{
					continue;
				}

				// If the parent doesn't exist, then we continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// Otherwise, move the items from the current game to a subfolder of the parent game
				Machine parentMachine = this[parent][0].Machine;
				List<DatItem> items = this[game];
				foreach (DatItem item in items)
				{
					item.Name = item.Machine.Name + "\\" + item.Name;
					item.Machine = parentMachine;
				}

				// Finally, remove the old game so it's not picked up by the writer
				Remove(game);
			}
		}

		/// <summary>
		/// Use cloneof tags to create split sets and remove the tags
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void CreateSplitSets(bool mergeroms, Logger logger, bool output = true)
		{
			// For sake of ease, the first thing we want to do is sort by game
			BucketByGame(mergeroms, false, logger, output);

			// Now we want to loop through all of the games and set the correct information
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// Determine if the game has a parent or not
				string parent = null;
				if (this[game][0].Machine.CloneOf != null)
				{
					parent = this[game][0].Machine.CloneOf;
				}
				else if (this[game][0].Machine.RomOf != null)
				{
					parent = this[game][0].Machine.RomOf;
				}

				// If there is no parent, then we continue
				if (parent == null)
				{
					continue;
				}

				// If the parent doesn't exist, then we continue and remove
				if (this[parent].Count == 0)
				{
					List<DatItem> curitems = this[game];
					foreach (DatItem item in curitems)
					{
						item.Machine.CloneOf = null;
						item.Machine.RomOf = null;
					}
					continue;
				}

				// Otherwise, copy the items from the parent to the current game
				Machine currentMachine = this[game][0].Machine;
				List<DatItem> items = this[parent];
				foreach (DatItem item in items)
				{
					// Figure out the type of the item and add it accordingly
					switch (item.Type)
					{
						case ItemType.Archive:
							Archive archive = (Archive)item;
							archive.Machine = currentMachine;
							this[game].Add(archive);
							break;
						case ItemType.BiosSet:
							BiosSet biosSet = (BiosSet)item;
							biosSet.Machine = currentMachine;
							this[game].Add(biosSet);
							break;
						case ItemType.Disk:
							Disk disk = (Disk)item;
							disk.Machine = currentMachine;
							this[game].Add(disk);
							break;
						case ItemType.Release:
							Release release = (Release)item;
							release.Machine = currentMachine;
							this[game].Add(release);
							break;
						case ItemType.Rom:
							Rom rom = (Rom)item;
							rom.Machine = currentMachine;
							this[game].Add(rom);
							break;
						case ItemType.Sample:
							Sample sample = (Sample)item;
							sample.Machine = currentMachine;
							this[game].Add(sample);
							break;
					}
				}

				// Finally, remove the romof and cloneof tags so it's not picked up by the manager
				items = this[game];
				foreach (DatItem item in items)
				{
					item.Machine.CloneOf = null;
					item.Machine.RomOf = null;
				}
			}
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
