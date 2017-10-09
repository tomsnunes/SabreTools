using SabreTools.Library.Data;

namespace SabreTools.Library.Items
{
	/// <summary>
	/// Represents generic archive files to be included in a set
	/// </summary>
	public class Archive : DatItem
	{
		#region Constructors

		/// <summary>
		/// Create a default, empty Archive object
		/// </summary>
		public Archive()
		{
			_name = "";
			_itemType = ItemType.Archive;
		}

		#endregion

		#region Cloning Methods

		public new object Clone()
		{
			Archive item = new Archive()
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

				SystemID = this.SystemID,
				System = this.System,
				SourceID = this.SourceID,
				Source = this.Source,
			};

			item.CopyMachineInformation(this);
			return item;
		}

		#endregion

		#region Comparision Methods

		public override bool Equals(DatItem other)
		{
			// If we don't have an archive, return false
			if (_itemType != other.Type)
			{
				return false;
			}

			// Otherwise, treat it as an archive
			Archive newOther = (Archive)other;

			// If the archive information matches
			return (_name == newOther.Name);
		}

		#endregion
	}
}
