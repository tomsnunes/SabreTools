using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Import data into the database from existing DATs
	/// </summary>
	public class Import
	{
		// Private instance variables
		private string _filepath;
		private string _connectionString;
		private Logger _logger;

		// Regex File Name Patterns
		private static string _defaultPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.dat$";
		private static string _defaultSpecialPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.xml$";
		private static string _goodPattern = @"^(Good.*?)_.*\.dat";
		private static string _goodXmlPattern = @"^(Good.*?)_.*\.xml";
		private static string _mamePattern = @"^(.*)\.xml$";
		private static string _maybeIntroPattern = @"(.*?) \[T-En\].*\((\d{8})\)\.dat$";
		private static string _noIntroPattern = @"^(.*?) \((\d{8}-\d{6})_CM\)\.dat$";
		private static string _noIntroNumberedPattern = @"(.*? - .*?) \(\d.*?_CM\).dat";
		private static string _noIntroSpecialPattern = @"(.*? - .*?) \((\d{8})\)\.dat";
		private static string _nonGoodPattern = @"^(NonGood.*?)( .*?)?.xml";
		private static string _nonGoodSpecialPattern = @"^(NonGood.*?)( .*)?.dat";
		private static string _redumpPattern = @"^(.*?) \((\d{8} \d{2}-\d{2}-\d{2})\)\.dat$";
		private static string _redumpBiosPattern = @"^(.*?) \(\d+\) \((\d{4}-\d{2}-\d{2})\)\.dat$";
		private static string _tosecPattern = @"^(.*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\)\.dat$";
		private static string _tosecSpecialPatternA = @"^(.*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\)\.dat$";
		private static string _tosecSpecialPatternB = @"^(.*? - .*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\)\.dat$";
		private static string _truripPattern = @"^(.*) - .* \(trurip_XML\)\.dat$";
		private static string _zandroPattern = @"^SMW-.*.xml";

		// Regex Mapped Name Patterns
		private static string _remappedPattern = @"^(.*) - (.*)$";

		// Regex Date Patterns
		private static string _defaultDatePattern = @"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})";
		private static string _noIntroDatePattern = @"(\d{4})(\d{2})(\d{2})-(\d{2})(\d{2})(\d{2})";
		private static string _noIntroSpecialDatePattern = @"(\d{4})(\d{2})(\d{2})";
		private static string _redumpDatePattern = @"(\d{4})(\d{2})(\d{2}) (\d{2})-(\d{2})-(\d{2})";
		private static string _tosecDatePattern = @"(\d{4})-(\d{2})-(\d{2})";

		/// <summary>
		/// Initialize an Import object with the given information
		/// </summary>
		/// <param name="filepath">Path to the file that is going to be imported</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <param name="logger">Logger object for file or console output</param>
		public Import(string filepath, string connectionString, Logger logger)
		{
			_filepath = filepath.Replace("\"", "");
			_connectionString = connectionString;
			_logger = logger;
		}

		/// <summary>
		/// Import the data from file into the database
		/// </summary>
		/// <returns>True if the data was imported, false otherwise</returns>
		public bool ImportData()
		{
			// If file doesn't exist, error and return
			if (!File.Exists(_filepath))
			{
				_logger.Error("File '" + _filepath + "' doesn't exist");
				return false;
			}

			// Determine which dattype we have
			string filename = Path.GetFileName(_filepath);
			GroupCollection fileinfo;
			DatType type = DatType.none;

			if (Regex.IsMatch(filename, _nonGoodPattern))
			{
				fileinfo = Regex.Match(filename, _nonGoodPattern).Groups;
				type = DatType.NonGood;
			}
			else if (Regex.IsMatch(filename, _nonGoodSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _nonGoodSpecialPattern).Groups;
				type = DatType.NonGood;
			}
			else if (Regex.IsMatch(filename, _goodPattern))
			{
				fileinfo = Regex.Match(filename, _goodPattern).Groups;
				type = DatType.Good;
			}
			else if (Regex.IsMatch(filename, _goodXmlPattern))
			{
				fileinfo = Regex.Match(filename, _goodXmlPattern).Groups;
				type = DatType.Good;
			}
			else if (Regex.IsMatch(filename, _maybeIntroPattern))
			{
				fileinfo = Regex.Match(filename, _maybeIntroPattern).Groups;
				type = DatType.MaybeIntro;
			}
			else if (Regex.IsMatch(filename, _noIntroPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroPattern).Groups;
				type = DatType.NoIntro;
			}
			// For numbered DATs only
			else if (Regex.IsMatch(filename, _noIntroNumberedPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroNumberedPattern).Groups;
				type = DatType.NoIntro;
			}
			// For N-Gage and Gizmondo only
			else if (Regex.IsMatch(filename, _noIntroSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroSpecialPattern).Groups;
				type = DatType.NoIntro;
			}
			else if (Regex.IsMatch(filename, _redumpPattern))
			{
				fileinfo = Regex.Match(filename, _redumpPattern).Groups;
				type = DatType.Redump;
			}
			// For special BIOSes only
			else if (Regex.IsMatch(filename, _redumpBiosPattern))
			{
				fileinfo = Regex.Match(filename, _redumpBiosPattern).Groups;
				type = DatType.Redump;
			}
			else if (Regex.IsMatch(filename, _tosecPattern))
			{
				fileinfo = Regex.Match(filename, _tosecPattern).Groups;
				type = DatType.TOSEC;
			}
			else if (Regex.IsMatch(filename, _truripPattern))
			{
				fileinfo = Regex.Match(filename, _truripPattern).Groups;
				type = DatType.TruRip;
			}
			else if (Regex.IsMatch(filename, _zandroPattern))
			{
				filename = "Nintendo - Super Nintendo Entertainment System (Zandro " + File.GetLastWriteTime(_filepath).ToString("yyyyMMddHHmmss") + ").dat";
				fileinfo = Regex.Match(filename, _defaultPattern).Groups;
				type = DatType.Custom;
			}
			else if (Regex.IsMatch(filename, _defaultPattern))
			{
				fileinfo = Regex.Match(filename, _defaultPattern).Groups;
				type = DatType.Custom;
			}
			else if (Regex.IsMatch(filename, _defaultSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _defaultSpecialPattern).Groups;
				type = DatType.Custom;
			}
			else if (Regex.IsMatch(filename, _mamePattern))
			{
				fileinfo = Regex.Match(filename, _mamePattern).Groups;
				type = DatType.MAME;
			}
			// If the type is still unmatched, the data can't be imported yet
			else
			{
				_logger.Warning("File " + filename + " cannot be imported at this time because it is not a known pattern.\nPlease try again with an unrenamed version.");
				return false;
			}

			_logger.Log("Type detected: " + type.ToString());

			// Check for and extract import information from the file name based on type
			string manufacturer = "";
			string system = "";
			string source = "";
			string datestring = "";
			string date = "";

			switch (type)
			{
				case DatType.Good:
					if (!Remapping.Good.ContainsKey(fileinfo[1].Value))
					{
						_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection goodInfo = Regex.Match(Remapping.Good[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = goodInfo[1].Value;
					system = goodInfo[2].Value;
					source = "Good";
					date = File.GetLastWriteTime(_filepath).ToString("yyyy-MM-dd HH:mm:ss");
					break;
				case DatType.MAME:
					if (!Remapping.MAME.ContainsKey(fileinfo[1].Value))
					{
						_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection mameInfo = Regex.Match(Remapping.MAME[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = mameInfo[1].Value;
					system = mameInfo[2].Value;
					source = "MAME";
					date = File.GetLastWriteTime(_filepath).ToString("yyyy-MM-dd HH:mm:ss");
					break;
				case DatType.MaybeIntro:
					if (!Remapping.MaybeIntro.ContainsKey(fileinfo[1].Value))
					{
						_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection maybeIntroInfo = Regex.Match(Remapping.MaybeIntro[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = maybeIntroInfo[1].Value;
					system = maybeIntroInfo[2].Value;
					source = "Maybe-Intro";
					datestring = fileinfo[2].Value;
					GroupCollection miDateInfo = Regex.Match(datestring, _noIntroSpecialDatePattern).Groups;
					date = miDateInfo[1].Value + "-" + miDateInfo[2].Value + "-" + miDateInfo[3].Value + " 00:00:00";
					break;
				case DatType.NoIntro:
					if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
					{
						_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection nointroInfo = Regex.Match(Remapping.NoIntro[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = nointroInfo[1].Value;
					system = nointroInfo[2].Value;
					source = "no-Intro";
					if (fileinfo.Count < 2)
					{
						date = File.GetLastWriteTime(_filepath).ToString("yyyy-MM-dd HH:mm:ss");
					}
					else if (Regex.IsMatch(fileinfo[2].Value, _noIntroDatePattern))
					{
						datestring = fileinfo[2].Value;
						GroupCollection niDateInfo = Regex.Match(datestring, _noIntroDatePattern).Groups;
						date = niDateInfo[1].Value + "-" + niDateInfo[2].Value + "-" + niDateInfo[3].Value + " " +
							niDateInfo[4].Value + ":" + niDateInfo[5].Value + ":" + niDateInfo[6].Value;
					}
					else
					{
						datestring = fileinfo[2].Value;
						GroupCollection niDateInfo = Regex.Match(datestring, _noIntroSpecialDatePattern).Groups;
						date = niDateInfo[1].Value + "-" + niDateInfo[2].Value + "-" + niDateInfo[3].Value + " 00:00:00";
					}
					break;
				case DatType.NonGood:
					if (!Remapping.NonGood.ContainsKey(fileinfo[1].Value))
					{
						_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection nonGoodInfo = Regex.Match(Remapping.NonGood[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = nonGoodInfo[1].Value;
					system = nonGoodInfo[2].Value;
					source = "NonGood";
					date = File.GetLastWriteTime(_filepath).ToString("yyyy-MM-dd HH:mm:ss");
					break;
				case DatType.Redump:
					if (!Remapping.Redump.ContainsKey(fileinfo[1].Value))
					{
						// Handle special case mappings found only in Redump
						fileinfo = Regex.Match(filename, _redumpBiosPattern).Groups;

						if (!Remapping.Redump.ContainsKey(fileinfo[1].Value))
						{
							_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
							return false;
						}
					}
					GroupCollection redumpInfo = Regex.Match(Remapping.Redump[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = redumpInfo[1].Value;
					system = redumpInfo[2].Value;
					source = "Redump";
					datestring = fileinfo[2].Value;
					if (Regex.IsMatch(datestring, _redumpDatePattern))
					{
						GroupCollection rdDateInfo = Regex.Match(datestring, _redumpDatePattern).Groups;
						date = rdDateInfo[1].Value + "-" + rdDateInfo[2].Value + "-" + rdDateInfo[3].Value + " " +
							rdDateInfo[4].Value + ":" + rdDateInfo[5].Value + ":" + rdDateInfo[6].Value;
					}
					else
					{
						GroupCollection rdDateInfo = Regex.Match(datestring, _tosecDatePattern).Groups;
						date = rdDateInfo[1].Value + "-" + rdDateInfo[2].Value + "-" + rdDateInfo[3].Value + " 00:00:00";
					}
					
					break;
				case DatType.TOSEC:
					if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
					{
						// Handle special case mappings found only in TOSEC
						fileinfo = Regex.Match(filename, _tosecSpecialPatternA).Groups;

						if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
						{
							fileinfo = Regex.Match(filename, _tosecSpecialPatternB).Groups;

							if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
							{
								_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
								return false;
							}
						}
					}
					GroupCollection tosecInfo = Regex.Match(Remapping.TOSEC[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = tosecInfo[1].Value;
					system = tosecInfo[2].Value;
					source = "TOSEC";
					datestring = fileinfo[2].Value;
					GroupCollection toDateInfo = Regex.Match(datestring, _tosecDatePattern).Groups;
					date = toDateInfo[1].Value + "-" + toDateInfo[2].Value + "-" + toDateInfo[3].Value + " 00:00:00";
					break;
				case DatType.TruRip:
					if (!Remapping.TruRip.ContainsKey(fileinfo[1].Value))
					{
						_logger.Warning("The filename " + fileinfo[1].Value + " could not be mapped! Please check the mappings and try again");
						return false;
					}
					GroupCollection truripInfo = Regex.Match(Remapping.TruRip[fileinfo[1].Value], _remappedPattern).Groups;

					manufacturer = truripInfo[1].Value;
					system = truripInfo[2].Value;
					source = "trurip";
					date = File.GetLastWriteTime(_filepath).ToString("yyyy-MM-dd HH:mm:ss");
					break;
				case DatType.Custom:
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
			//string query = "SELECT id FROM system WHERE manufacturer='" + manufacturer + "' AND name='" + system + "'";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							_logger.Error("No suitable system for '" + filename + "' (" + manufacturer + " " + system + ") found! Please add the system and then try again.");
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
			//query = "SELECT id FROM source WHERE name='" + source + "'";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							_logger.Error("No suitable source for '" + filename + "' found! Please add the source and then try again.");
							return false;
						}

						// Set the source ID from the first found value
						sldr.Read();
						srcid = sldr.GetInt32(0);
					}
				}
			}

			// Get all roms that are found in the DAT to see what needs to be added
			List<RomData> roms = RomManipulation.Parse(_filepath, sysid, srcid, _logger);

			// Sort and loop over all roms, checking for adds
			RomManipulation.Sort(roms, true);
			string lastgame = "";
			long gameid = -1;
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				foreach (RomData rom in roms)
				{
					// BEGIN COMMENT
					// If we have a new game, check for a new ID
					if (rom.Game != lastgame)
					{
						gameid = AddGame(sysid, rom.Game, srcid, dbc);
						lastgame = rom.Game;
					}

					// Try to add the rom with the game information
					AddRom(rom, gameid, date, dbc);
					// END COMMENT

					/*
					// Try to add the romdata
					AddHash(rom, sysid, srcid, date, dbc);
					*/
				}
			}

			return true;
		}

		/// <summary>
		/// Add a game to the database if it doesn't already exist
		/// </summary>
		/// <param name="sysid">System ID for the game to be added with</param>
		/// <param name="machinename">Name of the game to be added</param>
		/// <param name="srcid">Source ID for the game to be added with</param>
		/// <param name="dbc">SQLite database connection to use</param>
		/// <returns>Game ID of the inserted (or found) game, -1 on error</returns>
		private long AddGame(int sysid, string machinename, int srcid, SqliteConnection dbc)
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
			machinename = machinename.Trim();

			long gameid = -1;
			string query = "SELECT id FROM games WHERE system=" + sysid +
				" AND name='" + machinename.Replace("'", "''") + "'" +
				" AND source=" + srcid;

			using (SqliteCommand slc = new SqliteCommand(query, dbc))
			{
				using (SqliteDataReader sldr = slc.ExecuteReader())
				{
					// If nothing is found, add the game and get the insert ID
					if (!sldr.HasRows)
					{
						query = "INSERT INTO games (system, name, source)" +
							" VALUES (" + sysid + ", '" + machinename.Replace("'", "''") + "', " + srcid + ")";

						using (SqliteCommand slc2 = new SqliteCommand(query, dbc))
						{
							slc2.ExecuteNonQuery();
						}

						query = "SELECT last_insert_rowid()";
						using (SqliteCommand slc2 = new SqliteCommand(query, dbc))
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

			return gameid;
		}

		/// <summary>
		/// Add a file to the database if it doesn't already exist
		/// </summary>
		/// <param name="rom">RomData object representing the rom</param>
		/// <param name="gameid">ID of the parent game to be mapped to</param>
		/// <param name="date">Last updated date</param>
		/// <param name="dbc">SQLite database connection to use</param>
		/// <returns>True if the file exists or could be added, false on error</returns>
		private bool AddRom(RomData rom, long gameid, string date, SqliteConnection dbc)
		{
			// WOD origninally stripped out any subdirs from the imported files, we do the same
			rom.Name = Path.GetFileName(rom.Name);

			// Run the name through the filters to make sure that it's correct
			rom.Name = Style.NormalizeChars(rom.Name);
			rom.Name = Style.RussianToLatin(rom.Name);
			rom.Name = Regex.Replace(rom.Name, @"(.*) \.(.*)", "$1.$2");

			if (rom.Type != "rom" && rom.Type != "disk")
			{
				rom.Type = "rom";
			}

			// Check to see if this exact file is in the database already
			string query = @"
SELECT files.id FROM files
	JOIN checksums
	ON files.id=checksums.file
	WHERE files.name='" + rom.Name.Replace("'", "''") + @"'
		AND files.type='" + rom.Type + @"' 
		AND files.setid=" + gameid +
		" AND checksums.size=" + rom.Size +
		" AND checksums.crc='" + rom.CRC + "'" +
		" AND checksums.md5='" + rom.MD5 + "'" +
		" AND checksums.sha1='" + rom.SHA1 + "'";

			using (SqliteCommand slc = new SqliteCommand(query, dbc))
			{
				using (SqliteDataReader sldr = slc.ExecuteReader())
				{
					// If the file doesn't exist, add it with its checksums
					if (!sldr.HasRows)
					{
						query = @"BEGIN;
INSERT INTO files (setid, name, type, lastupdated)
VALUES (" + gameid + ", '" + rom.Name.Replace("'", "''") + "', '" + rom.Type + "', '" + date + @"');
INSERT INTO checksums (file, size, crc, md5, sha1)
VALUES ((SELECT last_insert_rowid()), " + rom.Size + ", '" + rom.CRC + "'" + ", '" + rom.MD5 + "'" + ", '" + rom.SHA1 + @"');
COMMIT;";
						using (SqliteCommand slc2 = new SqliteCommand(query, dbc))
						{
							int affected = slc2.ExecuteNonQuery();

							// If the insert was unsuccessful, something bad happened
							if (affected < 1)
							{
								_logger.Error("There was an error adding " + rom.Name + " to the database!");
								return false;
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Add a hash to the database if it doesn't exist already
		/// </summary>
		/// <param name="rom">RomData object representing the rom</param>
		/// <param name="sysid">System ID for the game to be added with</param>
		/// <param name="srcid">Source ID for the game to be added with</param>
		/// <param name="date">Last updated date</param>
		/// <param name="dbc">SQLite database connection to use</param>
		/// <returns>True if the hash exists or could be added, false on error</returns>
		/// <remarks>This is currently unused. It is a test method for the new SabreTools DB schema</remarks>
		private bool AddHash(RomData rom, int sysid, int srcid, string date, SqliteConnection dbc)
		{
			// Process the game name

			// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same
			string stripPattern = @"(([[(].*[\)\]] )?([^([]+))";
			Regex stripRegex = new Regex(stripPattern);
			Match stripMatch = stripRegex.Match(rom.Game);
			rom.Game = stripMatch.Groups[1].Value;

			// Run the name through the filters to make sure that it's correct
			rom.Game = Style.NormalizeChars(rom.Game);
			rom.Game = Style.RussianToLatin(rom.Game);
			rom.Game = Style.SearchPattern(rom.Game);
			rom.Game = rom.Game.Trim();

			// Process the rom name

			// WOD origninally stripped out any subdirs from the imported files, we do the same
			rom.Name = Path.GetFileName(rom.Name);

			// Run the name through the filters to make sure that it's correct
			rom.Name = Style.NormalizeChars(rom.Name);
			rom.Name = Style.RussianToLatin(rom.Name);
			rom.Name = Regex.Replace(rom.Name, @"(.*) \.(.*)", "$1.$2");

			// Retrieve or insert the hash
			long hashid = -1;
			string query = "SELECT id FROM hash WHERE size=" + rom.Size + " AND crc='" + rom.CRC + "' AND md5='" + rom.MD5 + "' AND sha1='" + rom.SHA1 + "'";
			using (SqliteCommand slc = new SqliteCommand(query, dbc))
			{
				using (SqliteDataReader sldr = slc.ExecuteReader())
				{
					// If nothing is found, add the hash and get the insert ID
					if (!sldr.HasRows)
					{
						query = "INSERT INTO hash (size, crc, md5, sha1)" +
							" VALUES (" + rom.Size + ", '" + rom.CRC + "', '" + rom.MD5 + "', '" + rom.SHA1 + "')";

						using (SqliteCommand slc2 = new SqliteCommand(query, dbc))
						{
							slc2.ExecuteNonQuery();
						}

						query = "SELECT last_insert_rowid()";
						using (SqliteCommand slc2 = new SqliteCommand(query, dbc))
						{
							hashid = (long)slc2.ExecuteScalar();
						}
					}
					// Otherwise, retrieve the ID
					else
					{
						sldr.Read();
						hashid = sldr.GetInt64(0);
					}
				}
			}

			// Ignore or insert the file and game
			query = @"BEGIN;
INSERT OR IGNORE INTO hashdata (hashid, key, value) VALUES " +
	"(" + hashid + ", 'name', '" + rom.Name.Replace("'", "''") + "'), " +
	"(" + hashid + ", 'game', '" + rom.Game.Replace("'", "''") + "'), " +
	"(" + hashid + ", 'type', '" + rom.Type + "'), " +
	"(" + hashid + ", 'lastupdated', '" + date + @"');
INSERT OR IGNORE INTO gamesystem (game, systemid) VALUES ('" + rom.Game.Replace("'", "''") + "', " + sysid + @");
INSERT OR IGNORE INTO gamesource (game, sourceid) VALUES ('" + rom.Game.Replace("'", "''") + "', " + srcid + @");
COMMIT;";

			using (SqliteCommand slc = new SqliteCommand(query, dbc))
			{
				int ret = slc.ExecuteNonQuery();
				if ((SQLiteErrorCode)ret == SQLiteErrorCode.Error)
				{
					_logger.Error("A SQLite error has occurred: " + ((SQLiteErrorCode)ret).ToString());
					return false;
				}
			}

			return true;
		}
	}
}
