using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace SabreTools.Helper
{
	/// <summary>
	/// Contains all remappings of known import classes
	/// </summary>
	public class Remapping
	{
		// Local paths
		private const string _remappersPath = "Mappings";
		private const string _skippersPath = "Skippers";

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

		// Header skippers represented by a list of skipper objects
		private static List<Skipper> _list;
		public static List<Skipper> List
		{
			get
			{
				if (_list == null || _list.Count == 0)
				{
					PopulateSkippers();
				}
				return _list;
			}
		}

		// Header skippers classes represented by a dictionary of dictionaries (name, (header, size))
		private static Dictionary<string, Dictionary<string, int>> _headerMaps = new Dictionary<string, Dictionary<string, int>>();
		public static Dictionary<string, Dictionary<string, int>> HeaderMaps
		{
			get
			{
				if (_headerMaps.Count == 0)
				{
					CreateHeaderSkips();
				}
				return _headerMaps;
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
				doc.LoadXml(File.ReadAllText(Path.Combine(_remappersPath, mapping + ".xml")));
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

		#region Header Skips (new)

		/// <summary>
		/// Populate the entire list of header Skippers
		/// </summary>
		/// <remarks>
		/// http://mamedev.emulab.it/clrmamepro/docs/xmlheaders.txt
		/// http://www.emulab.it/forum/index.php?topic=127.0
		/// </remarks>
		private static void PopulateSkippers()
		{
			if (_list == null)
			{
				_list = new List<Skipper>();
			}

			foreach (string skipperFile in Directory.EnumerateFiles(_skippersPath, "*", SearchOption.AllDirectories))
			{
				_list.Add(PopulateSkippersHelper(Path.GetFullPath(skipperFile)));
			}
		}

		/// <summary>
		/// Populate an individual Skipper from file
		/// </summary>
		/// <param name="filename">Name of the file to be read from</param>
		/// <returns>The Skipper object associated with the file</returns>
		private static Skipper PopulateSkippersHelper(string filename)
		{
			Skipper skipper = new Skipper();

			if (!File.Exists(filename))
			{
				return skipper;
			}

			Logger logger = new Logger(false, "");
			logger.Start();
			XmlTextReader xtr = DatTools.GetXmlTextReader(filename, logger);

			if (xtr == null)
			{
				return skipper;
			}

			bool valid = false;
			xtr.MoveToContent();
			while (!xtr.EOF)
			{
				if (xtr.NodeType != XmlNodeType.Element)
				{
					xtr.Read();
				}

				switch (xtr.Name.ToLowerInvariant())
				{
					case "detector":
						valid = true;
						break;
					case "name":
						skipper.Name = xtr.ReadElementContentAsString();
						break;
					case "author":
						skipper.Author = xtr.ReadElementContentAsString();
						break;
					case "version":
						skipper.Version = xtr.ReadElementContentAsString();
						break;
					case "rule":
						// Get the information from the rule first
						SkipperRule rule = new SkipperRule
						{
							StartOffset = 0,
							EndOffset = 0,
							Operation = HeaderSkipOperation.None,
							Tests = new List<SkipperTest>(),
						};

						if (xtr.GetAttribute("start_offset") != null)
						{
							string offset = xtr.GetAttribute("start_offset");
							if (offset.ToLowerInvariant() == "eof")
							{
								rule.StartOffset = null;
							}
							else
							{
								long temp = 0;
								Int64.TryParse(offset, out temp);
								rule.StartOffset = temp;
							}
						}
						if (xtr.GetAttribute("end_offset") != null)
						{
							string offset = xtr.GetAttribute("end_offset");
							if (offset.ToLowerInvariant() == "eof")
							{
								rule.EndOffset = null;
							}
							else
							{
								long temp = 0;
								Int64.TryParse(offset, out temp);
								rule.EndOffset = temp;
							}
						}
						if (xtr.GetAttribute("operation") != null)
						{
							string operation = xtr.GetAttribute("operation");
							switch (operation.ToLowerInvariant())
							{
								case "bitswap":
									rule.Operation = HeaderSkipOperation.Bitswap;
									break;
								case "byteswap":
									rule.Operation = HeaderSkipOperation.Byteswap;
									break;
								case "wordswap":
									rule.Operation = HeaderSkipOperation.Wordswap;
									break;
							}
						}

						// Now read the individual tests into the Rule
						XmlReader subreader = xtr.ReadSubtree();

						if (subreader != null)
						{
							while (!subreader.EOF)
							{
								if (subreader.NodeType != XmlNodeType.Element)
								{
									subreader.Read();
								}

								// Get the test type
								SkipperTest test = new SkipperTest
								{
									Offset = 0,
									Value = new byte[0],
									Result = true,
									Mask = new byte[0],
									Size = 0,
									Operator = HeaderSkipTestFileOperator.Equal,
								};
								switch (subreader.Name.ToLowerInvariant())
								{
									case "data":
										test.Type = HeaderSkipTest.Data;
										break;
									case "or":
										test.Type = HeaderSkipTest.Or;
										break;
									case "xor":
										test.Type = HeaderSkipTest.Xor;
										break;
									case "and":
										test.Type = HeaderSkipTest.And;
										break;
									case "file":
										test.Type = HeaderSkipTest.File;
										break;
									default:
										subreader.Read();
										break;
								}

								// Now populate all the parts that we can
								if (subreader.GetAttribute("offset") != null)
								{
									string offset = subreader.GetAttribute("offset");
									if (offset.ToLowerInvariant() == "eof")
									{
										test.Offset = null;
									}
									else
									{
										long temp = 0;
										Int64.TryParse(offset, out temp);
										test.Offset = temp;
									}
								}
								if (subreader.GetAttribute("value") != null)
								{
									string value = subreader.GetAttribute("value");

									// http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
									test.Value = new byte[value.Length / 2];
									for (int index = 0; index < test.Value.Length; index++)
									{
										string byteValue = value.Substring(index * 2, 2);
										test.Value[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
									}
								}
								if (subreader.GetAttribute("result") != null)
								{
									string result = subreader.GetAttribute("result");
									switch (result.ToLowerInvariant())
									{
										case "false":
											test.Result = false;
											break;
										case "true":
										default:
											test.Result = true;
											break;
									}
								}
								if (subreader.GetAttribute("mask") != null)
								{
									string mask = subreader.GetAttribute("mask");

									// http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
									test.Mask = new byte[mask.Length / 2];
									for (int index = 0; index < test.Mask.Length; index++)
									{
										string byteValue = mask.Substring(index * 2, 2);
										test.Mask[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
									}
								}
								if (subreader.GetAttribute("size") != null)
								{
									string size = subreader.GetAttribute("size");
									if (size.ToLowerInvariant() == "po2")
									{
										test.Size = null;
									}
									else
									{
										long temp = 0;
										Int64.TryParse(size, out temp);
										test.Size = temp;
									}
								}
								if (subreader.GetAttribute("operator") != null)
								{
									string oper = subreader.GetAttribute("operator");
									switch (oper.ToLowerInvariant())
									{
										case "less":
											test.Operator = HeaderSkipTestFileOperator.Less;
											break;
										case "greater":
											test.Operator = HeaderSkipTestFileOperator.Greater;
											break;
										case "equal":
										default:
											test.Operator = HeaderSkipTestFileOperator.Equal;
											break;
									}
								}

								// Add the created test to the rule
								rule.Tests.Add(test);
								subreader.Read();
							}
						}

						// Add the created rule to the skipper
						skipper.Rules.Add(rule);
						xtr.Skip();
						break;
					default:
						xtr.Read();
						break;
				}
			}

			return (valid ? skipper : new Skipper());
		}

		#endregion

		#region Header Skips (old)

		/// <summary>
		/// Create all header mappings to be used by the program
		/// </summary>
		private static void CreateHeaderSkips()
		{
			// Create array of dictionary names
			string[] skippers =
			{
				"a7800", "fds", "lynx", /* "n64", */ "nes", "pce", "psid", "snes", "spc",
			};

			// Loop through and add all remappings
			foreach (string skipper in skippers)
			{
				_headerMaps.Add(skipper, new Dictionary<string, int>());
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
				doc.LoadXml(File.ReadAllText(Path.Combine(_skippersPath, skipper + ".xml")));
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
						_headerMaps[skipper].Add(header, size);
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

		#endregion
	}
}
