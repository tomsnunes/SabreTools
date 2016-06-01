using System;
using System.Collections.Generic;

using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Get statistics on one or more DAT files
	/// </summary>
	public class Stats
	{
		// Private instance variables
		private List<String> _inputs;
		private bool _single;
		private Logger _logger;

		/// <summary>
		/// Create a new UncompressedSize object
		/// </summary>
		/// <param name="inputs">List of files and folders to parse</param>
		/// <param name="single">True if single DAT stats are output, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		public Stats(List<String> inputs, bool single, Logger logger)
		{
			_inputs = inputs;
			_single = single;
			_logger = logger;
		}

		/// <summary>
		/// Output all requested statistics
		/// </summary>
		/// <returns>True if output succeeded, false otherwise</returns>
		public bool Process()
		{
			// Init all total variables
			long totalSize = 0;
			long totalGame = 0;
			long totalRom = 0;
			long totalDisk = 0;
			long totalCRC = 0;
			long totalMD5 = 0;
			long totalSHA1 = 0;
			long totalNodump = 0;

			/// Now process each of the input files
			foreach (string filename in _inputs)
			{
				_logger.User("Beginning stat collection for '" + filename + "'");
				List<String> games = new List<String>();
				DatData datdata = new DatData();
				datdata = RomManipulation.Parse(filename, 0, 0, datdata, _logger);
				SortedDictionary<string, List<RomData>> newroms = RomManipulation.BucketByGame(datdata.Roms, false, true, _logger);

				// Output single DAT stats (if asked)
				if (_single)
				{
					_logger.User(@"\nFor file '" + filename + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(datdata.TotalSize) + @"
    Games found:             " + newroms.Count + @"
    Roms found:              " + datdata.RomCount + @"
    Disks found:             " + datdata.DiskCount + @"
    Roms with CRC:           " + datdata.CRCCount + @"
    Roms with MD5:           " + datdata.MD5Count + @"
    Roms with SHA-1:         " + datdata.SHA1Count + @"
    Roms with Nodump status: " + datdata.NodumpCount + @"
");
				}
				else
				{
					_logger.User("\nAdding stats for file '" + filename + "'");
				}

				// Add single DAT stats to totals
				totalSize += datdata.TotalSize;
				totalGame += newroms.Count;
				totalRom += datdata.RomCount;
				totalDisk += datdata.DiskCount;
				totalCRC += datdata.CRCCount;
				totalMD5 += datdata.MD5Count;
				totalSHA1 += datdata.SHA1Count;
				totalNodump += datdata.NodumpCount;
			}

			// Output total DAT stats
			if (!_single) { _logger.User(""); }
			_logger.User(@"For ALL DATs found
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(totalSize) + @"
    Games found:             " + totalGame + @"
    Roms found:              " + totalRom + @"
    Disks found:             " + totalDisk + @"
    Roms with CRC:           " + totalCRC + @"
    Roms with MD5:           " + totalMD5 + @"
    Roms with SHA-1:         " + totalSHA1 + @"
    Roms with Nodump status: " + totalNodump + @"

Please check the log folder if the stats scrolled offscreen");

			return true;
		}
	}
}
