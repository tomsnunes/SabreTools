using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SabreTools
{
	public class MergeDiff
	{
		// Listing related variables
		private List<String> _inputs;

		// User specified flags
		private bool _diff;
		private bool _dedup;
		private bool _bare;
		private bool _forceunpack;
		private bool _old;
		private bool _superdat;
		private bool _cascade;
		private bool _inplace;
		private bool _clean;

		// User specified strings
		private string _name;
		private string _desc;
		private string _cat;
		private string _version;
		private string _author;
		private string _outdir;

		// Other required variables
		private string _date = DateTime.Now.ToString("yyyy-MM-dd");
		private Logger _logger;

		/// <summary>
		/// Create a new MergeDAT object
		/// </summary>
		/// <param name="inputs">A List of Strings representing the DATs or DAT folders to be merged</param>
		/// <param name="name">Internal name of the DAT</param>
		/// <param name="desc">Description and external name of the DAT</param>
		/// <param name="cat">Category for the DAT</param>
		/// <param name="version">Version of the DAT</param>
		/// <param name="author">Author of the DAT</param>
		/// <param name="ad">True if all diff variants should be outputted, false otherwise</param>
		/// <param name="diff">True if a DiffDat of all inputs is wanted, false otherwise</param>
		/// <param name="dedup">True if the outputted file should remove duplicates, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="forceunpack">True if the forcepacking="unzip" tag is to be added, false otherwise</param>
		/// <param name="old">True if a old-style DAT should be output, false otherwise</param>
		/// <param name="superdat">True if DATs should be parsed into SuperDAT format, false otherwise</param>
		/// <param name="cascade">True if the outputted diffs should be cascaded, false otherwise</param>
		/// <param name="inplace">True if cascaded diffs overwrite the source files, false otherwise</param>
		/// <param name="outdir">New output directory for outputted DATs (blank means default)</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="logger">Logger object for console and file output</param>
		public MergeDiff(List<String> inputs, string name, string desc, string cat, string version, string author,
			bool diff, bool dedup, bool bare, bool forceunpack, bool old, bool superdat, bool cascade, bool inplace,
			string outdir, bool clean, Logger logger)
		{
			_inputs = inputs;
			_name = name;
			_desc = desc;
			_cat = cat;
			_version = version;
			_author = author;
			_diff = diff;
			_dedup = dedup;
			_bare = bare;
			_forceunpack = forceunpack;
			_old = old;
			_superdat = superdat;
			_cascade = cascade;
			_inplace = inplace;
			_outdir = outdir;
			_clean = clean;
			_logger = logger;
		}

		/// <summary>
		/// Combine DATs, optionally diffing and deduping them
		/// </summary>
		/// <returns>True if the DATs merged correctly, false otherwise</returns>
		public bool Process()
		{
			// Check if there are enough inputs
			if (_inputs.Count < 1)
			{
				_logger.Warning("At least 1 input is required!");
				return false;
			}

			// Get the values that will be used
			if (_name == "")
			{
				_name = (_diff ? "DiffDAT" : "MergeDAT") + (_superdat ? "-SuperDAT" : "") + (_dedup ? "-deduped" : "");
			}
			if (_desc == "")
			{
				_desc = (_diff ? "DiffDAT" : "MergeDAT") + (_superdat ? "-SuperDAT" : "") + (_dedup ? " - deduped" : "");
				if (!_bare)
				{
					_desc += " (" + _date + ")";
				}
			}
			if (_cat == "" && _diff)
			{
				_cat = "DiffDAT";
			}
			if (_author == "")
			{
				_author = "SabreTools";
			}

			// Create a dictionary of all ROMs from the input DATs
			Dat userData;
			List<Dat> datHeaders = PopulateUserData(out userData);

			// Modify the Dictionary if necessary and output the results
			if (_diff && !_cascade)
			{
				DiffNoCascade(_outdir, userData, _inputs, _logger);
			}
			// If we're in cascade and diff, output only cascaded diffs
			else if (_diff && _cascade)
			{
				DiffCascade(_outdir, _inplace, userData, _inputs, datHeaders, _logger);
			}
			// Output all entries with user-defined merge
			else
			{
				MergeNoDiff(_outdir, userData, _inputs, datHeaders, _logger);
			}

			return true;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="userData">Output user DatData object to output</param>
		/// <returns>List of DatData objects representing headers</returns>
		private List<Dat> PopulateUserData(out Dat userData)
		{
			List<Dat> datHeaders = new List<Dat>();

			int i = 0;
			userData = new Dat
			{
				Roms = new Dictionary<string, List<Rom>>(),
				MergeRoms = _dedup,
			};
			foreach (string input in _inputs)
			{
				_logger.User("Adding DAT: " + input.Split('¬')[0]);
				userData = DatTools.Parse(input.Split('¬')[0], i, 0, userData, _logger, true, _clean);
				i++;

				// If we are in inplace mode or redirecting output, save the DAT data
				if (_inplace || !String.IsNullOrEmpty(_outdir))
				{
					datHeaders.Add((Dat)userData.CloneHeader());

					// Reset the header values so the next can be captured
					userData.FileName = "";
					userData.Name = "";
					userData.Description = "";
					userData.Version = "";
					userData.Date = "";
					userData.Category = "";
					userData.Author = "";
					userData.ForcePacking = ForcePacking.None;
					userData.OutputFormat = OutputFormat.None;
					userData.Type = "";
				}
			}

			// Set the output values
			userData.FileName = _desc;
			userData.Name = _name;
			userData.Description = _desc;
			userData.Version = _version;
			userData.Date = _date;
			userData.Category = _cat;
			userData.Author = _author;
			userData.ForcePacking = (_forceunpack ? ForcePacking.Unzip : ForcePacking.None);
			userData.OutputFormat = (_old ? OutputFormat.ClrMamePro : OutputFormat.Xml);
			userData.Type = (_superdat ? "SuperDAT" : "");

			return datHeaders;
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="userData">Main DatData to draw information from</param>
		public void DiffNoCascade(string outdir, Dat userData, List<string> inputs, Logger logger)
		{
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			// Don't have External dupes
			string post = " (No Duplicates)";
			Dat outerDiffData = (Dat)userData.CloneHeader();
			outerDiffData.FileName += post;
			outerDiffData.Name += post;
			outerDiffData.Description += post;

			// Have External dupes
			post = " (Duplicates)";
			Dat dupeData = (Dat)userData.CloneHeader();
			dupeData.FileName += post;
			dupeData.Name += post;
			dupeData.Description += post;

			// Create a list of DatData objects representing individual output files
			List<Dat> outDats = new List<Dat>();

			// Loop through each of the inputs and get or create a new DatData object
			for (int j = 0; j < inputs.Count; j++)
			{
				post = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				Dat diffData = (Dat)userData.CloneHeader();
				diffData.FileName += post;
				diffData.Name += post;
				diffData.Description += post;
				outDats.Add(diffData);
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = userData.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = RomTools.Merge(userData.Roms[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						// No duplicates
						if (rom.Dupe < DupeType.ExternalHash)
						{
							// Individual DATs that are output
							if (outDats[rom.Metadata.SystemID].Roms.ContainsKey(key))
							{
								outDats[rom.Metadata.SystemID].Roms[key].Add(rom);
							}
							else
							{
								List<Rom> tl = new List<Rom>();
								tl.Add(rom);
								outDats[rom.Metadata.SystemID].Roms.Add(key, tl);
							}

							// Merged no-duplicates DAT
							Rom newrom = rom;
							newrom.Game += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.Metadata.SystemID].Split('¬')[0]) + ")";

							if (outerDiffData.Roms.ContainsKey(key))
							{
								outerDiffData.Roms[key].Add(newrom);
							}
							else
							{
								List<Rom> tl = new List<Rom>();
								tl.Add(rom);
								outerDiffData.Roms.Add(key, tl);
							}
						}

						// Duplicates only
						if (rom.Dupe >= DupeType.ExternalHash)
						{
							Rom newrom = rom;
							newrom.Game += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.Metadata.SystemID].Split('¬')[0]) + ")";

							if (dupeData.Roms.ContainsKey(key))
							{
								dupeData.Roms[key].Add(newrom);
							}
							else
							{
								List<Rom> tl = new List<Rom>();
								tl.Add(rom);
								dupeData.Roms.Add(key, tl);
							}
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");

			// Output the difflist (a-b)+(b-a) diff
			Output.WriteDatfile(outerDiffData, outdir, logger);

			// Output the (ab) diff
			Output.WriteDatfile(dupeData, outdir, logger);

			// Output the individual (a-b) DATs
			for (int j = 0; j < inputs.Count; j++)
			{
				// If we have an output directory set, replace the path
				string path = outdir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));

				// If we have more than 0 roms, output
				if (outDats[j].Roms.Count > 0)
				{
					Output.WriteDatfile(outDats[j], path, logger);
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output cascading diffs
		/// </summary>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		private void DiffCascade(string outdir, bool inplace, Dat userData, List<string> inputs, List<Dat> datHeaders, Logger logger)
		{
			string post = "";

			// Create a list of DatData objects representing output files
			List<Dat> outDats = new List<Dat>();

			// Loop through each of the inputs and get or create a new DatData object
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");
			for (int j = 0; j < inputs.Count; j++)
			{
				post = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				Dat diffData;

				// If we're in inplace mode, take the appropriate DatData object already stored
				if (inplace || !String.IsNullOrEmpty(outdir))
				{
					diffData = datHeaders[j];
				}
				else
				{
					diffData = (Dat)userData.CloneHeader();
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
				}
				outDats.Add(diffData);
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = userData.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<Rom> roms = RomTools.Merge(userData.Roms[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (Rom rom in roms)
					{
						if (outDats[rom.Metadata.SystemID].Roms.ContainsKey(key))
						{
							outDats[rom.Metadata.SystemID].Roms[key].Add(rom);
						}
						else
						{
							List<Rom> tl = new List<Rom>();
							tl.Add(rom);
							outDats[rom.Metadata.SystemID].Roms.Add(key, tl);
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");
			for (int j = 0; j < inputs.Count; j++)
			{
				// If we have an output directory set, replace the path
				string path = "";
				if (inplace)
				{
					path = Path.GetDirectoryName(inputs[j].Split('¬')[0]);
				}
				else if (!String.IsNullOrEmpty(outdir))
				{
					path = outdir + (Path.GetDirectoryName(inputs[j].Split('¬')[0]).Remove(0, inputs[j].Split('¬')[1].Length));
				}

				// If we have more than 0 roms, output
				if (outDats[j].Roms.Count > 0)
				{
					Output.WriteDatfile(outDats[j], path, logger);
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output user defined merge
		/// </summary>
		/// <param name="userData">Main DatData to draw information from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		private void MergeNoDiff(string outdir, Dat userData, List<string> inputs, List<Dat> datHeaders, Logger logger)
		{
			// If we're in SuperDAT mode, prefix all games with their respective DATs
			if (userData.Type == "SuperDAT")
			{
				List<string> keys = userData.Roms.Keys.ToList();
				foreach (string key in keys)
				{
					List<Rom> newroms = new List<Rom>();
					foreach (Rom rom in userData.Roms[key])
					{
						Rom newrom = rom;
						string filename = inputs[newrom.Metadata.SystemID].Split('¬')[0];
						string rootpath = inputs[newrom.Metadata.SystemID].Split('¬')[1];

						rootpath += (rootpath == "" ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newrom.Game = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename), newrom.Game);
						newroms.Add(newrom);
					}
					userData.Roms[key] = newroms;
				}
			}

			Output.WriteDatfile(userData, outdir, logger);
		}
	}
}
