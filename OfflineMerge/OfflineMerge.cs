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

		// Static required variables
		private static long sizezero = 0;
		private static string crczero = "00000000";
		private static string md5zero = "d41d8cd98f00b204e9800998ecf8427e";
		private static string sha1zero = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

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
			_currentNewMerged = currentNewMerged;
			_fake = fake;
			_logger = logger;
		}

		public static void Main(string[] args)
		{
			// Read in inputs and start the processing
		}

		/// <summary>
		/// Process the supplied inputs and create the three required outputs:
		/// (a) Net New - (currentNewMerged)-(currentAllMerged)
		/// (b) Unneeded - (currentAllMerged)-(currentNewMerged)
		/// (c) New Missing - (a)+(currentMissingMerged-(b))
		/// </summary>
		/// <returns>True if the files were created properly, false otherwise</returns>
		public bool Process()
		{
			// First get the combination Dictionary of currentWithReplaced and currentAllMerged
			Dictionary<string, List<RomData>> completeDats = new Dictionary<string, List<RomData>>();
			completeDats = RomManipulation.ParseDict(_currentAllMerged, 0, 0, completeDats, _logger);
			completeDats = RomManipulation.ParseDict(_currentNewMerged, 0, 0, completeDats, _logger);

			// Now get Net New output dictionary
			Dictionary<string, List<RomData>> netNew = new Dictionary<string, List<RomData>>();
			foreach (string key in completeDats.Keys)
			{
				if (completeDats[key].Count == 1)
				{
					if (completeDats[key][0].System == _currentNewMerged)
					{
						if (netNew.ContainsKey(key))
						{
							netNew[key].Add(completeDats[key][0]);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(completeDats[key][0]);
							netNew.Add(key, temp);
						}

					}
				}
			}

			// Now create the Unneeded dictionary
			Dictionary<string, List<RomData>> unneeded = new Dictionary<string, List<RomData>>();
			foreach (string key in completeDats.Keys)
			{
				if (completeDats[key].Count == 1)
				{
					if (completeDats[key][0].System == _currentAllMerged)
					{
						if (netNew.ContainsKey(key))
						{
							netNew[key].Add(completeDats[key][0]);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(completeDats[key][0]);
							netNew.Add(key, temp);
						}

					}
				}
			}

			// Now create the New Missing dictionary
			Dictionary<string, List<RomData>> midMissing = new Dictionary<string, List<RomData>>();
			midMissing = RomManipulation.ParseDict(_currentMissingMerged, 0, 0, midMissing, _logger);
			foreach (string key in unneeded.Keys)
			{
				if (midMissing.ContainsKey(key))
				{
					midMissing[key].AddRange(unneeded[key]);
				}
				else
				{
					midMissing.Add(key, unneeded[key]);
				}
			}
			Dictionary<string, List<RomData>> newMissing = new Dictionary<string, List<RomData>>();
			foreach (string key in midMissing.Keys)
			{
				if (midMissing[key].Count == 1)
				{
					if (midMissing[key][0].System == _currentMissingMerged)
					{
						if (newMissing.ContainsKey(key))
						{
							newMissing[key].Add(midMissing[key][0]);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(midMissing[key][0]);
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

			// If we are supposed to replace everything in the output with default values, do so
			if (_fake)
			{
				foreach (string key in netNew.Keys)
				{
					List<RomData> temp = new List<RomData>();
					List<RomData> roms = netNew[key];
					for (int i = 0; i < roms.Count; i++)
					{
						RomData rom = roms[i];
						rom.Size = sizezero;
						rom.CRC = crczero;
						rom.MD5 = md5zero;
						rom.SHA1 = sha1zero;
						temp.Add(rom);
					}
					netNew.Remove(key);
					netNew.Add(key, temp);
				}
				foreach (string key in unneeded.Keys)
				{
					List<RomData> temp = new List<RomData>();
					List<RomData> roms = unneeded[key];
					for (int i = 0; i < roms.Count; i++)
					{
						RomData rom = roms[i];
						rom.Size = sizezero;
						rom.CRC = crczero;
						rom.MD5 = md5zero;
						rom.SHA1 = sha1zero;
						temp.Add(rom);
					}
					unneeded.Remove(key);
					unneeded.Add(key, temp);
				}
				foreach (string key in newMissing.Keys)
				{
					List<RomData> temp = new List<RomData>();
					List<RomData> roms = newMissing[key];
					for (int i = 0; i < roms.Count; i++)
					{
						RomData rom = roms[i];
						rom.Size = sizezero;
						rom.CRC = crczero;
						rom.MD5 = md5zero;
						rom.SHA1 = sha1zero;
						temp.Add(rom);
					}
					newMissing.Remove(key);
					newMissing.Add(key, temp);
				}
			}

			// Finally, output all of the files
			Output.WriteToDatFromDict("netnew-merged", "netnew-merged", "", DateTime.Now.ToString("yyyy-MM-dd"), "", "SabreTools", false, false, true, "", netNew, _logger);
			Output.WriteToDatFromDict("unneeded-merged", "unneeded-merged", "", DateTime.Now.ToString("yyyy-MM-dd"), "", "SabreTools", false, false, true, "", unneeded, _logger);
			Output.WriteToDatFromDict("newmissing-merged", "newmissing-merged", "", DateTime.Now.ToString("yyyy-MM-dd"), "", "SabreTools", false, false, true, "", newMissing, _logger);

			return true;
		}
	}
}
