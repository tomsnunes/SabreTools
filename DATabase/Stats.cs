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
			// Init all single-dat variables
			long singleSize = 0;
			long singleGame = 0;
			long singleRom = 0;
			long singleDisk = 0;
			long singleCRC = 0;
			long singleMD5 = 0;
			long singleSHA1 = 0;
			long singleNodump = 0;

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
				foreach (List<RomData> romlist in datdata.Roms.Values)
				{
					foreach (RomData rom in romlist)
					{
						singleSize += rom.Size;
						if (!games.Contains(rom.Game))
						{
							singleGame++;
							games.Add(rom.Game);
						}
						if (rom.Type == "rom")
						{
							singleRom++;
						}
						if (rom.Type == "disk")
						{
							singleDisk++;
						}
						if (!String.IsNullOrEmpty(rom.CRC))
						{
							singleCRC++;
						}
						if (!String.IsNullOrEmpty(rom.MD5))
						{
							singleMD5++;
						}
						if (!String.IsNullOrEmpty(rom.SHA1))
						{
							singleSHA1++;
						}
						if (rom.Nodump)
						{
							singleNodump++;
						}
					}
				}

				// Output single DAT stats (if asked)
				if (_single)
				{
					_logger.User(@"For file '" + filename + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(singleSize) + @"
    Games found:             " + singleGame + @"
    Roms found:              " + singleRom + @"
    Disks found:             " + singleDisk + @"
    Roms with CRC:           " + singleCRC + @"
    Roms with MD5:           " + singleMD5 + @"
    Roms with SHA-1:         " + singleSHA1 + @"
    Roms with Nodump status: " + singleNodump + @"
");
				}
				else
				{
					_logger.User("Adding stats for file '" + filename + "'");
				}

				// Add single DAT stats to totals
				totalSize += singleSize;
				totalGame += singleGame;
				totalRom += singleRom;
				totalDisk += singleDisk;
				totalCRC += singleCRC;
				totalMD5 += singleMD5;
				totalSHA1 += singleSHA1;
				totalNodump += singleNodump;

				// Reset single DAT stats
				singleSize = 0;
				singleGame = 0;
				singleRom = 0;
				singleDisk = 0;
				singleCRC = 0;
				singleMD5 = 0;
				singleSHA1 = 0;
				singleNodump = 0;
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
