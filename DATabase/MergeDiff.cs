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
		/// <remarks>
		/// TODO: @tractivo -for the A and B and AB output you could let this be determined by comparing the hashes.
		/// 	when a hash is present in both dats then this entry goes to AB, if its only in A then it stay in A if in B then in B.
		/// </remarks>
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
				_name = (_diff ? "diffdat" : "mergedat") + (_dedup ? "-merged" : "");
			}
			if (_desc == "")
			{
				_desc = (_diff ? "diffdat" : "mergedat") + (_dedup ? "-merged" : "");
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

			List<RomData> A = new List<RomData>();

			SqliteConnection dbc = DBTools.InMemoryDb();

			foreach (string input in _inputs)
			{
				_logger.Log("Adding DAT: " + input);
				RomManipulation.Parse2(input, 0, 0, _dedup, dbc, _logger);

				/*
				List<RomData> B = RomManipulation.Parse(input, 0, 0, _logger);
				if (_diff)
				{
					A = RomManipulation.Diff(A, B);
				}
				else
				{
					A.AddRange(B);
				}
				*/
			}

			// Until I find a way to output the roms from the db, here's just a count of the items in it
			using (SqliteCommand slc = new SqliteCommand("SELECT count(*) FROM roms", dbc))
			{
				_logger.Log("Total number of lines in database: " + slc.ExecuteScalar());
			}
			Output.WriteToDat2(_name + "-db", _desc + "-db", _version, _date, _cat, _author, _forceunpack, _old, _diff, "", dbc, _logger);

			dbc.Close();

			// If we're in Alldiff mode, we can only use the first 2 inputs
			if (_ad)
			{
				List<RomData> AB1 = RomManipulation.Parse(_inputs[0], 0, 0, _logger);
				List<RomData> AB2 = RomManipulation.Parse(_inputs[1], 0, 0, _logger);

				List<RomData> OnlyA = RomManipulation.DiffOnlyInA(AB1, AB2);
				List<RomData> OnlyB = RomManipulation.DiffOnlyInA(AB2, AB1);
				List<RomData> BothAB = RomManipulation.DiffInAB(AB1, AB2);

				string input0 = Path.GetFileNameWithoutExtension(_inputs[0]);
				string input1 = Path.GetFileNameWithoutExtension(_inputs[1]);

				Output.WriteToDat(_name + "-" + input0 + "-only", _desc + "-" + input0 + "-only", _version, _date, _cat, _author, _forceunpack, _old, "", OnlyA, _logger);
				Output.WriteToDat(_name + "-" + input1 + "-only", _desc + "-" + input1 + "-only", _version, _date, _cat, _author, _forceunpack, _old, "", OnlyB, _logger);
				Output.WriteToDat(_name + "-inboth", _desc + "-inboth", _version, _date, _cat, _author, _forceunpack, _old, "", BothAB, _logger);
			}

			/*
			// If we want a merged list, send it for merging before outputting
			if (_dedup)
			{
				A = RomManipulation.Merge(A);
			}

			// Sort the file by names for ease
			RomManipulation.Sort(A, true);

			// Now write the file out
			Output.WriteToDat(_name, _desc, _version, _date, _cat, _author, _forceunpack, _old, "", A, _logger);
			*/

			return true;
		}
	}
}
