using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

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
		private static string _basePath;
		private static string _tempDir;

		// Extraction and listing related variables
		private static List<RomData> _roms;

		// User specified flags
		private static bool _noMD5;
		private static bool _noSHA1;
		private static bool _noDate;
		private static bool _forceunzip;
		private static bool _allfiles;
		private static bool _old;
		private static bool _log;
		private static bool _superDat;

		// User specified strings
		private static string _name;
		private static string _desc;
		private static string _cat;
		private static string _version;
		private static string _author;

		// Other required variables
		private static string _date = DateTime.Now.ToString("yyyy-MM-dd");
		private static Logger _logger;

		/// <summary>
		/// Start help or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			Console.Clear();
			Console.Title = "DATFromDir " + Build.Version;

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			// First things first, take care of all of the arguments that this could have
			_noMD5 = false; _noSHA1 = false; _forceunzip = false; _allfiles = false; _old = false; _log = false; _superDat = false;
			_name = ""; _desc = ""; _cat = ""; _version = ""; _author = ""; _basePath = "";
			List<string> inputs = new List<string>();
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-h":
					case "-?":
					case "--help":
						Build.Help();
						_logger.Close();
						return;
					case "-m":
					case "--noMD5":
						_noMD5 = true;
						break;
					case "-s":
					case "--noSHA1":
						_noSHA1 = true;
						break;
					case "-b":
					case "--bare":
						_noDate = true;
						break;
					case "-u":
					case "--unzip":
						_forceunzip = true;
						break;
					case "-f":
					case "--files":
						_allfiles = true;
						break;
					case "-o":
					case "--old":
						_old = true;
						break;
					case "-l":
					case "--log":
						_log = true;
						break;
					case "-sd":
					case "--superdat":
						_superDat = true;
						break;
					default:
						if (arg.StartsWith("-n=") || arg.StartsWith("--name="))
						{
							_name = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-d=") || arg.StartsWith("--desc="))
						{
							_desc = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-c=") || arg.StartsWith("--cat="))
						{
							_cat = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-a=") || arg.StartsWith("--author="))
						{
							_author = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-v=") || arg.StartsWith("--version="))
						{
							_version = arg.Split('=')[1];
						}
						else
						{
							inputs.Add(arg);
						}
						break;
				}
			}

			_logger = new Logger(_log, "datfromdir.log");
			_logger.Start();

			// If there's no inputs, show the help
			if (inputs.Count == 0)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// If any of the inputs are not valid, show the help
			foreach (string input in inputs)
			{
				if (!File.Exists(input) && !Directory.Exists(input))
				{
					_logger.Error(input + " is not a valid input!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// Create an output array for all found items
			_roms = new List<RomData>();

			/*
			For clarity, here is the process for SuperDAT:
			1) Check to see if the input is a directory
			2) If it is, loop through and get ONLY the directories
			3) Treat each subdirectory like a base path
			4) Process each subdirectory like a normal one
			5) Prefix any added game names with the parent directory name
			6) Process like normal
			*/

			// If we're in SuperDAT mode, we have to treat it separately
			if (_superDat)
			{

			}
			else
			{
				// Loop over each of the found paths, if any
				foreach (string path in inputs)
				{
					// Set local paths and vars
					_tempDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "temp" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.DirectorySeparatorChar;

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
						foreach (string item in Directory.EnumerateFiles(_basePath, "*", SearchOption.AllDirectories))
						{
							ProcessFile(item);
						}
					}
					// If this somehow skips past the original sensors
					else
					{
						_logger.Error(path + " is not a valid input!");
					}
				}
			}

			// If we found nothing (error state), show the help and exit
			if (_roms.Count == 0)
			{
				Console.WriteLine();
				Build.Help();
				return;
			}

			// Order the roms by name of parent, then name of rom
			_roms.Sort(delegate (RomData A, RomData B)
			{
				if (A.Game == B.Game)
				{
					if (A.Name == B.Name)
					{
						return (int)(A.Size - B.Size);
					}
					return String.Compare(A.Name, B.Name);
				}
				return String.Compare(A.Game, B.Game);
			});

			// Double check to see what it needs to be named
			if (_name == "")
			{
				if (inputs.Count > 1)
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
			_desc = (_desc == "" ? _name + (_noDate ? "" : " (" + _date + ")") : _desc);

			// Now write it all out as a DAT
			Output.WriteToDat(_name, _desc, _version, _date, _cat, _author, _forceunzip, _old, Environment.CurrentDirectory, _roms, _logger);
			
			_logger.Close();
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		private static void ProcessFile (string item)
		{
			// Create the temporary output directory
			DirectoryInfo di = Directory.CreateDirectory(_tempDir);

			bool encounteredErrors = true;
			if (!_allfiles)
			{
				try
				{
					IArchive archive = ArchiveFactory.Open(item);
					ArchiveType at = archive.Type;
					_logger.Log("Found archive of type: " + at);

					if (at == ArchiveType.Zip || at == ArchiveType.SevenZip || at == ArchiveType.Rar)
					{
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
					_logger.Log("\tFound file: " + entry);
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

					_roms.Add(new RomData
					{
						Type = "rom",
						Game = Path.GetFileNameWithoutExtension(item),
						Name = entry.Remove(0, _tempDir.Length),
						Size = (new FileInfo(entry)).Length,
						CRC = fileCRC,
						MD5 = fileMD5,
						SHA1 = fileSHA1,
					});

					_logger.Log("File added" + Environment.NewLine);
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
					actualroot = (actualroot == "" ? _basePath.Split(Path.DirectorySeparatorChar).Last() : actualroot);
					string actualitem = (item == _basePath ? item : item.Remove(0, _basePath.Length + 1)); //.Remove(0, actualroot.Length)

					// Drag and drop is funny
					if (actualitem == Path.GetFullPath(actualitem))
					{
						actualitem = Path.GetFileName(actualitem);
					}

					_logger.Log("Actual item added: " + actualitem);

					_roms.Add(new RomData
					{
						Type = "rom",
						Game = actualroot,
						Name = actualitem,
						Size = (new FileInfo(item)).Length,
						CRC = fileCRC,
						MD5 = fileMD5,
						SHA1 = fileSHA1,
					});

					_logger.Log("File added" + Environment.NewLine);
				}
				catch (IOException ex)
				{
					_logger.Error(ex.ToString());
				}
			}

			// Delete the temp directory
			if (Directory.Exists(_tempDir))
			{
				di.Delete(true);
			}
		}
	}
}
