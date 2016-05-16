using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SabreTools.Helper
{
	public class RomManipulation
	{
		// 0-byte file constants
		public static long SizeZero = 0;
		public static string CRCZero = "00000000";
		public static string MD5Zero = "d41d8cd98f00b204e9800998ecf8427e";
		public static string SHA1Zero = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

		/// <summary>
		/// Return if the file is XML or not
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>public static bool IsXmlDat(string filename)
		public static bool IsXmlDat(string filename)
		{
			try
			{
				StreamReader sr = new StreamReader(File.OpenRead(filename));
				string first = sr.ReadLine();
				sr.Close();
				return first.Contains("<") && first.Contains(">");
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Get the XmlDocument associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The XmlDocument representing the (possibly converted) file, null otherwise</returns>
		public static XmlDocument GetXmlDocument(string filename, Logger logger)
		{
			logger.Log("Attempting to read file: " + filename);

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return null;
			}

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(filename);
			}
			catch (XmlException)
			{
				try
				{
					doc.LoadXml(Converters.ClrMameProToXML(File.ReadAllLines(filename)).ToString());
				}
				catch (Exception ex)
				{
					logger.Error(ex.ToString());
					return null;
				}
			}
			catch (IOException)
			{
				logger.Error("File '" + filename + "' could not be open or read");
			}
			catch (OutOfMemoryException)
			{
				logger.Error("File '" + filename + "' is too large to be processed!");
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return null;
			}

			return doc;
		}

		/// <summary>
		/// Get the XmlTextReader associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The XmlTextReader representing the (possibly converted) file, null otherwise</returns>
		public static XmlTextReader GetXmlTextReader(string filename, Logger logger)
		{
			logger.Log("Attempting to read file: " + filename);

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return null;
			}

			if (IsXmlDat(filename))
			{
				logger.Log("XML DAT detected");
				XmlTextReader xtr = new XmlTextReader(filename);
				xtr.WhitespaceHandling = WhitespaceHandling.None;
				xtr.DtdProcessing = DtdProcessing.Ignore;
				return xtr;
			}
			else
			{
				logger.Log("Non-XML DAT detected");
				StringReader sr = new StringReader(Converters.ClrMameProToXML(File.ReadAllLines(filename)).ToString());
				XmlTextReader xtr = new XmlTextReader(sr);
				xtr.WhitespaceHandling = WhitespaceHandling.None;
				xtr.DtdProcessing = DtdProcessing.Ignore;
				return xtr;
			}
		}

		/// <summary>
		/// Get the name of the DAT for external use
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The internal name of the DAT on success, empty string otherwise</returns>
		/// <remarks>Needs to be upgraded to XmlTextReader</remarks>
		public static string GetDatName(string filename, Logger logger)
		{
			string name = "";
			XmlDocument doc = GetXmlDocument(filename, logger);

			// If the returned document is null, return the blank string
			if (doc == null)
			{
				return name;
			}

			// Experimental looping using only XML parsing
			XmlNode node = doc.FirstChild;
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

			// Get the name from the header
			if (node != null && node.Name == "header")
			{
				XmlNode temp = node.SelectSingleNode("name");
				if (temp != null)
				{
					name = temp.InnerText;
				}
			}

			return name;
		}

		/// <summary>
		/// Get the description of the DAT for external use
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The internal name of the DAT on success, empty string otherwise</returns>
		/// <remarks>Needs to be upgraded to XmlTextReader</remarks>
		public static string GetDatDescription(string filename, Logger logger)
		{
			string desc = "";
			XmlDocument doc = GetXmlDocument(filename, logger);

			// If the returned document is null, return the blank string
			if (doc == null)
			{
				return desc;
			}

			// Experimental looping using only XML parsing
			XmlNode node = doc.FirstChild;
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

			// Get the name from the header
			if (node != null && node.Name == "header")
			{
				XmlNode temp = node.SelectSingleNode("description");
				if (temp != null)
				{
					desc = temp.InnerText;
				}
			}

			return desc;
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>List of RomData objects representing the found data</returns>
		public static List<RomData> Parse(string filename, int sysid, int srcid, Logger logger)
		{
			List<RomData> roms = new List<RomData>();
			XmlTextReader xtr = GetXmlTextReader(filename, logger);
			bool superdat = false, shouldbreak = false;
			string parent = "";
			if (xtr != null)
			{
				xtr.MoveToContent();
				while (xtr.NodeType != XmlNodeType.None)
				{
					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						case "datafile":
						case "softwarelist":
							parent = xtr.Name;
							xtr.Read();
							break;
						case "header":
							xtr.ReadToDescendant("name");
							string content = xtr.ReadElementContentAsString();
							superdat = (content != null ? content.Contains(" - SuperDAT") : false);
							while (xtr.Name != "header")
							{
								xtr.Read();
							}
							xtr.Read();
							break;
						case "machine":
						case "game":
						case "software":
							string temptype = xtr.Name;
							string tempname = "";

							// We want to process the entire subtree of the game
							XmlReader subreader = xtr.ReadSubtree();

							if (subreader != null)
							{
								if (temptype == "software" && subreader.ReadToFollowing("description"))
								{
									tempname = subreader.ReadElementContentAsString();
								}
								else
								{
									// There are rare cases where a malformed XML will not have the required attributes. We can only skip them.
									if (xtr.AttributeCount == 0)
									{
										logger.Error("No attributes were found");
										xtr.ReadToNextSibling(xtr.Name);
										continue;
									}
									tempname = xtr.GetAttribute("name");
								}

								if (superdat)
								{
									tempname = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
								}

								while (subreader.Read())
								{
									// We only want elements
									if (subreader.NodeType != XmlNodeType.Element)
									{
										continue;
									}

									// Get the roms from the machine
									switch (subreader.Name)
									{
										case "rom":
										case "disk":
											// Take care of hex-sized files
											long size = -1;
											if (xtr.GetAttribute("size") != null && xtr.GetAttribute("size").Contains("0x"))
											{
												size = Convert.ToInt64(xtr.GetAttribute("size"), 16);
											}
											else if (xtr.GetAttribute("size") != null)
											{
												Int64.TryParse(xtr.GetAttribute("size"), out size);
											}

											// Take care of hex-prefixed hashes
											string crc = (xtr.GetAttribute("crc") != null ? xtr.GetAttribute("crc").ToLowerInvariant().Trim() : "");
											crc = (crc.StartsWith("0x") ? crc.Remove(0, 2) : crc);
											string md5 = (xtr.GetAttribute("md5") != null ? xtr.GetAttribute("md5").ToLowerInvariant().Trim() : "");
											md5 = (md5.StartsWith("0x") ? md5.Remove(0, 2) : md5);
											string sha1 = (xtr.GetAttribute("sha1") != null ? xtr.GetAttribute("sha1").ToLowerInvariant().Trim() : "");
											sha1 = (sha1.StartsWith("0x") ? sha1.Remove(0, 2) : sha1);

											roms.Add(new RomData
											{
												Game = tempname,
												Name = xtr.GetAttribute("name"),
												Type = xtr.Name,
												SystemID = sysid,
												SourceID = srcid,
												Size = size,
												CRC = crc,
												MD5 = md5,
												SHA1 = sha1,
											});
											break;
									}
								}
							}

							// Read to next game
							if (!xtr.ReadToNextSibling(temptype))
							{
								shouldbreak = true;
							}
							break;
						default:
							xtr.Read();
							break;
					}

					// If we hit an endpoint, break out of the loop early
					if (shouldbreak)
					{
						break;
					}
				}
			}

			return roms;
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>DatData object representing the read-in data</returns>
		public static DatData ParseDict(string filename, int sysid, int srcid, DatData datdata, Logger logger)
		{
			Dictionary<string, List<RomData>> roms = ParseDict(filename, sysid, srcid, datdata.Roms, logger);
			datdata.Roms = roms;
			return datdata;
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="dict">The dictionary to add found roms to</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>Dictionary with "size-crc" key and List of RomData objects value representing the found data</returns>
		public static Dictionary<string, List<RomData>> ParseDict(string filename, int sysid, int srcid, Dictionary<string, List<RomData>> dict, Logger logger)
		{
			XmlTextReader xtr = GetXmlTextReader(filename, logger);
			bool superdat = false, shouldbreak = false;
			string parent = "";
			if (xtr != null)
			{
				xtr.MoveToContent();
				while (xtr.NodeType != XmlNodeType.None)
				{
					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						case "datafile":
						case "softwarelist":
							parent = xtr.Name;
							xtr.Read();
							break;
						case "header":
							xtr.ReadToDescendant("name");
							string content = xtr.ReadElementContentAsString();
							superdat = (content != null ? content.Contains(" - SuperDAT") : false);
							while (xtr.Name != "header")
							{
								xtr.Read();
							}
							xtr.Read();
							break;
						case "machine":
						case "game":
						case "software":
							string temptype = xtr.Name;
							string tempname = "";

							// We want to process the entire subtree of the game
							XmlReader subreader = xtr.ReadSubtree();

							if (subreader != null)
							{
								if (temptype == "software" && subreader.ReadToFollowing("description"))
								{
									tempname = subreader.ReadElementContentAsString();
								}
								else
								{
									// There are rare cases where a malformed XML will not have the required attributes. We can only skip them.
									if (xtr.AttributeCount == 0)
									{
										logger.Error("No attributes were found");
										xtr.ReadToNextSibling(xtr.Name);
										continue;
									}
									tempname = xtr.GetAttribute("name");
								}

								if (superdat)
								{
									tempname = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
								}

								string key = "";
								while (subreader.Read())
								{
									// We only want elements
									if (subreader.NodeType != XmlNodeType.Element)
									{
										continue;
									}

									// Get the roms from the machine
									switch (subreader.Name)
									{
										case "rom":
										case "disk":
											// If the rom is nodump, skip it
											if (xtr.GetAttribute("flags") == "nodump" || xtr.GetAttribute("status") == "nodump")
											{
												logger.Log("Nodump detected; skipping...");
												break;
											}

											// Take care of hex-sized files
											long size = -1;
											if (xtr.GetAttribute("size") != null && xtr.GetAttribute("size").Contains("0x"))
											{
												size = Convert.ToInt64(xtr.GetAttribute("size"), 16);
											}
											else if (xtr.GetAttribute("size") != null)
											{
												Int64.TryParse(xtr.GetAttribute("size"), out size);
											}

											// If the rom is continue or ignore, add the size to the previous rom
											if (xtr.GetAttribute("loadflag") == "continue" || xtr.GetAttribute("loadflag") == "ignore")
											{
												int index = dict[key].Count() - 1;
												RomData lastrom = dict[key][index];
												lastrom.Size += size;
												dict[key].RemoveAt(index);
												dict[key].Add(lastrom);
												continue;
											}

											// Sanitize the hashes from null, hex sizes, and "true blank" strings
											string crc = (xtr.GetAttribute("crc") != null ? xtr.GetAttribute("crc").ToLowerInvariant().Trim() : "");
											crc = (crc.StartsWith("0x") ? crc.Remove(0, 2) : crc);
											crc = (crc == "-" ? "" : crc);
											crc = (crc == "" ? "" : crc.PadLeft(8, '0'));
											string md5 = (xtr.GetAttribute("md5") != null ? xtr.GetAttribute("md5").ToLowerInvariant().Trim() : "");
											md5 = (md5.StartsWith("0x") ? md5.Remove(0, 2) : md5);
											md5 = (md5 == "-" ? "" : md5);
											md5 = (md5 == "" ? "" : md5.PadLeft(32, '0'));
											string sha1 = (xtr.GetAttribute("sha1") != null ? xtr.GetAttribute("sha1").ToLowerInvariant().Trim() : "");
											sha1 = (sha1.StartsWith("0x") ? sha1.Remove(0, 2) : sha1);
											sha1 = (sha1 == "-" ? "" : sha1);
											sha1 = (sha1 == "" ? "" : sha1.PadLeft(40, '0'));

											// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
											if (subreader.Name == "rom" && (size == 0 || size == -1) && (crc == CRCZero || md5 == MD5Zero || sha1 == SHA1Zero))
											{
												size = SizeZero;
												crc = CRCZero;
												md5 = MD5Zero;
												sha1 = SHA1Zero;
											}
											// If the file has no size and it's not the above case, skip and log
											else if (subreader.Name == "rom" && (size == 0 || size == -1))
											{
												logger.Warning("Potentially incomplete entry found for " + xtr.GetAttribute("name"));
												break;
											}

											// Only add the rom if there's useful information in it
											if (!(crc == "" && md5 == "" && sha1 == ""))
											{
												// Get the new values to add
												key = size + "-" + crc;

												RomData value = new RomData
												{
													Game = tempname,
													Name = xtr.GetAttribute("name"),
													Type = xtr.Name,
													SystemID = sysid,
													SourceID = srcid,
													Size = size,
													CRC = crc,
													MD5 = md5,
													SHA1 = sha1,
													System = filename,
												};

												if (dict.ContainsKey(key))
												{
													dict[key].Add(value);
												}
												else
												{
													List<RomData> newvalue = new List<RomData>();
													newvalue.Add(value);
													dict.Add(key, newvalue);
												}
											}
											break;
									}
								}
							}

							// Read to next game
							if (!xtr.ReadToNextSibling(temptype))
							{
								shouldbreak = true;
							}
							break;
						default:
							xtr.Read();
							break;
					}

					// If we hit an endpoint, break out of the loop early
					if (shouldbreak)
					{
						break;
					}
				}
			}

			return dict;
		}

		/// <summary>
		/// Merge an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="inroms">List of RomData objects representing the roms to be merged</param>
		/// <param name="presorted">True if the list should be considered pre-sorted (default false)</param>
		/// <returns>A List of RomData objects representing the merged roms</returns>
		public static List<RomData> Merge(List<RomData> inroms, bool presorted = false)
		{
			List<RomData> outroms = new List<RomData>();

			// First sort the roms by size, crc, md5, sha1 (in order), if not sorted already
			if (!presorted)
			{
				inroms.Sort(delegate (RomData x, RomData y)
				{
					if (x.Size == y.Size)
					{
						if (x.CRC == y.CRC)
						{
							if (x.MD5 == y.MD5)
							{
								return String.Compare(x.SHA1, y.SHA1);
							}
							return String.Compare(x.MD5, y.MD5);
						}
						return String.Compare(x.CRC, y.CRC);
					}
					return (int)(x.Size - y.Size);
				});
			}

			// Then, deduplicate them by checking to see if data matches
			foreach (RomData rom in inroms)
			{
				// If it's the first rom in the list, don't touch it
				if (outroms.Count != 0)
				{
					// Check if the rom is a duplicate
					RomData last = outroms[outroms.Count - 1];

					bool shouldcont = false;
					if (rom.Type == "rom" && last.Type == "rom")
					{
						shouldcont = ((rom.Size == last.Size) &&
							((rom.CRC == "" || last.CRC == "") || rom.CRC == last.CRC) &&
							((rom.MD5 == "" || last.MD5 == "") || rom.MD5 == last.MD5) &&
							((rom.SHA1 == "" || last.SHA1 == "") || rom.SHA1 == last.SHA1)
						);
					}
					else if (rom.Type == "disk" && last.Type == "disk")
					{
						shouldcont = (((rom.MD5 == "" || last.MD5 == "") || rom.MD5 == last.MD5) &&
							((rom.SHA1 == "" || last.SHA1 == "") || rom.SHA1 == last.SHA1)
						);
					}

					// If it's a duplicate, skip adding it to the output but add any missing information
					if (shouldcont)
					{
						last.CRC = (last.CRC == "" && rom.CRC != "" ? rom.CRC : last.CRC);
						last.MD5 = (last.MD5 == "" && rom.MD5 != "" ? rom.MD5 : last.MD5);
						last.SHA1 = (last.SHA1 == "" && rom.SHA1 != "" ? rom.SHA1 : last.SHA1);

						// If the current system has a lower ID than the previous, set the system accordingly
						if (rom.SystemID < last.SystemID)
						{
							last.SystemID = rom.SystemID;
							last.System = rom.System;
							last.Game = rom.Game;
							last.Name = rom.Name;
						}

						// If the current source has a lower ID than the previous, set the source accordingly
						if (rom.SourceID < last.SourceID)
						{
							last.SourceID = rom.SourceID;
							last.Source = rom.Source;
							last.Game = rom.Game;
							last.Name = rom.Name;
						}

						// If the duplicate is external already or should be, set it
						if (last.Dupe >= DupeType.ExternalHash || last.SystemID != rom.SystemID || last.SourceID != rom.SourceID)
						{
							if (last.Game == rom.Game && last.Name == rom.Name)
							{
								last.Dupe = DupeType.ExternalAll;
							}
							else
							{
								last.Dupe = DupeType.ExternalHash;
							}
						}

						// Otherwise, it's considered an internal dupe
						else
						{
							if (last.Game == rom.Game && last.Name == rom.Name)
							{
								last.Dupe = DupeType.InternalAll;
							}
							else
							{
								last.Dupe = DupeType.InternalHash;
							}
						}

						outroms.RemoveAt(outroms.Count - 1);
						outroms.Insert(outroms.Count, last);

						continue;
					}
					else
					{
						outroms.Add(rom);
					}
				}
				else
				{
					outroms.Add(rom);
				}
			}

			// Then return the result
			return outroms;
		}

		/// <summary>
		/// Sort a list of RomData objects by SystemID, SourceID, Game, and Name (in order)
		/// </summary>
		/// <param name="roms">List of RomData objects representing the roms to be sorted</param>
		/// <param name="norename">True if files are not renamed, false otherwise</param>
		/// <returns>True if it sorted correctly, false otherwise</returns>
		public static bool Sort(List<RomData> roms, bool norename)
		{
			roms.Sort(delegate (RomData x, RomData y)
			{
				if (x.SystemID == y.SystemID)
				{
					if (x.SourceID == y.SourceID)
					{
						if (x.Game == y.Game)
						{
							return String.Compare(x.Name, y.Name);
						}
						return String.Compare(x.Game, y.Game);
					}
					return (norename ? String.Compare(x.Game, y.Game) : x.SourceID - y.SourceID);
				}
				return (norename ? String.Compare(x.Game, y.Game) : x.SystemID - y.SystemID);
			});
			return true;
		}

		/// <summary>
		/// Get differences between two lists of RomData objects
		/// </summary>
		/// <param name="A">First RomData list</param>
		/// <param name="B">Second RomData list</param>
		/// <returns>Any rom that's not in both lists</returns>
		/// <remarks>Adapted from http://stackoverflow.com/questions/5620266/the-opposite-of-intersect</remarks>
		public static List<RomData> Diff(List<RomData> A, List<RomData> B)
		{
			List<String> AString = Output.RomDataToString(A);
			List<String> BString = Output.RomDataToString(B);
			List<String> CString = AString.Except(BString).Union(BString.Except(AString)).ToList();
			return Output.StringToRomData(CString);
		}

		/// <summary>
		/// Get all RomData objects that are in A but not in B
		/// </summary>
		/// <param name="A">First RomData list</param>
		/// <param name="B">Second RomData list</param>
		/// <returns>Any rom that's only in the first list</returns>
		/// <remarks>Adapted from http://stackoverflow.com/questions/5620266/the-opposite-of-intersect</remarks>
		public static List<RomData> DiffOnlyInA(List<RomData> A, List<RomData> B)
		{
			List<String> AString = Output.RomDataToString(A);
			List<String> BString = Output.RomDataToString(B);
			List<String> CString = AString.Except(BString).ToList();
			return Output.StringToRomData(CString);
		}

		/// <summary>
		/// Get all RomData objects that are in A and B
		/// </summary>
		/// <param name="A">First RomData list</param>
		/// <param name="B">Second RomData list</param>
		/// <returns>Any rom that's in both lists</returns>
		/// <remarks>Adapted from http://stackoverflow.com/questions/5620266/the-opposite-of-intersect</remarks>
		public static List<RomData> DiffInAB(List<RomData> A, List<RomData> B)
		{
			List<String> AString = Output.RomDataToString(A);
			List<String> BString = Output.RomDataToString(B);
			List<String> CString = AString.Union(BString).Except(AString.Except(BString).Union(BString.Except(AString))).ToList();
			return Output.StringToRomData(CString);
		}
	}
}
