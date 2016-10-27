using System;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	public class Filter
	{
		#region Private instance variables

		private string _gameName;
		private string _romName;
		private string _romType;
		private long _sizeGreaterThanOrEqual;
		private long _sizeLessThanOrEqual;
		private long _sizeEqualTo;
		private string _crc;
		private string _md5;
		private string _sha1;
		private ItemStatus _itemStatus;

		#endregion

		/// <summary>
		/// Create an empty Filter object
		/// </summary>
		public Filter()
		{
			_gameName = null;
			_romName = null;
			_romType = null;
			_sizeGreaterThanOrEqual = -1;
			_sizeLessThanOrEqual = -1;
			_sizeEqualTo = -1;
			_crc = null;
			_md5 = null;
			_sha1 = null;
			_itemStatus = ItemStatus.NULL;
		}

		/// <summary>
		/// Create a populated Filter object
		/// </summary>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemStatus">Select roms with the given status</param>
		public Filter(string gamename, string romname, string romtype, long sgt,
			long slt, long seq, string crc, string md5, string sha1, ItemStatus itemStatus)
		{
			_gameName = gamename;
			_romName = romname;
			_romType = romtype;
			_sizeGreaterThanOrEqual = sgt;
			_sizeLessThanOrEqual = slt;
			_sizeEqualTo = seq;
			_crc = crc;
			_md5 = md5;
			_sha1 = sha1;
			_itemStatus = itemStatus;
		}

		/// <summary>
		/// Check to see if a DatItem passes the filter
		/// </summary>
		/// <param name="item">DatItem to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the file passed the filter, false otherwise</returns>
		public bool ItemPasses(DatItem item, Logger logger)
		{
			// Take care of Rom and Disk specific differences
			if (item.Type == ItemType.Rom)
			{
				Rom rom = (Rom)item;

				// Filter on status
				if (_itemStatus != ItemStatus.NULL)
				{
					if (_itemStatus == ItemStatus.NotNodump && rom.ItemStatus == ItemStatus.Nodump)
					{
						return false;
					}
					else if (_itemStatus != ItemStatus.NotNodump && rom.ItemStatus != _itemStatus)
					{
						return false;
					}
				}

				// Filter on rom size
				if (_sizeEqualTo != -1 && rom.Size != _sizeEqualTo)
				{
					return false;
				}
				else
				{
					if (_sizeGreaterThanOrEqual != -1 && rom.Size < _sizeGreaterThanOrEqual)
					{
						return false;
					}
					if (_sizeLessThanOrEqual != -1 && rom.Size > _sizeLessThanOrEqual)
					{
						return false;
					}
				}

				// Filter on _crc
				if (!String.IsNullOrEmpty(_crc))
				{
					if (_crc.StartsWith("*") && _crc.EndsWith("*"))
					{
						if (!rom.CRC.ToLowerInvariant().Contains(_crc.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_crc.StartsWith("*"))
					{
						if (!rom.CRC.EndsWith(_crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_crc.EndsWith("*"))
					{
						if (!rom.CRC.StartsWith(_crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (!String.Equals(rom.CRC, _crc, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}

				// Filter on _md5
				if (!String.IsNullOrEmpty(_md5))
				{
					if (_md5.StartsWith("*") && _md5.EndsWith("*"))
					{
						if (!rom.MD5.ToLowerInvariant().Contains(_md5.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_md5.StartsWith("*"))
					{
						if (!rom.MD5.EndsWith(_md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_md5.EndsWith("*"))
					{
						if (!rom.MD5.StartsWith(_md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (!String.Equals(rom.MD5, _md5, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}

				// Filter on _sha1
				if (!String.IsNullOrEmpty(_sha1))
				{
					if (_sha1.StartsWith("*") && _sha1.EndsWith("*"))
					{
						if (!rom.SHA1.ToLowerInvariant().Contains(_sha1.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_sha1.StartsWith("*"))
					{
						if (!rom.SHA1.EndsWith(_sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_sha1.EndsWith("*"))
					{
						if (!rom.SHA1.StartsWith(_sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (!String.Equals(rom.SHA1, _sha1, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}
			}
			else if (item.Type == ItemType.Disk)
			{
				Disk rom = (Disk)item;

				// Filter on status
				if (_itemStatus != ItemStatus.NULL && rom.ItemStatus != _itemStatus)
				{
					if (_itemStatus == ItemStatus.NotNodump && rom.ItemStatus == ItemStatus.Nodump)
					{
						return false;
					}
					else if (_itemStatus != ItemStatus.NotNodump && rom.ItemStatus != _itemStatus)
					{
						return false;
					}
				}

				// Filter on _md5
				if (!String.IsNullOrEmpty(_md5))
				{
					if (_md5.StartsWith("*") && _md5.EndsWith("*"))
					{
						if (!rom.MD5.ToLowerInvariant().Contains(_md5.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_md5.StartsWith("*"))
					{
						if (!rom.MD5.EndsWith(_md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_md5.EndsWith("*"))
					{
						if (!rom.MD5.StartsWith(_md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (!String.Equals(rom.MD5, _md5, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}

				// Filter on _sha1
				if (!String.IsNullOrEmpty(_sha1))
				{
					if (_sha1.StartsWith("*") && _sha1.EndsWith("*"))
					{
						if (!rom.SHA1.ToLowerInvariant().Contains(_sha1.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_sha1.StartsWith("*"))
					{
						if (!rom.SHA1.EndsWith(_sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_sha1.EndsWith("*"))
					{
						if (!rom.SHA1.StartsWith(_sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (!String.Equals(rom.SHA1, _sha1, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}
			}

			// Filter on game name
			if (!String.IsNullOrEmpty(_gameName))
			{
				if (_gameName.StartsWith("*") && _gameName.EndsWith("*"))
				{
					if (!item.Machine.Name.ToLowerInvariant().Contains(_gameName.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (_gameName.StartsWith("*"))
				{
					if (!item.Machine.Name.EndsWith(_gameName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (_gameName.EndsWith("*"))
				{
					if (!item.Machine.Name.StartsWith(_gameName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (!String.Equals(item.Machine.Name, _gameName, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			// Filter on rom name
			if (!String.IsNullOrEmpty(_romName))
			{
				if (_romName.StartsWith("*") && _romName.EndsWith("*"))
				{
					if (!item.Name.ToLowerInvariant().Contains(_romName.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (_romName.StartsWith("*"))
				{
					if (!item.Name.EndsWith(_romName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (_romName.EndsWith("*"))
				{
					if (!item.Name.StartsWith(_romName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (!String.Equals(item.Name, _romName, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			// Filter on rom type
			if (String.IsNullOrEmpty(_romType) && item.Type != ItemType.Rom && item.Type != ItemType.Disk)
			{
				return false;
			}
			if (!String.IsNullOrEmpty(_romType) && !String.Equals(item.Type.ToString(), _romType, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			return true;
		}
	}
}
