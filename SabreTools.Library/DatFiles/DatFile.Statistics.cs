using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using SabreTools.Library.Data;
using SabreTools.Library.Items;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
using StreamWriter = System.IO.StreamWriter;
#endif

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents a format-agnostic DAT
	/// </summary>
	/// <remarks>
	/// TODO: Make output standard width (HTML, without making the entire thing a table)
	/// TODO: Multithreading? Either StringBuilder or locking
	/// </remarks>
	public partial class DatFile
	{
		#region Instance Methods

		#region Statistics

		/// <summary>
		/// Recalculate the statistics for the Dat
		/// </summary>
		public void RecalculateStats()
		{
			// Wipe out any stats already there
			_datStats.Reset();

			// If we have a blank Dat in any way, return
			if (this == null || Count == 0)
			{
				return;
			}

			// Loop through and add
			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				foreach(DatItem item in items)
				{
					_datStats.AddItem(item);
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

			BucketBy(SortedBy.Game, DedupeType.None, norename: true);
			if (TotalSize < 0)
			{
				TotalSize = Int64.MaxValue + TotalSize;
			}

			// Log the results to screen
			string results = @"For '" + FileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(TotalSize) + @"
    Games found:             " + (game == -1 ? Keys.Count() : game) + @"
    Roms found:              " + RomCount + @"
    Disks found:             " + DiskCount + @"
    Roms with CRC:           " + CRCCount + @"
    Roms with MD5:           " + MD5Count + @"
    Roms with SHA-1:         " + SHA1Count + @"
    Roms with SHA-256:       " + SHA256Count + @"
    Roms with SHA-384:       " + SHA384Count + @"
    Roms with SHA-512:       " + SHA512Count + "\n";

			if (baddumpCol)
			{
				results += "	Roms with BadDump status: " + BaddumpCount + "\n";
			}
			if (nodumpCol)
			{
				results += "	Roms with Nodump status: " + NodumpCount + "\n";
			}

			// For spacing between DATs
			results += "\n\n";

			Globals.Logger.User(results);

			// Now write it out to file as well
			string line = "";
			if (outputs.ContainsKey(StatDatFormat.None))
			{
				line = @"'" + FileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(TotalSize) + @"
    Games found:             " + (game == -1 ? Keys.Count() : game) + @"
    Roms found:              " + RomCount + @"
    Disks found:             " + DiskCount + @"
    Roms with CRC:           " + CRCCount + @"
    Roms with SHA-1:         " + SHA1Count + @"
    Roms with SHA-256:       " + SHA256Count + @"
    Roms with SHA-384:       " + SHA384Count + @"
    Roms with SHA-512:       " + SHA512Count + "\n";

				if (baddumpCol)
				{
					line += "	Roms with BadDump status: " + BaddumpCount + "\n";
				}
				if (nodumpCol)
				{
					line += "	Roms with Nodump status: " + NodumpCount + "\n";
				}

				// For spacing between DATs
				line += "\n\n";

				outputs[StatDatFormat.None].Write(line);
			}
			if (outputs.ContainsKey(StatDatFormat.CSV))
			{
				line = "\"" + FileName + "\","
					+ "\"" + TotalSize + "\","
					+ "\"" + (game == -1 ? Keys.Count() : game) + "\","
					+ "\"" + RomCount + "\","
					+ "\"" + DiskCount + "\","
					+ "\"" + CRCCount + "\","
					+ "\"" + MD5Count + "\","
					+ "\"" + SHA1Count + "\","
					+ "\"" + SHA256Count + "\","
					+ "\"" + SHA384Count + "\","
					+ "\"" + SHA512Count + "\"";

				if (baddumpCol)
				{
					line += ",\"" + BaddumpCount + "\"";
				}
				if (nodumpCol)
				{
					line += ",\"" + NodumpCount + "\"";
				}

				line += "\n";
				outputs[StatDatFormat.CSV].Write(line);
			}
			if (outputs.ContainsKey(StatDatFormat.HTML))
			{
				line = "\t\t\t<tr" + (FileName.StartsWith("DIR: ")
							? " class=\"dir\"><td>" + HttpUtility.HtmlEncode(FileName.Remove(0, 5))
							: "><td>" + HttpUtility.HtmlEncode(FileName)) + "</td>"
						+ "<td align=\"right\">" + Style.GetBytesReadable(TotalSize) + "</td>"
						+ "<td align=\"right\">" + (game == -1 ? Keys.Count() : game) + "</td>"
						+ "<td align=\"right\">" + RomCount + "</td>"
						+ "<td align=\"right\">" + DiskCount + "</td>"
						+ "<td align=\"right\">" + CRCCount + "</td>"
						+ "<td align=\"right\">" + MD5Count + "</td>"
						+ "<td align=\"right\">" + SHA1Count + "</td>"
						+ "<td align=\"right\">" + SHA256Count + "</td>";

				if (baddumpCol)
				{
					line += "<td align=\"right\">" + BaddumpCount + "</td>";
				}
				if (nodumpCol)
				{
					line += "<td align=\"right\">" + NodumpCount + "</td>";
				}

				line += "</tr>\n";
				outputs[StatDatFormat.HTML].Write(line);
			}
			if (outputs.ContainsKey(StatDatFormat.TSV))
			{
				line = "\"" + FileName + "\"\t"
						+ "\"" + TotalSize + "\"\t"
						+ "\"" + (game == -1 ? Keys.Count() : game) + "\"\t"
						+ "\"" + RomCount + "\"\t"
						+ "\"" + DiskCount + "\"\t"
						+ "\"" + CRCCount + "\"\t"
						+ "\"" + MD5Count + "\"\t"
						+ "\"" + SHA1Count + "\"\t"
						+ "\"" + SHA256Count + "\"\t"
						+ "\"" + SHA384Count + "\"\t"
						+ "\"" + SHA512Count + "\"";

				if (baddumpCol)
				{
					line += "\t\"" + BaddumpCount + "\"";
				}
				if (nodumpCol)
				{
					line += "\t\"" + NodumpCount + "\"";
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
			Parallel.ForEach(inputs, Globals.ParallelOptions, input =>
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
			DatStats totalStats = new DatStats();
			long totalGame = 0;

			// Init directory-level variables
			string lastdir = null;
			string basepath = null;
			DatStats dirStats = new DatStats();
			long dirGame = 0;
			
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
						FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
						_datStats = dirStats,
					};

					lastdirdat.OutputStats(outputs, statDatFormat,
						game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);

					// Write the mid-footer, if any
					OutputStatsWriteMidFooter(outputs, statDatFormat, baddumpCol, nodumpCol);

					// Write the header, if any
					OutputStatsWriteMidHeader(outputs, statDatFormat, baddumpCol, nodumpCol);

					// Reset the directory stats
					dirStats.Reset();
					dirGame = 0;
				}

				Globals.Logger.Verbose("Beginning stat collection for '{0}'", false, filename.Item1);
				List<string> games = new List<string>();
				DatFile datdata = new DatFile();
				datdata.Parse(filename.Item1, 0, 0);
				datdata.BucketBy(SortedBy.Game, DedupeType.None, norename: true);

				// Output single DAT stats (if asked)
				Globals.Logger.User("Adding stats for file '{0}'\n", false, filename.Item1);
				if (single)
				{
					datdata.OutputStats(outputs, statDatFormat,
						baddumpCol: baddumpCol, nodumpCol: nodumpCol);
				}

				// Add single DAT stats to dir
				dirStats.AddStats(datdata._datStats);
				dirGame += datdata.Keys.Count();

				// Add single DAT stats to totals
				totalStats.AddStats(datdata._datStats);
				totalGame += datdata.Keys.Count();

				// Make sure to assign the new directory
				lastdir = thisdir;
			}

			// Output the directory stats one last time
			OutputStatsWriteMidSeparator(outputs, statDatFormat, baddumpCol, nodumpCol);

			if (single)
			{
				DatFile dirdat = new DatFile
				{
					FileName = "DIR: " + HttpUtility.HtmlEncode(lastdir.Remove(0, basepath.Length + (basepath.Length == 0 ? 0 : 1))),
					_datStats = dirStats,
				};

				dirdat.OutputStats(outputs, statDatFormat,
					game: dirGame, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
			}

			// Write the mid-footer, if any
			OutputStatsWriteMidFooter(outputs, statDatFormat, baddumpCol, nodumpCol);

			// Write the header, if any
			OutputStatsWriteMidHeader(outputs, statDatFormat, baddumpCol, nodumpCol);

			// Reset the directory stats
			dirStats.Reset();
			dirGame = 0;

			// Output total DAT stats
			DatFile totaldata = new DatFile
			{
				FileName = "DIR: All DATs",
				_datStats = totalStats,
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
