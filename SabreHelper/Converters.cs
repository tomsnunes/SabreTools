using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace SabreTools.Helper
{
	/// <summary>
	/// Provide DAT conversion functionality
	/// 
	/// The following features have been requested:
	/// - Implement converting from/to DOSCenter format
	/// - Implement converting to RomCenter format
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

		/// <summary>
		/// Convert an XML derived DAT to a ClrMamePro style DAT
		/// </summary>
		/// <param name="root">XElement representing the file</param>
		/// <returns>String representing the output ClrMamePro DAT file</returns>
		public static String XMLToClrMamePro(XmlDocument root)
		{
			string output = "";

			// Experimental looping using only XML parsing
			XmlNode node = root.FirstChild;
			if (node != null && node.Name == "xml")
			{
				// Skip over everything that's not an element
				while (node.NodeType != XmlNodeType.Element)
				{
					node = node.NextSibling;
				}
			}

			// Once we find the main body, enter it
			if (node != null && (node.Name == "datafile" || node.Name == "softwarelist"))
			{
				node = node.FirstChild;
			}

			// Read the header if it exists
			if (node != null && node.Name == "header")
			{
				output += "clrmamepro (";

				XmlNode child = node.FirstChild;
				while (child != null)
				{
					output += "\n\t" + child.Name + " \"" + child.InnerText + "\"";
					child = child.NextSibling;
				}
				output += "\n)";

				// Skip over anything that's not an element
				while (node.NodeType != XmlNodeType.Element)
				{
					node = node.NextSibling;
				}
			}

			while (node != null)
			{
				if (node.NodeType == XmlNodeType.Element && (node.Name == "machine" || node.Name == "game" || node.Name == "software"))
				{
					// There are rare cases where a malformed XML will not have the required attributes. We can only skip them.
					if (node.Attributes.Count == 0)
					{
						node = node.NextSibling;
						continue;
					}

					output += "\ngame (\n\tname \"" + node.Attributes["name"].Value;

					// Get the roms from the machine
					if (node.HasChildNodes)
					{
						// If this node has children, traverse the children
						foreach (XmlNode child in node.ChildNodes)
						{
							// If we find a rom or disk, add it
							if (node.NodeType == XmlNodeType.Element && (child.Name == "rom" || child.Name == "disk"))
							{
								output += "\n\t" + child.Name + " ( name \"" + child.Attributes["name"].Value + "\"" +
									(child.Attributes["size"] != null ? " size " + Int32.Parse(child.Attributes["size"].Value) : "") +
									(child.Attributes["crc"] != null ? " crc " + child.Attributes["crc"].Value.ToLowerInvariant().Trim() : "") +
									(child.Attributes["md5"] != null ? " md5 " + child.Attributes["md5"].Value.ToLowerInvariant().Trim() : "") +
									(child.Attributes["sha1"] != null ? " sha1 " + child.Attributes["sha1"].Value.ToLowerInvariant().Trim() : "") + " )";
							}
							// If we find the signs of a software list, traverse the children
							else if (child.NodeType == XmlNodeType.Element && child.Name == "part" && child.HasChildNodes)
							{
								foreach (XmlNode part in child.ChildNodes)
								{
									// If we find a dataarea, traverse the children
									if (part.NodeType == XmlNodeType.Element && part.Name == "dataarea")
									{
										foreach (XmlNode data in part.ChildNodes)
										{
											// If we find a rom or disk, add it
											if (data.NodeType == XmlNodeType.Element && (data.Name == "rom" || data.Name == "disk") && data.Attributes["name"] != null)
											{
												output += "\n\t" + data.Name + " ( name \"" + data.Attributes["name"].Value +
													(data.Attributes["size"] != null ? " size " + Int32.Parse(data.Attributes["size"].Value) : "") +
													(data.Attributes["crc"] != null ? " crc " + data.Attributes["crc"].Value.ToLowerInvariant().Trim() : "") +
													(data.Attributes["md5"] != null ? " md5 " + data.Attributes["md5"].Value.ToLowerInvariant().Trim() : "") +
													(data.Attributes["sha1"] != null ? " sha1 " + data.Attributes["sha1"].Value.ToLowerInvariant().Trim() : "") + " )";
											}
										}
									}
								}
							}
							else
							{
								output += "\n\t" + child.Name + " \"" + child.InnerText + "\"";
							}
						}
					}
					output += "\n)";
				}
				node = node.NextSibling;
			}

			return output;
		}

		/// <summary>
		/// Convert an XML derived DAT to a RomCenter style DAT
		/// </summary>
		/// <param name="root">XElement representing the file</param>
		/// <returns>String representing the output RomCenter DAT file</returns>
		public static String XMLToRomCenter(XmlDocument root)
		{
			string output = "";

			return output;
		}
	}
}
