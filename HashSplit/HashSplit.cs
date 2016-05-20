using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SabreTools.Helper;

namespace SabreTools
{
	public class HashSplit
	{
		// Internal variables
		List<string> _inputs;
		string _outdir;
		Logger _logger;

		/// <summary>
		/// Create a HashSplit object
		/// </summary>
		/// <param name="filename">The name of the file to be split</param>
		/// <param name="outdir">Optional output directory, defaults to DAT directory</param>
		/// <param name="logger">Logger object for file and console output</param>
		public HashSplit(List<string> inputs, string outdir, Logger logger)
		{
			_inputs = inputs;
			_outdir = (outdir.EndsWith(Path.DirectorySeparatorChar.ToString()) ? outdir : outdir + Path.DirectorySeparatorChar).Replace("\"", "");
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

			Logger logger = new Logger(true, "hashsplit.log");
			logger.Start();

			// First things first, take care of all of the arguments that this could have
			string outdir = "";
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
					default:
						if (arg.StartsWith("out="))
						{
							outdir = arg.Split('=')[1];
						}
						else
						{
							inputs.Add(arg);
						}
						break;
				}
			}

			// If there's no inputs, show the help
			if (inputs.Count() == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("HashSplit");

			// Verify the input file
			foreach (string input in inputs)
			{
				if (!File.Exists(input.Replace("\"", "")) && !Directory.Exists(input.Replace("\"", "")))
				{
					logger.Error(input + " is not a valid file!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// If so, run the program
			HashSplit hs = new HashSplit(inputs, outdir, logger);
			hs.Split();
		}

		/// <summary>
		/// Split the DAT into parts by best-available hash data
		/// </summary>
		/// <returns>True if the DATs were output, false otherwise</returns>
		public bool Split()
		{
			// First, clean the original filenames
			List<string> newinputs = new List<string>();
			foreach (string input in _inputs)
			{
				newinputs.Add(Path.GetFullPath(input.Replace("\"", "")));
			}
			_inputs = newinputs;

			// Now, process each file and folder in the input
			bool finalreturn = true;
			foreach (string input in _inputs)
			{
				if (File.Exists(input))
				{
					finalreturn &= SplitHelper(input, Path.GetDirectoryName(input));
				}
				if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						finalreturn &= SplitHelper(Path.GetFullPath(file), (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar));
					}
				}
			}

			return finalreturn;
		}

		private bool SplitHelper(string filename, string basepath)
		{
			// Get the file data to be split
			OutputFormat outputFormat = RomManipulation.GetOutputFormat(filename);
			DatData datdata = new DatData
			{
				Roms = new Dictionary<string, List<RomData>>(),
			};
			datdata = RomManipulation.Parse(filename, 0, 0, datdata, _logger, true);

			// Create each of the respective output DATs
			_logger.User("Creating and populating new DATs");
			DatData sha1 = new DatData
			{
				Name = datdata.Name + " (SHA-1)",
				Description = datdata.Description + " (SHA-1)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Roms = new Dictionary<string, List<RomData>>(),
			};
			DatData md5 = new DatData
			{
				Name = datdata.Name + " (MD5)",
				Description = datdata.Description + " (MD5)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Roms = new Dictionary<string, List<RomData>>(),
			};
			DatData crc = new DatData
			{
				Name = datdata.Name + " (CRC and None)",
				Description = datdata.Description + " (CRC and None)",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Header = datdata.Header,
				Type = datdata.Type,
				ForceMerging = datdata.ForceMerging,
				ForceNodump = datdata.ForceNodump,
				ForcePacking = datdata.ForcePacking,
				OutputFormat = outputFormat,
				MergeRoms = datdata.MergeRoms,
				Roms = new Dictionary<string, List<RomData>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = datdata.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<RomData> roms = datdata.Roms[key];
				foreach (RomData rom in roms)
				{
					// If the file has a SHA-1
					if (rom.SHA1 != null && rom.SHA1 != "")
					{
						if (sha1.Roms.ContainsKey(key))
						{
							sha1.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(rom);
							sha1.Roms.Add(key, temp);
						}
					}
					// If the file has no SHA-1 but has an MD5
					else if (rom.MD5 != null && rom.MD5 != "")
					{
						if (md5.Roms.ContainsKey(key))
						{
							md5.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(rom);
							md5.Roms.Add(key, temp);
						}
					}
					// All other cases
					else
					{
						if (crc.Roms.ContainsKey(key))
						{
							crc.Roms[key].Add(rom);
						}
						else
						{
							List<RomData> temp = new List<RomData>();
							temp.Add(rom);
							crc.Roms.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			string outdir = "";
			if (_outdir != "")
			{
				outdir = _outdir + Path.GetDirectoryName(filename).Remove(0, filename.Length);
			}
			else
			{
				outdir = Path.GetDirectoryName(filename);
			}

			// Now, output all of the files to the output directory
			_logger.User("DAT information created, outputting new files");
			bool success = true;
			success &= Output.WriteDatfile(sha1, outdir, _logger);
			success &= Output.WriteDatfile(md5, outdir, _logger);
			success &= Output.WriteDatfile(crc, outdir, _logger);

			return success;
		}
	}
}
