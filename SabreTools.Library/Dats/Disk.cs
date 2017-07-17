using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public class Disk : DatItem
	{
		#region Private instance variables

		// Disk information
		protected string _md5;
		protected string _sha1;
		protected string _sha256;
		protected string _sha384;
		protected string _sha512;
		protected ItemStatus _itemStatus;

		#endregion

		#region Publicly facing variables

		// Disk information
		public string MD5
		{
			get { return _md5; }
			set { _md5 = value; }
		}
		public string SHA1
		{
			get { return _sha1; }
			set { _sha1 = value; }
		}
		public string SHA256
		{
			get { return _sha256; }
			set { _sha256 = value; }
		}
		public string SHA384
		{
			get { return _sha384; }
			set { _sha384 = value; }
		}
		public string SHA512
		{
			get { return _sha512; }
			set { _sha512 = value; }
		}
		public ItemStatus ItemStatus
		{
			get { return _itemStatus; }
			set { _itemStatus = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Disk object
		/// </summary>
		public Disk()
		{
			_name = "";
			_itemType = ItemType.Disk;
			_dupeType = 0x00;
			_itemStatus = ItemStatus.None;
		}

		#endregion

		#region Cloning Methods

		public new object Clone()
		{
			return new Disk()
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
			Disk newOther = (Disk)other;

			// If either is a nodump, it's never a match
			if (_itemStatus == ItemStatus.Nodump || newOther.ItemStatus == ItemStatus.Nodump)
			{
				return dupefound;
			}

			// If we can determine that the disks have no non-empty hashes in common, we return false
			if (!(String.IsNullOrEmpty(_md5) && String.IsNullOrEmpty(newOther.MD5))
				|| !(String.IsNullOrEmpty(_sha1) && String.IsNullOrEmpty(newOther.SHA1))
				|| !(String.IsNullOrEmpty(_sha256) && String.IsNullOrEmpty(newOther.SHA256))
				|| !(String.IsNullOrEmpty(_sha384) && String.IsNullOrEmpty(newOther.SHA384))
				|| !(String.IsNullOrEmpty(_sha512) && String.IsNullOrEmpty(newOther.SHA512)))
			{
				dupefound = false;
			}
			else if (((String.IsNullOrEmpty(_md5) || String.IsNullOrEmpty(newOther.MD5)) || this.MD5 == newOther.MD5)
				&& ((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(newOther.SHA1)) || this.SHA1 == newOther.SHA1)
				&& ((String.IsNullOrEmpty(this.SHA256) || String.IsNullOrEmpty(newOther.SHA256)) || this.SHA256 == newOther.SHA256)
				&& ((String.IsNullOrEmpty(this.SHA384) || String.IsNullOrEmpty(newOther.SHA384)) || this.SHA384 == newOther.SHA384)
				&& ((String.IsNullOrEmpty(this.SHA512) || String.IsNullOrEmpty(newOther.SHA512)) || this.SHA256 == newOther.SHA512))
			{
				dupefound = true;
			}

			return dupefound;
		}

		#endregion
	}
}
