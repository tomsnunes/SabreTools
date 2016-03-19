using System;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using System.Reflection;

namespace DATabase
{
	public class Importer
	{
		// Private instance variables
		private string _filepath;
		private string _connectionString;

		// Regex File Name Patterns
		private static string _defaultPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.dat$";
		private static string _mamePattern = @"^(.*)\.xml$";
        private static string _nointroPattern = @"^(.*?) \((\d{8}-\d{6})_CM\)\.dat$";
		private static string _redumpPattern = @"^(.*?) \((\d{8} \d{2}-\d{2}-\d{2})\)\.dat$";
		private static string _tosecPattern = @"^(.*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\)\.dat$";
		private static string _truripPattern = @"^(.*?) - .* \(trurip_XML\)\.dat$";

		// Regex Mapped Name Patterns
		private static string _remappedPattern = @"^(.*) - (.*)$";

		// Regex Date Patterns
		private static string _defaultDatePattern = @"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})";
		private static string _nointroDatePattern = @"(\d{4})(\d{2})(\d{2})-(\d{2})(\d{2})(\d{2})";
		private static string _redumpDatePattern = @"(\d{4})(\d{2})(\d{2}) (\d{2})-(\d{2})-(\d{2})";
		private static string _tosecDatePattern = @"(\d{4})-(\d{2})-(\d{2})";

		private enum DatType
		{
			none = 0,
			custom,
			mame,
			nointro,
			redump,
			tosec,
			trurip,
		}

		// Public instance variables
		public string FilePath
		{
			get { return _filepath; }
		}

		// Constructor
		public Importer(string filepath, string connectionString)
		{
			if (File.Exists(filepath))
			{
				_filepath = filepath;
			}
			else
			{
				throw new IOException("File " + filepath + " does not exist!");
			}

			_connectionString = connectionString;
		}

		// Import the data from file into the database
		public bool ImportData ()
		{
			// Determine which dattype we have
			string filename = Path.GetFileName(_filepath);
			GroupCollection fileinfo;
			DatType type = DatType.none;

			if (Regex.IsMatch(filename, _mamePattern))
			{
				fileinfo = Regex.Match(filename, _mamePattern).Groups;
				type = DatType.mame;
			}
			else if (Regex.IsMatch(filename, _nointroPattern))
			{
				fileinfo = Regex.Match(filename, _nointroPattern).Groups;
				type = DatType.nointro;
			}
			else if (Regex.IsMatch(filename, _redumpPattern))
			{
				fileinfo = Regex.Match(filename, _redumpPattern).Groups;
				type = DatType.redump;
			}
			else if (Regex.IsMatch(filename, _tosecPattern))
			{
				fileinfo = Regex.Match(filename, _tosecPattern).Groups;
				type = DatType.tosec;
			}
			else if (Regex.IsMatch(filename, _truripPattern))
			{
				fileinfo = Regex.Match(filename, _truripPattern).Groups;
				type = DatType.trurip;
			}
			else if (Regex.IsMatch(filename, _defaultPattern))
			{
				fileinfo = Regex.Match(filename, _defaultPattern).Groups;
				type = DatType.custom;
			}
			// If the type is still unmatched, the data can't be imported yet
			else
			{
				Console.WriteLine("File " + filename + " cannot be imported at this time because it is not a known pattern.\nPlease try again with an unrenamed version.");
				return false;
			}

			// Check for and extract import information from the file name based on type
			string manufacturer = "";
			string system = "";
			string source = "";
			string datestring = "";
			string date = "";

			switch (type)
			{
				case DatType.mame:
					if (!Remapping.MAME.ContainsKey(fileinfo[1].Value))
					{
						Console.WriteLine("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection mameInfo = Regex.Match(Remapping.MAME[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = mameInfo[1].Value;
					system = mameInfo[2].Value;
					source = "MAME";
					date = File.GetLastWriteTime(_filepath).ToString("yyyy-MM-dd HH:mm:ss");
					break;
				case DatType.nointro:
					if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
					{
						Console.WriteLine("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection nointroInfo = Regex.Match(Remapping.NoIntro[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = nointroInfo[1].Value;
					system = nointroInfo[2].Value;
					source = "no-Intro";
					datestring = fileinfo[2].Value;
					GroupCollection niDateInfo = Regex.Match(datestring, _nointroDatePattern).Groups;
					date = niDateInfo[1].Value + "-" + niDateInfo[2].Value + "-" + niDateInfo[3].Value + " " +
						niDateInfo[4].Value + ":" + niDateInfo[5].Value + ":" + niDateInfo[6].Value;
					break;
				case DatType.redump:
					if (!Remapping.Redump.ContainsKey(fileinfo[1].Value))
					{
						Console.WriteLine("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection redumpInfo = Regex.Match(Remapping.Redump[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = redumpInfo[1].Value;
					system = redumpInfo[2].Value;
					source = "Redump";
					datestring = fileinfo[2].Value;
					GroupCollection rdDateInfo = Regex.Match(datestring, _redumpDatePattern).Groups;
					date = rdDateInfo[1].Value + "-" + rdDateInfo[2].Value + "-" + rdDateInfo[3].Value + " " +
						rdDateInfo[4].Value + ":" + rdDateInfo[5].Value + ":" + rdDateInfo[6].Value;
					break;
				case DatType.tosec:
					if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
					{
						Console.WriteLine("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection tosecInfo = Regex.Match(Remapping.TOSEC[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = tosecInfo[1].Value;
					system = tosecInfo[2].Value;
					source = "TOSEC";
					datestring = fileinfo[2].Value;
					GroupCollection toDateInfo = Regex.Match(datestring, _tosecDatePattern).Groups;
					date = toDateInfo[1].Value + "-" + toDateInfo[2].Value + "-" + toDateInfo[3].Value + " 00:00:00";
					break;
				case DatType.trurip:
					if (!Remapping.TruRip.ContainsKey(fileinfo[1].Value))
					{
						Console.WriteLine("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection truripInfo = Regex.Match(Remapping.TruRip[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = truripInfo[1].Value;
					system = truripInfo[2].Value;
					source = "trurip";
					date = File.GetLastWriteTime(_filepath).ToString("yyyy-MM-dd HH:mm:ss");
					break;
				case DatType.custom:
				default:
					manufacturer = fileinfo[1].Value;
					system = fileinfo[2].Value;
					source = fileinfo[3].Value;
					datestring = fileinfo[4].Value;

					GroupCollection cDateInfo = Regex.Match(datestring, _defaultDatePattern).Groups;
					date = cDateInfo[1].Value + "-" + cDateInfo[2].Value + "-" + cDateInfo[3].Value + " " +
						cDateInfo[4].Value + ":" + cDateInfo[5].Value + ":" + cDateInfo[6].Value;
					break;
			}

			// Check to make sure that the manufacturer and system are valid according to the database
			int sysid = -1;
			string query = "SELECT id FROM systems WHERE manufacturer='" + manufacturer + "' AND system='" + system +"'";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							Console.WriteLine("Error: No suitable system found! Please add the system and then try again.");
							return false;
						}

						// Set the system ID from the first found value
						sldr.Read();
						sysid = sldr.GetInt32(0);
					}
				}
			}

			// Check to make sure that the source is valid according to the database
			int srcid = -1;
			query = "SELECT id FROM sources WHERE name='" + source + "'";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							Console.WriteLine("Error: No suitable source found! Please add the source and then try again.");
							return false;
						}

						// Set the source ID from the first found value
						sldr.Read();
						srcid = sldr.GetInt32(0);
					}
				}
			}

			// Attempt to open the given file
			try
			{
				FileStream fs = File.OpenRead(_filepath);
				StreamReader sr = new StreamReader(fs);

				Console.WriteLine("got here");

				// Set necessary dat values
				string format = "";
				bool machinefound = false;
				string machinename = "";
				string description = "";
				long gameid = 0;
				bool comment = false;

				// Parse the file for its rom information
				while (sr.Peek() > 0)
				{
					string line = sr.ReadLine();

					// First each string has to be normalized
					line = Style.NormalizeChars(line);

					// If the input style hasn't been set, set it according to the header
					if (format == "")
					{
						if (line.IndexOf("<?xml version=\"1.0\" encoding=\"utf-8\"?>") != -1)
                        {
							format = "logiqx";
						}
						else if (line.IndexOf("clrmamepro (") != -1 || line.IndexOf("romvault (") != -1)
						{
							format = "romvault";
						}
					}
					else if (line.IndexOf("<!DOCTYPE softwarelist") != -1)
					{
						format = "softwarelist";
					}

					// If there's an XML-style comment, stop the presses and skip until it's over
					else if (line.IndexOf("-->") != -1)
					{
						comment = false;
					}
					else if (line.IndexOf("<!--") != -1)
					{
						comment = true;
					}

					// Process Logiqx XML-derived DATs
					else if(format == "logiqx" && !comment)
					{
						if (line.IndexOf("<machine") != -1 || line.IndexOf("<game") != -1)
						{
							machinefound = true;

							XElement xml = XElement.Parse(line + (line.IndexOf("<machine") != -1 ? "</machine>" : "</game>"));
							machinename = xml.Attribute("name").Value;

							gameid = AddGame(sysid, machinename, srcid);
						}
						else if (line.IndexOf("<rom") != -1 && machinefound)
						{
							AddRom(line, machinename, "rom", gameid, date);
						}
						else if (line.IndexOf("<disk") != -1 && machinefound)
						{
							AddRom(line, machinename, "disk", gameid, date);
						}
						else if (line.IndexOf("</machine>") != -1 || line.IndexOf("</game>") != -1)
						{
							machinefound = false;
							machinename = "";
							description = "";
							gameid = 0;
						}
					}

					// Process SoftwareList XML-derived DATs
					else if (format == "softwarelist" && !comment)
					{
						if (line.IndexOf("<software ") != -1)
						{
							machinefound = true;
						}
						else if (line.IndexOf("<description") != -1 && machinefound)
						{
							XElement xml = XElement.Parse(line);
							machinename = xml.Value;
							gameid = AddGame(sysid, machinename, srcid);
						}
						else if (line.IndexOf("<rom") != -1 && machinefound)
						{
							AddRom(line, machinename, "rom", gameid, date);
						}
						else if (line.IndexOf("<disk") != -1 && machinefound)
						{
							AddRom(line, machinename, "disk", gameid, date);
						}
						else if (line.IndexOf("</software>") != -1)
						{
							machinefound = false;
							machinename = "";
							description = "";
							gameid = 0;
						}
					}

					// Process original style RomVault DATs
					else if (format == "romvault")
					{
						if (line.IndexOf("game") != -1 && !machinefound)
						{
							machinefound = true;
						}
						else if (line.IndexOf("rom (") != -1 && machinefound)
						{
							AddRomOld(line, machinename, "rom", gameid, date);
						}
						else if (line.IndexOf("disk (") != -1 && machinefound)
						{
							AddRomOld(line, machinename, "disk", gameid, date);
						}
						else if (line.IndexOf("name \"") != -1 && machinefound)
						{
							string machineNamePattern = "^\\s*name \"(.*)\"";
							Regex machineNameRegex = new Regex(machineNamePattern);
							Match machineNameMatch = machineNameRegex.Match(line);
							machinename = machineNameMatch.Groups[1].Value;

							gameid = AddGame(sysid, machinename, srcid);
						}
						else if (line.IndexOf("description \"") == -1 && line.IndexOf(")") != -1)
						{
							machinefound = false;
							machinename = "";
							description = "";
							gameid = 0;
						}
					}
				}

				sr.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}

			return true;
		}

		private long AddGame(int sysid, string machinename, int srcid)
		{
			// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same
			string stripPattern = @"(([[(].*[\)\]] )?([^([]+))";
			Regex stripRegex = new Regex(stripPattern);
			Match stripMatch = stripRegex.Match(machinename);
			machinename = stripMatch.Groups[1].Value;

			// Run the name through the filters to make sure that it's correct
			machinename = Style.NormalizeChars(machinename);
			machinename = Style.RussianToLatin(machinename);
			machinename = Style.SearchPattern(machinename);
			machinename.Trim();

			long gameid = -1;
			string query = "SELECT id FROM games WHERE system=" + sysid +
				" AND name='" + machinename.Replace("'", "''") + "'" +
				" AND source=" + srcid;

			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, add the game and get the insert ID
						if (!sldr.HasRows)
						{
							query = "INSERT INTO games (system, name, source)" +
								" VALUES (" + sysid + ", '" + machinename.Replace("'", "''") + "', " + srcid + ")";

							using (SQLiteCommand slc2 = new SQLiteCommand(query, dbc))
							{
								slc2.ExecuteNonQuery();
							}

							query = "SELECT last_insert_rowid()";
							using (SQLiteCommand slc2 = new SQLiteCommand(query, dbc))
							{
								gameid = (long)slc2.ExecuteScalar();
							}
						}
						// Otherwise, retrieve the ID
						else
						{
							sldr.Read();
							gameid = sldr.GetInt64(0);
						}
					}
				}
			}

			return gameid;
		}

		private bool AddRom(string line, string machinename, string romtype, long gameid, string date)
		{
			XElement xml = XElement.Parse(line);

			string name = (xml.Attribute("name") != null ? xml.Attribute("name").Value : "");
			int size = (xml.Attribute("size") != null ? Int32.Parse(xml.Attribute("size").Value) : -1);
			string crc = (xml.Attribute("crc") != null ? xml.Attribute("crc").Value : "");
			string md5 = (xml.Attribute("md5") != null ? xml.Attribute("md5").Value : "");
			string sha1 = (xml.Attribute("sha1") != null ? xml.Attribute("sha1").Value : "");

			return AddRomHelper(machinename, romtype, gameid, name, date, size, crc, md5, sha1);
		}

		private bool AddRomOld(string line, string machinename, string romtype, long gameid, string date)
		{
			string infoPattern = "name \"(.*)\"";
			Regex infoRegex = new Regex(infoPattern);
			Match infoMatch = infoRegex.Match(line);
			string name = infoMatch.Groups[1].Value;

			string[] rominfo = line.Split(' ');
			int size = -1;
			string crc = "";
			string md5 = "";
			string sha1 = "";

			string next = "";
			foreach (string info in rominfo)
			{
				if (info == "size" || info == "crc" || info == "md5" || info == "sha1")
				{
					next = info;
				}
				else if (next != "")
				{
					switch (next)
					{
						case "size": size = Int32.Parse(info); break;
						case "crc": crc = info; break;
						case "md5": md5 = info; break;
						case "sha1": sha1 = info; break;
						default: break;
					}
					next = "";
				}
			}

			return AddRomHelper(machinename, romtype, gameid, name, date, size, crc, md5, sha1);
		}

		private bool AddRomHelper(string machinename, string romtype, long gameid, string name, string date, int size, string crc, string md5, string sha1)
		{
			// WOD origninally stripped out any subdirs from the imported files, we do the same
			name = Path.GetFileName(name);

			// Run the name through the filters to make sure that it's correct
			name = Style.NormalizeChars(name);
			name = Style.RussianToLatin(name);
			name = Regex.Replace(name, @"(.*) \.(.*)", "$1.$2");

			if (romtype != "rom" && romtype != "disk")
			{
				romtype = "rom";
			}

			// Check to see if this exact file is in the database already
			string query = @"
SELECT files.id FROM files
	JOIN checksums
	ON files.id=checksums.file
	WHERE files.name='" + name.Replace("'", "''") + @"'
		AND files.type='" + romtype + @"' 
		AND files.setid=" + gameid + " " + 
        " AND checksums.size=" + size +
        " AND checksums.crc='" + crc + "'" +
        " AND checksums.md5='" + md5 + "'" +
        " AND checksums.sha1='" + sha1 + "'";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If the file doesn't exist, add it
						if (!sldr.HasRows)
						{
							query = @"
INSERT INTO files (setid, name, type, lastupdated)
	VALUES (" + gameid + ", '" + name.Replace("'", "''") + "', '" + romtype + "', '" + date + "')";
							using (SQLiteCommand slc2 = new SQLiteCommand(query, dbc))
							{
								int affected = slc2.ExecuteNonQuery();

								// If the insert was successful, add the checksums for the file
								if (affected >= 1)
								{
									query = "SELECT last_insert_rowid()";
									long romid = -1;
									using (SQLiteCommand slc3 = new SQLiteCommand(query, dbc))
									{
										romid = (long)slc3.ExecuteScalar();
									}

									query = @"INSERT INTO checksums (file, size, crc, md5, sha1) VALUES (" +
										romid + ", " + size + ", '" + crc + "'" + ", '" + md5 + "'" + ", '" + sha1 + "')";
									using (SQLiteCommand slc3 = new SQLiteCommand(query, dbc))
									{
										affected = slc3.ExecuteNonQuery();
									}

									// If the insert of the checksums failed, that's bad
									if (affected < 1)
									{
										Console.WriteLine("There was an error adding checksums for " + name + " to the database!");
										return false;
									}
								}
								// Otherwise, something happened which is bad
								else
								{
									Console.WriteLine("There was an error adding " + name + " to the database!");
									return false;
								}
							}
						}
					}
				}
			}

			return true;
		}
	}
}
