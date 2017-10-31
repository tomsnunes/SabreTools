using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.Items;
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
	/// Represents parsing and writing of a Missfile
	/// </summary>
	public class Missfile : DatFile
	{
		/// <summary>
		/// Constructor designed for casting a base DatFile
		/// </summary>
		/// <param name="datFile">Parent DatFile to copy from</param>
		public Missfile(DatFile datFile)
		{
			this._datHeader = datFile._datHeader;
			this._items = datFile._items;
			this._sortedBy = datFile._sortedBy;
			this._datStats = datFile._datStats;
		}

		/// <summary>
		/// Parse a Missfileand return all found games and roms within
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
			// There is no consistent way to parse a missfile...
			throw new NotImplementedException();
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
						WriteRomData(sw, rom, lastgame, ignoreblanks);

						// Set the new data to compare against
						lastgame = rom.MachineName;
					}
				}

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
		/// Write out RomData using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteRomData(StreamWriter sw, DatItem rom, string lastgame, bool ignoreblanks = false)
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
				string state = "", name = "", pre = "", post = "";
				pre = Prefix + (Quotes ? "\"" : "");
				post = (Quotes ? "\"" : "") + Postfix;

				if (rom.Type == ItemType.Rom)
				{
					// Check for special strings in prefix and postfix
					pre = pre
						.Replace("%game%", rom.MachineName)
						.Replace("%name%", rom.Name)
						.Replace("%crc%", ((Rom)rom).CRC)
						.Replace("%md5%", ((Rom)rom).MD5)
						.Replace("%sha1%", ((Rom)rom).SHA1)
						.Replace("%sha256%", ((Rom)rom).SHA256)
						.Replace("%sha384%", ((Rom)rom).SHA384)
						.Replace("%sha512%", ((Rom)rom).SHA512)
						.Replace("%size%", ((Rom)rom).Size.ToString());
					post = post
						.Replace("%game%", rom.MachineName)
						.Replace("%name%", rom.Name)
						.Replace("%crc%", ((Rom)rom).CRC)
						.Replace("%md5%", ((Rom)rom).MD5)
						.Replace("%sha1%", ((Rom)rom).SHA1)
						.Replace("%sha256%", ((Rom)rom).SHA256)
						.Replace("%sha384%", ((Rom)rom).SHA384)
						.Replace("%sha512%", ((Rom)rom).SHA512)
						.Replace("%size%", ((Rom)rom).Size.ToString());
				}
				else if (rom.Type == ItemType.Disk)
				{
					// Check for special strings in prefix and postfix
					pre = pre
						.Replace("%game%", rom.MachineName)
						.Replace("%name%", rom.Name)
						.Replace("%crc%", string.Empty)
						.Replace("%md5%", ((Disk)rom).MD5)
						.Replace("%sha1%", ((Disk)rom).SHA1)
						.Replace("%sha256%", ((Disk)rom).SHA256)
						.Replace("%sha384%", ((Disk)rom).SHA384)
						.Replace("%sha512%", ((Disk)rom).SHA512)
						.Replace("%size%", string.Empty);
					post = post
						.Replace("%game%", rom.MachineName)
						.Replace("%name%", rom.Name)
						.Replace("%crc%", string.Empty)
						.Replace("%md5%", ((Disk)rom).MD5)
						.Replace("%sha1%", ((Disk)rom).SHA1)
						.Replace("%sha256%", ((Disk)rom).SHA256)
						.Replace("%sha384%", ((Disk)rom).SHA384)
						.Replace("%sha512%", ((Disk)rom).SHA512)
						.Replace("%size%", string.Empty);
				}
				else
				{
					// Check for special strings in prefix and postfix
					pre = pre
						.Replace("%game%", rom.MachineName)
						.Replace("%name%", rom.Name)
						.Replace("%crc%", string.Empty)
						.Replace("%md5%", string.Empty)
						.Replace("%sha1%", string.Empty)
						.Replace("%sha256%", string.Empty)
						.Replace("%sha384%", string.Empty)
						.Replace("%sha512%", string.Empty)
						.Replace("%size%", string.Empty);
					post = post
						.Replace("%game%", rom.MachineName)
						.Replace("%name%", rom.Name)
						.Replace("%crc%", string.Empty)
						.Replace("%md5%", string.Empty)
						.Replace("%sha1%", string.Empty)
						.Replace("%sha256%", string.Empty)
						.Replace("%sha384%", string.Empty)
						.Replace("%sha512%", string.Empty)
						.Replace("%size%", string.Empty);
				}

				// If we're in Romba mode, the state is consistent
				if (Romba)
				{
					if (rom.Type == ItemType.Rom)
					{
						// We can only write out if there's a SHA-1
						if (((Rom)rom).SHA1 != "")
						{
							name = ((Rom)rom).SHA1.Substring(0, 2)
								+ "/" + ((Rom)rom).SHA1.Substring(2, 2)
								+ "/" + ((Rom)rom).SHA1.Substring(4, 2)
								+ "/" + ((Rom)rom).SHA1.Substring(6, 2)
								+ "/" + ((Rom)rom).SHA1 + ".gz";
							state += pre + name + post + "\n";
						}
					}
					else if (rom.Type == ItemType.Disk)
					{
						// We can only write out if there's a SHA-1
						if (((Disk)rom).SHA1 != "")
						{
							name = ((Disk)rom).SHA1.Substring(0, 2)
								+ "/" + ((Disk)rom).SHA1.Substring(2, 2)
								+ "/" + ((Disk)rom).SHA1.Substring(4, 2)
								+ "/" + ((Disk)rom).SHA1.Substring(6, 2)
								+ "/" + ((Disk)rom).SHA1 + ".gz";
							state += pre + name + post + "\n";
						}
					}
				}

				// Otherwise, use any flags
				else
				{
					name = (UseGame ? rom.MachineName : rom.Name);
					if (RepExt != "" || RemExt)
					{
						if (RemExt)
						{
							RepExt = "";
						}

						string dir = Path.GetDirectoryName(name);
						dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
						name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + RepExt);
					}
					if (AddExt != "")
					{
						name += AddExt;
					}
					if (!UseGame && GameName)
					{
						name = Path.Combine(rom.MachineName, name);
					}

					if (UseGame && rom.MachineName != lastgame)
					{
						state += pre + name + post + "\n";
						lastgame = rom.MachineName;
					}
					else if (!UseGame)
					{
						state += pre + name + post + "\n";
					}
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
	}
}
