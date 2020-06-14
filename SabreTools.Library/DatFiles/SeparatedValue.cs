using System;
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
    /// Represents parsing and writing of a value-separated DAT
    /// </summary>
    internal class SeparatedValue : DatFile
    {
        // Private instance variables specific to Separated Value DATs
        private readonly char _delim;

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        /// <param name="delim">Delimiter for parsing individual lines</param>
        public SeparatedValue(DatFile datFile, char delim)
            : base(datFile, cloneHeader: false)
        {
            _delim = delim;
        }

        /// <summary>
        /// Parse a character-separated value DAT and return all found games and roms within
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

            // Create an empty list of columns to parse though
            List<string> columns = new List<string>();

            long linenum = -1;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                linenum++;

                // Parse the first line, getting types from the column names
                if (linenum == 0)
                {
                    string[] parsedColumns = line.Split(_delim);
                    foreach (string parsed in parsedColumns)
                    {
                        switch (parsed.ToLowerInvariant().Trim('"'))
                        {
                            case "file":
                            case "filename":
                            case "file name":
                                columns.Add("DatFile.FileName");
                                break;

                            case "internal name":
                                columns.Add("DatFile.Name");
                                break;

                            case "description":
                            case "dat description":
                                columns.Add("DatFile.Description");
                                break;

                            case "game name":
                            case "game":
                            case "machine":
                                columns.Add("Machine.Name");
                                break;

                            case "game description":
                                columns.Add("Description");
                                break;

                            case "type":
                                columns.Add("DatItem.Type");
                                break;

                            case "rom":
                            case "romname":
                            case "rom name":
                            case "name":
                                columns.Add("Rom.Name");
                                break;

                            case "disk":
                            case "diskname":
                            case "disk name":
                                columns.Add("Disk.Name");
                                break;

                            case "size":
                                columns.Add("DatItem.Size");
                                break;

                            case "crc":
                            case "crc hash":
                                columns.Add("DatItem.CRC");
                                break;

                            case "md5":
                            case "md5 hash":
                                columns.Add("DatItem.MD5");
                                break;

                            case "ripemd":
                            case "ripemd160":
                            case "ripemd hash":
                            case "ripemd160 hash":
                                columns.Add("DatItem.RIPEMD160");
                                break;

                            case "sha1":
                            case "sha-1":
                            case "sha1 hash":
                            case "sha-1 hash":
                                columns.Add("DatItem.SHA1");
                                break;

                            case "sha256":
                            case "sha-256":
                            case "sha256 hash":
                            case "sha-256 hash":
                                columns.Add("DatItem.SHA256");
                                break;

                            case "sha384":
                            case "sha-384":
                            case "sha384 hash":
                            case "sha-384 hash":
                                columns.Add("DatItem.SHA384");
                                break;

                            case "sha512":
                            case "sha-512":
                            case "sha512 hash":
                            case "sha-512 hash":
                                columns.Add("DatItem.SHA512");
                                break;

                            case "nodump":
                            case "no dump":
                            case "status":
                            case "item status":
                                columns.Add("DatItem.Nodump");
                                break;

                            case "date":
                                columns.Add("DatItem.Date");
                                break;

                            default:
                                columns.Add("INVALID");
                                break;
                        }
                    }

                    continue;
                }

                // Otherwise, we want to split the line and parse
                string[] parsedLine = line.Split(_delim);

                // If the line doesn't have the correct number of columns, we log and skip
                if (parsedLine.Length != columns.Count)
                {
                    Globals.Logger.Warning($"Malformed line found in '{filename}' at line {linenum}");
                    continue;
                }

                // Set the output item information
                string machineName = null, machineDesc = null, name = null, crc = null, md5 = null,
                    ripemd160 = null, sha1 = null, sha256 = null, sha384 = null, sha512 = null, date = null;
                long size = -1;
                ItemType itemType = ItemType.Rom;
                ItemStatus status = ItemStatus.None;

                // Now we loop through and get values for everything
                for (int i = 0; i < columns.Count; i++)
                {
                    string value = parsedLine[i].Trim('"');
                    switch (columns[i])
                    {
                        case "DatFile.FileName":
                            FileName = (string.IsNullOrWhiteSpace(FileName) ? value : FileName);
                            break;

                        case "DatFile.Name":
                            Name = (string.IsNullOrWhiteSpace(Name) ? value : Name);
                            break;

                        case "DatFile.Description":
                            Description = (string.IsNullOrWhiteSpace(Description) ? value : Description);
                            break;

                        case "Machine.Name":
                            machineName = value;
                            break;

                        case "Description":
                            machineDesc = value;
                            break;

                        case "DatItem.Type":
                            itemType = Utilities.GetItemType(value) ?? ItemType.Rom;
                            break;

                        case "Rom.Name":
                        case "Disk.Name":
                            name = string.IsNullOrWhiteSpace(value) ? name : value;
                            break;

                        case "DatItem.Size":
                            if (!Int64.TryParse(value, out size))
                                size = -1;

                            break;

                        case "DatItem.CRC":
                            crc = Utilities.CleanHashData(value, Constants.CRCLength);
                            break;

                        case "DatItem.MD5":
                            md5 = Utilities.CleanHashData(value, Constants.MD5Length);
                            break;

                        case "DatItem.RIPEMD160":
                            ripemd160 = Utilities.CleanHashData(value, Constants.RIPEMD160Length);
                            break;

                        case "DatItem.SHA1":
                            sha1 = Utilities.CleanHashData(value, Constants.SHA1Length);
                            break;

                        case "DatItem.SHA256":
                            sha256 = Utilities.CleanHashData(value, Constants.SHA256Length);
                            break;

                        case "DatItem.SHA384":
                            sha384 = Utilities.CleanHashData(value, Constants.SHA384Length);
                            break;

                        case "DatItem.SHA512":
                            sha512 = Utilities.CleanHashData(value, Constants.SHA512Length);
                            break;

                        case "DatItem.Nodump":
                            status = Utilities.GetItemStatus(value);
                            break;

                        case "DatItem.Date":
                            date = value;
                            break;
                    }
                }

                // And now we populate and add the new item
                switch (itemType)
                {
                    case ItemType.Archive:
                        Archive archive = new Archive()
                        {
                            Name = name,

                            MachineName = machineName,
                            MachineDescription = machineDesc,
                        };

                        ParseAddHelper(archive, clean, remUnicode);
                        break;

                    case ItemType.BiosSet:
                        BiosSet biosset = new BiosSet()
                        {
                            Name = name,

                            MachineName = machineName,
                            Description = machineDesc,
                        };

                        ParseAddHelper(biosset, clean, remUnicode);
                        break;

                    case ItemType.Disk:
                        Disk disk = new Disk()
                        {
                            Name = name,
                            MD5 = md5,
                            RIPEMD160 = ripemd160,
                            SHA1 = sha1,
                            SHA256 = sha256,
                            SHA384 = sha384,
                            SHA512 = sha512,

                            MachineName = machineName,
                            MachineDescription = machineDesc,

                            ItemStatus = status,
                        };

                        ParseAddHelper(disk, clean, remUnicode);
                        break;

                    case ItemType.Release:
                        Release release = new Release()
                        {
                            Name = name,

                            MachineName = machineName,
                            MachineDescription = machineDesc,
                        };

                        ParseAddHelper(release, clean, remUnicode);
                        break;

                    case ItemType.Rom:
                        Rom rom = new Rom()
                        {
                            Name = name,
                            Size = size,
                            CRC = crc,
                            MD5 = md5,
                            RIPEMD160 = ripemd160,
                            SHA1 = sha1,
                            SHA256 = sha256,
                            SHA384 = sha384,
                            SHA512 = sha512,
                            Date = date,

                            MachineName = machineName,
                            MachineDescription = machineDesc,

                            ItemStatus = status,
                        };

                        ParseAddHelper(rom, clean, remUnicode);
                        break;

                    case ItemType.Sample:
                        Sample sample = new Sample()
                        {
                            Name = name,

                            MachineName = machineName,
                            MachineDescription = machineDesc,
                        };

                        ParseAddHelper(sample, clean, remUnicode);
                        break;
                }
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

                SeparatedValueWriter svw = new SeparatedValueWriter(fs, new UTF8Encoding(false));
                svw.Quotes = true;
                svw.Separator = this._delim.ToString();
                svw.VerifyFieldCount = true;

                // Write out the header
                WriteHeader(svw);

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
                        }

                        // Now, output the rom data
                        WriteDatItem(svw, rom, ignoreblanks);
                    }
                }

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
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
                    "File Name",
                    "Internal Name",
                    "Description",
                    "Game Name",
                    "Game Description",
                    "Type",
                    "Rom Name",
                    "Disk Name",
                    "Size",
                    "CRC",
                    "MD5",
                    //"RIPEMD160",
                    "SHA1",
                    "SHA256",
                    //"SHA384",
                    //"SHA512",
                    "Nodump",
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
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(SeparatedValueWriter svw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Separated values should only output Rom and Disk
                if (datItem.ItemType != ItemType.Disk && datItem.ItemType != ItemType.Rom)
                    return true;

                // Build the state based on excluded fields
                string[] fields = new string[14]; // 17;
                fields[0] = FileName;
                fields[1] = Name;
                fields[2] = Description;
                fields[3] = datItem.GetField(Field.MachineName, ExcludeFields);
                fields[4] = datItem.GetField(Field.Description, ExcludeFields);

                switch (datItem.ItemType)
                {
                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        fields[5] = "disk";
                        fields[6] = string.Empty;
                        fields[7] = disk.GetField(Field.Name, ExcludeFields);
                        fields[8] = string.Empty;
                        fields[9] = string.Empty;
                        fields[10] = disk.GetField(Field.MD5, ExcludeFields).ToLowerInvariant();
                        //fields[11] = disk.GetField(Field.RIPEMD160, ExcludeFields).ToLowerInvariant();
                        fields[11] = disk.GetField(Field.SHA1, ExcludeFields).ToLowerInvariant();
                        fields[12] = disk.GetField(Field.SHA256, ExcludeFields).ToLowerInvariant();
                        //fields[13] = disk.GetField(Field.SHA384, ExcludeFields).ToLowerInvariant();
                        //fields[14] = disk.GetField(Field.SHA512, ExcludeFields).ToLowerInvariant();
                        fields[13] = disk.GetField(Field.Status, ExcludeFields);
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        fields[5] = "rom";
                        fields[6] = rom.GetField(Field.Name, ExcludeFields);
                        fields[7] = string.Empty;
                        fields[8] = rom.GetField(Field.Size, ExcludeFields);
                        fields[9] = rom.GetField(Field.CRC, ExcludeFields).ToLowerInvariant();
                        fields[10] = rom.GetField(Field.MD5, ExcludeFields).ToLowerInvariant();
                        //fields[11] = rom.GetField(Field.RIPEMD160, ExcludeFields).ToLowerInvariant();
                        fields[11] = rom.GetField(Field.SHA1, ExcludeFields).ToLowerInvariant();
                        fields[12] = rom.GetField(Field.SHA256, ExcludeFields).ToLowerInvariant();
                        //fields[13] = rom.GetField(Field.SHA384, ExcludeFields).ToLowerInvariant();
                        //fields[14] = rom.GetField(Field.SHA512, ExcludeFields).ToLowerInvariant();
                        fields[13] = rom.GetField(Field.Status, ExcludeFields);
                        break;
                }

                svw.WriteString(CreatePrefixPostfix(datItem, true));
                svw.WriteValues(fields, false);
                svw.WriteString(CreatePrefixPostfix(datItem, false));
                svw.WriteLine();

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
