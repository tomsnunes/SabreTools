using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Helper;

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
		private static string _tmpdir;	//Temp folder location
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
			DBTools.EnsureDatabase(_dbSchema, _db, _connectionString);

			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				_logger.Close();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// Set all default values
			bool help = false,
				archive = false,
				build = false,
				dbstats = false,
				diffdat = false,
				dir2dat = false,
				fixdat = false,
				lookup = false,
				memstats = false,
				miss = false,
				onlyNeeded = false,
				progress = false,
				purgeBackup = false,
				purgeDelete = false,
				refreshDats = false,
				shutdown = false;
			string newdat ="",
				outdat = "";
			List<string> inputs = new List<string>();

			// Determine which switches are enabled (with values if necessary)
			foreach (string arg in args)
			{
				switch (arg)
				{
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
					case "diffdat":
						diffdat = true;
						break;
					case "dir2dat":
						dir2dat = true;
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
					case "-only-needed":
					case "--only-needed":
						onlyNeeded = true;
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
					default:
						string temparg = arg.Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-new=") || temparg.StartsWith("--new="))
						{
							newdat = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-out=") || temparg.StartsWith("--out="))
						{
							outdat = temparg.Split('=')[1];
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
						}
						else
						{
							_logger.Error("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							Console.WriteLine();
							_logger.Error("Invalid input detected: " + arg);
							_logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// If more than one switch is enabled, show the help screen
			if (!(archive ^ build ^ dbstats ^ diffdat ^ dir2dat ^ fixdat ^ lookup ^ memstats ^ miss ^
				progress ^ purgeBackup ^ purgeDelete ^ refreshDats ^ shutdown))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help();
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (archive || build || dir2dat || fixdat || lookup || miss))
			{
				_logger.Error("This feature requires at least one input");
				Build.Help();
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
				InitBuild(inputs);
			}

			// Prints db stats
			else if (dbstats)
			{
				DisplayDBStats();
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
				Build.Help();
			}

			_logger.Close();
			return;
		}
	}
}
