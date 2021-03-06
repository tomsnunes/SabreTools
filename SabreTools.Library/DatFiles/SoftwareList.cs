﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a SofwareList, M1, or MAME XML DAT
    /// </summary>
    /// TODO: Verify that all write for this DatFile type is correct
    internal class SoftwareList : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SoftwareList(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse an SofwareList XML DAT and return all found games and roms within
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
            // Prepare all internal variables
            Encoding enc = Utilities.GetEncoding(filename);
            XmlReader xtr = Utilities.GetXmlTextReader(filename);

            // If we got a null reader, just return
            if (xtr == null)
                return;

            // Otherwise, read the file to the end
            try
            {
                xtr.MoveToContent();
                while (!xtr.EOF)
                {
                    // We only want elements
                    if (xtr.NodeType != XmlNodeType.Element)
                    {
                        xtr.Read();
                        continue;
                    }

                    switch (xtr.Name)
                    {
                        case "softwarelist":
                            Name = (string.IsNullOrWhiteSpace(Name) ? xtr.GetAttribute("name") ?? string.Empty : Name);
                            Description = (string.IsNullOrWhiteSpace(Description) ? xtr.GetAttribute("description") ?? string.Empty : Description);
                            if (ForceMerging == ForceMerging.None)
                                ForceMerging = Utilities.GetForceMerging(xtr.GetAttribute("forcemerging"));

                            if (ForceNodump == ForceNodump.None)
                                ForceNodump = Utilities.GetForceNodump(xtr.GetAttribute("forcenodump"));

                            if (ForcePacking == ForcePacking.None)
                                ForcePacking = Utilities.GetForcePacking(xtr.GetAttribute("forcepacking"));

                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the machine
                        case "software":
                            ReadSoftware(xtr.ReadSubtree(), filename, sysid, srcid, keep, clean, remUnicode);

                            // Skip the software now that we've processed it
                            xtr.Skip();
                            break;

                        default:
                            xtr.Read();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Warning($"Exception found while parsing '{filename}': {ex}");

                // For XML errors, just skip the affected node
                xtr?.Read();
            }

            xtr.Dispose();
        }

        /// <summary>
        /// Read software information
        /// </summary>
        /// <param name="reader">XmlReader representing a software block</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadSoftware(
            XmlReader reader,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // If we have an empty software, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            string key = string.Empty;
            string temptype = reader.Name;
            bool containsItems = false;

            // Create a new machine
            MachineType machineType = MachineType.NULL;
            if (Utilities.GetYesNo(reader.GetAttribute("isbios")) == true)
                machineType |= MachineType.Bios;

            if (Utilities.GetYesNo(reader.GetAttribute("isdevice")) == true)
                machineType |= MachineType.Device;

            if (Utilities.GetYesNo(reader.GetAttribute("ismechanical")) == true)
                machineType |= MachineType.Mechanical;

            Machine machine = new Machine
            {
                Name = reader.GetAttribute("name"),
                Description = reader.GetAttribute("name"),
                Supported = Utilities.GetYesNo(reader.GetAttribute("supported")), // (yes|partial|no) "yes"

                CloneOf = reader.GetAttribute("cloneof") ?? string.Empty,
                Infos = new List<KeyValuePair<string, string>>(),

                MachineType = (machineType == MachineType.NULL ? MachineType.None : machineType),
            };

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "description":
                        machine.Description = reader.ReadElementContentAsString();
                        break;

                    case "year":
                        machine.Year = reader.ReadElementContentAsString();
                        break;

                    case "publisher":
                        machine.Publisher = reader.ReadElementContentAsString();
                        break;

                    case "info":
                        machine.Infos.Add(new KeyValuePair<string, string>(reader.GetAttribute("name"), reader.GetAttribute("value")));
                        reader.Read();
                        break;

                    case "sharedfeat":
                        // string sharedfeat_name = reader.GetAttribute("name");
                        // string sharedfeat_value = reader.GetAttribute("value");
                        reader.Read();
                        break;

                    case "part": // Contains all rom and disk information
                        containsItems = ReadPart(reader.ReadSubtree(), machine, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the part now that we've processed it
                        reader.Skip();
                        break;

                    default:
                        reader.Read();
                        break;
                }
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
        /// Read part information
        /// </summary>
        /// <param name="reader">XmlReader representing a part block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadPart(
            XmlReader reader,
            Machine machine,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string key = string.Empty, areaname = string.Empty, partname = string.Empty, partinterface = string.Empty;
            string temptype = reader.Name;
            long? areasize = null;
            var features = new List<KeyValuePair<string, string>>();
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "part")
                    {
                        partname = string.Empty;
                        partinterface = string.Empty;
                        features = new List<KeyValuePair<string, string>>();
                    }

                    if (reader.NodeType == XmlNodeType.EndElement && (reader.Name == "dataarea" || reader.Name == "diskarea"))
                    {
                        areaname = string.Empty;
                        areasize = null;
                    }

                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "part":
                        partname = reader.GetAttribute("name");
                        partinterface = reader.GetAttribute("interface");
                        reader.Read();
                        break;

                    case "feature":
                        features.Add(new KeyValuePair<string, string>(reader.GetAttribute("name"), reader.GetAttribute("value")));
                        reader.Read();
                        break;

                    case "dataarea":
                        areaname = reader.GetAttribute("name");
                        if (reader.GetAttribute("size") != null)
                        {
                            if (Int64.TryParse(reader.GetAttribute("size"), out long tempas))
                                areasize = tempas;
                        }

                        // string dataarea_width = reader.GetAttribute("width"); // (8|16|32|64) "8"
                        // string dataarea_endianness = reader.GetAttribute("endianness"); // endianness (big|little) "little"

                        containsItems = ReadDataArea(reader.ReadSubtree(), machine, features, areaname, areasize, 
                            partname, partinterface, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the dataarea now that we've processed it
                        reader.Skip();
                        break;

                    case "diskarea":
                        areaname = reader.GetAttribute("name");

                        containsItems = ReadDiskArea(reader.ReadSubtree(), machine, features, areaname, areasize,
                            partname, partinterface, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the diskarea now that we've processed it
                        reader.Skip();
                        break;

                    case "dipswitch":
                        // string dipswitch_name = reader.GetAttribute("name");
                        // string dipswitch_tag = reader.GetAttribute("tag");
                        // string dipswitch_mask = reader.GetAttribute("mask");

                        // For every <dipvalue> element...
                        // string dipvalue_name = reader.GetAttribute("name");
                        // string dipvalue_value = reader.GetAttribute("value");
                        // bool? dipvalue_default = Utilities.GetYesNo(reader.GetAttribute("default")); // (yes|no) "no"

                        reader.Skip();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return containsItems;
        }

        /// <summary>
        /// Read dataarea information
        /// </summary>
        /// <param name="reader">XmlReader representing a dataarea block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="features">List of features from the parent part</param>
        /// <param name="areaname">Name of the containing area</param>
        /// <param name="areasize">Size of the containing area</param>
        /// <param name="partname">Name of the containing part</param>
        /// <param name="partinterface">Interface of the containing part</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadDataArea(
            XmlReader reader,
            Machine machine,
            List<KeyValuePair<string, string>> features,
            string areaname,
            long? areasize,
            string partname,
            string partinterface,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string key = string.Empty;
            string temptype = reader.Name;
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "rom":
                        containsItems = true;

                        // If the rom is continue or ignore, add the size to the previous rom
                        if (reader.GetAttribute("loadflag") == "continue" || reader.GetAttribute("loadflag") == "ignore")
                        {
                            int index = this[key].Count - 1;
                            DatItem lastrom = this[key][index];
                            if (lastrom.ItemType == ItemType.Rom)
                            {
                                ((Rom)lastrom).Size += Utilities.GetSize(reader.GetAttribute("size"));
                            }
                            this[key].RemoveAt(index);
                            this[key].Add(lastrom);
                            reader.Read();
                            continue;
                        }

                        DatItem rom = new Rom
                        {
                            Name = reader.GetAttribute("name"),
                            Size = Utilities.GetSize(reader.GetAttribute("size")),
                            CRC = Utilities.CleanHashData(reader.GetAttribute("crc"), Constants.CRCLength),
                            MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                            RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.RIPEMD160Length),
                            SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                            SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                            SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                            SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                            Offset = reader.GetAttribute("offset"),
                            // Value = reader.GetAttribute("value");
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
                            // LoadFlag = reader.GetAttribute("loadflag"), // (load16_byte|load16_word|load16_word_swap|load32_byte|load32_word|load32_word_swap|load32_dword|load64_word|load64_word_swap|reload|fill|continue|reload_plain|ignore)

                            AreaName = areaname,
                            AreaSize = areasize,
                            Features = features,
                            PartName = partname,
                            PartInterface = partinterface,

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        rom.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(rom, clean, remUnicode);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return containsItems;
        }

        /// <summary>
        /// Read diskarea information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="features">List of features from the parent part</param>
        /// <param name="areaname">Name of the containing area</param>
        /// <param name="areasize">Size of the containing area</param>
        /// <param name="partname">Name of the containing part</param>
        /// <param name="partinterface">Interface of the containing part</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadDiskArea(
            XmlReader reader,
            Machine machine,
            List<KeyValuePair<string, string>> features,
            string areaname,
            long? areasize,
            string partname,
            string partinterface,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string key = string.Empty;
            string temptype = reader.Name;
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "disk":
                        containsItems = true;

                        DatItem disk = new Disk
                        {
                            Name = reader.GetAttribute("name"),
                            MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                            RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.RIPEMD160Length),
                            SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                            SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                            SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                            SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
                            Writable = Utilities.GetYesNo(reader.GetAttribute("writable")),

                            AreaName = areaname,
                            AreaSize = areasize,
                            Features = features,
                            PartName = partname,
                            PartInterface = partinterface,

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        disk.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(disk, clean, remUnicode);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return containsItems;
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

                XmlTextWriter xtw = new XmlTextWriter(fs, new UTF8Encoding(false));
                xtw.Formatting = Formatting.Indented;
                xtw.IndentChar = '\t';
                xtw.Indentation = 1;

                // Write out the header
                WriteHeader(xtw);

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
                            WriteEndGame(xtw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(xtw, rom);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.MachineName}");

                            lastgame = rom.MachineName;
                            continue;
                        }

                        // Now, output the rom data
                        WriteDatItem(xtw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(xtw);

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
                xtw.Dispose();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(XmlTextWriter xtw)
        {
            try
            {
                xtw.WriteStartDocument();
                xtw.WriteDocType("softwarelist", null, "softwarelist.dtd", null);

                xtw.WriteStartElement("softwarelist");
                xtw.WriteAttributeString("name", Name);
                xtw.WriteAttributeString("description", Description);

                switch (ForcePacking)
                {
                    case ForcePacking.Unzip:
                        xtw.WriteAttributeString("forcepacking", "unzip");
                        break;
                    case ForcePacking.Zip:
                        xtw.WriteAttributeString("forcepacking", "zip");
                        break;
                }

                switch (ForceMerging)
                {
                    case ForceMerging.Full:
                        xtw.WriteAttributeString("forcemerging", "full");
                        break;
                    case ForceMerging.Split:
                        xtw.WriteAttributeString("forcemerging", "split");
                        break;
                    case ForceMerging.Merged:
                        xtw.WriteAttributeString("forcemerging", "merged");
                        break;
                    case ForceMerging.NonMerged:
                        xtw.WriteAttributeString("forcemerging", "nonmerged");
                        break;
                }

                switch (ForceNodump)
                {
                    case ForceNodump.Ignore:
                        xtw.WriteAttributeString("forcenodump", "ignore");
                        break;
                    case ForceNodump.Obsolete:
                        xtw.WriteAttributeString("forcenodump", "obsolete");
                        break;
                    case ForceNodump.Required:
                        xtw.WriteAttributeString("forcenodump", "required");
                        break;
                }

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(XmlTextWriter xtw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                xtw.WriteStartElement("software");
                xtw.WriteAttributeString("name", datItem.GetField(Field.MachineName, ExcludeFields));

                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)) && !string.Equals(datItem.MachineName, datItem.CloneOf, StringComparison.OrdinalIgnoreCase))
                    xtw.WriteAttributeString("cloneof", datItem.CloneOf);
                
                if (!ExcludeFields[(int)Field.Supported])
                {
                    if (datItem.Supported == true)
                        xtw.WriteAttributeString("supported", "yes");
                    else if (datItem.Supported == false)
                        xtw.WriteAttributeString("supported", "no");
                    else
                        xtw.WriteAttributeString("supported", "partial");
                }

                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, ExcludeFields)))
                    xtw.WriteElementString("description", datItem.MachineDescription);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Year, ExcludeFields)))
                    xtw.WriteElementString("year", datItem.Year);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Publisher, ExcludeFields)))
                    xtw.WriteElementString("publisher", datItem.Publisher);

                if (!ExcludeFields[(int)Field.Infos] && datItem.Infos != null && datItem.Infos.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in datItem.Infos)
                    {
                        xtw.WriteStartElement("info");
                        xtw.WriteAttributeString("name", kvp.Key);
                        xtw.WriteAttributeString("value", kvp.Value);
                        xtw.WriteEndElement();
                    }
                }

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(XmlTextWriter xtw)
        {
            try
            {
                // End software
                xtw.WriteEndElement();

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(XmlTextWriter xtw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                xtw.WriteStartElement("part");
                xtw.WriteAttributeString("name", datItem.GetField(Field.PartName, ExcludeFields));
                xtw.WriteAttributeString("interface", datItem.GetField(Field.PartInterface, ExcludeFields));

                if (!ExcludeFields[(int)Field.Features] && datItem.Features != null && datItem.Features.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in datItem.Features)
                    {
                        xtw.WriteStartElement("feature");
                        xtw.WriteAttributeString("name", kvp.Key);
                        xtw.WriteAttributeString("value", kvp.Value);
                        xtw.WriteEndElement();
                    }
                }

                string areaName = datItem.GetField(Field.AreaName, ExcludeFields);
                switch (datItem.ItemType)
                {
                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        if (!ExcludeFields[(int)Field.AreaName] && string.IsNullOrWhiteSpace(areaName))
                            areaName = "cdrom";

                        xtw.WriteStartElement("diskarea");
                        xtw.WriteAttributeString("name", areaName);
                        if (!ExcludeFields[(int)Field.AreaSize] && disk.AreaSize != null)
                            xtw.WriteAttributeString("size", disk.AreaSize.ToString());

                        xtw.WriteStartElement("disk");
                        xtw.WriteAttributeString("name", disk.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            xtw.WriteAttributeString("md5", disk.MD5.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            xtw.WriteAttributeString("ripemd160", disk.RIPEMD160.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            xtw.WriteAttributeString("sha1", disk.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            xtw.WriteAttributeString("sha256", disk.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            xtw.WriteAttributeString("sha384", disk.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            xtw.WriteAttributeString("sha512", disk.SHA512.ToLowerInvariant());
                        if (!ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                            xtw.WriteAttributeString("status", disk.ItemStatus.ToString().ToLowerInvariant());
                        if (!ExcludeFields[(int)Field.Writable] && disk.Writable != null)
                            xtw.WriteAttributeString("writable", disk.Writable == true ? "yes" : "no");
                        xtw.WriteEndElement();

                        // End diskarea
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        if (!ExcludeFields[(int)Field.AreaName] && string.IsNullOrWhiteSpace(areaName))
                            areaName = "rom";

                        xtw.WriteStartElement("dataarea");
                        xtw.WriteAttributeString("name", areaName);
                        if (!ExcludeFields[(int)Field.AreaSize] && rom.AreaSize != null)
                            xtw.WriteAttributeString("size", rom.AreaSize.ToString());

                        xtw.WriteStartElement("rom");
                        xtw.WriteAttributeString("name", rom.GetField(Field.Name, ExcludeFields));
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            xtw.WriteAttributeString("size", rom.Size.ToString());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, ExcludeFields)))
                            xtw.WriteAttributeString("crc", rom.CRC.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            xtw.WriteAttributeString("md5", rom.MD5.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            xtw.WriteAttributeString("ripemd160", rom.RIPEMD160.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            xtw.WriteAttributeString("sha1", rom.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            xtw.WriteAttributeString("sha256", rom.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            xtw.WriteAttributeString("sha384", rom.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            xtw.WriteAttributeString("sha512", rom.SHA512.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Offset, ExcludeFields)))
                            xtw.WriteAttributeString("offset", rom.Offset);
                        //if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Value, ExcludeFields)))
                        //    xtw.WriteAttributeString("value", rom.Value);
                        if (!ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                            xtw.WriteAttributeString("status", rom.ItemStatus.ToString().ToLowerInvariant());
                        //if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Loadflag, ExcludeFields)))
                        //    xtw.WriteAttributeString("loadflag", rom.Loadflag);
                        xtw.WriteEndElement();

                        // End dataarea
                        xtw.WriteEndElement();
                        break;
                }

                // End part
                xtw.WriteEndElement();

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(XmlTextWriter xtw)
        {
            try
            {
                // End software
                xtw.WriteEndElement();

                // End softwarelist
                xtw.WriteEndElement();

                xtw.Flush();
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
