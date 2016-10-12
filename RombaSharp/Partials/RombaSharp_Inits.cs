using Mono.Data.Sqlite;
using SabreTools.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
				df.PopulateDatFromDir(dir, false, false, false, false, true, false, false, _tmpdir, false, null, 4, _logger);
			}

			// Create an empty Dat for files that need to be rebuilt
			DatFile need = new DatFile();
			need.Files = new SortedDictionary<string, List<DatItem>>();

			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Now that we have the Dats, add the files to the database
			foreach (string key in df.Files.Keys)
			{
				List<DatItem> datItems = df.Files[key];
				foreach (Rom rom in datItems)
				{
					string query = "SELECT id FROM data WHERE size=" + rom.Size
								+ " AND (crc=\"" + rom.CRC + "\" OR crc=\"null\")"
								+ " AND (md5=\"" + rom.MD5 + "\" OR md5=\"null\")"
								+ " AND (sha1=\"" + rom.SHA1 + "\" OR sha1=\"null\")"
								+ " AND indepot=0";
					SqliteCommand slc = new SqliteCommand(query, dbc);
					SqliteDataReader sldr = slc.ExecuteReader();

					// If a row is returned, add the file and change the existence
					if (sldr.HasRows)
					{
						sldr.Read();
						long id = sldr.GetInt64(0);

						string squery = "UPDATE data SET indepot=1 WHERE id=" + id;
						SqliteCommand sslc = new SqliteCommand(squery, dbc);
						sslc.ExecuteNonQuery();
						sslc.Dispose();

						// Add the rom to the files that need to be rebuilt
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

					// If it doesn't exist, and we're not adding only needed files
					else if (!onlyNeeded)
					{
						string squery = "INSERT INTO data (size, crc, md5, sha1, indepot) VALUES ("
							+ rom.Size + ","
							+ "\"" + (rom.CRC == "" ? "null" : rom.CRC) + "\","
							+ "\"" + (rom.MD5 == "" ? "null" : rom.MD5) + "\","
							+ "\"" + (rom.SHA1 == "" ? "null" : rom.SHA1) + "\","
							+ "1)";
						SqliteCommand sslc = new SqliteCommand(squery, dbc);
						sslc.ExecuteNonQuery();
						sslc.Dispose();

						// Add the rom to the files that need to be rebuilt
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

			// Create the sorting object to use and rebuild the needed files
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(0, 0, 0, 0);
			SimpleSort ss = new SimpleSort(need, onlyDirs, _depots.Keys.ToList()[0], _tmpdir, false, false, false, false, true, true, asl, false, _logger);
			ss.StartProcessing();
		}

		/// <summary>
		/// Wrap building all files from a set of DATs
		/// </summary>
		/// <param name="inputs"></param>
		private static void InitBuild(List<string> inputs)
		{
			// Verify the filenames
			Dictionary<string, string> foundDats = GetValidDats(inputs);

			// Create a base output folder
			if (!Directory.Exists("out"))
			{
				Directory.CreateDirectory("out");
			}

			// Open the database
			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Now that we have the dictionary, we can loop through and output to a new folder for each
			foreach (string key in foundDats.Keys)
			{
				// Get the DAT file associated with the key
				DatFile datFile = new DatFile();
				datFile.Parse(Path.Combine(_dats, foundDats[key]), 0, 0, _logger, softlist: true);
				ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(0, 0, 0, 0);

				// Create the new output directory if it doesn't exist
				string outputFolder = Path.Combine("out", Path.GetFileNameWithoutExtension(foundDats[key]));
				if (!Directory.Exists(outputFolder))
				{
					Directory.CreateDirectory(outputFolder);
				}

				// Then get all hashes associated with this DAT
				string query = "SELECT sha1 FROM dats JOIN data ON dats.id=data.id WHERE hash=\"" + key + "\"";
				SqliteCommand slc = new SqliteCommand(query, dbc);
				SqliteDataReader sldr = slc.ExecuteReader();
				if (sldr.HasRows)
				{
					while (sldr.Read())
					{
						string sha1 = sldr.GetString(0);
						string filename = Path.Combine(sha1.Substring(0, 2), sha1.Substring(2, 2), sha1.Substring(4, 2), sha1.Substring(6, 2), sha1 + ".gz");

						// Find the first depot that contains the folder
						foreach (string depot in _depots.Keys)
						{
							// If the depot is online, check it
							if (_depots[depot].Item2)
							{
								if (File.Exists(Path.Combine(depot, filename)))
								{
									ArchiveTools.ExtractArchive(Path.Combine(depot, filename), _tmpdir, asl, _logger);
									continue;
								}
							}
						}
					}
				}

				// Now that we have extracted everything, we rebuild to the output folder
				List<string> temp = new List<string>();
				temp.Add(_tmpdir);
				SimpleSort ss = new SimpleSort(datFile, temp, outputFolder, "", false, false, false, true, false, false, asl, false, _logger);
				ss.StartProcessing();
			}

			dbc.Dispose();
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
				OutputFormat = OutputFormat.Logiqx,
				Files = new SortedDictionary<string, List<DatItem>>(),
			};

			Logger logger = new Logger(false, "");
			foreach (string input in inputs)
			{
				datdata.PopulateDatFromDir(input, false /* noMD5 */, false /* noSHA1 */, true /* bare */, false /* archivesAsFiles */,
					true /* enableGzip */, false /* addBlanks */, false /* addDate */, _tmpdir /* tempDir */, false /* copyFiles */,
					null /* headerToCheckAgainst */, 4 /* maxDegreeOfParallelism */, _logger);
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
				string query = "SELECT * FROM data WHERE crc=\"" + input + "\"";
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
				string query = "SELECT * FROM data WHERE md5=\"" + input + "\"";
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
				string query = "SELECT * FROM data WHERE sha1=\"" + input + "\"";
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
