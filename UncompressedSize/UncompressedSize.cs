using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class UncompressedSize
	{
		public static void Main(string[] args)
		{
			Console.Clear();
			Build.Start("UncompressedSize");

			List<string> inputs = new List<string>();

			foreach (string arg in args)
			{
				if (File.Exists(arg.Replace("\"", "")))
				{
					inputs.Add(arg.Replace("\"", ""));
				}
				if (Directory.Exists(arg.Replace("\"", "")))
				{
					foreach (string file in Directory.GetFiles(arg.Replace("\"", ""), "*", SearchOption.AllDirectories))
					{
						inputs.Add(file.Replace("\"", ""));
					}
				}
			}

			Logger logger = new Logger(true, "uncompressedsize.log");
			logger.Start();

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


			foreach (string filename in inputs)
			{
				List<String> games = new List<String>();

				DatData datdata = new DatData();
				datdata = RomManipulation.Parse(filename, 0, 0, datdata, logger);
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

				// Output single DAT stats
				logger.User(@"For file '" + filename + @"':
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
			logger.User(@"For ALL DATs found
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(totalSize) + @"
    Games found:             " + totalGame + @"
    Roms found:              " + totalRom + @"
    Disks found:             " + totalDisk + @"
    Roms with CRC:           " + totalCRC + @"
    Roms with MD5:           " + totalMD5 + @"
    Roms with SHA-1:         " + totalSHA1 + @"
    Roms with Nodump status: " + totalNodump + @"

For all individual stats, check the log folder for a complete list");
			logger.Close();
		}
	}
}
