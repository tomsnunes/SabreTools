using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools
{
	public class SimpleSortApp
	{
		/// <summary>
		/// Main entry point for the program
		/// </summary>
		/// <param name="args">List of arguments to be parsed</param>
		public static void Main(string[] args)
		{
			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Perform initial setup and verification
			Logger logger = new Logger(true, "simplesort.log");

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("SimpleSort");

			// Set all default values
			bool help = false,
				convert = false,
				delete = false,
				quickScan = false,
				romba = false,
				simpleSort = true,
				toFolder = false,
				tzip = false,
				updateDat = false,
				verify = false;
			bool? torrentX = null;
			int sevenzip = 0,
				gz = 2,
				rar = 2,
				zip = 0;
			string outDir = "",
				tempDir = "";
			List<string> inputs = new List<string>();
			List<string> datfiles = new List<string>();

			// Determine which switches are enabled (with values if necessary)
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-c":
					case "--convert":
						convert = true;
						break;
					case "-d":
					case "--delete":
						delete = true;
						break;
					case "-do":
					case "--directory":
						toFolder = true;
						break;
					case "-qs":
					case "--quick":
						quickScan = true;
						break;
					case "-r":
					case "--romba":
						romba = true;
						break;
					case "-tgz":
					case "--tgz":
						torrentX = false;
						break;
					case "-tzip":
					case "--tzip":
						torrentX = true;
						break;
					case "-ud":
					case "--updated-dat":
						updateDat = true;
						break;
					case "-v":
					case "--verify":
						verify = true;
						break;
					default:
						string temparg = arg.Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-7z=") || temparg.StartsWith("--7z="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out sevenzip))
							{
								sevenzip = 0;
							}
						}
						else if (temparg.StartsWith("-dat=") || temparg.StartsWith("--dat="))
						{
							string datfile = temparg.Split('=')[1];
							if (!File.Exists(datfile))
							{
								logger.Error("DAT must be a valid file: " + datfile);
								Console.WriteLine();
								Build.Help();
								logger.Close();
								return;
							}
							datfiles.Add(datfile);
						}
						else if (temparg.StartsWith("-gz=") || temparg.StartsWith("--gz="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out gz))
							{
								gz = 2;
							}
						}
						else if (temparg.StartsWith("-out=") || temparg.StartsWith("--out="))
						{
							outDir = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-rar=") || temparg.StartsWith("--rar="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out rar))
							{
								rar = 2;
							}
						}
						else if (temparg.StartsWith("-t=") || temparg.StartsWith("--temp="))
						{
							tempDir = temparg.Split('=')[1];
						}
						else if (temparg.StartsWith("-zip=") || temparg.StartsWith("--zip="))
						{
							if (!Int32.TryParse(temparg.Split('=')[1], out zip))
							{
								zip = 0;
							}
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
						}
						else
						{
							logger.Error("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							Console.WriteLine();
							logger.Error("Invalid input detected: " + arg);
							logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && ((simpleSort && !verify) || convert || tzip))
			{
				logger.Error("This feature requires at least one input");
				Build.Help();
				logger.Close();
				return;
			}

			// If we are converting the folder to TGZ
			else if (convert)
			{
				InitConvertFolderTGZ(datfiles, inputs, outDir, tempDir, delete, romba, sevenzip, gz, rar, zip, logger);
			}

			// If we are doing a simple sort
			else if (simpleSort)
			{
				if (datfiles.Count > 0)
				{
					InitSortVerify(datfiles, inputs, outDir, tempDir, quickScan, toFolder,
						verify, delete, torrentX, romba, sevenzip, gz, rar, zip, updateDat, logger);
				}
				else
				{
					logger.Error("A datfile is required to use this feature");
					Build.Help();
					logger.Close();
					return;
				}
			}

			// If nothing is set, show the help
			else
			{
				Build.Help();
			}

			logger.Close();
			return;
		}

		/// <summary>
		/// Wrap converting a folder to TGZ, optionally filtering by an input DAT(s)
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of all inputted files and folders</param>
		/// <param name="outDir">Output directory (empty for default directory)</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		public static bool InitConvertFolderTGZ(List<string> datfiles, List<string> inputs, string outDir, string tempDir, bool delete,
			bool romba, int sevenzip, int gz, int rar, int zip, Logger logger)
		{
			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, logger, keep: true, softlist: true);
			}

			// Get the archive scanning level
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

			// Get all individual files from the inputs
			List<string> newinputs = new List<string>();
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					newinputs.Add(Path.GetFullPath(input));
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						newinputs.Add(Path.GetFullPath(file));
					}
				}
			}

			SimpleSort ss = new SimpleSort(datdata, newinputs, outDir, tempDir, false, false, false,
				delete, false, romba, asl, false, logger);
			return ss.Convert();
		}

		/// <summary>
		/// Wrap sorting files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="verify">True if output directory should be checked instead of rebuilt to, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="torrentX">True is for TorrentZip, False is for TorrentGZ, Null is for standard zip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitSortVerify(List<string> datfiles, List<string> inputs, string outDir, string tempDir, bool quickScan,
			bool toFolder, bool verify, bool delete, bool? torrentX, bool romba, int sevenzip, int gz, int rar, int zip, bool updateDat, Logger logger)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, logger, keep: true, softlist: true);
			}

			SimpleSort ss = new SimpleSort(datdata, inputs, outDir, tempDir, quickScan, toFolder, verify,
				delete, torrentX, romba, asl, updateDat, logger);
			ss.StartProcessing();
		}
	}
}