using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
		/// <param name="overwrite">True if we ignore existing files (default), false otherwise</param>
		/// <returns>Dictionary of output formats mapped to file names</returns>
		public static Dictionary<OutputFormat, string> CreateOutfileNames(string outDir, DatFile datdata, bool overwrite = true)
		{
			// Create the output dictionary
			Dictionary<OutputFormat, string> outfileNames = new Dictionary<OutputFormat, string>();

			// Double check the outDir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// Get the extensions from the output type
			if ((datdata.OutputFormat & OutputFormat.Xml) != 0)
			{
				outfileNames.Add(OutputFormat.Xml, CreateOutfileNamesHelper(outDir, ".xml", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.ClrMamePro) != 0)
			{
				outfileNames.Add(OutputFormat.ClrMamePro, CreateOutfileNamesHelper(outDir, ".dat", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.RomCenter) != 0)
			{
				outfileNames.Add(OutputFormat.RomCenter, CreateOutfileNamesHelper(outDir, ".rc.dat", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.DOSCenter) != 0)
			{
				outfileNames.Add(OutputFormat.DOSCenter, CreateOutfileNamesHelper(outDir, ".dc.dat", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.MissFile) != 0)
			{
				outfileNames.Add(OutputFormat.MissFile, CreateOutfileNamesHelper(outDir, ".txt", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.SabreDat) != 0)
			{
				outfileNames.Add(OutputFormat.SabreDat, CreateOutfileNamesHelper(outDir, ".sd.xml", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.RedumpMD5) != 0)
			{
				outfileNames.Add(OutputFormat.RedumpMD5, CreateOutfileNamesHelper(outDir, ".md5", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.RedumpSHA1) != 0)
			{
				outfileNames.Add(OutputFormat.RedumpSHA1, CreateOutfileNamesHelper(outDir, ".sha1", datdata, overwrite));
			};
			if ((datdata.OutputFormat & OutputFormat.RedumpSFV) != 0)
			{
				outfileNames.Add(OutputFormat.RedumpSFV, CreateOutfileNamesHelper(outDir, ".sfv", datdata, overwrite));
			};

			return outfileNames;
		}

		/// <summary>
		/// Help generating the outfile name
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="extension">Extension to use for the file</param>
		/// <param name="datdata">DAT information</param>
		/// <param name="overwrite">True if we ignore existing files, false otherwise</param>
		/// <returns>String containing the new filename</returns>
		private static string CreateOutfileNamesHelper(string outDir, string extension, DatFile datdata, bool overwrite)
		{
			string filename = (String.IsNullOrEmpty(datdata.FileName) ? datdata.Description : datdata.FileName);
			string outfile = outDir + filename + extension;
			outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
				outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
				outfile);
			if (!overwrite)
			{
				int i = 1;
				while (File.Exists(outfile))
				{
					outfile = outDir + filename + "_" + i + extension;
					outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
						outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
						outfile);
					i++;
				}
			}

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

		/// <summary>
		/// https://psycodedeveloper.wordpress.com/2013/04/12/c-numeric-sorting-revisited/
		/// </summary>
		public static int CompareNumeric(string s, string other)
		{
			if (s != null && other != null &&
				(s = s.Replace(" ", string.Empty)).Length > 0 &&
				(other = other.Replace(" ", string.Empty)).Length > 0)
			{
				int sIndex = 0, otherIndex = 0;

				while (sIndex < s.Length)
				{
					if (otherIndex >= other.Length)
					{
						return 1;
					}

					if (char.IsDigit(s[sIndex]))
					{
						if (!char.IsDigit(other[otherIndex]))
						{
							return -1;
						}

						// Compare the numbers
						StringBuilder sBuilder = new StringBuilder(), otherBuilder = new StringBuilder();

						while (sIndex < s.Length && char.IsDigit(s[sIndex]))
						{
							sBuilder.Append(s[sIndex++]);
						}

						while (otherIndex < other.Length && char.IsDigit(other[otherIndex]))
						{
							otherBuilder.Append(other[otherIndex++]);
						}

						long sValue = 0L, otherValue = 0L;

						if (!Int64.TryParse(sBuilder.ToString(), out sValue))
						{
							sValue = Int64.MaxValue;
						}

						if (!Int64.TryParse(otherBuilder.ToString(), out otherValue))
						{
							otherValue = Int64.MaxValue;
						}

						if (sValue < otherValue)
						{
							return -1;
						}
						else if (sValue > otherValue)
						{
							return 1;
						}
					}
					else if (char.IsDigit(other[otherIndex]))
					{
						return 1;
					}
					else
					{
						int difference = string.Compare(s[sIndex].ToString(), other[otherIndex].ToString(), StringComparison.InvariantCultureIgnoreCase);

						if (difference > 0)
						{
							return 1;
						}
						else if (difference < 0)
						{
							return -1;
						}

						sIndex++;
						otherIndex++;
					}
				}

				if (otherIndex < other.Length)
				{
					return -1;
				}
			}

			return 0;
		}

		/// <summary>
		/// http://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames
		/// </summary>
		public static string StripInvalidPathChars(string s)
		{
			string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
			Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			s = r.Replace(s, "");
			return s;
		}

		/// <summary>
		/// http://stackoverflow.com/questions/5613279/c-sharp-hex-to-ascii
		/// </summary>
		public static string ConvertHexToAscii(String hexString)
		{
			if (hexString.Contains("-"))
			{
				hexString = hexString.Replace("-", "");
			}

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < hexString.Length; i += 2)
			{
				String hs = hexString.Substring(i, 2);
				sb.Append(Convert.ToChar(Convert.ToUInt32(hs, 16)));
			}

			return sb.ToString();
		}

		/// <summary>
		/// http://stackoverflow.com/questions/15920741/convert-from-string-ascii-to-string-hex
		/// </summary>
		public static string ConvertAsciiToHex(string asciiString)
		{
			string hexOutput = "";
			foreach (char _eachChar in asciiString.ToCharArray())
			{
				// Get the integral value of the character.
				int value = Convert.ToInt32(_eachChar);
				// Convert the decimal value to a hexadecimal value in string form.
				hexOutput += String.Format("{0:X2}", value).Remove(0, 2);
				// to make output as your eg 
				//  hexOutput +=" "+ String.Format("{0:X}", value);
			}

			return hexOutput;
		}

		/// <summary>
		/// https://github.com/gjefferyes/RomVault/blob/master/ROMVault2/SupportedFiles/Zip/zipFile.cs
		/// </summary>
		public static bool IsUnicode(string s)
		{
			char[] c = s.ToCharArray();
			for (int i = 0; i < c.Length; i++)
				if (c[i] > 255) return true;
			return false;
		}

		/// <summary>
		/// Determines a text file's encoding by analyzing its byte order mark (BOM).
		/// Defaults to ASCII when detection of the text file's endianness fails.
		/// http://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding
		/// </summary>
		/// <param name="filename">The text file to analyze.</param>
		/// <returns>The detected encoding.</returns>
		public static Encoding GetEncoding(string filename)
		{
			// Read the BOM
			var bom = new byte[4];
			using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				file.Read(bom, 0, 4);
			}

			// Analyze the BOM
			if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
			if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
			if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
			if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
			if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
			return Encoding.Default;
		}

		#endregion
	}
}
