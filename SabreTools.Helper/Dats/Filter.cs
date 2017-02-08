using System;
using System.Collections.Generic;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Dats
{
	public class Filter
	{
		#region Private instance variables

		#region Positive
		
		private List<string> _gameNames;
		private List<string> _romNames;
		private List<string> _romTypes;
		private List<string> _crcs;
		private List<string> _md5s;
		private List<string> _sha1s;
		private ItemStatus _itemStatuses;
		private MachineType _machineTypes;

		#endregion

		#region Negative

		private List<string> _notGameNames;
		private List<string> _notRomNames;
		private List<string> _notRomTypes;
		private List<string> _notCrcs;
		private List<string> _notMd5s;
		private List<string> _notSha1s;
		private ItemStatus _itemNotStatuses;
		private MachineType _machineNotTypes;

		#endregion

		#region Neutral

		private long _sizeGreaterThanOrEqual;
		private long _sizeLessThanOrEqual;
		private long _sizeEqualTo;
		private bool _includeOfInGame;
		private bool? _runnable;

		#endregion

		#endregion // Private instance variables

		#region Pubically facing variables

		#region Positive

		public List<string> GameNames
		{
			get { return _gameNames; }
			set { _gameNames = value; }
		}
		public List<string> RomNames
		{
			get { return _romNames; }
			set { _romNames = value; }
		}
		public List<string> RomTypes
		{
			get { return _romTypes; }
			set { _romTypes = value; }
		}
		public List<string> CRCs
		{
			get { return _crcs; }
			set { _crcs = value; }
		}
		public List<string> MD5s
		{
			get { return _md5s; }
			set { _md5s = value; }
		}
		public List<string> SHA1s
		{
			get { return _sha1s; }
			set { _sha1s = value; }
		}
		public ItemStatus ItemStatuses
		{
			get { return _itemStatuses; }
			set { _itemStatuses = value; }
		}
		public MachineType MachineTypes
		{
			get { return _machineTypes; }
			set { _machineTypes = value; }
		}

		#endregion

		#region Negative

		public List<string> NotGameNames
		{
			get { return _notGameNames; }
			set { _notGameNames = value; }
		}
		public List<string> NotRomNames
		{
			get { return _notRomNames; }
			set { _notRomNames = value; }
		}
		public List<string> NotRomTypes
		{
			get { return _notRomTypes; }
			set { _notRomTypes = value; }
		}
		public List<string> NotCRCs
		{
			get { return _notCrcs; }
			set { _notCrcs = value; }
		}
		public List<string> NotMD5s
		{
			get { return _notMd5s; }
			set { _notMd5s = value; }
		}
		public List<string> NotSHA1s
		{
			get { return _notSha1s; }
			set { _notSha1s = value; }
		}
		public ItemStatus NotItemStatuses
		{
			get { return _itemNotStatuses; }
			set { _itemNotStatuses = value; }
		}
		public MachineType NotMachineTypes
		{
			get { return _machineNotTypes; }
			set { _machineNotTypes = value; }
		}

		#endregion

		#region Neutral

		public long SizeGreaterThanOrEqual
		{
			get { return _sizeGreaterThanOrEqual; }
			set { _sizeGreaterThanOrEqual = value; }
		}
		public long SizeLessThanOrEqual
		{
			get { return _sizeLessThanOrEqual; }
			set { _sizeLessThanOrEqual = value; }
		}
		public long SizeEqualTo
		{
			get { return _sizeEqualTo; }
			set { _sizeEqualTo = value; }
		}
		public bool IncludeOfInGame
		{
			get { return _includeOfInGame; }
			set { _includeOfInGame = value; }
		}
		public bool? Runnable
		{
			get { return _runnable; }
			set { _runnable = value; }
		}

		#endregion

		#endregion // Pubically facing variables

		#region Constructors

		/// <summary>
		/// Create an empty Filter object
		/// </summary>
		public Filter()
		{
			// Positive
			_gameNames = new List<string>();
			_romNames = new List<string>();
			_romTypes = new List<string>();
			_crcs = new List<string>();
			_md5s = new List<string>();
			_sha1s = new List<string>();
			_itemStatuses = ItemStatus.NULL;
			_machineTypes = MachineType.NULL;

			// Negative
			_notGameNames = new List<string>();
			_notRomNames = new List<string>();
			_notRomTypes = new List<string>();
			_notCrcs = new List<string>();
			_notMd5s = new List<string>();
			_notSha1s = new List<string>();
			_itemNotStatuses = ItemStatus.NULL;
			_machineNotTypes = MachineType.NULL;

			// Neutral
			_sizeGreaterThanOrEqual = -1;
			_sizeLessThanOrEqual = -1;
			_sizeEqualTo = -1;
			_includeOfInGame = false;
			_runnable = null;
		}

		#endregion

		#region Instance methods

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
								if (!rom.CRC.ToLowerInvariant().Contains(crc.ToLowerInvariant().Replace("*", "")))
								{
									found = false;
								}
							}
							else if (crc.StartsWith("*"))
							{
								if (!rom.CRC.EndsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (crc.EndsWith("*"))
							{
								if (!rom.CRC.StartsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
							if (rom.CRC.ToLowerInvariant().Contains(crc.ToLowerInvariant().Replace("*", "")))
							{
								found = false;
							}
						}
						else if (crc.StartsWith("*"))
						{
							if (rom.CRC.EndsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (crc.EndsWith("*"))
						{
							if (rom.CRC.StartsWith(crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
								if (!rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", "")))
								{
									found = false;
								}
							}
							else if (md5.StartsWith("*"))
							{
								if (!rom.MD5.EndsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (md5.EndsWith("*"))
							{
								if (!rom.MD5.StartsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
							if (rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", "")))
							{
								found = false;
							}
						}
						else if (md5.StartsWith("*"))
						{
							if (rom.MD5.EndsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (md5.EndsWith("*"))
						{
							if (rom.MD5.StartsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
								if (!rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", "")))
								{
									found = false;
								}
							}
							else if (sha1.StartsWith("*"))
							{
								if (!rom.SHA1.EndsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (sha1.EndsWith("*"))
							{
								if (!rom.SHA1.StartsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
							if (rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", "")))
							{
								found = false;
							}
						}
						else if (sha1.StartsWith("*"))
						{
							if (rom.SHA1.EndsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (sha1.EndsWith("*"))
						{
							if (rom.SHA1.StartsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
								if (!rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", "")))
								{
									found = false;
								}
							}
							else if (md5.StartsWith("*"))
							{
								if (!rom.MD5.EndsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (md5.EndsWith("*"))
							{
								if (!rom.MD5.StartsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
							if (rom.MD5.ToLowerInvariant().Contains(md5.ToLowerInvariant().Replace("*", "")))
							{
								found = false;
							}
						}
						else if (md5.StartsWith("*"))
						{
							if (rom.MD5.EndsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (md5.EndsWith("*"))
						{
							if (rom.MD5.StartsWith(md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
								if (!rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", "")))
								{
									found = false;
								}
							}
							else if (sha1.StartsWith("*"))
							{
								if (!rom.SHA1.EndsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
								{
									found = false;
								}
							}
							else if (sha1.EndsWith("*"))
							{
								if (!rom.SHA1.StartsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
							if (rom.SHA1.ToLowerInvariant().Contains(sha1.ToLowerInvariant().Replace("*", "")))
							{
								found = false;
							}
						}
						else if (sha1.StartsWith("*"))
						{
							if (rom.SHA1.EndsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
							{
								found = false;
							}
						}
						else if (sha1.EndsWith("*"))
						{
							if (rom.SHA1.StartsWith(sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
						if (_includeOfInGame
							? (!item.Machine.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*')))
								&& (!item.Machine.CloneOf.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*')))
								&& (!item.Machine.RomOf.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*')))
							: (!item.Machine.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*'))))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (_includeOfInGame
							? (!item.Machine.Name.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								&& (!item.Machine.CloneOf.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								&& (!item.Machine.RomOf.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
							: (!item.Machine.Name.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase)))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (_includeOfInGame
							? (!item.Machine.Name.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								&& (!item.Machine.CloneOf.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								&& (!item.Machine.RomOf.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
							: (!item.Machine.Name.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase)))
						{
							found = false;
						}
					}
					else
					{
						if (_includeOfInGame
							? (!String.Equals(item.Machine.Name, name, StringComparison.InvariantCultureIgnoreCase))
								&& (!String.Equals(item.Machine.CloneOf, name, StringComparison.InvariantCultureIgnoreCase))
								&& (!String.Equals(item.Machine.RomOf, name, StringComparison.InvariantCultureIgnoreCase))
							: (!String.Equals(item.Machine.Name, name, StringComparison.InvariantCultureIgnoreCase)))
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
						if (_includeOfInGame
							? (item.Machine.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*')))
								|| (item.Machine.CloneOf.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*')))
								|| (item.Machine.RomOf.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*')))
							: (item.Machine.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Trim('*'))))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (_includeOfInGame
							? (item.Machine.Name.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								|| (item.Machine.CloneOf.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								|| (item.Machine.RomOf.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
							: (item.Machine.Name.EndsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase)))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (_includeOfInGame
							? (item.Machine.Name.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								|| (item.Machine.CloneOf.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
								|| (item.Machine.RomOf.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase))
							: (item.Machine.Name.StartsWith(name.Trim('*'), StringComparison.InvariantCultureIgnoreCase)))
						{
							found = false;
						}
					}
					else
					{
						if (_includeOfInGame
							? (String.Equals(item.Machine.Name, name, StringComparison.InvariantCultureIgnoreCase))
								|| (String.Equals(item.Machine.CloneOf, name, StringComparison.InvariantCultureIgnoreCase))
								|| (String.Equals(item.Machine.RomOf, name, StringComparison.InvariantCultureIgnoreCase))
							: (String.Equals(item.Machine.Name, name, StringComparison.InvariantCultureIgnoreCase)))
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
						if (!item.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Replace("*", "")))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (!item.Name.EndsWith(name.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (!item.Name.StartsWith(name.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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
						if (item.Name.ToLowerInvariant().Contains(name.ToLowerInvariant().Replace("*", "")))
						{
							found = false;
						}
					}
					else if (name.StartsWith("*"))
					{
						if (item.Name.EndsWith(name.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							found = false;
						}
					}
					else if (name.EndsWith("*"))
					{
						if (item.Name.StartsWith(name.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
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

		#endregion

		#region Static methods

		/// <summary>
		/// Get the machine type from a string
		/// </summary>
		/// <param name="gametype">Machine type as a string</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>A machine type based on the input</returns>
		public static MachineType GetMachneTypeFromString(string gametype, Logger logger)
		{
			MachineType machineType = MachineType.NULL;
			switch (gametype.ToLowerInvariant())
			{
				case "none":
					machineType |= MachineType.None;
					break;
				case "bios":
					machineType |= MachineType.Bios;
					break;
				case "dev":
				case "device":
					machineType |= MachineType.Device;
					break;
				case "mech":
				case "mechanical":
					machineType |= MachineType.Mechanical;
					break;
				default:
					logger.Warning(gametype + " is not a valid type");
					break;
			}

			return machineType;
		}

		/// <summary>
		/// Get the item status from a string
		/// </summary>
		/// <param name="status">Item status as a string</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>An item status based on the input</returns>
		public static ItemStatus GetStatusFromString(string status, Logger logger)
		{
			ItemStatus itemStatus = ItemStatus.NULL;
			switch (status.ToLowerInvariant())
			{
				case "none":
					itemStatus |= ItemStatus.None;
					break;
				case "good":
					itemStatus |= ItemStatus.Good;
					break;
				case "baddump":
					itemStatus |= ItemStatus.BadDump;
					break;
				case "nodump":
					itemStatus |= ItemStatus.Nodump;
					break;
				case "verified":
					itemStatus |= ItemStatus.Verified;
					break;
				default:
					logger.Warning(status + " is not a valid status");
					break;
			}

			return itemStatus;
		}

		#endregion
	}
}
