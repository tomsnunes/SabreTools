﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;
using NaturalSort;
using Newtonsoft.Json;

namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Base class for all items included in a set
    /// </summary>
    public abstract class DatItem : IEquatable<DatItem>, IComparable<DatItem>, ICloneable
    {
        #region Protected instance variables

        [JsonIgnore]
        protected Machine _machine = new Machine();

        #endregion

        #region Publicly facing variables

        #region Standard item information

        /// <summary>
        /// Name of the item
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Item type for outputting
        /// </summary>
        [JsonIgnore]
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Duplicate type when compared to another item
        /// </summary>
        [JsonIgnore]
        public DupeType DupeType { get; set; }

        #endregion

        #region Machine information

        /// <summary>
        /// Name of the machine associated with the item
        /// </summary>
        [JsonIgnore]
        public string MachineName
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Name;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Name = value;
            }
        }

        /// <summary>
        /// Additional notes on the machine
        /// </summary>
        [JsonIgnore]
        public string Comment
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Comment;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Comment = value;
            }
        }

        /// <summary>
        /// Extended description of the machine
        /// </summary>
        [JsonIgnore]
        public string MachineDescription
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Description;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Description = value;
            }
        }

        /// <summary>
        /// Machine year(s) of release/manufacture
        /// </summary>
        [JsonIgnore]
        public string Year
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Year;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Year = value;
            }
        }

        /// <summary>
        /// Machine manufacturer, if available
        /// </summary>
        [JsonIgnore]
        public string Manufacturer
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Manufacturer;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Manufacturer = value;
            }
        }

        /// <summary>
        /// Machine publisher, if available
        /// </summary>
        [JsonIgnore]
        public string Publisher
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Publisher;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Publisher = value;
            }
        }

        /// <summary>
        /// Machine romof parent
        /// </summary>
        [JsonIgnore]
        public string RomOf
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.RomOf;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.RomOf = value;
            }
        }

        /// <summary>
        /// Machine cloneof parent
        /// </summary>
        [JsonIgnore]
        public string CloneOf
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.CloneOf;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.CloneOf = value;
            }
        }

        /// <summary>
        /// Machine sampleof parent
        /// </summary>
        [JsonIgnore]
        public string SampleOf
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.SampleOf;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.SampleOf = value;
            }
        }

        /// <summary>
        /// Machine support status
        /// </summary>
        /// <remarks>yes = true, partial = null, no = false</remarks>
        [JsonIgnore]
        public bool? Supported
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Supported;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Supported = value;
            }
        }

        /// <summary>
        /// Emulator source file related to the machine
        /// </summary>
        [JsonIgnore]
        public string SourceFile
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.SourceFile;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.SourceFile = value;
            }
        }

        /// <summary>
        /// Machine runnable status
        /// </summary>
        /// <remarks>yes = true, partial = null, no = false</remarks>
        [JsonIgnore]
        public bool? Runnable
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Runnable;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Runnable = value;
            }
        }

        /// <summary>
        /// Machine board name
        /// </summary>
        [JsonIgnore]
        public string Board
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Board;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Board = value;
            }
        }

        /// <summary>
        /// Rebuild location if different than machine name
        /// </summary>
        [JsonIgnore]
        public string RebuildTo
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.RebuildTo;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.RebuildTo = value;
            }
        }

        /// <summary>
        /// List of associated device names
        /// </summary>
        [JsonIgnore]
        public List<string> Devices
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Devices;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Devices = value;
            }
        }

        /// <summary>
        /// List of slot options
        /// </summary>
        [JsonIgnore]
        public List<string> SlotOptions
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.SlotOptions;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.SlotOptions = value;
            }
        }

        /// <summary>
        /// List of info items
        /// </summary>
        [JsonIgnore]
        public List<KeyValuePair<string, string>> Infos
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.Infos;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.Infos = value;
            }
        }

        /// <summary>
        /// Type of the associated machine
        /// </summary>
        [JsonIgnore]
        public MachineType MachineType
        {
            get
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                return _machine.MachineType;
            }
            set
            {
                if (_machine == null)
                {
                    _machine = new Machine();
                }

                _machine.MachineType = value;
            }
        }

        #endregion

        #region Software list information

        /// <summary>
        /// Original hardware part associated with the item
        /// </summary>
        [JsonProperty("partname")]
        public string PartName { get; set; }

        /// <summary>
        /// Original hardware interface associated with the item
        /// </summary>
        [JsonProperty("partinterface")]
        public string PartInterface { get; set; }

        /// <summary>
        /// Features provided to/by the item
        /// </summary>
        [JsonProperty("features")]
        public List<KeyValuePair<string, string>> Features { get; set; }

        /// <summary>
        /// Original hardware part name within an item
        /// </summary>
        [JsonProperty("areaname")]
        public string AreaName { get; set; }

        /// <summary>
        /// Original hardware size within the part
        /// </summary>
        [JsonProperty("areasize")]
        public long? AreaSize { get; set; }

        #endregion

        #region Source metadata information

        /// <summary>
        /// Internal system ID for organization
        /// </summary>
        [JsonIgnore]
        public int SystemID { get; set; }

        /// <summary>
        /// Internal system name for organization
        /// </summary>
        [JsonIgnore]
        public string System { get; set; }

        /// <summary>
        /// Internal source ID for organization
        /// </summary>
        [JsonIgnore]
        public int SourceID { get; set; }

        /// <summary>
        /// Internal source name for organization
        /// </summary>
        [JsonIgnore]
        public string Source { get; set; }

        /// <summary>
        /// Flag if item should be removed
        /// </summary>
        [JsonIgnore]
        public bool Remove { get; set; }

        #endregion

        #endregion

        #region Instance Methods

        #region Accessors

        /// <summary>
        /// Get the value of that field as a string, if possible
        /// </summary>
        public string GetField(Field field, bool[] excludeFields)
        {
            // If the field is to be excluded, return empty string
            if (excludeFields[(int)field])
                return string.Empty;

            string fieldValue = null;
            switch (field)
            {
                case Field.Name:
                    fieldValue = this.Name;
                    break;
                case Field.PartName:
                    fieldValue = this.PartName;
                    break;
                case Field.PartInterface:
                    fieldValue = this.PartInterface;
                    break;
                case Field.Features:
                    fieldValue = string.Join(";", (this.Features ?? new List<KeyValuePair<string, string>>()).Select(f => $"{f.Key}={f.Value}"));
                    break;
                case Field.AreaName:
                    fieldValue = this.AreaName;
                    break;
                case Field.AreaSize:
                    fieldValue = this.AreaSize?.ToString();
                    break;

                case Field.MachineName:
                    fieldValue = this.MachineName;
                    break;
                case Field.Comment:
                    fieldValue = this.Comment;
                    break;
                case Field.Description:
                    fieldValue = this.MachineDescription;
                    break;
                case Field.Year:
                    fieldValue = this.Year;
                    break;
                case Field.Manufacturer:
                    fieldValue = this.Manufacturer;
                    break;
                case Field.Publisher:
                    fieldValue = this.Publisher;
                    break;
                case Field.RomOf:
                    fieldValue = this.RomOf;
                    break;
                case Field.CloneOf:
                    fieldValue = this.CloneOf;
                    break;
                case Field.SampleOf:
                    fieldValue = this.SampleOf;
                    break;
                case Field.Supported:
                    fieldValue = this.Supported?.ToString();
                    break;
                case Field.SourceFile:
                    fieldValue = this.SourceFile;
                    break;
                case Field.Runnable:
                    fieldValue = this.Runnable?.ToString();
                    break;
                case Field.Board:
                    fieldValue = this.Board;
                    break;
                case Field.RebuildTo:
                    fieldValue = this.RebuildTo;
                    break;
                case Field.Devices:
                    fieldValue = string.Join(";", this.Devices ?? new List<string>());
                    break;
                case Field.SlotOptions:
                    fieldValue = string.Join(";", this.SlotOptions ?? new List<string>());
                    break;
                case Field.Infos:
                    fieldValue = string.Join(";", (this.Infos ?? new List<KeyValuePair<string, string>>()).Select(i => $"{i.Key}={i.Value}"));
                    break;
                case Field.MachineType:
                    fieldValue = this.MachineType.ToString();
                    break;

                case Field.Default:
                    if (ItemType == ItemType.BiosSet)
                        fieldValue = (this as BiosSet).Default?.ToString();
                    else if (ItemType == ItemType.Release)
                        fieldValue = (this as Release).Default?.ToString();
                    break;
                case Field.BiosDescription:
                    if (ItemType == ItemType.BiosSet)
                        fieldValue = (this as BiosSet).Description;
                    break;

                case Field.MD5:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).MD5;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).MD5;
                    break;
                case Field.RIPEMD160:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).RIPEMD160;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).RIPEMD160;
                    break;
                case Field.SHA1:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).SHA1;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).SHA1;
                    break;
                case Field.SHA256:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).SHA256;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).SHA256;
                    break;
                case Field.SHA384:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).SHA384;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).SHA384;
                    break;
                case Field.SHA512:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).SHA512;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).SHA512;
                    break;
                case Field.Merge:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).MergeTag;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).MergeTag;
                    break;
                case Field.Region:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).Region;
                    else if (ItemType == ItemType.Release)
                        fieldValue = (this as Release).Region;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).Region;
                    break;
                case Field.Index:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).Index;
                    break;
                case Field.Writable:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).Writable?.ToString();
                    break;
                case Field.Optional:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).Optional?.ToString();
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).Optional?.ToString();
                    break;
                case Field.Status:
                    if (ItemType == ItemType.Disk)
                        fieldValue = (this as Disk).ItemStatus.ToString();
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).ItemStatus.ToString();
                    break;

                case Field.Language:
                    if (ItemType == ItemType.Release)
                        fieldValue = (this as Release).Language;
                    break;
                case Field.Date:
                    if (ItemType == ItemType.Release)
                        fieldValue = (this as Release).Date;
                    else if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).Date;
                    break;

                case Field.Bios:
                    if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).Bios;
                    break;
                case Field.Size:
                    if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).Size.ToString();
                    break;
                case Field.CRC:
                    if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).CRC;
                    break;
                case Field.Offset:
                    if (ItemType == ItemType.Rom)
                        fieldValue = (this as Rom).Offset;
                    break;

                case Field.NULL:
                default:
                    return string.Empty;
            }

            // Make sure we don't return null
            if (string.IsNullOrEmpty(fieldValue))
                fieldValue = string.Empty;

            return fieldValue;
        }

        #endregion

        #region Cloning Methods

        /// <summary>
        /// Clone the DatItem
        /// </summary>
        /// <returns>Clone of the DatItem</returns>
        public abstract object Clone();

        /// <summary>
        /// Copy all machine information over in one shot
        /// </summary>
        /// <param name="item">Existing item to copy information from</param>
        public void CopyMachineInformation(DatItem item)
        {
            _machine = (Machine)item._machine.Clone();
        }

        /// <summary>
        /// Copy all machine information over in one shot
        /// </summary>
        /// <param name="machine">Existing machine to copy information from</param>
        public void CopyMachineInformation(Machine machine)
        {
            _machine = (Machine)machine.Clone();
        }

        #endregion

        #region Comparision Methods

        public int CompareTo(DatItem other)
        {
            int ret = 0;

            try
            {
                if (this.Name == other.Name)
                    ret = (this.Equals(other) ? 0 : 1);

                ret = String.Compare(this.Name, other.Name);
            }
            catch
            {
                ret = 1;
            }

            return ret;
        }

        /// <summary>
        /// Determine if an item is a duplicate using partial matching logic
        /// </summary>
        /// <param name="other">DatItem to use as a baseline</param>
        /// <returns>True if the roms are duplicates, false otherwise</returns>
        public abstract bool Equals(DatItem other);

        /// <summary>
        /// Return the duplicate status of two items
        /// </summary>
        /// <param name="lastItem">DatItem to check against</param>
        /// <returns>The DupeType corresponding to the relationship between the two</returns>
        public DupeType GetDuplicateStatus(DatItem lastItem)
        {
            DupeType output = 0x00;

            // If we don't have a duplicate at all, return none
            if (!this.Equals(lastItem))
                return output;

            // If the duplicate is external already or should be, set it
            if ((lastItem.DupeType & DupeType.External) != 0 || lastItem.SystemID != this.SystemID || lastItem.SourceID != this.SourceID)
            {
                if (lastItem.MachineName == this.MachineName && lastItem.Name == this.Name)
                    output = DupeType.External | DupeType.All;
                else
                    output = DupeType.External | DupeType.Hash;
            }

            // Otherwise, it's considered an internal dupe
            else
            {
                if (lastItem.MachineName == this.MachineName && lastItem.Name == this.Name)
                    output = DupeType.Internal | DupeType.All;
                else
                    output = DupeType.Internal | DupeType.Hash;
            }

            return output;
        }

        #endregion

        #region Sorting and Merging

        /// <summary>
        /// Check if a DAT contains the given rom
        /// </summary>
        /// <param name="datdata">Dat to match against</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>True if it contains the rom, false otherwise</returns>
        public bool HasDuplicates(DatFile datdata, bool sorted = false)
        {
            // Check for an empty rom list first
            if (datdata.Count == 0)
                return false;

            // We want to get the proper key for the DatItem
            string key = SortAndGetKey(datdata, sorted);

            // If the key doesn't exist, return the empty list
            if (!datdata.Contains(key))
                return false;

            // Try to find duplicates
            List<DatItem> roms = datdata[key];
            return roms.Any(r => this.Equals(r));
        }

        /// <summary>
        /// List all duplicates found in a DAT based on a rom
        /// </summary>
        /// <param name="datdata">Dat to match against</param>
        /// <param name="remove">True to mark matched roms for removal from the input, false otherwise (default)</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>List of matched DatItem objects</returns>
        public List<DatItem> GetDuplicates(DatFile datdata, bool remove = false, bool sorted = false)
        {
            List<DatItem> output = new List<DatItem>();

            // Check for an empty rom list first
            if (datdata.Count == 0)
                return output;

            // We want to get the proper key for the DatItem
            string key = SortAndGetKey(datdata, sorted);

            // If the key doesn't exist, return the empty list
            if (!datdata.Contains(key))
                return output;

            // Try to find duplicates
            List<DatItem> roms = datdata[key];
            List<DatItem> left = new List<DatItem>();
            for (int i = 0; i < roms.Count; i++)
            {
                DatItem datItem = roms[i];

                if (this.Equals(datItem))
                {
                    datItem.Remove = true;
                    output.Add(datItem);
                }
                else
                {
                    left.Add(datItem);
                }
            }

            // If we're in removal mode, add back all roms with the proper flags
            if (remove)
            {
                datdata.Remove(key);
                datdata.AddRange(key, output);
                datdata.AddRange(key, left);
            }

            return output;
        }

        /// <summary>
        /// Sort the input DAT and get the key to be used by the item
        /// </summary>
        /// <param name="datdata">Dat to match against</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>Key to try to use</returns>
        private string SortAndGetKey(DatFile datdata, bool sorted = false)
        {
            // If we're not already sorted, take care of it
            if (!sorted)
                datdata.BucketByBestAvailable();

            // Now that we have the sorted type, we get the proper key
            return Utilities.GetKeyFromDatItem(this, datdata.SortedBy);
        }

        #endregion

        #endregion // Instance Methods

        #region Static Methods

        #region Sorting and Merging

        /// <summary>
        /// Merge an arbitrary set of ROMs based on the supplied information
        /// </summary>
        /// <param name="infiles">List of File objects representing the roms to be merged</param>
        /// <returns>A List of DatItem objects representing the merged roms</returns>
        public static List<DatItem> Merge(List<DatItem> infiles)
        {
            // Check for null or blank roms first
            if (infiles == null || infiles.Count == 0)
                return new List<DatItem>();

            // Create output list
            List<DatItem> outfiles = new List<DatItem>();

            // Then deduplicate them by checking to see if data matches previous saved roms
            int nodumpCount = 0;
            foreach (DatItem file in infiles)
            {
                // If we don't have a Rom or a Disk, we skip checking for duplicates
                if (file.ItemType != ItemType.Rom && file.ItemType != ItemType.Disk)
                    continue;

                // If it's a nodump, add and skip
                if (file.ItemType == ItemType.Rom && ((Rom)file).ItemStatus == ItemStatus.Nodump)
                {
                    outfiles.Add(file);
                    nodumpCount++;
                    continue;
                }
                else if (file.ItemType == ItemType.Disk && ((Disk)file).ItemStatus == ItemStatus.Nodump)
                {
                    outfiles.Add(file);
                    nodumpCount++;
                    continue;
                }
                // If it's the first non-nodump rom in the list, don't touch it
                else if (outfiles.Count == 0 || outfiles.Count == nodumpCount)
                {
                    outfiles.Add(file);
                    continue;
                }

                // Check if the rom is a duplicate
                DupeType dupetype = 0x00;
                DatItem saveditem = new Rom();
                int pos = -1;
                for (int i = 0; i < outfiles.Count; i++)
                {
                    DatItem lastrom = outfiles[i];

                    // Get the duplicate status
                    dupetype = file.GetDuplicateStatus(lastrom);

                    // If it's a duplicate, skip adding it to the output but add any missing information
                    if (dupetype != 0x00)
                    {
                        saveditem = lastrom;
                        pos = i;

                        // Roms have more infomration to save
                        if (file.ItemType == ItemType.Rom)
                        {
                            ((Rom)saveditem).Size = (((Rom)saveditem).Size == -1 && ((Rom)file).Size != -1
                                ? ((Rom)file).Size
                                : ((Rom)saveditem).Size);
                            ((Rom)saveditem).CRC = (string.IsNullOrWhiteSpace(((Rom)saveditem).CRC) && !string.IsNullOrWhiteSpace(((Rom)file).CRC)
                                ? ((Rom)file).CRC
                                : ((Rom)saveditem).CRC);
                            ((Rom)saveditem).MD5 = (string.IsNullOrWhiteSpace(((Rom)saveditem).MD5) && !string.IsNullOrWhiteSpace(((Rom)file).MD5)
                                ? ((Rom)file).MD5
                                : ((Rom)saveditem).MD5);
                            ((Rom)saveditem).RIPEMD160 = (string.IsNullOrWhiteSpace(((Rom)saveditem).RIPEMD160) && !string.IsNullOrWhiteSpace(((Rom)file).RIPEMD160)
                                ? ((Rom)file).RIPEMD160
                                : ((Rom)saveditem).RIPEMD160);
                            ((Rom)saveditem).SHA1 = (string.IsNullOrWhiteSpace(((Rom)saveditem).SHA1) && !string.IsNullOrWhiteSpace(((Rom)file).SHA1)
                                ? ((Rom)file).SHA1
                                : ((Rom)saveditem).SHA1);
                            ((Rom)saveditem).SHA256 = (string.IsNullOrWhiteSpace(((Rom)saveditem).SHA256) && !string.IsNullOrWhiteSpace(((Rom)file).SHA256)
                                ? ((Rom)file).SHA256
                                : ((Rom)saveditem).SHA256);
                            ((Rom)saveditem).SHA384 = (string.IsNullOrWhiteSpace(((Rom)saveditem).SHA384) && !string.IsNullOrWhiteSpace(((Rom)file).SHA384)
                                ? ((Rom)file).SHA384
                                : ((Rom)saveditem).SHA384);
                            ((Rom)saveditem).SHA512 = (string.IsNullOrWhiteSpace(((Rom)saveditem).SHA512) && !string.IsNullOrWhiteSpace(((Rom)file).SHA512)
                                ? ((Rom)file).SHA512
                                : ((Rom)saveditem).SHA512);
                        }
                        else if (file.ItemType == ItemType.Disk)
                        {
                            ((Disk)saveditem).MD5 = (string.IsNullOrWhiteSpace(((Disk)saveditem).MD5) && !string.IsNullOrWhiteSpace(((Disk)file).MD5)
                                ? ((Disk)file).MD5
                                : ((Disk)saveditem).MD5);
                            ((Disk)saveditem).RIPEMD160 = (string.IsNullOrWhiteSpace(((Disk)saveditem).RIPEMD160) && !string.IsNullOrWhiteSpace(((Disk)file).RIPEMD160)
                                ? ((Disk)file).RIPEMD160
                                : ((Disk)saveditem).RIPEMD160);
                            ((Disk)saveditem).SHA1 = (string.IsNullOrWhiteSpace(((Disk)saveditem).SHA1) && !string.IsNullOrWhiteSpace(((Disk)file).SHA1)
                                ? ((Disk)file).SHA1
                                : ((Disk)saveditem).SHA1);
                            ((Disk)saveditem).SHA256 = (string.IsNullOrWhiteSpace(((Disk)saveditem).SHA256) && !string.IsNullOrWhiteSpace(((Disk)file).SHA256)
                                ? ((Disk)file).SHA256
                                : ((Disk)saveditem).SHA256);
                            ((Disk)saveditem).SHA384 = (string.IsNullOrWhiteSpace(((Disk)saveditem).SHA384) && !string.IsNullOrWhiteSpace(((Disk)file).SHA384)
                                ? ((Disk)file).SHA384
                                : ((Disk)saveditem).SHA384);
                            ((Disk)saveditem).SHA512 = (string.IsNullOrWhiteSpace(((Disk)saveditem).SHA512) && !string.IsNullOrWhiteSpace(((Disk)file).SHA512)
                                ? ((Disk)file).SHA512
                                : ((Disk)saveditem).SHA512);
                        }

                        saveditem.DupeType = dupetype;

                        // If the current system has a lower ID than the previous, set the system accordingly
                        if (file.SystemID < saveditem.SystemID)
                        {
                            saveditem.SystemID = file.SystemID;
                            saveditem.System = file.System;
                            saveditem.CopyMachineInformation(file);
                            saveditem.Name = file.Name;
                        }

                        // If the current source has a lower ID than the previous, set the source accordingly
                        if (file.SourceID < saveditem.SourceID)
                        {
                            saveditem.SourceID = file.SourceID;
                            saveditem.Source = file.Source;
                            saveditem.CopyMachineInformation(file);
                            saveditem.Name = file.Name;
                        }

                        // If the current machine is a child of the new machine, use the new machine instead
                        if (saveditem.CloneOf == file.MachineName || saveditem.RomOf == file.MachineName)
                        {
                            saveditem.CopyMachineInformation(file);
                            saveditem.Name = file.Name;
                        }

                        break;
                    }
                }

                // If no duplicate is found, add it to the list
                if (dupetype == 0x00)
                {
                    outfiles.Add(file);
                }
                // Otherwise, if a new rom information is found, add that
                else
                {
                    outfiles.RemoveAt(pos);
                    outfiles.Insert(pos, saveditem);
                }
            }

            // Then return the result
            return outfiles;
        }

        /// <summary>
        /// Resolve name duplicates in an arbitrary set of ROMs based on the supplied information
        /// </summary>
        /// <param name="infiles">List of File objects representing the roms to be merged</param>
        /// <returns>A List of DatItem objects representing the renamed roms</returns>
        public static List<DatItem> ResolveNames(List<DatItem> infiles)
        {
            // Create the output list
            List<DatItem> output = new List<DatItem>();

            // First we want to make sure the list is in alphabetical order
            Sort(ref infiles, true);

            // Now we want to loop through and check names
            DatItem lastItem = null;
            string lastrenamed = null;
            int lastid = 0;
            for (int i = 0; i < infiles.Count; i++)
            {
                DatItem datItem = infiles[i];

                // If we have the first item, we automatically add it
                if (lastItem == null)
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    continue;
                }

                // If the current item exactly matches the last item, then we don't add it
                if ((datItem.GetDuplicateStatus(lastItem) & DupeType.All) != 0)
                {
                    Globals.Logger.Verbose($"Exact duplicate found for '{datItem.Name}'");
                    continue;
                }

                // If the current name matches the previous name, rename the current item
                else if (datItem.Name == lastItem.Name)
                {
                    Globals.Logger.Verbose($"Name duplicate found for '{datItem.Name}'");

                    if (datItem.ItemType == ItemType.Disk || datItem.ItemType == ItemType.Rom)
                    {
                        datItem.Name += GetDuplicateSuffix(datItem);
                        lastrenamed = lastrenamed ?? datItem.Name;
                    }

                    // If we have a conflict with the last renamed item, do the right thing
                    if (datItem.Name == lastrenamed)
                    {
                        lastrenamed = datItem.Name;
                        datItem.Name += (lastid == 0 ? string.Empty : "_" + lastid);
                        lastid++;
                    }
                    // If we have no conflict, then we want to reset the lastrenamed and id
                    else
                    {
                        lastrenamed = null;
                        lastid = 0;
                    }

                    output.Add(datItem);
                }

                // Otherwise, we say that we have a valid named file
                else
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    lastrenamed = null;
                    lastid = 0;
                }
            }

            // One last sort to make sure this is ordered
            Sort(ref output, true);

            return output;
        }

        /// <summary>
        /// Get duplicate suffix based on the item type
        /// </summary>
        private static string GetDuplicateSuffix(DatItem datItem)
        {
            if (datItem.ItemType == ItemType.Disk)
            {
                Disk disk = datItem as Disk;

                if (string.IsNullOrWhiteSpace(disk.MD5))
                    return $"_{disk.MD5}";
                else if (string.IsNullOrWhiteSpace(disk.SHA1))
                    return $"_{disk.SHA1}";
                else
                    return "_1";
            }
            else if (datItem.ItemType == ItemType.Rom)
            {
                Rom rom = datItem as Rom;

                if (string.IsNullOrWhiteSpace(rom.CRC))
                    return $"_{rom.CRC}";
                else if (string.IsNullOrWhiteSpace(rom.MD5))
                    return $"_{rom.MD5}";
                else if (string.IsNullOrWhiteSpace(rom.SHA1))
                    return $"_{rom.SHA1}";
                else
                    return "_1";
            }

            return "_1";
        }

        /// <summary>
        /// Sort a list of File objects by SystemID, SourceID, Game, and Name (in order)
        /// </summary>
        /// <param name="roms">List of File objects representing the roms to be sorted</param>
        /// <param name="norename">True if files are not renamed, false otherwise</param>
        /// <returns>True if it sorted correctly, false otherwise</returns>
        public static bool Sort(ref List<DatItem> roms, bool norename)
        {
            roms.Sort(delegate (DatItem x, DatItem y)
            {
                try
                {
                    NaturalComparer nc = new NaturalComparer();
                    if (x.SystemID == y.SystemID)
                    {
                        if (x.SourceID == y.SourceID)
                        {
                            if (x.MachineName == y.MachineName)
                            {
                                if ((x.ItemType == ItemType.Rom || x.ItemType == ItemType.Disk) && (y.ItemType == ItemType.Rom || y.ItemType == ItemType.Disk))
                                {
                                    if (Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(x.Name)) == Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(y.Name)))
                                    {
                                        return nc.Compare(Path.GetFileName(Utilities.RemovePathUnsafeCharacters(x.Name)), Path.GetFileName(Utilities.RemovePathUnsafeCharacters(y.Name)));
                                    }

                                    return nc.Compare(Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(x.Name)), Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(y.Name)));
                                }
                                else if ((x.ItemType == ItemType.Rom || x.ItemType == ItemType.Disk) && (y.ItemType != ItemType.Rom && y.ItemType != ItemType.Disk))
                                {
                                    return -1;
                                }
                                else if ((x.ItemType != ItemType.Rom && x.ItemType != ItemType.Disk) && (y.ItemType == ItemType.Rom || y.ItemType == ItemType.Disk))
                                {
                                    return 1;
                                }
                                else
                                {
                                    if (Path.GetDirectoryName(x.Name) == Path.GetDirectoryName(y.Name))
                                    {
                                        return nc.Compare(Path.GetFileName(x.Name), Path.GetFileName(y.Name));
                                    }

                                    return nc.Compare(Path.GetDirectoryName(x.Name), Path.GetDirectoryName(y.Name));
                                }
                            }

                            return nc.Compare(x.MachineName, y.MachineName);
                        }

                        return (norename ? nc.Compare(x.MachineName, y.MachineName) : x.SourceID - y.SourceID);
                    }

                    return (norename ? nc.Compare(x.MachineName, y.MachineName) : x.SystemID - y.SystemID);
                }
                catch (Exception)
                {
                    // Absorb the error
                    return 0;
                }
            });

            return true;
        }

        #endregion

        #endregion // Static Methods
    }
}
