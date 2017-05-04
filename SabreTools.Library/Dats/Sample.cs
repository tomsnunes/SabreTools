using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public class Sample : DatItem, ICloneable
	{
		#region Constructors

		/// <summary>
		/// Create a default, empty Sample object
		/// </summary>
		public Sample()
		{
			_name = "";
			_itemType = ItemType.Sample;
		}

		#endregion

		#region Cloning Methods

		public object Clone()
		{
			return new Sample()
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
			};
		}

		#endregion

		#region Comparision Methods

		public override bool Equals(DatItem other)
		{
			// If we don't have a sample, return false
			if (_itemType != other.Type)
			{
				return false;
			}

			// Otherwise, treat it as a sample
			Sample newOther = (Sample)other;

			// If the archive information matches
			return (_name == newOther.Name);
		}

		#endregion
	}
}
