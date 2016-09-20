using System;
using System.Collections.Generic;

namespace SabreTools.Helper
{
	#region Hash-to-Dat structs [Currently Unused]

	/* Thought experiment

		So, here's a connundrum: Should the internal structure of how DATs work (down to the hash level) mirror
		how people see it (Dat to multiple games, game to multiple roms, rom to single hash) or
		should it more closely mirror real life (Hash to multiple roms, rom to multiple games, game to single DAT)

		If I use the "how people see it":
			Things are pretty much how they are now with redundant data and the like
			It makes sense to write things out to file, though. And life is easier when output is easier.
			No code changes (big plus!)

		If I use the "how it is":
			Less data is likely to be mirrored
			Refs to DAT files are possible so that there aren't duplicates
			A lot of code will have to change...
	*/

	/// <summary>
	/// Intermediate struct for holding and processing Hash data (NEW SYSTEM)
	/// </summary>
	public struct HashData : IEquatable<HashData>
	{
		public long Size;
		public byte[] CRC;
		public byte[] MD5;
		public byte[] SHA1;
		public List<RomData> Roms;

		public bool Equals(HashData other)
		{
			return this.Equals(other, false);
		}

		public bool Equals(HashData other, bool IsDisk)
		{
			bool equals = false;

			if (!IsDisk &&
					((this.MD5 == null || other.MD5 == null) || this.MD5 == other.MD5) &&
					((this.SHA1 == null || other.SHA1 == null) || this.SHA1 == other.SHA1))
			{
				equals = true;
			}
			else if (!IsDisk &&
					(this.Size == other.Size) &&
					((this.CRC == null || other.CRC != null) || this.CRC == other.CRC) &&
					((this.MD5 == null || other.MD5 == null) || this.MD5 == other.MD5) &&
					((this.SHA1 == null || other.SHA1 == null) || this.SHA1 == other.SHA1))
			{
				equals = true;
			}

			return equals;
		}
	}

	/// <summary>
	/// Intermediate struct for holding and processing Rom data (NEW SYSTEM)
	/// </summary>
	public struct RomData
	{
		public string Name;
		public ItemType Type;
		public bool Nodump;
		public string Date;
		public DupeType DupeType;
		public MachineData Machine;
	}

	/// <summary>
	/// Intermediate struct for holding and processing Game/Machine data (NEW SYSTEM)
	/// </summary>
	public struct MachineData
	{
		// Data specific to Machine/Game
		public string Name;
		public string Comment;
		public string Description;
		public string Year;
		public string Manufacturer;
		public string RomOf;
		public string CloneOf;
		public string SampleOf;
		public string SourceFile;
		public bool IsBios;
		public string Board;
		public string RebuildTo;
		public bool TorrentZipped;

		// Data specific to the source of the Machine/Game
		public int SystemID;
		public string System;
		public int SourceID;
		public string Source;
	}

	/// <summary>
	/// Intermediate struct for holding and processing DAT data (NEW SYSTEM)
	/// </summary>
	public struct DatData
	{
		// Data common to most DAT types
		public string FileName;
		public string Name;
		public string Description;
		public string RootDir;
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
		public List<HashData> Hashes;

		// Data specific to the Miss DAT type
		public bool UseGame;
		public string Prefix;
		public string Postfix;
		public bool Quotes;
		public string RepExt;
		public string AddExt;
		public bool GameName;
		public bool Romba;
		public bool? XSV; // true for tab-deliminated output, false for comma-deliminated output

		// Statistical data related to the DAT
		public long RomCount;
		public long DiskCount;
		public long TotalSize;
		public long CRCCount;
		public long MD5Count;
		public long SHA1Count;
		public long NodumpCount;
	}

	#endregion

	#region Dat-to-Hash structs

	/// <summary>
	/// Intermediate struct for holding and processing hash data
	/// </summary>
	public struct Hash : IEquatable<Hash>
	{
		public long Size;
		public string CRC;
		public string MD5;
		public string SHA1;

		public bool Equals(Hash other)
		{
			return this.Equals(other, false);
		}

		public bool Equals(Hash other, bool IsDisk)
		{
			bool equals = false;

			if (IsDisk &&
					((String.IsNullOrEmpty(this.MD5) || String.IsNullOrEmpty(other.MD5)) || this.MD5 == other.MD5) &&
					((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(other.SHA1)) || this.SHA1 == other.SHA1))
			{
				equals = true;
			}
			else if (!IsDisk &&
					(this.Size == other.Size) &&
					((String.IsNullOrEmpty(this.CRC) || String.IsNullOrEmpty(other.CRC)) || this.CRC == other.CRC) &&
					((String.IsNullOrEmpty(this.MD5) || String.IsNullOrEmpty(other.MD5)) || this.MD5 == other.MD5) &&
					((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(other.SHA1)) || this.SHA1 == other.SHA1))
			{
				equals = true;
			}

			return equals;
		}
	}

	/// <summary>
	/// Intermediate struct for holding DAT information
	/// </summary>
	public struct Dat : ICloneable
	{
		// Data common to most DAT types
		public string FileName;
		public string Name;
		public string Description;
		public string RootDir;
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
		public Dictionary<string, List<DatItem>> Files;

		// Data specific to the Miss DAT type
		public bool UseGame;
		public string Prefix;
		public string Postfix;
		public bool Quotes;
		public string RepExt;
		public string AddExt;
		public bool RemExt;
		public bool GameName;
		public bool Romba;
		public bool? XSV; // true for tab-deliminated output, false for comma-deliminated output

		// Statistical data related to the DAT
		public long RomCount;
		public long DiskCount;
		public long TotalSize;
		public long CRCCount;
		public long MD5Count;
		public long SHA1Count;
		public long NodumpCount;

		public object Clone()
		{
			return new Dat
			{
				FileName = this.FileName,
				Name = this.Name,
				Description = this.Description,
				RootDir = this.RootDir,
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = this.Files,
				UseGame = this.UseGame,
				Prefix = this.Prefix,
				Postfix = this.Postfix,
				Quotes = this.Quotes,
				RepExt = this.RepExt,
				AddExt = this.AddExt,
				RemExt = this.RemExt,
				GameName = this.GameName,
				Romba = this.Romba,
				XSV = this.XSV,
				RomCount = this.RomCount,
				DiskCount = this.DiskCount,
				TotalSize = this.TotalSize,
				CRCCount = this.CRCCount,
				MD5Count = this.MD5Count,
				SHA1Count = this.SHA1Count,
				NodumpCount = this.NodumpCount,
			};
		}

		public object CloneHeader()
		{
			return new Dat
			{
				FileName = this.FileName,
				Name = this.Name,
				Description = this.Description,
				RootDir = this.RootDir,
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				OutputFormat = this.OutputFormat,
				MergeRoms = this.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
				UseGame = this.UseGame,
				Prefix = this.Prefix,
				Postfix = this.Postfix,
				Quotes = this.Quotes,
				RepExt = this.RepExt,
				AddExt = this.AddExt,
				RemExt = this.RemExt,
				GameName = this.GameName,
				Romba = this.Romba,
				XSV = this.XSV,
			};
		}
	}

	#endregion

	#region Skipper structs

	/// <summary>
	/// Intermediate struct for holding header skipper information
	/// </summary>
	public struct Skipper
	{
		public string Name;
		public string Author;
		public string Version;
		public List<SkipperRule> Rules;
	}

	/// <summary>
	/// Intermediate struct for holding header skipper rule information
	/// </summary>
	public struct SkipperRule
	{
		public long? StartOffset; // null is EOF
		public long? EndOffset; // null if EOF
		public HeaderSkipOperation Operation;
		public List<SkipperTest> Tests;
	}

	/// <summary>
	/// Intermediate struct for holding header test information
	/// </summary>
	public struct SkipperTest
	{
		public HeaderSkipTest Type;
		public long? Offset; // null is EOF
		public byte[] Value;
		public bool Result;
		public byte[] Mask;
		public long? Size; // null is PO2, "power of 2" filesize
		public HeaderSkipTestFileOperator Operator;
	}

	#endregion
}
