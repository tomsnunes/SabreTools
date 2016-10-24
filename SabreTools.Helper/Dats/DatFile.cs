using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

using NaturalSort;
using SharpCompress.Common;

namespace SabreTools.Helper.Dats
{
	[Serializable]
	public class DatFile : ICloneable
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
		private OutputFormat _outputFormat;
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
		public OutputFormat OutputFormat
		{
			get { return _outputFormat; }
			set { _outputFormat = value; }
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
		public SortedDictionary<string, List<DatItem>> Files
		{
			get
			{
				if (_files == null)
				{
					_files = new SortedDictionary<string, List<DatItem>>();
				}
				return _files;
			}
			set
			{
				_files = value;
			}
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

		#region Bucketing

		/// <summary>
		/// Take the arbitrarily sorted Files Dictionary and convert to one sorted by Game
		/// </summary>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		public void BucketByGame(bool mergeroms, bool norename, Logger logger, bool output = true)
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
								: rom.Machine.Name.ToLowerInvariant());
					newkey = HttpUtility.HtmlEncode(newkey);
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(rom);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(rom);
						sortable.Add(newkey, temp);
					}
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
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(rom);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(rom);
						sortable.Add(newkey, temp);
					}
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
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(rom);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(rom);
						sortable.Add(newkey, temp);
					}
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
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(rom);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(rom);
						sortable.Add(newkey, temp);
					}
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
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(rom);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(rom);
						sortable.Add(newkey, temp);
					}
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

		#region Cloning Methods

		public object Clone()
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
				OutputFormat = _outputFormat,
				MergeRoms = _mergeRoms,
				Files = _files,
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
				OutputFormat = _outputFormat,
				MergeRoms = _mergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
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

		#region Converting and Updating

		/// <summary>
		/// Convert, update, and filter a DAT file or set of files using a base
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="softlist">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void Update(List<string> inputFileNames, string outDir, bool merge, DiffMode diff, bool inplace, bool skip,
			bool bare, bool clean, bool softlist, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc,
			string md5, string sha1, ItemStatus itemStatus, bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || diff != 0)
			{
				// Make sure there are no folders in inputs
				List<string> newInputFileNames = new List<string>();
				foreach (string input in inputFileNames)
				{
					if (Directory.Exists(input))
					{
						List<string> files = FileTools.RetrieveFiles(input, new List<string>());
						foreach (string file in files)
						{
							try
							{
								newInputFileNames.Add(Path.GetFullPath(file) + "¬" + Path.GetFullPath(input));
							}
							catch (PathTooLongException)
							{
								logger.Warning("The path for " + file + " was too long");
							}
							catch (Exception ex)
							{
								logger.Error(ex.ToString());
							}
						}
					}
					else if (File.Exists(input))
					{
						try
						{
							newInputFileNames.Add(Path.GetFullPath(input) + "¬" + Path.GetDirectoryName(Path.GetFullPath(input)));
						}
						catch (PathTooLongException)
						{
							logger.Warning("The path for " + input + " was too long");
						}
						catch (Exception ex)
						{
							logger.Error(ex.ToString());
						}
					}
				}

				// If we're in inverse cascade, reverse the list
				if ((diff & DiffMode.ReverseCascade) != 0)
				{
					newInputFileNames.Reverse();
				}

				// Create a dictionary of all ROMs from the input DATs
				List<DatFile> datHeaders = PopulateUserData(newInputFileNames, inplace, clean, softlist,
					outDir, gamename, romname, romtype, sgt, slt, seq,
					crc, md5, sha1, itemStatus, trim, single, root, maxDegreeOfParallelism, logger);

				// Modify the Dictionary if necessary and output the results
				if (diff != 0 && diff < DiffMode.Cascade)
				{
					DiffNoCascade(diff, outDir, newInputFileNames, logger);
				}
				// If we're in cascade and diff, output only cascaded diffs
				else if (diff != 0 && diff >= DiffMode.Cascade)
				{
					DiffCascade(outDir, inplace, newInputFileNames, datHeaders, skip, logger);
				}
				// Output all entries with user-defined merge
				else
				{
					MergeNoDiff(outDir, newInputFileNames, datHeaders, logger);
				}
			}
			// Otherwise, loop through all of the inputs individually
			else
			{
				Parallel.ForEach(inputFileNames,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
					inputFileName =>
					{
						// Clean the input string
						if (inputFileName != "")
						{
							inputFileName = Path.GetFullPath(inputFileName);
						}

						if (File.Exists(inputFileName))
						{
							DatFile innerDatdata = (DatFile)CloneHeader();
							logger.User("Processing \"" + Path.GetFileName(inputFileName) + "\"");
							innerDatdata.Parse(inputFileName, 0, 0, gamename, romname,
								romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single,
								root, logger, true, clean, softlist,
								keepext: ((innerDatdata.OutputFormat & OutputFormat.TSV) != 0 || (innerDatdata.OutputFormat & OutputFormat.CSV) != 0));

							// If we have roms, output them
							if (innerDatdata.Files.Count != 0)
							{
								innerDatdata.WriteToFile((outDir == "" ? Path.GetDirectoryName(inputFileName) : outDir), logger, overwrite: (outDir != ""));
							}
						}
						else if (Directory.Exists(inputFileName))
						{
							inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

							Parallel.ForEach(Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories),
								new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
								file =>
								{
									logger.User("Processing \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
									DatFile innerDatdata = (DatFile)Clone();
									innerDatdata.Files = null;
									innerDatdata.Parse(file, 0, 0, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus,
										trim, single, root, logger, true, clean, softlist,
										keepext: ((innerDatdata.OutputFormat & OutputFormat.TSV) != 0 || (innerDatdata.OutputFormat & OutputFormat.CSV) != 0));

									// If we have roms, output them
									if (innerDatdata.Files != null && innerDatdata.Files.Count != 0)
									{
										innerDatdata.WriteToFile((outDir == "" ? Path.GetDirectoryName(file) : outDir + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)), logger, overwrite: (outDir != ""));
									}
								});
						}
						else
						{
							logger.Error("I'm sorry but " + inputFileName + " doesn't exist!");
						}
					});
			}
			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		/// <returns>List of DatData objects representing headers</returns>
		private List<DatFile> PopulateUserData(List<string> inputs, bool inplace, bool clean, bool softlist, string outDir,
			string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc,
			string md5, string sha1, ItemStatus itemStatus, bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			DatFile[] datHeaders = new DatFile[inputs.Count];
			DateTime start = DateTime.Now;
			logger.User("Processing individual DATs");

			Parallel.For(0,
				inputs.Count,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				i =>
				{
					string input = inputs[i];
					logger.User("Adding DAT: " + input.Split('¬')[0]);
					datHeaders[i] = new DatFile
					{
						OutputFormat = (OutputFormat != 0 ? OutputFormat : 0),
						Files = new SortedDictionary<string, List<DatItem>>(),
						MergeRoms = MergeRoms,
					};

					datHeaders[i].Parse(input.Split('¬')[0], i, 0, gamename, romname, romtype, sgt, slt, seq,
						crc, md5, sha1, itemStatus, trim, single, root, logger, true, clean, softlist);
				});

			logger.User("Processing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			logger.User("Populating internal DAT");
			Files = new SortedDictionary<string, List<DatItem>>();
			for (int i = 0; i < inputs.Count; i++)
			{
				List<string> keys = datHeaders[i].Files.Keys.ToList();
				foreach (string key in keys)
				{
					if (Files.ContainsKey(key))
					{
						Files[key].AddRange(datHeaders[i].Files[key]);
					}
					else
					{
						Files.Add(key, datHeaders[i].Files[key]);
					}
					datHeaders[i].Files.Remove(key);
				}
				datHeaders[i].Files = null;
			}

			logger.User("Processing and populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datHeaders.ToList();
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void DiffNoCascade(DiffMode diff, string outDir, List<string> inputs, Logger logger)
		{
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			// Default vars for use
			string post = "";
			DatFile outerDiffData = new DatFile();
			DatFile dupeData = new DatFile();

			// Don't have External dupes
			if ((diff & DiffMode.NoDupes) != 0)
			{
				post = " (No Duplicates)";
				outerDiffData = (DatFile)CloneHeader();
				outerDiffData.FileName += post;
				outerDiffData.Name += post;
				outerDiffData.Description += post;
				outerDiffData.Files = new SortedDictionary<string, List<DatItem>>();
			}

			// Have External dupes
			if ((diff & DiffMode.Dupes) != 0)
			{
				post = " (Duplicates)";
				dupeData = (DatFile)CloneHeader();
				dupeData.FileName += post;
				dupeData.Name += post;
				dupeData.Description += post;
				dupeData.Files = new SortedDictionary<string, List<DatItem>>();
			}

			// Create a list of DatData objects representing individual output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			if ((diff & DiffMode.Individuals) != 0)
			{
				DatFile[] outDatsArray = new DatFile[inputs.Count];

				Parallel.For(0, inputs.Count, j =>
				{
					string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
					DatFile diffData = (DatFile)CloneHeader();
					diffData.FileName += innerpost;
					diffData.Name += innerpost;
					diffData.Description += innerpost;
					diffData.Files = new SortedDictionary<string, List<DatItem>>();
					outDatsArray[j] = diffData;
				});

				outDats = outDatsArray.ToList();
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(Files[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (DatItem rom in roms)
					{
						// No duplicates
						if ((diff & DiffMode.NoDupes) != 0 || (diff & DiffMode.Individuals) != 0)
						{
							if ((rom.Dupe & DupeType.Internal) != 0)
							{
								// Individual DATs that are output
								if ((diff & DiffMode.Individuals) != 0)
								{
									if (outDats[rom.SystemID].Files.ContainsKey(key))
									{
										outDats[rom.SystemID].Files[key].Add(rom);
									}
									else
									{
										List<DatItem> tl = new List<DatItem>();
										tl.Add(rom);
										outDats[rom.SystemID].Files.Add(key, tl);
									}
								}

								// Merged no-duplicates DAT
								if ((diff & DiffMode.NoDupes) != 0)
								{
									DatItem newrom = rom;
									newrom.Machine.Name += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

									if (outerDiffData.Files.ContainsKey(key))
									{
										outerDiffData.Files[key].Add(newrom);
									}
									else
									{
										List<DatItem> tl = new List<DatItem>();
										tl.Add(rom);
										outerDiffData.Files.Add(key, tl);
									}
								}
							}
						}

						// Duplicates only
						if ((diff & DiffMode.Dupes) != 0)
						{
							if ((rom.Dupe & DupeType.External) != 0)
							{
								DatItem newrom = rom;
								newrom.Machine.Name += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

								if (dupeData.Files.ContainsKey(key))
								{
									dupeData.Files[key].Add(newrom);
								}
								else
								{
									List<DatItem> tl = new List<DatItem>();
									tl.Add(rom);
									dupeData.Files.Add(key, tl);
								}
							}
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");

			// Output the difflist (a-b)+(b-a) diff
			if ((diff & DiffMode.NoDupes) != 0)
			{
				outerDiffData.WriteToFile(outDir, logger);
			}

			// Output the (ab) diff
			if ((diff & DiffMode.Dupes) != 0)
			{
				dupeData.WriteToFile(outDir, logger);
			}

			// Output the individual (a-b) DATs
			if ((diff & DiffMode.Individuals) != 0)
			{
				for (int j = 0; j < inputs.Count; j++)
				{
					// If we have an output directory set, replace the path
					string path = outDir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));

					// If we have more than 0 roms, output
					if (outDats[j].Files.Count > 0)
					{
						outDats[j].WriteToFile(path, logger);
					}
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output cascading diffs
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void DiffCascade(string outDir, bool inplace, List<string> inputs, List<DatFile> datHeaders, bool skip, Logger logger)
		{
			string post = "";

			// Create a list of DatData objects representing output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			DatFile[] outDatsArray = new DatFile[inputs.Count];

			Parallel.For(0, inputs.Count, j =>
			{
				string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				DatFile diffData;

				// If we're in inplace mode, take the appropriate DatData object already stored
				if (inplace || !String.IsNullOrEmpty(outDir))
				{
					diffData = datHeaders[j];
				}
				else
				{
					diffData = (DatFile)CloneHeader();
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
				}
				diffData.Files = new SortedDictionary<string, List<DatItem>>();

				outDatsArray[j] = diffData;
			});

			outDats = outDatsArray.ToList();
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = Files.Keys.ToList();

			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(Files[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (DatItem rom in roms)
					{
						// There's odd cases where there are items with System ID < 0. Skip them for now
						if (rom.SystemID < 0)
						{
							logger.Warning("Item found with a <0 SystemID: " + rom.Name);
							continue;
						}

						if (outDats[rom.SystemID].Files.ContainsKey(key))
						{
							outDats[rom.SystemID].Files[key].Add(rom);
						}
						else
						{
							List<DatItem> tl = new List<DatItem>();
							tl.Add(rom);
							outDats[rom.SystemID].Files.Add(key, tl);
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");
			for (int j = (skip ? 1 : 0); j < inputs.Count; j++)
			{
				// If we have an output directory set, replace the path
				string path = "";
				if (inplace)
				{
					path = Path.GetDirectoryName(inputs[j].Split('¬')[0]);
				}
				else if (!String.IsNullOrEmpty(outDir))
				{
					path = outDir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));
				}

				// If we have more than 0 roms, output
				if (outDats[j].Files.Count > 0)
				{
					outDats[j].WriteToFile(path, logger);
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output user defined merge
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void MergeNoDiff(string outDir, List<string> inputs, List<DatFile> datHeaders, Logger logger)
		{
			// If we're in SuperDAT mode, prefix all games with their respective DATs
			if (Type == "SuperDAT")
			{
				List<string> keys = Files.Keys.ToList();
				foreach (string key in keys)
				{
					List<DatItem> newroms = new List<DatItem>();
					foreach (DatItem rom in Files[key])
					{
						DatItem newrom = rom;
						string filename = inputs[newrom.SystemID].Split('¬')[0];
						string rootpath = inputs[newrom.SystemID].Split('¬')[1];

						rootpath += (rootpath == "" ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newrom.Machine.Name = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
							+ Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
							+ newrom.Machine.Name;
						newroms.Add(newrom);
					}
					Files[key] = newroms;
				}
			}

			// Output a DAT only if there are roms
			if (Files.Count != 0)
			{
				WriteToFile(outDir, logger);
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
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		public void Parse(string filename, int sysid, int srcid, Logger logger, bool keep = false, bool clean = false, bool softlist = false, bool keepext = false)
		{
			Parse(filename, sysid, srcid, null, null, null, -1, -1, -1, null, null, null, ItemStatus.NULL, false, false, "", logger, keep, clean, softlist, keepext);
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		public void Parse(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			ItemStatus itemStatus,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep = false,
			bool clean = false,
			bool softlist = false,
			bool keepext = false)
		{
			// Check the file extension first as a safeguard
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}
			if (ext != "dat" && ext != "md5" && ext != "sfv" && ext != "sha1" && ext != "txt" && ext != "xml")
			{
				return;
			}

			// If the output filename isn't set already, get the internal filename
			FileName = (String.IsNullOrEmpty(FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : FileName);

			// If the output type isn't set already, get the internal output type
			OutputFormat = (OutputFormat == 0 ? FileTools.GetOutputFormat(filename, logger) : OutputFormat);

			// Make sure there's a dictionary to read to
			if (Files == null)
			{
				Files = new SortedDictionary<string, List<DatItem>>();
			}

			// Now parse the correct type of DAT
			switch (FileTools.GetOutputFormat(filename, logger))
			{
				case OutputFormat.ClrMamePro:
				case OutputFormat.DOSCenter:
					ParseCMP(filename, sysid, srcid, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, logger, keep, clean);
					break;
				case OutputFormat.Logiqx:
				case OutputFormat.OfflineList:
				case OutputFormat.SabreDat:
				case OutputFormat.SoftwareList:
					ParseGenericXML(filename, sysid, srcid, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, logger, keep, clean, softlist);
					break;
				case OutputFormat.RedumpMD5:
					ParseRedumpMD5(filename, sysid, srcid, romname, md5, trim, single, root, logger, clean);
					break;
				case OutputFormat.RedumpSFV:
					ParseRedumpSFV(filename, sysid, srcid, romname, crc, trim, single, root, logger, clean);
					break;
				case OutputFormat.RedumpSHA1:
					ParseRedumpSHA1(filename, sysid, srcid, romname, sha1, trim, single, root, logger, clean);
					break;
				case OutputFormat.RomCenter:
					ParseRC(filename, sysid, srcid, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, logger, clean);
					break;
				default:
					return;
			}
		}

		/// <summary>
		/// Parse a ClrMamePro DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseCMP(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			ItemStatus itemStatus,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			bool block = false, superdat = false;
			string blockname = "", tempgamename = "", gamedesc = "", cloneof = "",
				romof = "", sampleof = "", year = "", manufacturer = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// Comments in CMP DATs start with a #
				if (line.Trim().StartsWith("#"))
				{
					continue;
				}

				// If the line is the header or a game
				if (Regex.IsMatch(line, Constants.HeaderPatternCMP))
				{
					GroupCollection gc = Regex.Match(line, Constants.HeaderPatternCMP).Groups;

					if (gc[1].Value == "clrmamepro" || gc[1].Value == "romvault" || gc[1].Value.ToLowerInvariant() == "doscenter")
					{
						blockname = "header";
					}

					block = true;
				}

				// If the line is a rom-like item and we're in a block
				else if ((line.Trim().StartsWith("rom (")
						|| line.Trim().StartsWith("disk (")
						|| line.Trim().StartsWith("file (")
						|| (line.Trim().StartsWith("sample") && !line.Trim().StartsWith("sampleof"))
					) && block)
				{
					ItemType temptype = ItemType.Rom;
					if (line.Trim().StartsWith("rom ("))
					{
						temptype = ItemType.Rom;
					}
					else if (line.Trim().StartsWith("disk ("))
					{
						temptype = ItemType.Disk;
					}
					else if (line.Trim().StartsWith("file ("))
					{
						temptype = ItemType.Rom;
					}
					else if (line.Trim().StartsWith("sample"))
					{
						temptype = ItemType.Sample;
					}

					// Create the proper DatItem based on the type
					DatItem item;
					switch (temptype)
					{
						case ItemType.Archive:
							item = new Archive();
							break;
						case ItemType.BiosSet:
							item = new BiosSet();
							break;
						case ItemType.Disk:
							item = new Disk();
							break;
						case ItemType.Release:
							item = new Release();
							break;
						case ItemType.Sample:
							item = new Sample();
							break;
						case ItemType.Rom:
						default:
							item = new Rom();
							break;
					}

					// Then populate it with information
					item.Machine.Name = tempgamename;
					item.Machine.Description = gamedesc;
					item.Machine.CloneOf = cloneof;
					item.Machine.RomOf = romof;
					item.Machine.SampleOf = sampleof;
					item.Machine.Manufacturer = manufacturer;
					item.Machine.Year = year;
					item.SystemID = sysid;
					item.SourceID = srcid;

					// If we have a sample, treat it special
					if (temptype == ItemType.Sample)
					{
						line = line.Trim().Remove(0, 6).Trim().Replace("\"", ""); // Remove "sample" from the input string
						item.Name = line;
					}

					// Otherwise, process the rest of the line
					else
					{
						string[] gc = line.Trim().Split(' ');

						// Loop over all attributes and add them if possible
						bool quote = false;
						string attrib = "", val = "";
						for (int i = 2; i < gc.Length; i++)
						{
							//If the item is empty, we automatically skip it because it's a fluke
							if (gc[i].Trim() == String.Empty)
							{
								continue;
							}

							// Special cases for standalone item statuses
							else if (gc[i] == "baddump" && attrib != "name" && attrib != "status" && attrib != "flags")
							{
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.BadDump;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.BadDump;
								}
							}
							else if (gc[i] == "good" && attrib != "name" && attrib != "status" && attrib != "flags")
							{
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.Good;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.Good;
								}
							}
							else if (gc[i] == "nodump" && attrib != "name" && attrib != "status" && attrib != "flags")
							{
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.Nodump;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.Nodump;
								}
							}
							else if (gc[i] == "verified" && attrib != "name" && attrib != "status" && attrib != "flags")
							{
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.Verified;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.Verified;
								}
							}

							// Special cases for DOSCenter DATs only
							else if (line.Trim().StartsWith("file ("))
							{
								// Loop over the specifics
								for (int j = i; j < gc.Length; j++)
								{
									// Names are not quoted, for some stupid reason
									if (gc[j] == "name")
									{
										// Advance to the first part of the name
										j++;
										item.Name = gc[j];

										// Advance to the next item, adding until we find "size"
										j++;
										while (j < gc.Length && gc[j] != "size" && gc[j] != "date" && gc[j] != "crc")
										{
											item.Name += " " + gc[j];
											j++;
										}
									}

									// Get the size from the next part
									else if (gc[j] == "size")
									{
										j++;
										long tempsize = -1;
										if (!Int64.TryParse(gc[j], out tempsize))
										{
											tempsize = 0;
										}
										((Rom)item).Size = tempsize;
										j++;
									}

									// Get the date from the next part
									else if (gc[j] == "date")
									{
										j++;
										((Rom)item).Date = gc[j].Replace("\"", "") + " " + gc[j + 1].Replace("\"", "");
										j += 3;
									}

									// Get the CRC from the next part
									else if (gc[j] == "crc")
									{
										j++;
										((Rom)item).CRC = gc[j].Replace("\"", "").ToLowerInvariant();
									}
								}
							}

							// Even number of quotes, not in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib == "")
							{
								attrib = gc[i].Replace("\"", "");
							}
							// Even number of quotes, not in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib != "")
							{
								switch (attrib.ToLowerInvariant())
								{
									case "name":
										item.Name = gc[i].Replace("\"", "");
										break;
									case "size":
										if (item.Type == ItemType.Rom)
										{
											long size = -1;
											if (Int64.TryParse(gc[i].Replace("\"", ""), out size))
											{
												((Rom)item).Size = size;
											}
										}
										
										break;
									case "crc":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).CRC = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "md5":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "sha1":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "status":
									case "flags":
										if (gc[i].ToLowerInvariant() == "good")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.Good;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.Good;
											}
										}
										else if (gc[i].ToLowerInvariant() == "baddump")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.BadDump;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.BadDump;
											}
										}
										else if (gc[i].ToLowerInvariant() == "nodump")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.Nodump;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.Nodump;
											}
										}
										else if (gc[i].ToLowerInvariant() == "verified")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.Verified;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.Verified;
											}
										}
										break;
									case "date":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).Date = gc[i].Replace("\"", "") + " " + gc[i + 1].Replace("\"", "");
										}
										i++;
										break;

									// Special cases for item statuses
									case "good":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).ItemStatus = ItemStatus.Good;
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).ItemStatus = ItemStatus.Good;
										}
										break;
									case "baddump":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).ItemStatus = ItemStatus.BadDump;
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).ItemStatus = ItemStatus.BadDump;
										}
										break;
									case "nodump":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).ItemStatus = ItemStatus.Nodump;
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).ItemStatus = ItemStatus.Nodump;
										}
										break;
									case "verified":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).ItemStatus = ItemStatus.Verified;
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).ItemStatus = ItemStatus.Verified;
										}
										break;
								}

								attrib = "";
							}
							// Even number of quotes, in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && quote && attrib == "")
							{
								// Attributes can't have quoted names
							}
							// Even number of quotes, in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && quote && attrib != "")
							{
								val += " " + gc[i];
							}
							// Odd number of quotes, not in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && !quote && attrib == "")
							{
								// Attributes can't have quoted names
							}
							// Odd number of quotes, not in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && !quote && attrib != "")
							{
								val = gc[i].Replace("\"", "");
								quote = true;
							}
							// Odd number of quotes, in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && quote && attrib == "")
							{
								quote = false;
							}
							// Odd number of quotes, in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && quote && attrib != "")
							{
								val += " " + gc[i].Replace("\"", "");
								switch (attrib.ToLowerInvariant())
								{
									case "name":
										item.Name = val;
										break;
									case "size":
										if (item.Type == ItemType.Rom)
										{
											long size = -1;
											if (Int64.TryParse(gc[i].Replace("\"", ""), out size))
											{
												((Rom)item).Size = size;
											}
										}
										break;
									case "crc":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).CRC = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "md5":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "sha1":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "status":
									case "flags":
										if (gc[i].ToLowerInvariant() == "good")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.Good;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.Good;
											}
										}
										else if (gc[i].ToLowerInvariant() == "baddump")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.BadDump;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.BadDump;
											}
										}
										else if (gc[i].ToLowerInvariant() == "nodump")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.Nodump;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.Nodump;
											}
										}
										else if (gc[i].ToLowerInvariant() == "verified")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).ItemStatus = ItemStatus.Verified;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).ItemStatus = ItemStatus.Verified;
											}
										}
										break;
									case "date":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).Date = gc[i].Replace("\"", "") + " " + gc[i + 1].Replace("\"", "");
										}
										i++;
										break;
								}

								quote = false;
								attrib = "";
								val = "";
							}
						}
					}

					// Now process and add the rom
					string key = "";
					ParseAddHelper(item, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);
				}
				// If the line is anything but a rom or disk and we're in a block
				else if (Regex.IsMatch(line, Constants.ItemPatternCMP) && block)
				{
					GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;

					if (blockname != "header")
					{
						string itemval = gc[2].Value.Replace("\"", "");
						switch (gc[1].Value)
						{
							case "name":
								tempgamename = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
								break;
							case "description":
								gamedesc = itemval;
								break;
							case "romof":
								romof = itemval;
								break;
							case "cloneof":
								cloneof = itemval;
								break;
							case "year":
								year = itemval;
								break;
							case "manufacturer":
								manufacturer = itemval;
								break;
							case "sampleof":
								sampleof = itemval;
								break;
						}
					}
					else
					{
						string itemval = gc[2].Value.Replace("\"", "");

						if (line.Trim().StartsWith("Name:"))
						{
							Name = (String.IsNullOrEmpty(Name) ? line.Substring(6) : Name);
							superdat = superdat || itemval.Contains(" - SuperDAT");
							if (keep && superdat)
							{
								Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
							}
							continue;
						}

						switch (gc[1].Value)
						{
							case "name":
							case "Name:":
								Name = (String.IsNullOrEmpty(Name) ? itemval : Name);
								superdat = superdat || itemval.Contains(" - SuperDAT");
								if (keep && superdat)
								{
									Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
								}
								break;
							case "description":
							case "Description:":
								Description = (String.IsNullOrEmpty(Description) ? itemval : Description);
								break;
							case "rootdir":
								RootDir = (String.IsNullOrEmpty(RootDir) ? itemval : RootDir);
								break;
							case "category":
								Category = (String.IsNullOrEmpty(Category) ? itemval : Category);
								break;
							case "version":
							case "Version:":
								Version = (String.IsNullOrEmpty(Version) ? itemval : Version);
								break;
							case "date":
							case "Date:":
								Date = (String.IsNullOrEmpty(Date) ? itemval : Date);
								break;
							case "author":
							case "Author:":
								Author = (String.IsNullOrEmpty(Author) ? itemval : Author);
								break;
							case "email":
								Email = (String.IsNullOrEmpty(Email) ? itemval : Email);
								break;
							case "homepage":
							case "Homepage:":
								Homepage = (String.IsNullOrEmpty(Homepage) ? itemval : Homepage);
								break;
							case "url":
								Url = (String.IsNullOrEmpty(Url) ? itemval : Url);
								break;
							case "comment":
							case "Comment:":
								Comment = (String.IsNullOrEmpty(Comment) ? itemval : Comment);
								break;
							case "header":
								Header = (String.IsNullOrEmpty(Header) ? itemval : Header);
								break;
							case "type":
								Type = (String.IsNullOrEmpty(Type) ? itemval : Type);
								superdat = superdat || itemval.Contains("SuperDAT");
								break;
							case "forcemerging":
								switch (itemval)
								{
									case "none":
										ForceMerging = ForceMerging.None;
										break;
									case "split":
										ForceMerging = ForceMerging.Split;
										break;
									case "full":
										ForceMerging = ForceMerging.Full;
										break;
								}
								break;
							case "forcezipping":
								switch (itemval)
								{
									case "yes":
										ForcePacking = ForcePacking.Zip;
										break;
									case "no":
										ForcePacking = ForcePacking.Unzip;
										break;
								}
								break;
							case "forcepacking":
								switch (itemval)
								{
									case "zip":
										ForcePacking = ForcePacking.Zip;
										break;
									case "unzip":
										ForcePacking = ForcePacking.Unzip;
										break;
								}
								break;
						}
					}
				}

				// If we find an end bracket that's not associated with anything else, the block is done
				else if (Regex.IsMatch(line, Constants.EndPatternCMP) && block)
				{
					block = false;
					blockname = ""; tempgamename = ""; gamedesc = ""; cloneof = "";
						romof = ""; sampleof = ""; year = ""; manufacturer = "";
				}
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse an XML DAT (Logiqx, OfflineList, SabreDAT, and Software List) and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		private void ParseGenericXML(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			ItemStatus itemStatus,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep,
			bool clean,
			bool softlist)
		{
			// Prepare all internal variables
			XmlReader subreader, headreader, flagreader;
			bool superdat = false, empty = true;
			string key = "", date = "";
			long size = -1;
			ItemStatus its = ItemStatus.None;
			List<string> parent = new List<string>();

			Encoding enc = Style.GetEncoding(filename);
			XmlReader xtr = FileTools.GetXmlTextReader(filename, logger);

			// If we got a null reader, just return
			if (xtr == null)
			{
				return;
			}

			// Otherwise, read the file to the end
			try
			{
				xtr.MoveToContent();
				while (!xtr.EOF)
				{
					// If we're ending a folder or game, take care of possibly empty games and removing from the parent
					if (xtr.NodeType == XmlNodeType.EndElement && (xtr.Name == "directory" || xtr.Name == "dir"))
					{
						// If we didn't find any items in the folder, make sure to add the blank rom
						if (empty)
						{
							string tempgame = String.Join("\\", parent);
							Rom rom = new Rom("null", tempgame);

							// Now process and add the rom
							ParseAddHelper(rom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);
						}

						// Regardless, end the current folder
						int parentcount = parent.Count;
						if (parentcount == 0)
						{
							logger.Verbose("Empty parent: " + String.Join("\\", parent));
							empty = true;
						}

						// If we have an end folder element, remove one item from the parent, if possible
						if (parentcount > 0)
						{
							parent.RemoveAt(parent.Count - 1);
							if (keep && parentcount > 1)
							{
								Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
								superdat = true;
							}
						}
					}

					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						// Handle MAME listxml since they're halfway between a SL and a Logiqx XML
						case "mame":
							if (xtr.GetAttribute("build") != null)
							{
								Name = (String.IsNullOrEmpty(Name) ? xtr.GetAttribute("build") : Name);
								Description = (String.IsNullOrEmpty(Description) ? Name : Name);
							}
							xtr.Read();
							break;
						// New software lists have this behavior
						case "softwarelist":
							if (xtr.GetAttribute("name") != null)
							{
								Name = (String.IsNullOrEmpty(Name) ? xtr.GetAttribute("name") : Name);
							}
							if (xtr.GetAttribute("description") != null)
							{
								Description = (String.IsNullOrEmpty(Description) ? xtr.GetAttribute("description") : Description);
							}
							if (xtr.GetAttribute("forcemerging") != null)
							{
								switch (xtr.GetAttribute("forcemerging"))
								{
									case "split":
										ForceMerging = ForceMerging.Split;
										break;
									case "none":
										ForceMerging = ForceMerging.None;
										break;
									case "full":
										ForceMerging = ForceMerging.Full;
										break;
								}
							}
							if (xtr.GetAttribute("forceitemStatus") != null)
							{
								switch (xtr.GetAttribute("forceitemStatus"))
								{
									case "obsolete":
										ForceNodump = ForceNodump.Obsolete;
										break;
									case "required":
										ForceNodump = ForceNodump.Required;
										break;
									case "ignore":
										ForceNodump = ForceNodump.Ignore;
										break;
								}
							}
							if (xtr.GetAttribute("forcepacking") != null)
							{
								switch (xtr.GetAttribute("forcepacking"))
								{
									case "zip":
										ForcePacking = ForcePacking.Zip;
										break;
									case "unzip":
										ForcePacking = ForcePacking.Unzip;
										break;
								}
							}
							xtr.Read();
							break;
						// Handle M1 DATs since they're 99% the same as a SL DAT
						case "m1":
							Name = (String.IsNullOrEmpty(Name) ? "M1" : Name);
							Description = (String.IsNullOrEmpty(Description) ? "M1" : Description);
							if (xtr.GetAttribute("version") != null)
							{
								Version = (String.IsNullOrEmpty(Version) ? xtr.GetAttribute("version") : Version);
							}
							xtr.Read();
							break;
						// OfflineList has a different header format
						case "configuration":
							headreader = xtr.ReadSubtree();

							// If there's no subtree to the header, skip it
							if (headreader == null)
							{
								xtr.Skip();
								continue;
							}

							// Otherwise, read what we can from the header
							while (!headreader.EOF)
							{
								// We only want elements
								if (headreader.NodeType != XmlNodeType.Element || headreader.Name == "configuration")
								{
									headreader.Read();
									continue;
								}

								// Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
								string content = "";
								switch (headreader.Name.ToLowerInvariant())
								{
									case "datname":
										content = headreader.ReadElementContentAsString(); ;
										Name = (String.IsNullOrEmpty(Name) ? content : Name);
										superdat = superdat || content.Contains(" - SuperDAT");
										if (keep && superdat)
										{
											Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
										}
										break;
									case "datversionurl":
										content = headreader.ReadElementContentAsString(); ;
										Url = (String.IsNullOrEmpty(Name) ? content : Url);
										break;
									default:
										headreader.Read();
										break;
								}
							}

							break;
						// We want to process the entire subtree of the header
						case "header":
							headreader = xtr.ReadSubtree();

							// If there's no subtree to the header, skip it
							if (headreader == null)
							{
								xtr.Skip();
								continue;
							}

							// Otherwise, read what we can from the header
							while (!headreader.EOF)
							{
								// We only want elements
								if (headreader.NodeType != XmlNodeType.Element || headreader.Name == "header")
								{
									headreader.Read();
									continue;
								}

								// Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
								string content = "";
								switch (headreader.Name)
								{
									case "name":
										content = headreader.ReadElementContentAsString(); ;
										Name = (String.IsNullOrEmpty(Name) ? content : Name);
										superdat = superdat || content.Contains(" - SuperDAT");
										if (keep && superdat)
										{
											Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
										}
										break;
									case "description":
										content = headreader.ReadElementContentAsString();
										Description = (String.IsNullOrEmpty(Description) ? content : Description);
										break;
									case "rootdir":
										content = headreader.ReadElementContentAsString();
										RootDir = (String.IsNullOrEmpty(RootDir) ? content : RootDir);
										break;
									case "category":
										content = headreader.ReadElementContentAsString();
										Category = (String.IsNullOrEmpty(Category) ? content : Category);
										break;
									case "version":
										content = headreader.ReadElementContentAsString();
										Version = (String.IsNullOrEmpty(Version) ? content : Version);
										break;
									case "date":
										content = headreader.ReadElementContentAsString();
										Date = (String.IsNullOrEmpty(Date) ? content.Replace(".", "/") : Date);
										break;
									case "author":
										content = headreader.ReadElementContentAsString();
										Author = (String.IsNullOrEmpty(Author) ? content : Author);

										// Special cases for SabreDAT
										Email = (String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(headreader.GetAttribute("email")) ?
											headreader.GetAttribute("email") : Email);
										Homepage = (String.IsNullOrEmpty(Homepage) && !String.IsNullOrEmpty(headreader.GetAttribute("homepage")) ?
											headreader.GetAttribute("homepage") : Email);
										Url = (String.IsNullOrEmpty(Url) && !String.IsNullOrEmpty(headreader.GetAttribute("url")) ?
											headreader.GetAttribute("url") : Email);
										break;
									case "email":
										content = headreader.ReadElementContentAsString();
										Email = (String.IsNullOrEmpty(Email) ? content : Email);
										break;
									case "homepage":
										content = headreader.ReadElementContentAsString();
										Homepage = (String.IsNullOrEmpty(Homepage) ? content : Homepage);
										break;
									case "url":
										content = headreader.ReadElementContentAsString();
										Url = (String.IsNullOrEmpty(Url) ? content : Url);
										break;
									case "comment":
										content = headreader.ReadElementContentAsString();
										Comment = (String.IsNullOrEmpty(Comment) ? content : Comment);
										break;
									case "type":
										content = headreader.ReadElementContentAsString();
										Type = (String.IsNullOrEmpty(Type) ? content : Type);
										superdat = superdat || content.Contains("SuperDAT");
										break;
									case "clrmamepro":
									case "romcenter":
										if (headreader.GetAttribute("header") != null)
										{
											Header = (String.IsNullOrEmpty(Header) ? headreader.GetAttribute("header") : Header);
										}
										if (headreader.GetAttribute("plugin") != null)
										{
											Header = (String.IsNullOrEmpty(Header) ? headreader.GetAttribute("plugin") : Header);
										}
										if (headreader.GetAttribute("forcemerging") != null)
										{
											switch (headreader.GetAttribute("forcemerging"))
											{
												case "split":
													ForceMerging = ForceMerging.Split;
													break;
												case "none":
													ForceMerging = ForceMerging.None;
													break;
												case "full":
													ForceMerging = ForceMerging.Full;
													break;
											}
										}
										if (headreader.GetAttribute("forceitemStatus") != null)
										{
											switch (headreader.GetAttribute("forceitemStatus"))
											{
												case "obsolete":
													ForceNodump = ForceNodump.Obsolete;
													break;
												case "required":
													ForceNodump = ForceNodump.Required;
													break;
												case "ignore":
													ForceNodump = ForceNodump.Ignore;
													break;
											}
										}
										if (headreader.GetAttribute("forcepacking") != null)
										{
											switch (headreader.GetAttribute("forcepacking"))
											{
												case "zip":
													ForcePacking = ForcePacking.Zip;
													break;
												case "unzip":
													ForcePacking = ForcePacking.Unzip;
													break;
											}
										}
										headreader.Read();
										break;
									case "flags":
										flagreader = xtr.ReadSubtree();

										// If we somehow have a null flag section, skip it
										if (flagreader == null)
										{
											xtr.Skip();
											continue;
										}

										while (!flagreader.EOF)
										{
											// We only want elements
											if (flagreader.NodeType != XmlNodeType.Element || flagreader.Name == "flags")
											{
												flagreader.Read();
												continue;
											}

											switch (flagreader.Name)
											{
												case "flag":
													if (flagreader.GetAttribute("name") != null && flagreader.GetAttribute("value") != null)
													{
														content = flagreader.GetAttribute("value");
														switch (flagreader.GetAttribute("name"))
														{
															case "type":
																Type = (String.IsNullOrEmpty(Type) ? content : Type);
																superdat = superdat || content.Contains("SuperDAT");
																break;
															case "forcemerging":
																switch (content)
																{
																	case "split":
																		ForceMerging = ForceMerging.Split;
																		break;
																	case "none":
																		ForceMerging = ForceMerging.None;
																		break;
																	case "full":
																		ForceMerging = ForceMerging.Full;
																		break;
																}
																break;
															case "forceitemStatus":
																switch (content)
																{
																	case "obsolete":
																		ForceNodump = ForceNodump.Obsolete;
																		break;
																	case "required":
																		ForceNodump = ForceNodump.Required;
																		break;
																	case "ignore":
																		ForceNodump = ForceNodump.Ignore;
																		break;
																}
																break;
															case "forcepacking":
																switch (content)
																{
																	case "zip":
																		ForcePacking = ForcePacking.Zip;
																		break;
																	case "unzip":
																		ForcePacking = ForcePacking.Unzip;
																		break;
																}
																break;
														}
													}
													flagreader.Read();
													break;
												default:
													flagreader.Read();
													break;
											}
										}
										headreader.Skip();
										break;
									default:
										headreader.Read();
										break;
								}
							}

							// Skip the header node now that we've processed it
							xtr.Skip();
							break;
						case "machine":
						case "game":
						case "software":
							string temptype = xtr.Name, publisher = "", partname = "", partinterface = "", areaname = "";
							bool? supported = null;
							long? areasize = null;
							List<Tuple<string, string>> infos = new List<Tuple<string, string>>();
							List<Tuple<string, string>> features = new List<Tuple<string, string>>();

							// We want to process the entire subtree of the game
							subreader = xtr.ReadSubtree();

							// Safeguard for interesting case of "software" without anything except roms
							bool software = false;

							// If we have an empty machine, skip it
							if (subreader == null)
							{
								xtr.Skip();
								continue;
							}

							// Otherwise, add what is possible
							subreader.MoveToContent();

							// Create a new machine
							Machine machine = new Machine
							{
								Name = xtr.GetAttribute("name"),
								Description = xtr.GetAttribute("name"),
								RomOf = xtr.GetAttribute("romof") ?? "",
								CloneOf = xtr.GetAttribute("cloneof") ?? "",
								SampleOf = xtr.GetAttribute("sampleof") ?? "",
							};

							if (subreader.GetAttribute("supported") != null)
							{
								switch (subreader.GetAttribute("supported"))
								{
									case "no":
										supported = false;
										break;
									case "yes":
										supported = true;
										break;
								}
							}

							if (superdat && !keep)
							{
								string tempout = Regex.Match(machine.Name, @".*?\\(.*)").Groups[1].Value;
								if (tempout != "")
								{
									machine.Name = tempout;
								}
							}
							// Get the name of the game from the parent
							else if (superdat && keep && parent.Count > 0)
							{
								machine.Name = String.Join("\\", parent) + "\\" + machine.Name;
							}

							// Special offline list parts
							string ext = "";
							string releaseNumber = "";

							while (software || !subreader.EOF)
							{
								software = false;

								// We only want elements
								if (subreader.NodeType != XmlNodeType.Element)
								{
									if (subreader.NodeType == XmlNodeType.EndElement && subreader.Name == "part")
									{
										partname = "";
										partinterface = "";
										features = new List<Tuple<string, string>>();
									}
									if (subreader.NodeType == XmlNodeType.EndElement && (subreader.Name == "dataarea" || subreader.Name == "diskarea"))
									{
										areaname = "";
										areasize = null;
									}

									subreader.Read();
									continue;
								}

								// Get the roms from the machine
								switch (subreader.Name)
								{
									// For OfflineList only
									case "title":
										machine.Name = subreader.ReadElementContentAsString();
										break;
									case "releaseNumber":
										releaseNumber = subreader.ReadElementContentAsString();
										break;
									case "romSize":
										if (!Int64.TryParse(subreader.ReadElementContentAsString(), out size))
										{
											size = -1;
										}
										break;
									case "romCRC":
										empty = false;

										ext = (subreader.GetAttribute("extension") != null ? subreader.GetAttribute("extension") : "");

										DatItem olrom = new Rom
										{
											Name = releaseNumber + " - " + machine.Name + ext,
											Size = size,
											CRC = subreader.ReadElementContentAsString(),
											ItemStatus = ItemStatus.None,

											Machine = machine,
										};

										// Now process and add the rom
										ParseAddHelper(olrom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);
										break;

									// For Software List only
									case "publisher":
										publisher = subreader.ReadElementContentAsString();
										break;
									case "info":
										infos.Add(Tuple.Create(subreader.GetAttribute("name"), subreader.GetAttribute("value")));
										subreader.Read();
										break;
									case "part":
										partname = subreader.GetAttribute("name");
										partinterface = subreader.GetAttribute("interface");
										subreader.Read();
										break;
									case "feature":
										features.Add(Tuple.Create(subreader.GetAttribute("name"), subreader.GetAttribute("value")));
										subreader.Read();
										break;
									case "dataarea":
									case "diskarea":
										areaname = subreader.GetAttribute("name");
										long areasizetemp = -1;
										if (Int64.TryParse(subreader.GetAttribute("size"), out areasizetemp))
										{
											areasize = areasizetemp;
										}
										subreader.Read();
										break;

									// For Logiqx, SabreDAT, and Software List
									case "description":
										machine.Description = subreader.ReadElementContentAsString();
										if (!softlist && temptype == "software")
										{
											machine.Name = machine.Description.Replace('/', '_').Replace("\"", "''");
										}
										break;
									case "year":
										machine.Year = subreader.ReadElementContentAsString();
										break;
									case "manufacturer":
										machine.Manufacturer = subreader.ReadElementContentAsString();
										break;
									case "release":
										empty = false;

										bool? defaultrel = null;
										if (subreader.GetAttribute("default") != null)
										{
											if (subreader.GetAttribute("default") == "yes")
											{
												defaultrel = true;
											}
											else if (subreader.GetAttribute("default") == "no")
											{
												defaultrel = false;
											}
										}

										DatItem relrom = new Release
										{
											Name = subreader.GetAttribute("name"),
											Region = subreader.GetAttribute("region"),
											Language = subreader.GetAttribute("language"),
											Date = date,
											Default = defaultrel,

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,
										};

										// Now process and add the rom
										ParseAddHelper(relrom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "biosset":
										empty = false;

										bool? defaultbios = null;
										if (subreader.GetAttribute("default") != null)
										{
											if (subreader.GetAttribute("default") == "yes")
											{
												defaultbios = true;
											}
											else if (subreader.GetAttribute("default") == "no")
											{
												defaultbios = false;
											}
										}

										DatItem biosrom = new BiosSet
										{
											Name = subreader.GetAttribute("name"),
											Description = subreader.GetAttribute("description"),
											Default = defaultbios,

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,

											SystemID = sysid,
											System = filename,
											SourceID = srcid,
										};

										// Now process and add the rom
										ParseAddHelper(biosrom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "archive":
										empty = false;

										DatItem archiverom = new Archive
										{
											Name = subreader.GetAttribute("name"),

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,

											SystemID = sysid,
											System = filename,
											SourceID = srcid,
										};

										// Now process and add the rom
										ParseAddHelper(archiverom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "sample":
										empty = false;

										DatItem samplerom = new Sample
										{
											Name = subreader.GetAttribute("name"),

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,

											SystemID = sysid,
											System = filename,
											SourceID = srcid,
										};

										// Now process and add the rom
										ParseAddHelper(samplerom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "rom":
									case "disk":
										empty = false;

										// If the rom has a status, flag it
										its = ItemStatus.None;
										if (subreader.GetAttribute("flags") == "good" || subreader.GetAttribute("status") == "good")
										{
											its = ItemStatus.Good;
										}
										if (subreader.GetAttribute("flags") == "baddump" || subreader.GetAttribute("status") == "baddump")
										{
											logger.Verbose("Bad dump detected: " +
												(subreader.GetAttribute("name") != null && subreader.GetAttribute("name") != "" ? "\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
											its = ItemStatus.BadDump;
										}
										if (subreader.GetAttribute("flags") == "nodump" || subreader.GetAttribute("status") == "nodump")
										{
											logger.Verbose("Nodump detected: " +
												(subreader.GetAttribute("name") != null && subreader.GetAttribute("name") != "" ? "\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
											its = ItemStatus.Nodump;
										}
										if (subreader.GetAttribute("flags") == "verified" || subreader.GetAttribute("status") == "verified")
										{
											its = ItemStatus.Verified;
										}

										// If the rom has a Date attached, read it in and then sanitize it
										date = "";
										if (subreader.GetAttribute("date") != null)
										{
											DateTime dateTime = DateTime.Now;
											if (DateTime.TryParse(subreader.GetAttribute("date"), out dateTime))
											{
												date = dateTime.ToString();
											}
											else
											{
												date = subreader.GetAttribute("date");
											}
										}

										// Take care of hex-sized files
										size = -1;
										if (subreader.GetAttribute("size") != null && subreader.GetAttribute("size").Contains("0x"))
										{
											size = Convert.ToInt64(subreader.GetAttribute("size"), 16);
										}
										else if (subreader.GetAttribute("size") != null)
										{
											Int64.TryParse(subreader.GetAttribute("size"), out size);
										}

										// If the rom is continue or ignore, add the size to the previous rom
										if (subreader.GetAttribute("loadflag") == "continue" || subreader.GetAttribute("loadflag") == "ignore")
										{
											int index = Files[key].Count() - 1;
											DatItem lastrom = Files[key][index];
											if (lastrom.Type == ItemType.Rom)
											{
												((Rom)lastrom).Size += size;
											}
											Files[key].RemoveAt(index);
											Files[key].Add(lastrom);
											subreader.Read();
											continue;
										}

										// If we're in clean mode, sanitize the game name
										if (clean)
										{
											machine.Name = Style.CleanGameName(machine.Name.Split(Path.DirectorySeparatorChar));
										}

										DatItem inrom;
										switch (subreader.Name.ToLowerInvariant())
										{
											case "disk":
												inrom = new Disk
												{
													Name = subreader.GetAttribute("name"),
													MD5 = subreader.GetAttribute("md5")?.ToLowerInvariant(),
													SHA1 = subreader.GetAttribute("sha1")?.ToLowerInvariant(),
													ItemStatus = its,

													Machine = machine,

													Supported = supported,
													Publisher = publisher,
													Infos = infos,
													PartName = partname,
													PartInterface = partinterface,
													Features = features,
													AreaName = areaname,
													AreaSize = areasize,

													SystemID = sysid,
													System = filename,
													SourceID = srcid,
												};
												break;
											case "rom":
											default:
												inrom = new Rom
												{
													Name = subreader.GetAttribute("name"),
													Size = size,
													CRC = subreader.GetAttribute("crc"),
													MD5 = subreader.GetAttribute("md5")?.ToLowerInvariant(),
													SHA1 = subreader.GetAttribute("sha1")?.ToLowerInvariant(),
													ItemStatus = its,
													Date = date,

													Machine = machine,

													Supported = supported,
													Publisher = publisher,
													Infos = infos,
													PartName = partname,
													PartInterface = partinterface,
													Features = features,
													AreaName = areaname,
													AreaSize = areasize,

													SystemID = sysid,
													System = filename,
													SourceID = srcid,
												};
												break;
										}

										// Now process and add the rom
										ParseAddHelper(inrom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									default:
										subreader.Read();
										break;
								}
							}
							xtr.Skip();
							break;
						case "dir":
						case "directory":
							// Set SuperDAT flag for all SabreDAT inputs, regardless of depth
							superdat = true;
							if (keep)
							{
								Type = (Type == "" ? "SuperDAT" : Type);
							}

							string foldername = (xtr.GetAttribute("name") == null ? "" : xtr.GetAttribute("name"));
							if (foldername != "")
							{
								parent.Add(foldername);
							}

							xtr.Read();
							break;
						case "file":
							empty = false;

							// If the rom is itemStatus, flag it
							its = ItemStatus.None;
							flagreader = xtr.ReadSubtree();

							// If the subtree is empty, skip it
							if (flagreader == null)
							{
								xtr.Skip();
								continue;
							}

							while (!flagreader.EOF)
							{
								// We only want elements
								if (flagreader.NodeType != XmlNodeType.Element || flagreader.Name == "flags")
								{
									flagreader.Read();
									continue;
								}

								switch (flagreader.Name)
								{
									case "flag":
									case "status":
										if (flagreader.GetAttribute("name") != null && flagreader.GetAttribute("value") != null)
										{
											string content = flagreader.GetAttribute("value");
											switch (flagreader.GetAttribute("name"))
											{
												case "good":
													its = ItemStatus.Good;
													break;
												case "baddump":
													logger.Verbose("Bad dump detected: " + (xtr.GetAttribute("name") != null && xtr.GetAttribute("name") != "" ?
														"\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
													its = ItemStatus.BadDump;
													break;
												case "nodump":
													logger.Verbose("Nodump detected: " + (xtr.GetAttribute("name") != null && xtr.GetAttribute("name") != "" ?
														"\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
													its = ItemStatus.Nodump;
													break;
												case "verified":
													its = ItemStatus.Verified;
													break;
											}
										}
										break;
								}

								flagreader.Read();
							}

							// If the rom has a Date attached, read it in and then sanitize it
							date = "";
							if (xtr.GetAttribute("date") != null)
							{
								date = DateTime.Parse(xtr.GetAttribute("date")).ToString();
							}

							// Take care of hex-sized files
							size = -1;
							if (xtr.GetAttribute("size") != null && xtr.GetAttribute("size").Contains("0x"))
							{
								size = Convert.ToInt64(xtr.GetAttribute("size"), 16);
							}
							else if (xtr.GetAttribute("size") != null)
							{
								Int64.TryParse(xtr.GetAttribute("size"), out size);
							}

							// If the rom is continue or ignore, add the size to the previous rom
							if (xtr.GetAttribute("loadflag") == "continue" || xtr.GetAttribute("loadflag") == "ignore")
							{
								int index = Files[key].Count() - 1;
								DatItem lastrom = Files[key][index];
								if (lastrom.Type == ItemType.Rom)
								{
									((Rom)lastrom).Size += size;
								}
								Files[key].RemoveAt(index);
								Files[key].Add(lastrom);
								continue;
							}

							Machine dir = new Machine();

							// Get the name of the game from the parent
							dir.Name = String.Join("\\", parent);
							dir.Description = dir.Name;

							// If we aren't keeping names, trim out the path
							if (!keep || !superdat)
							{
								string tempout = Regex.Match(dir.Name, @".*?\\(.*)").Groups[1].Value;
								if (tempout != "")
								{
									dir.Name = tempout;
								}
							}

							DatItem rom;
							switch (xtr.GetAttribute("type").ToLowerInvariant())
							{
								case "disk":
									rom = new Disk
									{
										Name = xtr.GetAttribute("name"),
										MD5 = xtr.GetAttribute("md5")?.ToLowerInvariant(),
										SHA1 = xtr.GetAttribute("sha1")?.ToLowerInvariant(),
										ItemStatus = its,

										Machine = dir,

										SystemID = sysid,
										System = filename,
										SourceID = srcid,
									};
									break;
								case "rom":
								default:
									rom = new Rom
									{
										Name = xtr.GetAttribute("name"),
										Size = size,
										CRC = xtr.GetAttribute("crc")?.ToLowerInvariant(),
										MD5 = xtr.GetAttribute("md5")?.ToLowerInvariant(),
										SHA1 = xtr.GetAttribute("sha1")?.ToLowerInvariant(),
										ItemStatus = its,
										Date = date,

										Machine = dir,

										SystemID = sysid,
										System = filename,
										SourceID = srcid,
									};
									break;
							}

							// Now process and add the rom
							ParseAddHelper(rom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);

							xtr.Read();
							break;
						default:
							xtr.Read();
							break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Warning(ex.ToString());

				// For XML errors, just skip the affected node
				xtr?.Read();
			}

			xtr.Dispose();
		}

		/// <summary>
		/// Parse a Redump MD5 and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRedumpMD5(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			string romname,
			string md5,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				Rom rom = new Rom
				{
					Name = line.Split(' ')[1].Replace("*", String.Empty),
					Size = -1,
					MD5 = line.Split(' ')[0],
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = Path.GetFileNameWithoutExtension(filename),
					},

					SystemID = sysid,
					SourceID = srcid,
				};

				// Now process and add the rom
				string key = "";
				ParseAddHelper(rom, null, romname, null, -1, -1, -1, null, md5, null, ItemStatus.NULL, trim, single, root, clean, logger, out key);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a Redump SFV and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRedumpSFV(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			string romname,
			string crc,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				Rom rom = new Rom
				{
					Name = line.Split(' ')[0].Replace("*", String.Empty),
					Size = -1,
					CRC = line.Split(' ')[1],
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = Path.GetFileNameWithoutExtension(filename),
					},

					SystemID = sysid,
					SourceID = srcid,
				};

				// Now process and add the rom
				string key = "";
				ParseAddHelper(rom, null, romname, null, -1, -1, -1, crc, null, null, ItemStatus.NULL, trim, single, root, clean, logger, out key);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a Redump SHA-1 and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRedumpSHA1(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			string romname,
			string sha1,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				Rom rom = new Rom
				{
					Name = line.Split(' ')[1].Replace("*", String.Empty),
					Size = -1,
					SHA1 = line.Split(' ')[0],
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = Path.GetFileNameWithoutExtension(filename),
					},

					SystemID = sysid,
					SourceID = srcid,
				};

				// Now process and add the rom
				string key = "";
				ParseAddHelper(rom, null, romname, null, -1, -1, -1, null, null, sha1, ItemStatus.NULL, trim, single, root, clean, logger, out key);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a RomCenter DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRC(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			ItemStatus itemStatus,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			string blocktype = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// If the line is the start of the credits section
				if (line.ToLowerInvariant().StartsWith("[credits]"))
				{
					blocktype = "credits";
				}
				// If the line is the start of the dat section
				else if (line.ToLowerInvariant().StartsWith("[dat]"))
				{
					blocktype = "dat";
				}
				// If the line is the start of the emulator section
				else if (line.ToLowerInvariant().StartsWith("[emulator]"))
				{
					blocktype = "emulator";
				}
				// If the line is the start of the game section
				else if (line.ToLowerInvariant().StartsWith("[games]"))
				{
					blocktype = "games";
				}
				// Otherwise, it's not a section and it's data, so get out all data
				else
				{
					// If we have an author
					if (line.ToLowerInvariant().StartsWith("author="))
					{
						Author = (String.IsNullOrEmpty(Author) ? line.Split('=')[1] : Author);
					}
					// If we have one of the three version tags
					else if (line.ToLowerInvariant().StartsWith("version="))
					{
						switch (blocktype)
						{
							case "credits":
								Version = (String.IsNullOrEmpty(Version) ? line.Split('=')[1] : Version);
								break;
							case "emulator":
								Description = (String.IsNullOrEmpty(Description) ? line.Split('=')[1] : Description);
								break;
						}
					}
					// If we have a URL
					else if (line.ToLowerInvariant().StartsWith("url="))
					{
						Url = (String.IsNullOrEmpty(Url) ? line.Split('=')[1] : Url);
					}
					// If we have a comment
					else if (line.ToLowerInvariant().StartsWith("comment="))
					{
						Comment = (String.IsNullOrEmpty(Comment) ? line.Split('=')[1] : Comment);
					}
					// If we have the split flag
					else if (line.ToLowerInvariant().StartsWith("split="))
					{
						int split = 0;
						if (Int32.TryParse(line.Split('=')[1], out split))
						{
							if (split == 1)
							{
								ForceMerging = ForceMerging.Split;
							}
						}
					}
					// If we have the merge tag
					else if (line.ToLowerInvariant().StartsWith("merge="))
					{
						int merge = 0;
						if (Int32.TryParse(line.Split('=')[1], out merge))
						{
							if (merge == 1)
							{
								ForceMerging = ForceMerging.Full;
							}
						}
					}
					// If we have the refname tag
					else if (line.ToLowerInvariant().StartsWith("refname="))
					{
						Name = (String.IsNullOrEmpty(Name) ? line.Split('=')[1] : Name);
					}
					// If we have a rom
					else if (line.StartsWith("¬"))
					{
						// Some old RC DATs have this behavior
						if (line.Contains("¬N¬O"))
						{
							line = line.Replace("¬N¬O", "") + "¬¬";
						}

						/*
						The rominfo order is as follows:
						1 - parent name
						2 - parent description
						3 - game name
						4 - game description
						5 - rom name
						6 - rom crc
						7 - rom size
						8 - romof name
						9 - merge name
						*/
						string[] rominfo = line.Split('¬');

						// Try getting the size separately
						long size = 0;
						if (!Int64.TryParse(rominfo[7], out size))
						{
							size = 0;
						}

						Rom rom = new Rom
						{
							Name = rominfo[5],
							Size = size,
							CRC = rominfo[6],
							ItemStatus = ItemStatus.None,

							Machine = new Machine
							{
								Name = rominfo[3],
								Description = rominfo[4],
								CloneOf = rominfo[1],
								RomOf = rominfo[8],
							},

							SystemID = sysid,
							SourceID = srcid,
						};

						// Now process and add the rom
						string key = "";
						ParseAddHelper(rom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, trim, single, root, clean, logger, out key);
					}
				}
			}

			sr.Dispose();
		}

		/// <summary>
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="item">Item data to check against</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		private void ParseAddHelper(DatItem item, string gamename, string romname, string romtype, long sgt, long slt,
			long seq, string crc, string md5, string sha1, ItemStatus itemStatus, bool trim, bool single, string root, bool clean, Logger logger, out string key)
		{
			key = "";

			// If there's no name in the rom, we log and skip it
			if (item.Name == null)
			{
				logger.Warning("Rom with no name found! Skipping...");
				return;
			}

			// If we're in cleaning mode, sanitize the game name
			item.Machine.Name = (clean ? Style.CleanGameName(item.Machine.Name) : item.Machine.Name);

			// If we have a Rom or a Disk, clean the hash data
			if (item.Type == ItemType.Rom)
			{
				Rom itemRom = (Rom)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemRom.CRC = Style.CleanHashData(itemRom.CRC, Constants.CRCLength);
				itemRom.MD5 = Style.CleanHashData(itemRom.MD5, Constants.MD5Length);
				itemRom.SHA1 = Style.CleanHashData(itemRom.SHA1, Constants.SHA1Length);

				// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
				if ((itemRom.Size == 0 || itemRom.Size == -1)
					&& ((itemRom.CRC == Constants.CRCZero || String.IsNullOrEmpty(itemRom.CRC))
						|| itemRom.MD5 == Constants.MD5Zero
						|| itemRom.SHA1 == Constants.SHA1Zero))
				{
					itemRom.Size = Constants.SizeZero;
					itemRom.CRC = Constants.CRCZero;
					itemRom.MD5 = Constants.MD5Zero;
					itemRom.SHA1 = Constants.SHA1Zero;
				}
				// If the file has no size and it's not the above case, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump && (itemRom.Size == 0 || itemRom.Size == -1))
				{
					logger.Verbose("Incomplete entry for \"" + itemRom.Name + "\" will be output as nodump");
					itemRom.ItemStatus = ItemStatus.Nodump;
				}
				// If the file has a size but aboslutely no hashes, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump
					&& itemRom.Size > 0
					&& String.IsNullOrEmpty(itemRom.CRC)
					&& String.IsNullOrEmpty(itemRom.MD5)
					&& String.IsNullOrEmpty(itemRom.SHA1))
				{
					logger.Verbose("Incomplete entry for \"" + itemRom.Name + "\" will be output as nodump");
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

				// If the file has aboslutely no hashes, skip and log
				if (itemDisk.ItemStatus != ItemStatus.Nodump
					&& String.IsNullOrEmpty(itemDisk.MD5)
					&& String.IsNullOrEmpty(itemDisk.SHA1))
				{
					logger.Verbose("Incomplete entry for \"" + itemDisk.Name + "\" will be output as nodump");
					itemDisk.ItemStatus = ItemStatus.Nodump;
				}

				item = itemDisk;
			}

			// If the rom passes the filter, include it
			if (item.Filter(gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, itemStatus, logger))
			{
				// If we are in single game mode, rename all games
				if (single)
				{
					item.Machine.Name = "!";
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

				lock (Files)
				{
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

							// Add statistical data
							DiskCount += 1;
							TotalSize += 0;
							MD5Count += (String.IsNullOrEmpty(((Disk)item).MD5) ? 0 : 1);
							SHA1Count += (String.IsNullOrEmpty(((Disk)item).SHA1) ? 0 : 1);
							BaddumpCount += (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
							NodumpCount += (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
							break;
						case ItemType.Rom:
							key = ((Rom)item).Size + "-" + ((Rom)item).CRC;

							// Add statistical data
							RomCount += 1;
							TotalSize += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 0 : ((Rom)item).Size);
							CRCCount += (String.IsNullOrEmpty(((Rom)item).CRC) ? 0 : 1);
							MD5Count += (String.IsNullOrEmpty(((Rom)item).MD5) ? 0 : 1);
							SHA1Count += (String.IsNullOrEmpty(((Rom)item).SHA1) ? 0 : 1);
							BaddumpCount += (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
							NodumpCount += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
							break;
						default:
							key = "default";
							break;
					}

					// Add the item to the DAT
					if (Files.ContainsKey(key))
					{
						Files[key].Add(item);
					}
					else
					{
						List<DatItem> newvalue = new List<DatItem>();
						newvalue.Add(item);
						Files.Add(key, newvalue);
					}
				}
			}
		}

		#endregion

		#region Populate DAT from Directory

		/// <summary>
		/// Create a new Dat from a directory
		/// </summary>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="outDir">Output directory to </param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logger object for console and file output</param>
		public bool PopulateDatFromDir(string basePath, bool noMD5, bool noSHA1, bool bare, bool archivesAsFiles,
			bool enableGzip, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst,
			int maxDegreeOfParallelism, Logger logger)
		{
			// If the description is defined but not the name, set the name from the description
			if (String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				Name = Description;
			}

			// If the name is defined but not the description, set the description from the name
			else if (!String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Description = Name + (bare ? "" : " (" + Date + ")");
			}

			// If neither the name or description are defined, set them from the automatic values
			else if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Name = basePath.Split(Path.DirectorySeparatorChar).Last();
				Description = Name + (bare ? "" : " (" + Date + ")");
			}

			// Process the input
			if (Directory.Exists(basePath))
			{
				logger.Verbose("Folder found: " + basePath);

				// Process the files in all subfolders
				List<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories).ToList();
				Parallel.ForEach(files,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
					item =>
					{
						DFDProcessPossibleArchive(item, basePath, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, addBlanks, addDate,
							tempDir, copyFiles, headerToCheckAgainst, maxDegreeOfParallelism, logger);
					});

				// Now find all folders that are empty, if we are supposed to
				if (!Romba && addBlanks)
				{
					List<string> empties = Directory.EnumerateDirectories(basePath, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(empties,
						new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
						dir =>
						{
							if (Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly).Count() == 0)
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
									romname = "-";
								}

							// Otherwise, we want just the top level folder as the game, and the file as everything else
							else
								{
									gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
									romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "-");
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

								logger.Verbose("Adding blank empty folder: " + gamename);
								Files["null"].Add(new Rom(romname, gamename));
							}
						});
				}
			}
			else if (File.Exists(basePath))
			{
				DFDProcessPossibleArchive(basePath, basePath, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, addBlanks, addDate,
					tempDir, copyFiles, headerToCheckAgainst, maxDegreeOfParallelism, logger);
			}

			// Now that we're done, delete the temp folder (if it's not the default)
			logger.User("Cleaning temp folder");
			try
			{
				if (tempDir != Path.GetTempPath())
				{
					Directory.Delete(tempDir, true);
				}
			}
			catch
			{
				// Just absorb the error for now
			}

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logger object for console and file output</param>
		private void DFDProcessPossibleArchive(string item, string basePath, bool noMD5, bool noSHA1, bool bare, bool archivesAsFiles,
			bool enableGzip, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst,
			int maxDegreeOfParallelism, Logger logger)
		{
			// Define the temporary directory
			string tempSubDir = Path.GetFullPath(Path.Combine(tempDir, Path.GetRandomFileName())) + Path.DirectorySeparatorChar;

			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (Romba)
			{
				Rom rom = ArchiveTools.GetTorrentGZFileInfo(item, logger);

				// If the rom is valid, write it out
				if (rom.Name != null)
				{
					// Add the list if it doesn't exist already
					string key = rom.Size + "-" + rom.CRC;

					lock (Files)
					{
						if (!Files.ContainsKey(key))
						{
							Files.Add(key, new List<DatItem>());
						}

						Files[key].Add(rom);
						logger.User("File added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
					}

				}
				else
				{
					logger.User("File not added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
					return;
				}

				return;
			}

			// If we're copying files, copy it first and get the new filename
			string newItem = item;
			string newBasePath = basePath;
			if (copyFiles)
			{
				newBasePath = Path.Combine(tempDir, Path.GetRandomFileName());
				newItem = Path.GetFullPath(Path.Combine(newBasePath, Path.GetFullPath(item).Remove(0, basePath.Length + 1)));
				Directory.CreateDirectory(Path.GetDirectoryName(newItem));
				File.Copy(item, newItem, true);
			}

			// If both deep hash skip flags are set, do a quickscan
			if (noMD5 && noSHA1)
			{
				ArchiveType? type = ArchiveTools.GetCurrentArchiveType(newItem, logger);

				// If we have an archive, scan it
				if (type != null && !archivesAsFiles)
				{
					List<Rom> extracted = ArchiveTools.GetArchiveFileInfo(newItem, logger);

					foreach (Rom rom in extracted)
					{
						DFDProcessFileHelper(newItem,
							rom,
							basePath,
							(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item),
							logger);
					}
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(newItem))
				{
					DFDProcessFile(newItem, "", newBasePath, noMD5, noSHA1, addDate, headerToCheckAgainst, logger);
				}
			}
			// Otherwise, attempt to extract the files to the temporary directory
			else
			{
				ArchiveScanLevel asl = (archivesAsFiles ? ArchiveScanLevel.SevenZipExternal : ArchiveScanLevel.SevenZipInternal)
					| (!archivesAsFiles && enableGzip ? ArchiveScanLevel.GZipInternal : ArchiveScanLevel.GZipExternal)
					| (archivesAsFiles ? ArchiveScanLevel.RarExternal : ArchiveScanLevel.RarInternal)
					| (archivesAsFiles ? ArchiveScanLevel.ZipExternal : ArchiveScanLevel.ZipInternal);

				bool encounteredErrors = ArchiveTools.ExtractArchive(newItem, tempSubDir, asl, logger);

				// If the file was an archive and was extracted successfully, check it
				if (!encounteredErrors)
				{
					logger.Verbose(Path.GetFileName(item) + " treated like an archive");
					List<string> extracted = Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(extracted,
						new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
						entry =>
						{
							DFDProcessFile(entry,
								Path.Combine((Type == "SuperDAT"
									? (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length)
									: ""),
								Path.GetFileNameWithoutExtension(item)),
								tempSubDir,
								noMD5,
								noSHA1,
								addDate,
								headerToCheckAgainst,
								logger);
						});
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(newItem))
				{
					DFDProcessFile(newItem, "", newBasePath, noMD5, noSHA1, addDate, headerToCheckAgainst, logger);
				}
			}

			// Cue to delete the file if it's a copy
			if (copyFiles && item != newItem)
			{
				try
				{
					Directory.Delete(newBasePath, true);
				}
				catch { }
			}

			// Delete the sub temp directory
			if (Directory.Exists(tempSubDir))
			{
				Directory.Delete(tempSubDir, true);
			}
		}

		/// <summary>
		/// Process a single file as a file
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="basePath">Path the represents the parent directory</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>
		private void DFDProcessFile(string item, string parent, string basePath, bool noMD5, bool noSHA1, bool addDate, string headerToCheckAgainst, Logger logger)
		{
			logger.Verbose(Path.GetFileName(item) + " treated like a file");
			Rom rom = FileTools.GetFileInfo(item, logger, noMD5: noMD5, noSHA1: noSHA1, date: addDate, header: headerToCheckAgainst);

			DFDProcessFileHelper(item, rom, basePath, parent, logger);
		}

		/// <summary>
		/// Process a single file as a file (with found Rom data)
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="item">Rom data to be used to write to file</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		private void DFDProcessFileHelper(string item, DatItem datItem, string basepath, string parent, Logger logger)
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
			else
			{
				key = ((Disk)datItem).MD5;
			}

			// Add the list if it doesn't exist already
			lock (Files)
			{
				if (!Files.ContainsKey(key))
				{
					Files.Add(key, new List<DatItem>());
				}
			}

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
				if (parent == "")
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
						romname = item.Remove(0, basepath.Length);
					}

					// Otherwise, we want the archive name as the game, and the file as everything else
					else
					{
						gamename = parent;
						romname = item.Remove(0, basepath.Length);
					}
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

				// Update rom information
				datItem.Name = romname;
				if (datItem.Machine == null)
				{
					datItem.Machine = new Machine
					{
						Name = gamename,
						Description = gamename,
					};
				}
				else
				{
					datItem.Machine.Name = gamename;
					datItem.Machine.Description = gamename;
				}

				// Add the file information to the DAT
				lock (Files)
				{
					if (Files.ContainsKey(key))
					{
						Files[key].Add(datItem);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(datItem);
						Files.Add(key, temp);
					}
				}

				logger.User("File added: " + romname + Environment.NewLine);
			}
			catch (IOException ex)
			{
				logger.Error(ex.ToString());
				return;
			}
		}

		#endregion

		#region Splitting

		/// <summary>
		/// Split a DAT by input extensions
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="extA">List of extensions to split on (first DAT)</param>
		/// <param name="extB">List of extensions to split on (second DAT)</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByExt(string outDir, string basepath, List<string> extA, List<string> extB, Logger logger)
		{
			// Make sure all of the extensions have a dot at the beginning
			List<string> newExtA = new List<string>();
			foreach (string s in extA)
			{
				newExtA.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
			}
			string newExtAString = string.Join(",", newExtA);

			List<string> newExtB = new List<string>();
			foreach (string s in extB)
			{
				newExtB.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
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
				Files = new SortedDictionary<string, List<DatItem>>(),
				OutputFormat = this.OutputFormat,
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
				Files = new SortedDictionary<string, List<DatItem>>(),
				OutputFormat = this.OutputFormat,
			};

			// If roms is empty, return false
			if (this.Files.Count == 0)
			{
				return false;
			}

			// Now separate the roms accordingly
			foreach (string key in this.Files.Keys)
			{
				foreach (DatItem rom in this.Files[key])
				{
					if (newExtA.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataA.Files.ContainsKey(key))
						{
							datdataA.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataA.Files.Add(key, temp);
						}
					}
					else if (newExtB.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataB.Files.ContainsKey(key))
						{
							datdataB.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataB.Files.Add(key, temp);
						}
					}
					else
					{
						if (datdataA.Files.ContainsKey(key))
						{
							datdataA.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataA.Files.Add(key, temp);
						}
						if (datdataB.Files.ContainsKey(key))
						{
							datdataB.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataB.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(this.FileName).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(this.FileName);
			}

			// Then write out both files
			bool success = datdataA.WriteToFile(outDir, logger);
			success &= datdataB.WriteToFile(outDir, logger);

			return success;
		}

		/// <summary>
		/// Split a DAT by best available hashes
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByHash(string outDir, string basepath, Logger logger)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Create each of the respective output DATs
			logger.User("Creating and populating new DATs");
			DatFile itemStatus = new DatFile
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = this.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = this.Files[key];
				foreach (DatItem rom in roms)
				{
					// If the file is not a Rom or Disk, continue
					if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
					{
						continue;
					}

					// If the file is a itemStatus
					if ((rom.Type == ItemType.Rom && ((Rom)rom).ItemStatus == ItemStatus.Nodump)
						|| (rom.Type == ItemType.Disk && ((Disk)rom).ItemStatus == ItemStatus.Nodump))
					{
						if (itemStatus.Files.ContainsKey(key))
						{
							itemStatus.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							itemStatus.Files.Add(key, temp);
						}
					}
					// If the file has a SHA-1
					else if ((rom.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)rom).SHA1))
						|| (rom.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)rom).SHA1)))
					{
						if (sha1.Files.ContainsKey(key))
						{
							sha1.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							sha1.Files.Add(key, temp);
						}
					}
					// If the file has no SHA-1 but has an MD5
					else if ((rom.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)rom).MD5))
						|| (rom.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)rom).MD5)))
					{
						if (md5.Files.ContainsKey(key))
						{
							md5.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							md5.Files.Add(key, temp);
						}
					}
					// If the file has no MD5 but a CRC
					else if ((rom.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)rom).SHA1))
						|| (rom.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)rom).SHA1)))
					{
						if (crc.Files.ContainsKey(key))
						{
							crc.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							crc.Files.Add(key, temp);
						}
					}
					else
					{
						if (other.Files.ContainsKey(key))
						{
							other.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							other.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(this.FileName).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(this.FileName);
			}

			// Now, output all of the files to the output directory
			logger.User("DAT information created, outputting new files");
			bool success = true;
			if (itemStatus.Files.Count > 0)
			{
				success &= itemStatus.WriteToFile(outDir, logger);
			}
			if (sha1.Files.Count > 0)
			{
				success &= sha1.WriteToFile(outDir, logger);
			}
			if (md5.Files.Count > 0)
			{
				success &= md5.WriteToFile(outDir, logger);
			}
			if (crc.Files.Count > 0)
			{
				success &= crc.WriteToFile(outDir, logger);
			}

			return success;
		}

		/// <summary>
		/// Split a DAT by type of Rom
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByType(string outDir, string basepath, Logger logger)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Create each of the respective output DATs
			logger.User("Creating and populating new DATs");
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
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
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new SortedDictionary<string, List<DatItem>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = this.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = this.Files[key];
				foreach (DatItem rom in roms)
				{
					// If the file is a Rom
					if (rom.Type == ItemType.Rom)
					{
						if (romdat.Files.ContainsKey(key))
						{
							romdat.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							romdat.Files.Add(key, temp);
						}
					}
					// If the file is a Disk
					else if (rom.Type == ItemType.Disk)
					{
						if (diskdat.Files.ContainsKey(key))
						{
							diskdat.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							diskdat.Files.Add(key, temp);
						}
					}

					// If the file is a Sample
					else if (rom.Type == ItemType.Sample)
					{
						if (sampledat.Files.ContainsKey(key))
						{
							sampledat.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							sampledat.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(this.FileName).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(this.FileName);
			}

			// Now, output all of the files to the output directory
			logger.User("DAT information created, outputting new files");
			bool success = true;
			if (romdat.Files.Count > 0)
			{
				success &= romdat.WriteToFile(outDir, logger);
			}
			if (diskdat.Files.Count > 0)
			{
				success &= diskdat.WriteToFile(outDir, logger);
			}
			if (sampledat.Files.Count > 0)
			{
				success &= sampledat.WriteToFile(outDir, logger);
			}

			return success;
		}

		#endregion

		#region Statistics

		/// <summary>
		/// Recalculate the statistics for the Dat
		/// </summary>
		public void RecalculateStats()
		{
			// Wipe out any stats already there
			RomCount = 0;
			DiskCount = 0;
			TotalSize = 0;
			CRCCount = 0;
			MD5Count = 0;
			SHA1Count = 0;
			BaddumpCount = 0;
			NodumpCount = 0;

			// If we have a blank Dat in any way, return
			if (this == null || Files == null || Files.Count == 0)
			{
				return;
			}

			// Loop through and add
			foreach (List<DatItem> roms in Files.Values)
			{
				foreach (Rom rom in roms)
				{
					RomCount += (rom.Type == ItemType.Rom ? 1 : 0);
					DiskCount += (rom.Type == ItemType.Disk ? 1 : 0);
					TotalSize += (rom.ItemStatus == ItemStatus.Nodump ? 0 : rom.Size);
					CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
					MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
					SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
					BaddumpCount += (rom.Type == ItemType.Disk 
						? (((Disk)rom).ItemStatus == ItemStatus.BadDump ? 1 : 0)
						: (rom.Type == ItemType.Rom
							? (((Rom)rom).ItemStatus == ItemStatus.BadDump ? 1 : 0)
							: 0)
						);
					NodumpCount += (rom.Type == ItemType.Disk
							? (((Disk)rom).ItemStatus == ItemStatus.Nodump ? 1 : 0)
							: (rom.Type == ItemType.Rom
								? (((Rom)rom).ItemStatus == ItemStatus.Nodump ? 1 : 0)
								: 0)
							);
				}
			}
		}

		/// <summary>
		/// Output the stats for the Dat in a human-readable format
		/// </summary>
		/// <param name="sw">StreamWriter representing the output file or stream for the statistics</param>
		/// <param name="statOutputFormat">Set the statistics output format to use</param>
		/// <param name="logger">Logger object for file and console writing</param>
		/// <param name="recalculate">True if numbers should be recalculated for the DAT, false otherwise (default)</param>
		/// <param name="game">Number of games to use, -1 means recalculate games (default)</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise (default)</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise (default)</param>
		public void OutputStats(StreamWriter sw, StatOutputFormat statOutputFormat, Logger logger, bool recalculate = false, long game = -1, bool baddumpCol = false, bool nodumpCol = false)
		{
			// If we're supposed to recalculate the statistics, do so
			if (recalculate)
			{
				RecalculateStats();
			}

			BucketByGame(false, true, logger, false);
			if (TotalSize < 0)
			{
				TotalSize = Int64.MaxValue + TotalSize;
			}

			// Log the results to screen
			string results = @"For '" + FileName + @"':
--------------------------------------------------
	Uncompressed size:	   " + Style.GetBytesReadable(TotalSize) + @"
	Games found:			 " + (game == -1 ? Files.Count : game) + @"
	Roms found:			  " + RomCount + @"
	Disks found:			 " + DiskCount + @"
	Roms with CRC:		   " + CRCCount + @"
	Roms with MD5:		   " + MD5Count + @"
	Roms with SHA-1:		 " + SHA1Count + "\n";

			if (baddumpCol)
			{
				results += "	Roms with BadDump status: " + BaddumpCount + "\n";
			}
			if (nodumpCol)
			{
				results += "	Roms with Nodump status: " + NodumpCount + "\n";
			}

			logger.User(results);

			// Now write it out to file as well
			string line = "";
			switch (statOutputFormat)
			{
				case StatOutputFormat.CSV:
					line = "\"" + FileName + "\","
						+ "\"" + Style.GetBytesReadable(TotalSize) + "\","
						+ "\"" + (game == -1 ? Files.Count : game) + "\","
						+ "\"" + RomCount + "\","
						+ "\"" + DiskCount + "\","
						+ "\"" + CRCCount + "\","
						+ "\"" + MD5Count + "\","
						+ "\"" + SHA1Count + "\"";

					if (baddumpCol)
					{
						line += ",\"" + BaddumpCount + "\"";
					}
					if (nodumpCol)
					{
						line += ",\"" + NodumpCount + "\"";
					}

					line += "\n";
					break;
				case StatOutputFormat.HTML:
					line = "\t\t\t<tr" + (FileName.StartsWith("DIR: ")
							? " class=\"dir\"><td>" + HttpUtility.HtmlEncode(FileName.Remove(0, 5))
							: "><td>" + HttpUtility.HtmlEncode(FileName)) + "</td>"
						+ "<td align=\"right\">" + Style.GetBytesReadable(TotalSize) + "</td>"
						+ "<td align=\"right\">" + (game == -1 ? Files.Count : game) + "</td>"
						+ "<td align=\"right\">" + RomCount + "</td>"
						+ "<td align=\"right\">" + DiskCount + "</td>"
						+ "<td align=\"right\">" + CRCCount + "</td>"
						+ "<td align=\"right\">" + MD5Count + "</td>"
						+ "<td align=\"right\">" + SHA1Count + "</td>";

					if (baddumpCol)
					{
						line += "<td align=\"right\">" + BaddumpCount + "</td>";
					}
					if (nodumpCol)
					{
						line += "<td align=\"right\">" + NodumpCount + "</td>";
					}

					line += "</tr>\n";
					break;
				case StatOutputFormat.None:
				default:
					line = @"'" + FileName + @"':
--------------------------------------------------
	Uncompressed size:	   " + Style.GetBytesReadable(TotalSize) + @"
	Games found:			 " + (game == -1 ? Files.Count : game) + @"
	Roms found:			  " + RomCount + @"
	Disks found:			 " + DiskCount + @"
	Roms with CRC:		   " + CRCCount + @"
	Roms with MD5:		   " + MD5Count + @"
	Roms with SHA-1:		 " + SHA1Count + "\n";

					if (baddumpCol)
					{
						line += "	Roms with BadDump status: " + BaddumpCount + "\n";
					}
					if (nodumpCol)
					{
						line += "	Roms with Nodump status: " + NodumpCount + "\n";
					}
					break;
				case StatOutputFormat.TSV:
					line = "\"" + FileName + "\"\t"
						+ "\"" + Style.GetBytesReadable(TotalSize) + "\"\t"
						+ "\"" + (game == -1 ? Files.Count : game) + "\"\t"
						+ "\"" + RomCount + "\"\t"
						+ "\"" + DiskCount + "\"\t"
						+ "\"" + CRCCount + "\"\t"
						+ "\"" + MD5Count + "\"\t"
						+ "\"" + SHA1Count + "\"";

					if (baddumpCol)
					{
						line += "\t\"" + BaddumpCount + "\"";
					}
					if (nodumpCol)
					{
						line += "\t\"" + NodumpCount + "\"";
					}

					line += "\n";
					break;
			}

			// Output the line to the streamwriter
			sw.Write(line);
		}

		#endregion

		#region Writing

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datdata">All information for creating the datfile header</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for file output:
		/// - Have the ability to strip special (non-ASCII) characters from rom information
		/// </remarks>
		public bool WriteToFile(string outDir, Logger logger, bool norename = true, bool stats = false, bool ignoreblanks = false, bool overwrite = true)
		{
			// If there's nothing there, abort
			if (Files == null || Files.Count == 0)
			{
				return false;
			}

			// If output directory is empty, use the current folder
			if (outDir.Trim() == "")
			{
				outDir = Environment.CurrentDirectory;
			}

			// Create the output directory if it doesn't already exist
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// If the DAT has no output format, default to XML
			if (OutputFormat == 0)
			{
				OutputFormat = OutputFormat.Logiqx;
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
				StreamWriter sw = new StreamWriter(new MemoryStream());
				OutputStats(sw, StatOutputFormat.None, logger, recalculate: (RomCount + DiskCount == 0), baddumpCol: true, nodumpCol: true);
				sw.Dispose();
			}

			// Bucket roms by game name and optionally dedupe
			BucketByGame(MergeRoms, norename, logger);

			// Get the outfile name
			Dictionary<OutputFormat, string> outfiles = Style.CreateOutfileNames(outDir, this, overwrite);

			try
			{
				foreach (OutputFormat outputFormat in outfiles.Keys)
				{
					string outfile = outfiles[outputFormat];

					logger.User("Opening file for writing: " + outfile);
					FileStream fs = File.Create(outfile);
					StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(true));

					// Write out the header
					WriteHeader(sw, outputFormat, logger);

					// Write out each of the machines and roms
					int depth = 2, last = -1;
					string lastgame = null;
					List<string> splitpath = new List<string>();

					// Get a properly sorted set of keys
					List<string> keys = Files.Keys.ToList();
					keys.Sort(new NaturalComparer());

					foreach (string key in keys)
					{
						List<DatItem> roms = Files[key];

						for (int index = 0; index < roms.Count; index++)
						{
							DatItem rom = roms[index];

							// There are apparently times when a null rom can skip by, skip them
							if (rom.Name == null || rom.Machine.Name == null)
							{
								logger.Warning("Null rom found!");
								continue;
							}

							List<string> newsplit = rom.Machine.Name.Split('\\').ToList();

							// If we have a different game and we're not at the start of the list, output the end of last item
							if (lastgame != null && lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
							{
								depth = WriteEndGame(sw, outputFormat, rom, splitpath, newsplit, lastgame, depth, out last, logger);
							}

							// If we have a new game, output the beginning of the new item
							if (lastgame == null || lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
							{
								depth = WriteStartGame(sw, outputFormat, rom, newsplit, lastgame, depth, last, logger);
							}

							// If we have a "null" game (created by DATFromDir or something similar), log it to file
							if (rom.Type == ItemType.Rom
								&& ((Rom)rom).Size == -1
								&& ((Rom)rom).CRC == "null"
								&& ((Rom)rom).MD5 == "null"
								&& ((Rom)rom).SHA1 == "null")
							{
								logger.Verbose("Empty folder found: " + rom.Machine.Name);

								// If we're in a mode that doesn't allow for actual empty folders, add the blank info
								if (outputFormat != OutputFormat.CSV
									&& outputFormat != OutputFormat.MissFile
									&& outputFormat != OutputFormat.SabreDat
									&& outputFormat != OutputFormat.TSV)
								{
									rom.Name = (rom.Name == "null" ? "-" : rom.Name);
									((Rom)rom).Size = Constants.SizeZero;
									((Rom)rom).CRC = Constants.CRCZero;
									((Rom)rom).MD5 = Constants.MD5Zero;
									((Rom)rom).SHA1 = Constants.SHA1Zero;
								}

								// Otherwise, set the new path and such, write out, and continue
								else
								{
									splitpath = newsplit;
									lastgame = rom.Machine.Name;
									continue;
								}
							}

							// Now, output the rom data
							WriteRomData(sw, outputFormat, rom, lastgame, depth, logger, ignoreblanks);

							// Set the new data to compare against
							splitpath = newsplit;
							lastgame = rom.Machine.Name;
						}
					}

					// Write the file footer out
					WriteFooter(sw, outputFormat, depth, logger);

					logger.Verbose("File written!" + Environment.NewLine);
					sw.Dispose();
					fs.Dispose();
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT header using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteHeader(StreamWriter sw, OutputFormat outputFormat, Logger logger)
		{
			try
			{
				string header = "";
				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
						header = "clrmamepro (\n" +
							"\tname \"" + Name + "\"\n" +
							"\tdescription \"" + Description + "\"\n" +
							(!String.IsNullOrEmpty(Category) ? "\tcategory \"" + Category + "\"\n" : "") +
							"\tversion \"" + Version + "\"\n" +
							(!String.IsNullOrEmpty(Date) ? "\tdate \"" + Date + "\"\n" : "") +
							"\tauthor \"" + Author + "\"\n" +
							(!String.IsNullOrEmpty(Email) ? "\temail \"" + Email + "\"\n" : "") +
							(!String.IsNullOrEmpty(Homepage) ? "\thomepage \"" + Homepage + "\"\n" : "") +
							(!String.IsNullOrEmpty(Url) ? "\turl \"" + Url + "\"\n" : "") +
							(!String.IsNullOrEmpty(Comment) ? "\tcomment \"" + Comment + "\"\n" : "") +
							(ForcePacking == ForcePacking.Unzip ? "\tforcezipping no\n" : "") +
							(ForcePacking == ForcePacking.Zip ? "\tforcezipping yes\n" : "") +
							(ForceMerging == ForceMerging.Full ? "\tforcemerging full\n" : "") +
							(ForceMerging == ForceMerging.Split ? "\tforcemerging split\n" : "") +
							")\n";
						break;
					case OutputFormat.CSV:
						header = "\"File Name\",\"Internal Name\",\"Description\",\"Game Name\",\"Game Description\",\"Type\",\"" +
								"Rom Name\",\"Disk Name\",\"Size\",\"CRC\",\"MD5\",\"SHA1\",\"Nodump\"\n";
						break;
					case OutputFormat.DOSCenter:
						header = "DOSCenter (\n" +
							"\tName: " + Name + "\n" +
							"\tDescription: " + Description + "\n" +
							"\tVersion: " + Version + "\n" +
							"\tDate: " + Date + "\n" +
							"\tAuthor: " + Author + "\n" +
							"\tHomepage: " + Homepage + "\n" +
							"\tComment: " + Comment + "\n" +
							")\n";
						break;
					case OutputFormat.Logiqx:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(Description) + "</description>\n" +
							(!String.IsNullOrEmpty(RootDir) ? "\t\t<rootdir>" + HttpUtility.HtmlEncode(RootDir) + "</rootdir>\n" : "") +
							(!String.IsNullOrEmpty(Category) ? "\t\t<category>" + HttpUtility.HtmlEncode(Category) + "</category>\n" : "") +
							"\t\t<version>" + HttpUtility.HtmlEncode(Version) + "</version>\n" +
							(!String.IsNullOrEmpty(Date) ? "\t\t<date>" + HttpUtility.HtmlEncode(Date) + "</date>\n" : "") +
							"\t\t<author>" + HttpUtility.HtmlEncode(Author) + "</author>\n" +
							(!String.IsNullOrEmpty(Email) ? "\t\t<email>" + HttpUtility.HtmlEncode(Email) + "</email>\n" : "") +
							(!String.IsNullOrEmpty(Homepage) ? "\t\t<homepage>" + HttpUtility.HtmlEncode(Homepage) + "</homepage>\n" : "") +
							(!String.IsNullOrEmpty(Url) ? "\t\t<url>" + HttpUtility.HtmlEncode(Url) + "</url>\n" : "") +
							(!String.IsNullOrEmpty(Comment) ? "\t\t<comment>" + HttpUtility.HtmlEncode(Comment) + "</comment>\n" : "") +
							(!String.IsNullOrEmpty(Type) ? "\t\t<type>" + HttpUtility.HtmlEncode(Type) + "</type>\n" : "") +
							(ForcePacking != ForcePacking.None || ForceMerging != ForceMerging.None || ForceNodump != ForceNodump.None ?
								"\t\t<clrmamepro" +
									(ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : "") +
									(ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : "") +
									(ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : "") +
									(ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : "") +
									(ForceNodump == ForceNodump.Ignore ? " forceitemStatus=\"ignore\"" : "") +
									(ForceNodump == ForceNodump.Obsolete ? " forceitemStatus=\"obsolete\"" : "") +
									(ForceNodump == ForceNodump.Required ? " forceitemStatus=\"required\"" : "") +
									" />\n"
							: "") +
							"\t</header>\n";
						break;
					case OutputFormat.TSV:
						header = "\"File Name\"\t\"Internal Name\"\t\"Description\"\t\"Game Name\"\t\"Game Description\"\t\"Type\"\t\"" +
								"Rom Name\"\t\"Disk Name\"\t\"Size\"\t\"CRC\"\t\"MD5\"\t\"SHA1\"\t\"Nodump\"\n";
						break;
					case OutputFormat.OfflineList:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n"
							+ "<dat xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"datas.xsd\">\n"
							+ "\t<configuration>\n"
							+ "\t\t<datName>" + HttpUtility.HtmlEncode(Name) + "</datName>\n"
							+ "\t\t<datVersion>" + Files.Count + "</datVersion>\n"
							+ "\t\t<system>none</system>\n"
							+ "\t\t<screenshotsWidth>240</screenshotsWidth>\n"
							+ "\t\t<screenshotsHeight>160</screenshotsHeight>\n"
							+ "\t\t<infos>\n"
							+ "\t\t\t<title visible=\"false\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<location visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<publisher visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<sourceRom visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<saveType visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<romSize visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<releaseNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<languageNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<comment visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<romCRC visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<im1CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
							+ "\t\t\t<im2CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
							+ "\t\t\t<languages visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t</infos>\n"
							+ "\t\t<canOpen>\n"
							+ "\t\t\t<extension>.bin</extension>\n"
							+ "\t\t</canOpen>\n"
							+ "\t\t<newDat>\n"
							+ "\t\t\t<datVersionURL>" + HttpUtility.HtmlEncode(Url) + "</datVersionURL>\n"
							+ "\t\t\t<datURL fileName=\"" + HttpUtility.HtmlEncode(FileName) + ".zip\">" + HttpUtility.HtmlEncode(Url) + "</datURL>\n"
							+ "\t\t\t<imURL>" + HttpUtility.HtmlEncode(Url) + "</imURL>\n"
							+ "\t\t</newDat>\n"
							+ "\t\t<search>\n"
							+ "\t\t\t<to value=\"location\" default=\"true\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"romSize\" default=\"true\" auto=\"false\"/>\n"
							+ "\t\t\t<to value=\"languages\" default=\"true\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"saveType\" default=\"false\" auto=\"false\"/>\n"
							+ "\t\t\t<to value=\"publisher\" default=\"false\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"sourceRom\" default=\"false\" auto=\"true\"/>\n"
							+ "\t\t</search>\n"
							+ "\t\t<romTitle >%u - %n</romTitle>\n"
							+ "\t</configuration>\n"
							+ "\t<games>\n";
						break;
					case OutputFormat.RomCenter:
						header = "[CREDITS]\n" +
							"author=" + Author + "\n" +
							"version=" + Version + "\n" +
							"comment=" + Comment + "\n" +
							"[DAT]\n" +
							"version=2.50\n" +
							"split=" + (ForceMerging == ForceMerging.Split ? "1" : "0") + "\n" +
							"merge=" + (ForceMerging == ForceMerging.Full ? "1" : "0") + "\n" +
							"[EMULATOR]\n" +
							"refname=" + Name + "\n" +
							"version=" + Description + "\n" +
							"[GAMES]\n";
						break;
					case OutputFormat.SabreDat:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE sabredat SYSTEM \"newdat.xsd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(Description) + "</description>\n" +
							(!String.IsNullOrEmpty(RootDir) ? "\t\t<rootdir>" + HttpUtility.HtmlEncode(RootDir) + "</rootdir>\n" : "") +
							(!String.IsNullOrEmpty(Category) ? "\t\t<category>" + HttpUtility.HtmlEncode(Category) + "</category>\n" : "") +
							"\t\t<version>" + HttpUtility.HtmlEncode(Version) + "</version>\n" +
							(!String.IsNullOrEmpty(Date) ? "\t\t<date>" + HttpUtility.HtmlEncode(Date) + "</date>\n" : "") +
							"\t\t<author>" + HttpUtility.HtmlEncode(Author) + "</author>\n" +
							(!String.IsNullOrEmpty(Comment) ? "\t\t<comment>" + HttpUtility.HtmlEncode(Comment) + "</comment>\n" : "") +
							(!String.IsNullOrEmpty(Type) || ForcePacking != ForcePacking.None || ForceMerging != ForceMerging.None || ForceNodump != ForceNodump.None ?
								"\t\t<flags>\n" +
									(!String.IsNullOrEmpty(Type) ? "\t\t\t<flag name=\"type\" value=\"" + HttpUtility.HtmlEncode(Type) + "\"/>\n" : "") +
									(ForcePacking == ForcePacking.Unzip ? "\t\t\t<flag name=\"forcepacking\" value=\"unzip\"/>\n" : "") +
									(ForcePacking == ForcePacking.Zip ? "\t\t\t<flag name=\"forcepacking\" value=\"zip\"/>\n" : "") +
									(ForceMerging == ForceMerging.Full ? "\t\t\t<flag name=\"forcemerging\" value=\"full\"/>\n" : "") +
									(ForceMerging == ForceMerging.Split ? "\t\t\t<flag name=\"forcemerging\" value=\"split\"/>\n" : "") +
									(ForceNodump == ForceNodump.Ignore ? "\t\t\t<flag name=\"forceitemStatus\" value=\"ignore\"/>\n" : "") +
									(ForceNodump == ForceNodump.Obsolete ? "\t\t\t<flag name=\"forceitemStatus\" value=\"obsolete\"/>\n" : "") +
									(ForceNodump == ForceNodump.Required ? "\t\t\t<flag name=\"forceitemStatus\" value=\"required\"/>\n" : "") +
									"\t\t</flags>\n"
							: "") +
							"\t</header>\n" +
							"\t<data>\n";
						break;
					case OutputFormat.SoftwareList:
						header = "<?xml version=\"1.0\"?>\n" +
							"<!DOCTYPE softwarelist SYSTEM \"softwarelist.dtd\">\n\n" +
							"<softwarelist name=\"" + HttpUtility.HtmlEncode(Name) + "\"" +
								" description=\"" + HttpUtility.HtmlEncode(Description) + "\"" +
								(ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : "") +
								(ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : "") +
								(ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : "") +
								(ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : "") +
								(ForceNodump == ForceNodump.Ignore ? " forceitemStatus=\"ignore\"" : "") +
								(ForceNodump == ForceNodump.Obsolete ? " forceitemStatus=\"obsolete\"" : "") +
								(ForceNodump == ForceNodump.Required ? " forceitemStatus=\"required\"" : "") +
								">\n\n";
						break;
				}

				// Write the header out
				sw.Write(header);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		private int WriteStartGame(StreamWriter sw, OutputFormat outputFormat, DatItem rom, List<string> newsplit, string lastgame, int depth, int last, Logger logger)
		{
			try
			{
				// No game should start with a path separator
				if (rom.Machine.Name.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.Machine.Name = rom.Machine.Name.Substring(1);
				}

				string state = "";
				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "game (\n\tname \"" + rom.Machine.Name + "\"\n" +
							(ExcludeOf ? "" :
								(String.IsNullOrEmpty(rom.Machine.RomOf) ? "" : "\tromof \"" + rom.Machine.RomOf + "\"\n") +
								(String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : "\tcloneof \"" + rom.Machine.CloneOf + "\"\n") +
								(String.IsNullOrEmpty(rom.Machine.SampleOf) ? "" : "\tsampleof \"" + rom.Machine.SampleOf + "\"\n")
							) +
							"\tdescription \"" + (String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description) + "\"\n" +
							(String.IsNullOrEmpty(rom.Machine.Year) ? "" : "\tyear " + rom.Machine.Year + "\n") +
							(String.IsNullOrEmpty(rom.Machine.Manufacturer) ? "" : "\tmanufacturer \"" + rom.Machine.Manufacturer + "\"\n");
						break;
					case OutputFormat.DOSCenter:
						state += "game (\n\tname \"" + rom.Machine.Name + ".zip\"\n";
						break;
					case OutputFormat.Logiqx:
						state += "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Machine.Name) + "\"" +
								(rom.Machine.IsBios ? " isbios=\"yes\"" : "") +
								(ExcludeOf ? "" :
									(String.IsNullOrEmpty(rom.Machine.CloneOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.CloneOf.ToLowerInvariant())
										? ""
										: " cloneof=\"" + HttpUtility.HtmlEncode(rom.Machine.CloneOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.RomOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.RomOf.ToLowerInvariant())
										? ""
										: " romof=\"" + HttpUtility.HtmlEncode(rom.Machine.RomOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.SampleOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.SampleOf.ToLowerInvariant())
										? ""
										: " sampleof=\"" + HttpUtility.HtmlEncode(rom.Machine.SampleOf) + "\"")
								) +
								">\n" +
							(String.IsNullOrEmpty(rom.Machine.Comment) ? "" : "\t\t<comment>" + HttpUtility.HtmlEncode(rom.Machine.Comment) + "</comment>\n") +
							"\t\t<description>" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) + "</description>\n" +
							(String.IsNullOrEmpty(rom.Machine.Year) ? "" : "\t\t<year>" + HttpUtility.HtmlEncode(rom.Machine.Year) + "</year>\n") +
							(String.IsNullOrEmpty(rom.Machine.Manufacturer) ? "" : "\t\t<manufacturer>" + HttpUtility.HtmlEncode(rom.Machine.Manufacturer) + "</manufacturer>\n");
						break;
					case OutputFormat.SabreDat:
						for (int i = (last == -1 ? 0 : last); i < newsplit.Count; i++)
						{
							for (int j = 0; j < depth - last + i - (lastgame == null ? 1 : 0); j++)
							{
								state += "\t";
							}
							state += "<directory name=\"" + HttpUtility.HtmlEncode(newsplit[i]) + "\" description=\"" +
							HttpUtility.HtmlEncode(newsplit[i]) + "\">\n";
						}
						depth = depth - (last == -1 ? 0 : last) + newsplit.Count;
						break;
					case OutputFormat.SoftwareList:
						state += "\t<software name=\"" + HttpUtility.HtmlEncode(rom.Machine.Name) + "\""
							+ (rom.Supported != null ? " supported=\"" + (rom.Supported == true ? "yes" : "no") + "\"" : "") +
							(ExcludeOf ? "" :
									(String.IsNullOrEmpty(rom.Machine.CloneOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.CloneOf.ToLowerInvariant())
										? ""
										: " cloneof=\"" + HttpUtility.HtmlEncode(rom.Machine.CloneOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.RomOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.RomOf.ToLowerInvariant())
										? ""
										: " romof=\"" + HttpUtility.HtmlEncode(rom.Machine.RomOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.SampleOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.SampleOf.ToLowerInvariant())
										? ""
										: " sampleof=\"" + HttpUtility.HtmlEncode(rom.Machine.SampleOf) + "\"")
								) + ">\n"
							+ "\t\t<description>" + HttpUtility.HtmlEncode(rom.Machine.Description) + "</description>\n"
							+ (rom.Machine.Year != null ? "\t\t<year>" + HttpUtility.HtmlEncode(rom.Machine.Year) + "</year>\n" : "")
							+ (rom.Publisher != null ? "\t\t<publisher>" + HttpUtility.HtmlEncode(rom.Publisher) + "</publisher>\n" : "");

						foreach (Tuple<string, string> kvp in rom.Infos)
						{
							state += "\t\t<info name=\"" + HttpUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + HttpUtility.HtmlEncode(kvp.Item2) + "\" />\n";
						}
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		private int WriteEndGame(StreamWriter sw, OutputFormat outputFormat, DatItem rom, List<string> splitpath, List<string> newsplit, string lastgame, int depth, out int last, Logger logger)
		{
			last = 0;

			try
			{
				string state = "";

				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
					case OutputFormat.DOSCenter:
						state += (String.IsNullOrEmpty(rom.Machine.SampleOf) ? "" : "\tsampleof \"" + rom.Machine.SampleOf + "\"\n") + ")\n";
						break;
					case OutputFormat.Logiqx:
						state += "\t</machine>\n";
						break;
					case OutputFormat.OfflineList:
						state += "\t\t</game>\n";
						break;
					case OutputFormat.SabreDat:
						if (splitpath != null)
						{
							for (int i = 0; i < newsplit.Count && i < splitpath.Count; i++)
							{
								// Always keep track of the last seen item
								last = i;

								// If we find a difference, break
								if (newsplit[i] != splitpath[i])
								{
									break;
								}
							}

							// Now that we have the last known position, take down all open folders
							for (int i = depth - 1; i > last + 1; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									state += "\t";
								}
								state += "</directory>\n";
							}

							// Reset the current depth
							depth = 2 + last;
						}
						break;
					case OutputFormat.SoftwareList:
						state += "\t</software>\n\n";
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out RomData using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteRomData(StreamWriter sw, OutputFormat outputFormat, DatItem rom, string lastgame, int depth, Logger logger, bool ignoreblanks = false)
		{
			// If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
			if (ignoreblanks
				&& (rom.Type == ItemType.Rom
				&& (((Rom)rom).Size == 0 || ((Rom)rom).Size == -1)))
			{
				return true;
			}

			try
			{
				string state = "", name = "", pre = "", post = "";
				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\tarchive ( name\"" + rom.Name + "\""
									+ " )\n";
								break;
							case ItemType.BiosSet:
								state += "\tbiosset ( name\"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description \"" + ((BiosSet)rom).Description + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? "default " + ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ " )\n";
								break;
							case ItemType.Disk:
								state += "\tdisk ( name \"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5 " + ((Disk)rom).MD5.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1 " + ((Disk)rom).SHA1.ToLowerInvariant() : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? " flags " + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() : "")
									+ " )\n";
								break;
							case ItemType.Release:
								state += "\trelease ( name\"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region \"" + ((Release)rom).Region + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language \"" + ((Release)rom).Language + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date \"" + ((Release)rom).Date + "\"" : "")
									+ (((Release)rom).Default != null
										? "default " + ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ " )\n";
								break;
							case ItemType.Rom:
								state += "\trom ( name \"" + rom.Name + "\""
									+ (((Rom)rom).Size != -1 ? " size " + ((Rom)rom).Size : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc " + ((Rom)rom).CRC.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5 " + ((Rom)rom).MD5.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1 " + ((Rom)rom).SHA1.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date \"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? " flags " + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() : "")
									+ " )\n";
								break;
							case ItemType.Sample:
								state += "\tsample ( name\"" + rom.Name + "\""
									+ " )\n";
								break;
						}

						break;
					case OutputFormat.CSV:
						// CSV should only output Rom and Disk
						if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
						{
							return true;
						}

						pre = Prefix + (Quotes ? "\"" : "");
						post = (Quotes ? "\"" : "") + Postfix;

						if (rom.Type == ItemType.Rom)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
						}
						else if (rom.Type == ItemType.Disk)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
						}

						if (rom.Type == ItemType.Rom)
						{
							string inline = "\"" + FileName + "\""
								+ ",\"" + Name + "\""
								+ ",\"" + Description + "\""
								+ ",\"" + rom.Machine.Name + "\""
								+ ",\"" + rom.Machine.Description + "\""
								+ "," + "\"rom\""
								+ ",\"" + rom.Name + "\""
								+ "," + "\"\""
								+ ",\"" + ((Rom)rom).Size + "\""
								+ ",\"" + ((Rom)rom).CRC + "\""
								+ ",\"" + ((Rom)rom).MD5 + "\""
								+ ",\"" + ((Rom)rom).SHA1 + "\""
								+ "," + (((Rom)rom).ItemStatus != ItemStatus.None ? "\"" + ((Rom)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							string inline = "\"" + FileName + "\""
								+ ",\"" + Name + "\""
								+ ",\"" + Description + "\""
								+ ",\"" + rom.Machine.Name + "\""
								+ ",\"" + rom.Machine.Description + "\""
								+ "," + "\"disk\""
								+ "," + "\"\""
								+ ",\"" + rom.Name + "\""
								+ "," + "\"\""
								+ "," + "\"\""
								+ ",\"" + ((Disk)rom).MD5 + "\""
								+ ",\"" + ((Disk)rom).SHA1 + "\""
								+ "," + (((Disk)rom).ItemStatus != ItemStatus.None ? "\"" + ((Disk)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						break;
					case OutputFormat.DOSCenter:
						switch (rom.Type)
						{
							case ItemType.Archive:
							case ItemType.BiosSet:
							case ItemType.Disk:
							case ItemType.Release:
							case ItemType.Sample:
								// We don't output these at all
								break;
							case ItemType.Rom:
								state += "\tfile ( name " + ((Rom)rom).Name
									+ (((Rom)rom).Size != -1 ? " size " + ((Rom)rom).Size : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date " + ((Rom)rom).Date : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc " + ((Rom)rom).CRC.ToLowerInvariant() : "")
									+ " )\n";
								break;
						}
						break;
					case OutputFormat.Logiqx:
						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\t\t<archive name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
							case ItemType.BiosSet:
								state += "\t\t<biosset name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Disk:
								state += "\t\t<disk name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n";
								break;
							case ItemType.Release:
								state += "\t\t<release name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Rom:
								state += "\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n";
								break;
							case ItemType.Sample:
								state += "\t\t<file type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
						}
						break;
					case OutputFormat.MissFile:
						// Missfile should only output Rom and Disk
						if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
						{
							return true;
						}

						pre = Prefix + (Quotes ? "\"" : "");
						post = (Quotes ? "\"" : "") + Postfix;

						if (rom.Type == ItemType.Rom)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
						}
						else if (rom.Type == ItemType.Disk)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
						}

						// If we're in Romba mode, the state is consistent
						if (Romba)
						{
							if (rom.Type == ItemType.Rom)
							{
								// We can only write out if there's a SHA-1
								if (((Rom)rom).SHA1 != "")
								{
									name = ((Rom)rom).SHA1.Substring(0, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(2, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(4, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(6, 2)
										+ "/" + ((Rom)rom).SHA1 + ".gz";
									state += pre + name + post + "\n";
								}
							}
							else if (rom.Type == ItemType.Disk)
							{
								// We can only write out if there's a SHA-1
								if (((Disk)rom).SHA1 != "")
								{
									name = ((Disk)rom).SHA1.Substring(0, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(2, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(4, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(6, 2)
										+ "/" + ((Disk)rom).SHA1 + ".gz";
									state += pre + name + post + "\n";
								}
							}
						}

						// Otherwise, use any flags
						name = (UseGame ? rom.Machine.Name : rom.Name);
						if (RepExt != "" || RemExt)
						{
							if (RemExt)
							{
								RepExt = "";
							}

							string dir = Path.GetDirectoryName(name);
							dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
							name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + RepExt);
						}
						if (AddExt != "")
						{
							name += AddExt;
						}
						if (!UseGame && GameName)
						{
							name = Path.Combine(rom.Machine.Name, name);
						}

						if (UseGame && rom.Machine.Name != lastgame)
						{
							state += pre + name + post + "\n";
							lastgame = rom.Machine.Name;
						}
						else if (!UseGame)
						{
							state += pre + name + post + "\n";
						}
						break;
					case OutputFormat.OfflineList:
						state += "\t\t<game>\n"
							+ "\t\t\t<imageNumber>1</imageNumber>\n"
							+ "\t\t\t<releaseNumber>1</releaseNumber>\n"
							+ "\t\t\t<title>" + HttpUtility.HtmlEncode(rom.Name) + "</title>\n"
							+ "\t\t\t<saveType>None</saveType>\n";

						if (rom.Type == ItemType.Rom)
						{
							state += "\t\t\t<romSize>" + ((Rom)rom).Size + "</romSize>\n";
						}

						state += "\t\t\t<publisher>None</publisher>\n"
							+ "\t\t\t<location>0</location>\n"
							+ "\t\t\t<sourceRom>None</sourceRom>\n"
							+ "\t\t\t<language>0</language>\n";

						if (rom.Type == ItemType.Disk)
						{
							state += "\t\t\t<files>\n"
								+ (((Disk)rom).MD5 != null
									? "\t\t\t\t<romMD5 extension=\".chd\">" + ((Disk)rom).MD5.ToUpperInvariant() + "</romMD5>\n"
									: "\t\t\t\t<romSHA1 extension=\".chd\">" + ((Disk)rom).SHA1.ToUpperInvariant() + "</romSHA1>\n")
								+ "\t\t\t</files>\n";
						}
						else if (rom.Type == ItemType.Rom)
						{
							string tempext = Path.GetExtension(((Rom)rom).Name);
							if (!tempext.StartsWith("."))
							{
								tempext = "." + tempext;
							}

							state += "\t\t\t<files>\n"
								+ (((Rom)rom).CRC != null
									? "\t\t\t\t<romCRC extension=\"" + tempext + "\">" + ((Rom)rom).CRC.ToUpperInvariant() + "</romMD5>\n"
									: ((Rom)rom).MD5 != null
										? "\t\t\t\t<romMD5 extension=\"" + tempext + "\">" + ((Rom)rom).MD5.ToUpperInvariant() + "</romMD5>\n"
										: "\t\t\t\t<romSHA1 extension=\"" + tempext + "\">" + ((Rom)rom).SHA1.ToUpperInvariant() + "</romSHA1>\n")
								+ "\t\t\t</files>\n";
						}

						state += "\t\t\t<im1CRC>00000000</im1CRC>\n"
							+ "\t\t\t<im2CRC>00000000</im2CRC>\n"
							+ "\t\t\t<comment></comment>\n"
							+ "\t\t\t<duplicateID>0</duplicateID>\n"
							+ "\t\t</game>\n";
						break;
					case OutputFormat.RedumpMD5:
						if (rom.Type == ItemType.Rom)
						{
							state += ((Rom)rom).MD5 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += ((Disk)rom).MD5 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						break;
					case OutputFormat.RedumpSFV:
						if (rom.Type == ItemType.Rom)
						{
							state += (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + " " + ((Rom)rom).CRC + "\n";
						}
						break;
					case OutputFormat.RedumpSHA1:
						if (rom.Type == ItemType.Rom)
						{
							state += ((Rom)rom).SHA1 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += ((Disk)rom).SHA1 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						break;
					case OutputFormat.RomCenter:
						if (rom.Type == ItemType.Rom)
						{
							state += "¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + HttpUtility.HtmlEncode(rom.Machine.Name) +
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬" + ((Rom)rom).CRC.ToLowerInvariant() +
							"¬" + (((Rom)rom).Size != -1 ? ((Rom)rom).Size.ToString() : "") + "¬¬¬\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += "¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + HttpUtility.HtmlEncode(rom.Machine.Name) +
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬¬¬¬¬\n";
						}

						break;
					case OutputFormat.SabreDat:
						string prefix = "";
						for (int i = 0; i < depth; i++)
						{
							prefix += "\t";
						}
						state += prefix;

						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "<file type=\"archive\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
							case ItemType.BiosSet:
								state += "<file type=\"biosset\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Disk:
								state += "<file type=\"disk\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? prefix + "/>\n" + prefix + "\t<flags>\n" +
										prefix + "\t\t<flag name=\"status\" value=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"/>\n" +
										prefix + "\t</flags>\n" +
										prefix + "</file>\n" : "/>\n");
								break;
							case ItemType.Release:
								state += "<file type=\"release\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Rom:
								state += "<file type=\"rom\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? prefix + "/>\n" + prefix + "\t<flags>\n" +
										prefix + "\t\t<flag name=\"status\" value=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"/>\n" +
										prefix + "\t</flags>\n" +
										prefix + "</file>\n" : "/>\n");
								break;
							case ItemType.Sample:
								state += "<file type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
						}
						break;
					case OutputFormat.SoftwareList:
						state += "\t\t<part name=\"" + rom.PartName + "\" interface=\"" + rom.PartInterface + "\">\n";

						foreach (Tuple<string, string> kvp in rom.Features)
						{
							state += "\t\t\t<feature name=\"" + HttpUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + HttpUtility.HtmlEncode(kvp.Item2) + "\"/>\n";
						}

						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "archive" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<archive name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.BiosSet:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "biosset" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<biosset name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.Disk:
								state += "\t\t\t<diskarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "cdrom" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<disk name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n"
									+ "\t\t\t</diskarea>\n";
								break;
							case ItemType.Release:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "release" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<release name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.Rom:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "rom" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.Sample:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "sample" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<sample type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
						}

						state += "\t\t</part>\n";
						break;
					case OutputFormat.TSV:
						// TSV should only output Rom and Disk
						if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
						{
							return true;
						}

						pre = Prefix + (Quotes ? "\"" : "");
						post = (Quotes ? "\"" : "") + Postfix;

						if (rom.Type == ItemType.Rom)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
						}
						else if (rom.Type == ItemType.Disk)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
						}

						if (rom.Type == ItemType.Rom)
						{
							string inline = "\"" + FileName + "\""
								+ "\t\"" + Name + "\""
								+ "\t\"" + Description + "\""
								+ "\t\"" + rom.Machine.Name + "\""
								+ "\t\"" + rom.Machine.Description + "\""
								+ "\t" + "\"rom\""
								+ "\t\"" + rom.Name + "\""
								+ "\t" + "\"\""
								+ "\t\"" + ((Rom)rom).Size + "\""
								+ "\t\"" + ((Rom)rom).CRC + "\""
								+ "\t\"" + ((Rom)rom).MD5 + "\""
								+ "\t\"" + ((Rom)rom).SHA1 + "\""
								+ "\t" + (((Rom)rom).ItemStatus != ItemStatus.None ? "\"" + ((Rom)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							string inline = "\"" + FileName + "\""
								+ "\t\"" + Name + "\""
								+ "\t\"" + Description + "\""
								+ "\t\"" + rom.Machine.Name + "\""
								+ "\t\"" + rom.Machine.Description + "\""
								+ "\t" + "\"disk\""
								+ "\t" + "\"\""
								+ "\t\"" + rom.Name + "\""
								+ "\t" + "\"\""
								+ "\t" + "\"\""
								+ "\t\"" + ((Disk)rom).MD5 + "\""
								+ "\t\"" + ((Disk)rom).SHA1 + "\""
								+ "\t" + (((Disk)rom).ItemStatus != ItemStatus.None ? "\"" + ((Disk)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT footer using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteFooter(StreamWriter sw, OutputFormat outputFormat, int depth, Logger logger)
		{
			try
			{
				string footer = "";

				// If we have roms, output the full footer
				if (Files != null && Files.Count > 0)
				{
					switch (outputFormat)
					{
						case OutputFormat.ClrMamePro:
						case OutputFormat.DOSCenter:
							footer = ")\n";
							break;
						case OutputFormat.Logiqx:
							footer = "\t</machine>\n</datafile>\n";
							break;
						case OutputFormat.OfflineList:
							footer = "\t\t</game>"
								+ "\t</games>\n"
								+ "\t<gui>\n"
								+ "\t\t<images width=\"487\" height=\"162\">\n"
								+ "\t\t\t<image x=\"0\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t\t<image x=\"245\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t</images>\n"
								+ "\t</gui>\n"
								+ "</dat>";
							break;
						case OutputFormat.SabreDat:
							for (int i = depth - 1; i >= 2; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									footer += "\t";
								}
								footer += "</directory>\n";
							}
							footer += "\t</data>\n</datafile>\n";
							break;
						case OutputFormat.SoftwareList:
							footer = "\t</software>\n\n</softwarelist>\n";
							break;
					}
				}

				// Otherwise, output the abbreviated form
				else
				{
					switch (outputFormat)
					{
						case OutputFormat.Logiqx:
						case OutputFormat.SabreDat:
							footer = "</datafile>\n";
							break;
						case OutputFormat.OfflineList:
							footer = "\t</games>\n"
								+ "\t<gui>\n"
								+ "\t\t<images width=\"487\" height=\"162\">\n"
								+ "\t\t\t<image x=\"0\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t\t<image x=\"245\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t</images>\n"
								+ "\t</gui>\n"
								+ "</dat>";
							break;
						case OutputFormat.SoftwareList:
							footer = "</softwarelist>\n";
							break;
					}
				}

				// Write the footer out
				sw.Write(footer);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		#endregion

		#endregion // Instance Methods

		#region Static Methods

		#region Bucketing

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
				count++;
				string newkey = (norename ? ""
						: rom.SystemID.ToString().PadLeft(10, '0')
							+ "-"
							+ rom.SourceID.ToString().PadLeft(10, '0') + "-")
					+ (String.IsNullOrEmpty(rom.Machine.Name)
							? "Default"
							: rom.Machine.Name.ToLowerInvariant());
				newkey = HttpUtility.HtmlEncode(newkey);
				if (sortable.ContainsKey(newkey))
				{
					sortable[newkey].Add(rom);
				}
				else
				{
					List<DatItem> temp = new List<DatItem>();
					temp.Add(rom);
					sortable.Add(newkey, temp);
				}
			}

			return sortable;
		}

		#endregion

		#region Statistics

		/// <summary>
		/// Output the stats for a list of input dats as files in a human-readable format
		/// </summary>
		/// <param name="inputs">List of input files and folders</param>
		/// <param name="reportName">Name of the output file</param>
		/// <param name="single">True if single DAT stats are output, false otherwise</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		/// <param name="statOutputFormat" > Set the statistics output format to use</param>
		/// <param name="logger">Logger object for file and console output</param>
		public static void OutputStats(List<string> inputs, string reportName, bool single, bool baddumpCol,
			bool nodumpCol, StatOutputFormat statOutputFormat, Logger logger)
		{
			reportName += OutputStatsGetExtension(statOutputFormat);
			StreamWriter sw = new StreamWriter(File.Open(reportName, FileMode.Create, FileAccess.Write));

			// Make sure we have all files
			List<Tuple<string, string>> newinputs = new List<Tuple<string, string>>(); // item, basepath
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					newinputs.Add(Tuple.Create(Path.GetFullPath(input), Path.GetDirectoryName(Path.GetFullPath(input))));
				}
				if (Directory.Exists(input))
				{
					foreach (string file in Directory.GetFiles(input, "*", SearchOption.AllDirectories))
					{
						newinputs.Add(Tuple.Create(Path.GetFullPath(file), Path.GetFullPath(input)));
					}
				}
			}
			newinputs = newinputs
				.OrderBy(i => Path.GetDirectoryName(i.Item1))
				.ThenBy(i => Path.GetFileName(i.Item1))
				.ToList();

			// Write the header, if any
			OutputStatsWriteHeader(sw, statOutputFormat, baddumpCol, nodumpCol);

			// Init all total variables
			long totalSize = 0;
			long totalGame = 0;
			long totalRom = 0;
			long totalDisk = 0;
			long totalCRC = 0;
			long totalMD5 = 0;
			long totalSHA1 = 0;
			long totalBaddump = 0;
			long totalNodump = 0;

			// Init directory-level variables
			string lastdir = null;
			string basepath = null;
			long dirSize = 0;
			long dirGame = 0;
			long dirRom = 0;
			long dirDisk = 0;
			long dirCRC = 0;
			long dirMD5 = 0;
			long dirSHA1 = 0;
			long dirBaddump = 0;
			long dirNodump = 0;

			// Now process each of the input files
			foreach (Tuple<string, string> filename in newinputs)
			{
				// Get the directory for the current file
				string thisdir = Path.GetDirectoryName(filename.Item1);
				basepath = Path.GetDirectoryName(filename.Item2);

				// If we don't have the first file and the directory has changed, show the previous directory stats and reset
				if (lastdir != null && thisdir != lastdir)
				{
					// Output separator if needed
					OutputStatsWriteMidSeparator(sw, statOutputFormat, baddumpCol, nodumpCol);
					
					DatFile lastdirdat = new DatFile
					{
						FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
						TotalSize = dirSize,
						RomCount = dirRom,
						DiskCount = dirDisk,
						CRCCount = dirCRC,
						MD5Count = dirMD5,
						SHA1Count = dirSHA1,
						BaddumpCount = dirBaddump,
						NodumpCount = dirNodump,
					};
					lastdirdat.OutputStats(sw, statOutputFormat, logger, game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);

					// Write the mid-footer, if any
					OutputStatsWriteMidFooter(sw, statOutputFormat, baddumpCol, nodumpCol);

					// Write the header, if any
					OutputStatsWriteMidHeader(sw, statOutputFormat, baddumpCol, nodumpCol);

					// Reset the directory stats
					dirSize = 0;
					dirGame = 0;
					dirRom = 0;
					dirDisk = 0;
					dirCRC = 0;
					dirMD5 = 0;
					dirSHA1 = 0;
					dirBaddump = 0;
					dirNodump = 0;
				}

				logger.Verbose("Beginning stat collection for '" + filename.Item1 + "'", false);
				List<string> games = new List<string>();
				DatFile datdata = new DatFile();
				datdata.Parse(filename.Item1, 0, 0, logger);
				datdata.BucketByGame(false, true, logger, false);

				// Output single DAT stats (if asked)
				logger.User("Adding stats for file '" + filename.Item1 + "'\n", false);
				if (single)
				{
					datdata.OutputStats(sw, statOutputFormat, logger, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
				}

				// Add single DAT stats to dir
				dirSize += datdata.TotalSize;
				dirGame += datdata.Files.Count;
				dirRom += datdata.RomCount;
				dirDisk += datdata.DiskCount;
				dirCRC += datdata.CRCCount;
				dirMD5 += datdata.MD5Count;
				dirSHA1 += datdata.SHA1Count;
				dirBaddump += datdata.BaddumpCount;
				dirNodump += datdata.NodumpCount;

				// Add single DAT stats to totals
				totalSize += datdata.TotalSize;
				totalGame += datdata.Files.Count;
				totalRom += datdata.RomCount;
				totalDisk += datdata.DiskCount;
				totalCRC += datdata.CRCCount;
				totalMD5 += datdata.MD5Count;
				totalSHA1 += datdata.SHA1Count;
				totalBaddump += datdata.BaddumpCount;
				totalNodump += datdata.NodumpCount;

				// Make sure to assign the new directory
				lastdir = thisdir;
			}

			// Output the directory stats one last time
			OutputStatsWriteMidSeparator(sw, statOutputFormat, baddumpCol, nodumpCol);

			if (single)
			{
				DatFile dirdat = new DatFile
				{
					FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
					TotalSize = dirSize,
					RomCount = dirRom,
					DiskCount = dirDisk,
					CRCCount = dirCRC,
					MD5Count = dirMD5,
					SHA1Count = dirSHA1,
					BaddumpCount = dirBaddump,
					NodumpCount = dirNodump,
				};
				dirdat.OutputStats(sw, statOutputFormat, logger, game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
			}

			// Write the mid-footer, if any
			OutputStatsWriteMidFooter(sw, statOutputFormat, baddumpCol, nodumpCol);

			// Write the header, if any
			OutputStatsWriteMidHeader(sw, statOutputFormat, baddumpCol, nodumpCol);

			// Reset the directory stats
			dirSize = 0;
			dirGame = 0;
			dirRom = 0;
			dirDisk = 0;
			dirCRC = 0;
			dirMD5 = 0;
			dirSHA1 = 0;
			dirNodump = 0;

			// Output total DAT stats
			DatFile totaldata = new DatFile
			{
				FileName = "DIR: All DATs",
				TotalSize = totalSize,
				RomCount = totalRom,
				DiskCount = totalDisk,
				CRCCount = totalCRC,
				MD5Count = totalMD5,
				SHA1Count = totalSHA1,
				BaddumpCount = totalBaddump,
				NodumpCount = totalNodump,
			};
			totaldata.OutputStats(sw, statOutputFormat, logger, game: totalGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);

			// Output footer if needed
			OutputStatsWriteFooter(sw, statOutputFormat);

			sw.Flush();
			sw.Dispose();

			logger.User(@"
Please check the log folder if the stats scrolled offscreen", false);
		}

		/// <summary>
		/// Get the proper extension for the stat output format
		/// </summary>
		/// <param name="statOutputFormat">StatOutputFormat to get the extension for</param>
		/// <returns>File extension with leading period</returns>
		private static string OutputStatsGetExtension(StatOutputFormat statOutputFormat)
		{
			string reportExtension = "";
			switch (statOutputFormat)
			{
				case StatOutputFormat.CSV:
					reportExtension = ".csv";
					break;
				case StatOutputFormat.HTML:
					reportExtension = ".html";
					break;
				case StatOutputFormat.None:
				default:
					reportExtension = ".txt";
					break;
				case StatOutputFormat.TSV:
					reportExtension = ".csv";
					break;
			}
			return reportExtension;
		}

		/// <summary>
		/// Write out the header to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statOutputFormat">StatOutputFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteHeader(StreamWriter sw, StatOutputFormat statOutputFormat, bool baddumpCol, bool nodumpCol)
		{
			string head = "";
			switch (statOutputFormat)
			{
				case StatOutputFormat.CSV:
					break;
				case StatOutputFormat.HTML:
					head = @"<!DOCTYPE html>
<html>
	<header>
		<title>DAT Statistics Report</title>
		<style>
			body {
				background-color: lightgray;
			}
			.dir {
				color: #0088FF;
			}
			.right {
				align: right;
			}
		</style>
	</header>
	<body>
		<h2>DAT Statistics Report (" + DateTime.Now.ToShortDateString() + @")</h2>
		<table border=""1"" cellpadding=""5"" cellspacing=""0"">
";
					break;
				case StatOutputFormat.None:
				default:
					break;
				case StatOutputFormat.TSV:
					break;
			}
			sw.Write(head);

			// Now write the mid header for those who need it
			OutputStatsWriteMidHeader(sw, statOutputFormat, baddumpCol, nodumpCol);
		}

		/// <summary>
		/// Write out the mid-header to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statOutputFormat">StatOutputFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidHeader(StreamWriter sw, StatOutputFormat statOutputFormat, bool baddumpCol, bool nodumpCol)
		{
			string head = "";
			switch (statOutputFormat)
			{
				case StatOutputFormat.CSV:
					head = "\"File Name\",\"Total Size\",\"Games\",\"Roms\",\"Disks\",\"# with CRC\",\"# with MD5\",\"# with SHA-1\""
						+ (baddumpCol ? ",\"BadDumps\"" : "") + (nodumpCol ? ",\"Nodumps\"" : "") + "\n";
					break;
				case StatOutputFormat.HTML:
					head = @"			<tr bgcolor=""gray""><th>File Name</th><th align=""right"">Total Size</th><th align=""right"">Games</th><th align=""right"">Roms</th>"
+ @"<th align=""right"">Disks</th><th align=""right"">&#35; with CRC</th><th align=""right"">&#35; with MD5</th><th align=""right"">&#35; with SHA-1</th>"
+ (baddumpCol ? "<th class=\".right\">Baddumps</th>" : "") + (nodumpCol ? "<th class=\".right\">Nodumps</th>" : "") + "</tr>\n";
					break;
				case StatOutputFormat.None:
				default:
					break;
				case StatOutputFormat.TSV:
					head = "\"File Name\"\t\"Total Size\"\t\"Games\"\t\"Roms\"\t\"Disks\"\t\"# with CRC\"\t\"# with MD5\"\t\"# with SHA-1\""
						+ (baddumpCol ? "\t\"BadDumps\"" : "") + (nodumpCol ? "\t\"Nodumps\"" : "") + "\n";
					break;
			}
			sw.Write(head);
		}

		/// <summary>
		/// Write out the separator to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statOutputFormat">StatOutputFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidSeparator(StreamWriter sw, StatOutputFormat statOutputFormat, bool baddumpCol, bool nodumpCol)
		{
			string mid = "";
			switch (statOutputFormat)
			{
				case StatOutputFormat.CSV:
					break;
				case StatOutputFormat.HTML:
					mid = "<tr><td colspan=\""
						+ (baddumpCol && nodumpCol
							? "11"
							: (baddumpCol ^ nodumpCol
								? "10"
								: "9")
							)
						+ "\"></td></tr>\n";
					break;
				case StatOutputFormat.None:
				default:
					break;
			}
			sw.Write(mid);
		}

		/// <summary>
		/// Write out the footer-separator to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statOutputFormat">StatOutputFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidFooter(StreamWriter sw, StatOutputFormat statOutputFormat, bool baddumpCol, bool nodumpCol)
		{
			string end = "";
			switch (statOutputFormat)
			{
				case StatOutputFormat.CSV:
					end = "\n";
					break;
				case StatOutputFormat.HTML:
					end = "<tr border=\"0\"><td colspan=\""
						+ (baddumpCol && nodumpCol
							? "11"
							: (baddumpCol ^ nodumpCol
								? "10"
								: "9")
							)
						+ "\"></td></tr>\n";
					break;
				case StatOutputFormat.None:
				default:
					end = "\n";
					break;
				case StatOutputFormat.TSV:
					end = "\n";
					break;
			}
			sw.Write(end);
		}

		/// <summary>
		/// Write out the footer to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statOutputFormat">StatOutputFormat representing output format</param>
		private static void OutputStatsWriteFooter(StreamWriter sw, StatOutputFormat statOutputFormat)
		{
			string end = "";
			switch (statOutputFormat)
			{
				case StatOutputFormat.CSV:
					break;
				case StatOutputFormat.HTML:
					end = @"		</table>
	</body>
</html>
";
					break;
				case StatOutputFormat.None:
				default:
					break;
				case StatOutputFormat.TSV:
					break;
			}
			sw.Write(end);
		}

		#endregion

		#endregion // Static Methods
	}
}
