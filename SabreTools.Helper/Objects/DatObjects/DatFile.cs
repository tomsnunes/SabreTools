using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace SabreTools.Helper
{
	public class DatFile : ICloneable
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
		private OutputFormat _outputFormat;
		private bool _mergeRoms;
		private Dictionary<string, List<DatItem>> _files;

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
		private bool? _xsv; // true for tab-deliminated output, false for comma-deliminated output

		// Statistical data related to the DAT
		private long _romCount;
		private long _diskCount;
		private long _totalSize;
		private long _crcCount;
		private long _md5Count;
		private long _sha1Count;
		private long _nodumpCount;

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
		public OutputFormat OutputFormat
		{
			get { return _outputFormat; }
			set { _outputFormat = value; }
		}
		public bool MergeRoms
		{
			get { return _mergeRoms; }
			set { _mergeRoms = value; }
		}
		public Dictionary<string, List<DatItem>> Files
		{
			get
			{
				if (_files == null)
				{
					_files = new Dictionary<string, List<DatItem>>();
				}
				return _files;
			}
			set
			{
				_files = value;
			}
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
		public bool? XSV // true for tab-deliminated output, false for comma-deliminated output
		{
			get { return _xsv; }
			set { _xsv = value; }
		}

		// Statistical data related to the DAT
		public long RomCount
		{
			get { return _romCount; }
			set { _romCount = value; }
		}
		public long DiskCount
		{
			get { return _diskCount; }
			set { _diskCount = value; }
		}
		public long TotalSize
		{
			get { return _totalSize; }
			set { _totalSize = value; }
		}
		public long CRCCount
		{
			get { return _crcCount; }
			set { _crcCount = value; }
		}
		public long MD5Count
		{
			get { return _md5Count; }
			set { _md5Count = value; }
		}
		public long SHA1Count
		{
			get { return _sha1Count; }
			set { _sha1Count = value; }
		}
		public long NodumpCount
		{
			get { return _nodumpCount; }
			set { _nodumpCount = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Dat object
		/// </summary>
		public DatFile()
		{
			// Nothing needs to be done
		}

		/// <summary>
		/// Create a new Dat object with the included information (standard Dats)
		/// </summary>
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="rootdir">New rootdir</param>
		/// <param name="category">New category</param>
		/// <param name="version">New version</param>
		/// <param name="date">New date</param>
		/// <param name="author">New author</param>
		/// <param name="email">New email</param>
		/// <param name="homepage">New homepage</param>
		/// <param name="url">New URL</param>
		/// <param name="comment">New comment</param>
		/// <param name="header">New header</param>
		/// <param name="superdat">True to set SuperDAT type, false otherwise</param>
		/// <param name="forcemerge">None, Split, Full</param>
		/// <param name="forcend">None, Obsolete, Required, Ignore</param>
		/// <param name="forcepack">None, Zip, Unzip</param>
		/// <param name="outputFormat">Non-zero flag for output format, zero otherwise for default</param>
		/// <param name="mergeRoms">True to dedupe the roms in the DAT, false otherwise (default)</param>
		/// <param name="files">Dictionary of lists of DatItem objects</param>
		public DatFile(string fileName, string name, string description, string rootDir, string category, string version, string date,
			string author, string email, string homepage, string url, string comment, string header, string type, ForceMerging forceMerging,
			ForceNodump forceNodump, ForcePacking forcePacking, OutputFormat outputFormat, bool mergeRoms, Dictionary<string, List<DatItem>> files)
		{
			_fileName = fileName;
			_name = name;
			_description = description;
			_rootDir = rootDir;
			_category = category;
			_version = version;
			_date = date;
			_author = author;
			_email = email;
			_homepage = homepage;
			_url = url;
			_comment = comment;
			_header = header;
			_type = type;
			_forceMerging = forceMerging;
			_forceNodump = forceNodump;
			_forcePacking = forcePacking;
			_outputFormat = outputFormat;
			_mergeRoms = mergeRoms;
			_files = files;

			_romCount = 0;
			_diskCount = 0;
			_totalSize = 0;
			_crcCount = 0;
			_md5Count = 0;
			_sha1Count = 0;
			_nodumpCount = 0;
		}

		/// <summary>
		/// Create a new Dat object with the included information (missfile)
		/// </summary>
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="outputFormat">Non-zero flag for output format, zero otherwise for default</param>
		/// <param name="mergeRoms">True to dedupe the roms in the DAT, false otherwise (default)</param>
		/// <param name="files">Dictionary of lists of DatItem objects</param>
		/// <param name="useGame">True if games are to be used in output, false if roms are</param>
		/// <param name="prefix">Generic prefix to be added to each line</param>
		/// <param name="postfix">Generic postfix to be added to each line</param>
		/// <param name="quotes">Add quotes to each item</param>
		/// <param name="repExt">Replace all extensions with another</param>
		/// <param name="addExt">Add an extension to all items</param>
		/// <param name="remExt">Remove all extensions</param>
		/// <param name="gameName">Add the dat name as a directory prefix</param>
		/// <param name="romba">Output files in romba format</param>
		/// <param name="xsv">True to output files in TSV format, false to output files in CSV format, null otherwise</param>
		public DatFile(string fileName, string name, string description, OutputFormat outputFormat, bool mergeRoms,
			Dictionary<string, List<DatItem>> files, bool useGame, string prefix, string postfix, bool quotes,
			string repExt, string addExt, bool remExt, bool gameName, bool romba, bool? xsv)
		{
			_fileName = fileName;
			_name = name;
			_description = description;
			_outputFormat = outputFormat;
			_mergeRoms = mergeRoms;
			_files = files;

			_useGame = useGame;
			_prefix = prefix;
			_postfix = postfix;
			_quotes = quotes;
			_repExt = repExt;
			_addExt = addExt;
			_remExt = remExt;
			_gameName = gameName;
			_romba = romba;
			_xsv = xsv;

			_romCount = 0;
			_diskCount = 0;
			_totalSize = 0;
			_crcCount = 0;
			_md5Count = 0;
			_sha1Count = 0;
			_nodumpCount = 0;
	}

		#endregion

		#region Cloning Methods

		public object Clone()
		{
			return new DatFile
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
			return new DatFile
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

		#endregion

		#region DAT Parsing

		/// <summary>
		/// Get what type of DAT the input file is
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The OutputFormat corresponding to the DAT</returns>
		/// <remarks>There is currently no differentiation between XML and SabreDAT here</remarks>
		public static OutputFormat GetOutputFormat(string filename, Logger logger)
		{
			// Limit the output formats based on extension
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext != ".dat" && ext != ".xml")
			{
				return 0;
			}

			// Read the input file, if possible
			logger.Log("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return 0;
			}

			try
			{
				StreamReader sr = File.OpenText(filename);
				string first = sr.ReadLine();
				sr.Close();
				sr.Dispose();
				if (first.Contains("<") && first.Contains(">"))
				{
					return OutputFormat.Xml;
				}
				else if (first.Contains("[") && first.Contains("]"))
				{
					return OutputFormat.RomCenter;
				}
				else
				{
					return OutputFormat.ClrMamePro;
				}
			}
			catch (Exception)
			{
				return 0;
			}
		}

		/// <summary>
		/// Get the XmlTextReader associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The XmlTextReader representing the (possibly converted) file, null otherwise</returns>
		public static XmlTextReader GetXmlTextReader(string filename, Logger logger)
		{
			logger.Log("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return null;
			}

			XmlTextReader xtr;
			xtr = new XmlTextReader(filename);
			xtr.WhitespaceHandling = WhitespaceHandling.None;
			xtr.DtdProcessing = DtdProcessing.Ignore;
			return xtr;
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		public static void Parse(string filename, int sysid, int srcid, ref DatFile datdata, Logger logger, bool keep = false, bool clean = false, bool softlist = false, bool keepext = false)
		{
			Parse(filename, sysid, srcid, ref datdata, null, null, null, -1, -1, -1, null, null, null, null, false, false, "", logger, keep, clean, softlist, keepext);
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		public static void Parse(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			ref DatFile datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep = false,
			bool clean = false,
			bool softlist = false,
			bool keepext = false)
		{
			// Check the file extension first as a safeguard
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext != ".txt" && ext != ".dat" && ext != ".xml")
			{
				return;
			}

			// If the output filename isn't set already, get the internal filename
			datdata.FileName = (String.IsNullOrEmpty(datdata.FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : datdata.FileName);

			// If the output type isn't set already, get the internal output type
			datdata.OutputFormat = (datdata.OutputFormat == 0 ? GetOutputFormat(filename, logger) : datdata.OutputFormat);

			// Make sure there's a dictionary to read to
			if (datdata.Files == null)
			{
				datdata.Files = new Dictionary<string, List<DatItem>>();
			}

			// Now parse the correct type of DAT
			switch (GetOutputFormat(filename, logger))
			{
				case OutputFormat.ClrMamePro:
					ParseCMP(filename, sysid, srcid, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, keep, clean);
					break;
				case OutputFormat.RomCenter:
					ParseRC(filename, sysid, srcid, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, clean);
					break;
				case OutputFormat.SabreDat:
				case OutputFormat.Xml:
					ParseXML(filename, sysid, srcid, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, keep, clean, softlist);
					break;
				default:
					return;
			}
		}

		/// <summary>
		/// Parse a ClrMamePro DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private static void ParseCMP(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			ref DatFile datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep,
			bool clean)
		{
			// Open a file reader
			StreamReader sr = new StreamReader(File.OpenRead(filename));

			bool block = false, superdat = false;
			string blockname = "", tempgamename = "", gamedesc = "", cloneof = "",
				romof = "", sampleof = "", year = "", manufacturer = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// Comments in CMP DATs start with a #
				if (line.Trim().StartsWith("#"))
				{
					continue;
				}

				// If the line is the header or a game
				if (Regex.IsMatch(line, Constants.HeaderPatternCMP))
				{
					GroupCollection gc = Regex.Match(line, Constants.HeaderPatternCMP).Groups;

					if (gc[1].Value == "clrmamepro" || gc[1].Value == "romvault" || gc[1].Value.ToLowerInvariant() == "doscenter")
					{
						blockname = "header";
					}

					block = true;
				}

				// If the line is a rom-like item and we're in a block
				else if ((line.Trim().StartsWith("rom (")
						|| line.Trim().StartsWith("disk (")
						|| line.Trim().StartsWith("file (")
						|| (line.Trim().StartsWith("sample") && !line.Trim().StartsWith("sampleof"))
					) && block)
				{
					ItemType temptype = ItemType.Rom;
					if (line.Trim().StartsWith("rom ("))
					{
						temptype = ItemType.Rom;
					}
					else if (line.Trim().StartsWith("disk ("))
					{
						temptype = ItemType.Disk;
					}
					else if (line.Trim().StartsWith("file ("))
					{
						temptype = ItemType.Rom;
					}
					else if (line.Trim().StartsWith("sample"))
					{
						temptype = ItemType.Sample;
					}

					// Create the proper DatItem based on the type
					DatItem item;
					switch (temptype)
					{
						case ItemType.Archive:
							item = new Archive();
							break;
						case ItemType.BiosSet:
							item = new BiosSet();
							break;
						case ItemType.Disk:
							item = new Disk();
							break;
						case ItemType.Release:
							item = new Release();
							break;
						case ItemType.Sample:
							item = new Sample();
							break;
						case ItemType.Rom:
						default:
							item = new Rom();
							break;
					}

					// Then populate it with information
					item.MachineName = tempgamename;
					item.MachineDescription = gamedesc;
					item.CloneOf = cloneof;
					item.RomOf = romof;
					item.SampleOf = sampleof;
					item.Manufacturer = manufacturer;
					item.Year = year;
					item.SystemID = sysid;
					item.SourceID = srcid;

					// If we have a sample, treat it special
					if (temptype == ItemType.Sample)
					{
						line = line.Trim().Remove(0, 6).Trim().Replace("\"", ""); // Remove "sample" from the input string
						item.Name = line;
					}

					// Otherwise, process the rest of the line
					else
					{
						string[] gc = line.Trim().Split(' ');

						// Loop over all attributes and add them if possible
						bool quote = false;
						string attrib = "", val = "";
						for (int i = 2; i < gc.Length; i++)
						{
							//If the item is empty, we automatically skip it because it's a fluke
							if (gc[i].Trim() == String.Empty)
							{
								continue;
							}
							// Special case for nodump...
							else if (gc[i] == "nodump" && attrib != "status" && attrib != "flags")
							{
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).Nodump = true;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).Nodump = true;
								}
							}
							// Even number of quotes, not in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib == "")
							{
								attrib = gc[i].Replace("\"", "");
							}
							// Even number of quotes, not in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib != "")
							{
								switch (attrib.ToLowerInvariant())
								{
									case "name":
										item.Name = gc[i].Replace("\"", "");
										break;
									case "size":
										if (item.Type == ItemType.Rom)
										{
											long size = -1;
											if (Int64.TryParse(gc[i].Replace("\"", ""), out size))
											{
												((Rom)item).Size = size;
											}
										}
										
										break;
									case "crc":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).CRC = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "md5":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "sha1":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "flags":
										if (gc[i].Replace("\"", "").ToLowerInvariant() == "nodump")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).Nodump = true;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).Nodump = true;
											}
										}
										break;
									case "date":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).Date = gc[i].Replace("\"", "") + " " + gc[i + 1].Replace("\"", "");
										}
										i++;
										break;
								}

								attrib = "";
							}
							// Even number of quotes, in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && quote && attrib == "")
							{
								// Attributes can't have quoted names
							}
							// Even number of quotes, in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && quote && attrib != "")
							{
								val += " " + gc[i];
							}
							// Odd number of quotes, not in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && !quote && attrib == "")
							{
								// Attributes can't have quoted names
							}
							// Odd number of quotes, not in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && !quote && attrib != "")
							{
								val = gc[i].Replace("\"", "");
								quote = true;
							}
							// Odd number of quotes, in a quote, not in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && quote && attrib == "")
							{
								quote = false;
							}
							// Odd number of quotes, in a quote, in attribute
							else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && quote && attrib != "")
							{
								val += " " + gc[i].Replace("\"", "");
								switch (attrib.ToLowerInvariant())
								{
									case "name":
										item.Name = val;
										break;
									case "size":
										if (item.Type == ItemType.Rom)
										{
											long size = -1;
											if (Int64.TryParse(gc[i].Replace("\"", ""), out size))
											{
												((Rom)item).Size = size;
											}
										}
										break;
									case "crc":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).CRC = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "md5":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "sha1":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										else if (item.Type == ItemType.Disk)
										{
											((Disk)item).SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
										}
										break;
									case "flags":
										if (val.ToLowerInvariant() == "nodump")
										{
											if (item.Type == ItemType.Rom)
											{
												((Rom)item).Nodump = true;
											}
											else if (item.Type == ItemType.Disk)
											{
												((Disk)item).Nodump = true;
											}
										}
										break;
									case "date":
										if (item.Type == ItemType.Rom)
										{
											((Rom)item).Date = gc[i].Replace("\"", "") + " " + gc[i + 1].Replace("\"", "");
										}
										i++;
										break;
								}

								quote = false;
								attrib = "";
								val = "";
							}
						}
					}

					// Now process and add the rom
					string key = "";
					ParseAddHelper(item, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);
				}
				// If the line is anything but a rom or disk and we're in a block
				else if (Regex.IsMatch(line, Constants.ItemPatternCMP) && block)
				{
					GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;

					if (blockname != "header")
					{
						string itemval = gc[2].Value.Replace("\"", "");
						switch (gc[1].Value)
						{
							case "name":
								tempgamename = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
								break;
							case "description":
								gamedesc = itemval;
								break;
							case "romof":
								romof = itemval;
								break;
							case "cloneof":
								cloneof = itemval;
								break;
							case "year":
								year = itemval;
								break;
							case "manufacturer":
								manufacturer = itemval;
								break;
							case "sampleof":
								sampleof = itemval;
								break;
						}
					}
					else
					{
						string itemval = gc[2].Value.Replace("\"", "");
						switch (gc[1].Value)
						{
							case "name":
							case "Name:":
								datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? itemval : datdata.Name);
								superdat = superdat || itemval.Contains(" - SuperDAT");
								if (keep && superdat)
								{
									datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? "SuperDAT" : datdata.Type);
								}
								break;
							case "description":
							case "Description:":
								datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? itemval : datdata.Description);
								break;
							case "rootdir":
								datdata.RootDir = (String.IsNullOrEmpty(datdata.RootDir) ? itemval : datdata.RootDir);
								break;
							case "category":
								datdata.Category = (String.IsNullOrEmpty(datdata.Category) ? itemval : datdata.Category);
								break;
							case "version":
							case "Version:":
								datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? itemval : datdata.Version);
								break;
							case "date":
							case "Date:":
								datdata.Date = (String.IsNullOrEmpty(datdata.Date) ? itemval : datdata.Date);
								break;
							case "author":
							case "Author:":
								datdata.Author = (String.IsNullOrEmpty(datdata.Author) ? itemval : datdata.Author);
								break;
							case "email":
								datdata.Email = (String.IsNullOrEmpty(datdata.Email) ? itemval : datdata.Email);
								break;
							case "homepage":
							case "Homepage:":
								datdata.Homepage = (String.IsNullOrEmpty(datdata.Homepage) ? itemval : datdata.Homepage);
								break;
							case "url":
								datdata.Url = (String.IsNullOrEmpty(datdata.Url) ? itemval : datdata.Url);
								break;
							case "comment":
							case "Comment:":
								datdata.Comment = (String.IsNullOrEmpty(datdata.Comment) ? itemval : datdata.Comment);
								break;
							case "header":
								datdata.Header = (String.IsNullOrEmpty(datdata.Header) ? itemval : datdata.Header);
								break;
							case "type":
								datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? itemval : datdata.Type);
								superdat = superdat || itemval.Contains("SuperDAT");
								break;
							case "forcemerging":
								switch (itemval)
								{
									case "none":
										datdata.ForceMerging = ForceMerging.None;
										break;
									case "split":
										datdata.ForceMerging = ForceMerging.Split;
										break;
									case "full":
										datdata.ForceMerging = ForceMerging.Full;
										break;
								}
								break;
							case "forcezipping":
								switch (itemval)
								{
									case "yes":
										datdata.ForcePacking = ForcePacking.Zip;
										break;
									case "no":
										datdata.ForcePacking = ForcePacking.Unzip;
										break;
								}
								break;
						}
					}
				}

				// If we find an end bracket that's not associated with anything else, the block is done
				else if (Regex.IsMatch(line, Constants.EndPatternCMP) && block)
				{
					block = false;
					blockname = "";
					tempgamename = "";
				}
			}

			sr.Close();
			sr.Dispose();
		}

		/// <summary>
		/// Parse a RomCenter DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private static void ParseRC(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			ref DatFile datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			StreamReader sr = new StreamReader(File.OpenRead(filename));

			string blocktype = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// If the line is the start of the credits section
				if (line.ToLowerInvariant().Contains("[credits]"))
				{
					blocktype = "credits";
				}
				// If the line is the start of the dat section
				else if (line.ToLowerInvariant().Contains("[dat]"))
				{
					blocktype = "dat";
				}
				// If the line is the start of the emulator section
				else if (line.ToLowerInvariant().Contains("[emulator]"))
				{
					blocktype = "emulator";
				}
				// If the line is the start of the game section
				else if (line.ToLowerInvariant().Contains("[games]"))
				{
					blocktype = "games";
				}
				// Otherwise, it's not a section and it's data, so get out all data
				else
				{
					// If we have an author
					if (line.StartsWith("author="))
					{
						datdata.Author = (String.IsNullOrEmpty(datdata.Author) ? line.Split('=')[1] : datdata.Author);
					}
					// If we have one of the three version tags
					else if (line.StartsWith("version="))
					{
						switch (blocktype)
						{
							case "credits":
								datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? line.Split('=')[1] : datdata.Version);
								break;
							case "emulator":
								datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? line.Split('=')[1] : datdata.Description);
								break;
						}
					}
					// If we have a comment
					else if (line.StartsWith("comment="))
					{
						datdata.Comment = (String.IsNullOrEmpty(datdata.Comment) ? line.Split('=')[1] : datdata.Comment);
					}
					// If we have the split flag
					else if (line.StartsWith("split="))
					{
						int split = 0;
						if (Int32.TryParse(line.Split('=')[1], out split))
						{
							if (split == 1)
							{
								datdata.ForceMerging = ForceMerging.Split;
							}
						}
					}
					// If we have the merge tag
					else if (line.StartsWith("merge="))
					{
						int merge = 0;
						if (Int32.TryParse(line.Split('=')[1], out merge))
						{
							if (merge == 1)
							{
								datdata.ForceMerging = ForceMerging.Full;
							}
						}
					}
					// If we have the refname tag
					else if (line.StartsWith("refname="))
					{
						datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? line.Split('=')[1] : datdata.Name);
					}
					// If we have a rom
					else if (line.StartsWith("¬"))
					{
						/*
						The rominfo order is as follows:
						1 - parent name
						2 - parent description
						3 - game name
						4 - game description
						5 - rom name
						6 - rom crc
						7 - rom size
						8 - romof name
						9 - merge name
						*/
						string[] rominfo = line.Split('¬');

						Rom rom = new Rom(rominfo[5], Int64.Parse(rominfo[7]), rominfo[6], null, null, false, null, rominfo[3], null,
							rominfo[4], null, null, rominfo[8], rominfo[1], null, null, false, null, null, sysid, null, srcid, null);

						// Now process and add the rom
						string key = "";
						ParseAddHelper(rom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);
					}
				}
			}

			sr.Close();
			sr.Dispose();
		}

		/// <summary>
		/// Parse an XML DAT (Logiqx, SabreDAT, or SL) and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		private static void ParseXML(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			ref DatFile datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep,
			bool clean,
			bool softlist)
		{
			// Prepare all internal variables
			XmlReader subreader, headreader, flagreader;
			bool superdat = false, isnodump = false, empty = true;
			string key = "", date = "";
			long size = -1;
			List<string> parent = new List<string>();

			XmlTextReader xtr = GetXmlTextReader(filename, logger);
			if (xtr != null)
			{
				xtr.MoveToContent();
				while (!xtr.EOF)
				{
					// If we're ending a folder or game, take care of possibly empty games and removing from the parent
					if (xtr.NodeType == XmlNodeType.EndElement && (xtr.Name == "directory" || xtr.Name == "dir"))
					{
						// If we didn't find any items in the folder, make sure to add the blank rom
						if (empty)
						{
							string tempgame = String.Join("\\", parent);
							Rom rom = new Rom("null", tempgame);

							// Now process and add the rom
							ParseAddHelper(rom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);
						}

						// Regardless, end the current folder
						int parentcount = parent.Count;
						if (parentcount == 0)
						{
							logger.Log("Empty parent: " + String.Join("\\", parent));
							empty = true;
						}

						// If we have an end folder element, remove one item from the parent, if possible
						if (parentcount > 0)
						{
							parent.RemoveAt(parent.Count - 1);
							if (keep && parentcount > 1)
							{
								datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? "SuperDAT" : datdata.Type);
								superdat = true;
							}
						}
					}

					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						// Handle MAME listxml since they're halfway between a SL and a Logiqx XML
						case "mame":
							if (xtr.GetAttribute("build") != null)
							{
								datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? xtr.GetAttribute("build") : datdata.Name);
								datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? datdata.Name : datdata.Name);
							}
							xtr.Read();
							break;
						// New software lists have this behavior
						case "softwarelist":
							if (xtr.GetAttribute("name") != null)
							{
								datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? xtr.GetAttribute("name") : datdata.Name);
							}
							if (xtr.GetAttribute("description") != null)
							{
								datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? xtr.GetAttribute("description") : datdata.Description);
							}
							xtr.Read();
							break;
						// Handle M1 DATs since they're 99% the same as a SL DAT
						case "m1":
							datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? "M1" : datdata.Name);
							datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? "M1" : datdata.Description);
							if (xtr.GetAttribute("version") != null)
							{
								datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? xtr.GetAttribute("version") : datdata.Version);
							}
							xtr.Read();
							break;
						// We want to process the entire subtree of the header
						case "header":
							headreader = xtr.ReadSubtree();

							if (headreader != null)
							{
								while (!headreader.EOF)
								{
									// We only want elements
									if (headreader.NodeType != XmlNodeType.Element || headreader.Name == "header")
									{
										headreader.Read();
										continue;
									}

									// Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
									string content = "";
									switch (headreader.Name)
									{
										case "name":
											content = headreader.ReadElementContentAsString(); ;
											datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? content : datdata.Name);
											superdat = superdat || content.Contains(" - SuperDAT");
											if (keep && superdat)
											{
												datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? "SuperDAT" : datdata.Type);
											}
											break;
										case "description":
											content = headreader.ReadElementContentAsString();
											datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? content : datdata.Description);
											break;
										case "rootdir":
											content = headreader.ReadElementContentAsString();
											datdata.RootDir = (String.IsNullOrEmpty(datdata.RootDir) ? content : datdata.RootDir);
											break;
										case "category":
											content = headreader.ReadElementContentAsString();
											datdata.Category = (String.IsNullOrEmpty(datdata.Category) ? content : datdata.Category);
											break;
										case "version":
											content = headreader.ReadElementContentAsString();
											datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? content : datdata.Version);
											break;
										case "date":
											content = headreader.ReadElementContentAsString();
											datdata.Date = (String.IsNullOrEmpty(datdata.Date) ? content.Replace(".", "/") : datdata.Date);
											break;
										case "author":
											content = headreader.ReadElementContentAsString();
											datdata.Author = (String.IsNullOrEmpty(datdata.Author) ? content : datdata.Author);

											// Special cases for SabreDAT
											datdata.Email = (String.IsNullOrEmpty(datdata.Email) && !String.IsNullOrEmpty(headreader.GetAttribute("email")) ?
												headreader.GetAttribute("email") : datdata.Email);
											datdata.Homepage = (String.IsNullOrEmpty(datdata.Homepage) && !String.IsNullOrEmpty(headreader.GetAttribute("homepage")) ?
												headreader.GetAttribute("homepage") : datdata.Email);
											datdata.Url = (String.IsNullOrEmpty(datdata.Url) && !String.IsNullOrEmpty(headreader.GetAttribute("url")) ?
												headreader.GetAttribute("url") : datdata.Email);
											break;
										case "email":
											content = headreader.ReadElementContentAsString();
											datdata.Email = (String.IsNullOrEmpty(datdata.Email) ? content : datdata.Email);
											break;
										case "homepage":
											content = headreader.ReadElementContentAsString();
											datdata.Homepage = (String.IsNullOrEmpty(datdata.Homepage) ? content : datdata.Homepage);
											break;
										case "url":
											content = headreader.ReadElementContentAsString();
											datdata.Url = (String.IsNullOrEmpty(datdata.Url) ? content : datdata.Url);
											break;
										case "comment":
											content = headreader.ReadElementContentAsString();
											datdata.Comment = (String.IsNullOrEmpty(datdata.Comment) ? content : datdata.Comment);
											break;
										case "type":
											content = headreader.ReadElementContentAsString();
											datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? content : datdata.Type);
											superdat = superdat || content.Contains("SuperDAT");
											break;
										case "clrmamepro":
										case "romcenter":
											if (headreader.GetAttribute("header") != null)
											{
												datdata.Header = (String.IsNullOrEmpty(datdata.Header) ? headreader.GetAttribute("header") : datdata.Header);
											}
											if (headreader.GetAttribute("plugin") != null)
											{
												datdata.Header = (String.IsNullOrEmpty(datdata.Header) ? headreader.GetAttribute("plugin") : datdata.Header);
											}
											if (headreader.GetAttribute("forcemerging") != null)
											{
												switch (headreader.GetAttribute("forcemerging"))
												{
													case "split":
														datdata.ForceMerging = ForceMerging.Split;
														break;
													case "none":
														datdata.ForceMerging = ForceMerging.None;
														break;
													case "full":
														datdata.ForceMerging = ForceMerging.Full;
														break;
												}
											}
											if (headreader.GetAttribute("forcenodump") != null)
											{
												switch (headreader.GetAttribute("forcenodump"))
												{
													case "obsolete":
														datdata.ForceNodump = ForceNodump.Obsolete;
														break;
													case "required":
														datdata.ForceNodump = ForceNodump.Required;
														break;
													case "ignore":
														datdata.ForceNodump = ForceNodump.Ignore;
														break;
												}
											}
											if (headreader.GetAttribute("forcepacking") != null)
											{
												switch (headreader.GetAttribute("forcepacking"))
												{
													case "zip":
														datdata.ForcePacking = ForcePacking.Zip;
														break;
													case "unzip":
														datdata.ForcePacking = ForcePacking.Unzip;
														break;
												}
											}
											headreader.Read();
											break;
										case "flags":
											flagreader = xtr.ReadSubtree();
											if (flagreader != null)
											{
												while (!flagreader.EOF)
												{
													// We only want elements
													if (flagreader.NodeType != XmlNodeType.Element || flagreader.Name == "flags")
													{
														flagreader.Read();
														continue;
													}

													switch (flagreader.Name)
													{
														case "flag":
															if (flagreader.GetAttribute("name") != null && flagreader.GetAttribute("value") != null)
															{
																content = flagreader.GetAttribute("value");
																switch (flagreader.GetAttribute("name"))
																{
																	case "type":
																		datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? content : datdata.Type);
																		superdat = superdat || content.Contains("SuperDAT");
																		break;
																	case "forcemerging":
																		switch (content)
																		{
																			case "split":
																				datdata.ForceMerging = ForceMerging.Split;
																				break;
																			case "none":
																				datdata.ForceMerging = ForceMerging.None;
																				break;
																			case "full":
																				datdata.ForceMerging = ForceMerging.Full;
																				break;
																		}
																		break;
																	case "forcenodump":
																		switch (content)
																		{
																			case "obsolete":
																				datdata.ForceNodump = ForceNodump.Obsolete;
																				break;
																			case "required":
																				datdata.ForceNodump = ForceNodump.Required;
																				break;
																			case "ignore":
																				datdata.ForceNodump = ForceNodump.Ignore;
																				break;
																		}
																		break;
																	case "forcepacking":
																		switch (content)
																		{
																			case "zip":
																				datdata.ForcePacking = ForcePacking.Zip;
																				break;
																			case "unzip":
																				datdata.ForcePacking = ForcePacking.Unzip;
																				break;
																		}
																		break;
																}
															}
															flagreader.Read();
															break;
														default:
															flagreader.Read();
															break;
													}
												}
											}
											headreader.Skip();
											break;
										default:
											headreader.Read();
											break;
									}
								}
							}

							// Skip the header node now that we've processed it
							xtr.Skip();
							break;
						case "machine":
						case "game":
						case "software":
							string temptype = xtr.Name;
							string tempname = "", gamedesc = "", romof = "",
								cloneof = "", sampleof = "", year = "", manufacturer = "";

							// We want to process the entire subtree of the game
							subreader = xtr.ReadSubtree();

							// Safeguard for interesting case of "software" without anything except roms
							bool software = false;

							// If we have a subtree, add what is possible
							if (subreader != null)
							{
								subreader.MoveToContent();
								if (!softlist && temptype == "software" && subreader.ReadToFollowing("description"))
								{
									tempname = subreader.ReadElementContentAsString();
									gamedesc = tempname;
									tempname = tempname.Replace('/', '_').Replace("\"", "''");
									software = true;
								}
								else
								{
									// There are rare cases where a malformed XML will not have the required attributes. We can only skip them.
									if (xtr.AttributeCount == 0)
									{
										logger.Error("No attributes were found");
										xtr.Skip();
										continue;
									}
									tempname = xtr.GetAttribute("name");
									romof = (xtr.GetAttribute("romof") != null ? xtr.GetAttribute("romof") : "");
									cloneof = (xtr.GetAttribute("cloneof") != null ? xtr.GetAttribute("cloneof") : "");
									sampleof = (xtr.GetAttribute("sampleof") != null ? xtr.GetAttribute("sampleof") : "");
								}

								if (superdat && !keep)
								{
									string tempout = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
									if (tempout != "")
									{
										tempname = tempout;
									}
								}
								// Get the name of the game from the parent
								else if (superdat && keep && parent.Count > 0)
								{
									tempname = String.Join("\\", parent) + "\\" + tempname;
								}

								while (software || !subreader.EOF)
								{
									software = false;

									// We only want elements
									if (subreader.NodeType != XmlNodeType.Element)
									{
										subreader.Read();
										continue;
									}

									// Get the roms from the machine
									switch (subreader.Name)
									{
										case "description":
											gamedesc = subreader.ReadElementContentAsString();
											break;
										case "year":
											year = subreader.ReadElementContentAsString();
											break;
										case "manufacturer":
											manufacturer = subreader.ReadElementContentAsString();
											break;
										case "release":
											bool? defaultrel = null;
											if (subreader.GetAttribute("default") != null)
											{
												if (subreader.GetAttribute("default") == "yes")
												{
													defaultrel = true;
												}
												else if (subreader.GetAttribute("default") == "no")
												{
													defaultrel = false;
												}
											}

											DatItem relrom = new Release(subreader.GetAttribute("name"), subreader.GetAttribute("region"), subreader.GetAttribute("language"), date, defaultrel);

											// Now process and add the rom
											ParseAddHelper(relrom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

											subreader.Read();
											break;
										case "biosset":
											bool? defaultbios = null;
											if (subreader.GetAttribute("default") != null)
											{
												if (subreader.GetAttribute("default") == "yes")
												{
													defaultbios = true;
												}
												else if (subreader.GetAttribute("default") == "no")
												{
													defaultbios = false;
												}
											}

											DatItem biosrom = new BiosSet(subreader.GetAttribute("name"), subreader.GetAttribute("description"), defaultbios,
												tempname, null, gamedesc, null, null, romof, cloneof, sampleof, null, false, null, null, sysid, filename, srcid, null);

											// Now process and add the rom
											ParseAddHelper(biosrom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

											subreader.Read();
											break;
										case "archive":
											DatItem archiverom = new Archive(subreader.GetAttribute("name"), tempname, null, gamedesc, null, null,
												romof, cloneof, sampleof, null, false, null, null, sysid, filename, srcid, null);

											// Now process and add the rom
											ParseAddHelper(archiverom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

											subreader.Read();
											break;
										case "sample":
											DatItem samplerom = new Sample(subreader.GetAttribute("name"), tempname, null, gamedesc, null, null,
												romof, cloneof, sampleof, null, false, null, null, sysid, filename, srcid, null);

											// Now process and add the rom
											ParseAddHelper(samplerom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

											subreader.Read();
											break;
										case "rom":
										case "disk":
											empty = false;

											// If the rom is nodump, flag it
											isnodump = false;
											if (subreader.GetAttribute("flags") == "nodump" || subreader.GetAttribute("status") == "nodump")
											{
												logger.Log("Nodump detected: " +
													(subreader.GetAttribute("name") != null && subreader.GetAttribute("name") != "" ? "\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
												isnodump = true;
											}

											// If the rom has a Date attached, read it in and then sanitize it
											date = "";
											if (subreader.GetAttribute("date") != null)
											{
												DateTime dateTime = DateTime.Now;
												if (DateTime.TryParse(subreader.GetAttribute("date"), out dateTime))
												{
													date = dateTime.ToString();
												}
												else
												{
													date = subreader.GetAttribute("date");
												}
											}

											// Take care of hex-sized files
											size = -1;
											if (subreader.GetAttribute("size") != null && subreader.GetAttribute("size").Contains("0x"))
											{
												size = Convert.ToInt64(subreader.GetAttribute("size"), 16);
											}
											else if (subreader.GetAttribute("size") != null)
											{
												Int64.TryParse(subreader.GetAttribute("size"), out size);
											}

											// If the rom is continue or ignore, add the size to the previous rom
											if (subreader.GetAttribute("loadflag") == "continue" || subreader.GetAttribute("loadflag") == "ignore")
											{
												int index = datdata.Files[key].Count() - 1;
												DatItem lastrom = datdata.Files[key][index];
												if (lastrom.Type == ItemType.Rom)
												{
													((Rom)lastrom).Size += size;
												}
												datdata.Files[key].RemoveAt(index);
												datdata.Files[key].Add(lastrom);
												subreader.Read();
												continue;
											}

											// If we're in clean mode, sanitize the game name
											if (clean)
											{
												tempname = Style.CleanGameName(tempname.Split(Path.DirectorySeparatorChar));
											}

											DatItem inrom;
											switch (subreader.Name.ToLowerInvariant())
											{
												case "disk":
													inrom = new Disk(subreader.GetAttribute("name"), subreader.GetAttribute("md5"), subreader.GetAttribute("sha1"),
														isnodump, tempname, null, gamedesc, null, null, romof, cloneof, sampleof, null, false, null, null, sysid,
														filename, srcid, null);
													break;
												case "rom":
												default:
													inrom = new Rom(subreader.GetAttribute("name"), size, subreader.GetAttribute("crc"), subreader.GetAttribute("md5"),
														subreader.GetAttribute("sha1"), isnodump, date, tempname, null, gamedesc, null, null, romof, cloneof, sampleof,
														null, false, null, null, sysid, filename, srcid, null);
													break;
											}

											// Now process and add the rom
											ParseAddHelper(inrom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

											subreader.Read();
											break;
										default:
											subreader.Read();
											break;
									}
								}
							}

							// If we didn't find any items in the folder, make sure to add the blank rom
							if (empty)
							{
								tempname = (parent.Count > 0 ? String.Join("\\", parent) + Path.DirectorySeparatorChar : "") + tempname;

								Rom inrom = new Rom("null", tempname);

								// Now process and add the rom
								ParseAddHelper(inrom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

								// Regardless, end the current folder
								if (parent.Count == 0)
								{
									empty = true;
								}
							}
							xtr.Skip();
							break;
						case "dir":
						case "directory":
							// Set SuperDAT flag for all SabreDAT inputs, regardless of depth
							superdat = true;
							if (keep)
							{
								datdata.Type = (datdata.Type == "" ? "SuperDAT" : datdata.Type);
							}

							string foldername = (xtr.GetAttribute("name") == null ? "" : xtr.GetAttribute("name"));
							if (foldername != "")
							{
								parent.Add(foldername);
							}

							xtr.Read();
							break;
						case "file":
							empty = false;

							// If the rom is nodump, flag it
							isnodump = false;
							flagreader = xtr.ReadSubtree();
							if (flagreader != null)
							{
								while (!flagreader.EOF)
								{
									// We only want elements
									if (flagreader.NodeType != XmlNodeType.Element || flagreader.Name == "flags")
									{
										flagreader.Read();
										continue;
									}

									switch (flagreader.Name)
									{
										case "flag":
										case "status":
											if (flagreader.GetAttribute("name") != null && flagreader.GetAttribute("value") != null)
											{
												string content = flagreader.GetAttribute("value");
												switch (flagreader.GetAttribute("name"))
												{
													case "nodump":
														logger.Log("Nodump detected: " + (xtr.GetAttribute("name") != null && xtr.GetAttribute("name") != "" ?
															"\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
														isnodump = true;
														break;
												}
											}
											break;
									}

									flagreader.Read();
								}
							}

							// If the rom has a Date attached, read it in and then sanitize it
							date = "";
							if (xtr.GetAttribute("date") != null)
							{
								date = DateTime.Parse(xtr.GetAttribute("date")).ToString();
							}

							// Take care of hex-sized files
							size = -1;
							if (xtr.GetAttribute("size") != null && xtr.GetAttribute("size").Contains("0x"))
							{
								size = Convert.ToInt64(xtr.GetAttribute("size"), 16);
							}
							else if (xtr.GetAttribute("size") != null)
							{
								Int64.TryParse(xtr.GetAttribute("size"), out size);
							}

							// If the rom is continue or ignore, add the size to the previous rom
							if (xtr.GetAttribute("loadflag") == "continue" || xtr.GetAttribute("loadflag") == "ignore")
							{
								int index = datdata.Files[key].Count() - 1;
								DatItem lastrom = datdata.Files[key][index];
								if (lastrom.Type == ItemType.Rom)
								{
									((Rom)lastrom).Size += size;
								}
								datdata.Files[key].RemoveAt(index);
								datdata.Files[key].Add(lastrom);
								continue;
							}

							// Get the name of the game from the parent
							tempname = String.Join("\\", parent);

							// If we aren't keeping names, trim out the path
							if (!keep || !superdat)
							{
								string tempout = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
								if (tempout != "")
								{
									tempname = tempout;
								}
							}

							DatItem rom;
							switch (xtr.GetAttribute("type").ToLowerInvariant())
							{
								case "disk":
									rom = new Disk(xtr.GetAttribute("name"), xtr.GetAttribute("md5")?.ToLowerInvariant(),
										xtr.GetAttribute("sha1")?.ToLowerInvariant(), isnodump, tempname, null, tempname, null, null,
										null, null, null, null, false, null, null, sysid, filename, srcid, null);
									break;
								case "rom":
								default:
									rom = new Rom(xtr.GetAttribute("name"), size, xtr.GetAttribute("crc")?.ToLowerInvariant(),
										xtr.GetAttribute("md5")?.ToLowerInvariant(), xtr.GetAttribute("sha1")?.ToLowerInvariant(), isnodump,
										date, tempname, null, tempname, null, null, null, null, null, null, false, null, null, sysid, filename,
										srcid, null);
									break;
							}

							// Now process and add the rom
							ParseAddHelper(rom, ref datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

							xtr.Read();
							break;
						default:
							xtr.Read();
							break;
					}
				}

				xtr.Close();
				xtr.Dispose();
			}
		}

		/// <summary>
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="item">Item data to check against</param>
		/// <param name="datdata">Dat to add information to, if possible</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		private static void ParseAddHelper(DatItem item, ref DatFile datdata, string gamename, string romname, string romtype, long sgt, long slt,
			long seq, string crc, string md5, string sha1, bool? nodump, bool trim, bool single, string root, bool clean, Logger logger, out string key)
		{
			key = "";

			// If there's no name in the rom, we log and skip it
			if (String.IsNullOrEmpty(item.Name))
			{
				logger.Warning("Rom with no name found! Skipping...");
				return;
			}

			// If we're in cleaning mode, sanitize the game name
			item.MachineName = (clean ? Style.CleanGameName(item.MachineName) : item.MachineName);

			// If we have a Rom or a Disk, clean the hash data
			if (item.Type == ItemType.Rom)
			{
				Rom itemRom = (Rom)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemRom.CRC = Style.CleanHashData(itemRom.CRC, Constants.CRCLength);
				itemRom.MD5 = Style.CleanHashData(itemRom.MD5, Constants.MD5Length);
				itemRom.SHA1 = Style.CleanHashData(itemRom.SHA1, Constants.SHA1Length);

				// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
				if ((itemRom.Size == 0 || itemRom.Size == -1)
					&& ((itemRom.CRC == Constants.CRCZero || itemRom.CRC == "")
					|| itemRom.MD5 == Constants.MD5Zero
					|| itemRom.SHA1 == Constants.SHA1Zero))
				{
					itemRom.Size = Constants.SizeZero;
					itemRom.CRC = Constants.CRCZero;
					itemRom.MD5 = Constants.MD5Zero;
					itemRom.SHA1 = Constants.SHA1Zero;
				}
				// If the file has no size and it's not the above case, skip and log
				else if (itemRom.Type == ItemType.Rom && (itemRom.Size == 0 || itemRom.Size == -1))
				{
					logger.Warning("Incomplete entry for \"" + itemRom.Name + "\" will be output as nodump");
					itemRom.Nodump = true;
				}

				item = itemRom;
			}
			else if (item.Type == ItemType.Disk)
			{
				Disk itemDisk = (Disk)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemDisk.MD5 = Style.CleanHashData(itemDisk.MD5, Constants.MD5Length);
				itemDisk.SHA1 = Style.CleanHashData(itemDisk.SHA1, Constants.SHA1Length);

				item = itemDisk;
			}

			// If the rom passes the filter, include it
			if (DatItem.Filter(item, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, logger))
			{
				// If we are in single game mode, rename all games
				if (single)
				{
					item.MachineName = "!";
				}

				// If we are in NTFS trim mode, trim the game name
				if (trim)
				{
					// Windows max name length is 260
					int usableLength = 260 - item.MachineName.Length - root.Length;
					if (item.Name.Length > usableLength)
					{
						string ext = Path.GetExtension(item.Name);
						item.Name = item.Name.Substring(0, usableLength - ext.Length);
						item.Name += ext;
					}
				}

				lock (datdata.Files)
				{
					// Get the key and add statistical data
					switch (item.Type)
					{
						case ItemType.Archive:
						case ItemType.BiosSet:
						case ItemType.Release:
						case ItemType.Sample:
							key = item.Type.ToString();
							break;
						case ItemType.Disk:
							key = ((Disk)item).MD5;

							// Add statistical data
							datdata.DiskCount += 1;
							datdata.TotalSize += 0;
							datdata.MD5Count += (String.IsNullOrEmpty(((Disk)item).MD5) ? 0 : 1);
							datdata.SHA1Count += (String.IsNullOrEmpty(((Disk)item).SHA1) ? 0 : 1);
							datdata.NodumpCount += (((Disk)item).Nodump ? 1 : 0);
							break;
						case ItemType.Rom:
							key = ((Rom)item).Size + "-" + ((Rom)item).CRC;

							// Add statistical data
							datdata.RomCount += 1;
							datdata.TotalSize += (((Rom)item).Nodump ? 0 : ((Rom)item).Size);
							datdata.CRCCount += (String.IsNullOrEmpty(((Rom)item).CRC) ? 0 : 1);
							datdata.MD5Count += (String.IsNullOrEmpty(((Rom)item).MD5) ? 0 : 1);
							datdata.SHA1Count += (String.IsNullOrEmpty(((Rom)item).SHA1) ? 0 : 1);
							datdata.NodumpCount += (((Rom)item).Nodump ? 1 : 0);
							break;
						default:
							key = "default";
							break;
					}

					// Add the item to the DAT
					if (datdata.Files.ContainsKey(key))
					{
						datdata.Files[key].Add(item);
					}
					else
					{
						List<DatItem> newvalue = new List<DatItem>();
						newvalue.Add(item);
						datdata.Files.Add(key, newvalue);
					}
				}
			}
		}

		#endregion

		#region Bucketing

		/// <summary>
		/// Take an arbitrarily ordered List and return a Dictionary sorted by Game
		/// </summary>
		/// <param name="list">Input unsorted list</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by game name</returns>
		public static SortedDictionary<string, List<DatItem>> BucketByGame(List<DatItem> list, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			Dictionary<string, List<DatItem>> dict = new Dictionary<string, List<DatItem>>();
			dict.Add("key", list);
			return BucketByGame(dict, mergeroms, norename, logger, output);
		}

		/// <summary>
		/// Take an arbitrarily bucketed Dictionary and return one sorted by Game
		/// </summary>
		/// <param name="dict">Input unsorted dictionary</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by game name</returns>
		public static SortedDictionary<string, List<DatItem>> BucketByGame(IDictionary<string, List<DatItem>> dict, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			logger.User("Organizing " + (mergeroms ? "and merging " : "") + "roms for output");

			SortedDictionary<string, List<DatItem>> sortable = new SortedDictionary<string, List<DatItem>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (dict == null || dict.Count == 0)
			{
				return sortable;
			}

			// Process each all of the roms
			foreach (string key in dict.Keys)
			{
				List<DatItem> roms = dict[key];
				if (mergeroms)
				{
					roms = DatItem.Merge(roms, logger);
				}

				foreach (DatItem rom in roms)
				{
					count++;
					string newkey = (norename ? ""
							: rom.SystemID.ToString().PadLeft(10, '0')
								+ "-"
								+ rom.SourceID.ToString().PadLeft(10, '0') + "-")
						+ (String.IsNullOrEmpty(rom.MachineName)
								? "Default"
								: rom.MachineName.ToLowerInvariant());
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(rom);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(rom);
						sortable.Add(newkey, temp);
					}
				}
			}

			// Now go through and sort all of the lists
			List<string> keys = sortable.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> sortedlist = sortable[key];
				DatItem.Sort(ref sortedlist, norename);
				sortable[key] = sortedlist;
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			return sortable;
		}

		#endregion

		#region Converting and Updating

		/// <summary>
		/// Convert, update, and filter a DAT file
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="datdata">User specified inputs contained in a DatData object</param>
		/// <param name="outputFormat">Non-zero flag for output format, zero otherwise for default</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="cascade">True if the diffed files should be cascade diffed, false if diffed files should be reverse cascaded, null otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="softlist">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void Update(List<string> inputFileNames, DatFile datdata, OutputFormat outputFormat, string outDir, bool merge,
			DiffMode diff, bool? cascade, bool inplace, bool skip, bool bare, bool clean, bool softlist, string gamename, string romname, string romtype,
			long sgt, long slt, long seq, string crc, string md5, string sha1, bool? nodump, bool trim, bool single, string root, int maxDegreeOfParallelism,
			Logger logger)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || diff != 0)
			{
				// Make sure there are no folders in inputs
				List<string> newInputFileNames = new List<string>();
				foreach (string input in inputFileNames)
				{
					if (Directory.Exists(input))
					{
						foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
						{
							try
							{
								newInputFileNames.Add(Path.GetFullPath(file) + "¬" + Path.GetFullPath(input));
							}
							catch (PathTooLongException)
							{
								logger.Warning("The path for " + file + " was too long");
							}
							catch (Exception ex)
							{
								logger.Error(ex.ToString());
							}
						}
					}
					else if (File.Exists(input))
					{
						try
						{
							newInputFileNames.Add(Path.GetFullPath(input) + "¬" + Path.GetDirectoryName(Path.GetFullPath(input)));
						}
						catch (PathTooLongException)
						{
							logger.Warning("The path for " + input + " was too long");
						}
						catch (Exception ex)
						{
							logger.Error(ex.ToString());
						}
					}
				}

				// If we're in inverse cascade, reverse the list
				if (cascade == false)
				{
					newInputFileNames.Reverse();
				}

				// Create a dictionary of all ROMs from the input DATs
				DatFile userData;
				List<DatFile> datHeaders = PopulateUserData(newInputFileNames, inplace, clean, softlist,
					outDir, datdata, out userData, gamename, romname, romtype, sgt, slt, seq,
					crc, md5, sha1, nodump, trim, single, root, maxDegreeOfParallelism, logger);

				// Modify the Dictionary if necessary and output the results
				if (diff != 0 && cascade == null)
				{
					DiffNoCascade(diff, outDir, userData, newInputFileNames, logger);
				}
				// If we're in cascade and diff, output only cascaded diffs
				else if (diff != 0 && cascade != null)
				{
					DiffCascade(outDir, inplace, userData, newInputFileNames, datHeaders, skip, logger);
				}
				// Output all entries with user-defined merge
				else
				{
					MergeNoDiff(outDir, userData, newInputFileNames, datHeaders, logger);
				}
			}
			// Otherwise, loop through all of the inputs individually
			else
			{
				Parallel.ForEach(inputFileNames,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
					inputFileName =>
					{
						// Clean the input string
						if (inputFileName != "")
						{
							inputFileName = Path.GetFullPath(inputFileName);
						}

						if (File.Exists(inputFileName))
						{
							DatFile innerDatdata = (DatFile)datdata.CloneHeader();
							logger.User("Processing \"" + Path.GetFileName(inputFileName) + "\"");
							Parse(inputFileName, 0, 0, ref innerDatdata, gamename, romname,
								romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single,
								root, logger, true, clean, softlist, keepext: (innerDatdata.XSV != null));

							// If we have roms, output them
							if (innerDatdata.Files.Count != 0)
							{
								WriteDatfile(innerDatdata, (outDir == "" ? Path.GetDirectoryName(inputFileName) : outDir), logger, overwrite: (outDir != ""));
							}
						}
						else if (Directory.Exists(inputFileName))
						{
							inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

							Parallel.ForEach(Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories),
								new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
								file =>
								{
									logger.User("Processing \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
									DatFile innerDatdata = (DatFile)datdata.Clone();
									innerDatdata.Files = null;
									Parse(file, 0, 0, ref innerDatdata, gamename, romname, romtype, sgt,
									slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, true, clean, keepext: (datdata.XSV != null));

								// If we have roms, output them
								if (innerDatdata.Files != null && innerDatdata.Files.Count != 0)
									{
										WriteDatfile(innerDatdata, (outDir == "" ? Path.GetDirectoryName(file) : outDir + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)), logger, overwrite: (outDir != ""));
									}
								});
						}
						else
						{
							logger.Error("I'm sorry but " + inputFileName + " doesn't exist!");
						}
					});
			}
			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="userData">Output user DatData object to output</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		/// <returns>List of DatData objects representing headers</returns>
		private static List<DatFile> PopulateUserData(List<string> inputs, bool inplace, bool clean, bool softlist, string outDir,
			DatFile inputDat, out DatFile userData, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc,
			string md5, string sha1, bool? nodump, bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			DatFile[] datHeaders = new DatFile[inputs.Count];
			DateTime start = DateTime.Now;
			logger.User("Processing individual DATs");

			Parallel.For(0,
				inputs.Count,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				i =>
				{
					string input = inputs[i];
					logger.User("Adding DAT: " + input.Split('¬')[0]);
					datHeaders[i] = new DatFile
					{
						OutputFormat = (inputDat.OutputFormat != 0 ? inputDat.OutputFormat : 0),
						Files = new Dictionary<string, List<DatItem>>(),
						MergeRoms = inputDat.MergeRoms,
					};

					Parse(input.Split('¬')[0], i, 0, ref datHeaders[i], gamename, romname, romtype, sgt, slt, seq,
						crc, md5, sha1, nodump, trim, single, root, logger, true, clean, softlist);
				});

			logger.User("Processing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			logger.User("Populating internal DAT");
			userData = (DatFile)inputDat.CloneHeader();
			userData.Files = new Dictionary<string, List<DatItem>>();
			for (int i = 0; i < inputs.Count; i++)
			{
				List<string> keys = datHeaders[i].Files.Keys.ToList();
				foreach (string key in keys)
				{
					if (userData.Files.ContainsKey(key))
					{
						userData.Files[key].AddRange(datHeaders[i].Files[key]);
					}
					else
					{
						userData.Files.Add(key, datHeaders[i].Files[key]);
					}
					datHeaders[i].Files.Remove(key);
				}
				datHeaders[i].Files = null;
			}

			logger.User("Processing and populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datHeaders.ToList();
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void DiffNoCascade(DiffMode diff, string outDir, DatFile userData, List<string> inputs, Logger logger)
		{
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			// Default vars for use
			string post = "";
			DatFile outerDiffData = new DatFile();
			DatFile dupeData = new DatFile();

			// Don't have External dupes
			if ((diff & DiffMode.NoDupes) != 0)
			{
				post = " (No Duplicates)";
				outerDiffData = (DatFile)userData.CloneHeader();
				outerDiffData.FileName += post;
				outerDiffData.Name += post;
				outerDiffData.Description += post;
				outerDiffData.Files = new Dictionary<string, List<DatItem>>();
			}

			// Have External dupes
			if ((diff & DiffMode.Dupes) != 0)
			{
				post = " (Duplicates)";
				dupeData = (DatFile)userData.CloneHeader();
				dupeData.FileName += post;
				dupeData.Name += post;
				dupeData.Description += post;
				dupeData.Files = new Dictionary<string, List<DatItem>>();
			}

			// Create a list of DatData objects representing individual output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			if ((diff & DiffMode.Individuals) != 0)
			{
				DatFile[] outDatsArray = new DatFile[inputs.Count];

				Parallel.For(0, inputs.Count, j =>
				{
					string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
					DatFile diffData = (DatFile)userData.CloneHeader();
					diffData.FileName += innerpost;
					diffData.Name += innerpost;
					diffData.Description += innerpost;
					diffData.Files = new Dictionary<string, List<DatItem>>();
					outDatsArray[j] = diffData;
				});

				outDats = outDatsArray.ToList();
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = userData.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(userData.Files[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						// No duplicates
						if ((diff & DiffMode.NoDupes) != 0 || (diff & DiffMode.Individuals) != 0)
						{
							if (rom.Dupe < DupeType.ExternalHash)
							{
								// Individual DATs that are output
								if ((diff & DiffMode.Individuals) != 0)
								{
									if (outDats[rom.SystemID].Files.ContainsKey(key))
									{
										outDats[rom.SystemID].Files[key].Add(rom);
									}
									else
									{
										List<DatItem> tl = new List<DatItem>();
										tl.Add(rom);
										outDats[rom.SystemID].Files.Add(key, tl);
									}
								}

								// Merged no-duplicates DAT
								if ((diff & DiffMode.NoDupes) != 0)
								{
									Rom newrom = rom;
									newrom.MachineName += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

									if (outerDiffData.Files.ContainsKey(key))
									{
										outerDiffData.Files[key].Add(newrom);
									}
									else
									{
										List<DatItem> tl = new List<DatItem>();
										tl.Add(rom);
										outerDiffData.Files.Add(key, tl);
									}
								}
							}
						}

						// Duplicates only
						if ((diff & DiffMode.Dupes) != 0)
						{
							if (rom.Dupe >= DupeType.ExternalHash)
							{
								Rom newrom = rom;
								newrom.MachineName += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

								if (dupeData.Files.ContainsKey(key))
								{
									dupeData.Files[key].Add(newrom);
								}
								else
								{
									List<DatItem> tl = new List<DatItem>();
									tl.Add(rom);
									dupeData.Files.Add(key, tl);
								}
							}
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");

			// Output the difflist (a-b)+(b-a) diff
			if ((diff & DiffMode.NoDupes) != 0)
			{
				WriteDatfile(outerDiffData, outDir, logger);
			}

			// Output the (ab) diff
			if ((diff & DiffMode.Dupes) != 0)
			{
				WriteDatfile(dupeData, outDir, logger);
			}

			// Output the individual (a-b) DATs
			if ((diff & DiffMode.Individuals) != 0)
			{
				for (int j = 0; j < inputs.Count; j++)
				{
					// If we have an output directory set, replace the path
					string path = outDir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));

					// If we have more than 0 roms, output
					if (outDats[j].Files.Count > 0)
					{
						WriteDatfile(outDats[j], path, logger);
					}
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output cascading diffs
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void DiffCascade(string outDir, bool inplace, DatFile userData, List<string> inputs, List<DatFile> datHeaders, bool skip, Logger logger)
		{
			string post = "";

			// Create a list of DatData objects representing output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			DatFile[] outDatsArray = new DatFile[inputs.Count];

			Parallel.For(0, inputs.Count, j =>
			{
				string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				DatFile diffData;

				// If we're in inplace mode, take the appropriate DatData object already stored
				if (inplace || !String.IsNullOrEmpty(outDir))
				{
					diffData = datHeaders[j];
				}
				else
				{
					diffData = (DatFile)userData.CloneHeader();
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
				}
				diffData.Files = new Dictionary<string, List<DatItem>>();

				outDatsArray[j] = diffData;
			});

			outDats = outDatsArray.ToList();
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = userData.Files.Keys.ToList();

			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(userData.Files[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						if (outDats[rom.SystemID].Files.ContainsKey(key))
						{
							outDats[rom.SystemID].Files[key].Add(rom);
						}
						else
						{
							List<DatItem> tl = new List<DatItem>();
							tl.Add(rom);
							outDats[rom.SystemID].Files.Add(key, tl);
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");
			for (int j = (skip ? 1 : 0); j < inputs.Count; j++)
			{
				// If we have an output directory set, replace the path
				string path = "";
				if (inplace)
				{
					path = Path.GetDirectoryName(inputs[j].Split('¬')[0]);
				}
				else if (!String.IsNullOrEmpty(outDir))
				{
					path = outDir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));
				}

				// If we have more than 0 roms, output
				if (outDats[j].Files.Count > 0)
				{
					WriteDatfile(outDats[j], path, logger);
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output user defined merge
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void MergeNoDiff(string outDir, DatFile userData, List<string> inputs, List<DatFile> datHeaders, Logger logger)
		{
			// If we're in SuperDAT mode, prefix all games with their respective DATs
			if (userData.Type == "SuperDAT")
			{
				List<string> keys = userData.Files.Keys.ToList();
				foreach (string key in keys)
				{
					List<DatItem> newroms = new List<DatItem>();
					foreach (DatItem rom in userData.Files[key])
					{
						DatItem newrom = rom;
						string filename = inputs[newrom.SystemID].Split('¬')[0];
						string rootpath = inputs[newrom.SystemID].Split('¬')[1];

						rootpath += (rootpath == "" ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newrom.MachineName = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
							+ Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
							+ newrom.MachineName;
						newroms.Add(newrom);
					}
					userData.Files[key] = newroms;
				}
			}

			// Output a DAT only if there are roms
			if (userData.Files.Count != 0)
			{
				WriteDatfile(userData, outDir, logger);
			}
		}

		#endregion

		#region DAT Writing

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datdata">All information for creating the datfile header</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for file output:
		/// - Have the ability to strip special (non-ASCII) characters from rom information
		/// </remarks>
		public static bool WriteDatfile(DatFile datdata, string outDir, Logger logger, bool norename = true, bool stats = false, bool ignoreblanks = false, bool overwrite = true)
		{
			// If there's nothing there, abort
			if (datdata.Files == null || datdata.Files.Count == 0)
			{
				return false;
			}

			// If output directory is empty, use the current folder
			if (outDir.Trim() == "")
			{
				outDir = Environment.CurrentDirectory;
			}

			// Create the output directory if it doesn't already exist
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// If the DAT has no output format, default to XML
			if (datdata.OutputFormat == 0)
			{
				datdata.OutputFormat = OutputFormat.Xml;
			}

			// Make sure that the three essential fields are filled in
			if (String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Name = datdata.Description = "Default";
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Name = datdata.Description;
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Description = datdata.Name;
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Description;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Name = datdata.Description = datdata.FileName;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Name = datdata.Description;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Description = datdata.Name;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				// Nothing is needed
			}

			// Output initial statistics, for kicks
			if (stats)
			{
				Stats.OutputStats(datdata, logger, (datdata.RomCount + datdata.DiskCount == 0));
			}

			// Bucket roms by game name and optionally dedupe
			SortedDictionary<string, List<DatItem>> sortable = BucketByGame(datdata.Files, datdata.MergeRoms, norename, logger);

			// Get the outfile name
			Dictionary<OutputFormat, string> outfiles = Style.CreateOutfileNames(outDir, datdata, overwrite);

			try
			{
				foreach (OutputFormat outputFormat in outfiles.Keys)
				{
					string outfile = outfiles[outputFormat];

					logger.User("Opening file for writing: " + outfile);
					FileStream fs = File.Create(outfile);
					StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

					// Write out the header
					WriteHeader(sw, outputFormat, datdata, logger);

					// Write out each of the machines and roms
					int depth = 2, last = -1;
					string lastgame = null;
					List<string> splitpath = new List<string>();

					// Get a properly sorted set of keys
					List<string> keys = sortable.Keys.ToList();
					keys.Sort(Style.CompareNumeric);

					foreach (string key in keys)
					{
						List<DatItem> roms = sortable[key];

						for (int index = 0; index < roms.Count; index++)
						{
							DatItem rom = roms[index];

							// There are apparently times when a null rom can skip by, skip them
							if (rom.Name == null || rom.MachineName == null)
							{
								logger.Warning("Null rom found!");
								continue;
							}

							List<string> newsplit = rom.MachineName.Split('\\').ToList();

							// If we have a different game and we're not at the start of the list, output the end of last item
							if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
							{
								depth = WriteEndGame(sw, outputFormat, rom, splitpath, newsplit, lastgame, depth, out last, logger);
							}

							// If we have a new game, output the beginning of the new item
							if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
							{
								depth = WriteStartGame(sw, outputFormat, rom, newsplit, lastgame, depth, last, logger);
							}

							// If we have a "null" game (created by DATFromDir or something similar), log it to file
							if (rom.Type == ItemType.Rom
								&& ((Rom)rom).Size == -1
								&& ((Rom)rom).CRC == "null"
								&& ((Rom)rom).MD5 == "null"
								&& ((Rom)rom).SHA1 == "null")
							{
								logger.Log("Empty folder found: " + rom.MachineName);

								// If we're in a mode that doesn't allow for actual empty folders, add the blank info
								if (outputFormat != OutputFormat.SabreDat && outputFormat != OutputFormat.MissFile)
								{
									rom.Name = (rom.Name == "null" ? "-" : rom.Name);
									((Rom)rom).Size = Constants.SizeZero;
									((Rom)rom).CRC = Constants.CRCZero;
									((Rom)rom).MD5 = Constants.MD5Zero;
									((Rom)rom).SHA1 = Constants.SHA1Zero;
								}

								// Otherwise, set the new path and such, write out, and continue
								else
								{
									splitpath = newsplit;
									lastgame = rom.MachineName;
									continue;
								}
							}

							// Now, output the rom data
							WriteRomData(sw, outputFormat, rom, lastgame, datdata, depth, logger, ignoreblanks);

							// Set the new data to compare against
							splitpath = newsplit;
							lastgame = rom.MachineName;
						}
					}

					// Write the file footer out
					WriteFooter(sw, outputFormat, datdata, depth, logger);

					logger.Log("File written!" + Environment.NewLine);
					sw.Close();
					fs.Close();
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT header using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteHeader(StreamWriter sw, OutputFormat outputFormat, DatFile datdata, Logger logger)
		{
			try
			{
				string header = "";
				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
						header = "clrmamepro (\n" +
							"\tname \"" + datdata.Name + "\"\n" +
							"\tdescription \"" + datdata.Description + "\"\n" +
							"\tcategory \"" + datdata.Category + "\"\n" +
							"\tversion \"" + datdata.Version + "\"\n" +
							"\tdate \"" + datdata.Date + "\"\n" +
							"\tauthor \"" + datdata.Author + "\"\n" +
							"\temail \"" + datdata.Email + "\"\n" +
							"\thomepage \"" + datdata.Homepage + "\"\n" +
							"\turl \"" + datdata.Url + "\"\n" +
							"\tcomment \"" + datdata.Comment + "\"\n" +
							(datdata.ForcePacking == ForcePacking.Unzip ? "\tforcezipping no\n" : "") +
							(datdata.ForcePacking == ForcePacking.Zip ? "\tforcezipping yes\n" : "") +
							(datdata.ForceMerging == ForceMerging.Full ? "\tforcemerging full\n" : "") +
							(datdata.ForceMerging == ForceMerging.Split ? "\tforcemerging split\n" : "") +
							")\n";
						break;
					case OutputFormat.MissFile:
						if (datdata.XSV == true)
						{
							header = "\"File Name\"\t\"Internal Name\"\t\"Description\"\t\"Game Name\"\t\"Game Description\"\t\"Type\"\t\"" +
								"Rom Name\"\t\"Disk Name\"\t\"Size\"\t\"CRC\"\t\"MD5\"\t\"SHA1\"\t\"Nodump\"\n";
						}
						else if (datdata.XSV == false)
						{
							header = "\"File Name\",\"Internal Name\",\"Description\",\"Game Name\",\"Game Description\",\"Type\",\"" +
								"Rom Name\",\"Disk Name\",\"Size\",\"CRC\",\"MD5\",\"SHA1\",\"Nodump\"\n";
						}
						break;
					case OutputFormat.RomCenter:
						header = "[CREDITS]\n" +
							"author=" + datdata.Author + "\n" +
							"version=" + datdata.Version + "\n" +
							"comment=" + datdata.Comment + "\n" +
							"[DAT]\n" +
							"version=2.50\n" +
							"split=" + (datdata.ForceMerging == ForceMerging.Split ? "1" : "0") + "\n" +
							"merge=" + (datdata.ForceMerging == ForceMerging.Full ? "1" : "0") + "\n" +
							"[EMULATOR]\n" +
							"refname=" + datdata.Name + "\n" +
							"version=" + datdata.Description + "\n" +
							"[GAMES]\n";
						break;
					case OutputFormat.SabreDat:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(datdata.Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(datdata.Description) + "</description>\n" +
							(!String.IsNullOrEmpty(datdata.RootDir) ? "\t\t<rootdir>" + HttpUtility.HtmlEncode(datdata.RootDir) + "</rootdir>\n" : "") +
							(!String.IsNullOrEmpty(datdata.Category) ? "\t\t<category>" + HttpUtility.HtmlEncode(datdata.Category) + "</category>\n" : "") +
							"\t\t<version>" + HttpUtility.HtmlEncode(datdata.Version) + "</version>\n" +
							(!String.IsNullOrEmpty(datdata.Date) ? "\t\t<date>" + HttpUtility.HtmlEncode(datdata.Date) + "</date>\n" : "") +
							"\t\t<author>" + HttpUtility.HtmlEncode(datdata.Author) + "</author>\n" +
							(!String.IsNullOrEmpty(datdata.Comment) ? "\t\t<comment>" + HttpUtility.HtmlEncode(datdata.Comment) + "</comment>\n" : "") +
							(!String.IsNullOrEmpty(datdata.Type) || datdata.ForcePacking != ForcePacking.None || datdata.ForceMerging != ForceMerging.None || datdata.ForceNodump != ForceNodump.None ?
								"\t\t<flags>\n" +
									(!String.IsNullOrEmpty(datdata.Type) ? "\t\t\t<flag name=\"type\" value=\"" + HttpUtility.HtmlEncode(datdata.Type) + "\"/>\n" : "") +
									(datdata.ForcePacking == ForcePacking.Unzip ? "\t\t\t<flag name=\"forcepacking\" value=\"unzip\"/>\n" : "") +
									(datdata.ForcePacking == ForcePacking.Zip ? "\t\t\t<flag name=\"forcepacking\" value=\"zip\"/>\n" : "") +
									(datdata.ForceMerging == ForceMerging.Full ? "\t\t\t<flag name=\"forcemerging\" value=\"full\"/>\n" : "") +
									(datdata.ForceMerging == ForceMerging.Split ? "\t\t\t<flag name=\"forcemerging\" value=\"split\"/>\n" : "") +
									(datdata.ForceNodump == ForceNodump.Ignore ? "\t\t\t<flag name=\"forcenodump\" value=\"ignore\"/>\n" : "") +
									(datdata.ForceNodump == ForceNodump.Obsolete ? "\t\t\t<flag name=\"forcenodump\" value=\"obsolete\"/>\n" : "") +
									(datdata.ForceNodump == ForceNodump.Required ? "\t\t\t<flag name=\"forcenodump\" value=\"required\"/>\n" : "") +
									"\t\t</flags>\n"
							: "") +
							"\t</header>\n" +
							"\t<data>\n";
						break;
					case OutputFormat.Xml:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(datdata.Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(datdata.Description) + "</description>\n" +
							(!String.IsNullOrEmpty(datdata.RootDir) ? "\t\t<rootdir>" + HttpUtility.HtmlEncode(datdata.RootDir) + "</rootdir>\n" : "") +
							(!String.IsNullOrEmpty(datdata.Category) ? "\t\t<category>" + HttpUtility.HtmlEncode(datdata.Category) + "</category>\n" : "") +
							"\t\t<version>" + HttpUtility.HtmlEncode(datdata.Version) + "</version>\n" +
							(!String.IsNullOrEmpty(datdata.Date) ? "\t\t<date>" + HttpUtility.HtmlEncode(datdata.Date) + "</date>\n" : "") +
							"\t\t<author>" + HttpUtility.HtmlEncode(datdata.Author) + "</author>\n" +
							(!String.IsNullOrEmpty(datdata.Email) ? "\t\t<email>" + HttpUtility.HtmlEncode(datdata.Email) + "</email>\n" : "") +
							(!String.IsNullOrEmpty(datdata.Homepage) ? "\t\t<homepage>" + HttpUtility.HtmlEncode(datdata.Homepage) + "</homepage>\n" : "") +
							(!String.IsNullOrEmpty(datdata.Url) ? "\t\t<url>" + HttpUtility.HtmlEncode(datdata.Url) + "</url>\n" : "") +
							(!String.IsNullOrEmpty(datdata.Comment) ? "\t\t<comment>" + HttpUtility.HtmlEncode(datdata.Comment) + "</comment>\n" : "") +
							(!String.IsNullOrEmpty(datdata.Type) ? "\t\t<type>" + HttpUtility.HtmlEncode(datdata.Type) + "</type>\n" : "") +
							(datdata.ForcePacking != ForcePacking.None || datdata.ForceMerging != ForceMerging.None || datdata.ForceNodump != ForceNodump.None ?
								"\t\t<clrmamepro" +
									(datdata.ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : "") +
									(datdata.ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : "") +
									(datdata.ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : "") +
									(datdata.ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : "") +
									(datdata.ForceNodump == ForceNodump.Ignore ? " forcenodump=\"ignore\"" : "") +
									(datdata.ForceNodump == ForceNodump.Obsolete ? " forcenodump=\"obsolete\"" : "") +
									(datdata.ForceNodump == ForceNodump.Required ? " forcenodump=\"required\"" : "") +
									" />\n"
							: "") +
							"\t</header>\n";
						break;
				}

				// Write the header out
				sw.Write(header);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		public static int WriteStartGame(StreamWriter sw, OutputFormat outputFormat, DatItem rom, List<string> newsplit, string lastgame, int depth, int last, Logger logger)
		{
			try
			{
				// No game should start with a path separator
				if (rom.MachineName.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.MachineName = rom.MachineName.Substring(1);
				}

				string state = "";
				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "game (\n\tname \"" + rom.MachineName + "\"\n" +
							(String.IsNullOrEmpty(rom.RomOf) ? "" : "\tromof \"" + rom.RomOf + "\"\n") +
							(String.IsNullOrEmpty(rom.CloneOf) ? "" : "\tcloneof \"" + rom.CloneOf + "\"\n") +
							"\tdescription \"" + (String.IsNullOrEmpty(rom.MachineDescription) ? rom.MachineName : rom.MachineDescription) + "\"\n" +
							(String.IsNullOrEmpty(rom.Year) ? "" : "\tyear " + rom.Year + "\n") +
							(String.IsNullOrEmpty(rom.Manufacturer) ? "" : "\tmanufacturer \"" + rom.Manufacturer + "\"\n");

						break;
					case OutputFormat.SabreDat:
						for (int i = (last == -1 ? 0 : last); i < newsplit.Count; i++)
						{
							for (int j = 0; j < depth - last + i - (lastgame == null ? 1 : 0); j++)
							{
								state += "\t";
							}
							state += "<directory name=\"" + HttpUtility.HtmlEncode(newsplit[i]) + "\" description=\"" +
							HttpUtility.HtmlEncode(newsplit[i]) + "\">\n";
						}
						depth = depth - (last == -1 ? 0 : last) + newsplit.Count;
						break;
					case OutputFormat.Xml:
						state += "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.MachineName) + "\"" +
							(rom.IsBios ? " isbios=\"yes\"" : "") +
							(String.IsNullOrEmpty(rom.CloneOf) ? "" : " cloneof=\"" + HttpUtility.HtmlEncode(rom.CloneOf) + "\"") +
							(String.IsNullOrEmpty(rom.RomOf) ? "" : " romof=\"" + HttpUtility.HtmlEncode(rom.RomOf) + "\"") +
							(String.IsNullOrEmpty(rom.SampleOf) ? "" : " sampleof=\"" + HttpUtility.HtmlEncode(rom.SampleOf) + "\"") +
							">\n" +
							(String.IsNullOrEmpty(rom.Comment) ? "" : "\t\t<comment>" + HttpUtility.HtmlEncode(rom.Comment) + "</comment>\n") +
							"\t\t<description>" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.MachineDescription) ? rom.MachineName : rom.MachineDescription)) + "</description>\n" +
							(String.IsNullOrEmpty(rom.Year) ? "" : "\t\t<year>" + HttpUtility.HtmlEncode(rom.Year) + "</year>\n") +
							(String.IsNullOrEmpty(rom.Manufacturer) ? "" : "\t\t<manufacturer>" + HttpUtility.HtmlEncode(rom.Manufacturer) + "</manufacturer>\n");
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		public static int WriteEndGame(StreamWriter sw, OutputFormat outputFormat, DatItem rom, List<string> splitpath, List<string> newsplit, string lastgame, int depth, out int last, Logger logger)
		{
			last = 0;

			try
			{
				string state = "";

				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += (String.IsNullOrEmpty(rom.SampleOf) ? "" : "\tsampleof \"" + rom.SampleOf + "\"\n") + ")\n";
						break;
					case OutputFormat.SabreDat:
						if (splitpath != null)
						{
							for (int i = 0; i < newsplit.Count && i < splitpath.Count; i++)
							{
								// Always keep track of the last seen item
								last = i;

								// If we find a difference, break
								if (newsplit[i] != splitpath[i])
								{
									break;
								}
							}

							// Now that we have the last known position, take down all open folders
							for (int i = depth - 1; i > last + 1; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									state += "\t";
								}
								state += "</directory>\n";
							}

							// Reset the current depth
							depth = 2 + last;
						}
						break;
					case OutputFormat.Xml:
						state += "\t</machine>\n";
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out RomData using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="outputFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteRomData(StreamWriter sw, OutputFormat outputFormat, DatItem rom, string lastgame, DatFile datdata, int depth, Logger logger, bool ignoreblanks = false)
		{
			// If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
			if (ignoreblanks
				&& (rom.Type == ItemType.Rom
				&& (((Rom)rom).Size == 0 || ((Rom)rom).Size == -1)))
			{
				return true;
			}

			try
			{
				string state = "";
				switch (outputFormat)
				{
					case OutputFormat.ClrMamePro:
						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\tarchive ( name\"" + rom.Name + "\""
									+ " )\n";
								break;
							case ItemType.BiosSet:
								state += "\tbiosset ( name\"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description \"" + ((BiosSet)rom).Description + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? "default " + ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ " )\n";
								break;
							case ItemType.Disk:
								state += "\tdisk ( name \"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5 " + ((Disk)rom).MD5.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1 " + ((Disk)rom).SHA1.ToLowerInvariant() : "")
									+ (((Disk)rom).Nodump ? " flags nodump" : "")
									+ " )\n";
								break;
							case ItemType.Release:
								state += "\trelease ( name\"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region \"" + ((Release)rom).Region + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language \"" + ((Release)rom).Language + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date \"" + ((Release)rom).Date + "\"" : "")
									+ (((Release)rom).Default != null
										? "default " + ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ " )\n";
								break;
							case ItemType.Rom:
								state += "\trom ( name \"" + rom.Name + "\""
									+ (((Rom)rom).Size != -1 ? " size " + ((Rom)rom).Size : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc " + ((Rom)rom).CRC.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5 " + ((Rom)rom).MD5.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1 " + ((Rom)rom).SHA1.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date \"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).Nodump ? " flags nodump" : "")
									+ " )\n";
								break;
							case ItemType.Sample:
								state += "\tsample ( name\"" + rom.Name + "\""
									+ " )\n";
								break;
						}

						break;
					case OutputFormat.MissFile:
						// Missfile should only output Rom and Disk
						if (rom.Type != ItemType.Disk && rom.Type != ItemType.Disk)
						{
							return true;
						}

						string pre = datdata.Prefix + (datdata.Quotes ? "\"" : "");
						string post = (datdata.Quotes ? "\"" : "") + datdata.Postfix;

						if (rom.Type == ItemType.Rom)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.MachineName)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
							post = post
								.Replace("%game%", rom.MachineName)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
						}
						else if (rom.Type == ItemType.Disk)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.MachineName)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
							post = post
								.Replace("%game%", rom.MachineName)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
						}

						// If we're in Romba mode, the state is consistent
						if (datdata.Romba)
						{
							if (rom.Type == ItemType.Rom)
							{
								// We can only write out if there's a SHA-1
								if (((Rom)rom).SHA1 != "")
								{
									string name = ((Rom)rom).SHA1.Substring(0, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(2, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(4, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(6, 2)
										+ "/" + ((Rom)rom).SHA1 + ".gz";
									state += pre + name + post + "\n";
								}
							}
							else if (rom.Type == ItemType.Disk)
							{
								// We can only write out if there's a SHA-1
								if (((Disk)rom).SHA1 != "")
								{
									string name = ((Disk)rom).SHA1.Substring(0, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(2, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(4, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(6, 2)
										+ "/" + ((Disk)rom).SHA1 + ".gz";
									state += pre + name + post + "\n";
								}
							}
						}
						// If we're in TSV/CSV mode, similarly the state is consistent
						else if (datdata.XSV != null)
						{
							string separator = (datdata.XSV == true ? "\t" : ",");

							if (rom.Type == ItemType.Rom)
							{
								string inline = "\"" + datdata.FileName + "\""
									+ separator + "\"" + datdata.Name + "\""
									+ separator + "\"" + datdata.Description + "\""
									+ separator + "\"" + rom.MachineName + "\""
									+ separator + "\"" + rom.MachineDescription + "\""
									+ separator + "\"rom\""
									+ separator + "\"" + rom.Name + "\""
									+ separator + "\"\""
									+ separator + "\"" + ((Rom)rom).Size + "\""
									+ separator + "\"" + ((Rom)rom).CRC + "\""
									+ separator + "\"" + ((Rom)rom).MD5 + "\""
									+ separator + "\"" + ((Rom)rom).SHA1 + "\""
									+ separator + (((Rom)rom).Nodump ? "\"Nodump\"" : "\"\"");
								state += pre + inline + post + "\n";
							}
							else if (rom.Type == ItemType.Disk)
							{
								string inline = "\"" + datdata.FileName + "\""
									+ separator + "\"" + datdata.Name + "\""
									+ separator + "\"" + datdata.Description + "\""
									+ separator + "\"" + rom.MachineName + "\""
									+ separator + "\"" + rom.MachineDescription + "\""
									+ separator + "\"disk\""
									+ separator + "\"\""
									+ separator + "\"" + rom.Name + "\""
									+ separator + "\"\""
									+ separator + "\"\""
									+ separator + "\"" + ((Disk)rom).MD5 + "\""
									+ separator + "\"" + ((Disk)rom).SHA1 + "\""
									+ separator + (((Disk)rom).Nodump ? "\"Nodump\"" : "\"\"");
								state += pre + inline + post + "\n";
							}
						}
						// Otherwise, use any flags
						else
						{
							string name = (datdata.UseGame ? rom.MachineName : rom.Name);
							if (datdata.RepExt != "" || datdata.RemExt)
							{
								if (datdata.RemExt)
								{
									datdata.RepExt = "";
								}

								string dir = Path.GetDirectoryName(name);
								dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
								name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + datdata.RepExt);
							}
							if (datdata.AddExt != "")
							{
								name += datdata.AddExt;
							}
							if (!datdata.UseGame && datdata.GameName)
							{
								name = Path.Combine(rom.MachineName, name);
							}

							if (datdata.UseGame && rom.MachineName != lastgame)
							{
								state += pre + name + post + "\n";
								lastgame = rom.MachineName;
							}
							else if (!datdata.UseGame)
							{
								state += pre + name + post + "\n";
							}
						}
						break;
					case OutputFormat.RedumpMD5:
						if (rom.Type == ItemType.Rom)
						{
							state += ((Rom)rom).MD5 + " *" + rom.Name + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += ((Disk)rom).MD5 + " *" + rom.Name + "\n";
						}
						break;
					case OutputFormat.RedumpSFV:
						if (rom.Type == ItemType.Rom)
						{
							state += rom.Name + " " + ((Rom)rom).CRC + "\n";
						}
						break;
					case OutputFormat.RedumpSHA1:
						if (rom.Type == ItemType.Rom)
						{
							state += ((Rom)rom).SHA1 + " *" + rom.Name + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += ((Disk)rom).SHA1 + " *" + rom.Name + "\n";
						}
						break;
					case OutputFormat.RomCenter:
						if (rom.Type == ItemType.Rom)
						{
							state += "¬" + (String.IsNullOrEmpty(rom.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.CloneOf)) +
							"¬" + (String.IsNullOrEmpty(rom.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.CloneOf)) +
							"¬" + HttpUtility.HtmlEncode(rom.MachineName) +
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.MachineDescription) ? rom.MachineName : rom.MachineDescription)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬" + ((Rom)rom).CRC.ToLowerInvariant() +
							"¬" + (((Rom)rom).Size != -1 ? ((Rom)rom).Size.ToString() : "") + "¬¬¬\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += "¬" + (String.IsNullOrEmpty(rom.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.CloneOf)) +
							"¬" + (String.IsNullOrEmpty(rom.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.CloneOf)) +
							"¬" + HttpUtility.HtmlEncode(rom.MachineName) +
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.MachineDescription) ? rom.MachineName : rom.MachineDescription)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬¬¬¬¬\n";
						}

						break;
					case OutputFormat.SabreDat:
						string prefix = "";
						for (int i = 0; i < depth; i++)
						{
							prefix += "\t";
						}
						state += prefix;

						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "<file type=\"archive\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
							case ItemType.BiosSet:
								state += "<file type=\"biosset\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Disk:
								state += "<file type=\"disk\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).Nodump ? prefix + "/>\n" + prefix + "\t<flags>\n" +
										prefix + "\t\t<flag name=\"status\" value=\"nodump\"/>\n" +
										prefix + "\t</flags>\n" +
										prefix + "</file>\n" : "/>\n");
								break;
							case ItemType.Release:
								state += "<file type=\"release\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Rom:
								state += "<file type=\"rom\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).Nodump ? prefix + "/>\n" + prefix + "\t<flags>\n" +
										prefix + "\t\t<flag name=\"status\" value=\"nodump\"/>\n" +
										prefix + "\t</flags>\n" +
										prefix + "</file>\n" : "/>\n");
								break;
							case ItemType.Sample:
								state += "<file type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
						}
						break;
					case OutputFormat.Xml:
						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\t\t<archive name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
							case ItemType.BiosSet:
								state += "\t\t<biosset name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Disk:
								state += "\t\t<disk name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).Nodump ? " status=\"nodump\"" : "")
									+ "/>\n";
								break;
							case ItemType.Release:
								state += "\t\t<release name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Rom:
								state += "\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).Nodump ? " status=\"nodump\"" : "")
									+ "/>\n";
								break;
							case ItemType.Sample:
								state += "\t\t<file type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
						}
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT footer using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteFooter(StreamWriter sw, OutputFormat outputFormat, DatFile datdata, int depth, Logger logger)
		{
			try
			{
				string footer = "";

				// If we have roms, output the full footer
				if (datdata.Files != null && datdata.Files.Count > 0)
				{
					switch (outputFormat)
					{
						case OutputFormat.ClrMamePro:
							footer = ")";
							break;
						case OutputFormat.SabreDat:
							for (int i = depth - 1; i >= 2; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									footer += "\t";
								}
								footer += "</directory>\n";
							}
							footer += "\t</data>\n</datafile>";
							break;
						case OutputFormat.Xml:
							footer = "\t</machine>\n</datafile>";
							break;
					}
				}

				// Otherwise, output the abbreviated form
				else
				{
					switch (outputFormat)
					{
						case OutputFormat.SabreDat:
						case OutputFormat.Xml:
							footer = "</datafile>";
							break;
					}
				}

				// Write the footer out
				sw.Write(footer);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		#endregion

		#region DAT Splitting

		/// <summary>
		/// Split a DAT by input extensions
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="extA">List of extensions to split on (first DAT)</param>
		/// <param name="extB">List of extensions to split on (second DAT)</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public static bool SplitByExt(string filename, string outDir, string basepath, List<string> extA, List<string> extB, Logger logger)
		{
			// Make sure all of the extensions have a dot at the beginning
			List<string> newExtA = new List<string>();
			foreach (string s in extA)
			{
				newExtA.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
			}
			string newExtAString = string.Join(",", newExtA);

			List<string> newExtB = new List<string>();
			foreach (string s in extB)
			{
				newExtB.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
			}
			string newExtBString = string.Join(",", newExtB);

			// Get the file format
			OutputFormat outputFormat = GetOutputFormat(filename, logger);
			if (outputFormat == 0)
			{
				return true;
			}

			// Get the file data to be split
			DatFile datdata = new DatFile();
			Parse(filename, 0, 0, ref datdata, logger, softlist: true);

			// Set all of the appropriate outputs for each of the subsets
			DatFile datdataA = new DatFile
			{
				FileName = datdata.FileName + " (" + newExtAString + ")",
				Name = datdata.Name + " (" + newExtAString + ")",
				Description = datdata.Description + " (" + newExtAString + ")",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Files = new Dictionary<string, List<DatItem>>(),
				OutputFormat = outputFormat,
			};
			DatFile datdataB = new DatFile
			{
				FileName = datdata.FileName + " (" + newExtBString + ")",
				Name = datdata.Name + " (" + newExtBString + ")",
				Description = datdata.Description + " (" + newExtBString + ")",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Files = new Dictionary<string, List<DatItem>>(),
				OutputFormat = outputFormat,
			};

			// If roms is empty, return false
			if (datdata.Files.Count == 0)
			{
				return false;
			}

			// Now separate the roms accordingly
			foreach (string key in datdata.Files.Keys)
			{
				foreach (DatItem rom in datdata.Files[key])
				{
					if (newExtA.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataA.Files.ContainsKey(key))
						{
							datdataA.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataA.Files.Add(key, temp);
						}
					}
					else if (newExtB.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataB.Files.ContainsKey(key))
						{
							datdataB.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataB.Files.Add(key, temp);
						}
					}
					else
					{
						if (datdataA.Files.ContainsKey(key))
						{
							datdataA.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataA.Files.Add(key, temp);
						}
						if (datdataB.Files.ContainsKey(key))
						{
							datdataB.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							datdataB.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(filename);
			}

			// Then write out both files
			bool success = WriteDatfile(datdataA, outDir, logger);
			success &= WriteDatfile(datdataB, outDir, logger);

			return success;
		}

		/// <summary>
		/// Split a DAT by best available hashes
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public static bool SplitByHash(string filename, string outDir, string basepath, Logger logger)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Get the file format
			OutputFormat outputFormat = GetOutputFormat(filename, logger);
			if (outputFormat == 0)
			{
				return true;
			}

			// Get the file data to be split
			DatFile datdata = new DatFile();
			Parse(filename, 0, 0, ref datdata, logger, true, softlist: true);

			// Create each of the respective output DATs
			logger.User("Creating and populating new DATs");
			DatFile nodump = new DatFile
			{
				FileName = datdata.FileName + " (Nodump)",
				Name = datdata.Name + " (Nodump)",
				Description = datdata.Description + " (Nodump)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};
			DatFile sha1 = new DatFile
			{
				FileName = datdata.FileName + " (SHA-1)",
				Name = datdata.Name + " (SHA-1)",
				Description = datdata.Description + " (SHA-1)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};
			DatFile md5 = new DatFile
			{
				FileName = datdata.FileName + " (MD5)",
				Name = datdata.Name + " (MD5)",
				Description = datdata.Description + " (MD5)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};
			DatFile crc = new DatFile
			{
				FileName = datdata.FileName + " (CRC)",
				Name = datdata.Name + " (CRC)",
				Description = datdata.Description + " (CRC)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};

			DatFile other = new DatFile
			{
				FileName = datdata.FileName + " (Other)",
				Name = datdata.Name + " (Other)",
				Description = datdata.Description + " (Other)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = datdata.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = datdata.Files[key];
				foreach (DatItem rom in roms)
				{
					// If the file is not a Rom or Disk, continue
					if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
					{
						continue;
					}

					// If the file is a nodump
					if ((rom.Type == ItemType.Rom && ((Rom)rom).Nodump)
						|| (rom.Type == ItemType.Disk && ((Disk)rom).Nodump))
					{
						if (nodump.Files.ContainsKey(key))
						{
							nodump.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							nodump.Files.Add(key, temp);
						}
					}
					// If the file has a SHA-1
					else if ((rom.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)rom).SHA1))
						|| (rom.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)rom).SHA1)))
					{
						if (sha1.Files.ContainsKey(key))
						{
							sha1.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							sha1.Files.Add(key, temp);
						}
					}
					// If the file has no SHA-1 but has an MD5
					else if ((rom.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)rom).MD5))
						|| (rom.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)rom).MD5)))
					{
						if (md5.Files.ContainsKey(key))
						{
							md5.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							md5.Files.Add(key, temp);
						}
					}
					// If the file has no MD5 but a CRC
					else if ((rom.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)rom).SHA1))
						|| (rom.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)rom).SHA1)))
					{
						if (crc.Files.ContainsKey(key))
						{
							crc.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							crc.Files.Add(key, temp);
						}
					}
					else
					{
						if (other.Files.ContainsKey(key))
						{
							other.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							other.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(filename);
			}

			// Now, output all of the files to the output directory
			logger.User("DAT information created, outputting new files");
			bool success = true;
			if (nodump.Files.Count > 0)
			{
				success &= WriteDatfile(nodump, outDir, logger);
			}
			if (sha1.Files.Count > 0)
			{
				success &= WriteDatfile(sha1, outDir, logger);
			}
			if (md5.Files.Count > 0)
			{
				success &= WriteDatfile(md5, outDir, logger);
			}
			if (crc.Files.Count > 0)
			{
				success &= WriteDatfile(crc, outDir, logger);
			}

			return success;
		}

		/// <summary>
		/// Split a DAT by type of Rom
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public static bool SplitByType(string filename, string outDir, string basepath, Logger logger)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Get the file format
			OutputFormat outputFormat = GetOutputFormat(filename, logger);
			if (outputFormat == 0)
			{
				return true;
			}

			// Get the file data to be split
			DatFile datdata = new DatFile();
			Parse(filename, 0, 0, ref datdata, logger, true, softlist: true);

			// Create each of the respective output DATs
			logger.User("Creating and populating new DATs");
			DatFile romdat = new DatFile
			{
				FileName = datdata.FileName + " (ROM)",
				Name = datdata.Name + " (ROM)",
				Description = datdata.Description + " (ROM)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};
			DatFile diskdat = new DatFile
			{
				FileName = datdata.FileName + " (Disk)",
				Name = datdata.Name + " (Disk)",
				Description = datdata.Description + " (Disk)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};
			DatFile sampledat = new DatFile
			{
				FileName = datdata.FileName + " (Sample)",
				Name = datdata.Name + " (Sample)",
				Description = datdata.Description + " (Sample)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<DatItem>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = datdata.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = datdata.Files[key];
				foreach (DatItem rom in roms)
				{
					// If the file is a Rom
					if (rom.Type == ItemType.Rom)
					{
						if (romdat.Files.ContainsKey(key))
						{
							romdat.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							romdat.Files.Add(key, temp);
						}
					}
					// If the file is a Disk
					else if (rom.Type == ItemType.Disk)
					{
						if (diskdat.Files.ContainsKey(key))
						{
							diskdat.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							diskdat.Files.Add(key, temp);
						}
					}

					// If the file is a Sample
					else if (rom.Type == ItemType.Sample)
					{
						if (sampledat.Files.ContainsKey(key))
						{
							sampledat.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							sampledat.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(filename);
			}

			// Now, output all of the files to the output directory
			logger.User("DAT information created, outputting new files");
			bool success = true;
			if (romdat.Files.Count > 0)
			{
				success &= WriteDatfile(romdat, outDir, logger);
			}
			if (diskdat.Files.Count > 0)
			{
				success &= WriteDatfile(diskdat, outDir, logger);
			}
			if (sampledat.Files.Count > 0)
			{
				success &= WriteDatfile(sampledat, outDir, logger);
			}

			return success;
		}

		#endregion
	}
}
