using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Deheader
{
	class Program
	{
		private static Dictionary<string, int> types;
        private static string help = @"Deheader.exe type filename|dirname

Type can be one of the following:
	a7800, fds, lynx, nes, snes";

		static void Main(string[] args)
		{
			// Type mapped to header size (in decimal bytes)
			types = new Dictionary<string, int>();
			types.Add("a7800", 128);
			types.Add("fds", 16);
			types.Add("lynx", 64);
			types.Add("nes", 16);
			types.Add("snes", 512);

			if (args.Length != 2 || !types.ContainsKey(args[0]))
			{
				Console.WriteLine(help);
				return;
			}

			// Get type of file and the filename (or foldername) itself
			string type = args[0];
			string file = args[1];
			int hs = types[type];

			// If it's a single file, just check it
			if (File.Exists(file))
			{
				DetectRemoveHeader(type, file, hs);
			}
			// If it's a directory, recursively check all
			else if (Directory.Exists(file))
			{
				foreach (string sub in Directory.GetFiles(file))
				{
					if (sub != ".." && sub != ".")
					{
						DetectRemoveHeader(type, sub, hs);
					}
				}
			}
			// Else, show that help text
			else
			{
				Console.WriteLine(help);
			}
		}

		private static void DetectRemoveHeader(string type, string file, int hs)
		{
			// Open the file in read mode
			BinaryReader br = new BinaryReader(File.OpenRead(file));

			// Extract the header from the file
			byte[] hbin = br.ReadBytes(hs);
			string header = BitConverter.ToString(hbin).Replace("-", string.Empty);

			Console.WriteLine("Possible header: " + header);

			// Deal with each possible type
			bool hasHeader = false;
			switch (type)
			{
				case "a7800":
					hasHeader = Regex.IsMatch(header, "^.415441524937383030") || Regex.IsMatch(header, "^.{64}41435455414C20434152542044415441205354415254532048455245");
					break;
				case "fds":
					hasHeader = Regex.IsMatch(header, "^4644531A0[1-4]0000000000000000000000");
					break;
				case "lynx":
					hasHeader = Regex.IsMatch(header, "^4C594E58") || Regex.IsMatch(header, "^425339");
					break;
				case "nes":
					hasHeader = Regex.IsMatch(header, "^4E45531A");
					break;
				case "snes":
					// fig, smc, ufo
					hasHeader = Regex.IsMatch(header, "^.{16}0000000000000000") || Regex.IsMatch(header, "^.{16}AABB040000000000") || Regex.IsMatch(header, "^.{16}535550455255464F");
					break;
			}

			Console.WriteLine("File has header: " + hasHeader);

			if (hasHeader)
			{
				Console.WriteLine("Creating unheadered file: " + file + ".new");
				BinaryWriter bw = new BinaryWriter(File.OpenWrite(file + ".new"));
				FileInfo fi = new FileInfo(file);
				bw.Write(br.ReadBytes((int)fi.Length - hs));
				bw.Close();
				Console.WriteLine("Unheadered file created!");
			}

			br.Close();
		}
	}
}
