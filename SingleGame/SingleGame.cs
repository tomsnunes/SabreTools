using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using SabreTools.Helper;

namespace SabreTools
{
	public class SingleGame
	{
		private static string filename;

		public static void Main(string[] args)
		{
			Console.Title = "SingleGame " + Build.Version;

			if (args.Length != 1)
			{
				Help();
				return;
			}

			filename = args[0];

			// Take the filename, and load it as an XML document
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(File.ReadAllText(filename));
			}
			catch (XmlException)
			{
				doc.LoadXml(Converters.RomVaultToXML(File.ReadAllLines(filename)).ToString());
			}

			// We all start the same
			XmlNode node = doc.FirstChild;
			if (node != null && node.Name == "xml")
			{
				// Skip over everything that's not an element
				while (node.NodeType != XmlNodeType.Element)
				{
					node = node.NextSibling;
				}
			}

			XmlDocument tempDoc = new XmlDocument();
			XmlNode outNode = tempDoc.CreateNode(XmlNodeType.Element, node.Name, "");

			// Once we find the main body, enter it
			if (node != null && node.Name == "datafile")
			{
				node = node.FirstChild;
			}

			// Now here's where it differs from import
			bool inGame = false;
			while (node != null)
			{
				// If we're at a game node, add the parent node but not all the internals
				if (node.NodeType == XmlNodeType.Element && (node.Name == "machine" || node.Name == "game"))
				{
					if (!inGame)
					{
						XmlElement tempNode = tempDoc.CreateElement(node.Name);
						tempNode.SetAttribute("name", "!");
						outNode.AppendChild(tempNode);
						outNode = outNode.LastChild;
						inGame = true;
					}

					// Get the roms from the machine
					if (node.HasChildNodes)
					{
						// If this node has children, traverse the children
						foreach (XmlNode child in node.ChildNodes)
						{
							// If we find a rom or disk, add it
							if (child.NodeType == XmlNodeType.Element && (child.Name == "rom" || child.Name == "disk"))
							{
								// Take care of hex-sized files
								long size = -1;
								if (child.Attributes["size"] != null && child.Attributes["size"].Value.Contains("0x"))
								{
									size = Convert.ToInt64(child.Attributes["size"].Value, 16);
								}
								else if (child.Attributes["size"] != null)
								{
									size = Int64.Parse(child.Attributes["size"].Value);
								}

								XmlElement tempNode = (XmlElement)tempDoc.ImportNode(child, true);

								// Windows max name length is 260. Taking into account the game name of "!", we can use 259 characters
								tempNode.SetAttribute("name", "(" + node.Attributes["name"].Value + ")" + child.Attributes["name"].Value);
								tempNode.Attributes["name"].Value = (tempNode.Attributes["name"].Value.Length > 259 ? tempNode.Attributes["name"].Value.Substring(0, 259) : tempNode.Attributes["name"].Value);
								outNode.AppendChild(tempNode);
							}
						}
					}
				}
				else
				{
					XmlNode tempNode = tempDoc.ImportNode(node, true);
					outNode.AppendChild(tempNode);
				}
				node = node.NextSibling;
			}

			tempDoc.AppendChild(tempDoc.CreateDocumentType("datafile", "-//Logiqx//DTD ROM Management Datafile//EN", "http://www.logiqx.com/Dats/datafile.dtd", null));
			tempDoc.AppendChild(outNode);
			string outPath = Path.GetFileNameWithoutExtension(filename) + ".new" + Path.GetExtension(filename);
			File.WriteAllText(outPath, Style.Beautify(tempDoc));
		}

		private static void Help()
		{
			Console.WriteLine("SingleGame.exe <filename>");
		}
	}
}
