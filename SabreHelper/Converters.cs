using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace SabreTools.Helper
{
	/// <summary>
	/// Provide DAT conversion functionality
	/// </summary>
	public class Converters
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

				// Comments in RV DATs start with a #
				if (line.StartsWith("#"))
				{
					continue;
				}

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

		/// <summary>
		/// Convert an XML derived DAT to a RomVault style DAT
		/// </summary>
		/// <param name="root">XElement representing the file</param>
		/// <returns>String representing the output RomVault DAT file</returns>
		public static String XMLToRomVault(XmlDocument root)
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
	}
}
