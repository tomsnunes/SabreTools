using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using SabreTools.Library.DatItems;
using SabreTools.Library.Reports;
using SabreTools.Library.Skippers;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using IOException = System.IO.IOException;
using MemoryStream = System.IO.MemoryStream;
using SearchOption = System.IO.SearchOption;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
#endif
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents a format-agnostic DAT
	/// </summary>
	public class DatFile
	{
		#region Private instance variables

		// Internal DatHeader values
		internal DatHeader _datHeader = new DatHeader();

		// DatItems dictionary
		internal SortedDictionary<string, List<DatItem>> _items = new SortedDictionary<string, List<DatItem>>();
		internal SortedBy _sortedBy;

		// Internal statistical data
		internal DatStats _datStats = new DatStats();

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
		public bool SceneDateStrip
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.SceneDateStrip;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.SceneDateStrip = value;
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
		public string ReplaceExtension
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.ReplaceExtension;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.ReplaceExtension = value;
			}
		}
		public string AddExtension
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.AddExtension;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.AddExtension = value;
			}
		}
		public bool RemoveExtension
		{
			get
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				return _datHeader.RemoveExtension;
			}
			set
			{
				if (_datHeader == null)
				{
					_datHeader = new DatHeader();
				}

				_datHeader.RemoveExtension = value;
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
		public StatReportFormat ReportFormat
		{
			get
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				return _datStats.ReportFormat;
			}
			set
			{
				if (_datStats == null)
				{
					_datStats = new DatStats();
				}

				_datStats.ReportFormat = value;
			}
		}
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
				// Ensure the dictionary is created
				EnsureDictionary();

				lock (_items)
				{
					// Ensure the key exists
					EnsureKey(key);

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
			// Ensure the dictionary is created
			EnsureDictionary();

			lock (_items)
			{
				// Ensure the key exists
				EnsureKey(key);
			}
		}

		/// <summary>
		/// Add a value to the file dictionary
		/// </summary>
		/// <param name="key">Key in the dictionary to add to</param>
		/// <param name="value">Value to add to the dictionary</param>
		public void Add(string key, DatItem value)
		{
			// Ensure the dictionary is created
			EnsureDictionary();

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
			// Ensure the dictionary is created
			EnsureDictionary();

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

			// Ensure the dictionary is created
			EnsureDictionary();

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

			// Ensure the dictionary is created
			EnsureDictionary();

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
		/// <returns>List of the keys</returns>
		public List<string> Keys
		{
			get
			{
				// Ensure the dictionary is created
				EnsureDictionary();

				lock (_items)
				{
					return _items.Keys.Select(item => (String)item.Clone()).ToList();
				}
			}
		}

		/// <summary>
		/// Remove a key from the file dictionary if it exists
		/// </summary>
		/// <param name="key">Key in the dictionary to remove</param>
		public void Remove(string key)
		{
			// Ensure the dictionary is created
			EnsureDictionary();

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
			// Ensure the dictionary is created
			EnsureDictionary();

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
		
		/// <summary>
		/// Ensure the items dictionary
		/// </summary>
		private void EnsureDictionary()
		{
			// If the dictionary is null, create it
			if (_items == null)
			{
				_items = new SortedDictionary<string, List<DatItem>>();
			}
		}

		/// <summary>
		/// Ensure the key exists in the items dictionary
		/// </summary>
		/// <param name="key">Key to ensure</param>
		private void EnsureKey(string key)
		{
			// If the key is missing from the dictionary, add it
			if (!_items.ContainsKey(key))
			{
				_items.Add(key, new List<DatItem>());
			}
		}

		#endregion

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
			// If we have a situation where there's no dictionary or no keys at all, we skip
			if (_items == null || _items.Count == 0)
			{
				return;
			}

			// If the sorted type isn't the same, we want to sort the dictionary accordingly
			if (_sortedBy != bucketBy)
			{
				Globals.Logger.User("Organizing roms by {0}" + (deduperoms != DedupeType.None ? " and merging" : ""), bucketBy);

				// Set the sorted type
				_sortedBy = bucketBy;

				// First do the initial sort of all of the roms inplace
				List<string> oldkeys = Keys;
				Parallel.ForEach(oldkeys, Globals.ParallelOptions, key =>
				{
					// Get the unsorted current list
					List<DatItem> roms = this[key];

					// Now add each of the roms to their respective keys
					for (int i = 0; i < roms.Count; i++)
					{
						DatItem rom = roms[i];

						// We want to get the key most appropriate for the given sorting type
						string newkey = GetKey(rom, bucketBy, lower, norename);

						// If the key is different, move the item to the new key
						if (newkey != key)
						{
							Add(newkey, rom);
							Remove(key, rom);
							i--; // This make sure that the pointer stays on the correct since one was removed
						}
					}
				});
			}

			// Now go through and sort all of the individual lists
			List<string> keys = Keys;
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				// Get the possibly unsorted list
				List<DatItem> sortedlist = this[key];

				// Sort the list of items to be consistent
				DatItem.Sort(ref sortedlist, false);

				// If we're merging the roms, do so
				if (deduperoms == DedupeType.Full || (deduperoms == DedupeType.Game && bucketBy == SortedBy.Game))
				{
					sortedlist = DatItem.Merge(sortedlist);
				}

				// Add the list back to the dictionary
				Remove(key);
				AddRange(key, sortedlist);
			});
		}

		/// <summary>
		/// Get the dictionary key that should be used for a given item and sorting type
		/// </summary>
		/// <param name="item">DatItem to get the key for</param>
		/// <param name="sortedBy">SortedBy enum representing what key to get</param>
		/// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <returns>String representing the key to be used for the DatItem</returns>
		private string GetKey(DatItem item, SortedBy sortedBy, bool lower = true, bool norename = true)
		{
			// Set the output key as the default blank string
			string key = "";

			// Now determine what the key should be based on the sortedBy value
			switch (sortedBy)
			{
				case SortedBy.CRC:
					key = (item.Type == ItemType.Rom ? ((Rom)item).CRC : Constants.CRCZero);
					break;
				case SortedBy.Game:
					key = (norename ? ""
						: item.SystemID.ToString().PadLeft(10, '0')
							+ "-"
							+ item.SourceID.ToString().PadLeft(10, '0') + "-")
					+ (String.IsNullOrWhiteSpace(item.MachineName)
							? "Default"
							: item.MachineName);
					if (lower)
					{
						key = key.ToLowerInvariant();
					}
					if (key == null)
					{
						key = "null";
					}

					key = HttpUtility.HtmlEncode(key);
					break;
				case SortedBy.MD5:
					key = (item.Type == ItemType.Rom
						? ((Rom)item).MD5
						: (item.Type == ItemType.Disk
							? ((Disk)item).MD5
							: Constants.MD5Zero));
					break;
				case SortedBy.SHA1:
					key = (item.Type == ItemType.Rom
						? ((Rom)item).SHA1
						: (item.Type == ItemType.Disk
							? ((Disk)item).SHA1
							: Constants.SHA1Zero));
					break;
				case SortedBy.SHA256:
					key = (item.Type == ItemType.Rom
						? ((Rom)item).SHA256
						: (item.Type == ItemType.Disk
							? ((Disk)item).SHA256
							: Constants.SHA256Zero));
					break;
				case SortedBy.SHA384:
					key = (item.Type == ItemType.Rom
						? ((Rom)item).SHA384
						: (item.Type == ItemType.Disk
							? ((Disk)item).SHA384
							: Constants.SHA384Zero));
					break;
				case SortedBy.SHA512:
					key = (item.Type == ItemType.Rom
						? ((Rom)item).SHA512
						: (item.Type == ItemType.Disk
							? ((Disk)item).SHA512
							: Constants.SHA512Zero));
					break;
			}

			// Double and triple check the key for corner cases
			if (key == null)
			{
				key = "";
			}

			return key;
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

		/// <summary>
		/// Create a new DatFile from an existing DatHeader
		/// </summary>
		/// <param name="datHeader"></param>
		public DatFile(DatHeader datHeader)
		{
			_datHeader = (DatHeader)datHeader.Clone();
		}

		#endregion

		#region Converting and Updating

		/// <summary>
		/// Determine if input files should be merged, diffed, or processed invidually
		/// </summary>
		/// <param name="inputPaths">Names of the input files and/or folders</param>
		/// <param name="basePaths">Names of base files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="updateMode">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="updateHashes">True if hashes should be updated along with names, false otherwise [Only for base replacement]</param>
		public void DetermineUpdateType(List<string> inputPaths, List<string> basePaths, string outDir, UpdateMode updateMode, bool inplace, bool skip,
			bool bare, bool clean, bool remUnicode, bool descAsName, Filter filter, SplitType splitType, bool updateHashes)
		{
			// Ensure we only have files in the inputs
			List<string> inputFileNames = Utilities.GetOnlyFilesFromInputs(inputPaths, appendparent: true);
			List<string> baseFileNames = Utilities.GetOnlyFilesFromInputs(basePaths);

			// If we're in standard update mode, run through all of the inputs
			if (updateMode == UpdateMode.None)
			{
				Update(inputFileNames, outDir, inplace, clean, remUnicode, descAsName, filter, splitType);
				return;
			}

			// Reverse inputs if we're in a required mode
			if ((updateMode & UpdateMode.DiffReverseCascade) != 0)
			{
				inputFileNames.Reverse();
			}
			if ((updateMode & UpdateMode.ReverseBaseReplace) != 0)
			{
				baseFileNames.Reverse();
			}

			// Populate the combined data and get the headers
			List<DatFile> datHeaders = PopulateUserData(inputFileNames, inplace, clean,
				   remUnicode, descAsName, outDir, filter, splitType);

			// If we're in merging mode
			if ((updateMode & UpdateMode.Merge) != 0)
			{
				MergeNoDiff(outDir, inputFileNames, datHeaders);
			}
			// If we have one of the standard diffing modes
			else if ((updateMode & UpdateMode.DiffDupesOnly) != 0
				|| (updateMode & UpdateMode.DiffNoDupesOnly) != 0
				|| (updateMode & UpdateMode.DiffIndividualsOnly) != 0)
			{
				DiffNoCascade(updateMode, outDir, inputFileNames);
			}
			// If we have one of the cascaded diffing modes
			else if ((updateMode & UpdateMode.DiffCascade) != 0
				|| (updateMode & UpdateMode.DiffReverseCascade) != 0)
			{
				DiffCascade(outDir, inplace, inputFileNames, datHeaders, skip);
			}
			// If we have diff against mode
			else if ((updateMode & UpdateMode.DiffAgainst) != 0)
			{
				DiffAgainst(inputFileNames, baseFileNames, outDir, inplace, clean, remUnicode, descAsName, filter, splitType);
			}
			// If we have one of the base replacement modes
			else if ((updateMode & UpdateMode.BaseReplace) != 0
				|| (updateMode & UpdateMode.ReverseBaseReplace) != 0)
			{
				BaseReplace(inputFileNames, baseFileNames, outDir, inplace, clean, remUnicode, descAsName, filter, splitType, updateHashes);
			}

			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="inputs">Paths to DATs to parse</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <returns>List of DatData objects representing headers</returns>
		private List<DatFile> PopulateUserData(List<string> inputs, bool inplace, bool clean, bool remUnicode, bool descAsName,
			string outDir, Filter filter, SplitType splitType)
		{
			DatFile[] datHeaders = new DatFile[inputs.Count];
			InternalStopwatch watch = new InternalStopwatch("Processing individual DATs");

			// Parse all of the DATs into their own DatFiles in the array
			Parallel.For(0, inputs.Count, Globals.ParallelOptions, i =>
			{
				string input = inputs[i];
				Globals.Logger.User("Adding DAT: {0}", input.Split('¬')[0]);
				datHeaders[i] = new DatFile
				{
					DatFormat = (DatFormat != 0 ? DatFormat : 0),
					DedupeRoms = DedupeRoms,
				};

				datHeaders[i].Parse(input, i, 0, splitType, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);
			});

			watch.Stop();

			watch.Start("Populating internal DAT");
			Parallel.For(0, inputs.Count, Globals.ParallelOptions, i =>
			{
				// Get the list of keys from the DAT
				List<string> keys = datHeaders[i].Keys;
				foreach (string key in keys)
				{
					// Add everything from the key to the internal DAT
					AddRange(key, datHeaders[i][key]);

					// Now remove the key from the source DAT
					datHeaders[i].Remove(key);
				}

				// Now remove the file dictionary from the souce DAT to save memory
				datHeaders[i].DeleteDictionary();
			});

			// Now that we have a merged DAT, filter it
			filter.FilterDatFile(this);

			watch.Stop();

			return datHeaders.ToList();
		}

		/// <summary>
		/// Replace item names from on a base set
		/// </summary>
		/// <param name="inputFileNames">Names of the input files</param>
		/// <param name="baseFileNames">Names of base files</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="updateHashes">True if hashes should be updated along with names, false otherwise</param>
		public void BaseReplace(List<string> inputFileNames, List<string> baseFileNames, string outDir, bool inplace, bool clean, bool remUnicode,
			bool descAsName, Filter filter, SplitType splitType, bool updateHashes)
		{
			// First we want to parse all of the base DATs into the input
			InternalStopwatch watch = new InternalStopwatch("Populating base DAT for replacement...");

			Parallel.For(0, baseFileNames.Count, Globals.ParallelOptions, i =>
			{
				string path = "";
				int id = 0;

				lock (baseFileNames)
				{
					path = baseFileNames[i];
					id = baseFileNames.Count - i;
				}

				Parse(path, id, id, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);
			});

			watch.Stop();

			// For comparison's sake, we want to use CRC as the base ordering
			BucketBy(SortedBy.CRC, DedupeType.Full);

			// Now we want to try to replace each item in each input DAT from the base
			foreach (string path in inputFileNames)
			{
				Globals.Logger.User("Replacing items in '{0}'' from the base DAT", path.Split('¬')[0]);

				// First we parse in the DAT internally
				DatFile intDat = new DatFile();
				intDat.Parse(path, 1, 1, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);

				// For comparison's sake, we want to use CRC as the base ordering
				intDat.BucketBy(SortedBy.CRC, DedupeType.Full);

				// Then we do a hashwise comparison against the base DAT
				List<string> keys = intDat.Keys;
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					List<DatItem> datItems = intDat[key];
					List<DatItem> newDatItems = new List<DatItem>();
					foreach (DatItem datItem in datItems)
					{
						List<DatItem> dupes = datItem.GetDuplicates(this, sorted: true);
						DatItem newDatItem = (DatItem)datItem.Clone();

						if (dupes.Count > 0)
						{
							newDatItem.Name = dupes[0].Name;

							// If we're updating hashes too, only replace if the current item doesn't have them
							if (updateHashes)
							{
								if (newDatItem.Type == ItemType.Rom)
								{
									Rom newRomItem = (Rom)newDatItem;
									if (String.IsNullOrEmpty(newRomItem.CRC) && !String.IsNullOrEmpty(((Rom)dupes[0]).CRC))
									{
										newRomItem.CRC = ((Rom)dupes[0]).CRC;
									}
									if (String.IsNullOrEmpty(newRomItem.MD5) && !String.IsNullOrEmpty(((Rom)dupes[0]).MD5))
									{
										newRomItem.MD5 = ((Rom)dupes[0]).MD5;
									}
									if (String.IsNullOrEmpty(newRomItem.SHA1) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA1))
									{
										newRomItem.SHA1 = ((Rom)dupes[0]).SHA1;
									}
									if (String.IsNullOrEmpty(newRomItem.SHA256) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA256))
									{
										newRomItem.SHA256 = ((Rom)dupes[0]).SHA256;
									}
									if (String.IsNullOrEmpty(newRomItem.SHA384) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA384))
									{
										newRomItem.SHA384 = ((Rom)dupes[0]).SHA384;
									}
									if (String.IsNullOrEmpty(newRomItem.SHA512) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA512))
									{
										newRomItem.SHA512 = ((Rom)dupes[0]).SHA512;
									}

									newDatItem = (Rom)newRomItem.Clone();
								}
								else if (newDatItem.Type == ItemType.Disk)
								{
									Disk newDiskItem = (Disk)newDatItem;
									if (String.IsNullOrEmpty(newDiskItem.MD5) && !String.IsNullOrEmpty(((Rom)dupes[0]).MD5))
									{
										newDiskItem.MD5 = ((Rom)dupes[0]).MD5;
									}
									if (String.IsNullOrEmpty(newDiskItem.SHA1) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA1))
									{
										newDiskItem.SHA1 = ((Rom)dupes[0]).SHA1;
									}
									if (String.IsNullOrEmpty(newDiskItem.SHA256) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA256))
									{
										newDiskItem.SHA256 = ((Rom)dupes[0]).SHA256;
									}
									if (String.IsNullOrEmpty(newDiskItem.SHA384) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA384))
									{
										newDiskItem.SHA384 = ((Rom)dupes[0]).SHA384;
									}
									if (String.IsNullOrEmpty(newDiskItem.SHA512) && !String.IsNullOrEmpty(((Rom)dupes[0]).SHA512))
									{
										newDiskItem.SHA512 = ((Rom)dupes[0]).SHA512;
									}

									newDatItem = (Disk)newDiskItem.Clone();
								}
							}
						}

						newDatItems.Add(newDatItem);
					}

					// Now add the new list to the key
					intDat.Remove(key);
					intDat.AddRange(key, newDatItems);
				});

				// Determine the output path for the DAT
				string interOutDir = Utilities.GetOutputPath(outDir, path, inplace);

				// Once we're done, try writing out
				intDat.Write(interOutDir);

				// Due to possible memory requirements, we force a garbage collection
				GC.Collect();
			}
		}

		/// <summary>
		/// Output diffs against a base set
		/// </summary>
		/// <param name="inputFileNames">Names of the input files</param>
		/// <param name="baseFileNames">Names of base files</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void DiffAgainst(List<string> inputFileNames, List<string> baseFileNames, string outDir, bool inplace, bool clean, bool remUnicode,
			bool descAsName, Filter filter, SplitType splitType)
		{
			// First we want to parse all of the base DATs into the input
			InternalStopwatch watch = new InternalStopwatch("Populating base DAT for comparison...");

			Parallel.ForEach(baseFileNames, Globals.ParallelOptions, path =>
			{
				Parse(path, 0, 0, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);
			});

			watch.Stop();

			// For comparison's sake, we want to use CRC as the base ordering
			BucketBy(SortedBy.CRC, DedupeType.Full);

			// Now we want to compare each input DAT against the base
			foreach (string path in inputFileNames)
			{
				Globals.Logger.User("Comparing '{0}'' to base DAT", path.Split('¬')[0]);

				// First we parse in the DAT internally
				DatFile intDat = new DatFile();
				intDat.Parse(path, 1, 1, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);

				// For comparison's sake, we want to use CRC as the base ordering
				intDat.BucketBy(SortedBy.CRC, DedupeType.Full);

				// Then we do a hashwise comparison against the base DAT
				List<string> keys = intDat.Keys;
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					List<DatItem> datItems = intDat[key];
					List<DatItem> keepDatItems = new List<DatItem>();
					foreach (DatItem datItem in datItems)
					{
						if (!datItem.HasDuplicates(this, true))
						{
							keepDatItems.Add(datItem);
						}
					}

					// Now add the new list to the key
					intDat.Remove(key);
					intDat.AddRange(key, keepDatItems);
				});

				// Determine the output path for the DAT
				string interOutDir = Utilities.GetOutputPath(outDir, path, inplace);

				// Once we're done, try writing out
				intDat.Write(interOutDir);

				// Due to possible memory requirements, we force a garbage collection
				GC.Collect();
			}
		}

		/// <summary>
		/// Output cascading diffs
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		public void DiffCascade(string outDir, bool inplace, List<string> inputs, List<DatFile> datHeaders, bool skip)
		{
			string post = "";

			// Create a list of DatData objects representing output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			InternalStopwatch watch = new InternalStopwatch("Initializing all output DATs");

			DatFile[] outDatsArray = new DatFile[inputs.Count];

			Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
			{
				string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				DatFile diffData;

				// If we're in inplace mode, take the appropriate DatData object already stored
				if (inplace || outDir != Environment.CurrentDirectory)
				{
					diffData = datHeaders[j];
				}
				else
				{
					diffData = new DatFile(this);
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
				}
				diffData.ResetDictionary();

				outDatsArray[j] = diffData;
			});

			outDats = outDatsArray.ToList();
			watch.Stop();

			// Now, loop through the dictionary and populate the correct DATs
			watch.Start("Populating all output DATs");
			List<string> keys = Keys;

			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = DatItem.Merge(this[key]);

				// If the rom list is empty or null, just skip it
				if (items == null || items.Count == 0)
				{
					return;
				}

				foreach (DatItem item in items)
				{
					// There's odd cases where there are items with System ID < 0. Skip them for now
					if (item.SystemID < 0)
					{
						Globals.Logger.Warning("Item found with a <0 SystemID: {0}", item.Name);
						continue;
					}

					outDats[item.SystemID].Add(key, item);
				}
			});

			watch.Stop();

			// Finally, loop through and output each of the DATs
			watch.Start("Outputting all created DATs");

			Parallel.For((skip ? 1 : 0), inputs.Count, Globals.ParallelOptions, j =>
			{
				string path = Utilities.GetOutputPath(outDir, inputs[j], inplace);

				// Try to output the file
				outDats[j].Write(path);
			});

			watch.Stop();
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		public void DiffNoCascade(UpdateMode diff, string outDir, List<string> inputs)
		{
			InternalStopwatch watch = new InternalStopwatch("Initializing all output DATs");

			// Default vars for use
			string post = "";
			DatFile outerDiffData = new DatFile();
			DatFile dupeData = new DatFile();

			// Fill in any information not in the base DAT
			if (String.IsNullOrWhiteSpace(FileName))
			{
				FileName = "All DATs";
			}
			if (String.IsNullOrWhiteSpace(Name))
			{
				Name = "All DATs";
			}
			if (String.IsNullOrWhiteSpace(Description))
			{
				Description = "All DATs";
			}

			// Don't have External dupes
			if ((diff & UpdateMode.DiffNoDupesOnly) != 0)
			{
				post = " (No Duplicates)";
				outerDiffData = new DatFile(this);
				outerDiffData.FileName += post;
				outerDiffData.Name += post;
				outerDiffData.Description += post;
				outerDiffData.ResetDictionary();
			}

			// Have External dupes
			if ((diff & UpdateMode.DiffDupesOnly) != 0)
			{
				post = " (Duplicates)";
				dupeData = new DatFile(this);
				dupeData.FileName += post;
				dupeData.Name += post;
				dupeData.Description += post;
				dupeData.ResetDictionary();
			}

			// Create a list of DatData objects representing individual output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			if ((diff & UpdateMode.DiffIndividualsOnly) != 0)
			{
				DatFile[] outDatsArray = new DatFile[inputs.Count];

				Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
				{
					string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
					DatFile diffData = new DatFile(this);
					diffData.FileName += innerpost;
					diffData.Name += innerpost;
					diffData.Description += innerpost;
					diffData.ResetDictionary();
					outDatsArray[j] = diffData;
				});

				outDats = outDatsArray.ToList();
			}

			watch.Stop();

			// Now, loop through the dictionary and populate the correct DATs
			watch.Start("Populating all output DATs");

			List<string> keys = Keys;
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = DatItem.Merge(this[key]);

				// If the rom list is empty or null, just skip it
				if (items == null || items.Count == 0)
				{
					return;
				}

				// Loop through and add the items correctly
				foreach (DatItem item in items)
				{
					// No duplicates
					if ((diff & UpdateMode.DiffNoDupesOnly) != 0 || (diff & UpdateMode.DiffIndividualsOnly) != 0)
					{
						if ((item.Dupe & DupeType.Internal) != 0)
						{
							// Individual DATs that are output
							if ((diff & UpdateMode.DiffIndividualsOnly) != 0)
							{
								outDats[item.SystemID].Add(key, item);
							}

							// Merged no-duplicates DAT
							if ((diff & UpdateMode.DiffNoDupesOnly) != 0)
							{
								DatItem newrom = item.Clone() as DatItem;
								newrom.MachineName += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

								outerDiffData.Add(key, newrom);
							}
						}
					}

					// Duplicates only
					if ((diff & UpdateMode.DiffDupesOnly) != 0)
					{
						if ((item.Dupe & DupeType.External) != 0)
						{
							DatItem newrom = item.Clone() as DatItem;
							newrom.MachineName += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

							dupeData.Add(key, newrom);
						}
					}
				}
			});

			watch.Stop();

			// Finally, loop through and output each of the DATs
			watch.Start("Outputting all created DATs");

			// Output the difflist (a-b)+(b-a) diff
			if ((diff & UpdateMode.DiffNoDupesOnly) != 0)
			{
				outerDiffData.Write(outDir);
			}

			// Output the (ab) diff
			if ((diff & UpdateMode.DiffDupesOnly) != 0)
			{
				dupeData.Write(outDir);
			}

			// Output the individual (a-b) DATs
			if ((diff & UpdateMode.DiffIndividualsOnly) != 0)
			{
				Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
				{
					string path = Utilities.GetOutputPath(outDir, inputs[j], false /* inplace */);

					// Try to output the file
					outDats[j].Write(path);
				});
			}

			watch.Stop();
		}

		/// <summary>
		/// Output user defined merge
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		public void MergeNoDiff(string outDir, List<string> inputs, List<DatFile> datHeaders)
		{
			// If we're in SuperDAT mode, prefix all games with their respective DATs
			if (Type == "SuperDAT")
			{
				List<string> keys = Keys;
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					List<DatItem> items = this[key].ToList();
					List<DatItem> newItems = new List<DatItem>();
					foreach (DatItem item in items)
					{
						DatItem newItem = item;
						string filename = inputs[newItem.SystemID].Split('¬')[0];
						string rootpath = inputs[newItem.SystemID].Split('¬')[1];

						rootpath += (String.IsNullOrWhiteSpace(rootpath) ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newItem.MachineName = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
							+ Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
							+ newItem.MachineName;

						newItems.Add(newItem);
					}

					Remove(key);
					AddRange(key, newItems);
				});
			}

			// Try to output the file
			Write(outDir);
		}

		/// <summary>
		/// Convert, update, and filter a DAT file or set of files using a base
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void Update(List<string> inputFileNames, string outDir, bool inplace, bool clean, bool remUnicode, bool descAsName,
			Filter filter, SplitType splitType)
		{
			// Iterate over the files
			foreach (string file in inputFileNames)
			{
				DatFile innerDatdata = new DatFile(this);
				Globals.Logger.User("Processing '{0}'", Path.GetFileName(file.Split('¬')[0]));
				innerDatdata.Parse(file, 0, 0, splitType, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName,
					keepext: ((innerDatdata.DatFormat & DatFormat.TSV) != 0 || (innerDatdata.DatFormat & DatFormat.CSV) != 0));
				filter.FilterDatFile(innerDatdata);

				// Get the correct output path
				string realOutDir = Utilities.GetOutputPath(outDir, file, inplace);

				// Try to output the file, overwriting only if it's not in the current directory
				// TODO: Figure out if overwriting should always happen of if there should be a user flag
				innerDatdata.Write(realOutDir, overwrite: (realOutDir != Environment.CurrentDirectory));
			}
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
			List<string> keys = Keys;
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

		#region Filtering

		/// <summary>
		/// Use game descriptions as names in the DAT, updating cloneof/romof/sampleof
		/// </summary>
		private void MachineDescriptionToName()
		{
			try
			{
				// First we want to get a mapping for all games to description
				ConcurrentDictionary<string, string> mapping = new ConcurrentDictionary<string, string>();
				List<string> keys = Keys;
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					List<DatItem> items = this[key];
					foreach (DatItem item in items)
					{
						// If the key mapping doesn't exist, add it
						if (!mapping.ContainsKey(item.MachineName))
						{
							mapping.TryAdd(item.MachineName, item.MachineDescription.Replace('/', '_').Replace("\"", "''"));
						}
					}
				});

				// Now we loop through every item and update accordingly
				keys = Keys;
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					List<DatItem> items = this[key];
					List<DatItem> newItems = new List<DatItem>();
					foreach (DatItem item in items)
					{
						// Update machine name
						if (!String.IsNullOrWhiteSpace(item.MachineName) && mapping.ContainsKey(item.MachineName))
						{
							item.MachineName = mapping[item.MachineName];
						}

						// Update cloneof
						if (!String.IsNullOrWhiteSpace(item.CloneOf) && mapping.ContainsKey(item.CloneOf))
						{
							item.CloneOf = mapping[item.CloneOf];
						}

						// Update romof
						if (!String.IsNullOrWhiteSpace(item.RomOf) && mapping.ContainsKey(item.RomOf))
						{
							item.RomOf = mapping[item.RomOf];
						}

						// Update sampleof
						if (!String.IsNullOrWhiteSpace(item.SampleOf) && mapping.ContainsKey(item.SampleOf))
						{
							item.SampleOf = mapping[item.SampleOf];
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
		/// Remove all items marked for removal from the DAT
		/// </summary>
		private void RemoveMarkedItems()
		{
			List<string> keys = Keys;
			foreach (string key in keys)
			{
				List<DatItem> items = this[key];
				List<DatItem> newItems = new List<DatItem>();
				foreach (DatItem item in items)
				{
					if (!item.Remove)
					{
						newItems.Add(item);
					}
				}

				Remove(key);
				AddRange(key, newItems);
			}
		}

		/// <summary>
		/// Strip the given hash types from the DAT
		/// </summary>
		private void StripHashesFromItems()
		{
			// Output the logging statement
			Globals.Logger.User("Stripping requested hashes");

			// Now process all of the roms
			List<string> keys = Keys;
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

		/// <summary>
		/// Strip the dates from the beginning of scene-style set names
		/// </summary>
		private void StripSceneDatesFromItems()
		{
			// Output the logging statement
			Globals.Logger.User("Stripping scene-style dates");

			// Set the regex pattern to use
			string pattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{2}-)(.*?-.*?)";

			// Now process all of the roms
			List<string> keys = Keys;
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				for (int j = 0; j < items.Count; j++)
				{
					DatItem item = items[j];
					if (Regex.IsMatch(item.MachineName, pattern))
					{
						item.MachineName = Regex.Replace(item.MachineName, pattern, "$2");
					}
					if (Regex.IsMatch(item.MachineDescription, pattern))
					{
						item.MachineDescription = Regex.Replace(item.MachineDescription, pattern, "$2");
					}

					items[j] = item;
				}

				Remove(key);
				AddRange(key, items);
			});
		}

		#endregion

		#region Internal Merging/Splitting

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
			AddRomsFromChildren();

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

		/// <summary>
		/// Use romof tags to add roms to the children
		/// </summary>
		private void AddRomsFromBios()
		{
			List<string> games = Keys;
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrWhiteSpace(this[game][0].RomOf))
				{
					parent = this[game][0].RomOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrWhiteSpace(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we copy the items from the parent to the current game
				DatItem copyFrom = this[game][0];
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					DatItem datItem = (DatItem)item.Clone();
					datItem.CopyMachineInformation(copyFrom);
					if (this[game].Where(i => i.Name == datItem.Name).Count() == 0 && !this[game].Contains(datItem))
					{
						Add(game, datItem);
					}
				}
			}
		}

		/// <summary>
		/// Use device_ref tags to add roms to the children
		/// </summary>
		private void AddRomsFromDevices()
		{
			List<string> games = Keys;
			foreach (string game in games)
			{
				// If the game has no devices, we continue
				if (this[game][0].Devices == null || this[game][0].Devices.Count == 0)
				{
					continue;
				}

				// Determine if the game has any devices or not
				List<string> devices = this[game][0].Devices;
				foreach (string device in devices)
				{
					// If the device doesn't exist then we continue
					if (this[device].Count == 0)
					{
						continue;
					}

					// Otherwise, copy the items from the device to the current game
					DatItem copyFrom = this[game][0];
					List<DatItem> devItems = this[device];
					foreach (DatItem item in devItems)
					{
						DatItem datItem = (DatItem)item.Clone();
						datItem.CopyMachineInformation(copyFrom);
						if (this[game].Where(i => i.Name == datItem.Name).Count() == 0 && !this[game].Contains(datItem))
						{
							Add(game, datItem);
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
			List<string> games = Keys;
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrWhiteSpace(this[game][0].CloneOf))
				{
					parent = this[game][0].CloneOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrWhiteSpace(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we copy the items from the parent to the current game
				DatItem copyFrom = this[game][0];
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					DatItem datItem = (DatItem)item.Clone();
					datItem.CopyMachineInformation(copyFrom);
					if (this[game].Where(i => i.Name == datItem.Name).Count() == 0 && !this[game].Contains(datItem))
					{
						Add(game, datItem);
					}
				}

				// Now we want to get the parent romof tag and put it in each of the items
				List<DatItem> items = this[game];
				string romof = this[parent][0].RomOf;
				foreach (DatItem item in items)
				{
					item.RomOf = romof;
				}
			}
		}

		/// <summary>
		/// Use cloneof tags to add roms to the parents, removing the child sets in the process
		/// </summary>
		private void AddRomsFromChildren()
		{
			List<string> games = Keys;
			foreach (string game in games)
			{
				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrWhiteSpace(this[game][0].CloneOf))
				{
					parent = this[game][0].CloneOf;
				}

				// If there is no parent, then we continue
				if (String.IsNullOrWhiteSpace(parent))
				{
					continue;
				}

				// Otherwise, move the items from the current game to a subfolder of the parent game
				DatItem copyFrom = this[parent].Count == 0 ? new Rom { MachineName = parent, MachineDescription = parent } : this[parent][0];
				List<DatItem> items = this[game];
				foreach (DatItem item in items)
				{
					// If the disk doesn't have a valid merge tag OR the merged file doesn't exist in the parent, then add it
					if (item.Type == ItemType.Disk && (item.MergeTag == null || !this[parent].Select(i => i.Name).Contains(item.MergeTag)))
					{
						item.CopyMachineInformation(copyFrom);
						Add(parent, item);
					}

					// Otherwise, if the parent doesn't already contain the non-disk, add it
					else if (item.Type != ItemType.Disk && !this[parent].Contains(item))
					{
						// Rename the child so it's in a subfolder
						item.Name = item.Name + "\\" + item.Name;

						// Update the machine to be the new parent
						item.CopyMachineInformation(copyFrom);

						// Add the rom to the parent set
						Add(parent, item);
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
			List<string> games = Keys;
			foreach (string game in games)
			{
				if (this[game].Count > 0
					&& (this[game][0].MachineType == MachineType.Bios
						|| this[game][0].MachineType == MachineType.Device))
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
			List<string> games = Keys;
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrWhiteSpace(this[game][0].RomOf))
				{
					parent = this[game][0].RomOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrWhiteSpace(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we remove the items that are in the parent from the current game
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					DatItem datItem = (DatItem)item.Clone();
					Remove(game, datItem);
				}
			}
		}

		/// <summary>
		/// Use cloneof tags to remove roms from the children
		/// </summary>
		private void RemoveRomsFromChild()
		{
			List<string> games = Keys;
			foreach (string game in games)
			{
				// If the game has no items in it, we want to continue
				if (this[game].Count == 0)
				{
					continue;
				}

				// Determine if the game has a parent or not
				string parent = null;
				if (!String.IsNullOrWhiteSpace(this[game][0].CloneOf))
				{
					parent = this[game][0].CloneOf;
				}

				// If the parent doesnt exist, we want to continue
				if (String.IsNullOrWhiteSpace(parent))
				{
					continue;
				}

				// If the parent doesn't have any items, we want to continue
				if (this[parent].Count == 0)
				{
					continue;
				}

				// If the parent exists and has items, we copy the items from the parent to the current game
				List<DatItem> parentItems = this[parent];
				foreach (DatItem item in parentItems)
				{
					DatItem datItem = (DatItem)item.Clone();
					Remove(game, datItem);
				}

				// Now we want to get the parent romof tag and put it in each of the items
				List<DatItem> items = this[game];
				string romof = this[parent][0].RomOf;
				foreach (DatItem item in items)
				{
					item.RomOf = romof;
				}
			}
		}

		/// <summary>
		/// Remove all romof and cloneof tags from all games
		/// </summary>
		private void RemoveTagsFromChild()
		{
			List<string> games = Keys;
			foreach (string game in games)
			{
				List<DatItem> items = this[game];
				foreach (DatItem item in items)
				{
					item.CloneOf = null;
					item.RomOf = null;
				}
			}
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
			// Check if we have a split path and get the filename accordingly
			if (filename.Contains("¬"))
			{
				filename = filename.Split('¬')[0];
			}

			// Check the file extension first as a safeguard
			if (!Utilities.HasValidDatExtension(filename))
			{
				return;
			}

			// If the output filename isn't set already, get the internal filename
			FileName = (String.IsNullOrWhiteSpace(FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : FileName);

			// If the output type isn't set already, get the internal output type
			DatFormat = (DatFormat == 0 ? Utilities.GetDatFormat(filename) : DatFormat);

			// Now parse the correct type of DAT
			try
			{
				Utilities.GetDatFile(filename, this)?.ParseFile(filename, sysid, srcid, keep, clean, remUnicode);
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
				splitType = Utilities.GetSplitType(ForceMerging);
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
			item.MachineName = (clean ? Utilities.CleanGameName(item.MachineName) : item.MachineName);

			// If we're stripping unicode characters, do so from all relevant things
			if (remUnicode)
			{
				item.Name = Utilities.RemoveUnicodeCharacters(item.Name);
				item.MachineName = Utilities.RemoveUnicodeCharacters(item.MachineName);
				item.MachineDescription = Utilities.RemoveUnicodeCharacters(item.MachineDescription);
			}

			// If we have a Rom or a Disk, clean the hash data
			if (item.Type == ItemType.Rom)
			{
				Rom itemRom = (Rom)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemRom.CRC = Utilities.CleanHashData(itemRom.CRC, Constants.CRCLength);
				itemRom.MD5 = Utilities.CleanHashData(itemRom.MD5, Constants.MD5Length);
				itemRom.SHA1 = Utilities.CleanHashData(itemRom.SHA1, Constants.SHA1Length);
				itemRom.SHA256 = Utilities.CleanHashData(itemRom.SHA256, Constants.SHA256Length);
				itemRom.SHA384 = Utilities.CleanHashData(itemRom.SHA384, Constants.SHA384Length);
				itemRom.SHA512 = Utilities.CleanHashData(itemRom.SHA512, Constants.SHA512Length);

				// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
				if ((itemRom.Size == 0 || itemRom.Size == -1)
					&& ((itemRom.CRC == Constants.CRCZero || String.IsNullOrWhiteSpace(itemRom.CRC))
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
					&& String.IsNullOrWhiteSpace(itemRom.CRC)
					&& String.IsNullOrWhiteSpace(itemRom.MD5)
					&& String.IsNullOrWhiteSpace(itemRom.SHA1)
					&& String.IsNullOrWhiteSpace(itemRom.SHA256)
					&& String.IsNullOrWhiteSpace(itemRom.SHA384)
					&& String.IsNullOrWhiteSpace(itemRom.SHA512))
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
				itemDisk.MD5 = Utilities.CleanHashData(itemDisk.MD5, Constants.MD5Length);
				itemDisk.SHA1 = Utilities.CleanHashData(itemDisk.SHA1, Constants.SHA1Length);
				itemDisk.SHA256 = Utilities.CleanHashData(itemDisk.SHA256, Constants.SHA256Length);
				itemDisk.SHA384 = Utilities.CleanHashData(itemDisk.SHA384, Constants.SHA384Length);
				itemDisk.SHA512 = Utilities.CleanHashData(itemDisk.SHA512, Constants.SHA512Length);

				// If the file has aboslutely no hashes, skip and log
				if (itemDisk.ItemStatus != ItemStatus.Nodump
					&& String.IsNullOrWhiteSpace(itemDisk.MD5)
					&& String.IsNullOrWhiteSpace(itemDisk.SHA1)
					&& String.IsNullOrWhiteSpace(itemDisk.SHA256)
					&& String.IsNullOrWhiteSpace(itemDisk.SHA384)
					&& String.IsNullOrWhiteSpace(itemDisk.SHA512))
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
					key = ((Disk)item).MD5 ?? ((Disk)item).SHA1;
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

		/// <summary>
		/// Parse DatFile and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		public virtual void ParseFile(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Miscellaneous
			bool keep,
			bool clean,
			bool remUnicode)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Populate DAT from Directory

		/// <summary>
		/// Create a new Dat from a directory
		/// </summary>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="skipFileType">Type of files that should be skipped</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="outDir">Output directory to </param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		public bool PopulateFromDir(string basePath, Hash omitFromScan, bool bare, bool archivesAsFiles, SkipFileType skipFileType,
			bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst, bool chdsAsFiles)
		{
			// If the description is defined but not the name, set the name from the description
			if (String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Description))
			{
				Name = Description;
			}

			// If the name is defined but not the description, set the description from the name
			else if (!String.IsNullOrWhiteSpace(Name) && String.IsNullOrWhiteSpace(Description))
			{
				Description = Name + (bare ? "" : " (" + Date + ")");
			}

			// If neither the name or description are defined, set them from the automatic values
			else if (String.IsNullOrWhiteSpace(Name) && String.IsNullOrWhiteSpace(Description))
			{
				string[] splitpath = basePath.Split(Path.DirectorySeparatorChar);
				Name = String.IsNullOrWhiteSpace(splitpath.Last()) ? splitpath[splitpath.Length - 2] : splitpath.Last();
				Description = Name + (bare ? "" : " (" + Date + ")");
			}

			// Clean the temp directory path
			tempDir = (String.IsNullOrWhiteSpace(tempDir) ? Path.GetTempPath() : tempDir);

			// Process the input
			if (Directory.Exists(basePath))
			{
				Globals.Logger.Verbose("Folder found: {0}", basePath);

				// Process the files in the main folder
				List<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.TopDirectoryOnly).ToList();
				Parallel.ForEach(files, Globals.ParallelOptions, item =>
				{
					CheckFileForHashes(item, basePath, omitFromScan, bare, archivesAsFiles, skipFileType,
						addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst, chdsAsFiles);
				});

				// Find all top-level subfolders
				files = Directory.EnumerateDirectories(basePath, "*", SearchOption.TopDirectoryOnly).ToList();
				foreach (string item in files)
				{
					List<string> subfiles = Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(subfiles, Globals.ParallelOptions, subitem =>
					{
						CheckFileForHashes(subitem, basePath, omitFromScan, bare, archivesAsFiles, skipFileType,
							addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst, chdsAsFiles);
					});
				}

				// Now find all folders that are empty, if we are supposed to
				if (!Romba && addBlanks)
				{
					List<string> empties = Utilities.GetEmptyDirectories(basePath).ToList();
					Parallel.ForEach(empties, Globals.ParallelOptions, dir =>
					{
						// Get the full path for the directory
						string fulldir = Path.GetFullPath(dir);

						// Set the temporary variables
						string gamename = "";
						string romname = "";

						// If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
						if (Type == "SuperDAT")
						{
							gamename = fulldir.Remove(0, basePath.Length + 1);
							romname = "_";
						}

						// Otherwise, we want just the top level folder as the game, and the file as everything else
						else
						{
							gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
							romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "_");
						}

						// Sanitize the names
						if (gamename.StartsWith(Path.DirectorySeparatorChar.ToString()))
						{
							gamename = gamename.Substring(1);
						}
						if (gamename.EndsWith(Path.DirectorySeparatorChar.ToString()))
						{
							gamename = gamename.Substring(0, gamename.Length - 1);
						}
						if (romname.StartsWith(Path.DirectorySeparatorChar.ToString()))
						{
							romname = romname.Substring(1);
						}
						if (romname.EndsWith(Path.DirectorySeparatorChar.ToString()))
						{
							romname = romname.Substring(0, romname.Length - 1);
						}

						Globals.Logger.Verbose("Adding blank empty folder: {0}", gamename);
						this["null"].Add(new Rom(romname, gamename, omitFromScan));
					});
				}
			}
			else if (File.Exists(basePath))
			{
				CheckFileForHashes(basePath, Path.GetDirectoryName(Path.GetDirectoryName(basePath)), omitFromScan, bare, archivesAsFiles,
					skipFileType, addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst, chdsAsFiles);
			}

			// Now that we're done, delete the temp folder (if it's not the default)
			Globals.Logger.User("Cleaning temp folder");
			if (tempDir != Path.GetTempPath())
			{
				Utilities.TryDeleteDirectory(tempDir);
			}

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="skipFileType">Type of files that should be skipped</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		private void CheckFileForHashes(string item, string basePath, Hash omitFromScan, bool bare, bool archivesAsFiles,
			SkipFileType skipFileType, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst, bool chdsAsFiles)
		{
			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (Romba)
			{
				GZipArchive archive = new GZipArchive(item);
				Rom rom = archive.GetTorrentGZFileInfo();

				// If the rom is valid, write it out
				if (rom != null && rom.Name != null)
				{
					// Add the list if it doesn't exist already
					Add(rom.Size + "-" + rom.CRC, rom);
					Globals.Logger.User("File added: {0}", Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
				}
				else
				{
					Globals.Logger.User("File not added: {0}", Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
					return;
				}

				return;
			}

			// If we're copying files, copy it first and get the new filename
			string newItem = item;
			string newBasePath = basePath;
			if (copyFiles)
			{
				newBasePath = Path.Combine(tempDir, new Guid().ToString());
				newItem = Path.GetFullPath(Path.Combine(newBasePath, Path.GetFullPath(item).Remove(0, basePath.Length + 1)));
				Directory.CreateDirectory(Path.GetDirectoryName(newItem));
				File.Copy(item, newItem, true);
			}

			// Create a list for all found items
			List<Rom> extracted = null;

			// If we don't have archives as files, try to scan the file as an archive
			if (!archivesAsFiles)
			{
				// Get the base archive first
				BaseArchive archive = Utilities.GetArchive(newItem);

				// Now get all extracted items from the archive
				if (archive != null)
				{
					extracted = archive.GetArchiveFileInfo(omitFromScan: omitFromScan, date: addDate);
				}
			}

			// If the file should be skipped based on type, do so now
			if ((extracted != null && skipFileType == SkipFileType.Archive)
				|| (extracted == null && skipFileType == SkipFileType.File))
			{
				return;
			}

			// If the extracted list is null, just scan the item itself
			if (extracted == null || archivesAsFiles)
			{
				ProcessFile(newItem, "", newBasePath, omitFromScan, addDate, headerToCheckAgainst, chdsAsFiles);
			}
			// Otherwise, add all of the found items
			else
			{
				// First take care of the found items
				Parallel.ForEach(extracted, Globals.ParallelOptions, rom =>
				{
					ProcessFileHelper(newItem,
						rom,
						basePath,
						(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
				});

				// Then, if we're looking for blanks, get all of the blank folders and add them
				if (addBlanks)
				{
					List<string> empties = new List<string>();

					// Get the base archive first
					BaseArchive archive = Utilities.GetArchive(newItem);

					// Now get all blank folders from the archive
					if (archive != null)
					{
						empties = archive.GetEmptyFolders();
					}
					
					// Add add all of the found empties to the DAT
					Parallel.ForEach(empties, Globals.ParallelOptions, empty =>
					{
						Rom emptyRom = new Rom(Path.Combine(empty, "_"), newItem, omitFromScan);
						ProcessFileHelper(newItem,
							emptyRom,
							basePath,
							(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
					});
				}
			}

			// Cue to delete the file if it's a copy
			if (copyFiles && item != newItem)
			{
				Utilities.TryDeleteDirectory(newBasePath);
			}
		}

		/// <summary>
		/// Process a single file as a file
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="basePath">Path the represents the parent directory</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		private void ProcessFile(string item, string parent, string basePath, Hash omitFromScan,
			bool addDate, string headerToCheckAgainst, bool chdsAsFiles)
		{
			Globals.Logger.Verbose("'{0}' treated like a file", Path.GetFileName(item));
			DatItem datItem = Utilities.GetFileInfo(item, omitFromScan: omitFromScan, date: addDate, header: headerToCheckAgainst, chdsAsFiles: chdsAsFiles);

			ProcessFileHelper(item, datItem, basePath, parent);
		}

		/// <summary>
		/// Process a single file as a file (with found Rom data)
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="item">Rom data to be used to write to file</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		private void ProcessFileHelper(string item, DatItem datItem, string basepath, string parent)
		{
			// If the datItem isn't a Rom or Disk, return
			if (datItem.Type != ItemType.Rom && datItem.Type != ItemType.Disk)
			{
				return;
			}

			string key = "";
			if (datItem.Type == ItemType.Rom)
			{
				key = ((Rom)datItem).Size + "-" + ((Rom)datItem).CRC;
			}
			else if (datItem.Type == ItemType.Disk)
			{
				key = ((Disk)datItem).SHA1;
			}

			// Add the list if it doesn't exist already
			Add(key);

			try
			{
				// If the basepath ends with a directory separator, remove it
				if (!basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					basepath += Path.DirectorySeparatorChar.ToString();
				}

				// Make sure we have the full item path
				item = Path.GetFullPath(item);

				// Get the data to be added as game and item names
				string gamename = "";
				string romname = "";

				// If the parent is blank, then we have a non-archive file
				if (String.IsNullOrWhiteSpace(parent))
				{
					// If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
					if (Type == "SuperDAT")
					{
						gamename = Path.GetDirectoryName(item.Remove(0, basepath.Length));
						romname = Path.GetFileName(item);
					}

					// Otherwise, we want just the top level folder as the game, and the file as everything else
					else
					{
						gamename = item.Remove(0, basepath.Length).Split(Path.DirectorySeparatorChar)[0];
						romname = item.Remove(0, (Path.Combine(basepath, gamename).Length));
					}
				}

				// Otherwise, we assume that we have an archive
				else
				{
					// If we have a SuperDAT, we want the archive name as the game, and the file as everything else (?)
					if (Type == "SuperDAT")
					{
						gamename = parent;
						romname = datItem.Name;
					}

					// Otherwise, we want the archive name as the game, and the file as everything else
					else
					{
						gamename = parent;
						romname = datItem.Name;
					}
				}

				// Sanitize the names
				if (romname == null)
				{
					romname = "";
				}
				if (gamename.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					gamename = gamename.Substring(1);
				}
				if (gamename.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					gamename = gamename.Substring(0, gamename.Length - 1);
				}
				if (romname.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					romname = romname.Substring(1);
				}
				if (romname.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					romname = romname.Substring(0, romname.Length - 1);
				}
				if (!String.IsNullOrWhiteSpace(gamename) && String.IsNullOrWhiteSpace(romname))
				{
					romname = gamename;
					gamename = "Default";
				}

				// Update rom information
				datItem.Name = romname;
				datItem.MachineName = gamename;
				datItem.MachineDescription = gamename;

				// If we have a Disk, then the ".chd" extension needs to be removed
				if (datItem.Type == ItemType.Disk)
				{
					datItem.Name = datItem.Name.Replace(".chd", "");
				}

				// Add the file information to the DAT
				Add(key, datItem);

				Globals.Logger.User("File added: {0}", romname + Environment.NewLine);
			}
			catch (IOException ex)
			{
				Globals.Logger.Error(ex.ToString());
				return;
			}
		}

		#endregion

		#region Rebuilding and Verifying

		/// <summary>
		/// Process the DAT and find all matches in input files and folders assuming they're a depot
		/// </summary>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildDepot(List<string> inputs, string outDir, bool date, bool delete,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, string headerToCheckAgainst)
		{
			#region Perform setup

			// If the DAT is not populated and inverse is not set, inform the user and quit
			if (Count == 0 && !inverse)
			{
				Globals.Logger.User("No entries were found to rebuild, exiting...");
				return false;
			}

			// Check that the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				outDir = Path.GetFullPath(outDir);
			}

			// Now we want to get forcepack flag if it's not overridden
			if (outputFormat == OutputFormat.Folder && ForcePacking != ForcePacking.None)
			{
				switch (ForcePacking)
				{
					case ForcePacking.Zip:
						outputFormat = OutputFormat.TorrentZip;
						break;
					case ForcePacking.Unzip:
						outputFormat = OutputFormat.Folder;
						break;
				}
			}

			// Preload the Skipper list
			int listcount = Skipper.List.Count;

			#endregion

			bool success = true;

			#region Rebuild from depots in order

			string format = "";
			switch (outputFormat)
			{
				case OutputFormat.Folder:
					format = "directory";
					break;
				case OutputFormat.TapeArchive:
					format = "TAR";
					break;
				case OutputFormat.Torrent7Zip:
					format = "Torrent7Z";
					break;
				case OutputFormat.TorrentGzip:
					format = "TorrentGZ";
					break;
				case OutputFormat.TorrentLRZip:
					format = "TorrentLRZ";
					break;
				case OutputFormat.TorrentRar:
					format = "TorrentRAR";
					break;
				case OutputFormat.TorrentXZ:
					format = "TorrentXZ";
					break;
				case OutputFormat.TorrentZip:
					format = "TorrentZip";
					break;
			}

			InternalStopwatch watch = new InternalStopwatch("Rebuilding all files to {0}", format);

			// Now loop through and get only directories from the input paths
			List<string> directories = new List<string>();
			Parallel.ForEach(inputs, Globals.ParallelOptions, input =>
			{
				// Add to the list if the input is a directory
				if (Directory.Exists(input))
				{
					Globals.Logger.Verbose("Adding depot: {0}", input);
					lock (directories)
					{
						directories.Add(input);
					}
				}
			});

			// If we don't have any directories, we want to exit
			if (directories.Count == 0)
			{
				return success;
			}

			// Now that we have a list of depots, we want to sort the input DAT by SHA-1
			BucketBy(SortedBy.SHA1, DedupeType.None);

			// Then we want to loop through each of the hashes and see if we can rebuild
			List<string> hashes = Keys;
			foreach (string hash in hashes)
			{
				// Pre-empt any issues that could arise from string length
				if (hash.Length != Constants.SHA1Length)
				{
					continue;
				}

				Globals.Logger.User("Checking hash '{0}'", hash);

				// Get the extension path for the hash
				string subpath = Utilities.GetRombaPath(hash);

				// Find the first depot that includes the hash
				string foundpath = null;
				foreach (string directory in directories)
				{
					if (File.Exists(Path.Combine(directory, subpath)))
					{
						foundpath = Path.Combine(directory, subpath);
						break;
					}
				}

				// If we didn't find a path, then we continue
				if (foundpath == null)
				{
					continue;
				}

				// If we have a path, we want to try to get the rom information
				GZipArchive archive = new GZipArchive(foundpath);
				Rom fileinfo = archive.GetTorrentGZFileInfo();

				// If the file information is null, then we continue
				if (fileinfo == null)
				{
					continue;
				}

				// Otherwise, we rebuild that file to all locations that we need to
				RebuildIndividualFile(fileinfo, foundpath, outDir, date, inverse, outputFormat, romba,
					updateDat, false /* isZip */, headerToCheckAgainst);
			}

			watch.Stop();

			#endregion

			// If we're updating the DAT, output to the rebuild directory
			if (updateDat)
			{
				FileName = "fixDAT_" + FileName;
				Name = "fixDAT_" + Name;
				Description = "fixDAT_" + Description;
				RemoveMarkedItems();
				Write(outDir);
			}

			return success;
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildGeneric(List<string> inputs, string outDir, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst, bool chdsAsFiles)
		{
			#region Perform setup

			// If the DAT is not populated and inverse is not set, inform the user and quit
			if (Count == 0 && !inverse)
			{
				Globals.Logger.User("No entries were found to rebuild, exiting...");
				return false;
			}

			// Check that the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				outDir = Path.GetFullPath(outDir);
			}

			// Now we want to get forcepack flag if it's not overridden
			if (outputFormat == OutputFormat.Folder && ForcePacking != ForcePacking.None)
			{
				switch (ForcePacking)
				{
					case ForcePacking.Zip:
						outputFormat = OutputFormat.TorrentZip;
						break;
					case ForcePacking.Unzip:
						outputFormat = OutputFormat.Folder;
						break;
				}
			}

			// Preload the Skipper list
			int listcount = Skipper.List.Count;

			#endregion

			bool success = true;

			#region Rebuild from sources in order

			string format = "";
			switch (outputFormat)
			{
				case OutputFormat.Folder:
					format = "directory";
					break;
				case OutputFormat.TapeArchive:
					format = "TAR";
					break;
				case OutputFormat.Torrent7Zip:
					format = "Torrent7Z";
					break;
				case OutputFormat.TorrentGzip:
					format = "TorrentGZ";
					break;
				case OutputFormat.TorrentLRZip:
					format = "TorrentLRZ";
					break;
				case OutputFormat.TorrentRar:
					format = "TorrentRAR";
					break;
				case OutputFormat.TorrentXZ:
					format = "TorrentXZ";
					break;
				case OutputFormat.TorrentZip:
					format = "TorrentZip";
					break;
			}

			InternalStopwatch watch = new InternalStopwatch("Rebuilding all files to {0}", format);

			// Now loop through all of the files in all of the inputs
			foreach (string input in inputs)
			{
				// If the input is a file
				if (File.Exists(input))
				{
					Globals.Logger.User("Checking file: {0}", input);
					RebuildGenericHelper(input, outDir, quickScan, date, delete, inverse,
						outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, chdsAsFiles);
				}

				// If the input is a directory
				else if (Directory.Exists(input))
				{
					Globals.Logger.Verbose("Checking directory: {0}", input);
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						Globals.Logger.User("Checking file: {0}", file);
						RebuildGenericHelper(file, outDir, quickScan, date, delete, inverse,
							outputFormat, romba, archiveScanLevel, updateDat, headerToCheckAgainst, chdsAsFiles);
					}
				}
			}

			watch.Stop();

			#endregion

			// If we're updating the DAT, output to the rebuild directory
			if (updateDat)
			{
				FileName = "fixDAT_" + FileName;
				Name = "fixDAT_" + Name;
				Description = "fixDAT_" + Description;
				RemoveMarkedItems();
				Write(outDir);
			}

			return success;
		}

		/// <summary>
		/// Attempt to add a file to the output if it matches
		/// </summary>
		/// <param name="file">Name of the file to process</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		private void RebuildGenericHelper(string file, string outDir, bool quickScan, bool date,
			bool delete, bool inverse, OutputFormat outputFormat, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat,
			string headerToCheckAgainst, bool chdsAsFiles)
		{
			// If we somehow have a null filename, return
			if (file == null)
			{
				return;
			}

			// Set the deletion variables
			bool usedExternally = false;
			bool usedInternally = false;

			// Get the required scanning level for the file
			Utilities.GetInternalExternalProcess(file, archiveScanLevel, out bool shouldExternalProcess, out bool shouldInternalProcess);

			// If we're supposed to scan the file externally
			if (shouldExternalProcess)
			{
				// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
				DatItem fileinfo = Utilities.GetFileInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes),
					header: headerToCheckAgainst, chdsAsFiles: chdsAsFiles);
				usedExternally = RebuildIndividualFile(fileinfo, file, outDir, date, inverse, outputFormat,
					romba, updateDat, null /* isZip */, headerToCheckAgainst);
			}

			// If we're supposed to scan the file internally
			if (shouldInternalProcess)
			{
				// Create an empty list of Roms for archive entries
				List<Rom> entries = null;
				usedInternally = true;

				// Get the TGZ status for later
				GZipArchive tgz = new GZipArchive(file);
				bool isTorrentGzip = tgz.IsTorrent();

				// Get the base archive first
				BaseArchive archive = Utilities.GetArchive(file);

				// Now get all extracted items from the archive
				if (archive != null)
				{
					// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
					entries = archive.GetArchiveFileInfo(omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), date: date);
				}

				// If the entries list is null, we encountered an error and should scan exteranlly
				if (entries == null && File.Exists(file))
				{
					// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
					DatItem fileinfo = Utilities.GetFileInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), chdsAsFiles: chdsAsFiles);
					usedExternally = RebuildIndividualFile(fileinfo, file, outDir, date, inverse, outputFormat,
						romba, updateDat, null /* isZip */, headerToCheckAgainst);
				}
				// Otherwise, loop through the entries and try to match
				else
				{
					foreach (Rom entry in entries)
					{
						usedInternally &= RebuildIndividualFile(entry, file, outDir, date, inverse, outputFormat,
							romba, updateDat, !isTorrentGzip /* isZip */, headerToCheckAgainst);
					}
				}
			}

			// If we are supposed to delete the file, do so
			if (delete && (usedExternally || usedInternally))
			{
				Utilities.TryDeleteFile(file);
			}
		}

		/// <summary>
		/// Find duplicates and rebuild individual files to output
		/// </summary>
		/// <param name="datItem">Information for the current file to rebuild from</param>
		/// <param name="file">Name of the file to process</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="isZip">True if the input file is an archive, false if the file is TGZ, null otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <returns>True if the file was able to be rebuilt, false otherwise</returns>
		private bool RebuildIndividualFile(DatItem datItem, string file, string outDir, bool date,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, bool? isZip, string headerToCheckAgainst)
		{
			// Set the output value
			bool rebuilt = false;

			// If the DatItem is a Disk, force rebuilding to a folder except if TGZ
			if (datItem.Type == ItemType.Disk && outputFormat != OutputFormat.TorrentGzip)
			{
				outputFormat = OutputFormat.Folder;
			}

			// Prepopluate a few key strings based on DatItem type
			string crc = null;
			string sha1 = null;
			if (datItem.Type == ItemType.Rom)
			{
				crc = ((Rom)datItem).CRC;
				sha1 = ((Rom)datItem).SHA1;
			}
			else if (datItem.Type == ItemType.Disk)
			{
				crc = "";
				sha1 = ((Disk)datItem).SHA1;
			}

			// Find if the file has duplicates in the DAT
			bool hasDuplicates = datItem.HasDuplicates(this);

			// If it has duplicates and we're not filtering, rebuild it
			if (hasDuplicates && !inverse)
			{
				// Get the list of duplicates to rebuild to
				List<DatItem> dupes = datItem.GetDuplicates(this, remove: updateDat);

				// If we don't have any duplicates, continue
				if (dupes.Count == 0)
				{
					return rebuilt;
				}

				// If we have a very specifc TGZ->TGZ case, just copy it accordingly
				GZipArchive tgz = new GZipArchive(file);
				Rom rom = tgz.GetTorrentGZFileInfo();
				if (isZip == false && rom != null && outputFormat == OutputFormat.TorrentGzip)
				{
					// Get the proper output path
					if (romba)
					{
						outDir = Path.Combine(outDir, Utilities.GetRombaPath(sha1));
					}
					else
					{
						outDir = Path.Combine(outDir, sha1 + ".gz");
					}

					// Make sure the output folder is created
					Directory.CreateDirectory(Path.GetDirectoryName(outDir));

					// Now copy the file over
					try
					{
						File.Copy(file, outDir);
						rebuilt &= true;
					}
					catch
					{
						rebuilt = false;
					}

					return rebuilt;
				}

				// Get a generic stream for the file
				Stream fileStream = new MemoryStream();

				// If we have a zipfile, extract the stream to memory
				if (isZip != null)
				{
					string realName = null;
					BaseArchive archive = Utilities.GetArchive(file);
					if (archive != null)
					{
						(fileStream, realName) = archive.ExtractEntryStream(datItem.Name);
					}
				}
				// Otherwise, just open the filestream
				else
				{
					fileStream = Utilities.TryOpenRead(file);
				}

				// If the stream is null, then continue
				if (fileStream == null)
				{
					return rebuilt;
				}

				// Seek to the beginning of the stream
				fileStream.Seek(0, SeekOrigin.Begin);

				Globals.Logger.User("Matches found for '{0}', rebuilding accordingly...", Path.GetFileName(datItem.Name));
				rebuilt = true;

				// Now loop through the list and rebuild accordingly
				foreach (DatItem item in dupes)
				{
					// Get the output archive, if possible
					BaseArchive outputArchive = Utilities.GetArchive(outputFormat);

					// Now rebuild to the output file
					outputArchive.Write(fileStream, outDir, (Rom)item, date: date, romba: romba);
				}

				// Close the input stream
				fileStream?.Dispose();
			}

			// If we have no duplicates and we're filtering, rebuild it
			else if (!hasDuplicates && inverse)
			{
				string machinename = null;

				// If we have a very specifc TGZ->TGZ case, just copy it accordingly
				GZipArchive tgz = new GZipArchive(file);
				Rom rom = tgz.GetTorrentGZFileInfo();
				if (isZip == false && rom != null && outputFormat == OutputFormat.TorrentGzip)
				{
					// Get the proper output path
					if (romba)
					{
						outDir = Path.Combine(outDir, Utilities.GetRombaPath(sha1));
					}
					else
					{
						outDir = Path.Combine(outDir, sha1 + ".gz");
					}

					// Make sure the output folder is created
					Directory.CreateDirectory(Path.GetDirectoryName(outDir));

					// Now copy the file over
					try
					{
						File.Copy(file, outDir);
						rebuilt &= true;
					}
					catch
					{
						rebuilt = false;
					}

					return rebuilt;
				}

				// Get a generic stream for the file
				Stream fileStream = new MemoryStream();

				// If we have a zipfile, extract the stream to memory
				if (isZip != null)
				{
					string realName = null;
					BaseArchive archive = Utilities.GetArchive(file);
					if (archive != null)
					{
						(fileStream, realName) = archive.ExtractEntryStream(datItem.Name);
					}
				}
				// Otherwise, just open the filestream
				else
				{
					fileStream = Utilities.TryOpenRead(file);
				}

				// If the stream is null, then continue
				if (fileStream == null)
				{
					return rebuilt;
				}

				// Get the item from the current file
				Rom item = (Rom)Utilities.GetStreamInfo(fileStream, fileStream.Length, keepReadOpen: true);
				item.MachineName = Path.GetFileNameWithoutExtension(item.Name);
				item.MachineDescription = Path.GetFileNameWithoutExtension(item.Name);

				// If we are coming from an archive, set the correct machine name
				if (machinename != null)
				{
					item.MachineName = machinename;
					item.MachineDescription = machinename;
				}

				Globals.Logger.User("No matches found for '{0}', rebuilding accordingly from inverse flag...", Path.GetFileName(datItem.Name));

				// Get the output archive, if possible
				BaseArchive outputArchive = Utilities.GetArchive(outputFormat);

				// Now rebuild to the output file
				if (outputArchive == null)
				{
					string outfile = Path.Combine(outDir, Utilities.RemovePathUnsafeCharacters(item.MachineName), item.Name);

					// Make sure the output folder is created
					Directory.CreateDirectory(Path.GetDirectoryName(outfile));

					// Now copy the file over
					try
					{
						FileStream writeStream = Utilities.TryCreate(outfile);

						// Copy the input stream to the output
						int bufferSize = 4096 * 128;
						byte[] ibuffer = new byte[bufferSize];
						int ilen;
						while ((ilen = fileStream.Read(ibuffer, 0, bufferSize)) > 0)
						{
							writeStream.Write(ibuffer, 0, ilen);
							writeStream.Flush();
						}
						writeStream.Dispose();

						if (date && !String.IsNullOrWhiteSpace(item.Date))
						{
							File.SetCreationTime(outfile, DateTime.Parse(item.Date));
						}

						rebuilt &= true;
					}
					catch
					{
						rebuilt &= false;
					}
				}
				else
				{
					rebuilt &= outputArchive.Write(fileStream, outDir, item, date: date, romba: romba);
				}

				// Close the input stream
				fileStream?.Dispose();
			}

			// Now we want to take care of headers, if applicable
			if (headerToCheckAgainst != null)
			{
				// Get a generic stream for the file
				Stream fileStream = new MemoryStream();

				// If we have a zipfile, extract the stream to memory
				if (isZip != null)
				{
					string realName = null;
					BaseArchive archive = Utilities.GetArchive(file);
					if (archive != null)
					{
						(fileStream, realName) = archive.ExtractEntryStream(datItem.Name);
					}
				}
				// Otherwise, just open the filestream
				else
				{
					fileStream = Utilities.TryOpenRead(file);
				}

				// If the stream is null, then continue
				if (fileStream == null)
				{
					return rebuilt;
				}

				// Check to see if we have a matching header first
				SkipperRule rule = Skipper.GetMatchingRule(fileStream, Path.GetFileNameWithoutExtension(headerToCheckAgainst));

				// If there's a match, create the new file to write
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// If the file could be transformed correctly
					MemoryStream transformStream = new MemoryStream();
					if (rule.TransformStream(fileStream, transformStream, keepReadOpen: true, keepWriteOpen: true))
					{
						// Get the file informations that we will be using
						Rom headerless = (Rom)Utilities.GetStreamInfo(transformStream, transformStream.Length, keepReadOpen: true);

						// Find if the file has duplicates in the DAT
						hasDuplicates = headerless.HasDuplicates(this);

						// If it has duplicates and we're not filtering, rebuild it
						if (hasDuplicates && !inverse)
						{
							// Get the list of duplicates to rebuild to
							List<DatItem> dupes = headerless.GetDuplicates(this, remove: updateDat);

							// If we don't have any duplicates, continue
							if (dupes.Count == 0)
							{
								return rebuilt;
							}

							Globals.Logger.User("Headerless matches found for '{0}', rebuilding accordingly...", Path.GetFileName(datItem.Name));
							rebuilt = true;

							// Now loop through the list and rebuild accordingly
							foreach (DatItem item in dupes)
							{
								// Create a headered item to use as well
								datItem.CopyMachineInformation(item);
								datItem.Name += "_" + crc;

								// If either copy succeeds, then we want to set rebuilt to true
								bool eitherSuccess = false;

								// Get the output archive, if possible
								BaseArchive outputArchive = Utilities.GetArchive(outputFormat);

								// Now rebuild to the output file
								eitherSuccess |= outputArchive.Write(transformStream, outDir, (Rom)item, date: date, romba: romba);
								eitherSuccess |= outputArchive.Write(fileStream, outDir, (Rom)datItem, date: date, romba: romba);

								// Now add the success of either rebuild
								rebuilt &= eitherSuccess;
							}
						}
					}

					// Dispose of the stream
					transformStream?.Dispose();
				}

				// Dispose of the stream
				fileStream?.Dispose();
			}

			return rebuilt;
		}

		/// <summary>
		/// Process the DAT and verify from the depots
		/// </summary>
		/// <param name="inputs">List of input directories to compare against</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyDepot(List<string> inputs, string headerToCheckAgainst)
		{
			bool success = true;

			InternalStopwatch watch = new InternalStopwatch("Verifying all from supplied depots");

			// Now loop through and get only directories from the input paths
			List<string> directories = new List<string>();
			foreach (string input in inputs)
			{
				// Add to the list if the input is a directory
				if (Directory.Exists(input))
				{
					Globals.Logger.Verbose("Adding depot: {0}", input);
					directories.Add(input);
				}
			}

			// If we don't have any directories, we want to exit
			if (directories.Count == 0)
			{
				return success;
			}

			// Now that we have a list of depots, we want to sort the input DAT by SHA-1
			BucketBy(SortedBy.SHA1, DedupeType.None);

			// Then we want to loop through each of the hashes and see if we can rebuild
			List<string> hashes = Keys;
			foreach (string hash in hashes)
			{
				// Pre-empt any issues that could arise from string length
				if (hash.Length != Constants.SHA1Length)
				{
					continue;
				}

				Globals.Logger.User("Checking hash '{0}'", hash);

				// Get the extension path for the hash
				string subpath = Utilities.GetRombaPath(hash);

				// Find the first depot that includes the hash
				string foundpath = null;
				foreach (string directory in directories)
				{
					if (File.Exists(Path.Combine(directory, subpath)))
					{
						foundpath = Path.Combine(directory, subpath);
						break;
					}
				}

				// If we didn't find a path, then we continue
				if (foundpath == null)
				{
					continue;
				}

				// If we have a path, we want to try to get the rom information
				GZipArchive tgz = new GZipArchive(foundpath);
				Rom fileinfo = tgz.GetTorrentGZFileInfo();

				// If the file information is null, then we continue
				if (fileinfo == null)
				{
					continue;
				}

				// Now we want to remove all duplicates from the DAT
				fileinfo.GetDuplicates(this, remove: true);
			}

			watch.Stop();

			// If there are any entries in the DAT, output to the rebuild directory
			FileName = "fixDAT_" + FileName;
			Name = "fixDAT_" + Name;
			Description = "fixDAT_" + Description;
			Write();

			return success;
		}

		/// <summary>
		/// Process the DAT and verify the output directory
		/// </summary>
		/// <param name="inputs">List of input directories to compare against</param>
		/// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyGeneric(List<string> inputs, bool hashOnly, bool quickScan, string headerToCheckAgainst, bool chdsAsFiles)
		{
			// TODO: We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
			bool success = true;

			// Then, loop through and check each of the inputs
			Globals.Logger.User("Processing files:\n");
			foreach (string input in inputs)
			{
				// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
				PopulateFromDir(input, (quickScan ? Hash.SecureHashes : Hash.DeepHashes) /* omitFromScan */, true /* bare */, false /* archivesAsFiles */,
					SkipFileType.None, false /* addBlanks */, false /* addDate */, "" /* tempDir */, false /* copyFiles */, headerToCheckAgainst, chdsAsFiles);
			}

			// Setup the fixdat
			DatFile matched = new DatFile(this);
			matched.ResetDictionary();
			matched.FileName = "fixDat_" + matched.FileName;
			matched.Name = "fixDat_" + matched.Name;
			matched.Description = "fixDat_" + matched.Description;
			matched.DatFormat = DatFormat.Logiqx;

			// If we are checking hashes only, essentially diff the inputs
			if (hashOnly)
			{
				// First we need to sort and dedupe by hash to get duplicates
				BucketBy(SortedBy.CRC, DedupeType.Full);

				// Then follow the same tactics as before
				foreach (string key in Keys)
				{
					List<DatItem> roms = this[key];
					foreach (DatItem rom in roms)
					{
						if (rom.SourceID == 99)
						{
							if (rom.Type == ItemType.Disk || rom.Type == ItemType.Rom)
							{
								matched.Add(((Disk)rom).SHA1, rom);
							}
						}
					}
				}
			}
			// If we are checking full names, get only files found in directory
			else
			{
				foreach (string key in Keys)
				{
					List<DatItem> roms = this[key];
					List<DatItem> newroms = DatItem.Merge(roms);
					foreach (Rom rom in newroms)
					{
						if (rom.SourceID == 99)
						{
							matched.Add(rom.Size + "-" + rom.CRC, rom);
						}
					}
				}
			}

			// Now output the fixdat to the main folder
			success &= matched.Write(stats: true);

			return success;
		}

		#endregion

		#region Splitting

		/// <summary>
		/// Split a set of input DATs based on the given information
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outDir">Output directory for the split files</param>
		/// <param name="inplace">True if files should be written to the source folders, false otherwise</param>
		/// <param name="splittingMode">Type of split to perform, if any</param>
		/// <param name="exta">First extension to split on (Extension Split only)</param>
		/// <param name="extb">Second extension to split on (Extension Split only)</param>
		/// <param name="shortname">True if short filenames should be used, false otherwise (Level Split only)</param>
		/// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise (Level Split only)</param>
		public void DetermineSplitType(List<string> inputs, string outDir, bool inplace, SplittingMode splittingMode,
			List<string> exta, List<string> extb, bool shortname, bool basedat)
		{
			// If we somehow have the "none" split type, return
			if (splittingMode == SplittingMode.None)
			{
				return;
			}

			// Get only files from the inputs
			List<string> files = Utilities.GetOnlyFilesFromInputs(inputs, appendparent: true);

			// Loop over the input files
			foreach (string file in files)
			{
				// Create and fill the new DAT
				Parse(file, 0, 0);

				// Get the output directory
				outDir = Utilities.GetOutputPath(outDir, file, inplace);

				// Split and write the DAT
				if ((splittingMode & SplittingMode.Extension) != 0)
				{
					SplitByExtension(outDir, exta, extb);
				}
				if ((splittingMode & SplittingMode.Hash) != 0)
				{
					SplitByHash(outDir);
				}
				if ((splittingMode & SplittingMode.Level) != 0)
				{
					SplitByLevel(outDir, shortname, basedat);
				}
				if ((splittingMode & SplittingMode.Type) != 0)
				{
					SplitByType(outDir);
				}

				// Now re-empty the DAT to make room for the next one
				DatFormat tempFormat = DatFormat;
				_datHeader = new DatHeader();
				ResetDictionary();
				DatFormat = tempFormat;
			}
		}

		/// <summary>
		/// Split a DAT by input extensions
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="extA">List of extensions to split on (first DAT)</param>
		/// <param name="extB">List of extensions to split on (second DAT)</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByExtension(string outDir, List<string> extA, List<string> extB)
		{
			// Make sure all of the extensions have a dot at the beginning
			List<string> newExtA = new List<string>();
			foreach (string s in extA)
			{
				newExtA.Add((s.StartsWith(".") ? s.Substring(1) : s).ToUpperInvariant());
			}
			string newExtAString = string.Join(",", newExtA);

			List<string> newExtB = new List<string>();
			foreach (string s in extB)
			{
				newExtB.Add((s.StartsWith(".") ? s.Substring(1) : s).ToUpperInvariant());
			}
			string newExtBString = string.Join(",", newExtB);

			// Set all of the appropriate outputs for each of the subsets
			DatFile datdataA = new DatFile
			{
				FileName = this.FileName + " (" + newExtAString + ")",
				Name = this.Name + " (" + newExtAString + ")",
				Description = this.Description + " (" + newExtAString + ")",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				DatFormat = this.DatFormat,
			};
			DatFile datdataB = new DatFile
			{
				FileName = this.FileName + " (" + newExtBString + ")",
				Name = this.Name + " (" + newExtBString + ")",
				Description = this.Description + " (" + newExtBString + ")",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				DatFormat = this.DatFormat,
			};

			// If roms is empty, return false
			if (Count == 0)
			{
				return false;
			}

			// Now separate the roms accordingly
			List<string> keys = Keys;
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				foreach (DatItem item in items)
				{
					if (newExtA.Contains(Utilities.GetExtension(item.Name.ToUpperInvariant())))
					{
						datdataA.Add(key, item);
					}
					else if (newExtB.Contains(Utilities.GetExtension(item.Name.ToUpperInvariant())))
					{
						datdataB.Add(key, item);
					}
					else
					{
						datdataA.Add(key, item);
						datdataB.Add(key, item);
					}
				}
			});

			// Then write out both files
			bool success = datdataA.Write(outDir);
			success &= datdataB.Write(outDir);

			return success;
		}

		/// <summary>
		/// Split a DAT by best available hashes
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByHash(string outDir)
		{
			// Create each of the respective output DATs
			Globals.Logger.User("Creating and populating new DATs");
			DatFile nodump = new DatFile
			{
				FileName = this.FileName + " (Nodump)",
				Name = this.Name + " (Nodump)",
				Description = this.Description + " (Nodump)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile sha512 = new DatFile
			{
				FileName = this.FileName + " (SHA-512)",
				Name = this.Name + " (SHA-512)",
				Description = this.Description + " (SHA-512)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile sha384 = new DatFile
			{
				FileName = this.FileName + " (SHA-384)",
				Name = this.Name + " (SHA-384)",
				Description = this.Description + " (SHA-384)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile sha256 = new DatFile
			{
				FileName = this.FileName + " (SHA-256)",
				Name = this.Name + " (SHA-256)",
				Description = this.Description + " (SHA-256)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile sha1 = new DatFile
			{
				FileName = this.FileName + " (SHA-1)",
				Name = this.Name + " (SHA-1)",
				Description = this.Description + " (SHA-1)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile md5 = new DatFile
			{
				FileName = this.FileName + " (MD5)",
				Name = this.Name + " (MD5)",
				Description = this.Description + " (MD5)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile crc = new DatFile
			{
				FileName = this.FileName + " (CRC)",
				Name = this.Name + " (CRC)",
				Description = this.Description + " (CRC)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile other = new DatFile
			{
				FileName = this.FileName + " (Other)",
				Name = this.Name + " (Other)",
				Description = this.Description + " (Other)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = Keys;
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				foreach (DatItem item in items)
				{
					// If the file is not a Rom or Disk, continue
					if (item.Type != ItemType.Disk && item.Type != ItemType.Rom)
					{
						return;
					}

					// If the file is a nodump
					if ((item.Type == ItemType.Rom && ((Rom)item).ItemStatus == ItemStatus.Nodump)
						|| (item.Type == ItemType.Disk && ((Disk)item).ItemStatus == ItemStatus.Nodump))
					{
						nodump.Add(key, item);
					}
					// If the file has a SHA-512
					else if ((item.Type == ItemType.Rom && !String.IsNullOrWhiteSpace(((Rom)item).SHA512))
						|| (item.Type == ItemType.Disk && !String.IsNullOrWhiteSpace(((Disk)item).SHA512)))
					{
						sha512.Add(key, item);
					}
					// If the file has a SHA-384
					else if ((item.Type == ItemType.Rom && !String.IsNullOrWhiteSpace(((Rom)item).SHA384))
						|| (item.Type == ItemType.Disk && !String.IsNullOrWhiteSpace(((Disk)item).SHA384)))
					{
						sha384.Add(key, item);
					}
					// If the file has a SHA-256
					else if ((item.Type == ItemType.Rom && !String.IsNullOrWhiteSpace(((Rom)item).SHA256))
						|| (item.Type == ItemType.Disk && !String.IsNullOrWhiteSpace(((Disk)item).SHA256)))
					{
						sha256.Add(key, item);
					}
					// If the file has a SHA-1
					else if ((item.Type == ItemType.Rom && !String.IsNullOrWhiteSpace(((Rom)item).SHA1))
						|| (item.Type == ItemType.Disk && !String.IsNullOrWhiteSpace(((Disk)item).SHA1)))
					{
						sha1.Add(key, item);
					}
					// If the file has no SHA-1 but has an MD5
					else if ((item.Type == ItemType.Rom && !String.IsNullOrWhiteSpace(((Rom)item).MD5))
						|| (item.Type == ItemType.Disk && !String.IsNullOrWhiteSpace(((Disk)item).MD5)))
					{
						md5.Add(key, item);
					}
					// If the file has no MD5 but a CRC
					else if ((item.Type == ItemType.Rom && !String.IsNullOrWhiteSpace(((Rom)item).CRC)))
					{
						crc.Add(key, item);
					}
					else
					{
						other.Add(key, item);
					}
				}
			});

			// Now, output all of the files to the output directory
			Globals.Logger.User("DAT information created, outputting new files");
			bool success = true;
			success &= nodump.Write(outDir);
			success &= sha512.Write(outDir);
			success &= sha384.Write(outDir);
			success &= sha256.Write(outDir);
			success &= sha1.Write(outDir);
			success &= md5.Write(outDir);
			success &= crc.Write(outDir);

			return success;
		}

		/// <summary>
		/// Split a SuperDAT by lowest available directory level
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="shortname">True if short names should be used, false otherwise</param>
		/// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByLevel(string outDir, bool shortname, bool basedat)
		{
			// First, organize by games so that we can do the right thing
			BucketBy(SortedBy.Game, DedupeType.None, lower: false, norename: true);

			// Create a temporary DAT to add things to
			DatFile tempDat = new DatFile(this)
			{
				Name = null,
			};

			// Sort the input keys
			List<string> keys = Keys;
			keys.Sort(SplitByLevelSort);

			// Then, we loop over the games
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				// Here, the key is the name of the game to be used for comparison
				if (tempDat.Name != null && tempDat.Name != Path.GetDirectoryName(key))
				{
					// Reset the DAT for the next items
					tempDat = new DatFile(this)
					{
						Name = null,
					};
				}

				// Clean the input list and set all games to be pathless
				List<DatItem> items = this[key];
				items.ForEach(item => item.MachineName = Path.GetFileName(item.MachineName));
				items.ForEach(item => item.MachineDescription = Path.GetFileName(item.MachineDescription));

				// Now add the game to the output DAT
				tempDat.AddRange(key, items);

				// Then set the DAT name to be the parent directory name
				tempDat.Name = Path.GetDirectoryName(key);
			});

			return true;
		}

		/// <summary>
		/// Helper function for SplitByLevel to sort the input game names
		/// </summary>
		/// <param name="a">First string to compare</param>
		/// <param name="b">Second string to compare</param>
		/// <returns>-1 for a coming before b, 0 for a == b, 1 for a coming after b</returns>
		private int SplitByLevelSort(string a, string b)
		{
			NaturalComparer nc = new NaturalComparer();
			int adeep = a.Count(c => c == '/' || c == '\\');
			int bdeep = b.Count(c => c == '/' || c == '\\');

			if (adeep == bdeep)
			{
				return nc.Compare(a, b);
			}
			return adeep - bdeep;
		}

		/// <summary>
		/// Helper function for SplitByLevel to clean and write out a DAT
		/// </summary>
		/// <param name="datFile">DAT to clean and write out</param>
		/// <param name="outDir">Directory to write out to</param>
		/// <param name="shortname">True if short naming scheme should be used, false otherwise</param>
		/// <param name="restore">True if original filenames should be used as the base for output filename, false otherwise</param>
		private void SplitByLevelHelper(DatFile datFile, string outDir, bool shortname, bool restore)
		{
			// Get the name from the DAT to use separately
			string name = datFile.Name;
			string expName = name.Replace("/", " - ").Replace("\\", " - ");

			// Now set the new output values
			datFile.FileName = HttpUtility.HtmlDecode(String.IsNullOrWhiteSpace(name)
				? FileName
				: (shortname
					? Path.GetFileName(name)
					: expName
					)
				);
			datFile.FileName = (restore ? FileName + " (" + datFile.FileName + ")" : datFile.FileName);
			datFile.Name = Name + " (" + expName + ")";
			datFile.Description = (String.IsNullOrWhiteSpace(Description) ? datFile.Name : Description + " (" + expName + ")");
			datFile.Type = null;

			// Write out the temporary DAT to the proper directory
			datFile.Write(outDir);
		}

		/// <summary>
		/// Split a DAT by type of Rom
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByType(string outDir)
		{
			// Create each of the respective output DATs
			Globals.Logger.User("Creating and populating new DATs");
			DatFile romdat = new DatFile
			{
				FileName = this.FileName + " (ROM)",
				Name = this.Name + " (ROM)",
				Description = this.Description + " (ROM)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile diskdat = new DatFile
			{
				FileName = this.FileName + " (Disk)",
				Name = this.Name + " (Disk)",
				Description = this.Description + " (Disk)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};
			DatFile sampledat = new DatFile
			{
				FileName = this.FileName + " (Sample)",
				Name = this.Name + " (Sample)",
				Description = this.Description + " (Sample)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				DedupeRoms = this.DedupeRoms,
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = Keys;
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				foreach (DatItem item in items)
				{
					// If the file is a Rom
					if (item.Type == ItemType.Rom)
					{
						romdat.Add(key, item);
					}
					// If the file is a Disk
					else if (item.Type == ItemType.Disk)
					{
						diskdat.Add(key, item);
					}
					// If the file is a Sample
					else if (item.Type == ItemType.Sample)
					{
						sampledat.Add(key, item);
					}
				}
			});

			// Now, output all of the files to the output directory
			Globals.Logger.User("DAT information created, outputting new files");
			bool success = true;
			success &= romdat.Write(outDir);
			success &= diskdat.Write(outDir);
			success &= sampledat.Write(outDir);

			return success;
		}

		#endregion

		#region Statistics

		/// <summary>
		/// Output the stats for the Dat in a human-readable format
		/// </summary>
		/// <param name="recalculate">True if numbers should be recalculated for the DAT, false otherwise (default)</param>
		/// <param name="game">Number of games to use, -1 means recalculate games (default)</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise (default)</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise (default)</param>
		public void WriteStatsToScreen(bool recalculate = false, long game = -1, bool baddumpCol = false, bool nodumpCol = false)
		{
			// If we're supposed to recalculate the statistics, do so
			if (recalculate)
			{
				RecalculateStats();
			}

			BucketBy(SortedBy.Game, DedupeType.None, norename: true);
			if (TotalSize < 0)
			{
				TotalSize = Int64.MaxValue + TotalSize;
			}

			// Log the results to screen
			string results = @"For '" + FileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Utilities.GetBytesReadable(TotalSize) + @"
    Games found:             " + (game == -1 ? Keys.Count() : game) + @"
    Roms found:              " + RomCount + @"
    Disks found:             " + DiskCount + @"
    Roms with CRC:           " + CRCCount + @"
    Roms with MD5:           " + MD5Count + @"
    Roms with SHA-1:         " + SHA1Count + @"
    Roms with SHA-256:       " + SHA256Count + @"
    Roms with SHA-384:       " + SHA384Count + @"
    Roms with SHA-512:       " + SHA512Count + "\n";

			if (baddumpCol)
			{
				results += "	Roms with BadDump status: " + BaddumpCount + "\n";
			}
			if (nodumpCol)
			{
				results += "	Roms with Nodump status: " + NodumpCount + "\n";
			}

			// For spacing between DATs
			results += "\n\n";

			Globals.Logger.User(results);
		}

		/// <summary>
		/// Recalculate the statistics for the Dat
		/// </summary>
		private void RecalculateStats()
		{
			// Wipe out any stats already there
			_datStats.Reset();

			// If we have a blank Dat in any way, return
			if (this == null || Count == 0)
			{
				return;
			}

			// Loop through and add
			List<string> keys = Keys;
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				foreach (DatItem item in items)
				{
					_datStats.AddItem(item);
				}
			});
		}

		#endregion

		#region Writing

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="outDir">Set the output directory (default current directory)</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		public bool Write(string outDir = null, bool norename = true, bool stats = false, bool ignoreblanks = false, bool overwrite = true)
		{
			// If there's nothing there, abort
			if (Count == 0)
			{
				Globals.Logger.User("There were no items to write out!");
				return false;
			}

			// Ensure the output directory is set and created
			outDir = Utilities.EnsureOutputDirectory(outDir, create: true);

			// If the DAT has no output format, default to XML
			if (DatFormat == 0)
			{
				Globals.Logger.Verbose("No DAT format defined, defaulting to XML");
				DatFormat = DatFormat.Logiqx;
			}

			// Make sure that the three essential fields are filled in
			if (String.IsNullOrWhiteSpace(FileName) && String.IsNullOrWhiteSpace(Name) && String.IsNullOrWhiteSpace(Description))
			{
				FileName = Name = Description = "Default";
			}
			else if (String.IsNullOrWhiteSpace(FileName) && String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Description))
			{
				FileName = Name = Description;
			}
			else if (String.IsNullOrWhiteSpace(FileName) && !String.IsNullOrWhiteSpace(Name) && String.IsNullOrWhiteSpace(Description))
			{
				FileName = Description = Name;
			}
			else if (String.IsNullOrWhiteSpace(FileName) && !String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Description))
			{
				FileName = Description;
			}
			else if (!String.IsNullOrWhiteSpace(FileName) && String.IsNullOrWhiteSpace(Name) && String.IsNullOrWhiteSpace(Description))
			{
				Name = Description = FileName;
			}
			else if (!String.IsNullOrWhiteSpace(FileName) && String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Description))
			{
				Name = Description;
			}
			else if (!String.IsNullOrWhiteSpace(FileName) && !String.IsNullOrWhiteSpace(Name) && String.IsNullOrWhiteSpace(Description))
			{
				Description = Name;
			}
			else if (!String.IsNullOrWhiteSpace(FileName) && !String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Description))
			{
				// Nothing is needed
			}

			// Output initial statistics, for kicks
			if (stats)
			{
				WriteStatsToScreen(recalculate: (RomCount + DiskCount == 0), baddumpCol: true, nodumpCol: true);
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

			// If we are removing hashes, do that now
			if (StripHash != 0x0)
			{
				StripHashesFromItems();
			}

			// If we are removing scene dates, do that now
			if (SceneDateStrip)
			{
				StripSceneDatesFromItems();
			}

			// Get the outfile names
			Dictionary<DatFormat, string> outfiles = CreateOutfileNames(outDir, overwrite);

			try
			{
				// Write out all required formats
				Parallel.ForEach(outfiles.Keys, Globals.ParallelOptions, datFormat =>
				{
					string outfile = outfiles[datFormat];
					try
					{
						Utilities.GetDatFile(datFormat, this)?.WriteToFile(outfile, ignoreblanks);
					}
					catch (Exception ex)
					{
						Globals.Logger.Error("Datfile {0} could not be written out: {1}", outfile, ex.ToString());
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

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="outfile">Name of the file to write to</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		public virtual bool WriteToFile(string outfile, bool ignoreblanks = false)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Generate a proper outfile name based on a DAT and output directory
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="overwrite">True if we ignore existing files (default), false otherwise</param>
		/// <returns>Dictionary of output formats mapped to file names</returns>
		private Dictionary<DatFormat, string> CreateOutfileNames(string outDir, bool overwrite = true)
		{
			// Create the output dictionary
			Dictionary<DatFormat, string> outfileNames = new Dictionary<DatFormat, string>();

			// Double check the outDir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// Get the extensions from the output type

			// AttractMode
			if ((DatFormat & DatFormat.AttractMode) != 0)
			{
				outfileNames.Add(DatFormat.AttractMode, CreateOutfileNamesHelper(outDir, ".txt", overwrite));
			}

			// ClrMamePro
			if ((DatFormat & DatFormat.ClrMamePro) != 0)
			{
				outfileNames.Add(DatFormat.ClrMamePro, CreateOutfileNamesHelper(outDir, ".dat", overwrite));
			};

			// CSV
			if ((DatFormat & DatFormat.CSV) != 0)
			{
				outfileNames.Add(DatFormat.CSV, CreateOutfileNamesHelper(outDir, ".csv", overwrite));
			};

			// DOSCenter
			if ((DatFormat & DatFormat.DOSCenter) != 0
				&& (DatFormat & DatFormat.ClrMamePro) == 0
				&& (DatFormat & DatFormat.RomCenter) == 0)
			{
				outfileNames.Add(DatFormat.DOSCenter, CreateOutfileNamesHelper(outDir, ".dat", overwrite));
			};
			if ((DatFormat & DatFormat.DOSCenter) != 0
				&& ((DatFormat & DatFormat.ClrMamePro) != 0
					|| (DatFormat & DatFormat.RomCenter) != 0))
			{
				outfileNames.Add(DatFormat.DOSCenter, CreateOutfileNamesHelper(outDir, ".dc.dat", overwrite));
			}

			//MAME Listroms
			if ((DatFormat & DatFormat.Listroms) != 0
				&& (DatFormat & DatFormat.AttractMode) == 0)
			{
				outfileNames.Add(DatFormat.Listroms, CreateOutfileNamesHelper(outDir, ".txt", overwrite));
			}
			if ((DatFormat & DatFormat.Listroms) != 0
				&& (DatFormat & DatFormat.AttractMode) != 0)
			{
				outfileNames.Add(DatFormat.Listroms, CreateOutfileNamesHelper(outDir, ".lr.txt", overwrite));
			}

			// Logiqx XML
			if ((DatFormat & DatFormat.Logiqx) != 0)
			{
				outfileNames.Add(DatFormat.Logiqx, CreateOutfileNamesHelper(outDir, ".xml", overwrite));
			}

			// Missfile
			if ((DatFormat & DatFormat.MissFile) != 0
				&& (DatFormat & DatFormat.AttractMode) == 0)
			{
				outfileNames.Add(DatFormat.MissFile, CreateOutfileNamesHelper(outDir, ".txt", overwrite));
			}
			if ((DatFormat & DatFormat.MissFile) != 0
				&& (DatFormat & DatFormat.AttractMode) != 0)
			{
				outfileNames.Add(DatFormat.MissFile, CreateOutfileNamesHelper(outDir, ".miss.txt", overwrite));
			}

			// OfflineList
			if (((DatFormat & DatFormat.OfflineList) != 0)
				&& (DatFormat & DatFormat.Logiqx) == 0
				&& (DatFormat & DatFormat.SabreDat) == 0
				&& (DatFormat & DatFormat.SoftwareList) == 0)
			{
				outfileNames.Add(DatFormat.OfflineList, CreateOutfileNamesHelper(outDir, ".xml", overwrite));
			}
			if (((DatFormat & DatFormat.OfflineList) != 0
				&& ((DatFormat & DatFormat.Logiqx) != 0
					|| (DatFormat & DatFormat.SabreDat) != 0
					|| (DatFormat & DatFormat.SoftwareList) != 0)))
			{
				outfileNames.Add(DatFormat.OfflineList, CreateOutfileNamesHelper(outDir, ".ol.xml", overwrite));
			}

			// Redump MD5
			if ((DatFormat & DatFormat.RedumpMD5) != 0)
			{
				outfileNames.Add(DatFormat.RedumpMD5, CreateOutfileNamesHelper(outDir, ".md5", overwrite));
			};

			// Redump SFV
			if ((DatFormat & DatFormat.RedumpSFV) != 0)
			{
				outfileNames.Add(DatFormat.RedumpSFV, CreateOutfileNamesHelper(outDir, ".sfv", overwrite));
			};

			// Redump SHA-1
			if ((DatFormat & DatFormat.RedumpSHA1) != 0)
			{
				outfileNames.Add(DatFormat.RedumpSHA1, CreateOutfileNamesHelper(outDir, ".sha1", overwrite));
			};

			// Redump SHA-256
			if ((DatFormat & DatFormat.RedumpSHA256) != 0)
			{
				outfileNames.Add(DatFormat.RedumpSHA256, CreateOutfileNamesHelper(outDir, ".sha256", overwrite));
			};

			// RomCenter
			if ((DatFormat & DatFormat.RomCenter) != 0
				&& (DatFormat & DatFormat.ClrMamePro) == 0)
			{
				outfileNames.Add(DatFormat.RomCenter, CreateOutfileNamesHelper(outDir, ".dat", overwrite));
			};
			if ((DatFormat & DatFormat.RomCenter) != 0
				&& (DatFormat & DatFormat.ClrMamePro) != 0)
			{
				outfileNames.Add(DatFormat.RomCenter, CreateOutfileNamesHelper(outDir, ".rc.dat", overwrite));
			};

			// SabreDAT
			if ((DatFormat & DatFormat.SabreDat) != 0 && (DatFormat & DatFormat.Logiqx) == 0)
			{
				outfileNames.Add(DatFormat.SabreDat, CreateOutfileNamesHelper(outDir, ".xml", overwrite));
			};
			if ((DatFormat & DatFormat.SabreDat) != 0 && (DatFormat & DatFormat.Logiqx) != 0)
			{
				outfileNames.Add(DatFormat.SabreDat, CreateOutfileNamesHelper(outDir, ".sd.xml", overwrite));
			};

			// Software List
			if ((DatFormat & DatFormat.SoftwareList) != 0
				&& (DatFormat & DatFormat.Logiqx) == 0
				&& (DatFormat & DatFormat.SabreDat) == 0)
			{
				outfileNames.Add(DatFormat.SoftwareList, CreateOutfileNamesHelper(outDir, ".xml", overwrite));
			}
			if ((DatFormat & DatFormat.SoftwareList) != 0
				&& ((DatFormat & DatFormat.Logiqx) != 0
					|| (DatFormat & DatFormat.SabreDat) != 0))
			{
				outfileNames.Add(DatFormat.SoftwareList, CreateOutfileNamesHelper(outDir, ".sl.xml", overwrite));
			}

			// TSV
			if ((DatFormat & DatFormat.TSV) != 0)
			{
				outfileNames.Add(DatFormat.TSV, CreateOutfileNamesHelper(outDir, ".tsv", overwrite));
			};

			return outfileNames;
		}

		/// <summary>
		/// Help generating the outfile name
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="extension">Extension to use for the file</param>
		/// <param name="overwrite">True if we ignore existing files, false otherwise</param>
		/// <returns>String containing the new filename</returns>
		private string CreateOutfileNamesHelper(string outDir, string extension, bool overwrite)
		{
			string filename = (String.IsNullOrWhiteSpace(FileName) ? Description : FileName);
			string outfile = outDir + filename + extension;
			outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
				outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
				outfile);
			if (!overwrite)
			{
				int i = 1;
				while (File.Exists(outfile))
				{
					outfile = outDir + filename + "_" + i + extension;
					outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
						outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
						outfile);
					i++;
				}
			}

			return outfile;
		}

		#endregion

		#endregion // Instance Methods

		#region Static Methods

		#region Statistics

		/// <summary>
		/// Output the stats for a list of input dats as files in a human-readable format
		/// </summary>
		/// <param name="inputs">List of input files and folders</param>
		/// <param name="reportName">Name of the output file</param>
		/// <param name="single">True if single DAT stats are output, false otherwise</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		/// <param name="statDatFormat" > Set the statistics output format to use</param>
		public static void OutputStats(List<string> inputs, string reportName, string outDir, bool single,
			bool baddumpCol, bool nodumpCol, StatReportFormat statDatFormat)
		{
			// If there's no output format, set the default
			if (statDatFormat == StatReportFormat.None)
			{
				statDatFormat = StatReportFormat.Textfile;
			}

			// Get the proper output file name
			if (String.IsNullOrWhiteSpace(reportName))
			{
				reportName = "report";
			}

			// Get the proper output directory name
			outDir = Utilities.EnsureOutputDirectory(outDir);

			// Get the dictionary of desired output report names
			Dictionary<StatReportFormat, string> outputs = CreateOutStatsNames(outDir, statDatFormat, reportName);

			// Make sure we have all files and then order them
			List<string> files = Utilities.GetOnlyFilesFromInputs(inputs);
			files = files
				.OrderBy(i => Path.GetDirectoryName(i))
				.ThenBy(i => Path.GetFileName(i))
				.ToList();

			// Get all of the writers that we need
			List<BaseReport> reports = new List<BaseReport>();

			// Loop through and output based on the inputs
			foreach (KeyValuePair<StatReportFormat, string> kvp in outputs)
			{
				reports.Add(Utilities.GetBaseReport(kvp.Key, kvp.Value, baddumpCol, nodumpCol));
			}

			// Write the header, if any
			reports.ForEach(report => report.WriteHeader());

			// Init all total variables
			DatStats totalStats = new DatStats();

			// Init directory-level variables
			string lastdir = null;
			string basepath = null;
			DatStats dirStats = new DatStats();

			// Now process each of the input files
			foreach (string file in files)
			{
				// Get the directory for the current file
				string thisdir = Path.GetDirectoryName(file);
				basepath = Path.GetDirectoryName(Path.GetDirectoryName(file));

				// If we don't have the first file and the directory has changed, show the previous directory stats and reset
				if (lastdir != null && thisdir != lastdir)
				{
					// Output separator if needed
					reports.ForEach(report => report.WriteMidSeparator());

					DatFile lastdirdat = new DatFile
					{
						FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir),
						_datStats = dirStats,
					};

					lastdirdat.WriteStatsToScreen(recalculate: false, game: dirStats.GameCount, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
					reports.ForEach(report => report.ReplaceDatFile(lastdirdat));
					reports.ForEach(report => report.Write(game: dirStats.GameCount));

					// Write the mid-footer, if any
					reports.ForEach(report => report.WriteFooterSeparator());

					// Write the header, if any
					reports.ForEach(report => report.WriteMidHeader());

					// Reset the directory stats
					dirStats.Reset();
				}

				Globals.Logger.Verbose("Beginning stat collection for '{0}'", false, file);
				List<string> games = new List<string>();
				DatFile datdata = new DatFile();
				datdata.Parse(file, 0, 0);
				datdata.BucketBy(SortedBy.Game, DedupeType.None, norename: true);

				// Output single DAT stats (if asked)
				Globals.Logger.User("Adding stats for file '{0}'\n", false, file);
				if (single)
				{
					datdata.WriteStatsToScreen(recalculate: false, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
					reports.ForEach(report => report.ReplaceDatFile(datdata));
					reports.ForEach(report => report.Write());
				}

				// Add single DAT stats to dir
				dirStats.AddStats(datdata._datStats);
				dirStats.GameCount += datdata.Keys.Count();

				// Add single DAT stats to totals
				totalStats.AddStats(datdata._datStats);
				totalStats.GameCount += datdata.Keys.Count();

				// Make sure to assign the new directory
				lastdir = thisdir;
			}

			// Output the directory stats one last time
			reports.ForEach(report => report.WriteMidSeparator());

			if (single)
			{
				DatFile dirdat = new DatFile
				{
					FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir),
					_datStats = dirStats,
				};

				dirdat.WriteStatsToScreen(recalculate: false, game: dirStats.GameCount, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
				reports.ForEach(report => report.ReplaceDatFile(dirdat));
				reports.ForEach(report => report.Write(dirStats.GameCount));
			}

			// Write the mid-footer, if any
			reports.ForEach(report => report.WriteFooterSeparator());

			// Write the header, if any
			reports.ForEach(report => report.WriteMidHeader());

			// Reset the directory stats
			dirStats.Reset();

			// Output total DAT stats
			DatFile totaldata = new DatFile
			{
				FileName = "DIR: All DATs",
				_datStats = totalStats,
			};

			totaldata.WriteStatsToScreen(recalculate: false, game: totalStats.GameCount, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
			reports.ForEach(report => report.ReplaceDatFile(totaldata));
			reports.ForEach(report => report.Write(totalStats.GameCount));

			// Output footer if needed
			reports.ForEach(report => report.WriteFooter());

			Globals.Logger.User(@"
Please check the log folder if the stats scrolled offscreen", false);
		}

		/// <summary>
		/// Get the proper extension for the stat output format
		/// </summary>
		/// <param name="outDir">Output path to use</param>
		/// <param name="statDatFormat">StatDatFormat to get the extension for</param>
		/// <param name="reportName">Name of the input file to use</param>
		/// <returns>Dictionary of output formats mapped to file names</returns>
		private static Dictionary<StatReportFormat, string> CreateOutStatsNames(string outDir, StatReportFormat statDatFormat, string reportName, bool overwrite = true)
		{
			Dictionary<StatReportFormat, string> output = new Dictionary<StatReportFormat, string>();

			// First try to create the output directory if we need to
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// Double check the outDir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// For each output format, get the appropriate stream writer
			if ((statDatFormat & StatReportFormat.Textfile) != 0)
			{
				output.Add(StatReportFormat.Textfile, CreateOutStatsNamesHelper(outDir, ".txt", reportName, overwrite));
			}
			if ((statDatFormat & StatReportFormat.CSV) != 0)
			{
				output.Add(StatReportFormat.CSV, CreateOutStatsNamesHelper(outDir, ".csv", reportName, overwrite));
			}
			if ((statDatFormat & StatReportFormat.HTML) != 0)
			{
				output.Add(StatReportFormat.HTML, CreateOutStatsNamesHelper(outDir, ".html", reportName, overwrite));
			}
			if ((statDatFormat & StatReportFormat.TSV) != 0)
			{
				output.Add(StatReportFormat.TSV, CreateOutStatsNamesHelper(outDir, ".tsv", reportName, overwrite));
			}

			return output;
		}

		/// <summary>
		/// Help generating the outstats name
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="extension">Extension to use for the file</param>
		/// <param name="reportName">Name of the input file to use</param>
		/// <param name="overwrite">True if we ignore existing files, false otherwise</param>
		/// <returns>String containing the new filename</returns>
		private static string CreateOutStatsNamesHelper(string outDir, string extension, string reportName, bool overwrite)
		{
			string outfile = outDir + reportName + extension;
			outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
				outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
				outfile);
			if (!overwrite)
			{
				int i = 1;
				while (File.Exists(outfile))
				{
					outfile = outDir + reportName + "_" + i + extension;
					outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
						outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
						outfile);
					i++;
				}
			}

			return outfile;
		}

		#endregion

		#endregion // Static Methods
	}
}
