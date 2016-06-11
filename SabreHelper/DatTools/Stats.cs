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
				datdata = DatTools.Parse(filename, 0, 0, datdata, _logger);
				SortedDictionary<string, List<RomData>> newroms = DatTools.BucketByGame(datdata.Roms, false, true, _logger);

				// Output single DAT stats (if asked)
				if (_single)
				{
					_logger.User(@"\nFor file '" + filename + @"':
--------------------------------------------------");
					OutputStats(datdata, _logger);
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
			DatData totaldata = new DatData
			{
				TotalSize = totalSize,
				RomCount = totalRom,
				DiskCount = totalDisk,
				CRCCount = totalCRC,
				MD5Count = totalMD5,
				SHA1Count = totalSHA1,
				NodumpCount = totalNodump,
			};
			_logger.User(@"For ALL DATs found
--------------------------------------------------");
			OutputStats(totaldata, _logger);
			_logger.User(@"
Please check the log folder if the stats scrolled offscreen");

			return true;
		}

		/// <summary>
		/// Output the stats in a human-readable format
		/// </summary>
		/// <param name="datdata">DatData object to read stats from</param>
		/// <param name="logger">Logger object for file and console writing</param>
		/// <param name="recalculate">True if numbers should be recalculated for the DAT, false otherwise (default)</param>
		public static void OutputStats(DatData datdata, Logger logger, bool recalculate = false)
		{
			if (recalculate)
			{
				// Wipe out any stats already there
				datdata.RomCount = 0;
				datdata.DiskCount = 0;
				datdata.TotalSize = 0;
				datdata.CRCCount = 0;
				datdata.MD5Count = 0;
				datdata.SHA1Count = 0;
				datdata.NodumpCount = 0;

				// Loop through and add
				foreach (List<RomData> roms in datdata.Roms.Values)
				{
					foreach (RomData rom in roms)
					{
						datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
						datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
						datdata.TotalSize += rom.Size;
						datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
						datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
						datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
						datdata.NodumpCount += (rom.Nodump ? 1 : 0);
					}
				}
			}

			SortedDictionary<string, List<RomData>> newroms = DatTools.BucketByGame(datdata.Roms, false, true, logger);
			logger.User(@"    Uncompressed size:       " + Style.GetBytesReadable(datdata.TotalSize) + @"
    Games found:             " + newroms.Count + @"
    Roms found:              " + datdata.RomCount + @"
    Disks found:             " + datdata.DiskCount + @"
    Roms with CRC:           " + datdata.CRCCount + @"
    Roms with MD5:           " + datdata.MD5Count + @"
    Roms with SHA-1:         " + datdata.SHA1Count + @"
    Roms with Nodump status: " + datdata.NodumpCount + @"
");
		}
	}
}
