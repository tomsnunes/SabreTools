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
		// Remapping classes represented by dictionaries (from, to)
		public static Dictionary<string, string> Good = new Dictionary<string, string>();
		public static Dictionary<string, string> MAME = new Dictionary<string, string>();
		public static Dictionary<string, string> MaybeIntro = new Dictionary<string, string>();
		public static Dictionary<string, string> NoIntro = new Dictionary<string, string>();
		public static Dictionary<string, string> NonGood = new Dictionary<string, string>();
		public static Dictionary<string, string> Redump = new Dictionary<string, string>();
		public static Dictionary<string, string> TOSEC = new Dictionary<string, string>();
		public static Dictionary<string, string> TruRip = new Dictionary<string, string>();

		// Header skip classes represented by dictionaries (header, size)
		public static Dictionary<string, int> A7800 = new Dictionary<string, int>();
		public static Dictionary<string, int> FDS = new Dictionary<string, int>();
		public static Dictionary<string, int> Lynx = new Dictionary<string, int>();
		//public static Dictionary<string, int> N64 = new Dictionary<string, int>();
		public static Dictionary<string, int> NES = new Dictionary<string, int>();
		public static Dictionary<string, int> PCE = new Dictionary<string, int>();
		public static Dictionary<string, int> SNES = new Dictionary<string, int>();

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
					case "Good":
						Good.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "MAME":
						MAME.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "MaybeIntro":
						MaybeIntro.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "NoIntro":
						NoIntro.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
						break;
					case "NonGood":
						NonGood.Add(node.Attributes["from"].Value, node.Attributes["to"].Value);
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

		/// <summary>
		/// Create all header mappings to be used by the program
		/// </summary>
		public static void CreateHeaderSkips()
		{
			// Create array of dictionary names
			string[] skippers =
			{
				"a7800", "fds", "lynx", /* "n64", */ "nes", "pce", "snes",
			};

			// Loop through and add all remappings
			foreach (string skipper in skippers)
			{
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
						switch (skipper)
						{
							case "a7800":
								A7800.Add(header, size);
								break;
							case "fds":
								FDS.Add(header, size);
								break;
							case "lynx":
								Lynx.Add(header, size);
								break;
							/*
							case "n64":
								N64.Add(header, size);
								break;
							*/
							case "nes":
								NES.Add(header, size);
								break;
							case "pce":
								PCE.Add(header, size);
								break;
							case "snes":
								SNES.Add(header, size);
								break;
						}
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
