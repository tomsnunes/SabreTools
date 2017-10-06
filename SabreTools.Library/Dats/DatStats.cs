namespace SabreTools.Library.Dats
{
	public class DatStats
	{
		#region Private instance variables

		// Statistical data related to the DAT
		private object _lockObject;
		private long _count;
		private long _romCount;
		private long _diskCount;
		private long _totalSize;
		private long _crcCount;
		private long _md5Count;
		private long _sha1Count;
		private long _sha256Count;
		private long _sha384Count;
		private long _sha512Count;
		private long _baddumpCount;
		private long _nodumpCount;

		#endregion

		#region Publicly facing variables

		// Statistical data related to the DAT
		public object LockObject
		{
			get
			{
				if (_lockObject == null)
				{
					_lockObject = new object();
				}
				return _lockObject;
			}
		}
		public long Count
		{
			get { return _count; }
		}
		public long RomCount
		{
			get { return _romCount; }
		}
		public long DiskCount
		{
			get { return _diskCount; }
		}
		public long TotalSize
		{
			get { return _totalSize; }
		}
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
		public long BaddumpCount
		{
			get { return _baddumpCount; }
		}
		public long NodumpCount
		{
			get { return _nodumpCount; }
		}

		#endregion
	}
}
