﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
	public class DATFromDir
	{
		// Path-related variables
		private string _basePath;
		private string _tempDir;

		// User specified inputs
		private List<String> _inputs;
		private DatData _datdata;
		private bool _noMD5;
		private bool _noSHA1;
		private bool _bare;
		private bool _archivesAsFiles;
		private bool _enableGzip;

		// Other required variables
		private Logger _logger;

		/// <summary>
		/// Create a new DATFromDir object
		/// </summary>
		/// <param name="inputs">A List of Strings representing the files and folders to be DATted</param>
		/// <param name="datdata">DatData object representing the requested output DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>>
		/// <param name="logger">Logger object for console and file output</param>
		public DATFromDir(List<String> inputs, DatData datdata, bool noMD5, bool noSHA1, bool bare, bool archivesAsFiles, bool enableGzip, Logger logger)
		{
			_inputs = inputs;
			_datdata = datdata;
			_noMD5 = noMD5;
			_noSHA1 = noSHA1;
			_bare = bare;
			_archivesAsFiles = archivesAsFiles;
			_enableGzip = enableGzip;
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
			bool noMD5 = false, noSHA1 = false, forceunpack = false, archivesAsFiles = false, old = false, superDat = false, bare = false, romba = false, enableGzip = false;
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
					case "-b":
					case "--bare":
						bare = true;
						break;
					case "-f":
					case "--files":
						archivesAsFiles = true;
						break;
					case "-gz":
					case "--gz-files":
						enableGzip = true;
						break;
					case "-m":
					case "--noMD5":
						noMD5 = true;
						break;
					case "-o":
					case "--old":
						old = true;
						break;
					case "-ro":
					case "--romba":
						romba = true;
						break;
					case "-s":
					case "--noSHA1":
						noSHA1 = true;
						break;
					case "-sd":
					case "--superdat":
						superDat = true;
						break;
					case "-u":
					case "--unzip":
						forceunpack = true;
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
			DatData datdata = new DatData
			{
				Name = name,
				Description = desc,
				Category = cat,
				Version = version,
				Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Author = author,
				ForcePacking = (forceunpack ? ForcePacking.Unzip : ForcePacking.None),
				OutputFormat = (old ? OutputFormat.ClrMamePro : OutputFormat.Xml),
				Romba = romba,
				Type = (superDat ? "SuperDAT" : ""),
				Roms = new Dictionary<string, List<RomData>>(),
			};
			DATFromDir dfd = new DATFromDir(inputs, datdata, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, logger);
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
		/// <remarks>Try to get the hashing multithreaded (either on a per-hash or per-file level)</remarks>
		public bool Start()
		{
			// Double check to see what it needs to be named
			_basePath = (_inputs.Count > 0 ? (File.Exists(_inputs[0]) ? _inputs[0] : _inputs[0] + Path.DirectorySeparatorChar) : "");
			_basePath = (_basePath != "" ? Path.GetFullPath(_basePath) : "");
			if (String.IsNullOrEmpty(_datdata.Name))
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
			}
			_datdata.Name = (String.IsNullOrEmpty(_datdata.Name) ? "Default" : _datdata.Name);
			_datdata.Description = (String.IsNullOrEmpty(_datdata.Description) ? _datdata.Name + (_bare ? "" : " (" + _datdata.Date + ")") : _datdata.Description);

			// Create and open the output file for writing
			FileStream fs = File.Create(Style.CreateOutfileName(Environment.CurrentDirectory, _datdata));
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.AutoFlush = true;

			// Write out the initial file header
			Output.WriteHeader(sw, _datdata, _logger);

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
					lastparent = ProcessFile(_basePath, sw, lastparent);
				}
				else if (Directory.Exists(_basePath))
				{
					_logger.Log("Folder found: " + _basePath);

					// Process the files in the base folder first
					foreach (string item in Directory.EnumerateFiles(_basePath, "*", SearchOption.TopDirectoryOnly))
					{
						lastparent = ProcessFile(item, sw, lastparent);
					}

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
						foreach (string subitem in Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories))
						{
							items = true;
							lastparent = ProcessFile(subitem, sw, lastparent);
						}

						// If there were no subitems, add a "blank" game to to the set
						if (!items)
						{
							string actualroot = item.Remove(0, basePathBackup.Length);
							RomData rom = new RomData
							{
								Name = "null",
								Game = (_datdata.Type == "SuperDAT" ?
									_datdata.Name + (actualroot != "" && !actualroot.StartsWith(Path.DirectorySeparatorChar.ToString()) ?
										Path.DirectorySeparatorChar.ToString() :
										"") + actualroot :
									actualroot),
								Size = -1,
								CRC = "null",
								MD5 = "null",
								SHA1 = "null",
							};

							string key = rom.Size + "-" + rom.CRC;
							if (_datdata.Roms.ContainsKey(key))
							{
								_datdata.Roms[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
								temp.Add(rom);
								_datdata.Roms.Add(key, temp);
							}
						}

						// Now scour subdirectories for empties and add those as well
						foreach (string subdir in Directory.EnumerateDirectories(item, "*", SearchOption.AllDirectories))
						{
							if (Directory.EnumerateFiles(subdir, "*", SearchOption.AllDirectories).Count() == 0)
							{
								string actualroot = subdir.Remove(0, basePathBackup.Length);
								RomData rom = new RomData
								{
									Name = "null",
									Game = (_datdata.Type == "SuperDAT" ?
										_datdata.Name + (actualroot != "" && !actualroot.StartsWith(Path.DirectorySeparatorChar.ToString()) ?
											Path.DirectorySeparatorChar.ToString() :
											"") + actualroot :
										actualroot),
									Size = -1,
									CRC = "null",
									MD5 = "null",
									SHA1 = "null",
								};

								string key = rom.Size + "-" + rom.CRC;
								if (_datdata.Roms.ContainsKey(key))
								{
									_datdata.Roms[key].Add(rom);
								}
								else
								{
									List<RomData> temp = new List<RomData>();
									temp.Add(rom);
									_datdata.Roms.Add(key, temp);
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

			// Now output any empties to the stream
			foreach (List<RomData> roms in _datdata.Roms.Values)
			{
				for (int i = 0; i < roms.Count; i++)
				{
					RomData rom = roms[i];

					// If we're in a mode that doesn't allow for actual empty folders, add the blank info
					if (_datdata.OutputFormat != OutputFormat.SabreDat && _datdata.OutputFormat != OutputFormat.MissFile)
					{
						rom.Type = "rom";
						rom.Name = "-";
						rom.Size = Constants.SizeZero;
						rom.CRC = Constants.CRCZero;
						rom.MD5 = Constants.MD5Zero;
						rom.SHA1 = Constants.SHA1Zero;
					}

					// If we have a different game and we're not at the start of the list, output the end of last item
					int last = 0;
					if (lastparent != null && lastparent.ToLowerInvariant() != rom.Game.ToLowerInvariant())
					{
						Output.WriteEndGame(sw, rom, new List<string>(), new List<string>(), lastparent, _datdata, 0, out last, _logger);
					}

					// If we have a new game, output the beginning of the new item
					if (lastparent == null || lastparent.ToLowerInvariant() != rom.Game.ToLowerInvariant())
					{
						Output.WriteStartGame(sw, rom, new List<string>(), lastparent, _datdata, 0, last, _logger);
					}

					// Write out the rom data
					if (_datdata.OutputFormat != OutputFormat.SabreDat && _datdata.OutputFormat != OutputFormat.MissFile)
					{
						Output.WriteRomData(sw, rom, lastparent, _datdata, 0, _logger);
					}
					
					lastparent = rom.Game;
				}
			}

			// Now write the final piece and close the output stream
			Output.WriteFooter(sw, _datdata, 0, _logger);
			sw.Close();

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		/// <param name="sw">StreamWriter representing the output file</param>
		private string ProcessFile(string item, StreamWriter sw, string lastparent = "")
		{
			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (_datdata.Romba)
			{
				string datum = Path.GetFileNameWithoutExtension(item).ToLowerInvariant();

				// Check if the name is the right length
				if (!Regex.IsMatch(datum, @"^[0-9a-f]{40}"))
				{
					_logger.Warning("Non SHA-1 filename found, skipping: '" + datum + "'");
					return "";
				}

				RomData rom = new RomData
				{
					Type = "rom",
					Game = datum,
					Name = datum,
					Size = (new FileInfo(item)).Length,
					SHA1 = Path.GetFileNameWithoutExtension(item),
				};

				int last = 0;
				Output.WriteStartGame(sw, rom, new List<string>(), "", _datdata, 0, 0, _logger);
				Output.WriteRomData(sw, rom, "", _datdata, 0, _logger);
				Output.WriteEndGame(sw, rom, new List<string>(), new List<string>(), "", _datdata, 0, out last, _logger);

				_logger.Log("File added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
				return "";
			}

			// Create the temporary output directory
			bool encounteredErrors = true;
			if (!_archivesAsFiles)
			{
				IArchive archive = null;
				try
				{
					archive = ArchiveFactory.Open(item);
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
					else if (at == ArchiveType.GZip && _enableGzip)
					{
						archive.Dispose();
						_tempDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "temp" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.DirectorySeparatorChar;
						DirectoryInfo di = Directory.CreateDirectory(_tempDir);

						using (FileStream itemstream = File.OpenRead(item))
						{
							using (FileStream outstream = File.Create(_tempDir + Path.GetFileNameWithoutExtension(item)))
							{
								using (GZipStream gz = new GZipStream(itemstream, CompressionMode.Decompress))
								{
									gz.CopyTo(outstream);
								}
							}
						}
						encounteredErrors = false;
					}
					archive.Dispose();
				}
				catch (InvalidOperationException)
				{
					encounteredErrors = true;
					if (archive != null)
					{
						archive.Dispose();
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex.ToString());
					encounteredErrors = true;
					if (archive != null)
					{
						archive.Dispose();
					}
				}
			}

			// Get a list of files including size and hashes
			Crc32 crc = new Crc32();
			MD5 md5 = MD5.Create();
			SHA1 sha1 = SHA1.Create();

			// If the file was an archive and was extracted successfully, check it
			if (!encounteredErrors)
			{
				string lastgame = null;
				int last = 0;

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
					if (_datdata.Type == "SuperDAT")
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
						Game = (_datdata.Type == "SuperDAT" ?
							_datdata.Name + (actualroot != "" && !actualroot.StartsWith(Path.DirectorySeparatorChar.ToString()) ?
								Path.DirectorySeparatorChar.ToString() :
								"") + actualroot :
							actualroot),
						Name = actualitem,
						Size = (new FileInfo(entry)).Length,
						CRC = fileCRC,
						MD5 = fileMD5,
						SHA1 = fileSHA1,
					};

					// If we have a different game and we're not at the start of the list, output the end of last item
					if (lastgame != null && lastgame.ToLowerInvariant() != rom.Game.ToLowerInvariant())
					{
						Output.WriteEndGame(sw, rom, new List<string>(), new List<string>(), lastgame, _datdata, 0, out last, _logger);
					}
					// If we have a different game and we're not at the start of the list, output the end of the last item
					else if (lastparent != null && lastparent.ToLowerInvariant() != rom.Game.ToLowerInvariant())
					{
						Output.WriteEndGame(sw, rom, new List<string>(), new List<string>(), lastparent, _datdata, 0, out last, _logger);
					}

					// If we have a new game, output the beginning of the new item
					if (lastgame == null || lastgame.ToLowerInvariant() != rom.Game.ToLowerInvariant())
					{
						Output.WriteStartGame(sw, rom, new List<string>(), lastgame, _datdata, 0, last, _logger);
					}

					// Write out the rom data
					Output.WriteRomData(sw, rom, lastgame, _datdata, 0, _logger);
					_logger.User("File added: " + entry + Environment.NewLine);

					lastgame = rom.Game;
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
					actualroot = (actualroot == "" && _datdata.Type != "SuperDAT" ? _basePath.Split(Path.DirectorySeparatorChar).Last() : actualroot);
					string actualitem = (item == _basePath ? item : item.Remove(0, _basePath.Length + 1));

					// If we're in SuperDAT mode, make sure the added item is by itself
					if (_datdata.Type == "SuperDAT")
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
						Game = (_datdata.Type == "SuperDAT" ?
							_datdata.Name + (actualroot != "" && !actualroot.StartsWith(Path.DirectorySeparatorChar.ToString()) ?
								Path.DirectorySeparatorChar.ToString() :
								"") + actualroot :
							actualroot),
						Name = actualitem,
						Size = (new FileInfo(item)).Length,
						CRC = fileCRC,
						MD5 = fileMD5,
						SHA1 = fileSHA1,
					};

					// If we have a different game and we're not at the start of the list, output the end of last item
					int last = 0;
					if (lastparent != null && lastparent.ToLowerInvariant() != rom.Game.ToLowerInvariant())
					{
						Output.WriteEndGame(sw, rom, new List<string>(), new List<string>(), lastparent, _datdata, 0, out last, _logger);
					}

					// If we have a new game, output the beginning of the new item
					if (lastparent == null || lastparent.ToLowerInvariant() != rom.Game.ToLowerInvariant())
					{
						Output.WriteStartGame(sw, rom, new List<string>(), lastparent, _datdata, 0, last, _logger);
					}

					// Write out the rom data
					Output.WriteRomData(sw, rom, lastparent, _datdata, 0, _logger);
					_logger.User("File added: " + actualitem + Environment.NewLine);

					return rom.Game;
				}
				catch (IOException ex)
				{
					_logger.Error(ex.ToString());
					return null;
				}
			}
			return "";
		}
	}
}
