using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;

namespace SabreTools.Library.DatFiles
{
	/// <summary>
	/// Represents the filtering operations that need to be performed on a set of items, usually a DAT
	/// </summary>
	public class Filter
	{
		#region Private instance variables

		#region Positive
		
		private List<string> _gameNames;
		private List<string> _gameDescriptions;
		private List<string> _romNames;
		private List<string> _romTypes;
		private List<string> _crcs;
		private List<string> _md5s;
		private List<string> _sha1s;
		private List<string> _sha256s;
		private List<string> _sha384s;
		private List<string> _sha512s;
		private ItemStatus _itemStatuses;
		private MachineType _machineTypes;

		#endregion

		#region Negative

		private List<string> _notGameNames;
		private List<string> _notGameDescriptions;
		private List<string> _notRomNames;
		private List<string> _notRomTypes;
		private List<string> _notCrcs;
		private List<string> _notMd5s;
		private List<string> _notSha1s;
		private List<string> _notSha256s;
		private List<string> _notSha384s;
		private List<string> _notSha512s;
		private ItemStatus _itemNotStatuses;
		private MachineType _machineNotTypes;

		#endregion

		#region Neutral

		private long _sizeGreaterThanOrEqual;
		private long _sizeLessThanOrEqual;
		private long _sizeEqualTo;
		private bool _includeOfInGame;
		private bool? _runnable;
		private bool _single;
		private bool _trim;
		private string _root;

		#endregion

		#endregion // Private instance variables

		#region Pubically facing variables

		#region Positive

		public List<string> MachineNames
		{
			get { return _gameNames; }
			set { _gameNames = value; }
		}
		public List<string> MachineDescriptions
		{
			get { return _gameDescriptions; }
			set { _gameDescriptions = value; }
		}
		public List<string> ItemNames
		{
			get { return _romNames; }
			set { _romNames = value; }
		}
		public List<string> ItemTypes
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
		public List<string> SHA256s
		{
			get { return _sha256s; }
			set { _sha256s = value; }
		}
		public List<string> SHA384s
		{
			get { return _sha384s; }
			set { _sha384s = value; }
		}
		public List<string> SHA512s
		{
			get { return _sha512s; }
			set { _sha512s = value; }
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

		public List<string> NotMachineNames
		{
			get { return _notGameNames; }
			set { _notGameNames = value; }
		}
		public List<string> NotMachineDescriptions
		{
			get { return _notGameDescriptions; }
			set { _notGameDescriptions = value; }
		}
		public List<string> NotItemNames
		{
			get { return _notRomNames; }
			set { _notRomNames = value; }
		}
		public List<string> NotItemTypes
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
		public List<string> NotSHA256s
		{
			get { return _notSha256s; }
			set { _notSha256s = value; }
		}
		public List<string> NotSHA384s
		{
			get { return _notSha384s; }
			set { _notSha384s = value; }
		}
		public List<string> NotSHA512s
		{
			get { return _notSha512s; }
			set { _notSha512s = value; }
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
		public bool Single
		{
			get { return _single; }
			set { _single = value; }
		}
		public bool Trim
		{
			get { return _trim; }
			set { _trim = value; }
		}
		public string Root
		{
			get { return _root; }
			set { _root = value; }
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
			_gameDescriptions = new List<string>();
			_romNames = new List<string>();
			_romTypes = new List<string>();
			_crcs = new List<string>();
			_md5s = new List<string>();
			_sha1s = new List<string>();
			_sha256s = new List<string>();
			_sha384s = new List<string>();
			_sha512s = new List<string>();
			_itemStatuses = ItemStatus.NULL;
			_machineTypes = MachineType.NULL;

			// Negative
			_notGameNames = new List<string>();
			_notGameDescriptions = new List<string>();
			_notRomNames = new List<string>();
			_notRomTypes = new List<string>();
			_notCrcs = new List<string>();
			_notMd5s = new List<string>();
			_notSha1s = new List<string>();
			_notSha256s = new List<string>();
			_notSha384s = new List<string>();
			_notSha512s = new List<string>();
			_itemNotStatuses = ItemStatus.NULL;
			_machineNotTypes = MachineType.NULL;

			// Neutral
			_sizeGreaterThanOrEqual = -1;
			_sizeLessThanOrEqual = -1;
			_sizeEqualTo = -1;
			_includeOfInGame = false;
			_runnable = null;
			_single = false;
			_trim = false;
			_root = null;
		}

		#endregion

		#region Instance methods

		/// <summary>
		/// Filter a DatFile using the inputs
		/// </summary>
		/// <param name="datFile"></param>
		/// <returns>True if the DatFile was filtered, false on error</returns>
		public bool FilterDatFile(DatFile datFile)
		{
			try
			{
				// Loop over every key in the dictionary
				List<string> keys = datFile.Keys;
				foreach (string key in keys)
				{
					// For every item in the current key
					List<DatItem> items = datFile[key];
					List<DatItem> newitems = new List<DatItem>();
					foreach (DatItem item in items)
					{
						// If the rom passes the filter, include it
						if (ItemPasses(item))
						{
							// If we are in single game mode, rename all games
							if (_single)
							{
								item.MachineName = "!";
							}

							// If we are in NTFS trim mode, trim the game name
							if (_trim)
							{
								// Windows max name length is 260
								int usableLength = 260 - item.MachineName.Length - _root.Length;
								if (item.Name.Length > usableLength)
								{
									string ext = Path.GetExtension(item.Name);
									item.Name = item.Name.Substring(0, usableLength - ext.Length);
									item.Name += ext;
								}
							}

							// Lock the list and add the item back
							lock (newitems)
							{
								newitems.Add(item);
							}
						}
					}

					datFile.Remove(key);
					datFile.AddRange(key, newitems);
				}
			}
			catch (Exception ex)
			{
				Globals.Logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Check to see if a DatItem passes the filter
		/// </summary>
		/// <param name="item">DatItem to check</param>
		/// <returns>True if the file passed the filter, false otherwise</returns>
		public bool ItemPasses(DatItem item)
		{
			// If the item is null, we automatically fail it
			if (item == null)
			{
				return false;
			}

			// Filter on machine type
			if (_machineTypes != MachineType.NULL && (item.MachineType & _machineTypes) == 0)
			{
				return false;
			}
			if (_machineNotTypes != MachineType.NULL && (item.MachineType & _machineNotTypes) != 0)
			{
				return false;
			}

			// Filter on machine runability
			if (_runnable != null && item.Runnable != _runnable)
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
					// If the CRC isn't in the list, return false
					if (!FindValueInList(_crcs, rom.CRC))
					{
						return false;
					}
				}
				if (_notCrcs.Count > 0)
				{
					// If the CRC is in the list, return false
					if (FindValueInList(_notCrcs, rom.CRC))
					{
						return false;
					}
				}

				// Filter on MD5
				if (_md5s.Count > 0)
				{
					// If the MD5 isn't in the list, return false
					if (!FindValueInList(_md5s, rom.MD5))
					{
						return false;
					}
				}
				if (_notMd5s.Count > 0)
				{
					// If the MD5 is in the list, return false
					if (FindValueInList(_notMd5s, rom.MD5))
					{
						return false;
					}
				}

				// Filter on SHA1
				if (_sha1s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha1s, rom.SHA1))
					{
						return false;
					}
				}
				if (_notSha1s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha1s, rom.SHA1))
					{
						return false;
					}
				}

				// Filter on SHA256
				if (_sha256s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha256s, rom.SHA256))
					{
						return false;
					}
				}
				if (_notSha256s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha256s, rom.SHA256))
					{
						return false;
					}
				}

				// Filter on SHA384
				if (_sha384s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha384s, rom.SHA384))
					{
						return false;
					}
				}
				if (_notSha384s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha384s, rom.SHA384))
					{
						return false;
					}
				}

				// Filter on SHA512
				if (_sha512s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha512s, rom.SHA512))
					{
						return false;
					}
				}
				if (_notSha512s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha512s, rom.SHA512))
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
					// If the MD5 isn't in the list, return false
					if (!FindValueInList(_md5s, rom.MD5))
					{
						return false;
					}
				}
				if (_notMd5s.Count > 0)
				{
					// If the MD5 is in the list, return false
					if (FindValueInList(_notMd5s, rom.MD5))
					{
						return false;
					}
				}

				// Filter on SHA1
				if (_sha1s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha1s, rom.SHA1))
					{
						return false;
					}
				}
				if (_notSha1s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha1s, rom.SHA1))
					{
						return false;
					}
				}

				// Filter on SHA256
				if (_sha256s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha256s, rom.SHA256))
					{
						return false;
					}
				}
				if (_notSha256s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha256s, rom.SHA256))
					{
						return false;
					}
				}

				// Filter on SHA384
				if (_sha384s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha384s, rom.SHA384))
					{
						return false;
					}
				}
				if (_notSha384s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha384s, rom.SHA384))
					{
						return false;
					}
				}

				// Filter on SHA512
				if (_sha512s.Count > 0)
				{
					// If the SHA-1 isn't in the list, return false
					if (!FindValueInList(_sha512s, rom.SHA512))
					{
						return false;
					}
				}
				if (_notSha512s.Count > 0)
				{
					// If the SHA-1 is in the list, return false
					if (FindValueInList(_notSha512s, rom.SHA512))
					{
						return false;
					}
				}
			}

			// Filter on game name
			if (_gameNames.Count > 0)
			{
				bool found = FindValueInList(_gameNames, item.MachineName);

				// If we are checking CloneOf and RomOf, add them in as well
				if (_includeOfInGame)
				{
					found |= FindValueInList(_gameNames, item.CloneOf);
					found |= FindValueInList(_gameNames, item.RomOf);
				}

				// If the game name was not found in the list, return false
				if (!found)
				{
					return false;
				}
			}
			if (_notGameNames.Count > 0)
			{
				bool found = FindValueInList(_notGameNames, item.MachineName);

				// If we are checking CloneOf and RomOf, add them in as well
				if (_includeOfInGame)
				{
					found |= FindValueInList(_notGameNames, item.CloneOf);
					found |= FindValueInList(_notGameNames, item.RomOf);
				}

				// If the game name was found in the list, return false
				if (found)
				{
					return false;
				}
			}

			// Filter on game description
			if (_gameDescriptions.Count > 0)
			{
				bool found = FindValueInList(_gameDescriptions, item.MachineDescription);

				// If the game description was not found in the list, return false
				if (!found)
				{
					return false;
				}
			}
			if (_notGameDescriptions.Count > 0)
			{
				bool found = FindValueInList(_notGameDescriptions, item.MachineDescription);

				// If the game description was found in the list, return false
				if (found)
				{
					return false;
				}
			}

			// Filter on rom name
			if (_romNames.Count > 0)
			{
				// If the rom name was not found in the list, return false
				if (!FindValueInList(_romNames, item.Name))
				{
					return false;
				}
			}
			if (_notRomNames.Count > 0)
			{
				// If the rom name was found in the list, return false
				if (FindValueInList(_notRomNames, item.Name))
				{
					return false;
				}
			}

			// Filter on rom type
			if (_romTypes.Count == 0 && _notRomTypes.Count == 0 && item.Type != ItemType.Rom && item.Type != ItemType.Disk && item.Type != ItemType.Blank)
			{
				return false;
			}
			if (_romTypes.Count > 0)
			{
				// If the rom type was not found in the list, return false
				if (!FindValueInList(_romTypes, item.Type.ToString()))
				{
					return false;
				}
			}
			if (_notRomTypes.Count > 0)
			{
				// If the rom type was found in the list, return false
				if (FindValueInList(_notRomTypes, item.Type.ToString()))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Generic code to check if a specific value is in the list given
		/// </summary>
		/// <param name="haystack">List to search for the value in</param>
		/// <param name="needle">Value to search the list for</param>
		/// <returns>True if the value could be found, false otherwise</returns>
		private bool FindValueInList(List<string> haystack, string needle)
		{
			bool found = false;
			foreach (string straw in haystack)
			{
				if (!String.IsNullOrWhiteSpace(straw))
				{
					string regexStraw = straw;

					// If the straw has no special characters at all, treat it as an exact match
					if (regexStraw == Regex.Escape(regexStraw))
					{
						regexStraw = "^" + regexStraw + "$";
					}

					// Check if a match is found with the regex
					found |= Regex.IsMatch(needle, regexStraw, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
				}
			}

			return found;
		}

		#endregion
	}
}
