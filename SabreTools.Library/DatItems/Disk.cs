using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatItems
{
	/// <summary>
	/// Represents Compressed Hunks of Data (CHD) formatted disks which use internal hashes
	/// </summary>
	public class Disk : DatItem
	{
		#region Private instance variables

		// Disk information
		private byte[] _md5; // 16 bytes
		private byte[] _sha1; // 20 bytes
		private byte[] _sha256; // 32 bytes
		private byte[] _sha384; // 48 bytes
		private byte[] _sha512; // 64 bytes
		private string _merge;
		private string _region;
		private string _index;
		private bool? _writable;
		private ItemStatus _itemStatus;
		private bool? _optional;

		#endregion

		#region Publicly facing variables

		// Disk information
		public string MD5
		{
			get { return _md5.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_md5); }
			set { _md5 = Utilities.StringToByteArray(value); }
		}
		public string SHA1
		{
			get { return _sha1.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha1); }
			set { _sha1 = Utilities.StringToByteArray(value); }
		}
		public string SHA256
		{
			get { return _sha256.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha256); }
			set { _sha256 = Utilities.StringToByteArray(value); }
		}
		public string SHA384
		{
			get { return _sha384.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha384); }
			set { _sha384 = Utilities.StringToByteArray(value); }
		}
		public string SHA512
		{
			get { return _sha512.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha512); }
			set { _sha512 = Utilities.StringToByteArray(value); }
		}
		public string MergeTag
		{
			get { return _merge; }
			set { _merge = value; }
		}
		public string Region
		{
			get { return _region; }
			set { _region = value; }
		}
		public string Index
		{
			get { return _index; }
			set { _index = value; }
		}
		public bool? Writable
		{
			get { return _writable; }
			set { _writable = value; }
		}
		public ItemStatus ItemStatus
		{
			get { return _itemStatus; }
			set { _itemStatus = value; }
		}
		public bool? Optional
		{
			get { return _optional; }
			set { _optional = value; }
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

		/// <summary>
		/// Create a Rom object from a BaseFile
		/// </summary>
		/// <param name="baseFile"></param>
		public Disk(BaseFile baseFile)
		{
			_name = baseFile.Filename;
			_md5 = baseFile.MD5;
			_sha1 = baseFile.SHA1;
			_sha256 = baseFile.SHA256;
			_sha384 = baseFile.SHA384;
			_sha512 = baseFile.SHA512;
		}

		#endregion

		#region Cloning Methods

		public override object Clone()
		{
			return new Disk()
			{
				Name = this.Name,
				Type = this.Type,
				Dupe = this.Dupe,

				Supported = this.Supported,
				Publisher = this.Publisher,
				Infos = this.Infos,
				PartName = this.PartName,
				PartInterface = this.PartInterface,
				Features = this.Features,
				AreaName = this.AreaName,
				AreaSize = this.AreaSize,

				MachineName = this.MachineName,
				Comment = this.Comment,
				MachineDescription = this.MachineDescription,
				Year = this.Year,
				Manufacturer = this.Manufacturer,
				RomOf = this.RomOf,
				CloneOf = this.CloneOf,
				SampleOf = this.SampleOf,
				SourceFile = this.SourceFile,
				Runnable = this.Runnable,
				Board = this.Board,
				RebuildTo = this.RebuildTo,
				Devices = this.Devices,
				MachineType = this.MachineType,

				SystemID = this.SystemID,
				System = this.System,
				SourceID = this.SourceID,
				Source = this.Source,

				_md5 = this._md5,
				_sha1 = this._sha1,
				_sha256 = this._sha256,
				_sha384 = this._sha384,
				_sha512 = this._sha512,
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

			// If all hashes are empty but they're both nodump and the names match, then they're dupes
			if ((this._itemStatus == ItemStatus.Nodump && newOther._itemStatus == ItemStatus.Nodump)
				&& (this._name == newOther._name)
				&& (this._md5.IsNullOrWhiteSpace() && newOther._md5.IsNullOrWhiteSpace())
				&& (this._sha1.IsNullOrWhiteSpace() && newOther._sha1.IsNullOrWhiteSpace())
				&& (this._sha256.IsNullOrWhiteSpace() && newOther._sha256.IsNullOrWhiteSpace())
				&& (this._sha384.IsNullOrWhiteSpace() && newOther._sha384.IsNullOrWhiteSpace())
				&& (this._sha512.IsNullOrWhiteSpace() && newOther._sha512.IsNullOrWhiteSpace()))
			{
				dupefound = true;
			}

			// If we can determine that the disks have no non-empty hashes in common, we return false
			else if ((this._md5.IsNullOrWhiteSpace() || newOther._md5.IsNullOrWhiteSpace())
				&& (this._sha1.IsNullOrWhiteSpace() || newOther._sha1.IsNullOrWhiteSpace())
				&& (this._sha256.IsNullOrWhiteSpace() || newOther._sha256.IsNullOrWhiteSpace())
				&& (this._sha384.IsNullOrWhiteSpace() || newOther._sha384.IsNullOrWhiteSpace())
				&& (this._sha512.IsNullOrWhiteSpace() || newOther._sha512.IsNullOrWhiteSpace()))
			{
				dupefound = false;
			}

			// Otherwise if we get a partial match
			else if (((this._md5.IsNullOrWhiteSpace() || newOther._md5.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
				&& ((this._sha1.IsNullOrWhiteSpace() || newOther._sha1.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha1, newOther._sha1))
				&& ((this._sha256.IsNullOrWhiteSpace() || newOther._sha256.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha256, newOther._sha256))
				&& ((this._sha384.IsNullOrWhiteSpace() || newOther._sha384.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha384, newOther._sha384))
				&& ((this._sha512.IsNullOrWhiteSpace() || newOther._sha512.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha512, newOther._sha512)))
			{
				dupefound = true;
			}

			return dupefound;
		}

		#endregion
	}
}
