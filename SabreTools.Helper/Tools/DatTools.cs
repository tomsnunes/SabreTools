using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace SabreTools.Helper
{
	/// <summary>
	/// DAT manipulation tools that rely on Rom and related structs
	/// </summary>
	public class DatTools
	{
		#region DAT Parsing

		/// <summary>
		/// Get what type of DAT the input file is
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The OutputFormat corresponding to the DAT</returns>
		/// <remarks>There is currently no differentiation between XML and SabreDAT here</remarks>
		public static OutputFormat GetOutputFormat(string filename, Logger logger)
		{
			// Limit the output formats based on extension
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext != ".dat" && ext != ".xml")
			{
				return OutputFormat.None;
			}

			// Read the input file, if possible
			logger.Log("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return OutputFormat.None;
			}

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
		public static Dat Parse(string filename, int sysid, int srcid, Dat datdata, Logger logger, bool keep = false, bool clean = false, bool softlist = false, bool keepext = false)
		{
			return Parse(filename, sysid, srcid, datdata, null, null, null, -1, -1, -1, null, null, null, null, false, false, "", logger, keep, clean, softlist, keepext);
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
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
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		public static Dat Parse(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			Dat datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep = false,
			bool clean = false,
			bool softlist = false,
			bool keepext = false)
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
			datdata.OutputFormat = (datdata.OutputFormat == OutputFormat.None ? GetOutputFormat(filename, logger) : datdata.OutputFormat);

			// Make sure there's a dictionary to read to
			if (datdata.Files == null)
			{
				datdata.Files = new Dictionary<string, List<Rom>>();
			}

			// Now parse the correct type of DAT
			switch (GetOutputFormat(filename, logger))
			{
				case OutputFormat.ClrMamePro:
					return ParseCMP(filename, sysid, srcid, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, keep, clean);
				case OutputFormat.RomCenter:
					return ParseRC(filename, sysid, srcid, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, clean);
				case OutputFormat.SabreDat:
				case OutputFormat.Xml:
					return ParseXML(filename, sysid, srcid, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, keep, clean, softlist);
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
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		private static Dat ParseCMP(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			Dat datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep,
			bool clean)
		{
			// Open a file reader
			StreamReader sr = new StreamReader(File.OpenRead(filename));

			bool block = false, superdat = false;
			string blockname = "", tempgamename = "", gamedesc = "";
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
					Rom rom = new Rom
					{
						Machine = new Machine
						{
							Name = tempgamename,
							Description = gamedesc,
						},
						Type = (line.Trim().StartsWith("disk (") ? ItemType.Disk : ItemType.Rom),
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
									Int64.TryParse(gc[i].Replace("\"", ""), out rom.HashData.Size);
									break;
								case "crc":
									rom.HashData.CRC = gc[i].Replace("\"", "").ToLowerInvariant();
									break;
								case "md5":
									rom.HashData.MD5 = gc[i].Replace("\"", "").ToLowerInvariant();
									break;
								case "sha1":
									rom.HashData.SHA1 = gc[i].Replace("\"", "").ToLowerInvariant();
									break;
								case "flags":
									if (gc[i].Replace("\"", "").ToLowerInvariant() == "nodump")
									{
										rom.Nodump = true;
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
									rom.Name = val;
									break;
								case "size":
									Int64.TryParse(val, out rom.HashData.Size);
									break;
								case "crc":
									rom.HashData.CRC = val.ToLowerInvariant();
									break;
								case "md5":
									rom.HashData.MD5 = val.ToLowerInvariant();
									break;
								case "sha1":
									rom.HashData.SHA1 = val.ToLowerInvariant();
									break;
								case "flags":
									if (val.ToLowerInvariant() == "nodump")
									{
										rom.Nodump = true;
									}
									break;
							}

							quote = false;
							attrib = "";
							val = "";
						}
					}

					// Now process and add the rom
					string key = "";
					datdata = ParseAddHelper(rom, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);
				}
				// If the line is anything but a rom or disk and we're in a block
				else if (Regex.IsMatch(line, Constants.ItemPatternCMP) && block)
				{
					GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;

					if (gc[1].Value == "name" && blockname != "header")
					{
						tempgamename = gc[2].Value.Replace("\"", "");
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
					tempgamename = "";
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
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		private static Dat ParseRC(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			Dat datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
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

						Rom rom = new Rom
						{
							Machine = new Machine
							{
								Name = rominfo[3],
								Description = rominfo[4],
							},
							Name = rominfo[5],
							HashData = new Hash
							{
								CRC = rominfo[6].ToLowerInvariant(),
								Size = Int64.Parse(rominfo[7]),
							},
							Metadata = new SourceMetadata { SystemID = sysid, SourceID = srcid },
						};

						// Now process and add the rom
						string key = "";
						datdata = ParseAddHelper(rom, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);
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
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <returns>DatData object representing the read-in data</returns>
		private static Dat ParseXML(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			Dat datdata,

			// Rom filtering
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool keep,
			bool clean,
			bool softlist)
		{
			// Prepare all internal variables
			XmlReader subreader, headreader, flagreader;
			bool superdat = false, isnodump = false, empty = true;
			string key = "", date = "";
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

							Rom rom = new Rom
							{
								Type = ItemType.Rom,
								Name = "null",
								Machine = new Machine
								{
									Name = tempgame,
									Description = tempgame,
								},
								HashData = new Hash
								{
									Size = -1,
									CRC = "null",
									MD5 = "null",
									SHA1 = "null",
								},
							};

							// Now process and add the rom
							datdata = ParseAddHelper(rom, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);
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
						// We want to process the entire subtree of the header
						case "header":
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
								subreader.MoveToContent();
								if (!softlist && temptype == "software" && subreader.ReadToFollowing("description"))
								{
									tempname = subreader.ReadElementContentAsString();
									gamedesc = tempname;
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

								while (software || !subreader.EOF)
								{
									software = false;

									// We only want elements
									if (subreader.NodeType != XmlNodeType.Element)
									{
										subreader.Read();
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
											isnodump = false;
											if (subreader.GetAttribute("flags") == "nodump" || subreader.GetAttribute("status") == "nodump")
											{
												logger.Log("Nodump detected: " +
													(subreader.GetAttribute("name") != null && subreader.GetAttribute("name") != "" ? "\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
												isnodump = true;
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
												int index = datdata.Files[key].Count() - 1;
												Rom lastrom = datdata.Files[key][index];
												lastrom.HashData.Size += size;
												datdata.Files[key].RemoveAt(index);
												datdata.Files[key].Add(lastrom);
												subreader.Read();
												continue;
											}
											// If the rom has another type of loadflag, skip it completely
											else if (subreader.GetAttribute("loadflag") != null)
											{
												continue;
											}

											// If we're in clean mode, sanitize the game name
											if (clean)
											{
												tempname = Style.CleanGameName(tempname.Split(Path.DirectorySeparatorChar));
											}

											Rom inrom = new Rom
											{
												Machine = new Machine
												{
													Name = tempname,
													Description = gamedesc,
												},
												Name = subreader.GetAttribute("name"),
												Type = (subreader.Name.ToLowerInvariant() == "disk" ? ItemType.Disk : ItemType.Rom),
												HashData = new Hash
												{
													Size = size,
													CRC = subreader.GetAttribute("crc")?.ToLowerInvariant(),
													MD5 = subreader.GetAttribute("md5")?.ToLowerInvariant(),
													SHA1 = subreader.GetAttribute("sha1")?.ToLowerInvariant(),
												},
												Nodump = isnodump,
												Date = date,
												Metadata = new SourceMetadata { SystemID = sysid, System = filename, SourceID = srcid },
											};

											// Now process and add the rom
											datdata = ParseAddHelper(inrom, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

											subreader.Read();
											break;
										default:
											subreader.Read();
											break;
									}
								}
							}

							// If we didn't find any items in the folder, make sure to add the blank rom
							if (empty)
							{
								tempname = (parent.Count > 0 ? String.Join("\\", parent) + Path.DirectorySeparatorChar : "") + tempname;

								Rom inrom = new Rom
								{
									Type = ItemType.Rom,
									Name = "null",
									Machine = new Machine
									{
										Name = tempname,
										Description = tempname,
									},
									HashData = new Hash
									{
										Size = -1,
										CRC = "null",
										MD5 = "null",
										SHA1 = "null",
									}
								};

								// Now process and add the rom
								datdata = ParseAddHelper(inrom, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

								// Regardless, end the current folder
								if (parent.Count == 0)
								{
									empty = true;
								}
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
							isnodump = false;
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
														isnodump = true;
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
								int index = datdata.Files[key].Count() - 1;
								Rom lastrom = datdata.Files[key][index];
								lastrom.HashData.Size += size;
								datdata.Files[key].RemoveAt(index);
								datdata.Files[key].Add(lastrom);
								continue;
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

							Rom rom = new Rom
							{
								Machine = new Machine
								{
									Name = tempname,
								},
								Name = xtr.GetAttribute("name"),
								Type = (xtr.GetAttribute("type").ToLowerInvariant() == "disk" ? ItemType.Disk : ItemType.Rom),
								HashData = new Hash
								{
									Size = size,
									CRC = xtr.GetAttribute("crc")?.ToLowerInvariant(),
									MD5 = xtr.GetAttribute("md5")?.ToLowerInvariant(),
									SHA1 = xtr.GetAttribute("sha1")?.ToLowerInvariant(),
								},
								Nodump = isnodump,
								Date = date,
								Metadata = new SourceMetadata { SystemID = sysid, System = filename, SourceID = srcid },
							};

							// Now process and add the rom
							datdata = ParseAddHelper(rom, datdata, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single, root, clean, logger, out key);

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
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="rom">Rom data to check against</param>
		/// <param name="datdata">Dat to add information to, if possible</param>
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
		/// <param name="logger">Logger object for console and/or file output</param>
		private static Dat ParseAddHelper(Rom rom, Dat datdata, string gamename, string romname, string romtype, long sgt, long slt,
			long seq, string crc, string md5, string sha1, bool? nodump, bool trim, bool single, string root, bool clean, Logger logger, out string key)
		{
			key = "";

			// If there's no name in the rom, we log and skip it
			if (String.IsNullOrEmpty(rom.Name))
			{
				logger.Warning("Rom with no name found! Skipping...");
				return datdata;
			}

			// If we're in cleaning mode, sanitize the game name
			rom.Machine.Name = (clean ? Style.CleanGameName(rom.Machine.Name) : rom.Machine.Name);

			// Sanitize the hashes from null, hex sizes, and "true blank" strings
			rom.HashData.CRC = Style.CleanHashData(rom.HashData.CRC, Constants.CRCLength);
			rom.HashData.MD5 = Style.CleanHashData(rom.HashData.MD5, Constants.MD5Length);
			rom.HashData.SHA1 = Style.CleanHashData(rom.HashData.SHA1, Constants.SHA1Length);

			// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
			if (rom.Type == ItemType.Rom
				&& (rom.HashData.Size == 0 || rom.HashData.Size == -1)
				&& ((rom.HashData.CRC == Constants.CRCZero || rom.HashData.CRC == "")
					|| rom.HashData.MD5 == Constants.MD5Zero
					|| rom.HashData.SHA1 == Constants.SHA1Zero))
			{
				rom.HashData.Size = Constants.SizeZero;
				rom.HashData.CRC = Constants.CRCZero;
				rom.HashData.MD5 = Constants.MD5Zero;
				rom.HashData.SHA1 = Constants.SHA1Zero;
			}
			// If the file has no size and it's not the above case, skip and log
			else if (rom.Type == ItemType.Rom && (rom.HashData.Size == 0 || rom.HashData.Size == -1))
			{
				logger.Warning("Incomplete entry for \"" + rom.Name + "\" will be output as nodump");
				rom.Nodump = true;
			}

			// If the rom passes the filter, include it
			if (Filter(rom, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, logger))
			{
				// If we are in single game mode, rename all games
				if (single)
				{
					rom.Machine.Name = "!";
				}

				// If we are in NTFS trim mode, trim the game name
				if (trim)
				{
					// Windows max name length is 260
					int usableLength = 260 - rom.Machine.Name.Length - root.Length;
					if (rom.Name.Length > usableLength)
					{
						string ext = Path.GetExtension(rom.Name);
						rom.Name = rom.Name.Substring(0, usableLength - ext.Length);
						rom.Name += ext;
					}
				}

				// If we have a disk, make sure that the value for size is -1
				if (rom.Type == ItemType.Disk)
				{
					logger.Log("Disk found: \"" + rom.Name + "\"");
					rom.HashData.Size = -1;
				}

				lock (datdata.Files)
				{
					key = rom.HashData.Size + "-" + rom.HashData.CRC;
					if (datdata.Files.ContainsKey(key))
					{
						datdata.Files[key].Add(rom);
					}
					else
					{
						List<Rom> newvalue = new List<Rom>();
						newvalue.Add(rom);
						datdata.Files.Add(key, newvalue);
					}

					// Add statistical data
					datdata.RomCount += (rom.Type == ItemType.Rom ? 1 : 0);
					datdata.DiskCount += (rom.Type == ItemType.Disk ? 1 : 0);
					datdata.TotalSize += (rom.Nodump ? 0 : rom.HashData.Size);
					datdata.CRCCount += (String.IsNullOrEmpty(rom.HashData.CRC) ? 0 : 1);
					datdata.MD5Count += (String.IsNullOrEmpty(rom.HashData.MD5) ? 0 : 1);
					datdata.SHA1Count += (String.IsNullOrEmpty(rom.HashData.SHA1) ? 0 : 1);
					datdata.NodumpCount += (rom.Nodump ? 1 : 0);
				}
			}

			return datdata;
		}

		#endregion

		#region Bucketing

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
			foreach (string key in dict.Keys)
			{
				List<Rom> roms = dict[key];
				if (mergeroms)
				{
					roms = RomTools.Merge(roms, logger);
				}

				foreach (Rom rom in roms)
				{
					count++;
					string newkey = (norename ? ""
										: rom.Metadata.SystemID.ToString().PadLeft(10, '0')
											+ "-"
											+ rom.Metadata.SourceID.ToString().PadLeft(10, '0') + "-")
									+ (String.IsNullOrEmpty(rom.Machine.Name)
											? "Default"
											: rom.Machine.Name.ToLowerInvariant());
					if (sortable.ContainsKey(newkey))
					{
						sortable[newkey].Add(rom);
					}
					else
					{
						List<Rom> temp = new List<Rom>();
						temp.Add(rom);
						sortable.Add(newkey, temp);
					}
				}
			}

			// Now go through and sort all of the lists
			foreach (string key in sortable.Keys)
			{
				RomTools.Sort(sortable[key], norename);
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
					string key = rom.HashData.Size + "-" + rom.HashData.CRC;
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

			// Now go through and sort all of the lists
			foreach (string key in sortable.Keys)
			{
				RomTools.Sort(sortable[key], norename);
			}

			// Output the count if told to
			if (output)
			{
				logger.User("A total of " + count + " file hashes will be written out to file");
			}

			return sortable;
		}

		#endregion

		#region Converting and Updating

		/// <summary>
		/// Convert, update, and filter a DAT file
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="datdata">User specified inputs contained in a DatData object</param>
		/// <param name="outputDirectory">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="cascade">True if the diffed files should be cascade diffed, false if diffed files should be reverse cascaded, null otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="softlist">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
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
		public static void Update(List<string> inputFileNames, Dat datdata, string outputDirectory, bool merge, DiffMode diff, bool? cascade, bool inplace,
			bool skip, bool bare, bool clean, bool softlist, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc,
			string md5, string sha1, bool? nodump, bool trim, bool single, string root, Logger logger)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || diff != 0)
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

				// If we're in inverse cascade, reverse the list
				if (cascade == false)
				{
					newInputFileNames.Reverse();
				}

				// Create a dictionary of all ROMs from the input DATs
				Dat userData;
				List<Dat> datHeaders = PopulateUserData(newInputFileNames, inplace, clean, softlist,
					outputDirectory, datdata, out userData, gamename, romname, romtype, sgt, slt, seq,
					crc, md5, sha1, nodump, trim, single, root, logger);

				// Modify the Dictionary if necessary and output the results
				if (diff != 0 && cascade == null)
				{
					DiffNoCascade(diff, outputDirectory, userData, newInputFileNames, logger);
				}
				// If we're in cascade and diff, output only cascaded diffs
				else if (diff != 0 && cascade != null)
				{
					DiffCascade(outputDirectory, inplace, userData, newInputFileNames, datHeaders, skip, logger);
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
						Dat innerDatdata = (Dat)datdata.CloneHeader();
						logger.User("Processing \"" + Path.GetFileName(inputFileName) + "\"");
						innerDatdata = Parse(inputFileName, 0, 0, innerDatdata, gamename, romname,
							romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single,
							root, logger, true, clean, softlist, keepext:(innerDatdata.XSV != null));

						// If the extension matches, append ".new" to the filename
						string extension = (innerDatdata.OutputFormat == OutputFormat.Xml || innerDatdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
						if (outputDirectory == "" && Path.GetExtension(inputFileName) == extension)
						{
							innerDatdata.FileName += ".new";
						}

						// If we have roms, output them
						if (innerDatdata.Files.Count != 0)
						{
							WriteDatfile(innerDatdata, (outputDirectory == "" ? Path.GetDirectoryName(inputFileName) : outputDirectory), logger);
						}
					}
					else if (Directory.Exists(inputFileName))
					{
						inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

						foreach (string file in Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories))
						{
							logger.User("Processing \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
							Dat innerDatdata = (Dat)datdata.Clone();
							innerDatdata.Files = null;
							innerDatdata = Parse(file, 0, 0, innerDatdata, gamename, romname, romtype, sgt,
								slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, true, clean, keepext:(datdata.XSV != null));

							// If the extension matches, append ".new" to the filename
							string extension = (innerDatdata.OutputFormat == OutputFormat.Xml || innerDatdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
							if (outputDirectory == "" && Path.GetExtension(file) == extension)
							{
								innerDatdata.FileName += ".new";
							}

							// If we have roms, output them
							if (innerDatdata.Files != null && innerDatdata.Files.Count != 0)
							{
								WriteDatfile(innerDatdata, (outputDirectory == "" ? Path.GetDirectoryName(file) : outputDirectory + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)), logger);
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
		/// <returns>List of DatData objects representing headers</returns>
		private static List<Dat> PopulateUserData(List<string> inputs, bool inplace, bool clean, bool softlist, string outdir,
			Dat inputDat, out Dat userData, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc,
			string md5, string sha1, bool? nodump, bool trim, bool single, string root, Logger logger)
		{
			List<Dat> datHeaders = new List<Dat>();
			DateTime start = DateTime.Now;
			logger.User("Populating internal DAT");

			int i = 0;
			userData = new Dat
			{
				OutputFormat = (inputDat.OutputFormat != OutputFormat.None ? inputDat.OutputFormat : OutputFormat.None),
				Files = new Dictionary<string, List<Rom>>(),
				MergeRoms = inputDat.MergeRoms,
			};
			foreach (string input in inputs)
			{
				logger.User("Adding DAT: " + input.Split('¬')[0]);
				userData = Parse(input.Split('¬')[0], i, 0, userData, gamename, romname, romtype, sgt, slt, seq,
					crc, md5, sha1, nodump, trim, single, root, logger, true, clean, softlist);
				i++;

				// If we are in inplace mode or redirecting output, save the DAT data
				if (inplace || !String.IsNullOrEmpty(outdir))
				{
					datHeaders.Add((Dat)userData.CloneHeader());

					// Reset the header values so the next can be captured
					Dictionary<string, List<Rom>> temp = userData.Files;
					userData = new Dat
					{
						OutputFormat = (inputDat.OutputFormat != OutputFormat.None ? inputDat.OutputFormat : OutputFormat.None),
						Files = temp,
						MergeRoms = inputDat.MergeRoms,
					};
				}
			}

			// Set the output values
			Dictionary<string, List<Rom>> roms = userData.Files;
			userData = (Dat)inputDat.CloneHeader();
			userData.Files = roms;

			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datHeaders;
		}

		/// <summary>
		/// Determine if a rom should be included based on filters
		/// </summary>
		/// <param name="romdata">User supplied Rom to check</param>
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
		/// <param name="logger">Logging object for console and file output</param>
		/// <returns>Returns true if it should be included, false otherwise</returns>
		public static bool Filter(Rom romdata, string gamename, string romname, string romtype, long sgt,
			long slt, long seq, string crc, string md5, string sha1, bool? nodump, Logger logger)
		{
			// Filter on nodump status
			if (nodump == true && !romdata.Nodump)
			{
				return false;
			}
			if (nodump == false && romdata.Nodump)
			{
				return false;
			}

			// Filter on game name
			if (!String.IsNullOrEmpty(gamename))
			{
				if (gamename.StartsWith("*") && gamename.EndsWith("*"))
				{
					if (!romdata.Machine.Name.ToLowerInvariant().Contains(gamename.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (gamename.StartsWith("*"))
				{
					if (!romdata.Machine.Name.EndsWith(gamename.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (gamename.EndsWith("*"))
				{
					if (!romdata.Machine.Name.StartsWith(gamename.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (!String.Equals(romdata.Machine.Name, gamename, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			// Filter on rom name
			if (!String.IsNullOrEmpty(romname))
			{
				if (romname.StartsWith("*") && romname.EndsWith("*"))
				{
					if (!romdata.Name.ToLowerInvariant().Contains(romname.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (romname.StartsWith("*"))
				{
					if (!romdata.Name.EndsWith(romname.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (romname.EndsWith("*"))
				{
					if (!romdata.Name.StartsWith(romname.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (!String.Equals(romdata.Name, romname, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			// Filter on rom type
			if (!String.IsNullOrEmpty(romtype) && !String.Equals(romdata.Type.ToString(), romtype, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			// Filter on rom size
			if (seq != -1 && romdata.HashData.Size != seq)
			{
				return false;
			}
			else
			{
				if (sgt != -1 && romdata.HashData.Size < sgt)
				{
					return false;
				}
				if (slt != -1 && romdata.HashData.Size > slt)
				{
					return false;
				}
			}

			// Filter on crc
			if (!String.IsNullOrEmpty(crc))
			{
				if (crc.StartsWith("*") && crc.EndsWith("*"))
				{
					if (!romdata.HashData.CRC.ToLowerInvariant().Contains(crc.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (crc.StartsWith("*"))
				{
					if (!romdata.HashData.CRC.EndsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (crc.EndsWith("*"))
				{
					if (!romdata.HashData.CRC.StartsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (!String.Equals(romdata.HashData.CRC, crc, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			// Filter on md5
			if (!String.IsNullOrEmpty(md5))
			{
				if (md5.StartsWith("*") && md5.EndsWith("*"))
				{
					if (!romdata.HashData.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (md5.StartsWith("*"))
				{
					if (!romdata.HashData.MD5.EndsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (md5.EndsWith("*"))
				{
					if (!romdata.HashData.MD5.StartsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (!String.Equals(romdata.HashData.MD5, md5, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			// Filter on sha1
			if (!String.IsNullOrEmpty(sha1))
			{
				if (sha1.StartsWith("*") && sha1.EndsWith("*"))
				{
					if (!romdata.HashData.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (sha1.StartsWith("*"))
				{
					if (!romdata.HashData.SHA1.EndsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (sha1.EndsWith("*"))
				{
					if (!romdata.HashData.SHA1.StartsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (!String.Equals(romdata.HashData.SHA1, sha1, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="outdir">Output directory to write the DATs to</param>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void DiffNoCascade(DiffMode diff, string outdir, Dat userData, List<string> inputs, Logger logger)
		{
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			// Default vars for use
			string post = "";
			Dat outerDiffData = new Dat();
			Dat dupeData = new Dat();

			// Don't have External dupes
			if ((diff & DiffMode.NoDupes) != 0)
			{
				post = " (No Duplicates)";
				outerDiffData = (Dat)userData.CloneHeader();
				outerDiffData.FileName += post;
				outerDiffData.Name += post;
				outerDiffData.Description += post;
			}

			// Have External dupes
			if ((diff & DiffMode.Dupes) != 0)
			{
				post = " (Duplicates)";
				dupeData = (Dat)userData.CloneHeader();
				dupeData.FileName += post;
				dupeData.Name += post;
				dupeData.Description += post;
			}

			// Create a list of DatData objects representing individual output files
			List<Dat> outDats = new List<Dat>();

			// Loop through each of the inputs and get or create a new DatData object
			if ((diff & DiffMode.Individuals) != 0)
			{
				for (int j = 0; j < inputs.Count; j++)
				{
					post = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
					Dat diffData = (Dat)userData.CloneHeader();
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
					outDats.Add(diffData);
				}
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = userData.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = RomTools.Merge(userData.Files[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						// No duplicates
						if ((diff & DiffMode.NoDupes) != 0 || (diff & DiffMode.Individuals) != 0)
						{
							if (rom.Dupe < DupeType.ExternalHash)
							{
								// Individual DATs that are output
								if ((diff & DiffMode.Individuals) != 0)
								{
									if (outDats[rom.Metadata.SystemID].Files.ContainsKey(key))
									{
										outDats[rom.Metadata.SystemID].Files[key].Add(rom);
									}
									else
									{
										List<Rom> tl = new List<Rom>();
										tl.Add(rom);
										outDats[rom.Metadata.SystemID].Files.Add(key, tl);
									}
								}

								// Merged no-duplicates DAT
								if ((diff & DiffMode.NoDupes) != 0)
								{
									Rom newrom = rom;
									newrom.Machine.Name += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.Metadata.SystemID].Split('¬')[0]) + ")";

									if (outerDiffData.Files.ContainsKey(key))
									{
										outerDiffData.Files[key].Add(newrom);
									}
									else
									{
										List<Rom> tl = new List<Rom>();
										tl.Add(rom);
										outerDiffData.Files.Add(key, tl);
									}
								}
							}
						}

						// Duplicates only
						if ((diff & DiffMode.Dupes) != 0)
						{
							if (rom.Dupe >= DupeType.ExternalHash)
							{
								Rom newrom = rom;
								newrom.Machine.Name += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.Metadata.SystemID].Split('¬')[0]) + ")";

								if (dupeData.Files.ContainsKey(key))
								{
									dupeData.Files[key].Add(newrom);
								}
								else
								{
									List<Rom> tl = new List<Rom>();
									tl.Add(rom);
									dupeData.Files.Add(key, tl);
								}
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
			if ((diff & DiffMode.NoDupes) != 0)
			{
				WriteDatfile(outerDiffData, outdir, logger);
			}

			// Output the (ab) diff
			if ((diff & DiffMode.Dupes) != 0)
			{
				WriteDatfile(dupeData, outdir, logger);
			}

			// Output the individual (a-b) DATs
			if ((diff & DiffMode.Individuals) != 0)
			{
				for (int j = 0; j < inputs.Count; j++)
				{
					// If we have an output directory set, replace the path
					string path = outdir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));

					// If we have more than 0 roms, output
					if (outDats[j].Files.Count > 0)
					{
						WriteDatfile(outDats[j], path, logger);
					}
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
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void DiffCascade(string outdir, bool inplace, Dat userData, List<string> inputs, List<Dat> datHeaders, bool skip, Logger logger)
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
			List<string> keys = userData.Files.Keys.ToList();

			foreach (string key in keys)
			{
				List<Rom> roms = RomTools.Merge(userData.Files[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						if (outDats[rom.Metadata.SystemID].Files.ContainsKey(key))
						{
							outDats[rom.Metadata.SystemID].Files[key].Add(rom);
						}
						else
						{
							List<Rom> tl = new List<Rom>();
							tl.Add(rom);
							outDats[rom.Metadata.SystemID].Files.Add(key, tl);
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");
			for (int j = (skip ? 1 : 0); j < inputs.Count; j++)
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
				if (outDats[j].Files.Count > 0)
				{
					WriteDatfile(outDats[j], path, logger);
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
				List<string> keys = userData.Files.Keys.ToList();
				foreach (string key in keys)
				{
					List<Rom> newroms = new List<Rom>();
					foreach (Rom rom in userData.Files[key])
					{
						Rom newrom = rom;
						string filename = inputs[newrom.Metadata.SystemID].Split('¬')[0];
						string rootpath = inputs[newrom.Metadata.SystemID].Split('¬')[1];

						rootpath += (rootpath == "" ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newrom.Machine.Name = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
							+ Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
							+ newrom.Machine.Name;
						newroms.Add(newrom);
					}
					userData.Files[key] = newroms;
				}
			}

			// Output a DAT only if there are roms
			if (userData.Files.Count != 0)
			{
				WriteDatfile(userData, outdir, logger);
			}
		}

		#endregion

		#region Converting and Updating (Parallel)

		/// <summary>
		/// Convert, update, and filter a DAT file (Parallel)
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="datdata">User specified inputs contained in a DatData object</param>
		/// <param name="outputDirectory">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="cascade">True if the diffed files should be cascade diffed, false if diffed files should be reverse cascaded, null otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="softlist">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
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
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		public static void UpdateParallel(List<string> inputFileNames, Dat datdata, string outputDirectory, bool merge, DiffMode diff, bool? cascade, bool inplace,
			bool skip, bool bare, bool clean, bool softlist, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc,
			string md5, string sha1, bool? nodump, bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || diff != 0)
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

				// If we're in inverse cascade, reverse the list
				if (cascade == false)
				{
					newInputFileNames.Reverse();
				}

				// Create a dictionary of all ROMs from the input DATs
				Dat userData;
				List<Dat> datHeaders = PopulateUserData(newInputFileNames, inplace, clean, softlist,
					outputDirectory, datdata, out userData, gamename, romname, romtype, sgt, slt, seq,
					crc, md5, sha1, nodump, trim, single, root, logger);

				// Modify the Dictionary if necessary and output the results
				if (diff != 0 && cascade == null)
				{
					DiffNoCascade(diff, outputDirectory, userData, newInputFileNames, logger);
				}
				// If we're in cascade and diff, output only cascaded diffs
				else if (diff != 0 && cascade != null)
				{
					DiffCascade(outputDirectory, inplace, userData, newInputFileNames, datHeaders, skip, logger);
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
				Parallel.ForEach(inputFileNames,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
					inputFileName =>
				{
					// Clean the input string
					if (inputFileName != "")
					{
						inputFileName = Path.GetFullPath(inputFileName);
					}

					if (File.Exists(inputFileName))
					{
						Dat innerDatdata = (Dat)datdata.CloneHeader();
						logger.User("Processing \"" + Path.GetFileName(inputFileName) + "\"");
						innerDatdata = Parse(inputFileName, 0, 0, innerDatdata, gamename, romname,
							romtype, sgt, slt, seq, crc, md5, sha1, nodump, trim, single,
							root, logger, true, clean, softlist, keepext: (innerDatdata.XSV != null));

						// If the extension matches, append ".new" to the filename
						string extension = (innerDatdata.OutputFormat == OutputFormat.Xml || innerDatdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
						if (outputDirectory == "" && Path.GetExtension(inputFileName) == extension)
						{
							innerDatdata.FileName += ".new";
						}

						// If we have roms, output them
						if (innerDatdata.Files.Count != 0)
						{
							WriteDatfile(innerDatdata, (outputDirectory == "" ? Path.GetDirectoryName(inputFileName) : outputDirectory), logger);
						}
					}
					else if (Directory.Exists(inputFileName))
					{
						inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

						Parallel.ForEach(Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories),
							new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
							file =>
						{
							logger.User("Processing \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
							Dat innerDatdata = (Dat)datdata.Clone();
							innerDatdata.Files = null;
							innerDatdata = Parse(file, 0, 0, innerDatdata, gamename, romname, romtype, sgt,
								slt, seq, crc, md5, sha1, nodump, trim, single, root, logger, true, clean, keepext: (datdata.XSV != null));

							// If the extension matches, append ".new" to the filename
							string extension = (innerDatdata.OutputFormat == OutputFormat.Xml || innerDatdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
							if (outputDirectory == "" && Path.GetExtension(file) == extension)
							{
								innerDatdata.FileName += ".new";
							}

							// If we have roms, output them
							if (innerDatdata.Files != null && innerDatdata.Files.Count != 0)
							{
								WriteDatfile(innerDatdata, (outputDirectory == "" ? Path.GetDirectoryName(file) : outputDirectory + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)), logger);
							}
						});
					}
					else
					{
						logger.Error("I'm sorry but " + inputFileName + " doesn't exist!");
					}
				});
			}
			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="userData">Output user DatData object to output</param>
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
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		/// <returns>List of DatData objects representing headers</returns>
		private static List<Dat> PopulateUserDataParallel(List<string> inputs, bool inplace, bool clean, bool softlist, string outdir,
			Dat inputDat, out Dat userData, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc,
			string md5, string sha1, bool? nodump, bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			Dat[] datHeaders = new Dat[inputs.Count];
			DateTime start = DateTime.Now;
			logger.User("Processing individual DATs");

			userData = new Dat
			{
				OutputFormat = (inputDat.OutputFormat != OutputFormat.None ? inputDat.OutputFormat : OutputFormat.None),
				Files = new Dictionary<string, List<Rom>>(),
				MergeRoms = inputDat.MergeRoms,
			};

			Parallel.For(0, inputs.Count, i =>
			{
				string input = inputs[i];
				logger.User("Adding DAT: " + input.Split('¬')[0]);
				datHeaders[i] = new Dat
				{
					OutputFormat = (inputDat.OutputFormat != OutputFormat.None ? inputDat.OutputFormat : OutputFormat.None),
					Files = new Dictionary<string, List<Rom>>(),
					MergeRoms = inputDat.MergeRoms,
				};

				datHeaders[i] = Parse(input.Split('¬')[0], i, 0, datHeaders[i], gamename, romname, romtype, sgt, slt, seq,
					crc, md5, sha1, nodump, trim, single, root, logger, true, clean, softlist);
			});

			logger.User("Populating internal DAT");
			for (int i = 0; i < inputs.Count; i++)
			{
				foreach (string key in datHeaders[i].Files.Keys)
				{
					if (userData.Files.ContainsKey(key))
					{
						userData.Files[key].AddRange(datHeaders[i].Files[key]);
					}
					else
					{
						userData.Files.Add(key, datHeaders[i].Files[key]);
					}
				}
				datHeaders[i].Files = null;
			}

			// Set the output values
			Dictionary<string, List<Rom>> roms = userData.Files;
			userData = (Dat)inputDat.CloneHeader();
			userData.Files = roms;

			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datHeaders.ToList();
		}

		#endregion

		#region DAT Writing

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datdata">All information for creating the datfile header</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for file output:
		/// - Have the ability to strip special (non-ASCII) characters from rom information
		/// </remarks>
		public static bool WriteDatfile(Dat datdata, string outDir, Logger logger, bool norename = true, bool stats = false, bool ignoreblanks = false)
		{
			// If there's nothing there, abort
			if (datdata.Files == null || datdata.Files.Count == 0)
			{
				return false;
			}

			// If output directory is empty, use the current folder
			if (outDir.Trim() == "")
			{
				outDir = Environment.CurrentDirectory;
			}

			// Create the output directory if it doesn't already exist
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// If the DAT has no output format, default to XML
			if (datdata.OutputFormat == OutputFormat.None)
			{
				datdata.OutputFormat = OutputFormat.Xml;
			}

			// Make sure that the three essential fields are filled in
			if (String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Name = datdata.Description = "Default";
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Name = datdata.Description;
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Description = datdata.Name;
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Description;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Name = datdata.Description = datdata.FileName;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Name = datdata.Description;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Description = datdata.Name;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				// Nothing is needed
			}

			// Output initial statistics, for kicks
			if (stats)
			{
				Stats.OutputStats(datdata, logger, (datdata.RomCount + datdata.DiskCount == 0));
			}

			// Bucket roms by game name and optionally dedupe
			SortedDictionary<string, List<Rom>> sortable = BucketByGame(datdata.Files, datdata.MergeRoms, norename, logger);

			// Get the outfile name
			string outfile = Style.CreateOutfileName(outDir, datdata);

			logger.User("Opening file for writing: " + outfile);

			try
			{
				FileStream fs = File.Create(outfile);
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				// Write out the header
				WriteHeader(sw, datdata, logger);

				// Write out each of the machines and roms
				int depth = 2, last = -1;
				string lastgame = null;
				List<string> splitpath = new List<string>();

				// Get a properly sorted set of keys
				List<string> keys = sortable.Keys.ToList();
				keys.Sort(Style.CompareNumeric);

				foreach (string key in keys)
				{
					List<Rom> roms = sortable[key];

					for (int index = 0; index < roms.Count; index++)
					{
						Rom rom = roms[index];
						List<string> newsplit = rom.Machine.Name.Split('\\').ToList();

						// If we have a different game and we're not at the start of the list, output the end of last item
						if (lastgame != null && lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
						{
							depth = WriteEndGame(sw, rom, splitpath, newsplit, lastgame, datdata, depth, out last, logger);
						}

						// If we have a new game, output the beginning of the new item
						if (lastgame == null || lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
						{
							depth = WriteStartGame(sw, rom, newsplit, lastgame, datdata, depth, last, logger);
						}

						// If we have a "null" game (created by DATFromDir or something similar), log it to file
						if (rom.HashData.Size == -1 && rom.HashData.CRC == "null" && rom.HashData.MD5 == "null" && rom.HashData.SHA1 == "null")
						{
							logger.Log("Empty folder found: " + rom.Machine.Name);

							// If we're in a mode that doesn't allow for actual empty folders, add the blank info
							if (datdata.OutputFormat != OutputFormat.SabreDat && datdata.OutputFormat != OutputFormat.MissFile)
							{
								rom.Name = (rom.Name == "null" ? "-" : rom.Name);
								rom.HashData.Size = Constants.SizeZero;
								rom.HashData.CRC = Constants.CRCZero;
								rom.HashData.MD5 = Constants.MD5Zero;
								rom.HashData.SHA1 = Constants.SHA1Zero;
							}

							// Otherwise, set the new path and such, write out, and continue
							else
							{
								splitpath = newsplit;
								lastgame = rom.Machine.Name;
								continue;
							}
						}

						// Now, output the rom data
						WriteRomData(sw, rom, lastgame, datdata, depth, logger, ignoreblanks);

						// Set the new data to compare against
						splitpath = newsplit;
						lastgame = rom.Machine.Name;
					}
				}

				// Write the file footer out
				WriteFooter(sw, datdata, depth, logger);

				logger.Log("File written!" + Environment.NewLine);
				sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT header using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteHeader(StreamWriter sw, Dat datdata, Logger logger)
		{
			try
			{
				string header = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						header = "clrmamepro (\n" +
							"\tname \"" + datdata.Name + "\"\n" +
							"\tdescription \"" + datdata.Description + "\"\n" +
							"\tcategory \"" + datdata.Category + "\"\n" +
							"\tversion \"" + datdata.Version + "\"\n" +
							"\tdate \"" + datdata.Date + "\"\n" +
							"\tauthor \"" + datdata.Author + "\"\n" +
							"\temail \"" + datdata.Email + "\"\n" +
							"\thomepage \"" + datdata.Homepage + "\"\n" +
							"\turl \"" + datdata.Url + "\"\n" +
							"\tcomment \"" + datdata.Comment + "\"\n" +
							(datdata.ForcePacking == ForcePacking.Unzip ? "\tforcezipping no\n" : "") +
							")\n";
						break;
					case OutputFormat.MissFile:
						if (datdata.XSV == true)
						{
							header = "\"File Name\"\t\"Internal Name\"\t\"Description\"\t\"Game Name\"\t\"Game Description\"\t\"Type\"\t\"" +
								"Rom Name\"\t\"Disk Name\"\t\"Size\"\t\"CRC\"\t\"MD5\"\t\"SHA1\"\t\"Nodump\"\n";
						}
						else if (datdata.XSV == false)
						{
							header = "\"File Name\",\"Internal Name\",\"Description\",\"Game Name\",\"Game Description\",\"Type\",\"" +
								"Rom Name\",\"Disk Name\",\"Size\",\"CRC\",\"MD5\",\"SHA1\",\"Nodump\"\n";
						}
						break;
					case OutputFormat.RomCenter:
						header = "[CREDITS]\n" +
							"author=" + datdata.Author + "\n" +
							"version=" + datdata.Version + "\n" +
							"comment=" + datdata.Comment + "\n" +
							"[DAT]\n" +
							"version=2.50\n" +
							"split=" + (datdata.ForceMerging == ForceMerging.Split ? "1" : "0") + "\n" +
							"merge=" + (datdata.ForceMerging == ForceMerging.Full ? "1" : "0") + "\n" +
							"[EMULATOR]\n" +
							"refname=" + datdata.Name + "\n" +
							"version=" + datdata.Description + "\n" +
							"[GAMES]\n";
						break;
					case OutputFormat.SabreDat:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(datdata.Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(datdata.Description) + "</description>\n" +
							"\t\t<rootdir>" + HttpUtility.HtmlEncode(datdata.RootDir) + "</rootdir>\n" +
							"\t\t<category>" + HttpUtility.HtmlEncode(datdata.Category) + "</category>\n" +
							"\t\t<version>" + HttpUtility.HtmlEncode(datdata.Version) + "</version>\n" +
							"\t\t<date>" + HttpUtility.HtmlEncode(datdata.Date) + "</date>\n" +
							"\t\t<author>" + HttpUtility.HtmlEncode(datdata.Author) + "</author>\n" +
							"\t\t<comment>" + HttpUtility.HtmlEncode(datdata.Comment) + "</comment>\n" +
							(!String.IsNullOrEmpty(datdata.Type) && datdata.ForcePacking != ForcePacking.Unzip ?
								"\t\t<flags>\n" +
								(!String.IsNullOrEmpty(datdata.Type) ? "\t\t\t<flag name=\"type\" value=\"" + datdata.Type + "\"/>\n" : "") +
								(datdata.ForcePacking == ForcePacking.Unzip ? "\t\t\t<flag name=\"forcepacking\" value=\"unzip\"/>\n" : "") +
								"\t\t</flags>\n" : "") +
							"\t</header>\n" +
							"\t<data>\n";
						break;
					case OutputFormat.Xml:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(datdata.Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(datdata.Description) + "</description>\n" +
							"\t\t<rootdir>" + HttpUtility.HtmlEncode(datdata.RootDir) + "</rootdir>\n" +
							"\t\t<category>" + HttpUtility.HtmlEncode(datdata.Category) + "</category>\n" +
							"\t\t<version>" + HttpUtility.HtmlEncode(datdata.Version) + "</version>\n" +
							"\t\t<date>" + HttpUtility.HtmlEncode(datdata.Date) + "</date>\n" +
							"\t\t<author>" + HttpUtility.HtmlEncode(datdata.Author) + "</author>\n" +
							"\t\t<email>" + HttpUtility.HtmlEncode(datdata.Email) + "</email>\n" +
							"\t\t<homepage>" + HttpUtility.HtmlEncode(datdata.Homepage) + "</homepage>\n" +
							"\t\t<url>" + HttpUtility.HtmlEncode(datdata.Url) + "</url>\n" +
							"\t\t<comment>" + HttpUtility.HtmlEncode(datdata.Comment) + "</comment>\n" +
							(!String.IsNullOrEmpty(datdata.Type) ? "\t\t<type>" + datdata.Type + "</type>\n" : "") +
							(datdata.ForcePacking == ForcePacking.Unzip ? "\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
							"\t</header>\n";
						break;
				}

				// Write the header out
				sw.Write(header);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		public static int WriteStartGame(StreamWriter sw, Rom rom, List<string> newsplit, string lastgame, Dat datdata, int depth, int last, Logger logger)
		{
			try
			{
				// No game should start with a path separator
				if (rom.Machine.Name.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.Machine.Name = rom.Machine.Name.Substring(1);
				}

				string state = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "game (\n\tname \"" + rom.Machine.Name + "\"\n" +
							"\tdescription \"" + (String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description) + "\"\n";
						break;
					case OutputFormat.SabreDat:
						for (int i = (last == -1 ? 0 : last); i < newsplit.Count; i++)
						{
							for (int j = 0; j < depth - last + i - (lastgame == null ? 1 : 0); j++)
							{
								state += "\t";
							}
							state += "<directory name=\"" + HttpUtility.HtmlEncode(newsplit[i]) + "\" description=\"" +
							HttpUtility.HtmlEncode(newsplit[i]) + "\">\n";
						}
						depth = depth - (last == -1 ? 0 : last) + newsplit.Count;
						break;
					case OutputFormat.Xml:
						state += "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Machine.Name) + "\">\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) + "</description>\n";
						break;
				}

				sw.Write(state);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		public static int WriteEndGame(StreamWriter sw, Rom rom, List<string> splitpath, List<string> newsplit, string lastgame, Dat datdata, int depth, out int last, Logger logger)
		{
			last = 0;

			try
			{
				string state = "";

				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += ")\n";
						break;
					case OutputFormat.SabreDat:
						if (splitpath != null)
						{
							for (int i = 0; i < newsplit.Count && i < splitpath.Count; i++)
							{
								// Always keep track of the last seen item
								last = i;

								// If we find a difference, break
								if (newsplit[i] != splitpath[i])
								{
									break;
								}
							}

							// Now that we have the last known position, take down all open folders
							for (int i = depth - 1; i > last + 1; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									state += "\t";
								}
								state += "</directory>\n";
							}

							// Reset the current depth
							depth = 2 + last;
						}
						break;
					case OutputFormat.Xml:
						state += "\t</machine>\n";
						break;
				}

				sw.Write(state);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out RomData using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteRomData(StreamWriter sw, Rom rom, string lastgame, Dat datdata, int depth, Logger logger, bool ignoreblanks = false)
		{
			// If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
			if (ignoreblanks && (rom.HashData.Size == 0 || rom.HashData.Size == -1))
			{
				return true;
			}

			try
			{
				string state = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "\t" + rom.Type.ToString().ToLowerInvariant() + " ( name \"" + rom.Name + "\"" +
							(rom.HashData.Size != -1 ? " size " + rom.HashData.Size : "") +
							(!String.IsNullOrEmpty(rom.HashData.CRC) ? " crc " + rom.HashData.CRC.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.HashData.MD5) ? " md5 " + rom.HashData.MD5.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.HashData.SHA1) ? " sha1 " + rom.HashData.SHA1.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date \"" + rom.Date + "\"" : "") +
							(rom.Nodump ? " flags nodump" : "") +
							" )\n";
						break;
					case OutputFormat.MissFile:
						string pre = datdata.Prefix + (datdata.Quotes ? "\"" : "");
						string post = (datdata.Quotes ? "\"" : "") + datdata.Postfix;

						// Check for special strings in prefix and postfix
						pre = pre.Replace("%crc%", rom.HashData.CRC).Replace("%md5%", rom.HashData.MD5).Replace("%sha1%", rom.HashData.SHA1).Replace("%size%", rom.HashData.Size.ToString());
						post = post.Replace("%crc%", rom.HashData.CRC).Replace("%md5%", rom.HashData.MD5).Replace("%sha1%", rom.HashData.SHA1).Replace("%size%", rom.HashData.Size.ToString());

						// If we're in Romba mode, the state is consistent
						if (datdata.Romba)
						{
							// We can only write out if there's a SHA-1
							if (rom.HashData.SHA1 != "")
							{
								string name = rom.HashData.SHA1.Substring(0, 2) + "/" + rom.HashData.SHA1.Substring(2, 2) + "/" + rom.HashData.SHA1.Substring(4, 2) + "/" +
									rom.HashData.SHA1.Substring(6, 2) + "/" + rom.HashData.SHA1 + ".gz";
								state += pre + name + post + "\n";
							}
						}
						// If we're in TSV mode, similarly the state is consistent
						else if (datdata.XSV == true)
						{
							string inline = "\"" + datdata.FileName + "\"\t\"" + datdata.Name + "\"\t\"" + datdata.Description + "\"\t\"" + rom.Machine + "\"\t\"" + rom.Machine + "\"\t\"" +
								rom.Type.ToString().ToLowerInvariant() + "\"\t\"" + (rom.Type == ItemType.Rom ? rom.Name : "") + "\"\t\"" + (rom.Type == ItemType.Disk ? rom.Name : "") + "\"\t\"" + rom.HashData.Size + "\"\t\"" +
								rom.HashData.CRC + "\"\t\"" + rom.HashData.MD5 + "\"\t\"" + rom.HashData.SHA1 + "\"\t" + (rom.Nodump ? "\"Nodump\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						// If we're in CSV mode, similarly the state is consistent
						else if (datdata.XSV == false)
						{
							string inline = "\"" + datdata.FileName + "\",\"" + datdata.Name + "\",\"" + datdata.Description + "\",\"" + rom.Machine + "\",\"" + rom.Machine + "\",\"" +
								rom.Type.ToString().ToLowerInvariant() + "\",\"" + (rom.Type == ItemType.Rom ? rom.Name : "") + "\",\"" + (rom.Type == ItemType.Disk ? rom.Name : "") + "\",\"" + rom.HashData.Size + "\",\"" +
								rom.HashData.CRC + "\",\"" + rom.HashData.MD5 + "\",\"" + rom.HashData.SHA1 + "\"," + (rom.Nodump ? "\"Nodump\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						// Otherwise, use any flags
						else
						{
							string name = (datdata.UseGame ? rom.Machine.Name : rom.Name);
							if (datdata.RepExt != "")
							{
								string dir = Path.GetDirectoryName(name);
								dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
								name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + datdata.RepExt);
							}
							if (datdata.AddExt != "")
							{
								name += datdata.AddExt;
							}
							if (!datdata.UseGame && datdata.GameName)
							{
								name = Path.Combine(rom.Machine.Name, name);
							}

							if (datdata.UseGame && rom.Machine.Name != lastgame)
							{
								state += pre + name + post + "\n";
								lastgame = rom.Machine.Name;
							}
							else if (!datdata.UseGame)
							{
								state += pre + name + post + "\n";
							}
						}
						break;
					case OutputFormat.RomCenter:
						state += "¬¬¬" + HttpUtility.HtmlEncode(rom.Machine) +
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬" + rom.HashData.CRC.ToLowerInvariant() +
							"¬" + (rom.HashData.Size != -1 ? rom.HashData.Size.ToString() : "") + "¬¬¬\n";
						break;
					case OutputFormat.SabreDat:
						string prefix = "";
						for (int i = 0; i < depth; i++)
						{
							prefix += "\t";
						}

						state += prefix;
						state += "<file type=\"" + rom.Type.ToString().ToLowerInvariant() + "\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.HashData.Size != -1 ? " size=\"" + rom.HashData.Size + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.CRC) ? " crc=\"" + rom.HashData.CRC.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.MD5) ? " md5=\"" + rom.HashData.MD5.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.SHA1) ? " sha1=\"" + rom.HashData.SHA1.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date=\"" + rom.Date + "\"" : "") +
							(rom.Nodump ? prefix + "/>\n" + prefix + "\t<flags>\n" +
								prefix + "\t\t<flag name=\"status\" value=\"nodump\"/>\n" +
								prefix + "\t</flags>\n" +
								prefix + "</file>\n" :
							"/>\n");
						break;
					case OutputFormat.Xml:
						state += "\t\t<" + rom.Type.ToString().ToLowerInvariant() + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.HashData.Size != -1 ? " size=\"" + rom.HashData.Size + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.CRC) ? " crc=\"" + rom.HashData.CRC.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.MD5) ? " md5=\"" + rom.HashData.MD5.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.SHA1) ? " sha1=\"" + rom.HashData.SHA1.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date=\"" + rom.Date + "\"" : "") +
							(rom.Nodump ? " status=\"nodump\"" : "") +
							"/>\n";
						break;
				}

				sw.Write(state);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT footer using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteFooter(StreamWriter sw, Dat datdata, int depth, Logger logger)
		{
			try
			{
				string footer = "";

				// If we have roms, output the full footer
				if (datdata.Files != null && datdata.Files.Count > 0)
				{
					switch (datdata.OutputFormat)
					{
						case OutputFormat.ClrMamePro:
							footer = ")";
							break;
						case OutputFormat.SabreDat:
							for (int i = depth - 1; i >= 2; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									footer += "\t";
								}
								footer += "</directory>\n";
							}
							footer += "\t</data>\n</datafile>";
							break;
						case OutputFormat.Xml:
							footer = "\t</machine>\n</datafile>";
							break;
					}
				}

				// Otherwise, output the abbreviated form
				else
				{
					switch (datdata.OutputFormat)
					{
						case OutputFormat.SabreDat:
						case OutputFormat.Xml:
							footer = "</datafile>";
							break;
					}
				}

				// Write the footer out
				sw.Write(footer);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		#endregion

		#region DAT Splitting

		/// <summary>
		/// Split a DAT by input extensions
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="outdir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="extA">List of extensions to split on (first DAT)</param>
		/// <param name="extB">List of extensions to split on (second DAT)</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public static bool SplitByExt(string filename, string outdir, string basepath, List<string> extA, List<string> extB, Logger logger)
		{
			// Make sure all of the extensions have a dot at the beginning
			List<string> newExtA = new List<string>();
			foreach (string s in extA)
			{
				newExtA.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
			}
			string newExtAString = string.Join(",", newExtA);

			List<string> newExtB = new List<string>();
			foreach (string s in extB)
			{
				newExtB.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
			}
			string newExtBString = string.Join(",", newExtB);

			// Get the file format
			OutputFormat outputFormat = GetOutputFormat(filename, logger);
			if (outputFormat == OutputFormat.None)
			{
				return true;
			}

			// Get the file data to be split
			Dat datdata = new Dat();
			datdata = Parse(filename, 0, 0, datdata, logger, softlist:true);

			// Set all of the appropriate outputs for each of the subsets
			Dat datdataA = new Dat
			{
				FileName = datdata.FileName + " (" + newExtAString + ")",
				Name = datdata.Name + " (" + newExtAString + ")",
				Description = datdata.Description + " (" + newExtAString + ")",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Files = new Dictionary<string, List<Rom>>(),
				OutputFormat = outputFormat,
			};
			Dat datdataB = new Dat
			{
				FileName = datdata.FileName + " (" + newExtBString + ")",
				Name = datdata.Name + " (" + newExtBString + ")",
				Description = datdata.Description + " (" + newExtBString + ")",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Files = new Dictionary<string, List<Rom>>(),
				OutputFormat = outputFormat,
			};

			// If roms is empty, return false
			if (datdata.Files.Count == 0)
			{
				return false;
			}

			// Now separate the roms accordingly
			foreach (string key in datdata.Files.Keys)
			{
				foreach (Rom rom in datdata.Files[key])
				{
					if (newExtA.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataA.Files.ContainsKey(key))
						{
							datdataA.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							datdataA.Files.Add(key, temp);
						}
					}
					else if (newExtB.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataB.Files.ContainsKey(key))
						{
							datdataB.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							datdataB.Files.Add(key, temp);
						}
					}
					else
					{
						if (datdataA.Files.ContainsKey(key))
						{
							datdataA.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							datdataA.Files.Add(key, temp);
						}
						if (datdataB.Files.ContainsKey(key))
						{
							datdataB.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							datdataB.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outdir != "")
			{
				outdir = outdir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outdir = Path.GetDirectoryName(filename);
			}

			// Then write out both files
			bool success = WriteDatfile(datdataA, outdir, logger);
			success &= WriteDatfile(datdataB, outdir, logger);

			return success;
		}

		/// <summary>
		/// Split a DAT by best available hashes
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="outdir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public static bool SplitByHash(string filename, string outdir, string basepath, Logger logger)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Get the file format
			OutputFormat outputFormat = GetOutputFormat(filename, logger);
			if (outputFormat == OutputFormat.None)
			{
				return true;
			}

			// Get the file data to be split
			Dat datdata = new Dat();
			datdata = Parse(filename, 0, 0, datdata, logger, true, softlist:true);

			// Create each of the respective output DATs
			logger.User("Creating and populating new DATs");
			Dat nodump = new Dat
			{
				FileName = datdata.FileName + " (Nodump)",
				Name = datdata.Name + " (Nodump)",
				Description = datdata.Description + " (Nodump)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<Rom>>(),
			};
			Dat sha1 = new Dat
			{
				FileName = datdata.FileName + " (SHA-1)",
				Name = datdata.Name + " (SHA-1)",
				Description = datdata.Description + " (SHA-1)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<Rom>>(),
			};
			Dat md5 = new Dat
			{
				FileName = datdata.FileName + " (MD5)",
				Name = datdata.Name + " (MD5)",
				Description = datdata.Description + " (MD5)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<Rom>>(),
			};
			Dat crc = new Dat
			{
				FileName = datdata.FileName + " (CRC)",
				Name = datdata.Name + " (CRC)",
				Description = datdata.Description + " (CRC)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<Rom>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = datdata.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = datdata.Files[key];
				foreach (Rom rom in roms)
				{
					// If the file is a nodump
					if (rom.Nodump)
					{
						if (nodump.Files.ContainsKey(key))
						{
							nodump.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							nodump.Files.Add(key, temp);
						}
					}
					// If the file has a SHA-1
					else if (rom.HashData.SHA1 != null && rom.HashData.SHA1 != "")
					{
						if (sha1.Files.ContainsKey(key))
						{
							sha1.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							sha1.Files.Add(key, temp);
						}
					}
					// If the file has no SHA-1 but has an MD5
					else if (rom.HashData.MD5 != null && rom.HashData.MD5 != "")
					{
						if (md5.Files.ContainsKey(key))
						{
							md5.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							md5.Files.Add(key, temp);
						}
					}
					// All other cases
					else
					{
						if (crc.Files.ContainsKey(key))
						{
							crc.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							crc.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outdir != "")
			{
				outdir = outdir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outdir = Path.GetDirectoryName(filename);
			}

			// Now, output all of the files to the output directory
			logger.User("DAT information created, outputting new files");
			bool success = true;
			if (nodump.Files.Count > 0)
			{
				success &= WriteDatfile(nodump, outdir, logger);
			}
			if (sha1.Files.Count > 0)
			{
				success &= WriteDatfile(sha1, outdir, logger);
			}
			if (md5.Files.Count > 0)
			{
				success &= WriteDatfile(md5, outdir, logger);
			}
			if (crc.Files.Count > 0)
			{
				success &= WriteDatfile(crc, outdir, logger);
			}

			return success;
		}

		/// <summary>
		/// Split a DAT by type of Rom
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="outdir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="logger">Logger object for console and file writing</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public static bool SplitByType(string filename, string outdir, string basepath, Logger logger)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Get the file format
			OutputFormat outputFormat = GetOutputFormat(filename, logger);
			if (outputFormat == OutputFormat.None)
			{
				return true;
			}

			// Get the file data to be split
			Dat datdata = new Dat();
			datdata = Parse(filename, 0, 0, datdata, logger, true, softlist:true);

			// Create each of the respective output DATs
			logger.User("Creating and populating new DATs");
			Dat romdat = new Dat
			{
				FileName = datdata.FileName + " (ROM)",
				Name = datdata.Name + " (ROM)",
				Description = datdata.Description + " (ROM)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<Rom>>(),
			};
			Dat diskdat = new Dat
			{
				FileName = datdata.FileName + " (Disk)",
				Name = datdata.Name + " (Disk)",
				Description = datdata.Description + " (Disk)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Files = new Dictionary<string, List<Rom>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = datdata.Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = datdata.Files[key];
				foreach (Rom rom in roms)
				{
					// If the file is a Rom
					if (rom.Type == ItemType.Rom)
					{
						if (romdat.Files.ContainsKey(key))
						{
							romdat.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							romdat.Files.Add(key, temp);
						}
					}
					// If the file is a Disk
					else if (rom.Type == ItemType.Disk)
					{
						if (diskdat.Files.ContainsKey(key))
						{
							diskdat.Files[key].Add(rom);
						}
						else
						{
							List<Rom> temp = new List<Rom>();
							temp.Add(rom);
							diskdat.Files.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			if (outdir != "")
			{
				outdir = outdir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outdir = Path.GetDirectoryName(filename);
			}

			// Now, output all of the files to the output directory
			logger.User("DAT information created, outputting new files");
			bool success = true;
			if (romdat.Files.Count > 0)
			{
				success &= WriteDatfile(romdat, outdir, logger);
			}
			if (diskdat.Files.Count > 0)
			{
				success &= WriteDatfile(diskdat, outdir, logger);
			}

			return success;
		}

		#endregion
	}
}
