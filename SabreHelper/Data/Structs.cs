using System.Collections.Generic;

namespace SabreTools.Helper
{
	/// <summary>
	/// Intermediate struct for holding and processing rom data
	/// </summary>
	public struct RomData
	{
		public string Manufacturer;
		public string System;
		public int SystemID;
		public string Source;
		public string URL;
		public int SourceID;
		public string Game;
		public string Name;
		public string Type;
		public long Size;
		public string CRC;
		public string MD5;
		public string SHA1;
		public DupeType Dupe;
		public bool Nodump;
		public string Date;
	}

	/// <summary>
	/// Intermediate struct for holding DAT information
	/// </summary>
	public struct DatData
	{
		// Data common to most DAT types
		public string FileName;
		public string Name;
		public string Description;
		public string Category;
		public string Version;
		public string Date;
		public string Author;
		public string Email;
		public string Homepage;
		public string Url;
		public string Comment;
		public string Header;
		public string Type; // Generally only used for SuperDAT
		public ForceMerging ForceMerging;
		public ForceNodump ForceNodump;
		public ForcePacking ForcePacking;
		public OutputFormat OutputFormat;
		public bool MergeRoms;
		public Dictionary<string, List<RomData>> Roms;

		// Data specific to the Miss DAT type
		public bool UseGame;
		public string Prefix;
		public string Postfix;
		public bool Quotes;
		public string RepExt;
		public string AddExt;
		public bool GameName;
		public bool Romba;
		public bool TSV; // tab-deliminated output

		// Statistical data related to the DAT
		public long RomCount;
		public long DiskCount;
		public long TotalSize;
		public long CRCCount;
		public long MD5Count;
		public long SHA1Count;
		public long NodumpCount;
	}
}
