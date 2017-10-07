using System;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

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

			// Determine which switches are enabled (with values if necessary)
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					// Feature flags
					case "-?":
					case "-h":
					case "--help":
						if (i + 1 < args.Length)
						{
							_help.OutputIndividualFeature(args[i + 1]);
						}
						else
						{
							_help.OutputGenericHelp();
						}
						return;
					case "archive":
						archive = true;
						break;
					case "build":
						build = true;
						break;
					case "dbstats":
						dbstats = true;
						break;
					case "depot-rescan":
						depotRescan = true;
						break;
					case "diffdat":
						diffdat = true;
						break;
					case "dir2dat":
						dir2dat = true;
						break;
					case "export":
						export = true;
						break;
					case "fixdat":
						fixdat = true;
						break;
					case "import":
						import = true;
						break;
					case "lookup":
						lookup = true;
						break;
					case "memstats":
						memstats = true;
						break;
					case "merge":
						merge = true;
						break;
					case "miss":
						miss = true;
						break;
					case "purge-backup":
						purgeBackup = true;
						break;
					case "purge-delete":
						purgeDelete = true;
						break;
					case "progress":
						progress = true;
						break;
					case "refresh-dats":
						refreshDats = true;
						break;
					case "shutdown":
						shutdown = true;
						break;

					// User flags
					case "-copy":
					case "--copy":
						copy = true;
						break;
					case "-log-only":
					case "--log-only":
						logOnly = true;
						break;
					case "-only-needed":
					case "--only-needed":
						onlyNeeded = true;
						break;

					// User inputs
					case "-depot":
					case "--depot":
						i++;
						depotPath = args[i];
						break;
					case "-new":
					case "--new":
						i++;
						newdat = args[i];
						break;
					case "-out":
					case "--out":
						i++;
						outdat = args[i];
						break;
					default:
						string temparg = args[i].Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-") && temparg.Contains("="))
						{
							// Split the argument
							string[] split = temparg.Split('=');
							if (split[1] == null)
							{
								split[1] = "";
							}

							switch (split[0])
							{
								case "-depot":
								case "--depot":
									i++;
									depotPath = split[1];
									break;
								case "-new":
								case "--new":
									newdat = split[1];
									break;
								case "-out":
								case "--out":
									outdat = split[i];
									break;
								default:
									inputs.Add(temparg);
									break;
							}
						}
						else
						{
							inputs.Add(temparg);
						}
						break;
				}
			}

			// If more than one switch is enabled, show the help screen
			if (!(archive ^ build ^ dbstats ^ depotRescan ^ diffdat ^ dir2dat ^ export ^ fixdat ^ import ^ lookup ^
				memstats ^ merge ^ miss ^ progress ^ purgeBackup ^ purgeDelete ^ refreshDats ^ shutdown))
			{
				Globals.Logger.Error("Only one feature switch is allowed at a time");
				_help.OutputGenericHelp();
				Globals.Logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (archive || build || depotRescan || dir2dat || fixdat || 
				import || lookup || merge || miss))
			{
				Globals.Logger.Error("This feature requires at least one input");
				_help.OutputGenericHelp();
				Globals.Logger.Close();
				return;
			}

			// Now take care of each mode in succesion

			// Adds ROM files from the specified directories to the ROM archive
			if (archive)
			{
				InitArchive(inputs, onlyNeeded);
			}

			// For each specified DAT file it creates the torrentzip files
			else if (build)
			{
				InitBuild(inputs, copy);
			}

			// Prints db stats
			else if (dbstats)
			{
				DisplayDBStats();
			}

			// Rescan a specific depot
			else if (depotRescan)
			{
				foreach (string input in inputs)
				{
					Rescan(input);
				}
			}

			// Creates a DAT file with those entries that are in new DAT
			else if (diffdat)
			{
				InitDiffDat(newdat);
			}

			// Creates a DAT file for the specified input directory
			else if (dir2dat)
			{
				InitDir2Dat(inputs);
			}

			// Export the database to file
			else if (export)
			{
				ExportDatabase();
			}

			// For each specified DAT file it creates a fix DAT
			else if (fixdat)
			{
				InitFixdat(inputs);
			}

			// Import a CSV into the database
			else if (import)
			{
				InitImport(inputs);
			}

			// For each specified hash it looks up any available information
			else if (lookup)
			{
				InitLookup(inputs);
			}

			// Prints memory stats
			else if (memstats)
			{
				DisplayMemoryStats();
			}

			// Merges depots
			else if (merge)
			{
				InitMerge(inputs, depotPath, onlyNeeded);
			}

			// For each specified DAT file it creates a miss file and a have file
			else if (miss)
			{
				InitMiss(inputs);
			}

			// Shows progress of the currently running command
			else if (progress)
			{
				Globals.Logger.User("This feature is not used in RombaSharp: progress");
			}

			// Moves DAT index entries for orphaned DATs
			else if (purgeBackup)
			{
				PurgeBackup(logOnly);
			}

			// Deletes DAT index entries for orphaned DATs
			else if (purgeDelete)
			{
				PurgeDelete(logOnly);
			}

			// Refreshes the DAT index from the files in the DAT master directory tree
			else if (refreshDats)
			{
				RefreshDatabase();
			}

			// Gracefully shuts down server
			else if (shutdown)
			{
				Globals.Logger.User("This feature is not used in RombaSharp: shutdown");
			}

			// If nothing is set, show the help
			else
			{
				_help.OutputGenericHelp();
			}

			Globals.Logger.Close();
			return;
		}
	}
}
