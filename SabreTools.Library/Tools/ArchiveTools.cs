using System;

using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using BinaryReader = System.IO.BinaryReader;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
#endif
using SharpCompress.Common;

namespace SabreTools.Library.Tools
{
	/// <summary>
	/// Tools for working with archives
	/// </summary>
	public static class ArchiveTools
	{
		#region Factories

		/// <summary>
		/// Create an archive of the specified type, if possible
		/// </summary>
		/// <param name="input">Name of the file to create the archive from</param>
		/// <returns>Archive object representing the inputs</returns>
		public static BaseArchive CreateArchiveFromExistingInput(string input)
		{
			BaseArchive archive = null;

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return archive;
			}

			// Create the archive based on the type
			Globals.Logger.Verbose("Found archive of type: {0}", at);
			switch (at)
			{
				case ArchiveType.GZip:
					archive = new GZipArchive(input);
					break;
				case ArchiveType.Rar:
					archive = new RarArchive(input);
					break;
				case ArchiveType.SevenZip:
					archive = new SevenZipArchive(input);
					break;
				case ArchiveType.Tar:
					archive = new TapeArchive(input);
					break;
				case ArchiveType.Zip:
					archive = new TorrentZipArchive(input);
					break;
			}

			return archive;
		}

		/// <summary>
		/// Create an archive of the specified type, if possible
		/// </summary>
		/// <param name="archiveType">SharpCompress.Common.ArchiveType representing the archive to create</param>
		/// <returns>Archive object representing the inputs</returns>
		public static BaseArchive CreateArchiveFromArchiveType(ArchiveType archiveType)
		{
			switch(archiveType)
			{
				case ArchiveType.GZip:
					return new GZipArchive();
				case ArchiveType.Rar:
					return new RarArchive();
				case ArchiveType.SevenZip:
					return new SevenZipArchive();
				case ArchiveType.Tar:
					return new TapeArchive();
				case ArchiveType.Zip:
					return new TorrentZipArchive();
				default:
					return null;
			}
		}

		/// <summary>
		/// Create an archive of the specified type, if possible
		/// </summary>
		/// <param name="archiveType">SabreTools.Library.Data.SharpCompress.OutputFormat representing the archive to create</param>
		/// <returns>Archive object representing the inputs</returns>
		public static BaseArchive CreateArchiveFromOutputFormat(OutputFormat outputFormat)
		{
			switch (outputFormat)
			{
				case OutputFormat.Folder:
					return new Folder();
				case OutputFormat.TapeArchive:
					return new TapeArchive();
				case OutputFormat.Torrent7Zip:
					return new SevenZipArchive();
				case OutputFormat.TorrentGzip:
					return new GZipArchive();
				case OutputFormat.TorrentLRZip:
					return new LRZipArchive();
				case OutputFormat.TorrentLZ4:
					return new LZ4Archive();
				case OutputFormat.TorrentRar:
					return new RarArchive();
				case OutputFormat.TorrentXZ:
					return new XZArchive();
				case OutputFormat.TorrentZip:
					return new TorrentZipArchive();
				case OutputFormat.TorrentZPAQ:
					return new ZPAQArchive();
				case OutputFormat.TorrentZstd:
					return new ZstdArchive();
				default:
					return null;
			}
		}

		#endregion

		#region Information

		/// <summary>
		/// Returns the archive type of an input file
		/// </summary>
		/// <param name="input">Input file to check</param>
		/// <returns>ArchiveType of inputted file (null on error)</returns>
		public static ArchiveType? GetCurrentArchiveType(string input)
		{
			ArchiveType? outtype = null;

			// If the file is null, then we have no archive type
			if (input == null)
			{
				return outtype;
			}

			// First line of defense is going to be the extension, for better or worse
			string ext = Path.GetExtension(input).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}

			if (ext != "7z" && ext != "gz" && ext != "lzma" && ext != "rar"
				&& ext != "rev" && ext != "r00" && ext != "r01" && ext != "tar"
				&& ext != "tgz" && ext != "tlz" && ext != "zip" && ext != "zipx")
			{
				return outtype;
			}

			// Read the first bytes of the file and get the magic number
			try
			{
				byte[] magic = new byte[8];
				BinaryReader br = new BinaryReader(FileTools.TryOpenRead(input));
				magic = br.ReadBytes(8);
				br.Dispose();

				// Convert it to an uppercase string
				string mstr = string.Empty;
				for (int i = 0; i < magic.Length; i++)
				{
					mstr += BitConverter.ToString(new byte[] { magic[i] });
				}
				mstr = mstr.ToUpperInvariant();

				// Now try to match it to a known signature
				if (mstr.StartsWith(Constants.SevenZipSig))
				{
					outtype = ArchiveType.SevenZip;
				}
				else if (mstr.StartsWith(Constants.GzSig))
				{
					outtype = ArchiveType.GZip;
				}
				else if (mstr.StartsWith(Constants.RarSig) || mstr.StartsWith(Constants.RarFiveSig))
				{
					outtype = ArchiveType.Rar;
				}
				else if (mstr.StartsWith(Constants.TarSig) || mstr.StartsWith(Constants.TarZeroSig))
				{
					outtype = ArchiveType.Tar;
				}
				else if (mstr.StartsWith(Constants.ZipSig) || mstr.StartsWith(Constants.ZipSigEmpty) || mstr.StartsWith(Constants.ZipSigSpanned))
				{
					outtype = ArchiveType.Zip;
				}
			}
			catch (Exception)
			{
				// Don't log file open errors
			}

			return outtype;
		}

		/// <summary>
		/// Get if the current file should be scanned internally and externally
		/// </summary>
		/// <param name="input">Name of the input file to check</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="shouldExternalProcess">Output parameter determining if file should be processed externally</param>
		/// <param name="shouldInternalProcess">Output parameter determining if file should be processed internally</param>
		public static void GetInternalExternalProcess(string input, ArchiveScanLevel archiveScanLevel,
			out bool shouldExternalProcess, out bool shouldInternalProcess)
		{
			shouldExternalProcess = true;
			shouldInternalProcess = true;

			ArchiveType? archiveType = GetCurrentArchiveType(input);
			switch (archiveType)
			{
				case null:
					shouldExternalProcess = true;
					shouldInternalProcess = false;
					break;
				case ArchiveType.GZip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipInternal) != 0);
					break;
				case ArchiveType.Rar:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarInternal) != 0);
					break;
				case ArchiveType.SevenZip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipInternal) != 0);
					break;
				case ArchiveType.Zip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipInternal) != 0);
					break;
			}
		}

		/// <summary>
		/// Get the archive scan level based on the inputs
		/// </summary>
		/// <param name="sevenzip">User-defined scan level for 7z archives</param>
		/// <param name="gzip">User-defined scan level for GZ archives</param>
		/// <param name="rar">User-defined scan level for RAR archives</param>
		/// <param name="zip">User-defined scan level for Zip archives</param>
		/// <returns>ArchiveScanLevel representing the levels</returns>
		public static ArchiveScanLevel GetArchiveScanLevelFromNumbers(int sevenzip, int gzip, int rar, int zip)
		{
			ArchiveScanLevel archiveScanLevel = 0x0000;

			// 7z
			sevenzip = (sevenzip < 0 || sevenzip > 2 ? 0 : sevenzip);
			switch (sevenzip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.SevenZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.SevenZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.SevenZipExternal;
					break;
			}

			// GZip
			gzip = (gzip < 0 || gzip > 2 ? 0 : gzip);
			switch (gzip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.GZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.GZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.GZipExternal;
					break;
			}

			// RAR
			rar = (rar < 0 || rar > 2 ? 0 : rar);
			switch (rar)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.RarBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.RarInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.RarExternal;
					break;
			}

			// Zip
			zip = (zip < 0 || zip > 2 ? 0 : zip);
			switch (zip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.ZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.ZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.ZipExternal;
					break;
			}

			return archiveScanLevel;
		}

		#endregion
	}
}
