using System;
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
		private static string _itemPattern = @"^\s+(\S*?) (.*)";
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
				else if ((line.Trim().StartsWith("rom (") || line.Trim().StartsWith("disk (")) && block)
				{
					string[] gc = line.Trim().Split(' ');

					XElement temp = new XElement(gc[0]);

					// Loop over all attributes and add them if possible
					bool quote = false;
					string attrib = "", val = "";
					for (int i = 2; i < gc.Length; i++)
					{
						// Even number of quotes, not in a quote, not in attribute
						if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib == "")
						{
							attrib = gc[i].Replace("\"", "");
						}
						// Even number of quotes, not in a quote, in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib != "")
						{
							temp.SetAttributeValue(attrib, gc[i].Replace("\"", ""));

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
							temp.SetAttributeValue(attrib, val);

							quote = false;
							attrib = "";
							val = "";
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
						elem.SetAttributeValue(gc[1].Value, gc[2].Value.Replace("\"", ""));
						elem.Add(new XElement("description", gc[2].Value.Replace("\"", "")));
					}
					else
					{
						elem.Add(new XElement(gc[1].Value, gc[2].Value.Replace("\"", "")));
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
