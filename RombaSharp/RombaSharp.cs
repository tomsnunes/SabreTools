using System;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

namespace RombaSharp
{
	/// <summary>
	/// Entry class for the RombaSharp application
	/// </summary>
	/// <remarks>
	/// In the database, we want to enable "offline mode". That is, when a user does an operation
	/// that needs to read from the depot themselves, if the depot folder cannot be found, the
	/// user is prompted to reconnect the depot OR skip that depot entirely.
	/// </remarks>
	public partial class RombaSharp
	{
		// General settings
		private static string _logdir;		// Log folder location
		private static string _tmpdir;		// Temp folder location
		private static string _webdir;		// Web frontend location
		private static string _baddir;		// Fail-to-unpack file folder location
		private static int _verbosity;		// Verbosity of the output
		private static int _cores;			// Forced CPU cores

		// DatRoot settings
		private static string _dats;		// DatRoot folder location
		private static string _db;			// Database name

		// Depot settings
		private static Dictionary<string, Tuple<long, bool>> _depots; // Folder location, Max size

		// Server settings
		private static int _port;			// Web server port

		// Other private variables
		private static string _config = "config.xml";
		private static string _dbSchema = "rombasharp";
		private static string _connectionString;
		private static Help _help;

		/// <summary>
		/// Entry class for the RombaSharp application
		/// </summary>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			Globals.Logger = new Logger(true, "romba.log");

			InitializeConfiguration();
			DatabaseTools.EnsureDatabase(_dbSchema, _db, _connectionString);

			// Create a new Help object for this program
			_help = RetrieveHelp();

			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				_help.OutputCredits();
				Globals.Logger.Close();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				_help.OutputGenericHelp();
				Globals.Logger.Close();
				return;
			}

			// Feature flags
			bool archive = false,
				build = false,
				dbstats = false,
				depotRescan = false,
				diffdat = false,
				dir2dat = false,
				export = false,
				fixdat = false,
				import = false,
				lookup = false,
				memstats = false,
				merge = false,
				miss = false,
				progress = false,
				purgeBackup = false,
				purgeDelete = false,
				refreshDats = false,
				shutdown = false;

			// User flags
			bool copy = false,
				logOnly = false,
				onlyNeeded = false;

			// User inputs
			string depotPath = "",
				newdat = "",
				outdat = "";
			List<string> inputs = new List<string>();

			// Get the first argument as a feature flag
			string feature = args[0];

			// Verify that the flag is valid
			if (!_help.TopLevelFlag(feature))
			{
				Globals.Logger.User("'{0}' is not valid feature flag", feature);
				_help.OutputIndividualFeature(feature);
				Globals.Logger.Close();
				return;
			}

			// Now get the proper name for the feature
			feature = _help.GetFeatureName(feature);

			// If we had the help feature first
			if (feature == "Help")
			{
				// If we had something else after help
				if (args.Length > 1)
				{
					_help.OutputIndividualFeature(args[1]);
					Globals.Logger.Close();
					return;
				}
				// Otherwise, show generic help
				else
				{
					_help.OutputGenericHelp();
					Globals.Logger.Close();
					return;
				}
			}

			// Now verify that all other flags are valid
			for (int i = 1; i < args.Length; i++)
			{
				// Verify that the current flag is proper for the feature
				if (!_help[feature].ValidateInput(args[i]))
				{
					Globals.Logger.Error("Invalid input detected: {0}", args[i]);
					_help.OutputIndividualFeature(feature);
					Globals.Logger.Close();
					return;
				}

				// Special precautions for files and directories
				if (File.Exists(args[i]) || Directory.Exists(args[i]))
				{
					inputs.Add(args[i]);
				}
			}

			// Now loop through all inputs
			Dictionary<string, Feature> features = _help.GetEnabledFeatures();
			foreach (KeyValuePair<string, Feature> feat in features)
			{
				// Check all of the flag names and translate to arguments
				switch (feat.Key)
				{
					// User flags
					case "copy":
						copy = true;
						break;
					case "log-only":
						logOnly = true;
						break;
					case "only-needed":
						onlyNeeded = true;
						break;

					// User inputs
					case "depot":
						depotPath = (string)feat.Value.GetValue();
						break;
					case "new":
						newdat = (string)feat.Value.GetValue();
						break;
					case "out":
						outdat = (string)feat.Value.GetValue();
						break;
				}
			}

			// Now take care of each mode in succesion
			switch(feature)
			{
				case "Help":
					// No-op as this should be caught
					break;
				// Adds ROM files from the specified directories to the ROM archive
				case "Archive":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitArchive(inputs, onlyNeeded);
					break;
				// For each specified DAT file it creates the torrentzip files
				case "Build":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitBuild(inputs, copy);
					break;
				// Prints db stats
				case "Stats":
					DisplayDBStats();
					break;
				// Rescan a specific depot
				case "Rescan Depots":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					foreach (string input in inputs)
					{
						Rescan(input);
					}
					break;
				// Creates a DAT file with those entries that are in new DAT
				case "Diffdat":
					InitDiffDat(newdat);
					break;
				// Creates a DAT file for the specified input directory
				case "Dir2Dat":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitDir2Dat(inputs);
					break;
				// Export the database to file
				case "Export":
					ExportDatabase();
					break;
				// For each specified DAT file it creates a fix DAT
				case "Fixdat":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitFixdat(inputs);
					break;
				// Import a CSV into the database
				case "Import":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitImport(inputs);
					break;
				// For each specified hash it looks up any available information
				case "Lookup":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitLookup(inputs);
					break;
				// Prints memory stats
				case "Memstats":
					DisplayMemoryStats();
					break;
				// Merges depots
				case "Merge":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitMerge(inputs, depotPath, onlyNeeded);
					break;
				// For each specified DAT file it creates a miss file and a have file
				case "Miss":
					if (inputs.Count == 0)
					{
						Globals.Logger.Error("This feature requires at least one input");
						_help.OutputIndividualFeature(feature);
						break;
					}
					InitMiss(inputs);
					break;
				// Moves DAT index entries for orphaned DATs
				case "Purge Backup":
					PurgeBackup(logOnly);
					break;
				// Deletes DAT index entries for orphaned DATs
				case "Purge Delete":
					PurgeDelete(logOnly);
					break;
				// Refreshes the DAT index from the files in the DAT master directory tree
				case "Refresh DATs":
					RefreshDatabase();
					break;
				// Shows progress of the currently running command
				case "Progress":
					Globals.Logger.User("This feature is not used in RombaSharp: progress");
					break;
				case "Shutdown":
					Globals.Logger.User("This feature is not used in RombaSharp: shutdown");
					break;
				default:
					_help.OutputGenericHelp();
					break;
			}

			Globals.Logger.Close();
			return;
		}
	}
}
