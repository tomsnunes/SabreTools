using SabreTools.Helper;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SabreTools
{
	/// <summary>
	/// Create a DAT file from a specified file, directory, or set thereof
	/// </summary>
	public class DATFromDirParallel
	{
		// Path-related variables
		private string _basePath;
		private string _tempDir;

		// User specified inputs
		private Dat _datdata;
		private bool _noMD5;
		private bool _noSHA1;
		private bool _bare;
		private bool _archivesAsFiles;
		private bool _enableGzip;
		private bool _addblanks;
		private int _maxDegreeOfParallelism;

		// Other required variables
		private Logger _logger;

		// Public variables
		public Dat DatData
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
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logger object for console and file output</param>
		public DATFromDirParallel(string basePath, Dat datdata, bool noMD5, bool noSHA1, bool bare,
			bool archivesAsFiles, bool enableGzip, string tempDir, int maxDegreeOfParallelism, Logger logger)
		{
			_basePath = Path.GetFullPath(basePath);
			_datdata = datdata;
			_datdata.Files = new Dictionary<string, List<Rom>>();
			_datdata.Files.Add("null", new List<Rom>());
			_noMD5 = noMD5;
			_noSHA1 = noSHA1;
			_bare = bare;
			_archivesAsFiles = archivesAsFiles;
			_enableGzip = enableGzip;
			_addblanks = true;
			_tempDir = tempDir;
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
			Parallel.ForEach(Directory.EnumerateFiles(_basePath, "*", SearchOption.AllDirectories),
				new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
				item =>
			{
				ProcessPossibleArchive(item);
			});

			// Now find all folders that are empty, if we are supposed to
			if (!_datdata.Romba && _addblanks)
			{
				Parallel.ForEach(Directory.EnumerateDirectories(_basePath, "*", SearchOption.AllDirectories),
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

						Rom blankrom = new Rom
						{
							Name = romname,
							Machine = new Machine
							{
								Name = gamename,
							},
							HashData = new Hash
							{
								Size = -1,
								CRC = "null",
								MD5 = "null",
								SHA1 = "null",
							},
						};

						_datdata.Files["null"].Add(blankrom);
					}
				});
			}

			// Now that we're done, delete the temp folder
			try
			{
				Directory.Delete(_tempDir, true);
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
			string tempSubDir = (String.IsNullOrEmpty(_tempDir) ? Environment.CurrentDirectory : _tempDir);
			tempSubDir = Path.GetFullPath(Path.Combine(tempSubDir, "__temp__", Path.GetFileNameWithoutExtension(item))) + Path.DirectorySeparatorChar;

			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (_datdata.Romba)
			{
				Rom rom = FileTools.GetTorrentGZFileInfo(item, _logger);

				// If the rom is valid, write it out
				if (rom.Name != null)
				{
					// Add the list if it doesn't exist already
					string key = rom.HashData.Size + "-" + rom.HashData.CRC;
					if (!_datdata.Files.ContainsKey(key))
					{
						_datdata.Files.Add(key, new List<Rom>());
					}

					_datdata.Files[key].Add(rom);
					_logger.User("File added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
				}
				else
				{
					_logger.User("File not added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
					return;
				}
				
				return;
			}

			// If both deep hash skip flags are set, do a quickscan
			if (_noMD5 && _noSHA1)
			{
				ArchiveType? type = FileTools.GetCurrentArchiveType(item, _logger);

				// If we have an archive, scan it
				if (type != null && !_archivesAsFiles)
				{
					List<Rom> extracted = FileTools.GetArchiveFileInfo(item, _logger);
					foreach (Rom rom in extracted)
					{
						ProcessFileHelper(item, rom, _basePath,
							(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, _basePath.Length) + Path.GetFileNameWithoutExtension(item), _datdata);
					}
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(item))
				{
					ProcessFile(item, _basePath, "", _datdata);
				}
			}
			// Otherwise, attempt to extract the files to the temporary directory
			else
			{
				bool encounteredErrors = FileTools.ExtractArchive(item,
					tempSubDir,
					(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
					(!_archivesAsFiles && _enableGzip ? ArchiveScanLevel.Internal : ArchiveScanLevel.External),
					(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
					(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
					_logger);

				// If the file was an archive and was extracted successfully, check it
				if (!encounteredErrors)
				{
					_logger.Log(Path.GetFileName(item) + " treated like an archive");
					Parallel.ForEach(Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories),
						new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
						entry =>
					{
						ProcessFile(entry, tempSubDir, Path.GetFileNameWithoutExtension(item), _datdata);
					});

					// Clear the temp directory
					if (Directory.Exists(tempSubDir))
					{
						FileTools.CleanDirectory(tempSubDir);
					}
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(item))
				{
					ProcessFile(item, _basePath, "", _datdata);
				}
			}
		}

		/// <summary>
		/// Process a single file as a file
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="datdata">DatData object with output information</param>
		private void ProcessFile(string item, string basepath, string parent, Dat datdata)
		{
			_logger.Log(Path.GetFileName(item) + " treated like a file");
			Rom rom = FileTools.GetSingleFileInfo(item, _noMD5, _noSHA1);

			ProcessFileHelper(item, rom, basepath, parent, datdata);
		}

		/// <summary>
		/// Process a single file as a file (with found Rom data)
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="rom">Rom data to be used to write to file</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="datdata">DatData object with output information</param>
		private void ProcessFileHelper(string item, Rom rom, string basepath, string parent, Dat datdata)
		{
			// Add the list if it doesn't exist already
			string key = rom.HashData.Size + "-" + rom.HashData.CRC;
			if (!datdata.Files.ContainsKey(key))
			{
				datdata.Files.Add(key, new List<Rom>());
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
					if (datdata.Type == "SuperDAT")
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
					if (datdata.Type == "SuperDAT")
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
				rom.Machine = new Machine
				{
					Name = gamename,
					Description = gamename,
				};
				rom.Name = romname;

				// Add the file information to the DAT
				datdata.Files[key].Add(rom);

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
