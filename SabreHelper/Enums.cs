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
		InternalHash,
		InternalAll,
		ExternalHash,
		ExternalAll,
	}

	public enum ForceMerging
	{
		None = 0,
		Split,
		Full,
	}

	public enum ForceNodump
	{
		None = 0,
		Obsolete,
		Required,
		Ignore,
	}

	public enum ForcePacking
	{
		None = 0,
		Zip,
		Unzip,
	}

	public enum OutputFormat
	{
		Xml = 0,
		ClrMamePro,
		RomCenter,
		DOSCenter,
		MissFile,
	}
}
