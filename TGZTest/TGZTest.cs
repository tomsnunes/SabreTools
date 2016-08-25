using System.IO;
using SabreTools.Helper;

namespace SabreTools
{
	public class TGZTest
	{
		public static void Main(string[] args)
		{
			Logger logger = new Logger(true, "tgztest.log");
			logger.Start();

			foreach (string arg in args)
			{
				string temparg = arg.Replace("\"", "").Replace("file://", "");

				if (File.Exists(temparg))
				{
					ArchiveTools.WriteTorrentGZ(temparg, "tgz", logger);
				}
				else if (Directory.Exists(temparg))
				{
					foreach (string file in Directory.EnumerateFiles(temparg, "*", SearchOption.AllDirectories))
					{
						ArchiveTools.WriteTorrentGZ(file, "tgz", logger);
					}
				}
			}

			logger.Close();
		}
	}
}
