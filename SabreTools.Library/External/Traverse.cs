using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using IOException = System.IO.IOException;
using SearchOption = System.IO.SearchOption;
#endif
using SabreTools.Helper.Data;

namespace SabreTools.Helper.External
{
	public class Traverse
	{
		/// Original version: Microsoft (example code), updated by edc
		public void TraverseTreeParallelForEach(string root, Action<FileInfo> action)
		{
			List<string> dirs = new List<string>();

			if (!Directory.Exists(root))
			{
				throw new ArgumentException();
			}

			dirs.Add(root);

			List<string> subdirs = new List<string>();

			while (dirs.Count > 0 || subdirs.Count > 0)
			{
				foreach (string dir in subdirs)
				{
					dirs.Add(dir);
				}
				subdirs.Clear();

				Parallel.ForEach(dirs, Globals.ParallelOptions, currentDir =>
				{
					string[] subDirs = Directory.GetDirectories(currentDir);

					lock (subdirs)
					{
						foreach (string str in subDirs)
						{
							subdirs.Add(str);
						}
					}

					var dir = new DirectoryInfo(currentDir);
					try
					{
						FileInfo[] files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
						Parallel.ForEach(files, Globals.ParallelOptions, info =>
						{
							action(info);
						});
					}
					catch { }
				});
				dirs.Clear();
			}
		}
	}
}
