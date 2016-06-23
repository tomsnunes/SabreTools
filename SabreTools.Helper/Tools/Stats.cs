using System;
using System.Collections.Generic;

namespace SabreTools.Helper
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
				Dat datdata = new Dat();
				datdata = DatTools.Parse(filename, 0, 0, datdata, _logger);
				SortedDictionary<string, List<Rom>> newroms = DatTools.BucketByGame(datdata.Roms, false, true, _logger, false);

				// Output single DAT stats (if asked)
				if (_single)
				{
					_logger.User(@"\nFor file '" + filename + @"':
--------------------------------------------------");
					OutputStats(datdata, _logger);
				}
				else
				{
					_logger.User("Adding stats for file '" + filename + "'\n");
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
			Dat totaldata = new Dat
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
			OutputStats(totaldata, _logger, game:totalGame);
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
		/// <param name="game">Number of games to use, -1 means recalculate games (default)</param>
		public static void OutputStats(Dat datdata, Logger logger, bool recalculate = false, long game = -1)
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
				foreach (List<Rom> roms in datdata.Roms.Values)
				{
					foreach (Rom rom in roms)
					{
						datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
						datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
						datdata.TotalSize += (rom.Nodump ? 0 : rom.Size);
						datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
						datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
						datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
						datdata.NodumpCount += (rom.Nodump ? 1 : 0);
					}
				}
			}

			SortedDictionary<string, List<Rom>> newroms = DatTools.BucketByGame(datdata.Roms, false, true, logger, false);
			string line = "    Uncompressed size:       " + Style.GetBytesReadable(datdata.TotalSize);
			logger.User(line.PadRight(79, ' '));
			line = "    Games found:             " + (game == -1 ? newroms.Count : game);
			logger.User(line.PadRight(79, ' '));
			line = "    Roms found:              " + datdata.RomCount;
			logger.User(line.PadRight(79, ' '));
			line = "    Disks found:             " + datdata.DiskCount;
			logger.User(line.PadRight(79, ' '));
			line = "    Roms with CRC:           " + datdata.CRCCount;
			logger.User(line.PadRight(79, ' '));
			line = "    Roms with MD5:           " + datdata.MD5Count;
			logger.User(line.PadRight(79, ' '));
			line = "    Roms with SHA-1:         " + datdata.SHA1Count;
			logger.User(line.PadRight(79, ' '));
			line = "    Roms with Nodump status: " + datdata.NodumpCount;
			logger.User(line.PadRight(79, ' '));
			logger.User("");
		}
	}
}
