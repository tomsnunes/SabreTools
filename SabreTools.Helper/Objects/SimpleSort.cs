using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SabreTools.Helper
{
	public class SimpleSort
	{
		// Private instance variables
		private DatFile _datdata;
		private List<string> _inputs;
		private string _outDir;
		private string _tempDir;
		private bool _quickScan;
		private bool _date;
		private bool _toFolder;
		private bool _verify;
		private bool _delete;
		private bool _tgz;
		private bool _romba;
		private bool _updateDat;
		private ArchiveScanLevel _archiveScanLevel;
		private Logger _logger;
		private int _maxDegreeOfParallelism = 4; // Hardcoded for now, should be an input later

		// Other private variables
		private int _cursorTop;
		private int _cursorLeft;
		private DatFile _matched;

		/// <summary>
		/// Create a new SimpleSort object
		/// </summary>
		/// <param name="datdata">DAT to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="verify">True if output directory should be checked instead of rebuilt to, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="tgz">True if output files should be written to TorrentGZ instead of TorrentZip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		public SimpleSort(DatFile datdata, List<string> inputs, string outDir, string tempDir,
			bool quickScan, bool date, bool toFolder, bool verify, bool delete, bool tgz, bool romba,
			ArchiveScanLevel archiveScanLevel, bool updateDat, Logger logger)
		{
			_datdata = datdata;
			_inputs = inputs;
			_outDir = (outDir == "" ? "Rebuild" : outDir);
			_tempDir = (String.IsNullOrEmpty(tempDir) ? Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) : tempDir);
			_quickScan = quickScan;
			_date = date;
			_toFolder = toFolder;
			_verify = verify;
			_delete = delete;
			_tgz = tgz;
			_romba = romba;
			_archiveScanLevel = archiveScanLevel;
			_updateDat = updateDat;
			_logger = logger;

			_cursorTop = Console.CursorTop;
			_cursorLeft = Console.CursorLeft;
			_matched = new DatFile
			{
				Files = new SortedDictionary<string, List<DatItem>>(),
			};
		}

		/// <summary>
		/// Pick the appropriate action based on the inputs
		/// </summary>
		/// <returns>True if success, false otherwise</returns>
		public bool StartProcessing()
		{
			// First, check that the output directory exists
			if (!Directory.Exists(_outDir))
			{
				Directory.CreateDirectory(_outDir);
				_outDir = Path.GetFullPath(_outDir);
			}

			// Then create or clean the temp directory
			if (!Directory.Exists(_tempDir))
			{
				Directory.CreateDirectory(_tempDir);
			}
			else
			{
				FileTools.CleanDirectory(_tempDir);
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
		private bool VerifyDirectory()
		{
			bool success = true;

			// Enumerate all files from the output directory
			List<string> files = new List<string>();
			foreach (string file in Directory.EnumerateFiles(_outDir, "*", SearchOption.AllDirectories))
			{
				_logger.Verbose("File found: '" + file + "'");
				files.Add(Path.GetFullPath(file));
			}

			/*
			We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
			*/

			// Then, loop through and check each of the inputs
			_logger.User("Processing files:\n");
			foreach (string input in _inputs)
			{
				_datdata.PopulateDatFromDir(input, false /* noMD5 */, false /* noSHA1 */, true /* bare */, false /* archivesAsFiles */,
					true /* enableGzip */, false /* addBlanks */, false /* addDate */, "" /* tempDir */, false /* copyFiles */,
					null /* headerToCheckAgainst */, 4 /* maxDegreeOfParallelism */, _logger);
			}

			// Setup the fixdat
			_matched = (DatFile)_datdata.CloneHeader();
			_matched.Files = new SortedDictionary<string, List<DatItem>>();
			_matched.FileName = "fixDat_" + _matched.FileName;
			_matched.Name = "fixDat_" + _matched.Name;
			_matched.Description = "fixDat_" + _matched.Description;
			_matched.OutputFormat = OutputFormat.Logiqx;

			// Now that all files are parsed, get only files found in directory
			bool found = false;
			foreach (List<DatItem> roms in _datdata.Files.Values)
			{
				List<DatItem> newroms = DatItem.Merge(roms, _logger);
				foreach (Rom rom in newroms)
				{
					if (rom.SourceID == 99)
					{
						found = true;
						string key = rom.Size + "-" + rom.CRC;
						if (_matched.Files.ContainsKey(key))
						{
							_matched.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							_matched.Files.Add(key, temp);
						}
					}
				}
			}

			// Now output the fixdat to the main folder
			if (found)
			{
				_matched.WriteToFile("", _logger, stats: true);
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
		/// <remarks>
		/// This currently processes files as follows:
		/// 1) Get all file names from the input files/folders
		/// 2) Loop through and process each file individually
		///		a) Hash the file
		///		b) Check against the DAT for duplicates
		///		c) Check for headers
		///		d) Check headerless rom for duplicates
		/// 
		/// This is actually rather slow and inefficient. See below for more correct implemenation
		/// </remarks>
		private bool RebuildToOutput()
		{
			bool success = true;

			_logger.User("Retrieving list all files from input");
			DateTime start = DateTime.Now;

			// Create a list of just files from inputs
			List<string> files = new List<string>();
			foreach (string input in _inputs)
			{
				if (File.Exists(input))
				{
					_logger.Verbose("File found: '" + input + "'");
					files.Add(Path.GetFullPath(input));
				}
				else if (Directory.Exists(input))
				{
					_logger.Verbose("Directory found: '" + input + "'");
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						_logger.Verbose("File found: '" + file + "'");
						files.Add(Path.GetFullPath(file));
					}
				}
				else
				{
					_logger.Error("'" + input + "' is not a file or directory!");
				}
			}
			_logger.User("Retrieving complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Then, loop through and check each of the inputs
			_logger.User("Processing files:\n");
			_cursorTop = Console.CursorTop;
			for (int i = 0; i < files.Count; i++)
			{
				success &= RebuildToOutputHelper(files[i], i, files.Count);
				if (_tempDir != Path.GetTempPath())
				{
					FileTools.CleanDirectory(_tempDir);
				}
				if (success && _delete)
				{
					try
					{
						File.Delete(files[i]);
					}
					catch { }
				}
			}

			// Now one final delete of the temp directory
			while (Directory.Exists(_tempDir))
			{
				try
				{
					if (_tempDir != Path.GetTempPath())
					{
						Directory.Delete(_tempDir, true);
					}
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
			StreamWriter sw = new StreamWriter(new MemoryStream());
			_matched.OutputStats(sw, StatOutputFormat.None, _logger, recalculate: true, baddumpCol: true, nodumpCol: true);
			sw.Dispose();

			// Now output the fixdat based on the original input if asked
			if (_updateDat)
			{
				_datdata.FileName = "fixDat_" + _datdata.FileName;
				_datdata.Name = "fixDat_" + _datdata.Name;
				_datdata.Description = "fixDat_" + _datdata.Description;
				_datdata.OutputFormat = OutputFormat.Logiqx;
				_datdata.WriteToFile("", _logger);
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
			_logger.WriteExact(statement, _cursorTop, 0);

			// Get if the file should be scanned internally and externally
			bool shouldExternalScan, shouldInternalScan;
			ArchiveTools.GetInternalExternalProcess(input, _archiveScanLevel, _logger, out shouldExternalScan, out shouldInternalScan);

			// Hash and match the external files
			if (shouldExternalScan)
			{
				Rom rom = FileTools.GetFileInfo(input, _logger);

				// If we have a blank RomData, it's an error
				if (rom.Name == null)
				{
					return false;
				}

				// Try to find the matches to the file that was found
				List<DatItem> foundroms = rom.GetDuplicates(_datdata, _logger);
				_logger.Verbose("File '" + input + "' had " + foundroms.Count + " matches in the DAT!");
				foreach (Rom found in foundroms)
				{
					_logger.Verbose("Matched name: " + found.Name);

					// Add rom to the matched list
					string key = found.Size + "-" + found.CRC;
					if (_matched.Files.ContainsKey(key))
					{
						_matched.Files[key].Add(found);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(found);
						_matched.Files.Add(key, temp);
					}

					if (_toFolder)
					{
						// Copy file to output directory
						string gamedir = Path.Combine(_outDir, found.MachineName);
						if (!Directory.Exists(gamedir))
						{
							Directory.CreateDirectory(gamedir);
						}

						_logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (_tgz ? found.SHA1 : found.Name) + "'");
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
							ArchiveTools.WriteTorrentGZ(input, _outDir, _romba, _logger);
						}
						else
						{
							ArchiveTools.WriteToArchive(input, _outDir, found, _logger, date: _date);
						}
					}
				}

				// Now get the transformed file if it exists
				SkipperRule rule = Skipper.GetMatchingRule(input, "", _logger);

				// If we have have a non-empty rule, apply it
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Otherwise, apply the rule to the file
					string newinput = input + ".new";
					rule.TransformFile(input, newinput, _logger);
					Rom drom = FileTools.GetFileInfo(newinput, _logger);

					// If we have a blank RomData, it's an error
					if (String.IsNullOrEmpty(drom.Name))
					{
						return false;
					}

					// Try to find the matches to the file that was found
					List<DatItem> founddroms = drom.GetDuplicates(_datdata, _logger);
					_logger.Verbose("File '" + newinput + "' had " + founddroms.Count + " matches in the DAT!");
					foreach (Rom found in founddroms)
					{
						// Add rom to the matched list
						string key = found.Size + "-" + found.CRC;
						if (_matched.Files.ContainsKey(key))
						{
							_matched.Files[key].Add(found);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(found);
							_matched.Files.Add(key, temp);
						}

						// First output the headerless rom
						_logger.Verbose("Matched name: " + found.Name);

						if (_toFolder)
						{
							// Copy file to output directory
							string gamedir = Path.Combine(_outDir, found.MachineName);
							if (!Directory.Exists(gamedir))
							{
								Directory.CreateDirectory(gamedir);
							}

							_logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (_tgz ? found.SHA1 : found.Name) + "'");
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
								ArchiveTools.WriteTorrentGZ(newinput, _outDir, _romba, _logger);
							}
							else
							{
								ArchiveTools.WriteToArchive(newinput, _outDir, found, _logger, date: _date);
							}
						}

						// Then output the headered rom (renamed)
						Rom newfound = found;
						newfound.Name = Path.GetFileNameWithoutExtension(newfound.Name) + " (" + rom.CRC + ")" + Path.GetExtension(newfound.Name);
						newfound.Size = rom.Size;
						newfound.CRC = rom.CRC;
						newfound.MD5 = rom.MD5;
						newfound.SHA1 = rom.SHA1;

						// Add rom to the matched list
						key = newfound.Size + "-" + newfound.CRC;
						if (_matched.Files.ContainsKey(key))
						{
							_matched.Files[key].Add(newfound);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(newfound);
							_matched.Files.Add(key, temp);
						}

						if (_toFolder)
						{
							// Copy file to output directory
							string gamedir = Path.Combine(_outDir, found.MachineName);
							if (!Directory.Exists(gamedir))
							{
								Directory.CreateDirectory(gamedir);
							}

							_logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + newfound.Name + "'");
							try
							{
								File.Copy(input, Path.Combine(gamedir, Path.GetFileName(newfound.Name)));
							}
							catch { }
						}
						else
						{
							_logger.Verbose("Matched name: " + newfound.Name);
							if (_tgz)
							{
								ArchiveTools.WriteTorrentGZ(input, _outDir, _romba, _logger);
							}
							else
							{
								ArchiveTools.WriteToArchive(input, _outDir, newfound, _logger, date: _date);
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
					_logger.Verbose("Beginning quick scan of contents from '" + input + "'");
					List<Rom> internalRomData = ArchiveTools.GetArchiveFileInfo(input, _logger);
					_logger.Verbose(internalRomData.Count + " entries found in '" + input + "'");

					// If the list is populated, then the file was a filled archive
					if (internalRomData.Count > 0)
					{
						foreach (Rom rom in internalRomData)
						{
							// Try to find the matches to the file that was found
							List<DatItem> foundroms = rom.GetDuplicates(_datdata, _logger);
							_logger.Verbose("File '" + rom.Name + "' had " + foundroms.Count + " matches in the DAT!");
							foreach (Rom found in foundroms)
							{
								// Add rom to the matched list
								string key = found.Size + "-" + found.CRC;
								if (_matched.Files.ContainsKey(key))
								{
									_matched.Files[key].Add(found);
								}
								else
								{
									List<DatItem> temp = new List<DatItem>();
									temp.Add(found);
									_matched.Files.Add(key, temp);
								}

								if (_toFolder)
								{
									// Copy file to output directory
									_logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + found.Name + "'");
									string outfile = ArchiveTools.ExtractSingleItemFromArchive(input, rom.Name, _tempDir, _logger);
									if (File.Exists(outfile))
									{
										string gamedir = Path.Combine(_outDir, found.MachineName);
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
									_logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (_tgz ? found.SHA1 : found.Name) + "'");

									if (Build.MonoEnvironment || _tgz)
									{
										string outfile = ArchiveTools.ExtractSingleItemFromArchive(input, rom.Name, _tempDir, _logger);
										if (File.Exists(outfile))
										{
											if (_tgz)
											{
												ArchiveTools.WriteTorrentGZ(outfile, _outDir, _romba, _logger);
											}
											else
											{
												ArchiveTools.WriteToArchive(outfile, _outDir, found, _logger);
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
										ArchiveTools.CopyFileBetweenArchives(input, _outDir, rom.Name, found, _logger);
									}
								}
							}
						}
					}
				}
				else
				{
					// Now, if the file is a supported archive type, also run on all files within
					bool encounteredErrors = ArchiveTools.ExtractArchive(input, _tempDir, _archiveScanLevel, _logger);

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
						_logger.Verbose("Archive found! Successfully extracted");
						foreach (string file in Directory.EnumerateFiles(_tempDir, "*", SearchOption.AllDirectories))
						{
							success &= RebuildToOutputHelper(file, index, total, true);
						}
					}
				}
			}

			return success;
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		/// <remarks>
		/// This implemenation of the code should do the following:
		/// 1) Get all file names from the input files/folders (parallel)
		/// 2) Loop through and get the file info from every file (including headerless)
		/// 3) Find all duplicate files in the input DAT(s)
		/// 4) Order by output game
		/// 5) Rebuild all files
		/// </remarks>
		private bool RebuiltToOutputAlternate()
		{
			bool success = true;

			#region Find all files

			// Create a list of just files from inputs
			_logger.User("Finding all files...");
			List<string> files = new List<string>();
			Parallel.ForEach(_inputs,
				new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
				input =>
			{
				if (File.Exists(input))
				{
					_logger.Verbose("File found: '" + input + "'");
					lock (files)
					{
						files.Add(Path.GetFullPath(input));
					}
				}
				else if (Directory.Exists(input))
				{
					_logger.Verbose("Directory found: '" + input + "'");

					List<string> infiles = Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(infiles,
						new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
						file =>
					{
						_logger.Verbose("File found: '" + input + "'");
						lock (files)
						{
							files.Add(Path.GetFullPath(file));
						}
					});
				}
				else
				{
					_logger.Error("'" + input + "' is not a file or directory!");
				}
			});
			_logger.User("Finding files complete!");

			#endregion

			#region Get source file information

			// Now loop through all of the files and check them, DFD style
			_logger.User("Getting source file information...");
			DatFile matchdat = new DatFile
			{
				Files = new SortedDictionary<string, List<DatItem>>(),
			};
			foreach (string file in files)
			{
				// Get if the file should be scanned internally and externally
				bool shouldExternalScan, shouldInternalScan;
				ArchiveTools.GetInternalExternalProcess(file, _archiveScanLevel, _logger, out shouldExternalScan, out shouldInternalScan);

				// Hash and match the external files
				if (shouldExternalScan)
				{
					RebuildToOutputAlternateParseRomHelper(file, ref matchdat, _logger);
				}

				// If we should scan the file as an archive
				if (shouldInternalScan)
				{
					// If external scanning is enabled, use that method instead
					if (_quickScan)
					{
						_logger.Verbose("Beginning quick scan of contents from '" + file + "'");
						List<Rom> internalRomData = ArchiveTools.GetArchiveFileInfo(file, _logger);
						_logger.Verbose(internalRomData.Count + " entries found in '" + file + "'");

						// Now add all of the roms to the DAT
						for (int i = 0; i < internalRomData.Count; i++)
						{
							RebuildToOutputAlternateParseRomHelper(file, ref matchdat, _logger);
						}
					}
					// Otherwise, try to extract the file to the temp folder
					else
					{
						// Now, if the file is a supported archive type, also run on all files within
						bool encounteredErrors = ArchiveTools.ExtractArchive(file, _tempDir, _archiveScanLevel, _logger);

						// If we succeeded in extracting, loop through the files
						if (!encounteredErrors)
						{
							List<string> extractedFiles = Directory.EnumerateFiles(_tempDir, "*", SearchOption.AllDirectories).ToList();
							foreach (string extractedFile in extractedFiles)
							{
								RebuildToOutputAlternateParseRomHelper(extractedFile, ref matchdat, _logger);
							}
						}
						// Otherwise, skip extracting and just get information on the file itself (if we didn't already)
						else if (!shouldExternalScan)
						{
							RebuildToOutputAlternateParseRomHelper(file, ref matchdat, _logger);
						}

						// Clean the temp directory for the next round
						if (Directory.Exists(_tempDir))
						{
							FileTools.CleanDirectory(_tempDir);
						}
					}
				}
			}
			_logger.User("Getting source file information complete!");

			#endregion

			#region Find all files to rebuild and bucket by game

			// Create a dictionary of from/to Rom mappings
			Dictionary<DatItem, DatItem> toFromMap = new Dictionary<DatItem, DatItem>();

			// Now populate it
			foreach (string key in matchdat.Files.Keys)
			{
				foreach (DatItem rom in matchdat.Files[key])
				{
					List<DatItem> matched = rom.GetDuplicates(_datdata, _logger, true);
					foreach (DatItem match in matched)
					{
						try
						{
							toFromMap.Add(match, rom);
						}
						catch { }
					}
				}
			}

			// Then bucket the keys by game for better output
			SortedDictionary<string, List<DatItem>> keysByGame = DatFile.BucketListByGame(toFromMap.Keys.ToList(), false, true, _logger);

			#endregion

			#region Rebuild all files

			// At this point, we have "toFromMap" which maps output files to input files as well as
			// as SortedDictionary called keysByGame which is the output files sorted by game in
			// alphabetical order. We should be able to use these to do everything we need =)

			// Now write out each game sequentially
			foreach (string key in keysByGame.Keys)
			{
				
			}

			#endregion

			return success;
		}

		/// <summary>
		/// Wrap adding a file to the dictionary in custom DFD, files that matched a skipper a prefixed with "HEAD::"
		/// </summary>
		/// <param name="file">Name of the file to attempt to add</param>
		/// <param name="matchdat">Reference to the Dat to add to</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the file could be added, false otherwise</returns>
		private bool RebuildToOutputAlternateParseRomHelper(string file, ref DatFile matchdat, Logger logger)
		{
			Rom rom = FileTools.GetFileInfo(file, logger);

			// If we have a blank RomData, it's an error
			if (rom.Name == null)
			{
				return false;
			}

			// Otherwise, set the machine name as the full path to the file
			rom.MachineName = Path.GetDirectoryName(Path.GetFullPath(file));

			// Add the rom information to the Dat
			string key = rom.Size + "-" + rom.CRC;
			if (matchdat.Files.ContainsKey(key))
			{
				matchdat.Files[key].Add(rom);
			}
			else
			{
				List<DatItem> temp = new List<DatItem>();
				temp.Add(rom);
				matchdat.Files.Add(key, temp);
			}

			// Now attempt to see if the file has a header
			FileStream input = File.OpenRead(file);
			SkipperRule rule = Skipper.GetMatchingRule(input, "", _logger);

			// If there's a match, get the new information from the stream
			if (rule.Tests != null && rule.Tests.Count != 0)
			{
				// Create the input and output streams
				MemoryStream output = new MemoryStream();

				// Transform the stream and get the information from it
				rule.TransformStream(input, output, _logger, false, true);
				Rom romNH = FileTools.GetStreamInfo(output, output.Length);
				romNH.Name = "HEAD::" + rom.Name;
				romNH.MachineName = rom.MachineName;

				// Add the rom information to the Dat
				key = romNH.Size + "-" + romNH.CRC;
				if (matchdat.Files.ContainsKey(key))
				{
					matchdat.Files[key].Add(romNH);
				}
				else
				{
					List<DatItem> temp = new List<DatItem>();
					temp.Add(romNH);
					matchdat.Files.Add(key, temp);
				}

				// Dispose of the stream
				output.Dispose();
			}

			// Dispose of the stream
			input.Dispose();

			return true;
		}

		/// <summary>
		/// Clean an individual folder based on the DAT
		/// </summary>
		/// <returns>True if the cleaning succeeded, false otherwise</returns>
		/// <remarks>This method is incomplete, it need to be finished before it can be used</remarks>
		private bool InplaceRebuild()
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
			foreach (string directory in Directory.EnumerateDirectories(_outDir, "*", SearchOption.TopDirectoryOnly))
			{
				Directory.Move(directory, Path.Combine(_tempDir, Path.GetFileNameWithoutExtension(directory)));
			}

			// Now process the inputs (assumed that it's archived sets as of right now
			Dictionary<string, List<DatItem>> scanned = new Dictionary<string, List<DatItem>>();
			foreach (string archive in Directory.EnumerateFiles(_outDir, "*", SearchOption.AllDirectories))
			{
				// If we are in quickscan, get the list of roms that way
				List<Rom> roms = new List<Rom>();
				if (_quickScan)
				{
					roms = ArchiveTools.GetArchiveFileInfo(Path.GetFullPath(archive), _logger);
				}
				// Otherwise, extract it and get info one by one
				else
				{
					string temparcdir = Path.Combine(_tempDir, Path.GetFileNameWithoutExtension(archive));
					ArchiveTools.ExtractArchive(Path.GetFullPath(archive), temparcdir, _logger);
					foreach (string tempfile in Directory.EnumerateFiles(temparcdir, "*", SearchOption.AllDirectories))
					{
						roms.Add(FileTools.GetFileInfo(Path.GetFullPath(tempfile), _logger));
					}

					// Clear the temporary archive directory
					FileTools.CleanDirectory(temparcdir);
				}

				// Then add each of the found files to the new dictionary
				foreach (Rom rom in roms)
				{
					string key = rom.Size + "-" + rom.CRC;
					if (scanned.ContainsKey(key))
					{
						scanned[key].Add(rom);
					}
					else
					{
						List<DatItem> templist = new List<DatItem>();
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
			Dictionary<string, List<DatItem>> remove = new Dictionary<string, List<DatItem>>();
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
					List<DatItem> romsList = _datdata.Files[key];
					List<DatItem> scannedList = scanned[key];
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
								List<DatItem> templist = new List<DatItem>();
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

		/// <summary>
		/// Process inputs and convert to TorrentZip or TorrentGZ, optionally converting to Romba
		/// </summary>
		/// <returns>True if processing was a success, false otherwise</returns>
		public bool Convert()
		{
			bool success = true;

			// First, check that the output directory exists
			if (!Directory.Exists(_outDir))
			{
				Directory.CreateDirectory(_outDir);
				_outDir = Path.GetFullPath(_outDir);
			}

			// Then create or clean the temp directory
			if (!Directory.Exists(_tempDir))
			{
				Directory.CreateDirectory(_tempDir);
			}
			else
			{
				FileTools.CleanDirectory(_tempDir);
			}

			// Now process all of the inputs
			foreach (string input in _inputs)
			{
				_logger.User("Examining file " + input);

				// Get if the file should be scanned internally and externally
				bool shouldExternalProcess, shouldInternalProcess;
				ArchiveTools.GetInternalExternalProcess(input, _archiveScanLevel, _logger, out shouldExternalProcess, out shouldInternalProcess);

				// Do an external scan of the file, if necessary
				if (shouldExternalProcess)
				{
					// If a DAT is defined, we want to make sure that this file is not in there
					Rom rom = FileTools.GetFileInfo(input, _logger);
					if (_datdata != null && _datdata.Files.Count > 0)
					{
						if (rom.HasDuplicates(_datdata, _logger))
						{
							_logger.User("File '" + input + "' existed in the DAT, skipping...");
							continue;
						}
					}

					_logger.User("Processing file " + input);

					if (_tgz)
					{
						success &= ArchiveTools.WriteTorrentGZ(input, _outDir, _romba, _logger);
					}
					else
					{
						success &= ArchiveTools.WriteToArchive(input, _outDir, rom, _logger);
					}
				}

				// Process the file as an archive, if necessary
				if (shouldInternalProcess)
				{
					// Now, if the file is a supported archive type, also run on all files within
					bool encounteredErrors = ArchiveTools.ExtractArchive(input, _tempDir, _archiveScanLevel, _logger);

					// If no errors were encountered, we loop through the temp directory
					if (!encounteredErrors)
					{
						_logger.Verbose("Archive found! Successfully extracted");
						foreach (string file in Directory.EnumerateFiles(_tempDir, "*", SearchOption.AllDirectories))
						{
							// If a DAT is defined, we want to make sure that this file is not in there
							Rom rom = FileTools.GetFileInfo(file, _logger);
							if (_datdata != null && _datdata.Files.Count > 0)
							{
								if (rom.HasDuplicates(_datdata, _logger))
								{
									_logger.User("File '" + file + "' existed in the DAT, skipping...");
									continue;
								}
							}

							_logger.User("Processing file " + input);

							if (_tgz)
							{
								success &= ArchiveTools.WriteTorrentGZ(input, _outDir, _romba, _logger);
							}
							else
							{
								success &= ArchiveTools.WriteToArchive(input, _outDir, rom, _logger);
							}
						}

						FileTools.CleanDirectory(_tempDir);
					}
				}

				// Delete the source file if we're supposed to
				if (_delete)
				{
					try
					{
						_logger.User("Attempting to delete " + input);
						File.Delete(input);
					}
					catch (Exception ex)
					{
						_logger.Error(ex.ToString());
						success &= false;
					}
				}
			}

			// Now one final delete of the temp directory
			while (Directory.Exists(_tempDir))
			{
				try
				{
					Directory.Delete(_tempDir, true);
				}
				catch
				{
					continue;
				}
			}

			// If we're in romba mode and the size file doesn't exist, create it
			if (_romba && !File.Exists(Path.Combine(_outDir, ".romba_size")))
			{
				// Get the size of all of the files in the output folder
				long size = 0;
				foreach (string file in Directory.EnumerateFiles(_outDir, "*", SearchOption.AllDirectories))
				{
					FileInfo tempinfo = new FileInfo(file);
					size += tempinfo.Length;
				}

				// Write out the value to each of the romba depot files
				StreamWriter tw = new StreamWriter(File.Open(Path.Combine(_outDir, ".romba_size"), FileMode.Create, FileAccess.Write));
				StreamWriter twb = new StreamWriter(File.Open(Path.Combine(_outDir, ".romba_size.backup"), FileMode.Create, FileAccess.Write));

				tw.Write(size);
				twb.Write(size);

				tw.Dispose();
				twb.Dispose();
			}

			return success;
		}
	}
}
