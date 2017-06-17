using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public struct DatHeader
	{
		#region Publicly facing variables

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
		public DatFormat DatFormat;
		public bool ExcludeOf;
		public bool MergeRoms;
		public Hash StripHash;
		public bool OneGameOneRegion;
		public List<string> Regions;

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

		#endregion
	}
}
