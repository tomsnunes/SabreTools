using System;

namespace SabreTools.Helper
{
	public class Constants
	{
		/// <summary>
		/// The current toolset version to be used by all child applications
		/// </summary>
		public const string Version = "v0.9.3";
		public const int HeaderHeight = 3;

		#region 0-byte file constants

		public const long SizeZero = 0;
		public const string CRCZero = "00000000";
		public static byte[] CRCZeroBytes = new byte[] { 0x00, 0x00, 0x00, 0x00 };
		public const string MD5Zero = "d41d8cd98f00b204e9800998ecf8427e";
		public static byte[] MD5ZeroBytes = new byte[] { 0xd4, 0x1d, 0x8c, 0xd9, 0x8f, 0x00, 0xb2, 0x04, 0xe9, 0x80, 0x09, 0x98, 0xec, 0xf8, 0x42, 0x7e };
		public const string SHA1Zero = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
		public static byte[] SHA1ZeroBytes = new byte[] { 0xda, 0x39, 0xa3, 0xee, 0x5e, 0x6b, 0x4b, 0x0d, 0x32, 0x55, 0xbf, 0xef, 0x95, 0x60, 0x18, 0x90, 0xaf, 0xd8, 0x07, 0x09 };

		#endregion

		#region Hash string length constants

		public const int CRCLength = 8;
		public const int CRCBytesLength = 4;
		public const int MD5Length = 32;
		public const int MD5BytesLength = 16;
		public const int SHA1Length = 40;
		public const int SHA1BytesLength = 20;

		#endregion

		#region Regex File Name Patterns

		public const string DefaultPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.dat$";
		public const string DefaultSpecialPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.xml$";
		public const string GoodPattern = @"^(Good.*?)_.*\.dat";
		public const string GoodXmlPattern = @"^(Good.*?)_.*\.xml";
		public const string MamePattern = @"^(.*)\.xml$";
		public const string MaybeIntroPattern = @"(.*?) \[T-En\].*\((\d{8})\)\.dat$";
		public const string NoIntroPattern = @"^(.*?) \((\d{8}-\d{6})_CM\).*\.dat$";
		public const string NoIntroNumberedPattern = @"(.*? - .*?) \(\d.*?_CM\).*.dat";
		public const string NoIntroSpecialPattern = @"(.*? - .*?) \((\d{8})\).*\.dat";
		public const string NonGoodPattern = @"^(NonGood.*?)( .*?)?.xml";
		public const string NonGoodSpecialPattern = @"^(NonGood.*?)( .*)?.dat";
		public const string RedumpPattern = @"^(.*?) \((\d{8} \d{2}-\d{2}-\d{2})\)\.dat$";
		public const string RedumpBiosPattern = @"^(.*?) \(\d+\) \((\d{4}-\d{2}-\d{2})\)\.dat$";
		public const string TosecPattern = @"^(.*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\).*\.dat$";
		public const string TosecSpecialPatternA = @"^(.*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\).*\.dat$";
		public const string TosecSpecialPatternB = @"^(.*? - .*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\).*\.dat$";
		public const string TruripPattern = @"^(.*) - .* \(trurip_XML\)\.dat$";
		public const string ZandroPattern = @"^SMW-.*.xml";

		#endregion

		#region Regex Mapped Name Patterns

		public const string RemappedPattern = @"^(.*) - (.*)$";

		#endregion

		#region Regex Date Patterns

		public const string DefaultDatePattern = @"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})";
		public const string NoIntroDatePattern = @"(\d{4})(\d{2})(\d{2})-(\d{2})(\d{2})(\d{2})";
		public const string NoIntroSpecialDatePattern = @"(\d{4})(\d{2})(\d{2})";
		public const string RedumpDatePattern = @"(\d{4})(\d{2})(\d{2}) (\d{2})-(\d{2})-(\d{2})";
		public const string TosecDatePattern = @"(\d{4})-(\d{2})-(\d{2})";

		#endregion

		#region Regex conversion patterns

		public const string HeaderPatternCMP = @"(^.*?) \($";
		public const string ItemPatternCMP = @"^\s*(\S*?) (.*)";
		public const string EndPatternCMP = @"^\s*\)\s*$";

		#endregion

		#region Byte (1024-based) size comparisons

		public const long KibiByte = 1024;
		public const long MibiByte = (long)Math.Pow(KibiByte, 2);
		public const long GibiByte = (long)Math.Pow(KibiByte, 3);
		public const long TibiByte = (long)Math.Pow(KibiByte, 4);
		public const long PibiByte = (long)Math.Pow(KibiByte, 5);

		#endregion

		#region Byte (1000-based) size comparisons

		public const long KiloByte = 1000;
		public const long MegaByte = (long)Math.Pow(KiloByte, 2);
		public const long GigaByte = (long)Math.Pow(KiloByte, 2);
		public const long TeraByte = (long)Math.Pow(KiloByte, 2);
		public const long PetaByte = (long)Math.Pow(KiloByte, 2);

		#endregion

		#region Magic numbers as byte arrays

		public const byte[] SevenZipSigBytes = new byte[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c };
		public const byte[] GzSigBytes = new byte[] { 0x1f, 0x8b };
		public const byte[] RarSigBytes = new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x00 };
		public const byte[] RarFiveSigBytes = new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x01, 0x00 };
		public const byte[] TarSigBytes = new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20, 0x00 };
		public const byte[] TarZeroSigBytes = new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30, 0x30 };
		public const byte[] ZipSigBytes = new byte[] { 0x50, 0x4b, 0x03, 0x04 };
		public const byte[] ZipSigEmptyBytes = new byte[] { 0x50, 0x4b, 0x05, 0x06 };
		public const byte[] ZipSigSpannedBytes = new byte[] { 0x50, 0x4b, 0x07, 0x08 };

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
		public const byte[] TorrentZipHeader = new byte[] { 0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x02, 0x00, 0x08, 0x00, 0x00, 0xbc, 0x98, 0x21 };

		/* Torrent7z Header Format
			http://cpansearch.perl.org/src/BJOERN/Compress-Deflate7-1.0/7zip/DOC/7zFormat.txt

			00-05		Local file header signature (0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C)
			06-07		ArchiveVersion (0x00, 0x03)
			The rest is unknown
		*/
		public const byte[] Torrent7ZipHeader = new byte[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c, 0x00, 0x03 };

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
		public const byte[] TorrentGZHeader = new byte[] { 0x1f, 0x8b, 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1c, 0x00 };

		#endregion

		#region ZIP internal signatures

		public const uint LocalFileHeaderSignature = 0x04034b50;
		public const uint EndOfLocalFileHeaderSignature = 0x08074b50;
		public const uint CentralDirectoryHeaderSignature = 0x02014b50;
		public const uint EndOfCentralDirSignature = 0x06054b50;
		public const uint Zip64EndOfCentralDirSignatue = 0x06064b50;
		public const uint Zip64EndOfCentralDirectoryLocator = 0x07064b50;

		#endregion

		#region Database schema

		public const string DATabaseDbSchema = "dats";
		public const string DATabaseFileName = "dats.sqlite";
		public const string DATabaseConnectionString = "Data Source=" + DATabaseFileName + ";Version = 3;";
		public const string HeadererDbSchema = "Headerer";
		public const string HeadererFileName = "Headerer.sqlite";
		public const string HeadererConnectionString = "Data Source=" + HeadererFileName + ";Version = 3;";

		#endregion
	}
}
