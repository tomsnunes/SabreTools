using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class ExtSplit
	{
		// Instance variables
		private string _extA;
		private string _extB;
		private string _filename;
		private string _outdir;
		private static Logger _logger;

		/// <summary>
		/// Create a new DatSplit object
		/// </summary>
		/// <param name="filename">Filename of the DAT to split</param>
		/// <param name="extA">First extension to split on</param>
		/// <param name="extB">Second extension to split on</param>
		/// <param name="logger">Logger object for console and file writing</param>
		public ExtSplit(string filename, string extA, string extB, string outdir, Logger logger)
		{
			_filename = filename.Replace("\"", "");
			_extA = (extA.StartsWith(".") ? extA : "." + extA).ToUpperInvariant();
			_extB = (extB.StartsWith(".") ? extB : "." + extB).ToUpperInvariant();
			_outdir = outdir.Replace("\"", "");
			_logger = logger;
		}

		/// <summary>
		/// Split a DAT based on filtering by 2 extensions
		/// </summary>
		/// <returns>True if DAT was split, false otherwise</returns>
		public bool Split()
		{
			// If file doesn't exist, error and return
			if (!File.Exists(_filename))
			{
				_logger.Error("File '" + _filename + "' doesn't exist");
				return false;
			}

			// If the output directory doesn't exist, create it
			if (!Directory.Exists(_outdir))
			{
				Directory.CreateDirectory(_outdir);
			}

			List<RomData> romsA = new List<RomData>();
			List<RomData> romsB = new List<RomData>();

			// Load the current DAT to be processed
			List<RomData> roms = RomManipulation.Parse(_filename, 0, 0, _logger);

			// If roms is empty, return false
			if (roms.Count == 0)
			{
				return false;
			}

			// Now separate the roms accordingly
			foreach (RomData rom in roms)
			{
				if (rom.Name.ToUpperInvariant().EndsWith(_extA))
				{
					romsA.Add(rom);
				}
				else if (rom.Name.ToUpperInvariant().EndsWith(_extB))
				{
					romsB.Add(rom);
				}
				else
				{
					romsA.Add(rom);
					romsB.Add(rom);
				}
			}

			// Then write out both files
			bool success = Output.WriteToDat(Path.GetFileNameWithoutExtension(_filename) + "." + _extA, Path.GetFileNameWithoutExtension(_filename) + "." + _extA,
				"", "", "", "", false, !RomManipulation.IsXmlDat(_filename), _outdir, romsA, _logger);
			success &= Output.WriteToDat(Path.GetFileNameWithoutExtension(_filename) + "." + _extB, Path.GetFileNameWithoutExtension(_filename) + "." + _extB,
				"", "", "", "", false, !RomManipulation.IsXmlDat(_filename), _outdir, romsB, _logger);

			return success;
		}
	}
}
