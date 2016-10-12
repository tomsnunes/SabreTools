using System;

namespace SabreTools.Helper
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

		/// <summary>
		/// Create a new Sample object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="description">Description of the Bios set item</param>
		/// <param name="default">True if this is the default BIOS, false if it is not, null for undefined</param>
		public BiosSet(string name, string description, bool? @default)
		{
			_name = name;
			_itemType = ItemType.BiosSet;
			_description = description;
			_default = @default;
		}

		/// <summary>
		/// Create a new Sample object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="description">Description of the Bios set item</param>
		/// <param name="default">True if this is the default BIOS, false if it is not, null for undefined</param>
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
		public BiosSet(string name, string description, bool? @default, string machineName, string comment, string machineDescription, string year,
			string manufacturer, string romOf, string cloneOf, string sampleOf, string sourceFile, bool isBios, string board, string rebuildTo,
			int systemId, string systemName, int sourceId, string sourceName)
		{
			_name = name;
			_itemType = ItemType.BiosSet;
			_description = description;
			_default = @default;
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
