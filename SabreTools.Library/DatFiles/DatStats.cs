using System;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents statistical data associated with a DAT
    /// </summary>
    public class DatStats
    {
        #region Private instance variables

        // Object used to lock stats updates
        private object _lockObject = new object();

        #endregion

        #region Publicly facing variables

        // Statistics report format
        public StatReportFormat ReportFormat { get; set; } = StatReportFormat.None;

        // Overall item count
        public long Count { get; set; } = 0;

        // Individual DatItem type counts
        public long ArchiveCount { get; set; } = 0;
        public long BiosSetCount { get; set; } = 0;
        public long DiskCount { get; set; } = 0;
        public long ReleaseCount { get; set; } = 0;
        public long RomCount { get; set; } = 0;
        public long SampleCount { get; set; } = 0;

        // Special count only used by statistics output
        public long GameCount { get; set; } = 0;

        // Total reported size
        public long TotalSize { get; set; } = 0;

        // Individual hash counts
        public long CRCCount { get; set; } = 0;
        public long MD5Count { get; set; } = 0;
        public long SHA1Count { get; set; } = 0;
        public long SHA256Count { get; set; } = 0;
        public long SHA384Count { get; set; } = 0;
        public long SHA512Count { get; set; } = 0;

        // Individual status counts
        public long BaddumpCount { get; set; } = 0;
        public long GoodCount { get; set; } = 0;
        public long NodumpCount { get; set; } = 0;
        public long VerifiedCount { get; set; } = 0;

        #endregion

        #region Instance Methods

        /// <summary>
        /// Add to the statistics given a DatItem
        /// </summary>
        /// <param name="item">Item to add info from</param>
        public void AddItem(DatItem item)
        {
            // No matter what the item is, we increate the count
            lock (_lockObject)
            {
                this.Count += 1;

                // Now we do different things for each item type

                switch (item.Type)
                {
                    case ItemType.Archive:
                        this.ArchiveCount += 1;
                        break;
                    case ItemType.BiosSet:
                        this.BiosSetCount += 1;
                        break;
                    case ItemType.Disk:
                        this.DiskCount += 1;
                        if (((Disk)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.MD5Count += (String.IsNullOrWhiteSpace(((Disk)item).MD5) ? 0 : 1);
                            this.SHA1Count += (String.IsNullOrWhiteSpace(((Disk)item).SHA1) ? 0 : 1);
                            this.SHA256Count += (String.IsNullOrWhiteSpace(((Disk)item).SHA256) ? 0 : 1);
                            this.SHA384Count += (String.IsNullOrWhiteSpace(((Disk)item).SHA384) ? 0 : 1);
                            this.SHA512Count += (String.IsNullOrWhiteSpace(((Disk)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount += (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount += (((Disk)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount += (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount += (((Disk)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Release:
                        this.ReleaseCount += 1;
                        break;
                    case ItemType.Rom:
                        this.RomCount += 1;
                        if (((Rom)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.TotalSize += ((Rom)item).Size;
                            this.CRCCount += (String.IsNullOrWhiteSpace(((Rom)item).CRC) ? 0 : 1);
                            this.MD5Count += (String.IsNullOrWhiteSpace(((Rom)item).MD5) ? 0 : 1);
                            this.SHA1Count += (String.IsNullOrWhiteSpace(((Rom)item).SHA1) ? 0 : 1);
                            this.SHA256Count += (String.IsNullOrWhiteSpace(((Rom)item).SHA256) ? 0 : 1);
                            this.SHA384Count += (String.IsNullOrWhiteSpace(((Rom)item).SHA384) ? 0 : 1);
                            this.SHA512Count += (String.IsNullOrWhiteSpace(((Rom)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount += (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount += (((Rom)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount += (((Rom)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Sample:
                        this.SampleCount += 1;
                        break;
                }
            }
        }

        /// <summary>
        /// Add statistics from another DatStats object
        /// </summary>
        /// <param name="stats">DatStats object to add from</param>
        public void AddStats(DatStats stats)
        {
            this.Count += stats.Count;

            this.ArchiveCount += stats.ArchiveCount;
            this.BiosSetCount += stats.BiosSetCount;
            this.DiskCount += stats.DiskCount;
            this.ReleaseCount += stats.ReleaseCount;
            this.RomCount += stats.RomCount;
            this.SampleCount += stats.SampleCount;

            this.GameCount += stats.GameCount;

            this.TotalSize += stats.TotalSize;

            // Individual hash counts
            this.CRCCount += stats.CRCCount;
            this.MD5Count += stats.MD5Count;
            this.SHA1Count += stats.SHA1Count;
            this.SHA256Count += stats.SHA256Count;
            this.SHA384Count += stats.SHA384Count;
            this.SHA512Count += stats.SHA512Count;

            // Individual status counts
            this.BaddumpCount += stats.BaddumpCount;
            this.GoodCount += stats.GoodCount;
            this.NodumpCount += stats.NodumpCount;
            this.VerifiedCount += stats.VerifiedCount;
    }

        /// <summary>
        /// Remove from the statistics given a DatItem
        /// </summary>
        /// <param name="item">Item to remove info for</param>
        public void RemoveItem(DatItem item)
        {
            // No matter what the item is, we increate the count
            lock (_lockObject)
            {
                this.Count -= 1;

                // Now we do different things for each item type

                switch (item.Type)
                {
                    case ItemType.Archive:
                        this.ArchiveCount -= 1;
                        break;
                    case ItemType.BiosSet:
                        this.BiosSetCount -= 1;
                        break;
                    case ItemType.Disk:
                        this.DiskCount -= 1;
                        if (((Disk)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.MD5Count -= (String.IsNullOrWhiteSpace(((Disk)item).MD5) ? 0 : 1);
                            this.SHA1Count -= (String.IsNullOrWhiteSpace(((Disk)item).SHA1) ? 0 : 1);
                            this.SHA256Count -= (String.IsNullOrWhiteSpace(((Disk)item).SHA256) ? 0 : 1);
                            this.SHA384Count -= (String.IsNullOrWhiteSpace(((Disk)item).SHA384) ? 0 : 1);
                            this.SHA512Count -= (String.IsNullOrWhiteSpace(((Disk)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount -= (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount -= (((Disk)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount -= (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount -= (((Disk)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Release:
                        this.ReleaseCount -= 1;
                        break;
                    case ItemType.Rom:
                        this.RomCount -= 1;
                        if (((Rom)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.TotalSize -= ((Rom)item).Size;
                            this.CRCCount -= (String.IsNullOrWhiteSpace(((Rom)item).CRC) ? 0 : 1);
                            this.MD5Count -= (String.IsNullOrWhiteSpace(((Rom)item).MD5) ? 0 : 1);
                            this.SHA1Count -= (String.IsNullOrWhiteSpace(((Rom)item).SHA1) ? 0 : 1);
                            this.SHA256Count -= (String.IsNullOrWhiteSpace(((Rom)item).SHA256) ? 0 : 1);
                            this.SHA384Count -= (String.IsNullOrWhiteSpace(((Rom)item).SHA384) ? 0 : 1);
                            this.SHA512Count -= (String.IsNullOrWhiteSpace(((Rom)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount -= (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount -= (((Rom)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount -= (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount -= (((Rom)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Sample:
                        this.SampleCount -= 1;
                        break;
                }
            }
        }

        /// <summary>
        /// Reset all statistics
        /// </summary>
        public void Reset()
        {
            this.Count = 0;

            this.ArchiveCount = 0;
            this.BiosSetCount = 0;
            this.DiskCount = 0;
            this.ReleaseCount = 0;
            this.RomCount = 0;
            this.SampleCount = 0;

            this.GameCount = 0;

            this.TotalSize = 0;

            this.CRCCount = 0;
            this.MD5Count = 0;
            this.SHA1Count = 0;
            this.SHA256Count = 0;
            this.SHA384Count = 0;
            this.SHA512Count = 0;

            this.BaddumpCount = 0;
            this.GoodCount = 0;
            this.NodumpCount = 0;
            this.VerifiedCount = 0;
        }

        #endregion // Instance Methods
    }
}
