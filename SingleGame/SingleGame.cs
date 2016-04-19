using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using SabreTools.Helper;

namespace SabreTools
{
	public class SingleGame
	{
		private static string _filename = "";
		private static string _path = "";
		private static bool _rename = true;

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

			Logger logger = new Logger(false, "singlegame.log");
			logger.Start();

			// Output the title
			Build.Start("SingleGame");

			_filename = args[0];

			if (args.Length > 1)
			{
				for (int i = 1; i < args.Length; i++)
				{
					switch (args[i])
					{
						case "-n":
							_rename = false;
							break;
						default:
							if (args[i].StartsWith("-r"))
							{
								_path = args[i].Split('=')[1];
							}
							break;
					}
				}
			}

			_path = (_path == "" ? Environment.CurrentDirectory : _path);

			// Import the existing DAT
			List<RomData> roms = RomManipulation.Parse(_filename, 0, 0, logger);

			// If we are in single game mode, rename all games
			if (_rename)
			{
				roms.ForEach(delegate (RomData x)
				{
					x.Game = "!";
				});
			}

			// Trim all file names according to the path that's set
			roms.ForEach(delegate (RomData x)
			{
				// Windows max name length is 260
				int usableLength = 259 - _path.Length;

				if (x.Name.Length > usableLength)
				{
					string ext = Path.GetExtension(x.Name);
					x.Name = x.Name.Substring(0, usableLength - ext.Length);
					x.Name += ext;
				}
			});

			// Now write the file out accordingly
			Output.WriteToDat(Path.GetFileNameWithoutExtension(_filename),
				Path.GetFileNameWithoutExtension(_filename), "", "", "", "", false, Path.GetExtension(_filename) == ".dat", "", roms, logger);

			logger.Close();
		}
	}
}
