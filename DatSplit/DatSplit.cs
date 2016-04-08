using System;
using System.IO;
using System.Text;
using System.Xml;

using SabreTools.Helper;

namespace DatSplit
{
	class DatSplit
	{
		private static string _extA;
		private static string _extB;
		private static string _filename;

		public static void Main(string[] args)
		{
			Console.Title = "DatSplit " + Build.Version;

			// If we don't have arguments, show help
			if (args.Length == 0 && args.Length != 3)
			{
				Build.Help();
				return;
			}

			// Set needed strings
			_filename = args[0];
			_extA = (args[1].StartsWith(".") ? args[1] : "." + args[1]).ToUpperInvariant();
			_extB = (args[2].StartsWith(".") ? args[2] : "." + args[2]).ToUpperInvariant();

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

			XmlDocument outDocA = new XmlDocument();
			outDocA.AppendChild(outDocA.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, null));
			outDocA.AppendChild(outDocA.CreateDocumentType("datafile", "-//Logiqx//DTD ROM Management Datafile//EN", "http://www.logiqx.com/Dats/datafile.dtd", null));
			XmlNode outA = outDocA.CreateNode(XmlNodeType.Element, node.Name, "");

			XmlDocument outDocB = new XmlDocument();
			outDocB.AppendChild(outDocB.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, null));
			outDocB.AppendChild(outDocB.CreateDocumentType("datafile", "-//Logiqx//DTD ROM Management Datafile//EN", "http://www.logiqx.com/Dats/datafile.dtd", null));
			XmlNode outB = outDocB.CreateNode(XmlNodeType.Element, node.Name, "");

			// Once we find the main body, enter it
			if (node != null && node.Name == "datafile")
			{
				node = node.FirstChild;
			}

			// Now here's where it differs from import
			while (node != null)
			{
				// If we're at a game node, add the parent node but not all the internals
				if (node.NodeType == XmlNodeType.Element && (node.Name == "machine" || node.Name == "game"))
				{
					bool inA = false;
					bool inB = false;

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

								if (child.Attributes["name"].Value.ToUpperInvariant().EndsWith(_extA))
								{
									if (!inA)
									{
										//XmlNode temp = tempDoc.CreateNode(XmlNodeType.Element, node.Name, "");
										XmlNode temp = outDocA.ImportNode(node, false);
										outA.AppendChild(temp);
										outA = outA.LastChild;
										inA = true;
									}
									outA.AppendChild(outDocA.ImportNode(child, true));
								}
								else if (child.Attributes["name"].Value.ToUpperInvariant().EndsWith(_extB))
								{
									if (!inB)
									{
										//XmlNode temp = tempDoc.CreateNode(XmlNodeType.Element, node.Name, "");
										XmlNode temp = outDocB.ImportNode(node, false);
										outB.AppendChild(temp);
										outB = outB.LastChild;
										inB = true;
									}
									outB.AppendChild(outDocB.ImportNode(child, true));
								}
								else
								{
									outA.AppendChild(outDocA.ImportNode(child, true));
									outB.AppendChild(outDocB.ImportNode(child, true));
								}
							}
						}

						// Set the output node to the right one for both
						if (inA)
						{
							outA = outA.ParentNode;
						}
						if (inB)
						{
							outB = outB.ParentNode;
						}
					}
				}
				else
				{
					XmlNode tempNode = outDocA.ImportNode(node, true);
					outA.AppendChild(tempNode);
					tempNode = outDocB.ImportNode(node, true);
					outB.AppendChild(tempNode);
				}
				node = node.NextSibling;
			}

			// Append the built nodes to the documents
			outDocA.AppendChild(outDocA.ImportNode(outA, true));
			string outPathA = Path.GetFileNameWithoutExtension(_filename) + _extA + Path.GetExtension(_filename);
			File.WriteAllText(outPathA, Style.Beautify(outDocA), Encoding.UTF8);

			outDocB.AppendChild(outDocB.ImportNode(outB, true));
			string outPathB = Path.GetFileNameWithoutExtension(_filename) + _extB + Path.GetExtension(_filename);
			File.WriteAllText(outPathB, Style.Beautify(outDocB), Encoding.UTF8);
		}
	}
}
