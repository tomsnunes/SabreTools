using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
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
		}

		// Individual DatItem type counts
		public long ArchiveCount
		{
			get { return _archiveCount; }
		}
		public long BiosSetCount
		{
			get { return _biosSetCount; }
		}
		public long DiskCount
		{
			get { return _diskCount; }
		}
		public long ReleaseCount
		{
			get { return _releaseCount; }
		}
		public long RomCount
		{
			get { return _romCount; }
		}
		public long SampleCount
		{
			get { return _sampleCount; }
		}

		// Total reported size
		public long TotalSize
		{
			get { return _totalSize; }
		}

		// Individual hash counts
		public long CRCCount
		{
			get { return _crcCount; }
		}
		public long MD5Count
		{
			get { return _md5Count; }
		}
		public long SHA1Count
		{
			get { return _sha1Count; }
		}
		public long SHA256Count
		{
			get { return _sha256Count; }
		}
		public long SHA384Count
		{
			get { return _sha384Count; }
		}
		public long SHA512Count
		{
			get { return _sha512Count; }
		}

		// Individual status counts
		public long BaddumpCount
		{
			get { return _baddumpCount; }
		}
		public long GoodCount
		{
			get { return _goodCount; }
		}
		public long NodumpCount
		{
			get { return _nodumpCount; }
		}
		public long VerifiedCount
		{
			get { return _verifiedCount; }
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
