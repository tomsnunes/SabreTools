using System;
using System.Collections.Generic;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	public partial class DatFile : ICloneable
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
		private bool _mergeRoms;
		private SortedDictionary<string, List<DatItem>> _files;
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
		private long _romCount;
		private long _diskCount;
		private long _totalSize;
		private long _crcCount;
		private long _md5Count;
		private long _sha1Count;
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
		public bool MergeRoms
		{
			get { return _mergeRoms; }
			set { _mergeRoms = value; }
		}
		public SortedBy SortedBy
		{
			get { return _sortedBy; }
			set { _sortedBy = value; }
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
		public long RomCount
		{
			get { return _romCount; }
			set { _romCount = value; }
		}
		public long DiskCount
		{
			get { return _diskCount; }
			set { _diskCount = value; }
		}
		public long TotalSize
		{
			get { return _totalSize; }
			set { _totalSize = value; }
		}
		public long CRCCount
		{
			get { return _crcCount; }
			set { _crcCount = value; }
		}
		public long MD5Count
		{
			get { return _md5Count; }
			set { _md5Count = value; }
		}
		public long SHA1Count
		{
			get { return _sha1Count; }
			set { _sha1Count = value; }
		}
		public long BaddumpCount
		{
			get { return _baddumpCount; }
			set { _baddumpCount = value; }
		}
		public long NodumpCount
		{
			get { return _nodumpCount; }
			set { _nodumpCount = value; }
		}

		#endregion

		#region Instance Methods

		#region Accessors

		/// <summary>
		/// Passthrough to access the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to reference</param>
		public List<DatItem> this[string key]
		{
			get
			{
				// If the dictionary is null, create it
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}

				// If the key is missing from the dictionary, add it
				if (!_files.ContainsKey(key))
				{
					_files.Add(key, new List<DatItem>());
				}

				// Now return the value
				return _files[key];
			}
			set
			{
				// If the dictionary is null, create it
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}

				// If the key is missing from the dictionary, add it
				if (!_files.ContainsKey(key))
				{
					_files.Add(key, new List<DatItem>());
				}

				// Now set the value
				_files[key] = value;
			}
		}

		/// <summary>
		/// Add a new key to the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to add to</param>
		public void Add(string key)
		{
			lock (_files)
			{
				// If the dictionary is null, create it
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}

				// If the key is missing from the dictionary, add it
				if (!_files.ContainsKey(key))
				{
					_files.Add(key, new List<DatItem>());
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
			lock (_files)
			{
				// If the dictionary is null, create it
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}

				// If the key is missing from the dictionary, add it
				if (!_files.ContainsKey(key))
				{
					_files.Add(key, new List<DatItem>());
				}

				// Now add the value
				_files[key].Add(value);
			}
		}

		/// <summary>
		/// Add a range of values to the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to add to</param>
		/// <param name="value">Value to add to the dictionary</param>
		public void AddRange(string key, List<DatItem> value)
		{
			lock (_files)
			{
				// If the dictionary is null, create it
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}

				// If the key is missing from the dictionary, add it
				if (!_files.ContainsKey(key))
				{
					_files.Add(key, new List<DatItem>());
				}

				// Now add the value
				_files[key].AddRange(value);
			}
		}

		/// <summary>
		/// Get if the file dictionary contains the key
		/// </summary>
		/// <param name="key">Key in the dictionary to check</param>
		/// <returns>True if the key exists, false otherwise</returns>
		public bool ContainsKey(string key)
		{
			lock (_files)
			{
				// If the dictionary is null, create it
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}

				return _files.ContainsKey(key);
			}
		}

		/// <summary>
		/// Get the number of keys in the file dictionary
		/// </summary>
		/// <returns>Number of keys in the file dictionary</returns>
		public long Count
		{
			get
			{
				lock (_files)
				{
					// If the dictionary is null, create it
					if (_files == null)
					{
						_files = new SortedDictionary<string, List<DatItem>>();
					}

					return _files.Count;
				}
			}
		}

		/// <summary>
		/// Delete the file dictionary
		/// </summary>
		public void Delete()
		{
			_files = null;
		}

		/// <summary>
		/// Get the keys from the file dictionary
		/// </summary>
		/// <returns>IEnumerable of the keys</returns>
		public IEnumerable<string> Keys
		{
			get
			{
				lock (_files)
				{
					// If the dictionary is null, create it
					if (_files == null)
					{
						_files = new SortedDictionary<string, List<DatItem>>();
					}

					return _files.Keys;
				}
			}
		}

		/// <summary>
		/// Remove a key from the file dictionary
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			lock (_files)
			{
				// If the dictionary is null, create it
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}

				// If the key is in the dictionary, remove it
				if (_files.ContainsKey(key))
				{
					_files.Remove(key);
				}
			}
		}

		/// <summary>
		/// Reset the file dictionary
		/// </summary>
		public void Reset()
		{
			_files = new SortedDictionary<string, List<DatItem>>();
		}

		/// <summary>
		/// Set a new file dictionary from an existing one
		/// </summary>
		/// <param name="newdict"></param>
		public void Set(SortedDictionary<string, List<DatItem>> newdict)
		{
			_files = newdict;
		}

		#endregion

		#region Cloning Methods [MODULAR DONE]

		public object Clone()
		{
			DatFile df = new DatFile
			{
				FileName = _fileName,
				Name = _name,
				Description = _description,
				RootDir = _rootDir,
				Category = _category,
				Version = _version,
				Date = _date,
				Author = _author,
				Email = _email,
				Homepage = _homepage,
				Url = _url,
				Comment = _comment,
				Header = _header,
				Type = _type,
				ForceMerging = _forceMerging,
				ForceNodump = _forceNodump,
				ForcePacking = _forcePacking,
				ExcludeOf = _excludeOf,
				DatFormat = _datFormat,
				MergeRoms = _mergeRoms,
				SortedBy = _sortedBy,
				UseGame = _useGame,
				Prefix = _prefix,
				Postfix = _postfix,
				Quotes = _quotes,
				RepExt = _repExt,
				AddExt = _addExt,
				RemExt = _remExt,
				GameName = _gameName,
				Romba = _romba,
				RomCount = _romCount,
				DiskCount = _diskCount,
				TotalSize = _totalSize,
				CRCCount = _crcCount,
				MD5Count = _md5Count,
				SHA1Count = _sha1Count,
				BaddumpCount = _baddumpCount,
				NodumpCount = _nodumpCount,
			};

			df.Set(_files);
			return df;
		}

		public object CloneHeader()
		{
			return new DatFile
			{
				FileName = _fileName,
				Name = _name,
				Description = _description,
				RootDir = _rootDir,
				Category = _category,
				Version = _version,
				Date = _date,
				Author = _author,
				Email = _email,
				Homepage = _homepage,
				Url = _url,
				Comment = _comment,
				Header = _header,
				Type = _type,
				ForceMerging = _forceMerging,
				ForceNodump = _forceNodump,
				ForcePacking = _forcePacking,
				ExcludeOf = _excludeOf,
				DatFormat = _datFormat,
				MergeRoms = _mergeRoms,
				SortedBy = SortedBy.Default,
				UseGame = _useGame,
				Prefix = _prefix,
				Postfix = _postfix,
				Quotes = _quotes,
				RepExt = _repExt,
				AddExt = _addExt,
				RemExt = _remExt,
				GameName = _gameName,
				Romba = _romba,
			};
		}

		#endregion

		#endregion // Instance Methods
	}
}
