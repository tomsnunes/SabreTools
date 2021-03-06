﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Readers;
using SabreTools.Library.Tools;
using SabreTools.Library.Writers;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a RomCenter DAT
    /// </summary>
    internal class RomCenter : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public RomCenter(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse a RomCenter DAT and return all found games and roms within
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
            // Prepare all intenral variables
            Encoding enc = Utilities.GetEncoding(filename);
            IniReader ir = Utilities.GetIniReader(filename, false);

            // If we got a null reader, just return
            if (ir == null)
                return;

            // Otherwise, read teh file to the end
            try
            {
                ir.ReadNextLine();
                while (!ir.EndOfStream)
                {
                    // We don't care about whitespace or comments
                    if (ir.RowType == IniRowType.None || ir.RowType == IniRowType.Comment)
                    {
                        ir.ReadNextLine();
                        continue;
                    }

                    // If we have a section
                    if (ir.RowType == IniRowType.SectionHeader)
                    {
                        switch (ir.Section.ToLowerInvariant())
                        {
                            case "credits":
                                ReadCreditsSection(ir);
                                break;

                            case "dat":
                                ReadDatSection(ir);
                                break;

                            case "emulator":
                                ReadEmulatorSection(ir);
                                break;

                            case "games":
                                ReadGamesSection(ir, sysid, srcid, clean, remUnicode);
                                break;

                            // Unknown section so we ignore it
                            default:
                                ir.ReadNextLine();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Warning($"Exception found while parsing '{filename}': {ex}");
            }

            ir.Dispose();
        }

        /// <summary>
        /// Read credits information
        /// </summary>
        /// <param name="reader">IniReader to use to parse the credits</param>
        private void ReadCreditsSection(IniReader reader)
        {
            // If the reader is somehow null, skip it
            if (reader == null)
                return;

            reader.ReadNextLine();
            while (!reader.EndOfStream && reader.Section.ToLowerInvariant() == "credits")
            {
                // We don't care about whitespace, comments, or invalid
                if (reader.RowType != IniRowType.KeyValue)
                {
                    reader.ReadNextLine();
                    continue;
                }

                var kvp = reader.KeyValuePair;

                // If the KeyValuePair is invalid, skip it
                if (kvp == null)
                {
                    reader.ReadNextLine();
                    continue;
                }

                // Get all credits items (ONLY OVERWRITE IF THERE'S NO DATA)
                switch (kvp?.Key.ToLowerInvariant())
                {
                    case "author":
                        Author = string.IsNullOrWhiteSpace(Author) ? kvp?.Value : Author;
                        reader.ReadNextLine();
                        break;

                    case "version":
                        Version = string.IsNullOrWhiteSpace(Version) ? kvp?.Value : Version;
                        reader.ReadNextLine();
                        break;

                    case "email":
                        Email = string.IsNullOrWhiteSpace(Email) ? kvp?.Value : Email;
                        reader.ReadNextLine();
                        break;

                    case "homepage":
                        Homepage = string.IsNullOrWhiteSpace(Homepage) ? kvp?.Value : Homepage;
                        reader.ReadNextLine();
                        break;

                    case "url":
                        Url = string.IsNullOrWhiteSpace(Url) ? kvp?.Value : Url;
                        reader.ReadNextLine();
                        break;

                    case "date":
                        Date = string.IsNullOrWhiteSpace(Date) ? kvp?.Value : Date;
                        reader.ReadNextLine();
                        break;

                    // Unknown value, just skip
                    default:
                        reader.ReadNextLine();
                        break;
                }
            }
        }

        /// <summary>
        /// Read dat information
        /// </summary>
        /// <param name="reader">IniReader to use to parse the credits</param>
        private void ReadDatSection(IniReader reader)
        {
            // If the reader is somehow null, skip it
            if (reader == null)
                return;

            reader.ReadNextLine();
            while (!reader.EndOfStream && reader.Section.ToLowerInvariant() == "dat")
            {
                // We don't care about whitespace, comments, or invalid
                if (reader.RowType != IniRowType.KeyValue)
                {
                    reader.ReadNextLine();
                    continue;
                }

                var kvp = reader.KeyValuePair;

                // If the KeyValuePair is invalid, skip it
                if (kvp == null)
                {
                    reader.ReadNextLine();
                    continue;
                }

                // Get all dat items (ONLY OVERWRITE IF THERE'S NO DATA)
                switch (kvp?.Key.ToLowerInvariant())
                {
                    case "version":
                        string rcVersion = kvp?.Value;
                        reader.ReadNextLine();
                        break;

                    case "plugin":
                        string plugin = kvp?.Value;
                        reader.ReadNextLine();
                        break;

                    case "split":
                        if (ForceMerging == ForceMerging.None && kvp?.Value == "1")
                            ForceMerging = ForceMerging.Split;

                        reader.ReadNextLine();
                        break;

                    case "merge":
                        if (ForceMerging == ForceMerging.None && kvp?.Value == "1")
                            ForceMerging = ForceMerging.Merged;

                        reader.ReadNextLine();
                        break;

                    // Unknown value, just skip
                    default:
                        reader.ReadNextLine();
                        break;
                }
            }
        }

        /// <summary>
        /// Read emulator information
        /// </summary>
        /// <param name="reader">IniReader to use to parse the credits</param>
        private void ReadEmulatorSection(IniReader reader)
        {
            // If the reader is somehow null, skip it
            if (reader == null)
                return;

            reader.ReadNextLine();
            while (!reader.EndOfStream && reader.Section.ToLowerInvariant() == "emulator")
            {
                // We don't care about whitespace, comments, or invalid
                if (reader.RowType != IniRowType.KeyValue)
                {
                    reader.ReadNextLine();
                    continue;
                }

                var kvp = reader.KeyValuePair;

                // If the KeyValuePair is invalid, skip it
                if (kvp == null)
                {
                    reader.ReadNextLine();
                    continue;
                }

                // Get all emulator items (ONLY OVERWRITE IF THERE'S NO DATA)
                switch (kvp?.Key.ToLowerInvariant())
                {
                    case "refname":
                        Name = string.IsNullOrWhiteSpace(Name) ? kvp?.Value : Name;
                        reader.ReadNextLine();
                        break;

                    case "version":
                        Description = string.IsNullOrWhiteSpace(Description) ? kvp?.Value : Description;
                        reader.ReadNextLine();
                        break;

                    // Unknown value, just skip
                    default:
                        reader.ReadNextLine();
                        break;
                }
            }
        }

        /// <summary>
        /// Read games information
        /// </summary>
        /// <param name="reader">IniReader to use to parse the credits</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadGamesSection(IniReader reader, int sysid, int srcid, bool clean, bool remUnicode)
        {
            // If the reader is somehow null, skip it
            if (reader == null)
                return;

            reader.ReadNextLine();
            while (!reader.EndOfStream && reader.Section.ToLowerInvariant() == "games")
            {
                // We don't care about whitespace or comments
                // We're keeping keyvalue in case a file has '=' in the row
                if (reader.RowType != IniRowType.Invalid && reader.RowType != IniRowType.KeyValue)
                {
                    reader.ReadNextLine();
                    continue;
                }

                // Roms are not valid row formats, usually
                string line = reader.Line;

                // If we don't have a valid game, keep reading
                if (!line.StartsWith("¬"))
                {
                    reader.ReadNextLine();
                    continue;
                }

                // Some old RC DATs have this behavior
                if (line.Contains("¬N¬O"))
                    line = line.Replace("¬N¬O", string.Empty) + "¬¬";

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
                    size = 0;

                Rom rom = new Rom
                {
                    Name = rominfo[5],
                    Size = size,
                    CRC = Utilities.CleanHashData(rominfo[6], Constants.CRCLength),
                    ItemStatus = ItemStatus.None,

                    MachineName = rominfo[3],
                    MachineDescription = rominfo[4],
                    CloneOf = rominfo[1],
                    RomOf = rominfo[8],
                    MergeTag = rominfo[9],

                    SystemID = sysid,
                    SourceID = srcid,
                };

                // Now process and add the rom
                ParseAddHelper(rom, clean, remUnicode);

                reader.ReadNextLine();
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
                Globals.Logger.User($"Opening file for writing: {outfile}");
                FileStream fs = Utilities.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                IniWriter iw = new IniWriter(fs, new UTF8Encoding(false));

                // Write out the header
                WriteHeader(iw);

                // Write out each of the machines and roms
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

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.MachineName}");

                            rom.Name = (rom.Name == "null" ? "-" : rom.Name);
                            ((Rom)rom).Size = Constants.SizeZero;
                            ((Rom)rom).CRC = ((Rom)rom).CRC == "null" ? Constants.CRCZero : null;
                            ((Rom)rom).MD5 = ((Rom)rom).MD5 == "null" ? Constants.MD5Zero : null;
                            ((Rom)rom).RIPEMD160 = ((Rom)rom).RIPEMD160 == "null" ? Constants.RIPEMD160Zero : null;
                            ((Rom)rom).SHA1 = ((Rom)rom).SHA1 == "null" ? Constants.SHA1Zero : null;
                            ((Rom)rom).SHA256 = ((Rom)rom).SHA256 == "null" ? Constants.SHA256Zero : null;
                            ((Rom)rom).SHA384 = ((Rom)rom).SHA384 == "null" ? Constants.SHA384Zero : null;
                            ((Rom)rom).SHA512 = ((Rom)rom).SHA512 == "null" ? Constants.SHA512Zero : null;
                        }

                        // Now, output the rom data
                        WriteDatItem(iw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
                iw.Dispose();
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
        /// <param name="iw">IniWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(IniWriter iw)
        {
            try
            {
                iw.WriteSection("CREDITS");
                iw.WriteKeyValuePair("author", Author);
                iw.WriteKeyValuePair("version", Version);
                iw.WriteKeyValuePair("comment", Comment);

                iw.WriteSection("DAT");
                iw.WriteKeyValuePair("version", "2.50");
                iw.WriteKeyValuePair("split", ForceMerging == ForceMerging.Split ? "1" : "0");
                iw.WriteKeyValuePair("merge", ForceMerging == ForceMerging.Full || ForceMerging == ForceMerging.Merged ? "1" : "0");

                iw.WriteSection("EMULATOR");
                iw.WriteKeyValuePair("refname", Name);
                iw.WriteKeyValuePair("version", Description);

                iw.WriteSection("GAMES");

                iw.Flush();
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
        /// <param name="iw">IniWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(IniWriter iw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

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

            try
            {
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                iw.WriteString($"¬{datItem.GetField(Field.CloneOf, ExcludeFields)}");
                iw.WriteString($"¬{datItem.GetField(Field.CloneOf, ExcludeFields)}");
                iw.WriteString($"¬{datItem.GetField(Field.MachineName, ExcludeFields)}");
                if (string.IsNullOrWhiteSpace(datItem.MachineDescription))
                    iw.WriteString($"¬{datItem.GetField(Field.MachineName, ExcludeFields)}");
                else
                    iw.WriteString($"¬{datItem.GetField(Field.Description, ExcludeFields)}");
                iw.WriteString($"¬{datItem.GetField(Field.Name, ExcludeFields)}");
                iw.WriteString($"¬{datItem.GetField(Field.CRC, ExcludeFields)}");
                iw.WriteString($"¬{datItem.GetField(Field.Size, ExcludeFields)}");
                iw.WriteString($"¬{datItem.GetField(Field.RomOf, ExcludeFields)}");
                iw.WriteString($"¬{datItem.GetField(Field.Merge, ExcludeFields)}");
                iw.WriteString("¬");
                iw.WriteLine();

                iw.Flush();
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
