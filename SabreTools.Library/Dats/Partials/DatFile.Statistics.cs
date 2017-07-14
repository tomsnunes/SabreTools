using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
using StreamWriter = System.IO.StreamWriter;
#endif

namespace SabreTools.Library.Dats
{
	/*
	 * TODO: Make output standard width (HTML, without making the entire thing a table)
	 * TODO: Multithreading? Either StringBuilder or locking
	 */
	public partial class DatFile
	{
		#region Instance Methods

		#region Statistics

		/// <summary>
		/// Add to the internal statistics given a DatItem
		/// </summary>
		/// <param name="item">Item to add info from</param>
		private void AddItemStatistics(DatItem item)
		{
			// No matter what the item is, we increate the count
			lock (_statslock)
			{
				_count += 1;

				// Now we do different things for each item type

				switch (item.Type)
				{
					case ItemType.Archive:
						break;
					case ItemType.BiosSet:
						break;
					case ItemType.Disk:
						_diskCount += 1;
						if (((Disk)item).ItemStatus != ItemStatus.Nodump)
						{
							_md5Count += (String.IsNullOrEmpty(((Disk)item).MD5) ? 0 : 1);
							_sha1Count += (String.IsNullOrEmpty(((Disk)item).SHA1) ? 0 : 1);
							_sha256Count += (String.IsNullOrEmpty(((Disk)item).SHA256) ? 0 : 1);
							_sha384Count += (String.IsNullOrEmpty(((Disk)item).SHA384) ? 0 : 1);
							_sha512Count += (String.IsNullOrEmpty(((Disk)item).SHA512) ? 0 : 1);
						}

						_baddumpCount += (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_nodumpCount += (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						break;
					case ItemType.Release:
						break;
					case ItemType.Rom:
						_romCount += 1;
						if (((Rom)item).ItemStatus != ItemStatus.Nodump)
						{
							_totalSize += ((Rom)item).Size;
							_crcCount += (String.IsNullOrEmpty(((Rom)item).CRC) ? 0 : 1);
							_md5Count += (String.IsNullOrEmpty(((Rom)item).MD5) ? 0 : 1);
							_sha1Count += (String.IsNullOrEmpty(((Rom)item).SHA1) ? 0 : 1);
							_sha256Count += (String.IsNullOrEmpty(((Rom)item).SHA256) ? 0 : 1);
							_sha384Count += (String.IsNullOrEmpty(((Rom)item).SHA384) ? 0 : 1);
							_sha512Count += (String.IsNullOrEmpty(((Rom)item).SHA512) ? 0 : 1);
						}

						_baddumpCount += (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_nodumpCount += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						break;
					case ItemType.Sample:
						break;
				}
			}
		}

		/// <summary>
		/// Remove from the internal statistics given a DatItem
		/// </summary>
		/// <param name="item">Item to remove info for</param>
		private void RemoveItemStatistics(DatItem item)
		{
			// No matter what the item is, we increate the count
			lock (_statslock)
			{
				_count -= 1;

				// Now we do different things for each item type

				switch (item.Type)
				{
					case ItemType.Archive:
						break;
					case ItemType.BiosSet:
						break;
					case ItemType.Disk:
						_diskCount -= 1;
						if (((Disk)item).ItemStatus != ItemStatus.Nodump)
						{
							_md5Count -= (String.IsNullOrEmpty(((Disk)item).MD5) ? 0 : 1);
							_sha1Count -= (String.IsNullOrEmpty(((Disk)item).SHA1) ? 0 : 1);
							_sha256Count -= (String.IsNullOrEmpty(((Disk)item).SHA256) ? 0 : 1);
							_sha384Count -= (String.IsNullOrEmpty(((Disk)item).SHA384) ? 0 : 1);
							_sha512Count -= (String.IsNullOrEmpty(((Disk)item).SHA512) ? 0 : 1);
						}

						_baddumpCount -= (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_nodumpCount -= (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						break;
					case ItemType.Release:
						break;
					case ItemType.Rom:
						_romCount -= 1;
						if (((Rom)item).ItemStatus != ItemStatus.Nodump)
						{
							_totalSize -= ((Rom)item).Size;
							_crcCount -= (String.IsNullOrEmpty(((Rom)item).CRC) ? 0 : 1);
							_md5Count -= (String.IsNullOrEmpty(((Rom)item).MD5) ? 0 : 1);
							_sha1Count -= (String.IsNullOrEmpty(((Rom)item).SHA1) ? 0 : 1);
							_sha256Count -= (String.IsNullOrEmpty(((Rom)item).SHA256) ? 0 : 1);
							_sha384Count -= (String.IsNullOrEmpty(((Rom)item).SHA384) ? 0 : 1);
							_sha512Count -= (String.IsNullOrEmpty(((Rom)item).SHA512) ? 0 : 1);
						}

						_baddumpCount -= (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_nodumpCount -= (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						break;
					case ItemType.Sample:
						break;
				}
			}
		}

		/// <summary>
		/// Reset all statistics
		/// </summary>
		private void ResetStatistics()
		{
			_count = 0;
			_romCount = 0;
			_diskCount = 0;
			_totalSize = 0;
			_crcCount = 0;
			_md5Count = 0;
			_sha1Count = 0;
			_sha256Count = 0;
			_sha384Count = 0;
			_sha512Count = 0;
			_baddumpCount = 0;
			_nodumpCount = 0;
		}

		/// <summary>
		/// Recalculate the statistics for the Dat
		/// </summary>
		public void RecalculateStats()
		{
			// Wipe out any stats already there
			ResetStatistics();

			// If we have a blank Dat in any way, return
			if (this == null || Count == 0)
			{
				return;
			}

			// Loop through and add
			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, key =>
			{
				List<DatItem> items = this[key];
				foreach(DatItem item in items)
				{
					AddItemStatistics(item);
				}
			});
		}

		/// <summary>
		/// Output the stats for the Dat in a human-readable format
		/// </summary>
		/// <param name="outputs">Dictionary representing the outputs</param>
		/// <param name="statDatFormat">Set the statistics output format to use</param>
		/// <param name="recalculate">True if numbers should be recalculated for the DAT, false otherwise (default)</param>
		/// <param name="game">Number of games to use, -1 means recalculate games (default)</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise (default)</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise (default)</param>
		public void OutputStats(Dictionary<StatDatFormat, StreamWriter> outputs, StatDatFormat statDatFormat,
			bool recalculate = false, long game = -1, bool baddumpCol = false, bool nodumpCol = false)
		{
			// If we're supposed to recalculate the statistics, do so
			if (recalculate)
			{
				RecalculateStats();
			}

			BucketBy(SortedBy.Game, false /* mergeroms */, norename: true);
			if (_totalSize < 0)
			{
				_totalSize = Int64.MaxValue + _totalSize;
			}

			// Log the results to screen
			string results = @"For '" + _fileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(_totalSize) + @"
    Games found:             " + (game == -1 ? Keys.Count() : game) + @"
    Roms found:              " + _romCount + @"
    Disks found:             " + _diskCount + @"
    Roms with CRC:           " + _crcCount + @"
    Roms with MD5:           " + _md5Count + @"
    Roms with SHA-1:         " + _sha1Count + @"
    Roms with SHA-256:       " + _sha256Count + @"
    Roms with SHA-384:       " + _sha384Count + @"
    Roms with SHA-512:       " + _sha512Count + "\n";

			if (baddumpCol)
			{
				results += "	Roms with BadDump status: " + _baddumpCount + "\n";
			}
			if (nodumpCol)
			{
				results += "	Roms with Nodump status: " + _nodumpCount + "\n";
			}

			// For spacing between DATs
			results += "\n\n";

			Globals.Logger.User(results);

			// Now write it out to file as well
			string line = "";
			if (outputs.ContainsKey(StatDatFormat.None))
			{
				line = @"'" + _fileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(_totalSize) + @"
    Games found:             " + (game == -1 ? Keys.Count() : game) + @"
    Roms found:              " + _romCount + @"
    Disks found:             " + _diskCount + @"
    Roms with CRC:           " + _crcCount + @"
    Roms with SHA-1:         " + _sha1Count + @"
    Roms with SHA-256:       " + _sha256Count + @"
    Roms with SHA-384:       " + _sha384Count + @"
    Roms with SHA-512:       " + _sha512Count + "\n";

				if (baddumpCol)
				{
					line += "	Roms with BadDump status: " + _baddumpCount + "\n";
				}
				if (nodumpCol)
				{
					line += "	Roms with Nodump status: " + _nodumpCount + "\n";
				}

				// For spacing between DATs
				line += "\n\n";

				outputs[StatDatFormat.None].Write(line);
			}
			if (outputs.ContainsKey(StatDatFormat.CSV))
			{
				line = "\"" + _fileName + "\","
					+ "\"" + _totalSize + "\","
					+ "\"" + (game == -1 ? Keys.Count() : game) + "\","
					+ "\"" + _romCount + "\","
					+ "\"" + _diskCount + "\","
					+ "\"" + _crcCount + "\","
					+ "\"" + _md5Count + "\","
					+ "\"" + _sha1Count + "\","
					+ "\"" + _sha256Count + "\","
					+ "\"" + _sha384Count + "\","
					+ "\"" + _sha512Count + "\"";

				if (baddumpCol)
				{
					line += ",\"" + _baddumpCount + "\"";
				}
				if (nodumpCol)
				{
					line += ",\"" + _nodumpCount + "\"";
				}

				line += "\n";
				outputs[StatDatFormat.CSV].Write(line);
			}
			if (outputs.ContainsKey(StatDatFormat.HTML))
			{
				line = "\t\t\t<tr" + (_fileName.StartsWith("DIR: ")
							? " class=\"dir\"><td>" + HttpUtility.HtmlEncode(_fileName.Remove(0, 5))
							: "><td>" + HttpUtility.HtmlEncode(_fileName)) + "</td>"
						+ "<td align=\"right\">" + Style.GetBytesReadable(_totalSize) + "</td>"
						+ "<td align=\"right\">" + (game == -1 ? Keys.Count() : game) + "</td>"
						+ "<td align=\"right\">" + _romCount + "</td>"
						+ "<td align=\"right\">" + _diskCount + "</td>"
						+ "<td align=\"right\">" + _crcCount + "</td>"
						+ "<td align=\"right\">" + _md5Count + "</td>"
						+ "<td align=\"right\">" + _sha1Count + "</td>"
						+ "<td align=\"right\">" + _sha256Count + "</td>";

				if (baddumpCol)
				{
					line += "<td align=\"right\">" + _baddumpCount + "</td>";
				}
				if (nodumpCol)
				{
					line += "<td align=\"right\">" + _nodumpCount + "</td>";
				}

				line += "</tr>\n";
				outputs[StatDatFormat.HTML].Write(line);
			}
			if (outputs.ContainsKey(StatDatFormat.TSV))
			{
				line = "\"" + _fileName + "\"\t"
						+ "\"" + _totalSize + "\"\t"
						+ "\"" + (game == -1 ? Keys.Count() : game) + "\"\t"
						+ "\"" + _romCount + "\"\t"
						+ "\"" + _diskCount + "\"\t"
						+ "\"" + _crcCount + "\"\t"
						+ "\"" + _md5Count + "\"\t"
						+ "\"" + _sha1Count + "\"\t"
						+ "\"" + _sha256Count + "\"\t"
						+ "\"" + _sha384Count + "\"\t"
						+ "\"" + _sha512Count + "\"";

				if (baddumpCol)
				{
					line += "\t\"" + _baddumpCount + "\"";
				}
				if (nodumpCol)
				{
					line += "\t\"" + _nodumpCount + "\"";
				}

				line += "\n";
				outputs[StatDatFormat.TSV].Write(line);
			}
		}

		#endregion

		#endregion // Instance Methods

		#region Static Methods

		#region Statistics

		/// <summary>
		/// Output the stats for a list of input dats as files in a human-readable format
		/// </summary>
		/// <param name="inputs">List of input files and folders</param>
		/// <param name="reportName">Name of the output file</param>
		/// <param name="single">True if single DAT stats are output, false otherwise</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		/// <param name="statDatFormat" > Set the statistics output format to use</param>
		public static void OutputStats(List<string> inputs, string reportName, string outDir, bool single,
			bool baddumpCol, bool nodumpCol, StatDatFormat statDatFormat)
		{
			// If there's no output format, set the default
			if (statDatFormat == 0x0)
			{
				statDatFormat = StatDatFormat.None;
			}

			// Get the proper output file name
			if (String.IsNullOrEmpty(reportName))
			{
				reportName = "report";
			}
			outDir = Path.GetFullPath(outDir);

			// Get the dictionary of desired outputs
			Dictionary<StatDatFormat, StreamWriter> outputs = OutputStatsGetOutputWriters(statDatFormat, reportName, outDir);

			// Make sure we have all files
			List<Tuple<string, string>> newinputs = new List<Tuple<string, string>>(); // item, basepath
			Parallel.ForEach(inputs, input =>
			{
				if (File.Exists(input))
				{
					lock (newinputs)
					{
						newinputs.Add(Tuple.Create(Path.GetFullPath(input), Path.GetDirectoryName(Path.GetFullPath(input))));
					}
				}
				if (Directory.Exists(input))
				{
					foreach (string file in Directory.GetFiles(input, "*", SearchOption.AllDirectories))
					{
						lock (newinputs)
						{
							newinputs.Add(Tuple.Create(Path.GetFullPath(file), Path.GetFullPath(input)));
						}
					}
				}
			});

			newinputs = newinputs
				.OrderBy(i => Path.GetDirectoryName(i.Item1))
				.ThenBy(i => Path.GetFileName(i.Item1))
				.ToList();

			// Write the header, if any
			OutputStatsWriteHeader(outputs, statDatFormat, baddumpCol, nodumpCol);

			// Init all total variables
			long totalSize = 0;
			long totalGame = 0;
			long totalRom = 0;
			long totalDisk = 0;
			long totalCRC = 0;
			long totalMD5 = 0;
			long totalSHA1 = 0;
			long totalSHA256 = 0;
			long totalSHA384 = 0;
			long totalSHA512 = 0;
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
			long dirSHA256 = 0;
			long dirSHA384 = 0;
			long dirSHA512 = 0;
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
					OutputStatsWriteMidSeparator(outputs, statDatFormat, baddumpCol, nodumpCol);

					DatFile lastdirdat = new DatFile
					{
						_fileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
						_totalSize = dirSize,
						_romCount = dirRom,
						_diskCount = dirDisk,
						_crcCount = dirCRC,
						_md5Count = dirMD5,
						_sha1Count = dirSHA1,
						_sha256Count = dirSHA256,
						_sha384Count = dirSHA384,
						_sha512Count = dirSHA512,
						_baddumpCount = dirBaddump,
						_nodumpCount = dirNodump,
					};
					lastdirdat.OutputStats(outputs, statDatFormat,
						game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);

					// Write the mid-footer, if any
					OutputStatsWriteMidFooter(outputs, statDatFormat, baddumpCol, nodumpCol);

					// Write the header, if any
					OutputStatsWriteMidHeader(outputs, statDatFormat, baddumpCol, nodumpCol);

					// Reset the directory stats
					dirSize = 0;
					dirGame = 0;
					dirRom = 0;
					dirDisk = 0;
					dirCRC = 0;
					dirMD5 = 0;
					dirSHA1 = 0;
					dirSHA256 = 0;
					dirSHA384 = 0;
					dirSHA512 = 0;
					dirBaddump = 0;
					dirNodump = 0;
				}

				Globals.Logger.Verbose("Beginning stat collection for '" + filename.Item1 + "'", false);
				List<string> games = new List<string>();
				DatFile datdata = new DatFile();
				datdata.Parse(filename.Item1, 0, 0);
				datdata.BucketBy(SortedBy.Game, false /* mergeroms */, norename: true);

				// Output single DAT stats (if asked)
				Globals.Logger.User("Adding stats for file '" + filename.Item1 + "'\n", false);
				if (single)
				{
					datdata.OutputStats(outputs, statDatFormat,
						baddumpCol: baddumpCol, nodumpCol: nodumpCol);
				}

				// Add single DAT stats to dir
				dirSize += datdata.TotalSize;
				dirGame += datdata.Keys.Count();
				dirRom += datdata.RomCount;
				dirDisk += datdata.DiskCount;
				dirCRC += datdata.CRCCount;
				dirMD5 += datdata.MD5Count;
				dirSHA1 += datdata.SHA1Count;
				dirSHA256 += datdata.SHA256Count;
				dirSHA384 += datdata.SHA384Count;
				dirSHA512 += datdata.SHA512Count;
				dirBaddump += datdata.BaddumpCount;
				dirNodump += datdata.NodumpCount;

				// Add single DAT stats to totals
				totalSize += datdata.TotalSize;
				totalGame += datdata.Keys.Count();
				totalRom += datdata.RomCount;
				totalDisk += datdata.DiskCount;
				totalCRC += datdata.CRCCount;
				totalMD5 += datdata.MD5Count;
				totalSHA1 += datdata.SHA1Count;
				totalSHA256 += datdata.SHA256Count;
				totalSHA384 += datdata.SHA384Count;
				totalSHA512 += datdata.SHA512Count;
				totalBaddump += datdata.BaddumpCount;
				totalNodump += datdata.NodumpCount;

				// Make sure to assign the new directory
				lastdir = thisdir;
			}

			// Output the directory stats one last time
			OutputStatsWriteMidSeparator(outputs, statDatFormat, baddumpCol, nodumpCol);

			if (single)
			{
				DatFile dirdat = new DatFile
				{
					_fileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
					_totalSize = dirSize,
					_romCount = dirRom,
					_diskCount = dirDisk,
					_crcCount = dirCRC,
					_md5Count = dirMD5,
					_sha1Count = dirSHA1,
					_sha256Count = dirSHA256,
					_sha384Count = dirSHA384,
					_sha512Count = dirSHA512,
					_baddumpCount = dirBaddump,
					_nodumpCount = dirNodump,
				};
				dirdat.OutputStats(outputs, statDatFormat,
					game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
			}

			// Write the mid-footer, if any
			OutputStatsWriteMidFooter(outputs, statDatFormat, baddumpCol, nodumpCol);

			// Write the header, if any
			OutputStatsWriteMidHeader(outputs, statDatFormat, baddumpCol, nodumpCol);

			// Reset the directory stats
			dirSize = 0;
			dirGame = 0;
			dirRom = 0;
			dirDisk = 0;
			dirCRC = 0;
			dirMD5 = 0;
			dirSHA1 = 0;
			dirSHA256 = 0;
			dirSHA384 = 0;
			dirSHA512 = 0;
			dirNodump = 0;

			// Output total DAT stats
			DatFile totaldata = new DatFile
			{
				_fileName = "DIR: All DATs",
				_totalSize = totalSize,
				_romCount = totalRom,
				_diskCount = totalDisk,
				_crcCount = totalCRC,
				_md5Count = totalMD5,
				_sha1Count = totalSHA1,
				_sha256Count = totalSHA256,
				_sha384Count = totalSHA384,
				_sha512Count = totalSHA512,
				_baddumpCount = totalBaddump,
				_nodumpCount = totalNodump,
			};
			totaldata.OutputStats(outputs, statDatFormat,
				game: totalGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);

			// Output footer if needed
			OutputStatsWriteFooter(outputs, statDatFormat);

			// Flush and dispose of the stream writers
			foreach (StatDatFormat format in outputs.Keys)
			{
				outputs[format].Flush();
				outputs[format].Dispose();
			}

			Globals.Logger.User(@"
Please check the log folder if the stats scrolled offscreen", false);
		}

		/// <summary>
		/// Get the proper extension for the stat output format
		/// </summary>
		/// <param name="statDatFormat">StatDatFormat to get the extension for</param>
		/// <param name="reportName">Name of the input file to use</param>
		/// <param name="outDir">Output path to use</param>
		/// <returns>Dictionary of file types to StreamWriters</returns>
		private static Dictionary<StatDatFormat, StreamWriter> OutputStatsGetOutputWriters(StatDatFormat statDatFormat, string reportName, string outDir)
		{
			Dictionary<StatDatFormat, StreamWriter> output = new Dictionary<StatDatFormat, StreamWriter>();

			// First try to create the output directory if we need to
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// For each output format, get the appropriate stream writer
			if ((statDatFormat & StatDatFormat.None) != 0)
			{
				reportName = Style.GetFileNameWithoutExtension(reportName) + ".txt";
				reportName = Path.Combine(outDir, reportName);

				// Create the StreamWriter for this file
				output.Add(StatDatFormat.None, new StreamWriter(FileTools.TryCreate(reportName)));
			}
			if ((statDatFormat & StatDatFormat.CSV) != 0)
			{
				reportName = Style.GetFileNameWithoutExtension(reportName) + ".csv";
				reportName = Path.Combine(outDir, reportName);

				// Create the StreamWriter for this file
				output.Add(StatDatFormat.CSV, new StreamWriter(FileTools.TryCreate(reportName)));
			}
			if ((statDatFormat & StatDatFormat.HTML) != 0)
			{
				reportName = Style.GetFileNameWithoutExtension(reportName) + ".html";
				reportName = Path.Combine(outDir, reportName);

				// Create the StreamWriter for this file
				output.Add(StatDatFormat.HTML, new StreamWriter(FileTools.TryCreate(reportName)));
			}
			if ((statDatFormat & StatDatFormat.TSV) != 0)
			{
				reportName = Style.GetFileNameWithoutExtension(reportName) + ".csv";
				reportName = Path.Combine(outDir, reportName);

				// Create the StreamWriter for this file
				output.Add(StatDatFormat.TSV, new StreamWriter(FileTools.TryCreate(reportName)));
			}

			return output;
		}

		/// <summary>
		/// Write out the header to the stream, if any exists
		/// </summary>
		/// <param name="outputs">Dictionary representing the outputs</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteHeader(Dictionary<StatDatFormat, StreamWriter> outputs, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			if (outputs.ContainsKey(StatDatFormat.None))
			{
				// Nothing
			}
			if (outputs.ContainsKey(StatDatFormat.CSV))
			{
				outputs[StatDatFormat.CSV].Write("\"File Name\",\"Total Size\",\"Games\",\"Roms\",\"Disks\",\"# with CRC\",\"# with MD5\",\"# with SHA-1\",\"# with SHA-256\""
					+ (baddumpCol ? ",\"BadDumps\"" : "") + (nodumpCol ? ",\"Nodumps\"" : "") + "\n");
			}
			if (outputs.ContainsKey(StatDatFormat.HTML))
			{
				outputs[StatDatFormat.HTML].Write(@"<!DOCTYPE html>
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
");
			}
			if (outputs.ContainsKey(StatDatFormat.TSV))
			{
				outputs[StatDatFormat.TSV].Write("\"File Name\"\t\"Total Size\"\t\"Games\"\t\"Roms\"\t\"Disks\"\t\"# with CRC\"\t\"# with MD5\"\t\"# with SHA-1\"\t\"# with SHA-256\""
						+ (baddumpCol ? "\t\"BadDumps\"" : "") + (nodumpCol ? "\t\"Nodumps\"" : "") + "\n");
			}

			// Now write the mid header for those who need it
			OutputStatsWriteMidHeader(outputs, statDatFormat, baddumpCol, nodumpCol);
		}

		/// <summary>
		/// Write out the mid-header to the stream, if any exists
		/// </summary>
		/// <param name="outputs">Dictionary representing the outputs</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidHeader(Dictionary<StatDatFormat, StreamWriter> outputs, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			if (outputs.ContainsKey(StatDatFormat.None))
			{
				// Nothing
			}
			if (outputs.ContainsKey(StatDatFormat.CSV))
			{
				// Nothing
			}
			if (outputs.ContainsKey(StatDatFormat.HTML))
			{
				outputs[StatDatFormat.HTML].Write(@"			<tr bgcolor=""gray""><th>File Name</th><th align=""right"">Total Size</th><th align=""right"">Games</th><th align=""right"">Roms</th>"
+ @"<th align=""right"">Disks</th><th align=""right"">&#35; with CRC</th><th align=""right"">&#35; with MD5</th><th align=""right"">&#35; with SHA-1</th><th align=""right"">&#35; with SHA-256</th>"
+ (baddumpCol ? "<th class=\".right\">Baddumps</th>" : "") + (nodumpCol ? "<th class=\".right\">Nodumps</th>" : "") + "</tr>\n");
			}
			if (outputs.ContainsKey(StatDatFormat.TSV))
			{
				// Nothing
			}
		}

		/// <summary>
		/// Write out the separator to the stream, if any exists
		/// </summary>
		/// <param name="outputs">Dictionary representing the outputs</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidSeparator(Dictionary<StatDatFormat, StreamWriter> outputs, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			if (outputs.ContainsKey(StatDatFormat.None))
			{
				// Nothing
			}
			if (outputs.ContainsKey(StatDatFormat.CSV))
			{
				// Nothing
			}
			if (outputs.ContainsKey(StatDatFormat.HTML))
			{
				outputs[StatDatFormat.HTML].Write("<tr><td colspan=\""
						+ (baddumpCol && nodumpCol
							? "12"
							: (baddumpCol ^ nodumpCol
								? "11"
								: "10")
							)
						+ "\"></td></tr>\n");
			}
			if (outputs.ContainsKey(StatDatFormat.TSV))
			{
				// Nothing
			}
		}

		/// <summary>
		/// Write out the footer-separator to the stream, if any exists
		/// </summary>
		/// <param name="outputs">Dictionary representing the outputs</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		private static void OutputStatsWriteMidFooter(Dictionary<StatDatFormat, StreamWriter> outputs, StatDatFormat statDatFormat, bool baddumpCol, bool nodumpCol)
		{
			if (outputs.ContainsKey(StatDatFormat.None))
			{
				outputs[StatDatFormat.None].Write("\n");
			}
			if (outputs.ContainsKey(StatDatFormat.CSV))
			{
				outputs[StatDatFormat.CSV].Write("\n");
			}
			if (outputs.ContainsKey(StatDatFormat.HTML))
			{
				outputs[StatDatFormat.HTML].Write("<tr border=\"0\"><td colspan=\""
						+ (baddumpCol && nodumpCol
							? "12"
							: (baddumpCol ^ nodumpCol
								? "11"
								: "10")
							)
						+ "\"></td></tr>\n");
			}
			if (outputs.ContainsKey(StatDatFormat.TSV))
			{
				outputs[StatDatFormat.TSV].Write("\n");
			}
		}

		/// <summary>
		/// Write out the footer to the stream, if any exists
		/// </summary>
		/// <param name="sw">StreamWriter representing the output</param>
		/// <param name="statDatFormat">StatDatFormat representing output format</param>
		private static void OutputStatsWriteFooter(Dictionary<StatDatFormat, StreamWriter> outputs, StatDatFormat statDatFormat)
		{
			if (outputs.ContainsKey(StatDatFormat.None))
			{
				// Nothing
			}
			if (outputs.ContainsKey(StatDatFormat.CSV))
			{
				// Nothing
			}
			if (outputs.ContainsKey(StatDatFormat.HTML))
			{
				outputs[StatDatFormat.HTML].Write(@"		</table>
	</body>
</html>
");
			}
			if (outputs.ContainsKey(StatDatFormat.TSV))
			{
				// Nothing
			}
		}

		#endregion

		#endregion // Static Methods
	}
}
