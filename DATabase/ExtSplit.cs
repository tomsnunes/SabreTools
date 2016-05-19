using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class ExtSplit
	{
		// Instance variables
		private string _extA;
		private string _extB;
		private string _filename;
		private string _outdir;
		private static Logger _logger;

		/// <summary>
		/// Create a new DatSplit object
		/// </summary>
		/// <param name="filename">Filename of the DAT to split</param>
		/// <param name="extA">First extension to split on</param>
		/// <param name="extB">Second extension to split on</param>
		/// <param name="logger">Logger object for console and file writing</param>
		public ExtSplit(string filename, string extA, string extB, string outdir, Logger logger)
		{
			_filename = filename.Replace("\"", "");
			_extA = (extA.StartsWith(".") ? extA : "." + extA).ToUpperInvariant();
			_extB = (extB.StartsWith(".") ? extB : "." + extB).ToUpperInvariant();
			_outdir = outdir.Replace("\"", "");
			_logger = logger;
		}

		/// <summary>
		/// Split a DAT based on filtering by 2 extensions
		/// </summary>
		/// <returns>True if DAT was split, false otherwise</returns>
		public bool Split()
		{
			// If file doesn't exist, error and return
			if (!File.Exists(_filename))
			{
				_logger.Error("File '" + _filename + "' doesn't exist");
				return false;
			}

			// If it's empty, use the current folder
			if (_outdir.Trim() == "")
			{
				_outdir = Environment.CurrentDirectory;
			}

			// If the output directory doesn't exist, create it
			if (!Directory.Exists(_outdir))
			{
				Directory.CreateDirectory(_outdir);
			}

			// Create the initial DatData object
			DatData datdata = new DatData
			{
				Roms = new Dictionary<string, List<RomData>>(),
			};

			// Load the current DAT to be processed
			datdata = RomManipulation.Parse(_filename, 0, 0, datdata, _logger);

			// Set all of the appropriate outputs for each of the subsets
			OutputFormat outputFormat = RomManipulation.GetOutputFormat(_filename);
			DatData datdataA = new DatData
			{
				Name = datdata.Name + "." + _extA,
				Description = datdata.Description + "." + _extA,
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Roms = new Dictionary<string, List<RomData>>(),
				OutputFormat = outputFormat,
			};
			DatData datdataB = new DatData
			{
				Name = datdata.Name + "." + _extB,
				Description = datdata.Description + "." + _extB,
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Roms = new Dictionary<string, List<RomData>>(),
				OutputFormat = outputFormat,
			};

			// If roms is empty, return false
			if (datdata.Roms.Count == 0)
			{
				return false;
			}

			// Now separate the roms accordingly
			foreach (string key in datdata.Roms.Keys)
			{
				foreach (RomData rom in datdata.Roms[key])
				{
					if (rom.Name.ToUpperInvariant().EndsWith(_extA))
					{
						if (datdataA.Roms.ContainsKey(key))
						{
							datdataA.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(rom);
							datdataA.Roms.Add(key, temp);
						}
					}
					else if (rom.Name.ToUpperInvariant().EndsWith(_extB))
					{
						if (datdataB.Roms.ContainsKey(key))
						{
							datdataB.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(rom);
							datdataB.Roms.Add(key, temp);
						}
					}
					else
					{
						if (datdataA.Roms.ContainsKey(key))
						{
							datdataA.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(rom);
							datdataA.Roms.Add(key, temp);
						}
						if (datdataB.Roms.ContainsKey(key))
						{
							datdataB.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(rom);
							datdataB.Roms.Add(key, temp);
						}
					}
				}
			}

			// Then write out both files
			bool success = Output.WriteDatfile(datdataA, _outdir, _logger);
			success &= Output.WriteDatfile(datdataB, _outdir, _logger);

			return success;
		}
	}
}
