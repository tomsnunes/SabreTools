using System;

namespace SabreTools.Helper
{
	public class Disk : DatItem
	{
		#region Private instance variables

		// Disk information
		protected string _md5;
		protected string _sha1;
		// private string _merge;
		// private DiskStatus _romStatus;
		protected bool _nodump;

		#endregion

		#region Publicly facing variables

		// Disk information
		public string MD5
		{
			get { return _md5; }
			set { _md5 = value; }
		}
		public string SHA1
		{
			get { return _sha1; }
			set { _sha1 = value; }
		}
		public bool Nodump
		{
			get { return _nodump; }
			set { _nodump = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Disk object
		/// </summary>
		public Disk()
		{
			_name = "";
			_itemType = ItemType.Disk;
			_dupeType = DupeType.None;
			_nodump = false;
		}

		/// <summary>
		/// Create a new Disk object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="md5">String representation of the MD5</param>
		/// <param name="sha1">String representation of the SHA-1</param>
		/// <param name="nodump">True if the file is a nodump, false otherwise</param>
		public Disk(string name, string md5, string sha1, bool nodump)
		{
			_name = name;
			_itemType = ItemType.Disk;
			_md5 = md5.ToLowerInvariant();
			_sha1 = sha1.ToLowerInvariant();
			_nodump = nodump;
		}

		/// <summary>
		/// Create a new Disk object with the included information
		/// </summary>
		/// <param name="name">Name of the item, including extension</param>
		/// <param name="md5">String representation of the MD5</param>
		/// <param name="sha1">String representation of the SHA-1</param>
		/// <param name="nodump">True if the file is a nodump, false otherwise</param>
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
		public Disk(string name, string md5, string sha1, bool nodump, string machineName, string comment,
			string machineDescription, string year, string manufacturer, string romOf, string cloneOf, string sampleOf, string sourceFile,
			bool isBios, string board, string rebuildTo, int systemId, string systemName, int sourceId, string sourceName)
		{
			_name = name;
			_itemType = ItemType.Disk;
			_md5 = md5.ToLowerInvariant();
			_sha1 = sha1.ToLowerInvariant();
			_nodump = nodump;
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
			Disk newOther = (Disk)other;

			// If either is a nodump, it's never a match
			if (_nodump || newOther.Nodump)
			{
				return dupefound;
			}

			if (((String.IsNullOrEmpty(_md5) || String.IsNullOrEmpty(newOther.MD5)) || this.MD5 == newOther.MD5) &&
					((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(newOther.SHA1)) || this.SHA1 == newOther.SHA1))
			{
				dupefound = true;
			}

			return dupefound;
		}

		#endregion
	}
}
