using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SabreTools.Helper
{
	/// <summary>
	/// Include character normalization and replacement mappings
	/// </summary>
	public class Style
	{
		/// <summary>
		/// Replace accented characters
		/// </summary>
		/// <param name="input">String to be parsed</param>
		/// <returns>String with characters replaced</returns>
		public static string NormalizeChars(string input)
		{
			string[,] charmap = {
				{ "Á", "A" },   { "á", "a" },
				{ "À", "A" },   { "à", "a" },
				{ "Â", "A" },   { "â", "a" },
				{ "Ä", "Ae" },  { "ä", "ae" },
				{ "Ã", "A" },   { "ã", "a" },
				{ "Å", "A" },   { "å", "a" },
				{ "Æ", "Ae" },  { "æ", "ae" },
				{ "Ç", "C" },   { "ç", "c" },
				{ "Ð", "D" },   { "ð", "d" },
				{ "É", "E" },   { "é", "e" },
				{ "È", "E" },   { "è", "e" },
				{ "Ê", "E" },   { "ê", "e" },
				{ "Ë", "E" },   { "ë", "e" },
				{ "ƒ", "f" },
				{ "Í", "I" },   { "í", "i" },
				{ "Ì", "I" },   { "ì", "i" },
				{ "Î", "I" },   { "î", "i" },
				{ "Ï", "I" },   { "ï", "i" },
				{ "Ñ", "N" },   { "ñ", "n" },
				{ "Ó", "O" },   { "ó", "o" },
				{ "Ò", "O" },   { "ò", "o" },
				{ "Ô", "O" },   { "ô", "o" },
				{ "Ö", "Oe" },  { "ö", "oe" },
				{ "Õ", "O" },   { "õ", "o" },
				{ "Ø", "O" },   { "ø", "o" },
				{ "Š", "S" },   { "š", "s" },
				{ "ß", "ss" },
				{ "Þ", "B" },   { "þ", "b" },
				{ "Ú", "U" },   { "ú", "u" },
				{ "Ù", "U" },   { "ù", "u" },
				{ "Û", "U" },   { "û", "u" },
				{ "Ü", "Ue" },  { "ü", "ue" },
				{ "ÿ", "y" },
				{ "Ý", "Y" },   { "ý", "y" },
				{ "Ž", "Z" },   { "ž", "z" },
			};

			for (int i = 0; i < charmap.GetLength(0); i++)
			{
				input = input.Replace(charmap[i, 0], charmap[i, 1]);
			}

			return input;
		}

		/// <summary>
		/// Replace special characters and patterns
		/// </summary>
		/// <param name="input">String to be parsed</param>
		/// <returns>String with characters replaced</returns>
		public static string SearchPattern(string input)
		{
			string[,] charmap = {
				{ @"~", " - " },
				{ @"_", " " },
				{ @":", " " },
				{ @">", ")" },
				{ @"<", "(" },
				{ @"\|", "-" },
				{ "\"", "'" },
				{ @"\*", "." },
				{ @"\\", "-" },
				{ @"/", "-" },
				{ @"\?", " " },
				{ @"\(([^)(]*)\(([^)]*)\)([^)(]*)\)", " " },
				{ @"\(([^)]+)\)", " " },
				{ @"\[([^]]+)\]", " " },
				{ @"\{([^}]+)\}", " " },
				{ @"(ZZZJUNK|ZZZ-UNK-|ZZZ-UNK |zzz unknow |zzz unk |Copy of |[.][a-z]{3}[.][a-z]{3}[.]|[.][a-z]{3}[.])", " " },
				{ @" (r|rev|v|ver)\s*[\d\.]+[^\s]*", " " },
				{ @"(( )|(\A))(\d{6}|\d{8})(( )|(\Z))", " " },
				{ @"(( )|(\A))(\d{1,2})-(\d{1,2})-(\d{4}|\d{2})", " " },
				{ @"(( )|(\A))(\d{4}|\d{2})-(\d{1,2})-(\d{1,2})", " " },
				{ @"[-]+", "-" },
				{ @"\A\s*\)", " " },
				{ @"\A\s*(,|-)", " " },
				{ @"\s+", " " },
				{ @"\s+,", "," },
				{ @"\s*(,|-)\s*\Z", " " },
			};

			for (int i = 0; i < charmap.GetLength(0); i++)
			{
				input = Regex.Replace(input, charmap[i, 0], charmap[i, 1]);
			}

			return input;
		}

		/// <summary>
		/// Convert Cyrillic lettering to Latin lettering
		/// </summary>
		/// <param name="input">String to be parsed</param>
		/// <returns>String with characters replaced</returns>
		public static string RussianToLatin(string input)
		{
			string [,] charmap = {
					{ "А", "A" }, { "Б", "B" }, { "В", "V" }, { "Г", "G" }, { "Д", "D" },
					{ "Е", "E" }, { "Ё", "Yo" }, { "Ж", "Zh" }, { "З", "Z" }, { "И", "I" },
					{ "Й", "J" }, { "К", "K" }, { "Л", "L" }, { "М", "M" }, { "Н", "N" },
					{ "О", "O" }, { "П", "P" }, { "Р", "R" }, { "С", "S" }, { "Т", "T" },
					{ "У", "U" }, { "Ф", "f" }, { "Х", "Kh" }, { "Ц", "Ts" }, { "Ч", "Ch" },
					{ "Ш", "Sh" }, { "Щ", "Sch" }, { "Ъ", "" }, { "Ы", "y" }, { "Ь", "" },
					{ "Э", "e" }, { "Ю", "yu" }, { "Я", "ya" }, { "а", "a" }, { "б", "b" },
					{ "в", "v" }, { "г", "g" }, { "д", "d" }, { "е", "e" }, { "ё", "yo" },
					{ "ж", "zh" }, { "з", "z" }, { "и", "i" }, { "й", "j" }, { "к", "k" },
					{ "л", "l" }, { "м", "m" }, { "н", "n" }, { "о", "o" }, { "п", "p" },
					{ "р", "r" }, { "с", "s" }, { "т", "t" }, { "у", "u" }, { "ф", "f" },
					{ "х", "kh" }, { "ц", "ts" }, { "ч", "ch" }, { "ш", "sh" }, { "щ", "sch" },
					{ "ъ", "" }, { "ы", "y" }, { "ь", "" }, { "э", "e" }, { "ю", "yu" },
					{ "я", "ya" },
			};

			for (int i = 0; i < charmap.GetLength(0); i++)
			{
				input = input.Replace(charmap[i, 0], charmap[i, 1]);
			}

			return input;
		}

		/// <summary>
		/// Generate a proper outfile name based on a DAT and output directory
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="datdata">DAT information</param>
		/// <returns>String representing the proper name</returns>
		public static string CreateOutfileName(string outDir, DatData datdata)
		{
			// Double check the outdir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			string filename = (String.IsNullOrEmpty(datdata.FileName) ? datdata.Description : datdata.FileName);
			string outfile = outDir + filename + (datdata.OutputFormat == OutputFormat.Xml || datdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
			outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
				outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
				outfile);

			Console.WriteLine(outfile);

			return outfile;
		}

		/// <summary>
		///  Returns the human-readable file size for an arbitrary, 64-bit file size 
		/// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Human-readable file size</returns>
		/// <link>http://www.somacon.com/p576.php</link>
		public static string GetBytesReadable(long input)
		{
			// Get absolute value
			long absolute_i = (input < 0 ? -input : input);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (input >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (input >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (input >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (input >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (input >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = input;
			}
			else
			{
				return input.ToString("0 B"); // Byte
			}
			// Divide by 1024 to get fractional value
			readable = (readable / 1024);
			// Return formatted number with suffix
			return readable.ToString("0.### ") + suffix;
		}

		/// <summary>
		/// Converts a string to sentence case.
		/// </summary>
		/// <param name="input">The string to convert.</param>
		/// <returns>A string representing a sentence case string</returns>
		/// <remarks>http://stackoverflow.com/questions/3141426/net-method-to-convert-a-string-to-sentence-case</remarks>
		public static string SentenceCase(string input)
		{
			if (input.Length < 1)
			{
				return input;
			}

			string sentence = input.ToLower();
			return sentence[0].ToString().ToUpper() + sentence.Substring(1);
		}
	}
}
