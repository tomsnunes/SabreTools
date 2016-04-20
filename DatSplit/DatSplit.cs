using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using SabreTools.Helper;

namespace SabreTools
{
	class DatSplit
	{
		private static string _extA;
		private static string _extB;
		private static string _filename;

		public static void Main(string[] args)
		{
			Console.Clear();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			// If we don't have arguments, show help
			if (args.Length == 0 && args.Length != 3)
			{
				Build.Help();
				return;
			}

			Logger logger = new Logger(false, "datsplit.log");
			logger.Start();

			// Output the title
			Build.Start("DatSplit");

			// Set needed variables
			_filename = args[0];
			_extA = (args[1].StartsWith(".") ? args[1] : "." + args[1]).ToUpperInvariant();
			_extB = (args[2].StartsWith(".") ? args[2] : "." + args[2]).ToUpperInvariant();
			List<RomData> romsA = new List<RomData>();
			List<RomData> romsB = new List<RomData>();

			// Load the current DAT to be processed
			List<RomData> roms = RomManipulation.Parse(_filename, 0, 0, logger);

			// Now separate the roms accordingly
			foreach (RomData rom in roms)
			{
				if (rom.Name.ToUpperInvariant().EndsWith(_extA))
				{
					romsA.Add(rom);
				}
				else if (rom.Name.ToUpperInvariant().EndsWith(_extB))
				{
					romsB.Add(rom);
				}
				else
				{
					romsA.Add(rom);
					romsB.Add(rom);
				}
			}

			// Then write out both files
			Output.WriteToDat(Path.GetFileNameWithoutExtension(_filename) + "." + _extA, Path.GetFileNameWithoutExtension(_filename) + "." + _extA,
				"", "", "", "", false, !RomManipulation.IsXmlDat(_filename), "", romsA, logger);
			Output.WriteToDat(Path.GetFileNameWithoutExtension(_filename) + "." + _extB, Path.GetFileNameWithoutExtension(_filename) + "." + _extB,
				"", "", "", "", false, !RomManipulation.IsXmlDat(_filename), "", romsB, logger);

			logger.Close();
		}
	}
}
