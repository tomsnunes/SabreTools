using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class DatToMiss
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

			logger = new Logger(false, "dattomiss.log");
			logger.Start();

			// Output the title
			Build.Start("DatToMiss");

			string prefix = "", postfix = "", input = "";
			bool tofile = false, help = false, usegame = true;
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
					case "-r":
					case "--roms":
						usegame = false;
						break;
					default:
						if ((arg.StartsWith("-pre=") || arg.StartsWith("--prefix=")) && prefix == "")
						{
							prefix = arg.Split('=')[1];
						}
						else if ((arg.StartsWith("-post=") || arg.StartsWith("--postfix=")) && postfix == "")
						{
							postfix = arg.Split('=')[1];
						}
						else if (input == "" && File.Exists(arg.Replace("\"", "")))
						{
							input = arg.Replace("\"", "");
						}
						break;
				}
			}

			// Set the possibly new value for logger
			logger.ToFile = tofile;

			// Show help if explicitly asked for it or if not enough files are supplied
			if (help || input == "")
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Make sure that the path provided is real

			// Read in the roms from the DAT and then write them to the file
			Output.WriteToText(Path.GetDirectoryName(input) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(input) + "-miss.txt",
				Path.GetDirectoryName(input), RomManipulation.Parse(input, 0, 0, logger), logger, usegame, prefix, postfix);
		}
	}
}
