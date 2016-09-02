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
		private List<String> _inputs;
		private Dat _datdata;
		private bool _noMD5;
		private bool _noSHA1;
		private bool _bare;
		private bool _archivesAsFiles;
		private bool _enableGzip;

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
		/// <param name="inputs">A List of Strings representing the files and folders to be DATted</param>
		/// <param name="datdata">DatData object representing the requested output DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="nowrite">True if the file should not be written out, false otherwise (default)</param>
		/// <param name="logger">Logger object for console and file output</param>
		public DATFromDirParallel(List<String> inputs, Dat datdata, bool noMD5, bool noSHA1, bool bare, bool archivesAsFiles, bool enableGzip, string tempDir, Logger logger)
		{
			_inputs = inputs;
			_datdata = datdata;
			_noMD5 = noMD5;
			_noSHA1 = noSHA1;
			_bare = bare;
			_archivesAsFiles = archivesAsFiles;
			_enableGzip = enableGzip;
			_tempDir = tempDir;
			_logger = logger;
		}

		/// <summary>
		/// Process the file, folder, or list of some combination into a DAT file
		/// </summary>
		/// <returns>True if the DAT could be created, false otherwise</returns>
		/// <remarks>Try to get the hashing multithreaded (either on a per-hash or per-file level)</remarks>
		public bool Start()
		{
			// Double check to see what it needs to be named
			_basePath = (_inputs.Count > 0 ? (File.Exists(_inputs[0]) ? _inputs[0] : _inputs[0] + Path.DirectorySeparatorChar) : "");
			_basePath = (_basePath != "" ? Path.GetFullPath(_basePath) : "");

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
				if (_inputs.Count > 1)
				{
					_datdata.Name = Environment.CurrentDirectory.Split(Path.DirectorySeparatorChar).Last();
				}
				else
				{
					if (_basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
					{
						_basePath = _basePath.Substring(0, _basePath.Length - 1);
					}
					_datdata.Name = _basePath.Split(Path.DirectorySeparatorChar).Last();
				}

				// If the name is still somehow empty, populate it with defaults
				_datdata.Name = (String.IsNullOrEmpty(_datdata.Name) ? "Default" : _datdata.Name);
				_datdata.Description = _datdata.Name + (_bare ? "" : " (" + _datdata.Date + ")");
			}

			// Loop over each of the found paths, if any
			string lastparent = null;
			foreach (string path in _inputs)
			{
				// Set local paths and vars
				_basePath = (File.Exists(path) ? path : path + Path.DirectorySeparatorChar);
				_basePath = Path.GetFullPath(_basePath);

				// This is where the main loop would go
				if (File.Exists(_basePath))
				{
					lastparent = ProcessPossibleArchive(_basePath, lastparent);
				}
				else if (Directory.Exists(_basePath))
				{
					_logger.Log("Folder found: " + _basePath);

					// Process the files in the base folder first
					Parallel.ForEach(Directory.EnumerateFiles(_basePath, "*", SearchOption.TopDirectoryOnly), item =>
					{
						lastparent = ProcessPossibleArchive(item, lastparent);
					});

					// Then process each of the subfolders themselves
					string basePathBackup = _basePath;
					foreach (string item in Directory.EnumerateDirectories(_basePath))
					{
						if (_datdata.Type != "SuperDAT")
						{
							_basePath = (File.Exists(item) ? item : item + Path.DirectorySeparatorChar);
							_basePath = Path.GetFullPath(_basePath);
						}

						bool items = false;
						Parallel.ForEach(Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories), subitem =>
						{
							items = true;
							lastparent = ProcessPossibleArchive(subitem, lastparent);
						});

						// In romba mode we ignore empty folders completely
						if (!_datdata.Romba)
						{
							// If there were no subitems, add a "blank" game to to the set (if not in Romba mode)
							if (!items)
							{
								string actualroot = item.Remove(0, basePathBackup.Length);
								Rom rom = new Rom
								{
									Name = "null",
									Machine = new Machine
									{
										Name = (_datdata.Type == "SuperDAT" ?
											(actualroot != "" && !actualroot.StartsWith(Path.DirectorySeparatorChar.ToString()) ?
												Path.DirectorySeparatorChar.ToString() :
												"") + actualroot :
											actualroot),
									},
									HashData = new Hash
									{
										Size = -1,
										CRC = "null",
										MD5 = "null",
										SHA1 = "null",
									},
								};

								string key = rom.HashData.Size + "-" + rom.HashData.CRC;
								if (_datdata.Files.ContainsKey(key))
								{
									_datdata.Files[key].Add(rom);
								}
								else
								{
									List<Rom> temp = new List<Rom>();
									temp.Add(rom);
									_datdata.Files.Add(key, temp);
								}
							}

							// Now scour subdirectories for empties and add those as well (if not in Romba mode)
							foreach (string subdir in Directory.EnumerateDirectories(item, "*", SearchOption.AllDirectories))
							{
								if (Directory.EnumerateFiles(subdir, "*", SearchOption.AllDirectories).Count() == 0)
								{
									string actualroot = subdir.Remove(0, basePathBackup.Length);
									Rom rom = new Rom
									{
										Name = "null",
										Machine = new Machine
										{
											Name = (_datdata.Type == "SuperDAT" ?
												(actualroot != "" && !actualroot.StartsWith(Path.DirectorySeparatorChar.ToString()) ?
													Path.DirectorySeparatorChar.ToString() :
													"") + actualroot :
												actualroot),
										},
										HashData = new Hash
										{
											Size = -1,
											CRC = "null",
											MD5 = "null",
											SHA1 = "null",
										},
									};

									string key = rom.HashData.Size + "-" + rom.HashData.CRC;
									if (_datdata.Files.ContainsKey(key))
									{
										_datdata.Files[key].Add(rom);
									}
									else
									{
										List<Rom> temp = new List<Rom>();
										temp.Add(rom);
										_datdata.Files.Add(key, temp);
									}
								}
							}
						}
					}
					_basePath = basePathBackup;
				}
				// If this somehow skips past the original sensors
				else
				{
					_logger.Error(path + " is not a valid input!");
				}
			}

			// Now output any empties to the stream (if not in Romba mode)
			if (!_datdata.Romba)
			{
				List<string> keys = _datdata.Files.Keys.ToList();
				foreach (string key in keys)
				{
					List<Rom> roms = _datdata.Files[key];
					for (int i = 0; i < roms.Count; i++)
					{
						Rom rom = roms[i];

						// If we're in a mode that doesn't allow for actual empty folders, add the blank info
						if (_datdata.OutputFormat != OutputFormat.SabreDat && _datdata.OutputFormat != OutputFormat.MissFile)
						{
							rom.Type = ItemType.Rom;
							rom.Name = "-";
							rom.HashData.Size = Constants.SizeZero;
							rom.HashData.CRC = Constants.CRCZero;
							rom.HashData.MD5 = Constants.MD5Zero;
							rom.HashData.SHA1 = Constants.SHA1Zero;
						}

						string inkey = rom.HashData.Size + "-" + rom.HashData.CRC;
						if (_datdata.Files.ContainsKey(inkey))
						{
							_datdata.Files[inkey].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							_datdata.Files.Add(inkey, temp);
						}

						lastparent = rom.Machine.Name;
					}
				}

				// If we had roms but not blanks (and not in Romba mode), create an artifical rom for the purposes of outputting
				if (lastparent != null && _datdata.Files.Count == 0)
				{
					_datdata.Files.Add("temp", new List<Rom>());
				}
			}

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		/// <param name="lastparent">Name of the last parent rom to make sure that everything is grouped as well as possible</param>
		/// <returns>New parent to be used</returns>
		private string ProcessPossibleArchive(string item, string lastparent)
		{
			// Define the temporary directory
			string tempdir = (String.IsNullOrEmpty(_tempDir) ? Environment.CurrentDirectory : _tempDir);
			tempdir += (tempdir.EndsWith(Path.DirectorySeparatorChar.ToString()) ? "" : Path.DirectorySeparatorChar.ToString());
			tempdir += "__temp__" + Path.DirectorySeparatorChar;

			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (_datdata.Romba)
			{
				Rom rom = FileTools.GetTorrentGZFileInfo(item, _logger);

				// If the rom is valid, write it out
				if (rom.Name != null)
				{
					string key = rom.HashData.Size + "-" + rom.HashData.CRC;
					if (_datdata.Files.ContainsKey(key))
					{
						_datdata.Files[key].Add(rom);
					}
					else
					{
						List<Rom> temp = new List<Rom>();
						temp.Add(rom);
						_datdata.Files.Add(key, temp);
					}
				}
				else
				{
					return string.Empty;
				}

				_logger.User("File added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
				return rom.Machine.Name;
			}

			// If both deep hash skip flags are set, do a quickscan
			if (_noMD5 && _noSHA1)
			{
				ArchiveType? type = FileTools.GetCurrentArchiveType(item, _logger);

				// If we have an archive, scan it
				if (type != null)
				{
					List<Rom> extracted = FileTools.GetArchiveFileInfo(item, _logger);
					foreach (Rom rom in extracted)
					{
						lastparent = ProcessFileHelper(item, rom, _basePath,
							Path.Combine((Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, _basePath.Length) +
								Path.GetFileNameWithoutExtension(item)
							), _datdata, lastparent);
					}
				}
				// Otherwise, just get the info on the file itself
				else if (!Directory.Exists(item) && File.Exists(item))
				{
					lastparent = ProcessFile(item, _basePath, "", _datdata, lastparent);
				}
			}
			// Otherwise, attempt to extract the files to the temporary directory
			else
			{
				bool encounteredErrors = FileTools.ExtractArchive(item,
				tempdir,
				(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
				(!_archivesAsFiles && _enableGzip ? ArchiveScanLevel.Internal : ArchiveScanLevel.External),
				(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
				(_archivesAsFiles ? ArchiveScanLevel.External : ArchiveScanLevel.Internal),
				_logger);

				// If the file was an archive and was extracted successfully, check it
				if (!encounteredErrors)
				{
					_logger.Log(Path.GetFileName(item) + " treated like an archive");
					Parallel.ForEach(Directory.EnumerateFiles(tempdir, "*", SearchOption.AllDirectories), entry =>
					{
						string tempbasepath = (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar);
						lastparent = ProcessFile(Path.GetFullPath(entry), Path.GetFullPath(tempdir),
							(String.IsNullOrEmpty(tempbasepath)
								? ""
								: (tempbasepath.Length < _basePath.Length
									? tempbasepath
									: tempbasepath.Remove(0, _basePath.Length))) +
							Path.GetFileNameWithoutExtension(item), _datdata, lastparent);
					});

					// Clear the temp directory
					if (Directory.Exists(tempdir))
					{
						FileTools.CleanDirectory(tempdir);
					}
				}
				// Otherwise, just get the info on the file itself
				else if (!Directory.Exists(item) && File.Exists(item))
				{
					lastparent = ProcessFile(item, _basePath, "", _datdata, lastparent);
				}
			}

			return lastparent;
		}

		/// <summary>
		/// Process a single file as a file
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="datdata">DatData object with output information</param>
		/// <param name="lastparent">Last known parent game name</param>
		/// <returns>New last known parent game name</returns>
		private string ProcessFile(string item, string basepath, string parent, Dat datdata, string lastparent)
		{
			_logger.Log(Path.GetFileName(item) + " treated like a file");
			Rom rom = FileTools.GetSingleFileInfo(item, _noMD5, _noSHA1);

			return ProcessFileHelper(item, rom, basepath, parent, datdata, lastparent);
		}

		/// <summary>
		/// Process a single file as a file (with found Rom data)
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="rom">Rom data to be used to write to file</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="datdata">DatData object with output information</param>
		/// <param name="lastparent">Last known parent game name</param>
		/// <returns>New last known parent game name</returns>
		private string ProcessFileHelper(string item, Rom rom, string basepath, string parent, Dat datdata, string lastparent)
		{
			try
			{
				if (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					basepath = basepath.Substring(0, basepath.Length - 1);
				}

				string actualroot = (item == basepath ? item.Split(Path.DirectorySeparatorChar).Last() : item.Remove(0, basepath.Length).Split(Path.DirectorySeparatorChar)[0]);
				if (parent == "")
				{
					actualroot = (actualroot == "" && datdata.Type != "SuperDAT" ? basepath.Split(Path.DirectorySeparatorChar).Last() : actualroot);
				}
				string actualitem = (item == basepath ? item : item.Remove(0, basepath.Length + 1));

				// If we're in SuperDAT mode, make sure the added item is by itself
				if (datdata.Type == "SuperDAT")
				{
					actualroot += (actualroot != "" ? Path.DirectorySeparatorChar.ToString() : "") +
						(parent != "" ? parent + Path.DirectorySeparatorChar : "") +
						Path.GetDirectoryName(actualitem);
					actualroot = actualroot.TrimEnd(Path.DirectorySeparatorChar);
					actualitem = Path.GetFileName(actualitem);
				}
				else if (parent != "")
				{
					actualroot = parent.TrimEnd(Path.DirectorySeparatorChar);
				}

				// Drag and drop is funny
				if (actualitem == Path.GetFullPath(actualitem))
				{
					actualitem = Path.GetFileName(actualitem);
				}

				_logger.Log("Actual item added: " + actualitem);

				// Update rom information
				rom.Machine = new Machine
				{
					Name = (datdata.Type == "SuperDAT" ?
						(actualroot != "" && !actualroot.StartsWith(Path.DirectorySeparatorChar.ToString()) ?
							Path.DirectorySeparatorChar.ToString() :
							"") + actualroot :
						actualroot),
				};
				rom.Machine.Name = rom.Machine.Name.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
				rom.Name = actualitem;

				string key = rom.HashData.Size + "-" + rom.HashData.CRC;
				if (_datdata.Files.ContainsKey(key))
				{
					_datdata.Files[key].Add(rom);
				}
				else
				{
					List<Rom> temp = new List<Rom>();
					temp.Add(rom);
					_datdata.Files.Add(key, temp);
				}
				_logger.User("File added: " + actualitem + Environment.NewLine);

				return rom.Machine.Name;
			}
			catch (IOException ex)
			{
				_logger.Error(ex.ToString());
				return null;
			}
		}
	}
}
