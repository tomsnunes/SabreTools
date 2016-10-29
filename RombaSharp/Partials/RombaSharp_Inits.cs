using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;

using SabreTools.Helper;
using SabreTools.Helper.Data;
using SabreTools.Helper.Dats;
using SabreTools.Helper.Tools;

#if mono
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

namespace SabreTools
{
	public partial class RombaSharp
	{
		#region Init Methods

		/// <summary>
		/// Wrap adding files to the depots
		/// </summary>
		/// <param name="inputs">List of input folders to use</param>
		/// <param name="onlyNeeded">True if only files in the database and don't exist are added, false otherwise</param>
		private static void InitArchive(List<string> inputs, bool onlyNeeded)
		{
			// First we want to get just all directories from the inputs
			List<string> onlyDirs = new List<string>();
			foreach (string input in inputs)
			{
				if (Directory.Exists(input))
				{
					onlyDirs.Add(Path.GetFullPath(input));
				}
			}

			// Then process all of the input directories into an internal DAT
			DatFile df = new DatFile();
			foreach (string dir in onlyDirs)
			{
				df.PopulateDatFromDir(dir, false, false, false, false, true, false, false, _tmpdir, false, null, _workers, _logger);

				// If we're looking for only needed, consider the zipfiles themselves too
				if (onlyNeeded)
				{
					df.PopulateDatFromDir(dir, false, false, false, true, true, false, false, _tmpdir, false, null, _workers, _logger);
				}
			}

			// Create an empty Dat for files that need to be rebuilt
			DatFile need = new DatFile();
			need.Files = new SortedDictionary<string, List<DatItem>>();

			// Open the database connection
			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Now that we have the Dats, add the files to the database
			string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
			string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
			string sha1query = "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES";
			string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
			string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

			foreach (string key in df.Files.Keys)
			{
				List<DatItem> datItems = df.Files[key];
				foreach (Rom rom in datItems)
				{
					// If we care about if the file exists, check the databse first
					if (onlyNeeded)
					{
						string query = "SELECT * FROM crcsha1 JOIN md5sha1 ON crcsha1.sha1=md5sha1.sha1"
									+ " WHERE crcsha1.crc=\"" + rom.CRC + "\""
									+ " OR md5sha1.md5=\"" + rom.MD5 + "\""
									+ " OR md5sha1.sha1=\"" + rom.SHA1 + "\"";
						SqliteCommand slc = new SqliteCommand(query, dbc);
						SqliteDataReader sldr = slc.ExecuteReader();
						
						if (sldr.HasRows)
						{
							// Add to the queries
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
								sha1query += " (\"" + rom.SHA1 + "\", \"" + _depots.Keys.ToList()[0] + "\"),";

								if (!String.IsNullOrEmpty(rom.CRC))
								{
									crcsha1query += " (\"" + rom.CRC + "\", \"" + rom.SHA1 + "\"),";
								}
								if (!String.IsNullOrEmpty(rom.MD5))
								{
									md5sha1query += " (\"" + rom.MD5 + "\", \"" + rom.SHA1 + "\"),";
								}
							}

							// Add to the Dat
							if (need.Files.ContainsKey(key))
							{
								need.Files[key].Add(rom);
							}
							else
							{
								List<DatItem> temp = new List<DatItem>();
								temp.Add(rom);
								need.Files.Add(key, temp);
							}
						}
					}
					// Otherwise, just add the file to the list
					else
					{
						// Add to the queries
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
							sha1query += " (\"" + rom.SHA1 + "\", \"" + _depots.Keys.ToList()[0] + "\"),";

							if (!String.IsNullOrEmpty(rom.CRC))
							{
								crcsha1query += " (\"" + rom.CRC + "\", \"" + rom.SHA1 + "\"),";
							}
							if (!String.IsNullOrEmpty(rom.MD5))
							{
								md5sha1query += " (\"" + rom.MD5 + "\", \"" + rom.SHA1 + "\"),";
							}
						}

						// Add to the Dat
						if (need.Files.ContainsKey(key))
						{
							need.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							need.Files.Add(key, temp);
						}
					}
				}
			}

			// Now run the queries, if they're populated
			if (crcquery != "INSERT OR IGNORE INTO crc (crc) VALUES")
			{
				SqliteCommand slc = new SqliteCommand(crcquery.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
				slc.Dispose();
			}
			if (md5query != "INSERT OR IGNORE INTO md5 (md5) VALUES")
			{
				SqliteCommand slc = new SqliteCommand(md5query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
				slc.Dispose();
			}
			if (sha1query != "INSERT OR IGNORE INTO sha1 (sha1) VALUES")
			{
				SqliteCommand slc = new SqliteCommand(sha1query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
				slc.Dispose();
			}
			if (crcsha1query != "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES")
			{
				SqliteCommand slc = new SqliteCommand(crcsha1query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
				slc.Dispose();
			}
			if (md5sha1query != "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES")
			{
				SqliteCommand slc = new SqliteCommand(md5sha1query.TrimEnd(','), dbc);
				slc.ExecuteNonQuery();
				slc.Dispose();
			}

			// Create the sorting object to use and rebuild the needed files
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers((onlyNeeded ? 0 : 1), (onlyNeeded ? 0 : 1), (onlyNeeded ? 0 : 1), (onlyNeeded ? 0 : 1));
			need.RebuildToOutput(onlyDirs, _depots.Keys.ToList()[0], _tmpdir, false, false, false, false /*inverse*/, OutputFormat.TorrentGzip, true, asl, false, null, 4, _logger);
		}

		/// <summary>
		/// Wrap building all files from a set of DATs
		/// </summary>
		/// <param name="inputs">List of input DATs to rebuild from</param>
		/// <param name="copy">True if files should be copied to output, false for rebuild</param>
		private static void InitBuild(List<string> inputs, bool copy)
		{
			// Verify the filenames
			Dictionary<string, string> foundDats = GetValidDats(inputs);

			// Create a base output folder
			if (!Directory.Exists("out"))
			{
				Directory.CreateDirectory("out");
			}

			// Now that we have the dictionary, we can loop through and output to a new folder for each
			foreach (string key in foundDats.Keys)
			{
				// Get the DAT file associated with the key
				DatFile datFile = new DatFile();
				datFile.Parse(Path.Combine(_dats, foundDats[key]), 0, 0, _logger, softlist: true);

				// Create the new output directory if it doesn't exist
				string outputFolder = Path.Combine("out", Path.GetFileNameWithoutExtension(foundDats[key]));
				if (!Directory.Exists(outputFolder))
				{
					Directory.CreateDirectory(outputFolder);
				}

				// Get all online depots
				List<string> onlineDepots = _depots.Where(d => d.Value.Item2).Select(d => d.Key).ToList();

				// Now scan all of those depots and rebuild
				ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(1, 1, 1, 1);
				datFile.RebuildToOutput(onlineDepots, outputFolder, _tmpdir, true, false, false, false /*inverse*/, (copy ? OutputFormat.TorrentGzip : OutputFormat.TorrentZip), copy, asl, false, null, 4, _logger);
			}
		}

		/// <summary>
		/// Wrap finding all files that are in both the database and a new Dat
		/// </summary>
		/// <param name="newdat"></param>
		private static void InitDiffDat(string newdat)
		{
			_logger.User("This feature is not yet implemented: diffdat");

			// First, we want to read in the DAT. Then for each file listed in the DAT, we check if it's in there or not.
			// If it is in there, we add it to an output DAT. If it's not, we skip. Then we output the DAT.
		}

		/// <summary>
		/// Wrap creating a Dat from a directory
		/// </summary>
		/// <param name="inputs"></param>
		private static void InitDir2Dat(List<string> inputs)
		{
			// Create a simple Dat output
			DatFile datdata = new DatFile()
			{
				FileName = Path.GetFileName(inputs[0]) + " Dir2Dat",
				Name = Path.GetFileName(inputs[0]) + " Dir2Dat",
				Description = Path.GetFileName(inputs[0]) + " Dir2Dat",
				DatFormat = DatFormat.Logiqx,
				Files = new SortedDictionary<string, List<DatItem>>(),
			};

			Logger logger = new Logger();
			foreach (string input in inputs)
			{
				datdata.PopulateDatFromDir(input, false /* noMD5 */, false /* noSHA1 */, true /* bare */, false /* archivesAsFiles */,
					true /* enableGzip */, false /* addBlanks */, false /* addDate */, _tmpdir /* tempDir */, false /* copyFiles */,
					null /* headerToCheckAgainst */, _workers /* maxDegreeOfParallelism */, _logger);
				datdata.WriteToFile("", logger);
			}
			logger.Close();
		}

		/// <summary>
		/// Wrap creating a fixdat for each Dat
		/// </summary>
		/// <param name="inputs"></param>
		private static void InitFixdat(List<string> inputs)
		{
			_logger.User("This feature is not yet implemented: fixdat");

			// Verify the filenames
			Dictionary<string, string> foundDats = GetValidDats(inputs);

			// Once we have each DAT, look up each associated hash based on the hash of the DATs.
			// Then, for each rom, check to see if they exist in the folder. If they don't, add it
			// to the fixDAT. Then output when the DAT is done, processing, moving on to the next...
			// NOTE: This might share code with InitMiss
		}

		/// <summary>
		/// Wrap looking up if hashes exist in the database
		/// </summary>
		/// <param name="inputs">List of input strings representing hashes to check for</param>
		private static void InitLookup(List<string> inputs)
		{
			// First, try to figure out what type of hash each is by length and clean it
			List<string> crc = new List<string>();
			List<string> md5 = new List<string>();
			List<string> sha1 = new List<string>();
			foreach (string input in inputs)
			{
				string temp = "";
				if (input.Length == Constants.CRCLength)
				{
					temp = Style.CleanHashData(input, Constants.CRCLength);
					if (temp != "")
					{
						crc.Add(temp);
					}
				}
				else if (input.Length == Constants.MD5Length)
				{
					temp = Style.CleanHashData(input, Constants.MD5Length);
					if (temp != "")
					{
						md5.Add(temp);
					}
				}
				else if (input.Length == Constants.SHA1Length)
				{
					temp = Style.CleanHashData(input, Constants.SHA1Length);
					if (temp != "")
					{
						sha1.Add(temp);
					}
				}
			}

			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Now, search for each of them and return true or false for each
			foreach (string input in crc)
			{
				string query = "SELECT * FROM crc WHERE crc=\"" + input + "\"";
				SqliteCommand slc = new SqliteCommand(query, dbc);
				SqliteDataReader sldr = slc.ExecuteReader();
				if (sldr.HasRows)
				{
					_logger.User("For hash '" + input + "' there were " + sldr.RecordsAffected + " matches in the database");
				}
				else
				{
					_logger.User("Hash '" + input + "' had no matches in the database");
				}

				sldr.Dispose();
				slc.Dispose();
			}
			foreach (string input in md5)
			{
				string query = "SELECT * FROM md5 WHERE md5=\"" + input + "\"";
				SqliteCommand slc = new SqliteCommand(query, dbc);
				SqliteDataReader sldr = slc.ExecuteReader();
				if (sldr.HasRows)
				{
					_logger.User("For hash '" + input + "' there were " + sldr.RecordsAffected + " matches in the database");
				}
				else
				{
					_logger.User("Hash '" + input + "' had no matches in the database");
				}

				sldr.Dispose();
				slc.Dispose();
			}
			foreach (string input in sha1)
			{
				string query = "SELECT * FROM sha1 WHERE sha1=\"" + input + "\"";
				SqliteCommand slc = new SqliteCommand(query, dbc);
				SqliteDataReader sldr = slc.ExecuteReader();
				if (sldr.HasRows)
				{
					_logger.User("For hash '" + input + "' there were " + sldr.RecordsAffected + " matches in the database");
				}
				else
				{
					_logger.User("Hash '" + input + "' had no matches in the database");
				}

				sldr.Dispose();
				slc.Dispose();
			}

			dbc.Dispose();
		}

		/// <summary>
		/// Wrap creating a havefile and a missfile for each Dat
		/// </summary>
		/// <param name="inputs"></param>
		private static void InitMiss(List<string> inputs)
		{
			_logger.User("This feature is not yet implemented: miss");

			// Verify the filenames
			Dictionary<string, string> foundDats = GetValidDats(inputs);

			// Once we have each DAT, look up each associated hash based on the hash of the DATs.
			// Then, for each rom, check to see if they exist in the folder. If they do, add it
			// to the have DAT, else wise go to the miss DAT. Then output both when the DAT is done
			// processing, moving on to the next...
			// NOTE: This might share code with InitFixdat
		}

		#endregion
	}
}
