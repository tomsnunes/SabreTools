namespace SabreTools.Helper
{
	public class Rom : DatItem
	{
		#region Private instance variables

		// Rom information
		private Hash _hashData;
		// private string _merge;
		// private RomStatus _romStatus;
		private bool _nodump;
		private string _date;

		#endregion

		#region Publicly facing variables

		// Rom information
		public Hash HashData
		{
			get { return _hashData; }
			set { _hashData = value; }
		}
		public bool Nodump
		{
			get { return _nodump; }
			set { _nodump = value; }
		}
		public string Date
		{
			get { return _date; }
			set { _date = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Rom object
		/// </summary>
		public Rom()
		{
			_name = "";
			_itemType = ItemType.Rom;
			_dupeType = DupeType.None;
			_nodump = false;
			_date = "";
		}

		/// <summary>
		/// Create a "blank" Rom object
		/// </summary>
		/// <param name="name"></param>
		/// <param name="machineName"></param>
		public Rom(string name, string machineName) :
			this(name, -1, "null", "null", "null", false, null, machineName, null, machineName, null, null, null, null, null, null, false, null, null, -1, null, -1, null)
		{
		}

		/// <summary>
		/// Create a new Rom object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="size">Long size of the item</param>
		/// <param name="crc">String representation of the CRC</param>
		/// <param name="md5">String representation of the MD5</param>
		/// <param name="sha1">String representation of the SHA-1</param>
		/// <param name="nodump">True if the file is a nodump, false otherwise</param>
		/// <param name="date">String representation of the Date</param>
		public Rom(string name, long size, string crc, string md5, string sha1, bool nodump, string date)
		{
			_name = name;
			_itemType = ItemType.Rom;
			_hashData = new Hash
			{
				Size = size,
				CRC = crc.ToLowerInvariant(),
				MD5 = md5.ToLowerInvariant(),
				SHA1 = sha1.ToLowerInvariant(),
			};
			_nodump = nodump;
			_date = date;
		}

		/// <summary>
		/// Create a new Rom object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="size">Long size of the item</param>
		/// <param name="crc">String representation of the CRC</param>
		/// <param name="md5">String representation of the MD5</param>
		/// <param name="sha1">String representation of the SHA-1</param>
		/// <param name="nodump">True if the file is a nodump, false otherwise</param>
		/// <param name="date">String representation of the Date</param>
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
		public Rom(string name, long size, string crc, string md5, string sha1, bool nodump, string date, string machineName,
			string comment, string machineDescription, string year, string manufacturer, string romOf, string cloneOf, string sampleOf,
			string sourceFile, bool isBios, string board, string rebuildTo, int systemId, string systemName, int sourceId, string sourceName)
		{
			_name = name;
			_itemType = ItemType.Rom;
			_hashData = new Hash
			{
				Size = size,
				CRC = crc.ToLowerInvariant(),
				MD5 = md5.ToLowerInvariant(),
				SHA1 = sha1.ToLowerInvariant(),
			};
			_nodump = nodump;
			_date = date;
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
			bool dupefound = false;

			// If we don't have a rom, return false
			if (_itemType == other.Type)
			{
				return dupefound;
			}

			// Otherwise, treat it as a rom
			Rom newOther = (Rom)other;

			// If either is a nodump, it's never a match
			if (_nodump || newOther.Nodump)
			{
				return dupefound;
			}

			dupefound = _hashData.Equals(newOther.HashData, false);

			return dupefound;
		}

		#endregion
	}
}
