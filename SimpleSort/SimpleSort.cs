using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

using SabreTools.Helper;
using DamienG.Security.Cryptography;
using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Writer;

namespace SabreTools
{
	public class SimpleSort
	{
		// Private instance variables
		private DatData _datdata;
		private List<string> _inputs;
		private string _outdir;
		private string _tempdir;
		private ArchiveScanLevel _7z;
		private ArchiveScanLevel _gz;
		private ArchiveScanLevel _rar;
		private ArchiveScanLevel _zip;
		private Logger _logger;

		/// <summary>
		/// Create a new SimpleSort object
		/// </summary>
		/// <param name="datdata">Name of the DAT to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outdir">Output directory to use to build to</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		public SimpleSort(DatData datdata, List<string> inputs, string outdir, string tempdir,
			int sevenzip, int gz, int rar, int zip, Logger logger)
		{
			_datdata = datdata;
			_inputs = inputs;
			_outdir = (outdir == "" ? "Rebuild" : outdir);
			_tempdir = (tempdir == "" ? "__TEMP__" : tempdir);
			_7z = (ArchiveScanLevel)(sevenzip < 0 || sevenzip > 2 ? 0 : sevenzip);
			_gz = (ArchiveScanLevel)(gz < 0 || gz > 2 ? 0 : gz);
			_rar = (ArchiveScanLevel)(rar < 0 || rar > 2 ? 0 : rar);
			_zip = (ArchiveScanLevel)(zip < 0 || zip > 0 ? 0 : zip);
			_logger = logger;
		}

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
			logger.Start();
			Remapping.CreateHeaderSkips();

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
				simpleSort = true;
			int sevenzip = 0,
				gz = 2,
				rar = 2,
				zip = 0;
			string datfile = "",
				outdir = "",
				tempdir = "";
			List<string> inputs = new List<string>();

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
							datfile = temparg.Split('=')[1];
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
							outdir = temparg.Split('=')[1];
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
							tempdir = temparg.Split('=')[1];
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
			if (inputs.Count == 0 && (simpleSort))
			{
				logger.Error("This feature requires at least one input");
				Build.Help();
				logger.Close();
				return;
			}

			// If we are doing a simple sort
			if (simpleSort)
			{
				if (datfile != "")
				{
					InitSimpleSort(datfile, inputs, outdir, tempdir, sevenzip, gz, rar, zip, logger);
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
		/// Wrap sorting files using an input DAT
		/// </summary>
		/// <param name="datfile">Name of the DAT to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outdir">Output directory to use to build to</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitSimpleSort(string datfile, List<string> inputs, string outdir, string tempdir, 
			int sevenzip, int gz, int rar, int zip, Logger logger)
		{
			DatData datdata = new DatData();
			datdata = DatTools.Parse(datfile, 0, 0, datdata, logger);

			SimpleSort ss = new SimpleSort(datdata, inputs, outdir, tempdir, sevenzip, gz, rar, zip, logger);
			ss.Process();
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <returns></returns>
		public bool Process()
		{
			bool success = true;

			// First, check that the output directory exists
			if (!Directory.Exists(_outdir))
			{
				Directory.CreateDirectory(_outdir);
				_outdir = Path.GetFullPath(_outdir);
			}

			// Then, loop through and check each of the inputs
			_logger.User("Starting to loop through inputs");
			foreach (string input in _inputs)
			{
				if (File.Exists(input))
				{
					_logger.Log("File found: '" + input + "'");
					success &= ProcessFile(input);
					CleanTempDirectory();
				}
				else if (Directory.Exists(input))
				{
					_logger.Log("Directory found: '" + input + "'");
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						_logger.Log("File found: '" + file + "'");
						success &= ProcessFile(file);
						CleanTempDirectory();
					}
				}
				else
				{
					_logger.Error("'" + input + "' is not a file or directory!");
				}
			}

			// Now one final delete of the temp directory
			Directory.Delete(_tempdir, true);

			return success;
		}

		/// <summary>
		/// Process an individual file against the DAT
		/// </summary>
		/// <param name="input">The name of the input file</param>
		/// <param name="recurse">True if this is in a recurse step and the file should be deleted, false otherwise (default)</param>
		/// <returns>True if it was processed properly, false otherwise</returns>
		private bool ProcessFile(string input, bool recurse = false)
		{
			bool success = true;

			// Get the full path of the input for movement purposes
			input = Path.GetFullPath(input);
			_logger.User("Beginning processing of '" + input + "'");

			// Get the hash of the file first
			RomData rom = new RomData
			{
				Name = Path.GetFileName(input),
				Type = "rom",
				Size = (new FileInfo(input)).Length,
				CRC= string.Empty,
				MD5 = string.Empty,
				SHA1 = string.Empty,
			};

			try
			{
				Crc32 crc = new Crc32();
				MD5 md5 = MD5.Create();
				SHA1 sha1 = SHA1.Create();

				using (FileStream fs = File.Open(input, FileMode.Open))
				{
					foreach (byte b in crc.ComputeHash(fs))
					{
						rom.CRC += b.ToString("x2").ToLowerInvariant();
					}
				}
				using (FileStream fs = File.Open(input, FileMode.Open))
				{
					rom.MD5 = BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
				}
				using (FileStream fs = File.Open(input, FileMode.Open))
				{
					rom.SHA1 = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
				}
			}
			catch (IOException)
			{
				return false;
			}

			// Try to find the matches to the file that was found
			List<RomData> foundroms = DatTools.GetDuplicates(rom, _datdata);
			_logger.User("File '" + input + "' had " + foundroms.Count + " matches in the DAT!");
			foreach (RomData found in foundroms)
			{
				IWriter outarchive = null;
				FileStream fs = null;
				try
				{
					fs = File.OpenWrite(_outdir + Path.DirectorySeparatorChar + found.Game + ".zip");
					outarchive = WriterFactory.Open(fs, ArchiveType.Zip, CompressionType.Deflate);
					outarchive.Write(found.Name, input);
				}
				catch
				{
					if (!File.Exists(_outdir + Path.DirectorySeparatorChar + found.Game + Path.DirectorySeparatorChar + rom.Name))
					{
						File.Copy(input, _outdir + Path.DirectorySeparatorChar + found.Game + Path.DirectorySeparatorChar + rom.Name);
					}
				}
				finally
				{
					outarchive?.Dispose();
					fs?.Close();
					fs?.Dispose();
				}
			}

			// Now, if the file is a supported archive type, also run on all files within
			bool encounteredErrors = true;
			IArchive archive = null;
			try
			{
				archive = ArchiveFactory.Open(input);
				ArchiveType at = archive.Type;
				_logger.Log("Found archive of type: " + at);

				if ((at == ArchiveType.Zip && _zip != ArchiveScanLevel.External) ||
					(at == ArchiveType.SevenZip && _7z != ArchiveScanLevel.External) ||
					(at == ArchiveType.Rar && _rar != ArchiveScanLevel.External))
				{
					// Create the temp directory
					DirectoryInfo di = Directory.CreateDirectory(_tempdir);

					// Extract all files to the temp directory
					IReader reader = archive.ExtractAllEntries();
					reader.WriteAllToDirectory(_tempdir, ExtractOptions.ExtractFullPath);
					encounteredErrors = false;
				}
				else if (at == ArchiveType.GZip && _gz != ArchiveScanLevel.External)
				{
					// Close the original archive handle
					archive.Dispose();

					// Create the temp directory
					DirectoryInfo di = Directory.CreateDirectory(_tempdir);

					using (FileStream itemstream = File.OpenRead(input))
					{
						using (FileStream outstream = File.Create(_tempdir + Path.GetFileNameWithoutExtension(input)))
						{
							using (GZipStream gz = new GZipStream(itemstream, CompressionMode.Decompress))
							{
								gz.CopyTo(outstream);
							}
						}
					}
					encounteredErrors = false;
				}
				archive.Dispose();
			}
			catch (InvalidOperationException)
			{
				encounteredErrors = true;
				if (archive != null)
				{
					archive.Dispose();
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex.ToString());
				encounteredErrors = true;
				if (archive != null)
				{
					archive.Dispose();
				}
			}

			// Remove the current file if we are in recursion so it's not picked up in the next step
			if (recurse)
			{
				File.Delete(input);
			}

			// If no errors were encountered, we loop through the temp directory
			if (!encounteredErrors)
			{
				_logger.User("Archive found! Successfully extracted");
				foreach (string file in Directory.EnumerateFiles(_tempdir, "*", SearchOption.AllDirectories))
				{
					success &= ProcessFile(file, true);
				}
			}

			return success;
		}

		/// <summary>
		/// Cleans out the temporary directory
		/// </summary>
		private void CleanTempDirectory()
		{
			foreach (string file in Directory.EnumerateFiles(_tempdir, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					File.Delete(file);
				}
				catch { }
			}
			foreach (string dir in Directory.EnumerateDirectories(_tempdir, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					Directory.Delete(dir, true);
				}
				catch { }
			}
		}
	}
}
