using System;
using System.Collections.Generic;
using System.Linq;
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
	internal class SabreDat : DatFile
	{
		/// <summary>
		/// Constructor designed for casting a base DatFile
		/// </summary>
		/// <param name="datFile">Parent DatFile to copy from</param>
		public SabreDat(DatFile datFile)
		{
			this._datHeader = datFile._datHeader;
			this._items = datFile._items;
			this._sortedBy = datFile._sortedBy;
			this._mergedBy = datFile._mergedBy;
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
			new Logiqx(this).ParseFile(filename, sysid, srcid, keep, clean, remUnicode);
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
				int depth = 2, last = -1;
				string lastgame = null;
				List<string> splitpath = new List<string>();

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

						List<string> newsplit = rom.MachineName.Split('\\').ToList();

						// If we have a different game and we're not at the start of the list, output the end of last item
						if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
						{
							depth = WriteEndGame(sw, splitpath, newsplit, depth, out last);
						}

						// If we have a new game, output the beginning of the new item
						if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
						{
							depth = WriteStartGame(sw, rom, newsplit, lastgame, depth, last);
						}

						// If we have a "null" game (created by DATFromDir or something similar), log it to file
						if (rom.Type == ItemType.Rom
							&& ((Rom)rom).Size == -1
							&& ((Rom)rom).CRC == "null")
						{
							Globals.Logger.Verbose("Empty folder found: {0}", rom.MachineName);

							splitpath = newsplit;
							lastgame = rom.MachineName;
							continue;
						}

						// Now, output the rom data
						WriteDatItem(sw, rom, depth, ignoreblanks);

						// Set the new data to compare against
						splitpath = newsplit;
						lastgame = rom.MachineName;
					}
				}

				// Write the file footer out
				WriteFooter(sw, depth);

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
				string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE sabredat SYSTEM \"newdat.xsd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(Description) + "</description>\n" +
							(!String.IsNullOrWhiteSpace(RootDir) ? "\t\t<rootdir>" + HttpUtility.HtmlEncode(RootDir) + "</rootdir>\n" : "") +
							(!String.IsNullOrWhiteSpace(Category) ? "\t\t<category>" + HttpUtility.HtmlEncode(Category) + "</category>\n" : "") +
							"\t\t<version>" + HttpUtility.HtmlEncode(Version) + "</version>\n" +
							(!String.IsNullOrWhiteSpace(Date) ? "\t\t<date>" + HttpUtility.HtmlEncode(Date) + "</date>\n" : "") +
							"\t\t<author>" + HttpUtility.HtmlEncode(Author) + "</author>\n" +
							(!String.IsNullOrWhiteSpace(Comment) ? "\t\t<comment>" + HttpUtility.HtmlEncode(Comment) + "</comment>\n" : "") +
							(!String.IsNullOrWhiteSpace(Type) || ForcePacking != ForcePacking.None || ForceMerging != ForceMerging.None || ForceNodump != ForceNodump.None ?
								"\t\t<flags>\n" +
									(!String.IsNullOrWhiteSpace(Type) ? "\t\t\t<flag name=\"type\" value=\"" + HttpUtility.HtmlEncode(Type) + "\"/>\n" : "") +
									(ForcePacking == ForcePacking.Unzip ? "\t\t\t<flag name=\"forcepacking\" value=\"unzip\"/>\n" : "") +
									(ForcePacking == ForcePacking.Zip ? "\t\t\t<flag name=\"forcepacking\" value=\"zip\"/>\n" : "") +
									(ForceMerging == ForceMerging.Full ? "\t\t\t<flag name=\"forcemerging\" value=\"full\"/>\n" : "") +
									(ForceMerging == ForceMerging.Split ? "\t\t\t<flag name=\"forcemerging\" value=\"split\"/>\n" : "") +
									(ForceMerging == ForceMerging.Merged ? "\t\t\t<flag name=\"forcemerging\" value=\"merged\"/>\n" : "") +
									(ForceMerging == ForceMerging.NonMerged ? "\t\t\t<flag name=\"forcemerging\" value=\"nonmerged\"/>\n" : "") +
									(ForceNodump == ForceNodump.Ignore ? "\t\t\t<flag name=\"forcenodump\" value=\"ignore\"/>\n" : "") +
									(ForceNodump == ForceNodump.Obsolete ? "\t\t\t<flag name=\"forcenodump\" value=\"obsolete\"/>\n" : "") +
									(ForceNodump == ForceNodump.Required ? "\t\t\t<flag name=\"forcenodump\" value=\"required\"/>\n" : "") +
									"\t\t</flags>\n"
							: "") +
							"\t</header>\n" +
							"\t<data>\n";

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
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <returns>The new depth of the tag</returns>
		private int WriteStartGame(StreamWriter sw, DatItem rom, List<string> newsplit, string lastgame, int depth, int last)
		{
			try
			{
				// No game should start with a path separator
				if (rom.MachineName.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.MachineName = rom.MachineName.Substring(1);
				}

				string state = "";
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

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <returns>The new depth of the tag</returns>
		private int WriteEndGame(StreamWriter sw, List<string> splitpath, List<string> newsplit, int depth, out int last)
		{
			last = 0;

			try
			{
				string state = "";
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

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out DatItem using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">DatItem object to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteDatItem(StreamWriter sw, DatItem rom, int depth, bool ignoreblanks = false)
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
				string state = "", prefix = "";
				for (int i = 0; i < depth; i++)
				{
					prefix += "\t";
				}
				state += prefix;

				switch (rom.Type)
				{
					case ItemType.Archive:
						state += "<file type=\"archive\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ "/>\n";
						break;
					case ItemType.BiosSet:
						state += "<file type=\"biosset\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
							+ (((BiosSet)rom).Default != null
								? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
								: "")
							+ "/>\n";
						break;
					case ItemType.Disk:
						state += "<file type=\"disk\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA256) ? " sha256=\"" + ((Disk)rom).SHA256.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA384) ? " sha384=\"" + ((Disk)rom).SHA384.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Disk)rom).SHA512) ? " sha512=\"" + ((Disk)rom).SHA512.ToLowerInvariant() + "\"" : "")
							+ (((Disk)rom).ItemStatus != ItemStatus.None ? prefix + "/>\n" + prefix + "\t<flags>\n" +
								prefix + "\t\t<flag name=\"status\" value=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"/>\n" +
								prefix + "\t</flags>\n" +
								prefix + "</file>\n" : "/>\n");
						break;
					case ItemType.Release:
						state += "<file type=\"release\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
							+ (((Release)rom).Default != null
								? ((Release)rom).Default.ToString().ToLowerInvariant()
								: "")
							+ "/>\n";
						break;
					case ItemType.Rom:
						state += "<file type=\"rom\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
							+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA256) ? " sha256=\"" + ((Rom)rom).SHA256.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA384) ? " sha384=\"" + ((Rom)rom).SHA384.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).SHA512) ? " sha512=\"" + ((Rom)rom).SHA512.ToLowerInvariant() + "\"" : "")
							+ (!String.IsNullOrWhiteSpace(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
							+ (((Rom)rom).ItemStatus != ItemStatus.None ? prefix + "/>\n" + prefix + "\t<flags>\n" +
								prefix + "\t\t<flag name=\"status\" value=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"/>\n" +
								prefix + "\t</flags>\n" +
								prefix + "</file>\n" : "/>\n");
						break;
					case ItemType.Sample:
						state += "<file type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
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
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteFooter(StreamWriter sw, int depth)
		{
			try
			{
				string footer = "";
				for (int i = depth - 1; i >= 2; i--)
				{
					// Print out the number of tabs and the end folder
					for (int j = 0; j < i; j++)
					{
						footer += "\t";
					}
					footer += "</directory>\n";
				}
				footer += "\t</data>\n</datafile>\n";

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
