using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabreTools.Helper
{
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

	/// <summary>
	/// Possible detected header type
	/// </summary>
	public enum HeaderType
	{
		None = 0,
		A7800,
		FDS,
		Lynx,
		//N64,
		NES,
		PCE,
		PSID,
		SNES,
		SPC,
	}

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

	/// <summary>
	/// Determines the DAT output format
	/// </summary>
	public enum OutputFormat
	{
		None = 0,
		Xml,
		ClrMamePro,
		RomCenter,
		DOSCenter,
		MissFile,
		SabreDat,
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
}
