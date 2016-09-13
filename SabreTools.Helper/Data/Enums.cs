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
}
