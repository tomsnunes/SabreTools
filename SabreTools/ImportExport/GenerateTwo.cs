using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SabreTools
{
	public class GenerateTwo : IGenerate
	{
		// Private instance variables
		private string _systemid;
		private string _sourceid;
		private string _datroot;
		private string _outroot;
		private string _connectionString;
		private bool _norename;
		private bool _old;

		// Private required variables
		private Logger _logger;

		/// <summary>
		/// Initialize a Generate object with the given information
		/// </summary>
		/// <param name="systemid">String representing the system id (blank means all)</param>
		/// <param name="sourceid">String representing the source id (blank means all) [CURRENTLY UNUSED]</param>
		/// <param name="datroot">Root directory where all DAT files are held</param>
		/// <param name="outroot">Root directory where new DAT files are output</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <param name="logger">Logger object for file or console output</param>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in ClrMamePro format (default false)</param>
		public GenerateTwo(string systemid, string sourceid, string datroot, string outroot, string connectionString, Logger logger, bool norename = false, bool old = false)
		{
			_systemid = systemid;
			_sourceid = sourceid;
			_datroot = datroot;
			_outroot = outroot;
			_connectionString = connectionString;
			_logger = logger;
			_norename = norename;
			_old = old;
		}

		/// <summary>
		/// Generate a DAT file that is represented by the data in the Generate object.
		/// </summary>
		/// <returns>True if the file could be created, false otherwise</returns>
		public bool Export()
		{
			string name = "";
			string path = _datroot;

			// If the System ID isn't set, then we will merge everything
			if (_systemid != "")
			{
				string system = "";

				// First get the name of the system, if possible
				string query = "SELECT manufacturer, name FROM system WHERE id=" + _systemid;
				using (SqliteConnection dbc = new SqliteConnection(_connectionString))
				{
					dbc.Open();

					using (SqliteCommand slc = new SqliteCommand(query, dbc))
					{
						using (SqliteDataReader sldr = slc.ExecuteReader())
						{
							if (sldr.Read())
							{
								system = sldr.GetString(0) + " - " + sldr.GetString(1);
							}
						}
					}
				}

				// If we didn't find anything, then return
				if (system == "")
				{
					_logger.Warning("No system could be found with id " + _systemid);
					return false;
				}

				name = system.Trim();
				path = Path.Combine(path, name);
			}
			else
			{
				name = "ALL";
			}

			// Get the rest of the info as well
			string date = DateTime.Now.ToString("yyyyMMddHHmmss");
			string description = name + " (merged " + date + ")";
			name += " (merged)";

			// For good measure, get all sources
			Dictionary<int, string> sources = new Dictionary<int, string>();
			sources.Add(0, "Default");

			string squery = "SELECT id, name FROM source";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				using (SqliteCommand slc = new SqliteCommand(squery, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							sources.Add(sldr.GetInt32(0), sldr.GetString(1));
						}
					}
				}
			}

			// Get a list of files to sourceid mappings
			Dictionary<string, string> sourcemap = new Dictionary<string, string>();
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				string tquery = "SELECT DISTINCT dats.sha1, datsdata.value FROM dats JOIN datsdata ON dats.id=datsdata.id WHERE key='source'";
				using (SqliteCommand slc = new SqliteCommand(tquery, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							string tempsha1 = sldr.GetString(0);
							string tempval = sldr.GetString(1);
							if (!sourcemap.ContainsKey(tempsha1))
							{
								sourcemap.Add(tempsha1, tempval);
							}
						}
					}
				}
			}

			// Create the output DatData object
			Dat datdata = new Dat
			{
				FileName = description,
				Name = name,
				Description = description,
				Version = "",
				Date = date,
				Category = "SabreTools",
				Author = "SabreTools",
				ForcePacking = ForcePacking.None,
				OutputFormat = (_old ? OutputFormat.ClrMamePro : OutputFormat.Xml),
				MergeRoms = true,
			};

			// Now read in all of the files
			SHA1 sha1 = SHA1.Create();
			foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
			{
				string hash = "";
				using (FileStream fs = File.Open(file, FileMode.Open))
				{
					hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
				}

				int tempSrcId = 0;
				if (sourcemap.ContainsKey(hash))
				{
					Int32.TryParse(sourcemap[hash], out tempSrcId);
				}
				datdata = DatTools.Parse(file, 0, tempSrcId, datdata, _logger);
			}

			// If the dictionary is empty for any reason, tell the user and exit
			if (datdata.Roms.Keys.Count == 0 || datdata.Roms.Count == 0)
			{
				_logger.Log("No roms found for system ID " + _systemid);
				return false;
			}

			// Now process all of the roms
			_logger.User("Cleaning rom data");
			List<string> keys = datdata.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> temp = new List<Rom>();
				List<Rom> newroms = datdata.Roms[key];
				for (int i = 0; i < newroms.Count; i++)
				{
					Rom rom = newroms[i];

					// In the case that the RomData is incomplete, skip it
					if (rom.Name == null || rom.Machine.Name == null)
					{
						continue;
					}

					// WOD origninally stripped out any subdirs from the imported files, we do the same
					rom.Name = Path.GetFileName(rom.Name);

					// Run the name through the filters to make sure that it's correct
					rom.Name = Style.NormalizeChars(rom.Name);
					rom.Name = Style.RussianToLatin(rom.Name);
					rom.Name = Regex.Replace(rom.Name, @"(.*) \.(.*)", "$1.$2");

					// Run the name through the filters to make sure that it's correct
					rom.Machine.Name = Style.NormalizeChars(rom.Machine.Name);
					rom.Machine.Name = Style.RussianToLatin(rom.Machine.Name);
					rom.Machine.Name = Style.SearchPattern(rom.Machine.Name);

					// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same
					string stripPattern = @"(([[(].*[\)\]] )?([^([]+))";
					Regex stripRegex = new Regex(stripPattern);
					Match stripMatch = stripRegex.Match(rom.Machine.Name);
					rom.Machine.Name = stripMatch.Groups[1].Value;

					rom.Machine.Name = rom.Machine.Name.TrimEnd().TrimStart();

					if (!_norename)
					{
						rom.Machine.Name += " [" + sources[rom.Metadata.SourceID] + "]";
					}

					// If a game has source "0" it's Default. Make this Int32.MaxValue for sorting purposes
					if (rom.Metadata.SourceID == 0)
					{
						rom.Metadata.SourceID = Int32.MaxValue;
					}

					temp.Add(rom);
				}
				datdata.Roms[key] = temp;
			}

			// Then write out the file
			Output.WriteDatfile(datdata, _outroot, _logger, _norename);

			return true;
		}
	}
}
