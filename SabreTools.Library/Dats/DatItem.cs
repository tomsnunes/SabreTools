using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif
using NaturalSort;

namespace SabreTools.Library.Dats
{
	public abstract class DatItem : IEquatable<DatItem>, IComparable<DatItem>, ICloneable
	{
		#region Protected instance variables

		// Standard item information
		protected string _name;
		private string _merge;
		protected ItemType _itemType;
		protected DupeType _dupeType;

		// Machine information
		protected Machine _machine;

		// Software list information
		protected bool? _supported;
		protected string _publisher;
		protected List<Tuple<string, string>> _infos;
		protected string _partName;
		protected string _partInterface;
		protected List<Tuple<string, string>> _features;
		protected string _areaName;
		protected long? _areaSize;

		// Source metadata information
		protected int _systemId;
		protected string _systemName;
		protected int _sourceId;
		protected string _sourceName;

		#endregion

		#region Publicly facing variables

		// Standard item information
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		public string MergeTag
		{
			get { return _merge; }
			set { _merge = value; }
		}
		public ItemType Type
		{
			get { return _itemType; }
			set { _itemType = value; }
		}
		public DupeType Dupe
		{
			get { return _dupeType; }
			set { _dupeType = value; }
		}

		// Machine information
		public Machine Machine
		{
			get { return _machine; }
			set { _machine = value; }
		}

		// Software list information
		public bool? Supported
		{
			get { return _supported; }
			set { _supported = value; }
		}
		public string Publisher
		{
			get { return _publisher; }
			set { _publisher = value; }
		}
		public List<Tuple<string, string>> Infos
		{
			get { return _infos; }
			set { _infos = value; }
		}
		public string PartName
		{
			get { return _partName; }
			set { _partName = value; }
		}
		public string PartInterface
		{
			get { return _partInterface; }
			set { _partInterface = value; }
		}
		public List<Tuple<string, string>> Features
		{
			get { return _features; }
			set { _features = value; }
		}
		public string AreaName
		{
			get { return _areaName; }
			set { _areaName = value; }
		}
		public long? AreaSize
		{
			get { return _areaSize; }
			set { _areaSize = value; }
		}

		// Source metadata information
		public int SystemID
		{
			get { return _systemId; }
			set { _systemId = value; }
		}
		public string System
		{
			get { return _systemName; }
			set { _systemName = value; }
		}
		public int SourceID
		{
			get { return _sourceId; }
			set { _sourceId = value; }
		}
		public string Source
		{
			get { return _sourceName; }
			set { _sourceName = value; }
		}

		#endregion

		#region Instance Methods

		#region Cloning Methods

		public object Clone()
		{
			switch (_itemType)
			{
				case ItemType.Archive:
					return ((Archive)this).Clone();
				case ItemType.BiosSet:
					return ((BiosSet)this).Clone();
				case ItemType.Disk:
					return ((Disk)this).Clone();
				case ItemType.Release:
					return ((Release)this).Clone();
				case ItemType.Rom:
					return ((Rom)this).Clone();
				case ItemType.Sample:
					return ((Sample)this).Clone();
			}
			
			return null;
		}

		#endregion

		#region Comparision Methods

		public int CompareTo(DatItem other)
		{
			int ret = 0;

			try
			{
				if (_name == other.Name)
				{
					ret = (this.Equals(other) ? 0 : 1);
				}
				ret = String.Compare(_name, other.Name);
			}
			catch
			{
				ret = 1;
			}

			return ret;
		}

		/// <summary>
		/// Determine if an item is a duplicate using partial matching logic
		/// </summary>
		/// <param name="other">DatItem to use as a baseline</param>
		/// <returns>True if the roms are duplicates, false otherwise</returns>
		public abstract bool Equals(DatItem other);

		/// <summary>
		/// Return the duplicate status of two items
		/// </summary>
		/// <param name="lastItem">DatItem to check against</param>
		/// <returns>The DupeType corresponding to the relationship between the two</returns>
		public DupeType GetDuplicateStatus(DatItem lastItem)
		{
			DupeType output = 0x00;

			// If we don't have a duplicate at all, return none
			if (!this.Equals(lastItem))
			{
				return output;
			}

			// If the duplicate is external already or should be, set it
			if ((lastItem.Dupe & DupeType.External) != 0 || lastItem.SystemID != this.SystemID || lastItem.SourceID != this.SourceID)
			{
				if (lastItem.Machine.Name == this.Machine.Name && lastItem.Name == this.Name)
				{
					output = DupeType.External | DupeType.All;
				}
				else
				{
					output = DupeType.External | DupeType.Hash;
				}
			}

			// Otherwise, it's considered an internal dupe
			else
			{
				if (lastItem.Machine.Name == this.Machine.Name && lastItem.Name == this.Name)
				{
					output = DupeType.Internal | DupeType.All;
				}
				else
				{
					output = DupeType.Internal | DupeType.Hash;
				}
			}

			return output;
		}

		#endregion

		#region Sorting and Merging

		/// <summary>
		/// Check if a DAT contains the given rom
		/// </summary>
		/// <param name="datdata">Dat to match against</param>
		/// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
		/// <returns>True if it contains the rom, false otherwise</returns>
		public bool HasDuplicates(DatFile datdata, bool sorted = false)
		{
			// Check for an empty rom list first
			if (datdata.Count == 0)
			{
				return false;
			}

			// We want to get the proper key for the DatItem
			string key = SortAndGetKey(datdata, sorted);

			// If the key doesn't exist, return the empty list
			if (!datdata.ContainsKey(key))
			{
				return false;
			}

			// Try to find duplicates
			List<DatItem> roms = datdata[key];

			foreach (DatItem rom in roms)
			{
				if (this.Equals(rom))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// List all duplicates found in a DAT based on a rom
		/// </summary>
		/// <param name="datdata">Dat to match against</param>
		/// <param name="remove">True to remove matched roms from the input, false otherwise (default)</param>
		/// <returns>List of matched DatItem objects</returns>
		public List<DatItem> GetDuplicates(DatFile datdata, bool remove = false)
		{
			List<DatItem> output = new List<DatItem>();

			// Check for an empty rom list first
			if (datdata.Count == 0)
			{
				return output;
			}

			// We want to get the proper key for the DatItem
			string key = SortAndGetKey(datdata);

			// If the key doesn't exist, return the empty list
			if (!datdata.ContainsKey(key))
			{
				return output;
			}

			// Try to find duplicates
			List<DatItem> roms = datdata[key];
			List<DatItem> left = new List<DatItem>();

			foreach (DatItem rom in roms)
			{
				if (this.Equals(rom))
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
				datdata[key] = left;
			}

			return output;
		}

		/// <summary>
		/// Sort the input DAT and get the key to be used by the item
		/// </summary>
		/// <param name="datdata">Dat to match against</param>
		/// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
		/// <returns>Key to try to use</returns>
		private string SortAndGetKey(DatFile datdata, bool sorted = false)
		{
			string key = null;

			// If we're not already sorted, take care of it
			if (!sorted)
			{
				// If all items are supposed to have a SHA-512, we sort by that
				if (datdata.RomCount + datdata.DiskCount - datdata.NodumpCount == datdata.SHA512Count
					&& ((_itemType == ItemType.Rom && !String.IsNullOrEmpty(((Rom)this).SHA512))
						|| (_itemType == ItemType.Disk && !String.IsNullOrEmpty(((Disk)this).SHA512))))
				{
					datdata.BucketBy(SortedBy.SHA512, DedupeType.None);
				}

				// If all items are supposed to have a SHA-384, we sort by that
				else if (datdata.RomCount + datdata.DiskCount - datdata.NodumpCount == datdata.SHA384Count
					&& ((_itemType == ItemType.Rom && !String.IsNullOrEmpty(((Rom)this).SHA384))
						|| (_itemType == ItemType.Disk && !String.IsNullOrEmpty(((Disk)this).SHA384))))
				{
					datdata.BucketBy(SortedBy.SHA384, DedupeType.None);
				}

				// If all items are supposed to have a SHA-256, we sort by that
				else if (datdata.RomCount + datdata.DiskCount - datdata.NodumpCount == datdata.SHA256Count
					&& ((_itemType == ItemType.Rom && !String.IsNullOrEmpty(((Rom)this).SHA256))
						|| (_itemType == ItemType.Disk && !String.IsNullOrEmpty(((Disk)this).SHA256))))
				{
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).SHA256;
						datdata.BucketBy(SortedBy.SHA256, DedupeType.None);
					}
					else
					{
						key = ((Disk)this).SHA256;
						datdata.BucketBy(SortedBy.SHA256, DedupeType.None);
					}
				}

				// If all items are supposed to have a SHA-1, we sort by that
				else if (datdata.RomCount + datdata.DiskCount - datdata.NodumpCount == datdata.SHA1Count
					&& ((_itemType == ItemType.Rom && !String.IsNullOrEmpty(((Rom)this).SHA1))
						|| (_itemType == ItemType.Disk && !String.IsNullOrEmpty(((Disk)this).SHA1))))
				{
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).SHA1;
						datdata.BucketBy(SortedBy.SHA1, DedupeType.None);
					}
					else
					{
						key = ((Disk)this).SHA1;
						datdata.BucketBy(SortedBy.SHA1, DedupeType.None);
					}
				}

				// If all items are supposed to have an MD5, we sort by that
				else if (datdata.RomCount + datdata.DiskCount - datdata.NodumpCount == datdata.MD5Count
					&& ((_itemType == ItemType.Rom && !String.IsNullOrEmpty(((Rom)this).MD5))
						|| (_itemType == ItemType.Disk && !String.IsNullOrEmpty(((Disk)this).MD5))))
				{
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).MD5;
						datdata.BucketBy(SortedBy.MD5, DedupeType.None);
					}
					else
					{
						key = ((Disk)this).MD5;
						datdata.BucketBy(SortedBy.MD5, DedupeType.None);
					}
				}
			}			

			// Now that we have the sorted type, we get the proper key
			switch (datdata.SortedBy)
			{
				case SortedBy.SHA512:
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).SHA512;
					}
					else if (_itemType == ItemType.Disk)
					{
						key = ((Disk)this).SHA512;
					}
					break;
				case SortedBy.SHA384:
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).SHA384;
					}
					else if (_itemType == ItemType.Disk)
					{
						key = ((Disk)this).SHA384;
					}
					break;
				case SortedBy.SHA256:
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).SHA256;
					}
					else if (_itemType == ItemType.Disk)
					{
						key = ((Disk)this).SHA256;
					}
					break;
				case SortedBy.SHA1:
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).SHA1;
					}
					else if (_itemType == ItemType.Disk)
					{
						key = ((Disk)this).SHA1;
					}
					break;
				case SortedBy.MD5:
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).MD5;
					}
					else if (_itemType == ItemType.Disk)
					{
						key = ((Disk)this).MD5;
					}
					break;
				case SortedBy.CRC:
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).CRC;
					}
					break;
				case SortedBy.Game:
					key = this.Machine.Name;
					break;
				case SortedBy.Size:
					if (_itemType == ItemType.Rom)
					{
						key = ((Rom)this).Size.ToString();
					}
					break;
			}

			// If we got here and the key is still null...
			if (key == null)
			{
				// If we've gotten here and we have a Disk, sort by MD5
				if (_itemType == ItemType.Disk)
				{
					key = ((Disk)this).MD5;
					datdata.BucketBy(SortedBy.MD5, DedupeType.None);
				}

				// If we've gotten here and we have a Rom, sort by CRC
				else if (_itemType == ItemType.Rom)
				{
					key = ((Rom)this).CRC;
					datdata.BucketBy(SortedBy.CRC, DedupeType.None);
				}

				// Otherwise, we use -1 as the key
				else
				{
					key = "-1";
					datdata.BucketBy(SortedBy.Size, DedupeType.None);
				}
			}

			return key;
		}

		#endregion

		#endregion // Instance Methods

		#region Static Methods

		#region Sorting and Merging

		/// <summary>
		/// Merge an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="infiles">List of File objects representing the roms to be merged</param>
		/// <returns>A List of RomData objects representing the merged roms</returns>
		public static List<DatItem> Merge(List<DatItem> infiles)
		{
			// Check for null or blank roms first
			if (infiles == null || infiles.Count == 0)
			{
				return new List<DatItem>();
			}

			// Create output list
			List<DatItem> outfiles = new List<DatItem>();

			// Then deduplicate them by checking to see if data matches previous saved roms
			foreach (DatItem file in infiles)
			{
				// If it's a nodump, add and skip
				if (file.Type == ItemType.Rom && ((Rom)file).ItemStatus == ItemStatus.Nodump)
				{
					outfiles.Add(file);
					continue;
				}
				else if (file.Type == ItemType.Disk && ((Disk)file).ItemStatus == ItemStatus.Nodump)
				{
					outfiles.Add(file);
					continue;
				}

				// If it's the first rom in the list, don't touch it
				if (outfiles.Count != 0)
				{
					// Check if the rom is a duplicate
					DupeType dupetype = 0x00;
					DatItem saveditem = new Rom();
					int pos = -1;
					for (int i = 0; i < outfiles.Count; i++)
					{
						DatItem lastrom = outfiles[i];

						// Get the duplicate status
						dupetype = file.GetDuplicateStatus(lastrom);

						// If it's a duplicate, skip adding it to the output but add any missing information
						if (dupetype != 0x00)
						{
							// If we don't have a rom or disk, then just skip adding
							if (file.Type != ItemType.Rom && file.Type != ItemType.Disk)
							{
								continue;
							}

							saveditem = lastrom;
							pos = i;

							// Roms have more infomration to save
							if (file.Type == ItemType.Rom)
							{
								((Rom)saveditem).Size = ((Rom)saveditem).Size;
								((Rom)saveditem).CRC = (String.IsNullOrEmpty(((Rom)saveditem).CRC) && !String.IsNullOrEmpty(((Rom)file).CRC)
									? ((Rom)file).CRC
									: ((Rom)saveditem).CRC);
								((Rom)saveditem).MD5 = (String.IsNullOrEmpty(((Rom)saveditem).MD5) && !String.IsNullOrEmpty(((Rom)file).MD5)
									? ((Rom)file).MD5
									: ((Rom)saveditem).MD5);
								((Rom)saveditem).SHA1 = (String.IsNullOrEmpty(((Rom)saveditem).SHA1) && !String.IsNullOrEmpty(((Rom)file).SHA1)
									? ((Rom)file).SHA1
									: ((Rom)saveditem).SHA1);
							}
							else
							{
								((Disk)saveditem).MD5 = (String.IsNullOrEmpty(((Disk)saveditem).MD5) && !String.IsNullOrEmpty(((Disk)file).MD5)
									? ((Disk)file).MD5
									: ((Disk)saveditem).MD5);
								((Disk)saveditem).SHA1 = (String.IsNullOrEmpty(((Disk)saveditem).SHA1) && !String.IsNullOrEmpty(((Disk)file).SHA1)
									? ((Disk)file).SHA1
									: ((Disk)saveditem).SHA1);
							}

							saveditem.Dupe = dupetype;

							// If the current system has a lower ID than the previous, set the system accordingly
							if (file.SystemID < saveditem.SystemID)
							{
								saveditem.SystemID = file.SystemID;
								saveditem.System = file.System;
								saveditem.Machine = file.Machine;
								saveditem.Name = file.Name;
							}

							// If the current source has a lower ID than the previous, set the source accordingly
							if (file.SourceID < saveditem.SourceID)
							{
								saveditem.SourceID = file.SourceID;
								saveditem.Source = file.Source;
								saveditem.Machine = file.Machine;
								saveditem.Name = file.Name;
							}

							break;
						}
					}

					// If no duplicate is found, add it to the list
					if (dupetype == 0x00)
					{
						outfiles.Add(file);
					}
					// Otherwise, if a new rom information is found, add that
					else
					{
						outfiles.RemoveAt(pos);
						outfiles.Insert(pos, saveditem);
					}
				}
				else
				{
					outfiles.Add(file);
				}
			}

			// Then return the result
			return outfiles;
		}

		/// <summary>
		/// Resolve name duplicates in an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="infiles">List of File objects representing the roms to be merged</param>
		/// <returns>A List of RomData objects representing the renamed roms</returns>
		public static List<DatItem> ResolveNames(List<DatItem> infiles)
		{
			// Create the output list
			List<DatItem> output = new List<DatItem>();

			// First we want to make sure the list is in alphabetical order
			Sort(ref infiles, true);

			// Now we want to loop through and check names
			DatItem lastItem = null;
			string lastrenamed = null;
			int lastid = 0;
			for (int i = 0; i < infiles.Count; i++)
			{
				DatItem datItem = infiles[i];

				// If we have the first item, we automatically add it
				if (lastItem == null)
				{
					output.Add(datItem);
					lastItem = datItem;
					continue;
				}

				// If the current item exactly matches the last item, then we don't add it
				if ((datItem.GetDuplicateStatus(lastItem) & DupeType.All) != 0)
				{
					Globals.Logger.Verbose("Exact duplicate found for '{0}'", datItem.Name);
					continue;
				}

				// If the current name matches the previous name, rename the current item
				else if (datItem.Name == lastItem.Name)
				{
					Globals.Logger.Verbose("Name duplicate found for '{0}'", datItem.Name);

					if (datItem.Type == ItemType.Disk)
					{
						Disk disk = (Disk)datItem;
						disk.Name += "_" + (!String.IsNullOrEmpty(disk.MD5)
							? disk.MD5
							: !String.IsNullOrEmpty(disk.SHA1)
								? disk.SHA1
								: "1");
						datItem = disk;
						lastrenamed = lastrenamed ?? datItem.Name;
					}
					else if (datItem.Type == ItemType.Rom)
					{
						Rom rom = (Rom)datItem;
						rom.Name += "_" + (!String.IsNullOrEmpty(rom.CRC)
							? rom.CRC
							: !String.IsNullOrEmpty(rom.MD5)
								? rom.MD5
								: !String.IsNullOrEmpty(rom.SHA1)
									? rom.SHA1
									: "1");
						datItem = rom;
						lastrenamed = lastrenamed ?? datItem.Name;
					}

					// If we have a conflict with the last renamed item, do the right thing
					if (datItem.Name == lastrenamed)
					{
						lastrenamed = datItem.Name;
						datItem.Name += (lastid == 0 ? "" : "_" + lastid);
						lastid++;
					}
					// If we have no conflict, then we want to reset the lastrenamed and id
					else
					{
						lastrenamed = null;
						lastid = 0;
					}

					output.Add(datItem);
				}

				// Otherwise, we say that we have a valid named file
				else
				{
					output.Add(datItem);
					lastItem = datItem;
					lastrenamed = null;
					lastid = 0;
				}
			}

			// One last sort to make sure this is ordered
			Sort(ref output, true);

			return output;
		}

		/// <summary>
		/// Sort a list of File objects by SystemID, SourceID, Game, and Name (in order)
		/// </summary>
		/// <param name="roms">List of File objects representing the roms to be sorted</param>
		/// <param name="norename">True if files are not renamed, false otherwise</param>
		/// <returns>True if it sorted correctly, false otherwise</returns>
		public static bool Sort(ref List<DatItem> roms, bool norename)
		{
			roms.Sort(delegate (DatItem x, DatItem y)
			{
				try
				{
					NaturalComparer nc = new NaturalComparer();
					if (x.SystemID == y.SystemID)
					{
						if (x.SourceID == y.SourceID)
						{
							if (x.Machine != null && y.Machine != null && x.Machine.Name == y.Machine.Name)
							{
								if ((x.Type == ItemType.Rom || x.Type == ItemType.Disk) && (y.Type == ItemType.Rom || y.Type == ItemType.Disk))
								{
									if (Path.GetDirectoryName(Style.RemovePathUnsafeCharacters(x.Name)) == Path.GetDirectoryName(Style.RemovePathUnsafeCharacters(y.Name)))
									{
										return nc.Compare(Path.GetFileName(Style.RemovePathUnsafeCharacters(x.Name)), Path.GetFileName(Style.RemovePathUnsafeCharacters(y.Name)));
									}
									return nc.Compare(Path.GetDirectoryName(Style.RemovePathUnsafeCharacters(x.Name)), Path.GetDirectoryName(Style.RemovePathUnsafeCharacters(y.Name)));
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
										return nc.Compare(Path.GetFileName(x.Name), Path.GetFileName(y.Name));
									}
									return nc.Compare(Path.GetDirectoryName(x.Name), Path.GetDirectoryName(y.Name));
								}
							}
							return nc.Compare(x.Machine.Name, y.Machine.Name);
						}
						return (norename ? nc.Compare(x.Machine.Name, y.Machine.Name) : x.SourceID - y.SourceID);
					}
					return (norename ? nc.Compare(x.Machine.Name, y.Machine.Name) : x.SystemID - y.SystemID);
				}
				catch (Exception)
				{
					// Absorb the error
					return 0;
				}
			});
			
			return true;
		}

		#endregion

		#endregion // Static Methods
	}
}
