using System;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	public class Disk : DatItem, ICloneable
	{
		#region Private instance variables

		// Disk information
		protected string _md5;
		protected string _sha1;
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

		public object Clone()
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

			if (((String.IsNullOrEmpty(_md5) || String.IsNullOrEmpty(newOther.MD5)) || this.MD5 == newOther.MD5) &&
					((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(newOther.SHA1)) || this.SHA1 == newOther.SHA1))
			{
				dupefound = true;
			}

			return dupefound;
		}

		#endregion
	}
}
