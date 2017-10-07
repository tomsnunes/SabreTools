using System;

using SabreTools.Library.Data;
using SabreTools.Library.Items;

namespace SabreTools.Library.DatFiles
{
	public class DatStats
	{
		#region Private instance variables

		// Object used to lock stats updates
		private object _lockObject = new object();

		// Overall item count
		private long _count = 0;

		// Individual DatItem type counts
		private long _archiveCount = 0;
		private long _biosSetCount = 0;
		private long _diskCount = 0;
		private long _releaseCount = 0;
		private long _romCount = 0;
		private long _sampleCount = 0;

		// Total reported size
		private long _totalSize = 0;

		// Individual hash counts
		private long _crcCount = 0;
		private long _md5Count = 0;
		private long _sha1Count = 0;
		private long _sha256Count = 0;
		private long _sha384Count = 0;
		private long _sha512Count = 0;

		// Individual status counts
		private long _baddumpCount = 0;
		private long _goodCount = 0;
		private long _nodumpCount = 0;
		private long _verifiedCount = 0;

		#endregion

		#region Publicly facing variables

		// Overall item count
		public long Count
		{
			get { return _count; }
			set { _count = value; }
		}

		// Individual DatItem type counts
		public long ArchiveCount
		{
			get { return _archiveCount; }
			set { _archiveCount = value; }
		}
		public long BiosSetCount
		{
			get { return _biosSetCount; }
			set { _biosSetCount = value; }
		}
		public long DiskCount
		{
			get { return _diskCount; }
			set { _diskCount = value; }
		}
		public long ReleaseCount
		{
			get { return _releaseCount; }
			set { _releaseCount = value; }
		}
		public long RomCount
		{
			get { return _romCount; }
			set { _romCount = value; }
		}
		public long SampleCount
		{
			get { return _sampleCount; }
			set { _sampleCount = value; }
		}

		// Total reported size
		public long TotalSize
		{
			get { return _totalSize; }
			set { _totalSize = value; }
		}

		// Individual hash counts
		public long CRCCount
		{
			get { return _crcCount; }
			set { _crcCount = value; }
		}
		public long MD5Count
		{
			get { return _md5Count; }
			set { _md5Count = value; }
		}
		public long SHA1Count
		{
			get { return _sha1Count; }
			set { _sha1Count = value; }
		}
		public long SHA256Count
		{
			get { return _sha256Count; }
			set { _sha256Count = value; }
		}
		public long SHA384Count
		{
			get { return _sha384Count; }
			set { _sha384Count = value; }
		}
		public long SHA512Count
		{
			get { return _sha512Count; }
			set { _sha512Count = value; }
		}

		// Individual status counts
		public long BaddumpCount
		{
			get { return _baddumpCount; }
			set { _baddumpCount = value; }
		}
		public long GoodCount
		{
			get { return _goodCount; }
			set { _goodCount = value; }
		}
		public long NodumpCount
		{
			get { return _nodumpCount; }
			set { _nodumpCount = value; }
		}
		public long VerifiedCount
		{
			get { return _verifiedCount; }
			set { _verifiedCount = value; }
		}

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
				_count += 1;

				// Now we do different things for each item type

				switch (item.Type)
				{
					case ItemType.Archive:
						_archiveCount += 1;
						break;
					case ItemType.BiosSet:
						_biosSetCount += 1;
						break;
					case ItemType.Disk:
						_diskCount += 1;
						if (((Disk)item).ItemStatus != ItemStatus.Nodump)
						{
							_md5Count += (String.IsNullOrEmpty(((Disk)item).MD5) ? 0 : 1);
							_sha1Count += (String.IsNullOrEmpty(((Disk)item).SHA1) ? 0 : 1);
							_sha256Count += (String.IsNullOrEmpty(((Disk)item).SHA256) ? 0 : 1);
							_sha384Count += (String.IsNullOrEmpty(((Disk)item).SHA384) ? 0 : 1);
							_sha512Count += (String.IsNullOrEmpty(((Disk)item).SHA512) ? 0 : 1);
						}

						_baddumpCount += (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_goodCount += (((Disk)item).ItemStatus == ItemStatus.Good ? 1 : 0);
						_nodumpCount += (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						_verifiedCount += (((Disk)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
						break;
					case ItemType.Release:
						_releaseCount += 1;
						break;
					case ItemType.Rom:
						_romCount += 1;
						if (((Rom)item).ItemStatus != ItemStatus.Nodump)
						{
							_totalSize += ((Rom)item).Size;
							_crcCount += (String.IsNullOrEmpty(((Rom)item).CRC) ? 0 : 1);
							_md5Count += (String.IsNullOrEmpty(((Rom)item).MD5) ? 0 : 1);
							_sha1Count += (String.IsNullOrEmpty(((Rom)item).SHA1) ? 0 : 1);
							_sha256Count += (String.IsNullOrEmpty(((Rom)item).SHA256) ? 0 : 1);
							_sha384Count += (String.IsNullOrEmpty(((Rom)item).SHA384) ? 0 : 1);
							_sha512Count += (String.IsNullOrEmpty(((Rom)item).SHA512) ? 0 : 1);
						}

						_baddumpCount += (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_goodCount += (((Rom)item).ItemStatus == ItemStatus.Good ? 1 : 0);
						_nodumpCount += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						_verifiedCount += (((Rom)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
						break;
					case ItemType.Sample:
						_sampleCount += 1;
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
			_count += stats.Count;

			_archiveCount += stats.ArchiveCount;
			_biosSetCount += stats.BiosSetCount;
			_diskCount += stats.DiskCount;
			_releaseCount += stats.ReleaseCount;
			_romCount += stats.RomCount;
			_sampleCount += stats.SampleCount;

			_totalSize += stats.TotalSize;

			// Individual hash counts
			_crcCount += stats.CRCCount;
			_md5Count += stats.MD5Count;
			_sha1Count += stats.SHA1Count;
			_sha256Count += stats.SHA256Count;
			_sha384Count += stats.SHA384Count;
			_sha512Count += stats.SHA512Count;

			// Individual status counts
			_baddumpCount += stats.BaddumpCount;
			_goodCount += stats.GoodCount;
			_nodumpCount += stats.NodumpCount;
			_verifiedCount += stats.VerifiedCount;
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
				_count -= 1;

				// Now we do different things for each item type

				switch (item.Type)
				{
					case ItemType.Archive:
						_archiveCount -= 1;
						break;
					case ItemType.BiosSet:
						_biosSetCount -= 1;
						break;
					case ItemType.Disk:
						_diskCount -= 1;
						if (((Disk)item).ItemStatus != ItemStatus.Nodump)
						{
							_md5Count -= (String.IsNullOrEmpty(((Disk)item).MD5) ? 0 : 1);
							_sha1Count -= (String.IsNullOrEmpty(((Disk)item).SHA1) ? 0 : 1);
							_sha256Count -= (String.IsNullOrEmpty(((Disk)item).SHA256) ? 0 : 1);
							_sha384Count -= (String.IsNullOrEmpty(((Disk)item).SHA384) ? 0 : 1);
							_sha512Count -= (String.IsNullOrEmpty(((Disk)item).SHA512) ? 0 : 1);
						}

						_baddumpCount -= (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_goodCount -= (((Disk)item).ItemStatus == ItemStatus.Good ? 1 : 0);
						_nodumpCount -= (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						_verifiedCount -= (((Disk)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
						break;
					case ItemType.Release:
						_releaseCount -= 1;
						break;
					case ItemType.Rom:
						_romCount -= 1;
						if (((Rom)item).ItemStatus != ItemStatus.Nodump)
						{
							_totalSize -= ((Rom)item).Size;
							_crcCount -= (String.IsNullOrEmpty(((Rom)item).CRC) ? 0 : 1);
							_md5Count -= (String.IsNullOrEmpty(((Rom)item).MD5) ? 0 : 1);
							_sha1Count -= (String.IsNullOrEmpty(((Rom)item).SHA1) ? 0 : 1);
							_sha256Count -= (String.IsNullOrEmpty(((Rom)item).SHA256) ? 0 : 1);
							_sha384Count -= (String.IsNullOrEmpty(((Rom)item).SHA384) ? 0 : 1);
							_sha512Count -= (String.IsNullOrEmpty(((Rom)item).SHA512) ? 0 : 1);
						}

						_baddumpCount -= (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
						_goodCount -= (((Rom)item).ItemStatus == ItemStatus.Good ? 1 : 0);
						_nodumpCount -= (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
						_verifiedCount -= (((Rom)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
						break;
					case ItemType.Sample:
						_sampleCount -= 1;
						break;
				}
			}
		}

		/// <summary>
		/// Reset all statistics
		/// </summary>
		public void Reset()
		{
			_count = 0;

			_archiveCount = 0;
			_biosSetCount = 0;
			_diskCount = 0;
			_releaseCount = 0;
			_romCount = 0;
			_sampleCount = 0;

			_totalSize = 0;

			_crcCount = 0;
			_md5Count = 0;
			_sha1Count = 0;
			_sha256Count = 0;
			_sha384Count = 0;
			_sha512Count = 0;

			_baddumpCount = 0;
			_goodCount = 0;
			_nodumpCount = 0;
			_verifiedCount = 0;
		}

		#endregion // Instance Methods
	}
}
