using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class MergeDAT
	{
		// Instance variables
		private bool _diff;
		private bool _dedup;
		private List<String> _inputs;
		private Logger _logger;

		public MergeDAT(List<String> inputs, bool diff, bool dedup, Logger logger)
		{
			_inputs = inputs;
			_diff = diff;
			_dedup = dedup;
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
			bool tofile = false, help = false, dedup = false, diff = false;
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-h":
					case "-?":
					case "--help":
						help = true;
						break;
					case "-l":
					case "--log":
						tofile = true;
						break;
					case "-di":
					case "--diff":
						diff = true;
						break;
					case "-dd":
					case "--dedup":
						dedup = true;
						break;
					default:
						// Add actual files to the list of inputs
						if (File.Exists(arg.Replace("\"", "")))
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
			logger.ToFile = tofile;

			// Show help if explicitly asked for it or if not enough files are supplied
			if (help || inputs.Count < 2)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Otherwise, read in the files, process them and write the result to the file type that the first one is
			MergeDAT md = new MergeDAT(inputs, diff, dedup, logger);
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

			if (_diff)
			{
				Output.WriteToDat("diffdat" + (_dedup ? "-merged" : ""), "diffdat" + (_dedup ? "-merged" : ""), "", "", "DiffDat", "SabreTools", false, !RomManipulation.IsXmlDat(_inputs[0]), "", A, _logger);
			}
			else
			{
				Output.WriteToDat("combinedat" + (_dedup ? "-merged" : ""), "combinedat" + (_dedup ? "-merged" : ""), "", "", "", "SabreTools", false, !RomManipulation.IsXmlDat(_inputs[0]), "", A, _logger);
			}

			return true;
		}
	}
}
