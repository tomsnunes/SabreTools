using System;

namespace SabreTools.Helper
{
	/// <summary>
	/// Determines which diffs should be created
	/// </summary>
	[Flags]
	public enum DiffMode
	{
		Dupes = 0x01,
		NoDupes = 0x02,
		Individuals = 0x04,
		All = Dupes | NoDupes | Individuals,
	}

	/// <summary>
	/// Determines the DAT output format
	/// </summary>
	[Flags]
	public enum OutputFormat
	{
		Xml = 0x001,
		ClrMamePro = 0x002,
		RomCenter = 0x004,
		DOSCenter = 0x008,
		MissFile = 0x010,
		SabreDat = 0x020,
		RedumpMD5 = 0x040,
		RedumpSHA1 = 0x080,
		RedumpSFV = 0x100,
	}
}
