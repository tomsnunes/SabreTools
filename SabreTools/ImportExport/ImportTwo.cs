using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SabreTools
{
	public class ImportTwo : IImport
	{
		// Private instance variables
		private string _datroot;
		private string _connectionString;
		private Logger _logger;
		private bool _ignore;

		/// <summary>
		/// Initialize an Import object with the given information
		/// </summary>
		/// <param name="datroot">Root directory where all DAT files are held</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <param name="logger">Logger object for file or console output</param>
		/// <param name="ignore">False if each DAT that has no defined source asks for user input (default), true otherwise</param>
		public ImportTwo(string datroot, string connectionString, Logger logger, bool ignore = false)
		{
			_datroot = datroot;
			_connectionString = connectionString;
			_logger = logger;
			_ignore = ignore;
		}

		/// <summary>
		/// Perform initial or incremental import of DATs in the root folder
		/// </summary>
		/// <returns>True if the data could be inserted or updated correctly, false otherwise</returns>
		public bool UpdateDatabase()
		{
			_logger.User("Beginning import/update process");

			Dictionary<Tuple<long, string, string>, int> missing = ImportData();
			bool success = RemoveData(missing);

			_logger.User("Import/update process complete!");

			return success;
		}

		/// <summary>
		/// Import data into the database and return all files not found in the list
		/// </summary>
		/// <returns>List of files that were not found in the audit</returns>
		private Dictionary<Tuple<long, string, string>, int> ImportData()
		{
			// Create the empty dictionary for file filtering and output
			Dictionary<Tuple<long, string, string>, int> dbfiles = new Dictionary<Tuple<long, string, string>, int>();

			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				_logger.User("Populating reference objects");

				// Populate the list of files in the database with Tuples (size, sha1, name)
				string query = "SELECT id, size, sha1, name FROM dats";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							dbfiles.Add(Tuple.Create(sldr.GetInt64(1), sldr.GetString(2), sldr.GetString(3)), sldr.GetInt32(0));
						}
					}
				}

				// Populate the list of systems
				Dictionary<string, int> systems = new Dictionary<string, int>();
				query = "SELECT id, manufacturer, name FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							systems.Add(sldr.GetString(1) + " - " + sldr.GetString(2), sldr.GetInt32(0));
						}
					}
				}

				// Populate the list of sources (initial)
				SortedDictionary<string, int> sources = new SortedDictionary<string, int>();
				sources.Add("default", 0);
				query = "SELECT name, id FROM source";
				using (SqliteCommand sslc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader ssldr = sslc.ExecuteReader())
					{
						while (ssldr.Read())
						{
							sources.Add(ssldr.GetString(0).ToLowerInvariant(), ssldr.GetInt32(1));
						}
					}
				}

				// Interate through each system and check files
				SHA1 sha1 = SHA1.Create();
				foreach (KeyValuePair<string, int> kv in systems)
				{
					_logger.User("Processing DATs for system: '" + kv.Key + "'");

					// Set the folder to iterate through based on the DAT root
					string folder = _datroot + Path.DirectorySeparatorChar + kv.Key.Trim();

					// Audit all files in the folder
					foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
					{
						// First get the file information for comparison
						long size = (new FileInfo(file)).Length;
						string hash = "";
						using (FileStream fs = File.Open(file, FileMode.Open))
						{
							hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
						}

						// If it's not in the list of known files, add it
						if (!dbfiles.ContainsKey(Tuple.Create(size, hash, file)))
						{
							// First add the information to the database as is and return the new insert ID
							_logger.Log("Adding file information for " + Path.GetFileName(file));

							int hashid = -1;
							query = @"INSERT INTO dats (size, sha1, name)
VALUES (" + (new FileInfo(file)).Length + ", '" + hash + "', '" + file.Replace("'", "''") + @"')";
							using (SqliteCommand slc = new SqliteCommand(query, dbc))
							{
								slc.ExecuteNonQuery();
							}

							query = "SELECT last_insert_rowid()";
							using (SqliteCommand slc = new SqliteCommand(query, dbc))
							{
								using (SqliteDataReader sldr = slc.ExecuteReader())
								{
									if (sldr.Read())
									{
										hashid = sldr.GetInt32(0);
									}
								}
							}

							// Next we try to figure out the source ID from the name
							string possiblesource = GetSourceFromFileName(Path.GetFileName(file));

							// Try to get the source ID from the name
							int sourceid = (sources.ContainsKey(possiblesource.ToLowerInvariant()) ? sources[possiblesource] : 0);

							// If we have a "default" ID and we're not ignoring new sources, prompt for a source input
							if (!_ignore && sourceid <= 1)
							{
								// We want to reset "Default" at this point, just in case
								if (possiblesource.ToLowerInvariant() == "default")
								{
									possiblesource = "";
								}

								// If the source is blank, ask the user to supply one
								while (possiblesource == "" && sourceid == 0)
								{
									Console.Clear();
									Build.Start("DATabaseTwo");

									Console.WriteLine("Sources:");
									foreach (KeyValuePair<string, int> pair in sources)
									{
										Console.WriteLine("    " + pair.Value + " - " + Style.SentenceCase(pair.Key));
									}
									Console.WriteLine("\nFor file name: " + Path.GetFileName(file));
									Console.Write("Select a source above or enter a new one: ");
									possiblesource = Console.ReadLine();

									Int32.TryParse(possiblesource, out sourceid);

									// If the value could be parsed, reset the source string
									if (sourceid != 0)
									{
										possiblesource = "";
									}

									// If the source ID is set check to see if it's valid
									if (sourceid != 0 && !sources.ContainsValue(sourceid))
									{
										Console.WriteLine("Invalid selection: " + sourceid);
										Console.ReadLine();
										sourceid = 0;
									}
								}

								// If we have a non-empty possible source and it's in the database, get the id
								if (possiblesource != "" && sources.ContainsKey(possiblesource.ToLowerInvariant()))
								{
									sourceid = sources[possiblesource.ToLowerInvariant()];
								}

								// If we have a non-empty possible source and it's not in the database, insert and get the id
								else if (possiblesource != "" && !sources.ContainsKey(possiblesource.ToLowerInvariant()))
								{
									query = @"BEGIN;
INSERT INTO source (name, url)
VALUES ('" + possiblesource + @"', '');
SELECT last_insertConstants.Rowid();
COMMIT;";
									using (SqliteCommand slc = new SqliteCommand(query, dbc))
									{
										using (SqliteDataReader sldr = slc.ExecuteReader())
										{
											if (sldr.Read())
											{
												sourceid = sldr.GetInt32(0);
											}
										}
									}

									// Add the new source to the current dictionary
									sources.Add(possiblesource.ToLowerInvariant(), sourceid);
								}
							}

							// Now that we have a source ID, we can add the mappings for system and source to the database
							query = @"INSERT OR IGNORE INTO datsdata (id, key, value)
VALUES (" + hashid + ", 'source', '" + sourceid + @"'),
(" + hashid + ", 'system', '" + kv.Value + "')";
							using (SqliteCommand slc = new SqliteCommand(query, dbc))
							{
								slc.ExecuteNonQuery();
							}
						}

						// Otherwise, remove it from the list of found items
						else
						{
							dbfiles.Remove(Tuple.Create(size, hash, file));
						}
					}
				}
			}

			return dbfiles;
		}

		/// <summary>
		/// Remove all data associated with various files
		/// </summary>
		/// <param name="missing">List of file identifiers to remove from the database</param>
		/// <returns>True if everything went well, false otherwise</returns>
		private bool RemoveData(Dictionary<Tuple<long, string, string>, int> missing)
		{
			bool success = true;

			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				// Get a comma-separated list of IDs from the input files
				string idlist = String.Join(",", missing.Values);

				// Now remove all of the files from the database
				string query = @"BEGIN;
DELETE FROM datsdata WHERE id IN (" + idlist + @");
DELETE FROM dats WHERE id IN (" + idlist + @");
COMMIT;";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					slc.ExecuteNonQuery();
				}
			}

			return success;
		}

		/// <summary>
		/// Determine the source name from the file name, if possible
		/// </summary>
		/// <param name="filename">The name of the file to be checked</param>
		/// <returns>The name of the source if determined, blank otherwise</returns>
		private string GetSourceFromFileName(string filename)
		{
			string source = "Default";

			// Determine which dattype we have
			GroupCollection fileinfo;

			if (Regex.IsMatch(filename, Constants.NonGoodPattern))
			{
				fileinfo = Regex.Match(filename, Constants.NonGoodPattern).Groups;
				if (!Remapping.DatMaps["NonGood"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as NonGood but could not be mapped.");
					return source;
				}
				source = "NonGood";
			}
			else if (Regex.IsMatch(filename, Constants.NonGoodSpecialPattern))
			{
				fileinfo = Regex.Match(filename, Constants.NonGoodSpecialPattern).Groups;
				if (!Remapping.DatMaps["NonGood"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as NonGood but could not be mapped.");
					return source;
				}
				source = "NonGood";
			}
			else if (Regex.IsMatch(filename, Constants.GoodPattern))
			{
				fileinfo = Regex.Match(filename, Constants.GoodPattern).Groups;
				if (!Remapping.DatMaps["Good"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Good but could not be mapped.");
					return source;
				}
				source = "Good";
			}
			else if (Regex.IsMatch(filename, Constants.GoodXmlPattern))
			{
				fileinfo = Regex.Match(filename, Constants.GoodXmlPattern).Groups;
				if (!Remapping.DatMaps["Good"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Good but could not be mapped.");
					return source;
				}
				source = "Good";
			}
			else if (Regex.IsMatch(filename, Constants.MaybeIntroPattern))
			{
				fileinfo = Regex.Match(filename, Constants.MaybeIntroPattern).Groups;
				if (!Remapping.DatMaps["MaybeIntro"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Maybe-Intro but could not be mapped.");
					return source;
				}
				source = "Maybe-Intro";
			}
			else if (Regex.IsMatch(filename, Constants.NoIntroPattern))
			{
				fileinfo = Regex.Match(filename, Constants.NoIntroPattern).Groups;
				if (!Remapping.DatMaps["NoIntro"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			// For numbered DATs only
			else if (Regex.IsMatch(filename, Constants.NoIntroNumberedPattern))
			{
				fileinfo = Regex.Match(filename, Constants.NoIntroNumberedPattern).Groups;
				if (!Remapping.DatMaps["NoIntro"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			// For N-Gage and Gizmondo only
			else if (Regex.IsMatch(filename, Constants.NoIntroSpecialPattern))
			{
				fileinfo = Regex.Match(filename, Constants.NoIntroSpecialPattern).Groups;
				if (!Remapping.DatMaps["NoIntro"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			else if (Regex.IsMatch(filename, Constants.RedumpPattern))
			{
				fileinfo = Regex.Match(filename, Constants.RedumpPattern).Groups;
				if (!Remapping.DatMaps["Redump"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Redump but could not be mapped.");
					return source;
				}
				source = "Redump";
			}
			// For special BIOSes only
			else if (Regex.IsMatch(filename, Constants.RedumpBiosPattern))
			{
				fileinfo = Regex.Match(filename, Constants.RedumpBiosPattern).Groups;
				if (!Remapping.DatMaps["Redump"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Redump but could not be mapped.");
					return source;
				}
				source = "Redump";
			}
			else if (Regex.IsMatch(filename, Constants.TosecPattern))
			{
				fileinfo = Regex.Match(filename, Constants.TosecPattern).Groups;
				if (!Remapping.DatMaps["TOSEC"].ContainsKey(fileinfo[1].Value))
				{
					// Handle special case mappings found only in TOSEC
					fileinfo = Regex.Match(filename, Constants.TosecSpecialPatternA).Groups;

					if (!Remapping.DatMaps["TOSEC"].ContainsKey(fileinfo[1].Value))
					{
						fileinfo = Regex.Match(filename, Constants.TosecSpecialPatternB).Groups;

						if (!Remapping.DatMaps["TOSEC"].ContainsKey(fileinfo[1].Value))
						{
							_logger.Warning("The filename " + fileinfo[1].Value + " was matched as TOSEC but could not be mapped.");
							return source;
						}
					}
				}
				source = "TOSEC";
			}
			else if (Regex.IsMatch(filename, Constants.TruripPattern))
			{
				fileinfo = Regex.Match(filename, Constants.TruripPattern).Groups;
				if (!Remapping.DatMaps["TruRip"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as TruRip but could not be mapped.");
					return source;
				}
				source = "trurip";
			}
			else if (Regex.IsMatch(filename, Constants.ZandroPattern))
			{
				source = "Zandro";
			}
			else if (Regex.IsMatch(filename, Constants.DefaultPattern))
			{
				fileinfo = Regex.Match(filename, Constants.DefaultPattern).Groups;
				source = fileinfo[3].Value;
			}
			else if (Regex.IsMatch(filename, Constants.DefaultSpecialPattern))
			{
				fileinfo = Regex.Match(filename, Constants.DefaultSpecialPattern).Groups;
				source = fileinfo[3].Value;
			}
			else if (Regex.IsMatch(filename, Constants.MamePattern))
			{
				fileinfo = Regex.Match(filename, Constants.MamePattern).Groups;
				if (!Remapping.DatMaps["MAME"].ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as MAME but could not be mapped.");
					return source;
				}
				source = "MAME";
			}

			return source;
		}
	}
}
