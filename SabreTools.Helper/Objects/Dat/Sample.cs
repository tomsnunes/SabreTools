using System;
using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	[Serializable]
	public class Sample : DatItem
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

		/// <summary>
		/// Create a new Sample object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		public Sample(string name)
		{
			_name = name;
			_itemType = ItemType.Sample;
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
