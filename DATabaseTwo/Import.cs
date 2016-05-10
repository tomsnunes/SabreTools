using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using SabreTools.Helper;

namespace SabreTools
{
	public class Import
	{
		// Private instance variables
		private string _datroot;
		private string _connectionString;
		private Logger _logger;
		private bool _ignore;

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

		/// <summary>
		/// Initialize an Import object with the given information
		/// </summary>
		/// <param name="datroot">Root directory where all DAT files are held</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <param name="logger">Logger object for file or console output</param>
		/// <param name="ignore">False if each DAT that has no defined source asks for user input (default), true otherwise</param>
		public Import(string datroot, string connectionString, Logger logger, bool ignore = false)
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
		public bool ImportData()
		{
			_logger.Log("Beginning import/update process");
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				Dictionary<string, int> hashes = new Dictionary<string, int>();
				string query = "SELECT sha1, id FROM dats";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							hashes.Add(sldr.GetString(0), sldr.GetInt32(1));
						}
					}
				}

				SHA1 sha1 = SHA1.Create();
				query = "SELECT * FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							int systemid = sldr.GetInt32(0);
							string system = _datroot + Path.DirectorySeparatorChar + sldr.GetString(1) + " - " + sldr.GetString(2);
							system = system.Trim();

							_logger.Log("System: " + system.Remove(0, 5));

							// Audit all DATs in the folder
							foreach (string file in Directory.GetFiles(system, "*", SearchOption.AllDirectories))
							{
								string hash = "";
								using (FileStream fs = File.Open(file, FileMode.Open))
								{
									hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
								}

								// If the hash isn't in add it and all required information
								int hashid = -1;
								if (!hashes.ContainsKey(hash))
								{
									_logger.Log("Adding file information for " + Path.GetFileName(file));

									string squery = @"BEGIN;
INSERT INTO dats (size, sha1, name)
VALUES (" + (new FileInfo(file)).Length + ", '" + hash + "', '" + file.Replace("'", "''") + @"');
SELECT last_insert_rowid();
COMMIT;";
									using (SqliteCommand sslc = new SqliteCommand(squery, dbc))
									{
										using (SqliteDataReader ssldr = sslc.ExecuteReader())
										{
											if (ssldr.Read())
											{
												hashid = ssldr.GetInt32(0);
											}
										}
									}

									// Add the hash to the temporary Dictionary
									hashes.Add(hash, hashid);

									// Now try to determine the source for the file based on the name
									string source = GetSourceFromFileName(Path.GetFileName(file));
									int sourceid = 0;

									Dictionary<string, int> sources = new Dictionary<string, int>();
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

									// Only if we're not ignoring new sources should be ask the user for input
									if (!_ignore)
									{
										// We want to reset "Default" at this point, just in case
										if (source.ToLowerInvariant() == "default")
										{
											source = "";
										}

										// If the source is blank, ask the user to supply one
										while (source == "" && sourceid == 0)
										{
											Console.Clear();
											Build.Start("DATabaseTwo");

											Console.WriteLine("Sources:");
											foreach (KeyValuePair<string, int> pair in sources)
											{
												Console.WriteLine("    " + pair.Value + " - " + pair.Key);
											}
											Console.WriteLine("\nFor file name: " + Path.GetFileName(file));
											Console.Write("Select a source above or enter a new one: ");
											source = Console.ReadLine();

											Int32.TryParse(source, out sourceid);

											// If the value could be parsed, reset the source string
											if (sourceid != 0)
											{
												source = "";
											}

											// If the source ID is set check to see if it's valid
											if (sourceid != 0 && !sources.ContainsValue(sourceid))
											{
												Console.WriteLine("Invalid selection: " + sourceid);
												Console.ReadLine();
												sourceid = 0;
											}
										}

										// If the source isn't in, add it and get the insert id
										if (source != "" && sourceid == 0 && !sources.ContainsKey(source.ToLowerInvariant()))
										{
											string tquery = @"BEGIN;
INSERT INTO source (name, url)
VALUES ('" + source + @"', '');
SELECT last_insert_rowid();
COMMIT;";
											using (SqliteCommand sslc = new SqliteCommand(tquery, dbc))
											{
												using (SqliteDataReader ssldr = sslc.ExecuteReader())
												{
													if (ssldr.Read())
													{
														sourceid = ssldr.GetInt32(0);
													}
												}
											}

											// Add the new source to the temporary Dictionary
											sources.Add(source, sourceid);
										}
										// Otherwise, get the ID
										else if (source != "" && sourceid == 0 && sources.ContainsKey(source))
										{
											sourceid = sources[source.ToLowerInvariant()];
										}
										// Otherwise, we should already have an ID
									}
									else
									{
										sourceid = sources[source.ToLowerInvariant()];
									}

									// Add the source and system link to the database
									string uquery = @"INSERT OR IGNORE INTO datsdata (id, key, value)
VALUES (" + hashid + ", 'source', '" + sourceid + @"'),
(" + hashid + ", 'system', '" + systemid + "')";
									using (SqliteCommand uslc = new SqliteCommand(uquery, dbc))
									{
										uslc.ExecuteNonQuery();
									}
								}
							}
						}
					}
				}
			}

			_logger.Log("Import/update process complete!");

			return true;
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

			if (Regex.IsMatch(filename, _nonGoodPattern))
			{
				fileinfo = Regex.Match(filename, _nonGoodPattern).Groups;
				if (!Remapping.NonGood.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as NonGood but could not be mapped.");
					return source;
				}
				source = "NonGood";
			}
			else if (Regex.IsMatch(filename, _nonGoodSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _nonGoodSpecialPattern).Groups;
				if (!Remapping.NonGood.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as NonGood but could not be mapped.");
					return source;
				}
				source = "NonGood";
			}
			else if (Regex.IsMatch(filename, _goodPattern))
			{
				fileinfo = Regex.Match(filename, _goodPattern).Groups;
				if (!Remapping.Good.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Good but could not be mapped.");
					return source;
				}
				source = "Good";
			}
			else if (Regex.IsMatch(filename, _goodXmlPattern))
			{
				fileinfo = Regex.Match(filename, _goodXmlPattern).Groups;
			}
			else if (Regex.IsMatch(filename, _maybeIntroPattern))
			{
				fileinfo = Regex.Match(filename, _maybeIntroPattern).Groups;
				if (!Remapping.MaybeIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Maybe-Intro but could not be mapped.");
					return source;
				}
				source = "Maybe-Intro";
			}
			else if (Regex.IsMatch(filename, _noIntroPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroPattern).Groups;
				if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			// For numbered DATs only
			else if (Regex.IsMatch(filename, _noIntroNumberedPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroNumberedPattern).Groups;
				if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			// For N-Gage and Gizmondo only
			else if (Regex.IsMatch(filename, _noIntroSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroSpecialPattern).Groups;
				if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			else if (Regex.IsMatch(filename, _redumpPattern))
			{
				fileinfo = Regex.Match(filename, _redumpPattern).Groups;
				if (!Remapping.Redump.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Redump but could not be mapped.");
					return source;
				}
				source = "Redump";
			}
			// For special BIOSes only
			else if (Regex.IsMatch(filename, _redumpBiosPattern))
			{
				fileinfo = Regex.Match(filename, _redumpBiosPattern).Groups;
				if (!Remapping.Redump.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Redump but could not be mapped.");
					return source;
				}
				source = "Redump";
			}
			else if (Regex.IsMatch(filename, _tosecPattern))
			{
				fileinfo = Regex.Match(filename, _tosecPattern).Groups;
				if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
				{
					// Handle special case mappings found only in TOSEC
					fileinfo = Regex.Match(filename, _tosecSpecialPatternA).Groups;

					if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
					{
						fileinfo = Regex.Match(filename, _tosecSpecialPatternB).Groups;

						if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
						{
							_logger.Warning("The filename " + fileinfo[1].Value + " was matched as TOSEC but could not be mapped.");
							return source;
						}
					}
				}
				source = "TOSEC";
			}
			else if (Regex.IsMatch(filename, _truripPattern))
			{
				fileinfo = Regex.Match(filename, _truripPattern).Groups;
				if (!Remapping.TruRip.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as TruRip but could not be mapped.");
					return source;
				}
				source = "trurip";
			}
			else if (Regex.IsMatch(filename, _zandroPattern))
			{
				source = "Zandro";
			}
			else if (Regex.IsMatch(filename, _defaultPattern))
			{
				fileinfo = Regex.Match(filename, _defaultPattern).Groups;
				source = fileinfo[3].Value;
			}
			else if (Regex.IsMatch(filename, _defaultSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _defaultSpecialPattern).Groups;
				source = fileinfo[3].Value;
			}
			else if (Regex.IsMatch(filename, _mamePattern))
			{
				fileinfo = Regex.Match(filename, _mamePattern).Groups;
				if (!Remapping.MAME.ContainsKey(fileinfo[1].Value))
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
