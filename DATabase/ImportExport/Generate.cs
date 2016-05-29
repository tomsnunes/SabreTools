using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Text.RegularExpressions;

using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Generate a DAT from the data in the database
	/// </summary>
	class Generate : IGenerate
	{
		// Private instance variables
		private string _systems;
		private string _sources;
		private string _outdir;
		private string _connectionString;
		private bool _norename;
		private bool _old;

		// Private required variables
		private Logger _logger;

		/// <summary>
		/// Initialize a Generate object with the given information
		/// </summary>
		/// <param name="systems">Comma-separated list of systems to be included in the DAT (blank means all)</param>
		/// <param name="sources">Comma-separated list of sources to be included in the DAT (blank means all)</param>
		/// <param name="outdir">The output folder where the generated DAT will be put; blank means the current directory</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <param name="logger">Logger object for file or console output</param>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in ClrMamePro format (default false)</param>
		public Generate(string systems, string sources, string outdir, string connectionString, Logger logger, bool norename = false, bool old = false)
		{
			_systems = systems;
			_sources = sources;
			_connectionString = connectionString;
			_norename = norename;
			_old = old;
			_logger = logger;

			// Take care of special outfolder cases
			_outdir = (outdir == "" ? Environment.CurrentDirectory + Path.DirectorySeparatorChar :
				(!outdir.EndsWith(Path.DirectorySeparatorChar.ToString()) ? outdir + Path.DirectorySeparatorChar : outdir)
			);
			if (_outdir != "" && !Directory.Exists(_outdir))
			{
				Directory.CreateDirectory(_outdir);
			}
		}

		/// <summary>
		/// Generate a DAT file that is represented by the data in the Generate object.
		/// </summary>
		/// <returns>True if the file could be created, false otherwise</returns>
		public bool Export()
		{
			// Check to see if the source is an import-only. If so, tell the user and exit
			int id = 0;
			if (_sources != "" && Int32.TryParse(_sources, out id) && id <= 14)
			{
				_logger.Warning("This source (" + id + ") is import-only so a DAT cannot be created. We apologize for the inconvenience.");
				return false;
			}

			// Get the system name, if applicable
			string systemname = "";
			if (_systems != "")
			{
				string query = "SELECT manufacturer, system FROM systems WHERE id in (" + _systems + ")";
				//string query = "SELECT manufacturer, name FROM system WHERE id in (" + _systems + ")";

				using (SqliteConnection dbc = new SqliteConnection(_connectionString))
				{
					dbc.Open();
					using (SqliteCommand slc = new SqliteCommand(query, dbc))
					{
						using (SqliteDataReader sldr = slc.ExecuteReader())
						{
							// If there are no games for this combination, return nothing
							if (!sldr.HasRows)
							{
								_logger.Error("No system could be found with id in \"" + _systems + "\". Please check and try again.");
								return false;
							}

							// Retrieve and build the system name from all retrieved
							int tempsize = 0;
							while (sldr.Read() && tempsize < 3)
							{
								systemname += (tempsize == 0 ?
									sldr.GetString(0) + " - " + sldr.GetString(1) :
									"; " + sldr.GetString(0) + " - " + sldr.GetString(1));
								tempsize++;
							}

							// If there are more than 3 systems, just put "etc." on the end
							if (sldr.Read())
							{
								systemname += "; etc.";
							}
						}
					}
				}
			}
			else
			{
				systemname = "ALL";
			}

			string sourcename = "";
			if (_sources != "")
			{
				string query = "SELECT name FROM sources WHERE id in (" + _sources + ")";
				//string query = "SELECT name FROM source WHERE id in (" + _sources + ")";

				using (SqliteConnection dbc = new SqliteConnection(_connectionString))
				{
					dbc.Open();
					using (SqliteCommand slc = new SqliteCommand(query, dbc))
					{
						using (SqliteDataReader sldr = slc.ExecuteReader())
						{
							// If there are no games for this combination, return nothing
							if (!sldr.HasRows)
							{
								_logger.Error("No source could be found with id in \"" + _sources + "\". Please check and try again.");
								return false;
							}

							// Retrieve and build the source name from all retrieved
							int tempsize = 0;
							while (sldr.Read() && tempsize < 3)
							{
								sourcename += (tempsize == 0 ? sldr.GetString(0) : "; " + sldr.GetString(0));
								tempsize++;
							}

							// If there are more than 3 systems, just put "etc." on the end
							if (sldr.Read())
							{
								sourcename += "; etc.";
							}
						}
					}
				}
			}
			else
			{
				sourcename = "Merged";
			}

			// Retrieve the list of processed roms
			Dictionary<string, List<RomData>> dict = ProcessRoms();

			// If the output is null, nothing was found so return false
			if (dict.Count == 0)
			{
				return false;
			}

			// Create a name for the file based on the retrieved information
			string version = DateTime.Now.ToString("yyyyMMddHHmmss");
			string intname = systemname + " (" + sourcename + ")";
			string datname = systemname + " (" + sourcename + " " + version + ")";

			DatData datdata = new DatData
			{
				Name = intname,
				Description = datname,
				Version = version,
				Date = version,
				Category = "The Wizard of DATz",
				Author = "The Wizard of DATz",
				ForcePacking = ForcePacking.None,
				OutputFormat = (_old ? OutputFormat.ClrMamePro : OutputFormat.Xml),
				Roms = dict,
			};

			return Output.WriteDatfile(datdata, _outdir, _logger);
		}

		/// <summary>
		/// Preprocess the rom data that is to be included in the outputted DAT
		/// </summary>
		/// <returns>A List of RomData objects containing all information about the files</returns>
		public Dictionary<string, List<RomData>> ProcessRoms()
		{
			Dictionary<string, List<RomData>> roms = new Dictionary<string, List<RomData>>();

			// Check if we have listed sources or systems
			bool sysmerged = (_systems == "" || _systems.Split(',').Length > 1);
			bool srcmerged = (_sources == "" || _sources.Split(',').Length > 1);
			bool merged = sysmerged || srcmerged;

			// BEGIN COMMENT
			string query = @"
SELECT DISTINCT systems.manufacturer AS manufacturer, systems.system AS system, systems.id AS systemid,
	sources.name AS source, sources.url AS url, sources.id AS sourceid,
	games.name AS game, files.name AS name, files.type AS type,
	checksums.size AS size, checksums.crc AS crc, checksums.md5 AS md5, checksums.sha1 AS sha1,
	files.lastupdated AS lastupdated
FROM systems
JOIN games
	ON systems.id=games.system
JOIN sources
	ON games.source=sources.id
JOIN files
	ON games.id=files.setid
JOIN checksums
	ON files.id=checksums.file" +
	(_systems != "" || _sources != "" ? "\nWHERE" : "") +
	(_sources != "" ? " sources.id in (" + _sources + ")" : "") +
	(_systems != "" && _sources != "" ? " AND" : "") +
	(_systems != "" ? " systems.id in (" + _systems + ")" : "") +
"\nORDER BY " +
	(merged ? "checksums.size, checksums.crc, systems.id, sources.id, files.lastupdated DESC, checksums.md5, checksums.sha1"
			: "systems.id, sources.id, games.name, files.name");

			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If there are no games for this combination, return nothing
						if (!sldr.HasRows)
						{
							_logger.Error("No games could be found with those inputs. Please check and try again.");
							return null;
						}

						// Retrieve and process the roms for merging
						while (sldr.Read())
						{
							RomData temp = new RomData
							{
								Manufacturer = sldr.GetString(0),
								System = sldr.GetString(1),
								SystemID = sldr.GetInt32(2),
								Source = sldr.GetString(3),
								URL = sldr.GetString(4),
								SourceID = sldr.GetInt32(5),
								Game = sldr.GetString(6),
								Name = sldr.GetString(7),
								Type = sldr.GetString(8),
								Size = sldr.GetInt64(9),
								CRC = sldr.GetString(10),
								MD5 = sldr.GetString(11),
								SHA1 = sldr.GetString(12),
							};

							// Rename the game associated if it's still valid and we allow renames
							if (merged && !_norename)
							{
								temp.Game = temp.Game +
									(sysmerged ? " [" + temp.Manufacturer + " - " + temp.System + "]" : "") +
									(srcmerged ? " [" + temp.Source + "]" : "");
							}

							string key = temp.Size + "-" + temp.CRC;
							if (roms.ContainsKey(key))
							{
								roms[key].Add(temp);
							}
							else
							{
								List<RomData> templist = new List<RomData>();
								templist.Add(temp);
								roms.Add(key, templist);
							}
						}
					}
				}
			}

			// If we're in a merged mode, merge and then resort by the correct parameters
			if (merged)
			{
				foreach (string key in roms.Keys)
				{
					roms[key] = RomManipulation.Merge(roms[key], _logger);
				}
			}
			// END COMMENT

			/*
			// This block would replace the whole block above between BEGIN COMMENT and END COMMENT
			string query = @"
SELECT hash.id AS id, hash.size AS size, hash.crc AS crc, hash.md5 AS md5, hash.sha1 AS sha1,
	a.key AS key, a.value AS value,
	source.id, source.name, source.url,
	system.id, system.manufacturer, system.name
FROM hash
JOIN hashdata a
	ON hash.id=a.hashid
JOIN hashdata b
	ON a.hashid=b.hashid
JOIN gamesystem
	ON b.value=gamesystem.game
JOIN gamesource
	ON b.value=gamesource.game
JOIN system
	ON gamesystem.systemid=system.id
JOIN source
	ON gamesource.sourceid=source.id" + 
(_systems != "" || _sources != "" ? "\nWHERE" : "") +
	(_sources != "" ? " source.id in (" + _sources + ")" : "") +
	(_systems != "" && _sources != "" ? " AND" : "") +
	(_systems != "" ? " system.id in (" + _systems + ")" : "") +
"\nORDER BY hash.id";

			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If there are no games for this combination, return nothing
						if (!sldr.HasRows)
						{
							_logger.Error("No games could be found with those inputs. Please check and try again.");
							return null;
						}

						// Retrieve and process the roms for merging
						int systemid = -1, sourceid = -1;
						long lastid = -1, size = -1;
						string name = "", game = "", type = "", manufacturer = "", system = "", source = "", url = "", crc = "", md5 = "", sha1 = "";
						while (sldr.Read())
						{
							// If the hash is different than the last
							if (lastid != -1 && sldr.GetInt64(0) != lastid)
							{
								RomData temp = new RomData
								{
									Manufacturer = manufacturer,
									System = system,
									SystemID = systemid,
									Source = source,
									URL = url,
									SourceID = sourceid,
									Game = game,
									Name = name,
									Type = type,
									Size = size,
									CRC = crc,
									MD5 = md5,
									SHA1 = sha1,
								};

								// Rename the game associated if it's still valid and we allow renames
								if (merged && !_norename)
								{
									temp.Game = temp.Game +
										(sysmerged ? " [" + temp.Manufacturer + " - " + temp.System + "]" : "") +
										(srcmerged ? " [" + temp.Source + "]" : "");
								}

								string key = temp.Size + "-" + temp.CRC;
								if (roms.ContainsKey(key))
								{
									roms[key].Add(temp);
								}
								else
								{
									List<RomData> templist = new List<RomData>();
									templist.Add(temp);
									roms.Add(key, templist);
								}

								// Reset the variables
								game = "";
								name = "";
								type = "";
							}
							
							// Get all of the current ROM information
							manufacturer = sldr.GetString(11);
							system = sldr.GetString(12);
							systemid = sldr.GetInt32(10);
							source = sldr.GetString(8);
							url = sldr.GetString(9);
							sourceid = sldr.GetInt32(7);
							size = sldr.GetInt64(1);
							crc = sldr.GetString(2);
							md5 = sldr.GetString(3);
							sha1 = sldr.GetString(4);

							switch (sldr.GetString(5))
							{
								case "game":
									game = sldr.GetString(6);
									break;
								case "name":
									name = sldr.GetString(6);
									break;
								case "type":
									type = sldr.GetString(6);
									break;
							}

							lastid = sldr.GetInt64(0);
						}
					}
				}
			}

			// If we're in a merged mode, merge
			if (merged)
			{
				foreach (string key in roms.Keys)
				{
					roms[key] = RomManipulation.Merge(roms[key]);
				}
			}
			*/

			/*
			// THIS CODE SHOULD BE PUT IN WriteToDatFromDict

			// Now check rename within games
			string lastname = "", lastgame = "";
			for (int i = 0; i < roms.Count; i++)
			{
				RomData rom = roms[i];

				// Now relable any roms that have the same name inside of the same game
				bool samename = false, samegame = false;
				if (rom.Name != "")
				{
					samename = (lastname == rom.Name);
				}
				if (rom.Game != "")
				{
					samegame = (lastgame == rom.Game);
				}

				lastname = rom.Name;
				lastgame = rom.Game;

				// If the name and set are the same, rename it with whatever is different
				if (samename && samegame)
				{
					rom.Name = Regex.Replace(rom.Name, @"^(.*)(\..*)", "$1(" +
							(rom.CRC != "" ? rom.CRC :
									(rom.MD5 != "" ? rom.MD5 :
											(rom.SHA1 != "" ? rom.SHA1 : "Alt"))) +
							")$2");
				}

				// Assign back just in case
				roms[i] = rom;
			}
			*/

			return roms;
		}
	}
}
