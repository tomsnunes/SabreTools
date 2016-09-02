using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SabreTools.Helper
{
	/// <summary>
	/// Include character normalization and replacement mappings
	/// </summary>
	public class Style
	{
		#region WoD-based String Cleaning

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

		#endregion

		#region DAT Cleaning

		/// <summary>
		/// Generate a proper outfile name based on a DAT and output directory
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="datdata">DAT information</param>
		/// <returns>String representing the proper name</returns>
		public static string CreateOutfileName(string outDir, Dat datdata)
		{
			// Double check the outdir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// Get the extension from the output type
			string extension = "";
			switch(datdata.OutputFormat)
			{
				case OutputFormat.ClrMamePro:
				case OutputFormat.DOSCenter:
				case OutputFormat.RomCenter:
					extension = ".dat";
					break;
				case OutputFormat.MissFile:
					extension = ".txt";
					break;
				case OutputFormat.SabreDat:
				case OutputFormat.Xml:
					extension = ".xml";
					break;
			}
			string filename = (String.IsNullOrEmpty(datdata.FileName) ? datdata.Description : datdata.FileName);
			string outfile = outDir + filename + extension;
			outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
				outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
				outfile);

			Console.WriteLine(outfile);

			return outfile;
		}

		/// <summary>
		/// Clean a game (or rom) name to the WoD standard
		/// </summary>
		/// <param name="game">Name of the game to be cleaned</param>
		/// <returns>The cleaned name</returns>
		public static string CleanGameName(string game)
		{
			///Run the name through the filters to make sure that it's correct
			game = Style.NormalizeChars(game);
			game = Style.RussianToLatin(game);
			game = Style.SearchPattern(game);

			game = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(game).Groups[1].Value;
			game = game.TrimStart().TrimEnd();
			return game;
		}

		/// <summary>
		/// Clean a game (or rom) name to the WoD standard
		/// </summary>
		/// <param name="game">Array representing the path to be cleaned</param>
		/// <returns>The cleaned name</returns>
		public static string CleanGameName(string[] game)
		{
			game[game.Length - 1] = CleanGameName(game[game.Length - 1]);
			string outgame = String.Join(Path.DirectorySeparatorChar.ToString(), game);
			outgame = outgame.TrimStart().TrimEnd();
			return outgame;
		}

		/// <summary>
		/// Clean a hash string and pad to the correct size
		/// </summary>
		/// <param name="hash">Hash string to sanitize</param>
		/// <param name="padding">Amount of characters to pad to</param>
		/// <returns>Cleaned string</returns>
		public static string CleanHashData(string hash, int padding)
		{
			// First get the hash to the correct length
			hash = (String.IsNullOrEmpty(hash) ? "" : hash.Trim());
			hash = (hash.StartsWith("0x") ? hash.Remove(0, 2) : hash);
			hash = (hash == "-" ? "" : hash);
			hash = (String.IsNullOrEmpty(hash) ? "" : hash.PadLeft(padding, '0'));
			hash = hash.ToLowerInvariant();

			// Then make sure that it has the correct characters
			if (!Regex.IsMatch(hash, "[0-9a-f]{" + padding + "}"))
			{
				hash = "";
			}

			return hash;
		}

		/// <summary>
		/// Clean a hash byte array and pad to the correct size
		/// </summary>
		/// <param name="hash">Hash byte array to sanitize</param>
		/// <param name="padding">Amount of bytes to pad to</param>
		/// <returns>Cleaned byte array</returns>
		public static byte[] CleanHashData(byte[] hash, int padding)
		{
			// If we have a null hash or a <=0 padding, return the hash
			if (hash == null || padding <= 0)
			{
				return hash;
			}

			// If we have a hash longer than the padding, trim and return
			if (hash.Length > padding)
			{
				return hash.Take(padding).ToArray();
			}

			// If we have a hash of the correct length, return
			if (hash.Length == padding)
			{
				return hash;
			}

			// Otherwise get the output byte array of the correct length
			byte[] newhash = new byte[padding];

			// Then write the proper number of empty bytes
			int padNeeded = padding - hash.Length;
			int index = 0;
			for (index = 0; index < padNeeded; index++)
			{
				newhash[index] = 0x00;
			}

			// Now add the original hash
			for (int i = 0; i < hash.Length; i++)
			{
				newhash[index + i] = hash[index];
			}

			return newhash;
		}

		#endregion

		#region Externally sourced methods

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

		/// <summary>
		/// http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
		/// </summary>
		public static byte[] StringToByteArray(String hex)
		{
			int NumberChars = hex.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for (int i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}

		#endregion
	}
}
