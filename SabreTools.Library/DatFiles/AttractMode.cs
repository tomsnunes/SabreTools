﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using SabreTools.Library.Writers;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of an AttractMode DAT
    /// </summary>
    internal class AttractMode : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public AttractMode(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
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
                    RIPEMD160 = Constants.RIPEMD160Zero,
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
                ParseAddHelper(rom, clean, remUnicode);
            }

            sr.Dispose();
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
                Globals.Logger.User($"Opening file for writing: {outfile}");
                FileStream fs = Utilities.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                SeparatedValueWriter svw = new SeparatedValueWriter(fs, new UTF8Encoding(false));
                svw.Quotes = false;
                svw.Separator = ';';
                svw.VerifyFieldCount = true;

                // Write out the header
                WriteHeader(svw);

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
                        DatItem item = roms[index];

                        // There are apparently times when a null rom can skip by, skip them
                        if (item.Name == null || item.MachineName == null)
                        {
                            Globals.Logger.Warning("Null rom found!");
                            continue;
                        }

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != item.MachineName.ToLowerInvariant())
                            WriteDatItem(svw, item, ignoreblanks);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (item.ItemType == ItemType.Rom
                            && ((Rom)item).Size == -1
                            && ((Rom)item).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {item.MachineName}");

                            item.Name = (item.Name == "null" ? "-" : item.Name);
                            ((Rom)item).Size = Constants.SizeZero;
                        }

                        // Set the new data to compare against
                        lastgame = item.MachineName;
                    }
                }

                Globals.Logger.Verbose($"File written!{Environment.NewLine}");
                svw.Dispose();
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
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(SeparatedValueWriter svw)
        {
            try
            {
                string[] headers = new string[]
                {
                    "#Title",
                    "Name",
                    "Emulator",
                    "CloneOf",
                    "Year",
                    "Manufacturer",
                    "Category",
                    "Players",
                    "Rotation",
                    "Control",
                    "Status",
                    "DisplayCount",
                    "DisplayType",
                    "AltRomname",
                    "AltTitle",
                    "Extra",
                    "Buttons",
                };

                svw.WriteHeader(headers);

                svw.Flush();
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
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(SeparatedValueWriter svw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && (((Rom)datItem).Size == 0 || ((Rom)datItem).Size == -1)))
                return true;

            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Pre-process the item name
                ProcessItemName(datItem, true);

                string[] fields = new string[]
                {
                    datItem.GetField(Field.MachineName, ExcludeFields),
                    datItem.GetField(Field.Description, ExcludeFields),
                    FileName,
                    datItem.GetField(Field.CloneOf, ExcludeFields),
                    datItem.GetField(Field.Year, ExcludeFields),
                    datItem.GetField(Field.Manufacturer, ExcludeFields),
                    string.Empty, // datItem.GetField(Field.Category, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.Players, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.Rotation, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.Control, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.Status, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.DisplayCount, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.DisplayType, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.AltRomname, ExcludeFields)
                    string.Empty, // datItem.GetField(Field.AltTitle, ExcludeFields)
                    datItem.GetField(Field.Comment, ExcludeFields),
                    string.Empty, // datItem.GetField(Field.Buttons, ExcludeFields)
                };

                svw.WriteValues(fields);

                svw.Flush();
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
