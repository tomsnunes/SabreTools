using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SabreTools.Helper
{
	public class RomManipulation
	{
		/// <summary>
		/// Return if the file is XML or not
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>True if the file is XML, false otherwise</returns>
		public static bool IsXmlDat(string filename)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(File.ReadAllText(filename));
			}
			catch (XmlException)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Get the XmlDocument associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The XmlDocument representing the (possibly converted) file, null otherwise</returns>
		public static XmlDocument GetXmlDocument(string filename, Logger logger)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(File.ReadAllText(filename));
			}
			catch (XmlException)
			{
				try
				{
					doc.LoadXml(Converters.RomVaultToXML(File.ReadAllLines(filename)).ToString());
				}
				catch (Exception ex)
				{
					logger.Error(ex.ToString());
					return null;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return null;
			}

			return doc;
		}

		/// <summary>
		/// Get the name of the DAT for external use
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The internal name of the DAT on success, empty string otherwise</returns>
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
				name = node.SelectSingleNode("name").InnerText;
			}

			return name;
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

			bool superdat = false;
			XmlDocument doc = GetXmlDocument(filename, logger);

			// If the returned document is null, return the empty list
			if (doc == null)
			{
				return roms;
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

			// Skip the header if it exists
			if (node != null && node.Name == "header")
			{
				// Check for SuperDAT mode
				if (node.SelectSingleNode("name").InnerText.Contains(" - SuperDAT"))
				{
					superdat = true;
				}

				// Skip over anything that's not an element
				while (node.NodeType != XmlNodeType.Element)
				{
					node = node.NextSibling;
				}
			}

			// Loop over the document until the end
			while (node != null)
			{
				if (node.NodeType == XmlNodeType.Element && (node.Name == "machine" || node.Name == "game" || node.Name == "software"))
				{
					string tempname = "";
					if (node.Name == "software")
					{
						tempname = node.SelectSingleNode("description").InnerText;
					}
					else
					{
						// There are rare cases where a malformed XML will not have the required attributes. We can only skip them.
						if (node.Attributes.Count == 0)
						{
							logger.Error("No attributes were found");
							node = node.NextSibling;
							continue;
						}
						tempname = node.Attributes["name"].Value;
					}

					if (superdat)
					{
						tempname = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
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

								roms.Add(new RomData
								{
									Game = tempname,
									Name = child.Attributes["name"].Value,
									Type = child.Name,
									SystemID = sysid,
									SourceID = srcid,
									Size = size,
									CRC = (child.Attributes["crc"] != null ? child.Attributes["crc"].Value.ToLowerInvariant().Trim() : ""),
									MD5 = (child.Attributes["md5"] != null ? child.Attributes["md5"].Value.ToLowerInvariant().Trim() : ""),
									SHA1 = (child.Attributes["sha1"] != null ? child.Attributes["sha1"].Value.ToLowerInvariant().Trim() : ""),
								});
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
												// Take care of hex-sized files
												long size = -1;
												if (data.Attributes["size"] != null && data.Attributes["size"].Value.Contains("0x"))
												{
													size = Convert.ToInt64(data.Attributes["size"].Value, 16);
												}
												else if (data.Attributes["size"] != null)
												{
													size = Int64.Parse(data.Attributes["size"].Value);
												}

												roms.Add(new RomData
												{
													Game = tempname,
													Name = data.Attributes["name"].Value,
													Type = data.Name,
													SystemID = sysid,
													SourceID = srcid,
													Size = size,
													CRC = (data.Attributes["crc"] != null ? data.Attributes["crc"].Value.ToLowerInvariant().Trim() : ""),
													MD5 = (data.Attributes["md5"] != null ? data.Attributes["md5"].Value.ToLowerInvariant().Trim() : ""),
													SHA1 = (data.Attributes["sha1"] != null ? data.Attributes["sha1"].Value.ToLowerInvariant().Trim() : ""),
												});
											}
										}
									}
								}
							}
						}
					}
				}
				node = node.NextSibling;
			}

			return roms;
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

			// First sort the roms by size, crc, sysid, srcid, md5, and sha1 (in order), if not sorted already
			if (!presorted)
			{
				inroms.Sort(delegate (RomData x, RomData y)
				{
					if (x.Size == y.Size)
					{
						if (x.CRC == y.CRC)
						{
							if (x.SystemID == y.SystemID)
							{
								if (x.SourceID == y.SourceID)
								{
									if (x.MD5 == y.MD5)
									{
										return String.Compare(x.SHA1, y.SHA1);
									}
									return String.Compare(x.MD5, y.MD5);
								}
								return x.SourceID - y.SourceID;
							}
							return x.SystemID - y.SystemID;
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
						shouldcont = ((rom.Size != -1 && rom.Size == last.Size) && (
								(rom.CRC != "" && last.CRC != "" && rom.CRC == last.CRC) ||
								(rom.MD5 != "" && last.MD5 != "" && rom.MD5 == last.MD5) ||
								(rom.SHA1 != "" && last.SHA1 != "" && rom.SHA1 == last.SHA1)
								)
							);
					}
					else if (rom.Type == "disk" && last.Type == "disk")
					{
						shouldcont = ((rom.MD5 != "" && last.MD5 != "" && rom.MD5 == last.MD5) ||
								(rom.SHA1 != "" && last.SHA1 != "" && rom.SHA1 == last.SHA1)
							);
					}

					// If it's a duplicate, skip adding it to the output but add any missing information
					if (shouldcont)
					{
						last.CRC = (last.CRC == "" && rom.CRC != "" ? rom.CRC : last.CRC);
						last.MD5 = (last.MD5 == "" && rom.MD5 != "" ? rom.MD5 : last.MD5);
						last.SHA1 = (last.SHA1 == "" && rom.SHA1 != "" ? rom.SHA1 : last.SHA1);

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
		public static void Sort(List<RomData> roms, bool norename)
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
	}
}
