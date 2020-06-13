using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a ClrMamePro DAT
    /// </summary>
    /// TODO: Can there be a writer like XmlTextWriter for this? Or too inconsistent?
    internal class ClrMamePro : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public ClrMamePro(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
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

                // Comments in CMP DATs start with a #
                if (line.Trim().StartsWith("#"))
                    continue;

                // If the line is the header or a game
                if (Regex.IsMatch(line, Constants.HeaderPatternCMP))
                {
                    GroupCollection gc = Regex.Match(line, Constants.HeaderPatternCMP).Groups;
                    string normalizedValue = gc[1].Value.ToLowerInvariant();

                    // If we have a known header
                    if (normalizedValue == "clrmamepro"
                        || normalizedValue == "romvault")
                    {
                        ReadHeader(sr, keep);
                    }
                    // If we have a known set type
                    else if (normalizedValue == "set"      // Used by the most ancient DATs
                        || normalizedValue == "game"       // Used by most CMP DATs
                        || normalizedValue == "machine")   // Possibly used by MAME CMP DATs
                    {
                        ReadSet(sr, false, filename, sysid, srcid, clean, remUnicode);
                    }
                    else if (normalizedValue == "resource")  // Used by some other DATs to denote a BIOS set
                    {
                        ReadSet(sr, true, filename, sysid, srcid, clean, remUnicode);
                    }
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
            bool superdat = false;

            // If there's no subtree to the header, skip it
            if (reader == null || reader.EndOfStream)
                return;

            // Otherwise, add what is possible
            string line = reader.ReadLine();
            while (!Regex.IsMatch(line, Constants.EndPatternCMP))
            {
                // We only want elements
                if (line.Trim().StartsWith("#"))
                {
                    line = reader.ReadLine();
                    continue;
                }

                // Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
                GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
                string itemval = gc[2].Value.Replace("\"", string.Empty);

                switch (gc[1].Value)
                {
                    case "name":
                        Name = (string.IsNullOrWhiteSpace(Name) ? itemval : Name);
                        superdat = superdat || itemval.Contains(" - SuperDAT");

                        if (keep && superdat)
                            Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);

                        break;
                    case "description":
                        Description = (string.IsNullOrWhiteSpace(Description) ? itemval : Description);
                        break;
                    case "rootdir":
                        RootDir = (string.IsNullOrWhiteSpace(RootDir) ? itemval : RootDir);
                        break;
                    case "category":
                        Category = (string.IsNullOrWhiteSpace(Category) ? itemval : Category);
                        break;
                    case "version":
                        Version = (string.IsNullOrWhiteSpace(Version) ? itemval : Version);
                        break;
                    case "date":
                        Date = (string.IsNullOrWhiteSpace(Date) ? itemval : Date);
                        break;
                    case "author":
                        Author = (string.IsNullOrWhiteSpace(Author) ? itemval : Author);
                        break;
                    case "email":
                        Email = (string.IsNullOrWhiteSpace(Email) ? itemval : Email);
                        break;
                    case "homepage":
                        Homepage = (string.IsNullOrWhiteSpace(Homepage) ? itemval : Homepage);
                        break;
                    case "url":
                        Url = (string.IsNullOrWhiteSpace(Url) ? itemval : Url);
                        break;
                    case "comment":
                        Comment = (string.IsNullOrWhiteSpace(Comment) ? itemval : Comment);
                        break;
                    case "header":
                        Header = (string.IsNullOrWhiteSpace(Header) ? itemval : Header);
                        break;
                    case "type":
                        Type = (string.IsNullOrWhiteSpace(Type) ? itemval : Type);
                        superdat = superdat || itemval.Contains("SuperDAT");
                        break;
                    case "forcemerging":
                        if (ForceMerging == ForceMerging.None)
                            ForceMerging = Utilities.GetForceMerging(itemval);
                        
                        break;
                    case "forcezipping":
                        if (ForcePacking == ForcePacking.None)
                            ForcePacking = Utilities.GetForcePacking(itemval);

                        break;
                    case "forcepacking":
                        if (ForcePacking == ForcePacking.None)
                            ForcePacking = Utilities.GetForcePacking(itemval);

                        break;
                }

                line = reader.ReadLine();
            }
        }

        /// <summary>
        /// Read set information
        /// </summary>
        /// <param name="reader">StreamReader to use to parse the header</param>
        /// <param name="resource">True if the item is a resource (bios), false otherwise</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadSet(
            StreamReader reader,
            bool resource,

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
                MachineType = (resource ? MachineType.Bios : MachineType.None),
            };

            // If there's no subtree to the header, skip it
            if (reader == null || reader.EndOfStream)
                return;

            // Otherwise, add what is possible
            string line = reader.ReadLine();
            while (!Regex.IsMatch(line, Constants.EndPatternCMP))
            {
                // We only want elements
                if (line.Trim().StartsWith("#"))
                {
                    line = reader.ReadLine();
                    continue;
                }

                // Item-specific lines have a known pattern
                string trimmedline = line.Trim();
                if (trimmedline.StartsWith("archive (")
                    || trimmedline.StartsWith("biosset (")
                    || trimmedline.StartsWith("disk (")
                    || trimmedline.StartsWith("release (")
                    || trimmedline.StartsWith("rom (")
                    || (trimmedline.StartsWith("sample") && !trimmedline.StartsWith("sampleof")))
                {
                    containsItems = true;
                    ItemType temptype = ItemType.Rom;
                    if (line.Trim().StartsWith("rom ("))
                        temptype = ItemType.Rom;
                    else if (line.Trim().StartsWith("disk ("))
                        temptype = ItemType.Disk;
                    else if (line.Trim().StartsWith("sample"))
                        temptype = ItemType.Sample;

                    // Create the proper DatItem based on the type
                    DatItem item = Utilities.GetDatItem(temptype);

                    // Then populate it with information
                    item.CopyMachineInformation(machine);

                    item.SystemID = sysid;
                    item.System = filename;
                    item.SourceID = srcid;

                    // If we have a sample, treat it special
                    if (temptype == ItemType.Sample)
                    {
                        line = line.Trim().Remove(0, 6).Trim().Replace("\"", string.Empty); // Remove "sample" from the input string
                        item.Name = line;

                        // Now process and add the sample
                        ParseAddHelper(item, clean, remUnicode);
                        line = reader.ReadLine();
                        continue;
                    }

                    // Get the line split by spaces and quotes
                    string[] linegc = Utilities.SplitLineAsCMP(line);

                    // Loop over all attributes normally and add them if possible
                    for (int i = 0; i < linegc.Length; i++)
                    {
                        // Look at the current item and use it if possible
                        string quoteless = linegc[i].Replace("\"", string.Empty);
                        switch (quoteless)
                        {
                            //If the item is empty, we automatically skip it because it's a fluke
                            case "":
                                continue;

                            // Special cases for standalone item statuses
                            case "baddump":
                            case "good":
                            case "nodump":
                            case "verified":
                                ItemStatus tempStandaloneStatus = Utilities.GetItemStatus(quoteless);
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).ItemStatus = tempStandaloneStatus;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).ItemStatus = tempStandaloneStatus;

                                break;

                            // Regular attributes
                            case "name":
                                quoteless = linegc[++i].Replace("\"", string.Empty);
                                item.Name = quoteless;
                                break;
                            case "size":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    if (Int64.TryParse(quoteless, out long size))
                                        ((Rom)item).Size = size;
                                    else
                                        ((Rom)item).Size = -1;
                                }

                                break;
                            case "crc":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).CRC = Utilities.CleanHashData(quoteless, Constants.CRCLength);
                                }

                                break;
                            case "md5":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).MD5 = Utilities.CleanHashData(quoteless, Constants.MD5Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    i++;
                                    quoteless = linegc[i].Replace("\"", string.Empty);
                                    ((Disk)item).MD5 = Utilities.CleanHashData(quoteless, Constants.MD5Length);
                                }

                                break;
                            case "ripemd160":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).RIPEMD160 = Utilities.CleanHashData(quoteless, Constants.RIPEMD160Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).RIPEMD160 = Utilities.CleanHashData(quoteless, Constants.RIPEMD160Length);
                                }

                                break;
                            case "sha1":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA1 = Utilities.CleanHashData(quoteless, Constants.SHA1Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA1 = Utilities.CleanHashData(quoteless, Constants.SHA1Length);
                                }

                                break;
                            case "sha256":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA256 = Utilities.CleanHashData(quoteless, Constants.SHA256Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA256 = Utilities.CleanHashData(quoteless, Constants.SHA256Length);
                                }

                                break;
                            case "sha384":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA384 = Utilities.CleanHashData(quoteless, Constants.SHA384Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA384 = Utilities.CleanHashData(quoteless, Constants.SHA384Length);
                                }

                                break;
                            case "sha512":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA512 = Utilities.CleanHashData(quoteless, Constants.SHA512Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA512 = Utilities.CleanHashData(quoteless, Constants.SHA512Length);
                                }

                                break;
                            case "status":
                            case "flags":
                                quoteless = linegc[++i].Replace("\"", string.Empty);
                                ItemStatus tempFlagStatus = Utilities.GetItemStatus(quoteless);
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).ItemStatus = tempFlagStatus;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).ItemStatus = tempFlagStatus;

                                break;
                            case "date":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    // If we have quotes in the next item, assume only one item
                                    if (linegc[i + 1].Contains("\""))
                                        quoteless = linegc[++i].Replace("\"", string.Empty);

                                    // Otherwise, we assume we need to read the next two items
                                    else
                                        quoteless = $"{linegc[++i].Replace("\"", string.Empty)} {linegc[++i].Replace("\"", string.Empty)}";

                                    ((Rom)item).Date = quoteless;
                                }
                                else if (item.ItemType == ItemType.Release)
                                {
                                    // If we have quotes in the next item, assume only one item
                                    if (linegc[i + 1].Contains("\""))
                                        quoteless = linegc[++i].Replace("\"", string.Empty);

                                    // Otherwise, we assume we need to read the next two items
                                    else
                                        quoteless = $"{linegc[++i].Replace("\"", string.Empty)} {linegc[++i].Replace("\"", string.Empty)}";

                                    ((Release)item).Date = quoteless;
                                }

                                break;
                            case "default":
                                if (item.ItemType == ItemType.BiosSet)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((BiosSet)item).Default = Utilities.GetYesNo(quoteless.ToLowerInvariant());
                                }
                                else if (item.ItemType == ItemType.Release)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Release)item).Default = Utilities.GetYesNo(quoteless.ToLowerInvariant());
                                }

                                break;
                            case "description":
                                if (item.ItemType == ItemType.BiosSet)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((BiosSet)item).Description = quoteless.ToLowerInvariant();
                                }

                                break;
                            case "region":
                                if (item.ItemType == ItemType.Release)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Release)item).Region = quoteless.ToLowerInvariant();
                                }

                                break;
                            case "language":
                                if (item.ItemType == ItemType.Release)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Release)item).Language = quoteless.ToLowerInvariant();
                                }

                                break;
                        }
                    }

                    // Now process and add the rom
                    ParseAddHelper(item, clean, remUnicode);

                    line = reader.ReadLine();
                    continue;
                }

                // Set-specific lines have a known pattern
                GroupCollection setgc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
                string itemval = setgc[2].Value.Replace("\"", string.Empty);

                switch (setgc[1].Value)
                {
                    case "name":
                        machine.Name = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
                        machine.Description = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
                        break;
                    case "description":
                        machine.Description = itemval;
                        break;
                    case "year":
                        machine.Year = itemval;
                        break;
                    case "manufacturer":
                        machine.Manufacturer = itemval;
                        break;
                    case "cloneof":
                        machine.CloneOf = itemval;
                        break;
                    case "romof":
                        machine.RomOf = itemval;
                        break;
                    case "sampleof":
                        machine.SampleOf = itemval;
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
                cmpw.Quotes = true;

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

                            // If we're in a mode that doesn't allow for actual empty folders, add the blank info
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
                cmpw.Close();
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
                cmpw.WriteStartElement("clrmamepro");

                cmpw.WriteStandalone("name", Name);
                cmpw.WriteStandalone("description", Description);
                if (!string.IsNullOrWhiteSpace(Category))
                    cmpw.WriteStandalone("category", Category);
                cmpw.WriteStandalone("version", Version);
                if (!string.IsNullOrWhiteSpace(Date))
                    cmpw.WriteStandalone("date", Date);
                cmpw.WriteStandalone("author", Author);
                if (!string.IsNullOrWhiteSpace(Email))
                    cmpw.WriteStandalone("email", Email);
                if (!string.IsNullOrWhiteSpace(Homepage))
                    cmpw.WriteStandalone("homepage", Homepage);
                if (!string.IsNullOrWhiteSpace(Url))
                    cmpw.WriteStandalone("url", Url);
                if (!string.IsNullOrWhiteSpace(Comment))
                    cmpw.WriteStandalone("comment", Comment);
                
                switch (ForcePacking)
                {
                    case ForcePacking.Unzip:
                        cmpw.WriteStandalone("forcezipping", "no", false);
                        break;
                    case ForcePacking.Zip:
                        cmpw.WriteStandalone("forcezipping", "yes", false);
                        break;
                }

                switch (ForceMerging)
                {
                    case ForceMerging.Full:
                        cmpw.WriteStandalone("forcemerging", "full", false);
                        break;
                    case ForceMerging.Split:
                        cmpw.WriteStandalone("forcemerging", "split", false);
                        break;
                    case ForceMerging.Merged:
                        cmpw.WriteStandalone("forcemerging", "merged", false);
                        break;
                    case ForceMerging.NonMerged:
                        cmpw.WriteStandalone("forcemerging", "nonmerged", false);
                        break;
                }

                // End clrmamepro
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
                cmpw.WriteStartElement(datItem.MachineType == MachineType.Bios ? "resource" : "game");
                cmpw.WriteStandalone("name", datItem.GetField(Field.MachineName, ExcludeFields));
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RomOf, ExcludeFields)))
                    cmpw.WriteStandalone("romof", datItem.RomOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)))
                    cmpw.WriteStandalone("cloneof", datItem.CloneOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, ExcludeFields)))
                    cmpw.WriteStandalone("sampleof", datItem.SampleOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, ExcludeFields)))
                    cmpw.WriteStandalone("description", datItem.MachineDescription);
                else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, ExcludeFields)))
                    cmpw.WriteStandalone("description", datItem.MachineName);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Year, ExcludeFields)))
                    cmpw.WriteStandalone("year", datItem.Year);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Manufacturer, ExcludeFields)))
                    cmpw.WriteStandalone("manufacturer", datItem.Manufacturer);

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
                // Build the state based on excluded fields
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, ExcludeFields)))
                    cmpw.WriteStandalone("sampleof", datItem.SampleOf);

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
        /// <param name="datFile">DatFile to write out from</param>
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
                    case ItemType.Archive:
                        cmpw.WriteStartElement("archive");
                        cmpw.WriteAttributeString("name", datItem.GetField(Field.Name, ExcludeFields));
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        cmpw.WriteStartElement("biosset");
                        cmpw.WriteAttributeString("name", biosSet.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(biosSet.GetField(Field.BiosDescription, ExcludeFields)))
                            cmpw.WriteAttributeString("description", biosSet.Description);
                        if (!ExcludeFields[(int)Field.Default] && biosSet.Default != null)
                            cmpw.WriteAttributeString("default", biosSet.Default.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        cmpw.WriteStartElement("disk");
                        cmpw.WriteAttributeString("name", disk.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            cmpw.WriteAttributeString("md5", disk.MD5.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            cmpw.WriteAttributeString("ripemd160", disk.RIPEMD160.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            cmpw.WriteAttributeString("sha1", disk.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            cmpw.WriteAttributeString("sha256", disk.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            cmpw.WriteAttributeString("sha384", disk.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            cmpw.WriteAttributeString("sha512", disk.SHA512.ToLowerInvariant());
                        if (!ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                            cmpw.WriteAttributeString("flags", disk.ItemStatus.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Release:
                        var release = datItem as Release;
                        cmpw.WriteStartElement("release");
                        cmpw.WriteAttributeString("name", release.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Region, ExcludeFields)))
                            cmpw.WriteAttributeString("region", release.Region);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Language, ExcludeFields)))
                            cmpw.WriteAttributeString("language", release.Language);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, ExcludeFields)))
                            cmpw.WriteAttributeString("date", release.Date);
                        if (!ExcludeFields[(int)Field.Default] && release.Default != null)
                            cmpw.WriteAttributeString("default", release.Default.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        cmpw.WriteStartElement("rom");
                        cmpw.WriteAttributeString("name", rom.GetField(Field.Name, ExcludeFields));
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            cmpw.WriteAttributeString("size", rom.Size.ToString());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, ExcludeFields)))
                            cmpw.WriteAttributeString("crc", rom.CRC.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            cmpw.WriteAttributeString("md5", rom.MD5.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            cmpw.WriteAttributeString("ripemd160", rom.RIPEMD160.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            cmpw.WriteAttributeString("sha1", rom.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            cmpw.WriteAttributeString("sha256", rom.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            cmpw.WriteAttributeString("sha384", rom.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            cmpw.WriteAttributeString("sha512", rom.SHA512.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, ExcludeFields)))
                            cmpw.WriteAttributeString("date", rom.Date);
                        if (!ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                            cmpw.WriteAttributeString("flags", rom.ItemStatus.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Sample:
                        cmpw.WriteStartElement("sample");
                        cmpw.WriteAttributeString("name", datItem.GetField(Field.Name, ExcludeFields));
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
