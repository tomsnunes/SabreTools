using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
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
