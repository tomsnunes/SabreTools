using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SabreTools.Helper
{
	/// <summary>
	/// Contains all remappings of known import classes
	/// </summary>
	public class Remapping
	{
		// Remapping classes represented by a dictionary of dictionaries (name, (from, to))
		public static Dictionary<string, Dictionary<string, string>> DatMaps = new Dictionary<string, Dictionary<string, string>>();

		// Header skip classes represented by a dictionary of dictionaries (name, (header, size))
		public static Dictionary<string, Dictionary<string, int>> HeaderMaps = new Dictionary<string, Dictionary<string, int>>();

		/// <summary>
		/// Create all remappings to be used by the program
		/// </summary>
		public static void CreateRemappings()
		{
			// Create array of dictionary names
			string[] remappings =
			{
				"Good", "MAME", "MaybeIntro", "NoIntro", "NonGood", "Redump", "TOSEC", "TruRip",
			};

			// Loop through and add all remappings
			foreach (string remapping in remappings)
			{
				DatMaps.Add(remapping, new Dictionary<string, string>());
				RemappingHelper(remapping);
			}
		}

		/// <summary>
		/// Create a remapping from XML
		/// </summary>
		/// <param name="mapping">Name of the mapping to be populated</param>
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

			// If the node is empty, just return so it doesn't crash
			if (!node.HasChildNodes)
			{
				return;
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
				DatMaps[mapping].Add(node.Attributes["from"].Value, node.Attributes["to"].Value);

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

		/// <summary>
		/// Create all header mappings to be used by the program
		/// </summary>
		public static void CreateHeaderSkips()
		{
			// Create array of dictionary names
			string[] skippers =
			{
				"a7800", "fds", "lynx", /* "n64", */ "nes", "pce", "psid", "snes", "spc",
			};

			// Loop through and add all remappings
			foreach (string skipper in skippers)
			{
				HeaderMaps.Add(skipper, new Dictionary<string, int>());
				SkipperHelper(skipper);
			}
		}

		/// <summary>
		/// Create a remapping from XML
		/// </summary>
		/// <param name="skipper">Name of the header skipper to be populated</param>
		private static void SkipperHelper(string skipper)
		{
			// Read in remapping from file
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(File.ReadAllText("Skippers/" + skipper + ".xml"));
			}
			catch (XmlException ex)
			{
				Console.WriteLine(skipper + " header skippers could not be loaded! " + ex.ToString());
				return;
			}

			// Get the detector parent node
			XmlNode node = doc.FirstChild;
			while (node.Name != "detector")
			{
				node = node.NextSibling;
			}

			// Get the first rule node
			node = node.SelectSingleNode("rule");

			// Now read in the rules
			while (node != null && node.Name == "rule")
			{
				// Size is the offset for the actual game data
				int size = (node.Attributes["start_offset"] != null ? Convert.ToInt32(node.Attributes["start_offset"].Value, 16) : 0);

				// Each rule set can have more than one data rule. We can't really use multiples right now
				if (node.SelectNodes("data") != null)
				{
					foreach (XmlNode child in node.SelectNodes("data"))
					{
						// Add an offset to the match if one exists
						string header = (child.Attributes["offset"] != null && child.Attributes["offset"].Value != "0" ? "^.{" + (Convert.ToInt32(child.Attributes["offset"].Value, 16) * 2) + "}" : "^");
						header += child.Attributes["value"].Value;

						// Now add the header and value to the appropriate skipper dictionary
						HeaderMaps[skipper].Add(header, size);
					}
				}

				// Get the next node and skip over anything that's not an element
				node = node.NextSibling;

				if (node == null)
				{
					break;
				}

				while (node.NodeType != XmlNodeType.Element && node.Name != "rule")
				{
					node = node.NextSibling;
				}
			}
		}
	}
}
