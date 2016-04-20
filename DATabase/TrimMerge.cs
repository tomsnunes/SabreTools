using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class TrimMerge
	{
		// Instance variables
		private static string _filename = "";
		private static string _path = "";
		private static bool _rename;
		private static bool _forceunpack;
		private static Logger _logger;

		/// <summary>
		/// Create a new TrimMerge object
		/// </summary>
		/// <param name="filename">Name of the file or folder to be processed</param>
		/// <param name="path">Root path to use for trimming</param>
		/// <param name="rename">True if games should be renamed into a uniform string, false otherwise</param>
		/// <param name="forceunpack">True if forcepacking="unzip" should be set on the output, false otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>
		public TrimMerge(string filename, string path, bool rename, bool forceunpack, Logger logger)
		{
			_filename = filename.Replace("\"", "");
			_path = path;
			_rename = rename;
			_forceunpack = forceunpack;
			_logger = logger;
		}

		/// <summary>
		/// Trim and process the given DAT or folder of DATs
		/// </summary>
		/// <returns>True if the DAT could be updated, false otherwise</returns>
		public bool Process()
		{
			// If file doesn't exist, error and return
			if (!File.Exists(_filename) && !Directory.Exists(_filename))
			{
				_logger.Error("File or folder '" + _filename + "' doesn't exist");
				return false;
			}

			_path = (_path == "" ? Environment.CurrentDirectory : _path);

			// We want the full path of the file, just in case
			_filename = Path.GetFullPath(_filename);

			// If it's a single file, handle it as such
			if (!Directory.Exists(_filename) && File.Exists(_filename))
			{
				_logger.Log("File found: " + _filename);
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

				_logger.Log("Directory found: " + _filename);
				foreach (string file in Directory.EnumerateFiles(_filename, "*", SearchOption.AllDirectories))
				{
					_logger.Log("File found: " + file);
					ProcessDAT(file, _path, _rename);
				}
			}

			return true;
		}

		/// <summary>
		/// Import the existing DAT(s)
		/// </summary>
		/// <param name="filename">Name of the file to be processed</param>
		/// <param name="path">The base path to be used for comparison</param>
		/// <param name="rename">True if roms are to be renamed</param>
		private static void ProcessDAT(string filename, string path, bool rename)
		{
			List<RomData> roms = RomManipulation.Parse(filename, 0, 0, _logger);

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
				Path.GetFileNameWithoutExtension(filename), "", "", "", "", _forceunpack, !RomManipulation.IsXmlDat(filename), Path.GetDirectoryName(filename), outroms, _logger);

			// Remove the original file if different and inform the user
			if (Path.GetExtension(filename) != (RomManipulation.IsXmlDat(filename) ? ".xml" : ".dat"))
			{
				try
				{
					File.Delete(filename);
					_logger.Log("Original file \"" + filename + "\" deleted");
				}
				catch (Exception ex)
				{
					_logger.Error(ex.ToString());
				}
			}
		}
	}
}
