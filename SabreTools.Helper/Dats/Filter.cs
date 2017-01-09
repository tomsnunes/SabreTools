using System;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	public class Filter
	{
		#region Private instance variables

		private string _gameName;
		private string _notGameName;
		private string _romName;
		private string _notRomName;
		private string _romType;
		private string _notRomType;
		private long _sizeGreaterThanOrEqual;
		private long _sizeLessThanOrEqual;
		private long _sizeEqualTo;
		private string _crc;
		private string _notCrc;
		private string _md5;
		private string _notMd5;
		private string _sha1;
		private string _notSha1;
		private ItemStatus _itemStatus;
		private ItemStatus _itemNotStatus;
		private MachineType _machineType;
		private MachineType _machineNotType;
		private bool? _runnable;

		#endregion

		/// <summary>
		/// Create an empty Filter object
		/// </summary>
		public Filter()
		{
			_gameName = null;
			_notGameName = null;
			_romName = null;
			_notRomName = null;
			_romType = null;
			_notRomType = null;
			_sizeGreaterThanOrEqual = -1;
			_sizeLessThanOrEqual = -1;
			_sizeEqualTo = -1;
			_crc = null;
			_notCrc = null;
			_md5 = null;
			_notMd5 = null;
			_sha1 = null;
			_notSha1 = null;
			_itemStatus = ItemStatus.NULL;
			_itemNotStatus = ItemStatus.NULL;
			_machineType = MachineType.NULL;
			_machineNotType = MachineType.NULL;
			_runnable = null;
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
		/// <param name="machineType">Select games with the given type</param>
		/// <param name="notgamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="notromname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="notromtype">Type of the rom to match</param>
		/// <param name="notcrc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="notmd5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="notsha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="itemNotStatus">Select roms without the given status</param>
		/// <param name="machineNotType">Select games without the given type</param>
		/// <param name="runnable">Select games that have a value for runnable</param>
		public Filter(string gamename, string romname, string romtype, long sgt,
			long slt, long seq, string crc, string md5, string sha1, ItemStatus itemStatus,
			MachineType machineType, string notgamename, string notromname, string notromtype,
			string notcrc, string notmd5, string notsha1, ItemStatus itemNotStatus,
			MachineType machineNotType, bool? runnable)
		{
			_gameName = gamename;
			_notGameName = notgamename;
			_romName = romname;
			_notRomName = notromname;
			_romType = romtype;
			_notRomType = notromtype;
			_sizeGreaterThanOrEqual = sgt;
			_sizeLessThanOrEqual = slt;
			_sizeEqualTo = seq;
			_crc = crc;
			_notCrc = notcrc;
			_md5 = md5;
			_notMd5 = notmd5;
			_sha1 = sha1;
			_notSha1 = notsha1;
			_itemStatus = itemStatus;
			_itemNotStatus = itemNotStatus;
			_machineType = machineType;
			_machineNotType = machineNotType;
			_runnable = runnable;
		}

		/// <summary>
		/// Check to see if a DatItem passes the filter
		/// </summary>
		/// <param name="item">DatItem to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the file passed the filter, false otherwise</returns>
		public bool ItemPasses(DatItem item, Logger logger)
		{
			// Filter on machine type
			if (_machineType != MachineType.NULL && item.Machine.MachineType != _machineType)
			{
				return false;
			}
			if (_machineNotType != MachineType.NULL && item.Machine.MachineType == _machineNotType)
			{
				return false;
			}

			// Filter on machine runability
			if (_runnable != null && item.Machine.Runnable != _runnable)
			{
				return false;
			}

			// Take care of Rom and Disk specific differences
			if (item.Type == ItemType.Rom)
			{
				Rom rom = (Rom)item;

				// Filter on status
				if (_itemStatus != ItemStatus.NULL && rom.ItemStatus != _itemStatus)
				{
					return false;
				}
				if (_itemNotStatus != ItemStatus.NULL && rom.ItemStatus == _itemNotStatus)
				{
					return false;
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

				// Filter on CRC
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
				if (!String.IsNullOrEmpty(_notCrc))
				{
					if (_notCrc.StartsWith("*") && _notCrc.EndsWith("*"))
					{
						if (rom.CRC.ToLowerInvariant().Contains(_notCrc.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_notCrc.StartsWith("*"))
					{
						if (rom.CRC.EndsWith(_notCrc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_notCrc.EndsWith("*"))
					{
						if (rom.CRC.StartsWith(_notCrc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (String.Equals(rom.CRC, _notCrc, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}

				// Filter on MD5
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
				if (!String.IsNullOrEmpty(_notMd5))
				{
					if (_notMd5.StartsWith("*") && _notMd5.EndsWith("*"))
					{
						if (rom.MD5.ToLowerInvariant().Contains(_notMd5.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_notMd5.StartsWith("*"))
					{
						if (rom.MD5.EndsWith(_notMd5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_notMd5.EndsWith("*"))
					{
						if (rom.MD5.StartsWith(_notMd5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (String.Equals(rom.MD5, _notMd5, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}

				// Filter on SHA1
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
				if (!String.IsNullOrEmpty(_notSha1))
				{
					if (_notSha1.StartsWith("*") && _notSha1.EndsWith("*"))
					{
						if (rom.SHA1.ToLowerInvariant().Contains(_notSha1.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_notSha1.StartsWith("*"))
					{
						if (rom.SHA1.EndsWith(_notSha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_notSha1.EndsWith("*"))
					{
						if (rom.SHA1.StartsWith(_notSha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (String.Equals(rom.SHA1, _notSha1, StringComparison.InvariantCultureIgnoreCase))
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
					return false;
				}
				if (_itemNotStatus != ItemStatus.NULL && rom.ItemStatus == _itemNotStatus)
				{
					return false;
				}

				// Filter on MD5
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
				if (!String.IsNullOrEmpty(_notMd5))
				{
					if (_notMd5.StartsWith("*") && _notMd5.EndsWith("*"))
					{
						if (rom.MD5.ToLowerInvariant().Contains(_notMd5.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_notMd5.StartsWith("*"))
					{
						if (rom.MD5.EndsWith(_notMd5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_notMd5.EndsWith("*"))
					{
						if (rom.MD5.StartsWith(_notMd5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (String.Equals(rom.MD5, _notMd5, StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
				}

				// Filter on SHA1
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
				if (!String.IsNullOrEmpty(_notSha1))
				{
					if (_notSha1.StartsWith("*") && _notSha1.EndsWith("*"))
					{
						if (rom.SHA1.ToLowerInvariant().Contains(_notSha1.ToLowerInvariant().Replace("*", "")))
						{
							return false;
						}
					}
					else if (_notSha1.StartsWith("*"))
					{
						if (rom.SHA1.EndsWith(_notSha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else if (_notSha1.EndsWith("*"))
					{
						if (rom.SHA1.StartsWith(_notSha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							return false;
						}
					}
					else
					{
						if (String.Equals(rom.SHA1, _notSha1, StringComparison.InvariantCultureIgnoreCase))
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
			if (!String.IsNullOrEmpty(_notGameName))
			{
				if (_notGameName.StartsWith("*") && _notGameName.EndsWith("*"))
				{
					if (item.Machine.Name.ToLowerInvariant().Contains(_notGameName.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (_notGameName.StartsWith("*"))
				{
					if (item.Machine.Name.EndsWith(_notGameName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (_notGameName.EndsWith("*"))
				{
					if (item.Machine.Name.StartsWith(_notGameName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (String.Equals(item.Machine.Name, _notGameName, StringComparison.InvariantCultureIgnoreCase))
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
			if (!String.IsNullOrEmpty(_notRomName))
			{
				if (_notRomName.StartsWith("*") && _notRomName.EndsWith("*"))
				{
					if (item.Name.ToLowerInvariant().Contains(_notRomName.ToLowerInvariant().Replace("*", "")))
					{
						return false;
					}
				}
				else if (_notRomName.StartsWith("*"))
				{
					if (item.Name.EndsWith(_notRomName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else if (_notRomName.EndsWith("*"))
				{
					if (item.Name.StartsWith(_notRomName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (String.Equals(item.Name, _notRomName, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
			}

			// Filter on rom type
			if (String.IsNullOrEmpty(_romType) && String.IsNullOrEmpty(_notRomType) && item.Type != ItemType.Rom && item.Type != ItemType.Disk)
			{
				return false;
			}
			if (!String.IsNullOrEmpty(_romType) && !String.Equals(item.Type.ToString(), _romType, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (!String.IsNullOrEmpty(_notRomType) && String.Equals(item.Type.ToString(), _notRomType, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			return true;
		}
	}
}
