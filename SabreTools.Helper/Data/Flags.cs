using System;

namespace SabreTools.Helper.Data
{
	#region Archival

	/// <summary>
	/// Determines the level to scan archives at
	/// </summary>
	[Flags]
	public enum ArchiveScanLevel
	{
		// 7zip
		SevenZipExternal = 0x00001,
		SevenZipInternal = 0x00002,
		SevenZipBoth = SevenZipExternal | SevenZipInternal,

		// GZip
		GZipExternal = 0x00010,
		GZipInternal = 0x00020,
		GZipBoth = GZipExternal | GZipInternal,

		// RAR
		RarExternal = 0x00100,
		RarInternal = 0x00200,
		RarBoth = RarExternal | RarInternal,

		// Zip
		ZipExternal = 0x01000,
		ZipInternal = 0x02000,
		ZipBoth = ZipExternal | ZipInternal,

		// Tar
		TarExternal = 0x10000,
		TarInternal = 0x20000,
		TarBoth = TarExternal | TarInternal,
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
	/// RAR archive flags
	/// </summary>
	[Flags]
	public enum RarArchiveFlags : uint
	{
		Volume = 0x0001, // Volume. Archive is a part of multivolume set.
		VolumeNumberField = 0x0002, // Volume number field is present. This flag is present in all volumes except first.
		Solid = 0x0004, // Solid archive.
		RecoveryRecordPresent = 0x0008, // Recovery record is present.
		Locked = 0x0010, // Locked archive.
	}

	/// <summary>
	/// RAR entry encryption flags
	/// </summary>
	[Flags]
	public enum RarEncryptionFlags : uint
	{
		PasswordCheckDataPresent = 0x0001,
		UseTweakedChecksums = 0x0002,

		/*
		If flag 0x0002 is present, RAR transforms the checksum preserving file or service data integrity, so it becomes dependent on 
		encryption key. It makes guessing file contents based on checksum impossible. It affects both data CRC32 in file header and 
		checksums in file hash record in extra area.
		*/
	}

	/// <summary>
	/// RAR file flags
	/// </summary>
	[Flags]
	public enum RarFileFlags : uint
	{
		Directory = 0x0001, // Directory file system object (file header only)
		TimeInUnix = 0x0002, // Time field in Unix format is present
		CRCPresent = 0x0004, // CRC32 field is present
		UnpackedSizeUnknown = 0x0008, // Unpacked size is unknown

		/*
		If flag 0x0008 is set, unpacked size field is still present, but must be ignored and extraction
		must be performed until reaching the end of compression stream. This flag can be set if actual
		file size is larger than reported by OS or if file size is unknown such as for all volumes except
		last when archiving from stdin to multivolume archive
		*/
	}

	/// <summary>
	/// RAR header flags
	/// </summary>
	[Flags]
	public enum RarHeaderFlags : uint
	{
		ExtraAreaPresent = 0x0001, // Extra area is present in the end of header
		DataAreaPresent = 0x0002, // Data area is present in the end of header
		BlocksWithUnknownType = 0x0004, //  Blocks with unknown type and this flag must be skipped when updating an archive
		DataAreaContinuingFromPrevious = 0x0008, // Data area is continuing from previous volume
		DataAreaContinuingToNext = 0x0010, // Data area is continuing in next volume
		BlockDependsOnPreceding = 0x0020, // Block depends on preceding file block
		PreserveChildBlock = 0x0040, // Preserve a child block if host block is modified
	}

	[Flags]
	public enum RarUnixOwnerRecordFlags : uint
	{
		UserNameStringIsPresent = 0x0001,
		GroupNameStringIsPresent = 0x0002,
		NumericUserIdIsPresent = 0x0004,
		NumericGroupIdIsPresent = 0x0008,
	}

	/// <summary>
	/// RAR entry time flags
	/// </summary>
	[Flags]
	public enum RarTimeFlags : uint
	{
		TimeInUnixFormat = 0x0001,
		ModificationTimePresent = 0x0002,
		CreationTimePresent = 0x0004,
		LastAccessTimePresent = 0x0008,
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

    #endregion

    #region DatFile related

    /// <summary>
    /// Determines the DAT output format
    /// </summary>
    [Flags]
    public enum DatFormat
    {
        Logiqx = 0x00001,
        ClrMamePro = 0x00002,
        RomCenter = 0x00004,
        DOSCenter = 0x00008,
        MissFile = 0x00010,
        SabreDat = 0x00020,
        RedumpMD5 = 0x00040,
        RedumpSHA1 = 0x00080,
        RedumpSFV = 0x00100,
        SoftwareList = 0x00200,
        OfflineList = 0x00400,
        TSV = 0x00800,
        CSV = 0x01000,
        AttractMode = 0x02000,
        RedumpSHA256 = 0x04000,
		RedumpSHA384 = 0x08000,
		RedumpSHA512 = 0x10000,

        ALL = 0xFFFFF,
    }

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
	/// Determine which format to output Stats to
	/// </summary>
	/// [Flags]
	public enum StatDatFormat
	{
		None = 0x01,
		HTML = 0x02,
		CSV = 0x04,
		TSV = 0x08,
	}

    /// <summary>
    /// Determine what hashes to strip from the DAT
    /// </summary>
    [Flags]
    public enum Hash
    {
		CRC = 0x0001,
        MD5 = 0x0002,
        SHA1 = 0x0004,
        SHA256 = 0x0008,
		SHA384 = 0x0010,
		SHA512 = 0x0020,

		// Special combinations
		Standard = CRC | MD5 | SHA1,
		DeepHashes = MD5 | SHA1 | SHA256 | SHA384 | SHA512,
    }

    #endregion

    #region DatItem related

    /// <summary>
    /// Determines which type of duplicate a file is
    /// </summary>
    [Flags]
	public enum DupeType
	{
		// Type of match
		Hash = 0x01,
		All = 0x02,

		// Location of match
		Internal = 0x10,
		External = 0x20,
	}

	/// <summary>
	/// Determine the status of the item
	/// </summary>
	[Flags]
	public enum ItemStatus
	{
		NULL = 0x00, // This is a fake flag that is used for filter only
		None = 0x01,
		Good = 0x02,
		BadDump = 0x04,
		Nodump = 0x08,
		Verified = 0x10,
	}

	/// <summary>
	/// Determine what type of machine it is
	/// </summary>
	[Flags]
	public enum MachineType
	{
		NULL = 0x00, // This is a fake flag used for filter only
		None = 0x01,
		Bios = 0x02,
		Device = 0x04,
		Mechanical = 0x08,
	}

	#endregion
}
