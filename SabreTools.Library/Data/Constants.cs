using System;
using System.Reflection;

using SabreTools.Library.Tools;

namespace SabreTools.Library.Data
{
	/// <summary>
	/// Constants that are used throughout the library
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// The current toolset version to be used by all child applications
		/// </summary>
		public readonly static string Version = "v0.9.9 RC1"; // + Assembly.GetExecutingAssembly().GetLinkerTime().ToString("yyyy-MM-dd HH:mm:ss");
		public const int HeaderHeight = 3;

		#region 0-byte file constants

		public const long SizeZero = 0;
		public const string CRCZero = "00000000";
		public const string MD5Zero = "d41d8cd98f00b204e9800998ecf8427e";
		public const string SHA1Zero = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
		public const string SHA256Zero = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";
		public const string SHA384Zero = "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7";
		public const string SHA512Zero = "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f";

		#endregion

		#region Byte (1000-based) size comparisons

		public const long KiloByte = 1000;
		public readonly static long MegaByte = (long)Math.Pow(KiloByte, 2);
		public readonly static long GigaByte = (long)Math.Pow(KiloByte, 3);
		public readonly static long TeraByte = (long)Math.Pow(KiloByte, 4);
		public readonly static long PetaByte = (long)Math.Pow(KiloByte, 5);
		public readonly static long ExaByte = (long)Math.Pow(KiloByte, 6);
		public readonly static long ZettaByte = (long)Math.Pow(KiloByte, 7);
		public readonly static long YottaByte = (long)Math.Pow(KiloByte, 8);

		#endregion

		#region Byte (1024-based) size comparisons

		public const long KibiByte = 1024;
		public readonly static long MibiByte = (long)Math.Pow(KibiByte, 2);
		public readonly static long GibiByte = (long)Math.Pow(KibiByte, 3);
		public readonly static long TibiByte = (long)Math.Pow(KibiByte, 4);
		public readonly static long PibiByte = (long)Math.Pow(KibiByte, 5);
		public readonly static long ExiByte = (long)Math.Pow(KibiByte, 6);
		public readonly static long ZittiByte = (long)Math.Pow(KibiByte, 7);
		public readonly static long YittiByte = (long)Math.Pow(KibiByte, 8);

		#endregion

		#region CHD header values

		// CHD signature - "MComprHD"
		public readonly static byte[] CHDSignatureBytes = { 0x4d, 0x43, 0x6f, 0x6d, 0x70, 0x72, 0x48, 0x44 };
		public const ulong CHDSignature = 0x4d436f6d70724844;

		// Header versions and sizes
		public const int CHD_HEADER_VERSION = 5;
		public const int CHD_V3_HEADER_SIZE = 120;
		public const int CHD_V4_HEADER_SIZE = 108;
		public const int CHD_V5_HEADER_SIZE = 124;

		// Key offsets within the header (V3)
		public const long CHDv3MapOffsetOffset = 0;    // offset of map offset field
		public const long CHDv3MetaOffsetOffset = 36;  // offset of metaoffset field
		public const long CHDv3SHA1Offset = 80;        // offset of SHA1 field
		public const long CHDv3RawSHA1Offset = 0;      // offset of raw SHA1 field
		public const long CHDv3ParentSHA1Offset = 100; // offset of parent SHA1 field

		// Key offsets within the header (V4)
		public const long CHDv4MapOffsetOffset = 0;    // offset of map offset field
		public const long CHDv4MetaOffsetOffset = 36;  // offset of metaoffset field
		public const long CHDv4SHA1Offset = 48;        // offset of SHA1 field
		public const long CHDv4RawSHA1Offset = 88;     // offset of raw SHA1 field
		public const long CHDv4ParentSHA1Offset = 68;  // offset of parent SHA1 field

		// Key offsets within the header (V5)
		public const long CHDv5MapOffsetOffset = 40;   // offset of map offset field
		public const long CHDv5MetaOffsetOffset = 48;  // offset of metaoffset field
		public const long CHDv5SHA1Offset = 84;        // offset of SHA1 field
		public const long CHDv5RawSHA1Offset = 64;     // offset of raw SHA1 field
		public const long CHDv5ParentSHA1Offset = 104; // offset of parent SHA1 field

		#endregion

		#region Database schema

		public const string HeadererDbSchema = "Headerer";
		public const string HeadererFileName = "Headerer.sqlite";
		public const string HeadererConnectionString = "Data Source=" + HeadererFileName + ";Version = 3;";

		#endregion

		#region DTDs

		public const string LogiqxDTD = @"<!--
   ROM Management Datafile - DTD

   For further information, see: http://www.logiqx.com/

   This DTD module is identified by the PUBLIC and SYSTEM identifiers:

   PUBLIC "" -//Logiqx//DTD ROM Management Datafile//EN""
   SYSTEM ""http://www.logiqx.com/Dats/datafile.dtd""

   $Revision: 1.5 $
   $Date: 2008/10/28 21:39:16 $

-->

<!ELEMENT datafile(header?, game*, machine*)>
	<!ATTLIST datafile build CDATA #IMPLIED>
	<!ATTLIST datafile debug (yes|no) ""no"">
	<!ELEMENT header(name, description, category?, version, date?, author, email?, homepage?, url?, comment?, clrmamepro?, romcenter?)>
		<!ELEMENT name(#PCDATA)>
		<!ELEMENT description (#PCDATA)>
		<!ELEMENT category (#PCDATA)>
		<!ELEMENT version (#PCDATA)>
		<!ELEMENT date (#PCDATA)>
		<!ELEMENT author (#PCDATA)>
		<!ELEMENT email (#PCDATA)>
		<!ELEMENT homepage (#PCDATA)>
		<!ELEMENT url (#PCDATA)>
		<!ELEMENT comment (#PCDATA)>
		<!ELEMENT clrmamepro EMPTY>
			<!ATTLIST clrmamepro header CDATA #IMPLIED>
			<!ATTLIST clrmamepro forcemerging (none|split|full) ""split"">
			<!ATTLIST clrmamepro forcenodump(obsolete|required|ignore) ""obsolete"">
			<!ATTLIST clrmamepro forcepacking(zip|unzip) ""zip"">
		<!ELEMENT romcenter EMPTY>
			<!ATTLIST romcenter plugin CDATA #IMPLIED>
			<!ATTLIST romcenter rommode (merged|split|unmerged) ""split"">
			<!ATTLIST romcenter biosmode(merged|split|unmerged) ""split"">
			<!ATTLIST romcenter samplemode(merged|unmerged) ""merged"">
			<!ATTLIST romcenter lockrommode(yes|no) ""no"">
			<!ATTLIST romcenter lockbiosmode(yes|no) ""no"">
			<!ATTLIST romcenter locksamplemode(yes|no) ""no"">
	<!ELEMENT game(comment*, description, year?, manufacturer?, release*, biosset*, rom*, disk*, sample*, archive*)>
		<!ATTLIST game name CDATA #REQUIRED>
		<!ATTLIST game sourcefile CDATA #IMPLIED>
		<!ATTLIST game isbios (yes|no) ""no"">
		<!ATTLIST game cloneof CDATA #IMPLIED>
		<!ATTLIST game romof CDATA #IMPLIED>
		<!ATTLIST game sampleof CDATA #IMPLIED>
		<!ATTLIST game board CDATA #IMPLIED>
		<!ATTLIST game rebuildto CDATA #IMPLIED>
		<!ELEMENT year (#PCDATA)>
		<!ELEMENT manufacturer (#PCDATA)>
		<!ELEMENT release EMPTY>
			<!ATTLIST release name CDATA #REQUIRED>
			<!ATTLIST release region CDATA #REQUIRED>
			<!ATTLIST release language CDATA #IMPLIED>
			<!ATTLIST release date CDATA #IMPLIED>
			<!ATTLIST release default (yes|no) ""no"">
		<!ELEMENT biosset EMPTY>
			<!ATTLIST biosset name CDATA #REQUIRED>
			<!ATTLIST biosset description CDATA #REQUIRED>
			<!ATTLIST biosset default (yes|no) ""no"">
		<!ELEMENT rom EMPTY>
			<!ATTLIST rom name CDATA #REQUIRED>
			<!ATTLIST rom size CDATA #REQUIRED>
			<!ATTLIST rom crc CDATA #IMPLIED>
			<!ATTLIST rom md5 CDATA #IMPLIED>
			<!ATTLIST rom sha1 CDATA #IMPLIED>
			<!ATTLIST rom sha256 CDATA #IMPLIED>
			<!ATTLIST rom sha384 CDATA #IMPLIED>
			<!ATTLIST rom sha512 CDATA #IMPLIED>
			<!ATTLIST rom merge CDATA #IMPLIED>
			<!ATTLIST rom status (baddump|nodump|good|verified) ""good"">
			<!ATTLIST rom date CDATA #IMPLIED>
		<!ELEMENT disk EMPTY>
			<!ATTLIST disk name CDATA #REQUIRED>
			<!ATTLIST disk md5 CDATA #IMPLIED>
			<!ATTLIST disk sha1 CDATA #IMPLIED>
			<!ATTLIST disk sha256 CDATA #IMPLIED>
			<!ATTLIST disk sha384 CDATA #IMPLIED>
			<!ATTLIST disk sha512 CDATA #IMPLIED>
			<!ATTLIST disk merge CDATA #IMPLIED>
			<!ATTLIST disk status (baddump|nodump|good|verified) ""good"">
		<!ELEMENT sample EMPTY>
			<!ATTLIST sample name CDATA #REQUIRED>
		<!ELEMENT archive EMPTY>
			<!ATTLIST archive name CDATA #REQUIRED>
	<!ELEMENT machine (comment*, description, year?, manufacturer?, release*, biosset*, rom*, disk*, sample*, archive*)>
		<!ATTLIST machine name CDATA #REQUIRED>
		<!ATTLIST machine sourcefile CDATA #IMPLIED>
		<!ATTLIST machine isbios (yes|no) ""no"">
		<!ATTLIST machine cloneof CDATA #IMPLIED>
		<!ATTLIST machine romof CDATA #IMPLIED>
		<!ATTLIST machine sampleof CDATA #IMPLIED>
		<!ATTLIST machine board CDATA #IMPLIED>
		<!ATTLIST machine rebuildto CDATA #IMPLIED>
";
		public const string MAMEDTD = @"<!ELEMENT mame (machine+)>
	<!ATTLIST mame build CDATA #IMPLIED>
	<!ATTLIST mame debug (yes|no) ""no"">
	<!ATTLIST mame mameconfig CDATA #REQUIRED>
	<!ELEMENT machine (description, year?, manufacturer?, biosset*, rom*, disk*, device_ref*, sample*, chip*, display*, sound?, input?, dipswitch*, configuration*, port*, adjuster*, driver?, feature*, device*, slot*, softwarelist*, ramoption*)>
		<!ATTLIST machine name CDATA #REQUIRED>
		<!ATTLIST machine sourcefile CDATA #IMPLIED>
		<!ATTLIST machine isbios (yes|no) ""no"">
		<!ATTLIST machine isdevice (yes|no) ""no"">
		<!ATTLIST machine ismechanical (yes|no) ""no"">
		<!ATTLIST machine runnable (yes|no) ""yes"">
		<!ATTLIST machine cloneof CDATA #IMPLIED>
		<!ATTLIST machine romof CDATA #IMPLIED>
		<!ATTLIST machine sampleof CDATA #IMPLIED>
		<!ELEMENT description (#PCDATA)>
		<!ELEMENT year (#PCDATA)>
		<!ELEMENT manufacturer (#PCDATA)>
		<!ELEMENT biosset EMPTY>
			<!ATTLIST biosset name CDATA #REQUIRED>
			<!ATTLIST biosset description CDATA #REQUIRED>
			<!ATTLIST biosset default (yes|no) ""no"">
		<!ELEMENT rom EMPTY>
			<!ATTLIST rom name CDATA #REQUIRED>
			<!ATTLIST rom bios CDATA #IMPLIED>
			<!ATTLIST rom size CDATA #REQUIRED>
			<!ATTLIST rom crc CDATA #IMPLIED>
			<!ATTLIST rom md5 CDATA #IMPLIED>
			<!ATTLIST rom sha1 CDATA #IMPLIED>
			<!ATTLIST rom sha256 CDATA #IMPLIED>
			<!ATTLIST rom sha384 CDATA #IMPLIED>
			<!ATTLIST rom sha512 CDATA #IMPLIED>
			<!ATTLIST rom merge CDATA #IMPLIED>
			<!ATTLIST rom region CDATA #IMPLIED>
			<!ATTLIST rom offset CDATA #IMPLIED>
			<!ATTLIST rom status (baddump|nodump|good) ""good"">
			<!ATTLIST rom optional (yes|no) ""no"">
		<!ELEMENT disk EMPTY>
			<!ATTLIST disk name CDATA #REQUIRED>
			<!ATTLIST disk md5 CDATA #IMPLIED>
			<!ATTLIST disk sha1 CDATA #IMPLIED>
			<!ATTLIST disk sha256 CDATA #IMPLIED>
			<!ATTLIST disk sha384 CDATA #IMPLIED>
			<!ATTLIST disk sha512 CDATA #IMPLIED>
			<!ATTLIST disk merge CDATA #IMPLIED>
			<!ATTLIST disk region CDATA #IMPLIED>
			<!ATTLIST disk index CDATA #IMPLIED>
			<!ATTLIST disk writable (yes|no) ""no"">
			<!ATTLIST disk status (baddump|nodump|good) ""good"">
			<!ATTLIST disk optional (yes|no) ""no"">
		<!ELEMENT device_ref EMPTY>
			<!ATTLIST device_ref name CDATA #REQUIRED>
		<!ELEMENT sample EMPTY>
			<!ATTLIST sample name CDATA #REQUIRED>
		<!ELEMENT chip EMPTY>
			<!ATTLIST chip name CDATA #REQUIRED>
			<!ATTLIST chip tag CDATA #IMPLIED>
			<!ATTLIST chip type (cpu|audio) #REQUIRED>
			<!ATTLIST chip clock CDATA #IMPLIED>
		<!ELEMENT display EMPTY>
			<!ATTLIST display tag CDATA #IMPLIED>
			<!ATTLIST display type (raster|vector|lcd|svg|unknown) #REQUIRED>
			<!ATTLIST display rotate (0|90|180|270) #IMPLIED>
			<!ATTLIST display flipx (yes|no) ""no"">
			<!ATTLIST display width CDATA #IMPLIED>
			<!ATTLIST display height CDATA #IMPLIED>
			<!ATTLIST display refresh CDATA #REQUIRED>
			<!ATTLIST display pixclock CDATA #IMPLIED>
			<!ATTLIST display htotal CDATA #IMPLIED>
			<!ATTLIST display hbend CDATA #IMPLIED>
			<!ATTLIST display hbstart CDATA #IMPLIED>
			<!ATTLIST display vtotal CDATA #IMPLIED>
			<!ATTLIST display vbend CDATA #IMPLIED>
			<!ATTLIST display vbstart CDATA #IMPLIED>
		<!ELEMENT sound EMPTY>
			<!ATTLIST sound channels CDATA #REQUIRED>
		<!ELEMENT condition EMPTY>
			<!ATTLIST condition tag CDATA #REQUIRED>
			<!ATTLIST condition mask CDATA #REQUIRED>
			<!ATTLIST condition relation (eq|ne|gt|le|lt|ge) #REQUIRED>
			<!ATTLIST condition value CDATA #REQUIRED>
		<!ELEMENT input (control*)>
			<!ATTLIST input service (yes|no) ""no"">
			<!ATTLIST input tilt (yes|no) ""no"">
			<!ATTLIST input players CDATA #REQUIRED>
			<!ATTLIST input coins CDATA #IMPLIED>
			<!ELEMENT control EMPTY>
				<!ATTLIST control type CDATA #REQUIRED>
				<!ATTLIST control player CDATA #IMPLIED>
				<!ATTLIST control buttons CDATA #IMPLIED>
				<!ATTLIST control reqbuttons CDATA #IMPLIED>
				<!ATTLIST control minimum CDATA #IMPLIED>
				<!ATTLIST control maximum CDATA #IMPLIED>
				<!ATTLIST control sensitivity CDATA #IMPLIED>
				<!ATTLIST control keydelta CDATA #IMPLIED>
				<!ATTLIST control reverse (yes|no) ""no"">
				<!ATTLIST control ways CDATA #IMPLIED>
				<!ATTLIST control ways2 CDATA #IMPLIED>
				<!ATTLIST control ways3 CDATA #IMPLIED>
		<!ELEMENT dipswitch (condition?, diplocation*, dipvalue*)>
			<!ATTLIST dipswitch name CDATA #REQUIRED>
			<!ATTLIST dipswitch tag CDATA #REQUIRED>
			<!ATTLIST dipswitch mask CDATA #REQUIRED>
			<!ELEMENT diplocation EMPTY>
				<!ATTLIST diplocation name CDATA #REQUIRED>
				<!ATTLIST diplocation number CDATA #REQUIRED>
				<!ATTLIST diplocation inverted (yes|no) ""no"">
			<!ELEMENT dipvalue (condition?)>
				<!ATTLIST dipvalue name CDATA #REQUIRED>
				<!ATTLIST dipvalue value CDATA #REQUIRED>
				<!ATTLIST dipvalue default (yes|no) ""no"">
		<!ELEMENT configuration (condition?, conflocation*, confsetting*)>
			<!ATTLIST configuration name CDATA #REQUIRED>
			<!ATTLIST configuration tag CDATA #REQUIRED>
			<!ATTLIST configuration mask CDATA #REQUIRED>
			<!ELEMENT conflocation EMPTY>
				<!ATTLIST conflocation name CDATA #REQUIRED>
				<!ATTLIST conflocation number CDATA #REQUIRED>
				<!ATTLIST conflocation inverted (yes|no) ""no"">
			<!ELEMENT confsetting (condition?)>
				<!ATTLIST confsetting name CDATA #REQUIRED>
				<!ATTLIST confsetting value CDATA #REQUIRED>
				<!ATTLIST confsetting default (yes|no) ""no"">
		<!ELEMENT port (analog*)>
			<!ATTLIST port tag CDATA #REQUIRED>
			<!ELEMENT analog EMPTY>
				<!ATTLIST analog mask CDATA #REQUIRED>
		<!ELEMENT adjuster (condition?)>
			<!ATTLIST adjuster name CDATA #REQUIRED>
			<!ATTLIST adjuster default CDATA #REQUIRED>
		<!ELEMENT driver EMPTY>
			<!ATTLIST driver status (good|imperfect|preliminary) #REQUIRED>
			<!ATTLIST driver emulation (good|imperfect|preliminary) #REQUIRED>
			<!ATTLIST driver cocktail (good|imperfect|preliminary) #IMPLIED>
			<!ATTLIST driver savestate (supported|unsupported) #REQUIRED>
		<!ELEMENT feature EMPTY>
			<!ATTLIST feature type (protection|palette|graphics|sound|controls|keyboard|mouse|microphone|camera|disk|printer|lan|wan|timing) #REQUIRED>
			<!ATTLIST feature status (unemulated|imperfect) #IMPLIED>
			<!ATTLIST feature overall (unemulated|imperfect) #IMPLIED>
		<!ELEMENT device (instance?, extension*)>
			<!ATTLIST device type CDATA #REQUIRED>
			<!ATTLIST device tag CDATA #IMPLIED>
			<!ATTLIST device fixed_image CDATA #IMPLIED>
			<!ATTLIST device mandatory CDATA #IMPLIED>
			<!ATTLIST device interface CDATA #IMPLIED>
			<!ELEMENT instance EMPTY>
				<!ATTLIST instance name CDATA #REQUIRED>
				<!ATTLIST instance briefname CDATA #REQUIRED>
			<!ELEMENT extension EMPTY>
				<!ATTLIST extension name CDATA #REQUIRED>
		<!ELEMENT slot (slotoption*)>
			<!ATTLIST slot name CDATA #REQUIRED>
			<!ELEMENT slotoption EMPTY>
				<!ATTLIST slotoption name CDATA #REQUIRED>
				<!ATTLIST slotoption devname CDATA #REQUIRED>
				<!ATTLIST slotoption default (yes|no) ""no"">
		<!ELEMENT softwarelist EMPTY>
			<!ATTLIST softwarelist name CDATA #REQUIRED>
			<!ATTLIST softwarelist status (original|compatible) #REQUIRED>
			<!ATTLIST softwarelist filter CDATA #IMPLIED>
		<!ELEMENT ramoption (#PCDATA)>
			<!ATTLIST ramoption default CDATA #IMPLIED>
";
		public const string SoftwareListDTD = @"<!ELEMENT softwarelist (software+)>
	<!ATTLIST softwarelist name CDATA #REQUIRED>
	<!ATTLIST softwarelist description CDATA #IMPLIED>
	<!ELEMENT software (description, year, publisher, info*, sharedfeat*, part*)>
		<!ATTLIST software name CDATA #REQUIRED>
		<!ATTLIST software cloneof CDATA #IMPLIED>
		<!ATTLIST software supported (yes|partial|no) ""yes"">
		<!ELEMENT description (#PCDATA)>
		<!ELEMENT year (#PCDATA)>
		<!ELEMENT publisher (#PCDATA)>
		<!ELEMENT info EMPTY>
			<!ATTLIST info name CDATA #REQUIRED>
			<!ATTLIST info value CDATA #IMPLIED>
		<!ELEMENT sharedfeat EMPTY>
			<!ATTLIST sharedfeat name CDATA #REQUIRED>
			<!ATTLIST sharedfeat value CDATA #IMPLIED>
		<!ELEMENT part (feature*, dataarea*, diskarea*, dipswitch*)>
			<!ATTLIST part name CDATA #REQUIRED>
			<!ATTLIST part interface CDATA #REQUIRED>
			<!-- feature is used to store things like pcb-type, mapper type, etc. Specific values depend on the system. -->
			<!ELEMENT feature EMPTY>
				<!ATTLIST feature name CDATA #REQUIRED>
				<!ATTLIST feature value CDATA #IMPLIED>
			<!ELEMENT dataarea (rom*)>
				<!ATTLIST dataarea name CDATA #REQUIRED>
				<!ATTLIST dataarea size CDATA #REQUIRED>
				<!ATTLIST dataarea width (8|16|32|64) ""8"">
				<!ATTLIST dataarea endianness (big|little) ""little"">
				<!ELEMENT rom EMPTY>
					<!ATTLIST rom name CDATA #IMPLIED>
					<!ATTLIST rom size CDATA #IMPLIED>
					<!ATTLIST rom crc CDATA #IMPLIED>
					<!ATTLIST rom md5 CDATA #IMPLIED>
					<!ATTLIST rom sha1 CDATA #IMPLIED>
					<!ATTLIST rom sha256 CDATA #IMPLIED>
					<!ATTLIST rom sha384 CDATA #IMPLIED>
					<!ATTLIST rom sha512 CDATA #IMPLIED>
					<!ATTLIST rom offset CDATA #IMPLIED>
					<!ATTLIST rom value CDATA #IMPLIED>
					<!ATTLIST rom status (baddump|nodump|good) ""good"">
					<!ATTLIST rom loadflag (load16_byte|load16_word|load16_word_swap|load32_byte|load32_word|load32_word_swap|load32_dword|load64_word|load64_word_swap|reload|fill|continue|reload_plain|ignore) #IMPLIED>
			<!ELEMENT diskarea (disk*)>
				<!ATTLIST diskarea name CDATA #REQUIRED>
				<!ELEMENT disk EMPTY>
					<!ATTLIST disk name CDATA #REQUIRED>
					<!ATTLIST disk md5 CDATA #IMPLIED>
					<!ATTLIST disk sha1 CDATA #IMPLIED>
					<!ATTLIST disk sha256 CDATA #IMPLIED>
					<!ATTLIST disk sha384 CDATA #IMPLIED>
					<!ATTLIST disk sha512 CDATA #IMPLIED>
					<!ATTLIST disk status (baddump|nodump|good) ""good"">
					<!ATTLIST disk writeable (yes|no) ""no"">
			<!ELEMENT dipswitch (dipvalue*)>
				<!ATTLIST dipswitch name CDATA #REQUIRED>
				<!ATTLIST dipswitch tag CDATA #REQUIRED>
				<!ATTLIST dipswitch mask CDATA #REQUIRED>
				<!ELEMENT dipvalue EMPTY>
					<!ATTLIST dipvalue name CDATA #REQUIRED>
					<!ATTLIST dipvalue value CDATA #REQUIRED>
					<!ATTLIST dipvalue default (yes|no) ""no"">
";

		#endregion

		#region Hash string length constants

		public const int CRCLength = 8;
		public const int MD5Length = 32;
		public const int SHA1Length = 40;
		public const int SHA256Length = 64;
		public const int SHA384Length = 96;
		public const int SHA512Length = 128;

		#endregion

		#region Magic numbers as strings

		public const string SevenZipSig = "377ABCAF271C";
		public const string GzSig = "1F8B";
		public const string RarSig = "526172211A0700";
		public const string RarFiveSig = "526172211A070100";
		public const string TarSig = "7573746172202000";
		public const string TarZeroSig = "7573746172003030";
		public const string ZipSig = "504B0304";
		public const string ZipSigEmpty = "504B0506";
		public const string ZipSigSpanned = "504B0708";

		#endregion

		#region Regular Expressions

		public const string XmlPattern = @"<(.*?)>(.*?)</(.*?)>";
		public const string HeaderPatternCMP = @"(^.*?) \($";
		public const string ItemPatternCMP = @"^\s*(\S*?) (.*)";
		public const string EndPatternCMP = @"^\s*\)\s*$";

		#endregion

		#region TorrentZip, T7z, and TGZ headers

		/* TorrentZip Header Format
			https://pkware.cachefly.net/webdocs/APPNOTE/APPNOTE_6.2.0.txt
			http://www.romvault.com/trrntzip_explained.doc

			00-03		Local file header signature (0x50, 0x4B, 0x03, 0x04)
			04-05		Version needed to extract (0x14, 0x00)
			06-07		General purpose bit flag (0x02, 0x00)
			08-09		Compression method (0x08, 0x00)
			0A-0B		Last mod file time (0x00, 0xBC)
			0C-0D		Last mod file date (0x98, 0x21)
		*/
		public readonly static byte[] TorrentZipHeader = new byte[] { 0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x02, 0x00, 0x08, 0x00, 0x00, 0xbc, 0x98, 0x21 };

		/* Torrent7z Header Format
			http://cpansearch.perl.org/src/BJOERN/Compress-Deflate7-1.0/7zip/DOC/7zFormat.txt

			00-05		Local file header signature (0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C)
			06-07		ArchiveVersion (0x00, 0x03)
			The rest is unknown
		*/
		public readonly static byte[] Torrent7ZipHeader = new byte[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c, 0x00, 0x03 };
		public readonly static byte[] Torrent7ZipSignature = new byte[] { 0xa9, 0xa9, 0x9f, 0xd1, 0x57, 0x08, 0xa9, 0xd7, 0xea, 0x29, 0x64, 0xb2,
			0x36, 0x1b, 0x83, 0x52, 0x33, 0x00, 0x74, 0x6f, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x37, 0x7a, 0x5f, 0x30, 0x2e, 0x39, 0x62, 0x65, 0x74, 0x61 };

		/* (Torrent)GZ Header Format
			https://tools.ietf.org/html/rfc1952

			00			Identification 1 (0x1F)
			01			Identification 2 (0x8B)
			02			Compression Method (0-7 reserved, 8 deflate; 0x08)
			03			Flags (0 FTEXT, 1 FHCRC, 2 FEXTRA, 3 FNAME, 4 FCOMMENT, 5 reserved, 6 reserved, 7 reserved; 0x04)
			04-07		Modification time (Unix format; 0x00, 0x00, 0x00, 0x00)
			08			Extra Flags (2 maximum compression, 4 fastest algorithm; 0x00)
			09			OS (See list on https://tools.ietf.org/html/rfc1952; 0x00)
			0A-0B		Length of extra field (mirrored; 0x1C, 0x00)
			0C-27		Extra field
				0C-1B	MD5 Hash
				1C-1F	CRC hash
				20-27	Int64 size (mirrored)
		*/
		public readonly static byte[] TorrentGZHeader = new byte[] { 0x1f, 0x8b, 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1c, 0x00 };

		#endregion

		#region ZIP internal signatures

		public const uint LocalFileHeaderSignature = 0x04034b50;
		public const uint EndOfLocalFileHeaderSignature = 0x08074b50;
		public const uint CentralDirectoryHeaderSignature = 0x02014b50;
		public const uint EndOfCentralDirSignature = 0x06054b50;
		public const uint Zip64EndOfCentralDirSignature = 0x06064b50;
		public const uint Zip64EndOfCentralDirectoryLocator = 0x07064b50;
		public const uint TorrentZipFileDateTime = 0x2198BC00;

		#endregion
	}
}
