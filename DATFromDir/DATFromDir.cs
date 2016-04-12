using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using DamienG.Security.Cryptography;

namespace SabreTools
{
	class DATFromDir
	{
		private static string _7zPath;
		private static string _basePath;
		private static string _tempDir;
		private static string _baseExtract;

		static void Main(string[] args)
		{
			// Set local paths
			_7zPath = Environment.CurrentDirectory.Replace('/', '\\') + "\\7z" + (Environment.Is64BitOperatingSystem ? "\\x64" : "") + "\\";
			_tempDir = Environment.CurrentDirectory.Replace('/', '\\') + "\\temp" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\\";
			_basePath = (args.Length == 0 ? Environment.CurrentDirectory + "\\" : (File.Exists(args[0]) ? args[0] : args[0] + "\\").Replace('/', '\\'));

			// Set base arguments to be used
			_baseExtract = "x -o\"" + _tempDir + "\"";

			// Set the basic Process information for 7za
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = _7zPath + "7za.exe",
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
			};

			// Get an output array going that has the right mappings (parent, name, size, hash)
			List<Tuple<string, string, long, string, string, string>> roms = new List<Tuple<string, string, long, string, string, string>>();

			// This is where the main loop would go
			if (File.Exists(_basePath))
			{
				// Get a list of files including size and hashes
				Crc32 crc = new Crc32();
				MD5 md5 = MD5.Create();
				SHA1 sha1 = SHA1.Create();

				string fileCRC = String.Empty;
				string fileMD5 = String.Empty;
				string fileSHA1 = String.Empty;

				try
				{
					using (FileStream fs = File.Open(_basePath, FileMode.Open))
					{
						foreach (byte b in crc.ComputeHash(fs))
						{
							fileCRC += b.ToString("x2").ToLower();
						}
					}
					using (FileStream fs = File.Open(_basePath, FileMode.Open))
					{
						fileMD5 = BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
					}
					using (FileStream fs = File.Open(_basePath, FileMode.Open))
					{
						fileSHA1 = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
					}

					roms.Add(new Tuple<string, string, long, string, string, string>(
						"Default",
						_basePath,
						(new FileInfo(_basePath)).Length,
						fileCRC,
						fileMD5,
						fileSHA1));

					Console.WriteLine("File parsed: " + _basePath.Remove(0, _basePath.Length));
				}
				catch (IOException) { }
			}
			else
			{
				foreach (string item in Directory.GetFiles(_basePath, "*", SearchOption.AllDirectories))
				{
					// Create the temporary output directory
					Directory.CreateDirectory(_tempDir);

					psi.Arguments = _baseExtract + " " + item;
					Process zip = Process.Start(psi);
					zip.WaitForExit();

					bool encounteredErrors = zip.StandardError.ReadToEnd().Contains("ERROR");

					// Get a list of files including size and hashes
					Crc32 crc = new Crc32();
					MD5 md5 = MD5.Create();
					SHA1 sha1 = SHA1.Create();

					// If the file was an archive and was extracted successfully, check it
					if (!encounteredErrors)
					{
						foreach (string entry in Directory.GetFiles(_tempDir, "*", SearchOption.AllDirectories))
						{
							string fileCRC = String.Empty;
							string fileMD5 = String.Empty;
							string fileSHA1 = String.Empty;

							try
							{
								using (FileStream fs = File.Open(entry, FileMode.Open))
								{
									foreach (byte b in crc.ComputeHash(fs))
									{
										fileCRC += b.ToString("x2").ToLower();
									}
								}
								using (FileStream fs = File.Open(entry, FileMode.Open))
								{
									fileMD5 = BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
								}
								using (FileStream fs = File.Open(entry, FileMode.Open))
								{
									fileSHA1 = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
								}
							}
							catch (IOException)
							{
								continue;
							}

							roms.Add(new Tuple<string, string, long, string, string, string>(
								Path.GetFileNameWithoutExtension(item),
								entry.Remove(0, _tempDir.Length),
								(new FileInfo(entry)).Length,
								fileCRC,
								fileMD5,
								fileSHA1));

							Console.WriteLine("File parsed: " + entry.Remove(0, _tempDir.Length));
						}
					}
					// Otherwise, just get the info on the file itself
					else if (!Directory.Exists(item) && File.Exists(item))
					{
						string fileCRC = String.Empty;
						string fileMD5 = String.Empty;
						string fileSHA1 = String.Empty;

						try
						{
							using (FileStream fs = File.Open(item, FileMode.Open))
							{
								foreach (byte b in crc.ComputeHash(fs))
								{
									fileCRC += b.ToString("x2").ToLower();
								}
							}
							using (FileStream fs = File.Open(item, FileMode.Open))
							{
								fileMD5 = BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
							}
							using (FileStream fs = File.Open(item, FileMode.Open))
							{
								fileSHA1 = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
							}
						}
						catch (IOException)
						{
							continue;
						}

						string actualpath = Path.GetDirectoryName(item.Remove(0, _basePath.Length)).Replace('/', '\\').Split('\\')[0];

						roms.Add(new Tuple<string, string, long, string, string, string>(
							(actualpath == "" ? "Default" : actualpath),
							item.Remove(0, _basePath.Length).Remove(0, actualpath.Length + (actualpath != "" ? 1 : 0)),
							(new FileInfo(item)).Length,
							fileCRC,
							fileMD5,
							fileSHA1));

						Console.WriteLine("File parsed: " + item.Remove(0, _basePath.Length));
					}

					// Delete the temp directory
					if (Directory.Exists(_tempDir))
					{
						Directory.Delete(_tempDir, true);
					}
				}
			}

			// Order the roms by name of parent, then name of rom
			roms.Sort(delegate (Tuple<string, string, long, string, string, string> A, Tuple<string, string, long, string, string, string> B)
			{
				if (A.Item1 == B.Item1)
				{
					if (A.Item2 == B.Item2)
					{
						return (int)(A.Item3 - B.Item3);
					}
					return String.Compare(A.Item2, B.Item2);
				}
				return String.Compare(A.Item1, B.Item1);
			});

			//TODO: So, this below section is a pretty much one for one copy of code that is written in generate
			//		this means that in the future, "writing to DAT" will be abstracted out to the DLL so that any
			//		properly formatted data can be passed in and it will get written as necessary. That would open
			//		the possibiliites for different ways to generate a DAT from multiple things

			// Now write it all out as a DAT
			try
			{
				FileStream fs = File.Create("dirdat.xml");
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
					"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
					"\t<datafile>\n" +
					"\t\t<header>\n" +
					"\t\t\t<name>dirdat</name>\n" +
					"\t\t\t<description>dirdat</description>\n" +
					"\t\t\t<category>DATFromDir</category>\n" +
					"\t\t\t<version></version>\n" +
					"\t\t\t<date></date>\n" +
					"\t\t\t<author>DATFromDir</author>\n" +
					"\t\t\t<clrmamepro />\n" +
					"\t\t</header>\n";

				// Write the header out
				sw.Write(header);

				// Write out each of the machines and roms
				string lastgame = "";
				foreach (Tuple<string, string, long, string, string, string> rom in roms)
				{
					string state = "";
					if (lastgame != "" && lastgame != rom.Item1)
					{
						state += "\t</machine>\n";
					}

					if (lastgame != rom.Item1)
					{
						state += "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Item1) + "\">\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(rom.Item1) + "</description>\n";
					}

					state += "\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Item2) + "\"" +
						(rom.Item3 != -1 ? " size=\"" + rom.Item3 + "\"" : "") +
						(rom.Item4 != "" ? " crc=\"" + rom.Item4.ToLowerInvariant() + "\"" : "") +
						(rom.Item5 != "" ? " md5=\"" + rom.Item5.ToLowerInvariant() + "\"" : "") +
						(rom.Item6 != "" ? " sha1=\"" + rom.Item6.ToLowerInvariant() + "\"" : "") +
						" />\n";

					lastgame = rom.Item1;

					sw.Write(state);
				}

				sw.Write("\t</machine>\n</datafile>");
				Console.Write("File written!");
				sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				Console.Write(ex.ToString());
			}
		}
	}
}
