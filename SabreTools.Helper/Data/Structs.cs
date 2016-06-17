using System;
using System.Collections.Generic;

namespace SabreTools.Helper
{
	/// <summary>
	/// Intermediate struct for holding and processing rom data
	/// </summary>
	public struct RomData : IComparable, IEquatable<RomData>
	{
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
		public SourceMetadata Metadata;

		public int CompareTo(object obj)
		{
			try
			{
				RomData comp = (RomData)obj;

				if (this.Game == comp.Game)
				{
					if (this.Name == comp.Name)
					{
						return (RomTools.IsDuplicate(this, comp) ? 0 : 1);
					}
					return String.Compare(this.Name, comp.Name);
				}
				return String.Compare(this.Game, comp.Game);
			}
			catch
			{
				return 1;
			}
		}

		public bool Equals(RomData other)
		{
			return (this.Game == other.Game &&
				this.Name == other.Name &&
				RomTools.IsDuplicate(this, other));
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
	/// Intermediate struct for holding DAT information
	/// </summary>
	public struct DatData : ICloneable
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

		public object Clone()
		{
			return new DatData
			{
				FileName = this.FileName,
				Name = this.Name,
				Description = this.Description,
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
				Roms = this.Roms,
				UseGame = this.UseGame,
				Prefix = this.Prefix,
				Postfix = this.Postfix,
				Quotes = this.Quotes,
				RepExt = this.RepExt,
				AddExt = this.AddExt,
				GameName = this.GameName,
				Romba = this.Romba,
				TSV = this.TSV,
				RomCount = this.RomCount,
				DiskCount = this.DiskCount,
				TotalSize = this.TotalSize,
				CRCCount = this.CRCCount,
				MD5Count = this.MD5Count,
				SHA1Count = this.SHA1Count,
				NodumpCount = this.NodumpCount,
			};
		}
	}
}
