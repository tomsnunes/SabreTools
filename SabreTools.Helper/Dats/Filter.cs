using System;
using System.Collections.Generic;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	public class Filter
	{
		#region Private instance variables

		private List<string> _gameNames;
		private List<string> _notGameNames;
		private List<string> _romNames;
		private List<string> _notRomNames;
		private List<string> _romTypes;
		private List<string> _notRomTypes;
		private long _sizeGreaterThanOrEqual;
		private long _sizeLessThanOrEqual;
		private long _sizeEqualTo;
		private List<string> _crcs;
		private List<string> _notCrcs;
		private List<string> _md5s;
		private List<string> _notMd5s;
		private List<string> _sha1s;
		private List<string> _notSha1s;
		private ItemStatus _itemStatuses;
		private ItemStatus _itemNotStatuses;
		private MachineType _machineTypes;
		private MachineType _machineNotTypes;
		private bool? _runnable;

		#endregion

		/// <summary>
		/// Create an empty Filter object
		/// </summary>
		public Filter()
		{
			_gameNames = new List<string>();
			_notGameNames = new List<string>();
			_romNames = new List<string>();
			_notRomNames = new List<string>();
			_romTypes = new List<string>();
			_notRomTypes = new List<string>();
			_sizeGreaterThanOrEqual = -1;
			_sizeLessThanOrEqual = -1;
			_sizeEqualTo = -1;
			_crcs = new List<string>();
			_notCrcs = new List<string>();
			_md5s = new List<string>();
			_notMd5s = new List<string>();
			_sha1s = new List<string>();
			_notSha1s = new List<string>();
			_itemStatuses = ItemStatus.NULL;
			_itemNotStatuses = ItemStatus.NULL;
			_machineTypes = MachineType.NULL;
			_machineNotTypes = MachineType.NULL;
			_runnable = null;
		}

		/// <summary>
		/// Create a populated Filter object
		/// </summary>
		/// <param name="gamenames">Names of the games to match (can use asterisk-partials)</param>
		/// <param name="romnames">Names of the roms to match (can use asterisk-partials)</param>
		/// <param name="romtypes">Types of the roms to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crcs">CRCs of the roms to match (can use asterisk-partials)</param>
		/// <param name="md5s">MD5s of the roms to match (can use asterisk-partials)</param>
		/// <param name="sha1s">SHA-1s of the roms to match (can use asterisk-partials)</param>
		/// <param name="itemStatuses">Select roms with the given status</param>
		/// <param name="machineTypes">Select games with the given type</param>
		/// <param name="notgamenames">Names of the games to match (can use asterisk-partials)</param>
		/// <param name="notromnames">Names of the roms to match (can use asterisk-partials)</param>
		/// <param name="notromtypes">Type of the roms to match</param>
		/// <param name="notcrcs">CRCs of the roms to match (can use asterisk-partials)</param>
		/// <param name="notmd5s">MD5s of the roms to match (can use asterisk-partials)</param>
		/// <param name="notsha1s">SHA-1s of the roms to match (can use asterisk-partials)</param>
		/// <param name="itemNotStatuses">Select roms without the given status</param>
		/// <param name="machineNotTypes">Select games without the given type</param>
		/// <param name="runnable">Select games that have a value for runnable</param>
		public Filter(List<string> gamenames, List<string> romnames, List<string> romtypes, long sgt,
			long slt, long seq, List<string> crcs, List<string> md5s, List<string> sha1s, ItemStatus itemStatuses,
			MachineType machineTypes, List<string> notgamenames, List<string> notromnames, List<string> notromtypes,
			List<string> notcrcs, List<string> notmd5s, List<string> notsha1s, ItemStatus itemNotStatuses,
			MachineType machineNotTypes, bool? runnable)
		{
			_gameNames = gamenames;
			_notGameNames = notgamenames;
			_romNames = romnames;
			_notRomNames = notromnames;
			_romTypes = romtypes;
			_notRomTypes = notromtypes;
			_sizeGreaterThanOrEqual = sgt;
			_sizeLessThanOrEqual = slt;
			_sizeEqualTo = seq;
			_crcs = crcs;
			_notCrcs = notcrcs;
			_md5s = md5s;
			_notMd5s = notmd5s;
			_sha1s = sha1s;
			_notSha1s = notsha1s;
			_itemStatuses = itemStatuses;
			_itemNotStatuses = itemNotStatuses;
			_machineTypes = machineTypes;
			_machineNotTypes = machineNotTypes;
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
			// If the item is null, we automatically fail it
			if (item == null)
			{
				return false;
			}

			// If the item's machine is null, we automatically fail it
			if (item.Machine == null)
			{
				return false;
			}

			// Filter on machine type
			if (_machineTypes != MachineType.NULL && (item.Machine.MachineType & _machineTypes) == 0)
			{
				return false;
			}
			if (_machineNotTypes != MachineType.NULL && (item.Machine.MachineType & _machineNotTypes) != 0)
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
				if (_itemStatuses != ItemStatus.NULL && (rom.ItemStatus & _itemStatuses) == 0)
				{
					return false;
				}
				if (_itemNotStatuses != ItemStatus.NULL && (rom.ItemStatus & _itemNotStatuses) != 0)
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
				if (_crcs.Count > 0)
				{
					bool found = true;
					foreach (string crc in _crcs)
					{
						if (!String.IsNullOrEmpty(crc))
						{
							if (crc.StartsWith("*") && crc.EndsWith("*"))
							{
								if (!rom.CRC.ToLowerInvariant().Contains(crc.ToLowerInvariant().Replace("*", String.Empty)))
								{
									found = false;
								}
							}
							else if (crc.StartsWith("*"))
							{
								if (!rom.CRC.EndsWith(crc.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (crc.EndsWith("*"))
							{
								if (!rom.CRC.StartsWith(crc.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else
							{
								if (!String.Equals(rom.CRC, crc, StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
						}
					}

					// If the CRC didn't match, return false
					if (!found)
					{
						return false;
					}
				}
				if (_notCrcs.Count > 0)
				{
					bool found = true;
					foreach (string crc in _notCrcs)
					{
						if (crc.StartsWith("*") && crc.EndsWith("*"))
						{
							if (rom.CRC.ToLowerInvariant().Contains(crc.ToLowerInvariant().Replace("*", String.Empty)))
							{
								found = false;
							}
						}
						else if (crc.StartsWith("*"))
						{
							if (rom.CRC.EndsWith(crc.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (crc.EndsWith("*"))
						{
							if (rom.CRC.StartsWith(crc.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else
						{
							if (String.Equals(rom.CRC, crc, StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
					}

					// If the CRC matched, return false
					if (!found)
					{
						return false;
					}
				}

				// Filter on MD5
				if (_md5s.Count > 0)
				{
					bool found = true;
					foreach (string md5 in _md5s)
					{
						if (!String.IsNullOrEmpty(md5))
						{
							if (md5.StartsWith("*") && md5.EndsWith("*"))
							{
								if (!rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", String.Empty)))
								{
									found = false;
								}
							}
							else if (md5.StartsWith("*"))
							{
								if (!rom.MD5.EndsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (md5.EndsWith("*"))
							{
								if (!rom.MD5.StartsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else
							{
								if (!String.Equals(rom.MD5, md5, StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
						}
					}

					// If the MD5 didn't match, return false
					if (!found)
					{
						return false;
					}
				}
				if (_notMd5s.Count > 0)
				{
					bool found = true;
					foreach (string md5 in _notMd5s)
					{
						if (md5.StartsWith("*") && md5.EndsWith("*"))
						{
							if (rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", String.Empty)))
							{
								found = false;
							}
						}
						else if (md5.StartsWith("*"))
						{
							if (rom.MD5.EndsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (md5.EndsWith("*"))
						{
							if (rom.MD5.StartsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else
						{
							if (String.Equals(rom.MD5, md5, StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
					}

					// If the MD5 matched, return false
					if (!found)
					{
						return false;
					}
				}

				// Filter on SHA1
				if (_sha1s.Count > 0)
				{
					bool found = true;
					foreach (string sha1 in _sha1s)
					{
						if (!String.IsNullOrEmpty(sha1))
						{
							if (sha1.StartsWith("*") && sha1.EndsWith("*"))
							{
								if (!rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", String.Empty)))
								{
									found = false;
								}
							}
							else if (sha1.StartsWith("*"))
							{
								if (!rom.SHA1.EndsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (sha1.EndsWith("*"))
							{
								if (!rom.SHA1.StartsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else
							{
								if (!String.Equals(rom.SHA1, sha1, StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
						}
					}

					// If the SHA1 didn't match, return false
					if (!found)
					{
						return false;
					}
				}
				if (_notSha1s.Count > 0)
				{
					bool found = true;
					foreach (string sha1 in _notSha1s)
					{
						if (sha1.StartsWith("*") && sha1.EndsWith("*"))
						{
							if (rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", String.Empty)))
							{
								found = false;
							}
						}
						else if (sha1.StartsWith("*"))
						{
							if (rom.SHA1.EndsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (sha1.EndsWith("*"))
						{
							if (rom.SHA1.StartsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else
						{
							if (String.Equals(rom.SHA1, sha1, StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
					}

					// If the SHA1 matched, return false
					if (!found)
					{
						return false;
					}
				}
			}
			else if (item.Type == ItemType.Disk)
			{
				Disk rom = (Disk)item;

				// Filter on status
				if (_itemStatuses != ItemStatus.NULL && (rom.ItemStatus & _itemStatuses) == 0)
				{
					return false;
				}
				if (_itemNotStatuses != ItemStatus.NULL && (rom.ItemStatus & _itemNotStatuses) != 0)
				{
					return false;
				}

				// Filter on MD5
				if (_md5s.Count > 0)
				{
					bool found = true;
					foreach (string md5 in _md5s)
					{
						if (!String.IsNullOrEmpty(md5))
						{
							if (md5.StartsWith("*") && md5.EndsWith("*"))
							{
								if (!rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", String.Empty)))
								{
									found = false;
								}
							}
							else if (md5.StartsWith("*"))
							{
								if (!rom.MD5.EndsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (md5.EndsWith("*"))
							{
								if (!rom.MD5.StartsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else
							{
								if (!String.Equals(rom.MD5, md5, StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
						}
					}

					// If the MD5 didn't match, return false
					if (!found)
					{
						return false;
					}
				}
				if (_notMd5s.Count > 0)
				{
					bool found = true;
					foreach (string md5 in _notMd5s)
					{
						if (md5.StartsWith("*") && md5.EndsWith("*"))
						{
							if (rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", String.Empty)))
							{
								found = false;
							}
						}
						else if (md5.StartsWith("*"))
						{
							if (rom.MD5.EndsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (md5.EndsWith("*"))
						{
							if (rom.MD5.StartsWith(md5.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else
						{
							if (String.Equals(rom.MD5, md5, StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
					}

					// If the MD5 matched, return false
					if (!found)
					{
						return false;
					}
				}

				// Filter on SHA1
				if (_sha1s.Count > 0)
				{
					bool found = true;
					foreach (string sha1 in _sha1s)
					{
						if (!String.IsNullOrEmpty(sha1))
						{
							if (sha1.StartsWith("*") && sha1.EndsWith("*"))
							{
								if (!rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", String.Empty)))
								{
									found = false;
								}
							}
							else if (sha1.StartsWith("*"))
							{
								if (!rom.SHA1.EndsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (sha1.EndsWith("*"))
							{
								if (!rom.SHA1.StartsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else
							{
								if (!String.Equals(rom.SHA1, sha1, StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
						}
					}

					// If the SHA1 didn't match, return false
					if (!found)
					{
						return false;
					}
				}
				if (_notSha1s.Count > 0)
				{
					bool found = true;
					foreach (string sha1 in _notSha1s)
					{
						if (sha1.StartsWith("*") && sha1.EndsWith("*"))
						{
							if (rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", String.Empty)))
							{
								found = false;
							}
						}
						else if (sha1.StartsWith("*"))
						{
							if (rom.SHA1.EndsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (sha1.EndsWith("*"))
						{
							if (rom.SHA1.StartsWith(sha1.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else
						{
							if (String.Equals(rom.SHA1, sha1, StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
					}

					// If the SHA1 matched, return false
					if (!found)
					{
						return false;
					}
				}
			}

			// Filter on game name
			if (_gameNames.Count > 0)
			{
				bool found = true;
				foreach (string name in _gameNames)
				{
					if (name.StartsWith("*") && name.EndsWith("*"))
					{
						if (!item.Machine.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Replace("*", String.Empty)))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (!item.Machine.Name.EndsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (!item.Machine.Name.StartsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else
					{
						if (!String.Equals(item.Machine.Name, name, StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
				}

				// If the game name was not matched, return false
				if (!found)
				{
					return false;
				}
			}
			if (_notGameNames.Count > 0)
			{
				bool found = true;
				foreach (string name in _notGameNames)
				{
					if (name.StartsWith("*") && name.EndsWith("*"))
					{
						if (item.Machine.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Replace("*", String.Empty)))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (item.Machine.Name.EndsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (item.Machine.Name.StartsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else
					{
						if (String.Equals(item.Machine.Name, name, StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
				}

				// If the game name was matched, return false
				if (!found)
				{
					return false;
				}
			}

			// Filter on rom name
			if (_romNames.Count > 0)
			{
				bool found = true;
				foreach (string name in _romNames)
				{
					if (name.StartsWith("*") && name.EndsWith("*"))
					{
						if (!item.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Replace("*", String.Empty)))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (!item.Name.EndsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (!item.Name.StartsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else
					{
						if (!String.Equals(item.Name, name, StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
				}

				// If the rom name was not matched, return false
				if (!found)
				{
					return false;
				}
			}
			if (_notRomNames.Count > 0)
			{
				bool found = true;
				foreach (string name in _notRomNames)
				{
					if (name.StartsWith("*") && name.EndsWith("*"))
					{
						if (item.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Replace("*", String.Empty)))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (item.Name.EndsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (item.Name.StartsWith(name.Replace("*", String.Empty), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else
					{
						if (String.Equals(item.Name, name, StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
				}

				// If the rom name was matched, return false
				if (!found)
				{
					return false;
				}
			}

			// Filter on rom type
			if (_romTypes.Count == 0 && _notRomTypes.Count == 0 && item.Type != ItemType.Rom && item.Type != ItemType.Disk)
			{
				return false;
			}
			if (_romTypes.Count > 0)
			{
				bool found = true;
				foreach (string type in _romTypes)
				{
					if (!String.Equals(item.Type.ToString(), type, StringComparison.InvariantCultureIgnoreCase))
					{
						found = false;
					}
				}
				
				// If the rom type was not found, return false
				if (!found)
				{
					return false;
				}
			}
			if (_notRomTypes.Count > 0)
			{
				bool found = true;
				foreach (string type in _notRomTypes)
				{
					if (String.Equals(item.Type.ToString(), type, StringComparison.InvariantCultureIgnoreCase))
					{
						found = false;
					}
				}

				// If the rom type was found, return false
				if (!found)
				{
					return false;
				}
			}

			return true;
		}
	}
}
