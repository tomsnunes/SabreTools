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

		// http://stackoverflow.com/questions/203528/what-is-the-simplest-way-to-get-indented-xml-with-line-breaks-from-xmldocument
		// http://www.timvw.be/2007/01/08/generating-utf-8-with-systemxmlxmlwriter/
		public static string Beautify(XmlDocument doc)
		{
			MemoryStream ms = new MemoryStream();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Encoding = new UTF8Encoding(false),
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace
			};

			using (XmlWriter writer = XmlWriter.Create(ms, settings))
			{
				doc.Save(writer);
			}
			return Encoding.UTF8.GetString(ms.ToArray());
		}
	}
}
