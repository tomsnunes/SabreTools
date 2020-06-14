using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using SabreTools.Library.Writers;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a DosCenter DAT
    /// </summary>
    internal class DosCenter : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public DosCenter(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse a DOSCenter DAT and return all found games and roms within
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

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                // If the line is the header or a game
                if (Regex.IsMatch(line, Constants.HeaderPatternCMP))
                {
                    GroupCollection gc = Regex.Match(line, Constants.HeaderPatternCMP).Groups;
                    string normalizedValue = gc[1].Value.ToLowerInvariant();

                    // If we have a known header
                    if (normalizedValue == "doscenter")
                        ReadHeader(sr, keep);

                    // If we have a game
                    else if (normalizedValue == "game" )
                        ReadGame(sr, filename, sysid, srcid, clean, remUnicode);
                }
            }

            sr.Dispose();
        }

        /// <summary>
        /// Read header information
        /// </summary>
        /// <param name="reader">StreamReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        private void ReadHeader(StreamReader reader, bool keep)
        {
            // If there's no subtree to the header, skip it
            if (reader == null || reader.EndOfStream)
                return;

            // Otherwise, add what is possible
            string line = reader.ReadLine();
            while (!Regex.IsMatch(line, Constants.EndPatternCMP))
            {
                // Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
                GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
                string itemval = gc[2].Value.Replace("\"", string.Empty);

                // Some dats don't have the space between "Name:" and the dat name
                if (line.Trim().StartsWith("Name:"))
                {
                    Name = (string.IsNullOrWhiteSpace(Name) ? line.Substring(6).Trim() : Name);
                    line = reader.ReadLine();
                    continue;
                }

                switch (gc[1].Value)
                {
                    case "Name:":
                        Name = (string.IsNullOrWhiteSpace(Name) ? itemval : Name);
                        break;
                    case "Description:":
                        Description = (string.IsNullOrWhiteSpace(Description) ? itemval : Description);
                        break;
                    case "Version:":
                        Version = (string.IsNullOrWhiteSpace(Version) ? itemval : Version);
                        break;
                    case "Date:":
                        Date = (string.IsNullOrWhiteSpace(Date) ? itemval : Date);
                        break;
                    case "Author:":
                        Author = (string.IsNullOrWhiteSpace(Author) ? itemval : Author);
                        break;
                    case "Homepage:":
                        Homepage = (string.IsNullOrWhiteSpace(Homepage) ? itemval : Homepage);
                        break;
                    case "Comment:":
                        Comment = (string.IsNullOrWhiteSpace(Comment) ? itemval : Comment);
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
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadGame(
            StreamReader reader,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool clean,
            bool remUnicode)
        {
            // Prepare all internal variables
            bool containsItems = false;
            Machine machine = new Machine()
            {
                MachineType = MachineType.None,
            };

            // If there's no subtree to the header, skip it
            if (reader == null || reader.EndOfStream)
                return;

            // Otherwise, add what is possible
            string line = reader.ReadLine();
            while (!Regex.IsMatch(line, Constants.EndPatternCMP))
            {
                // Item-specific lines have a known pattern
                string trimmedline = line.Trim();
                if (trimmedline.StartsWith("file ("))
                {
                    containsItems = true;
                    ItemType temptype = ItemType.Rom;

                    // Create the proper DatItem based on the type
                    DatItem item = Utilities.GetDatItem(temptype);

                    // Then populate it with information
                    item.CopyMachineInformation(machine);

                    item.SystemID = sysid;
                    item.System = filename;
                    item.SourceID = srcid;

                    // Get the line split by spaces and quotes
                    string[] linegc = Utilities.SplitLineAsCMP(line);

                    // Loop over the specifics
                    for (int i = 0; i < linegc.Length; i++)
                    {
                        // Names are not quoted, for some stupid reason
                        if (linegc[i] == "name")
                        {
                            // Get the name in order until we find the next flag
                            while (++i < linegc.Length
                                && linegc[i] != "size"
                                && linegc[i] != "date"
                                && linegc[i] != "crc")
                            {
                                item.Name += "{linegc[i]}";
                            }

                            // Perform correction
                            item.Name = item.Name.TrimStart();
                            i--;
                        }

                        // Get the size from the next part
                        else if (linegc[i] == "size")
                        {
                            if (!Int64.TryParse(linegc[++i], out long tempsize))
                                tempsize = 0;

                            ((Rom)item).Size = tempsize;
                        }

                        // Get the date from the next part
                        else if (linegc[i] == "date")
                        {
                            ((Rom)item).Date = $"{linegc[++i].Replace("\"", string.Empty)} {linegc[++i].Replace("\"", string.Empty)}";
                        }

                        // Get the CRC from the next part
                        else if (linegc[i] == "crc")
                        {
                            ((Rom)item).CRC = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                        }
                    }

                    // Now process and add the rom
                    ParseAddHelper(item, clean, remUnicode);

                    line = reader.ReadLine();
                    continue;
                }

                // Game-specific lines have a known pattern
                GroupCollection setgc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
                string itemval = setgc[2].Value.Replace("\"", string.Empty);

                switch (setgc[1].Value)
                {
                    case "name":
                        machine.Name = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
                        machine.Description = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
                        break;
                }

                line = reader.ReadLine();
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
                Globals.Logger.User($"Opening file for writing: {outfile}");
                FileStream fs = Utilities.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                ClrMameProWriter cmpw = new ClrMameProWriter(fs, new UTF8Encoding(false));
                cmpw.Quotes = false;

                // Write out the header
                WriteHeader(cmpw);

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

                        List<string> newsplit = rom.MachineName.Split('\\').ToList();

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                             WriteEndGame(cmpw, rom);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(cmpw, rom);

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
                        WriteDatItem(cmpw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(cmpw);

                Globals.Logger.Verbose($"File written!{Environment.NewLine}");
                cmpw.Dispose();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(ClrMameProWriter cmpw)
        {
            try
            {
                cmpw.WriteStartElement("DOSCenter");
                cmpw.WriteStandalone("Name:", Name, false);
                cmpw.WriteStandalone("Description:", Description, false);
                cmpw.WriteStandalone("Version:", Version, false);
                cmpw.WriteStandalone("Date:", Date, false);
                cmpw.WriteStandalone("Author:", Author, false);
                cmpw.WriteStandalone("Homepage:", Homepage, false);
                cmpw.WriteStandalone("Comment:", Comment, false);
                cmpw.WriteEndElement();

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(ClrMameProWriter cmpw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                cmpw.WriteStartElement("game");
                cmpw.WriteStandalone("name", $"{datItem.GetField(Field.MachineName, ExcludeFields)}.zip", true);

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(ClrMameProWriter cmpw, DatItem datItem)
        {
            try
            {
                // End game
                cmpw.WriteEndElement();

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(ClrMameProWriter cmpw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                switch (datItem.ItemType)
                {
                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        cmpw.WriteStartElement("file");
                        cmpw.WriteAttributeString("name", datItem.GetField(Field.Name, ExcludeFields));
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            cmpw.WriteAttributeString("size", rom.Size.ToString());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, ExcludeFields)))
                            cmpw.WriteAttributeString("date", rom.Date);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, ExcludeFields)))
                            cmpw.WriteAttributeString("crc", rom.CRC.ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;
                }

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(ClrMameProWriter cmpw)
        {
            try
            {
                // End game
                cmpw.WriteEndElement();

                cmpw.Flush();
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
