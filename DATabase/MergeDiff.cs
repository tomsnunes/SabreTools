using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class MergeDiff
	{
		// Listing related variables
		private List<String> _inputs;

		// User specified flags
		private bool _diff;
		private bool _dedup;
		private bool _bare;
		private bool _forceunpack;
		private bool _old;

		// User specified strings
		private string _name;
		private string _desc;
		private string _cat;
		private string _version;
		private string _author;

		// Other required variables
		private string _date = DateTime.Now.ToString("yyyy-MM-dd");
		private Logger _logger;

		/// <summary>
		/// Create a new MergeDAT object
		/// </summary>
		/// <param name="inputs">A List of Strings representing the DATs or DAT folders to be merged</param>
		/// <param name="name">Internal name of the DAT</param>
		/// <param name="desc">Description and external name of the DAT</param>
		/// <param name="cat">Category for the DAT</param>
		/// <param name="version">Version of the DAT</param>
		/// <param name="author">Author of the DAT</param>
		/// <param name="ad">True if all diff variants should be outputted, false otherwise</param>
		/// <param name="diff">True if a DiffDat of all inputs is wanted, false otherwise</param>
		/// <param name="dedup">True if the outputted file should remove duplicates, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="forceunpack">True if the forcepacking="unzip" tag is to be added, false otherwise</param>
		/// <param name="old">True if a old-style DAT should be output, false otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>
		public MergeDiff(List<String> inputs, string name, string desc, string cat, string version, string author,
			bool diff, bool dedup, bool bare, bool forceunpack, bool old, Logger logger)
		{
			_inputs = inputs;
			_name = name;
			_desc = desc;
			_cat = cat;
			_version = version;
			_author = author;
			_diff = diff;
			_dedup = dedup;
			_bare = bare;
			_forceunpack = forceunpack;
			_old = old;
			_logger = logger;
		}

		/// <summary>
		/// Combine DATs, optionally diffing and deduping them
		/// </summary>
		/// <returns>True if the DATs merged correctly, false otherwise</returns>
		public bool Process()
		{
			// Check if there are enough inputs
			if (_inputs.Count < 1)
			{
				_logger.Warning("At least 1 input is required!");
				return false;
			}

			// Get the values that will be used
			if (_name == "")
			{
				_name = (_diff ? "DiffDAT" : "") + (_dedup ? "-deduped" : "");
			}
			if (_desc == "")
			{
				_desc = (_diff ? "DiffDAT" : "MergeDAT") + (_dedup ? " - deduped" : "");
				if (!_bare)
				{
					_desc += " (" + _date + ")";
				}
			}
			if (_cat == "" && _diff)
			{
				_cat = "DiffDAT";
			}
			if (_author == "")
			{
				_author = "SabreTools";
			}

			// Create a dictionary of all ROMs from the input DATs
			int i = 0;
			Dictionary<string, List<RomData>> dict = new Dictionary<string, List<RomData>>();
			foreach (string input in _inputs)
			{
				_logger.User("Adding DAT: " + input);
				dict = RomManipulation.ParseDict(input, i, 0, dict, _logger);
				i++;
			}
			
			// Modify the Dictionary if necessary and output the results
			if (_diff)
			{
				string post = "";

				// Get all entries that don't have External dupes
				Dictionary<string, List<RomData>> diffed = new Dictionary<string, List<RomData>>();
				foreach (string key in dict.Keys)
				{
					List<RomData> temp = dict[key];
					temp = RomManipulation.Merge(temp);

					foreach (RomData rom in temp)
					{
						if (rom.Dupe < DupeType.ExternalHash)
						{
							if (diffed.ContainsKey(key))
							{
								diffed[key].Add(rom);
							}
							else
							{
								List<RomData> tl = new List<RomData>();
								tl.Add(rom);
								diffed.Add(key, tl);
							}
						}
					}
				}

				post = " (No Duplicates)";

				// Output the difflist (a-b)+(b-a) diff
				Output.WriteToDatFromDict(_name + post, _desc + post, _version, _date, _cat, _author, _forceunpack, _old, _dedup, "", diffed, _logger);

				// For the AB mode-style diffs, get all required dictionaries and output with a new name
				// Loop through _inputs first and filter from all diffed roms to find the ones that have the same "System"
				for (int j = 0; j < _inputs.Count; j++)
				{
					Dictionary<string, List<RomData>> sysDict = new Dictionary<string, List<RomData>>();
					foreach (string key in diffed.Keys)
					{
						foreach (RomData rom in diffed[key])
						{
							if (rom.SystemID == j)
							{
								if (sysDict.ContainsKey(key))
								{
									sysDict[key].Add(rom);
								}
								else
								{
									List<RomData> tl = new List<RomData>();
									tl.Add(rom);
									sysDict.Add(key, tl);
								}
							}
						}
					}

					post = " (" + Path.GetFileNameWithoutExtension(_inputs[j]) + " Only)";
					Output.WriteToDatFromDict(_name + post, _desc + post, _version, _date, _cat, _author, _forceunpack, _old, _dedup, "", sysDict, _logger);
				}

				// Get all entries that have External dupes
				Dictionary<string, List<RomData>> duplicates = new Dictionary<string, List<RomData>>();
				post = " (Duplicates)";
				foreach (string key in dict.Keys)
				{
					List<RomData> temp = dict[key];
					temp = RomManipulation.Merge(temp);

					foreach (RomData rom in temp)
					{
						if (rom.Dupe >= DupeType.ExternalHash)
						{
							if (duplicates.ContainsKey(key))
							{
								duplicates[key].Add(rom);
							}
							else
							{
								List<RomData> tl = new List<RomData>();
								tl.Add(rom);
								duplicates.Add(key, tl);
							}
						}
					}
				}
				Output.WriteToDatFromDict(_name + post, _desc + post, _version, _date, _cat, _author, _forceunpack, _old, _dedup, "", duplicates, _logger);
			}
			// Output all entries with user-defined merge
			else
			{
				Output.WriteToDatFromDict(_name, _desc, _version, _date, _cat, _author, _forceunpack, _old, _dedup, "", dict, _logger);
			}

			return true;
		}
	}
}
