namespace SabreTools.Library.Data
{
	#region Archival

	/// <summary>
	/// Version of tool archive made by
	/// </summary>
	public enum ArchiveVersion : ushort
	{
		MSDOSandOS2 = 0,
		Amiga = 1,
		OpenVMS = 2,
		UNIX = 3,
		VMCMS = 4,
		AtariST = 5,
		OS2HPFS = 6,
		Macintosh = 7,
		ZSystem = 8,
		CPM = 9,
		WindowsNTFS = 10,
		MVS = 11,
		VSE = 12,
		AcornRisc = 13,
		VFAT = 14,
		AlternateMVS = 15,
		BeOS = 16,
		Tandem = 17,
		OS400 = 18,
		OSXDarwin = 19,
		TorrentZip = 20,
		TorrentZip64 = 45,
	}

	/// <summary>
	/// Availible CHD codec formats
	/// </summary>
	public enum CHDCodecType : uint
	{
		CHD_CODEC_NONE = 0,

		// general codecs
		CHD_CODEC_ZLIB = 0x7a6c6962, // zlib
		CHD_CODEC_LZMA = 0x6c7a6d61, // lzma
		CHD_CODEC_HUFFMAN = 0x68756666, // huff
		CHD_CODEC_FLAC = 0x666c6163, // flac

		// general codecs with CD frontend
		CHD_CODEC_CD_ZLIB = 0x63647a6c, // cdzl
		CHD_CODEC_CD_LZMA = 0x63646c7a, // cdlz
		CHD_CODEC_CD_FLAC = 0x6364666c, // cdfl

		// A/V codecs
		CHD_CODEC_AVHUFF = 0x61766875, // avhu

		// pseudo-codecs returned by hunk_info
		CHD_CODEC_SELF = 1,    // copy of another hunk
		CHD_CODEC_PARENT = 2,  // copy of a parent's hunk
		CHD_CODEC_MINI = 3,    // legacy "mini" 8-byte repeat
	}

	/// <summary>
	/// Compression method based on flag
	/// </summary>
	public enum CompressionMethod : ushort
	{
		Stored = 0,
		Shrunk = 1,
		ReducedCompressionFactor1 = 2,
		ReducedCompressionFactor2 = 3,
		ReducedCompressionFactor3 = 4,
		ReducedCompressionFactor4 = 5,
		Imploded = 6,
		Tokenizing = 7,
		Deflated = 8,
		Delfate64 = 9,
		PKWAREDataCompressionLibrary = 10,
		BZIP2 = 12,
		LZMA = 14,
		IBMTERSE = 18,
		IBMLZ77 = 19,
		WavPak = 97,
		PPMdVersionIRev1 = 98,

		// Reserved and unused (SHOULD NOT BE USED)
		Type11 = 11,
		Type13 = 13,
		Type15 = 15,
		Type16 = 16,
		Type17 = 17,
	}

	/// <summary>
	/// Type of file that is being looked at
	/// </summary>
	public enum FileType
	{
		// Singleton
		None = 0,
		CHD,

		// Can contain children
		Folder,
		SevenZipArchive,
		GZipArchive,
		LRZipArchive,
		LZ4Archive,
		RarArchive,
		TapeArchive,
		XZArchive,
		ZipArchive,
		ZPAQArchive,
		ZstdArchive,
	}

	/// <summary>
	/// Output format for rebuilt files
	/// </summary>
	public enum OutputFormat
	{
		// Currently implemented
		Folder = 0,
		TorrentZip = 1,
		TorrentGzip = 2,
		TapeArchive = 5,

		// Currently unimplemented fully
		Torrent7Zip = 3,
		TorrentRar = 4,
		TorrentXZ = 6,
		TorrentLRZip = 7,
		TorrentLZ4 = 8,
		TorrentZstd = 9,
		TorrentZPAQ = 10,
	}

	/// <summary>
	/// RAR extra area flag
	/// </summary>
	public enum RarExtraAreaFlag : uint
	{
		FileEncryption = 0x01,
		FileHash = 0x02,
		FileTime = 0x03,
		FileVersion = 0x04,
		Redirection = 0x05,
		UnixOwner = 0x06,
		ServiceData = 0x07,
	}

	/// <summary>
	/// RAR header types
	/// </summary>
	public enum RarHeaderType : uint
	{
		MainArchiveHeader = 1,
		File = 2,
		Service = 3,
		ArchiveEncryption = 4,
		EndOfArchive = 5,
	}

	/// <summary>
	/// RAR entry redirection type
	/// </summary>
	public enum RarRedirectionType : uint
	{
		UnixSymlink = 0x0001,
		WindowsSymlink = 0x0002,
		WindowsJunction = 0x0003,
		HardLink = 0x0004,
		FileCopy = 0x0005,
	}

	/// <summary>
	/// 7zip Properties
	/// </summary>
	public enum SevenZipProperties : uint
	{
		kEnd = 0x00,

		kHeader = 0x01,

		kArchiveProperties = 0x02,

		kAdditionalStreamsInfo = 0x03,
		kMainStreamsInfo = 0x04,
		kFilesInfo = 0x05,

		kPackInfo = 0x06,
		kUnPackInfo = 0x07,
		kSubStreamsInfo = 0x08,

		kSize = 0x09,
		kCRC = 0x0A,

		kFolder = 0x0B,

		kCodersUnPackSize = 0x0C,
		kNumUnPackStream = 0x0D,

		kEmptyStream = 0x0E,
		kEmptyFile = 0x0F,
		kAnti = 0x10,

		kName = 0x11,
		kCTime = 0x12,
		kATime = 0x13,
		kMTime = 0x14,
		kWinAttributes = 0x15,
		kComment = 0x16,

		kEncodedHeader = 0x17,

		kStartPos = 0x18,
		kDummy = 0x19,
	}

	/// <summary>
	/// Zip open type
	/// </summary>
	/// <remarks>https://raw.githubusercontent.com/gjefferyes/RomVault/5a93500001f0d068f32cf77a048950717507f733/ROMVault2/SupportedFiles/ZipEnums.cs</remarks>
	public enum ZipOpenType
	{
		Closed,
		OpenRead,
		OpenWrite
	}

	/// <summary>
	/// Zip testing type
	/// </summary>
	/// <remarks>https://raw.githubusercontent.com/gjefferyes/RomVault/5a93500001f0d068f32cf77a048950717507f733/ROMVault2/SupportedFiles/ZipEnums.cs</remarks>
	public enum ZipReturn
	{
		ZipGood,
		ZipFileLocked,
		ZipFileCountError,
		ZipSignatureError,
		ZipExtraDataOnEndOfZip,
		ZipUnsupportedCompression,
		ZipLocalFileHeaderError,
		ZipCentralDirError,
		ZipEndOfCentralDirectoryError,
		Zip64EndOfCentralDirError,
		Zip64EndOfCentralDirectoryLocatorError,
		ZipReadingFromOutputFile,
		ZipWritingToInputFile,
		ZipErrorGettingDataStream,
		ZipCRCDecodeError,
		ZipDecodeError,
		ZipFileNameToLong,
		ZipFileAlreadyOpen,
		ZipCannotFastOpen,
		ZipErrorOpeningFile,
		ZipErrorFileNotFound,
		ZipErrorReadingFile,
		ZipErrorTimeStamp,
		ZipErrorRollBackFile,
		ZipUntested
	}

	#endregion

	#region DatFile related

	/// <summary>
	/// Determines the DAT deduplication type
	/// </summary>
	public enum DedupeType
	{
		None = 0,
		Full,

		// Force only deduping with certain types
		Game,
		CRC,
		MD5,
		SHA1,
		SHA256,
		SHA384,
		SHA512,
	}

	/// <summary>
	/// Determines forcemerging tag for DAT output
	/// </summary>
	public enum ForceMerging
	{
		None = 0,
		Split,
		Merged,
		NonMerged,
		Full,
	}

	/// <summary>
	/// Determines forcenodump tag for DAT output
	/// </summary>
	public enum ForceNodump
	{
		None = 0,
		Obsolete,
		Required,
		Ignore,
	}

	/// <summary>
	/// Determines forcepacking tag for DAT output
	/// </summary>
	public enum ForcePacking
	{
		None = 0,
		Zip,
		Unzip,
	}

	/// <summary>
	/// Determines which files should be skipped in DFD
	/// </summary>
	public enum SkipFileType
	{
		None = 0,
		Archive,
		File,
	}

	/// <summary>
	/// Determines how the current dictionary is sorted by
	/// </summary>
	public enum SortedBy
	{
		Default = 0,
		Size,
		CRC,
		MD5,
		SHA1,
		SHA256,
		SHA384,
		SHA512,
		Game,
	}

	/// <summary>
	/// Determines how a DAT will be split internally
	/// </summary>
	public enum SplitType
	{
		None = 0,
		NonMerged,
		Merged,
		FullNonMerged,
		Split,
		DeviceNonMerged
	}

	#endregion

	#region DatItem related

	/// <summary>
	/// Determine what type of file an item is
	/// </summary>
	public enum ItemType
	{
		Rom = 0,
		Disk = 1,
		Sample = 2,
		Release = 3,
		BiosSet = 4,
		Archive = 5,

		Blank = 99, // This is not a real type, only used internally
	}

	#endregion

	#region Help related

	/// <summary>
	/// Determines the feature type to check for
	/// </summary>
	public enum FeatureType
	{
		Flag = 0,
		String,
		Int32,
		Int64,
		List,
	}

	#endregion

	#region Logging related

	/// <summary>
	/// Severity of the logging statement
	/// </summary>
	public enum LogLevel
	{
		VERBOSE = 0,
		USER,
		WARNING,
		ERROR,
	}

	#endregion

	#region Skippers and Mappers

	/// <summary>
	/// Determines the header skip operation
	/// </summary>
	public enum HeaderSkipOperation
	{
		None = 0,
		Bitswap,
		Byteswap,
		Wordswap,
		WordByteswap,
	}

	/// <summary>
	/// Determines the type of test to be done
	/// </summary>
	public enum HeaderSkipTest
	{
		Data = 0,
		Or,
		Xor,
		And,
		File,
	}

	/// <summary>
	/// Determines the operator to be used in a file test
	/// </summary>
	public enum HeaderSkipTestFileOperator
	{
		Equal = 0,
		Less,
		Greater,
	}

	#endregion
}
