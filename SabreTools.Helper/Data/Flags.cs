using System;

namespace SabreTools.Helper
{
	/// <summary>
	/// Determines which diffs should be created
	/// </summary>
	[Flags]
	public enum DiffMode
	{
		// Standard diffs
		Dupes = 0x01,
		NoDupes = 0x02,
		Individuals = 0x04,
		All = Dupes | NoDupes | Individuals,

		// Cascaded diffs
		Cascade = 0x08,
		ReverseCascade = 0x10,
	}

	/// <summary>
	/// Determines the DAT output format
	/// </summary>
	[Flags]
	public enum OutputFormat
	{
		Logiqx = 0x0001,
		ClrMamePro = 0x0002,
		RomCenter = 0x0004,
		DOSCenter = 0x0008,
		MissFile = 0x0010,
		SabreDat = 0x0020,
		RedumpMD5 = 0x0040,
		RedumpSHA1 = 0x0080,
		RedumpSFV = 0x0100,
		SoftwareList = 0x0200,
		OfflineList = 0x0400,
		TSV = 0x0800,
		CSV = 0x1000,

		ALL = 0xFFFF,
	}

	/// <summary>
	/// Determines the archive general bit flags
	/// </summary>
	[Flags]
	public enum GeneralPurposeBitFlag : ushort
	{
		Encrypted = 0x0001,
		ZeroedCRCAndSize = 0x0008,
		CompressedPatchedData = 0x0020,
		StrongEncryption = 0x0040,
		LanguageEncodingFlag = 0x0800,
		EncryptedCentralDirectory = 0x2000,

		// For Method 6 - Imploding
		Imploding8KSlidingDictionary = 0x0002,
		Imploding3ShannonFanoTrees = 0x0004,

		// For Methods 8 and 9 - Deflating
		DeflatingMaximumCompression = 0x0002,
		DeflatingFastCompression = 0x0004,
		DeflatingSuperFastCompression = 0x0006,
		EnhancedDeflating = 0x0010,

		// For Method 14 - LZMA
		LZMAEOSMarkerUsed = 0x0002,

		// Reserved and unused (SHOULD NOT BE USED)
		Bit7 = 0x0080,
		Bit8 = 0x0100,
		Bit9 = 0x0200,
		Bit10 = 0x0400,
		Bit12 = 0x1000, // Reserved by PKWARE for enhanced compression
		Bit14 = 0x4000, // Reserved by PKWARE
		Bit15 = 0x8000, // Reserved by PKWARE
	}

	/// <summary>
	/// Internal file attributes used by archives
	/// </summary>
	[Flags]
	public enum InternalFileAttributes : ushort
	{
		ASCIIOrTextFile = 0x0001,
		RecordLengthControl = 0x0002,

		// Reserved and unused (SHOULD NOT BE USED)
		Bit1 = 0x0002,
		Bit2 = 0x0004,
	}

	/// <summary>
	/// Zipfile special status
	/// </summary>
	/// <remarks>https://github.com/gjefferyes/RomVault/blob/5a93500001f0d068f32cf77a048950717507f733/ROMVault2/SupportedFiles/ZipEnums.cs</remarks>
	[Flags]
	public enum ZipStatus
	{
		None = 0x0,
		TorrentZip = 0x1,
		ExtraData = 0x2
	}
}
