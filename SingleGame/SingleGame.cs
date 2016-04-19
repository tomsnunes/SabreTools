using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using SabreTools.Helper;

namespace SabreTools
{
	public class SingleGame
	{
		private static string _filename = "";
		private static string _path = "";
		private static bool _rename = true;
		private static bool _forceunpack = true;
		private static Logger logger;

		public static void Main(string[] args)
		{
			Console.Clear();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			if (args.Length == 0)
			{
				Build.Help();
				return;
			}

			logger = new Logger(false, "singlegame.log");
			logger.Start();

			// Output the title
			Build.Start("SingleGame");

			_filename = args[0];

			bool tofile = false;
			if (args.Length > 1)
			{
				for (int i = 1; i < args.Length; i++)
				{
					switch (args[i])
					{
						case "-n":
							_rename = false;
							break;
						case "-z":
							_forceunpack = false;
							break;
						case "-l":
						case "--log":
							tofile = true;
							break;
						default:
							if (args[i].StartsWith("-r"))
							{
								_path = args[i].Split('=')[1];
							}
							break;
					}
				}
			}

			// Set the possibly new value for logger
			logger.ToFile = tofile;

			_path = (_path == "" ? Environment.CurrentDirectory : _path);

			// Drag and drop means quotes; we don't want quotes
			_filename = _filename.Replace("\"", "");

			// We also want the full path of the file, just in case
			_filename = Path.GetFullPath(_filename);

			// If it's a single file, handle it as such
			if (!Directory.Exists(_filename) && File.Exists(_filename))
			{
				logger.Log("File found: " + _filename);
				ProcessDAT(_filename, _path, _rename);
			}
			// If it's a directory, loop through the files and see if any are DATs
			else if (Directory.Exists(_filename))
			{
				// Make sure the path ends with the proper character
				if (!_filename.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					_filename += Path.DirectorySeparatorChar;
				}

				logger.Log("Directory found: " + _filename);
				foreach (string file in Directory.EnumerateFiles(_filename, "*", SearchOption.AllDirectories))
				{
					logger.Log("File found: " + file);
					ProcessDAT(file, _path, _rename);
				}
			}

			logger.Close();
		}

		/// <summary>
		/// Import the existing DAT(s)
		/// </summary>
		/// <param name="filename">Name of the file to be processed</param>
		/// <param name="path">The base path to be used for comparison</param>
		/// <param name="rename">True if roms are to be renamed</param>
		private static void ProcessDAT(string filename, string path, bool rename)
		{
			List<RomData> roms = RomManipulation.Parse(filename, 0, 0, logger);

			// Trim all file names according to the path that's set
			List<RomData> outroms = new List<RomData>();
			while (roms.Count != 0)
			{
				RomData rom = roms[0];
				roms.RemoveAt(0);

				// If we are in single game mode, rename all games
				if (rename)
				{
					rom.Game = "!";
				}

				// Windows max name length is 260
				int usableLength = 259 - _path.Length;
				if (rom.Name.Length > usableLength)
				{
					string ext = Path.GetExtension(rom.Name);
					rom.Name = rom.Name.Substring(0, usableLength - ext.Length);
					rom.Name += ext;
				}

				outroms.Add(rom);
			}

			// Now write the file out accordingly
			Output.WriteToDat(Path.GetFileNameWithoutExtension(filename),
				Path.GetFileNameWithoutExtension(filename), "", "", "", "", _forceunpack, !RomManipulation.IsXmlDat(filename), Path.GetDirectoryName(filename), outroms, logger);
		}
	}
}
