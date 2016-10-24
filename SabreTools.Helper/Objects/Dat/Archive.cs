using System;
using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	[Serializable]
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

		/// <summary>
		/// Create a new Archive object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		public Archive(string name)
		{
			_name = name;
			_itemType = ItemType.Archive;
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
