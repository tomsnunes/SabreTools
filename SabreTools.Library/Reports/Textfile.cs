using System.Linq;

using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using IOException = System.IO.IOException;
using MemoryStream = System.IO.MemoryStream;
using SearchOption = System.IO.SearchOption;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
using StreamWriter = System.IO.StreamWriter;
#endif

namespace SabreTools.Library.Reports
{
	/// <summary>
	/// Textfile report format
	/// </summary>
	public class Textfile : BaseReport
	{
		/// <summary>
		/// Create a new report from the input DatFile and the filename
		/// </summary>
		/// <param name="datfile">DatFile to write out statistics for</param>
		/// <param name="filename">Name of the file to write out to</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		public Textfile(DatFile datfile, string filename, bool baddumpCol = false, bool nodumpCol = false)
			: base(datfile, filename, baddumpCol, nodumpCol)
		{
		}

		/// <summary>
		/// Create a new report from the input DatFile and the stream
		/// </summary>
		/// <param name="datfile">DatFile to write out statistics for</param>
		/// <param name="stream">Output stream to write to</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		public Textfile(DatFile datfile, Stream stream, bool baddumpCol = false, bool nodumpCol = false)
			: base(datfile, stream, baddumpCol, nodumpCol)
		{
		}

		/// <summary>
		/// Write the report to file
		/// </summary>
		/// <param name="game">Number of games to use, -1 means use the number of keys</param>
		public override void Write(long game = -1)
		{
			string line = @"'" + _datFile.FileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Style.GetBytesReadable(_datFile.TotalSize) + @"
    Games found:             " + (game == -1 ? _datFile.Keys.Count() : game) + @"
    Roms found:              " + _datFile.RomCount + @"
    Disks found:             " + _datFile.DiskCount + @"
    Roms with CRC:           " + _datFile.CRCCount + @"
    Roms with SHA-1:         " + _datFile.SHA1Count + @"
    Roms with SHA-256:       " + _datFile.SHA256Count + @"
    Roms with SHA-384:       " + _datFile.SHA384Count + @"
    Roms with SHA-512:       " + _datFile.SHA512Count + "\n";

			if (_baddumpCol)
			{
				line += "	Roms with BadDump status: " + _datFile.BaddumpCount + "\n";
			}
			if (_nodumpCol)
			{
				line += "	Roms with Nodump status: " + _datFile.NodumpCount + "\n";
			}

			// For spacing between DATs
			line += "\n\n";

			_writer.Write(line);
			_writer.Flush();
		}

		/// <summary>
		/// Write out the header to the stream, if any exists
		/// </summary>
		public override void WriteStatsHeader()
		{
			// This call is a no-op for textfile output
		}

		/// <summary>
		/// Write out the mid-header to the stream, if any exists
		/// </summary>
		public override void WriteStatsMidHeader()
		{
			// This call is a no-op for textfile output
		}

		/// <summary>
		/// Write out the separator to the stream, if any exists
		/// </summary>
		public override void WriteStatsMidSeparator()
		{
			// This call is a no-op for textfile output
		}

		/// <summary>
		/// Write out the footer-separator to the stream, if any exists
		/// </summary>
		public override void WriteStatsFooterSeparator()
		{
			_writer.Write("\n");
			_writer.Flush();
		}

		/// <summary>
		/// Write out the footer to the stream, if any exists
		/// </summary>
		public override void WriteStatsFooter()
		{
			// This call is a no-op for textfile output
		}
	}
}
