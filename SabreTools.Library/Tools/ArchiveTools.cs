using System;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Items;

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
	/// <remarks>
	/// TODO: Full archive support for: RAR, LRZip, ZPAQ?, Zstd?, LZ4?
	/// ZPAQ: https://github.com/zpaq/zpaq - In progress as external DLL
	/// Zstd: https://github.com/skbkontur/ZstdNet
	/// LZ4: https://github.com/lz4/lz4
	/// </remarks>
	public static class ArchiveTools
	{
		#region Factory

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
				case OutputFormat.TorrentLrzip:
					return new LRZArchive();
				case OutputFormat.TorrentRar:
					return new RarArchive();
				case OutputFormat.TorrentXZ:
					return new XZArchive();
				case OutputFormat.TorrentZip:
					return new TorrentZipArchive();
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

		/// <summary>
		/// (INCOMPLETE) Retrieve file information for a RAR file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		public static void GetRarFileInfo(string input)
		{
			if (!File.Exists(input))
			{
				return;
			}

			BinaryReader br = new BinaryReader(FileTools.TryOpenRead(input));

			// Check for the signature first (Skipping the SFX Module)
			byte[] signature = br.ReadBytes(8);
			int startpos = 0;
			while (startpos < Constants.MibiByte && BitConverter.ToString(signature, 0, 7) != Constants.RarSig && BitConverter.ToString(signature) != Constants.RarFiveSig)
			{
				startpos++;
				br.BaseStream.Position = startpos;
				signature = br.ReadBytes(8);
			}
			if (BitConverter.ToString(signature, 0, 7) != Constants.RarSig && BitConverter.ToString(signature) != Constants.RarFiveSig)
			{
				return;
			}

			CoreRarArchive cra = new CoreRarArchive();
			if (startpos > 0)
			{
				br.BaseStream.Position = 0;
				cra.SFX = br.ReadBytes(startpos);
			}

			// Get all archive header information
			cra.HeaderCRC32 = br.ReadUInt32();
			cra.HeaderSize = br.ReadUInt32();
			uint headerType = br.ReadUInt32();

			// Special encryption information
			bool hasEncryptionHeader = false;

			// If it's encrypted
			if (headerType == (uint)RarHeaderType.ArchiveEncryption)
			{
				hasEncryptionHeader = true;
				cra.EncryptionHeaderCRC32 = cra.HeaderCRC32;
				cra.EncryptionHeaderSize = cra.HeaderSize;
				cra.EncryptionHeaderFlags = (RarHeaderFlags)br.ReadUInt32();
				cra.EncryptionVersion = br.ReadUInt32();
				cra.EncryptionFlags = br.ReadUInt32();
				cra.KDFCount = br.ReadByte();
				cra.Salt = br.ReadBytes(16);
				cra.CheckValue = br.ReadBytes(12);

				cra.HeaderCRC32 = br.ReadUInt32();
				cra.HeaderSize = br.ReadUInt32();
				headerType = br.ReadUInt32();
			}

			cra.HeaderFlags = (RarHeaderFlags)br.ReadUInt32();
			if ((cra.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0)
			{
				cra.ExtraAreaSize = br.ReadUInt32();
			}
			cra.ArchiveFlags = (RarArchiveFlags)br.ReadUInt32();
			if ((cra.ArchiveFlags & RarArchiveFlags.VolumeNumberField) != 0)
			{
				cra.VolumeNumber = br.ReadUInt32();
			}
			if (((cra.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0) && cra.ExtraAreaSize != 0)
			{
				cra.ExtraArea = br.ReadBytes((int)cra.ExtraAreaSize);
			}

			// Archive Comment Service Header

			// Now for file headers
			for (;;)
			{
				CoreRarArchiveEntry crae = new CoreRarArchiveEntry();
				crae.HeaderCRC32 = br.ReadUInt32();
				crae.HeaderSize = br.ReadUInt32();
				crae.HeaderType = (RarHeaderType)br.ReadUInt32();

				if (crae.HeaderType == RarHeaderType.EndOfArchive)
				{
					break;
				}

				crae.HeaderFlags = (RarHeaderFlags)br.ReadUInt32();
				if ((crae.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0)
				{
					crae.ExtraAreaSize = br.ReadUInt32();
				}
				if ((crae.HeaderFlags & RarHeaderFlags.DataAreaPresent) != 0)
				{
					crae.DataAreaSize = br.ReadUInt32();
				}
				crae.FileFlags = (RarFileFlags)br.ReadUInt32();
				crae.UnpackedSize = br.ReadUInt32();
				if ((crae.FileFlags & RarFileFlags.UnpackedSizeUnknown) != 0)
				{
					crae.UnpackedSize = 0;
				}
				crae.Attributes = br.ReadUInt32();
				crae.mtime = br.ReadUInt32();
				crae.DataCRC32 = br.ReadUInt32();
				crae.CompressionInformation = br.ReadUInt32();
				crae.HostOS = br.ReadUInt32();
				crae.NameLength = br.ReadUInt32();
				crae.Name = br.ReadBytes((int)crae.NameLength);
				if ((crae.HeaderFlags & RarHeaderFlags.ExtraAreaPresent) != 0)
				{
					uint extraSize = br.ReadUInt32();
					switch (br.ReadUInt32()) // Extra Area Type
					{
						case 0x01: // File encryption information
							crae.EncryptionSize = extraSize;
							crae.EncryptionFlags = (RarEncryptionFlags)br.ReadUInt32();
							crae.KDFCount = br.ReadByte();
							crae.Salt = br.ReadBytes(16);
							crae.IV = br.ReadBytes(16);
							crae.CheckValue = br.ReadBytes(12);
							break;

						case 0x02: // File data hash
							crae.HashSize = extraSize;
							crae.HashType = br.ReadUInt32();
							crae.HashData = br.ReadBytes(32);
							break;

						case 0x03: // High precision file time
							crae.TimeSize = extraSize;
							crae.TimeFlags = (RarTimeFlags)br.ReadUInt32();
							if ((crae.TimeFlags & RarTimeFlags.TimeInUnixFormat) != 0)
							{
								if ((crae.TimeFlags & RarTimeFlags.ModificationTimePresent) != 0)
								{
									crae.TimeMtime64 = br.ReadUInt64();
								}
								if ((crae.TimeFlags & RarTimeFlags.CreationTimePresent) != 0)
								{
									crae.TimeCtime64 = br.ReadUInt64();
								}
								if ((crae.TimeFlags & RarTimeFlags.LastAccessTimePresent) != 0)
								{
									crae.TimeLtime64 = br.ReadUInt64();
								}
							}
							else
							{
								if ((crae.TimeFlags & RarTimeFlags.ModificationTimePresent) != 0)
								{
									crae.TimeMtime = br.ReadUInt32();
								}
								if ((crae.TimeFlags & RarTimeFlags.CreationTimePresent) != 0)
								{
									crae.TimeCtime = br.ReadUInt32();
								}
								if ((crae.TimeFlags & RarTimeFlags.LastAccessTimePresent) != 0)
								{
									crae.TimeLtime = br.ReadUInt32();
								}
							}
							break;

						case 0x04: // File version number
							crae.VersionSize = extraSize;
							/* crae.VersionFlags = */ br.ReadUInt32();
							crae.VersionNumber = br.ReadUInt32();
							break;

						case 0x05: // File system redirection
							crae.RedirectionSize = extraSize;
							crae.RedirectionType = (RarRedirectionType)br.ReadUInt32();
							crae.RedirectionFlags = br.ReadUInt32();
							crae.RedirectionNameLength = br.ReadUInt32();
							crae.RedirectionName = br.ReadBytes((int)crae.RedirectionNameLength);
							break;

						case 0x06: // Unix owner and group information
							crae.UnixOwnerSize = extraSize;
							crae.UnixOwnerFlags = (RarUnixOwnerRecordFlags)br.ReadUInt32();
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.UserNameStringIsPresent) != 0)
							{
								crae.UnixOwnerUserNameLength = br.ReadUInt32();
								crae.UnixOwnerUserName = br.ReadBytes((int)crae.UnixOwnerUserNameLength);
							}
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.GroupNameStringIsPresent) != 0)
							{
								crae.UnixOwnerGroupNameLength = br.ReadUInt32();
								crae.UnixOwnerGroupName = br.ReadBytes((int)crae.UnixOwnerGroupNameLength);
							}
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.NumericUserIdIsPresent) != 0)
							{
								crae.UnixOwnerUserId = br.ReadUInt32();
							}
							if ((crae.UnixOwnerFlags & RarUnixOwnerRecordFlags.NumericGroupIdIsPresent) != 0)
							{
								crae.UnixOwnerGroupId = br.ReadUInt32();
							}
							break;

						case 0x07: // Service header data array

							break;
					}
				}
				if ((crae.HeaderFlags & RarHeaderFlags.DataAreaPresent) != 0)
				{
					crae.DataArea = br.ReadBytes((int)crae.DataAreaSize);
				}
			}
		}

		#endregion
	}
}
