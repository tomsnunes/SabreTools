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
		/// Get what type of DAT the input file is
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The OutputFormat corresponding to the DAT</returns>
		/// <remarks>There is currently no differentiation between XML and SabreDAT here</remarks>
		public static OutputFormat GetOutputFormat(string filename)
		{
			try
			{
				StreamReader sr = new StreamReader(File.OpenRead(filename));
				string first = sr.ReadLine();
				sr.Close();
				if (first.Contains("<") && first.Contains(">"))
				{
					return OutputFormat.Xml;
				}
				else if (first.Contains("[") && first.Contains("]"))
				{
					return OutputFormat.RomCenter;
				}
				else
				{
					return OutputFormat.ClrMamePro;
				}
			}
			catch (Exception)
			{
				return OutputFormat.None;
			}
		}

		/// <summary>
		/// Get the XmlTextReader associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The XmlTextReader representing the (possibly converted) file, null otherwise</returns>
		public static XmlTextReader GetXmlTextReader(string filename, Logger logger)
		{
			logger.Log("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return null;
			}

			XmlTextReader xtr;
			xtr = new XmlTextReader(filename);
			xtr.WhitespaceHandling = WhitespaceHandling.None;
			xtr.DtdProcessing = DtdProcessing.Ignore;
			return xtr;
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		public static DatData Parse(string filename, int sysid, int srcid, DatData datdata, Logger logger, bool keep = false, bool clean = false)
		{
			// If the output filename isn't set already, get the internal filename
			if (String.IsNullOrEmpty(datdata.FileName))
			{
				datdata.FileName = Path.GetFileNameWithoutExtension(filename);
			}

			// If the output type isn't set already, get the internal output type
			if (datdata.OutputFormat == OutputFormat.None)
			{
				datdata.OutputFormat = GetOutputFormat(filename);
			}

			// Make sure there's a dictionary to read to
			if (datdata.Roms == null)
			{
				datdata.Roms = new Dictionary<string, List<RomData>>();
			}

			// Now parse the correct type of DAT
			switch (GetOutputFormat(filename))
			{
				case OutputFormat.ClrMamePro:
					return ParseCMP(filename, sysid, srcid, datdata, logger, keep, clean);
				case OutputFormat.RomCenter:
					return ParseRC(filename, sysid, srcid, datdata, logger, clean);
				case OutputFormat.SabreDat:
				case OutputFormat.Xml:
					return ParseXML(filename, sysid, srcid, datdata, logger, keep, clean);
				default:
					return datdata;
			}
		}

		/// <summary>
		/// Parse a ClrMamePro DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		public static DatData ParseCMP(string filename, int sysid, int srcid, DatData datdata, Logger logger, bool keep, bool clean)
		{
			// Read the input file, if possible
			logger.Log("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return datdata;
			}

			// If it does, open a file reader
			StreamReader sr = new StreamReader(File.OpenRead(filename));

			bool block = false, superdat = false;
			string blockname = "", gamename = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// Comments in CMP DATs start with a #
				if (line.Trim().StartsWith("#"))
				{
					continue;
				}

				// If the line is the header or a game
				if (Regex.IsMatch(line, Constants.HeaderPatternCMP))
				{
					GroupCollection gc = Regex.Match(line, Constants.HeaderPatternCMP).Groups;

					if (gc[1].Value == "clrmamepro" || gc[1].Value == "romvault")
					{
						blockname = "header";
					}

					block = true;
				}

				// If the line is a rom or disk and we're in a block
				else if ((line.Trim().StartsWith("rom (") || line.Trim().StartsWith("disk (")) && block)
				{
					// If we're in cleaning mode, sanitize the game name
					if (clean)
					{
						///Run the name through the filters to make sure that it's correct
						gamename = Style.NormalizeChars(gamename);
						gamename = Style.RussianToLatin(gamename);
						gamename = Style.SearchPattern(gamename);
					}

					RomData rom = new RomData
					{
						Game = gamename,
						Type = (line.Trim().StartsWith("disk (") ? "disk" : "rom"),
						SystemID = sysid,
						SourceID = srcid,
					};

					string[] gc = line.Trim().Split(' ');

					// Loop over all attributes and add them if possible
					bool quote = false;
					string attrib = "", val = "";
					for (int i = 2; i < gc.Length; i++)
					{
						//If the item is empty, we automatically skip it because it's a fluke
						if (gc[i].Trim() == String.Empty)
						{
							continue;
						}
						// Special case for nodump...
						else if (gc[i] == "nodump" && attrib != "status" && attrib != "flags")
						{
							rom.Nodump = true;
						}
						// Even number of quotes, not in a quote, not in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib == "")
						{
							attrib = gc[i].Replace("\"", "");
						}
						// Even number of quotes, not in a quote, in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && !quote && attrib != "")
						{
							switch (attrib.ToLowerInvariant())
							{
								case "name":
									rom.Name = gc[i].Replace("\"", "");
									break;
								case "size":
									Int64.TryParse(gc[i].Replace("\"", ""), out rom.Size);
									break;
								case "crc":
									rom.CRC = gc[i].Replace("\"", "");
									break;
								case "md5":
									rom.MD5 = gc[i].Replace("\"", "");
									break;
								case "sha1":
									rom.SHA1 = gc[i].Replace("\"", "");
									break;
							}

							attrib = "";
						}
						// Even number of quotes, in a quote, not in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && quote && attrib == "")
						{
							// Attributes can't have quoted names
						}
						// Even number of quotes, in a quote, in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 0 && quote && attrib != "")
						{
							val += " " + gc[i];
						}
						// Odd number of quotes, not in a quote, not in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && !quote && attrib == "")
						{
							// Attributes can't have quoted names
						}
						// Odd number of quotes, not in a quote, in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && !quote && attrib != "")
						{
							val = gc[i].Replace("\"", "");
							quote = true;
						}
						// Odd number of quotes, in a quote, not in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && quote && attrib == "")
						{
							quote = false;
						}
						// Odd number of quotes, in a quote, in attribute
						else if (Regex.Matches(gc[i], "\"").Count % 2 == 1 && quote && attrib != "")
						{
							val += " " + gc[i].Replace("\"", "");
							switch (attrib.ToLowerInvariant())
							{
								case "name":
									rom.Name = val;
									break;
								case "size":
									Int64.TryParse(val, out rom.Size);
									break;
								case "crc":
									rom.CRC = val;
									break;
								case "md5":
									rom.MD5 = val;
									break;
								case "sha1":
									rom.SHA1 = val;
									break;
							}

							quote = false;
							attrib = "";
							val = "";
						}
					}

					// Now add the rom to the dictionary
					string key = rom.Size + "-" + rom.CRC;
					if (datdata.Roms.ContainsKey(key))
					{
						datdata.Roms[key].Add(rom);
					}
					else
					{
						List<RomData> templist = new List<RomData>();
						templist.Add(rom);
						datdata.Roms.Add(key, templist);
					}
				}
				// If the line is anything but a rom or disk and we're in a block
				else if (Regex.IsMatch(line, Constants.ItemPatternCMP) && block)
				{
					GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;

					if (gc[1].Value == "name" && blockname != "header")
					{
						gamename = gc[2].Value.Replace("\"", "");
					}
					else
					{
						string itemval = gc[2].Value.Replace("\"", "");
						switch (gc[1].Value)
						{
							case "name":
								datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? itemval : datdata.Name);
								superdat = superdat || itemval.Contains(" - SuperDAT");
								if (keep && superdat)
								{
									datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? "SuperDAT" : datdata.Type);
								}
								break;
							case "description":
								datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? itemval : datdata.Description);
								break;
							case "category":
								datdata.Category = (String.IsNullOrEmpty(datdata.Category) ? itemval : datdata.Category);
								break;
							case "version":
								datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? itemval : datdata.Version);
								break;
							case "date":
								datdata.Date = (String.IsNullOrEmpty(datdata.Date) ? itemval : datdata.Date);
								break;
							case "author":
								datdata.Author = (String.IsNullOrEmpty(datdata.Author) ? itemval : datdata.Author);
								break;
							case "email":
								datdata.Email = (String.IsNullOrEmpty(datdata.Email) ? itemval : datdata.Email);
								break;
							case "homepage":
								datdata.Homepage = (String.IsNullOrEmpty(datdata.Homepage) ? itemval : datdata.Homepage);
								break;
							case "url":
								datdata.Url = (String.IsNullOrEmpty(datdata.Url) ? itemval : datdata.Url);
								break;
							case "comment":
								datdata.Comment = (String.IsNullOrEmpty(datdata.Comment) ? itemval : datdata.Comment);
								break;
							case "type":
								datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? itemval : datdata.Type);
								superdat = superdat || itemval.Contains("SuperDAT");
								break;
							case "forcemerging":
								switch (itemval)
								{
									case "none":
										datdata.ForceMerging = ForceMerging.None;
										break;
									case "split":
										datdata.ForceMerging = ForceMerging.Split;
										break;
									case "full":
										datdata.ForceMerging = ForceMerging.Full;
										break;
								}
								break;
							case "forcezipping":
								datdata.ForcePacking = (itemval == "yes" ? ForcePacking.Zip : ForcePacking.Unzip);
								break;
						}
					}
				}

				// If we find an end bracket that's not associated with anything else, the block is done
				else if (Regex.IsMatch(line, Constants.EndPatternCMP) && block)
				{
					block = false;
					blockname = "";
					gamename = "";
				}
			}

			return datdata;
		}

		/// <summary>
		/// Parse a RomCenter DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		public static DatData ParseRC(string filename, int sysid, int srcid, DatData datdata, Logger logger, bool clean)
		{
			// Read the input file, if possible
			logger.Log("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return datdata;
			}

			// If it does, open a file reader
			StreamReader sr = new StreamReader(File.OpenRead(filename));

			string blocktype = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// If the line is the start of the credits section
				if (line.ToLowerInvariant().Contains("[credits]"))
				{
					blocktype = "credits";
				}
				// If the line is the start of the dat section
				else if (line.ToLowerInvariant().Contains("[dat]"))
				{
					blocktype = "dat";
				}
				// If the line is the start of the emulator section
				else if (line.ToLowerInvariant().Contains("[emulator]"))
				{
					blocktype = "emulator";
				}
				// If the line is the start of the game section
				else if (line.ToLowerInvariant().Contains("[games]"))
				{
					blocktype = "games";
				}
				// Otherwise, it's not a section and it's data, so get out all data
				else
				{
					// If we have an author
					if (line.StartsWith("author="))
					{
						datdata.Author = (String.IsNullOrEmpty(datdata.Author) ? line.Split('=')[1] : datdata.Author);
					}
					// If we have one of the three version tags
					else if (line.StartsWith("version="))
					{
						switch (blocktype)
						{
							case "credits":
								datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? line.Split('=')[1] : datdata.Version);
								break;
							case "emulator":
								datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? line.Split('=')[1] : datdata.Description);
								break;
						}
					}
					// If we have a comment
					else if (line.StartsWith("comment="))
					{
						datdata.Comment = (String.IsNullOrEmpty(datdata.Comment) ? line.Split('=')[1] : datdata.Comment);
					}
					// If we have the split flag
					else if (line.StartsWith("split="))
					{
						int split = 0;
						if (Int32.TryParse(line.Split('=')[1], out split))
						{
							if (split == 1)
							{
								datdata.ForceMerging = ForceMerging.Split;
							}
						}
					}
					// If we have the merge tag
					else if (line.StartsWith("merge="))
					{
						int merge = 0;
						if (Int32.TryParse(line.Split('=')[1], out merge))
						{
							if (merge == 1)
							{
								datdata.ForceMerging = ForceMerging.Full;
							}
						}
					}
					// If we have the refname tag
					else if (line.StartsWith("refname="))
					{
						datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? line.Split('=')[1] : datdata.Name);
					}
					// If we have a rom
					else if (line.StartsWith("¬"))
					{
						/*
						The rominfo order is as follows:
						1 - parent name
						2 - parent description
						3 - game name
						4 - game description
						5 - rom name
						6 - rom crc
						7 - rom size
						8 - romof name
						9 - merge name
						*/
						string[] rominfo = line.Split('¬');

						// If we're in cleaning mode, sanitize the game name
						if (clean)
						{
							///Run the name through the filters to make sure that it's correct
							rominfo[3] = Style.NormalizeChars(rominfo[3]);
							rominfo[3] = Style.RussianToLatin(rominfo[3]);
							rominfo[3] = Style.SearchPattern(rominfo[3]);
						}

						RomData rom = new RomData
						{
							Game = rominfo[3],
							Name = rominfo[5],
							CRC = rominfo[6],
							Size = Int64.Parse(rominfo[7]),
							SystemID = sysid,
							SourceID = srcid,
						};

						// Add the new rom
						string key = rom.Size + "-" + rom.CRC;
						if (datdata.Roms.ContainsKey(key))
						{
							datdata.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> templist = new List<RomData>();
							templist.Add(rom);
							datdata.Roms.Add(key, templist);
						}
					}
				}
			}

			return datdata;
		}

		/// <summary>
		/// Parse an XML DAT (Logiqx, SabreDAT, or SL) and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		public static DatData ParseXML(string filename, int sysid, int srcid, DatData datdata, Logger logger, bool keep, bool clean)
		{
			// Prepare all internal variables
			XmlReader subreader, headreader, flagreader;
			bool superdat = false, nodump = false, empty = true;
			string key = "", crc = "", md5 = "", sha1 = "", date = "";
			long size = -1;
			List<string> parent = new List<string>();

			XmlTextReader xtr = GetXmlTextReader(filename, logger);
			if (xtr != null)
			{
				xtr.MoveToContent();
				while (!xtr.EOF)
				{
					// If we're ending a folder or game, take care of possibly empty games and removing from the parent
					if (xtr.NodeType == XmlNodeType.EndElement && (xtr.Name == "directory" || xtr.Name == "dir"))
					{
						// If we didn't find any items in the folder, make sure to add the blank rom
						if (empty)
						{
							string tempgame = String.Join("\\", parent);

							// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same if in clean mode
							if (clean)
							{
								string[] splitgame = tempgame.Split(Path.DirectorySeparatorChar);
								splitgame[splitgame.Length - 1] = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(splitgame[splitgame.Length - 1]).Groups[1].Value;
								tempgame = String.Join(Path.DirectorySeparatorChar.ToString(), splitgame);
								tempgame = tempgame.TrimStart().TrimEnd();
							}

							RomData rom = new RomData
							{
								Type = "rom",
								Name = "null",
								Game = tempgame,
								Size = -1,
								CRC = "null",
								MD5 = "null",
								SHA1 = "null",
							};

							key = rom.Size + "-" + rom.CRC;
							if (datdata.Roms.ContainsKey(key))
							{
								datdata.Roms[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
								temp.Add(rom);
								datdata.Roms.Add(key, temp);
							}
						}

						// Regardless, end the current folder
						int parentcount = parent.Count;
						if (parentcount == 0)
						{
							logger.Log("Empty parent: " + String.Join("\\", parent));
							empty = true;
						}

						// If we have an end folder element, remove one item from the parent, if possible
						if (parentcount > 0)
						{
							parent.RemoveAt(parent.Count - 1);
							if (keep && parentcount > 1)
							{
								datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? "SuperDAT" : datdata.Type);
								superdat = true;
							}
						}
					}

					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						// New software lists have this behavior
						case "softwarelist":
							if (xtr.GetAttribute("name") != null)
							{
								datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? xtr.GetAttribute("name") : datdata.Name);
							}
							if (xtr.GetAttribute("description") != null)
							{
								datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? xtr.GetAttribute("description") : datdata.Description);
							}
							xtr.Read();
							break;
						// Handle M1 DATs since they're 99% the same as a SL DAT
						case "m1":
							datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? "M1" : datdata.Name);
							datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? "M1" : datdata.Description);
							if (xtr.GetAttribute("version") != null)
							{
								datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? xtr.GetAttribute("version") : datdata.Version);
							}
							break;
						case "header":
							// We want to process the entire subtree of the header
							headreader = xtr.ReadSubtree();

							if (headreader != null)
							{
								while (!headreader.EOF)
								{
									// We only want elements
									if (headreader.NodeType != XmlNodeType.Element || headreader.Name == "header")
									{
										headreader.Read();
										continue;
									}

									// Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
									string content = "";
									switch (headreader.Name)
									{
										case "name":
											content = headreader.ReadElementContentAsString(); ;
											datdata.Name = (String.IsNullOrEmpty(datdata.Name) ? content : datdata.Name);
											superdat = superdat || content.Contains(" - SuperDAT");
											if (keep && superdat)
											{
												datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? "SuperDAT" : datdata.Type);
											}
											break;
										case "description":
											content = headreader.ReadElementContentAsString();
											datdata.Description = (String.IsNullOrEmpty(datdata.Description) ? content : datdata.Description);
											break;
										case "category":
											content = headreader.ReadElementContentAsString();
											datdata.Category = (String.IsNullOrEmpty(datdata.Category) ? content : datdata.Category);
											break;
										case "version":
											content = headreader.ReadElementContentAsString();
											datdata.Version = (String.IsNullOrEmpty(datdata.Version) ? content : datdata.Version);
											break;
										case "date":
											content = headreader.ReadElementContentAsString();
											datdata.Date = (String.IsNullOrEmpty(datdata.Date) ? content : datdata.Date);
											break;
										case "author":
											content = headreader.ReadElementContentAsString();
											datdata.Author = (String.IsNullOrEmpty(datdata.Author) ? content : datdata.Author);

											// Special cases for SabreDAT
											datdata.Email = (String.IsNullOrEmpty(datdata.Email) && !String.IsNullOrEmpty(headreader.GetAttribute("email")) ?
												headreader.GetAttribute("email") : datdata.Email);
											datdata.Homepage = (String.IsNullOrEmpty(datdata.Homepage) && !String.IsNullOrEmpty(headreader.GetAttribute("homepage")) ?
												headreader.GetAttribute("homepage") : datdata.Email);
											datdata.Url = (String.IsNullOrEmpty(datdata.Url) && !String.IsNullOrEmpty(headreader.GetAttribute("url")) ?
												headreader.GetAttribute("url") : datdata.Email);
											break;
										case "email":
											content = headreader.ReadElementContentAsString();
											datdata.Email = (String.IsNullOrEmpty(datdata.Email) ? content : datdata.Email);
											break;
										case "homepage":
											content = headreader.ReadElementContentAsString();
											datdata.Homepage = (String.IsNullOrEmpty(datdata.Homepage) ? content : datdata.Homepage);
											break;
										case "url":
											content = headreader.ReadElementContentAsString();
											datdata.Url = (String.IsNullOrEmpty(datdata.Url) ? content : datdata.Url);
											break;
										case "comment":
											content = headreader.ReadElementContentAsString();
											datdata.Comment = (String.IsNullOrEmpty(datdata.Comment) ? content : datdata.Comment);
											break;
										case "type":
											content = headreader.ReadElementContentAsString();
											datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? content : datdata.Type);
											superdat = superdat || content.Contains("SuperDAT");
											break;
										case "clrmamepro":
											if (headreader.GetAttribute("forcemerging") != null)
											{
												switch (headreader.GetAttribute("forcemerging"))
												{
													case "split":
														datdata.ForceMerging = ForceMerging.Split;
														break;
													case "none":
														datdata.ForceMerging = ForceMerging.None;
														break;
													case "full":
														datdata.ForceMerging = ForceMerging.Full;
														break;
												}
											}
											if (headreader.GetAttribute("forcenodump") != null)
											{
												switch (headreader.GetAttribute("forcenodump"))
												{
													case "obsolete":
														datdata.ForceNodump = ForceNodump.Obsolete;
														break;
													case "required":
														datdata.ForceNodump = ForceNodump.Required;
														break;
													case "ignore":
														datdata.ForceNodump = ForceNodump.Ignore;
														break;
												}
											}
											if (headreader.GetAttribute("forcepacking") != null)
											{
												switch (headreader.GetAttribute("forcepacking"))
												{
													case "zip":
														datdata.ForcePacking = ForcePacking.Zip;
														break;
													case "unzip":
														datdata.ForcePacking = ForcePacking.Unzip;
														break;
												}
											}
											headreader.Read();
											break;
										case "flags":
											flagreader = xtr.ReadSubtree();
											if (flagreader != null)
											{
												while (!flagreader.EOF)
												{
													// We only want elements
													if (flagreader.NodeType != XmlNodeType.Element || flagreader.Name == "flags")
													{
														flagreader.Read();
														continue;
													}

													switch (flagreader.Name)
													{
														case "flag":
															if (flagreader.GetAttribute("name") != null && flagreader.GetAttribute("value") != null)
															{
																content = flagreader.GetAttribute("value");
																switch (flagreader.GetAttribute("name"))
																{
																	case "type":
																		datdata.Type = (datdata.Type == "" ? content : datdata.Type);
																		superdat = superdat || content.Contains("SuperDAT");
																		break;
																	case "forcemerging":
																		switch (content)
																		{
																			case "split":
																				datdata.ForceMerging = ForceMerging.Split;
																				break;
																			case "none":
																				datdata.ForceMerging = ForceMerging.None;
																				break;
																			case "full":
																				datdata.ForceMerging = ForceMerging.Full;
																				break;
																		}
																		break;
																	case "forcenodump":
																		switch (content)
																		{
																			case "obsolete":
																				datdata.ForceNodump = ForceNodump.Obsolete;
																				break;
																			case "required":
																				datdata.ForceNodump = ForceNodump.Required;
																				break;
																			case "ignore":
																				datdata.ForceNodump = ForceNodump.Ignore;
																				break;
																		}
																		break;
																	case "forcepacking":
																		switch (content)
																		{
																			case "zip":
																				datdata.ForcePacking = ForcePacking.Zip;
																				break;
																			case "unzip":
																				datdata.ForcePacking = ForcePacking.Unzip;
																				break;
																		}
																		break;
																}
															}
															flagreader.Read();
															break;
														default:
															flagreader.Read();
															break;
													}
												}
											}
											headreader.Skip();
											break;
										default:
											headreader.Read();
											break;
									}
								}
							}

							// Skip the header node now that we've processed it
							xtr.Skip();
							break;
						case "machine":
						case "game":
						case "software":
							string temptype = xtr.Name;
							string tempname = "";

							// We want to process the entire subtree of the game
							subreader = xtr.ReadSubtree();

							// Safeguard for interesting case of "software" without anything except roms
							bool software = false;

							// If we have a subtree, add what is possible
							if (subreader != null)
							{
								if (temptype == "software" && subreader.ReadToFollowing("description"))
								{
									tempname = subreader.ReadElementContentAsString();
									tempname = tempname.Replace('/', '_').Replace("\"", "''");
									software = true;
								}
								else
								{
									// There are rare cases where a malformed XML will not have the required attributes. We can only skip them.
									if (xtr.AttributeCount == 0)
									{
										logger.Error("No attributes were found");
										xtr.Skip();
										continue;
									}
									tempname = xtr.GetAttribute("name");
								}

								if (superdat && !keep)
								{
									string tempout = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
									if (tempout != "")
									{
										tempname = tempout;
									}
								}
								// Get the name of the game from the parent
								else if (superdat && keep && parent.Count > 0)
								{
									tempname = String.Join("\\", parent) + "\\" + tempname;
								}

								while (software || subreader.Read())
								{
									software = false;

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
											empty = false;

											// If the rom is nodump, flag it
											nodump = false;
											if (subreader.GetAttribute("flags") == "nodump" || subreader.GetAttribute("status") == "nodump")
											{
												logger.Log("Nodump detected: " +
													(subreader.GetAttribute("name") != null && subreader.GetAttribute("name") != "" ? "\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
												nodump = true;
											}

											// If the rom has a Date attached, read it in and then sanitize it
											date = "";
											if (subreader.GetAttribute("date") != null)
											{
												date = DateTime.Parse(subreader.GetAttribute("date")).ToString();
											}

											// Take care of hex-sized files
											size = -1;
											if (subreader.GetAttribute("size") != null && subreader.GetAttribute("size").Contains("0x"))
											{
												size = Convert.ToInt64(subreader.GetAttribute("size"), 16);
											}
											else if (subreader.GetAttribute("size") != null)
											{
												Int64.TryParse(subreader.GetAttribute("size"), out size);
											}

											// If the rom is continue or ignore, add the size to the previous rom
											if (subreader.GetAttribute("loadflag") == "continue" || subreader.GetAttribute("loadflag") == "ignore")
											{
												int index = datdata.Roms[key].Count() - 1;
												RomData lastrom = datdata.Roms[key][index];
												lastrom.Size += size;
												datdata.Roms[key].RemoveAt(index);
												datdata.Roms[key].Add(lastrom);
												continue;
											}

											// Sanitize the hashes from null, hex sizes, and "true blank" strings
											crc = (subreader.GetAttribute("crc") != null ? subreader.GetAttribute("crc").ToLowerInvariant().Trim() : "");
											crc = (crc.StartsWith("0x") ? crc.Remove(0, 2) : crc);
											crc = (crc == "-" ? "" : crc);
											crc = (crc == "" ? "" : crc.PadLeft(8, '0'));
											md5 = (subreader.GetAttribute("md5") != null ? subreader.GetAttribute("md5").ToLowerInvariant().Trim() : "");
											md5 = (md5.StartsWith("0x") ? md5.Remove(0, 2) : md5);
											md5 = (md5 == "-" ? "" : md5);
											md5 = (md5 == "" ? "" : md5.PadLeft(32, '0'));
											sha1 = (subreader.GetAttribute("sha1") != null ? subreader.GetAttribute("sha1").ToLowerInvariant().Trim() : "");
											sha1 = (sha1.StartsWith("0x") ? sha1.Remove(0, 2) : sha1);
											sha1 = (sha1 == "-" ? "" : sha1);
											sha1 = (sha1 == "" ? "" : sha1.PadLeft(40, '0'));

											// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
											if (subreader.Name == "rom" && (size == 0 || size == -1) &&
												((crc == Constants.CRCZero || crc == "") || md5 == Constants.MD5Zero || sha1 == Constants.SHA1Zero))
											{
												size = Constants.SizeZero;
												crc = Constants.CRCZero;
												md5 = Constants.MD5Zero;
												sha1 = Constants.SHA1Zero;
											}
											// If the file has no size and it's not the above case, skip and log
											else if (subreader.Name == "rom" && (size == 0 || size == -1))
											{
												logger.Warning("Incomplete entry for \"" + subreader.GetAttribute("name") + "\" will be output as nodump");
												nodump = true;
											}

											// If we're in clean mode, sanitize the game name
											if (clean)
											{
												string[] splitgame = tempname.Split(Path.DirectorySeparatorChar);
												string intname = splitgame[splitgame.Length - 1];

												///Run the name through the filters to make sure that it's correct
												intname = Style.NormalizeChars(intname);
												intname = Style.RussianToLatin(intname);
												intname = Style.SearchPattern(intname);

												// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same if in clean mode
												splitgame[splitgame.Length - 1] = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(intname).Groups[1].Value;
												tempname = String.Join(Path.DirectorySeparatorChar.ToString(), splitgame);
												tempname = tempname.TrimStart().TrimEnd();
											}

											// Only add the rom if there's useful information in it
											if (!(crc == "" && md5 == "" && sha1 == "") || nodump)
											{
												// If we got to this point and it's a disk, log it because some tools don't like disks
												if (subreader.Name == "disk")
												{
													logger.Log("Disk found: \"" + subreader.GetAttribute("name") + "\"");
												}

												// Get the new values to add
												key = size + "-" + crc;

												RomData value = new RomData
												{
													Game = tempname,
													Name = subreader.GetAttribute("name"),
													Type = subreader.Name,
													SystemID = sysid,
													SourceID = srcid,
													Size = size,
													CRC = crc,
													MD5 = md5,
													SHA1 = sha1,
													System = filename,
													Nodump = nodump,
													Date = date,
												};

												if (datdata.Roms.ContainsKey(key))
												{
													datdata.Roms[key].Add(value);
												}
												else
												{
													List<RomData> newvalue = new List<RomData>();
													newvalue.Add(value);
													datdata.Roms.Add(key, newvalue);
												}
											}
											// Otherwise, log that it wasn't added
											else
											{
												logger.Log("Rom was not added: '" + xtr.GetAttribute("name") + "'");
											}
											break;
									}
								}
							}

							// If we didn't find any items in the folder, make sure to add the blank rom
							if (empty)
							{
								tempname = (parent.Count > 0 ? String.Join("\\", parent) + Path.DirectorySeparatorChar : "") + tempname;

								// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same if in clean mode
								if (clean)
								{
									string[] splitgame = tempname.Split(Path.DirectorySeparatorChar);
									splitgame[splitgame.Length - 1] = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(splitgame[splitgame.Length - 1]).Groups[1].Value;
									tempname = String.Join(Path.DirectorySeparatorChar.ToString(), splitgame);
									tempname = tempname.TrimStart().TrimEnd();
								}

								RomData rom = new RomData
								{
									Type = "rom",
									Name = "null",
									Game = tempname,
									Size = -1,
									CRC = "null",
									MD5 = "null",
									SHA1 = "null",
								};

								key = rom.Size + "-" + rom.CRC;
								if (datdata.Roms.ContainsKey(key))
								{
									datdata.Roms[key].Add(rom);
								}
								else
								{
									List<RomData> temp = new List<RomData>();
									temp.Add(rom);
									datdata.Roms.Add(key, temp);
								}
							}

							// Regardless, end the current folder
							if (parent.Count == 0)
							{
								empty = true;
							}
							xtr.Skip();
							break;
						case "dir":
						case "directory":
							// Set SuperDAT flag for all SabreDAT inputs, regardless of depth
							superdat = true;
							if (keep)
							{
								datdata.Type = (datdata.Type == "" ? "SuperDAT" : datdata.Type);
							}

							string foldername = (xtr.GetAttribute("name") == null ? "" : xtr.GetAttribute("name"));
							if (foldername != "")
							{
								parent.Add(foldername);
							}

							xtr.Read();
							break;
						case "file":
							empty = false;

							// If the rom is nodump, flag it
							nodump = false;
							flagreader = xtr.ReadSubtree();
							if (flagreader != null)
							{
								while (!flagreader.EOF)
								{
									// We only want elements
									if (flagreader.NodeType != XmlNodeType.Element || flagreader.Name == "flags")
									{
										flagreader.Read();
										continue;
									}

									switch (flagreader.Name)
									{
										case "flag":
										case "status":
											if (flagreader.GetAttribute("name") != null && flagreader.GetAttribute("value") != null)
											{
												string content = flagreader.GetAttribute("value");
												switch (flagreader.GetAttribute("name"))
												{
													case "nodump":
														logger.Log("Nodump detected: " + (xtr.GetAttribute("name") != null && xtr.GetAttribute("name") != "" ?
															"\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
														nodump = true;
														break;
												}
											}
											break;
									}

									flagreader.Read();
								}
							}

							// If the rom has a Date attached, read it in and then sanitize it
							date = "";
							if (xtr.GetAttribute("date") != null)
							{
								date = DateTime.Parse(xtr.GetAttribute("date")).ToString();
							}

							// Take care of hex-sized files
							size = -1;
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
								int index = datdata.Roms[key].Count() - 1;
								RomData lastrom = datdata.Roms[key][index];
								lastrom.Size += size;
								datdata.Roms[key].RemoveAt(index);
								datdata.Roms[key].Add(lastrom);
								continue;
							}

							// Sanitize the hashes from null, hex sizes, and "true blank" strings
							crc = (xtr.GetAttribute("crc") != null ? xtr.GetAttribute("crc").ToLowerInvariant().Trim() : "");
							crc = (crc.StartsWith("0x") ? crc.Remove(0, 2) : crc);
							crc = (crc == "-" ? "" : crc);
							crc = (crc == "" ? "" : crc.PadLeft(8, '0'));
							md5 = (xtr.GetAttribute("md5") != null ? xtr.GetAttribute("md5").ToLowerInvariant().Trim() : "");
							md5 = (md5.StartsWith("0x") ? md5.Remove(0, 2) : md5);
							md5 = (md5 == "-" ? "" : md5);
							md5 = (md5 == "" ? "" : md5.PadLeft(32, '0'));
							sha1 = (xtr.GetAttribute("sha1") != null ? xtr.GetAttribute("sha1").ToLowerInvariant().Trim() : "");
							sha1 = (sha1.StartsWith("0x") ? sha1.Remove(0, 2) : sha1);
							sha1 = (sha1 == "-" ? "" : sha1);
							sha1 = (sha1 == "" ? "" : sha1.PadLeft(40, '0'));

							// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
							if (xtr.GetAttribute("type") == "rom" && (size == 0 || size == -1) && ((crc == Constants.CRCZero || crc == "") || md5 == Constants.MD5Zero || sha1 == Constants.SHA1Zero))
							{
								size = Constants.SizeZero;
								crc = Constants.CRCZero;
								md5 = Constants.MD5Zero;
								sha1 = Constants.SHA1Zero;
							}
							// If the file has no size and it's not the above case, skip and log
							else if (xtr.GetAttribute("type") == "rom" && (size == 0 || size == -1))
							{
								logger.Warning("Incomplete entry for \"" + xtr.GetAttribute("name") + "\" will be output as nodump");
								nodump = true;
							}

							// Get the name of the game from the parent
							tempname = String.Join("\\", parent);

							// If we have a SuperDAT and we aren't keeping names
							if (superdat && !keep)
							{
								string tempout = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
								if (tempout != "")
								{
									tempname = tempout;
								}
							}

							// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same if in clean mode
							if (clean)
							{
								string[] splitgame = tempname.Split(Path.DirectorySeparatorChar);
								splitgame[splitgame.Length - 1] = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(splitgame[splitgame.Length - 1]).Groups[1].Value;
								tempname = String.Join(Path.DirectorySeparatorChar.ToString(), splitgame);
								tempname = tempname.TrimStart().TrimEnd();
							}

							// Only add the rom if there's useful information in it
							if (!(crc == "" && md5 == "" && sha1 == "") || nodump)
							{
								// If we got to this point and it's a disk, log it because some tools don't like disks
								if (xtr.GetAttribute("type") == "disk")
								{
									logger.Log("Disk found: \"" + xtr.GetAttribute("name") + "\"");
								}

								// Get the new values to add
								key = size + "-" + crc;

								RomData value = new RomData
								{
									Game = tempname,
									Name = xtr.GetAttribute("name"),
									Type = xtr.GetAttribute("type"),
									SystemID = sysid,
									SourceID = srcid,
									Size = size,
									CRC = crc,
									MD5 = md5,
									SHA1 = sha1,
									System = filename,
									Nodump = nodump,
									Date = date,
								};

								if (datdata.Roms.ContainsKey(key))
								{
									datdata.Roms[key].Add(value);
								}
								else
								{
									List<RomData> newvalue = new List<RomData>();
									newvalue.Add(value);
									datdata.Roms.Add(key, newvalue);
								}
							}
							xtr.Read();
							break;
						default:
							xtr.Read();
							break;
					}
				}
			}

			return datdata;
		}

		/// <summary>
		/// Merge an arbitrary set of ROMs based on the supplied information
		/// </summary>
		/// <param name="inroms">List of RomData objects representing the roms to be merged</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>A List of RomData objects representing the merged roms</returns>
		public static List<RomData> Merge(List<RomData> inroms, Logger logger)
		{
			List<RomData> outroms = new List<RomData>();

			// First sort the roms by size, crc, md5, sha1 (in order)
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

			// Then, deduplicate them by checking to see if data matches
			foreach (RomData rom in inroms)
			{
				// If it's a nodump, add and skip
				if (rom.Nodump)
				{
					outroms.Add(rom);
					continue;
				}

				// If it's the first rom in the list, don't touch it
				if (outroms.Count != 0)
				{
					// Check if the rom is a duplicate
					bool dupefound = false;
					RomData savedrom = new RomData();
					int pos = -1;
					for (int i = 0; i < outroms.Count; i++)
					{
						RomData lastrom = outroms[i];

						// If last is a nodump, skip it
						if (lastrom.Nodump)
						{
							continue;
						}

						if (rom.Type == "rom" && lastrom.Type == "rom")
						{
							dupefound = ((rom.Size == lastrom.Size) &&
								((rom.CRC == "" || lastrom.CRC == "") || rom.CRC == lastrom.CRC) &&
								((rom.MD5 == "" || lastrom.MD5 == "") || rom.MD5 == lastrom.MD5) &&
								((rom.SHA1 == "" || lastrom.SHA1 == "") || rom.SHA1 == lastrom.SHA1)
							);
						}
						else if (rom.Type == "disk" && lastrom.Type == "disk")
						{
							dupefound = (((rom.MD5 == "" || lastrom.MD5 == "") || rom.MD5 == lastrom.MD5) &&
								((rom.SHA1 == "" || lastrom.SHA1 == "") || rom.SHA1 == lastrom.SHA1)
							);
						}

						// If it's a duplicate, skip adding it to the output but add any missing information
						if (dupefound)
						{
							logger.Log("Rom information of found duplicate: " + rom.Game + "\t" + rom.Name + "\t" + rom.Size + "\t" + rom.CRC + "\t" + rom.MD5 + "\t" + rom.SHA1);

							savedrom = lastrom;
							pos = i;

							savedrom.CRC = (savedrom.CRC == "" && rom.CRC != "" ? rom.CRC : savedrom.CRC);
							savedrom.MD5 = (savedrom.MD5 == "" && rom.MD5 != "" ? rom.MD5 : savedrom.MD5);
							savedrom.SHA1 = (savedrom.SHA1 == "" && rom.SHA1 != "" ? rom.SHA1 : savedrom.SHA1);

							// If the duplicate is external already or should be, set it
							if (savedrom.Dupe >= DupeType.ExternalHash || savedrom.SystemID != rom.SystemID || savedrom.SourceID != rom.SourceID)
							{
								if (savedrom.Game == rom.Game && savedrom.Name == rom.Name)
								{
									savedrom.Dupe = DupeType.ExternalAll;
								}
								else
								{
									savedrom.Dupe = DupeType.ExternalHash;
								}
							}

							// Otherwise, it's considered an internal dupe
							else
							{
								if (savedrom.Game == rom.Game && savedrom.Name == rom.Name)
								{
									savedrom.Dupe = DupeType.InternalAll;
								}
								else
								{
									savedrom.Dupe = DupeType.InternalHash;
								}
							}

							// If the current system has a lower ID than the previous, set the system accordingly
							if (rom.SystemID < savedrom.SystemID)
							{
								savedrom.SystemID = rom.SystemID;
								savedrom.System = rom.System;
								savedrom.Game = rom.Game;
								savedrom.Name = rom.Name;
							}

							// If the current source has a lower ID than the previous, set the source accordingly
							if (rom.SourceID < savedrom.SourceID)
							{
								savedrom.SourceID = rom.SourceID;
								savedrom.Source = rom.Source;
								savedrom.Game = rom.Game;
								savedrom.Name = rom.Name;
							}

							break;
						}
					}

					// If no duplicate is found, add it to the list
					if (!dupefound)
					{
						outroms.Add(rom);
					}
					// Otherwise, if a new rom information is found, add that
					else
					{
						outroms.RemoveAt(pos);
						outroms.Insert(pos, savedrom);
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
		/// Take an arbitrarily bucketed Dictionary and return one sorted by Game
		/// </summary>
		/// <param name="dict">Input unsorted dictionary</param>
		/// <param name="mergeRoms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>SortedDictionary bucketed by game name</returns>
		public static SortedDictionary<string, List<RomData>> BucketByGame(Dictionary<string, List<RomData>> dict, bool mergeroms, bool norename, Logger logger)
		{
			SortedDictionary<string, List<RomData>> sortable = new SortedDictionary<string, List<RomData>>();
			long count = 0;
			foreach (List<RomData> roms in dict.Values)
			{
				List<RomData> newroms = roms;
				if (mergeroms)
				{
					newroms = RomManipulation.Merge(newroms, logger);
				}

				foreach (RomData rom in newroms)
				{
					count++;
					string key = (norename ? "" : rom.SystemID.ToString().PadLeft(10, '0') + "-" + rom.SourceID.ToString().PadLeft(10, '0') + "-") + rom.Game.ToLowerInvariant();
					if (sortable.ContainsKey(key))
					{
						sortable[key].Add(rom);
					}
					else
					{
						List<RomData> temp = new List<RomData>();
						temp.Add(rom);
						sortable.Add(key, temp);
					}
				}
			}

			logger.User("A total of " + count + " file hashes will be written out to file");
			return sortable;
		}
	}
}
