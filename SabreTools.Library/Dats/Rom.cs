using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public class Rom : Disk, ICloneable
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
		/// <param name="omitFromScan"></param>
		/// <remarks>TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually</remarks>
		public Rom(string name, string machineName, Hash omitFromScan = Hash.DeepHashes)
		{
			_name = name;
			_itemType = ItemType.Rom;
			_size = -1;
			if ((omitFromScan & Hash.CRC) == 0)
			{
				_crc = "null";
			}
			if ((omitFromScan & Hash.MD5) == 0)
			{
				_md5 = "null";
			}
			if ((omitFromScan & Hash.SHA1) == 0)
			{
				_sha1 = "null";
			}
			if ((omitFromScan & Hash.SHA256) == 0)
			{
				_sha256 = "null";
			}
			if ((omitFromScan & Hash.SHA384) == 0)
			{
				_sha384 = "null";
			}
			if ((omitFromScan & Hash.SHA512) == 0)
			{
				_sha512 = "null";
			}
			_itemStatus = ItemStatus.None;

			_machine = new Machine
			{
				Name = machineName,
				Description = machineName,
			};
		}

		#endregion

		#region Cloning Methods

		public new object Clone()
		{
			return new Rom()
			{
				Name = this.Name,
				Type = this.Type,
				Dupe = this.Dupe,

				Machine = this.Machine,

				Supported = this.Supported,
				Publisher = this.Publisher,
				Infos = this.Infos,
				PartName = this.PartName,
				PartInterface = this.PartInterface,
				Features = this.Features,
				AreaName = this.AreaName,
				AreaSize = this.AreaSize,

				SystemID = this.SystemID,
				System = this.System,
				SourceID = this.SourceID,
				Source = this.Source,

				MD5 = this.MD5,
				SHA1 = this.SHA1,
				SHA256 = this.SHA256,
				SHA384 = this.SHA384,
				SHA512 = this.SHA512,
				ItemStatus = this.ItemStatus,
				Size = this.Size,
				CRC = this.CRC,
				Date = this.Date,
			};
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

			if ((this.Size == newOther.Size)
				&& ((String.IsNullOrEmpty(this.CRC) || String.IsNullOrEmpty(newOther.CRC)) || this.CRC == newOther.CRC)
				&& ((String.IsNullOrEmpty(this.MD5) || String.IsNullOrEmpty(newOther.MD5)) || this.MD5 == newOther.MD5)
				&& ((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(newOther.SHA1)) || this.SHA1 == newOther.SHA1)
				&& ((String.IsNullOrEmpty(this.SHA256) || String.IsNullOrEmpty(newOther.SHA256)) || this.SHA256 == newOther.SHA256)
				&& ((String.IsNullOrEmpty(this.SHA384) || String.IsNullOrEmpty(newOther.SHA384)) || this.SHA384 == newOther.SHA384)
				&& ((String.IsNullOrEmpty(this.SHA512) || String.IsNullOrEmpty(newOther.SHA512)) || this.SHA512 == newOther.SHA512))
			{
				dupefound = true;
			}

			return dupefound;
		}

		#endregion
	}
}
