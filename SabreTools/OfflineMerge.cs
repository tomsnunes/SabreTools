using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SabreTools
{
	public class OfflineMerge
	{
		// Instance variables
		private string _currentAllMerged;
		private string _currentMissingMerged;
		private string _currentNewMerged;
		private bool _fake;
		private Logger _logger;

		/// <summary>
		/// Instantiate an OfflineMerge object
		/// </summary>
		/// <param name="currentAllMerged">Old-current DAT with merged and deduped values</param>
		/// <param name="currentMissingMerged">Old-current missing DAT with merged and deduped values</param>
		/// <param name="currentNewMerged">New-current DAT with merged and deduped values</param>
		/// <param name="fake">True if all values should be replaced with default 0-byte values, false otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>
		public OfflineMerge (string currentAllMerged, string currentMissingMerged, string currentNewMerged, bool fake, Logger logger)
		{
			_currentAllMerged = currentAllMerged;
			_currentMissingMerged = currentMissingMerged;
			_currentNewMerged = currentNewMerged;
			_fake = fake;
			_logger = logger;
		}

		/// <summary>
		/// Process the supplied inputs and create the four outputs
		/// </summary>
		/// <returns>True if the files were created properly, false otherwise</returns>
		public bool Process()
		{
			// Check all of the files for validity and break if one doesn't exist
			if (_currentAllMerged != "" && !File.Exists(_currentAllMerged))
			{
				return false;
			}
			if (_currentMissingMerged != "" && !File.Exists(_currentMissingMerged))
			{
				return false;
			}
			if (_currentNewMerged != "" && !File.Exists(_currentNewMerged))
			{
				return false;
			}

			// If we have all three DATs, then generate everything
			if (_currentAllMerged != "" && _currentMissingMerged != "" && _currentNewMerged != "")
			{
				// First get the combination Dictionary of currentAllMerged and currentNewMerged
				_logger.User("Adding Current and New Merged DATs to the dictionary");
				Dat completeDats = new Dat();
				completeDats = DatTools.Parse(_currentAllMerged, 0, 0, completeDats, _logger);
				completeDats = DatTools.Parse(_currentNewMerged, 0, 0, completeDats, _logger);

				// Now get Net New output dictionary [(currentNewMerged)-(currentAllMerged)]
				_logger.User("Creating and populating Net New dictionary");
				Dictionary<string, List<Rom>> netNew = new Dictionary<string, List<Rom>>();
				foreach (string key in completeDats.Roms.Keys)
				{
					List<Rom> templist = RomTools.Merge(completeDats.Roms[key], _logger);
					foreach (Rom rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.Metadata.System == _currentNewMerged)
						{
							if (netNew.ContainsKey(key))
							{
								netNew[key].Add(rom);
							}
							else
							{
								List<Rom> temp = new List<Rom>();
								temp.Add(rom);
								netNew.Add(key, temp);
							}
						}
					}
				}

				// Now create the Unneeded dictionary [(currentAllMerged)-(currentNewMerged)]
				_logger.User("Creating and populating Uneeded dictionary");
				Dictionary<string, List<Rom>> unneeded = new Dictionary<string, List<Rom>>();
				foreach (string key in completeDats.Roms.Keys)
				{
					List<Rom> templist = RomTools.Merge(completeDats.Roms[key], _logger);
					foreach (Rom rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.Metadata.System == _currentAllMerged)
						{
							if (unneeded.ContainsKey(key))
							{
								unneeded[key].Add(rom);
							}
							else
							{
								List<Rom> temp = new List<Rom>();
								temp.Add(rom);
								unneeded.Add(key, temp);
							}
						}
					}
				}

				// Now create the New Missing dictionary [(Net New)+(currentMissingMerged-(Unneeded))]
				_logger.User("Creating and populating New Missing dictionary");
				Dat midMissing = new Dat();
				midMissing = DatTools.Parse(_currentMissingMerged, 0, 0, midMissing, _logger);
				foreach (string key in unneeded.Keys)
				{
					if (midMissing.Roms.ContainsKey(key))
					{
						midMissing.Roms[key].AddRange(unneeded[key]);
					}
					else
					{
						midMissing.Roms.Add(key, unneeded[key]);
					}
				}
				Dictionary<string, List<Rom>> newMissing = new Dictionary<string, List<Rom>>();
				foreach (string key in midMissing.Roms.Keys)
				{
					List<Rom> templist = RomTools.Merge(midMissing.Roms[key], _logger);
					foreach (Rom rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.Metadata.System == _currentMissingMerged)
						{
							if (newMissing.ContainsKey(key))
							{
								newMissing[key].Add(rom);
							}
							else
							{
								List<Rom> temp = new List<Rom>();
								temp.Add(rom);
								newMissing.Add(key, temp);
							}
						}
					}
				}
				foreach (string key in netNew.Keys)
				{
					if (newMissing.ContainsKey(key))
					{
						newMissing[key].AddRange(netNew[key]);
					}
					else
					{
						newMissing.Add(key, netNew[key]);
					}
				}

				// Now create the Have dictionary [(currentNewMerged)-(c)]
				_logger.User("Creating and populating Have dictionary");
				Dictionary<string, List<Rom>> midHave = new Dictionary<string, List<Rom>>();
				foreach (string key in newMissing.Keys)
				{
					if (midHave.ContainsKey(key))
					{
						midHave[key].AddRange(newMissing[key]);
					}
					else
					{
						midHave.Add(key, newMissing[key]);
					}
				}
				foreach (string key in completeDats.Roms.Keys)
				{
					if (midHave.ContainsKey(key))
					{
						foreach (Rom rom in completeDats.Roms[key])
						{
							if (rom.Metadata.System == _currentNewMerged)
							{
								midHave[key].Add(rom);
							}
						}
					}
					else
					{
						List<Rom> roms = new List<Rom>();
						foreach (Rom rom in completeDats.Roms[key])
						{
							if (rom.Metadata.System == _currentNewMerged)
							{
								roms.Add(rom);
							}
						}
						midHave.Add(key, roms);
					}
				}
				Dictionary<string, List<Rom>> have = new Dictionary<string, List<Rom>>();
				foreach (string key in midHave.Keys)
				{
					List<Rom> templist = RomTools.Merge(midHave[key], _logger);
					foreach (Rom rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.Metadata.System == _currentNewMerged)
						{
							if (have.ContainsKey(key))
							{
								have[key].Add(rom);
							}
							else
							{
								List<Rom> temp = new List<Rom>();
								temp.Add(rom);
								have.Add(key, temp);
							}
						}
					}
				}

				// If we are supposed to replace everything in the output with default values, do so
				if (_fake)
				{
					_logger.User("Replacing all hashes in Net New with 0-byte values");
					List<string> keys = netNew.Keys.ToList();
					foreach (string key in keys)
					{
						List<Rom> temp = new List<Rom>();
						List<Rom> roms = netNew[key];
						for (int i = 0; i < roms.Count; i++)
						{
							Rom rom = roms[i];
							rom.HashData.Size = Constants.SizeZero;
							rom.HashData.CRC = Constants.CRCZero;
							rom.HashData.MD5 = Constants.MD5Zero;
							rom.HashData.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						netNew[key] = temp;
					}

					_logger.User("Replacing all hashes in Unneeded with 0-byte values");
					keys = unneeded.Keys.ToList();
					foreach (string key in keys)
					{
						List<Rom> temp = new List<Rom>();
						List<Rom> roms = unneeded[key];
						for (int i = 0; i < roms.Count; i++)
						{
							Rom rom = roms[i];
							rom.HashData.Size = Constants.SizeZero;
							rom.HashData.CRC = Constants.CRCZero;
							rom.HashData.MD5 = Constants.MD5Zero;
							rom.HashData.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						unneeded[key] = temp;
					}

					_logger.User("Replacing all hashes in New Missing with 0-byte values");
					keys = newMissing.Keys.ToList();
					foreach (string key in keys)
					{
						List<Rom> temp = new List<Rom>();
						List<Rom> roms = newMissing[key];
						for (int i = 0; i < roms.Count; i++)
						{
							Rom rom = roms[i];
							rom.HashData.Size = Constants.SizeZero;
							rom.HashData.CRC = Constants.CRCZero;
							rom.HashData.MD5 = Constants.MD5Zero;
							rom.HashData.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						newMissing[key] = temp;
					}

					_logger.User("Replacing all hashes in Have with 0-byte values");
					keys = have.Keys.ToList();
					foreach (string key in keys)
					{
						List<Rom> temp = new List<Rom>();
						List<Rom> roms = have[key];
						for (int i = 0; i < roms.Count; i++)
						{
							Rom rom = roms[i];
							rom.HashData.Size = Constants.SizeZero;
							rom.HashData.CRC = Constants.CRCZero;
							rom.HashData.MD5 = Constants.MD5Zero;
							rom.HashData.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						have[key] = temp;
					}
				}

				// Finally, output all of the files
				Dat netNewData = new Dat
				{
					Name = "Net New",
					Description = "Net New",
					Version = "",
					Date = DateTime.Now.ToString("yyyy-MM-dd"),
					Category = "",
					Author = "SabreTools",
					ForcePacking = ForcePacking.None,
					OutputFormat = OutputFormat.Xml,
					MergeRoms = true,
					Roms = netNew,
				};
				Dat unneededData = new Dat
				{
					Name = "Unneeded",
					Description = "Unneeded",
					Version = "",
					Date = DateTime.Now.ToString("yyyy-MM-dd"),
					Category = "",
					Author = "SabreTools",
					ForcePacking = ForcePacking.None,
					OutputFormat = OutputFormat.Xml,
					MergeRoms = true,
					Roms = unneeded,
				};
				Dat newMissingData = new Dat
				{
					Name = "New Missing",
					Description = "New Missing",
					Version = "",
					Date = DateTime.Now.ToString("yyyy-MM-dd"),
					Category = "",
					Author = "SabreTools",
					ForcePacking = ForcePacking.None,
					OutputFormat = OutputFormat.Xml,
					MergeRoms = true,
					Roms = newMissing,
				};
				Dat haveData = new Dat
				{
					Name = "Have",
					Description = "Have",
					Version = "",
					Date = DateTime.Now.ToString("yyyy-MM-dd"),
					Category = "",
					Author = "SabreTools",
					ForcePacking = ForcePacking.None,
					OutputFormat = OutputFormat.Xml,
					MergeRoms = true,
					Roms = have,
				};

				Output.WriteDatfile(netNewData, "", _logger);
				Output.WriteDatfile(unneededData, "", _logger);
				Output.WriteDatfile(newMissingData, "", _logger);
				Output.WriteDatfile(haveData, "", _logger);

				return true;
			}

			// If we only have the old merged and missing, only generate Have
			else if (_currentAllMerged != "" && _currentMissingMerged != "")
			{
				// Now create the Have dictionary [(currentAllMerged)-(currentMissingMerged)]
				_logger.User("Creating and populating Have dictionary");
				Dat midHave = new Dat();
				midHave = DatTools.Parse(_currentMissingMerged, 0, 0, midHave, _logger);
				midHave = DatTools.Parse(_currentAllMerged, 0, 0, midHave, _logger);
				Dictionary<string, List<Rom>> have = new Dictionary<string, List<Rom>>();
				foreach (string key in midHave.Roms.Keys)
				{
					List<Rom> templist = RomTools.Merge(midHave.Roms[key], _logger);
					foreach (Rom rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.Metadata.System == _currentAllMerged)
						{
							if (have.ContainsKey(key))
							{
								have[key].Add(rom);
							}
							else
							{
								List<Rom> temp = new List<Rom>();
								temp.Add(rom);
								have.Add(key, temp);
							}
						}
					}
				}

				// If we are supposed to replace everything in the output with default values, do so
				if (_fake)
				{
					_logger.User("Replacing all hashes in Have with 0-byte values");
					List<string> keys = have.Keys.ToList();
					foreach (string key in keys)
					{
						List<Rom> temp = new List<Rom>();
						List<Rom> roms = have[key];
						for (int i = 0; i < roms.Count; i++)
						{
							Rom rom = roms[i];
							rom.HashData.Size = Constants.SizeZero;
							rom.HashData.CRC = Constants.CRCZero;
							rom.HashData.MD5 = Constants.MD5Zero;
							rom.HashData.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						have[key] = temp;
					}
				}

				Dat haveData = new Dat
				{
					Name = "Have",
					Description = "Have",
					Version = "",
					Date = DateTime.Now.ToString("yyyy-MM-dd"),
					Category = "",
					Author = "SabreTools",
					ForcePacking = ForcePacking.None,
					OutputFormat = OutputFormat.Xml,
					MergeRoms = true,
					Roms = have,
				};
				Output.WriteDatfile(haveData, "", _logger);

				return true;
			}

			// If we only have the new merged and missing, only generate Have
			else if (_currentNewMerged != "" && _currentMissingMerged != "")
			{
				// Now create the Have dictionary [(currentNewMerged)-(currentMissingMerged)]
				_logger.User("Creating and populating Have dictionary");
				Dat midHave = new Dat();
				midHave = DatTools.Parse(_currentMissingMerged, 0, 0, midHave, _logger);
				midHave = DatTools.Parse(_currentNewMerged, 0, 0, midHave, _logger);
				Dictionary<string, List<Rom>> have = new Dictionary<string, List<Rom>>();
				foreach (string key in midHave.Roms.Keys)
				{
					List<Rom> templist = RomTools.Merge(midHave.Roms[key], _logger);
					foreach (Rom rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.Metadata.System == _currentNewMerged)
						{
							if (have.ContainsKey(key))
							{
								have[key].Add(rom);
							}
							else
							{
								List<Rom> temp = new List<Rom>();
								temp.Add(rom);
								have.Add(key, temp);
							}
						}
					}
				}

				// If we are supposed to replace everything in the output with default values, do so
				if (_fake)
				{
					_logger.User("Replacing all hashes in Have with 0-byte values");
					List<string> keys = have.Keys.ToList();
					foreach (string key in keys)
					{
						List<Rom> temp = new List<Rom>();
						List<Rom> roms = have[key];
						for (int i = 0; i < roms.Count; i++)
						{
							Rom rom = roms[i];
							rom.HashData.Size = Constants.SizeZero;
							rom.HashData.CRC = Constants.CRCZero;
							rom.HashData.MD5 = Constants.MD5Zero;
							rom.HashData.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						have[key] = temp;
					}
				}

				Dat haveData = new Dat
				{
					Name = "Have",
					Description = "Have",
					Version = "",
					Date = DateTime.Now.ToString("yyyy-MM-dd"),
					Category = "",
					Author = "SabreTools",
					ForcePacking = ForcePacking.None,
					OutputFormat = OutputFormat.Xml,
					MergeRoms = true,
					Roms = have,
				};
				Output.WriteDatfile(haveData, "", _logger);

				return true;
			}

			return false;
		}
	}
}
