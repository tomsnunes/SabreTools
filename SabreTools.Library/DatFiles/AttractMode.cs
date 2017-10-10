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
using StreamReader = System.IO.StreamReader;
using StreamWriter = System.IO.StreamWriter;
#endif
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents parsing and writing of an AttractMode DAT
	/// </summary>
	public class AttractMode
	{
		/// <summary>
		/// Parse an AttractMode DAT and return all found games within
		/// </summary>
		/// <param name="datFile">DatFile to populate with the read information</param>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="sysid">System ID for the DAT</param>
		/// <param name="srcid">Source ID for the DAT</param>
		/// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
		/// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		public static void Parse(
			DatFile datFile,

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

					MachineName = gameinfo[0],
					MachineDescription = gameinfo[1],
					CloneOf = gameinfo[3],
					Year = gameinfo[4],
					Manufacturer = gameinfo[5],
					Comment = gameinfo[15],
				};

				// Now process and add the rom
				datFile.ParseAddHelper(rom, clean, remUnicode);
			}

			sr.Dispose();
		}

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datFile">DatFile to write out from</param>
		/// <param name="outfile">Name of the file to write to</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		public static bool WriteToFile(DatFile datFile, string outfile)
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
				List<string> keys = datFile.Keys.ToList();
				keys.Sort(new NaturalComparer());

				foreach (string key in keys)
				{
					List<DatItem> roms = datFile[key];

					// Resolve the names in the block
					roms = DatItem.ResolveNames(roms);

					for (int index = 0; index < roms.Count; index++)
					{
						DatItem item = roms[index];

						// There are apparently times when a null rom can skip by, skip them
						if (item.Name == null || item.MachineName == null)
						{
							Globals.Logger.Warning("Null rom found!");
							continue;
						}

						// If we have a new game, output the beginning of the new item
						if (lastgame == null || lastgame.ToLowerInvariant() != item.MachineName.ToLowerInvariant())
						{
							WriteStartGame(datFile, sw, item);
						}

						// If we have a "null" game (created by DATFromDir or something similar), log it to file
						if (item.Type == ItemType.Rom
							&& ((Rom)item).Size == -1
							&& ((Rom)item).CRC == "null")
						{
							Globals.Logger.Verbose("Empty folder found: {0}", item.MachineName);

							item.Name = (item.Name == "null" ? "-" : item.Name);
							((Rom)item).Size = Constants.SizeZero;
							((Rom)item).CRC = ((Rom)item).CRC == "null" ? Constants.CRCZero : null;
							((Rom)item).MD5 = ((Rom)item).MD5 == "null" ? Constants.MD5Zero : null;
							((Rom)item).SHA1 = ((Rom)item).SHA1 == "null" ? Constants.SHA1Zero : null;
							((Rom)item).SHA256 = ((Rom)item).SHA256 == "null" ? Constants.SHA256Zero : null;
							((Rom)item).SHA384 = ((Rom)item).SHA384 == "null" ? Constants.SHA384Zero : null;
							((Rom)item).SHA512 = ((Rom)item).SHA512 == "null" ? Constants.SHA512Zero : null;
						}

						// Set the new data to compare against
						lastgame = item.MachineName;
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
		/// Write out DAT header using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <returns>True if the data was written, false on error</returns>
		private static bool WriteHeader(StreamWriter sw)
		{
			try
			{
				string header = "#Title;Name;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra;Buttons\n";

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
		/// <param name="datFile">DatFile to write out from</param>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <returns>True if the data was written, false on error</returns>
		private static bool WriteStartGame(DatFile datFile, StreamWriter sw, DatItem rom)
		{
			try
			{
				// No game should start with a path separator
				if (rom.MachineName.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.MachineName = rom.MachineName.Substring(1);
				}

				string state = rom.MachineName + ";"
							+ rom.MachineDescription + ";"
							+ datFile.FileName + ";"
							+ rom.CloneOf + ";"
							+ rom.Year + ";"
							+ rom.Manufacturer + ";"
							/* + rom.Category */ + ";"
							/* + rom.Players */ + ";"
							/* + rom.Rotation */ + ";"
							/* + rom.Control */ + ";"
							/* + rom.Status */ + ";"
							/* + rom.DisplayCount */ + ";"
							/* + rom.DisplayType */ + ";"
							/* + rom.AltRomname */ + ";"
							/* + rom.AltTitle */ + ";"
							+ rom.Comment + ";"
							/* + rom.Buttons */ + "\n";

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
