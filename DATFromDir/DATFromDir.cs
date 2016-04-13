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
	public class DATFromDir
	{
		// Path-related variables
		private static string _basePath;
		private static string _tempDir;

		// Extraction and listing related variables
		private static List<RomData> _roms;

		// User specified variables
		private static bool _noMD5;
		private static bool _noSHA1;
		private static bool _noDate;
		private static bool _forceunzip;
		private static bool _allfiles;
		private static bool _old;
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
			_logger = new Logger(false, "datfromdir.log");
			_logger.Start();

			// First things first, take care of all of the arguments that this could have
			_noMD5 = false; _noSHA1 = false; _forceunzip = false; _allfiles = false; _old = false;
			_name = ""; _desc = ""; _cat = ""; _version = ""; _author = "";
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

			// If there's no inputs, show the help
			if (inputs.Count == 0)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// Create an output array for all found items
			_roms = new List<RomData>();

			// Loop over each of the found paths, if any
			foreach (string path in inputs)
			{
				// Set local paths and vars
				_tempDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "temp" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.DirectorySeparatorChar;
				_basePath = (File.Exists(path) ? path : path + Path.DirectorySeparatorChar); 

				// This is where the main loop would go
				if (File.Exists(_basePath))
				{
					ProcessFile(_basePath);
				}
				else
				{
					_logger.Log("Folder found: " + _basePath);
					foreach (string item in Directory.EnumerateFiles(_basePath, "*", SearchOption.AllDirectories))
					{
						ProcessFile(item);
					}
				}
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

			//TODO: So, this below section is a pretty much one for one copy of code that is written in generate
			//		this means that in the future, "writing to DAT" will be abstracted out to the DLL so that any
			//		properly formatted data can be passed in and it will get written as necessary. That would open
			//		the possibiliites for different ways to generate a DAT from multiple things

			// Double check to see what it needs to be named
			string[] splitPath = _basePath.Split(Path.DirectorySeparatorChar);
			_name = (_name == "" ? (inputs.Count > 1 ? Environment.CurrentDirectory.Split(Path.DirectorySeparatorChar).Last() :
				(_basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? splitPath[splitPath.Length - 2] : splitPath.Last())) : _name);
			_desc = (_desc == "" ? _name + (_noDate ? "" : " (" + _date + ")") : _desc);

			// Now write it all out as a DAT
			try
			{
				FileStream fs = File.Create(_desc + ".xml");
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				string header_old = "clrmamepro (\n" +
					"\tname \"" + _name + "\"\n" +
					"\tdescription \"" + _desc + "\"\n" +
					"\tversion \"" + _version + "\"\n" +
					"\tcomment \"\"\n" +
					"\tauthor \"" + _author + "\"\n" +
					(_forceunzip ? "\tforcezipping no\n" : "") + 
					")\n";

				string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
					"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
					"\t<datafile>\n" +
					"\t\t<header>\n" +
					"\t\t\t<name>" + HttpUtility.HtmlEncode(_name) + "</name>\n" +
					"\t\t\t<description>" + HttpUtility.HtmlEncode(_desc) + "</description>\n" +
					"\t\t\t<category>" + HttpUtility.HtmlEncode(_cat) + "</category>\n" +
					"\t\t\t<version>" + HttpUtility.HtmlEncode(_version) + "</version>\n" +
					"\t\t\t<date>" + _date + "</date>\n" +
					"\t\t\t<author>" + HttpUtility.HtmlEncode(_author) + "</author>\n" +
					(_forceunzip ? "\t\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
					"\t\t</header>\n";

				// Write the header out
				sw.Write((_old ? header_old : header));

				// Write out each of the machines and roms
				string lastgame = "";
				foreach (RomData rom in _roms)
				{
					string state = "";
					if (lastgame != "" && lastgame != rom.Game)
					{
						state += (_old ? ")\n" : "\t</machine>\n");
					}

					if (lastgame != rom.Game)
					{
						state += (_old ? "game (\n\tname \"" + rom.Game + "\"\n" +
							"\tdescription \"" + rom.Game + "\"\n" :
							"\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Game) + "\">\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(rom.Game) + "</description>\n");
					}

					if (_old)
					{
						state += "\trom ( name \"" + rom.Name + "\"" +
							(rom.Size != 0 ? " size " + rom.Size : "") +
							(rom.CRC != "" ? " crc " + rom.CRC.ToLowerInvariant() : "") +
							(rom.MD5 != "" ? " md5 " + rom.MD5.ToLowerInvariant() : "") +
							(rom.SHA1 != "" ? " sha1 " + rom.SHA1.ToLowerInvariant() : "") +
							" )\n";
					}
					else
					{
						state += "\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.Size != -1 ? " size=\"" + rom.Size + "\"" : "") +
							(rom.CRC != "" ? " crc=\"" + rom.CRC.ToLowerInvariant() + "\"" : "") +
							(rom.MD5 != "" ? " md5=\"" + rom.MD5.ToLowerInvariant() + "\"" : "") +
							(rom.SHA1 != "" ? " sha1=\"" + rom.SHA1.ToLowerInvariant() + "\"" : "") +
							" />\n";
					}

					lastgame = rom.Game;

					sw.Write(state);
				}

				sw.Write((_old ? ")" : "\t</machine>\n</datafile>"));
				_logger.Log("File written!" + Environment.NewLine);
				sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				_logger.Error(ex.ToString());
			}
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
					IReader reader = archive.ExtractAllEntries();
					reader.WriteAllToDirectory(_tempDir, ExtractOptions.ExtractFullPath);
					encounteredErrors = false;
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

					string actualroot = (item == _basePath ? "Default" : Path.GetDirectoryName(item.Remove(0, _basePath.Length)).Split(Path.DirectorySeparatorChar)[0]);
					actualroot = (actualroot == "" ? "Default" : actualroot);
					string actualitem = (item == _basePath ? item : item.Remove(0, _basePath.Length).Remove(0, (actualroot != "Default" ? actualroot.Length + 1 : 0)));

					// Drag and drop is funny
					if (actualitem == Path.GetFullPath(actualitem))
					{
						actualitem = Path.GetFileName(actualitem);
					}

					_roms.Add(new RomData
					{
						Game = actualroot,
						Name = actualitem,
						Size = (new FileInfo(item)).Length,
						CRC = fileCRC,
						MD5 = fileMD5,
						SHA1 = fileSHA1,
					});

					_logger.Log("File added" + Environment.NewLine);
				}
				catch (IOException) { }
			}

			// Delete the temp directory
			if (Directory.Exists(_tempDir))
			{
				di.Delete(true);
			}
		}
	}
}
