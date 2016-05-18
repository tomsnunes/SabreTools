using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

using SabreTools.Helper;
using DamienG.Security.Cryptography;
using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace SabreTools
{
	/// <summary>
	/// Create a DAT file from a specified file, directory, or set thereof
	/// </summary>
	/// <remarks>Add SuperDAT functionality</remarks>
	public class DATFromDir
	{
		// Path-related variables
		private string _basePath;
		private string _tempDir;

		// Extraction and listing related variables
		private Dictionary<string, List<RomData>> _dict;
		private List<String> _inputs;

		// User specified flags
		private bool _noMD5;
		private bool _noSHA1;
		private bool _bare;
		private bool _forceunpack;
		private bool _archivesAsFiles;
		private bool _old;
		private bool _superDat;

		// User specified strings
		private string _name;
		private string _desc;
		private string _cat;
		private string _version;
		private string _author;

		// Other required variables
		private string _date = DateTime.Now.ToString("yyyy-MM-dd");
		private Logger _logger;

		/// <summary>
		/// Create a new DATFromDir object
		/// </summary>
		/// <param name="inputs">A List of Strings representing the files and folders to be DATted</param>
		/// <param name="name">Internal name of the DAT</param>
		/// <param name="desc">Description and external name of the DAT</param>
		/// <param name="cat">Category for the DAT</param>
		/// <param name="version">Version of the DAT</param>
		/// <param name="author">Author of the DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="forceunpack">True if the forcepacking="unzip" tag is to be added, false otherwise</param>
		/// <param name="archivesAsFiles">True if all archives should be treated like files, false otherwise</param>
		/// <param name="old">True if a old-style DAT should be output, false otherwise</param>
		/// <param name="superDat">True if SuperDAT mode is enabled, false otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>
		public DATFromDir(List<String> inputs, string name, string desc, string cat, string version, string author,
			bool noMD5, bool noSHA1, bool bare, bool forceunpack, bool archivesAsFiles, bool old, bool superDat, Logger logger)
		{
			_inputs = inputs;
			_name = name;
			_desc = desc;
			_cat = cat;
			_version = version;
			_author = author;
			_noMD5 = noMD5;
			_noSHA1 = noSHA1;
			_bare = bare;
			_forceunpack = forceunpack;
			_archivesAsFiles = archivesAsFiles;
			_old = old;
			_superDat = superDat;
			_logger = logger;
		}

		/// <summary>
		/// Start help or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			Console.Clear();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			Logger logger = new Logger(true, "datfromdir.log");
			logger.Start();

			// First things first, take care of all of the arguments that this could have
			bool noMD5 = false, noSHA1 = false, forceunpack = false, archivesAsFiles = false, old = false, superDat = false, bare = false;
			string name = "", desc = "", cat = "", version = "", author = "";
			List<string> inputs = new List<string>();
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-h":
					case "-?":
					case "--help":
						Build.Help();
						logger.Close();
						return;
					case "-m":
					case "--noMD5":
						noMD5 = true;
						break;
					case "-s":
					case "--noSHA1":
						noSHA1 = true;
						break;
					case "-b":
					case "--bare":
						bare = true;
						break;
					case "-u":
					case "--unzip":
						forceunpack = true;
						break;
					case "-f":
					case "--files":
						archivesAsFiles = true;
						break;
					case "-o":
					case "--old":
						old = true;
						break;
					case "-sd":
					case "--superdat":
						superDat = true;
						break;
					default:
						if (arg.StartsWith("-n=") || arg.StartsWith("--name="))
						{
							name = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-d=") || arg.StartsWith("--desc="))
						{
							desc = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-c=") || arg.StartsWith("--cat="))
						{
							cat = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-au=") || arg.StartsWith("--author="))
						{
							author = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-v=") || arg.StartsWith("--version="))
						{
							version = arg.Split('=')[1];
						}
						else
						{
							inputs.Add(arg);
						}
						break;
				}
			}

			// If there's no inputs, show the help
			if (inputs.Count == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("DATFromDir");

			// If any of the inputs are not valid, show the help
			foreach (string input in inputs)
			{
				if (!File.Exists(input) && !Directory.Exists(input))
				{
					logger.Error(input + " is not a valid input!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// Create a new DATFromDir object and process the inputs
			DATFromDir dfd = new DATFromDir(inputs, name, desc, cat, version, author, noMD5, noSHA1, bare, forceunpack, archivesAsFiles, old, superDat, logger);
			bool success = dfd.Start();

			// If we failed, show the help
			if (!success)
			{
				Console.WriteLine();
				Build.Help();
			}
			
			logger.Close();
		}

		/// <summary>
		/// Process the file, folder, or list of some combination into a DAT file
		/// </summary>
		/// <returns>True if the DAT could be created, false otherwise</returns>
		public bool Start()
		{
			// Create an output dictionary for all found items
			_dict = new Dictionary<string, List<RomData>>();

			// Loop over each of the found paths, if any
			foreach (string path in _inputs)
			{
				// Set local paths and vars
				_basePath = (File.Exists(path) ? path : path + Path.DirectorySeparatorChar);
				_basePath = Path.GetFullPath(_basePath);

				// This is where the main loop would go
				if (File.Exists(_basePath))
				{
					ProcessFile(_basePath);
				}
				else if (Directory.Exists(_basePath))
				{
					_logger.Log("Folder found: " + _basePath);

					// Process the files in the base folder first
					foreach (string item in Directory.EnumerateFiles(_basePath, "*", SearchOption.TopDirectoryOnly))
					{
						ProcessFile(item);
					}

					// Then process each of the subfolders themselves
					string basePathBackup = _basePath;
					foreach (string item in Directory.EnumerateDirectories(_basePath))
					{
						if (!_superDat)
						{
							_basePath = (File.Exists(item) ? item : item + Path.DirectorySeparatorChar);
							_basePath = Path.GetFullPath(_basePath);
						}
						
						foreach (string subitem in Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories))
						{
							ProcessFile(subitem);
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

			// If we found nothing (error state), exit
			if (_dict.Count == 0)
			{
				return false;
			}

			// Double check to see what it needs to be named
			if (_name == "")
			{
				if (_inputs.Count > 1)
				{
					_name = Environment.CurrentDirectory.Split(Path.DirectorySeparatorChar).Last();
				}
				else
				{
					if (_basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
					{
						_basePath = _basePath.Substring(0, _basePath.Length - 1);
					}
					_name = _basePath.Split(Path.DirectorySeparatorChar).Last();
				}
			}
			_name = (_name == "" ? "Default" : _name);

			// If we're in a superdat, append the folder name to all game names
			if (_superDat)
			{
				List<string> keys = _dict.Keys.ToList();
				foreach (string key in keys)
				{
					List<RomData> newroms = new List<RomData>();
					foreach (RomData rom in _dict[key])
					{
						RomData newrom = rom;
						newrom.Game = _name + (newrom.Game != "" &&
							!newrom.Game.StartsWith(Path.DirectorySeparatorChar.ToString()) ? Path.DirectorySeparatorChar.ToString() : "") + newrom.Game;
						newroms.Add(newrom);
					}
					_dict[key] = newroms;
				}

				_name = _name += " - SuperDAT";
			}

			_desc = (_desc == "" ? _name + (_bare ? "" : " (" + _date + ")") : _desc);

			DatData datdata = new DatData
			{
				Name = _name,
				Description = _desc,
				Version = _version,
				Date = _date,
				Category = _cat,
				Author = _author,
				Type = (_superDat ? "SuperDAT" : ""),
				ForcePacking = (_forceunpack ? ForcePacking.Unzip : ForcePacking.None),
				OutputFormat = (_old ? OutputFormat.ClrMamePro : OutputFormat.Xml),
				Roms = _dict,
			};

			// Now write it all out as a DAT
			Output.WriteDatfile(datdata, Environment.CurrentDirectory, _logger);

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		private void ProcessFile(string item)
		{
			// Create the temporary output directory
			bool encounteredErrors = true;
			if (!_archivesAsFiles)
			{
				try
				{
					IArchive archive = ArchiveFactory.Open(item);
					ArchiveType at = archive.Type;
					_logger.Log("Found archive of type: " + at);

					if (at == ArchiveType.Zip || at == ArchiveType.SevenZip || at == ArchiveType.Rar)
					{
						_tempDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "temp" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.DirectorySeparatorChar;
						DirectoryInfo di = Directory.CreateDirectory(_tempDir);
						IReader reader = archive.ExtractAllEntries();
						reader.WriteAllToDirectory(_tempDir, ExtractOptions.ExtractFullPath);
						encounteredErrors = false;
					}
					archive.Dispose();
				}
				catch (InvalidOperationException)
				{
					encounteredErrors = true;
				}
				catch (Exception ex)
				{
					_logger.Error(ex.ToString());
					encounteredErrors = true;
				}
			}

			// Get a list of files including size and hashes
			Crc32 crc = new Crc32();
			MD5 md5 = MD5.Create();
			SHA1 sha1 = SHA1.Create();

			// If the file was an archive and was extracted successfully, check it
			if (!encounteredErrors)
			{
				_logger.Log(Path.GetFileName(item) + " treated like an archive");
				foreach (string entry in Directory.EnumerateFiles(_tempDir, "*", SearchOption.AllDirectories))
				{
					_logger.Log("Found file: " + entry);
					string fileCRC = String.Empty;
					string fileMD5 = String.Empty;
					string fileSHA1 = String.Empty;

					try
					{
						using (FileStream fs = File.Open(entry, FileMode.Open))
						{
							foreach (byte b in crc.ComputeHash(fs))
							{
								fileCRC += b.ToString("x2").ToLower();
							}
						}
						if (!_noMD5)
						{
							using (FileStream fs = File.Open(entry, FileMode.Open))
							{
								fileMD5 = BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
							}
						}
						if (!_noSHA1)
						{
							using (FileStream fs = File.Open(entry, FileMode.Open))
							{
								fileSHA1 = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
							}
						}
					}
					catch (IOException)
					{
						continue;
					}

					string actualroot = "";
					string actualitem = "";

					actualitem = entry.Remove(0, _tempDir.Length);

					// If we're in SuperDAT mode, make sure the added item is by itself
					if (_superDat)
					{
						actualroot = Path.GetDirectoryName(item.Remove(0, _basePath.Length));
						actualroot = (actualroot == "" ? _basePath.Split(Path.DirectorySeparatorChar).Last() : actualroot);
						actualroot += Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(item) + Path.DirectorySeparatorChar + Path.GetDirectoryName(actualitem);
						actualroot = actualroot.TrimEnd(Path.DirectorySeparatorChar);
						actualitem = Path.GetFileName(actualitem);
					}
					// Otherwise, set the correct root and such
					else
					{
						actualroot = Path.GetFileNameWithoutExtension(item);
						actualroot = actualroot.TrimEnd(Path.DirectorySeparatorChar);
					}

					RomData rom = new RomData
					{
						Type = "rom",
						Game = actualroot,
						Name = actualitem,
						Size = (new FileInfo(entry)).Length,
						CRC = fileCRC,
						MD5 = fileMD5,
						SHA1 = fileSHA1,
					};

					string key = rom.Size + "-" + rom.CRC;
					if (_dict.ContainsKey(key))
					{
						_dict[key].Add(rom);
					}
					else
					{
						List<RomData> temp = new List<RomData>();
						temp.Add(rom);
						_dict.Add(key, temp);
					}

					_logger.User("File added: " + entry + Environment.NewLine);
				}

				// Delete the temp directory
				if (Directory.Exists(_tempDir))
				{
					Directory.Delete(_tempDir, true);
				}
			}
			// Otherwise, just get the info on the file itself
			else if (!Directory.Exists(item) && File.Exists(item))
			{
				_logger.Log(Path.GetFileName(item) + " treated like a file");

				string fileCRC = String.Empty;
				string fileMD5 = String.Empty;
				string fileSHA1 = String.Empty;

				try
				{
					using (FileStream fs = File.Open(item, FileMode.Open))
					{
						foreach (byte b in crc.ComputeHash(fs))
						{
							fileCRC += b.ToString("x2").ToLower();
						}
					}
					if (!_noMD5)
					{
						using (FileStream fs = File.Open(item, FileMode.Open))
						{
							fileMD5 = BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
						}
					}
					if (!_noSHA1)
					{
						using (FileStream fs = File.Open(item, FileMode.Open))
						{
							fileSHA1 = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
						}
					}

					if (_basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
					{
						_basePath = _basePath.Substring(0, _basePath.Length - 1);
					}

					string actualroot = (item == _basePath ? item.Split(Path.DirectorySeparatorChar).Last() : item.Remove(0, _basePath.Length).Split(Path.DirectorySeparatorChar)[0]);
					actualroot = (actualroot == "" && !_superDat ? _basePath.Split(Path.DirectorySeparatorChar).Last() : actualroot);
					string actualitem = (item == _basePath ? item : item.Remove(0, _basePath.Length + 1));

					// If we're in SuperDAT mode, make sure the added item is by itself
					if (_superDat)
					{
						actualroot += (actualroot != "" ? Path.DirectorySeparatorChar.ToString() : "") + Path.GetDirectoryName(actualitem);
						actualroot = actualroot.TrimEnd(Path.DirectorySeparatorChar);
						actualitem = Path.GetFileName(actualitem);
					}

					// Drag and drop is funny
					if (actualitem == Path.GetFullPath(actualitem))
					{
						actualitem = Path.GetFileName(actualitem);
					}

					_logger.Log("Actual item added: " + actualitem);

					RomData rom = new RomData
					{
						Type = "rom",
						Game = actualroot,
						Name = actualitem,
						Size = (new FileInfo(item)).Length,
						CRC = fileCRC,
						MD5 = fileMD5,
						SHA1 = fileSHA1,
					};

					string key = rom.Size + "-" + rom.CRC;
					if (_dict.ContainsKey(key))
					{
						_dict[key].Add(rom);
					}
					else
					{
						List<RomData> temp = new List<RomData>();
						temp.Add(rom);
						_dict.Add(key, temp);
					}

					_logger.User("File added: " + actualitem + Environment.NewLine);
				}
				catch (IOException ex)
				{
					_logger.Error(ex.ToString());
				}
			}
		}
	}
}
