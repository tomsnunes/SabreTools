using System;
using System.Collections.Generic;

using SabreTools.Helper;
using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the RombaSharp application
	/// </summary>
	public partial class RombaSharp
	{
		// General settings
		private static int _workers;		//Number of parallel threads
		private static string _logdir;		//Log folder location
		private static string _tmpdir;		//Temp folder location
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
		private static Logger _logger;

		/// <summary>
		/// Entry class for the RombaSharp application
		/// </summary>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			_logger = new Logger(true, "romba.log");
			InitializeConfiguration();
			DatabaseTools.EnsureDatabase(_dbSchema, _db, _connectionString);

			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Help("Credits");
				_logger.Close();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help("RombaSharp");
				_logger.Close();
				return;
			}

			// Feature flags
			bool help = false,
				archive = false,
				build = false,
				dbstats = false,
				depotRescan = false,
				diffdat = false,
				dir2dat = false,
				export = false,
				fixdat = false,
				lookup = false,
				memstats = false,
				miss = false,
				progress = false,
				purgeBackup = false,
				purgeDelete = false,
				refreshDats = false,
				shutdown = false;

			// User flags
			bool copy = false,
				onlyNeeded = false;

			// User inputs
			string newdat =String.Empty,
				outdat = String.Empty;
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
						help = true;
						break;
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
					case "lookup":
						lookup = true;
						break;
					case "memstats":
						memstats = true;
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
						copy = true;
						break;
					case "-only-needed":
					case "--only-needed":
						onlyNeeded = true;
						break;

					// User inputs
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
						string temparg = args[i].Replace(""", String.Empty).Replace("file://", String.Empty);

						if (temparg.StartsWith("-") && temparg.Contains("="))
						{
							// Split the argument
							string[] split = temparg.Split('=');
							if (split[1] == null)
							{
								split[1] = String.Empty;
							}

							switch (split[0])
							{
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

			// If help is set, show the help screen
			if (help)
			{
				Build.Help("RombaSharp");
				_logger.Close();
				return;
			}

			// If more than one switch is enabled, show the help screen
			if (!(archive ^ build ^ dbstats ^ depotRescan ^ diffdat ^ dir2dat ^ export ^ fixdat ^ lookup ^
				memstats ^ miss ^ progress ^ purgeBackup ^ purgeDelete ^ refreshDats ^ shutdown))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help("RombaSharp");
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (archive || build || depotRescan || dir2dat || fixdat || lookup || miss))
			{
				_logger.Error("This feature requires at least one input");
				Build.Help("RombaSharp");
				_logger.Close();
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

			// For each specified DAT file it creates a miss file and a have file
			else if (miss)
			{
				InitMiss(inputs);
			}

			// Shows progress of the currently running command
			else if (progress)
			{
				_logger.User("This feature is not used in RombaSharp: progress");
			}

			// Moves DAT index entries for orphaned DATs
			else if (purgeBackup)
			{
				PurgeBackup();
			}

			// Deletes DAT index entries for orphaned DATs
			else if (purgeDelete)
			{
				PurgeDelete();
			}

			// Refreshes the DAT index from the files in the DAT master directory tree
			else if (refreshDats)
			{
				RefreshDatabase();
			}

			// Gracefully shuts down server
			else if (shutdown)
			{
				_logger.User("This feature is not used in RombaSharp: shutdown");
			}

			// If nothing is set, show the help
			else
			{
				Build.Help("RombaSharp");
			}

			_logger.Close();
			return;
		}
	}
}
