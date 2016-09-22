using SabreTools.Helper;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SabreTools
{
	/// <summary>
	/// Create a DAT file from a specified file, directory, or set thereof
	/// </summary>
	public class DATFromDir
	{
		// Path-related variables
		private string _basePath;
		private string _tempDir;

		// User specified inputs
		private DatFile _datdata;
		private bool _noMD5;
		private bool _noSHA1;
		private bool _bare;
		private bool _archivesAsFiles;
		private bool _enableGzip;
		private bool _addBlanks;
		private bool _addDate;
		private bool _copyFiles;
		private int _maxDegreeOfParallelism;

		// Other required variables
		private Logger _logger;

		// Public variables
		public DatFile DatData
		{
			get { return _datdata; }
		}

		/// <summary>
		/// Create a new DATFromDir object
		/// </summary>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="datdata">DatData object representing the requested output DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logger object for console and file output</param>
		public DATFromDir(string basePath, DatFile datdata, bool noMD5, bool noSHA1, bool bare, bool archivesAsFiles,
			bool enableGzip, bool addBlanks, bool addDate, string tempDir, bool copyFiles, int maxDegreeOfParallelism, Logger logger)
		{
			_basePath = Path.GetFullPath(basePath);
			_datdata = datdata;
			_datdata.Files = new Dictionary<string, List<DatItem>>();
			_datdata.Files.Add("null", new List<DatItem>());
			_noMD5 = noMD5;
			_noSHA1 = noSHA1;
			_bare = bare;
			_archivesAsFiles = archivesAsFiles;
			_enableGzip = enableGzip;
			_addBlanks = addBlanks;
			_addDate = addDate;
			_tempDir = (String.IsNullOrEmpty(tempDir) ? Path.GetTempPath() : tempDir);
			_copyFiles = copyFiles;
			_maxDegreeOfParallelism = maxDegreeOfParallelism;
			_logger = logger;
		}

		/// <suaxmary>
		/// Process the file, folder, or list of some combination into a DAT file
		/// </summary>
		/// <returns>True if the DAT could be created, false otherwise</returns>
		public bool Start()
		{
			// If the description is defined but not the name, set the name from the description
			if (String.IsNullOrEmpty(_datdata.Name) && !String.IsNullOrEmpty(_datdata.Description))
			{
				_datdata.Name = _datdata.Description;
			}

			// If the name is defined but not the description, set the description from the name
			else if (!String.IsNullOrEmpty(_datdata.Name) && String.IsNullOrEmpty(_datdata.Description))
			{
				_datdata.Description = _datdata.Name + (_bare ? "" : " (" + _datdata.Date + ")");
			}

			// If neither the name or description are defined, set them from the automatic values
			else if (String.IsNullOrEmpty(_datdata.Name) && String.IsNullOrEmpty(_datdata.Description))
			{
				_datdata.Name = _basePath.Split(Path.DirectorySeparatorChar).Last();
				_datdata.Description = _datdata.Name + (_bare ? "" : " (" + _datdata.Date + ")");
			}

			// Process the input folder
			_logger.Log("Folder found: " + _basePath);

			// Process the files in all subfolders
			List<string> files = Directory.EnumerateFiles(_basePath, "*", SearchOption.AllDirectories).ToList();
			Parallel.ForEach(files,
				new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
				item =>
			{
				ProcessPossibleArchive(item);
			});

			// Now find all folders that are empty, if we are supposed to
			if (!_datdata.Romba && _addBlanks)
			{
				List<string> empties = Directory.EnumerateDirectories(_basePath, "*", SearchOption.AllDirectories).ToList();
				Parallel.ForEach(empties,
					new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
					dir =>
				{
					if (Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly).Count() == 0)
					{
						// Get the full path for the directory
						string fulldir = Path.GetFullPath(dir);

						// Set the temporary variables
						string gamename = "";
						string romname = "";

						// If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
						if (_datdata.Type == "SuperDAT")
						{
							gamename = fulldir.Remove(0, _basePath.Length + 1);
							romname = "-";
						}

						// Otherwise, we want just the top level folder as the game, and the file as everything else
						else
						{
							gamename = fulldir.Remove(0, _basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
							romname = Path.Combine(fulldir.Remove(0, _basePath.Length + 1 + gamename.Length), "-");
						}

						// Sanitize the names
						if (gamename.StartsWith(Path.DirectorySeparatorChar.ToString()))
						{
							gamename = gamename.Substring(1);
						}
						if (gamename.EndsWith(Path.DirectorySeparatorChar.ToString()))
						{
							gamename = gamename.Substring(0, gamename.Length - 1);
						}
						if (romname.StartsWith(Path.DirectorySeparatorChar.ToString()))
						{
							romname = romname.Substring(1);
						}
						if (romname.EndsWith(Path.DirectorySeparatorChar.ToString()))
						{
							romname = romname.Substring(0, romname.Length - 1);
						}

						_logger.Log("Adding blank empty folder: " + gamename);
						_datdata.Files["null"].Add(new Rom(romname, gamename));
					}
				});
			}

			// Now that we're done, delete the temp folder (if it's not the default)
			_logger.User("Cleaning temp folder");
			try
			{
				if (_tempDir != Path.GetTempPath())
				{
					Directory.Delete(_tempDir, true);
				}
			}
			catch
			{
				// Just absorb the error for now
			}

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		private void ProcessPossibleArchive(string item)
		{
			// Define the temporary directory
			string tempSubDir = Path.GetFullPath(Path.Combine(_tempDir, Path.GetRandomFileName())) + Path.DirectorySeparatorChar;

			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (_datdata.Romba)
			{
				Rom rom = FileTools.GetTorrentGZFileInfo(item, _logger);

				// If the rom is valid, write it out
				if (rom.Name != null)
				{
					// Add the list if it doesn't exist already
					string key = rom.Size + "-" + rom.CRC;
					
					lock (_datdata.Files)
					{
						if (!_datdata.Files.ContainsKey(key))
						{
							_datdata.Files.Add(key, new List<DatItem>());
						}

						_datdata.Files[key].Add(rom);
						_logger.User("File added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
					}
					
				}
				else
				{
					_logger.User("File not added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
					return;
				}
				
				return;
			}

			// If we're copying files, copy it first and get the new filename
			string newItem = item;
			string newBasePath = _basePath;
			if (_copyFiles)
			{
				newBasePath = Path.Combine(_tempDir, Path.GetRandomFileName());
				newItem = Path.GetFullPath(Path.Combine(newBasePath, Path.GetFullPath(item).Remove(0, _basePath.Length + 1)));
				Directory.CreateDirectory(Path.GetDirectoryName(newItem));
				File.Copy(item, newItem, true);
			}

			// If both deep hash skip flags are set, do a quickscan
			if (_noMD5 && _noSHA1)
			{
				ArchiveType? type = FileTools.GetCurrentArchiveType(newItem, _logger);

				// If we have an archive, scan it
				if (type != null && !_archivesAsFiles)
				{
					List<Rom> extracted = FileTools.GetArchiveFileInfo(newItem, _logger);

					// Cue to delete the file if it's a copy
					if (_copyFiles && item != newItem)
					{
						FileTools.DeleteFile(newItem);
					}

					foreach (Rom rom in extracted)
					{
						ProcessFileHelper(newItem,
							rom,
							_basePath,
							(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, _basePath.Length) + Path.GetFileNameWithoutExtension(item));
					}
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(newItem))
				{
					ProcessFile(newItem, newBasePath, "");

					// Cue to delete the file if it's a copy
					if (_copyFiles && item != newItem)
					{
						FileTools.DeleteFile(newItem);
					}
				}
			}
			// Otherwise, attempt to extract the files to the temporary directory
			else
			{
				bool encounteredErrors = FileTools.ExtractArchive(newItem,
					tempSubDir,
					(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
					(!_archivesAsFiles && _enableGzip ? ArchiveScanLevel.Internal : ArchiveScanLevel.External),
					(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
					(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
					_logger);

				// If the file was an archive and was extracted successfully, check it
				if (!encounteredErrors)
				{
					// Cue to delete the file if it's a copy
					if (_copyFiles && item != newItem)
					{
						FileTools.DeleteFile(newItem);
					}

					_logger.Log(Path.GetFileName(item) + " treated like an archive");
					List<string> extracted = Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(extracted,
						new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
						entry =>
					{
						ProcessFile(entry,
							tempSubDir,
							Path.Combine((_datdata.Type == "SuperDAT"
								? (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, _basePath.Length)
								: ""),
							Path.GetFileNameWithoutExtension(item)));
					});
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(newItem))
				{
					ProcessFile(newItem, newBasePath, "");

					// Cue to delete the file if it's a copy
					if (_copyFiles && item != newItem)
					{
						FileTools.DeleteFile(newItem);
					}
				}
			}

			// Delete the sub temp directory
			if (Directory.Exists(tempSubDir))
			{
				Directory.Delete(tempSubDir, true);
			}
		}

		/// <summary>
		/// Process a single file as a file
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		private void ProcessFile(string item, string basepath, string parent)
		{
			_logger.Log(Path.GetFileName(item) + " treated like a file");
			Rom rom = FileTools.GetSingleFileInfo(item, noMD5:_noMD5, noSHA1: _noSHA1, date: _addDate);

			ProcessFileHelper(item, rom, basepath, parent);
		}

		/// <summary>
		/// Process a single file as a file (with found Rom data)
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="item">Rom data to be used to write to file</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		private void ProcessFileHelper(string item, DatItem datItem, string basepath, string parent)
		{
			// If the datItem isn't a Rom or Disk, return
			if (datItem.Type != ItemType.Rom && datItem.Type != ItemType.Disk)
			{
				return;
			}

			string key = "";
			if (datItem.Type == ItemType.Rom)
			{
				key = ((Rom)datItem).Size + "-" + ((Rom)datItem).CRC;
			}
			else
			{
				key = ((Disk)datItem).MD5;
			}

			// Add the list if it doesn't exist already
			lock (_datdata.Files)
			{
				if (!_datdata.Files.ContainsKey(key))
				{
					_datdata.Files.Add(key, new List<DatItem>());
				}
			}

			try
			{
				// If the basepath ends with a directory separator, remove it
				if (!basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					basepath += Path.DirectorySeparatorChar.ToString();
				}

				// Make sure we have the full item path
				item = Path.GetFullPath(item);

				// Get the data to be added as game and item names
				string gamename = "";
				string romname = "";

				// If the parent is blank, then we have a non-archive file
				if (parent == "")
				{
					// If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
					if (_datdata.Type == "SuperDAT")
					{
						gamename = Path.GetDirectoryName(item.Remove(0, basepath.Length));
						romname = Path.GetFileName(item);
					}

					// Otherwise, we want just the top level folder as the game, and the file as everything else
					else
					{
						gamename = item.Remove(0, basepath.Length).Split(Path.DirectorySeparatorChar)[0];
						romname = item.Remove(0, (Path.Combine(basepath, gamename).Length));
					}
				}
				
				// Otherwise, we assume that we have an archive
				else
				{
					// If we have a SuperDAT, we want the archive name as the game, and the file as everything else (?)
					if (_datdata.Type == "SuperDAT")
					{
						gamename = parent;
						romname = item.Remove(0, basepath.Length);
					}

					// Otherwise, we want the archive name as the game, and the file as everything else
					else
					{
						gamename = parent;
						romname = item.Remove(0, basepath.Length);
					}
				}

				// Sanitize the names
				if (gamename.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					gamename = gamename.Substring(1);
				}
				if (gamename.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					gamename = gamename.Substring(0, gamename.Length - 1);
				}
				if (romname.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					romname = romname.Substring(1);
				}
				if (romname.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					romname = romname.Substring(0, romname.Length - 1);
				}

				// Update rom information
				datItem.Name = romname;
				datItem.MachineName = gamename;
				datItem.MachineDescription = gamename;

				// Add the file information to the DAT
				lock (_datdata.Files)
				{
					_datdata.Files[key].Add(datItem);
				}

				_logger.User("File added: " + romname + Environment.NewLine);
			}
			catch (IOException ex)
			{
				_logger.Error(ex.ToString());
				return;
			}
		}
	}
}
