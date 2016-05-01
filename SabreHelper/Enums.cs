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
		WARNING,
		ERROR,
	}
}
