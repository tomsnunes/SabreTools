namespace SabreTools.Helper
{
	#region DATabase

	/// <summary>
	/// Possible DAT import classes
	/// </summary>
	public enum DatType
	{
		none = 0,
		Custom,
		MAME,
		NoIntro,
		Redump,
		TOSEC,
		TruRip,
		NonGood,
		MaybeIntro,
		Good,
	}

	#endregion

	#region DAT related

	/// <summary>
	/// Determines forcemerging tag for DAT output
	/// </summary>
	public enum ForceMerging
	{
		None = 0,
		Split,
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

	#endregion

	#region Rom related

	/// <summary>
	/// Determines which type of duplicate a file is
	/// </summary>
	public enum DupeType
	{
		None = 0,
		InternalHash = 1,
		InternalAll = 2,
		ExternalHash = 3,
		ExternalAll = 4,
	}

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
	}

	#endregion

	#region Skippers and Mappers

	/// <summary>
	/// Possible detected header type
	/// </summary>
	public enum HeaderType
	{
		None = 0,
		a7800,
		fds,
		lynx,
		//n64,
		nes,
		pce,
		psid,
		snes,
		spc,
	}

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

	#region Miscellaneous

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

	/// <summary>
	/// Determines the level to scan archives at
	/// </summary>
	public enum ArchiveScanLevel
	{
		Both = 0,
		Internal,
		External,
	}

	#endregion

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

	#endregion
}
