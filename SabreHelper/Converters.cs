using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace SabreTools.Helper
{
	/// <summary>
	/// Provide DAT to XML conversion functionality
	/// 
	/// The following features have been requested:
	/// - Implement converting from DOSCenter format
	/// </summary>
	public class Converters
	{
		// Regex matching patterns
		private static string _headerPatternCMP = @"(^.*?) \($";
		private static string _itemPatternCMP = @"^\s*(\S*?) (.*)";
		private static string _endPatternCMP = @"^\s*\)\s*$";

		/// <summary>
		/// Convert a ClrMamePro style DAT to an XML derived DAT
		/// </summary>
		/// <param name="filecontents">Array of strings representing the input file</param>
		/// <returns>XElement representing the output XML DAT file</returns>
		public static XElement ClrMameProToXML(string[] filecontents)
		{
			XElement elem = new XElement("datafile");

			bool block = false;
			for (int k = 0; k < filecontents.Length; k++)
			{
				string line = filecontents[k];

				// Comments in CMP DATs start with a #
				if (line.Trim().StartsWith("#"))
				{
					continue;
				}

				// If the line is the header or a game
				if (Regex.IsMatch(line, _headerPatternCMP))
				{
					GroupCollection gc = Regex.Match(line, _headerPatternCMP).Groups;

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
						//If the item is empty, we automatically skip it because it's a fluke
						if (gc[i].Trim() == String.Empty)
						{
							continue;
						}
						// Even number of quotes, not in a quote, not in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib == "")
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
				// If the line is anything but a rom or disk and we're in a block
				else if (Regex.IsMatch(line, _itemPatternCMP) && block)
				{
					GroupCollection gc = Regex.Match(line, _itemPatternCMP).Groups;

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
				else if (Regex.IsMatch(line, _endPatternCMP) && block)
				{
					block = false;
					elem = elem.Parent;
				}
			}

			return elem;
		}

		/// <summary>
		/// Convert a RomCenter style DAT to an XML derived DAT
		/// </summary>
		/// <param name="filecontents">Array of strings representing the input file</param>
		/// <returns>XElement representing the output XML DAT file</returns>
		public static XElement RomCenterToXML(string[] filecontents)
		{
			XElement elem = new XElement("datafile");

			string blocktype = "";
			string lastgame = null;
			for (int k = 0; k < filecontents.Length; k++)
			{
				string line = filecontents[k];

				// If the line is the start of the credits section
				if (line.ToLowerInvariant().Contains("[credits]"))
				{
					blocktype = "credits";
					if (elem.Name != "header")
					{
						elem.Add(new XElement("header"));
						elem = elem.Elements("header").Last();
					}
				}
				// If the line is the start of the dat section
				else if (line.ToLowerInvariant().Contains("[dat]"))
				{
					blocktype = "dat";
					if (elem.Name != "header")
					{
						elem.Add(new XElement("header"));
						elem = elem.Elements("header").Last();
					}
				}
				// If the line is the start of the emulator section
				else if (line.ToLowerInvariant().Contains("[emulator]"))
				{
					blocktype = "emulator";
					if (elem.Name != "header")
					{
						elem.Add(new XElement("header"));
						elem = elem.Elements("header").Last();
					}
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
						elem.Add(new XElement("author", line.Split('=')[1]));
					}
					// If we have one of the three version tags
					else if (line.StartsWith("version="))
					{
						switch (blocktype)
						{
							case "credits":
								elem.Add(new XElement("version", line.Split('=')[1]));
								break;
							case "emulator":
								elem.Add(new XElement("description", line.Split('=')[1]));
								break;
						}
					}
					// If we have a comment
					else if (line.StartsWith("comment="))
					{
						elem.Add(new XElement("comment", line.Split('=')[1]));
					}
					// If we have the split flag
					else if (line.StartsWith("split="))
					{
						int split = 0;
						if (Int32.TryParse(line.Split('=')[1], out split))
						{
							if (split == 1)
							{
								XElement cmp = new XElement("clrmamepro");
								cmp.Add(new XAttribute("forcemerging", "split"));
								elem.Add(cmp);
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
								XElement cmp = new XElement("clrmamepro");
								cmp.Add(new XAttribute("forcemerging", "full"));
								elem.Add(cmp);
							}
						}
					}
					// If we have the refname tag
					else if (line.StartsWith("refname="))
					{
						elem.Add(new XElement("name", line.Split('=')[1]));
					}
					// If we have a rom
					else if (line.StartsWith("¬"))
					{
						/*
						The rominfo order is as follows:
						0 - parent name
						1 - parent description
						2 - game name
						3 - game description
						4 - rom name
						5 - rom crc
						6 - rom size
						7 - romof name
						8 - merge name
						*/
						string[] rominfo = line.Split('¬');
						RomData rom = new RomData
						{
							Game = rominfo[2],
							Name = rominfo[4],
							CRC = rominfo[5],
							Size = Int64.Parse(rominfo[6]),
						};

						if (lastgame != rom.Game)
						{
							elem = elem.Parent;
							XElement current = new XElement("machine");
							current.Add(new XAttribute("name", rom.Game));
							current.Add(new XElement("description", rom.Game));
							elem.Add(current);
						}
						elem = elem.Elements("machine").Last();

						XElement romelem = new XElement("rom");
						romelem.Add(new XAttribute("name", rom.Name));
						romelem.Add(new XAttribute("size", rom.Size));
						romelem.Add(new XAttribute("crc", rom.CRC));

						elem.Add(romelem);
					}
				}
			}

			return elem;
		}
	}
}
