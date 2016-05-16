using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;

namespace SabreTools
{
	public class UncompressedSize
	{
		public static void Main(string[] args)
		{
			Build.Start("UncompressedSize");

			List<string> inputs = new List<string>();

			foreach (string arg in args)
			{
				if (File.Exists(arg.Replace("\"", "")))
				{
					inputs.Add(arg.Replace("\"", ""));
				}
				if (Directory.Exists(arg.Replace("\"", "")))
				{
					foreach (string file in Directory.GetFiles(arg.Replace("\"", ""), "*", SearchOption.AllDirectories))
					{
						inputs.Add(file.Replace("\"", ""));
					}
				}
			}

			Logger logger = new Logger(true, "uncompressedsize.log");
			logger.Start();

			long size = 0;
			foreach (string filename in inputs)
			{
				DatData datdata = new DatData
				{
					Roms = new Dictionary<string, List<RomData>>(),
				};
				datdata = RomManipulation.ParseDict(filename, 0, 0, datdata, logger);
				foreach (List<RomData> romlist in datdata.Roms.Values)
				{
					foreach (RomData rom in romlist)
					{
						size += rom.Size;
					}
				}
			}

			logger.User("The total file size is: " + GetBytesReadable(size));
			logger.Close();
		}

		/// <summary>
		///  Returns the human-readable file size for an arbitrary, 64-bit file size 
		/// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Human-readable file size</returns>
		/// <link>http://www.somacon.com/p576.php</link>
		public static string GetBytesReadable(long input)
		{
			// Get absolute value
			long absolute_i = (input < 0 ? -input : input);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (input >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (input >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (input >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (input >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (input >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = input;
			}
			else
			{
				return input.ToString("0 B"); // Byte
			}
			// Divide by 1024 to get fractional value
			readable = (readable / 1024);
			// Return formatted number with suffix
			return readable.ToString("0.### ") + suffix;
		}
	}
}
