using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace WoD.Helper
{
	class Remapping
	{
		public static Dictionary<string, string> MAME = new Dictionary<string, string>();
		public static Dictionary<string, string> NoIntro = new Dictionary<string, string>();
		public static Dictionary<string, string> Redump = new Dictionary<string, string>();
		public static Dictionary<string, string> TOSEC = new Dictionary<string, string>();
		public static Dictionary<string, string> TruRip = new Dictionary<string, string>();

		public static void CreateRemappings()
		{
			// Create array of dictionary names
			string[] remappings =
			{
				"MAME", "NoIntro", "Redump", "TOSEC", "TruRip",
			};

			// Loop through and add all remappings
			foreach (string remapping in remappings)
			{
				RemappingHelper(remapping);
			}
        }

		private static void RemappingHelper(string mapping)
		{
			// Read in remapping from file
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(File.ReadAllText("Mappings/" + mapping + ".xml"));
			}
			catch (XmlException ex)
			{
				Console.WriteLine(mapping + " remappings could not be loaded! " + ex.ToString());
				return;
			}

			// Get the mappings parent node
			XmlNode node = doc.FirstChild;
			while (node.Name != "mappings")
			{
				node = node.NextSibling;
			}

			// Get the first mapping node
			node = node.FirstChild;
			while (node.NodeType != XmlNodeType.Element && node.Name != "mapping")
			{
				node = node.NextSibling;
			}

			// Now read in the mappings
			while (node != null && node.Name == "mapping")
			{
				switch (mapping)
				{
					case "MAME":
						MAME.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "NoIntro":
						NoIntro.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "Redump":
						Redump.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "TOSEC":
						TOSEC.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "TruRip":
						TruRip.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
				}
				

				// Get the next node and skip over anything that's not an element
				node = node.NextSibling;

				if (node == null)
				{
					break;
				}

				while (node.NodeType != XmlNodeType.Element && node.Name != "mapping")
				{
					node = node.NextSibling;
				}
			}
		}
	}
}
