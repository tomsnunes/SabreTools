using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Helper;
using SabreTools.Helper.Data;
using SabreTools.Helper.Dats;
using SabreTools.Helper.Tools;

namespace SabreTools
{
	public class SimpleSort
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
				Build.Help("Credits");
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help("SimpleSort");
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("SimpleSort");

			// Feature flags
			bool help = false,
				convert = false,
				sort = true,
				verify = false;

			// User flags
			bool date = false,
				delete = false,
				quickScan = false,
				romba = false,
				toFolder = false,
				tgz = false,
				updateDat = false;

			// User inputs
			int sevenzip = 0,
				gz = 2,
				rar = 2,
				zip = 0;
			string header = null,
				outDir = "",
				tempDir = "";
			List<string> datfiles = new List<string>();
			List<string> inputs = new List<string>();

			// Determine which switches are enabled (with values if necessary)
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					// Feature flags
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-c":
					case "--convert":
						convert = true;
						break;
					case "-ss":
					case "--sort":
						sort = true;
						break;
					case "-v":
					case "--verify":
						verify = true;
						break;

					// User flags
					case "-ad":
					case "--add-date":
						date = true;
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
						tgz = true;
						break;
					case "-ud":
					case "--updated-dat":
						updateDat = true;
						break;

					// User inputs
					case "-7z":
					case "--7z":
						i++;
						if (!Int32.TryParse(args[i], out sevenzip))
						{
							sevenzip = 0;
						}
						break;
					case "-dat":
					case "--dat":
						i++;
						if (!File.Exists(args[i]))
						{
							logger.Error("DAT must be a valid file: " + args[i]);
							Console.WriteLine();
							Build.Help("SimpleSort");
							logger.Close();
							return;
						}
						datfiles.Add(args[i]);
						break;
					case "-gz":
					case "--gz":
						i++;
						if (!Int32.TryParse(args[i], out gz))
						{
							gz = 2;
						}
						break;
					case "-he":
					case "--header":
						i++;
						header = args[i];
						break;
					case "-out":
					case "--out":
						i++;
						outDir = args[i];
						break;
					case "-rar":
					case "--rar":
						i++;
						if (!Int32.TryParse(args[i], out rar))
						{
							rar = 2;
						}
						break;
					case "-t":
					case "--temp":
						i++;
						tempDir = args[i];
						break;
					case "-zip":
					case "--zip":
						i++;
						if (!Int32.TryParse(args[i], out zip))
						{
							zip = 0;
						}
						break;
					default:
						string temparg = args[i].Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-") && temparg.Contains("="))
						{
							// Split the argument
							string[] split = temparg.Split('=');
							if (split[1] == null)
							{
								split[1] = "";
							}

							switch (split[0])
							{
								case "-7z":
								case "--7z":
									if (!Int32.TryParse(split[1], out sevenzip))
									{
										sevenzip = 0;
									}
									break;
								case "-dat":
								case "--dat":
									if (!File.Exists(split[1]))
									{
										logger.Error("DAT must be a valid file: " + split[1]);
										Console.WriteLine();
										Build.Help("SimpleSort");
										logger.Close();
										return;
									}
									datfiles.Add(split[1]);
									break;
								case "-gz":
								case "--gz":
									if (!Int32.TryParse(split[1], out gz))
									{
										gz = 2;
									}
									break;
								case "-h":
								case "--header":
									header = split[1];
									break;
								case "-out":
								case "--out":
									outDir = split[1];
									break;
								case "-rar":
								case "--rar":
									if (!Int32.TryParse(split[1], out rar))
									{
										rar = 2;
									}
									break;
								case "-t":
								case "--temp":
									tempDir = split[1];
									break;
								case "-zip":
								case "--zip":
									if (!Int32.TryParse(split[1], out zip))
									{
										zip = 0;
									}
									break;
								default:
									if (File.Exists(temparg) || Directory.Exists(temparg))
									{
										inputs.Add(temparg);
									}
									else
									{
										logger.Error("Invalid input detected: " + args[i]);
										Console.WriteLine();
										Build.Help("SimpleSort");
										Console.WriteLine();
										logger.Error("Invalid input detected: " + args[i]);
										logger.Close();
										return;
									}
									break;
							}
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
						}
						else
						{
							logger.Error("Invalid input detected: " + args[i]);
							Console.WriteLine();
							Build.Help("SimpleSort");
							Console.WriteLine();
							logger.Error("Invalid input detected: " + args[i]);
							logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help("SimpleSort");
				logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0 && ((sort && !verify) || convert))
			{
				logger.Error("This feature requires at least one input");
				Build.Help("SimpleSort");
				logger.Close();
				return;
			}

			// If we are converting the folder
			else if (convert)
			{
				InitConvertFolder(datfiles, inputs, outDir, tempDir, delete, tgz, romba, sevenzip,
					gz, rar, zip, logger);
			}

			// If we are doing a simple sort
			else if (sort)
			{
				if (datfiles.Count > 0)
				{
					InitSort(datfiles, inputs, outDir, tempDir, quickScan, date, toFolder,
						delete, tgz, romba, sevenzip, gz, rar, zip, updateDat, header, logger);
				}
				else
				{
					logger.Error("A datfile is required to use this feature");
					Build.Help("SimpleSort");
					logger.Close();
					return;
				}
			}

			// If nothing is set, show the help
			else
			{
				Build.Help("SimpleSort");
			}

			logger.Close();
			return;
		}

		/// <summary>
		/// Wrap converting a folder to TorrentZip or TorrentGZ, optionally filtering by an input DAT(s)
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of all inputted files and folders</param>
		/// <param name="outDir">Output directory (empty for default directory)</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="tgz">True to output files in TorrentGZ format, false for TorrentZip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		public static bool InitConvertFolder(List<string> datfiles, List<string> inputs, string outDir, string tempDir, bool delete,
			bool tgz, bool romba, int sevenzip, int gz, int rar, int zip, Logger logger)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

			DateTime start = DateTime.Now;
			logger.User("Populating internal DAT...");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, logger, keep: true, softlist: true);
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Get all individual files from the inputs
			start = DateTime.Now;
			logger.User("Organizing input files...");
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
			logger.User("Organizing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datdata.ConvertFiles(inputs, outDir, tempDir, (tgz ? OutputFormat.TorrentGzip : OutputFormat.TorrentZip), romba, delete, asl, logger);
		}

		/// <summary>
		/// Wrap sorting files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="tgz">True to output files in TorrentGZ format, false for TorrentZip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitSort(List<string> datfiles, List<string> inputs, string outDir, string tempDir, bool quickScan, bool date,
			bool toFolder, bool delete, bool tgz, bool romba, int sevenzip, int gz, int rar, int zip, bool updateDat, string headerToCheckAgainst, Logger logger)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

			DateTime start = DateTime.Now;
			logger.User("Populating internal DAT...");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, logger, keep: true, softlist: true);
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			datdata.RebuildToOutput(inputs, outDir, tempDir, quickScan, date, toFolder, delete, tgz, romba, asl, updateDat, headerToCheckAgainst, 4, logger);
		}

		/// <summary>
		/// Wrap verifying files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">Input directories to compare against</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitVerify(List<string> datfiles, List<string> inputs, string tempDir, string headerToCheckAgainst, Logger logger)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = ArchiveTools.GetArchiveScanLevelFromNumbers(1, 1, 1, 1);

			DateTime start = DateTime.Now;
			logger.User("Populating internal DAT...");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, logger, keep: true, softlist: true);
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			datdata.VerifyDirectory(inputs, tempDir, headerToCheckAgainst, logger);
		}
	}
}