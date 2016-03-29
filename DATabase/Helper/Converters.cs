using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SabreTools.Helper
{
	/// <summary>
	/// Provide DAT conversion functionality
	/// </summary>
	class Converters
	{
		// Regex matching patterns
		private static string _headerPattern = @"(^.*?) \($";
		private static string _romPattern = @"^\s+((?:rom)|(?:disk)) \( (name) ""(.*?)"" (?:(size) (.*?) )?(?:(crc) (.*?))?(?:(md5) (.*?) )?(?:(sha1) (.*?) )?\)";
		private static string _itemPattern = @"^\s+(.*?) ""(.*?)""";
		private static string _endPattern = @"^\s*\)\s*$";

		/// <summary>
		/// Convert a RomVault style DAT to an XML derived DAT
		/// </summary>
		/// <param name="filecontents">Array of strings representing the input file</param>
		/// <returns>XElement representing the output XML DAT file</returns>
		public static XElement RomVaultToXML (string[] filecontents)
		{
			XElement elem = new XElement("datafile");

			bool block = false;
			for (int k = 0; k < filecontents.Length; k++)
			{
				string line = filecontents[k];

				// If the line is the header or a game
				if (Regex.IsMatch(line, _headerPattern))
				{
					GroupCollection gc = Regex.Match(line, _headerPattern).Groups;

					if (gc[1].Value == "clrmamepro" || gc[1].Value == "romvault")
					{
						elem.Add(new XElement("header"));
						elem = elem.Elements("header").Last();
					}
					else
					{
						elem.Add(new XElement(gc[1].Value));
						elem = elem.Elements(gc[1].Value).Last();
					}
					
					block = true;
				}

				// If the line is a rom or disk and we're in a block
				else if (Regex.IsMatch(line, _romPattern) && block)
			    {
					GroupCollection gc = Regex.Match(line, _romPattern).Groups;

					XElement temp = new XElement(gc[1].Value);

					// Loop over all attributes and add them if possible
					for (int i = 1; i < gc.Count; i++)
					{
						if (i + 2 < gc.Count)
						{
							temp.SetAttributeValue(gc[i+1].Value, gc[i+2].Value);
							i++;
						}
					}

					elem.Add(new XElement(temp));
				}
				// If the line is anything but a rom or disk  and we're in a block
				else if (Regex.IsMatch(line, _itemPattern) && block)
				{
					GroupCollection gc = Regex.Match(line, _itemPattern).Groups;

					if (gc[1].Value == "name" && elem.Name != "header")
					{
						elem.SetAttributeValue(gc[1].Value, gc[2].Value);
						elem.Add(new XElement("description", gc[2].Value));
					}
					else
					{
						elem.Add(new XElement(gc[1].Value, gc[2].Value));
					}
				}

				// If we find an end bracket that's not associated with anything else, the block is done
				else if (Regex.IsMatch(line, _endPattern) && block)
			    {
					block = false;
					elem = elem.Parent;
				}
			}

			return elem;
		}
	}
}
