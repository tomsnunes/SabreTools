using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Mono.Data.Sqlite;

using SabreTools.Helper.Data;
using SabreTools.Helper.Dats;
using SabreTools.Helper.Tools;

using Alphaleonis.Win32.Filesystem;

using FileAccess = System.IO.FileAccess;
using FileMode = System.IO.FileMode;
using SearchOption = System.IO.SearchOption;
using StreamWriter = System.IO.StreamWriter;

namespace SabreTools
{
	public partial class RombaSharp
	{
		#region Helper methods

		/// <summary>
		/// Display the statistics in the database
		/// </summary>
		private static void DisplayDBStats()
		{
			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Total number of CRCs
			string query = "SELECT COUNT(*) FROM crc";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			_logger.User("Total CRCs: " + (long)slc.ExecuteScalar());

			// Total number of MD5s
			query = "SELECT COUNT(*) FROM md5";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total MD5s: " + (long)slc.ExecuteScalar());

			// Total number of SHA1s
			query = "SELECT COUNT(*) FROM sha1";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total SHA1s: " + (long)slc.ExecuteScalar());

			// Total number of DATs
			query = "SELECT COUNT(*) FROM dat";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total DATs: " + (long)slc.ExecuteScalar());

			slc.Dispose();
			dbc.Dispose();
		}

		/// <summary>
		/// Display the current memory usage of the application
		/// </summary>
		private static void DisplayMemoryStats()
		{
			Process proc = Process.GetCurrentProcess();

			_logger.User("Current Nonpaged Memory: " + Style.GetBytesReadable(proc.NonpagedSystemMemorySize64));
			_logger.User("Current Paged Memory: " + Style.GetBytesReadable(proc.PagedMemorySize64));
			_logger.User("Peak Paged Memory: " + Style.GetBytesReadable(proc.PeakPagedMemorySize64));
			_logger.User("Peak Virtual Memory: " + Style.GetBytesReadable(proc.PeakVirtualMemorySize64));
			_logger.User("Peak Working Memory: " + Style.GetBytesReadable(proc.PeakWorkingSet64));
			_logger.User("Private Memory: " + Style.GetBytesReadable(proc.PrivateMemorySize64));
			_logger.User("Virtual Memory: " + Style.GetBytesReadable(proc.VirtualMemorySize64));
			_logger.User("Working Memory: " + Style.GetBytesReadable(proc.WorkingSet64));
			_logger.User("Total Processor Time: " + proc.TotalProcessorTime);
			_logger.User("User Processor Time: " + proc.UserProcessorTime);
		}

		/// <summary>
		/// Export the current database to CSV
		/// </summary>
		/// <remarks>REDO</remarks>
		private static void ExportDatabase()
		{
			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();
			StreamWriter sw = new StreamWriter(File.Open("export.csv", FileMode.Create, FileAccess.Write));

			sw.WriteLine("\"ID\",\"Size\",\"CRC\",\"MD5\",\"SHA-1\",\"In Depot\",\"DAT Hash\"");

			string query = "SELECT dats.id, size, crc, md5, sha1, indepot, hash FROM data JOIN dats ON data.id=dats.id";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			SqliteDataReader sldr = slc.ExecuteReader();

			if (sldr.HasRows)
			{
				while (sldr.Read())
				{
					string line = "\"" + sldr.GetInt32(0) + "\","
							+ "\"" + sldr.GetInt64(1) + "\","
							+ "\"" + sldr.GetString(2) + "\","
							+ "\"" + sldr.GetString(3) + "\","
							+ "\"" + sldr.GetString(4) + "\","
							+ "\"" + sldr.GetInt32(5) + "\","
							+ "\"" + sldr.GetString(6) + "\"";
					sw.WriteLine(line);
				}
			}

			sldr.Dispose();
			slc.Dispose();
			sw.Dispose();
			dbc.Dispose();
		}

		/// <summary>
		/// Gets all valid DATs that match in the DAT root
		/// </summary>
		/// <param name="inputs">List of input strings to check for, presumably file names</param>
		/// <returns>Dictionary of hash/full path for each of the valid DATs</returns>
		private static Dictionary<string, string> GetValidDats(List<string> inputs)
		{
			// Get a dictionary of filenames that actually exist in the DATRoot, logging which ones are not
			List<string> datRootDats = Directory.EnumerateFiles(_dats, "*", SearchOption.AllDirectories).ToList();
			List<string> lowerCaseDats = datRootDats.ConvertAll(i => Path.GetFileName(i).ToLowerInvariant());
			Dictionary<string, string> foundDats = new Dictionary<string, string>();
			foreach (string input in inputs)
			{
				if (lowerCaseDats.Contains(input.ToLowerInvariant()))
				{
					string fullpath = Path.GetFullPath(datRootDats[lowerCaseDats.IndexOf(input.ToLowerInvariant())]);
					string sha1 = FileTools.GetFileInfo(fullpath, _logger).SHA1;
					foundDats.Add(sha1, fullpath);
				}
				else
				{
					_logger.Warning("The file '" + input + "' could not be found in the DAT root");
				}
			}

			return foundDats;
		}

		/// <summary>
		/// Initialize the Romba application from XML config
		/// </summary>
		private static void InitializeConfiguration()
		{
			// Get default values if they're not written
			int workers = 4,
				verbosity = 1,
				cores = 4,
				port = 4003;
			string logdir = "logs",
				tmpdir = "tmp",
				webdir = "web",
				baddir = "bad",
				dats = "dats",
				db = "db",
				connectionString = "";
			Dictionary<string, Tuple<long, bool>> depots = new Dictionary<string, Tuple<long, bool>>();

			// Get the XML text reader for the configuration file, if possible
			XmlReader xtr = FileTools.GetXmlTextReader(_config, _logger);

			// Now parse the XML file for settings
			if (xtr != null)
			{
				xtr.MoveToContent();
				while (!xtr.EOF)
				{
					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						case "workers":
							workers = xtr.ReadElementContentAsInt();
							break;
						case "logdir":
							logdir = xtr.ReadElementContentAsString();
							break;
						case "tmpdir":
							tmpdir = xtr.ReadElementContentAsString();
							break;
						case "webdir":
							webdir = xtr.ReadElementContentAsString();
							break;
						case "baddir":
							baddir = xtr.ReadElementContentAsString();
							break;
						case "verbosity":
							verbosity = xtr.ReadElementContentAsInt();
							break;
						case "cores":
							cores = xtr.ReadElementContentAsInt();
							break;
						case "dats":
							dats = xtr.ReadElementContentAsString();
							break;
						case "db":
							db = xtr.ReadElementContentAsString();
							break;
						case "depot":
							XmlReader subreader = xtr.ReadSubtree();
							if (subreader != null)
							{
								string root = "";
								long maxsize = -1;
								bool online = true;

								while (!subreader.EOF)
								{
									// We only want elements
									if (subreader.NodeType != XmlNodeType.Element)
									{
										subreader.Read();
										continue;
									}

									switch (subreader.Name)
									{
										case "root":
											root = subreader.ReadElementContentAsString();
											break;
										case "maxsize":
											maxsize = subreader.ReadElementContentAsLong();
											break;
										case "online":
											online = subreader.ReadElementContentAsBoolean();
											break;
										default:
											subreader.Read();
											break;
									}
								}

								try
								{
									depots.Add(root, new Tuple<long, bool>(maxsize, online));
								}
								catch
								{
									// Ignore add errors
								}
							}

							xtr.Skip();
							break;
						case "port":
							port = xtr.ReadElementContentAsInt();
							break;
						default:
							xtr.Read();
							break;
					}
				}
			}

			// Now validate the values given
			if (workers < 1)
			{
				workers = 1;
			}
			if (workers > 8)
			{
				workers = 8;
			}
			if (!Directory.Exists(logdir))
			{
				Directory.CreateDirectory(logdir);
			}
			if (!Directory.Exists(tmpdir))
			{
				Directory.CreateDirectory(tmpdir);
			}
			if (!Directory.Exists(webdir))
			{
				Directory.CreateDirectory(webdir);
			}
			if (!Directory.Exists(baddir))
			{
				Directory.CreateDirectory(baddir);
			}
			if (verbosity < 0)
			{
				verbosity = 0;
			}
			if (verbosity > 3)
			{
				verbosity = 3;
			}
			if (cores < 1)
			{
				cores = 1;
			}
			if (cores > 16)
			{
				cores = 16;
			}
			if (!Directory.Exists(dats))
			{
				Directory.CreateDirectory(dats);
			}
			db = Path.GetFileNameWithoutExtension(db) + ".sqlite";
			connectionString = "Data Source=" + db + ";Version = 3;";
			foreach (string key in depots.Keys)
			{
				if (!Directory.Exists(key))
				{
					Directory.CreateDirectory(key);
					File.CreateText(Path.Combine(key, ".romba_size"));
					File.CreateText(Path.Combine(key, ".romba_size.backup"));
				}
				else
				{
					if (!File.Exists(Path.Combine(key, ".romba_size")))
					{
						File.CreateText(Path.Combine(key, ".romba_size"));
					}
					if (!File.Exists(Path.Combine(key, ".romba_size.backup")))
					{
						File.CreateText(Path.Combine(key, ".romba_size.backup"));
					}
				}
			}
			if (port < 0)
			{
				port = 0;
			}
			if (port > 65535)
			{
				port = 65535;
			}

			// Finally set all of the fields
			_workers = workers;
			_logdir = logdir;
			_tmpdir = tmpdir;
			_webdir = webdir;
			_baddir = baddir;
			_verbosity = verbosity;
			_cores = cores;
			_dats = dats;
			_db = db;
			_connectionString = connectionString;
			_depots = depots;
			_port = port;
		}

		/// <summary>
		/// Moves DAT index entries for orphaned DATs to backup folder
		/// </summary>
		private static void PurgeBackup()
		{
			_logger.User("This feature is not yet implemented: purge-backup");
		}

		/// <summary>
		/// Deletes DAT index entries for orphaned DATs
		/// </summary>
		private static void PurgeDelete()
		{
			_logger.User("This feature is not yet implemented: purge-delete");
		}

		/// <summary>
		/// Populate or refresh the database information
		/// </summary>
		/// <remarks>This has no link between Dat and file at all...</remarks>
		private static void RefreshDatabase()
		{
			// Make sure the db is set
			if (String.IsNullOrEmpty(_db))
			{
				_db = "db.sqlite";
				_connectionString = "Data Source=" + _db + ";Version = 3;";
			}

			// Make sure the file exists
			if (!File.Exists(_db))
			{
				DatabaseTools.EnsureDatabase(_dbSchema, _db, _connectionString);
			}

			// Make sure the dats dir is set
			if (String.IsNullOrEmpty(_dats))
			{
				_dats = "dats";
			}

			// Make sure the folder exists
			if (!Directory.Exists(_dats))
			{
				Directory.CreateDirectory(_dats);
			}

			// First get a list of SHA-1's from the input DATs
			DatFile datroot = new DatFile { Type = "SuperDAT", };
			datroot.PopulateDatFromDir(_dats, false, false, false, false, false, false, false, _tmpdir, false, null, 4, _logger);
			datroot.BucketBySHA1(false, _logger, false);

			// Create a List of dat hashes in the database (SHA-1)
			List<string> databaseDats = new List<string>();
			List<string> unneeded = new List<string>();

			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Populate the List from the database
			_logger.User("Populating the list of existing DATs");
			DateTime start = DateTime.Now;

			string query = "SELECT DISTINCT hash FROM dat";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			SqliteDataReader sldr = slc.ExecuteReader();
			if (sldr.HasRows)
			{
				sldr.Read();
				string hash = sldr.GetString(0);
				if (datroot.Files.ContainsKey(hash))
				{
					datroot.Files[hash] = null;
					databaseDats.Add(hash);
				}
				else if (!databaseDats.Contains(hash))
				{
					unneeded.Add(hash);
				}
			}
			datroot.BucketByGame(false, true, _logger, false);

			_logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			slc.Dispose();
			sldr.Dispose();

			// Loop through the Dictionary and add all data
			_logger.User("Adding new DAT information");
			start = DateTime.Now;
			foreach (string key in datroot.Files.Keys)
			{
				foreach (Rom value in datroot.Files[key])
				{
					AddDatToDatabase(value, dbc);
				}
			}

			_logger.User("Adding complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now loop through and remove all references to old Dats
			_logger.User("Removing unmatched DAT information");
			start = DateTime.Now;

			foreach (string dathash in unneeded)
			{
				query = "DELETE FROM dats WHERE hash=\"" + dathash + "\"";
				slc = new SqliteCommand(query, dbc);
				slc.ExecuteNonQuery();
				slc.Dispose();
			}
			_logger.User("Removing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			dbc.Dispose();
		}

		private static void AddDatToDatabase(Rom dat, SqliteConnection dbc)
		{
			// Get the dat full path
			string fullpath = Path.Combine(_dats, (dat.Machine.Name == "dats" ? "" : dat.Machine.Name), dat.Name);

			// Parse the Dat if possible
			_logger.User("Adding from '" + dat.Name + "'");
			DatFile tempdat = new DatFile();
			tempdat.Parse(fullpath, 0, 0, _logger);

			// If the Dat wasn't empty, add the information
			SqliteCommand slc = new SqliteCommand();
			if (tempdat.Files.Count != 0)
			{
				string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
				string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
				string sha1query = "INSERT OR IGNORE INTO sha1 (sha1) VALUES";
				string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
				string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

				// Loop through the parsed entries
				foreach (string romkey in tempdat.Files.Keys)
				{
					foreach (DatItem datItem in tempdat.Files[romkey])
					{
						_logger.Verbose("Checking and adding file '" + datItem.Name);

						if (datItem.Type == ItemType.Rom)
						{
							Rom rom = (Rom)datItem;

							if (!String.IsNullOrEmpty(rom.CRC))
							{
								crcquery += " (\"" + rom.CRC + "\"),";
							}
							if (!String.IsNullOrEmpty(rom.MD5))
							{
								md5query += " (\"" + rom.MD5 + "\"),";
							}
							if (!String.IsNullOrEmpty(rom.SHA1))
							{
								sha1query += " (\"" + rom.SHA1 + "\"),";

								if (!String.IsNullOrEmpty(rom.CRC))
								{
									crcsha1query += " (\"" + rom.CRC + "\", \"" + rom.SHA1 + "\"),";
								}
								if (!String.IsNullOrEmpty(rom.MD5))
								{
									md5sha1query += " (\"" + rom.MD5 + "\", \"" + rom.SHA1 + "\"),";
								}
							}
						}
						else if (datItem.Type == ItemType.Disk)
						{
							Disk disk = (Disk)datItem;

							if (!String.IsNullOrEmpty(disk.MD5))
							{
								md5query += " (\"" + disk.MD5 + "\"),";
							}
							if (!String.IsNullOrEmpty(disk.SHA1))
							{
								sha1query += " (\"" + disk.SHA1 + "\"),";

								if (!String.IsNullOrEmpty(disk.MD5))
								{
									md5sha1query += " (\"" + disk.MD5 + "\", \"" + disk.SHA1 + "\"),";
								}
							}
						}
					}
				}

				// Now run the queries after fixing them
				if (crcquery != "INSERT OR IGNORE INTO crc (crc) VALUES")
				{
					slc = new SqliteCommand(crcquery.TrimEnd(','), dbc);
					slc.ExecuteNonQuery();
				}
				if (md5query != "INSERT OR IGNORE INTO md5 (md5) VALUES")
				{
					slc = new SqliteCommand(md5query.TrimEnd(','), dbc);
					slc.ExecuteNonQuery();
				}
				if (sha1query != "INSERT OR IGNORE INTO sha1 (sha1) VALUES")
				{
					slc = new SqliteCommand(sha1query.TrimEnd(','), dbc);
					slc.ExecuteNonQuery();
				}
				if (crcsha1query != "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES")
				{
					slc = new SqliteCommand(crcsha1query.TrimEnd(','), dbc);
					slc.ExecuteNonQuery();
				}
				if (md5sha1query != "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES")
				{
					slc = new SqliteCommand(md5sha1query.TrimEnd(','), dbc);
					slc.ExecuteNonQuery();
				}
			}

			string datquery = "INSERT OR IGNORE INTO dat (hash) VALUES (\"" + dat.SHA1 + "\")";
			slc = new SqliteCommand(datquery, dbc);
			slc.ExecuteNonQuery();
			slc.Dispose();
		}

		/// <summary>
		/// Rescan a particular depot path into the database
		/// </summary>
		/// <param name="depotname">Path to the depot to be rescanned</param>
		private static void Rescan(string depotname)
		{
			// Check that it's a valid depot first
			if (!_depots.ContainsKey(depotname))
			{
				_logger.User("'" + depotname + "' is not a recognized depot. Please add it to your configuration file and try again");
				return;
			}

			// Then check that the depot is online
			if (!Directory.Exists(depotname))
			{
				_logger.User("'" + depotname + "' does not appear to be online. Please check its status and try again");
				return;
			}

			// Open the database connection
			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// If we have it, then check for all hashes that are in that depot
			List<string> hashes = new List<string>();
			string query = "SELECT sha1 FROM sha1 WHERE depot=\"" + depotname + "\"";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			SqliteDataReader sldr = slc.ExecuteReader();
			if (sldr.HasRows)
			{
				while (sldr.Read())
				{
					hashes.Add(sldr.GetString(0));
				}
			}

			// Now rescan the depot itself
			DatFile depot = new DatFile();
			depot.PopulateDatFromDir(depotname, false, false, false, false, true, false, false, _tmpdir, false, null, _workers, _logger);
			depot.BucketBySHA1(false, _logger, false);

			// Set the base queries to use
			string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
			string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
			string sha1query = "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES";
			string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
			string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

			// Once we have both, check for any new files
			List<string> dupehashes = new List<string>();
			List<string> keys = depot.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = depot.Files[key];
				foreach (Rom rom in roms)
				{
					if (hashes.Contains(rom.SHA1))
					{
						dupehashes.Add(rom.SHA1);
						hashes.Remove(rom.SHA1);
					}
					else if (!dupehashes.Contains(rom.SHA1))
					{
						if (!String.IsNullOrEmpty(rom.CRC))
						{
							crcquery += " (\"" + rom.CRC + "\"),";
						}
						if (!String.IsNullOrEmpty(rom.MD5))
						{
							md5query += " (\"" + rom.MD5 + "\"),";
						}
						if (!String.IsNullOrEmpty(rom.SHA1))
						{
							sha1query += " (\"" + rom.SHA1 + "\", \"" + depotname + "\"),";

							if (!String.IsNullOrEmpty(rom.CRC))
							{
								crcsha1query += " (\"" + rom.CRC + "\", \"" + rom.SHA1 + "\"),";
							}
							if (!String.IsNullOrEmpty(rom.MD5))
							{
								md5sha1query += " (\"" + rom.MD5 + "\", \"" + rom.SHA1 + "\"),";
							}
						}
					}
				}
			}

			// Now run the queries after fixing them
			if (crcquery != "INSERT OR IGNORE INTO crc (crc) VALUES")
			{
				slc = new SqliteCommand(crcquery.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
			}
			if (md5query != "INSERT OR IGNORE INTO md5 (md5) VALUES")
			{
				slc = new SqliteCommand(md5query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
			}
			if (sha1query != "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES")
			{
				slc = new SqliteCommand(sha1query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
			}
			if (crcsha1query != "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES")
			{
				slc = new SqliteCommand(crcsha1query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
			}
			if (md5sha1query != "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES")
			{
				slc = new SqliteCommand(md5sha1query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
			}

			// Now that we've added the information, we get to remove all of the hashes that we want to
			query = @"DELETE FROM sha1
JOIN crcsha1
	ON sha1.sha1=crcsha1.sha1
JOIN md5sha1
	ON sha1.sha1=md5sha1.sha1
JOIN crc
	ON crcsha1.crc=crc.crc
JOIN md5
	ON md5sha1.md5=md5.md5
WHERE sha1.sha1 IN (""" + String.Join("\",\"", hashes) + "\")";
			slc = new SqliteCommand(query, dbc);
			slc.ExecuteNonQuery();

			// Dispose of the database connection
			slc.Dispose();
			dbc.Dispose();
		}

		#endregion
	}
}
