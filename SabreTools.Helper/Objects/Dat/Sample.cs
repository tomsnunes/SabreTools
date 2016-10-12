using System;

namespace SabreTools.Helper
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

		/// <summary>
		/// Create a new Sample object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="machineName">Name for the machine/game</param>
		/// <param name="comment">Comment for the machine/game</param>
		/// <param name="machineDescription">Description for the machine/game</param>
		/// <param name="year">Year for the machine/game</param>
		/// <param name="manufacturer">Manufacturer name for the machine/game</param>
		/// <param name="romOf">Set that this machine/game is a rom of</param>
		/// <param name="cloneOf">Set that this machine/game is a clone of</param>
		/// <param name="sampleOf">Set that this machine/game is a sample of</param>
		/// <param name="sourceFile">Source file for the machine/game</param>
		/// <param name="isBios">True if this game is a BIOS, false otherwise</param>
		/// <param name="board">Name of the board for this machine/game</param>
		/// <param name="rebuildTo">Name of the game to rebuild to</param>
		/// <param name="systemId">System ID to be associated with</param>
		/// <param name="systemName">System Name to be associated with</param>
		/// <param name="sourceId">Source ID to be associated with</param>
		/// <param name="sourceName">Source Name to be associated with</param>
		public Sample(string name, string machineName, string comment, string machineDescription, string year,
			string manufacturer, string romOf, string cloneOf, string sampleOf, string sourceFile, bool isBios, string board, string rebuildTo,
			int systemId, string systemName, int sourceId, string sourceName)
		{
			_name = name;
			_itemType = ItemType.Sample;
			_machineName = machineName;
			_comment = comment;
			_machineDescription = machineDescription;
			_year = year;
			_manufacturer = manufacturer;
			_romOf = romOf;
			_cloneOf = cloneOf;
			_sampleOf = sampleOf;
			_sourceFile = sourceFile;
			_isBios = isBios;
			_board = board;
			_rebuildTo = rebuildTo;
			_systemId = systemId;
			_systemName = systemName;
			_sourceId = sourceId;
			_sourceName = sourceName;
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
