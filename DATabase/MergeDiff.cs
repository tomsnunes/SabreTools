using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class MergeDiff
	{
		// Listing related variables
		private List<String> _inputs;

		// User specified flags
		private bool _ad;
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
			bool ad, bool diff, bool dedup, bool bare, bool forceunpack, bool old, Logger logger)
		{
			_inputs = inputs;
			_name = name;
			_desc = desc;
			_cat = cat;
			_version = version;
			_author = author;
			_ad = ad;
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
			if ((!_ad && _inputs.Count < 1) || (_ad && _inputs.Count < 2))
			{
				_logger.Warning("At least " + (_ad ? "2" : "1") + " input(s) are required!");
				return false;
			}

			// Get the values that will be used
			if (_name == "")
			{
				_name = (_diff ? "diffdat" : "mergedat") + (_dedup ? "-deduped" : "");
			}
			if (_desc == "")
			{
				_desc = (_diff ? "diffdat" : "mergedat") + (_dedup ? "-deduped" : "");
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

			// Create the database of ROMs from the input DATs
			SqliteConnection dbc = DBTools.InMemoryDb();
			foreach (string input in _inputs)
			{
				_logger.Log("Adding DAT: " + input);
				RomManipulation.Parse2(input, 0, 0, _dedup, dbc, _logger);
			}

			// Output all DATs specified by user inputs
			Output.WriteToDatFromDb(_name, _desc, _version, _date, _cat, _author, _forceunpack, _old, _diff, _ad, "", dbc, _logger);
			dbc.Close();

			return true;
		}
	}
}
