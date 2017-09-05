﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using SabreTools.Library.Data;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

namespace SabreTools.Library.Dats
{
	public partial class DatFile
	{
		#region Instance Methods

		#region Bucketing

		/// <summary>
		/// Take the arbitrarily sorted Files Dictionary and convert to one sorted by a user-defined method
		/// </summary>
		/// <param name="bucketBy">SortedBy enum representing how to sort the individual items</param>
		/// <param name="deduperoms">Dedupe type that should be used</param>
		/// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		public void BucketBy(SortedBy bucketBy, DedupeType deduperoms, bool lower = true, bool norename = true)
		{
			// If we already have the right sorting, trust it
			if (_sortedBy == bucketBy)
			{
				return;
			}

			// If we have a situation where there's no dictionary or no keys at all, we skip
			if (_files == null || _files.Count == 0)
			{
				return;
			}

			// Set the sorted type
			_sortedBy = bucketBy;

			// Create the temporary dictionary to sort into
			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();

			Globals.Logger.User("Organizing roms by {0}" + (deduperoms != DedupeType.None ? " and merging" : ""), bucketBy);

			// First do the initial sort of all of the roms
			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> roms = this[key];

				// Now add each of the roms to their respective games
				foreach (DatItem rom in roms)
				{
					string newkey = "";

					// We want to get the key most appropriate for the given sorting type
					switch (bucketBy)
					{
						case SortedBy.CRC:
							newkey = (rom.Type == ItemType.Rom ? ((Rom)rom).CRC : Constants.CRCZero);
							break;
						case SortedBy.Game:
							newkey = (norename ? ""
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
							if (newkey == null)
							{
								newkey = "null";
							}

							newkey = HttpUtility.HtmlEncode(newkey);
							break;
						case SortedBy.MD5:
							newkey = (rom.Type == ItemType.Rom
								? ((Rom)rom).MD5
								: (rom.Type == ItemType.Disk
									? ((Disk)rom).MD5
									: Constants.MD5Zero));
							break;
						case SortedBy.SHA1:
							newkey = (rom.Type == ItemType.Rom
								? ((Rom)rom).SHA1
								: (rom.Type == ItemType.Disk
									? ((Disk)rom).SHA1
									: Constants.SHA1Zero));
							break;
						case SortedBy.SHA256:
							newkey = (rom.Type == ItemType.Rom
								? ((Rom)rom).SHA256
								: (rom.Type == ItemType.Disk
									? ((Disk)rom).SHA256
									: Constants.SHA256Zero));
							break;
						case SortedBy.SHA384:
							newkey = (rom.Type == ItemType.Rom
								? ((Rom)rom).SHA384
								: (rom.Type == ItemType.Disk
									? ((Disk)rom).SHA384
									: Constants.SHA384Zero));
							break;
						case SortedBy.SHA512:
							newkey = (rom.Type == ItemType.Rom
								? ((Rom)rom).SHA512
								: (rom.Type == ItemType.Disk
									? ((Disk)rom).SHA512
									: Constants.SHA512Zero));
							break;
					}

					// Double and triple check the key
					if (newkey == null)
					{
						newkey = "";
					}

					// Add the DatItem to the temp dictionary
					lock (sortable)
					{
						if (!sortable.ContainsKey(newkey))
						{
							sortable.Add(newkey, new List<DatItem>());
						}
						sortable[newkey].Add(rom);
					}
				}
			});

			// Now go through and sort all of the individual lists
			keys = sortable.Keys.ToList();
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				// Get the possibly unsorted list
				List<DatItem> sortedlist = sortable[key];

				// Sort the list of items to be consistent
				DatItem.Sort(ref sortedlist, false);

				// If we're merging the roms, do so
				if (deduperoms == DedupeType.Full || (deduperoms == DedupeType.Game && bucketBy == SortedBy.Game))
				{
					sortedlist = DatItem.Merge(sortedlist);
				}

				// Add the list back to the temp dictionary
				lock (sortable)
				{
					sortable[key] = sortedlist;
				}
			});

			// Now assign the dictionary back
			_files = sortable;
		}

		#endregion

		#region Filtering

		/// <summary>
		/// Filter a DAT based on input parameters and modify the items
		/// </summary>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void Filter(Filter filter, bool single, bool trim, string root)
		{
			try
			{
				// Loop over every key in the dictionary
				List<string> keys = Keys.ToList();
				foreach (string key in keys)
				{
					// For every item in the current key
					List<DatItem> items = this[key];
					List<DatItem> newitems = new List<DatItem>();
					foreach (DatItem item in items)
					{
						// If the rom passes the filter, include it
						if (filter.ItemPasses(item))
						{
							// If we are in single game mode, rename all games
							if (single)
							{
								item.Machine.UpdateName("!");
							}

							// If we are in NTFS trim mode, trim the game name
							if (trim)
							{
								// Windows max name length is 260
								int usableLength = 260 - item.Machine.Name.Length - root.Length;
								if (item.Name.Length > usableLength)
								{
									string ext = Path.GetExtension(item.Name);
									item.Name = item.Name.Substring(0, usableLength - ext.Length);
									item.Name += ext;
								}
							}

							// Lock the list and add the item back
							lock (newitems)
							{
								newitems.Add(item);
							}
						}
					}

					Remove(key);
					AddRange(key, newitems);
				}
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
			}
		}

		/// <summary>
		/// Use game descriptions as names in the DAT, updating cloneof/romof/sampleof
		/// </summary>
		public void MachineDescriptionToName()
		{
			try
			{
				// First we want to get a mapping for all games to description
				ConcurrentDictionary<string, string> mapping = new ConcurrentDictionary<string, string>();
				List<string> keys = Keys.ToList();
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					List<DatItem> items = this[key];
					foreach (DatItem item in items)
					{
						// If the key mapping doesn't exist, add it
						if (!mapping.ContainsKey(item.Machine.Name))
						{
							mapping.TryAdd(item.Machine.Name, item.Machine.Description.Replace('/', '_').Replace("\"", "''"));
						}
					}
				});

				// Now we loop through every item and update accordingly
				keys = Keys.ToList();
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					List<DatItem> items = this[key];
					List<DatItem> newItems = new List<DatItem>();
					foreach (DatItem item in items)
					{
						// Update machine name
						if (!String.IsNullOrEmpty(item.Machine.Name) && mapping.ContainsKey(item.Machine.Name))
						{
							item.Machine.UpdateName(mapping[item.Machine.Name]);
						}

						// Update cloneof
						if (!String.IsNullOrEmpty(item.Machine.CloneOf) && mapping.ContainsKey(item.Machine.CloneOf))
						{
							item.Machine.UpdateCloneOf(mapping[item.Machine.CloneOf]);
						}

						// Update romof
						if (!String.IsNullOrEmpty(item.Machine.RomOf) && mapping.ContainsKey(item.Machine.RomOf))
						{
							item.Machine.UpdateRomOf(mapping[item.Machine.RomOf]);
						}

						// Update sampleof
						if (!String.IsNullOrEmpty(item.Machine.SampleOf) && mapping.ContainsKey(item.Machine.SampleOf))
						{
							item.Machine.UpdateSampleOf(mapping[item.Machine.SampleOf]);
						}

						// Add the new item to the output list
						newItems.Add(item);
					}

					// Replace the old list of roms with the new one
					Remove(key);
					AddRange(key, newItems);
				});
			}
			catch (Exception ex)
			{
				Globals.Logger.Warning(ex.ToString());
			}
		}

		/// <summary>
		/// Strip the given hash types from the DAT
		/// </summary>
		public void StripHashesFromItems()
		{
			// Output the logging statement
			Globals.Logger.User("Stripping requested hashes");

			// Now process all of the roms
			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				for (int j = 0; j < items.Count; j++)
				{
					DatItem item = items[j];
					if (item.Type == ItemType.Rom)
					{
						Rom rom = (Rom)item;
						if ((StripHash & Hash.MD5) != 0)
						{
							rom.MD5 = null;
						}
						if ((StripHash & Hash.SHA1) != 0)
						{
							rom.SHA1 = null;
						}
						if ((StripHash & Hash.SHA256) != 0)
						{
							rom.SHA256 = null;
						}
						if ((StripHash & Hash.SHA384) != 0)
						{
							rom.SHA384 = null;
						}
						if ((StripHash & Hash.SHA512) != 0)
						{
							rom.SHA512 = null;
						}

						items[j] = rom;
					}
					else if (item.Type == ItemType.Disk)
					{
						Disk disk = (Disk)item;
						if ((StripHash & Hash.MD5) != 0)
						{
							disk.MD5 = null;
						}
						if ((StripHash & Hash.SHA1) != 0)
						{
							disk.SHA1 = null;
						}
						if ((StripHash & Hash.SHA256) != 0)
						{
							disk.SHA256 = null;
						}
						if ((StripHash & Hash.SHA384) != 0)
						{
							disk.SHA384 = null;
						}
						if ((StripHash & Hash.SHA512) != 0)
						{
							disk.SHA512 = null;
						}

						items[j] = disk;
					}
				}

				Remove(key);
				AddRange(key, items);
			});
		}

		#endregion

		#region Merging/Splitting Methods

		/// <summary>
		/// Use cdevice_ref tags to get full non-merged sets and remove parenting tags
		/// </summary>
		/// <param name="mergeroms">Dedupe type to be used</param>
		public void CreateDeviceNonMergedSets(DedupeType mergeroms)
		{
			Globals.Logger.User("Creating device non-merged sets from the DAT");

			// For sake of ease, the first thing we want to do is sort by game
			BucketBy(SortedBy.Game, mergeroms, norename: true);
			_sortedBy = SortedBy.Default;

			// Now we want to loop through all of the games and set the correct information
			AddRomsFromDevices();

			// Then, remove the romof and cloneof tags so it's not picked up by the manager
			RemoveTagsFromChild();

			// Finally, remove all sets that are labeled as bios or device
			//RemoveBiosAndDeviceSets(logger);
		}

		/// <summary>
		/// Use cloneof tags to create non-merged sets and remove the tags plus using the device_ref tags to get full sets
		/// </summary>
		/// <param name="mergeroms">Dedupe type to be used</param>
		public void CreateFullyNonMergedSets(DedupeType mergeroms)
		{
			Globals.Logger.User("Creating fully non-merged sets from the DAT");

			// For sake of ease, the first thing we want to do is sort by game
			BucketBy(SortedBy.Game, mergeroms, norename: true);
			_sortedBy = SortedBy.Default;

			// Now we want to loop through all of the games and set the correct information
			AddRomsFromDevices();
			AddRomsFromParent();

			// Now that we have looped through the cloneof tags, we loop through the romof tags
			AddRomsFromBios();

			// Then, remove the romof and cloneof tags so it's not picked up by the manager
			RemoveTagsFromChild();

			// Finally, remove all sets that are labeled as bios or device
			//RemoveBiosAndDeviceSets(logger);
		}

		/// <summary>
		/// Use cloneof tags to create merged sets and remove the tags
		/// </summary>
		/// <param name="mergeroms">Dedupe type to be used</param>
		public void CreateMergedSets(DedupeType mergeroms)
		{
			Globals.Logger.User("Creating merged sets from the DAT");

			// For sake of ease, the first thing we want to do is sort by game
			BucketBy(SortedBy.Game, mergeroms, norename: true);
			_sortedBy = SortedBy.Default;

			// Now we want to loop through all of the games and set the correct information
			AddRomsToParent();

			// Now that we have looped through the cloneof tags, we loop through the romof tags
			RemoveBiosRomsFromChild();

			// Finally, remove the romof and cloneof tags so it's not picked up by the manager
			RemoveTagsFromChild();
		}

		/// <summary>
		/// Use cloneof tags to create non-merged sets and remove the tags
		/// </summary>
		/// <param name="mergeroms">Dedupe type to be used</param>
		public void CreateNonMergedSets(DedupeType mergeroms)
		{
			Globals.Logger.User("Creating non-merged sets from the DAT");

			// For sake of ease, the first thing we want to do is sort by game
			BucketBy(SortedBy.Game, mergeroms, norename: true);
			_sortedBy = SortedBy.Default;

			// Now we want to loop through all of the games and set the correct information
			AddRomsFromParent();

			// Now that we have looped through the cloneof tags, we loop through the romof tags
			RemoveBiosRomsFromChild();

			// Finally, remove the romof and cloneof tags so it's not picked up by the manager
			RemoveTagsFromChild();
		}

		/// <summary>
		/// Use cloneof and romof tags to create split sets and remove the tags
		/// </summary>
		/// <param name="mergeroms">Dedupe type to be used</param>
		public void CreateSplitSets(DedupeType mergeroms)
		{
			Globals.Logger.User("Creating split sets from the DAT");

			// For sake of ease, the first thing we want to do is sort by game
			BucketBy(SortedBy.Game, mergeroms, norename: true);
			_sortedBy = SortedBy.Default;

			// Now we want to loop through all of the games and set the correct information
			RemoveRomsFromChild();

			// Now that we have looped through the cloneof tags, we loop through the romof tags
			RemoveBiosRomsFromChild();

			// Finally, remove the romof and cloneof tags so it's not picked up by the manager
			RemoveTagsFromChild();
		}

#endregion

		#region Merging/Splitting Helper Methods

		/// <summary>
		/// Use romof tags to add roms to the children
		/// </summary>
		private void AddRomsFromBios()
		{
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrEmpty(this[game][0].Machine.RomOf))
				{
					parent = this[game][0].Machine.RomOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrEmpty(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we copy the items from the parent to the current game
				Machine currentMachine = this[game][0].Machine;
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					// Figure out the type of the item and add it accordingly
					switch (item.Type)
					{
						case ItemType.Archive:
							Archive archive = ((Archive)item).Clone() as Archive;
							archive.Machine = currentMachine;
							if (this[game].Where(i => i.Name == archive.Name).Count() == 0 && !this[game].Contains(archive))
							{
								this[game].Add(archive);
							}

							break;
						case ItemType.BiosSet:
							BiosSet biosSet = ((BiosSet)item).Clone() as BiosSet;
							biosSet.Machine = currentMachine;
							if (this[game].Where(i => i.Name == biosSet.Name).Count() == 0 && !this[game].Contains(biosSet))
							{
								this[game].Add(biosSet);
							}

							break;
						case ItemType.Disk:
							Disk disk = ((Disk)item).Clone() as Disk;
							disk.Machine = currentMachine;
							if (this[game].Where(i => i.Name == disk.Name).Count() == 0 && !this[game].Contains(disk))
							{
								this[game].Add(disk);
							}

							break;
						case ItemType.Release:
							Release release = ((Release)item).Clone() as Release;
							release.Machine = currentMachine;
							if (this[game].Where(i => i.Name == release.Name).Count() == 0 && !this[game].Contains(release))
							{
								this[game].Add(release);
							}

							break;
						case ItemType.Rom:
							Rom rom = ((Rom)item).Clone() as Rom;
							rom.Machine = currentMachine;
							if (this[game].Where(i => i.Name == rom.Name).Count() == 0 && !this[game].Contains(rom))
							{
								this[game].Add(rom);
							}

							break;
						case ItemType.Sample:
							Sample sample = ((Sample)item).Clone() as Sample;
							sample.Machine = currentMachine;
							if (this[game].Where(i => i.Name == sample.Name).Count() == 0 && !this[game].Contains(sample))
							{
								this[game].Add(sample);
							}

							break;
					}
				}
			}
		}

		/// <summary>
		/// Use device_ref tags to add roms to the children
		/// </summary>
		private void AddRomsFromDevices()
		{
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// If the game has no devices, we continue
				if (this[game][0].Machine.Devices == null || this[game][0].Machine.Devices.Count == 0)
				{
					continue;
				}

				// Determine if the game has any devices or not
				List<string> devices = this[game][0].Machine.Devices;
				foreach (string device in devices)
				{
					// If the device doesn't exist then we continue
					if (this[device].Count == 0)
					{
						continue;
					}

					// Otherwise, copy the items from the device to the current game
					Machine musheen = this[game][0].Machine;
					List<DatItem> devItems = this[device];
					foreach (DatItem item in devItems)
					{
						// Figure out the type of the item and add it accordingly
						switch (item.Type)
						{
							case ItemType.Archive:
								Archive archive = ((Archive)item).Clone() as Archive;
								archive.Machine = musheen;
								if (this[game].Where(i => i.Name == archive.Name).Count() == 0 && !this[game].Contains(archive))
								{
									this[game].Add(archive);
								}

								break;
							case ItemType.BiosSet:
								BiosSet biosSet = ((BiosSet)item).Clone() as BiosSet;
								biosSet.Machine = musheen;
								if (this[game].Where(i => i.Name == biosSet.Name).Count() == 0 && !this[game].Contains(biosSet))
								{
									this[game].Add(biosSet);
								}

								break;
							case ItemType.Disk:
								Disk disk = ((Disk)item).Clone() as Disk;
								disk.Machine = musheen;
								if (this[game].Where(i => i.Name == disk.Name).Count() == 0 && !this[game].Contains(disk))
								{
									this[game].Add(disk);
								}

								break;
							case ItemType.Release:
								Release release = ((Release)item).Clone() as Release;
								release.Machine = musheen;
								if (this[game].Where(i => i.Name == release.Name).Count() == 0 && !this[game].Contains(release))
								{
									this[game].Add(release);
								}

								break;
							case ItemType.Rom:
								Rom rom = ((Rom)item).Clone() as Rom;
								rom.Machine = musheen;
								if (this[game].Where(i => i.Name == rom.Name).Count() == 0 && !this[game].Contains(rom))
								{
									this[game].Add(rom);
								}

								break;
							case ItemType.Sample:
								Sample sample = ((Sample)item).Clone() as Sample;
								sample.Machine = musheen;
								if (this[game].Where(i => i.Name == sample.Name).Count() == 0 && !this[game].Contains(sample))
								{
									this[game].Add(sample);
								}

								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Use cloneof tags to add roms to the children, setting the new romof tag in the process
		/// </summary>
		private void AddRomsFromParent()
		{
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrEmpty(this[game][0].Machine.CloneOf))
				{
					parent = this[game][0].Machine.CloneOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrEmpty(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we copy the items from the parent to the current game
				Machine currentMachine = this[game][0].Machine;
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					// Figure out the type of the item and add it accordingly
					switch (item.Type)
					{
						case ItemType.Archive:
							Archive archive = ((Archive)item).Clone() as Archive;
							archive.Machine = currentMachine;
							if (this[game].Where(i => i.Name == archive.Name).Count() == 0 && !this[game].Contains(archive))
							{
								this[game].Add(archive);
							}

							break;
						case ItemType.BiosSet:
							BiosSet biosSet = ((BiosSet)item).Clone() as BiosSet;
							biosSet.Machine = currentMachine;
							if (this[game].Where(i => i.Name == biosSet.Name).Count() == 0 && !this[game].Contains(biosSet))
							{
								this[game].Add(biosSet);
							}

							break;
						case ItemType.Disk:
							Disk disk = ((Disk)item).Clone() as Disk;
							disk.Machine = currentMachine;
							if (this[game].Where(i => i.Name == disk.Name).Count() == 0 && !this[game].Contains(disk))
							{
								this[game].Add(disk);
							}

							break;
						case ItemType.Release:
							Release release = ((Release)item).Clone() as Release;
							release.Machine = currentMachine;
							if (this[game].Where(i => i.Name == release.Name).Count() == 0 && !this[game].Contains(release))
							{
								this[game].Add(release);
							}

							break;
						case ItemType.Rom:
							Rom rom = ((Rom)item).Clone() as Rom;
							rom.Machine = currentMachine;
							if (this[game].Where(i => i.Name == rom.Name).Count() == 0 && !this[game].Contains(rom))
							{
								this[game].Add(rom);
							}

							break;
						case ItemType.Sample:
							Sample sample = ((Sample)item).Clone() as Sample;
							sample.Machine = currentMachine;
							if (this[game].Where(i => i.Name == sample.Name).Count() == 0 && !this[game].Contains(sample))
							{
								this[game].Add(sample);
							}

							break;
					}
				}

				// Now we want to get the parent romof tag and put it in each of the items
				List<DatItem> items = this[game];
				string romof = this[parent][0].Machine.RomOf;
				foreach (DatItem item in items)
				{
					item.Machine.UpdateRomOf(romof);
				}
			}
		}

		/// <summary>
		/// Use cloneof tags to add roms to the parents, removing the child sets in the process
		/// </summary>
		private void AddRomsToParent()
		{
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrEmpty(this[game][0].Machine.CloneOf))
				{
					parent = this[game][0].Machine.CloneOf;
				}

				// If there is no parent, then we continue
				if (String.IsNullOrEmpty(parent))
				{
					continue;
				}

				// Otherwise, move the items from the current game to a subfolder of the parent game
				Machine parentMachine = this[parent].Count == 0 ? new Machine { Name = parent, Description = parent } : this[parent][0].Machine;
				List<DatItem> items = this[game];
				foreach (DatItem item in items)
				{
					// If the disk doesn't have a valid merge tag OR the merged file doesn't exist in the parent, then add it
					if (item.Type == ItemType.Disk && (item.MergeTag == null || !this[parent].Select(i => i.Name).Contains(item.MergeTag)))
					{
						item.Machine = parentMachine;
						this[parent].Add(item);
					}

					// Otherwise, if the parent doesn't already contain the non-disk, add it
					else if (item.Type != ItemType.Disk && !this[parent].Contains(item))
					{
						// Rename the child so it's in a subfolder
						item.Name = item.Machine.Name + "\\" + item.Name;

						// Update the machine to be the new parent
						item.Machine = parentMachine;

						// Add the rom to the parent set
						this[parent].Add(item);
					}
				}

				// Then, remove the old game so it's not picked up by the writer
				Remove(game);
			}
		}

		/// <summary>
		/// Remove all BIOS and device sets
		/// </summary>
		private void RemoveBiosAndDeviceSets()
		{
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				if (this[game].Count > 0
					&& (this[game][0].Machine.MachineType == MachineType.Bios
						|| this[game][0].Machine.MachineType == MachineType.Device))
				{
					Remove(game);
				}
			}
		}

		/// <summary>
		/// Use romof tags to remove roms from the children
		/// </summary>
		private void RemoveBiosRomsFromChild()
		{
			// Loop through the romof tags
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrEmpty(this[game][0].Machine.RomOf))
				{
					parent = this[game][0].Machine.RomOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrEmpty(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we remove the items that are in the parent from the current game
				Machine currentMachine = this[game][0].Machine;
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					// Figure out the type of the item and add it accordingly
					switch (item.Type)
					{
						case ItemType.Archive:
							Archive archive = ((Archive)item).Clone() as Archive;
							if (this[game].Contains(archive))
							{
								this[game].Remove(archive);
							}

							break;
						case ItemType.BiosSet:
							BiosSet biosSet = ((BiosSet)item).Clone() as BiosSet;
							if (this[game].Contains(biosSet))
							{
								this[game].Remove(biosSet);
							}

							break;
						case ItemType.Disk:
							Disk disk = ((Disk)item).Clone() as Disk;
							if (this[game].Contains(disk))
							{
								this[game].Remove(disk);
							}

							break;
						case ItemType.Release:
							Release release = ((Release)item).Clone() as Release;
							if (this[game].Contains(release))
							{
								this[game].Remove(release);
							}

							break;
						case ItemType.Rom:
							Rom rom = ((Rom)item).Clone() as Rom;
							if (this[game].Contains(rom))
							{
								this[game].Remove(rom);
							}

							break;
						case ItemType.Sample:
							Sample sample = ((Sample)item).Clone() as Sample;
							if (this[game].Contains(sample))
							{
								this[game].Remove(sample);
							}

							break;
					}
				}
			}
		}

		/// <summary>
		/// Use cloneof tags to remove roms from the children
		/// </summary>
		private void RemoveRomsFromChild()
		{
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrEmpty(this[game][0].Machine.CloneOf))
				{
					parent = this[game][0].Machine.CloneOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrEmpty(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we copy the items from the parent to the current game
				Machine currentMachine = this[game][0].Machine;
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					// Figure out the type of the item and remove it accordingly
					switch (item.Type)
					{
						case ItemType.Archive:
							Archive archive = ((Archive)item).Clone() as Archive;
							while (this[game].Contains(archive))
							{
								this[game].Remove(archive);
							}

							break;
						case ItemType.BiosSet:
							BiosSet biosSet = ((BiosSet)item).Clone() as BiosSet;
							while (this[game].Contains(biosSet))
							{
								this[game].Remove(biosSet);
							}

							break;
						case ItemType.Disk:
							Disk disk = ((Disk)item).Clone() as Disk;
							while (this[game].Contains(disk))
							{
								this[game].Remove(disk);
							}

							break;
						case ItemType.Release:
							Release release = ((Release)item).Clone() as Release;
							while (this[game].Contains(release))
							{
								this[game].Remove(release);
							}

							break;
						case ItemType.Rom:
							Rom rom = ((Rom)item).Clone() as Rom;
							while (this[game].Contains(rom))
							{
								this[game].Remove(rom);
							}

							break;
						case ItemType.Sample:
							Sample sample = ((Sample)item).Clone() as Sample;
							while (this[game].Contains(sample))
							{
								this[game].Remove(sample);
							}

							break;
					}
				}

				// Now we want to get the parent romof tag and put it in each of the items
				List<DatItem> items = this[game];
				string romof = this[parent][0].Machine.RomOf;
				foreach (DatItem item in items)
				{
					item.Machine.UpdateRomOf(romof);
				}
			}
		}

		/// <summary>
		/// Remove all romof and cloneof tags from all games
		/// </summary>
		private void RemoveTagsFromChild()
		{
			List<string> games = Keys.ToList();
			foreach (string game in games)
			{
				List<DatItem> items = this[game];
				foreach (DatItem item in items)
				{
					item.Machine.UpdateCloneOf(null);
					item.Machine.UpdateRomOf(null);
				}
			}
		}

#endregion

		#endregion // Instance Methods
	}
}
