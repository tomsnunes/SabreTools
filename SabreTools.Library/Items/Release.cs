using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.Items
{
	public class Release : DatItem
	{
		#region Private instance variables

		private string _region;
		private string _language;
		private string _date;
		private bool? _default;

		#endregion

		#region Publicly facing variables

		public string Region
		{
			get { return _region; }
			set { _region = value; }
		}
		public string Language
		{
			get { return _language; }
			set { _language = value; }
		}
		public string Date
		{
			get { return _date; }
			set { _date = value; }
		}
		public bool? Default
		{
			get { return _default; }
			set { _default = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Release object
		/// </summary>
		public Release()
		{
			_name = "";
			_itemType = ItemType.Release;
			_region = "";
			_language = "";
			_date = "";
			_default = null;
		}

		#endregion

		#region Cloning Methods

		public new object Clone()
		{
			Release item = new Release()
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

				Region = this.Region,
				Language = this.Language,
				Date = this.Date,
				Default = this.Default,
			};

			item.CopyMachineInformation(this);
			return item;
		}

		#endregion

		#region Comparision Methods

		public override bool Equals(DatItem other)
		{
			// If we don't have a release return false
			if (_itemType != other.Type)
			{
				return false;
			}

			// Otherwise, treat it as a reease
			Release newOther = (Release)other;

			// If the archive information matches
			return (_name == newOther.Name
				&& _region == newOther.Region
				&& _language == newOther.Language
				&& _date == newOther.Date
				&& _default == newOther.Default);
		}

		#endregion
	}
}
