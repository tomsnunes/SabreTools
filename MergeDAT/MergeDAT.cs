using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class MergeDAT
	{
		// Listing related variables
		private List<String> _inputs;

		// User specified flags
		private bool _diff;
		private bool _dedup;
		private bool _noDate;
		private bool _forceunpack;
		private bool _old;

		// User specified strings
		private string _name;
		private string _desc;
		private string _cat;
		private string _version;
		private string _author;

		// Other required variables
		private string _date = DateTime.Now.ToString("yyyy-MM-dd");
		private Logger _logger;

		/// <param name="inputs">A List of Strings representing the DATs or DAT folders to be merged</param>
		/// <param name="diff">True if a DiffDat of all inputs is wanted, false otherwise</param>
		/// <param name="dedup">True if the outputted file should remove duplicates, false otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>

		public MergeDAT(List<String> inputs, string name, string desc, string cat, string version, string author,
			bool diff, bool dedup, bool noDate, bool forceunpack, bool old, Logger logger)
		{
			_inputs = inputs;
			_name = name;
			_desc = desc;
			_cat = cat;
			_version = version;
			_author = author;
			_diff = diff;
			_dedup = dedup;
			_noDate = noDate;
			_forceunpack = forceunpack;
			_old = old;
			_logger = logger;
		}

		/// <summary>
		/// Entry point for MergeDat program
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

			if (args.Length == 0)
			{
				Build.Help();
				return;
			}

			Logger logger = new Logger(false, "diffdat.log");
			logger.Start();

			// Output the title
			Build.Start("DiffDat");

			List<String> inputs = new List<String>();
			bool help = false, dedup = false, diff = false, forceunpack = false, old = false, log = false, noDate = false;
			string name = "", desc = "", cat = "", version = "", author = "";
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
					case "-di":
					case "--diff":
						diff = true;
						break;
					case "-dd":
					case "--dedup":
						dedup = true;
						break;
					case "-b":
					case "--bare":
						noDate = true;
						break;
					case "-u":
					case "--unzip":
						forceunpack = true;
						break;
					case "-o":
					case "--old":
						old = true;
						break;
					case "-l":
					case "--log":
						log = true;
						break;
					default:
						if (arg.StartsWith("-n=") || arg.StartsWith("--name="))
						{
							name = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-d=") || arg.StartsWith("--desc="))
						{
							desc = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-c=") || arg.StartsWith("--cat="))
						{
							cat = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-a=") || arg.StartsWith("--author="))
						{
							author = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-v=") || arg.StartsWith("--version="))
						{
							version = arg.Split('=')[1];
						}
						// Add actual files to the list of inputs
						else if (File.Exists(arg.Replace("\"", "")))
						{
							inputs.Add(Path.GetFullPath(arg.Replace("\"", "")));
						}
						else if (Directory.Exists(arg.Replace("\"", "")))
						{
							foreach (string file in Directory.EnumerateFiles(arg, "*", SearchOption.AllDirectories))
							{
								inputs.Add(Path.GetFullPath(file));
							}
						}
						break;
				}
			}

			// Set the possibly new value for logger
			logger.ToFile = log;

			// Show help if explicitly asked for it or if not enough files are supplied
			if (help || inputs.Count < 2)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Otherwise, read in the files, process them and write the result to the file type that the first one is
			MergeDAT md = new MergeDAT(inputs, name, desc, cat, version, author, diff, dedup, noDate, forceunpack, old, logger);
			md.MergeDiff();
		}

		/// <summary>
		/// Combine DATs, optionally diffing and deduping them
		/// </summary>
		/// <returns>True if the DATs merged correctly, false otherwise</returns>
		/// <remarks>
		/// TODO: @tractivo -for the A and B and AB output you could let this be determined by comparing the hashes.
		/// 	when a hash is present in both dats then this entry goes to AB, if its only in A then it stay in A if in B then in B.
		/// </remarks>
		public bool MergeDiff()
		{
			// Check if there are any inputs
			if (_inputs.Count == 0)
			{
				_logger.Warning("No inputs were found!");
				return false;
			}

			List<RomData> A = new List<RomData>();
			foreach (string input in _inputs)
			{
				_logger.Log("Adding DAT: " + input);
				List<RomData> B = RomManipulation.Parse(input, 0, 0, _logger);
				if (_diff)
				{
					A = RomManipulation.Diff(A, B);
				}
				else
				{
					A.AddRange(B);
				}
			}

			// If we want a merged list, send it for merging before outputting
			if (_dedup)
			{
				A = RomManipulation.Merge(A);
			}

			// Get the names that will be used
			if (_name == "")
			{
				_name = (_diff ? "diffdat" : "mergedat") + (_dedup ? "-merged" : "");
			}
			if (_desc == "")
			{
				_desc = (_diff ? "diffdat" : "mergedat") + (_dedup ? "-merged" : "");
				if (!_noDate)
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

			// Now write the file out
			Output.WriteToDat(_name, _desc, _version, _date, _cat, _author, _forceunpack, _old, "", A, _logger);

			return true;
		}
	}
}
