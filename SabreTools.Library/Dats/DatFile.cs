using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public partial class DatFile
	{
		#region Private instance variables

		// Data common to most DAT types
		private string _fileName;
		private string _name;
		private string _description;
		private string _rootDir;
		private string _category;
		private string _version;
		private string _date;
		private string _author;
		private string _email;
		private string _homepage;
		private string _url;
		private string _comment;
		private string _header;
		private string _type; // Generally only used for SuperDAT
		private ForceMerging _forceMerging;
		private ForceNodump _forceNodump;
		private ForcePacking _forcePacking;
		private DatFormat _datFormat;
		private bool _excludeOf;
		private DedupeType _dedupeRoms;
		private Hash _stripHash;
		private bool _oneGameOneRegion;
		private List<string> _regions = new List<string>();
		private SortedDictionary<string, List<DatItem>> _items = new SortedDictionary<string, List<DatItem>>();
		private SortedBy _sortedBy;

		// Data specific to the Miss DAT type
		private bool _useGame;
		private string _prefix;
		private string _postfix;
		private bool _quotes;
		private string _repExt;
		private string _addExt;
		private bool _remExt;
		private bool _gameName;
		private bool _romba;

		// Statistical data related to the DAT
		private object _statslock = new object();
		private long _count;
		private long _romCount;
		private long _diskCount;
		private long _totalSize;
		private long _crcCount;
		private long _md5Count;
		private long _sha1Count;
		private long _sha256Count;
		private long _sha384Count;
		private long _sha512Count;
		private long _baddumpCount;
		private long _nodumpCount;

		#endregion

		#region Publicly facing variables

		// Data common to most DAT types
		public string FileName
		{
			get { return _fileName; }
			set { _fileName = value; }
		}
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}
		public string RootDir
		{
			get { return _rootDir; }
			set { _rootDir = value; }
		}
		public string Category
		{
			get { return _category; }
			set { _category = value; }
		}
		public string Version
		{
			get { return _version; }
			set { _version = value; }
		}
		public string Date
		{
			get { return _date; }
			set { _date = value; }
		}
		public string Author
		{
			get { return _author; }
			set { _author = value; }
		}
		public string Email
		{
			get { return _email; }
			set { _email = value; }
		}
		public string Homepage
		{
			get { return _homepage; }
			set { _homepage = value; }
		}
		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}
		public string Comment
		{
			get { return _comment; }
			set { _comment = value; }
		}
		public string Header
		{
			get { return _header; }
			set { _header = value; }
		}
		public string Type // Generally only used for SuperDAT
		{
			get { return _type; }
			set { _type = value; }
		}
		public ForceMerging ForceMerging
		{
			get { return _forceMerging; }
			set { _forceMerging = value; }
		}
		public ForceNodump ForceNodump
		{
			get { return _forceNodump; }
			set { _forceNodump = value; }
		}
		public ForcePacking ForcePacking
		{
			get { return _forcePacking; }
			set { _forcePacking = value; }
		}
		public DatFormat DatFormat
		{
			get { return _datFormat; }
			set { _datFormat = value; }
		}
		public bool ExcludeOf
		{
			get { return _excludeOf; }
			set { _excludeOf = value; }
		}
		public DedupeType DedupeRoms
		{
			get { return _dedupeRoms; }
			set { _dedupeRoms = value; }
		}
		public Hash StripHash
		{
			get { return _stripHash; }
			set { _stripHash = value; }
		}
		public bool OneGameOneRegion
		{
			get { return _oneGameOneRegion; }
			set { _oneGameOneRegion = value; }
		}
		public List<string> Regions
		{
			get { return _regions; }
			set { _regions = value; }
		}
		public SortedBy SortedBy
		{
			get { return _sortedBy; }
		}

		// Data specific to the Miss DAT type
		public bool UseGame
		{
			get { return _useGame; }
			set { _useGame = value; }
		}
		public string Prefix
		{
			get { return _prefix; }
			set { _prefix = value; }
		}
		public string Postfix
		{
			get { return _postfix; }
			set { _postfix = value; }
		}
		public bool Quotes
		{
			get { return _quotes; }
			set { _quotes = value; }
		}
		public string RepExt
		{
			get { return _repExt; }
			set { _repExt = value; }
		}
		public string AddExt
		{
			get { return _addExt; }
			set { _addExt = value; }
		}
		public bool RemExt
		{
			get { return _remExt; }
			set { _remExt = value; }
		}
		public bool GameName
		{
			get { return _gameName; }
			set { _gameName = value; }
		}
		public bool Romba
		{
			get { return _romba; }
			set { _romba = value; }
		}

		// Statistical data related to the DAT
		public long Count
		{
			get { return _count; }
		}
		public long RomCount
		{
			get { return _romCount; }
		}
		public long DiskCount
		{
			get { return _diskCount; }
		}
		public long TotalSize
		{
			get { return _totalSize; }
		}
		public long CRCCount
		{
			get { return _crcCount; }
		}
		public long MD5Count
		{
			get { return _md5Count; }
		}
		public long SHA1Count
		{
			get { return _sha1Count; }
		}
		public long SHA256Count
		{
			get { return _sha256Count; }
		}
		public long SHA384Count
		{
			get { return _sha384Count; }
		}
		public long SHA512Count
		{
			get { return _sha512Count; }
		}
		public long BaddumpCount
		{
			get { return _baddumpCount; }
		}
		public long NodumpCount
		{
			get { return _nodumpCount; }
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
				AddItemStatistics(value);
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
					AddItemStatistics(item);
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
		/// Delete the file dictionary
		/// </summary>
		public void Delete()
		{
			_items = null;

			// Reset statistics
			ResetStatistics();
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
					RemoveItemStatistics(item);
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
					RemoveItemStatistics(value);

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

		/// <summary>
		/// Reset the file dictionary
		/// </summary>
		public void Reset()
		{
			_items = new SortedDictionary<string, List<DatItem>>();

			// Reset statistics
			ResetStatistics();
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
		/// Create a new DatFile from an existing one
		/// </summary>
		/// <param name="df"></param>
		public DatFile(DatFile datFile)
		{
			_fileName = datFile.FileName;
			_name = datFile.Name;
			_description = datFile.Description;
			_rootDir = datFile.RootDir;
			_category = datFile.Category;
			_version = datFile.Version;
			_date = datFile.Date;
			_author = datFile.Author;
			_email = datFile.Email;
			_homepage = datFile.Homepage;
			_url = datFile.Url;
			_comment = datFile.Comment;
			_header = datFile.Header;
			_type = datFile.Type;
			_forceMerging = datFile.ForceMerging;
			_forceNodump = datFile.ForceNodump;
			_forcePacking = datFile.ForcePacking;
			_excludeOf = datFile.ExcludeOf;
			_datFormat = datFile.DatFormat;
			_dedupeRoms = datFile.DedupeRoms;
			_stripHash = datFile.StripHash;
			_sortedBy = SortedBy.Default;
			_useGame = datFile.UseGame;
			_prefix = datFile.Prefix;
			_postfix = datFile.Postfix;
			_quotes = datFile.Quotes;
			_repExt = datFile.RepExt;
			_addExt = datFile.AddExt;
			_remExt = datFile.RemExt;
			_gameName = datFile.GameName;
			_romba = datFile.Romba;
		}

		#endregion

		#region Dictionary Manipulation

		/// <summary>
		/// Clones the files dictionary
		/// </summary>
		/// <returns>A new files dictionary instance</returns>
		public SortedDictionary<string, List<DatItem>> CloneFiles()
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

		#endregion

		#endregion // Instance Methods
	}
}
