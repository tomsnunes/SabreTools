using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SabreTools.Helper;

namespace SabreTools
{
	/*
	"Special Merge" by Obiwantje
	------------------------
	Inputs:
		(1) All - Current all merged
		(2) Missing - Current all missing
		(3) Add - Current to add
		(4) All-New - Current with replaced DATs (some add/remove)
		(5) [Flag] Set all roms to have 0-byte values; if set, prepend "Offline" to DAT name/desc

	0-byte Values:
		CRC - 00000000
		MD5 - d41d8cd98f00b204e9800998ecf8427e
		SHA-1 - da39a3ee5e6b4b0d3255bfef95601890afd80709
	*/
	public class OfflineMerge
	{
		// Instance variables
		private string _currentAllMerged;
		private string _currentAllMissing;
		private List<String> _toAdd;
		private string _currentWithReplaced;
		private bool _fake;
		private Logger _logger;

		/// <summary>
		/// Instantiate an OfflineMerge object
		/// </summary>
		/// <param name="currentAllMerged">Old-current DAT with merged values</param>
		/// <param name="currentAllMissing">Old-current missing DAT with merged values</param>
		/// <param name="toAdd">List of new files to merge in</param>
		/// <param name="currentWithReplaced">New-current DAT with merged values</param>
		/// <param name="fake">True if all values should be replaced with default 0-byte values, false otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>
		public OfflineMerge (string currentAllMerged, string currentAllMissing, string toAdd, string currentWithReplaced, bool fake, Logger logger)
		{
			_currentAllMerged = currentAllMerged.Replace("\"", "");
			_currentAllMissing = currentAllMissing.Replace("\"", "");
			_toAdd = new List<String>();
			if (File.Exists(toAdd.Replace("\"", "")))
			{
				_toAdd.Add(toAdd.Replace("\"", ""));
			}
			else if (Directory.Exists(toAdd.Replace("\"", "")))
			{
				foreach (string file in Directory.EnumerateFiles(toAdd, "*", SearchOption.AllDirectories))
				{
					_toAdd.Add(file);
				}
			}
			_currentWithReplaced = currentWithReplaced;
			_fake = fake;
			_logger = logger;
		}

		public static void Main(string[] args)
		{
			// Read in inputs and start the processing
		}

		/// <summary>
		/// Process the supplied inputs and create the three required outputs:
		/// (a) Net-New - (currentWithReplaced)-(currentAllMerged)
		/// (b) New Missing - (a)+(currentAllMissing)
		/// (c) Unneeded - (currentAllMerged)-(currentWithReplaced)
		/// </summary>
		/// <returns>True if the files were created properly, false otherwise</returns>
		public bool Process()
		{
			return true;
		}
	}
}
