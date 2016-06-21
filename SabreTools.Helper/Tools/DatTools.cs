using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SabreTools.Helper
{
	public class DatTools
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
				StreamReader sr = File.OpenText(filename);
				string first = sr.ReadLine();
				sr.Close();
				sr.Dispose();
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
		public static Dat Parse(string filename, int sysid, int srcid, Dat datdata, Logger logger, bool keep = false, bool clean = false)
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
				datdata.Roms = new Dictionary<string, List<Rom>>();
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
		public static Dat ParseCMP(string filename, int sysid, int srcid, Dat datdata, Logger logger, bool keep, bool clean)
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
					gamename = (clean ? Style.CleanGameName(gamename) : gamename);

					Rom rom = new Rom
					{
						Game = gamename,
						Type = (line.Trim().StartsWith("disk (") ? "disk" : "rom"),
						Metadata = new SourceMetadata { SystemID = sysid, SourceID = srcid },
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
									rom.CRC = gc[i].Replace("\"", "").ToLowerInvariant();
									break;
								case "md5":
									rom.MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
									break;
								case "sha1":
									rom.SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
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
									rom.CRC = val.ToLowerInvariant();
									break;
								case "md5":
									rom.MD5 = val.ToLowerInvariant();
									break;
								case "sha1":
									rom.SHA1 = val.ToLowerInvariant();
									break;
							}

							quote = false;
							attrib = "";
							val = "";
						}
					}

					// Sanitize the hashes from null, hex sizes, and "true blank" strings
					rom.CRC = RomTools.CleanHashData(rom.CRC, Constants.CRCLength);
					rom.MD5 = RomTools.CleanHashData(rom.MD5, Constants.MD5Length);
					rom.SHA1 = RomTools.CleanHashData(rom.SHA1, Constants.SHA1Length);

					// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
					if (rom.Type == "rom" && (rom.Size == 0 || rom.Size == -1) && ((rom.CRC == Constants.CRCZero || rom.CRC == "") || rom.MD5 == Constants.MD5Zero || rom.SHA1 == Constants.SHA1Zero))
					{
						rom.Size = Constants.SizeZero;
						rom.CRC = Constants.CRCZero;
						rom.MD5 = Constants.MD5Zero;
						rom.SHA1 = Constants.SHA1Zero;
					}
					// If the file has no size and it's not the above case, skip and log
					else if (rom.Type == "rom" && (rom.Size == 0 || rom.Size == -1))
					{
						logger.Warning("Incomplete entry for \"" + rom.Name + "\" will be output as nodump");
						rom.Nodump = true;
					}

					// If we have a disk, make sure that the value for size is -1
					if (rom.Type == "disk")
					{
						rom.Size = -1;
					}

					// Now add the rom to the dictionary
					string key = rom.Size + "-" + rom.CRC;
					if (datdata.Roms.ContainsKey(key))
					{
						datdata.Roms[key].Add(rom);
					}
					else
					{
						List<Rom> templist = new List<Rom>();
						templist.Add(rom);
						datdata.Roms.Add(key, templist);
					}

					// Add statistical data
					datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
					datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
					datdata.TotalSize += (rom.Nodump ? 0 : rom.Size);
					datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
					datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
					datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
					datdata.NodumpCount += (rom.Nodump ? 1 : 0);
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
							case "header":
								datdata.Header = (String.IsNullOrEmpty(datdata.Header) ? itemval : datdata.Header);
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

			sr.Close();
			sr.Dispose();

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
		public static Dat ParseRC(string filename, int sysid, int srcid, Dat datdata, Logger logger, bool clean)
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
						rominfo[3] = (clean ? Style.CleanGameName(rominfo[3]) : rominfo[3]);

						Rom rom = new Rom
						{
							Game = rominfo[3],
							Name = rominfo[5],
							CRC = rominfo[6].ToLowerInvariant(),
							Size = Int64.Parse(rominfo[7]),
							Metadata = new SourceMetadata { SystemID = sysid, SourceID = srcid },
						};

						// Sanitize the hashes from null, hex sizes, and "true blank" strings
						rom.CRC = RomTools.CleanHashData(rom.CRC, Constants.CRCLength);
						rom.MD5 = RomTools.CleanHashData(rom.MD5, Constants.MD5Length);
						rom.SHA1 = RomTools.CleanHashData(rom.SHA1, Constants.SHA1Length);

						// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
						if (rom.Type == "rom" && (rom.Size == 0 || rom.Size == -1) && ((rom.CRC == Constants.CRCZero || rom.CRC == "") || rom.MD5 == Constants.MD5Zero || rom.SHA1 == Constants.SHA1Zero))
						{
							rom.Size = Constants.SizeZero;
							rom.CRC = Constants.CRCZero;
							rom.MD5 = Constants.MD5Zero;
							rom.SHA1 = Constants.SHA1Zero;
						}
						// If the file has no size and it's not the above case, skip and log
						else if (rom.Type == "rom" && (rom.Size == 0 || rom.Size == -1))
						{
							logger.Warning("Incomplete entry for \"" + rom.Name + "\" will be output as nodump");
							rom.Nodump = true;
						}

						// If we have a disk, make sure that the value for size is -1
						if (rom.Type == "disk")
						{
							rom.Size = -1;
						}

						// Add the new rom
						string key = rom.Size + "-" + rom.CRC;
						if (datdata.Roms.ContainsKey(key))
						{
							datdata.Roms[key].Add(rom);
						}
						else
						{
							List<Rom> templist = new List<Rom>();
							templist.Add(rom);
							datdata.Roms.Add(key, templist);
						}

						// Add statistical data
						datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
						datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
						datdata.TotalSize += (rom.Nodump ? 0 : rom.Size);
						datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
						datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
						datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
						datdata.NodumpCount += (rom.Nodump ? 1 : 0);
					}
				}
			}

			sr.Close();
			sr.Dispose();

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
		public static Dat ParseXML(string filename, int sysid, int srcid, Dat datdata, Logger logger, bool keep, bool clean)
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

							// If we're in cleaning mode, sanitize the game name
							tempgame = (clean ? Style.CleanGameName(tempgame) : tempgame);

							Rom rom = new Rom
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
								List<Rom> temp = new List<Rom>();
								temp.Add(rom);
								datdata.Roms.Add(key, temp);
							}

							// Add statistical data
							datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
							datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
							datdata.TotalSize += (rom.Nodump ? 0 : rom.Size);
							datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
							datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
							datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
							datdata.NodumpCount += (rom.Nodump ? 1 : 0);
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
											if (headreader.GetAttribute("header") != null)
											{
												datdata.Header = (String.IsNullOrEmpty(datdata.Header) ? headreader.GetAttribute("header") : datdata.Header);
											}
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
																		datdata.Type = (String.IsNullOrEmpty(datdata.Type) ? content : datdata.Type);
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
												Rom lastrom = datdata.Roms[key][index];
												lastrom.Size += size;
												datdata.Roms[key].RemoveAt(index);
												datdata.Roms[key].Add(lastrom);
												continue;
											}

											// Sanitize the hashes from null, hex sizes, and "true blank" strings
											crc = RomTools.CleanHashData(subreader.GetAttribute("crc"), Constants.CRCLength);
											md5 = RomTools.CleanHashData(subreader.GetAttribute("md5"), Constants.MD5Length);
											sha1 = RomTools.CleanHashData(subreader.GetAttribute("sha1"), Constants.SHA1Length);

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
												tempname = Style.CleanGameName(tempname.Split(Path.DirectorySeparatorChar));
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

												Rom rom = new Rom
												{
													Game = tempname,
													Name = subreader.GetAttribute("name"),
													Type = subreader.Name,
													Size = size,
													CRC = crc,
													MD5 = md5,
													SHA1 = sha1,
													Nodump = nodump,
													Date = date,
													Metadata = new SourceMetadata { SystemID = sysid, System = filename, SourceID = srcid },
												};

												if (datdata.Roms.ContainsKey(key))
												{
													datdata.Roms[key].Add(rom);
												}
												else
												{
													List<Rom> newvalue = new List<Rom>();
													newvalue.Add(rom);
													datdata.Roms.Add(key, newvalue);
												}

												// Add statistical data
												datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
												datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
												datdata.TotalSize += (rom.Nodump ? 0 : rom.Size);
												datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
												datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
												datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
												datdata.NodumpCount += (rom.Nodump ? 1 : 0);
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

								// If we're in cleaning mode, sanitize the game name
								tempname = (clean ? Style.CleanGameName(tempname.Split(Path.DirectorySeparatorChar)) : tempname);

								Rom rom = new Rom
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
									List<Rom> temp = new List<Rom>();
									temp.Add(rom);
									datdata.Roms.Add(key, temp);
								}

								// Add statistical data
								datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
								datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
								datdata.TotalSize += (rom.Nodump ? 0 : rom.Size);
								datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
								datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
								datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
								datdata.NodumpCount += (rom.Nodump ? 1 : 0);
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
								Rom lastrom = datdata.Roms[key][index];
								lastrom.Size += size;
								datdata.Roms[key].RemoveAt(index);
								datdata.Roms[key].Add(lastrom);
								continue;
							}

							// Sanitize the hashes from null, hex sizes, and "true blank" strings
							crc = RomTools.CleanHashData(xtr.GetAttribute("crc"), Constants.CRCLength);
							md5 = RomTools.CleanHashData(xtr.GetAttribute("md5"), Constants.MD5Length);
							sha1 = RomTools.CleanHashData(xtr.GetAttribute("sha1"), Constants.SHA1Length);

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

							// If we aren't keeping names, trim out the path
							if (!keep || !superdat)
							{
								string tempout = Regex.Match(tempname, @".*?\\(.*)").Groups[1].Value;
								if (tempout != "")
								{
									tempname = tempout;
								}
							}

							// If we're in cleaning mode, sanitize the game name
							tempname = (clean ? Style.CleanGameName(tempname) : tempname);

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

								Rom rom = new Rom
								{
									Game = tempname,
									Name = xtr.GetAttribute("name"),
									Type = xtr.GetAttribute("type"),
									Size = size,
									CRC = crc,
									MD5 = md5,
									SHA1 = sha1,
									Nodump = nodump,
									Date = date,
									Metadata = new SourceMetadata { SystemID = sysid, System = filename, SourceID = srcid },
								};

								if (datdata.Roms.ContainsKey(key))
								{
									datdata.Roms[key].Add(rom);
								}
								else
								{
									List<Rom> newvalue = new List<Rom>();
									newvalue.Add(rom);
									datdata.Roms.Add(key, newvalue);
								}

								// Add statistical data
								datdata.RomCount += (rom.Type == "rom" ? 1 : 0);
								datdata.DiskCount += (rom.Type == "disk" ? 1 : 0);
								datdata.TotalSize += (rom.Nodump ? 0 : rom.Size);
								datdata.CRCCount += (String.IsNullOrEmpty(rom.CRC) ? 0 : 1);
								datdata.MD5Count += (String.IsNullOrEmpty(rom.MD5) ? 0 : 1);
								datdata.SHA1Count += (String.IsNullOrEmpty(rom.SHA1) ? 0 : 1);
								datdata.NodumpCount += (rom.Nodump ? 1 : 0);
							}
							xtr.Read();
							break;
						default:
							xtr.Read();
							break;
					}
				}

				xtr.Close();
				xtr.Dispose();
			}

			return datdata;
		}

		/// <summary>
		/// Take an arbitrarily ordered List and return a Dictionary sorted by Game
		/// </summary>
		/// <param name="list">Input unsorted list</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by game name</returns>
		public static SortedDictionary<string, List<Rom>> BucketByGame(List<Rom> list, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			Dictionary<string, List<Rom>> dict = new Dictionary<string, List<Rom>>();
			dict.Add("key", list);
			return BucketByGame(dict, mergeroms, norename, logger, output);
		}

		/// <summary>
		/// Take an arbitrarily bucketed Dictionary and return one sorted by Game
		/// </summary>
		/// <param name="dict">Input unsorted dictionary</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by game name</returns>
		public static SortedDictionary<string, List<Rom>> BucketByGame(IDictionary<string, List<Rom>> dict, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			SortedDictionary<string, List<Rom>> sortable = new SortedDictionary<string, List<Rom>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (dict == null || dict.Count == 0)
			{
				return sortable;
			}
			
			// Process each all of the roms
			foreach (List<Rom> roms in dict.Values)
			{
				List<Rom> newroms = roms;
				if (mergeroms)
				{
					newroms = RomTools.Merge(newroms, logger);
				}

				foreach (Rom rom in newroms)
				{
					count++;
					string key = (norename ? "" : rom.Metadata.SystemID.ToString().PadLeft(10, '0') + "-" + rom.Metadata.SourceID.ToString().PadLeft(10, '0') + "-") + rom.Game.ToLowerInvariant();
					if (sortable.ContainsKey(key))
					{
						sortable[key].Add(rom);
					}
					else
					{
						List<Rom> temp = new List<Rom>();
						temp.Add(rom);
						sortable.Add(key, temp);
					}
				}
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}
			
			return sortable;
		}

		/// <summary>
		/// Take an arbitrarily ordered List and return a Dictionary sorted by size and hash
		/// </summary>
		/// <param name="list">Input unsorted list</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by size and hash</returns>
		public static SortedDictionary<string, List<Rom>> BucketByHashSize(List<Rom> list, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			Dictionary<string, List<Rom>> dict = new Dictionary<string, List<Rom>>();
			dict.Add("key", list);
			return BucketByHashSize(dict, mergeroms, norename, logger, output);
		}

		/// <summary>
		/// Take an arbitrarily bucketed Dictionary and return one sorted by size and hash
		/// </summary>
		/// <param name="dict">Input unsorted dictionary</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by size and hash</returns>
		public static SortedDictionary<string, List<Rom>> BucketByHashSize(IDictionary<string, List<Rom>> dict, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			SortedDictionary<string, List<Rom>> sortable = new SortedDictionary<string, List<Rom>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (dict == null || dict.Count == 0)
			{
				return sortable;
			}

			// Process each all of the roms
			foreach (List<Rom> roms in dict.Values)
			{
				List<Rom> newroms = roms;
				if (mergeroms)
				{
					newroms = RomTools.Merge(newroms, logger);
				}

				foreach (Rom rom in newroms)
				{
					count++;
					string key = rom.Size + "-" + rom.CRC; ;
					if (sortable.ContainsKey(key))
					{
						sortable[key].Add(rom);
					}
					else
					{
						List<Rom> temp = new List<Rom>();
						temp.Add(rom);
						sortable.Add(key, temp);
					}
				}
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			return sortable;
		}

		/// <summary>
		/// Convert, update, and filter a DAT file
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="datdata">User specified inputs contained in a DatData object</param>
		/// <param name="outputDirectory">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">True if the input files should be diffed with each other, false otherwise</param>
		/// <param name="cascade">True if the diffed files should be cascade diffed, false otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void Update(List<string> inputFileNames, Dat datdata, string outputDirectory, bool merge, bool diff, bool cascade, bool inplace,
			bool bare, bool clean, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc, string md5,
			string sha1, bool? nodump, bool trim, bool single, string root, Logger logger)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || diff)
			{
				// Make sure there are no folders in inputs
				List<string> newInputFileNames = new List<string>();
				foreach (string input in inputFileNames)
				{
					if (Directory.Exists(input))
					{
						foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
						{
							try
							{
								newInputFileNames.Add(Path.GetFullPath(file) + "¬" + Path.GetFullPath(input));
							}
							catch (PathTooLongException)
							{
								logger.Warning("The path for " + file + " was too long");
							}
							catch (Exception ex)
							{
								logger.Error(ex.ToString());
							}
						}
					}
					else if (File.Exists(input))
					{
						try
						{
							newInputFileNames.Add(Path.GetFullPath(input) + "¬" + Path.GetDirectoryName(Path.GetFullPath(input)));
						}
						catch (PathTooLongException)
						{
							logger.Warning("The path for " + input + " was too long");
						}
						catch (Exception ex)
						{
							logger.Error(ex.ToString());
						}
					}
				}

				// Create a dictionary of all ROMs from the input DATs
				datdata.FileName = datdata.Description;
				Dat userData;
				List<Dat> datHeaders = PopulateUserData(newInputFileNames, inplace, clean, outputDirectory, datdata, out userData, logger);

				// If we want to filter, apply it to the userData now
				userData = Filter(userData, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger);

				// Modify the Dictionary if necessary and output the results
				if (diff && !cascade)
				{
					DiffNoCascade(outputDirectory, userData, newInputFileNames, logger);
				}
				// If we're in cascade and diff, output only cascaded diffs
				else if (diff && cascade)
				{
					DiffCascade(outputDirectory, inplace, userData, newInputFileNames, datHeaders, logger);
				}
				// Output all entries with user-defined merge
				else
				{
					MergeNoDiff(outputDirectory, userData, newInputFileNames, datHeaders, logger);
				}
			}
			// Otherwise, loop through all of the inputs individually
			else
			{
				for (int i = 0; i < inputFileNames.Count; i++)
				{
					string inputFileName = inputFileNames[i];

					// Clean the input string
					if (inputFileName != "")
					{
						inputFileName = Path.GetFullPath(inputFileName);
					}

					if (File.Exists(inputFileName))
					{
						logger.User("Processing \"" + Path.GetFileName(inputFileName) + "\"");
						datdata = Parse(inputFileName, 0, 0, datdata, logger, true, clean);
						datdata = Filter(datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger);

						// If the extension matches, append ".new" to the filename
						string extension = (datdata.OutputFormat == OutputFormat.Xml || datdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
						if (outputDirectory == "" && Path.GetExtension(inputFileName) == extension)
						{
							datdata.FileName += ".new";
						}

						// If we have roms, output them
						if (datdata.Roms.Count != 0)
						{
							Output.WriteDatfile(datdata, (outputDirectory == "" ? Path.GetDirectoryName(inputFileName) : outputDirectory), logger);
						}
					}
					else if (Directory.Exists(inputFileName))
					{
						inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

						foreach (string file in Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories))
						{
							logger.User("Processing \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
							Dat innerDatdata = (Dat)datdata.Clone();
							innerDatdata.Roms = null;
							innerDatdata = Parse(file, 0, 0, innerDatdata, logger, true, clean);
							innerDatdata = Filter(innerDatdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger);

							// If the extension matches, append ".new" to the filename
							string extension = (innerDatdata.OutputFormat == OutputFormat.Xml || innerDatdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
							if (outputDirectory == "" && Path.GetExtension(file) == extension)
							{
								innerDatdata.FileName += ".new";
							}


							// If we have roms, output them
							if (innerDatdata.Roms.Count != 0)
							{
								Output.WriteDatfile(innerDatdata, (outputDirectory == "" ? Path.GetDirectoryName(file) : outputDirectory + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)), logger);
							}
						}
					}
					else
					{
						logger.Error("I'm sorry but " + inputFileName + " doesn't exist!");
					}
				}
			}
			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="userData">Output user DatData object to output</param>
		/// <returns>List of DatData objects representing headers</returns>
		private static List<Dat> PopulateUserData(List<string> inputs, bool inplace, bool clean, string outdir, Dat inputDat, out Dat userData, Logger logger)
		{
			List<Dat> datHeaders = new List<Dat>();

			int i = 0;
			userData = new Dat
			{
				Roms = new Dictionary<string, List<Rom>>(),
				MergeRoms = inputDat.MergeRoms,
			};
			foreach (string input in inputs)
			{
				logger.User("Adding DAT: " + input.Split('¬')[0]);
				userData = Parse(input.Split('¬')[0], i, 0, userData, logger, true, clean);
				i++;

				// If we are in inplace mode or redirecting output, save the DAT data
				if (inplace || !String.IsNullOrEmpty(outdir))
				{
					datHeaders.Add((Dat)userData.CloneHeader());

					// Reset the header values so the next can be captured
					Dictionary<string, List<Rom>> temp = userData.Roms;
					userData = new Dat();
					userData.Roms = temp;
				}
			}

			// Set the output values
			Dictionary<string, List<Rom>> roms = userData.Roms;
			userData = (Dat)inputDat.CloneHeader();
			userData.Roms = roms;

			return datHeaders;
		}

		/// <summary>
		/// Filter an input DAT file
		/// </summary>
		/// <param name="datdata">User specified inputs contained in a DatData object</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logging object for console and file output</param>
		/// <returns>Returns filtered DatData object</returns>
		public static Dat Filter(Dat datdata, string gamename, string romname, string romtype, long sgt,
			long slt, long seq, string crc, string md5, string sha1, bool? nodump, bool trim, bool single, string root, Logger logger)
		{
			// Now loop through and create a new Rom dictionary using filtered values
			Dictionary<string, List<Rom>> dict = new Dictionary<string, List<Rom>>();
			List<string> keys = datdata.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = datdata.Roms[key];
				for (int i = 0; i < roms.Count; i++)
				{
					Rom rom = roms[i];

					// Filter on nodump status
					if (nodump == true && !rom.Nodump)
					{
						continue;
					}
					if (nodump == false && rom.Nodump)
					{
						continue;
					}

					// Filter on game name
					if (gamename != "")
					{
						if (gamename.StartsWith("*") && gamename.EndsWith("*") && !rom.Game.ToLowerInvariant().Contains(gamename.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (gamename.StartsWith("*") && !rom.Game.EndsWith(gamename.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (gamename.EndsWith("*") && !rom.Game.StartsWith(gamename.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on rom name
					if (romname != "")
					{
						if (romname.StartsWith("*") && romname.EndsWith("*") && !rom.Name.ToLowerInvariant().Contains(romname.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (romname.StartsWith("*") && !rom.Name.EndsWith(romname.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (romname.EndsWith("*") && !rom.Name.StartsWith(romname.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on rom type
					if (romtype != "" && rom.Type.ToLowerInvariant() != romtype.ToLowerInvariant())
					{
						continue;
					}

					// Filter on rom size
					if (seq != -1 && rom.Size != seq)
					{
						continue;
					}
					else
					{
						if (sgt != -1 && rom.Size < sgt)
						{
							continue;
						}
						if (slt != -1 && rom.Size > slt)
						{
							continue;
						}
					}

					// Filter on crc
					if (crc != "")
					{
						if (crc.StartsWith("*") && crc.EndsWith("*") && !rom.CRC.ToLowerInvariant().Contains(crc.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (crc.StartsWith("*") && !rom.CRC.EndsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (crc.EndsWith("*") && !rom.CRC.StartsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on md5
					if (md5 != "")
					{
						if (md5.StartsWith("*") && md5.EndsWith("*") && !rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (md5.StartsWith("*") && !rom.MD5.EndsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (md5.EndsWith("*") && !rom.MD5.StartsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on sha1
					if (sha1 != "")
					{
						if (sha1.StartsWith("*") && sha1.EndsWith("*") && !rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (sha1.StartsWith("*") && !rom.SHA1.EndsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (sha1.EndsWith("*") && !rom.SHA1.StartsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// If we are in single game mode, rename all games
					if (single)
					{
						rom.Game = "!";
					}

					// If we are in NTFS trim mode, trim the game name
					if (trim)
					{
						// Windows max name length is 260
						int usableLength = 260 - rom.Game.Length - root.Length;
						if (rom.Name.Length > usableLength)
						{
							string ext = Path.GetExtension(rom.Name);
							rom.Name = rom.Name.Substring(0, usableLength - ext.Length);
							rom.Name += ext;
						}
					}

					// If it made it this far, add the rom to the output dictionary
					if (dict.ContainsKey(key))
					{
						dict[key].Add(rom);
					}
					else
					{
						List<Rom> temp = new List<Rom>();
						temp.Add(rom);
						dict.Add(key, temp);
					}
				}

				// Now clean up by removing the old list
				datdata.Roms[key] = null;
			}

			// Resassign the new dictionary to the DatData object
			datdata.Roms = dict;

			return datdata;
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="outdir">Output directory to write the DATs to</param>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void DiffNoCascade(string outdir, Dat userData, List<string> inputs, Logger logger)
		{
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			// Don't have External dupes
			string post = " (No Duplicates)";
			Dat outerDiffData = (Dat)userData.CloneHeader();
			outerDiffData.FileName += post;
			outerDiffData.Name += post;
			outerDiffData.Description += post;

			// Have External dupes
			post = " (Duplicates)";
			Dat dupeData = (Dat)userData.CloneHeader();
			dupeData.FileName += post;
			dupeData.Name += post;
			dupeData.Description += post;

			// Create a list of DatData objects representing individual output files
			List<Dat> outDats = new List<Dat>();

			// Loop through each of the inputs and get or create a new DatData object
			for (int j = 0; j < inputs.Count; j++)
			{
				post = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				Dat diffData = (Dat)userData.CloneHeader();
				diffData.FileName += post;
				diffData.Name += post;
				diffData.Description += post;
				outDats.Add(diffData);
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = userData.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = RomTools.Merge(userData.Roms[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						// No duplicates
						if (rom.Dupe < DupeType.ExternalHash)
						{
							// Individual DATs that are output
							if (outDats[rom.Metadata.SystemID].Roms.ContainsKey(key))
							{
								outDats[rom.Metadata.SystemID].Roms[key].Add(rom);
							}
							else
							{
								List<Rom> tl = new List<Rom>();
								tl.Add(rom);
								outDats[rom.Metadata.SystemID].Roms.Add(key, tl);
							}

							// Merged no-duplicates DAT
							Rom newrom = rom;
							newrom.Game += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.Metadata.SystemID].Split('¬')[0]) + ")";

							if (outerDiffData.Roms.ContainsKey(key))
							{
								outerDiffData.Roms[key].Add(newrom);
							}
							else
							{
								List<Rom> tl = new List<Rom>();
								tl.Add(rom);
								outerDiffData.Roms.Add(key, tl);
							}
						}

						// Duplicates only
						if (rom.Dupe >= DupeType.ExternalHash)
						{
							Rom newrom = rom;
							newrom.Game += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.Metadata.SystemID].Split('¬')[0]) + ")";

							if (dupeData.Roms.ContainsKey(key))
							{
								dupeData.Roms[key].Add(newrom);
							}
							else
							{
								List<Rom> tl = new List<Rom>();
								tl.Add(rom);
								dupeData.Roms.Add(key, tl);
							}
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");

			// Output the difflist (a-b)+(b-a) diff
			Output.WriteDatfile(outerDiffData, outdir, logger);

			// Output the (ab) diff
			Output.WriteDatfile(dupeData, outdir, logger);

			// Output the individual (a-b) DATs
			for (int j = 0; j < inputs.Count; j++)
			{
				// If we have an output directory set, replace the path
				string path = outdir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));

				// If we have more than 0 roms, output
				if (outDats[j].Roms.Count > 0)
				{
					Output.WriteDatfile(outDats[j], path, logger);
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output cascading diffs
		/// </summary>
		/// <param name="outdir">Output directory to write the DATs to</param>
		/// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void DiffCascade(string outdir, bool inplace, Dat userData, List<string> inputs, List<Dat> datHeaders, Logger logger)
		{
			string post = "";

			// Create a list of DatData objects representing output files
			List<Dat> outDats = new List<Dat>();

			// Loop through each of the inputs and get or create a new DatData object
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");
			for (int j = 0; j < inputs.Count; j++)
			{
				post = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				Dat diffData;

				// If we're in inplace mode, take the appropriate DatData object already stored
				if (inplace || !String.IsNullOrEmpty(outdir))
				{
					diffData = datHeaders[j];
				}
				else
				{
					diffData = (Dat)userData.CloneHeader();
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
				}
				outDats.Add(diffData);
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = userData.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = RomTools.Merge(userData.Roms[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						if (outDats[rom.Metadata.SystemID].Roms.ContainsKey(key))
						{
							outDats[rom.Metadata.SystemID].Roms[key].Add(rom);
						}
						else
						{
							List<Rom> tl = new List<Rom>();
							tl.Add(rom);
							outDats[rom.Metadata.SystemID].Roms.Add(key, tl);
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");
			for (int j = 0; j < inputs.Count; j++)
			{
				// If we have an output directory set, replace the path
				string path = "";
				if (inplace)
				{
					path = Path.GetDirectoryName(inputs[j].Split('¬')[0]);
				}
				else if (!String.IsNullOrEmpty(outdir))
				{
					path = outdir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));
				}

				// If we have more than 0 roms, output
				if (outDats[j].Roms.Count > 0)
				{
					Output.WriteDatfile(outDats[j], path, logger);
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output user defined merge
		/// </summary>
		/// <param name="outdir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void MergeNoDiff(string outdir, Dat userData, List<string> inputs, List<Dat> datHeaders, Logger logger)
		{
			// If we're in SuperDAT mode, prefix all games with their respective DATs
			if (userData.Type == "SuperDAT")
			{
				List<string> keys = userData.Roms.Keys.ToList();
				foreach (string key in keys)
				{
					List<Rom> newroms = new List<Rom>();
					foreach (Rom rom in userData.Roms[key])
					{
						Rom newrom = rom;
						string filename = inputs[newrom.Metadata.SystemID].Split('¬')[0];
						string rootpath = inputs[newrom.Metadata.SystemID].Split('¬')[1];

						rootpath += (rootpath == "" ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newrom.Game = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
							+ Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
							+ newrom.Game;
						newroms.Add(newrom);
					}
					userData.Roms[key] = newroms;
				}
			}

			// Output a DAT only if there are roms
			if (userData.Roms.Count != 0)
			{
				Output.WriteDatfile(userData, outdir, logger);
			}
		}
	}
}
