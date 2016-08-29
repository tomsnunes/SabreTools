using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SabreTools
{
	public class Split
	{
		// Instance variables
		private bool _hash;
		private List<string> _extA;
		private List<string> _extB;
		private List<string> _inputs;
		private string _outdir;
		private static Logger _logger;

		/// <summary>
		/// Create a new Split object (extension split)
		/// </summary>
		/// <param name="filename">Filename of the DAT to split</param>
		/// <param name="extA">List of extensions to split on (first DAT)</param>
		/// <param name="extB">List of extensions to split on (second DAT)</param>
		/// <param name="logger">Logger object for console and file writing</param>
		public Split(List<string> inputs, List<string> extA, List<string> extB, string outdir, Logger logger)
		{
			_hash = false;
			_inputs = inputs;
			_extA = new List<string>();
			foreach (string s in extA)
			{
				_extA.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
			}
			_extB = new List<string>();
			foreach (string s in extB)
			{
				_extB.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
			}
			_outdir = outdir;
			_logger = logger;
		}

		/// <summary>
		/// Create a new Split object (hash split)
		/// </summary>
		/// <param name="filename">Filename of the DAT to split</param>
		/// <param name="logger">Logger object for console and file writing</param>
		public Split(List<string> inputs, string outdir, Logger logger)
		{
			_hash = true;
			_inputs = inputs;
			_extA = new List<string>();
			_extB = new List<string>();
			_outdir = outdir;
			_logger = logger;
		}

		/// <summary>
		/// Split a DAT based on filtering by 2 extensions
		/// </summary>
		/// <returns>True if DAT was split, false otherwise</returns>
		public bool Process()
		{
			bool success = true;

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

			// Loop over the inputs
			foreach (string input in _inputs)
			{
				// If it's a file, run the proper split on the file
				if (System.IO.File.Exists(input))
				{
					if (_hash)
					{
						success &= SplitByHash(Path.GetFullPath(input), Path.GetDirectoryName(input));
					}
					else
					{
						success &= SplitByExt(Path.GetFullPath(input), Path.GetDirectoryName(input));
					}
				}
				// If it's a directory, run the splits over the files within
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						if (_hash)
						{
							success &= SplitByHash(Path.GetFullPath(file), (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar));
						}
						else
						{
							success &= SplitByExt(Path.GetFullPath(file), (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar));
						}
					}
				}
				// If file doesn't exist, error and return
				else
				{
					_logger.Error("File or folder '" + input + "' doesn't exist");
					return false;
				}
			}

			return success;
		}

		/// <summary>
		/// Split a DAT by best available hashes
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		private bool SplitByHash(string filename, string basepath)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Get the file data to be split
			OutputFormat outputFormat = DatTools.GetOutputFormat(filename);
			Dat datdata = new Dat();
			datdata = DatTools.Parse(filename, 0, 0, datdata, _logger, true);

			// Create each of the respective output DATs
			_logger.User("Creating and populating new DATs");
			Dat nodump = new Dat
			{
				FileName = datdata.FileName + " (Nodump)",
				Name = datdata.Name + " (Nodump)",
				Description = datdata.Description + " (Nodump)",
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
				Roms = new Dictionary<string, List<Helper.File>>(),
			};
			Dat sha1 = new Dat
			{
				FileName = datdata.FileName + " (SHA-1)",
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
				Roms = new Dictionary<string, List<Helper.File>>(),
			};
			Dat md5 = new Dat
			{
				FileName = datdata.FileName + " (MD5)",
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
				Roms = new Dictionary<string, List<Helper.File>>(),
			};
			Dat crc = new Dat
			{
				FileName = datdata.FileName + " (CRC)",
				Name = datdata.Name + " (CRC)",
				Description = datdata.Description + " (CRC)",
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
				Roms = new Dictionary<string, List<Helper.File>>(),
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = datdata.Roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<Helper.File> roms = datdata.Roms[key];
				foreach (Helper.File rom in roms)
				{
					// If the file is a nodump
					if (rom.Nodump)
					{
						if (nodump.Roms.ContainsKey(key))
						{
							nodump.Roms[key].Add(rom);
						}
						else
						{
							List<Helper.File> temp = new List<Helper.File>();
							temp.Add(rom);
							nodump.Roms.Add(key, temp);
						}
					}
					// If the file has a SHA-1
					else if (rom.HashData.SHA1 != null && rom.HashData.SHA1 != "")
					{
						if (sha1.Roms.ContainsKey(key))
						{
							sha1.Roms[key].Add(rom);
						}
						else
						{
							List<Helper.File> temp = new List<Helper.File>();
							temp.Add(rom);
							sha1.Roms.Add(key, temp);
						}
					}
					// If the file has no SHA-1 but has an MD5
					else if (rom.HashData.MD5 != null && rom.HashData.MD5 != "")
					{
						if (md5.Roms.ContainsKey(key))
						{
							md5.Roms[key].Add(rom);
						}
						else
						{
							List<Helper.File> temp = new List<Helper.File>();
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
							List<Helper.File> temp = new List<Helper.File>();
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
				outdir = _outdir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outdir = Path.GetDirectoryName(filename);
			}

			// Now, output all of the files to the output directory
			_logger.User("DAT information created, outputting new files");
			bool success = true;
			if (nodump.Roms.Count > 0)
			{
				success &= Output.WriteDatfile(nodump, outdir, _logger);
			}
			if (sha1.Roms.Count > 0)
			{
				success &= Output.WriteDatfile(sha1, outdir, _logger);
			}
			if (md5.Roms.Count > 0)
			{
				success &= Output.WriteDatfile(md5, outdir, _logger);
			}
			if (crc.Roms.Count > 0)
			{
				success &= Output.WriteDatfile(crc, outdir, _logger);
			}

			return success;
		}

		/// <summary>
		/// Split a DAT by input extensions
		/// </summary>
		/// <param name="filename">Name of the file to be split</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		private bool SplitByExt(string filename, string basepath)
		{
			// Load the current DAT to be processed
			Dat datdata = new Dat();
			datdata = DatTools.Parse(filename, 0, 0, datdata, _logger);

			// Set all of the appropriate outputs for each of the subsets
			OutputFormat outputFormat = DatTools.GetOutputFormat(filename);
			Dat datdataA = new Dat
			{
				FileName = datdata.FileName + " (" + string.Join(",", _extA) + ")",
				Name = datdata.Name + " (" + string.Join(",", _extA) + ")",
				Description = datdata.Description + " (" + string.Join(",", _extA) + ")",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Roms = new Dictionary<string, List<Helper.File>>(),
				OutputFormat = outputFormat,
			};
			Dat datdataB = new Dat
			{
				FileName = datdata.FileName + " (" + string.Join(",", _extB) + ")",
				Name = datdata.Name + " (" + string.Join(",", _extB) + ")",
				Description = datdata.Description + " (" + string.Join(",", _extB) + ")",
				Category = datdata.Category,
				Version = datdata.Version,
				Date = datdata.Date,
				Author = datdata.Author,
				Email = datdata.Email,
				Homepage = datdata.Homepage,
				Url = datdata.Url,
				Comment = datdata.Comment,
				Roms = new Dictionary<string, List<Helper.File>>(),
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
				foreach (Helper.File rom in datdata.Roms[key])
				{
					if (_extA.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataA.Roms.ContainsKey(key))
						{
							datdataA.Roms[key].Add(rom);
						}
						else
						{
							List<Helper.File> temp = new List<Helper.File>();
							temp.Add(rom);
							datdataA.Roms.Add(key, temp);
						}
					}
					else if (_extB.Contains(Path.GetExtension(rom.Name.ToUpperInvariant())))
					{
						if (datdataB.Roms.ContainsKey(key))
						{
							datdataB.Roms[key].Add(rom);
						}
						else
						{
							List<Helper.File> temp = new List<Helper.File>();
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
							List<Helper.File> temp = new List<Helper.File>();
							temp.Add(rom);
							datdataA.Roms.Add(key, temp);
						}
						if (datdataB.Roms.ContainsKey(key))
						{
							datdataB.Roms[key].Add(rom);
						}
						else
						{
							List<Helper.File> temp = new List<Helper.File>();
							temp.Add(rom);
							datdataB.Roms.Add(key, temp);
						}
					}
				}
			}

			// Get the output directory
			string outdir = "";
			if (_outdir != "")
			{
				outdir = _outdir + Path.GetDirectoryName(filename).Remove(0, basepath.Length - 1);
			}
			else
			{
				outdir = Path.GetDirectoryName(filename);
			}

			// Then write out both files
			bool success = Output.WriteDatfile(datdataA, outdir, _logger);
			success &= Output.WriteDatfile(datdataB, outdir, _logger);

			return success;
		}
	}
}
