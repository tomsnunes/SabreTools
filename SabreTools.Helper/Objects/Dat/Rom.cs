using System;
using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	[Serializable]
	public class Rom : Disk
	{
		#region Private instance variables

		// Rom information
		protected long _size;
		protected string _crc;
		private string _date;

		#endregion

		#region Publicly facing variables

		// Rom information
		public long Size
		{
			get { return _size; }
			set { _size = value; }
		}
		public string CRC
		{
			get { return _crc; }
			set { _crc = value; }
		}
		public string Date
		{
			get { return _date; }
			set { _date = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Rom object
		/// </summary>
		public Rom()
		{
			_name = "";
			_itemType = ItemType.Rom;
			_dupeType = 0x00;
			_itemStatus = ItemStatus.None;
			_date = "";
		}

		/// <summary>
		/// Create a "blank" Rom object
		/// </summary>
		/// <param name="name"></param>
		/// <param name="machineName"></param>
		public Rom(string name, string machineName) :
			this(name, -1, "null", "null", "null", ItemStatus.None, null)
		{
			_machine = new Machine
			{
				Name = machineName,
				Description = machineName,
			};
		}

		/// <summary>
		/// Create a new Rom object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="size">Long size of the item</param>
		/// <param name="crc">String representation of the CRC</param>
		/// <param name="md5">String representation of the MD5</param>
		/// <param name="sha1">String representation of the SHA-1</param>
		/// <param name="itemStatus">Status of the current item</param>
		/// <param name="date">String representation of the Date</param>
		public Rom(string name, long size, string crc, string md5, string sha1, ItemStatus itemStatus, string date)
		{
			_name = name;
			_itemType = ItemType.Rom;
			_size = size;
			_crc = crc?.ToLowerInvariant();
			_md5 = md5?.ToLowerInvariant();
			_sha1 = sha1?.ToLowerInvariant();
			_itemStatus = itemStatus;
			_date = date;
		}

		#endregion

		#region Comparision Methods

		public override bool Equals(DatItem other)
		{
			bool dupefound = false;

			// If we don't have a rom, return false
			if (_itemType != other.Type)
			{
				return dupefound;
			}

			// Otherwise, treat it as a rom
			Rom newOther = (Rom)other;

			// If either is a nodump, it's never a match
			if (_itemStatus == ItemStatus.Nodump || newOther.ItemStatus == ItemStatus.Nodump)
			{
				return dupefound;
			}

			if ((this.Size == newOther.Size) &&
				((String.IsNullOrEmpty(this.CRC) || String.IsNullOrEmpty(newOther.CRC)) || this.CRC == newOther.CRC) &&
				((String.IsNullOrEmpty(this.MD5) || String.IsNullOrEmpty(newOther.MD5)) || this.MD5 == newOther.MD5) &&
				((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(newOther.SHA1)) || this.SHA1 == newOther.SHA1))
			{
				dupefound = true;
			}

			return dupefound;
		}

		#endregion
	}
}
