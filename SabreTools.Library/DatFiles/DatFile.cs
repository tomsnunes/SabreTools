using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.Items;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using StreamWriter = System.IO.StreamWriter;
#endif

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents a format-agnostic DAT
	/// </summary>
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
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.FileName;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.FileName = value;
			}
		}
		public string Name
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Name;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Name = value;
			}
		}
		public string Description
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Description;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Description = value;
			}
		}
		public string RootDir
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.RootDir;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.RootDir = value;
			}
		}
		public string Category
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Category;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Category = value;
			}
		}
		public string Version
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Version;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Version = value;
			}
		}
		public string Date
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Date;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Date = value;
			}
		}
		public string Author
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Author;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Author = value;
			}
		}
		public string Email
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Email;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Email = value;
			}
		}
		public string Homepage
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Homepage;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Homepage = value;
			}
		}
		public string Url
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Url;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Url = value;
			}
		}
		public string Comment
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Comment;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Comment = value;
			}
		}
		public string Header
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Header;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Header = value;
			}
		}
		public string Type // Generally only used for SuperDAT
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Type;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Type = value;
			}
		}
		public ForceMerging ForceMerging
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.ForceMerging;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.ForceMerging = value;
			}
		}
		public ForceNodump ForceNodump
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.ForceNodump;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.ForceNodump = value;
			}
		}
		public ForcePacking ForcePacking
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.ForcePacking;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.ForcePacking = value;
			}
		}
		public DatFormat DatFormat
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.DatFormat;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.DatFormat = value;
			}
		}
		public bool ExcludeOf
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.ExcludeOf;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.ExcludeOf = value;
			}
		}
		public DedupeType DedupeRoms
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.DedupeRoms;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.DedupeRoms = value;
			}
		}
		public Hash StripHash
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.StripHash;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.StripHash = value;
			}
		}
		public bool OneGameOneRegion
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.OneGameOneRegion;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.OneGameOneRegion = value;
			}
		}
		public List<string> Regions
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Regions;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Regions = value;
			}
		}
		public SortedBy SortedBy
		{
			get { return _sortedBy; }
		}

		// Data specific to the Miss DAT type
		public bool UseGame
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.UseGame;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.UseGame = value;
			}
		}
		public string Prefix
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Prefix;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Prefix = value;
			}
		}
		public string Postfix
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Postfix;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Postfix = value;
			}
		}
		public bool Quotes
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Quotes;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Quotes = value;
			}
		}
		public string RepExt
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.RepExt;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.RepExt = value;
			}
		}
		public string AddExt
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.AddExt;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.AddExt = value;
			}
		}
		public bool RemExt
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.RemExt;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.RemExt = value;
			}
		}
		public bool GameName
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.GameName;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.GameName = value;
			}
		}
		public bool Romba
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.Romba;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.Romba = value;
			}
		}

		// Statistical data related to the DAT
		public long Count
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.Count;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.Count = value;
			}
		}
		public long ArchiveCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.ArchiveCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.ArchiveCount = value;
			}
		}
		public long BiosSetCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.BiosSetCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.BiosSetCount = value;
			}
		}
		public long DiskCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.DiskCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.DiskCount = value;
			}
		}
		public long ReleaseCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.ReleaseCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.ReleaseCount = value;
			}
		}
		public long RomCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.RomCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.RomCount = value;
			}
		}
		public long SampleCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.SampleCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.SampleCount = value;
			}
		}
		public long TotalSize
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.TotalSize;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.TotalSize = value;
			}
		}
		public long CRCCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.CRCCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.CRCCount = value;
			}
		}
		public long MD5Count
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.MD5Count;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.MD5Count = value;
			}
		}
		public long SHA1Count
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.SHA1Count;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.SHA1Count = value;
			}
		}
		public long SHA256Count
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.SHA256Count;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.SHA256Count = value;
			}
		}
		public long SHA384Count
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.SHA384Count;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.SHA384Count = value;
			}
		}
		public long SHA512Count
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.SHA512Count;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.SHA512Count = value;
			}
		}
		public long BaddumpCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.BaddumpCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.BaddumpCount = value;
			}
		}
		public long GoodCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.GoodCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.GoodCount = value;
			}
		}
		public long NodumpCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.NodumpCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.NodumpCount = value;
			}
		}
		public long VerifiedCount
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.VerifiedCount;
			}
			private set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.VerifiedCount = value;
			}
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
		/// Remove a key from the file dictionary if it exists
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
		/// Remove a value from the file dictionary if it exists
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
		/// Remove a range of values from the file dictionary if they exists
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

		#region Parsing

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		/// <param name="useTags">True if tags from the DAT should be used to merge the output, false otherwise (default)</param>
		public void Parse(string filename, int sysid, int srcid, bool keep = false, bool clean = false,
			bool remUnicode = false, bool descAsName = false, bool keepext = false, bool useTags = false)
		{
			Parse(filename, sysid, srcid, SplitType.None, keep: keep, clean: clean,
				remUnicode: remUnicode, descAsName: descAsName, keepext: keepext, useTags: useTags);
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		/// <param name="useTags">True if tags from the DAT should be used to merge the output, false otherwise (default)</param>
		public void Parse(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom renaming
			SplitType splitType,

			// Miscellaneous
			bool keep = false,
			bool clean = false,
			bool remUnicode = false,
			bool descAsName = false,
			bool keepext = false,
			bool useTags = false)
		{
			// Check the file extension first as a safeguard
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}
			if (ext != "dat" && ext != "csv" && ext != "md5" && ext != "sfv" && ext != "sha1" && ext != "sha256"
				&& ext != "sha384" && ext != "sha512" && ext != "tsv" && ext != "txt" && ext != "xml")
			{
				return;
			}

			// If the output filename isn't set already, get the internal filename
			FileName = (String.IsNullOrEmpty(FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : FileName);

			// If the output type isn't set already, get the internal output type
			DatFormat = (DatFormat == 0 ? FileTools.GetDatFormat(filename) : DatFormat);

			// Now parse the correct type of DAT
			try
			{
				switch (FileTools.GetDatFormat(filename))
				{
					case DatFormat.AttractMode:
						(this as AttractMode).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.ClrMamePro:
						(this as ClrMamePro).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.CSV:
						(this as SeparatedValue).Parse(filename, sysid, srcid, ',', keep, clean, remUnicode);
						break;
					case DatFormat.DOSCenter:
						(this as DosCenter).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.Listroms:
						(this as Listroms).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.Logiqx:
						(this as Logiqx).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.OfflineList:
						(this as OfflineList).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.RedumpMD5:
						(this as Hashfile).Parse(filename, sysid, srcid, Hash.MD5, clean, remUnicode);
						break;
					case DatFormat.RedumpSFV:
						(this as Hashfile).Parse(filename, sysid, srcid, Hash.CRC, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA1:
						(this as Hashfile).Parse(filename, sysid, srcid, Hash.SHA1, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA256:
						(this as Hashfile).Parse(filename, sysid, srcid, Hash.SHA256, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA384:
						(this as Hashfile).Parse(filename, sysid, srcid, Hash.SHA384, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA512:
						(this as Hashfile).Parse(filename, sysid, srcid, Hash.SHA512, clean, remUnicode);
						break;
					case DatFormat.RomCenter:
						(this as RomCenter).Parse(filename, sysid, srcid, clean, remUnicode);
						break;
					case DatFormat.SabreDat:
						(this as SabreDat).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.SoftwareList:
						(this as SoftwareList).Parse(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.TSV:
						(this as SeparatedValue).Parse(filename, sysid, srcid, '\t', keep, clean, remUnicode);
						break;
					default:
						return;
				}
			}
			catch (Exception ex)
			{
				Globals.Logger.Error("Error with file '{0}': {1}", filename, ex);
			}

			// If we want to use descriptions as names, update everything
			if (descAsName)
			{
				MachineDescriptionToName();
			}

			// If we are using tags from the DAT, set the proper input for split type unless overridden
			if (useTags && splitType == SplitType.None)
			{
				switch (ForceMerging)
				{
					case ForceMerging.None:
						// No-op
						break;
					case ForceMerging.Split:
						splitType = SplitType.Split;
						break;
					case ForceMerging.Merged:
						splitType = SplitType.Merged;
						break;
					case ForceMerging.NonMerged:
						splitType = SplitType.NonMerged;
						break;
					case ForceMerging.Full:
						splitType = SplitType.FullNonMerged;
						break;
				}
			}

			// Now we pre-process the DAT with the splitting/merging mode
			switch (splitType)
			{
				case SplitType.None:
					// No-op
					break;
				case SplitType.DeviceNonMerged:
					CreateDeviceNonMergedSets(DedupeType.None);
					break;
				case SplitType.FullNonMerged:
					CreateFullyNonMergedSets(DedupeType.None);
					break;
				case SplitType.NonMerged:
					CreateNonMergedSets(DedupeType.None);
					break;
				case SplitType.Merged:
					CreateMergedSets(DedupeType.None);
					break;
				case SplitType.Split:
					CreateSplitSets(DedupeType.None);
					break;
			}
		}

		/// <summary>
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="item">Item data to check against</param>
		/// <param name="clean">True if the names should be cleaned to WoD standards, false otherwise</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <returns>The key for the item</returns>
		public string ParseAddHelper(DatItem item, bool clean, bool remUnicode)
		{
			string key = "";

			// If there's no name in the rom, we log and skip it
			if (item.Name == null)
			{
				Globals.Logger.Warning("{0}: Rom with no name found! Skipping...", FileName);
				return key;
			}

			// If the name ends with a directory separator, we log and skip it (DOSCenter only?)
			if (item.Name.EndsWith("/") || item.Name.EndsWith("\\"))
			{
				Globals.Logger.Warning("{0}: Rom ending with directory separator found: '{1}'. Skipping...", FileName, item.Name);
				return key;
			}

			// If we're in cleaning mode, sanitize the game name
			item.MachineName = (clean ? Style.CleanGameName(item.MachineName) : item.MachineName);

			// If we're stripping unicode characters, do so from all relevant things
			if (remUnicode)
			{
				item.Name = Style.RemoveUnicodeCharacters(item.Name);
				item.MachineName = Style.RemoveUnicodeCharacters(item.MachineName);
				item.MachineDescription = Style.RemoveUnicodeCharacters(item.MachineDescription);
			}

			// If we have a Rom or a Disk, clean the hash data
			if (item.Type == ItemType.Rom)
			{
				Rom itemRom = (Rom)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemRom.CRC = Style.CleanHashData(itemRom.CRC, Constants.CRCLength);
				itemRom.MD5 = Style.CleanHashData(itemRom.MD5, Constants.MD5Length);
				itemRom.SHA1 = Style.CleanHashData(itemRom.SHA1, Constants.SHA1Length);
				itemRom.SHA256 = Style.CleanHashData(itemRom.SHA256, Constants.SHA256Length);
				itemRom.SHA384 = Style.CleanHashData(itemRom.SHA384, Constants.SHA384Length);
				itemRom.SHA512 = Style.CleanHashData(itemRom.SHA512, Constants.SHA512Length);

				// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
				if ((itemRom.Size == 0 || itemRom.Size == -1)
					&& ((itemRom.CRC == Constants.CRCZero || String.IsNullOrEmpty(itemRom.CRC))
						|| itemRom.MD5 == Constants.MD5Zero
						|| itemRom.SHA1 == Constants.SHA1Zero
						|| itemRom.SHA256 == Constants.SHA256Zero
						|| itemRom.SHA384 == Constants.SHA384Zero
						|| itemRom.SHA512 == Constants.SHA512Zero))
				{
					// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
					itemRom.Size = Constants.SizeZero;
					itemRom.CRC = Constants.CRCZero;
					itemRom.MD5 = Constants.MD5Zero;
					itemRom.SHA1 = Constants.SHA1Zero;
					itemRom.SHA256 = null;
					itemRom.SHA384 = null;
					itemRom.SHA512 = null;
					//itemRom.SHA256 = Constants.SHA256Zero;
					//itemRom.SHA384 = Constants.SHA384Zero;
					//itemRom.SHA512 = Constants.SHA512Zero;
				}
				// If the file has no size and it's not the above case, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump && (itemRom.Size == 0 || itemRom.Size == -1))
				{
					Globals.Logger.Verbose("{0}: Incomplete entry for '{1}' will be output as nodump", FileName, itemRom.Name);
					itemRom.ItemStatus = ItemStatus.Nodump;
				}
				// If the file has a size but aboslutely no hashes, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump
					&& itemRom.Size > 0
					&& String.IsNullOrEmpty(itemRom.CRC)
					&& String.IsNullOrEmpty(itemRom.MD5)
					&& String.IsNullOrEmpty(itemRom.SHA1)
					&& String.IsNullOrEmpty(itemRom.SHA256)
					&& String.IsNullOrEmpty(itemRom.SHA384)
					&& String.IsNullOrEmpty(itemRom.SHA512))
				{
					Globals.Logger.Verbose("{0}: Incomplete entry for '{1}' will be output as nodump", FileName, itemRom.Name);
					itemRom.ItemStatus = ItemStatus.Nodump;
				}

				item = itemRom;
			}
			else if (item.Type == ItemType.Disk)
			{
				Disk itemDisk = (Disk)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemDisk.MD5 = Style.CleanHashData(itemDisk.MD5, Constants.MD5Length);
				itemDisk.SHA1 = Style.CleanHashData(itemDisk.SHA1, Constants.SHA1Length);
				itemDisk.SHA256 = Style.CleanHashData(itemDisk.SHA256, Constants.SHA256Length);
				itemDisk.SHA384 = Style.CleanHashData(itemDisk.SHA384, Constants.SHA384Length);
				itemDisk.SHA512 = Style.CleanHashData(itemDisk.SHA512, Constants.SHA512Length);

				// If the file has aboslutely no hashes, skip and log
				if (itemDisk.ItemStatus != ItemStatus.Nodump
					&& String.IsNullOrEmpty(itemDisk.MD5)
					&& String.IsNullOrEmpty(itemDisk.SHA1)
					&& String.IsNullOrEmpty(itemDisk.SHA256)
					&& String.IsNullOrEmpty(itemDisk.SHA384)
					&& String.IsNullOrEmpty(itemDisk.SHA512))
				{
					Globals.Logger.Verbose("Incomplete entry for '{0}' will be output as nodump", itemDisk.Name);
					itemDisk.ItemStatus = ItemStatus.Nodump;
				}

				item = itemDisk;
			}

			// Get the key and add statistical data
			switch (item.Type)
			{
				case ItemType.Archive:
				case ItemType.BiosSet:
				case ItemType.Release:
				case ItemType.Sample:
					key = item.Type.ToString();
					break;
				case ItemType.Disk:
					key = ((Disk)item).MD5;
					break;
				case ItemType.Rom:
					key = ((Rom)item).Size + "-" + ((Rom)item).CRC;
					break;
				default:
					key = "default";
					break;
			}

			// Add the item to the DAT
			Add(key, item);

			return key;
		}

		/// <summary>
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="item">Item data to check against</param>
		/// <param name="clean">True if the names should be cleaned to WoD standards, false otherwise</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <returns>The key for the item</returns>
		public async Task<string> ParseAddHelperAsync(DatItem item, bool clean, bool remUnicode)
		{
			return await Task.Run(() => ParseAddHelper(item, clean, remUnicode));
		}

		#endregion

		#region Writing

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datdata">All information for creating the datfile header</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		public bool WriteToFile(string outDir, bool norename = true, bool stats = false, bool ignoreblanks = false, bool overwrite = true)
		{
			// If there's nothing there, abort
			if (Count == 0)
			{
				Globals.Logger.User("There were no items to write out!");
				return false;
			}

			// If output directory is empty, use the current folder
			if (outDir == null || outDir.Trim() == "")
			{
				Globals.Logger.Verbose("No output directory defined, defaulting to curent folder");
				outDir = Environment.CurrentDirectory;
			}

			// Create the output directory if it doesn't already exist
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// If the DAT has no output format, default to XML
			if (DatFormat == 0)
			{
				Globals.Logger.Verbose("No DAT format defined, defaulting to XML");
				DatFormat = DatFormat.Logiqx;
			}

			// Make sure that the three essential fields are filled in
			if (String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				FileName = Name = Description = "Default";
			}
			else if (String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				FileName = Name = Description;
			}
			else if (String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				FileName = Description = Name;
			}
			else if (String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				FileName = Description;
			}
			else if (!String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Name = Description = FileName;
			}
			else if (!String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				Name = Description;
			}
			else if (!String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Description = Name;
			}
			else if (!String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				// Nothing is needed
			}

			// Output initial statistics, for kicks
			if (stats)
			{
				OutputStats(new Dictionary<StatDatFormat, StreamWriter>(), StatDatFormat.None,
					recalculate: (RomCount + DiskCount == 0), baddumpCol: true, nodumpCol: true);
			}

			// Bucket and dedupe according to the flag
			if (DedupeRoms == DedupeType.Full)
			{
				BucketBy(SortedBy.CRC, DedupeRoms, norename: norename);
			}
			else if (DedupeRoms == DedupeType.Game)
			{
				BucketBy(SortedBy.Game, DedupeRoms, norename: norename);
			}

			// Bucket roms by game name, if not already
			BucketBy(SortedBy.Game, DedupeType.None, norename: norename);

			// Output the number of items we're going to be writing
			Globals.Logger.User("A total of {0} items will be written out to '{1}'", Count, FileName);

			// Filter the DAT by 1G1R rules, if we're supposed to
			// TODO: Create 1G1R logic before write

			// If we are removing hashes, do that now
			if (StripHash != 0x0)
			{
				StripHashesFromItems();
			}

			// Get the outfile names
			Dictionary<DatFormat, string> outfiles = Style.CreateOutfileNames(outDir, this, overwrite);

			try
			{
				// Write out all required formats
				Parallel.ForEach(outfiles.Keys, Globals.ParallelOptions, datFormat =>
				{
					string outfile = outfiles[datFormat];
					switch (datFormat)
					{
						case DatFormat.AttractMode:
							(this as AttractMode).WriteToFile(outfile);
							break;
						case DatFormat.ClrMamePro:
							(this as ClrMamePro).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.CSV:
							(this as SeparatedValue).WriteToFile(outfile, ',', ignoreblanks);
							break;
						case DatFormat.DOSCenter:
							(this as DosCenter).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.Listroms:
							(this as Listroms).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.Logiqx:
							(this as Logiqx).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.MissFile:
							(this as Missfile).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.OfflineList:
							(this as OfflineList).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.RedumpMD5:
							(this as Hashfile).WriteToFile(outfile, Hash.MD5, ignoreblanks);
							break;
						case DatFormat.RedumpSFV:
							(this as Hashfile).WriteToFile(outfile, Hash.CRC, ignoreblanks);
							break;
						case DatFormat.RedumpSHA1:
							(this as Hashfile).WriteToFile(outfile, Hash.SHA1, ignoreblanks);
							break;
						case DatFormat.RedumpSHA256:
							(this as Hashfile).WriteToFile(outfile, Hash.SHA256, ignoreblanks);
							break;
						case DatFormat.RedumpSHA384:
							(this as Hashfile).WriteToFile(outfile, Hash.SHA384, ignoreblanks);
							break;
						case DatFormat.RedumpSHA512:
							(this as Hashfile).WriteToFile(outfile, Hash.SHA512, ignoreblanks);
							break;
						case DatFormat.RomCenter:
							(this as RomCenter).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.SabreDat:
							(this as SabreDat).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.SoftwareList:
							(this as SoftwareList).WriteToFile(outfile, ignoreblanks);
							break;
						case DatFormat.TSV:
							(this as SeparatedValue).WriteToFile(outfile, '\t', ignoreblanks);
							break;
					}
				});
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		#endregion

		#endregion // Instance Methods
	}
}
