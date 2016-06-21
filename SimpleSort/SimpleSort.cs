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
		private ArchiveScanLevel _7z;
		private ArchiveScanLevel _gz;
		private ArchiveScanLevel _rar;
		private ArchiveScanLevel _zip;
		private Logger _logger;

		/// <summary>
		/// Create a new SimpleSort object
		/// </summary>
		/// <param name="datdata">Name of the DAT to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outdir">Output directory to use to build to</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		public SimpleSort(Dat datdata, List<string> inputs, string outdir, string tempdir,
			bool quickScan, int sevenzip, int gz, int rar, int zip, Logger logger)
		{
			_datdata = datdata;
			_inputs = inputs;
			_outdir = (outdir == "" ? "Rebuild" : outdir);
			_tempdir = (tempdir == "" ? "__TEMP__" : tempdir);
			_quickScan = quickScan;
			_7z = (ArchiveScanLevel)(sevenzip < 0 || sevenzip > 2 ? 0 : sevenzip);
			_gz = (ArchiveScanLevel)(gz < 0 || gz > 2 ? 0 : gz);
			_rar = (ArchiveScanLevel)(rar < 0 || rar > 2 ? 0 : rar);
			_zip = (ArchiveScanLevel)(zip < 0 || zip > 2 ? 0 : zip);
			_logger = logger;
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
				simpleSort = true;
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
					case "-qs":
					case "--quick":
						quickScan = true;
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
			if (inputs.Count == 0 && (simpleSort))
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
					InitSimpleSort(datfiles, inputs, outdir, tempdir, quickScan, sevenzip, gz, rar, zip, logger);
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
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitSimpleSort(List<string> datfiles, List<string> inputs, string outdir, string tempdir, 
			bool quickScan, int sevenzip, int gz, int rar, int zip, Logger logger)
		{
			// Add all of the input DATs into one huge internal DAT
			Dat datdata = new Dat();
			foreach (string datfile in datfiles)
			{
				datdata = DatTools.Parse(datfile, 0, 0, datdata, logger);
			}

			SimpleSort ss = new SimpleSort(datdata, inputs, outdir, tempdir, quickScan, sevenzip, gz, rar, zip, logger);
			ss.RebuildToFolder();
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		public bool RebuildToFolder()
		{
			bool success = true;

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
				Output.CleanDirectory(_tempdir);
			}

			// Then, loop through and check each of the inputs
			_logger.User("Starting to loop through inputs");
			foreach (string input in _inputs)
			{
				if (File.Exists(input))
				{
					_logger.Log("File found: '" + input + "'");
					success &= RebuildToFolderHelper(input);
					Output.CleanDirectory(_tempdir);
				}
				else if (Directory.Exists(input))
				{
					_logger.Log("Directory found: '" + input + "'");
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						_logger.Log("File found: '" + file + "'");
						success &= RebuildToFolderHelper(file);
						Output.CleanDirectory(_tempdir);
					}
				}
				else
				{
					_logger.Error("'" + input + "' is not a file or directory!");
				}
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

			return success;
		}

		/// <summary>
		/// Process an individual file against the DAT
		/// </summary>
		/// <param name="input">The name of the input file</param>
		/// <param name="recurse">True if this is in a recurse step and the file should be deleted, false otherwise (default)</param>
		/// <returns>True if it was processed properly, false otherwise</returns>
		private bool RebuildToFolderHelper(string input, bool recurse = false)
		{
			bool success = true;

			// Get the full path of the input for movement purposes
			input = Path.GetFullPath(input);
			_logger.User("Beginning processing of '" + input + "'");

			// Get if the file should be scanned internally and externally
			bool shouldExternalScan = true;
			bool shouldInternalScan = true;

			ArchiveType? archiveType = ArchiveTools.GetCurrentArchiveType(input, _logger);
			switch (archiveType)
			{
				case null:
					shouldExternalScan = true;
					shouldInternalScan = false;
					break;
				case ArchiveType.GZip:
					shouldExternalScan = (_gz != ArchiveScanLevel.Internal);
					shouldInternalScan = (_gz != ArchiveScanLevel.External);
					break;
				case ArchiveType.Rar:
					shouldExternalScan = (_rar != ArchiveScanLevel.Internal);
					shouldInternalScan = (_rar != ArchiveScanLevel.External);
					break;
				case ArchiveType.SevenZip:
					shouldExternalScan = (_7z != ArchiveScanLevel.Internal);
					shouldInternalScan = (_7z != ArchiveScanLevel.External);
					break;
				case ArchiveType.Zip:
					shouldExternalScan = (_zip != ArchiveScanLevel.Internal);
					shouldInternalScan = (_zip != ArchiveScanLevel.External);
					break;
			}

			// Hash and match the external files
			if (shouldExternalScan)
			{
				Rom rom = RomTools.GetSingleFileInfo(input);

				// If we have a blank RomData, it's an error
				if (rom.Name == null)
				{
					return false;
				}

				// Try to find the matches to the file that was found
				List<Rom> foundroms = RomTools.GetDuplicates(rom, _datdata);
				_logger.User("File '" + input + "' had " + foundroms.Count + " matches in the DAT!");
				foreach (Rom found in foundroms)
				{
					_logger.Log("Matched name: " + found.Name);
					ArchiveTools.WriteToArchive(input, _outdir, found);
				}

				// Now get the transformed file if it exists
				SkipperRule rule = Skippers.MatchesSkipper(input, "", _logger);

				// If we have have a non-empty rule, apply it
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Otherwise, apply the rule ot the file
					string newinput = input + ".new";
					Skippers.TransformFile(input, input + ".new", rule, _logger);
					Rom drom = RomTools.GetSingleFileInfo(newinput);

					// If we have a blank RomData, it's an error
					if (drom.Name == null)
					{
						return false;
					}

					// Try to find the matches to the file that was found
					List<Rom> founddroms = RomTools.GetDuplicates(drom, _datdata);
					_logger.User("File '" + newinput + "' had " + founddroms.Count + " matches in the DAT!");
					foreach (Rom found in founddroms)
					{
						// First output the headerless rom
						_logger.Log("Matched name: " + found.Name);
						ArchiveTools.WriteToArchive(newinput, _outdir, found);

						// Then output the headered rom (renamed)
						Rom newfound = found;
						newfound.Name = Path.GetFileNameWithoutExtension(newfound.Name) + " (" + rom.CRC + ")" + Path.GetExtension(newfound.Name);

						_logger.Log("Matched name: " + newfound.Name);
						ArchiveTools.WriteToArchive(input, _outdir, newfound);
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
					List<Rom> internalRomData = ArchiveTools.GetArchiveFileInfo(input, _logger);
					_logger.Log(internalRomData.Count + " entries found in '" + input + "'");

					// If the list is populated, then the file was a filled archive
					if (internalRomData.Count > 0)
					{
						foreach (Rom rom in internalRomData)
						{
							// Try to find the matches to the file that was found
							List<Rom> foundroms = RomTools.GetDuplicates(rom, _datdata);
							_logger.User("File '" + rom.Name + "' had " + foundroms.Count + " matches in the DAT!");
							foreach (Rom found in foundroms)
							{
								_logger.Log("Matched name: " + found.Name);

								// Copy file between archives
								_logger.User("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + found.Name + "'");
								string archiveFileName = Path.Combine(_outdir, found.Game + ".zip");
								ArchiveTools.CopyFileBetweenArchives(input, archiveFileName, rom.Name, found.Name, _logger);
							}
						}
					}
				}
				else
				{
					// Now, if the file is a supported archive type, also run on all files within
					bool encounteredErrors = !ArchiveTools.ExtractArchive(input, _tempdir, _7z, _gz, _rar, _zip, _logger);

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
						_logger.User("Archive found! Successfully extracted");
						foreach (string file in Directory.EnumerateFiles(_tempdir, "*", SearchOption.AllDirectories))
						{
							success &= RebuildToFolderHelper(file, true);
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

			// Sort the input set(s) by game
			SortedDictionary<string, List<Rom>> sortedByGame = DatTools.BucketByGame(_datdata.Roms, false, true, _logger);

			// Assuming archived sets, move all toplevel folders to temp
			foreach (string directory in Directory.EnumerateDirectories(_outdir, "*", SearchOption.TopDirectoryOnly))
			{
				Directory.Move(directory, Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(directory)));
			}

			// Now process the inputs (assumed that it's archived sets as of right now
			foreach (string archive in Directory.EnumerateFiles(_outdir, "*", SearchOption.AllDirectories))
			{
				// If the name of the archive is not in the set exactly, move it to temp
				if (!sortedByGame.ContainsKey(Path.GetFileNameWithoutExtension(archive)))
				{
					File.Move(archive, Path.Combine(_tempdir, archive));
				}
				// Otherwise, we check if it's an archive. If it's not, move it to temp
				else if (ArchiveTools.GetCurrentArchiveType(Path.GetFullPath(archive), _logger) == null)
				{
					File.Move(Path.GetFullPath(archive), Path.Combine(_tempdir, archive));
				}
				// Finally, if it's an archive and exists properly, we check the insides
				else
				{
					List<Rom> roms = new List<Rom>();

					// If we are in quickscan, get the list of roms that way
					if (_quickScan)
					{
						roms = ArchiveTools.GetArchiveFileInfo(Path.GetFullPath(archive), _logger);
					}
					// Otherwise, extract it and get info one by one
					else
					{
						string temparcdir = Path.Combine(_tempdir, Path.GetFileNameWithoutExtension(archive));
						ArchiveTools.ExtractArchive(Path.GetFullPath(archive), temparcdir, _logger);
						foreach (string tempfile in Directory.EnumerateFiles(temparcdir, "*", SearchOption.AllDirectories))
						{
							roms.Add(RomTools.GetSingleFileInfo(Path.GetFullPath(tempfile)));
						}

						// Clear the temporary archive directory
						Output.CleanDirectory(temparcdir);
					}

					// Here, we traverse the newly created list and see if any of the files are in the list corresponding to the game
					/*
					Now, how do we do this WITHOUT traversing the list a billion times?
					Does "contains" work in this situation?
					Which is better: traversing the "should have" list or the "do have" list?
					*/
					List<Rom> fromDat = sortedByGame[Path.GetFileNameWithoutExtension(archive)];
					List<Rom> toRemove = new List<Rom>();
					foreach (Rom rom in roms)
					{
						// If it's not in at all or needs renaming, mark for removal
						if (!fromDat.Contains(rom))
						{
							toRemove.Add(rom);
						}
						// Otherwise, we leave it be
						else
						{
							
						}
					}
				}
			}

			return success;
		}
	}
}