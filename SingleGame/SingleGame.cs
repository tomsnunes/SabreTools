using System;
using System.IO;
using System.Xml;

using SabreTools.Helper;

namespace SabreTools
{
	public class SingleGame
	{
		private static string _filename = "";
		private static string _path = "";
		private static bool _rename = true;

		public static void Main(string[] args)
		{
			Console.Title = "SingleGame " + Build.Version;

			if (args.Length == 0)
			{
				Help();
				return;
			}

			_filename = args[0];

			if (args.Length > 1)
			{
				for (int i = 1; i < args.Length; i++)
				{
					_path = (args[i].StartsWith("-r") ? args[i].Split('=')[1] : _path);
					_rename = (args[i] == "-n" ? false : _rename);
				}
			}

			_path = (_path == "" ? Environment.CurrentDirectory : _path);

			// Take the filename, and load it as an XML document
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(File.ReadAllText(_filename));
			}
			catch (XmlException)
			{
				doc.LoadXml(Converters.RomVaultToXML(File.ReadAllLines(_filename)).ToString());
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
				if (_rename && node.NodeType == XmlNodeType.Element && (node.Name == "machine" || node.Name == "game"))
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

								// Windows max name length is 260
								string tempname = child.Attributes["name"].Value;
								int usableLength = 259 - _path.Length;

								if (tempname.Length > usableLength)
								{
									string ext = Path.GetExtension(tempname);
									tempname = tempname.Substring(0, usableLength - ext.Length);
									tempname += ext;
								}
								tempNode.SetAttribute("name", tempname);
								outNode.AppendChild(tempNode);
							}
						}
					}
				}
				else
				{
					XmlNode tempNode = tempDoc.ImportNode(node, true);

					if (tempNode.Name == "header")
					{
						if (tempNode.SelectSingleNode("clrmamepro") == null)
						{
							XmlElement tempChild = tempDoc.CreateElement("clrmamepro");
							tempChild.SetAttribute("forcepacking", "unzip");
							tempNode.AppendChild(tempChild);
						}
						else
						{
							(tempNode.SelectSingleNode("clrmamepro") as XmlElement).SetAttribute("forcepacking", "unzip");
						}
					}

					outNode.AppendChild(tempNode);
				}
				node = node.NextSibling;
			}
			if (inGame)
			{
				outNode = outNode.ParentNode;
			}

			tempDoc.AppendChild(tempDoc.CreateDocumentType("datafile", "-//Logiqx//DTD ROM Management Datafile//EN", "http://www.logiqx.com/Dats/datafile.dtd", null));
			tempDoc.AppendChild(outNode);
			string outPath = Path.GetFileNameWithoutExtension(_filename) + ".new" + Path.GetExtension(_filename);
			File.WriteAllText(outPath, Style.Beautify(tempDoc));
		}

		private static void Help()
		{
			Console.WriteLine(@"SingleGame.exe <filename> [-r=rootdir|-n]
    -r=rootdir		Set the directory name for path size
    -n			Disable single-game mode
");
		}
	}
}
