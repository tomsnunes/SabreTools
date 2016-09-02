using SabreTools.Helper;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools
{
	public class SimpleSort
	{
		// Private instance variables
		private Dat _datdata;
		private List<string> _inputs;
		private string _outdir;
		private string _tempdir;
		private bool _quickScan;
		private bool _toFolder;
		private bool _verify;
		private bool _tgz;
		private bool _romba;
		private bool _updateDat;
		private ArchiveScanLevel _7z;
		private ArchiveScanLevel _gz;
		private ArchiveScanLevel _rar;
		private ArchiveScanLevel _zip;
		private Logger _logger;

		// Other private variables
		private int _cursorTop;
		private int _cursorLeft;
		private Dat _matched;

		/// <summary>
		/// Create a new SimpleSort object
		/// </summary>
		/// <param name="datdata">Name of the DAT to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outdir">Output directory to use to build to</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="verify">True if output directory should be checked instead of rebuilt to, false otherwise</param>
		/// <param name="tgz">True if files should be output in TorrentGZ format, false for standard zip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		public SimpleSort(Dat datdata, List<string> inputs, string outdir, string tempdir,
			bool quickScan, bool toFolder, bool verify, bool tgz, bool romba, int sevenzip,
			int gz, int rar, int zip, bool updateDat, Logger logger)
		{
			_datdata = datdata;
			_inputs = inputs;
			_outdir = (outdir == "" ? "Rebuild" : outdir);
			_tempdir = (tempdir == "" ? "__TEMP__" : tempdir);
			_quickScan = quickScan;
			_toFolder = toFolder;
			_verify = verify;
			_tgz = tgz;
			_romba = romba;
			_7z = (ArchiveScanLevel)(sevenzip < 0 || sevenzip > 2 ? 0 : sevenzip);
			_gz = (ArchiveScanLevel)(gz < 0 || gz > 2 ? 0 : gz);
			_rar = (ArchiveScanLevel)(rar < 0 || rar > 2 ? 0 : rar);
			_zip = (ArchiveScanLevel)(zip < 0 || zip > 2 ? 0 : zip);
			_updateDat = updateDat;
			_logger = logger;

			_cursorTop = Console.CursorTop;
			_cursorLeft = Console.CursorLeft;
			_matched = new Dat
			{
				Files = new Dictionary<string, List<Rom>>(),
			};
		}

		/// <summary>
		/// Main entry point for the program
		/// </summary>
		/// <param name="args">List of arguments to be parsed</param>
		public static void Main(string[] args)
		{
			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Perform initial setup and verification
			Logger logger = new Logger(true, "simplesort.log");
			logger.Start();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("SimpleSort");

			// Set all default values
			bool help = false,
				quickScan = false,
				romba = false,
				simpleSort = true,
				tgz = false,
				toFolder = false,
				updateDat = false,
				verify = false;
			int sevenzip = 0,
				gz = 2,
				rar = 2,
				zip = 0;
			string outdir = "",
				tempdir = "";
			List<string> inputs = new List<string>();
			List<string> datfiles = new List<string>();

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
					case "-do":
					case "--directory":
						toFolder = true;
						break;
					case "-qs":
					case "--quick":
						quickScan = true;
						break;
					case "-r":
					case "--romba":
						romba = true;
						break;
					case "-tgz":
					case "--tgz":
						tgz = true;
						break;
					case "-ud":
					case "--updated-dat":
						updateDat = true;
						break;
					case "-v":
					case "--verify":
						verify = true;
						break;
					default:
						string temparg = arg.Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-7z=") || temparg.StartsWith("--7z="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out sevenzip))
							{
								sevenzip = 0;
							}
						}
						else if (temparg.StartsWith("-dat=") || temparg.StartsWith("--dat="))
						{
							string datfile = temparg.Split('=')[1];
							if (!File.Exists(datfile))
							{
								logger.Error("DAT must be a valid file: " + datfile);
								Console.WriteLine();
								Build.Help();
								logger.Close();
								return;
							}
							datfiles.Add(datfile);
						}
						else if (temparg.StartsWith("-gz=") || temparg.StartsWith("--gz="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out gz))
							{
								gz = 2;
							}
						}
						else if (temparg.StartsWith("-out=") || temparg.StartsWith("--out="))
						{
							outdir = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-rar=") || temparg.StartsWith("--rar="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out rar))
							{
								rar = 2;
							}
						}
						else if (temparg.StartsWith("-t=") || temparg.StartsWith("--temp="))
						{
							tempdir = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-zip=") || temparg.StartsWith("--zip="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out zip))
							{
								zip = 0;
							}
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
						}
						else
						{
							logger.Error("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							Console.WriteLine();
							logger.Error("Invalid input detected: " + arg);
							logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && (simpleSort && !verify))
			{
				logger.Error("This feature requires at least one input");
				Build.Help();
				logger.Close();
				return;
			}

			// If we are doing a simple sort
			if (simpleSort)
			{
				if (datfiles.Count > 0)
				{
					InitSimpleSort(datfiles, inputs, outdir, tempdir, quickScan, toFolder, verify, tgz, romba, sevenzip, gz, rar, zip, updateDat, logger);
				}
				else
				{
					logger.Error("A datfile is required to use this feature");
					Build.Help();
					logger.Close();
					return;
				}
			}

			// If nothing is set, show the help
			else
			{
				Build.Help();
			}

			logger.Close();
			return;
		}

		/// <summary>
		/// Wrap sorting files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outdir">Output directory to use to build to</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="verify">True if output directory should be checked instead of rebuilt to, false otherwise</param>
		/// <param name="tgz">True if files should be output in TorrentGZ format, false for standard zip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitSimpleSort(List<string> datfiles, List<string> inputs, string outdir, string tempdir, bool quickScan,
			bool toFolder, bool verify, bool tgz, bool romba, int sevenzip, int gz, int rar, int zip, bool updateDat, Logger logger)
		{
			// Add all of the input DATs into one huge internal DAT
			Dat datdata = new Dat();
			foreach (string datfile in datfiles)
			{
				datdata = DatTools.Parse(datfile, 99, 99, datdata, logger);
			}

			SimpleSort ss = new SimpleSort(datdata, inputs, outdir, tempdir, quickScan, toFolder, verify, tgz, romba, sevenzip, gz, rar, zip, updateDat, logger);
			ss.StartProcessing();
		}

		/// <summary>
		/// Pick the appropriate action based on the inputs
		/// </summary>
		/// <returns>True if success, false otherwise</returns>
		public bool StartProcessing()
		{
			// First, check that the output directory exists
			if (!Directory.Exists(_outdir))
			{
				Directory.CreateDirectory(_outdir);
				_outdir = Path.GetFullPath(_outdir);
			}

			// Then create or clean the temp directory
			if (!Directory.Exists(_tempdir))
			{
				Directory.CreateDirectory(_tempdir);
			}
			else
			{
				FileTools.CleanDirectory(_tempdir);
			}

			if (_verify)
			{
				return VerifyDirectory();
			}
			else
			{
				return RebuildToOutput();
			}
		}

		/// <summary>
		/// Process the DAT and verify the output directory
		/// </summary>
		/// <returns>True if verification was a success, false otherwise</returns>
		public bool VerifyDirectory()
		{
			bool success = true;

			// Enumerate all files from the output directory
			List<string> files = new List<string>();
			foreach (string file in Directory.EnumerateFiles(_outdir, "*", SearchOption.AllDirectories))
			{
				_logger.Log("File found: '" + file + "'");
				files.Add(Path.GetFullPath(file));
			}

			/*
			We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
			*/

			// Then, loop through and check each of the inputs
			_logger.User("Processing files:\n");
			DATFromDir dfd = new DATFromDir(files, _datdata, false, false, false, false, true, "", _logger, true);
			dfd.Start();

			// Setup the fixdat
			_matched = (Dat)_datdata.CloneHeader();
			_matched.Files = new Dictionary<string, List<Rom>>();
			_matched.FileName = "fixDat_" + _matched.FileName;
			_matched.Name = "fixDat_" + _matched.Name;
			_matched.Description = "fixDat_" + _matched.Description;
			_matched.OutputFormat = OutputFormat.Xml;

			// Now that all files are parsed, get only files found in directory
			bool found = false;
			foreach (List<Rom> roms in _datdata.Files.Values)
			{
				List<Rom> newroms = RomTools.Merge(roms, _logger);
				foreach (Rom rom in newroms)
				{
					if (rom.Metadata.SourceID == 99)
					{
						found = true;
						string key = rom.HashData.Size + "-" + rom.HashData.CRC;
						if (_matched.Files.ContainsKey(key))
						{
							_matched.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							_matched.Files.Add(key, temp);
						}
					}
				}
			}

			// Now output the fixdat to the main folder
			if (found)
			{
				DatTools.WriteDatfile(_matched, "", _logger, stats: true);
			}
			else
			{
				_logger.User("No fixDat needed");
			}

			return success;
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildToOutput()
		{
			bool success = true;

			// Create a list of just files from inputs
			List<string> files = new List<string>();
			foreach (string input in _inputs)
			{
				if (File.Exists(input))
				{
					_logger.Log("File found: '" + input + "'");
					files.Add(Path.GetFullPath(input));
				}
				else if (Directory.Exists(input))
				{
					_logger.Log("Directory found: '" + input + "'");
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						_logger.Log("File found: '" + file + "'");
						files.Add(Path.GetFullPath(file));
					}
				}
				else
				{
					_logger.Error("'" + input + "' is not a file or directory!");
				}
			}

			// Then, loop through and check each of the inputs
			_logger.User("Processing files:\n");
			_cursorTop = Console.CursorTop;
			for (int i = 0; i < files.Count; i++)
			{
				success &= RebuildToOutputHelper(files[i], i, files.Count);
				FileTools.CleanDirectory(_tempdir);
			}

			// Now one final delete of the temp directory
			while (Directory.Exists(_tempdir))
			{
				try
				{
					Directory.Delete(_tempdir, true);
				}
				catch
				{
					continue;
				}
			}

			// Now output the stats for the built files
			_logger.ClearBeneath(Constants.HeaderHeight);
			Console.SetCursorPosition(0, Constants.HeaderHeight + 1);
			_logger.User("Stats of the matched ROMs:");
			Stats.OutputStats(_matched, _logger, true);

			// Now output the fixdat based on the original input if asked
			if (_updateDat)
			{
				_datdata.FileName = "fixDat_" + _datdata.FileName;
				_datdata.Name = "fixDat_" + _datdata.Name;
				_datdata.Description = "fixDat_" + _datdata.Description;
				_datdata.OutputFormat = OutputFormat.Xml;
				DatTools.WriteDatfile(_datdata, "", _logger);
			}

			return success;
		}

		/// <summary>
		/// Process an individual file against the DAT for rebuilding
		/// </summary>
		/// <param name="input">Name of the input file</param>
		/// <param name="index">Index of the current file</param>
		/// <param name="total">Total number of files</param>
		/// <param name="recurse">True if this is in a recurse step and the file should be deleted, false otherwise (default)</param>
		/// <returns>True if it was processed properly, false otherwise</returns>
		private bool RebuildToOutputHelper(string input, int index, int total, bool recurse = false)
		{
			bool success = true;

			// Get the full path of the input for movement purposes
			string percentage = (index == 0 ? "0.00" : Math.Round((100 * ((double)index / total)), 2, MidpointRounding.AwayFromZero).ToString());
			string statement = percentage + "% - " + input;
			_logger.ClearBeneath(_cursorTop + 1);
			_logger.Log(statement, _cursorTop, 0);

			// Get if the file should be scanned internally and externally
			bool shouldExternalScan, shouldInternalScan;
			FileTools.GetInternalExternalProcess(input, _7z, _gz, _rar, _zip, _logger, out shouldExternalScan, out shouldInternalScan);

			// Hash and match the external files
			if (shouldExternalScan)
			{
				Rom rom = FileTools.GetSingleFileInfo(input);

				// If we have a blank RomData, it's an error
				if (rom.Name == null)
				{
					return false;
				}

				// Try to find the matches to the file that was found
				List<Rom> foundroms = RomTools.GetDuplicates(rom, _datdata, _logger, true);
				_logger.Log("File '" + input + "' had " + foundroms.Count + " matches in the DAT!");
				foreach (Rom found in foundroms)
				{
					_logger.Log("Matched name: " + found.Name);

					// Add rom to the matched list
					string key = found.HashData.Size + "-" + found.HashData.CRC;
					if(_matched.Files.ContainsKey(key))
					{
						_matched.Files[key].Add(found);
					}
					else
					{
						List<Rom> temp = new List<Rom>();
						temp.Add(found);
						_matched.Files.Add(key, temp);
					}

					if (_toFolder)
					{
						// Copy file to output directory
						string gamedir = Path.Combine(_outdir, found.Machine.Name);
						if (!Directory.Exists(gamedir))
						{
							Directory.CreateDirectory(gamedir);
						}

						_logger.Log("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (_tgz ? found.HashData.SHA1 : found.Name) + "'");
						try
						{
							File.Copy(input, Path.Combine(gamedir, Path.GetFileName(found.Name)));
						}
						catch { }
					}
					else
					{
						if (_tgz)
						{
							FileTools.WriteTorrentGZ(input, _outdir, _romba, _logger);
						}
						else
						{
							FileTools.WriteToManagedArchive(input, _outdir, found);
						}
					}
				}

				// Now get the transformed file if it exists
				SkipperRule rule = Skippers.MatchesSkipper(input, "", _logger);

				// If we have have a non-empty rule, apply it
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Otherwise, apply the rule ot the file
					string newinput = input + ".new";
					Skippers.TransformFile(input, newinput, rule, _logger);
					Rom drom = FileTools.GetSingleFileInfo(newinput);

					// If we have a blank RomData, it's an error
					if (drom.Name == null)
					{
						return false;
					}

					// Try to find the matches to the file that was found
					List<Rom> founddroms = RomTools.GetDuplicates(drom, _datdata, _logger, true);
					_logger.Log("File '" + newinput + "' had " + founddroms.Count + " matches in the DAT!");
					foreach (Rom found in founddroms)
					{
						// Add rom to the matched list
						string key = found.HashData.Size + "-" + found.HashData.CRC;
						if (_matched.Files.ContainsKey(key))
						{
							_matched.Files[key].Add(found);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(found);
							_matched.Files.Add(key, temp);
						}

						// First output the headerless rom
						_logger.Log("Matched name: " + found.Name);

						if (_toFolder)
						{
							// Copy file to output directory
							string gamedir = Path.Combine(_outdir, found.Machine.Name);
							if (!Directory.Exists(gamedir))
							{
								Directory.CreateDirectory(gamedir);
							}

							_logger.Log("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (_tgz ? found.HashData.SHA1 : found.Name) + "'");
							try
							{
								File.Copy(newinput, Path.Combine(gamedir, Path.GetFileName(found.Name)));
							}
							catch { }
						}
						else
						{
							if (_tgz)
							{
								FileTools.WriteTorrentGZ(newinput, _outdir, _romba, _logger);
							}
							else
							{
								FileTools.WriteToManagedArchive(newinput, _outdir, found);
							}
						}

						// Then output the headered rom (renamed)
						Rom newfound = found;
						newfound.Name = Path.GetFileNameWithoutExtension(newfound.Name) + " (" + rom.HashData.CRC + ")" + Path.GetExtension(newfound.Name);

						// Add rom to the matched list
						key = newfound.HashData.Size + "-" + newfound.HashData.CRC;
						if (_matched.Files.ContainsKey(key))
						{
							_matched.Files[key].Add(newfound);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(newfound);
							_matched.Files.Add(key, temp);
						}

						if (_toFolder)
						{
							// Copy file to output directory
							string gamedir = Path.Combine(_outdir, found.Machine.Name);
							if (!Directory.Exists(gamedir))
							{
								Directory.CreateDirectory(gamedir);
							}

							_logger.Log("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + newfound.Name + "'");
							try
							{
								File.Copy(input, Path.Combine(gamedir, Path.GetFileName(newfound.Name)));
							}
							catch { }
						}
						else
						{
							_logger.Log("Matched name: " + newfound.Name);
							if (_tgz)
							{
								FileTools.WriteTorrentGZ(input, _outdir, _romba, _logger);
							}
							else
							{
								FileTools.WriteToManagedArchive(input, _outdir, newfound);
							}
						}
					}

					// Now remove this temporary file
					try
					{
						File.Delete(newinput);
					}
					catch
					{
						// Don't log file deletion errors
					}
				}
			}

			// If we should scan the file as an archive
			if (shouldInternalScan)
			{
				// If external scanning is enabled, use that method instead
				if (_quickScan)
				{
					_logger.Log("Beginning quick scan of contents from '" + input + "'");
					List<Rom> internalRomData = FileTools.GetArchiveFileInfo(input, _logger);
					_logger.Log(internalRomData.Count + " entries found in '" + input + "'");

					// If the list is populated, then the file was a filled archive
					if (internalRomData.Count > 0)
					{
						foreach (Rom rom in internalRomData)
						{
							// Try to find the matches to the file that was found
							List<Rom> foundroms = RomTools.GetDuplicates(rom, _datdata, _logger, true);
							_logger.Log("File '" + rom.Name + "' had " + foundroms.Count + " matches in the DAT!");
							foreach (Rom found in foundroms)
							{
								// Add rom to the matched list
								string key = found.HashData.Size + "-" + found.HashData.CRC;
								if (_matched.Files.ContainsKey(key))
								{
									_matched.Files[key].Add(found);
								}
								else
								{
									List<Rom> temp = new List<Rom>();
									temp.Add(found);
									_matched.Files.Add(key, temp);
								}

								if (_toFolder)
								{
									// Copy file to output directory
									_logger.Log("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + found.Name + "'");
									string outfile = FileTools.ExtractSingleItemFromArchive(input, rom.Name, _tempdir, _logger);
									if (File.Exists(outfile))
									{
										string gamedir = Path.Combine(_outdir, found.Machine.Name);
										if (!Directory.Exists(gamedir))
										{
											Directory.CreateDirectory(gamedir);
										}

										try
										{
											File.Move(outfile, Path.Combine(gamedir, Path.GetFileName(found.Name)));
										}
										catch { }
									}
								}
								else
								{
									// Copy file between archives
									_logger.Log("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (_tgz ? found.HashData.SHA1 : found.Name) + "'");

									if (Build.MonoEnvironment || _tgz)
									{
										string outfile = FileTools.ExtractSingleItemFromArchive(input, rom.Name, _tempdir, _logger);
										if (File.Exists(outfile))
										{
											if (_tgz)
											{
												FileTools.WriteTorrentGZ(outfile, _outdir, _romba, _logger);
											}
											else
											{
												FileTools.WriteToManagedArchive(outfile, _outdir, found);
											}

											try
											{
												File.Delete(outfile);
											}
											catch { }
										}
									}
									else
									{
										string archiveFileName = Path.Combine(_outdir, found.Machine.Name + ".zip");
										FileTools.CopyFileBetweenManagedArchives(input, archiveFileName, rom.Name, found.Name, _logger);
									}
								}
							}
						}
					}
				}
				else
				{
					// Now, if the file is a supported archive type, also run on all files within
					bool encounteredErrors = FileTools.ExtractArchive(input, _tempdir, _7z, _gz, _rar, _zip, _logger);

					// Remove the current file if we are in recursion so it's not picked up in the next step
					if (recurse)
					{
						try
						{
							File.Delete(input);
						}
						catch (Exception)
						{
							// Don't log file deletion errors
						}
					}

					// If no errors were encountered, we loop through the temp directory
					if (!encounteredErrors)
					{
						_logger.Log("Archive found! Successfully extracted");
						foreach (string file in Directory.EnumerateFiles(_tempdir, "*", SearchOption.AllDirectories))
						{
							success &= RebuildToOutputHelper(file, index, total, true);
						}
					}
				}
			}

			return success;
		}

		/// <summary>
		/// Clean an individual folder based on the DAT
		/// </summary>
		/// <returns>True if the cleaning succeeded, false otherwise</returns>
		/// <remarks>This method is incomplete, it need to be finished before it can be used</remarks>
		public bool InplaceRebuild()
		{
			bool success = true;

			/*
			The process of rebuilding inplace is as follows:
			0) Resort the input roms by Game since that's more important in this case
			1) Scan the current folder according to the level specified (no recursion)
				a - If file is a match in all aspects, set correct flag and pass
					+ If the file has a header, skip?
				b - If file is a match in hash but not name, rename, set correct flag and pass
				c - If file is not a match, extract it to the output folder and remove from archive, set incorrect flag
			2) For all files that have been removed, check to see if they could be rebuilt to another location
				a - This behaves similarly (and indeed could call) "RebuildToFolder"
				b - If a file is a match and rebuilt, remove it from the output folder
			*/

			// Assuming archived sets, move all toplevel folders to temp
			foreach (string directory in Directory.EnumerateDirectories(_outdir, "*", SearchOption.TopDirectoryOnly))
			{
				Directory.Move(directory, Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(directory)));
			}

			// Now process the inputs (assumed that it's archived sets as of right now
			Dictionary<string, List<Rom>> scanned = new Dictionary<string, List<Rom>>();
			foreach (string archive in Directory.EnumerateFiles(_outdir, "*", SearchOption.AllDirectories))
			{
				// If we are in quickscan, get the list of roms that way
				List<Rom> roms = new List<Rom>();
				if (_quickScan)
				{
					roms = FileTools.GetArchiveFileInfo(Path.GetFullPath(archive), _logger);
				}
				// Otherwise, extract it and get info one by one
				else
				{
					string temparcdir = Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(archive));
					FileTools.ExtractArchive(Path.GetFullPath(archive), temparcdir, _logger);
					foreach (string tempfile in Directory.EnumerateFiles(temparcdir, "*", SearchOption.AllDirectories))
					{
						roms.Add(FileTools.GetSingleFileInfo(Path.GetFullPath(tempfile)));
					}

					// Clear the temporary archive directory
					FileTools.CleanDirectory(temparcdir);
				}

				// Then add each of the found files to the new dictionary
				foreach (Rom rom in roms)
				{
					string key = rom.HashData.Size + "-" + rom.HashData.CRC;
					if (scanned.ContainsKey(key))
					{
						scanned[key].Add(rom);
					}
					else
					{
						List<Rom> templist = new List<Rom>();
						templist.Add(rom);
						scanned.Add(key, templist);
					}
				}
			}

			// If nothing was found, we that it was successful
			if (scanned.Count == 0)
			{
				return success;
			}

			// Now that we have all of the from DAT and from folder roms, we try to match them, removing the perfect matches
			Dictionary<string, List<Rom>> remove = new Dictionary<string, List<Rom>>();
			foreach (string key in scanned.Keys)
			{
				// If the key doesn't even exist in the DAT, then mark the entire key for removal
				if (!_datdata.Files.ContainsKey(key))
				{
					if (remove.ContainsKey(key))
					{
						remove[key].AddRange(scanned[key]);
					}
					else
					{
						remove.Add(key, scanned[key]);
					}
				}
				// Otherwise check each of the values individually
				else
				{
					List<Rom> romsList = _datdata.Files[key];
					List<Rom> scannedList = scanned[key];
					foreach (Rom rom in scannedList)
					{
						if (!romsList.Contains(rom))
						{
							if (remove.ContainsKey(key))
							{
								remove[key].Add(rom);
							}
							else
							{
								List<Rom> templist = new List<Rom>();
								templist.Add(rom);
								remove.Add(key, templist);
							}
						}
					}
				}
			}

			// At this point, we have the complete list of files from the DAT, a complete
			// list of files that were scanned from the archives, and a complete list of
			// the files to be removed because they aren't matches. I think at this point,
			// we need to see if any of the files in "removed" can be rebuilt to something
			// that is missing. But we don't have a list of missings, so how do we get this
			// set of roms? Missing would be (_datdata.Roms - matches) I think. So if we
			// get this additional set, we then run it against the "removed" set and rebuild
			// as we go based on what we can do. Here is where we need some smarts. If the
			// game to rebuild from and to are the same, we want to copy within. You
			// should create a new helper function that "renames" an entry within the same
			// archive to help this along. Everything else rebuilding should be copied from
			// archive to archive. Once remove has been traversed, we will extract and remove
			// all of the files that have been found and put them in the temporary folder.

			return success;
		}
	}
}