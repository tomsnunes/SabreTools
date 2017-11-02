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
	/// Represents parsing and writing of an OfflineList XML DAT
	/// </summary>
	public class OfflineList : DatFile
	{
		/// <summary>
		/// Constructor designed for casting a base DatFile
		/// </summary>
		/// <param name="datFile">Parent DatFile to copy from</param>
		public OfflineList(DatFile datFile)
		{
			this._datHeader = datFile._datHeader;
			this._items = datFile._items;
			this._sortedBy = datFile._sortedBy;
			this._datStats = datFile._datStats;
		}

		/// <summary>
		/// Parse an OfflineList XML DAT and return all found games and roms within
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <remarks>
		/// </remarks>
		public void Parse(
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
			(this as DatFile as Logiqx).Parse(filename, sysid, srcid, keep, clean, remUnicode);
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
				FileStream fs = FileTools.TryCreate(outfile);

				// If we get back null for some reason, just log and return
				if (fs == null)
				{
					Globals.Logger.Warning("File '{0}' could not be created for writing! Please check to see if the file is writable", outfile);
					return false;
				}

				StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(true));

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

						// If we have a "null" game (created by DATFromDir or something similar), log it to file
						if (rom.Type == ItemType.Rom
							&& ((Rom)rom).Size == -1
							&& ((Rom)rom).CRC == "null")
						{
							Globals.Logger.Verbose("Empty folder found: {0}", rom.MachineName);

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
				string header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n"
							+ "<dat xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"datas.xsd\">\n"
							+ "\t<configuration>\n"
							+ "\t\t<datName>" + HttpUtility.HtmlEncode(Name) + "</datName>\n"
							+ "\t\t<datVersion>" + Count + "</datVersion>\n"
							+ "\t\t<system>none</system>\n"
							+ "\t\t<screenshotsWidth>240</screenshotsWidth>\n"
							+ "\t\t<screenshotsHeight>160</screenshotsHeight>\n"
							+ "\t\t<infos>\n"
							+ "\t\t\t<title visible=\"false\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<location visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<publisher visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<sourceRom visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<saveType visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<romSize visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<releaseNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<languageNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<comment visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<romCRC visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<im1CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
							+ "\t\t\t<im2CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
							+ "\t\t\t<languages visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t</infos>\n"
							+ "\t\t<canOpen>\n"
							+ "\t\t\t<extension>.bin</extension>\n"
							+ "\t\t</canOpen>\n"
							+ "\t\t<newDat>\n"
							+ "\t\t\t<datVersionURL>" + HttpUtility.HtmlEncode(Url) + "</datVersionURL>\n"
							+ "\t\t\t<datURL fileName=\"" + HttpUtility.HtmlEncode(FileName) + ".zip\">" + HttpUtility.HtmlEncode(Url) + "</datURL>\n"
							+ "\t\t\t<imURL>" + HttpUtility.HtmlEncode(Url) + "</imURL>\n"
							+ "\t\t</newDat>\n"
							+ "\t\t<search>\n"
							+ "\t\t\t<to value=\"location\" default=\"true\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"romSize\" default=\"true\" auto=\"false\"/>\n"
							+ "\t\t\t<to value=\"languages\" default=\"true\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"saveType\" default=\"false\" auto=\"false\"/>\n"
							+ "\t\t\t<to value=\"publisher\" default=\"false\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"sourceRom\" default=\"false\" auto=\"true\"/>\n"
							+ "\t\t</search>\n"
							+ "\t\t<romTitle >%u - %n</romTitle>\n"
							+ "\t</configuration>\n"
							+ "\t<games>\n";

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
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteEndGame(StreamWriter sw)
		{
			try
			{
				string state = "\t\t</game>\n";

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
				state += "\t\t<game>\n"
							+ "\t\t\t<imageNumber>1</imageNumber>\n"
							+ "\t\t\t<releaseNumber>1</releaseNumber>\n"
							+ "\t\t\t<title>" + HttpUtility.HtmlEncode(rom.Name) + "</title>\n"
							+ "\t\t\t<saveType>None</saveType>\n";

				if (rom.Type == ItemType.Rom)
				{
					state += "\t\t\t<romSize>" + ((Rom)rom).Size + "</romSize>\n";
				}

				state += "\t\t\t<publisher>None</publisher>\n"
					+ "\t\t\t<location>0</location>\n"
					+ "\t\t\t<sourceRom>None</sourceRom>\n"
					+ "\t\t\t<language>0</language>\n";

				if (rom.Type == ItemType.Disk)
				{
					state += "\t\t\t<files>\n"
						+ (((Disk)rom).MD5 != null
							? "\t\t\t\t<romMD5 extension=\".chd\">" + ((Disk)rom).MD5.ToUpperInvariant() + "</romMD5>\n"
							: "\t\t\t\t<romSHA1 extension=\".chd\">" + ((Disk)rom).SHA1.ToUpperInvariant() + "</romSHA1>\n")
						+ "\t\t\t</files>\n";
				}
				else if (rom.Type == ItemType.Rom)
				{
					string tempext = Path.GetExtension(((Rom)rom).Name);
					if (!tempext.StartsWith("."))
					{
						tempext = "." + tempext;
					}

					state += "\t\t\t<files>\n"
						+ (((Rom)rom).CRC != null
							? "\t\t\t\t<romCRC extension=\"" + tempext + "\">" + ((Rom)rom).CRC.ToUpperInvariant() + "</romMD5>\n"
							: ((Rom)rom).MD5 != null
								? "\t\t\t\t<romMD5 extension=\"" + tempext + "\">" + ((Rom)rom).MD5.ToUpperInvariant() + "</romMD5>\n"
								: "\t\t\t\t<romSHA1 extension=\"" + tempext + "\">" + ((Rom)rom).SHA1.ToUpperInvariant() + "</romSHA1>\n")
						+ "\t\t\t</files>\n";
				}

				state += "\t\t\t<im1CRC>00000000</im1CRC>\n"
					+ "\t\t\t<im2CRC>00000000</im2CRC>\n"
					+ "\t\t\t<comment></comment>\n"
					+ "\t\t\t<duplicateID>0</duplicateID>\n"
					+ "\t\t</game>\n";

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
				string footer = "\t\t</game>"
							+ "\t</games>\n"
							+ "\t<gui>\n"
							+ "\t\t<images width=\"487\" height=\"162\">\n"
							+ "\t\t\t<image x=\"0\" y=\"0\" width=\"240\" height=\"160\"/>\n"
							+ "\t\t\t<image x=\"245\" y=\"0\" width=\"240\" height=\"160\"/>\n"
							+ "\t\t</images>\n"
							+ "\t</gui>\n"
							+ "</dat>";

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
