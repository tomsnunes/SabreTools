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

			string prefix = "", postfix = "", input = "", addext = "", repext = "";
			bool tofile = false, help = false, usegame = true, quotes = false;
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
					case "-q":
					case "--quotes":
						quotes = true;
						break;
					default:
						if (arg.StartsWith("-pre=") || arg.StartsWith("--prefix="))
						{
							prefix = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-post=") || arg.StartsWith("--postfix="))
						{
							postfix = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-ae=") || arg.StartsWith("-add-ext="))
						{
							addext = arg.Split('=')[1];
						}
						else if (arg.StartsWith("-re=") || arg.StartsWith("-rep-ext="))
						{
							repext = arg.Split('=')[1];
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

			// Get the output name
			string name = Path.GetDirectoryName(input);
			if (!name.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				name += Path.DirectorySeparatorChar;
			}
			name += Path.GetFileNameWithoutExtension(input) + "-miss.txt";

			// Read in the roms from the DAT and then write them to the file
			Output.WriteToText(name, Path.GetDirectoryName(input), RomManipulation.Parse(input, 0, 0, logger), logger, usegame, prefix, postfix, addext, repext, quotes);
		}
	}
}
