using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public partial class DatFile
	{
		#region Private instance variables

		// Internal DatHeader values
		private DatHeader _datHeader = new DatHeader();

		// DatItems dictionary
		private SortedDictionary<string, List<DatItem>> _items = new SortedDictionary<string, List<DatItem>>();
		private SortedBy _sortedBy;

		// Internal statistical data
		DatStats _datStats = new DatStats();

		#endregion

		#region Publicly facing variables

		// Data common to most DAT types
		public string FileName
		{
			get { return _datHeader.FileName; }
			set { _datHeader.FileName = value; }
		}
		public string Name
		{
			get { return _datHeader.Name; }
			set { _datHeader.Name = value; }
		}
		public string Description
		{
			get { return _datHeader.Description; }
			set { _datHeader.Description = value; }
		}
		public string RootDir
		{
			get { return _datHeader.RootDir; }
			set { _datHeader.RootDir = value; }
		}
		public string Category
		{
			get { return _datHeader.Category; }
			set { _datHeader.Category = value; }
		}
		public string Version
		{
			get { return _datHeader.Version; }
			set { _datHeader.Version = value; }
		}
		public string Date
		{
			get { return _datHeader.Date; }
			set { _datHeader.Date = value; }
		}
		public string Author
		{
			get { return _datHeader.Author; }
			set { _datHeader.Author = value; }
		}
		public string Email
		{
			get { return _datHeader.Email; }
			set { _datHeader.Email = value; }
		}
		public string Homepage
		{
			get { return _datHeader.Homepage; }
			set { _datHeader.Homepage = value; }
		}
		public string Url
		{
			get { return _datHeader.Url; }
			set { _datHeader.Url = value; }
		}
		public string Comment
		{
			get { return _datHeader.Comment; }
			set { _datHeader.Comment = value; }
		}
		public string Header
		{
			get { return _datHeader.Header; }
			set { _datHeader.Header = value; }
		}
		public string Type // Generally only used for SuperDAT
		{
			get { return _datHeader.Type; }
			set { _datHeader.Type = value; }
		}
		public ForceMerging ForceMerging
		{
			get { return _datHeader.ForceMerging; }
			set { _datHeader.ForceMerging = value; }
		}
		public ForceNodump ForceNodump
		{
			get { return _datHeader.ForceNodump; }
			set { _datHeader.ForceNodump = value; }
		}
		public ForcePacking ForcePacking
		{
			get { return _datHeader.ForcePacking; }
			set { _datHeader.ForcePacking = value; }
		}
		public DatFormat DatFormat
		{
			get { return _datHeader.DatFormat; }
			set { _datHeader.DatFormat = value; }
		}
		public bool ExcludeOf
		{
			get { return _datHeader.ExcludeOf; }
			set { _datHeader.ExcludeOf = value; }
		}
		public DedupeType DedupeRoms
		{
			get { return _datHeader.DedupeRoms; }
			set { _datHeader.DedupeRoms = value; }
		}
		public Hash StripHash
		{
			get { return _datHeader.StripHash; }
			set { _datHeader.StripHash = value; }
		}
		public bool OneGameOneRegion
		{
			get { return _datHeader.OneGameOneRegion; }
			set { _datHeader.OneGameOneRegion = value; }
		}
		public List<string> Regions
		{
			get { return _datHeader.Regions; }
			set { _datHeader.Regions = value; }
		}
		public SortedBy SortedBy
		{
			get { return _sortedBy; }
		}

		// Data specific to the Miss DAT type
		public bool UseGame
		{
			get { return _datHeader.UseGame; }
			set { _datHeader.UseGame = value; }
		}
		public string Prefix
		{
			get { return _datHeader.Prefix; }
			set { _datHeader.Prefix = value; }
		}
		public string Postfix
		{
			get { return _datHeader.Postfix; }
			set { _datHeader.Postfix = value; }
		}
		public bool Quotes
		{
			get { return _datHeader.Quotes; }
			set { _datHeader.Quotes = value; }
		}
		public string RepExt
		{
			get { return _datHeader.RepExt; }
			set { _datHeader.RepExt = value; }
		}
		public string AddExt
		{
			get { return _datHeader.AddExt; }
			set { _datHeader.AddExt = value; }
		}
		public bool RemExt
		{
			get { return _datHeader.RemExt; }
			set { _datHeader.RemExt = value; }
		}
		public bool GameName
		{
			get { return _datHeader.GameName; }
			set { _datHeader.GameName = value; }
		}
		public bool Romba
		{
			get { return _datHeader.Romba; }
			set { _datHeader.Romba = value; }
		}

		// Statistical data related to the DAT
		public long Count
		{
			get { return _datStats.Count; }
			private set { _datStats.Count = value; }
		}
		public long ArchiveCount
		{
			get { return _datStats.ArchiveCount; }
			private set { _datStats.ArchiveCount = value; }
		}
		public long BiosSetCount
		{
			get { return _datStats.BiosSetCount; }
			private set { _datStats.BiosSetCount = value; }
		}
		public long DiskCount
		{
			get { return _datStats.DiskCount; }
			private set { _datStats.DiskCount = value; }
		}
		public long ReleaseCount
		{
			get { return _datStats.ReleaseCount; }
			private set { _datStats.ReleaseCount = value; }
		}
		public long RomCount
		{
			get { return _datStats.RomCount; }
			private set { _datStats.RomCount = value; }
		}
		public long SampleCount
		{
			get { return _datStats.SampleCount; }
			private set { _datStats.SampleCount = value; }
		}
		public long TotalSize
		{
			get { return _datStats.TotalSize; }
			private set { _datStats.TotalSize = value; }
		}
		public long CRCCount
		{
			get { return _datStats.CRCCount; }
			private set { _datStats.CRCCount = value; }
		}
		public long MD5Count
		{
			get { return _datStats.MD5Count; }
			private set { _datStats.MD5Count = value; }
		}
		public long SHA1Count
		{
			get { return _datStats.SHA1Count; }
			private set { _datStats.SHA1Count = value; }
		}
		public long SHA256Count
		{
			get { return _datStats.SHA256Count; }
			private set { _datStats.SHA256Count = value; }
		}
		public long SHA384Count
		{
			get { return _datStats.SHA384Count; }
			private set { _datStats.SHA384Count = value; }
		}
		public long SHA512Count
		{
			get { return _datStats.SHA512Count; }
			private set { _datStats.SHA512Count = value; }
		}
		public long BaddumpCount
		{
			get { return _datStats.BaddumpCount; }
			private set { _datStats.BaddumpCount = value; }
		}
		public long GoodCount
		{
			get { return _datStats.GoodCount; }
			private set { _datStats.GoodCount = value; }
		}
		public long NodumpCount
		{
			get { return _datStats.NodumpCount; }
			private set { _datStats.NodumpCount = value; }
		}
		public long VerifiedCount
		{
			get { return _datStats.VerifiedCount; }
			private set { _datStats.VerifiedCount = value; }
		}

		#endregion

		#region Instance Methods

		#region Accessors

		/// <summary>
		/// Passthrough to access the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to reference</param>
		/// <remarks>We don't want to allow direct setting of values because it bypasses the statistics</remarks>
		public List<DatItem> this[string key]
		{
			get
			{
				// If the dictionary is null, create it
				if (_items == null)
				{
					_items = new SortedDictionary<string, List<DatItem>>();
				}

				lock (_items)
				{
					// If the key is missing from the dictionary, add it
					if (!_items.ContainsKey(key))
					{
						_items.Add(key, new List<DatItem>());
					}

					// Now return the value
					return _items[key];
				}
			}
		}

		/// <summary>
		/// Add a new key to the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to add</param>
		public void Add(string key)
		{
			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}

			lock (_items)
			{
				// If the key is missing from the dictionary, add it
				if (!_items.ContainsKey(key))
				{
					_items.Add(key, new List<DatItem>());
				}
			}
		}

		/// <summary>
		/// Add a value to the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to add to</param>
		/// <param name="value">Value to add to the dictionary</param>
		public void Add(string key, DatItem value)
		{
			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}

			// Add the key, if necessary
			Add(key);

			lock (_items)
			{
				// Now add the value
				_items[key].Add(value);

				// Now update the statistics
				_datStats.AddItem(value);
			}
		}

		/// <summary>
		/// Add a range of values to the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to add to</param>
		/// <param name="value">Value to add to the dictionary</param>
		public void AddRange(string key, List<DatItem> value)
		{
			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}

			// Add the key, if necessary
			Add(key);

			lock (_items)
			{
				// Now add the value
				_items[key].AddRange(value);

				// Now update the statistics
				foreach (DatItem item in value)
				{
					_datStats.AddItem(item);
				}
			}
		}

		/// <summary>
		/// Get if the file dictionary contains the key
		/// </summary>
		/// <param name="key">Key in the dictionary to check</param>
		/// <returns>True if the key exists, false otherwise</returns>
		public bool Contains(string key)
		{
			bool contains = false;

			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}

			// If the key is null, we return false since keys can't be null
			if (key == null)
			{
				return contains;
			}

			lock (_items)
			{
				contains = _items.ContainsKey(key);
			}

			return contains;
		}

		/// <summary>
		/// Get if the file dictionary contains the key and value
		/// </summary>
		/// <param name="key">Key in the dictionary to check</param>
		/// <param name="value">Value in the dictionary to check</param>
		/// <returns>True if the key exists, false otherwise</returns>
		public bool Contains(string key, DatItem value)
		{
			bool contains = false;

			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}

			// If the key is null, we return false since keys can't be null
			if (key == null)
			{
				return contains;
			}

			lock (_items)
			{
				if (_items.ContainsKey(key))
				{
					contains = _items.ContainsKey(key);
				}
			}

			return contains;
		}

		/// <summary>
		/// Get the keys from the file dictionary
		/// </summary>
		/// <returns>IEnumerable of the keys</returns>
		public IEnumerable<string> Keys
		{
			get
			{
				// If the dictionary is null, create it
				if (_items == null)
				{
					_items = new SortedDictionary<string, List<DatItem>>();
				}

				lock (_items)
				{
					return _items.Keys;
				}
			}
		}

		/// <summary>
		/// Remove a key from the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to remove</param>
		public void Remove(string key)
		{
			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}

			// If the key doesn't exist, return
			if (!Contains(key))
			{
				return;
			}

			lock (_items)
			{
				// Remove the statistics first
				foreach (DatItem item in _items[key])
				{
					_datStats.RemoveItem(item);
				}

				// Remove the key from the dictionary
				_items.Remove(key);
			}
		}

		/// <summary>
		/// Remove a value from the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to remove from</param>
		/// <param name="value">Value to remove from the dictionary</param>
		public void Remove(string key, DatItem value)
		{
			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}

			// If the key and value doesn't exist, return
			if (!Contains(key, value))
			{
				return;
			}

			lock (_items)
			{
				// While the key is in the dictionary and the item is there, remove it
				while (_items.ContainsKey(key) && _items[key].Contains(value))
				{
					// Remove the statistics first
					_datStats.RemoveItem(value);

					_items[key].Remove(value);
				}
			}
		}

		/// <summary>
		/// Remove a range of values from the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to remove from</param>
		/// <param name="value">Value to remove from the dictionary</param>
		public void RemoveRange(string key, List<DatItem> value)
		{
			foreach(DatItem item in value)
			{
				Remove(key, item);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		///  Create a new, empty DatFile object
		/// </summary>
		public DatFile()
		{
			_items = new SortedDictionary<string, List<DatItem>>();
		}

		/// <summary>
		/// Create a new DatFile from an existing one using the header values only
		/// </summary>
		/// <param name="df"></param>
		public DatFile(DatFile datFile)
		{
			_datHeader = (DatHeader)datFile._datHeader.Clone();
		}

		#endregion

		#region Dictionary Manipulation

		/// <summary>
		/// Clones the files dictionary
		/// </summary>
		/// <returns>A new files dictionary instance</returns>
		public SortedDictionary<string, List<DatItem>> CloneDictionary()
		{
			// Create the placeholder dictionary to be used
			SortedDictionary<string, List<DatItem>> sorted = new SortedDictionary<string, List<DatItem>>();

			// Now perform a deep clone on the entire dictionary
			List<string> keys = Keys.ToList();
			foreach (string key in keys)
			{
				// Clone each list of DATs in the dictionary
				List<DatItem> olditems = this[key];
				List<DatItem> newitems = new List<DatItem>();
				foreach (DatItem item in olditems)
				{
					newitems.Add((DatItem)item.Clone());
				}

				// If the key is missing from the new dictionary, add it
				if (!sorted.ContainsKey(key))
				{
					sorted.Add(key, new List<DatItem>());
				}

				// Now add the list of items
				sorted[key].AddRange(newitems);
			}

			return sorted;
		}

		/// <summary>
		/// Delete the file dictionary
		/// </summary>
		public void DeleteDictionary()
		{
			_items = null;

			// Reset statistics
			_datStats.Reset();
		}

		/// <summary>
		/// Reset the file dictionary
		/// </summary>
		public void ResetDictionary()
		{
			_items = new SortedDictionary<string, List<DatItem>>();

			// Reset statistics
			_datStats.Reset();
		}

		#endregion

		#endregion // Instance Methods
	}
}
