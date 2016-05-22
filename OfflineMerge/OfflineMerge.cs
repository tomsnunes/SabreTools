using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SabreTools.Helper;

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
			_currentAllMerged = currentAllMerged.Replace("\"", "");
			_currentMissingMerged = currentMissingMerged.Replace("\"", "");
			_currentNewMerged = currentNewMerged.Replace("\"", "");
			_fake = fake;
			_logger = logger;
		}

		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			Logger logger = new Logger(true, "offlinemerge.log");
			logger.Start();
			Console.Clear();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				logger.Close();
				return;
			}

			// If there's no arguments, show the help
			if (args.Length == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Set all default values
			bool help = false, fake = false;
			string currentAllMerged = "", currentMissingMerged = "", currentNewMerged = "";

			// Determine which switches are enabled (with values if necessary)
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-f":
					case "--fake":
						fake = true;
						break;
					default:
						string temparg = arg.Replace("\"", "");
						if (temparg.StartsWith("-com="))
						{
							currentAllMerged = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-fix="))
						{
							currentMissingMerged = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-new="))
						{
							currentNewMerged = temparg.Split('=')[1];
						}
						else
						{
							logger.Warning("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set or all of the inputs are empty, show help
			if (help || (currentAllMerged == "" && currentMissingMerged == "" && currentNewMerged == ""))
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Otherwise, run the program
			OfflineMerge om = new OfflineMerge(currentAllMerged, currentMissingMerged, currentNewMerged, fake, logger);
			bool success = om.Process();
			if (!success)
			{
				logger.Warning("At least one complete DAT and the fixdat is needed to run!");
			}
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
				DatData completeDats = new DatData();
				completeDats = RomManipulation.Parse(_currentAllMerged, 0, 0, completeDats, _logger);
				completeDats = RomManipulation.Parse(_currentNewMerged, 0, 0, completeDats, _logger);

				// Now get Net New output dictionary [(currentNewMerged)-(currentAllMerged)]
				_logger.User("Creating and populating Net New dictionary");
				Dictionary<string, List<RomData>> netNew = new Dictionary<string, List<RomData>>();
				foreach (string key in completeDats.Roms.Keys)
				{
					List<RomData> templist = RomManipulation.Merge(completeDats.Roms[key]);
					foreach (RomData rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.System == _currentNewMerged)
						{
							if (netNew.ContainsKey(key))
							{
								netNew[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
								temp.Add(rom);
								netNew.Add(key, temp);
							}
						}
					}
				}

				// Now create the Unneeded dictionary [(currentAllMerged)-(currentNewMerged)]
				_logger.User("Creating and populating Uneeded dictionary");
				Dictionary<string, List<RomData>> unneeded = new Dictionary<string, List<RomData>>();
				foreach (string key in completeDats.Roms.Keys)
				{
					List<RomData> templist = RomManipulation.Merge(completeDats.Roms[key]);
					foreach (RomData rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.System == _currentAllMerged)
						{
							if (unneeded.ContainsKey(key))
							{
								unneeded[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
								temp.Add(rom);
								unneeded.Add(key, temp);
							}
						}
					}
				}

				// Now create the New Missing dictionary [(Net New)+(currentMissingMerged-(Unneeded))]
				_logger.User("Creating and populating New Missing dictionary");
				DatData midMissing = new DatData();
				midMissing = RomManipulation.Parse(_currentMissingMerged, 0, 0, midMissing, _logger);
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
				Dictionary<string, List<RomData>> newMissing = new Dictionary<string, List<RomData>>();
				foreach (string key in midMissing.Roms.Keys)
				{
					List<RomData> templist = RomManipulation.Merge(midMissing.Roms[key]);
					foreach (RomData rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.System == _currentMissingMerged)
						{
							if (newMissing.ContainsKey(key))
							{
								newMissing[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
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
				Dictionary<string, List<RomData>> midHave = new Dictionary<string, List<RomData>>();
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
						foreach (RomData rom in completeDats.Roms[key])
						{
							if (rom.System == _currentNewMerged)
							{
								midHave[key].Add(rom);
							}
						}
					}
					else
					{
						List<RomData> roms = new List<RomData>();
						foreach (RomData rom in completeDats.Roms[key])
						{
							if (rom.System == _currentNewMerged)
							{
								roms.Add(rom);
							}
						}
						midHave.Add(key, roms);
					}
				}
				Dictionary<string, List<RomData>> have = new Dictionary<string, List<RomData>>();
				foreach (string key in midHave.Keys)
				{
					List<RomData> templist = RomManipulation.Merge(midHave[key]);
					foreach (RomData rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.System == _currentNewMerged)
						{
							if (have.ContainsKey(key))
							{
								have[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
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
						List<RomData> temp = new List<RomData>();
						List<RomData> roms = netNew[key];
						for (int i = 0; i < roms.Count; i++)
						{
							RomData rom = roms[i];
							rom.Size = Constants.SizeZero;
							rom.CRC = Constants.CRCZero;
							rom.MD5 = Constants.MD5Zero;
							rom.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						netNew[key] = temp;
					}

					_logger.User("Replacing all hashes in Unneeded with 0-byte values");
					keys = unneeded.Keys.ToList();
					foreach (string key in keys)
					{
						List<RomData> temp = new List<RomData>();
						List<RomData> roms = unneeded[key];
						for (int i = 0; i < roms.Count; i++)
						{
							RomData rom = roms[i];
							rom.Size = Constants.SizeZero;
							rom.CRC = Constants.CRCZero;
							rom.MD5 = Constants.MD5Zero;
							rom.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						unneeded[key] = temp;
					}

					_logger.User("Replacing all hashes in New Missing with 0-byte values");
					keys = newMissing.Keys.ToList();
					foreach (string key in keys)
					{
						List<RomData> temp = new List<RomData>();
						List<RomData> roms = newMissing[key];
						for (int i = 0; i < roms.Count; i++)
						{
							RomData rom = roms[i];
							rom.Size = Constants.SizeZero;
							rom.CRC = Constants.CRCZero;
							rom.MD5 = Constants.MD5Zero;
							rom.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						newMissing[key] = temp;
					}

					_logger.User("Replacing all hashes in Have with 0-byte values");
					keys = have.Keys.ToList();
					foreach (string key in keys)
					{
						List<RomData> temp = new List<RomData>();
						List<RomData> roms = have[key];
						for (int i = 0; i < roms.Count; i++)
						{
							RomData rom = roms[i];
							rom.Size = Constants.SizeZero;
							rom.CRC = Constants.CRCZero;
							rom.MD5 = Constants.MD5Zero;
							rom.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						have[key] = temp;
					}
				}

				// Finally, output all of the files
				DatData netNewData = new DatData
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
				DatData unneededData = new DatData
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
				DatData newMissingData = new DatData
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
				DatData haveData = new DatData
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
				DatData midHave = new DatData();
				midHave = RomManipulation.Parse(_currentMissingMerged, 0, 0, midHave, _logger);
				midHave = RomManipulation.Parse(_currentAllMerged, 0, 0, midHave, _logger);
				Dictionary<string, List<RomData>> have = new Dictionary<string, List<RomData>>();
				foreach (string key in midHave.Roms.Keys)
				{
					List<RomData> templist = RomManipulation.Merge(midHave.Roms[key]);
					foreach (RomData rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.System == _currentAllMerged)
						{
							if (have.ContainsKey(key))
							{
								have[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
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
						List<RomData> temp = new List<RomData>();
						List<RomData> roms = have[key];
						for (int i = 0; i < roms.Count; i++)
						{
							RomData rom = roms[i];
							rom.Size = Constants.SizeZero;
							rom.CRC = Constants.CRCZero;
							rom.MD5 = Constants.MD5Zero;
							rom.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						have[key] = temp;
					}
				}

				DatData haveData = new DatData
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
				DatData midHave = new DatData();
				midHave = RomManipulation.Parse(_currentMissingMerged, 0, 0, midHave, _logger);
				midHave = RomManipulation.Parse(_currentNewMerged, 0, 0, midHave, _logger);
				Dictionary<string, List<RomData>> have = new Dictionary<string, List<RomData>>();
				foreach (string key in midHave.Roms.Keys)
				{
					List<RomData> templist = RomManipulation.Merge(midHave.Roms[key]);
					foreach (RomData rom in templist)
					{
						if (rom.Dupe == DupeType.None && rom.System == _currentNewMerged)
						{
							if (have.ContainsKey(key))
							{
								have[key].Add(rom);
							}
							else
							{
								List<RomData> temp = new List<RomData>();
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
						List<RomData> temp = new List<RomData>();
						List<RomData> roms = have[key];
						for (int i = 0; i < roms.Count; i++)
						{
							RomData rom = roms[i];
							rom.Size = Constants.SizeZero;
							rom.CRC = Constants.CRCZero;
							rom.MD5 = Constants.MD5Zero;
							rom.SHA1 = Constants.SHA1Zero;
							temp.Add(rom);
						}
						have[key] = temp;
					}
				}

				DatData haveData = new DatData
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
