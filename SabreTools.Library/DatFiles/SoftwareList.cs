using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

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
	/// Represents parsing and writing of an SabreDat XML DAT
	/// </summary>
	internal class SoftwareList : DatFile
	{
		/// <summary>
		/// Constructor designed for casting a base DatFile
		/// </summary>
		/// <param name="datFile">Parent DatFile to copy from</param>
		public SoftwareList(DatFile datFile)
		{
			this._datHeader = datFile._datHeader;
			this._items = datFile._items;
			this._sortedBy = datFile._sortedBy;
			this._datStats = datFile._datStats;

		}

		/// <summary>
		/// Parse an SabreDat XML DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <remarks>
		/// </remarks>
		public void ParseFile(
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
			new Logiqx(this).ParseFile(filename, sysid, srcid, keep, clean, remUnicode);
		}

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="outfile">Name of the file to write to</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		public bool WriteToFile(string outfile, bool ignoreblanks = false)
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
							"<!DOCTYPE softwarelist SYSTEM \"softwarelist.dtd\">\n\n" +
							"<softwarelist name=\"" + HttpUtility.HtmlEncode(Name) + "\"" +
								" description=\"" + HttpUtility.HtmlEncode(Description) + "\"" +
								(ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : "") +
								(ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : "") +
								(ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : "") +
								(ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : "") +
								(ForceMerging == ForceMerging.Merged ? " forcemerging=\"merged\"" : "") +
								(ForceMerging == ForceMerging.NonMerged ? " forcemerging=\"nonmerged\"" : "") +
								(ForceNodump == ForceNodump.Ignore ? " forcenodump=\"ignore\"" : "") +
								(ForceNodump == ForceNodump.Obsolete ? " forcenodump=\"obsolete\"" : "") +
								(ForceNodump == ForceNodump.Required ? " forcenodump=\"required\"" : "") +
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

				string state = "\t<software name=\"" + HttpUtility.HtmlEncode(rom.MachineName) + "\""
							+ (rom.Supported != null ? " supported=\"" + (rom.Supported == true ? "yes" : "no") + "\"" : "") +
							(ExcludeOf ? "" :
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
				string state = "\t</software>\n\n";

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
				state += "\t\t<part name=\"" + rom.PartName + "\" interface=\"" + rom.PartInterface + "\">\n";

				foreach (Tuple<string, string> kvp in rom.Features)
				{
					state += "\t\t\t<feature name=\"" + HttpUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + HttpUtility.HtmlEncode(kvp.Item2) + "\"/>\n";
				}

				switch (rom.Type)
				{
					case ItemType.Archive:
						state += "\t\t\t<dataarea name=\"" + (String.IsNullOrWhiteSpace(rom.AreaName) ? "archive" : rom.AreaName) + "\""
								+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
							+ "\t\t\t\t<archive name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ "/>\n"
							+ "\t\t\t</dataarea>\n";
						break;
					case ItemType.BiosSet:
						state += "\t\t\t<dataarea name=\"" + (String.IsNullOrWhiteSpace(rom.AreaName) ? "biosset" : rom.AreaName) + "\""
								+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
							+ "\t\t\t\t<biosset name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
							+ (((BiosSet)rom).Default != null
								? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
								: "")
							+ "/>\n"
							+ "\t\t\t</dataarea>\n";
						break;
					case ItemType.Disk:
						state += "\t\t\t<diskarea name=\"" + (String.IsNullOrWhiteSpace(rom.AreaName) ? "cdrom" : rom.AreaName) + "\""
								+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
							+ "\t\t\t\t<disk name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA256) ? " sha256=\"" + ((Disk)rom).SHA256.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA384) ? " sha384=\"" + ((Disk)rom).SHA384.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA512) ? " sha512=\"" + ((Disk)rom).SHA512.ToLowerInvariant() + "\"" : "")
							+ (((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
							+ "/>\n"
							+ "\t\t\t</diskarea>\n";
						break;
					case ItemType.Release:
						state += "\t\t\t<dataarea name=\"" + (String.IsNullOrWhiteSpace(rom.AreaName) ? "release" : rom.AreaName) + "\""
								+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
							+ "\t\t\t\t<release name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
							+ (((Release)rom).Default != null
								? ((Release)rom).Default.ToString().ToLowerInvariant()
								: "")
							+ "/>\n"
							+ "\t\t\t</dataarea>\n";
						break;
					case ItemType.Rom:
						state += "\t\t\t<dataarea name=\"" + (String.IsNullOrWhiteSpace(rom.AreaName) ? "rom" : rom.AreaName) + "\""
								+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
							+ "\t\t\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA256) ? " sha256=\"" + ((Rom)rom).SHA256.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA384) ? " sha384=\"" + ((Rom)rom).SHA384.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA512) ? " sha512=\"" + ((Rom)rom).SHA512.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
							+ (((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
							+ "/>\n"
							+ "\t\t\t</dataarea>\n";
						break;
					case ItemType.Sample:
						state += "\t\t\t<dataarea name=\"" + (String.IsNullOrWhiteSpace(rom.AreaName) ? "sample" : rom.AreaName) + "\""
								+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
							+ "\t\t\t\t<sample type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ "/>\n"
							+ "\t\t\t</dataarea>\n";
						break;
				}

				state += "\t\t</part>\n";

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
				string footer = "\t</software>\n\n</softwarelist>\n";

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
