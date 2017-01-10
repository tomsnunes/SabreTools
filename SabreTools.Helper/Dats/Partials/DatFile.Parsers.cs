using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using StreamReader = System.IO.StreamReader;
#endif

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Parsing [MODULAR DONE, FOR NOW]

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
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		public void Parse(string filename, int sysid, int srcid, Logger logger, bool keep = false, bool clean = false, bool softlist = false, bool keepext = false)
		{
			Parse(filename, sysid, srcid, new Filter(), SplitType.None, false, false, "", logger, keep, clean, softlist, keepext);
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		public void Parse(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

			// Rom renaming
			SplitType splitType,
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
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}
			if (ext != "dat" && ext != "md5" && ext != "sfv" && ext != "sha1" && ext != "txt" && ext != "xml")
			{
				return;
			}

			// If the output filename isn't set already, get the internal filename
			FileName = (String.IsNullOrEmpty(FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : FileName);

			// If the output type isn't set already, get the internal output type
			DatFormat = (DatFormat == 0 ? FileTools.GetDatFormat(filename, logger) : DatFormat);

			// Now parse the correct type of DAT
			switch (FileTools.GetDatFormat(filename, logger))
			{
				case DatFormat.AttractMode:
					ParseAttractMode(filename, sysid, srcid, filter, trim, single, root, logger, keep, clean);
					break;
				case DatFormat.ClrMamePro:
				case DatFormat.DOSCenter:
					ParseCMP(filename, sysid, srcid, filter, trim, single, root, logger, keep, clean);
					break;
				case DatFormat.Logiqx:
				case DatFormat.OfflineList:
				case DatFormat.SabreDat:
				case DatFormat.SoftwareList:
					ParseGenericXML(filename, sysid, srcid, filter, trim, single, root, logger, keep, clean, softlist);
					break;
				case DatFormat.RedumpMD5:
					ParseRedumpMD5(filename, sysid, srcid, filter, trim, single, root, logger, clean);
					break;
				case DatFormat.RedumpSFV:
					ParseRedumpSFV(filename, sysid, srcid, filter, trim, single, root, logger, clean);
					break;
				case DatFormat.RedumpSHA1:
					ParseRedumpSHA1(filename, sysid, srcid, filter, trim, single, root, logger, clean);
					break;
				case DatFormat.RomCenter:
					ParseRC(filename, sysid, srcid, filter, trim, single, root, logger, clean);
					break;
				default:
					return;
			}

			// Now we pre-process the DAT with the splitting/merging mode
			if (splitType == SplitType.NonMerged)
			{
				CreateNonMergedSets(false, logger, output: false);
			}
			else if (splitType == SplitType.Merged)
			{
				CreateMergedSets(false, logger, output: false);
			}
			else if (splitType == SplitType.NotMergedWithDevice)
			{
				CreateFullyNonMergedSets(false, logger, output: false);
			}
		}

		/// <summary>
		/// Parse an AttractMode DAT and return all found games within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseAttractMode(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

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
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			sr.ReadLine(); // Skip the first line since it's the header
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				/*
				The gameinfo order is as follows
				0 - game name
				1 - game description
				2 - emulator name (filename)
				3 - cloneof
				4 - year
				5 - manufacturer
				6 - category
				7 - players
				8 - rotation
				9 - control
				10 - status
				11 - displaycount
				12 - displaytype
				13 - alt romname
				14 - alt title
				15 - extra
				16 - buttons
				*/

				string[] gameinfo = line.Split(';');

				Rom rom = new Rom
				{
					Name = "-",
					Size = Constants.SizeZero,
					CRC = Constants.CRCZero,
					MD5 = Constants.MD5Zero,
					SHA1 = Constants.SHA1Zero,
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = gameinfo[0],
						Description = gameinfo[1],
						CloneOf = gameinfo[3],
						Year = gameinfo[4],
						Manufacturer = gameinfo[5],
						Comment = gameinfo[15],
					}
				};

				// Now process and add the rom
				string key = "";
				ParseAddHelper(rom, filter, trim, single, root, clean, logger, out key);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a ClrMamePro DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseCMP(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

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
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			bool block = false, superdat = false;
			string blockname = "", tempgamename = "", gamedesc = "", cloneof = "",
				romof = "", sampleof = "", year = "", manufacturer = "";
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

					if (gc[1].Value == "clrmamepro" || gc[1].Value == "romvault" || gc[1].Value.ToLowerInvariant() == "doscenter")
					{
						blockname = "header";
					}

					block = true;
				}

				// If the line is a rom-like item and we're in a block
				else if ((line.Trim().StartsWith("rom (")
						|| line.Trim().StartsWith("disk (")
						|| line.Trim().StartsWith("file (")
						|| (line.Trim().StartsWith("sample") && !line.Trim().StartsWith("sampleof"))
					) && block)
				{
					ItemType temptype = ItemType.Rom;
					if (line.Trim().StartsWith("rom ("))
					{
						temptype = ItemType.Rom;
					}
					else if (line.Trim().StartsWith("disk ("))
					{
						temptype = ItemType.Disk;
					}
					else if (line.Trim().StartsWith("file ("))
					{
						temptype = ItemType.Rom;
					}
					else if (line.Trim().StartsWith("sample"))
					{
						temptype = ItemType.Sample;
					}

					// Create the proper DatItem based on the type
					DatItem item;
					switch (temptype)
					{
						case ItemType.Archive:
							item = new Archive();
							break;
						case ItemType.BiosSet:
							item = new BiosSet();
							break;
						case ItemType.Disk:
							item = new Disk();
							break;
						case ItemType.Release:
							item = new Release();
							break;
						case ItemType.Sample:
							item = new Sample();
							break;
						case ItemType.Rom:
						default:
							item = new Rom();
							break;
					}

					// Then populate it with information
					item.Machine = new Machine
					{
						Name = tempgamename,
						Description = gamedesc,
						CloneOf = cloneof,
						RomOf = romof,
						SampleOf = sampleof,
						Manufacturer = manufacturer,
						Year = year,
					};

					item.SystemID = sysid;
					item.SourceID = srcid;

					// Get the blank key to write to
					string key = "";

					// If we have a sample, treat it special
					if (temptype == ItemType.Sample)
					{
						line = line.Trim().Remove(0, 6).Trim().Replace("\"", ""); // Remove "sample" from the input string
						item.Name = line;

						// Now process and add the sample
						key = "";
						ParseAddHelper(item, filter, trim, single, root, clean, logger, out key);

						continue;
					}

					// Get the line split by spaces and quotes
					string[] gc = Style.SplitLineAsCMP(line);

					// Special cases for DOSCenter DATs only because of how the lines are arranged
					if (line.Trim().StartsWith("file ("))
					{
						// Loop over the specifics
						for (int i = 0; i < gc.Length; i++)
						{
							// Names are not quoted, for some stupid reason
							if (gc[i] == "name")
							{
								// Advance to the first part of the name
								i++;
								item.Name = gc[i];

								// Advance to the next item, adding until we find "size"
								i++;
								while (i < gc.Length && gc[i] != "size" && gc[i] != "date" && gc[i] != "crc")
								{
									item.Name += " " + gc[i];
									i++;
								}
							}

							// Get the size from the next part
							else if (gc[i] == "size")
							{
								i++;
								long tempsize = -1;
								if (!Int64.TryParse(gc[i], out tempsize))
								{
									tempsize = 0;
								}
								((Rom)item).Size = tempsize;
								i++;
							}

							// Get the date from the next part
							else if (gc[i] == "date")
							{
								i++;
								((Rom)item).Date = gc[i].Replace("\"", "") + " " + gc[i + 1].Replace("\"", "");
								i += 3;
							}

							// Get the CRC from the next part
							else if (gc[i] == "crc")
							{
								i++;
								((Rom)item).CRC = gc[i].Replace("\"", "").ToLowerInvariant();
							}
						}

						// Now process and add the rom
						key = "";
						ParseAddHelper(item, filter, trim, single, root, clean, logger, out key);
						continue;
					}

					// Loop over all attributes normally and add them if possible
					for (int i = 0; i < gc.Length; i++)
					{
						// Look at the current item and use it if possible
						string quoteless = gc[i].Replace("\"", "");
						switch (quoteless)
						{
							//If the item is empty, we automatically skip it because it's a fluke
							case "":
								continue;

							// Special cases for standalone item statuses
							case "baddump":
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.BadDump;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.BadDump;
								}
								break;
							case "good":
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.Good;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.Good;
								}
								break;
							case "nodump":
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.Nodump;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.Nodump;
								}
								break;
							case "verified":
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = ItemStatus.Verified;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = ItemStatus.Verified;
								}
								break;

							// Regular attributes
							case "name":
								i++;
								quoteless = gc[i].Replace("\"", "");
								item.Name = quoteless;
								break;
							case "size":
								if (item.Type == ItemType.Rom)
								{
									i++;
									quoteless = gc[i].Replace("\"", "");
									long size = -1;
									if (Int64.TryParse(quoteless, out size))
									{
										((Rom)item).Size = size;
									}
								}

								break;
							case "crc":
								if (item.Type == ItemType.Rom)
								{
									i++;
									quoteless = gc[i].Replace("\"", "");
									((Rom)item).CRC = quoteless.ToLowerInvariant();
								}
								break;
							case "md5":
								if (item.Type == ItemType.Rom)
								{
									i++;
									quoteless = gc[i].Replace("\"", "");
									((Rom)item).MD5 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									i++;
									quoteless = gc[i].Replace("\"", "");
									((Disk)item).MD5 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha1":
								if (item.Type == ItemType.Rom)
								{
									i++;
									quoteless = gc[i].Replace("\"", "");
									((Rom)item).SHA1 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									i++;
									quoteless = gc[i].Replace("\"", "");
									((Disk)item).SHA1 = quoteless.ToLowerInvariant();
								}
								break;
							case "status":
							case "flags":
								i++;
								quoteless = gc[i].Replace("\"", "");
								if (quoteless.ToLowerInvariant() == "good")
								{
									if (item.Type == ItemType.Rom)
									{
										((Rom)item).ItemStatus = ItemStatus.Good;
									}
									else if (item.Type == ItemType.Disk)
									{
										((Disk)item).ItemStatus = ItemStatus.Good;
									}
								}
								else if (quoteless.ToLowerInvariant() == "baddump")
								{
									if (item.Type == ItemType.Rom)
									{
										((Rom)item).ItemStatus = ItemStatus.BadDump;
									}
									else if (item.Type == ItemType.Disk)
									{
										((Disk)item).ItemStatus = ItemStatus.BadDump;
									}
								}
								else if (quoteless.ToLowerInvariant() == "nodump")
								{
									if (item.Type == ItemType.Rom)
									{
										((Rom)item).ItemStatus = ItemStatus.Nodump;
									}
									else if (item.Type == ItemType.Disk)
									{
										((Disk)item).ItemStatus = ItemStatus.Nodump;
									}
								}
								else if (quoteless.ToLowerInvariant() == "verified")
								{
									if (item.Type == ItemType.Rom)
									{
										((Rom)item).ItemStatus = ItemStatus.Verified;
									}
									else if (item.Type == ItemType.Disk)
									{
										((Disk)item).ItemStatus = ItemStatus.Verified;
									}
								}
								break;
							case "date":
								if (item.Type == ItemType.Rom)
								{
									i++;
									quoteless = gc[i].Replace("\"", "") + " " + gc[i + 1].Replace("\"", "");
									((Rom)item).Date = quoteless;
								}
								i++;
								break;
						}
					}

					// Now process and add the rom
					key = "";
					ParseAddHelper(item, filter, trim, single, root, clean, logger, out key);
				}

				// If the line is anything but a rom or disk and we're in a block
				else if (Regex.IsMatch(line, Constants.ItemPatternCMP) && block)
				{
					GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;

					if (blockname != "header")
					{
						string itemval = gc[2].Value.Replace("\"", "");
						switch (gc[1].Value)
						{
							case "name":
								tempgamename = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
								break;
							case "description":
								gamedesc = itemval;
								break;
							case "romof":
								romof = itemval;
								break;
							case "cloneof":
								cloneof = itemval;
								break;
							case "year":
								year = itemval;
								break;
							case "manufacturer":
								manufacturer = itemval;
								break;
							case "sampleof":
								sampleof = itemval;
								break;
						}
					}
					else
					{
						string itemval = gc[2].Value.Replace("\"", "");

						if (line.Trim().StartsWith("Name:"))
						{
							Name = (String.IsNullOrEmpty(Name) ? line.Substring(6) : Name);
							superdat = superdat || itemval.Contains(" - SuperDAT");
							if (keep && superdat)
							{
								Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
							}
							continue;
						}

						switch (gc[1].Value)
						{
							case "name":
							case "Name:":
								Name = (String.IsNullOrEmpty(Name) ? itemval : Name);
								superdat = superdat || itemval.Contains(" - SuperDAT");
								if (keep && superdat)
								{
									Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
								}
								break;
							case "description":
							case "Description:":
								Description = (String.IsNullOrEmpty(Description) ? itemval : Description);
								break;
							case "rootdir":
								RootDir = (String.IsNullOrEmpty(RootDir) ? itemval : RootDir);
								break;
							case "category":
								Category = (String.IsNullOrEmpty(Category) ? itemval : Category);
								break;
							case "version":
							case "Version:":
								Version = (String.IsNullOrEmpty(Version) ? itemval : Version);
								break;
							case "date":
							case "Date:":
								Date = (String.IsNullOrEmpty(Date) ? itemval : Date);
								break;
							case "author":
							case "Author:":
								Author = (String.IsNullOrEmpty(Author) ? itemval : Author);
								break;
							case "email":
								Email = (String.IsNullOrEmpty(Email) ? itemval : Email);
								break;
							case "homepage":
							case "Homepage:":
								Homepage = (String.IsNullOrEmpty(Homepage) ? itemval : Homepage);
								break;
							case "url":
								Url = (String.IsNullOrEmpty(Url) ? itemval : Url);
								break;
							case "comment":
							case "Comment:":
								Comment = (String.IsNullOrEmpty(Comment) ? itemval : Comment);
								break;
							case "header":
								Header = (String.IsNullOrEmpty(Header) ? itemval : Header);
								break;
							case "type":
								Type = (String.IsNullOrEmpty(Type) ? itemval : Type);
								superdat = superdat || itemval.Contains("SuperDAT");
								break;
							case "forcemerging":
								if (ForceMerging == ForceMerging.None)
								{
									switch (itemval)
									{
										case "none":
											ForceMerging = ForceMerging.None;
											break;
										case "split":
											ForceMerging = ForceMerging.Split;
											break;
										case "full":
											ForceMerging = ForceMerging.Full;
											break;
									}
								}
								break;
							case "forcezipping":
								if (ForcePacking == ForcePacking.None)
								{
									switch (itemval)
									{
										case "yes":
											ForcePacking = ForcePacking.Zip;
											break;
										case "no":
											ForcePacking = ForcePacking.Unzip;
											break;
									}
								}
								break;
							case "forcepacking":
								if (ForcePacking == ForcePacking.None)
								{
									switch (itemval)
									{
										case "zip":
											ForcePacking = ForcePacking.Zip;
											break;
										case "unzip":
											ForcePacking = ForcePacking.Unzip;
											break;
									}
								}
								break;
						}
					}
				}

				// If we find an end bracket that's not associated with anything else, the block is done
				else if (Regex.IsMatch(line, Constants.EndPatternCMP) && block)
				{
					block = false;
					blockname = ""; tempgamename = ""; gamedesc = ""; cloneof = "";
					romof = ""; sampleof = ""; year = ""; manufacturer = "";
				}
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse an XML DAT (Logiqx, OfflineList, SabreDAT, and Software List) and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="softlist">True if SL XML names should be kept, false otherwise (default)</param>
		private void ParseGenericXML(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

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
			bool superdat = false, empty = true;
			string key = "", date = "";
			long size = -1;
			ItemStatus its = ItemStatus.None;
			List<string> parent = new List<string>();

			Encoding enc = Style.GetEncoding(filename);
			XmlReader xtr = FileTools.GetXmlTextReader(filename, logger);

			// If we got a null reader, just return
			if (xtr == null)
			{
				return;
			}

			// Otherwise, read the file to the end
			try
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
							Rom rom = new Rom("null", tempgame);

							// Now process and add the rom
							ParseAddHelper(rom, filter, trim, single, root, clean, logger, out key);
						}

						// Regardless, end the current folder
						int parentcount = parent.Count;
						if (parentcount == 0)
						{
							logger.Verbose("Empty parent: " + String.Join("\\", parent));
							empty = true;
						}

						// If we have an end folder element, remove one item from the parent, if possible
						if (parentcount > 0)
						{
							parent.RemoveAt(parent.Count - 1);
							if (keep && parentcount > 1)
							{
								Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
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
						// Handle MAME listxml since they're halfway between a SL and a Logiqx XML
						case "mame":
							if (xtr.GetAttribute("build") != null)
							{
								Name = (String.IsNullOrEmpty(Name) ? xtr.GetAttribute("build") : Name);
								Description = (String.IsNullOrEmpty(Description) ? Name : Name);
							}
							xtr.Read();
							break;
						// New software lists have this behavior
						case "softwarelist":
							if (xtr.GetAttribute("name") != null)
							{
								Name = (String.IsNullOrEmpty(Name) ? xtr.GetAttribute("name") : Name);
							}
							if (xtr.GetAttribute("description") != null)
							{
								Description = (String.IsNullOrEmpty(Description) ? xtr.GetAttribute("description") : Description);
							}
							if (xtr.GetAttribute("forcemerging") != null && ForceMerging == ForceMerging.None)
							{
								switch (xtr.GetAttribute("forcemerging"))
								{
									case "split":
										ForceMerging = ForceMerging.Split;
										break;
									case "none":
										ForceMerging = ForceMerging.None;
										break;
									case "full":
										ForceMerging = ForceMerging.Full;
										break;
								}
							}
							if (xtr.GetAttribute("forcenodump") != null && ForceNodump == ForceNodump.None)
							{
								switch (xtr.GetAttribute("forcenodump"))
								{
									case "obsolete":
										ForceNodump = ForceNodump.Obsolete;
										break;
									case "required":
										ForceNodump = ForceNodump.Required;
										break;
									case "ignore":
										ForceNodump = ForceNodump.Ignore;
										break;
								}
							}
							if (xtr.GetAttribute("forcepacking") != null && ForcePacking == ForcePacking.None)
							{
								switch (xtr.GetAttribute("forcepacking"))
								{
									case "zip":
										ForcePacking = ForcePacking.Zip;
										break;
									case "unzip":
										ForcePacking = ForcePacking.Unzip;
										break;
								}
							}
							xtr.Read();
							break;
						// Handle M1 DATs since they're 99% the same as a SL DAT
						case "m1":
							Name = (String.IsNullOrEmpty(Name) ? "M1" : Name);
							Description = (String.IsNullOrEmpty(Description) ? "M1" : Description);
							if (xtr.GetAttribute("version") != null)
							{
								Version = (String.IsNullOrEmpty(Version) ? xtr.GetAttribute("version") : Version);
							}
							xtr.Read();
							break;
						// OfflineList has a different header format
						case "configuration":
							headreader = xtr.ReadSubtree();

							// If there's no subtree to the header, skip it
							if (headreader == null)
							{
								xtr.Skip();
								continue;
							}

							// Otherwise, read what we can from the header
							while (!headreader.EOF)
							{
								// We only want elements
								if (headreader.NodeType != XmlNodeType.Element || headreader.Name == "configuration")
								{
									headreader.Read();
									continue;
								}

								// Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
								string content = "";
								switch (headreader.Name.ToLowerInvariant())
								{
									case "datname":
										content = headreader.ReadElementContentAsString(); ;
										Name = (String.IsNullOrEmpty(Name) ? content : Name);
										superdat = superdat || content.Contains(" - SuperDAT");
										if (keep && superdat)
										{
											Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
										}
										break;
									case "datversionurl":
										content = headreader.ReadElementContentAsString(); ;
										Url = (String.IsNullOrEmpty(Name) ? content : Url);
										break;
									default:
										headreader.Read();
										break;
								}
							}

							break;
						// We want to process the entire subtree of the header
						case "header":
							headreader = xtr.ReadSubtree();

							// If there's no subtree to the header, skip it
							if (headreader == null)
							{
								xtr.Skip();
								continue;
							}

							// Otherwise, read what we can from the header
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
										Name = (String.IsNullOrEmpty(Name) ? content : Name);
										superdat = superdat || content.Contains(" - SuperDAT");
										if (keep && superdat)
										{
											Type = (String.IsNullOrEmpty(Type) ? "SuperDAT" : Type);
										}
										break;
									case "description":
										content = headreader.ReadElementContentAsString();
										Description = (String.IsNullOrEmpty(Description) ? content : Description);
										break;
									case "rootdir":
										content = headreader.ReadElementContentAsString();
										RootDir = (String.IsNullOrEmpty(RootDir) ? content : RootDir);
										break;
									case "category":
										content = headreader.ReadElementContentAsString();
										Category = (String.IsNullOrEmpty(Category) ? content : Category);
										break;
									case "version":
										content = headreader.ReadElementContentAsString();
										Version = (String.IsNullOrEmpty(Version) ? content : Version);
										break;
									case "date":
										content = headreader.ReadElementContentAsString();
										Date = (String.IsNullOrEmpty(Date) ? content.Replace(".", "/") : Date);
										break;
									case "author":
										content = headreader.ReadElementContentAsString();
										Author = (String.IsNullOrEmpty(Author) ? content : Author);

										// Special cases for SabreDAT
										Email = (String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(headreader.GetAttribute("email")) ?
											headreader.GetAttribute("email") : Email);
										Homepage = (String.IsNullOrEmpty(Homepage) && !String.IsNullOrEmpty(headreader.GetAttribute("homepage")) ?
											headreader.GetAttribute("homepage") : Email);
										Url = (String.IsNullOrEmpty(Url) && !String.IsNullOrEmpty(headreader.GetAttribute("url")) ?
											headreader.GetAttribute("url") : Email);
										break;
									case "email":
										content = headreader.ReadElementContentAsString();
										Email = (String.IsNullOrEmpty(Email) ? content : Email);
										break;
									case "homepage":
										content = headreader.ReadElementContentAsString();
										Homepage = (String.IsNullOrEmpty(Homepage) ? content : Homepage);
										break;
									case "url":
										content = headreader.ReadElementContentAsString();
										Url = (String.IsNullOrEmpty(Url) ? content : Url);
										break;
									case "comment":
										content = headreader.ReadElementContentAsString();
										Comment = (String.IsNullOrEmpty(Comment) ? content : Comment);
										break;
									case "type":
										content = headreader.ReadElementContentAsString();
										Type = (String.IsNullOrEmpty(Type) ? content : Type);
										superdat = superdat || content.Contains("SuperDAT");
										break;
									case "clrmamepro":
									case "romcenter":
										if (headreader.GetAttribute("header") != null)
										{
											Header = (String.IsNullOrEmpty(Header) ? headreader.GetAttribute("header") : Header);
										}
										if (headreader.GetAttribute("plugin") != null)
										{
											Header = (String.IsNullOrEmpty(Header) ? headreader.GetAttribute("plugin") : Header);
										}
										if (headreader.GetAttribute("forcemerging") != null && ForceMerging == ForceMerging.None)
										{
											switch (headreader.GetAttribute("forcemerging"))
											{
												case "split":
													ForceMerging = ForceMerging.Split;
													break;
												case "none":
													ForceMerging = ForceMerging.None;
													break;
												case "full":
													ForceMerging = ForceMerging.Full;
													break;
											}
										}
										if (headreader.GetAttribute("forcenodump") != null && ForceNodump == ForceNodump.None)
										{
											switch (headreader.GetAttribute("forcenodump"))
											{
												case "obsolete":
													ForceNodump = ForceNodump.Obsolete;
													break;
												case "required":
													ForceNodump = ForceNodump.Required;
													break;
												case "ignore":
													ForceNodump = ForceNodump.Ignore;
													break;
											}
										}
										if (headreader.GetAttribute("forcepacking") != null && ForcePacking == ForcePacking.None)
										{
											switch (headreader.GetAttribute("forcepacking"))
											{
												case "zip":
													ForcePacking = ForcePacking.Zip;
													break;
												case "unzip":
													ForcePacking = ForcePacking.Unzip;
													break;
											}
										}
										headreader.Read();
										break;
									case "flags":
										flagreader = xtr.ReadSubtree();

										// If we somehow have a null flag section, skip it
										if (flagreader == null)
										{
											xtr.Skip();
											continue;
										}

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
																Type = (String.IsNullOrEmpty(Type) ? content : Type);
																superdat = superdat || content.Contains("SuperDAT");
																break;
															case "forcemerging":
																if (ForceMerging == ForceMerging.None)
																{
																	switch (content)
																	{
																		case "split":
																			ForceMerging = ForceMerging.Split;
																			break;
																		case "none":
																			ForceMerging = ForceMerging.None;
																			break;
																		case "full":
																			ForceMerging = ForceMerging.Full;
																			break;
																	}
																}
																break;
															case "forcenodump":
																if (ForceNodump == ForceNodump.None)
																{
																	switch (content)
																	{
																		case "obsolete":
																			ForceNodump = ForceNodump.Obsolete;
																			break;
																		case "required":
																			ForceNodump = ForceNodump.Required;
																			break;
																		case "ignore":
																			ForceNodump = ForceNodump.Ignore;
																			break;
																	}
																}
																break;
															case "forcepacking":
																if (ForcePacking == ForcePacking.None)
																{
																	switch (content)
																	{
																		case "zip":
																			ForcePacking = ForcePacking.Zip;
																			break;
																		case "unzip":
																			ForcePacking = ForcePacking.Unzip;
																			break;
																	}
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
										headreader.Skip();
										break;
									default:
										headreader.Read();
										break;
								}
							}

							// Skip the header node now that we've processed it
							xtr.Skip();
							break;
						case "machine":
						case "game":
						case "software":
							string temptype = xtr.Name, publisher = "", partname = "", partinterface = "", areaname = "";
							bool? supported = null;
							long? areasize = null;
							List<Tuple<string, string>> infos = new List<Tuple<string, string>>();
							List<Tuple<string, string>> features = new List<Tuple<string, string>>();

							// We want to process the entire subtree of the game
							subreader = xtr.ReadSubtree();

							// Safeguard for interesting case of "software" without anything except roms
							bool software = false;

							// If we have an empty machine, skip it
							if (subreader == null)
							{
								xtr.Skip();
								continue;
							}

							// Otherwise, add what is possible
							subreader.MoveToContent();

							// Create a new machine
							Machine machine = new Machine
							{
								Name = xtr.GetAttribute("name"),
								Description = xtr.GetAttribute("name"),

								RomOf = xtr.GetAttribute("romof") ?? "",
								CloneOf = xtr.GetAttribute("cloneof") ?? "",
								SampleOf = xtr.GetAttribute("sampleof") ?? "",

								Devices = new List<string>(),
								MachineType =
									xtr.GetAttribute("isbios") == "yes" ? MachineType.Bios :
									xtr.GetAttribute("isdevice") == "yes" ? MachineType.Device :
									xtr.GetAttribute("ismechanical") == "yes" ? MachineType.Mechanical :
									MachineType.None,
								Runnable = xtr.GetAttribute("runnable") == "yes" || xtr.GetAttribute("runnable") == null,
							};

							// Get the supported value from the reader
							if (subreader.GetAttribute("supported") != null)
							{
								switch (subreader.GetAttribute("supported"))
								{
									case "no":
										supported = false;
										break;
									case "yes":
										supported = true;
										break;
								}
							}

							if (superdat && !keep)
							{
								string tempout = Regex.Match(machine.Name, @".*?\\(.*)").Groups[1].Value;
								if (tempout != "")
								{
									machine.Name = tempout;
								}
							}
							// Get the name of the game from the parent
							else if (superdat && keep && parent.Count > 0)
							{
								machine.Name = String.Join("\\", parent) + "\\" + machine.Name;
							}

							// Special offline list parts
							string ext = "";
							string releaseNumber = "";

							while (software || !subreader.EOF)
							{
								software = false;

								// We only want elements
								if (subreader.NodeType != XmlNodeType.Element)
								{
									if (subreader.NodeType == XmlNodeType.EndElement && subreader.Name == "part")
									{
										partname = "";
										partinterface = "";
										features = new List<Tuple<string, string>>();
									}
									if (subreader.NodeType == XmlNodeType.EndElement && (subreader.Name == "dataarea" || subreader.Name == "diskarea"))
									{
										areaname = "";
										areasize = null;
									}

									subreader.Read();
									continue;
								}

								// Get the roms from the machine
								switch (subreader.Name)
								{
									// For OfflineList only
									case "title":
										machine.Name = subreader.ReadElementContentAsString();
										break;
									case "releaseNumber":
										releaseNumber = subreader.ReadElementContentAsString();
										break;
									case "romSize":
										if (!Int64.TryParse(subreader.ReadElementContentAsString(), out size))
										{
											size = -1;
										}
										break;
									case "romCRC":
										empty = false;

										ext = (subreader.GetAttribute("extension") != null ? subreader.GetAttribute("extension") : "");

										DatItem olrom = new Rom
										{
											Name = releaseNumber + " - " + machine.Name + ext,
											Size = size,
											CRC = subreader.ReadElementContentAsString(),
											ItemStatus = ItemStatus.None,

											Machine = machine,
										};

										// Now process and add the rom
										ParseAddHelper(olrom, filter, trim, single, root, clean, logger, out key);
										break;

									// For Software List and MAME listxml only
									case "device_ref":
										string device = subreader.GetAttribute("name");
										if (!machine.Devices.Contains(device))
										{
											machine.Devices.Add(device);
										}
										subreader.Read();
										
										break;
									case "publisher":
										publisher = subreader.ReadElementContentAsString();
										break;
									case "info":
										infos.Add(Tuple.Create(subreader.GetAttribute("name"), subreader.GetAttribute("value")));
										subreader.Read();
										break;
									case "part":
										partname = subreader.GetAttribute("name");
										partinterface = subreader.GetAttribute("interface");
										subreader.Read();
										break;
									case "feature":
										features.Add(Tuple.Create(subreader.GetAttribute("name"), subreader.GetAttribute("value")));
										subreader.Read();
										break;
									case "dataarea":
									case "diskarea":
										areaname = subreader.GetAttribute("name");
										long areasizetemp = -1;
										if (Int64.TryParse(subreader.GetAttribute("size"), out areasizetemp))
										{
											areasize = areasizetemp;
										}
										subreader.Read();
										break;

									// For Logiqx, SabreDAT, and Software List
									case "description":
										machine.Description = subreader.ReadElementContentAsString();
										if (!softlist && temptype == "software")
										{
											machine.Name = machine.Description.Replace('/', '_').Replace("\"", "''");
										}
										break;
									case "year":
										machine.Year = subreader.ReadElementContentAsString();
										break;
									case "manufacturer":
										machine.Manufacturer = subreader.ReadElementContentAsString();
										break;
									case "release":
										empty = false;

										bool? defaultrel = null;
										if (subreader.GetAttribute("default") != null)
										{
											if (subreader.GetAttribute("default") == "yes")
											{
												defaultrel = true;
											}
											else if (subreader.GetAttribute("default") == "no")
											{
												defaultrel = false;
											}
										}

										DatItem relrom = new Release
										{
											Name = subreader.GetAttribute("name"),
											Region = subreader.GetAttribute("region"),
											Language = subreader.GetAttribute("language"),
											Date = date,
											Default = defaultrel,

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,
										};

										// Now process and add the rom
										ParseAddHelper(relrom, filter, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "biosset":
										empty = false;

										bool? defaultbios = null;
										if (subreader.GetAttribute("default") != null)
										{
											if (subreader.GetAttribute("default") == "yes")
											{
												defaultbios = true;
											}
											else if (subreader.GetAttribute("default") == "no")
											{
												defaultbios = false;
											}
										}

										DatItem biosrom = new BiosSet
										{
											Name = subreader.GetAttribute("name"),
											Description = subreader.GetAttribute("description"),
											Default = defaultbios,

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,

											SystemID = sysid,
											System = filename,
											SourceID = srcid,
										};

										// Now process and add the rom
										ParseAddHelper(biosrom, filter, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "archive":
										empty = false;

										DatItem archiverom = new Archive
										{
											Name = subreader.GetAttribute("name"),

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,

											SystemID = sysid,
											System = filename,
											SourceID = srcid,
										};

										// Now process and add the rom
										ParseAddHelper(archiverom, filter, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "sample":
										empty = false;

										DatItem samplerom = new Sample
										{
											Name = subreader.GetAttribute("name"),

											Machine = machine,

											Supported = supported,
											Publisher = publisher,
											Infos = infos,
											PartName = partname,
											PartInterface = partinterface,
											Features = features,
											AreaName = areaname,
											AreaSize = areasize,

											SystemID = sysid,
											System = filename,
											SourceID = srcid,
										};

										// Now process and add the rom
										ParseAddHelper(samplerom, filter, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									case "rom":
									case "disk":
										empty = false;

										// If the rom has a status, flag it
										its = ItemStatus.None;
										if (subreader.GetAttribute("flags") == "good" || subreader.GetAttribute("status") == "good")
										{
											its = ItemStatus.Good;
										}
										if (subreader.GetAttribute("flags") == "baddump" || subreader.GetAttribute("status") == "baddump")
										{
											logger.Verbose("Bad dump detected: " +
												(subreader.GetAttribute("name") != null && subreader.GetAttribute("name") != "" ? "\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
											its = ItemStatus.BadDump;
										}
										if (subreader.GetAttribute("flags") == "nodump" || subreader.GetAttribute("status") == "nodump")
										{
											logger.Verbose("Nodump detected: " +
												(subreader.GetAttribute("name") != null && subreader.GetAttribute("name") != "" ? "\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
											its = ItemStatus.Nodump;
										}
										if (subreader.GetAttribute("flags") == "verified" || subreader.GetAttribute("status") == "verified")
										{
											its = ItemStatus.Verified;
										}

										// If the rom has a Date attached, read it in and then sanitize it
										date = "";
										if (subreader.GetAttribute("date") != null)
										{
											DateTime dateTime = DateTime.Now;
											if (DateTime.TryParse(subreader.GetAttribute("date"), out dateTime))
											{
												date = dateTime.ToString();
											}
											else
											{
												date = subreader.GetAttribute("date");
											}
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
											int index = this[key].Count() - 1;
											DatItem lastrom = this[key][index];
											if (lastrom.Type == ItemType.Rom)
											{
												((Rom)lastrom).Size += size;
											}
											this[key].RemoveAt(index);
											this[key].Add(lastrom);
											subreader.Read();
											continue;
										}

										// If we're in clean mode, sanitize the game name
										if (clean)
										{
											machine.Name = Style.CleanGameName(machine.Name.Split(Path.DirectorySeparatorChar));
										}

										DatItem inrom;
										switch (subreader.Name.ToLowerInvariant())
										{
											case "disk":
												inrom = new Disk
												{
													Name = subreader.GetAttribute("name"),
													MD5 = subreader.GetAttribute("md5")?.ToLowerInvariant(),
													SHA1 = subreader.GetAttribute("sha1")?.ToLowerInvariant(),
													ItemStatus = its,

													Machine = machine,

													Supported = supported,
													Publisher = publisher,
													Infos = infos,
													PartName = partname,
													PartInterface = partinterface,
													Features = features,
													AreaName = areaname,
													AreaSize = areasize,

													SystemID = sysid,
													System = filename,
													SourceID = srcid,
												};
												break;
											case "rom":
											default:
												inrom = new Rom
												{
													Name = subreader.GetAttribute("name"),
													Size = size,
													CRC = subreader.GetAttribute("crc"),
													MD5 = subreader.GetAttribute("md5")?.ToLowerInvariant(),
													SHA1 = subreader.GetAttribute("sha1")?.ToLowerInvariant(),
													ItemStatus = its,
													Date = date,

													Machine = machine,

													Supported = supported,
													Publisher = publisher,
													Infos = infos,
													PartName = partname,
													PartInterface = partinterface,
													Features = features,
													AreaName = areaname,
													AreaSize = areasize,

													SystemID = sysid,
													System = filename,
													SourceID = srcid,
												};
												break;
										}

										// Now process and add the rom
										ParseAddHelper(inrom, filter, trim, single, root, clean, logger, out key);

										subreader.Read();
										break;
									default:
										subreader.Read();
										break;
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
								Type = (Type == "" ? "SuperDAT" : Type);
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

							// If the rom is itemStatus, flag it
							its = ItemStatus.None;
							flagreader = xtr.ReadSubtree();

							// If the subtree is empty, skip it
							if (flagreader == null)
							{
								xtr.Skip();
								continue;
							}

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
												case "good":
													its = ItemStatus.Good;
													break;
												case "baddump":
													logger.Verbose("Bad dump detected: " + (xtr.GetAttribute("name") != null && xtr.GetAttribute("name") != "" ?
														"\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
													its = ItemStatus.BadDump;
													break;
												case "nodump":
													logger.Verbose("Nodump detected: " + (xtr.GetAttribute("name") != null && xtr.GetAttribute("name") != "" ?
														"\"" + xtr.GetAttribute("name") + "\"" : "ROM NAME NOT FOUND"));
													its = ItemStatus.Nodump;
													break;
												case "verified":
													its = ItemStatus.Verified;
													break;
											}
										}
										break;
								}

								flagreader.Read();
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
								int index = this[key].Count() - 1;
								DatItem lastrom = this[key][index];
								if (lastrom.Type == ItemType.Rom)
								{
									((Rom)lastrom).Size += size;
								}
								this[key].RemoveAt(index);
								this[key].Add(lastrom);
								continue;
							}

							Machine dir = new Machine();

							// Get the name of the game from the parent
							dir.Name = String.Join("\\", parent);
							dir.Description = dir.Name;

							// If we aren't keeping names, trim out the path
							if (!keep || !superdat)
							{
								string tempout = Regex.Match(dir.Name, @".*?\\(.*)").Groups[1].Value;
								if (tempout != "")
								{
									dir.Name = tempout;
								}
							}

							DatItem rom;
							switch (xtr.GetAttribute("type").ToLowerInvariant())
							{
								case "disk":
									rom = new Disk
									{
										Name = xtr.GetAttribute("name"),
										MD5 = xtr.GetAttribute("md5")?.ToLowerInvariant(),
										SHA1 = xtr.GetAttribute("sha1")?.ToLowerInvariant(),
										ItemStatus = its,

										Machine = dir,

										SystemID = sysid,
										System = filename,
										SourceID = srcid,
									};
									break;
								case "rom":
								default:
									rom = new Rom
									{
										Name = xtr.GetAttribute("name"),
										Size = size,
										CRC = xtr.GetAttribute("crc")?.ToLowerInvariant(),
										MD5 = xtr.GetAttribute("md5")?.ToLowerInvariant(),
										SHA1 = xtr.GetAttribute("sha1")?.ToLowerInvariant(),
										ItemStatus = its,
										Date = date,

										Machine = dir,

										SystemID = sysid,
										System = filename,
										SourceID = srcid,
									};
									break;
							}

							// Now process and add the rom
							ParseAddHelper(rom, filter, trim, single, root, clean, logger, out key);

							xtr.Read();
							break;
						default:
							xtr.Read();
							break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Warning(ex.ToString());

				// For XML errors, just skip the affected node
				xtr?.Read();
			}

			xtr.Dispose();
		}

		/// <summary>
		/// Parse a Redump MD5 and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRedumpMD5(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				Rom rom = new Rom
				{
					Name = line.Split(' ')[1].Replace("*", String.Empty),
					Size = -1,
					MD5 = line.Split(' ')[0],
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = Path.GetFileNameWithoutExtension(filename),
					},

					SystemID = sysid,
					SourceID = srcid,
				};

				// Now process and add the rom
				string key = "";
				ParseAddHelper(rom, filter, trim, single, root, clean, logger, out key);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a Redump SFV and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRedumpSFV(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				Rom rom = new Rom
				{
					Name = line.Split(' ')[0].Replace("*", String.Empty),
					Size = -1,
					CRC = line.Split(' ')[1],
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = Path.GetFileNameWithoutExtension(filename),
					},

					SystemID = sysid,
					SourceID = srcid,
				};

				// Now process and add the rom
				string key = "";
				ParseAddHelper(rom, filter, trim, single, root, clean, logger, out key);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a Redump SHA-1 and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRedumpSHA1(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				Rom rom = new Rom
				{
					Name = line.Split(' ')[1].Replace("*", String.Empty),
					Size = -1,
					SHA1 = line.Split(' ')[0],
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = Path.GetFileNameWithoutExtension(filename),
					},

					SystemID = sysid,
					SourceID = srcid,
				};

				// Now process and add the rom
				string key = "";
				ParseAddHelper(rom, filter, trim, single, root, clean, logger, out key);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a RomCenter DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		private void ParseRC(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom filtering
			Filter filter,

			// Rom renaming
			bool trim,
			bool single,
			string root,

			// Miscellaneous
			Logger logger,
			bool clean)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(File.OpenRead(filename), enc);

			string blocktype = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// If the line is the start of the credits section
				if (line.ToLowerInvariant().StartsWith("[credits]"))
				{
					blocktype = "credits";
				}
				// If the line is the start of the dat section
				else if (line.ToLowerInvariant().StartsWith("[dat]"))
				{
					blocktype = "dat";
				}
				// If the line is the start of the emulator section
				else if (line.ToLowerInvariant().StartsWith("[emulator]"))
				{
					blocktype = "emulator";
				}
				// If the line is the start of the game section
				else if (line.ToLowerInvariant().StartsWith("[games]"))
				{
					blocktype = "games";
				}
				// Otherwise, it's not a section and it's data, so get out all data
				else
				{
					// If we have an author
					if (line.ToLowerInvariant().StartsWith("author="))
					{
						Author = (String.IsNullOrEmpty(Author) ? line.Split('=')[1] : Author);
					}
					// If we have one of the three version tags
					else if (line.ToLowerInvariant().StartsWith("version="))
					{
						switch (blocktype)
						{
							case "credits":
								Version = (String.IsNullOrEmpty(Version) ? line.Split('=')[1] : Version);
								break;
							case "emulator":
								Description = (String.IsNullOrEmpty(Description) ? line.Split('=')[1] : Description);
								break;
						}
					}
					// If we have a URL
					else if (line.ToLowerInvariant().StartsWith("url="))
					{
						Url = (String.IsNullOrEmpty(Url) ? line.Split('=')[1] : Url);
					}
					// If we have a comment
					else if (line.ToLowerInvariant().StartsWith("comment="))
					{
						Comment = (String.IsNullOrEmpty(Comment) ? line.Split('=')[1] : Comment);
					}
					// If we have the split flag
					else if (line.ToLowerInvariant().StartsWith("split="))
					{
						int split = 0;
						if (Int32.TryParse(line.Split('=')[1], out split))
						{
							if (split == 1 && ForcePacking == ForcePacking.None)
							{
								ForceMerging = ForceMerging.Split;
							}
						}
					}
					// If we have the merge tag
					else if (line.ToLowerInvariant().StartsWith("merge="))
					{
						int merge = 0;
						if (Int32.TryParse(line.Split('=')[1], out merge))
						{
							if (merge == 1 && ForceMerging == ForceMerging.None)
							{
								ForceMerging = ForceMerging.Full;
							}
						}
					}
					// If we have the refname tag
					else if (line.ToLowerInvariant().StartsWith("refname="))
					{
						Name = (String.IsNullOrEmpty(Name) ? line.Split('=')[1] : Name);
					}
					// If we have a rom
					else if (line.StartsWith("¬"))
					{
						// Some old RC DATs have this behavior
						if (line.Contains("¬N¬O"))
						{
							line = line.Replace("¬N¬O", "") + "¬¬";
						}

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

						// Try getting the size separately
						long size = 0;
						if (!Int64.TryParse(rominfo[7], out size))
						{
							size = 0;
						}

						Rom rom = new Rom
						{
							Name = rominfo[5],
							Size = size,
							CRC = rominfo[6],
							ItemStatus = ItemStatus.None,

							Machine = new Machine
							{
								Name = rominfo[3],
								Description = rominfo[4],
								CloneOf = rominfo[1],
								RomOf = rominfo[8],
							},

							SystemID = sysid,
							SourceID = srcid,
						};

						// Now process and add the rom
						string key = "";
						ParseAddHelper(rom, filter, trim, single, root, clean, logger, out key);
					}
				}
			}

			sr.Dispose();
		}

		/// <summary>
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="item">Item data to check against</param>
		/// <param name="filter">Filter object for passing to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		private void ParseAddHelper(DatItem item, Filter filter, bool trim, bool single, string root, bool clean, Logger logger, out string key)
		{
			key = "";

			// If there's no name in the rom, we log and skip it
			if (item.Name == null)
			{
				logger.Warning("Rom with no name found! Skipping...");
				return;
			}

			// If we're in cleaning mode, sanitize the game name
			item.Machine.Name = (clean ? Style.CleanGameName(item.Machine.Name) : item.Machine.Name);

			// If we have a Rom or a Disk, clean the hash data
			if (item.Type == ItemType.Rom)
			{
				Rom itemRom = (Rom)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemRom.CRC = Style.CleanHashData(itemRom.CRC, Constants.CRCLength);
				itemRom.MD5 = Style.CleanHashData(itemRom.MD5, Constants.MD5Length);
				itemRom.SHA1 = Style.CleanHashData(itemRom.SHA1, Constants.SHA1Length);

				// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
				if ((itemRom.Size == 0 || itemRom.Size == -1)
					&& ((itemRom.CRC == Constants.CRCZero || String.IsNullOrEmpty(itemRom.CRC))
						|| itemRom.MD5 == Constants.MD5Zero
						|| itemRom.SHA1 == Constants.SHA1Zero))
				{
					itemRom.Size = Constants.SizeZero;
					itemRom.CRC = Constants.CRCZero;
					itemRom.MD5 = Constants.MD5Zero;
					itemRom.SHA1 = Constants.SHA1Zero;
				}
				// If the file has no size and it's not the above case, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump && (itemRom.Size == 0 || itemRom.Size == -1))
				{
					logger.Verbose("Incomplete entry for \"" + itemRom.Name + "\" will be output as nodump");
					itemRom.ItemStatus = ItemStatus.Nodump;
				}
				// If the file has a size but aboslutely no hashes, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump
					&& itemRom.Size > 0
					&& String.IsNullOrEmpty(itemRom.CRC)
					&& String.IsNullOrEmpty(itemRom.MD5)
					&& String.IsNullOrEmpty(itemRom.SHA1))
				{
					logger.Verbose("Incomplete entry for \"" + itemRom.Name + "\" will be output as nodump");
					itemRom.ItemStatus = ItemStatus.Nodump;
				}

				item = itemRom;
			}
			else if (item.Type == ItemType.Disk)
			{
				Disk itemDisk = (Disk)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemDisk.MD5 = Style.CleanHashData(itemDisk.MD5, Constants.MD5Length);
				itemDisk.SHA1 = Style.CleanHashData(itemDisk.SHA1, Constants.SHA1Length);

				// If the file has aboslutely no hashes, skip and log
				if (itemDisk.ItemStatus != ItemStatus.Nodump
					&& String.IsNullOrEmpty(itemDisk.MD5)
					&& String.IsNullOrEmpty(itemDisk.SHA1))
				{
					logger.Verbose("Incomplete entry for \"" + itemDisk.Name + "\" will be output as nodump");
					itemDisk.ItemStatus = ItemStatus.Nodump;
				}

				item = itemDisk;
			}

			// If the rom passes the filter, include it
			if (filter.ItemPasses(item, logger))
			{
				// If we are in single game mode, rename all games
				if (single)
				{
					item.Machine.Name = "!";
				}

				// If we are in NTFS trim mode, trim the game name
				if (trim)
				{
					// Windows max name length is 260
					int usableLength = 260 - item.Machine.Name.Length - root.Length;
					if (item.Name.Length > usableLength)
					{
						string ext = Path.GetExtension(item.Name);
						item.Name = item.Name.Substring(0, usableLength - ext.Length);
						item.Name += ext;
					}
				}

				// Get the key and add statistical data
				switch (item.Type)
				{
					case ItemType.Archive:
					case ItemType.BiosSet:
					case ItemType.Release:
					case ItemType.Sample:
						key = item.Type.ToString();
						break;
					case ItemType.Disk:
						key = ((Disk)item).MD5;

						// Add statistical data
						DiskCount += 1;
						TotalSize += 0;
						MD5Count += (String.IsNullOrEmpty(((Disk)item).MD5) ? 0 : 1);
						SHA1Count += (String.IsNullOrEmpty(((Disk)item).SHA1) ? 0 : 1);
						BaddumpCount += (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						NodumpCount += (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						break;
					case ItemType.Rom:
						key = ((Rom)item).Size + "-" + ((Rom)item).CRC;

						// Add statistical data
						RomCount += 1;
						TotalSize += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 0 : ((Rom)item).Size);
						CRCCount += (String.IsNullOrEmpty(((Rom)item).CRC) ? 0 : 1);
						MD5Count += (String.IsNullOrEmpty(((Rom)item).MD5) ? 0 : 1);
						SHA1Count += (String.IsNullOrEmpty(((Rom)item).SHA1) ? 0 : 1);
						BaddumpCount += (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						NodumpCount += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						break;
					default:
						key = "default";
						break;
				}

				// Add the item to the DAT
				Add(key, item);
			}
		}

		#endregion
	}
}
