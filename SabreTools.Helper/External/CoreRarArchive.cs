using System.Collections.Generic;

using SabreTools.Helper.Data;

/// <summary>
/// http://www.rarlab.com/technote.htm#srvheaders
/// </summary>
namespace SabreTools.Helper.Tools
{
	public class CoreRarArchive
	{
		// SFX Module Information
		public byte[] SFX;

		// Standard Header Information
		public uint HeaderCRC32;
		public uint HeaderSize; // vint
		public RarHeaderFlags HeaderFlags; // vint
		public uint ExtraAreaSize; // vint
		public RarArchiveFlags ArchiveFlags; // vint
		public uint VolumeNumber; // vint
		public byte[] ExtraArea;

		// Encryption Header Information
		public uint EncryptionHeaderCRC32;
		public uint EncryptionHeaderSize; // vint
		public RarHeaderFlags EncryptionHeaderFlags; // vint
		public uint EncryptionVersion; // vint
		public uint EncryptionFlags; // vint
		public byte KDFCount;
		public byte[] Salt = new byte[16];
		public byte[] CheckValue = new byte[12];

		// Locator Information
		public uint LocatorSize; // vint
		public uint LocatorFlags; // vint
		public uint QuickOpenOffset; // vint
		public uint RecoveryRecordOffset; // vint

		// Entry Information
		public List<CoreRarArchiveEntry> Entries = new List<CoreRarArchiveEntry>();
	}

	public class CoreRarArchiveEntry
	{
		// Standard Entry Information
		public uint HeaderCRC32;
		public uint HeaderSize; // vint
		public RarHeaderType HeaderType; // vint
		public RarHeaderFlags HeaderFlags; // vint
		public uint ExtraAreaSize; // vint
		public uint DataAreaSize; // vint
		public RarFileFlags FileFlags; // vint
		public uint UnpackedSize; // vint
		public uint Attributes; // vint
		public uint mtime;
		public uint DataCRC32;
		public uint CompressionInformation; // vint
		public uint HostOS; // vint
		public uint NameLength; // vint
		public byte[] Name;
		public byte[] DataArea;

		// File Encryption Information
		public uint EncryptionSize; // vint
		public RarEncryptionFlags EncryptionFlags; // vint
		public byte KDFCount;
		public byte[] Salt = new byte[16];
		public byte[] IV = new byte[16];
		public byte[] CheckValue = new byte[12];

		// File Hash Information
		public uint HashSize; // vint
		public uint HashType; // vint
		public byte[] HashData = new byte[32];

		// File Time Information
		public uint TimeSize; // vint
		public RarTimeFlags TimeFlags; // vint
		public uint TimeMtime;
		public ulong TimeMtime64;
		public uint TimeCtime;
		public ulong TimeCtime64;
		public uint TimeLtime;
		public ulong TimeLtime64;

		// File Version Information
		public uint VersionSize; // vint
		public const uint VersionFlags = 0; // vint
		public uint VersionNumber; // vint

		// File System Redirection Record
		public uint RedirectionSize; // vint
		public RarRedirectionType RedirectionType; // vint
		public uint RedirectionFlags; // vint
		public uint RedirectionNameLength; // vint
		public byte[] RedirectionName;

		// Unix Owner Record
		public uint UnixOwnerSize; // vint
		public RarUnixOwnerRecordFlags UnixOwnerFlags; // vint
		public uint UnixOwnerUserNameLength; // vint
		public byte[] UnixOwnerUserName;
		public uint UnixOwnerGroupNameLength; // vint
		public byte[] UnixOwnerGroupName;
		public uint UnixOwnerUserId; // vint
		public uint UnixOwnerGroupId; // vint

		// Service Data Information
		public uint ServiceSize; // vint
		public byte[] ServiceData;
	}
}
