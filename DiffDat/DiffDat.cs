using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class DiffDat
	{
		private static Logger logger;

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

			logger = new Logger(false, "diffdat.log");
			logger.Start();

			// Output the title
			Build.Start("DiffDat");

			List<String> inputs = new List<String>();
			bool tofile = false, help = false, merge = false; ;
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
					case "-m":
					case "--merge":
						merge = true;
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

			// Otherwise, read in the two files, diff them, and write the result to the file type that the first one is
			List<RomData> A = new List<RomData>();
			foreach (string input in inputs)
			{
				logger.Log("Merging in DAT: " + input);
				List<RomData> B = RomManipulation.Parse(input, 0, 0, logger);
				A = RomManipulation.Diff(A, B);
			}

			// If we want a merged list, send it for merging before outputting
			if (merge)
			{
				A = RomManipulation.Merge(A);
			}

			Output.WriteToDat("diffdat", "diffdat", "", "", "DiffDat", "SabreTools", false, !RomManipulation.IsXmlDat(inputs[0]), "", A, logger);
		}
	}
}
