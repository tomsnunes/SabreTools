using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Generate a DAT from the data in the database
	/// </summary>
	class Generate
	{
		// Private instance variables
		private string _systems;
		private string _sources;
		private string _outdir;
		private string _connectionString;
		private bool _norename;
		private bool _old;

		// Private required variables
		private Dictionary<int, string> _headers;
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
		/// <param name="old">True if the output file should be in RomVault format (default false)</param>
		public Generate(string systems, string sources, string outdir, string connectionString, Logger logger, bool norename = false, bool old = false)
		{
			_systems = systems;
			_sources = sources;
			_connectionString = connectionString;
			_norename = norename;
			_old = old;
			_logger = logger;

			// Take care of special outfolder cases
			_outdir = (outdir == "" ? outdir :
				(outdir.Contains("/") && !outdir.EndsWith("/") ? outdir + "/" :
					(outdir.Contains("\\") && !outdir.EndsWith("\\") ? outdir + "\\" :
						(!outdir.Contains("/") && !outdir.Contains("\\") ? outdir + "\\" : outdir)
					)
				)
			);
			if (_outdir != "" && !Directory.Exists(_outdir))
			{
				Directory.CreateDirectory(_outdir);
			}

			_headers = new Dictionary<int, string>();
			_headers.Add(25, "a7800.xml");
			_headers.Add(228, "fds.xml");
			_headers.Add(31, "lynx.xml");
			_headers.Add(0, "mega.xml");    // Merged version of all other headers
			_headers.Add(234, "n64.xml");
			_headers.Add(238, "nes.xml");
			_headers.Add(241, "snes.xml");  // Self-created to deal with various headers
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
				using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
				{
					dbc.Open();
					using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
					{
						using (SQLiteDataReader sldr = slc.ExecuteReader())
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

				using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
				{
					dbc.Open();
					using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
					{
						using (SQLiteDataReader sldr = slc.ExecuteReader())
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
			List<RomData> roms = ProcessRoms();

			// If the output is null, nothing was found so return false
			if (roms == null)
			{
				return false;
			}

			// Create a name for the file based on the retrieved information
			string version = DateTime.Now.ToString("yyyyMMddHHmmss");
			string intname = systemname + " (" + sourcename + ")";
			string datname = systemname + " (" + sourcename + " " + version + ")";

			return Output.WriteToDat(intname, datname, version, version, "The Wizard of DATz", "The Wizard of DATz", false, _old, _outdir, roms, _logger);
		}

		/// <summary>
		/// Preprocess the rom data that is to be included in the outputted DAT
		/// </summary>
		/// <returns>A List of RomData objects containing all information about the files</returns>
		public List<RomData> ProcessRoms()
		{
			List<RomData> roms = new List<RomData>();

			// Check if we have listed sources or systems
			bool sysmerged = (_systems == "" || _systems.Split(',').Length > 1);
			bool srcmerged = (_sources == "" || _sources.Split(',').Length > 1);
			bool merged = sysmerged || srcmerged;

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

			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
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

							if (merged)
							{
								// If it's the first rom in the list, don't touch it
								if (roms.Count != 0)
								{
									// Check if the rom is a duplicate
									RomData last = roms[roms.Count - 1];

									bool shouldcont = false;
									if (temp.Type == "rom" && last.Type == "rom")
									{
										shouldcont = ((temp.Size != -1 && temp.Size == last.Size) && (
												(temp.CRC != "" && last.CRC != "" && temp.CRC == last.CRC) ||
												(temp.MD5 != "" && last.MD5 != "" && temp.MD5 == last.MD5) ||
												(temp.SHA1 != "" && last.SHA1 != "" && temp.SHA1 == last.SHA1)
												)
											);
									}
									else if (temp.Type == "disk" && last.Type == "disk")
									{
										shouldcont = ((temp.MD5 != "" && last.MD5 != "" && temp.MD5 == last.MD5) ||
												(temp.SHA1 != "" && last.SHA1 != "" && temp.SHA1 == last.SHA1)
											);
									}

									// If it's a duplicate, skip adding it to the output but add any missing information
									if (shouldcont)
									{
										last.CRC = (last.CRC == "" && temp.CRC != "" ? temp.CRC : last.CRC);
										last.MD5 = (last.MD5 == "" && temp.MD5 != "" ? temp.MD5 : last.MD5);
										last.SHA1 = (last.SHA1 == "" && temp.SHA1 != "" ? temp.SHA1 : last.SHA1);

										roms.RemoveAt(roms.Count - 1);
										roms.Insert(roms.Count, last);

										continue;
									}
								}

								// Rename the game associated if it's still valid and we allow renames
								if (!_norename)
								{
									temp.Game = temp.Game +
										(sysmerged ? " [" + temp.Manufacturer + " - " + temp.System + "]" : "") +
										(srcmerged ? " [" + temp.Source + "]" : "");
								}
							}

							roms.Add(temp);
						}
					}
				}
			}

			// If we're in a merged mode, resort by the correct parameters
			if (merged)
			{
				roms.Sort(delegate (RomData x, RomData y)
				{
					if (x.SystemID == y.SystemID)
					{
						if (x.SourceID == y.SourceID)
						{
							if (x.Game == y.Game)
							{
								return String.Compare(x.Name, y.Name);
							}
							return String.Compare(x.Game, y.Game);
						}
						return (_norename ? String.Compare(x.Game, y.Game) : x.SourceID - y.SourceID);
					}
					return (_norename ? String.Compare(x.Game, y.Game) : x.SystemID - y.SystemID);
				});
			}

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

			return roms;
		}
	}
}
