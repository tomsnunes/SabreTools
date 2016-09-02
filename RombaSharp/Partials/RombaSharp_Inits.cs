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
		}

		/// <summary>
		/// Wrap building all files from a set of DATs
		/// </summary>
		/// <param name="inputs"></param>
		private static void InitBuild(List<string> inputs)
		{
			_logger.User("This feature is not yet implemented: build");
		}

		/// <summary>
		/// Wrap finding all files that are in both the database and a new Dat
		/// </summary>
		/// <param name="newdat"></param>
		private static void InitDiffDat(string newdat)
		{
			_logger.User("This feature is not yet implemented: diffdat");
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
		}

		#endregion
	}
}
