using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SabreTools.Helper
{
	/// <summary>
	/// DAT manipulation tools that rely on HashData and related structs
	/// </summary>
	public class DatToolsHash
	{
		#region DAT Parsing

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
		public static DatData Parse(string filename, int sysid, int srcid, DatData datdata, Logger logger, bool keep = false, bool clean = false, bool softlist = false, bool keepext = false)
		{
			// Check the file extension first as a safeguard
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext != ".txt" && ext != ".dat" && ext != ".xml")
			{
				return datdata;
			}

			// If the output filename isn't set already, get the internal filename
			datdata.FileName = (String.IsNullOrEmpty(datdata.FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : datdata.FileName);

			// If the output type isn't set already, get the internal output type
			datdata.OutputFormat = (datdata.OutputFormat == OutputFormat.None ? DatTools.GetOutputFormat(filename) : datdata.OutputFormat);

			// Make sure there's a dictionary to read to
			if (datdata.Hashes == null)
			{
				datdata.Hashes = new List<HashData>();
			}

			// Now parse the correct type of DAT
			switch (DatTools.GetOutputFormat(filename))
			{
				case OutputFormat.ClrMamePro:
					return ParseCMP(filename, sysid, srcid, datdata, logger, keep, clean);
				case OutputFormat.RomCenter:
					return ParseRC(filename, sysid, srcid, datdata, logger, clean);
				case OutputFormat.SabreDat:
				case OutputFormat.Xml:
					return ParseXML(filename, sysid, srcid, datdata, logger, keep, clean, softlist);
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
			string blockname = "", gamename = "", gamedesc = "";
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

					RomData romData = new RomData
					{
						Type = (line.Trim().StartsWith("disk (") ? ItemType.Disk : ItemType.Rom),
						Machine = new MachineData
						{
							Name = gamename,
							Description = gamedesc,
							SystemID = sysid,
							SourceID = srcid,
						},
					};
					HashData hashData = new HashData
					{
						Roms = new List<RomData>(),
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
							romData.Nodump = true;
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
									romData.Name = gc[i].Replace("\"", "");
									break;
								case "size":
									Int64.TryParse(gc[i].Replace("\"", ""), out hashData.Size);
									break;
								case "crc":
									hashData.CRC = Style.StringToByteArray(gc[i].Replace("\"", ""));
									break;
								case "md5":
									hashData.MD5 = Style.StringToByteArray(gc[i].Replace("\"", ""));
									break;
								case "sha1":
									hashData.SHA1 = Style.StringToByteArray(gc[i].Replace("\"", ""));
									break;
								case "flags":
									if (gc[i].Replace("\"", "").ToLowerInvariant() == "nodump")
									{
										romData.Nodump = true;
									}
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
									romData.Name = val;
									break;
								case "size":
									Int64.TryParse(val, out hashData.Size);
									break;
								case "crc":
									hashData.CRC = Style.StringToByteArray(val);
									break;
								case "md5":
									hashData.MD5 = Style.StringToByteArray(val);
									break;
								case "sha1":
									hashData.SHA1 = Style.StringToByteArray(val);
									break;
								case "flags":
									if (val.ToLowerInvariant() == "nodump")
									{
										romData.Nodump = true;
									}
									break;
							}

							quote = false;
							attrib = "";
							val = "";
						}
					}

					// Sanitize the hashes from null, hex sizes, and "true blank" strings
					hashData.CRC = Style.CleanHashData(hashData.CRC, Constants.CRCBytesLength);
					hashData.MD5 = Style.CleanHashData(hashData.MD5, Constants.MD5BytesLength);
					hashData.SHA1 = Style.CleanHashData(hashData.SHA1, Constants.SHA1BytesLength);

					// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
					if (romData.Type == ItemType.Rom
						&& (hashData.Size == 0 || hashData.Size == -1)
						&& ((hashData.CRC == Constants.CRCZeroBytes || hashData.CRC == null)
							|| hashData.MD5 == Constants.MD5ZeroBytes
							|| hashData.SHA1 == Constants.SHA1ZeroBytes))
					{
						hashData.Size = Constants.SizeZero;
						hashData.CRC = Constants.CRCZeroBytes;
						hashData.MD5 = Constants.MD5ZeroBytes;
						hashData.SHA1 = Constants.SHA1ZeroBytes;
					}
					// If the file has no size and it's not the above case, skip and log
					else if (romData.Type == ItemType.Rom && (hashData.Size == 0 || hashData.Size == -1))
					{
						logger.Warning("Incomplete entry for \"" + romData.Name + "\" will be output as nodump");
						romData.Nodump = true;
					}

					// If we have a disk, make sure that the value for size is -1
					if (romData.Type == ItemType.Disk)
					{
						hashData.Size = -1;
					}

					// Now add the hash to the Dat
					if (datdata.Hashes.Contains(hashData))
					{
						datdata.Hashes[datdata.Hashes.IndexOf(hashData)].Roms.Add(romData);
					}
					else
					{
						hashData.Roms.Add(romData);
						datdata.Hashes.Add(hashData);
					}

					// Add statistical data
					datdata.RomCount += (romData.Type == ItemType.Rom ? 1 : 0);
					datdata.DiskCount += (romData.Type == ItemType.Disk ? 1 : 0);
					datdata.TotalSize += (romData.Nodump ? 0 : hashData.Size);
					datdata.CRCCount += (hashData.CRC == null ? 0 : 1);
					datdata.MD5Count += (hashData.MD5 == null ? 0 : 1);
					datdata.SHA1Count += (hashData.SHA1 == null ? 0 : 1);
					datdata.NodumpCount += (romData.Nodump ? 1 : 0);
				}
				// If the line is anything but a rom or disk and we're in a block
				else if (Regex.IsMatch(line, Constants.ItemPatternCMP) && block)
				{
					GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;

					if (gc[1].Value == "name" && blockname != "header")
					{
						gamename = gc[2].Value.Replace("\"", "");
					}
					else if (gc[1].Value == "description" && blockname != "header")
					{
						gamedesc = gc[2].Value.Replace("\"", "");
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
							case "rootdir":
								datdata.RootDir = (String.IsNullOrEmpty(datdata.RootDir) ? itemval : datdata.RootDir);
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
						rominfo[3] = (clean ? Style.CleanGameName(rominfo[3]) : rominfo[3]);

						RomData romData = new RomData
						{
							Name = rominfo[5],
							Machine = new MachineData
							{
								Name = rominfo[3],
								Description = rominfo[4],
								CloneOf = rominfo[1],
								RomOf = rominfo[1],
								SystemID = sysid,
								SourceID = srcid,
							},
						};
						HashData hashData = new HashData
						{
							Size = Int64.Parse(rominfo[7]),
							CRC = Style.StringToByteArray(rominfo[6]),
						};

						// Sanitize the hashes from null, hex sizes, and "true blank" strings
						hashData.CRC = Style.CleanHashData(hashData.CRC, Constants.CRCBytesLength);
						hashData.MD5 = Style.CleanHashData(hashData.MD5, Constants.MD5BytesLength);
						hashData.SHA1 = Style.CleanHashData(hashData.SHA1, Constants.SHA1BytesLength);

						// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
						if (romData.Type == ItemType.Rom
							&& (hashData.Size == 0 || hashData.Size == -1)
							&& ((hashData.CRC == Constants.CRCZeroBytes || hashData.CRC == null)
								|| hashData.MD5 == Constants.MD5ZeroBytes
								|| hashData.SHA1 == Constants.SHA1ZeroBytes))
						{
							hashData.Size = Constants.SizeZero;
							hashData.CRC = Constants.CRCZeroBytes;
							hashData.MD5 = Constants.MD5ZeroBytes;
							hashData.SHA1 = Constants.SHA1ZeroBytes;
						}
						// If the file has no size and it's not the above case, skip and log
						else if (romData.Type == ItemType.Rom && (hashData.Size == 0 || hashData.Size == -1))
						{
							logger.Warning("Incomplete entry for \"" + romData.Name + "\" will be output as nodump");
							romData.Nodump = true;
						}

						// If we have a disk, make sure that the value for size is -1
						if (romData.Type == ItemType.Disk)
						{
							hashData.Size = -1;
						}

						// Now add the hash to the Dat
						if (datdata.Hashes.Contains(hashData))
						{
							datdata.Hashes[datdata.Hashes.IndexOf(hashData)].Roms.Add(romData);
						}
						else
						{
							hashData.Roms.Add(romData);
							datdata.Hashes.Add(hashData);
						}

						// Add statistical data
						datdata.RomCount += (romData.Type == ItemType.Rom ? 1 : 0);
						datdata.DiskCount += (romData.Type == ItemType.Disk ? 1 : 0);
						datdata.TotalSize += (romData.Nodump ? 0 : hashData.Size);
						datdata.CRCCount += (hashData.CRC == null ? 0 : 1);
						datdata.MD5Count += (hashData.MD5 == null ? 0 : 1);
						datdata.SHA1Count += (hashData.SHA1 == null ? 0 : 1);
						datdata.NodumpCount += (romData.Nodump ? 1 : 0);
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
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		public static DatData ParseXML(string filename, int sysid, int srcid, DatData datdata, Logger logger, bool keep, bool clean, bool softlist)
		{
			// Prepare all internal variables
			XmlReader subreader, headreader, flagreader;
			bool superdat = false, nodump = false, empty = true;
			string crc = "", md5 = "", sha1 = "", date = "";
			long size = -1;
			List<string> parent = new List<string>();

			XmlTextReader xtr = DatTools.GetXmlTextReader(filename, logger);
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

							RomData romData = new RomData
							{
								Type = ItemType.Rom,
								Name = "null",
								Machine = new MachineData
								{
									Name = tempgame,
									Description = tempgame,
								},
							};
							HashData hashData = new HashData
							{
								Size = -1,
								CRC = null,
								MD5 = null,
								SHA1 = null,
								Roms = new List<RomData>(),
							};

							// Now add the hash to the Dat
							if (datdata.Hashes.Contains(hashData))
							{
								datdata.Hashes[datdata.Hashes.IndexOf(hashData)].Roms.Add(romData);
							}
							else
							{
								hashData.Roms.Add(romData);
								datdata.Hashes.Add(hashData);
							}

							// Add statistical data
							datdata.RomCount += (romData.Type == ItemType.Rom ? 1 : 0);
							datdata.DiskCount += (romData.Type == ItemType.Disk ? 1 : 0);
							datdata.TotalSize += (romData.Nodump ? 0 : hashData.Size);
							datdata.CRCCount += (hashData.CRC == null ? 0 : 1);
							datdata.MD5Count += (hashData.MD5 == null ? 0 : 1);
							datdata.SHA1Count += (hashData.SHA1 == null ? 0 : 1);
							datdata.NodumpCount += (romData.Nodump ? 1 : 0);
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
										case "rootdir":
											content = headreader.ReadElementContentAsString();
											datdata.RootDir = (String.IsNullOrEmpty(datdata.RootDir) ? content : datdata.RootDir);
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
							string tempname = "", gamedesc = "";

							// We want to process the entire subtree of the game
							subreader = xtr.ReadSubtree();

							// Safeguard for interesting case of "software" without anything except roms
							bool software = false;

							// If we have a subtree, add what is possible
							if (subreader != null)
							{
								if (!softlist && temptype == "software" && subreader.ReadToFollowing("description"))
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
										case "description":
											gamedesc = subreader.ReadElementContentAsString();
											break;
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
												int index = datdata.Hashes.Count() - 1;
												HashData lasthash = datdata.Hashes[index];
												lasthash.Size += size;
												datdata.Hashes.RemoveAt(index);
												datdata.Hashes.Add(lasthash);
												continue;
											}

											// Sanitize the hashes from null, hex sizes, and "true blank" strings
											crc = Style.CleanHashData(subreader.GetAttribute("crc"), Constants.CRCLength);
											md5 = Style.CleanHashData(subreader.GetAttribute("md5"), Constants.MD5Length);
											sha1 = Style.CleanHashData(subreader.GetAttribute("sha1"), Constants.SHA1Length);

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

												// Now add the hash to the Dat
												RomData romData = new RomData
												{
													Name = subreader.GetAttribute("name"),
													Type = (subreader.Name.ToLowerInvariant() == "disk" ? ItemType.Disk : ItemType.Rom),
													Nodump = nodump,
													Date = date,
													Machine = new MachineData
													{
														Name = tempname,
														Description = gamedesc,
														SystemID = sysid,
														System = filename,
														SourceID = srcid,
													},
												};
												HashData hashData = new HashData
												{
													Size = size,
													CRC = Style.CleanHashData(Style.StringToByteArray(crc), Constants.CRCBytesLength),
													MD5 = Style.CleanHashData(Style.StringToByteArray(md5), Constants.MD5BytesLength),
													SHA1 = Style.CleanHashData(Style.StringToByteArray(sha1), Constants.SHA1BytesLength),
													Roms = new List<RomData>(),
												};

												if (datdata.Hashes.Contains(hashData))
												{
													datdata.Hashes[datdata.Hashes.IndexOf(hashData)].Roms.Add(romData);
												}
												else
												{
													hashData.Roms.Add(romData);
													datdata.Hashes.Add(hashData);
												}

												// Add statistical data
												datdata.RomCount += (romData.Type == ItemType.Rom ? 1 : 0);
												datdata.DiskCount += (romData.Type == ItemType.Disk ? 1 : 0);
												datdata.TotalSize += (romData.Nodump ? 0 : hashData.Size);
												datdata.CRCCount += (hashData.CRC == null ? 0 : 1);
												datdata.MD5Count += (hashData.MD5 == null ? 0 : 1);
												datdata.SHA1Count += (hashData.SHA1 == null ? 0 : 1);
												datdata.NodumpCount += (romData.Nodump ? 1 : 0);
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

								// Now add the hash to the Dat
								RomData romData = new RomData
								{
									Type = ItemType.Rom,
									Name = "null",
									Machine = new MachineData
									{
										Name = tempname,
										Description = tempname,
									},
								};
								HashData hashData = new HashData
								{
									Size = -1,
									CRC = null,
									MD5 = null,
									SHA1 = null,
									Roms = new List<RomData>(),
								};

								if (datdata.Hashes.Contains(hashData))
								{
									datdata.Hashes[datdata.Hashes.IndexOf(hashData)].Roms.Add(romData);
								}
								else
								{
									hashData.Roms.Add(romData);
									datdata.Hashes.Add(hashData);
								}

								// Add statistical data
								datdata.RomCount += (romData.Type == ItemType.Rom ? 1 : 0);
								datdata.DiskCount += (romData.Type == ItemType.Disk ? 1 : 0);
								datdata.TotalSize += (romData.Nodump ? 0 : hashData.Size);
								datdata.CRCCount += (hashData.CRC == null ? 0 : 1);
								datdata.MD5Count += (hashData.MD5 == null ? 0 : 1);
								datdata.SHA1Count += (hashData.SHA1 == null ? 0 : 1);
								datdata.NodumpCount += (romData.Nodump ? 1 : 0);
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
								int index = datdata.Hashes.Count() - 1;
								HashData lasthash = datdata.Hashes[index];
								lasthash.Size += size;
								datdata.Hashes.RemoveAt(index);
								datdata.Hashes.Add(lasthash);
								continue;
							}

							// Sanitize the hashes from null, hex sizes, and "true blank" strings
							crc = Style.CleanHashData(xtr.GetAttribute("crc"), Constants.CRCLength);
							md5 = Style.CleanHashData(xtr.GetAttribute("md5"), Constants.MD5Length);
							sha1 = Style.CleanHashData(xtr.GetAttribute("sha1"), Constants.SHA1Length);

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

								// Now add the hash to the Dat
								RomData romData = new RomData
								{
									Name = xtr.GetAttribute("name"),
									Type = (xtr.GetAttribute("type").ToLowerInvariant() == "disk" ? ItemType.Disk : ItemType.Rom),
									Nodump = nodump,
									Date = date,
									Machine = new MachineData
									{
										Name = tempname,
										SystemID = sysid,
										System = filename,
										SourceID = srcid,
									},
								};
								HashData hashData = new HashData
								{
									Size = size,
									CRC = Style.CleanHashData(Style.StringToByteArray(crc), Constants.CRCBytesLength),
									MD5 = Style.CleanHashData(Style.StringToByteArray(md5), Constants.MD5BytesLength),
									SHA1 = Style.CleanHashData(Style.StringToByteArray(sha1), Constants.SHA1BytesLength),
									Roms = new List<RomData>(),
								};

								if (datdata.Hashes.Contains(hashData))
								{
									datdata.Hashes[datdata.Hashes.IndexOf(hashData)].Roms.Add(romData);
								}
								else
								{
									hashData.Roms.Add(romData);
									datdata.Hashes.Add(hashData);
								}

								// Add statistical data
								datdata.RomCount += (romData.Type == ItemType.Rom ? 1 : 0);
								datdata.DiskCount += (romData.Type == ItemType.Disk ? 1 : 0);
								datdata.TotalSize += (romData.Nodump ? 0 : hashData.Size);
								datdata.CRCCount += (hashData.CRC == null ? 0 : 1);
								datdata.MD5Count += (hashData.MD5 == null ? 0 : 1);
								datdata.SHA1Count += (hashData.SHA1 == null ? 0 : 1);
								datdata.NodumpCount += (romData.Nodump ? 1 : 0);
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

		#endregion

		#region Bucketing methods

		/// <summary>
		/// Take an arbitrarily ordered List and return a Dictionary sorted by Game
		/// </summary>
		/// <param name="list">Input unsorted list</param>
		/// <param name="mergeroms">True if roms should be deduped, false otherwise</param>
		/// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="output">True if the number of hashes counted is to be output (default), false otherwise</param>
		/// <returns>SortedDictionary bucketed by game name</returns>
		public static SortedDictionary<string, List<HashData>> BucketByGame(List<HashData> list, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			Dictionary<string, List<HashData>> dict = new Dictionary<string, List<HashData>>();
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
		/// <returns>SortedDictionary bucketed by game name</returns
		public static SortedDictionary<string, List<HashData>> BucketByGame(IDictionary<string, List<HashData>> dict, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			SortedDictionary<string, List<HashData>> sortable = new SortedDictionary<string, List<HashData>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (dict == null || dict.Count == 0)
			{
				return sortable;
			}

			// Process each all of the roms
			foreach (string key in dict.Keys)
			{
				List<HashData> hashes = dict[key];
				if (mergeroms)
				{
					hashes = RomTools.Merge(hashes, logger);
				}

				foreach (HashData hash in hashes)
				{
					count++;
					string newkey = (norename ? ""
										: hash.Roms[0].Machine.SystemID.ToString().PadLeft(10, '0')
											+ "-"
											+ hash.Roms[0].Machine.SourceID.ToString().PadLeft(10, '0') + "-")
											+ (String.IsNullOrEmpty(hash.Roms[0].Machine.Name)
												? ""
												: hash.Roms[0].Machine.Name.ToLowerInvariant());
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(hash);
					}
					else
					{
						List<HashData> temp = new List<HashData>();
						temp.Add(hash);
						sortable.Add(newkey, temp);
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
		public static SortedDictionary<string, List<HashData>> BucketByHashSize(List<HashData> list, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			Dictionary<string, List<HashData>> dict = new Dictionary<string, List<HashData>>();
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
		public static SortedDictionary<string, List<HashData>> BucketByHashSize(IDictionary<string, List<HashData>> dict, bool mergeroms, bool norename, Logger logger, bool output = true)
		{
			SortedDictionary<string, List<HashData>> sortable = new SortedDictionary<string, List<HashData>>();
			long count = 0;

			// If we have a null dict or an empty one, output a new dictionary
			if (dict == null || dict.Count == 0)
			{
				return sortable;
			}

			// Process each all of the roms
			foreach (List<HashData> hashes in dict.Values)
			{
				List<HashData> newhashes = hashes;
				if (mergeroms)
				{
					newhashes = RomTools.Merge(newhashes, logger);
				}

				foreach (HashData hash in newhashes)
				{
					count++;
					string key = hash.Size + "-" + BitConverter.ToString(hash.CRC).Replace("-", string.Empty);
					if (sortable.ContainsKey(key))
					{
						sortable[key].Add(hash);
					}
					else
					{
						List<HashData> temp = new List<HashData>();
						temp.Add(hash);
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

		#endregion

		#region Converting and updating

		#endregion
	}
}
