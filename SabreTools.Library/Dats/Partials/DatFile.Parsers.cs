using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using StreamReader = System.IO.StreamReader;
#endif

namespace SabreTools.Library.Dats
{
	public partial class DatFile
	{
		#region Parsing

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="datdata">The DatData object representing found roms to this point</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		/// <param name="useTags">True if tags from the DAT should be used to merge the output, false otherwise (default)</param>
		public void Parse(string filename, int sysid, int srcid, bool keep = false, bool clean = false,
			bool remUnicode = false, bool descAsName = false, bool keepext = false, bool useTags = false)
		{
			Parse(filename, sysid, srcid, SplitType.None, keep: keep, clean: clean,
				remUnicode: remUnicode, descAsName: descAsName, keepext: keepext, useTags: useTags);
		}

		/// <summary>
		/// Parse a DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
		/// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
		/// <param name="useTags">True if tags from the DAT should be used to merge the output, false otherwise (default)</param>
		public void Parse(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Rom renaming
			SplitType splitType,

			// Miscellaneous
			bool keep = false,
			bool clean = false,
			bool remUnicode = false,
			bool descAsName = false,
			bool keepext = false,
			bool useTags = false)
		{
			// Check the file extension first as a safeguard
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}
			if (ext != "dat" && ext != "csv" && ext != "md5" && ext != "sfv" && ext != "sha1" && ext != "sha256"
				&& ext != "sha384" && ext != "sha512" && ext != "tsv" && ext != "txt" && ext != "xml")
			{
				return;
			}

			// If the output filename isn't set already, get the internal filename
			FileName = (String.IsNullOrEmpty(FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : FileName);

			// If the output type isn't set already, get the internal output type
			DatFormat = (DatFormat == 0 ? FileTools.GetDatFormat(filename) : DatFormat);

			// Now parse the correct type of DAT
			try
			{
				switch (FileTools.GetDatFormat(filename))
				{
					case DatFormat.AttractMode:
						ParseAttractMode(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.ClrMamePro:
					case DatFormat.DOSCenter:
						ParseCMP(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.CSV:
						ParseCSVTSV(filename, sysid, srcid, ',', keep, clean, remUnicode);
						break;
					case DatFormat.Listroms:
						ParseListroms(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.Logiqx:
					case DatFormat.OfflineList:
					case DatFormat.SabreDat:
					case DatFormat.SoftwareList:
						ParseGenericXML(filename, sysid, srcid, keep, clean, remUnicode);
						break;
					case DatFormat.RedumpMD5:
						ParseHashfile(filename, sysid, srcid, Hash.MD5, clean, remUnicode);
						break;
					case DatFormat.RedumpSFV:
						ParseHashfile(filename, sysid, srcid, Hash.CRC, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA1:
						ParseHashfile(filename, sysid, srcid, Hash.SHA1, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA256:
						ParseHashfile(filename, sysid, srcid, Hash.SHA256, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA384:
						ParseHashfile(filename, sysid, srcid, Hash.SHA384, clean, remUnicode);
						break;
					case DatFormat.RedumpSHA512:
						ParseHashfile(filename, sysid, srcid, Hash.SHA512, clean, remUnicode);
						break;
					case DatFormat.RomCenter:
						ParseRC(filename, sysid, srcid, clean, remUnicode);
						break;
					case DatFormat.TSV:
						ParseCSVTSV(filename, sysid, srcid, '\t', keep, clean, remUnicode);
						break;
					default:
						return;
				}
			}
			catch (Exception ex)
			{
				Globals.Logger.Error("Error with file '{0}': {1}", filename, ex);
			}

			// If we want to use descriptions as names, update everything
			if (descAsName)
			{
				MachineDescriptionToName();
			}

			// If we are using tags from the DAT, set the proper input for split type unless overridden
			if (useTags && splitType == SplitType.None)
			{
				switch (ForceMerging)
				{
					case ForceMerging.None:
						// No-op
						break;
					case ForceMerging.Split:
						splitType = SplitType.Split;
						break;
					case ForceMerging.Merged:
						splitType = SplitType.Merged;
						break;
					case ForceMerging.NonMerged:
						splitType = SplitType.NonMerged;
						break;
					case ForceMerging.Full:
						splitType = SplitType.FullNonMerged;
						break;
				}
			}

			// Now we pre-process the DAT with the splitting/merging mode
			switch (splitType)
			{
				case SplitType.None:
					// No-op
					break;
				case SplitType.DeviceNonMerged:
					CreateDeviceNonMergedSets(DedupeType.None);
					break;
				case SplitType.FullNonMerged:
					CreateFullyNonMergedSets(DedupeType.None);
					break;
				case SplitType.NonMerged:
					CreateNonMergedSets(DedupeType.None);
					break;
				case SplitType.Merged:
					CreateMergedSets(DedupeType.None);
					break;
				case SplitType.Split:
					CreateSplitSets(DedupeType.None);
					break;
			}
		}

		/// <summary>
		/// Parse an AttractMode DAT and return all found games within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		private void ParseAttractMode(
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
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(FileTools.TryOpenRead(filename), enc);

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
				ParseAddHelper(rom, clean, remUnicode);
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
		private void ParseCMP(
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
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(FileTools.TryOpenRead(filename), enc);

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
										case "merged":
											ForceMerging = ForceMerging.Merged;
											break;
										case "nonmerged":
											ForceMerging = ForceMerging.NonMerged;
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
		/// Parse a CSV or a TSV and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="delim">Delimiter for parsing individual lines</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		private void ParseCSVTSV(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,
			char delim,

			// Miscellaneous
			bool keep,
			bool clean,
			bool remUnicode)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(FileTools.TryOpenRead(filename), enc);

			// Create an empty list of columns to parse though
			List<string> columns = new List<string>();

			long linenum = -1;
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();
				linenum++;

				// Parse the first line, getting types from the column names
				if (linenum == 0)
				{
					string[] parsedColumns = line.Split(delim);
					foreach (string parsed in parsedColumns)
					{
						switch (parsed.ToLowerInvariant().Trim('"'))
						{
							case "file":
							case "filename":
							case "file name":
								columns.Add("DatFile.FileName");
								break;
							case "internal name":
								columns.Add("DatFile.Name");
								break;
							case "description":
							case "dat description":
								columns.Add("DatFile.Description");
								break;
							case "game name":
							case "game":
							case "machine":
								columns.Add("Machine.Name");
								break;
							case "game description":
								columns.Add("Machine.Description");
								break;
							case "type":
								columns.Add("DatItem.Type");
								break;
							case "rom":
							case "romname":
							case "rom name":
							case "name":
								columns.Add("Rom.Name");
								break;
							case "disk":
							case "diskname":
							case "disk name":
								columns.Add("Disk.Name");
								break;
							case "size":
								columns.Add("DatItem.Size");
								break;
							case "crc":
							case "crc hash":
								columns.Add("DatItem.CRC");
								break;
							case "md5":
							case "md5 hash":
								columns.Add("DatItem.MD5");
								break;
							case "sha1":
							case "sha-1":
							case "sha1 hash":
							case "sha-1 hash":
								columns.Add("DatItem.SHA1");
								break;
							case "sha256":
							case "sha-256":
							case "sha256 hash":
							case "sha-256 hash":
								columns.Add("DatItem.SHA256");
								break;
							case "sha384":
							case "sha-384":
							case "sha384 hash":
							case "sha-384 hash":
								columns.Add("DatItem.SHA384");
								break;
							case "sha512":
							case "sha-512":
							case "sha512 hash":
							case "sha-512 hash":
								columns.Add("DatItem.SHA512");
								break;
							case "nodump":
							case "no dump":
							case "status":
							case "item status":
								columns.Add("DatItem.Nodump");
								break;
							default:
								columns.Add("INVALID");
								break;
						}
					}

					continue;
				}

				// Otherwise, we want to split the line and parse
				string[] parsedLine = line.Split(delim);
				
				// If the line doesn't have the correct number of columns, we log and skip
				if (parsedLine.Length != columns.Count)
				{
					Globals.Logger.Warning("Malformed line found in '{0}' at line {1}", filename, linenum);
					continue;
				}

				// Set the output item information
				string machineName = null, machineDesc = null, name = null, crc = null, md5 = null, sha1 = null,
					sha256 = null, sha384 = null, sha512 = null;
				long size = -1;
				ItemType itemType = ItemType.Rom;
				ItemStatus status = ItemStatus.None;

				// Now we loop through and get values for everything
				for (int i = 0; i < columns.Count; i++)
				{
					string value = parsedLine[i].Trim('"');
					switch (columns[i])
					{
						case "DatFile.FileName":
							FileName = (String.IsNullOrEmpty(FileName) ? value : FileName);
							break;
						case "DatFile.Name":
							Name = (String.IsNullOrEmpty(Name) ? value : Name);
							break;
						case "DatFile.Description":
							Description = (String.IsNullOrEmpty(Description) ? value : Description);
							break;
						case "Machine.Name":
							machineName = value;
							break;
						case "Machine.Description":
							machineDesc = value;
							break;
						case "DatItem.Type":
							switch (value.ToLowerInvariant())
							{
								case "archive":
									itemType = ItemType.Archive;
									break;
								case "biosset":
									itemType = ItemType.BiosSet;
									break;
								case "disk":
									itemType = ItemType.Disk;
									break;
								case "release":
									itemType = ItemType.Release;
									break;
								case "rom":
									itemType = ItemType.Rom;
									break;
								case "sample":
									itemType = ItemType.Sample;
									break;
							}
							break;
						case "Rom.Name":
						case "Disk.Name":
							name = value == "" ? name : value;
							break;
						case "DatItem.Size":
							if (!Int64.TryParse(value, out size))
							{
								size = -1;
							}
							break;
						case "DatItem.CRC":
							crc = value;
							break;
						case "DatItem.MD5":
							md5 = value;
							break;
						case "DatItem.SHA1":
							sha1 = value;
							break;
						case "DatItem.SHA256":
							sha256 = value;
							break;
						case "DatItem.SHA384":
							sha384 = value;
							break;
						case "DatItem.SHA512":
							sha512 = value;
							break;
						case "DatItem.Nodump":
							switch (value.ToLowerInvariant())
							{
								case "baddump":
									status = ItemStatus.BadDump;
									break;
								case "good":
									status = ItemStatus.Good;
									break;
								case "no":
								case "none":
									status = ItemStatus.None;
									break;
								case "nodump":
								case "yes":
									status = ItemStatus.Nodump;
									break;
								case "verified":
									status = ItemStatus.Verified;
									break;
							}
							break;
					}
				}

				// And now we populate and add the new item
				switch (itemType)
				{
					case ItemType.Archive:
						Archive archive = new Archive()
						{
							Name = name,

							Machine = new Machine()
							{
								Name = machineName,
								Description = machineDesc,
							},
						};

						ParseAddHelper(archive, clean, remUnicode);
						break;
					case ItemType.BiosSet:
						BiosSet biosset = new BiosSet()
						{
							Name = name,

							Machine = new Machine()
							{
								Name = machineName,
								Description = machineDesc,
							},
						};

						ParseAddHelper(biosset, clean, remUnicode);
						break;
					case ItemType.Disk:
						Disk disk = new Disk()
						{
							Name = name,
							MD5 = md5,
							SHA1 = sha1,
							SHA256 = sha256,
							SHA384 = sha384,
							SHA512 = sha512,

							Machine = new Machine()
							{
								Name = machineName,
								Description = machineDesc,
							},

							ItemStatus = status,
						};

						ParseAddHelper(disk, clean, remUnicode);
						break;
					case ItemType.Release:
						Release release = new Release()
						{
							Name = name,

							Machine = new Machine()
							{
								Name = machineName,
								Description = machineDesc,
							},
						};

						ParseAddHelper(release, clean, remUnicode);
						break;
					case ItemType.Rom:
						Rom rom = new Rom()
						{
							Name = name,
							Size = size,
							CRC = crc,
							MD5 = md5,
							SHA1 = sha1,
							SHA256 = sha256,
							SHA384 = sha384,
							SHA512 = sha512,

							Machine = new Machine()
							{
								Name = machineName,
								Description = machineDesc,
							},

							ItemStatus = status,
						};

						ParseAddHelper(rom, clean, remUnicode);
						break;
					case ItemType.Sample:
						Sample sample = new Sample()
						{
							Name = name,

							Machine = new Machine()
							{
								Name = machineName,
								Description = machineDesc,
							},
						};

						ParseAddHelper(sample, clean, remUnicode);
						break;
				}
			}			
		}

		/// <summary>
		/// Parse an XML DAT (Logiqx, OfflineList, SabreDAT, and Software List) and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <remrks>
		/// TODO: Software Lists - sharedfeat tag (read-in, write-out)
		/// </remrks>
		private void ParseGenericXML(
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
			XmlReader subreader, headreader, flagreader;
			bool superdat = false, empty = true;
			string key = "", date = "";
			long size = -1;
			ItemStatus its = ItemStatus.None;
			List<string> parent = new List<string>();

			Encoding enc = Style.GetEncoding(filename);
			XmlReader xtr = FileTools.GetXmlTextReader(filename);

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
							Rom rom = new Rom("null", tempgame, omitFromScan: Hash.DeepHashes); // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually

							// Now process and add the rom
							key = ParseAddHelper(rom, clean, remUnicode);
						}

						// Regardless, end the current folder
						int parentcount = parent.Count;
						if (parentcount == 0)
						{
							Globals.Logger.Verbose("Empty parent '{0}' found in '{1}'", String.Join("\\", parent), filename);
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
									case "none":
										ForceMerging = ForceMerging.None;
										break;
									case "split":
										ForceMerging = ForceMerging.Split;
										break;
									case "merged":
										ForceMerging = ForceMerging.Merged;
										break;
									case "nonmerged":
										ForceMerging = ForceMerging.NonMerged;
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
												case "none":
													ForceMerging = ForceMerging.None;
													break;
												case "split":
													ForceMerging = ForceMerging.Split;
													break;
												case "merged":
													ForceMerging = ForceMerging.Merged;
													break;
												case "nonmerged":
													ForceMerging = ForceMerging.NonMerged;
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

							// Get the runnable value from the reader
							if (subreader.GetAttribute("runnable") != null)
							{
								switch (subreader.GetAttribute("runnable"))
								{
									case "no":
										machine.Runnable = false;
										break;
									case "yes":
										machine.Runnable = true;
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

										ext = (subreader.GetAttribute("extension") ?? "");

										DatItem olrom = new Rom
										{
											Name = releaseNumber + " - " + machine.Name + ext,
											Size = size,
											CRC = subreader.ReadElementContentAsString(),
											ItemStatus = ItemStatus.None,

											Machine = (Machine)machine.Clone(),
										};

										// Now process and add the rom
										key = ParseAddHelper(olrom, clean, remUnicode);
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
									case "slotoption":
										string slotoption = subreader.GetAttribute("devname");
										if (!machine.Devices.Contains(slotoption))
										{
											machine.Devices.Add(slotoption);
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

											Machine = (Machine)machine.Clone(),

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
										key = ParseAddHelper(relrom, clean, remUnicode);

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

											Machine = (Machine)machine.Clone(),

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
										key = ParseAddHelper(biosrom, clean, remUnicode);

										subreader.Read();
										break;
									case "archive":
										empty = false;

										DatItem archiverom = new Archive
										{
											Name = subreader.GetAttribute("name"),

											Machine = (Machine)machine.Clone(),

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
										key = ParseAddHelper(archiverom, clean, remUnicode);

										subreader.Read();
										break;
									case "sample":
										empty = false;

										DatItem samplerom = new Sample
										{
											Name = subreader.GetAttribute("name"),

											Machine = (Machine)machine.Clone(),

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
										key = ParseAddHelper(samplerom, clean, remUnicode);

										subreader.Read();
										break;
									case "rom":
									case "disk":
										empty = false;

										// If the rom has a merge tag, add it
										string merge = subreader.GetAttribute("merge");

										// If the rom has a status, flag it
										its = ItemStatus.None;
										if (subreader.GetAttribute("flags") == "good" || subreader.GetAttribute("status") == "good")
										{
											its = ItemStatus.Good;
										}
										if (subreader.GetAttribute("flags") == "baddump" || subreader.GetAttribute("status") == "baddump")
										{
											its = ItemStatus.BadDump;
										}
										if (subreader.GetAttribute("flags") == "nodump" || subreader.GetAttribute("status") == "nodump")
										{
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
													SHA256 = subreader.GetAttribute("sha256")?.ToLowerInvariant(),
													SHA384 = subreader.GetAttribute("sha384")?.ToLowerInvariant(),
													SHA512 = subreader.GetAttribute("sha512")?.ToLowerInvariant(),
													MergeTag = merge,
													ItemStatus = its,

													Machine = (Machine)machine.Clone(),

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
													SHA256 = subreader.GetAttribute("sha256")?.ToLowerInvariant(),
													SHA384 = subreader.GetAttribute("sha384")?.ToLowerInvariant(),
													SHA512 = subreader.GetAttribute("sha512")?.ToLowerInvariant(),
													ItemStatus = its,
													MergeTag = merge,
													Date = date,

													Machine = (Machine)machine.Clone(),

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
										key = ParseAddHelper(inrom, clean, remUnicode);

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

							string foldername = (xtr.GetAttribute("name") ?? "");
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
													its = ItemStatus.BadDump;
													break;
												case "nodump":
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
										SHA256 = xtr.GetAttribute("sha256")?.ToLowerInvariant(),
										SHA384 = xtr.GetAttribute("sha384")?.ToLowerInvariant(),
										SHA512 = xtr.GetAttribute("sha512")?.ToLowerInvariant(),
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
										SHA256 = xtr.GetAttribute("sha256")?.ToLowerInvariant(),
										SHA384 = xtr.GetAttribute("sha384")?.ToLowerInvariant(),
										SHA512 = xtr.GetAttribute("sha512")?.ToLowerInvariant(),
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
							key = ParseAddHelper(rom, clean, remUnicode);

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
				Globals.Logger.Warning("Exception found while parsing '{0}': {1}", filename, ex);

				// For XML errors, just skip the affected node
				xtr?.Read();
			}

			xtr.Dispose();
		}

		/// <summary>
		/// Parse a MAME Listroms DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <remarks>
		/// In a new style MAME listroms DAT, each game has the following format:
		/// 
		/// ROMs required for driver "005".
		/// Name                                   Size Checksum
		/// 1346b.cpu-u25                          2048 CRC(8e68533e) SHA1(a257c556d31691068ed5c991f1fb2b51da4826db)
		/// 6331.sound-u8                            32 BAD CRC(1d298cb0) SHA1(bb0bb62365402543e3154b9a77be9c75010e6abc) BAD_DUMP
		/// 16v8h-blue.u24                          279 NO GOOD DUMP KNOWN
		/// </remarks>
		private void ParseListroms(
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
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(FileTools.TryOpenRead(filename), enc);

			string gamename = "";
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine().Trim();

				// If we have a blank line, we just skip it
				if (String.IsNullOrEmpty(line))
				{
					continue;
				}

				// If we have the descriptor line, ignore it
				else if (line == "Name                                   Size Checksum")
				{
					continue;
				}

				// If we have the beginning of a game, set the name of the game
				else if (line.StartsWith("ROMs required for"))
				{
					gamename = Regex.Match(line, @"^ROMs required for \S*? ""(.*?)""\.").Groups[1].Value;
				}

				// If we have a machine with no required roms (usually internal devices), skip it
				else if (line.StartsWith("No ROMs required for"))
				{
					continue;
				}

				// Otherwise, we assume we have a rom that we need to add
				else
				{
					// First, we preprocess the line so that the rom name is consistently correct
					string romname = "";
					string[] split = line.Split(new string[] { "    " }, StringSplitOptions.RemoveEmptyEntries);

					// If the line doesn't have the 4 spaces of padding, check for 3
					if (split.Length == 1)
					{
						split = line.Split(new string[] { "   " }, StringSplitOptions.RemoveEmptyEntries);
					}

					// If the split is still unsuccessful, log it and skip
					if (split.Length == 1)
					{
						Globals.Logger.Warning("Possibly malformed line: '{0}'", line);
					}

					romname = split[0];
					line = line.Substring(romname.Length);

					// Next we separate the ROM into pieces
					split = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

					// Standard Disks have 2 pieces (name, sha1)
					if (split.Length == 1)
					{
						Disk disk = new Disk()
						{
							Name = romname,
							SHA1 = Style.CleanListromHashData(split[0]),

							Machine = new Machine()
							{
								Name = gamename,
							},
						};

						ParseAddHelper(disk, clean, remUnicode);
					}

					// Baddump Disks have 4 pieces (name, BAD, sha1, BAD_DUMP)
					else if (split.Length == 3 && line.EndsWith("BAD_DUMP"))
					{
						Disk disk = new Disk()
						{
							Name = romname,
							SHA1 = Style.CleanListromHashData(split[1]),
							ItemStatus = ItemStatus.BadDump,

							Machine = new Machine()
							{
								Name = gamename,
							},
						};

						ParseAddHelper(disk, clean, remUnicode);
					}

					// Standard ROMs have 4 pieces (name, size, crc, sha1)
					else if (split.Length == 3)
					{
						if (!Int64.TryParse(split[0], out long size))
						{
							size = 0;
						}

						Rom rom = new Rom()
						{
							Name = romname,
							Size = size,
							CRC = Style.CleanListromHashData(split[1]),
							SHA1 = Style.CleanListromHashData(split[2]),

							Machine = new Machine()
							{
								Name = gamename,
							},
						};

						ParseAddHelper(rom, clean, remUnicode);
					}

					// Nodump Disks have 5 pieces (name, NO, GOOD, DUMP, KNOWN)
					else if (split.Length == 4 && line.EndsWith("NO GOOD DUMP KNOWN"))
					{
						Disk disk = new Disk()
						{
							Name = romname,
							ItemStatus = ItemStatus.Nodump,

							Machine = new Machine()
							{
								Name = gamename,
							},
						};

						ParseAddHelper(disk, clean, remUnicode);
					}

					// Baddump ROMs have 6 pieces (name, size, BAD, crc, sha1, BAD_DUMP)
					else if (split.Length == 5 && line.EndsWith("BAD_DUMP"))
					{
						if (!Int64.TryParse(split[0], out long size))
						{
							size = 0;
						}

						Rom rom = new Rom()
						{
							Name = romname,
							Size = size,
							CRC = Style.CleanListromHashData(split[2]),
							SHA1 = Style.CleanListromHashData(split[3]),
							ItemStatus = ItemStatus.BadDump,

							Machine = new Machine()
							{
								Name = gamename,
							},
						};

						ParseAddHelper(rom, clean, remUnicode);
					}

					// Nodump ROMs have 6 pieces (name, size, NO, GOOD, DUMP, KNOWN)
					else if (split.Length == 5 && line.EndsWith("NO GOOD DUMP KNOWN"))
					{
						if (!Int64.TryParse(split[0], out long size))
						{
							size = 0;
						}

						Rom rom = new Rom()
						{
							Name = romname,
							Size = size,
							ItemStatus = ItemStatus.Nodump,

							Machine = new Machine()
							{
								Name = gamename,
							},
						};

						ParseAddHelper(rom, clean, remUnicode);
					}

					// If we have something else, it's invalid
					else
					{
						Globals.Logger.Warning("Invalid line detected: '{0} {1}'", romname, line);
					}
				}
			}
		}

		/// <summary>
		/// Parse a hashfile or SFV and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="hashtype">Hash type that should be assumed</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		private void ParseHashfile(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Specific to hash files
			Hash hashtype,

			// Miscellaneous
			bool clean,
			bool remUnicode)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(FileTools.TryOpenRead(filename), enc);

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				// Split the line and get the name and hash
				string[] split = line.Split(' ');
				string name = "";
				string hash = "";

				// If we have CRC, then it's an SFV file and the name is first are
				if ((hashtype & Hash.CRC) != 0)
				{
					name = split[0].Replace("*", String.Empty);
					hash = split[1];
				}
				// Otherwise, the name is second
				else
				{
					name = split[1].Replace("*", String.Empty);
					hash = split[0];
				}

				Rom rom = new Rom
				{
					Name = name,
					Size = -1,
					CRC = ((hashtype & Hash.CRC) != 0 ? hash : null),
					MD5 = ((hashtype & Hash.MD5) != 0 ? hash : null),
					SHA1 = ((hashtype & Hash.SHA1) != 0 ? hash : null),
					SHA256 = ((hashtype & Hash.SHA256) != 0 ? hash : null),
					SHA384 = ((hashtype & Hash.SHA384) != 0 ? hash : null),
					SHA512 = ((hashtype & Hash.SHA512) != 0 ? hash : null),
					ItemStatus = ItemStatus.None,

					Machine = new Machine
					{
						Name = Path.GetFileNameWithoutExtension(filename),
					},

					SystemID = sysid,
					SourceID = srcid,
				};

				// Now process and add the rom
				ParseAddHelper(rom, clean, remUnicode);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Parse a RomCenter DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		private void ParseRC(
			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Miscellaneous
			bool clean,
			bool remUnicode)
		{
			// Open a file reader
			Encoding enc = Style.GetEncoding(filename);
			StreamReader sr = new StreamReader(FileTools.TryOpenRead(filename), enc);

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
						if (Int32.TryParse(line.Split('=')[1], out int split))
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
						if (Int32.TryParse(line.Split('=')[1], out int merge))
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
						if (!Int64.TryParse(rominfo[7], out long size))
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
						ParseAddHelper(rom, clean, remUnicode);
					}
				}
			}

			sr.Dispose();
		}

		/// <summary>
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="item">Item data to check against</param>
		/// <param name="clean">True if the names should be cleaned to WoD standards, false otherwise</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <returns>The key for the item</returns>
		private string ParseAddHelper(DatItem item, bool clean, bool remUnicode)
		{
			string key = "";

			// If there's no name in the rom, we log and skip it
			if (item.Name == null)
			{
				Globals.Logger.Warning("{0}: Rom with no name found! Skipping...", FileName);
				return key;
			}

			// If the name ends with a directory separator, we log and skip it (DOSCenter only?)
			if (item.Name.EndsWith("/") || item.Name.EndsWith("\\"))
			{
				Globals.Logger.Warning("{0}: Rom ending with directory separator found: '{1}'. Skipping...", FileName, item.Name);
				return key;
			}

			// If we're in cleaning mode, sanitize the game name
			item.Machine.UpdateName((clean ? Style.CleanGameName(item.Machine.Name) : item.Machine.Name));

			// If we're stripping unicode characters, do so from all relevant things
			if (remUnicode)
			{
				item.Name = Style.RemoveUnicodeCharacters(item.Name);
				item.Machine.UpdateName(Style.RemoveUnicodeCharacters(item.Machine.Name));
				item.Machine.UpdateDescription(Style.RemoveUnicodeCharacters(item.Machine.Description));
			}

			// If we have a Rom or a Disk, clean the hash data
			if (item.Type == ItemType.Rom)
			{
				Rom itemRom = (Rom)item;

				// Sanitize the hashes from null, hex sizes, and "true blank" strings
				itemRom.CRC = Style.CleanHashData(itemRom.CRC, Constants.CRCLength);
				itemRom.MD5 = Style.CleanHashData(itemRom.MD5, Constants.MD5Length);
				itemRom.SHA1 = Style.CleanHashData(itemRom.SHA1, Constants.SHA1Length);
				itemRom.SHA256 = Style.CleanHashData(itemRom.SHA256, Constants.SHA256Length);
				itemRom.SHA384 = Style.CleanHashData(itemRom.SHA384, Constants.SHA384Length);
				itemRom.SHA512 = Style.CleanHashData(itemRom.SHA512, Constants.SHA512Length);

				// If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
				if ((itemRom.Size == 0 || itemRom.Size == -1)
					&& ((itemRom.CRC == Constants.CRCZero || String.IsNullOrEmpty(itemRom.CRC))
						|| itemRom.MD5 == Constants.MD5Zero
						|| itemRom.SHA1 == Constants.SHA1Zero
						|| itemRom.SHA256 == Constants.SHA256Zero
						|| itemRom.SHA384 == Constants.SHA384Zero
						|| itemRom.SHA512 == Constants.SHA512Zero))
				{
					// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
					itemRom.Size = Constants.SizeZero;
					itemRom.CRC = Constants.CRCZero;
					itemRom.MD5 = Constants.MD5Zero;
					itemRom.SHA1 = Constants.SHA1Zero;
					itemRom.SHA256 = null;
					itemRom.SHA384 = null;
					itemRom.SHA512 = null;
					//itemRom.SHA256 = Constants.SHA256Zero;
					//itemRom.SHA384 = Constants.SHA384Zero;
					//itemRom.SHA512 = Constants.SHA512Zero;
				}
				// If the file has no size and it's not the above case, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump && (itemRom.Size == 0 || itemRom.Size == -1))
				{
					Globals.Logger.Verbose("{0}: Incomplete entry for '{1}' will be output as nodump", FileName, itemRom.Name);
					itemRom.ItemStatus = ItemStatus.Nodump;
				}
				// If the file has a size but aboslutely no hashes, skip and log
				else if (itemRom.ItemStatus != ItemStatus.Nodump
					&& itemRom.Size > 0
					&& String.IsNullOrEmpty(itemRom.CRC)
					&& String.IsNullOrEmpty(itemRom.MD5)
					&& String.IsNullOrEmpty(itemRom.SHA1)
					&& String.IsNullOrEmpty(itemRom.SHA256)
					&& String.IsNullOrEmpty(itemRom.SHA384)
					&& String.IsNullOrEmpty(itemRom.SHA512))
				{
					Globals.Logger.Verbose("{0}: Incomplete entry for '{1}' will be output as nodump", FileName, itemRom.Name);
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
				itemDisk.SHA256 = Style.CleanHashData(itemDisk.SHA256, Constants.SHA256Length);
				itemDisk.SHA384 = Style.CleanHashData(itemDisk.SHA384, Constants.SHA384Length);
				itemDisk.SHA512 = Style.CleanHashData(itemDisk.SHA512, Constants.SHA512Length);

				// If the file has aboslutely no hashes, skip and log
				if (itemDisk.ItemStatus != ItemStatus.Nodump
					&& String.IsNullOrEmpty(itemDisk.MD5)
					&& String.IsNullOrEmpty(itemDisk.SHA1)
					&& String.IsNullOrEmpty(itemDisk.SHA256)
					&& String.IsNullOrEmpty(itemDisk.SHA384)
					&& String.IsNullOrEmpty(itemDisk.SHA512))
				{
					Globals.Logger.Verbose("Incomplete entry for '{0}' will be output as nodump", itemDisk.Name);
					itemDisk.ItemStatus = ItemStatus.Nodump;
				}

				item = itemDisk;
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
					break;
				case ItemType.Rom:
					key = ((Rom)item).Size + "-" + ((Rom)item).CRC;
					break;
				default:
					key = "default";
					break;
			}

			// Add the item to the DAT
			Add(key, item);

			return key;
		}

		/// <summary>
		/// Add a rom to the Dat after checking
		/// </summary>
		/// <param name="item">Item data to check against</param>
		/// <param name="clean">True if the names should be cleaned to WoD standards, false otherwise</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <returns>The key for the item</returns>
		private async Task<string> ParseAddHelperAsync(DatItem item, bool clean, bool remUnicode)
		{
			return await Task.Run(() => ParseAddHelper(item, clean, remUnicode));
		}

		#endregion
	}
}
