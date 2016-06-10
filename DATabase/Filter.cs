using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Helper;

namespace SabreTools
{
	public class Filter
	{
		// Private instance variables
		private string _filename;
		private string _outdir;
		private string _gamename;
		private string _romname;
		private string _romtype;
		private long _sgt;
		private long _slt;
		private long _seq;
		private string _crc;
		private string _md5;
		private string _sha1;
		private bool? _nodump;
		private Logger _logger;

		/// <summary>
		/// Create a Filter object
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="outdir">Output directory to write the file to</param>
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// <param name="logger">Logging object for file and console output</param>
		public Filter(string filename, string outdir, string gamename, string romname, string romtype,
			long sgt, long slt, long seq, string crc, string md5, string sha1, bool? nodump, Logger logger)
		{
			_filename = filename;
			_outdir = (outdir == "" ? Path.GetDirectoryName(_filename) : outdir);
			_gamename = gamename;
			_romname = romname;
			_romtype = romtype;
			_sgt = sgt;
			_slt = slt;
			_seq = seq;
			_crc = crc;
			_md5 = md5;
			_sha1 = sha1;
			_nodump = nodump;
			_logger = logger;
		}

		/// <summary>
		/// Process an individual DAT with the given information
		/// </summary>
		/// <returns>True if the DAT was output, false otherwise</returns>
		public bool Process()
		{
			_logger.User("Processing file: '" + _filename + "'");

			// Populated the DAT information
			DatData datdata = new DatData();
			datdata = RomManipulation.Parse(_filename, 0, 0, datdata, _logger);

			// Now loop through and create a new Rom dictionary using filtered values
			Dictionary<string, List<RomData>> dict = new Dictionary<string, List<RomData>>();
			List<string> keys = datdata.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<RomData> roms = datdata.Roms[key];
				foreach (RomData rom in roms)
				{
					// Filter on nodump status
					if (_nodump == true && !rom.Nodump)
					{
						continue;
					}
					if (_nodump == false && rom.Nodump)
					{
						continue;
					}

					// Filter on game name
					if (_gamename != "")
					{
						if (_gamename.StartsWith("*") && _gamename.EndsWith("*") && !rom.Game.ToLowerInvariant().Contains(_gamename.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (_gamename.StartsWith("*") && !rom.Game.EndsWith(_gamename.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (_gamename.EndsWith("*") && !rom.Game.StartsWith(_gamename.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on rom name
					if (_romname != "")
					{
						if (_romname.StartsWith("*") && _romname.EndsWith("*") && !rom.Name.ToLowerInvariant().Contains(_romname.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (_romname.StartsWith("*") && !rom.Name.EndsWith(_romname.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (_romname.EndsWith("*") && !rom.Name.StartsWith(_romname.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on rom type
					if (_romtype != "" && rom.Type.ToLowerInvariant() != _romtype.ToLowerInvariant())
					{
						continue;
					}

					// Filter on rom size
					if (_seq != -1 && rom.Size != _seq)
					{
						continue;
					}
					else
					{
						if (_sgt != -1 && rom.Size < _sgt)
						{
							continue;
						}
						if (_slt != -1 && rom.Size > _slt)
						{
							continue;
						}
					}

					// Filter on crc
					if (_crc != "")
					{
						if (_crc.StartsWith("*") && _crc.EndsWith("*") && !rom.CRC.ToLowerInvariant().Contains(_crc.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (_crc.StartsWith("*") && !rom.CRC.EndsWith(_crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (_crc.EndsWith("*") && !rom.CRC.StartsWith(_crc.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on md5
					if (_md5 != "")
					{
						if (_md5.StartsWith("*") && _md5.EndsWith("*") && !rom.MD5.ToLowerInvariant().Contains(_md5.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (_md5.StartsWith("*") && !rom.MD5.EndsWith(_md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (_md5.EndsWith("*") && !rom.MD5.StartsWith(_md5.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// Filter on sha1
					if (_sha1 != "")
					{
						if (_sha1.StartsWith("*") && _sha1.EndsWith("*") && !rom.SHA1.ToLowerInvariant().Contains(_sha1.ToLowerInvariant().Replace("*", "")))
						{
							continue;
						}
						else if (_sha1.StartsWith("*") && !rom.SHA1.EndsWith(_sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						else if (_sha1.EndsWith("*") && !rom.SHA1.StartsWith(_sha1.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
					}

					// If it made it this far, add the rom to the output dictionary
					if (dict.ContainsKey(key))
					{
						dict[key].Add(rom);
					}
					else
					{
						List<RomData> temp = new List<RomData>();
						temp.Add(rom);
						dict.Add(key, temp);
					}
				}

				// Now clean up by removing the old list
				datdata.Roms[key] = null;
			}

			// Get the correct output values
			datdata.FileName = Path.GetFileNameWithoutExtension(datdata.FileName) + " (Filtered)" + Path.GetExtension(datdata.FileName);
			datdata.Name += " (Filtered)";
			datdata.Description += " (Filtered)";
			datdata.Roms = dict;

			// Now write the file out if anything is there and return
			if (datdata.Roms.Count > 0)
			{
				return Output.WriteDatfile(datdata, _outdir, _logger);
			}

			// Otherwise, we return true because we did all we could
			return true;
		}
	}
}
