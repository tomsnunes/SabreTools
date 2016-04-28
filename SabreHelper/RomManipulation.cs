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
			XmlTextReader xtr = GetXmlTextReader(filename, logger);
			xtr.WhitespaceHandling = WhitespaceHandling.None;
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
							superdat = (xtr.ReadElementContentAsString() != null ? xtr.ReadElementContentAsString().Contains(" - SuperDAT") : false);
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
		/// <param name="merge">True if files should be matched by hash alone, false otherwise</param>
		/// <param name="dbc">Database connection for adding found ROMs</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>True if no errors occur, false otherwise</returns>
		/// <remarks>This doesn't have the same output as Parse + Merge OR even just Parse. Duplicates don't seem to be added either way, why?</remarks>
		public static bool ParseDb(string filename, int sysid, int srcid, bool merge, SqliteConnection dbc, Logger logger)
		{
			XmlTextReader xtr = GetXmlTextReader(filename, logger);
			xtr.WhitespaceHandling = WhitespaceHandling.None;
			bool superdat = false, shouldbreak = false;
			string parent = "";

			// If the reader is null, return false
			if (xtr == null)
			{
				return false;
			}

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
						superdat = (xtr.ReadElementContentAsString() != null ? xtr.ReadElementContentAsString().Contains(" - SuperDAT") : false);
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

										// If we're in merged mode, check before adding
										if (merge)
										{
											// If the rom doesn't exist, add it to the database
											string query = @"SELECT id FROM roms WHERE size=" + size +
	(xtr.GetAttribute("crc") != null ? " AND (crc='" + xtr.GetAttribute("crc").ToLowerInvariant().Trim() + "' OR crc='')" : "") +
	(xtr.GetAttribute("md5") != null ? " AND (md5='" + xtr.GetAttribute("md5").ToLowerInvariant().Trim() + "' OR md5='')" : "") +
	(xtr.GetAttribute("sha1") != null ? " AND (sha1='" + xtr.GetAttribute("sha1").ToLowerInvariant().Trim() + "' OR sha1='')" : "");

											using (SqliteCommand slc = new SqliteCommand(query, dbc))
											{
												using (SqliteDataReader sldr = slc.ExecuteReader())
												{
													// If there's no returns, then add the file
													if (!sldr.HasRows)
													{
														query = @"INSERT INTO roms 
(game, name, type, sysid, srcid, size, crc, md5, sha1, dupe)
VALUES ('" + tempname.Replace("'", "''") + "', '" +
			xtr.GetAttribute("name").Replace("'", "''") + "', '" +
			xtr.Name + "', " +
			sysid + ", " +
			srcid + ", " +
			size +
			(xtr.GetAttribute("crc") != null ? ", '" + xtr.GetAttribute("crc").ToLowerInvariant().Trim() + "'" : ", ''") +
			(xtr.GetAttribute("md5") != null ? ", '" + xtr.GetAttribute("md5").ToLowerInvariant().Trim() + "'" : ", ''") +
			(xtr.GetAttribute("sha1") != null ? ", '" + xtr.GetAttribute("sha1").ToLowerInvariant().Trim() + "'" : ", ''") +
			", '" + filename + "'" +
		")";
														using (SqliteCommand sslc = new SqliteCommand(query, dbc))
														{
															sslc.ExecuteNonQuery();
														}
													}
													// Otherwise, set the dupe flag to true
													else
													{
														query = @"UPDATE roms SET dupe='true' WHERE size=" + size +
	(xtr.GetAttribute("crc") != null ? " AND crc='" + xtr.GetAttribute("crc").ToLowerInvariant().Trim() + "'" : "") +
	(xtr.GetAttribute("md5") != null ? " AND md5='" + xtr.GetAttribute("md5").ToLowerInvariant().Trim() + "'" : "") +
	(xtr.GetAttribute("sha1") != null ? " AND sha1='" + xtr.GetAttribute("sha1").ToLowerInvariant().Trim() + "'" : "");

														using (SqliteCommand sslc = new SqliteCommand(query, dbc))
														{
															sslc.ExecuteNonQuery();
														}
													}
												}
											}
										}
										// If we're not in merged mode, just add it
										else
										{
											string query = @"INSERT INTO roms 
(game, name, type, sysid, srcid, size, crc, md5, sha1)
VALUES ('" + tempname.Replace("'", "''") + "', '" +
			xtr.GetAttribute("name").Replace("'", "''") + "', '" +
			xtr.Name + "', " +
			sysid + ", " +
			srcid + ", " +
			size +
			(xtr.GetAttribute("crc") != null ? ", '" + xtr.GetAttribute("crc").ToLowerInvariant().Trim() + "'" : ", ''") +
			(xtr.GetAttribute("md5") != null ? ", '" + xtr.GetAttribute("md5").ToLowerInvariant().Trim() + "'" : ", ''") +
			(xtr.GetAttribute("sha1") != null ? ", '" + xtr.GetAttribute("sha1").ToLowerInvariant().Trim() + "'" : ", ''") +
		")";
											using (SqliteCommand sslc = new SqliteCommand(query, dbc))
											{
												sslc.ExecuteNonQuery();
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

			return true;
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
