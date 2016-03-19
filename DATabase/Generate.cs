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
		private int _system;
		private int _source;
		private string _connectionString;
		private bool _old;

		// Private required variables
		private Dictionary<int, string> _headers;

		// Public instance variables
		public int System
		{
			get { return _system; }
		}
		public int Source
		{
			get { return _source; }
		}

		public Generate(int system, int source, string connectionString, bool old = false)
		{
			_system = system;
			_source = source;
			_connectionString = connectionString;
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
			if (_system != -1)
			{
				string query = "SELECT manufacturer, system FROM systems WHERE id=" + _system;
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
								Console.WriteLine("No system could be found with id " + _system + ". Please check and try again.");
								return false;
							}

							// Retieve and build the system name
							sldr.Read();
							systemname = sldr.GetString(0) + " - " + sldr.GetString(1);
						}
					}
				}
			}

			string sourcename = "Merged";
			if (_source != -1)
			{
				string query = "SELECT name FROM sources WHERE id=" + _source;
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
								Console.WriteLine("No source could be found with id " + _source + ". Please check and try again.");
								return false;
							}

							// Retieve and build the system name
							sldr.Read();
							sourcename = sldr.GetString(0);
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
				if (_system == -1 && _source == -1)
				{
					_system = 0;
				}

				string header_old = "clrmamepro (\n" +
					"\tname \"" + HttpUtility.HtmlEncode(datname) + "\"\n" +
					"\tdescription \"" + HttpUtility.HtmlEncode(datname) + "\"\n" +
					"\tversion \"" + version + "\"\n" +
					(_system >= 0 && _headers.ContainsKey(_system) ? " header \"" + _headers[_system] + "\"\n" : "") +
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
					"\t\t\t<clrmamepro" + (_system >= 0 && _headers.ContainsKey(_system) ? " header=\"" + _headers[_system] + "\"" : "") + "/>\n" +
					"\t\t</header>\n";

				// Unset _system again if we're in MEGAMERGED mode
				if (_system == 0 && _source == -1)
				{
					_system = -1;
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
							(rom.CRC != "" ? " crc " + rom.CRC : "") +
							(rom.MD5 != "" ? " md5 " + rom.MD5 : "") +
							(rom.SHA1 != "" ? " sha1 " + rom.SHA1 : "") +
							" )\n";
					}
					else
					{
						state += "\t\t<" + rom.Type + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.Size != -1 ? " size=\"" + rom.Size + "\"" : "") +
							(rom.CRC != "" ? " crc=\"" + rom.CRC + "\"" : "") +
							(rom.MD5 != "" ? " md5=\"" + rom.MD5 + "\"" : "") +
							(rom.SHA1 != "" ? " sha1=\"" + rom.SHA1 + "\"" : "") +
							" />\n";
					}

					lastgame = HttpUtility.HtmlEncode(rom.Game);

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
			bool sysmerged = (_system == -1);
			bool srcmerged = (_source == -1);
			bool merged = sysmerged || srcmerged;

			string query = @"
SELECT systems.manufacturer AS manufacturer, systems.system AS system, systems.id AS systemid,
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
	(!srcmerged ? " sources.id=" + _source : "") +
	(!srcmerged && !sysmerged ? " AND" : "") +
	(!sysmerged ? " systems.id=" + _system : "") +
(merged ? "\nGROUP BY checksums.size, checksums.crc, checksums.md5, checksums.sha1" : "") +
"\nORDER BY systems.id, sources.id, games.name, files.name";
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

						// Retrieve and process the roms
						string lastname = "", lastgame = "";
						while (sldr.Read())
						{
							RomData temp = new RomData();
							temp.Manufacturer = sldr.GetString(0);
							temp.System = sldr.GetString(1);
							temp.SystemID = sldr.GetInt32(2);
							temp.Source = sldr.GetString(3);
							temp.URL = sldr.GetString(4);
							temp.SourceID = sldr.GetInt32(5);
							temp.Game = sldr.GetString(6);
							temp.Name = sldr.GetString(7);
							temp.Type = sldr.GetString(8);
							temp.Size = sldr.GetInt32(9);
							temp.CRC = sldr.GetString(10);
							temp.MD5 = sldr.GetString(11);
							temp.SHA1 = sldr.GetString(12);

							// If we're in merged mode, rename the game associated
							if (merged)
							{
								temp.Game = temp.Game +
									(sysmerged ? " [" + temp.Manufacturer + " - " + temp.System + "]" : "") +
									(srcmerged ? " [" + temp.Source + "]" : "");
							}

							// Now relable any roms that have the same name inside of the same game
							bool samename = false, samegame = false;
							if (temp.Name != "")
							{
								samename = (lastname == temp.Name);
							}
							if (temp.Game != "")
							{
								samegame = (lastgame == temp.Game);
							}
		
							lastname = temp.Name;
							lastgame = temp.Game;

							// If the name and set are the same, rename it with whatever is different
							if (samename && samegame)
							{
								temp.Name = Regex.Replace(temp.Name, @"^(.*)(\..*)", "$1(" +
										(temp.CRC != "" ? temp.CRC :
												(temp.MD5 != "" ? temp.MD5 :
														(temp.SHA1 != "" ? temp.SHA1 : "Alt"))) +
										")$2");
							}

							roms.Add(temp);
						}
					}
				}
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
