using System;
using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents all possible DAT header information
	/// </summary>
	public class DatHeader : ICloneable
	{
		#region Private instance variables

		// Data common to most DAT types
		private string _fileName;
		private string _name;
		private string _description;
		private string _rootDir;
		private string _category;
		private string _version;
		private string _date;
		private string _author;
		private string _email;
		private string _homepage;
		private string _url;
		private string _comment;
		private string _header;
		private string _type; // Generally only used for SuperDAT
		private ForceMerging _forceMerging;
		private ForceNodump _forceNodump;
		private ForcePacking _forcePacking;
		private DatFormat _datFormat;
		private bool _excludeOf;
		private bool _sceneDateStrip;
		private DedupeType _dedupeRoms;
		private Hash _stripHash;
		private bool _oneGameOneRegion;
		private List<string> _regions;

		// Data specific to the Miss DAT type
		private bool _useGame;
		private string _prefix;
		private string _postfix;
		private bool _quotes;
		private string _repExt;
		private string _addExt;
		private bool _remExt;
		private bool _gameName;
		private bool _romba;

		#endregion

		#region Publicly facing variables

		// Data common to most DAT types
		public string FileName
		{
			get { return _fileName; }
			set { _fileName = value; }
		}
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}
		public string RootDir
		{
			get { return _rootDir; }
			set { _rootDir = value; }
		}
		public string Category
		{
			get { return _category; }
			set { _category = value; }
		}
		public string Version
		{
			get { return _version; }
			set { _version = value; }
		}
		public string Date
		{
			get { return _date; }
			set { _date = value; }
		}
		public string Author
		{
			get { return _author; }
			set { _author = value; }
		}
		public string Email
		{
			get { return _email; }
			set { _email = value; }
		}
		public string Homepage
		{
			get { return _homepage; }
			set { _homepage = value; }
		}
		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}
		public string Comment
		{
			get { return _comment; }
			set { _comment = value; }
		}
		public string Header
		{
			get { return _header; }
			set { _header = value; }
		}
		public string Type // Generally only used for SuperDAT
		{
			get { return _type; }
			set { _type = value; }
		}
		public ForceMerging ForceMerging
		{
			get { return _forceMerging; }
			set { _forceMerging = value; }
		}
		public ForceNodump ForceNodump
		{
			get { return _forceNodump; }
			set { _forceNodump = value; }
		}
		public ForcePacking ForcePacking
		{
			get { return _forcePacking; }
			set { _forcePacking = value; }
		}
		public DatFormat DatFormat
		{
			get { return _datFormat; }
			set { _datFormat = value; }
		}
		public bool ExcludeOf
		{
			get { return _excludeOf; }
			set { _excludeOf = value; }
		}
		public bool SceneDateStrip
		{
			get { return _sceneDateStrip; }
			set { _sceneDateStrip = value; }
		}
		public DedupeType DedupeRoms
		{
			get { return _dedupeRoms; }
			set { _dedupeRoms = value; }
		}
		public Hash StripHash
		{
			get { return _stripHash; }
			set { _stripHash = value; }
		}
		public bool OneGameOneRegion
		{
			get { return _oneGameOneRegion; }
			set { _oneGameOneRegion = value; }
		}
		public List<string> Regions
		{
			get
			{
				if (_regions == null)
				{
					_regions = new List<string>();
				}

				return _regions;
			}
			set { _regions = value; }
		}

		// Data specific to the Miss DAT type
		public bool UseGame
		{
			get { return _useGame; }
			set { _useGame = value; }
		}
		public string Prefix
		{
			get { return _prefix; }
			set { _prefix = value; }
		}
		public string Postfix
		{
			get { return _postfix; }
			set { _postfix = value; }
		}
		public bool Quotes
		{
			get { return _quotes; }
			set { _quotes = value; }
		}
		public string RepExt
		{
			get { return _repExt; }
			set { _repExt = value; }
		}
		public string AddExt
		{
			get { return _addExt; }
			set { _addExt = value; }
		}
		public bool RemExt
		{
			get { return _remExt; }
			set { _remExt = value; }
		}
		public bool GameName
		{
			get { return _gameName; }
			set { _gameName = value; }
		}
		public bool Romba
		{
			get { return _romba; }
			set { _romba = value; }
		}

		#endregion

		#region Instance Methods

		#region Cloning Methods

		/// <summary>
		/// Clone the current header
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new DatHeader()
			{
				_fileName = this._fileName,
				_name = this._name,
				_description = this._description,
				_rootDir = this._rootDir,
				_category = this._category,
				_version = this._version,
				_date = this._date,
				_author = this._author,
				_email = this._email,
				_homepage = this._homepage,
				_url = this._url,
				_comment = this._comment,
				_header = this._header,
				_type = this._type,
				_forceMerging = this._forceMerging,
				_forceNodump = this._forceNodump,
				_forcePacking = this._forcePacking,
				_datFormat = this._datFormat,
				_excludeOf = this._excludeOf,
				_sceneDateStrip = this._sceneDateStrip,
				_dedupeRoms = this._dedupeRoms,
				_stripHash = this._stripHash,
				_oneGameOneRegion = this._oneGameOneRegion,
				_regions = this._regions,

				_useGame = this._useGame,
				_prefix = this._prefix,
				_postfix = this._postfix,
				_quotes = this._quotes,
				_repExt = this._repExt,
				_addExt = this._addExt,
				_remExt = this._remExt,
				_gameName = this._gameName,
				_romba = this._romba,
			};
		}

		#endregion

		#endregion // Instance Methods
	}
}
