using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileAccess = System.IO.FileAccess;
using FileMode = System.IO.FileMode;
using SearchOption = System.IO.SearchOption;
using StreamWriter = System.IO.StreamWriter;
#endif

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Instance Methods

		#region Statistics [MODULAR DONE]

		/// <summary>
		/// Recalculate the statistics for the Dat
		/// </summary>
		public void RecalculateStats()
		{
			// Wipe out any stats already there
			RomCount = 0;
			DiskCount = 0;
			TotalSize = 0;
			CRCCount = 0;
			MD5Count = 0;
			SHA1Count = 0;
			BaddumpCount = 0;
			NodumpCount = 0;

			// If we have a blank Dat in any way, return
			if (this == null || Files == null || Files.Count == 0)
			{
				return;
			}

			// Loop through and add
			foreach (List<DatItem> roms in Files.Values)
			{
				foreach (Rom rom in roms)
				{
					RomCount += (rom.Type == ItemType.Rom ? 1 : 0);
					DiskCount += (rom.Type == ItemType.Disk ? 1 : 0);
					TotalSize += (rom.ItemStatus == ItemStatus.Nodump ? 0 : rom.Size);
					CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
					MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
					SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
					BaddumpCount += (rom.Type == ItemType.Disk
						? (((Disk)rom).ItemStatus == ItemStatus.BadDump ? 1 : 0)
						: (rom.Type == ItemType.Rom
							? (((Rom)rom).ItemStatus == ItemStatus.BadDump ? 1 : 0)
							: 0)
						);
					NodumpCount += (rom.Type == ItemType.Disk
							? (((Disk)rom).ItemStatus == ItemStatus.Nodump ? 1 : 0)
							: (rom.Type == ItemType.Rom
								? (((Rom)rom).ItemStatus == ItemStatus.Nodump ? 1 : 0)
								: 0)
							);
				}
			}
		}

		/// <summary>
		/// Output the stats for the Dat in a human-readable format
		/// </summary>
		/// <param name="sw">StreamWriter representing the output file or stream for the statistics</param>
		/// <param name="statDatFormat">Set the statistics output format to use</param>
		/// <param name="logger">Logger object for file and console writing</param>
		/// <param name="recalculate">True if numbers should be recalculated for the DAT, false otherwise (default)</param>
		/// <param name="game">Number of games to use, -1 means recalculate games (default)</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise (default)</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise (default)</param>
		public void OutputStats(StreamWriter sw, StatDatFormat statDatFormat, Logger logger, bool recalculate = false, long game = -1, bool baddumpCol = false, bool nodumpCol = false)
		{
			// If we're supposed to recalculate the statistics, do so
			if (recalculate)
			{
				RecalculateStats();
			}

			BucketByGame(false, true, logger, false);
			if (TotalSize < 0)
			{
				TotalSize = Int64.MaxValue + TotalSize;
			}

			// Log the results to screen
			string results = @"For '" + FileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(TotalSize) + @"
    Games found:             " + (game == -1 ? Files.Count : game) + @"
    Roms found:              " + RomCount + @"
    Disks found:             " + DiskCount + @"
    Roms with CRC:           " + CRCCount + @"
    Roms with MD5            " + MD5Count + @"
    Roms with SHA-1:         " + SHA1Count + "\n";

			if (baddumpCol)
			{
				results += "	Roms with BadDump status: " + BaddumpCount + "\n";
			}
			if (nodumpCol)
			{
				results += "	Roms with Nodump status: " + NodumpCount + "\n";
			}

			logger.User(results);

			// Now write it out to file as well
			string line = "";
			switch (statDatFormat)
			{
				case StatDatFormat.CSV:
					line = "\"" + FileName + "\","
						+ "\"" + Style.GetBytesReadable(TotalSize) + "\","
						+ "\"" + (game == -1 ? Files.Count : game) + "\","
						+ "\"" + RomCount + "\","
						+ "\"" + DiskCount + "\","
						+ "\"" + CRCCount + "\","
						+ "\"" + MD5Count + "\","
						+ "\"" + SHA1Count + "\"";

					if (baddumpCol)
					{
						line += ",\"" + BaddumpCount + "\"";
					}
					if (nodumpCol)
					{
						line += ",\"" + NodumpCount + "\"";
					}

					line += "\n";
					break;
				case StatDatFormat.HTML:
					line = "\t\t\t<tr" + (FileName.StartsWith("DIR: ")
							? " class=\"dir\"><td>" + HttpUtility.HtmlEncode(FileName.Remove(0, 5))
							: "><td>" + HttpUtility.HtmlEncode(FileName)) + "</td>"
						+ "<td align=\"right\">" + Style.GetBytesReadable(TotalSize) + "</td>"
						+ "<td align=\"right\">" + (game == -1 ? Files.Count : game) + "</td>"
						+ "<td align=\"right\">" + RomCount + "</td>"
						+ "<td align=\"right\">" + DiskCount + "</td>"
						+ "<td align=\"right\">" + CRCCount + "</td>"
						+ "<td align=\"right\">" + MD5Count + "</td>"
						+ "<td align=\"right\">" + SHA1Count + "</td>";

					if (baddumpCol)
					{
						line += "<td align=\"right\">" + BaddumpCount + "</td>";
					}
					if (nodumpCol)
					{
						line += "<td align=\"right\">" + NodumpCount + "</td>";
					}

					line += "</tr>\n";
					break;
				case StatDatFormat.None:
				default:
					line = @"'" + FileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(TotalSize) + @"
    Games found:             " + (game == -1 ? Files.Count : game) + @"
    Roms found:              " + RomCount + @"
    Disks found:             " + DiskCount + @"
    Roms with CRC:           " + CRCCount + @"
    Roms with MD5:           " + MD5Count + @"
    Roms with SHA-1:         " + SHA1Count + "\n";

					if (baddumpCol)
					{
						line += "	Roms with BadDump status: " + BaddumpCount + "\n";
					}
					if (nodumpCol)
					{
						line += "	Roms with Nodump status: " + NodumpCount + "\n";
					}
					break;
				case StatDatFormat.TSV:
					line = "\"" + FileName + "\"\t"
						+ "\"" + Style.GetBytesReadable(TotalSize) + "\"\t"
						+ "\"" + (game == -1 ? Files.Count : game) + "\"\t"
						+ "\"" + RomCount + "\"\t"
						+ "\"" + DiskCount + "\"\t"
						+ "\"" + CRCCount + "\"\t"
						+ "\"" + MD5Count + "\"\t"
						+ "\"" + SHA1Count + "\"";

					if (baddumpCol)
					{
						line += "\t\"" + BaddumpCount + "\"";
					}
					if (nodumpCol)
					{
						line += "\t\"" + NodumpCount + "\"";
					}

					line += "\n";
					break;
			}

			// Output the line to the streamwriter
			sw.Write(line);
		}

		#endregion

		#endregion // Instance Methods

		#region Static Methods

		#region Statistics [MODULAR DONE]

		/// <summary>
		/// Output the stats for a list of input dats as files in a human-readable format
		/// </summary>
		/// <param name="inputs">List of input files and folders</param>
		/// <param name="reportName">Name of the output file</param>
		/// <param name="single">True if single DAT stats are output, false otherwise</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		/// <param name="statDatFormat" > Set the statistics output format to use</param>
		/// <param name="logger">Logger object for file and console output</param>
		public static void OutputStats(List<string> inputs, string reportName, bool single, bool baddumpCol,
			bool nodumpCol, StatDatFormat statDatFormat, Logger logger)
		{
			reportName += OutputStatsGetExtension(statDatFormat);
			StreamWriter sw = new StreamWriter(File.Open(reportName, FileMode.Create, FileAccess.Write));

			// Make sure we have all files
			List<Tuple<string, string>> newinputs = new List<Tuple<string, string>>(); // item, basepath
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					newinputs.Add(Tuple.Create(Path.GetFullPath(input), Path.GetDirectoryName(Path.GetFullPath(input))));
				}
				if (Directory.Exists(input))
				{
					foreach (string file in Directory.GetFiles(input, "*", SearchOption.AllDirectories))
					{
						newinputs.Add(Tuple.Create(Path.GetFullPath(file), Path.GetFullPath(input)));
					}
				}
			}
			newinputs = newinputs
				.OrderBy(i => Path.GetDirectoryName(i.Item1))
				.ThenBy(i => Path.GetFileName(i.Item1))
				.ToList();

			// Write the header, if any
			OutputStatsWriteHeader(sw, statDatFormat, baddumpCol, nodumpCol);

			// Init all total variables
			long totalSize = 0;
			long totalGame = 0;
			long totalRom = 0;
			long totalDisk = 0;
			long totalCRC = 0;
			long totalMD5 = 0;
			long totalSHA1 = 0;
			long totalBaddump = 0;
			long totalNodump = 0;

			// Init directory-level variables
			string lastdir = null;
			string basepath = null;
			long dirSize = 0;
			long dirGame = 0;
			long dirRom = 0;
			long dirDisk = 0;
			long dirCRC = 0;
			long dirMD5 = 0;
			long dirSHA1 = 0;
			long dirBaddump = 0;
			long dirNodump = 0;

			// Now process each of the input files
			foreach (Tuple<string, string> filename in newinputs)
			{
				// Get the directory for the current file
				string thisdir = Path.GetDirectoryName(filename.Item1);
				basepath = Path.GetDirectoryName(filename.Item2);

				// If we don't have the first file and the directory has changed, show the previous directory stats and reset
				if (lastdir != null && thisdir != lastdir)
				{
					// Output separator if needed
					OutputStatsWriteMidSeparator(sw, statDatFormat, baddumpCol, nodumpCol);

					DatFile lastdirdat = new DatFile
					{
						FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
						TotalSize = dirSize,
						RomCount = dirRom,
						DiskCount = dirDisk,
						CRCCount = dirCRC,
						MD5Count = dirMD5,
						SHA1Count = dirSHA1,
						BaddumpCount = dirBaddump,
						NodumpCount = dirNodump,
					};
					lastdirdat.OutputStats(sw, statDatFormat, logger, game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);

					// Write the mid-footer, if any
					OutputStatsWriteMidFooter(sw, statDatFormat, baddumpCol, nodumpCol);

					// Write the header, if any
					OutputStatsWriteMidHeader(sw, statDatFormat, baddumpCol, nodumpCol);

					// Reset the directory stats
					dirSize = 0;
					dirGame = 0;
					dirRom = 0;
					dirDisk = 0;
					dirCRC = 0;
					dirMD5 = 0;
					dirSHA1 = 0;
					dirBaddump = 0;
					dirNodump = 0;
				}

				logger.Verbose("Beginning stat collection for '" + filename.Item1 + "'", false);
				List<string> games = new List<string>();
				DatFile datdata = new DatFile();
				datdata.Parse(filename.Item1, 0, 0, logger);
				datdata.BucketByGame(false, true, logger, false);

				// Output single DAT stats (if asked)
				logger.User("Adding stats for file '" + filename.Item1 + "'\n", false);
				if (single)
				{
					datdata.OutputStats(sw, statDatFormat, logger, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
				}

				// Add single DAT stats to dir
				dirSize += datdata.TotalSize;
				dirGame += datdata.Files.Count;
				dirRom += datdata.RomCount;
				dirDisk += datdata.DiskCount;
				dirCRC += datdata.CRCCount;
				dirMD5 += datdata.MD5Count;
				dirSHA1 += datdata.SHA1Count;
				dirBaddump += datdata.BaddumpCount;
				dirNodump += datdata.NodumpCount;

				// Add single DAT stats to totals
				totalSize += datdata.TotalSize;
				totalGame += datdata.Files.Count;
				totalRom += datdata.RomCount;
				totalDisk += datdata.DiskCount;
				totalCRC += datdata.CRCCount;
				totalMD5 += datdata.MD5Count;
				totalSHA1 += datdata.SHA1Count;
				totalBaddump += datdata.BaddumpCount;
				totalNodump += datdata.NodumpCount;

				// Make sure to assign the new directory
				lastdir = thisdir;
			}

			// Output the directory stats one last time
			OutputStatsWriteMidSeparator(sw, statDatFormat, baddumpCol, nodumpCol);

			if (single)
			{
				DatFile dirdat = new DatFile
				{
					FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
					TotalSize = dirSize,
					RomCount = dirRom,
					DiskCount = dirDisk,
					CRCCount = dirCRC,
					MD5Count = dirMD5,
					SHA1Count = dirSHA1,
					BaddumpCount = dirBaddump,
					NodumpCount = dirNodump,
				};
				dirdat.OutputStats(sw, statDatFormat, logger, game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
			}

			// Write the mid-footer, if any
			OutputStatsWriteMidFooter(sw, statDatFormat, baddumpCol, nodumpCol);

			// Write the header, if any
			OutputStatsWriteMidHeader(sw, statDatFormat, baddumpCol, nodumpCol);

			// Reset the directory stats
			dirSize = 0;
			dirGame = 0;
			dirRom = 0;
			dirDisk = 0;
			dirCRC = 0;
			dirMD5 = 0;
			dirSHA1 = 0;
			dirNodump = 0;

			// Output total DAT stats
			DatFile totaldata = new DatFile
			{
				FileName = "DIR: All DATs",
				TotalSize = totalSize,
				RomCount = totalRom,
				DiskCount = totalDisk,
				CRCCount = totalCRC,
				MD5Count = totalMD5,
				SHA1Count = totalSHA1,
				BaddumpCount = totalBaddump,
				NodumpCount = totalNodump,
			};
			totaldata.OutputStats(sw, statDatFormat, logger, game: totalGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);

			// Output footer if needed
			OutputStatsWriteFooter(sw, statDatFormat);

			sw.Flush();
			sw.Dispose();

			logger.User(@"
Please check the log folder if the stats scrolled offscreen", false);
		}

		/// <summary>
		/// Get the proper extension for the stat output format
		/// </summary>
		/// <param name="statDatFormat">StatDatFormat to get the extension for</param>
		/// <returns>File extension with leading period</returns>
		private static string OutputStatsGetExtension(StatDatFormat statDatFormat)
		{
			string reportExtension = "";
			switch (statDatFormat)
			{
				case StatDatFormat.CSV:
					reportExtension = ".csv";
					break;
				case StatDatFormat.HTML:
					reportExtension = ".html";
					break;
				case StatDatFormat.None:
				default:
					reportExtension = ".txt";
					break;
				case StatDatFormat.TSV:
					reportExtension = ".csv";
					break;
			}
			return reportExtension;
		}

		/// <summary>
		/// Write out the header to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteHeader(StreamWriter sw, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			string head = "";
			switch (statDatFormat)
			{
				case StatDatFormat.CSV:
					break;
				case StatDatFormat.HTML:
					head = @"<!DOCTYPE html>
<html>
	<header>
		<title>DAT Statistics Report</title>
		<style>
			body {
				background-color: lightgray;
			}
			.dir {
				color: #0088FF;
			}
			.right {
				align: right;
			}
		</style>
	</header>
	<body>
		<h2>DAT Statistics Report (" + DateTime.Now.ToShortDateString() + @")</h2>
		<table border=""1"" cellpadding=""5"" cellspacing=""0"">
";
					break;
				case StatDatFormat.None:
				default:
					break;
				case StatDatFormat.TSV:
					break;
			}
			sw.Write(head);

			// Now write the mid header for those who need it
			OutputStatsWriteMidHeader(sw, statDatFormat, baddumpCol, nodumpCol);
		}

		/// <summary>
		/// Write out the mid-header to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidHeader(StreamWriter sw, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			string head = "";
			switch (statDatFormat)
			{
				case StatDatFormat.CSV:
					head = "\"File Name\",\"Total Size\",\"Games\",\"Roms\",\"Disks\",\"# with CRC\",\"# with MD5\",\"# with SHA-1\""
						+ (baddumpCol ? ",\"BadDumps\"" : "") + (nodumpCol ? ",\"Nodumps\"" : "") + "\n";
					break;
				case StatDatFormat.HTML:
					head = @"			<tr bgcolor=""gray""><th>File Name</th><th align=""right"">Total Size</th><th align=""right"">Games</th><th align=""right"">Roms</th>"
+ @"<th align=""right"">Disks</th><th align=""right"">&#35; with CRC</th><th align=""right"">&#35; with MD5</th><th align=""right"">&#35; with SHA-1</th>"
+ (baddumpCol ? "<th class=\".right\">Baddumps</th>" : "") + (nodumpCol ? "<th class=\".right\">Nodumps</th>" : "") + "</tr>\n";
					break;
				case StatDatFormat.None:
				default:
					break;
				case StatDatFormat.TSV:
					head = "\"File Name\"\t\"Total Size\"\t\"Games\"\t\"Roms\"\t\"Disks\"\t\"# with CRC\"\t\"# with MD5\"\t\"# with SHA-1\""
						+ (baddumpCol ? "\t\"BadDumps\"" : "") + (nodumpCol ? "\t\"Nodumps\"" : "") + "\n";
					break;
			}
			sw.Write(head);
		}

		/// <summary>
		/// Write out the separator to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidSeparator(StreamWriter sw, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			string mid = "";
			switch (statDatFormat)
			{
				case StatDatFormat.CSV:
					break;
				case StatDatFormat.HTML:
					mid = "<tr><td colspan=\""
						+ (baddumpCol && nodumpCol
							? "11"
							: (baddumpCol ^ nodumpCol
								? "10"
								: "9")
							)
						+ "\"></td></tr>\n";
					break;
				case StatDatFormat.None:
				default:
					break;
			}
			sw.Write(mid);
		}

		/// <summary>
		/// Write out the footer-separator to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidFooter(StreamWriter sw, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			string end = "";
			switch (statDatFormat)
			{
				case StatDatFormat.CSV:
					end = "\n";
					break;
				case StatDatFormat.HTML:
					end = "<tr border=\"0\"><td colspan=\""
						+ (baddumpCol && nodumpCol
							? "11"
							: (baddumpCol ^ nodumpCol
								? "10"
								: "9")
							)
						+ "\"></td></tr>\n";
					break;
				case StatDatFormat.None:
				default:
					end = "\n";
					break;
				case StatDatFormat.TSV:
					end = "\n";
					break;
			}
			sw.Write(end);
		}

		/// <summary>
		/// Write out the footer to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		private static void OutputStatsWriteFooter(StreamWriter sw, StatDatFormat statDatFormat)
		{
			string end = "";
			switch (statDatFormat)
			{
				case StatDatFormat.CSV:
					break;
				case StatDatFormat.HTML:
					end = @"		</table>
	</body>
</html>
";
					break;
				case StatDatFormat.None:
				default:
					break;
				case StatDatFormat.TSV:
					break;
			}
			sw.Write(end);
		}

		#endregion

		#endregion // Static Methods
	}
}
