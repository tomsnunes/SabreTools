using SabreTools.Helper;
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
			Dat datdata = new Dat()
			{
				FileName = Path.GetFileName(inputs[0]) + " Dir2Dat",
				Name = Path.GetFileName(inputs[0]) + " Dir2Dat",
				Description = Path.GetFileName(inputs[0]) + " Dir2Dat",
				OutputFormat = OutputFormat.Xml,
				Files = new Dictionary<string, List<Rom>>(),
			};

			DATFromDir dfd = new DATFromDir(inputs, datdata, false, false, true, false, true, "__temp__", _logger);
			dfd.Start();
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
		}

		/// <summary>
		/// Wrap looking up if hashes exist in the database
		/// </summary>
		/// <param name="inputs"></param>
		private static void InitLookup(List<string> inputs)
		{
			_logger.User("This feature is not yet implemented: lookup");
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
		}

		#endregion
	}
}
