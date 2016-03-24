using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace DATabase
{
	class Generate
	{
		// Private instance variables
		private int _systems;
		//private string _systems;
		private int _sources;
		//private string _sources;
		private string _connectionString;
		private bool _norename;
		private bool _old;

		// Private required variables
		private Dictionary<int, string> _headers;

		//public Generate(string systems, string sources, string connectionString, bool norename = false, bool old = false)
		public Generate(int systems, int sources, string connectionString, bool norename = false, bool old = false)
		{
			_systems = systems;
			_sources = sources;
			_connectionString = connectionString;
			_norename = norename;
			_old = old;

			_headers = new Dictionary<int, string>();
			_headers.Add(25, "a7800.xml");
			_headers.Add(228, "fds.xml");
			_headers.Add(31, "lynx.xml");
			_headers.Add(0, "mega.xml");    // Merged version of all other headers
			_headers.Add(234, "n64.xml");
			_headers.Add(238, "nes.xml");
			_headers.Add(241, "snes.xml");  // Self-created to deal with various headers
		}

		public bool Export()
		{
			// Get the system name, if applicable
			string systemname = "ALL";
			//if (_systems != "")
			if (_systems != -1)
			{
				//string query = "SELECT manufacturer, system FROM systems WHERE id in (" + _systems + ")";
				string query = "SELECT manufacturer, system FROM systems WHERE id=" + _systems;
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
								//Console.WriteLine("No system could be found with id in \"" + _systems + "\". Please check and try again.");
								Console.WriteLine("No system could be found with id " + _systems + ". Please check and try again.");
								return false;
							}

							// Retieve and build the system name
							sldr.Read();
							systemname = sldr.GetString(0) + " - " + sldr.GetString(1);
							/*
							// Retrieve and build the system name from all retrieved
							int tempsize = 0;
							while (sldr.Read() && tempsize < 3)
							{
								systemname += (tempsize == 0 ?
									sldr.GetString(0) + " - " + sldr.GetString(1) :
									", " + sldr.GetString(0) + " - " + sldr.GetString(1));
								tempsize++;
							}

							// If there are more than 3 systems, just put "etc." on the end
							if (sldr.Read())
							{
								systemname += ", etc.";
							}
							*/
						}
					}
				}
			}

			string sourcename = "Merged";
			// if (_sources != "")
			if (_sources != -1)
			{
				//string query = "SELECT name FROM sources WHERE id in (" + _sources + ")";
				string query = "SELECT name FROM sources WHERE id=" + _sources;
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
								//Console.WriteLine("No source could be found with id in \"" + _sources + "\". Please check and try again.");
								Console.WriteLine("No source could be found with id " + _sources + ". Please check and try again.");
								return false;
							}

							// Retieve and build the system name
							sldr.Read();
							sourcename = sldr.GetString(0);
							/*
							// Retrieve and build the source name from all retrieved
							int tempsize = 0;
							while (sldr.Read() && tempsize < 3)
							{
								sourcename += (tempsize == 0 ? sldr.GetString(0) : ", " + sldr.GetString(0));
								tempsize++;
							}

							// If there are more than 3 systems, just put "etc." on the end
							if (sldr.Read())
							{
								sourcename += ", etc.";
							}
							*/
						}
					}
				}
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
			string datname = systemname + " (" + sourcename + " " + version + ")";

			// Create and open an output file for writing (currently uses current time, change to "last updated time"
			Console.WriteLine("Opening file for writing: " + datname + (_old ? ".dat" : ".xml"));
			
			try
			{
				FileStream fs = File.Create(datname + (_old ? ".dat" : ".xml"));
				StreamWriter sw = new StreamWriter(fs);

				// Temporarilly set _system if we're in MEGAMERGED mode to get the right header skip XML
				if (_systems == -1 && _sources == -1)
				{
					_systems = 0;
				}

				string header_old = "clrmamepro (\n" +
					"\tname \"" + HttpUtility.HtmlEncode(datname) + "\"\n" +
					"\tdescription \"" + HttpUtility.HtmlEncode(datname) + "\"\n" +
					"\tversion \"" + version + "\"\n" +
					(_systems >= 0 && _headers.ContainsKey(_systems) ? " header \"" + _headers[_systems] + "\"\n" : "") +
					"\tcomment \"\"\n" +
					"\tauthor \"The Wizard of DATz\"\n" +
					")\n";

				string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
					"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
					"\t<datafile>\n" +
					"\t\t<header>\n" +
					"\t\t\t<name>" + HttpUtility.HtmlEncode(datname) + "</name>\n" +
					"\t\t\t<description>" + HttpUtility.HtmlEncode(datname) + "</description>\n" +
					"\t\t\t<category>The Wizard of DATz</category>\n" +
					"\t\t\t<version>" + version + "</version>\n" +
					"\t\t\t<date>" + version + "</date>\n" +
					"\t\t\t<author>The Wizard of DATz</author>\n" +
					"\t\t\t<clrmamepro" + (_systems >= 0 && _headers.ContainsKey(_systems) ? " header=\"" + _headers[_systems] + "\"" : "") + "/>\n" +
					"\t\t</header>\n";

				// Unset _system again if we're in MEGAMERGED mode
				if (_systems == 0 && _sources == -1)
				{
					_systems = -1;
				}

				// Write the header out
				sw.Write((_old ? header_old : header));

				// Write out each of the machines and roms
				string lastgame = "";
				foreach (RomData rom in roms)
				{
					string state = "";
					if (lastgame != "" && lastgame != rom.Game)
					{
						state += (_old ? "}\n" : "\t</machine>\n");
					}
					
					if (lastgame != rom.Game)
					{
						state += (_old ? "game (\n\tname \"" + rom.Game + "\"\n" :
							"\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Game) + "\">\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(rom.Game) + "</description>\n");
					}

					if (_old)
					{
						state += "\t" + rom.Type + " ( name \"" + rom.Name + "\"" +
							(rom.Size != 0 ? " size " + rom.Size : "") +
							(rom.CRC != "" ? " crc " + rom.CRC.ToLowerInvariant() : "") +
							(rom.MD5 != "" ? " md5 " + rom.MD5.ToLowerInvariant() : "") +
							(rom.SHA1 != "" ? " sha1 " + rom.SHA1.ToLowerInvariant() : "") +
							" )\n";
					}
					else
					{
						state += "\t\t<" + rom.Type + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.Size != -1 ? " size=\"" + rom.Size + "\"" : "") +
							(rom.CRC != "" ? " crc=\"" + rom.CRC.ToLowerInvariant() + "\"" : "") +
							(rom.MD5 != "" ? " md5=\"" + rom.MD5.ToLowerInvariant() + "\"" : "") +
							(rom.SHA1 != "" ? " sha1=\"" + rom.SHA1.ToLowerInvariant() + "\"" : "") +
							" />\n";
					}

					lastgame = rom.Game;

					sw.Write(state);
				}

				sw.Write((_old ? ")" : "\t</machine>\n</datafile>"));
				Console.WriteLine("File written!");
                sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}

			return true;
		}

	    public List<RomData> ProcessRoms()
		{
			List<RomData> roms = new List<RomData>();

			// Check if we're in a merged mode
			bool sysmerged = (_systems == -1);
			bool srcmerged = (_sources == -1);
			bool merged = sysmerged || srcmerged;

			string query = @"
SELECT DISTINCT systems.manufacturer AS manufacturer, systems.system AS system, systems.id AS systemid,
	sources.name AS source, sources.url AS url, sources.id AS sourceid,
	games.name AS game, files.name AS name, files.type AS type, checksums.size AS size, checksums.crc AS crc,
	checksums.md5 AS md5, checksums.sha1 AS sha1
FROM systems
JOIN games
	ON systems.id=games.system
JOIN sources
	ON games.source=sources.id
JOIN files
	ON games.id=files.setid
JOIN checksums
	ON files.id=checksums.file" +
	(!sysmerged || !srcmerged ? "\nWHERE" : "") +
	//(!srcmerged ? " sources.id in (" + _sources + ")" : "") +
	(!srcmerged ? " sources.id=" + _sources : "") +
	(!srcmerged && !sysmerged ? " AND" : "") +
	//(!sysmerged ? " systems.id in (" + _systems + ")" : "") + "\n" +
	(!sysmerged ? " systems.id=" + _systems : "") + "\n" +
"\nORDER BY " +
	(merged ? "checksums.size, checksums.crc, checksums.md5, checksums.sha1"
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
							Console.WriteLine("No games could be found with those inputs. Please check and try again.");
							return null;
						}

						// Retrieve and process the roms for merging
						string lasttype = "", lastcrc = "", lastmd5 = "", lastsha1 = "";
						int lastsize = -1;
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
								Size = sldr.GetInt32(9),
								CRC = sldr.GetString(10),
								MD5 = sldr.GetString(11),
								SHA1 = sldr.GetString(12),
							};

							if (merged)
							{
								// Check if the rom is a duplicate
								bool shouldcont = false;
								if (temp.Type == "rom" && lasttype == "rom")
								{
									shouldcont = ((temp.Size != -1 && temp.Size == lastsize) && (
											(temp.CRC != "" && lastcrc != "" && temp.CRC == lastcrc) ||
											(temp.MD5 != "" && lastmd5 != "" && temp.MD5 == lastmd5) ||
											(temp.SHA1 != "" && lastsha1 != "" && temp.SHA1 == lastsha1)
											)
										);
								}
								else if (temp.Type == "disk" && lasttype == "disk")
								{
									shouldcont = ((temp.MD5 != "" && lastmd5 != "" && temp.MD5 == lastmd5) ||
											(temp.SHA1 != "" && lastsha1 != "" && temp.SHA1 == lastsha1)
										);
								}

								// Set the next variables
								lasttype = temp.Type;
								lastsize = temp.Size;
								lastcrc = temp.CRC;
								lastmd5 = temp.MD5;
								lastsha1 = temp.SHA1;

								// If it's a duplicate, skip adding it to the output
								if (shouldcont)
								{
									continue;
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

	// Intermediate struct for holding and processing rom data
	public struct RomData
	{
		public string Manufacturer;
		public string System;
		public int SystemID;
		public string Source;
		public string URL;
		public int SourceID;
		public string Game;
		public string Name;
		public string Type;
		public int Size;
		public string CRC;
		public string MD5;
		public string SHA1;
	}
}
