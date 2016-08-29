using System;
using System.Collections.Generic;

namespace SabreTools.Helper
{
	/// <summary>
	/// Intermediate struct for holding and processing hash data
	/// </summary>
	public struct HashData : IEquatable<HashData>
	{
		public long Size;
		public string CRC;
		public string MD5;
		public string SHA1;

		public bool Equals(HashData other)
		{
			return this.Equals(other, false);
		}

		public bool Equals(HashData other, bool IsDisk)
		{
			bool equals = false;

			if (!IsDisk &&
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
	/// Intermediate struct for holding and processing rom data
	/// </summary>
	public struct File : IComparable, IEquatable<File>
	{
		public Machine Machine;
		public string Name;
		public ItemType Type;
		public HashData HashData;
		public DupeType Dupe;
		public bool Nodump;
		public string Date;
		public SourceMetadata Metadata;

		public int CompareTo(object obj)
		{
			Logger temp = new Logger(false, "");
			temp.Start();

			int ret = 0;

			try
			{
				File comp = (File)obj;

				if (this.Machine.Name == comp.Machine.Name)
				{
					if (this.Name == comp.Name)
					{
						ret = (RomTools.IsDuplicate(this, comp, temp) ? 0 : 1);
					}
					ret = String.Compare(this.Name, comp.Name);
				}
				ret = String.Compare(this.Machine.Name, comp.Machine.Name);
			}
			catch
			{
				ret = 1;
			}

			temp.Close();
			return ret;
		}

		public bool Equals(File other)
		{
			Logger temp = new Logger(false, "");
			temp.Start();
			bool isdupe = RomTools.IsDuplicate(this, other, temp);
			temp.Close();

			return (this.Machine.Name == other.Machine.Name &&
				this.Name == other.Name &&
				isdupe);
		}
	}

	/// <summary>
	/// Intermediate metadata kept with a RomData object representing source information
	/// </summary>
	public struct SourceMetadata
	{
		public int SystemID;
		public string System;
		public int SourceID;
		public string Source;
	}

	/// <summary>
	/// Intermediate struct for holding and processing Rom/Machine data
	/// </summary>
	public struct Machine
	{
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
		public Dictionary<string, List<File>> Files;

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
				Files = new Dictionary<string, List<File>>(),
				UseGame = this.UseGame,
				Prefix = this.Prefix,
				Postfix = this.Postfix,
				Quotes = this.Quotes,
				RepExt = this.RepExt,
				AddExt = this.AddExt,
				GameName = this.GameName,
				Romba = this.Romba,
				XSV = this.XSV,
			};
		}
	}

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
}
