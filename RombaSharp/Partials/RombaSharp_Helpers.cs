using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace SabreTools
{
	public partial class RombaSharp
	{
		#region Helper methods

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
		/// Display the statistics in the database
		/// </summary>
		private static void DisplayDBStats()
		{
			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Total uncompressed size
			string query = "SELECT SUM(size) FROM data";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			_logger.User("Uncompressed size: " + Style.GetBytesReadable((long)slc.ExecuteScalar()));

			// Total number of files
			query = "SELECT COUNT(*) FROM data";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total files: " + (long)slc.ExecuteScalar());

			// Total number of files that exist
			query = "SELECT COUNT(*) FROM data WHERE indepot=1";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total files in depots: " + (long)slc.ExecuteScalar());

			// Total number of files that are missing
			query = "SELECT COUNT(*) FROM data WHERE indepot=0";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total files missing: " + (long)slc.ExecuteScalar());

			// Total number of CRCs
			query = "SELECT COUNT(crc) FROM data WHERE NOT crc=\"null\"";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total CRCs: " + (long)slc.ExecuteScalar());

			// Total number of MD5s
			query = "SELECT COUNT(md5) FROM data WHERE NOT md5=\"null\"";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total MD5s: " + (long)slc.ExecuteScalar());

			// Total number of SHA1s
			query = "SELECT COUNT(sha1) FROM data WHERE NOT sha1=\"null\"";
			slc = new SqliteCommand(query, dbc);
			_logger.User("Total SHA1s: " + (long)slc.ExecuteScalar());

			// Total number of DATs
			query = "SELECT COUNT(*) FROM dats GROUP BY hash";
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
		/// <remarks>Each hash has the following attributes: size, crc, md5, sha-1, dathash, indepot</remarks>
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

			// Create a List of dat hashes in the database (SHA-1)
			List<string> databaseDats = new List<string>();

			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Populate the List from the database
			_logger.User("Populating the list of existing DATs");
			DateTime start = DateTime.Now;

			string query = "SELECT DISTINCT hash FROM dats";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			SqliteDataReader sldr = slc.ExecuteReader();
			if (sldr.HasRows)
			{
				sldr.Read();
				string hash = sldr.GetString(0);
				if (!databaseDats.Contains(hash))
				{
					databaseDats.Add(hash);
				}
			}
			_logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			slc.Dispose();
			sldr.Dispose();

			// Now create a Dictionary of dats to parse from what's not in the database (SHA-1, Path)
			Dictionary<string, string> toscan = new Dictionary<string, string>();

			// Loop through the datroot and add only needed files
			_logger.User("Scanning DAT folder: '" + _dats + "'");
			start = DateTime.Now;

			foreach (string file in Directory.EnumerateFiles(_dats, "*", SearchOption.AllDirectories))
			{
				Rom dat = FileTools.GetFileInfo(file, _logger);

				// If the Dat isn't in the database and isn't already accounted for in the DatRoot, add it
				if (!databaseDats.Contains(dat.SHA1) && !toscan.ContainsKey(dat.SHA1))
				{
					toscan.Add(dat.SHA1, Path.GetFullPath(file));
				}

				// If the Dat is in the database already, remove it to find stragglers
				else if (databaseDats.Contains(dat.SHA1))
				{
					databaseDats.Remove(dat.SHA1);
				}
			}
			_logger.User("Scanning complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Loop through the Dictionary and add all data
			_logger.User("Adding new DAT information");
			start = DateTime.Now;

			foreach (string key in toscan.Keys)
			{
				// Parse the Dat if possible
				_logger.User("Adding from '" + toscan[key] + "'");
				DatFile tempdat = new DatFile();
				tempdat.Parse(toscan[key], 0, 0, _logger);

				// If the Dat wasn't empty, add the information
				if (tempdat.Files.Count != 0)
				{
					// Loop through the parsed entries
					foreach (string romkey in tempdat.Files.Keys)
					{
						foreach (Rom rom in tempdat.Files[romkey])
						{
							_logger.Verbose("Checking and adding file '" + rom.Name);

							query = "SELECT id FROM data WHERE size=" + rom.Size + " AND ("
								+ "(crc=\"" + rom.CRC + "\" OR crc=\"null\")"
								+ " AND (md5=\"" + rom.MD5 + "\" OR md5=\"null\")"
								+ " AND (sha1=\"" + rom.SHA1 + "\" OR sha1=\"null\"))";
							slc = new SqliteCommand(query, dbc);
							sldr = slc.ExecuteReader();
								
							// If the hash exists in the database, add the dat hash for that id if needed
							if (sldr.HasRows)
							{
								sldr.Read();
								long id = sldr.GetInt64(0);

								string squery = "SELECT * FROM dats WHERE id=" + id;
								SqliteCommand sslc = new SqliteCommand(squery, dbc);
								SqliteDataReader ssldr = sslc.ExecuteReader();

								// If the hash doesn't already exist, add it
								if (!ssldr.HasRows)
								{
									squery = "INSERT INTO dats (id, hash) VALUES (\"" + id + "\", \"" + key + "\")";
									sslc = new SqliteCommand(squery, dbc);
									sslc.ExecuteNonQuery();
								}

								ssldr.Dispose();
								sslc.Dispose();
							}

							// If it doesn't exist, add the hash and the dat hash for a new id
							else
							{
								string squery = "INSERT INTO data (size, crc, md5, sha1, indepot) VALUES ("
									+ rom.Size + ","
									+ "\"" + (rom.CRC == "" ? "null" : rom.CRC) + "\","
									+ "\"" + (rom.MD5 == "" ? "null" : rom.MD5) + "\","
									+ "\"" + (rom.SHA1 == "" ? "null" : rom.SHA1) + "\","
									+ "0)";
								SqliteCommand sslc = new SqliteCommand(squery, dbc);
								sslc.ExecuteNonQuery();

								long id = -1;

								squery = @"select last_insert_rowid()";
								sslc = new SqliteCommand(squery, dbc);
								id = (long)sslc.ExecuteScalar();

								squery = "INSERT INTO dats (id, hash) VALUES (\"" + id + "\", \"" + key + "\")";
								sslc = new SqliteCommand(squery, dbc);
								sslc.ExecuteNonQuery();
								sslc.Dispose();
							}
						}
					}
				}
			}
			_logger.User("Adding complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now loop through and remove all references to old Dats
			// TODO: Remove orphaned files as well
			_logger.User("Removing unmatched DAT information");
			start = DateTime.Now;

			foreach (string dathash in databaseDats)
			{
				query = "DELETE FROM dats WHERE hash=\"" + dathash + "\"";
				slc = new SqliteCommand(query, dbc);
				slc.ExecuteNonQuery();
				slc.Dispose();
			}
			_logger.User("Removing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

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

		#endregion
	}
}
