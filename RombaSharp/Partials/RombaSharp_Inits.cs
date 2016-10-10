using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools
{
	public partial class RombaSharp
	{
		#region Init Methods

		/// <summary>
		/// Wrap adding files to the depots
		/// </summary>
		/// <param name="inputs"></param>
		/// <param name="onlyNeeded"></param>
		private static void InitArchive(List<string> inputs, bool onlyNeeded)
		{
			_logger.User("This feature is not yet implemented: archive");

			// This should use the same thing as something like in SimpleSort, as in scan the archive and scan the files inside equally
			// Either during or after, we then check against the database to see if there's any matches. If there's no match and
			// we say onlyNeeded, then don't add it. If there's no match and not onlyNeeded, add it with no DAT hash. If there's a match,
			// and it doesn't say exists, add it and change the flag. If there's a match and says exists, skip it.
		}

		/// <summary>
		/// Wrap building all files from a set of DATs
		/// </summary>
		/// <param name="inputs"></param>
		private static void InitBuild(List<string> inputs)
		{
			_logger.User("This feature is not yet implemented: build");

			// Verify the filenames
			Dictionary<string, string> foundDats = GetValidDats(inputs);

			// Now that we have the dictionary, we can loop through and output to a new folder for each

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
					true /* enableGzip */, false /* addBlanks */, false /* addDate */, "__temp__" /* tempDir */, false /* copyFiles */,
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
		/// <param name="inputs"></param>
		private static void InitLookup(List<string> inputs)
		{
			// First, try to figure out what type of hash each is by length and clean it
			List<string> cleanedInputs = new List<string>();
			foreach (string input in inputs)
			{
				string temp = "";
				if (input.Length == Constants.CRCLength)
				{
					temp = Style.CleanHashData(input, Constants.CRCLength);
				}
				else if (input.Length == Constants.MD5Length)
				{
					temp = Style.CleanHashData(input, Constants.MD5Length);
				}
				else if (input.Length == Constants.SHA1Length)
				{
					temp = Style.CleanHashData(input, Constants.SHA1Length);
				}

				// If the hash isn't empty, add it
				if (temp != "")
				{
					cleanedInputs.Add(temp);
				}
			}

			// Now, search for each of them and return true or false for each
			SqliteConnection dbc = new SqliteConnection(_connectionString);

			foreach (string input in cleanedInputs)
			{
				string query = "SELECT * FROM data WHERE value=\"" + input + "\"";
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
