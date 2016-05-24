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
	Create new tool: Filter, with the following filters available
		Game name, Rom name, CRC, MD5, SHA-1 use asterisks as follows(case insensitive):
			-crc=*00 (means ends with "00")
			-crc=00* (means starts with "00")
			-crc=*00* (means contains "00")
			-crc=00 (means is "00")
	*/
	public class Filter
	{
		// Private instance variables
		private string _filename;
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
		public Filter(string filename, string gamename, string romname, string romtype, long sgt, long slt, long seq, string crc, string md5, string sha1, bool? nodump, Logger logger)
		{
			_filename = filename;
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
		/// Start help or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			Console.Clear();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			Logger logger = new Logger(true, "filter.log");
			logger.Start();

			// First things first, take care of all of the arguments that this could have
			bool? nodump = null;
			string gamename = "", romname = "", romtype = "", crc = "", md5 = "", sha1= "";
			long sgt = -1, slt = -1, seq = -1;
			List<string> inputs = new List<string>();
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-h":
					case "-?":
					case "--help":
						Build.Help();
						logger.Close();
						return;
					case "-nd":
					case "--nodump":
						nodump = true;
						break;
					case "-nnd":
					case "--not-nodump":
						nodump = false;
						break;
					default:
						// Numerical inputs
						if (arg.StartsWith("-seq=") || arg.StartsWith("--equal="))
						{
							if (!Int64.TryParse(arg.Split('=')[1], out seq))
							{
								seq = -1;
							}
						}
						else if (arg.StartsWith("-sgt=") || arg.StartsWith("--greater="))
						{
							if (!Int64.TryParse(arg.Split('=')[1], out sgt))
							{
								sgt = -1;
							}
						}
						else if (arg.StartsWith("-slt=") || arg.StartsWith("--less="))
						{
							if (!Int64.TryParse(arg.Split('=')[1], out slt))
							{
								slt = -1;
							}
						}

						// String inputs
						else if (arg.StartsWith("-crc=") || arg.StartsWith("--crc="))
						{
							crc = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-gn=") || arg.StartsWith("--game-name="))
						{
							gamename = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-md5=") || arg.StartsWith("--md5="))
						{
							md5 = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-rn=") || arg.StartsWith("--rom-name="))
						{
							romname = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-rt=") || arg.StartsWith("--rom-type="))
						{
							romtype = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-sha1=") || arg.StartsWith("--sha1="))
						{
							sha1 = arg.Split('=')[1];
						}
						else
						{
							inputs.Add(arg);
						}
						break;
				}
			}

			// If there's no inputs, show the help
			if (inputs.Count == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("Filter");

			// If any of the inputs are not valid, show the help
			foreach (string input in inputs)
			{
				if (!File.Exists(input) && !Directory.Exists(input))
				{
					logger.Error(input + " is not a valid input!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// Create new Filter objects for each input
			Filter filter;
			bool success = true;
			foreach (string input in inputs)
			{
				string newinput = Path.GetFullPath(input.Replace("\"", ""));

				if (File.Exists(newinput))
				{
					filter = new Filter(newinput, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, logger);
					success &= filter.Process();
				}

				if (Directory.Exists(newinput))
				{
					foreach (string file in Directory.EnumerateFiles(newinput, "*", SearchOption.AllDirectories))
					{
						filter = new Filter(file, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, logger);
						success &= filter.Process();
					}
				}
			}

			// If we failed, show the help
			if (!success)
			{
				Console.WriteLine();
				Build.Help();
			}

			logger.Close();
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
					// Filter on game name
					if (_gamename != "")
					{
						if (!(_gamename.StartsWith("*") && _gamename.EndsWith("*") && rom.Game.Contains(_gamename.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_gamename.StartsWith("*") && rom.Game.EndsWith(_gamename.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_gamename.EndsWith("*") && rom.Game.StartsWith(_gamename.Replace("*", ""))))
						{
							continue;
						}
					}

					// Filter on rom name
					if (_romname != "")
					{
						if (!(_romname.StartsWith("*") && _romname.EndsWith("*") && rom.Game.Contains(_romname.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_romname.StartsWith("*") && rom.Game.EndsWith(_romname.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_romname.EndsWith("*") && rom.Game.StartsWith(_romname.Replace("*", ""))))
						{
							continue;
						}
					}

					// Filter on rom type
					if (_romtype != "" && rom.Type != _romtype)
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
						if (!(_crc.StartsWith("*") && _crc.EndsWith("*") && rom.Game.Contains(_crc.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_crc.StartsWith("*") && rom.Game.EndsWith(_crc.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_crc.EndsWith("*") && rom.Game.StartsWith(_crc.Replace("*", ""))))
						{
							continue;
						}
					}

					// Filter on md5
					if (_md5 != "")
					{
						if (!(_md5.StartsWith("*") && _md5.EndsWith("*") && rom.Game.Contains(_md5.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_md5.StartsWith("*") && rom.Game.EndsWith(_md5.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_md5.EndsWith("*") && rom.Game.StartsWith(_md5.Replace("*", ""))))
						{
							continue;
						}
					}

					// Filter on sha1
					if (_sha1 != "")
					{
						if (!(_sha1.StartsWith("*") && _sha1.EndsWith("*") && rom.Game.Contains(_sha1.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_sha1.StartsWith("*") && rom.Game.EndsWith(_sha1.Replace("*", ""))))
						{
							continue;
						}
						else if (!(_sha1.EndsWith("*") && rom.Game.StartsWith(_sha1.Replace("*", ""))))
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
			datdata.Name += " (Filtered)";
			datdata.Description += " (Filtered)";
			datdata.Roms = dict;

			// Now write the file out and return
			return Output.WriteDatfile(datdata, Path.GetDirectoryName(_filename), _logger);
		}
	}
}
