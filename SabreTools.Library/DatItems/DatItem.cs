using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif
using NaturalSort;

namespace SabreTools.Library.DatItems
{
	/// <summary>
	/// Base class for all items included in a set
	/// </summary>
	public abstract class DatItem : IEquatable<DatItem>, IComparable<DatItem>, ICloneable
	{
		#region Protected instance variables

		// Standard item information
		protected string _name;
		protected ItemType _itemType;
		protected DupeType _dupeType;

		// Machine information
		protected Machine _machine = new Machine();

		// Software list information
		protected bool? _supported;
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
		protected bool _remove;

		#endregion

		#region Publicly facing variables

		// Standard item information
		public string Name
		{
			get { return _name; }
			set { _name = value; }
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
		public string MachineName
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Name;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Name = value;
			}
		}
		public string Comment
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Comment;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Comment = value;
			}
		}
		public string MachineDescription
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Description;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Description = value;
			}
		}
		public string Year
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Year;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Year = value;
			}
		}
		public string Manufacturer
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Manufacturer;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Manufacturer = value;
			}
		}
		public string RomOf
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.RomOf;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.RomOf = value;
			}
		}
		public string CloneOf
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.CloneOf;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.CloneOf = value;
			}
		}
		public string SampleOf
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.SampleOf;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.SampleOf = value;
			}
		}
		public string SourceFile
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.SourceFile;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.SourceFile = value;
			}
		}
		public bool? Runnable
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Runnable;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Runnable = value;
			}
		}
		public string Board
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Board;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Board = value;
			}
		}
		public string RebuildTo
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.RebuildTo;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.RebuildTo = value;
			}
		}
		public List<string> Devices
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.Devices;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.Devices = value;
			}
		}
		public MachineType MachineType
		{
			get
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				return _machine.MachineType;
			}
			set
			{
				if (_machine == null)
				{
					_machine = new Machine();
				}

				_machine.MachineType = value;
			}
		}

		// Software list information
		public bool? Supported // yes = true, partial = null, no = false
		{
			get { return _machine.Supported; }
			set { _machine.Supported = value; }
		}
		public string Publisher
		{
			get { return _machine.Publisher; }
			set { _machine.Publisher = value; }
		}
		public List<Tuple<string, string>> Infos
		{
			get { return _machine.Infos; }
			set { _machine.Infos = value; }
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
		public bool Remove
		{
			get { return _remove; }
			set { _remove = value; }
		}

		#endregion

		#region Instance Methods

		#region Cloning Methods

		/// <summary>
		/// Clone the DatItem
		/// </summary>
		/// <returns>Clone of the DatItem</returns>
		public abstract object Clone();

		/// <summary>
		/// Copy all machine information over in one shot
		/// </summary>
		/// <param name="item">Existing item to copy information from</param>
		public void CopyMachineInformation(DatItem item)
		{
			_machine = (Machine)item._machine.Clone();
		}

		/// <summary>
		/// Copy all machine information over in one shot
		/// </summary>
		/// <param name="machine">Existing machine to copy information from</param>
		public void CopyMachineInformation(Machine machine)
		{
			_machine = (Machine)machine.Clone();
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
				if (lastItem.MachineName == this.MachineName && lastItem.Name == this.Name)
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
				if (lastItem.MachineName == this.MachineName && lastItem.Name == this.Name)
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
			if (!datdata.Contains(key))
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
		/// <param name="remove">True to mark matched roms for removal from the input, false otherwise (default)</param>
		/// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
		/// <returns>List of matched DatItem objects</returns>
		public List<DatItem> GetDuplicates(DatFile datdata, bool remove = false, bool sorted = false)
		{
			List<DatItem> output = new List<DatItem>();

			// Check for an empty rom list first
			if (datdata.Count == 0)
			{
				return output;
			}

			// We want to get the proper key for the DatItem
			string key = SortAndGetKey(datdata, sorted);

			// If the key doesn't exist, return the empty list
			if (!datdata.Contains(key))
			{
				return output;
			}

			// Try to find duplicates
			List<DatItem> roms = datdata[key];
			List<DatItem> left = new List<DatItem>();
			for (int i = 0; i < roms.Count; i++)
			{
				DatItem datItem = roms[i];

				if (this.Equals(datItem))
				{
					datItem.Remove = true;
					output.Add(datItem);
				}
				else
				{
					left.Add(datItem);
				}
			}

			// If we're in removal mode, add back all roms with the proper flags
			if (remove)
			{
				datdata.Remove(key);
				datdata.AddRange(key, output);
				datdata.AddRange(key, left);
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
			// If we're not already sorted, take care of it
			if (!sorted)
			{
				datdata.BucketByBestAvailable();
			}

			// Now that we have the sorted type, we get the proper key
			return Utilities.GetKeyFromDatItem(this, datdata.SortedBy);
		}

		#endregion

		#endregion // Instance Methods

		#region Static Methods

		#region Sorting and Merging

		/// <summary>
		/// Merge an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="infiles">List of File objects representing the roms to be merged</param>
		/// <returns>A List of DatItem objects representing the merged roms</returns>
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
				// If we don't have a Rom or a Disk, we skip checking for duplicates
				if (file.Type != ItemType.Rom && file.Type != ItemType.Disk)
				{
					continue;
				}

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

				// If it's the first non-nodump rom in the list, don't touch it
				if (outfiles.Count == 0
					|| outfiles.Count == outfiles.Count(i =>
						(i.Type == ItemType.Rom && ((Rom)i).ItemStatus == ItemStatus.Nodump)
						|| (i.Type == ItemType.Disk && ((Disk)i).ItemStatus == ItemStatus.Nodump)))
				{
					outfiles.Add(file);
					continue;
				}

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
						saveditem = lastrom;
						pos = i;

						// Roms have more infomration to save
						if (file.Type == ItemType.Rom)
						{
							((Rom)saveditem).Size = ((Rom)saveditem).Size;
							((Rom)saveditem).CRC = (String.IsNullOrWhiteSpace(((Rom)saveditem).CRC) && !String.IsNullOrWhiteSpace(((Rom)file).CRC)
								? ((Rom)file).CRC
								: ((Rom)saveditem).CRC);
							((Rom)saveditem).MD5 = (String.IsNullOrWhiteSpace(((Rom)saveditem).MD5) && !String.IsNullOrWhiteSpace(((Rom)file).MD5)
								? ((Rom)file).MD5
								: ((Rom)saveditem).MD5);
							((Rom)saveditem).SHA1 = (String.IsNullOrWhiteSpace(((Rom)saveditem).SHA1) && !String.IsNullOrWhiteSpace(((Rom)file).SHA1)
								? ((Rom)file).SHA1
								: ((Rom)saveditem).SHA1);
							((Rom)saveditem).SHA256 = (String.IsNullOrWhiteSpace(((Rom)saveditem).SHA256) && !String.IsNullOrWhiteSpace(((Rom)file).SHA256)
								? ((Rom)file).SHA256
								: ((Rom)saveditem).SHA256);
							((Rom)saveditem).SHA384 = (String.IsNullOrWhiteSpace(((Rom)saveditem).SHA384) && !String.IsNullOrWhiteSpace(((Rom)file).SHA384)
								? ((Rom)file).SHA384
								: ((Rom)saveditem).SHA384);
							((Rom)saveditem).SHA512 = (String.IsNullOrWhiteSpace(((Rom)saveditem).SHA512) && !String.IsNullOrWhiteSpace(((Rom)file).SHA512)
								? ((Rom)file).SHA512
								: ((Rom)saveditem).SHA512);
						}
						else if (file.Type == ItemType.Disk)
						{
							((Disk)saveditem).MD5 = (String.IsNullOrWhiteSpace(((Disk)saveditem).MD5) && !String.IsNullOrWhiteSpace(((Disk)file).MD5)
								? ((Disk)file).MD5
								: ((Disk)saveditem).MD5);
							((Disk)saveditem).SHA1 = (String.IsNullOrWhiteSpace(((Disk)saveditem).SHA1) && !String.IsNullOrWhiteSpace(((Disk)file).SHA1)
								? ((Disk)file).SHA1
								: ((Disk)saveditem).SHA1);
							((Disk)saveditem).SHA256 = (String.IsNullOrWhiteSpace(((Disk)saveditem).SHA256) && !String.IsNullOrWhiteSpace(((Disk)file).SHA256)
								? ((Disk)file).SHA256
								: ((Disk)saveditem).SHA256);
							((Disk)saveditem).SHA384 = (String.IsNullOrWhiteSpace(((Disk)saveditem).SHA384) && !String.IsNullOrWhiteSpace(((Disk)file).SHA384)
								? ((Disk)file).SHA384
								: ((Disk)saveditem).SHA384);
							((Disk)saveditem).SHA512 = (String.IsNullOrWhiteSpace(((Disk)saveditem).SHA512) && !String.IsNullOrWhiteSpace(((Disk)file).SHA512)
								? ((Disk)file).SHA512
								: ((Disk)saveditem).SHA512);
						}

						saveditem.Dupe = dupetype;

						// If the current system has a lower ID than the previous, set the system accordingly
						if (file.SystemID < saveditem.SystemID)
						{
							saveditem.SystemID = file.SystemID;
							saveditem.System = file.System;
							saveditem.CopyMachineInformation(file);
							saveditem.Name = file.Name;
						}

						// If the current source has a lower ID than the previous, set the source accordingly
						if (file.SourceID < saveditem.SourceID)
						{
							saveditem.SourceID = file.SourceID;
							saveditem.Source = file.Source;
							saveditem.CopyMachineInformation(file);
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

			// Then return the result
			return outfiles;
		}

		/// <summary>
		/// Resolve name duplicates in an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="infiles">List of File objects representing the roms to be merged</param>
		/// <returns>A List of DatItem objects representing the renamed roms</returns>
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
						disk.Name += "_" + (!String.IsNullOrWhiteSpace(disk.MD5)
							? disk.MD5
							: !String.IsNullOrWhiteSpace(disk.SHA1)
								? disk.SHA1
								: "1");
						datItem = disk;
						lastrenamed = lastrenamed ?? datItem.Name;
					}
					else if (datItem.Type == ItemType.Rom)
					{
						Rom rom = (Rom)datItem;
						rom.Name += "_" + (!String.IsNullOrWhiteSpace(rom.CRC)
							? rom.CRC
							: !String.IsNullOrWhiteSpace(rom.MD5)
								? rom.MD5
								: !String.IsNullOrWhiteSpace(rom.SHA1)
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
							if (x.MachineName == y.MachineName)
							{
								if ((x.Type == ItemType.Rom || x.Type == ItemType.Disk) && (y.Type == ItemType.Rom || y.Type == ItemType.Disk))
								{
									if (Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(x.Name)) == Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(y.Name)))
									{
										return nc.Compare(Path.GetFileName(Utilities.RemovePathUnsafeCharacters(x.Name)), Path.GetFileName(Utilities.RemovePathUnsafeCharacters(y.Name)));
									}
									return nc.Compare(Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(x.Name)), Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(y.Name)));
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
							return nc.Compare(x.MachineName, y.MachineName);
						}
						return (norename ? nc.Compare(x.MachineName, y.MachineName) : x.SourceID - y.SourceID);
					}
					return (norename ? nc.Compare(x.MachineName, y.MachineName) : x.SystemID - y.SystemID);
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
