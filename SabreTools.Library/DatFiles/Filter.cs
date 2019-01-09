using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SabreTools.Library.Data;
using SabreTools.Library.DatItems;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents the filtering operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    public class Filter
    {
        #region Pubically facing variables

        #region Positive

        public List<string> MachineNames { get; set; } = new List<string>();
        public List<string> MachineDescriptions { get; set; } = new List<string>();
        public List<string> ItemNames { get; set; } = new List<string>();
        public List<string> ItemTypes { get; set; } = new List<string>();
        public List<string> CRCs { get; set; } = new List<string>();
        public List<string> MD5s { get; set; } = new List<string>();
        public List<string> SHA1s { get; set; } = new List<string>();
        public List<string> SHA256s { get; set; } = new List<string>();
        public List<string> SHA384s { get; set; } = new List<string>();
        public List<string> SHA512s { get; set; } = new List<string>();
        public ItemStatus ItemStatuses { get; set; } = ItemStatus.NULL;
        public MachineType MachineTypes { get; set; } = MachineType.NULL;

        #endregion

        #region Negative

        public List<string> NotMachineNames { get; set; } = new List<string>();
        public List<string> NotMachineDescriptions { get; set; } = new List<string>();
        public List<string> NotItemNames { get; set; } = new List<string>();
        public List<string> NotItemTypes { get; set; } = new List<string>();
        public List<string> NotCRCs { get; set; } = new List<string>();
        public List<string> NotMD5s { get; set; } = new List<string>();
        public List<string> NotSHA1s { get; set; } = new List<string>();
        public List<string> NotSHA256s { get; set; } = new List<string>();
        public List<string> NotSHA384s { get; set; } = new List<string>();
        public List<string> NotSHA512s { get; set; } = new List<string>();
        public ItemStatus NotItemStatuses { get; set; } = ItemStatus.NULL;
        public MachineType NotMachineTypes { get; set; } = MachineType.NULL;

        #endregion

        #region Neutral

        public long SizeGreaterThanOrEqual { get; set; } = -1;
        public long SizeLessThanOrEqual { get; set; } = -1;
        public long SizeEqualTo { get; set; } = -1;
        public bool IncludeOfInGame { get; set; } = false;
        public bool? Runnable { get; set; } = null;
        public bool Single { get; set; } = false;
        public bool Trim { get; set; } = false;
        public string Root { get; set; } = null;

        #endregion

        #endregion // Pubically facing variables

        #region Instance methods

        /// <summary>
        /// Filter a DatFile using the inputs
        /// </summary>
        /// <param name="datFile"></param>
        /// <returns>True if the DatFile was filtered, false on error</returns>
        public bool FilterDatFile(DatFile datFile)
        {
            try
            {
                // Loop over every key in the dictionary
                List<string> keys = datFile.Keys;
                foreach (string key in keys)
                {
                    // For every item in the current key
                    List<DatItem> items = datFile[key];
                    List<DatItem> newitems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // If the rom passes the filter, include it
                        if (ItemPasses(item))
                        {
                            // If we are in single game mode, rename all games
                            if (this.Single)
                            {
                                item.MachineName = "!";
                            }

                            // If we are in NTFS trim mode, trim the game name
                            if (this.Trim)
                            {
                                // Windows max name length is 260
                                int usableLength = 260 - item.MachineName.Length - this.Root.Length;
                                if (item.Name.Length > usableLength)
                                {
                                    string ext = Path.GetExtension(item.Name);
                                    item.Name = item.Name.Substring(0, usableLength - ext.Length);
                                    item.Name += ext;
                                }
                            }

                            // Lock the list and add the item back
                            lock (newitems)
                            {
                                newitems.Add(item);
                            }
                        }
                    }

                    datFile.Remove(key);
                    datFile.AddRange(key, newitems);
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check to see if a DatItem passes the filter
        /// </summary>
        /// <param name="item">DatItem to check</param>
        /// <returns>True if the file passed the filter, false otherwise</returns>
        public bool ItemPasses(DatItem item)
        {
            // If the item is null, we automatically fail it
            if (item == null)
            {
                return false;
            }

            // Filter on machine type
            if (this.MachineTypes != MachineType.NULL && (item.MachineType & this.MachineTypes) == 0)
            {
                return false;
            }
            if (this.NotMachineTypes != MachineType.NULL && (item.MachineType & this.NotMachineTypes) != 0)
            {
                return false;
            }

            // Filter on machine runability
            if (this.Runnable != null && item.Runnable != this.Runnable)
            {
                return false;
            }

            // Take care of Rom and Disk specific differences
            if (item.ItemType == ItemType.Rom)
            {
                Rom rom = (Rom)item;

                // Filter on status
                if (this.ItemStatuses != ItemStatus.NULL && (rom.ItemStatus & this.ItemStatuses) == 0)
                {
                    return false;
                }
                if (this.NotItemStatuses != ItemStatus.NULL && (rom.ItemStatus & this.NotItemStatuses) != 0)
                {
                    return false;
                }

                // Filter on rom size
                if (this.SizeEqualTo != -1 && rom.Size != this.SizeEqualTo)
                {
                    return false;
                }
                else
                {
                    if (this.SizeGreaterThanOrEqual != -1 && rom.Size < this.SizeGreaterThanOrEqual)
                    {
                        return false;
                    }
                    if (this.SizeLessThanOrEqual != -1 && rom.Size > this.SizeLessThanOrEqual)
                    {
                        return false;
                    }
                }

                // Filter on CRC
                if (this.CRCs.Count > 0)
                {
                    // If the CRC isn't in the list, return false
                    if (!FindValueInList(this.CRCs, rom.CRC))
                    {
                        return false;
                    }
                }
                if (this.NotCRCs.Count > 0)
                {
                    // If the CRC is in the list, return false
                    if (FindValueInList(this.NotCRCs, rom.CRC))
                    {
                        return false;
                    }
                }

                // Filter on MD5
                if (this.MD5s.Count > 0)
                {
                    // If the MD5 isn't in the list, return false
                    if (!FindValueInList(this.MD5s, rom.MD5))
                    {
                        return false;
                    }
                }
                if (this.NotMD5s.Count > 0)
                {
                    // If the MD5 is in the list, return false
                    if (FindValueInList(this.NotMD5s, rom.MD5))
                    {
                        return false;
                    }
                }

                // Filter on SHA-1
                if (this.SHA1s.Count > 0)
                {
                    // If the SHA-1 isn't in the list, return false
                    if (!FindValueInList(this.SHA1s, rom.SHA1))
                    {
                        return false;
                    }
                }
                if (this.NotSHA1s.Count > 0)
                {
                    // If the SHA-1 is in the list, return false
                    if (FindValueInList(this.NotSHA1s, rom.SHA1))
                    {
                        return false;
                    }
                }

                // Filter on SHA-256
                if (this.SHA256s.Count > 0)
                {
                    // If the SHA-256 isn't in the list, return false
                    if (!FindValueInList(this.SHA256s, rom.SHA256))
                    {
                        return false;
                    }
                }
                if (this.NotSHA256s.Count > 0)
                {
                    // If the SHA-256 is in the list, return false
                    if (FindValueInList(this.NotSHA256s, rom.SHA256))
                    {
                        return false;
                    }
                }

                // Filter on SHA-384
                if (this.SHA384s.Count > 0)
                {
                    // If the SHA-384 isn't in the list, return false
                    if (!FindValueInList(this.SHA384s, rom.SHA384))
                    {
                        return false;
                    }
                }
                if (this.NotSHA384s.Count > 0)
                {
                    // If the SHA-384 is in the list, return false
                    if (FindValueInList(this.NotSHA384s, rom.SHA384))
                    {
                        return false;
                    }
                }

                // Filter on SHA-512
                if (this.SHA512s.Count > 0)
                {
                    // If the SHA-512 isn't in the list, return false
                    if (!FindValueInList(this.SHA512s, rom.SHA512))
                    {
                        return false;
                    }
                }
                if (this.NotSHA512s.Count > 0)
                {
                    // If the SHA-512 is in the list, return false
                    if (FindValueInList(this.NotSHA512s, rom.SHA512))
                    {
                        return false;
                    }
                }
            }
            else if (item.ItemType == ItemType.Disk)
            {
                Disk rom = (Disk)item;

                // Filter on status
                if (this.ItemStatuses != ItemStatus.NULL && (rom.ItemStatus & this.ItemStatuses) == 0)
                {
                    return false;
                }
                if (this.NotItemStatuses != ItemStatus.NULL && (rom.ItemStatus & this.NotItemStatuses) != 0)
                {
                    return false;
                }

                // Filter on MD5
                if (this.MD5s.Count > 0)
                {
                    // If the MD5 isn't in the list, return false
                    if (!FindValueInList(this.MD5s, rom.MD5))
                    {
                        return false;
                    }
                }
                if (this.NotMD5s.Count > 0)
                {
                    // If the MD5 is in the list, return false
                    if (FindValueInList(this.NotMD5s, rom.MD5))
                    {
                        return false;
                    }
                }

                // Filter on SHA-1
                if (this.SHA1s.Count > 0)
                {
                    // If the SHA-1 isn't in the list, return false
                    if (!FindValueInList(this.SHA1s, rom.SHA1))
                    {
                        return false;
                    }
                }
                if (this.NotSHA1s.Count > 0)
                {
                    // If the SHA-1 is in the list, return false
                    if (FindValueInList(this.NotSHA1s, rom.SHA1))
                    {
                        return false;
                    }
                }

                // Filter on SHA-256
                if (this.SHA256s.Count > 0)
                {
                    // If the SHA-256 isn't in the list, return false
                    if (!FindValueInList(this.SHA256s, rom.SHA256))
                    {
                        return false;
                    }
                }
                if (this.NotSHA256s.Count > 0)
                {
                    // If the SHA-256 is in the list, return false
                    if (FindValueInList(this.NotSHA256s, rom.SHA256))
                    {
                        return false;
                    }
                }

                // Filter on SHA-384
                if (this.SHA384s.Count > 0)
                {
                    // If the SHA-384 isn't in the list, return false
                    if (!FindValueInList(this.SHA384s, rom.SHA384))
                    {
                        return false;
                    }
                }
                if (this.NotSHA384s.Count > 0)
                {
                    // If the SHA-384 is in the list, return false
                    if (FindValueInList(this.NotSHA384s, rom.SHA384))
                    {
                        return false;
                    }
                }

                // Filter on SHA-512
                if (this.SHA512s.Count > 0)
                {
                    // If the SHA-512 isn't in the list, return false
                    if (!FindValueInList(this.SHA512s, rom.SHA512))
                    {
                        return false;
                    }
                }
                if (this.NotSHA512s.Count > 0)
                {
                    // If the SHA-512 is in the list, return false
                    if (FindValueInList(this.NotSHA512s, rom.SHA512))
                    {
                        return false;
                    }
                }
            }

            // Filter on machine name
            if (this.MachineNames.Count > 0)
            {
                bool found = FindValueInList(this.MachineNames, item.MachineName);

                // If we are checking CloneOf and RomOf, add them in as well
                if (this.IncludeOfInGame)
                {
                    found |= FindValueInList(this.MachineNames, item.CloneOf);
                    found |= FindValueInList(this.MachineNames, item.RomOf);
                }

                // If the game name was not found in the list, return false
                if (!found)
                {
                    return false;
                }
            }
            if (this.NotMachineNames.Count > 0)
            {
                bool found = FindValueInList(this.NotMachineNames, item.MachineName);

                // If we are checking CloneOf and RomOf, add them in as well
                if (this.IncludeOfInGame)
                {
                    found |= FindValueInList(this.NotMachineNames, item.CloneOf);
                    found |= FindValueInList(this.NotMachineNames, item.RomOf);
                }

                // If the machine name was found in the list, return false
                if (found)
                {
                    return false;
                }
            }

            // Filter on machine description
            if (this.MachineDescriptions.Count > 0)
            {
                bool found = FindValueInList(this.MachineDescriptions, item.MachineDescription);

                // If the machine description was not found in the list, return false
                if (!found)
                {
                    return false;
                }
            }
            if (this.NotMachineDescriptions.Count > 0)
            {
                bool found = FindValueInList(this.NotMachineDescriptions, item.MachineDescription);

                // If the machine description was found in the list, return false
                if (found)
                {
                    return false;
                }
            }

            // Filter on item name
            if (this.ItemNames.Count > 0)
            {
                // If the item name was not found in the list, return false
                if (!FindValueInList(this.ItemNames, item.Name))
                {
                    return false;
                }
            }
            if (this.NotItemNames.Count > 0)
            {
                // If the item name was found in the list, return false
                if (FindValueInList(this.NotItemNames, item.Name))
                {
                    return false;
                }
            }

            // Filter on item type
            if (this.ItemTypes.Count == 0 && this.NotItemTypes.Count == 0 && item.ItemType != ItemType.Rom && item.ItemType != ItemType.Disk && item.ItemType != ItemType.Blank)
            {
                return false;
            }
            if (this.ItemTypes.Count > 0)
            {
                // If the item type was not found in the list, return false
                if (!FindValueInList(this.ItemTypes, item.ItemType.ToString()))
                {
                    return false;
                }
            }
            if (this.NotItemTypes.Count > 0)
            {
                // If the item type was found in the list, return false
                if (FindValueInList(this.NotItemTypes, item.ItemType.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generic code to check if a specific value is in the list given
        /// </summary>
        /// <param name="haystack">List to search for the value in</param>
        /// <param name="needle">Value to search the list for</param>
        /// <returns>True if the value could be found, false otherwise</returns>
        private bool FindValueInList(List<string> haystack, string needle)
        {
            bool found = false;
            foreach (string straw in haystack)
            {
                if (!String.IsNullOrWhiteSpace(straw))
                {
                    string regexStraw = straw;

                    // If the straw has no special characters at all, treat it as an exact match
                    if (regexStraw == Regex.Escape(regexStraw))
                    {
                        regexStraw = "^" + regexStraw + "$";
                    }

                    // Check if a match is found with the regex
                    found |= Regex.IsMatch(needle, regexStraw, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
            }

            return found;
        }

        #endregion
    }
}
