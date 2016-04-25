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
				return new XmlTextReader(filename);
			}
			else
			{
				logger.Log("Non-XML DAT detected");
				StringReader sr = new StringReader(Converters.ClrMameProToXML(File.ReadAllLines(filename)).ToString());
				return new XmlTextReader(sr);
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
									Int64.TryParse(child.Attributes["size"].Value, out size);
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
													Int64.TryParse(data.Attributes["size"].Value, out size);
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
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>List of RomData objects representing the found data</returns>
		public static List<RomData> Parse2(string filename, int sysid, int srcid, Logger logger)
		{
			List<RomData> roms = new List<RomData>();
			XmlTextReader xtr = GetXmlTextReader(filename, logger);
			xtr.WhitespaceHandling = WhitespaceHandling.None;
			bool superdat = false;
			string parent = "";
			if (xtr != null)
			{
				xtr.MoveToContent();
				while (true)
				{
					// If we're at the end element, break
					if (xtr.NodeType == XmlNodeType.EndElement && xtr.Name == parent)
					{
						break;
					}

					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element && xtr.NodeType != XmlNodeType.Text)
					{
						xtr.Read();
						continue;
					}

					Console.WriteLine(xtr.Name + " " + xtr.NodeType);

					if (xtr.Name == "datafile" || xtr.Name == "softwarelist")
					{
						parent = xtr.Name;
						xtr.Read();
					}
					else if (xtr.Name == "header")
					{
						xtr.ReadToDescendant("name");
						superdat = (xtr.ReadElementContentAsString() != null ? xtr.ReadElementContentAsString().Contains(" - SuperDAT") : false);
					}
					else if (xtr.Name == "machine" || xtr.Name == "game" || xtr.Name == "software")
					{
						string temptype = xtr.Name;
						string tempname = "";
						if (temptype == "software")
						{
							xtr.ReadToDescendant("description");
							tempname = xtr.ReadElementContentAsString();
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

						// Get the roms from the machine
						if (xtr.ReadInnerXml() != null && xtr.ReadInnerXml() != "")
						{
							while (!(xtr.Name == temptype && xtr.NodeType == XmlNodeType.EndElement))
							{
								// If we find a rom or disk, add it
								if (xtr.Name == "rom" || xtr.Name == "disk")
								{
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

									roms.Add(new RomData
									{
										Game = tempname,
										Name = xtr.GetAttribute("name"),
										Type = xtr.Name,
										SystemID = sysid,
										SourceID = srcid,
										Size = size,
										CRC = (xtr.GetAttribute("crc") != null ? xtr.GetAttribute("crc").ToLowerInvariant().Trim() : ""),
										MD5 = (xtr.GetAttribute("md5") != null ? xtr.GetAttribute("md5").ToLowerInvariant().Trim() : ""),
										SHA1 = (xtr.GetAttribute("sha1") != null ? xtr.GetAttribute("sha1").ToLowerInvariant().Trim() : ""),
									});

									xtr.Read();

									// If we hit end of file, break
									if (xtr.EOF)
									{
										break;
									}
								}
								// If we find the signs of a software list, traverse the children
								else if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "part" && xtr.ReadInnerXml() != null && xtr.ReadInnerXml() != "")
								{
									while (!(xtr.Name == "part" && xtr.NodeType == XmlNodeType.EndElement))
									{
										// If we find a dataarea, traverse the children
										if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "dataarea")
										{
											while (!(xtr.Name == "dataarea" && xtr.NodeType == XmlNodeType.EndElement))
											{
												// If we find a rom or disk, add it
												if ((xtr.Name == "rom" || xtr.Name == "disk") && xtr.GetAttribute("name") != null)
												{
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

													roms.Add(new RomData
													{
														Game = tempname,
														Name = xtr.GetAttribute("name"),
														Type = xtr.Name,
														SystemID = sysid,
														SourceID = srcid,
														Size = size,
														CRC = (xtr.GetAttribute("crc") != null ? xtr.GetAttribute("crc").ToLowerInvariant().Trim() : ""),
														MD5 = (xtr.GetAttribute("md5") != null ? xtr.GetAttribute("md5").ToLowerInvariant().Trim() : ""),
														SHA1 = (xtr.GetAttribute("sha1") != null ? xtr.GetAttribute("sha1").ToLowerInvariant().Trim() : ""),
													});
												}

												xtr.Read();

												// If we hit end of file, break
												if (xtr.EOF)
												{
													break;
												}
											}
										}

										xtr.Read();

										// If we hit end of file, break
										if (xtr.EOF)
										{
											break;
										}
									}
								}
								else
								{
									xtr.Read();
								}
							}
						}
						xtr.ReadToNextSibling(temptype);
					}
					else
					{
						xtr.Read();
					}

					// If we hit end of file, break
					if (xtr.EOF)
					{
						break;
					}
					// If we're at the end element, break
					if (xtr.NodeType == XmlNodeType.EndElement && xtr.Name == parent)
					{
						break;
					}
				}
			}

			logger.Log("Outputting roms");

			return roms;
		}
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
