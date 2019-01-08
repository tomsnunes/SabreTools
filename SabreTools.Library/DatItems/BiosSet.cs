using SabreTools.Library.Data;

namespace SabreTools.Library.DatItems
{
	/// <summary>
	/// Represents which BIOS(es) is associated with a set
	/// </summary>
	public class BiosSet : DatItem
	{
		#region Private instance variables

		private string _description;
		private bool? _default;

		#endregion

		#region Publicly facing variables

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}
		public bool? Default
		{
			get { return _default; }
			set { _default = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Sample object
		/// </summary>
		public BiosSet()
		{
			_name = "";
			_itemType = ItemType.BiosSet;
		}

		#endregion

		#region Cloning Methods

		public override object Clone()
		{
			return new BiosSet()
			{
				Name = this.Name,
				ItemType = this.ItemType,
				DupeType = this.DupeType,

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

				Description = this.Description,
				Default = this.Default,
			};
		}

		#endregion

		#region Comparision Methods

		public override bool Equals(DatItem other)
		{
			// If we don't have a biosset, return false
			if (_itemType != other.ItemType)
			{
				return false;
			}

			// Otherwise, treat it as a biosset
			BiosSet newOther = (BiosSet)other;

			// If the archive information matches
			return (_name == newOther.Name && _description == newOther.Description && _default == newOther.Default);
		}

		#endregion
	}
}
