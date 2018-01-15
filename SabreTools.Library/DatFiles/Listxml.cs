using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using StreamWriter = System.IO.StreamWriter;
#endif
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents parsing and writing of a MAME XML DAT
	/// </summary>
	/// TODO: Verify that all read/write for this DatFile type is correct
	internal class Listxml : DatFile
	{
		/// <summary>
		/// Constructor designed for casting a base DatFile
		/// </summary>
		/// <param name="datFile">Parent DatFile to copy from</param>
		public Listxml(DatFile datFile)
			: base(datFile, cloneHeader: false)
		{
		}

		/// <summary>
		/// Parse a MAME XML DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <remarks>
		/// </remarks>
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
			// All XML-derived DATs share a lot in common so it just calls one implementation
			// TODO: Use the following implementation instead of passing to Logiqx
			new Logiqx(this, false).ParseFile(filename, sysid, srcid, keep, clean, remUnicode);
			return;

			// Prepare all internal variables
			Encoding enc = Utilities.GetEncoding(filename);
			XmlReader xtr = Utilities.GetXmlTextReader(filename);

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
					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						case "mame":
							Name = (String.IsNullOrWhiteSpace(Name) ? xtr.GetAttribute("build") : Name);
							Description = (String.IsNullOrWhiteSpace(Description) ? Name : Name);
							// string debug = xtr.GetAttribute("debug"); // (yes|no) "no"
							// string mameconfig = xtr.GetAttribute("mameconfig"); CDATA
							xtr.Read();
							break;
						// Handle M1 DATs since they're 99% the same as a SL DAT
						case "m1":
							Name = (String.IsNullOrWhiteSpace(Name) ? "M1" : Name);
							Description = (String.IsNullOrWhiteSpace(Description) ? "M1" : Description);
							Version = (String.IsNullOrWhiteSpace(Version) ? xtr.GetAttribute("version") ?? "" : Version);
							xtr.Read();
							break;
						// We want to process the entire subtree of the machine
						case "machine":
							ReadMachine(xtr.ReadSubtree(), filename, sysid, srcid, keep, clean, remUnicode);

							// Skip the machine now that we've processed it
							xtr.Skip();
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
		/// Read machine information
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		private void ReadMachine(
			XmlReader reader,

			// Standard Dat parsing
			string filename,
			int sysid,
			int srcid,

			// Miscellaneous
			bool keep,
			bool clean,
			bool remUnicode)
		{
			// If we have an empty machine, skip it
			if (reader == null)
			{
				return;
			}

			// Otherwise, add what is possible
			reader.MoveToContent();

			string key = "";
			string temptype = reader.Name;
			bool containsItems = false;

			// Create a new machine
			MachineType machineType = MachineType.NULL;
			if (Utilities.GetYesNo(reader.GetAttribute("isbios")) == true)
			{
				machineType |= MachineType.Bios;
			}
			if (Utilities.GetYesNo(reader.GetAttribute("isdevice")) == true)
			{
				machineType |= MachineType.Device;
			}
			if (Utilities.GetYesNo(reader.GetAttribute("ismechanical")) == true)
			{
				machineType |= MachineType.Mechanical;
			}

			Machine machine = new Machine
			{
				Name = reader.GetAttribute("name"),
				Description = reader.GetAttribute("name"),
				SourceFile = reader.GetAttribute("sourcefile"),
				Runnable = Utilities.GetYesNo(reader.GetAttribute("runnable")),

				Comment = "",

				CloneOf = reader.GetAttribute("cloneof") ?? "",
				RomOf = reader.GetAttribute("romof") ?? "",
				SampleOf = reader.GetAttribute("sampleof") ?? "",

				MachineType = (machineType == MachineType.NULL ? MachineType.None : machineType),
			};

			while (!reader.EOF)
			{
				// We only want elements
				if (reader.NodeType != XmlNodeType.Element)
				{
					reader.Read();
					continue;
				}

				// Get the roms from the machine
				switch (reader.Name)
				{
					case "description":
						machine.Description = reader.ReadElementContentAsString();
						break;
					case "year":
						machine.Year = reader.ReadElementContentAsString();
						break;
					case "manufacturer":
						machine.Manufacturer = reader.ReadElementContentAsString();
						break;
					case "biosset":
						containsItems = true;

						DatItem biosset = new BiosSet
						{
							Name = reader.GetAttribute("name"),
							Description = reader.GetAttribute("description"),
							Default = Utilities.GetYesNo(reader.GetAttribute("default")),

							SystemID = sysid,
							System = filename,
							SourceID = srcid,
						};

						biosset.CopyMachineInformation(machine);

						// Now process and add the rom
						key = ParseAddHelper(biosset, clean, remUnicode);

						reader.Read();
						break;
					case "rom":
						containsItems = true;

						DatItem rom = new Rom
						{
							Name = reader.GetAttribute("name"),
							Bios = reader.GetAttribute("bios"),
							Size = Utilities.GetSize(reader.GetAttribute("size")),
							CRC = reader.GetAttribute("crc")?.ToLowerInvariant(),
							MD5 = reader.GetAttribute("md5")?.ToLowerInvariant(),
							SHA1 = reader.GetAttribute("sha1")?.ToLowerInvariant(),
							SHA256 = reader.GetAttribute("sha256")?.ToLowerInvariant(),
							SHA384 = reader.GetAttribute("sha384")?.ToLowerInvariant(),
							SHA512 = reader.GetAttribute("sha512")?.ToLowerInvariant(),
							MergeTag = reader.GetAttribute("merge"),
							Region = reader.GetAttribute("region"),
							Offset = reader.GetAttribute("offset"),
							ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
							Optional = Utilities.GetYesNo(reader.GetAttribute("optional")),

							SystemID = sysid,
							System = filename,
							SourceID = srcid,
						};

						rom.CopyMachineInformation(machine);

						// Now process and add the rom
						key = ParseAddHelper(rom, clean, remUnicode);

						reader.Read();
						break;
					case "disk":
						containsItems = true;

						DatItem disk = new Disk
						{
							Name = reader.GetAttribute("name"),
							MD5 = reader.GetAttribute("md5")?.ToLowerInvariant(),
							SHA1 = reader.GetAttribute("sha1")?.ToLowerInvariant(),
							SHA256 = reader.GetAttribute("sha256")?.ToLowerInvariant(),
							SHA384 = reader.GetAttribute("sha384")?.ToLowerInvariant(),
							SHA512 = reader.GetAttribute("sha512")?.ToLowerInvariant(),
							MergeTag = reader.GetAttribute("merge"),
							Region = reader.GetAttribute("region"),
							Index = reader.GetAttribute("index"),
							Writable = Utilities.GetYesNo(reader.GetAttribute("writable")),
							ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
							Optional = Utilities.GetYesNo(reader.GetAttribute("optional")),

							SystemID = sysid,
							System = filename,
							SourceID = srcid,
						};

						disk.CopyMachineInformation(machine);

						// Now process and add the rom
						key = ParseAddHelper(disk, clean, remUnicode);

						reader.Read();
						break;
					case "device_ref":
						machine.Devices.Add(reader.ReadElementContentAsString());

						reader.Read();
						break;
					case "sample":
						containsItems = true;

						DatItem samplerom = new Sample
						{
							Name = reader.GetAttribute("name"),

							SystemID = sysid,
							System = filename,
							SourceID = srcid,
						};

						samplerom.CopyMachineInformation(machine);

						// Now process and add the rom
						key = ParseAddHelper(samplerom, clean, remUnicode);

						reader.Read();
						break;
					case "chip":
						// string name = reader.GetAttribute("name");
						// string tag = reader.GetAttribute("tag");
						// string type = reader.GetAttribute("type"); // (cpu|audio)
						// string clock = reader.GetAttribute("clock");

						reader.Read();
						break;
					case "display":
						// string tag = reader.GetAttribute("tag");
						// string type = reader.GetAttribute("type"); // (raster|vector|lcd|svg|unknown)
						// string rotate = reader.GetAttribute("rotate"); // (0|90|180|270)
						// bool? flipx = Utilities.GetYesNo(reader.GetAttribute("flipx"));
						// string width = reader.GetAttribute("width");
						// string height = reader.GetAttribute("height");
						// string refresh = reader.GetAttribute("refresh");
						// string pixclock = reader.GetAttribute("pixclock");
						// string htotal = reader.GetAttribute("htotal");
						// string hbend = reader.GetAttribute("hbend");
						// string hbstart = reader.GetAttribute("hbstart");
						// string vtotal = reader.GetAttribute("vtotal");
						// string vbend = reader.GetAttribute("vbend");
						// string vbstart = reader.GetAttribute("vbstart");

						reader.Read();
						break;
					case "sound":
						// string channels = reader.GetAttribute("channels");

						reader.Read();
						break;
					case "condition":
						// string tag = reader.GetAttribute("tag");
						// string mask = reader.GetAttribute("mask");
						// string relation = reader.GetAttribute("relation"); // (eq|ne|gt|le|lt|ge)
						// string value = reader.GetAttribute("value");

						reader.Read();
						break;
					case "input":
						// bool? service = Utilities.GetYesNo(reader.GetAttribute("service"));
						// bool? tilt = Utilities.GetYesNo(reader.GetAttribute("tilt"));
						// string players = reader.GetAttribute("players");
						// string coins = reader.GetAttribute("coins");

						// // While the subtree contains <control> elements...
						// string type = reader.GetAttribute("type");
						// string player = reader.GetAttribute("player");
						// string buttons = reader.GetAttribute("buttons");
						// string regbuttons = reader.GetAttribute("regbuttons");
						// string minimum = reader.GetAttribute("minimum");
						// string maximum = reader.GetAttribute("maximum");
						// string sensitivity = reader.GetAttribute("sensitivity");
						// string keydelta = reader.GetAttribute("keydelta");
						// bool? reverse = Utilities.GetYesNo(reader.GetAttribute("reverse"));
						// string ways = reader.GetAttribute("ways");
						// string ways2 = reader.GetAttribute("ways2");
						// string ways3 = reader.GetAttribute("ways3");

						reader.Read();
						break;
					case "dipswitch":
						// string name = reader.GetAttribute("name");
						// string tag = reader.GetAttribute("tag");
						// string mask = reader.GetAttribute("mask");

						// // While the subtree contains <diplocation> elements...
						// string name = reader.GetAttribute("name");
						// string number = reader.GetAttribute("number");
						// bool? inverted = Utilities.GetYesNo(reader.GetAttribute("inverted"));

						// // While the subtree contains <dipvalue> elements...
						// string name = reader.GetAttribute("name");
						// string value = reader.GetAttribute("value");
						// bool? default = Utilities.GetYesNo(reader.GetAttribute("default"));

						reader.Read();
						break;
					case "configuration":
						// string name = reader.GetAttribute("name");
						// string tag = reader.GetAttribute("tag");
						// string mask = reader.GetAttribute("mask");

						// // While the subtree contains <conflocation> elements...
						// string name = reader.GetAttribute("name");
						// string number = reader.GetAttribute("number");
						// bool? inverted = Utilities.GetYesNo(reader.GetAttribute("inverted"));

						// // While the subtree contains <confsetting> elements...
						// string name = reader.GetAttribute("name");
						// string value = reader.GetAttribute("value");
						// bool? default = Utilities.GetYesNo(reader.GetAttribute("default"));

						reader.Read();
						break;
					case "port":
						// string tag = reader.GetAttribute("tag");

						// // While the subtree contains <analog> elements...
						// string mask = reader.GetAttribute("mask");

						reader.Read();
						break;
					case "adjuster":
						// string name = reader.GetAttribute("name");
						// bool? default = Utilities.GetYesNo(reader.GetAttribute("default"));

						// // For the one possible <condition> element...
						// string tag = reader.GetAttribute("tag");
						// string mask = reader.GetAttribute("mask");
						// string relation = reader.GetAttribute("relation"); // (eq|ne|gt|le|lt|ge)
						// string value = reader.GetAttribute("value");

						reader.Read();
						break;
					case "driver":
						// string status = reader.GetAttribute("status"); // (good|imperfect|preliminary)
						// string emulation = reader.GetAttribute("emulation"); // (good|imperfect|preliminary)
						// string cocktail = reader.GetAttribute("cocktail"); // (good|imperfect|preliminary)
						// string savestate = reader.GetAttribute("savestate"); // (supported|unsupported)

						reader.Read();
						break;
					case "feature":
						// string type = reader.GetAttribute("type"); // (protection|palette|graphics|sound|controls|keyboard|mouse|microphone|camera|disk|printer|lan|wan|timing)
						// string status = reader.GetAttribute("status"); // (unemulated|imperfect)
						// string overall = reader.GetAttribute("overall"); // (unemulated|imperfect)

						reader.Read();
						break;
					case "device":
						// string type = reader.GetAttribute("type");
						// string tag = reader.GetAttribute("tag");
						// string fixed_image = reader.GetAttribute("fixed_image");
						// string mandatory = reader.GetAttribute("mandatory");
						// string interface = reader.GetAttribute("interface");

						// // For the one possible <instance> element...
						// string name = reader.GetAttribute("name");
						// string briefname = reader.GetAttribute("briefname");

						// // While the subtree contains <extension> elements...
						// string name = reader.GetAttribute("name");

						reader.Read();
						break;
					case "slot":
						// string name = reader.GetAttribute("name");

						// // While the subtree contains <slotoption> elements... (These get added as devices currently)
						// string name = reader.GetAttribute("name");
						// string devname = reader.GetAttribute("devname");
						// bool? default = Utilities.GetYesNo(reader.GetAttribute("default"));

						reader.Read();
						break;
					case "softwarelist":
						// string name = reader.GetAttribute("name");
						// string status = reader.GetAttribute("status"); // (original|compatible)
						// string filter = reader.GetAttribute("filter");

						reader.Read();
						break;
					case "ramoption":
						// string default = reader.GetAttribute("default");

						reader.Read();
						break;
					default:
						reader.Read();
						break;
				}
			}

			// If no items were found for this machine, add a Blank placeholder
			if (!containsItems)
			{
				Blank blank = new Blank()
				{
					SystemID = sysid,
					System = filename,
					SourceID = srcid,
				};

				blank.CopyMachineInformation(machine);

				// Now process and add the rom
				key = ParseAddHelper(blank, clean, remUnicode);
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
							WriteEndGame(sw);
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

							lastgame = rom.MachineName;
							continue;
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
				string header = "<?xml version=\"1.0\"?>\n" +
							"<mame build=\"" + HttpUtility.HtmlEncode(Name) + "\"" +
								// " debug=\"" + Debug + "\"" +
								// " mameconfig=\"" + MameConfig + "\"" +
								">\n\n";

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

				string state = "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.MachineName) + "\""
							+ (rom.SourceFile != null ? " sourcefile=\"" + rom.SourceFile + "\"" : "")
							+ ((rom.MachineType & MachineType.Bios) != 0 ? " isbios=\"yes\"" : "")
							+ ((rom.MachineType & MachineType.Device) != 0 ? " isdevice=\"yes\"" : "")
							+ ((rom.MachineType & MachineType.Mechanical) != 0 ? " ismechanical=\"yes\"" : "")
							+ (rom.Runnable != null ? " runnable=\"" + rom.Runnable + "\"" : "")
							+ (ExcludeOf ? "" :
									(String.IsNullOrWhiteSpace(rom.CloneOf) || (rom.MachineName.ToLowerInvariant() == rom.CloneOf.ToLowerInvariant())
										? ""
										: " cloneof=\"" + HttpUtility.HtmlEncode(rom.CloneOf) + "\"") +
									(String.IsNullOrWhiteSpace(rom.RomOf) || (rom.MachineName.ToLowerInvariant() == rom.RomOf.ToLowerInvariant())
										? ""
										: " romof=\"" + HttpUtility.HtmlEncode(rom.RomOf) + "\"") +
									(String.IsNullOrWhiteSpace(rom.SampleOf) || (rom.MachineName.ToLowerInvariant() == rom.SampleOf.ToLowerInvariant())
										? ""
										: " sampleof=\"" + HttpUtility.HtmlEncode(rom.SampleOf) + "\"")
								) + ">\n"
							+ "\t\t<description>" + HttpUtility.HtmlEncode(rom.MachineDescription) + "</description>\n"
							+ (rom.Year != null ? "\t\t<year>" + HttpUtility.HtmlEncode(rom.Year) + "</year>\n" : "")
							+ (rom.Publisher != null ? "\t\t<publisher>" + HttpUtility.HtmlEncode(rom.Publisher) + "</publisher>\n" : "");

				foreach (Tuple<string, string> kvp in rom.Infos)
				{
					state += "\t\t<info name=\"" + HttpUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + HttpUtility.HtmlEncode(kvp.Item2) + "\" />\n";
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
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteEndGame(StreamWriter sw)
		{
			try
			{
				string state = "\t</machine>\n";

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
						break;
					case ItemType.BiosSet:
						state += "\t\t<biosset name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
							+ (((BiosSet)rom).Default != null
								? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
								: "")
							+ "/>\n";
						break;
					case ItemType.Disk:
						state += "\t\t<disk name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA256) ? " sha256=\"" + ((Disk)rom).SHA256.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA384) ? " sha384=\"" + ((Disk)rom).SHA384.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA512) ? " sha512=\"" + ((Disk)rom).SHA512.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).MergeTag) ? " merge=\"" + ((Disk)rom).MergeTag + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).Region) ? " region=\"" + ((Disk)rom).Region + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).Index) ? " index=\"" + ((Disk)rom).Index + "\"" : "")
							+ (((Disk)rom).Writable != null ? " writable=\"" + (((Disk)rom).Writable == true ? "yes" : "no") + "\"" : "")
							+ (((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
							+ (((Disk)rom).Optional != null ? " optional=\"" + (((Disk)rom).Optional == true ? "yes" : "no") + "\"" : "")
							+ "/>\n";
						break;
					case ItemType.Rom:
						state += "\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA256) ? " sha256=\"" + ((Rom)rom).SHA256.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA384) ? " sha384=\"" + ((Rom)rom).SHA384.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA512) ? " sha512=\"" + ((Rom)rom).SHA512.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).Bios) ? " bios=\"" + ((Rom)rom).Bios + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).MergeTag) ? " merge=\"" + ((Rom)rom).MergeTag + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).Region) ? " region=\"" + ((Rom)rom).Region + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).Offset) ? " offset=\"" + ((Rom)rom).Offset + "\"" : "")
							+ (((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
							+ (((Rom)rom).Optional != null ? " optional=\"" + (((Rom)rom).Optional == true ? "yes" : "no") + "\"" : "")
							+ "/>\n";
						break;
					case ItemType.Sample:
						state += "\t\t<sample name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ "/>\n";
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
				string footer = "\t</machine>\n</mame>\n";

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
