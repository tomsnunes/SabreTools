using System;
using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	[Serializable]
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

		#region Comparision Methods

		public override bool Equals(DatItem other)
		{
			// If we don't have a biosset, return false
			if (_itemType != other.Type)
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
