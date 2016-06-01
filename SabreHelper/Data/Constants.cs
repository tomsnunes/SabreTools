using System;

namespace SabreTools.Helper
{
	public class Constants
	{
		/// <summary>
		/// The current toolset version to be used by all child applications
		/// </summary>
		public static string Version = "v0.8.3";

		// 0-byte file constants
		public static long SizeZero = 0;
		public static string CRCZero = "00000000";
		public static string MD5Zero = "d41d8cd98f00b204e9800998ecf8427e";
		public static string SHA1Zero = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

		// Regex File Name Patterns
		public static string DefaultPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.dat$";
		public static string DefaultSpecialPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.xml$";
		public static string GoodPattern = @"^(Good.*?)_.*\.dat";
		public static string GoodXmlPattern = @"^(Good.*?)_.*\.xml";
		public static string MamePattern = @"^(.*)\.xml$";
		public static string MaybeIntroPattern = @"(.*?) \[T-En\].*\((\d{8})\)\.dat$";
		public static string NoIntroPattern = @"^(.*?) \((\d{8}-\d{6})_CM\).*\.dat$";
		public static string NoIntroNumberedPattern = @"(.*? - .*?) \(\d.*?_CM\).*.dat";
		public static string NoIntroSpecialPattern = @"(.*? - .*?) \((\d{8})\).*\.dat";
		public static string NonGoodPattern = @"^(NonGood.*?)( .*?)?.xml";
		public static string NonGoodSpecialPattern = @"^(NonGood.*?)( .*)?.dat";
		public static string RedumpPattern = @"^(.*?) \((\d{8} \d{2}-\d{2}-\d{2})\)\.dat$";
		public static string RedumpBiosPattern = @"^(.*?) \(\d+\) \((\d{4}-\d{2}-\d{2})\)\.dat$";
		public static string TosecPattern = @"^(.*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\).*\.dat$";
		public static string TosecSpecialPatternA = @"^(.*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\).*\.dat$";
		public static string TosecSpecialPatternB = @"^(.*? - .*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\).*\.dat$";
		public static string TruripPattern = @"^(.*) - .* \(trurip_XML\)\.dat$";
		public static string ZandroPattern = @"^SMW-.*.xml";

		// Regex Mapped Name Patterns
		public static string RemappedPattern = @"^(.*) - (.*)$";

		// Regex Date Patterns
		public static string DefaultDatePattern = @"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})";
		public static string NoIntroDatePattern = @"(\d{4})(\d{2})(\d{2})-(\d{2})(\d{2})(\d{2})";
		public static string NoIntroSpecialDatePattern = @"(\d{4})(\d{2})(\d{2})";
		public static string RedumpDatePattern = @"(\d{4})(\d{2})(\d{2}) (\d{2})-(\d{2})-(\d{2})";
		public static string TosecDatePattern = @"(\d{4})-(\d{2})-(\d{2})";

		// Regex conversion patterns
		public static string HeaderPatternCMP = @"(^.*?) \($";
		public static string ItemPatternCMP = @"^\s*(\S*?) (.*)";
		public static string EndPatternCMP = @"^\s*\)\s*$";

		// Byte (1024-based) size comparisons
		public static long KibiByte = 1024;
		public static long MibiByte = (long)Math.Pow(KibiByte, 2);
		public static long GibiByte = (long)Math.Pow(KibiByte, 3);
		public static long TibiByte = (long)Math.Pow(KibiByte, 4);
		public static long PibiByte = (long)Math.Pow(KibiByte, 5);

		// Byte (1000-based) size comparisons
		public static long KiloByte = 1000;
		public static long MegaByte = (long)Math.Pow(KiloByte, 2);
		public static long GigaByte = (long)Math.Pow(KiloByte, 2);
		public static long TeraByte = (long)Math.Pow(KiloByte, 2);
		public static long PetaByte = (long)Math.Pow(KiloByte, 2);
	}
}
