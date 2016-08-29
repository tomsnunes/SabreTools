using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SabreTools.Helper
{
	/// <summary>
	/// Contains all remappings of known import classes
	/// </summary>
	public class Mappings
	{
		// Local paths
		private const string _remappersPath = "Mappings";

		// Remapping classes represented by a dictionary of dictionaries (name, (from, to))
		private static Dictionary<string, Dictionary<string, string>> _datMaps = new Dictionary<string, Dictionary<string, string>>();
		public static Dictionary<string, Dictionary<string, string>> DatMaps
		{
			get
			{
				if (_datMaps.Count == 0)
				{
					CreateRemappings();
				}
				return _datMaps;
			}
		}

		#region DAT Name Remappings

		/// <summary>
		/// Create all remappings to be used by the program
		/// </summary>
		private static void CreateRemappings()
		{
			// Create array of dictionary names
			string[] remappings =
			{
				"Good", "MAME", "MaybeIntro", "NoIntro", "NonGood", "Redump", "TOSEC", "TruRip",
			};

			// Loop through and add all remappings
			foreach (string remapping in remappings)
			{
				_datMaps.Add(remapping, new Dictionary<string, string>());
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
				doc.LoadXml(System.IO.File.ReadAllText(Path.Combine(_remappersPath, mapping + ".xml")));
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
				_datMaps[mapping].Add(node.Attributes["from"].Value, node.Attributes["to"].Value);

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

		#endregion
	}
}
