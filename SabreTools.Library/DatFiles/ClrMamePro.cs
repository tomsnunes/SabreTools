using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using StreamReader = System.IO.StreamReader;
using StreamWriter = System.IO.StreamWriter;
#endif
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents parsing and writing of a ClrMamePro DAT
	/// </summary>
	/// TODO: Separate Parse out into multiple parts, similar to the XML-derived dats
	internal class ClrMamePro : DatFile
	{
		/// <summary>
		/// Constructor designed for casting a base DatFile
		/// </summary>
		/// <param name="datFile">Parent DatFile to copy from</param>
		public ClrMamePro(DatFile datFile)
			: base(datFile, cloneHeader: false)
		{
		}

		/// <summary>
		/// Parse a ClrMamePro DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		public override void ParseFile(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Miscellaneous
			bool keep,
			bool clean,
			bool remUnicode)
		{
			// Open a file reader
			Encoding enc = Utilities.GetEncoding(filename);
			StreamReader sr = new StreamReader(Utilities.TryOpenRead(filename), enc);

			bool block = false, superdat = false, containsItems = false;
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
					containsItems = false;
				}

				// If the line is a rom-like item and we're in a block
				else if ((line.Trim().StartsWith("rom (")
						|| line.Trim().StartsWith("disk (")
						|| line.Trim().StartsWith("file (")
						|| (line.Trim().StartsWith("sample") && !line.Trim().StartsWith("sampleof"))
					) && block)
				{
					containsItems = true;
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
					DatItem item = Utilities.GetDatItem(temptype);

					// Then populate it with information
					item.MachineName = tempgamename;
					item.MachineDescription = gamedesc;
					item.CloneOf = cloneof;
					item.RomOf = romof;
					item.SampleOf = sampleof;
					item.Manufacturer = manufacturer;
					item.Year = year;

					item.SystemID = sysid;
					item.SourceID = srcid;

					// If we have a sample, treat it special
					if (temptype == ItemType.Sample)
					{
						line = line.Trim().Remove(0, 6).Trim().Replace("\"", ""); // Remove "sample" from the input string
						item.Name = line;

						// Now process and add the sample
						ParseAddHelper(item, clean, remUnicode);

						continue;
					}

					// Get the line split by spaces and quotes
					string[] gc = Utilities.SplitLineAsCMP(line);

					// Special cases for DOSCenter DATs only because of how the lines are arranged
					if (line.Trim().StartsWith("file ("))
					{
						// Loop over the specifics
						for (int i = 0; i < gc.Length; i++)
						{
							// Names are not quoted, for some stupid reason
							if (gc[i] == "name")
							{
								// Get the name in order until we find the next flag
								while (++i < gc.Length && gc[i] != "size" && gc[i] != "date" && gc[i] != "crc" && gc[i] != "md5"
									&& gc[i] != "sha1" && gc[i] != "sha256" && gc[i] != "sha384" && gc[i] != "sha512")
								{
									item.Name += " " + gc[i];
								}

								// Perform correction
								item.Name = item.Name.TrimStart();
								i--;
							}

							// Get the size from the next part
							else if (gc[i] == "size")
							{
								long tempsize = -1;
								if (!Int64.TryParse(gc[++i], out tempsize))
								{
									tempsize = 0;
								}
								((Rom)item).Size = tempsize;
							}

							// Get the date from the next part
							else if (gc[i] == "date")
							{
								((Rom)item).Date = gc[++i].Replace("\"", "") + " " + gc[++i].Replace("\"", "");
							}

							// Get the CRC from the next part
							else if (gc[i] == "crc")
							{
								((Rom)item).CRC = gc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the MD5 from the next part
							else if (gc[i] == "md5")
							{
								((Rom)item).MD5 = gc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA1 from the next part
							else if (gc[i] == "sha1")
							{
								((Rom)item).SHA1 = gc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA256 from the next part
							else if (gc[i] == "sha256")
							{
								((Rom)item).SHA256 = gc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA384 from the next part
							else if (gc[i] == "sha384")
							{
								((Rom)item).SHA384 = gc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA512 from the next part
							else if (gc[i] == "sha512")
							{
								((Rom)item).SHA512 = gc[++i].Replace("\"", "").ToLowerInvariant();
							}
						}

						// Now process and add the rom
						ParseAddHelper(item, clean, remUnicode);
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
								quoteless = gc[++i].Replace("\"", "");
								item.Name = quoteless;
								break;
							case "size":
								if (item.Type == ItemType.Rom)
								{
									quoteless = gc[++i].Replace("\"", "");
									if (Int64.TryParse(quoteless, out long size))
									{
										((Rom)item).Size = size;
									}
									else
									{
										((Rom)item).Size = -1;
									}
								}
								break;
							case "crc":
								if (item.Type == ItemType.Rom)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Rom)item).CRC = quoteless.ToLowerInvariant();
								}
								break;
							case "md5":
								if (item.Type == ItemType.Rom)
								{
									quoteless = gc[++i].Replace("\"", "");
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
									quoteless = gc[++i].Replace("\"", "");
									((Rom)item).SHA1 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Disk)item).SHA1 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha256":
								if (item.Type == ItemType.Rom)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Rom)item).SHA256 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Disk)item).SHA256 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha384":
								if (item.Type == ItemType.Rom)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Rom)item).SHA384 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Disk)item).SHA384 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha512":
								if (item.Type == ItemType.Rom)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Rom)item).SHA512 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = gc[++i].Replace("\"", "");
									((Disk)item).SHA512 = quoteless.ToLowerInvariant();
								}
								break;
							case "status":
							case "flags":
								quoteless = gc[++i].Replace("\"", "");
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
									// If we have quotes in the next item, assume only one item
									if (gc[i + 1].Contains("\""))
									{
										quoteless = gc[++i].Replace("\"", "");
									}
									// Otherwise, we assume we need to read the next two items
									else
									{
										quoteless = gc[++i].Replace("\"", "") + " " + gc[++i].Replace("\"", "");
									}
									((Rom)item).Date = quoteless;
								}
								break;
						}
					}

					// Now process and add the rom
					ParseAddHelper(item, clean, remUnicode);
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
							Name = (String.IsNullOrWhiteSpace(Name) ? line.Substring(6) : Name);
							superdat = superdat || itemval.Contains(" - SuperDAT");
							if (keep && superdat)
							{
								Type = (String.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
							}
							continue;
						}

						switch (gc[1].Value)
						{
							case "name":
							case "Name:":
								Name = (String.IsNullOrWhiteSpace(Name) ? itemval : Name);
								superdat = superdat || itemval.Contains(" - SuperDAT");
								if (keep && superdat)
								{
									Type = (String.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
								}
								break;
							case "description":
							case "Description:":
								Description = (String.IsNullOrWhiteSpace(Description) ? itemval : Description);
								break;
							case "rootdir":
								RootDir = (String.IsNullOrWhiteSpace(RootDir) ? itemval : RootDir);
								break;
							case "category":
								Category = (String.IsNullOrWhiteSpace(Category) ? itemval : Category);
								break;
							case "version":
							case "Version:":
								Version = (String.IsNullOrWhiteSpace(Version) ? itemval : Version);
								break;
							case "date":
							case "Date:":
								Date = (String.IsNullOrWhiteSpace(Date) ? itemval : Date);
								break;
							case "author":
							case "Author:":
								Author = (String.IsNullOrWhiteSpace(Author) ? itemval : Author);
								break;
							case "email":
								Email = (String.IsNullOrWhiteSpace(Email) ? itemval : Email);
								break;
							case "homepage":
							case "Homepage:":
								Homepage = (String.IsNullOrWhiteSpace(Homepage) ? itemval : Homepage);
								break;
							case "url":
								Url = (String.IsNullOrWhiteSpace(Url) ? itemval : Url);
								break;
							case "comment":
							case "Comment:":
								Comment = (String.IsNullOrWhiteSpace(Comment) ? itemval : Comment);
								break;
							case "header":
								Header = (String.IsNullOrWhiteSpace(Header) ? itemval : Header);
								break;
							case "type":
								Type = (String.IsNullOrWhiteSpace(Type) ? itemval : Type);
								superdat = superdat || itemval.Contains("SuperDAT");
								break;
							case "forcemerging":
								if (ForceMerging == ForceMerging.None)
								{
									ForceMerging = Utilities.GetForceMerging(itemval);
								}
								break;
							case "forcezipping":
								if (ForcePacking == ForcePacking.None)
								{
									ForcePacking = Utilities.GetForcePacking(itemval);
								}
								break;
							case "forcepacking":
								if (ForcePacking == ForcePacking.None)
								{
									ForcePacking = Utilities.GetForcePacking(itemval);
								}
								break;
						}
					}
				}

				// If we find an end bracket that's not associated with anything else, the block is done
				else if (Regex.IsMatch(line, Constants.EndPatternCMP) && block)
				{
					// If no items were found for this machine, add a Blank placeholder
					if (!containsItems)
					{
						Blank blank = new Blank()
						{
							MachineName = tempgamename,
							MachineDescription = gamedesc,
							CloneOf = cloneof,
							RomOf = romof,
							SampleOf = sampleof,
							Manufacturer = manufacturer,
							Year = year,

							SystemID = sysid,
							SourceID = srcid,
						};

						// Now process and add the rom
						ParseAddHelper(blank, clean, remUnicode);
					}

					block = false; containsItems = false;
					blockname = ""; tempgamename = ""; gamedesc = ""; cloneof = "";
					romof = ""; sampleof = ""; year = ""; manufacturer = "";
				}
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a ClrMamePro DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		public void ParseFileStripped(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Miscellaneous
			bool keep,
			bool clean,
			bool remUnicode)
		{
			// Open a file reader
			Encoding enc = Utilities.GetEncoding(filename);
			StreamReader sr = new StreamReader(Utilities.TryOpenRead(filename), enc);

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

					// If we have a known header
					if (gc[1].Value == "clrmamepro"
						|| gc[1].Value == "romvault"
						|| gc[1].Value.ToLowerInvariant() == "doscenter")
					{
						ReadHeader(sr, keep);
					}
					// If we have a known set type
					else if (gc[1].Value == "set"
						|| gc[1].Value == "game"
						|| gc[1].Value == "machine")
					{
						ReadSet(sr, filename, sysid, srcid, keep, clean, remUnicode);
					}
				}
			}

			sr.Dispose();
		}

		/// <summary>
		/// Read header information
		/// </summary>
		/// <param name="reader">StreamReader to use to parse the header</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// TODO: Make sure this only is called if the block is "clrmamepro", "doscenter", "romcenter"
		private void ReadHeader(StreamReader reader, bool keep)
		{
			bool superdat = false;

			// If there's no subtree to the header, skip it
			if (reader == null || reader.EndOfStream)
			{
				return;
			}

			// Otherwise, add what is possible
			string line = reader.ReadLine();
			while (!Regex.IsMatch(line, Constants.EndPatternCMP))
			{
				// We only want elements
				if (line.Trim().StartsWith("#"))
				{
					line = reader.ReadLine();
					continue;
				}

				// Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
				GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
				string itemval = gc[2].Value.Replace("\"", "");

				if (line.Trim().StartsWith("Name:"))
				{
					Name = (String.IsNullOrWhiteSpace(Name) ? line.Substring(6) : Name);
					superdat = superdat || itemval.Contains(" - SuperDAT");
					if (keep && superdat)
					{
						Type = (String.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
					}

					line = reader.ReadLine();
					continue;
				}

				switch (gc[1].Value)
				{
					case "name":
					case "Name:":
						Name = (String.IsNullOrWhiteSpace(Name) ? itemval : Name);
						superdat = superdat || itemval.Contains(" - SuperDAT");
						if (keep && superdat)
						{
							Type = (String.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
						}
						break;
					case "description":
					case "Description:":
						Description = (String.IsNullOrWhiteSpace(Description) ? itemval : Description);
						break;
					case "rootdir":
					case "Rootdir:":
						RootDir = (String.IsNullOrWhiteSpace(RootDir) ? itemval : RootDir);
						break;
					case "category":
					case "Category:":
						Category = (String.IsNullOrWhiteSpace(Category) ? itemval : Category);
						break;
					case "version":
					case "Version:":
						Version = (String.IsNullOrWhiteSpace(Version) ? itemval : Version);
						break;
					case "date":
					case "Date:":
						Date = (String.IsNullOrWhiteSpace(Date) ? itemval : Date);
						break;
					case "author":
					case "Author:":
						Author = (String.IsNullOrWhiteSpace(Author) ? itemval : Author);
						break;
					case "email":
					case "Email:":
						Email = (String.IsNullOrWhiteSpace(Email) ? itemval : Email);
						break;
					case "homepage":
					case "Homepage:":
						Homepage = (String.IsNullOrWhiteSpace(Homepage) ? itemval : Homepage);
						break;
					case "url":
					case "Url:":
						Url = (String.IsNullOrWhiteSpace(Url) ? itemval : Url);
						break;
					case "comment":
					case "Comment:":
						Comment = (String.IsNullOrWhiteSpace(Comment) ? itemval : Comment);
						break;
					case "header":
					case "Header:":
						Header = (String.IsNullOrWhiteSpace(Header) ? itemval : Header);
						break;
					case "type":
					case "Type:":
						Type = (String.IsNullOrWhiteSpace(Type) ? itemval : Type);
						superdat = superdat || itemval.Contains("SuperDAT");
						break;
					case "forcemerging":
						if (ForceMerging == ForceMerging.None)
						{
							ForceMerging = Utilities.GetForceMerging(itemval);
						}
						break;
					case "forcezipping":
						if (ForcePacking == ForcePacking.None)
						{
							ForcePacking = Utilities.GetForcePacking(itemval);
						}
						break;
					case "forcepacking":
						if (ForcePacking == ForcePacking.None)
						{
							ForcePacking = Utilities.GetForcePacking(itemval);
						}
						break;
				}

				line = reader.ReadLine();
			}
		}

		/// <summary>
		/// Read set information
		/// </summary>
		/// <param name="reader">StreamReader to use to parse the header</param>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// TODO: Make sure this is only called if the block is "set", "game", "machine"
		private void ReadSet(
			StreamReader reader,

			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Miscellaneous
			bool keep,
			bool clean,
			bool remUnicode)
		{
			// Prepare all internal variables
			bool containsItems = false;
			Machine machine = new Machine();

			// If there's no subtree to the header, skip it
			if (reader == null || reader.EndOfStream)
			{
				return;
			}

			// Otherwise, add what is possible
			string line = reader.ReadLine();
			while (!Regex.IsMatch(line, Constants.EndPatternCMP))
			{
				// We only want elements
				if (line.Trim().StartsWith("#"))
				{
					line = reader.ReadLine();
					continue;
				}

				// Item-specific lines have a known pattern
				string trimmedline = line.Trim();
				if (trimmedline.StartsWith("archive (")
					|| trimmedline.StartsWith("biosset (")
					|| trimmedline.StartsWith("disk (")
					|| trimmedline.StartsWith("file (") // This is a DOSCenter file, not a SabreDAT file
					|| trimmedline.StartsWith("release (")
					|| trimmedline.StartsWith("rom (")
					|| (trimmedline.StartsWith("sample") && !trimmedline.StartsWith("sampleof")))
				{
					containsItems = true;
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
					DatItem item = Utilities.GetDatItem(temptype);

					// Then populate it with information
					item.CopyMachineInformation(machine);

					item.SourceFile = filename;
					item.SystemID = sysid;
					item.SourceID = srcid;

					// If we have a sample, treat it special
					if (temptype == ItemType.Sample)
					{
						line = line.Trim().Remove(0, 6).Trim().Replace("\"", ""); // Remove "sample" from the input string
						item.Name = line;

						// Now process and add the sample
						ParseAddHelper(item, clean, remUnicode);
						line = reader.ReadLine();
						continue;
					}

					// Get the line split by spaces and quotes
					string[] linegc = Utilities.SplitLineAsCMP(line);

					// Special cases for DOSCenter DATs only because of how the lines are arranged
					if (line.Trim().StartsWith("file ("))
					{
						// Loop over the specifics
						for (int i = 0; i < linegc.Length; i++)
						{
							// Names are not quoted, for some stupid reason
							if (linegc[i] == "name")
							{
								// Get the name in order until we find the next flag
								while (++i < linegc.Length && linegc[i] != "size"
									&& linegc[i] != "date"
									&& linegc[i] != "crc"
									&& linegc[i] != "md5"
									&& linegc[i] != "sha1"
									&& linegc[i] != "sha256"
									&& linegc[i] != "sha384"
									&& linegc[i] != "sha512")
								{
									item.Name += " " + linegc[i];
								}

								// Perform correction
								item.Name = item.Name.TrimStart();
								i--;
							}

							// Get the size from the next part
							else if (linegc[i] == "size")
							{
								long tempsize = -1;
								if (!Int64.TryParse(linegc[++i], out tempsize))
								{
									tempsize = 0;
								}
								((Rom)item).Size = tempsize;
							}

							// Get the date from the next part
							else if (linegc[i] == "date")
							{
								((Rom)item).Date = linegc[++i].Replace("\"", "") + " " + linegc[++i].Replace("\"", "");
							}

							// Get the CRC from the next part
							else if (linegc[i] == "crc")
							{
								((Rom)item).CRC = linegc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the MD5 from the next part
							else if (linegc[i] == "md5")
							{
								((Rom)item).MD5 = linegc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA1 from the next part
							else if (linegc[i] == "sha1")
							{
								((Rom)item).SHA1 = linegc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA256 from the next part
							else if (linegc[i] == "sha256")
							{
								((Rom)item).SHA256 = linegc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA384 from the next part
							else if (linegc[i] == "sha384")
							{
								((Rom)item).SHA384 = linegc[++i].Replace("\"", "").ToLowerInvariant();
							}

							// Get the SHA512 from the next part
							else if (linegc[i] == "sha512")
							{
								((Rom)item).SHA512 = linegc[++i].Replace("\"", "").ToLowerInvariant();
							}
						}

						// Now process and add the rom
						ParseAddHelper(item, clean, remUnicode);
						line = reader.ReadLine();
						continue;
					}

					// Loop over all attributes normally and add them if possible
					for (int i = 0; i < linegc.Length; i++)
					{
						// Look at the current item and use it if possible
						string quoteless = linegc[i].Replace("\"", "");
						switch (quoteless)
						{
							//If the item is empty, we automatically skip it because it's a fluke
							case "":
								continue;

							// Special cases for standalone item statuses
							case "baddump":
							case "good":
							case "nodump":
							case "verified":
								ItemStatus tempStandaloneStatus = Utilities.GetItemStatus(quoteless);
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = tempStandaloneStatus;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = tempStandaloneStatus;
								}
								break;

							// Regular attributes
							case "name":
								quoteless = linegc[++i].Replace("\"", "");
								item.Name = quoteless;
								break;
							case "size":
								if (item.Type == ItemType.Rom)
								{
									quoteless = linegc[++i].Replace("\"", "");
									if (Int64.TryParse(quoteless, out long size))
									{
										((Rom)item).Size = size;
									}
									else
									{
										((Rom)item).Size = -1;
									}
								}
								break;
							case "crc":
								if (item.Type == ItemType.Rom)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Rom)item).CRC = quoteless.ToLowerInvariant();
								}
								break;
							case "md5":
								if (item.Type == ItemType.Rom)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Rom)item).MD5 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									i++;
									quoteless = linegc[i].Replace("\"", "");
									((Disk)item).MD5 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha1":
								if (item.Type == ItemType.Rom)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Rom)item).SHA1 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Disk)item).SHA1 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha256":
								if (item.Type == ItemType.Rom)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Rom)item).SHA256 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Disk)item).SHA256 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha384":
								if (item.Type == ItemType.Rom)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Rom)item).SHA384 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Disk)item).SHA384 = quoteless.ToLowerInvariant();
								}
								break;
							case "sha512":
								if (item.Type == ItemType.Rom)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Rom)item).SHA512 = quoteless.ToLowerInvariant();
								}
								else if (item.Type == ItemType.Disk)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Disk)item).SHA512 = quoteless.ToLowerInvariant();
								}
								break;
							case "status":
							case "flags":
								quoteless = linegc[++i].Replace("\"", "");
								ItemStatus tempFlagStatus = Utilities.GetItemStatus(quoteless);
								if (item.Type == ItemType.Rom)
								{
									((Rom)item).ItemStatus = tempFlagStatus;
								}
								else if (item.Type == ItemType.Disk)
								{
									((Disk)item).ItemStatus = tempFlagStatus;
								}
								break;
							case "date":
								if (item.Type == ItemType.Rom)
								{
									// If we have quotes in the next item, assume only one item
									if (linegc[i + 1].Contains("\""))
									{
										quoteless = linegc[++i].Replace("\"", "");
									}
									// Otherwise, we assume we need to read the next two items
									else
									{
										quoteless = linegc[++i].Replace("\"", "") + " " + linegc[++i].Replace("\"", "");
									}
									((Rom)item).Date = quoteless;
								}
								else if (item.Type == ItemType.Release)
								{
									// If we have quotes in the next item, assume only one item
									if (linegc[i + 1].Contains("\""))
									{
										quoteless = linegc[++i].Replace("\"", "");
									}
									// Otherwise, we assume we need to read the next two items
									else
									{
										quoteless = linegc[++i].Replace("\"", "") + " " + linegc[++i].Replace("\"", "");
									}
									((Release)item).Date = quoteless;
								}
								break;
							case "default":
								if (item.Type == ItemType.BiosSet)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((BiosSet)item).Default = Utilities.GetYesNo(quoteless.ToLowerInvariant());
								}
								else if (item.Type == ItemType.Release)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Release)item).Default = Utilities.GetYesNo(quoteless.ToLowerInvariant());
								}
								break;
							case "description":
								if (item.Type == ItemType.BiosSet)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((BiosSet)item).Description = quoteless.ToLowerInvariant();
								}
								break;
							case "region":
								if (item.Type == ItemType.Release)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Release)item).Region = quoteless.ToLowerInvariant();
								}
								break;
							case "language":
								if (item.Type == ItemType.Release)
								{
									quoteless = linegc[++i].Replace("\"", "");
									((Release)item).Language = quoteless.ToLowerInvariant();
								}
								break;
						}
					}

					// Now process and add the rom
					ParseAddHelper(item, clean, remUnicode);

					line = reader.ReadLine();
					continue;
				}

				// Set-specific lines have a known pattern
				GroupCollection setgc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
				string itemval = setgc[2].Value.Replace("\"", "");

				switch (setgc[1].Value)
				{
					case "name":
						machine.Name = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
						machine.Description = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
						break;
					case "description":
						machine.Description = itemval;
						break;
					case "year":
						machine.Year = itemval;
						break;
					case "manufacturer":
						machine.Manufacturer = itemval;
						break;
					case "cloneof":
						machine.CloneOf = itemval;
						break;
					case "romof":
						machine.RomOf = itemval;
						break;
					case "sampleof":
						machine.SampleOf = itemval;
						break;
				}

				line = reader.ReadLine();
			}

			// If no items were found for this machine, add a Blank placeholder
			if (!containsItems)
			{
				Blank blank = new Blank()
				{
					SourceFile = filename,
					SystemID = sysid,
					SourceID = srcid,
				};
				blank.CopyMachineInformation(machine);

				// Now process and add the rom
				ParseAddHelper(blank, clean, remUnicode);
			}
		}

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="outfile">Name of the file to write to</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		public override bool WriteToFile(string outfile, bool ignoreblanks = false)
		{
			try
			{
				Globals.Logger.User("Opening file for writing: {0}", outfile);
				FileStream fs = Utilities.TryCreate(outfile);

				// If we get back null for some reason, just log and return
				if (fs == null)
				{
					Globals.Logger.Warning("File '{0}' could not be created for writing! Please check to see if the file is writable", outfile);
					return false;
				}

				StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));

				// Write out the header
				WriteHeader(sw);

				// Write out each of the machines and roms
				string lastgame = null;

				// Get a properly sorted set of keys
				List<string> keys = Keys;
				keys.Sort(new NaturalComparer());

				foreach (string key in keys)
				{
					List<DatItem> roms = this[key];

					// Resolve the names in the block
					roms = DatItem.ResolveNames(roms);

					for (int index = 0; index < roms.Count; index++)
					{
						DatItem rom = roms[index];

						// There are apparently times when a null rom can skip by, skip them
						if (rom.Name == null || rom.MachineName == null)
						{
							Globals.Logger.Warning("Null rom found!");
							continue;
						}

						// If we have a different game and we're not at the start of the list, output the end of last item
						if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
						{
							WriteEndGame(sw, rom);
						}

						// If we have a new game, output the beginning of the new item
						if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
						{
							WriteStartGame(sw, rom);
						}

						// If we have a "null" game (created by DATFromDir or something similar), log it to file
						if (rom.Type == ItemType.Rom
							&& ((Rom)rom).Size == -1
							&& ((Rom)rom).CRC == "null")
						{
							Globals.Logger.Verbose("Empty folder found: {0}", rom.MachineName);

							// If we're in a mode that doesn't allow for actual empty folders, add the blank info
							rom.Name = (rom.Name == "null" ? "-" : rom.Name);
							((Rom)rom).Size = Constants.SizeZero;
							((Rom)rom).CRC = ((Rom)rom).CRC == "null" ? Constants.CRCZero : null;
							((Rom)rom).MD5 = ((Rom)rom).MD5 == "null" ? Constants.MD5Zero : null;
							((Rom)rom).SHA1 = ((Rom)rom).SHA1 == "null" ? Constants.SHA1Zero : null;
							((Rom)rom).SHA256 = ((Rom)rom).SHA256 == "null" ? Constants.SHA256Zero : null;
							((Rom)rom).SHA384 = ((Rom)rom).SHA384 == "null" ? Constants.SHA384Zero : null;
							((Rom)rom).SHA512 = ((Rom)rom).SHA512 == "null" ? Constants.SHA512Zero : null;
						}

						// Now, output the rom data
						WriteDatItem(sw, rom, ignoreblanks);

						// Set the new data to compare against
						lastgame = rom.MachineName;
					}
				}

				// Write the file footer out
				WriteFooter(sw);

				Globals.Logger.Verbose("File written!" + Environment.NewLine);
				sw.Dispose();
				fs.Dispose();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT header using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteHeader(StreamWriter sw)
		{
			try
			{
				string header = "clrmamepro (\n" +
							"\tname \"" + Name + "\"\n" +
							"\tdescription \"" + Description + "\"\n" +
							(!String.IsNullOrWhiteSpace(Category) ? "\tcategory \"" + Category + "\"\n" : "") +
							"\tversion \"" + Version + "\"\n" +
							(!String.IsNullOrWhiteSpace(Date) ? "\tdate \"" + Date + "\"\n" : "") +
							"\tauthor \"" + Author + "\"\n" +
							(!String.IsNullOrWhiteSpace(Email) ? "\temail \"" + Email + "\"\n" : "") +
							(!String.IsNullOrWhiteSpace(Homepage) ? "\thomepage \"" + Homepage + "\"\n" : "") +
							(!String.IsNullOrWhiteSpace(Url) ? "\turl \"" + Url + "\"\n" : "") +
							(!String.IsNullOrWhiteSpace(Comment) ? "\tcomment \"" + Comment + "\"\n" : "") +
							(ForcePacking == ForcePacking.Unzip ? "\tforcezipping no\n" : "") +
							(ForcePacking == ForcePacking.Zip ? "\tforcezipping yes\n" : "") +
							(ForceMerging == ForceMerging.Full ? "\tforcemerging full\n" : "") +
							(ForceMerging == ForceMerging.Split ? "\tforcemerging split\n" : "") +
							(ForceMerging == ForceMerging.Merged ? "\tforcemerging merged\n" : "") +
							(ForceMerging == ForceMerging.NonMerged ? "\tforcemerging nonmerged\n" : "") +
							")\n";

				// Write the header out
				sw.Write(header);
				sw.Flush();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">DatItem object to be output</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteStartGame(StreamWriter sw, DatItem rom)
		{
			try
			{
				// No game should start with a path separator
				if (rom.MachineName.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.MachineName = rom.MachineName.Substring(1);
				}

				string state = "game (\n\tname \"" + rom.MachineName + "\"\n" +
							(ExcludeOf ? "" :
								(String.IsNullOrWhiteSpace(rom.RomOf) ? "" : "\tromof \"" + rom.RomOf + "\"\n") +
								(String.IsNullOrWhiteSpace(rom.CloneOf) ? "" : "\tcloneof \"" + rom.CloneOf + "\"\n") +
								(String.IsNullOrWhiteSpace(rom.SampleOf) ? "" : "\tsampleof \"" + rom.SampleOf + "\"\n")
							) +
							"\tdescription \"" + (String.IsNullOrWhiteSpace(rom.MachineDescription) ? rom.MachineName : rom.MachineDescription) + "\"\n" +
							(String.IsNullOrWhiteSpace(rom.Year) ? "" : "\tyear " + rom.Year + "\n") +
							(String.IsNullOrWhiteSpace(rom.Manufacturer) ? "" : "\tmanufacturer \"" + rom.Manufacturer + "\"\n");

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out Game end using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">DatItem object to be output</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteEndGame(StreamWriter sw, DatItem rom)
		{
			try
			{
				string state = (String.IsNullOrWhiteSpace(rom.SampleOf) ? "" : "\tsampleof \"" + rom.SampleOf + "\"\n") + ")\n";

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DatItem using the supplied StreamWriter
		/// </summary>
		/// <param name="datFile">DatFile to write out from</param>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">DatItem object to be output</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteDatItem(StreamWriter sw, DatItem rom, bool ignoreblanks = false)
		{
			// If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
			if (ignoreblanks
				&& (rom.Type == ItemType.Rom
				&& (((Rom)rom).Size == 0 || ((Rom)rom).Size == -1)))
			{
				return true;
			}

			try
			{
				string state = "";
				switch (rom.Type)
				{
					case ItemType.Archive:
						state += "\tarchive ( name\"" + rom.Name + "\""
							+ " )\n";
						break;
					case ItemType.BiosSet:
						state += "\tbiosset ( name\"" + rom.Name + "\""
							+ (!String.IsNullOrWhiteSpace(((BiosSet)rom).Description) ? " description \"" + ((BiosSet)rom).Description + "\"" : "")
							+ (((BiosSet)rom).Default != null
								? "default " + ((BiosSet)rom).Default.ToString().ToLowerInvariant()
								: "")
							+ " )\n";
						break;
					case ItemType.Disk:
						state += "\tdisk ( name \"" + rom.Name + "\""
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).MD5) ? " md5 " + ((Disk)rom).MD5.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA1) ? " sha1 " + ((Disk)rom).SHA1.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA256) ? " sha256 " + ((Disk)rom).SHA256.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA384) ? " sha384 " + ((Disk)rom).SHA384.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA512) ? " sha512 " + ((Disk)rom).SHA512.ToLowerInvariant() : "")
							+ (((Disk)rom).ItemStatus != ItemStatus.None ? " flags " + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() : "")
							+ " )\n";
						break;
					case ItemType.Release:
						state += "\trelease ( name\"" + rom.Name + "\""
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Region) ? " region \"" + ((Release)rom).Region + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Language) ? " language \"" + ((Release)rom).Language + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Date) ? " date \"" + ((Release)rom).Date + "\"" : "")
							+ (((Release)rom).Default != null
								? "default " + ((Release)rom).Default.ToString().ToLowerInvariant()
								: "")
							+ " )\n";
						break;
					case ItemType.Rom:
						state += "\trom ( name \"" + rom.Name + "\""
							+ (((Rom)rom).Size != -1 ? " size " + ((Rom)rom).Size : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).CRC) ? " crc " + ((Rom)rom).CRC.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).MD5) ? " md5 " + ((Rom)rom).MD5.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA1) ? " sha1 " + ((Rom)rom).SHA1.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA256) ? " sha256 " + ((Rom)rom).SHA256.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA384) ? " sha384 " + ((Rom)rom).SHA384.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA512) ? " sha512 " + ((Rom)rom).SHA512.ToLowerInvariant() : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).Date) ? " date \"" + ((Rom)rom).Date + "\"" : "")
							+ (((Rom)rom).ItemStatus != ItemStatus.None ? " flags " + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() : "")
							+ " )\n";
						break;
					case ItemType.Sample:
						state += "\tsample ( name\"" + rom.Name + "\""
							+ " )\n";
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT footer using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteFooter(StreamWriter sw)
		{
			try
			{
				string footer = footer = ")\n";

				// Write the footer out
				sw.Write(footer);
				sw.Flush();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}
	}
}
