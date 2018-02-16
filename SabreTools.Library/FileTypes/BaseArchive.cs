using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;

#if MONO
using System.IO;
#else
using MemoryStream = System.IO.MemoryStream;
using Stream = System.IO.Stream;
#endif

namespace SabreTools.Library.FileTypes
{
	public abstract class BaseArchive : BaseFile
	{
		#region Protected instance variables

		// Buffer size used by archives
		protected const int _bufferSize = 4096 * 128;

		#endregion

		#region Construtors

		/// <summary>
		/// Create a new Archive with no base file
		/// </summary>
		public BaseArchive()
		{
		}

		/// <summary>
		/// Create a new Archive from the given file
		/// </summary>
		/// <param name="filename">Name of the file to use as an archive</param>
		public BaseArchive(string filename)
			: base(filename)
		{
		}

		#endregion

		#region Extraction

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="outDir">Output directory for archive extraction</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public abstract bool ExtractAll(string outDir);

		/// <summary>
		/// Attempt to extract an entry from an archive
		/// </summary>
		/// <param name="entryName">Name of the entry to be extracted</param>
		/// <param name="outDir">Output directory for archive extraction</param>
		/// <returns>Name of the extracted file, null on error</returns>
		public abstract string ExtractEntry(string entryName, string outDir);

		/// <summary>
		/// Attempt to extract a stream from an archive
		/// </summary>
		/// <param name="entryName">Name of the entry to be extracted</param>
		/// <param name="realEntry">Output representing the entry name that was found</param>
		/// <returns>MemoryStream representing the entry, null on error</returns>
		public abstract (MemoryStream, string) ExtractEntryStream(string entryName);

		#endregion

		#region Information

		/// <summary>
		/// Generate a list of DatItem objects from the header values in an archive
		/// </summary>
		/// <param name="omitFromScan">Hash representing the hashes that should be skipped</param>
		/// <param name="date">True if entry dates should be included, false otherwise (default)</param>
		/// <returns>List of DatItem objects representing the found data</returns>
		/// <remarks>TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually</remarks>
		public abstract List<Rom> GetArchiveFileInfo(Hash omitFromScan = Hash.DeepHashes, bool date = false);

		/// <summary>
		/// Generate a list of empty folders in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <returns>List of empty folders in the archive</returns>
		public abstract List<string> GetEmptyFolders();

		/// <summary>
		/// Check whether the input file is a standardized format
		/// </summary>
		public abstract bool IsTorrent();

		#endregion

		#region Writing

		/// <summary>
		/// Write an input file to an archive
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">DatItem representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public abstract bool Write(string inputFile, string outDir, Rom rom, bool date = false, bool romba = false);

		/// <summary>
		/// Write an input stream to an archive
		/// </summary>
		/// <param name="inputStream">Input stream to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">DatItem representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public abstract bool Write(Stream inputStream, string outDir, Rom rom, bool date = false, bool romba = false);

		/// <summary>
		/// Write a set of input files to an archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFiles">Input files to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">DatItem representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public abstract bool Write(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false, bool romba = false);

		#endregion
	}
}
