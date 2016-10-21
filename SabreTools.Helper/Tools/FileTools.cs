using Mono.Data.Sqlite;
using OCRC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace SabreTools.Helper
{
	public static class FileTools
	{
		#region File Information

		/// <summary>
		/// Retrieve a list of files from a directory recursively in proper order
		/// </summary>
		/// <param name="directory">Directory to parse</param>
		/// <param name="infiles">List representing existing files</param>
		/// <returns>List with all new files</returns>
		public static List<string> RetrieveFiles(string directory, List<string> infiles)
		{
			// Take care of the files in the top directory
			List<string> toadd = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).ToList();
			toadd.Sort(new NaturalComparer());
			infiles.AddRange(toadd);

			// Then recurse through and add from the directories
			List<string> dirs = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly).ToList();
			dirs.Sort(new NaturalComparer());
			foreach (string dir in dirs)
			{
				infiles = RetrieveFiles(dir, infiles);
			}

			// Return the new list
			return infiles;
		}

		/// <summary>
		/// Get what type of DAT the input file is
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The OutputFormat corresponding to the DAT</returns>
		/// <remarks>There is currently no differentiation between XML and SabreDAT here</remarks>
		public static OutputFormat GetOutputFormat(string filename, Logger logger)
		{
			// Limit the output formats based on extension
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}
			if (ext != "dat" && ext != "md5" && ext != "sfv" && ext != "sha1" && ext != "txt" && ext != "xml")
			{
				return 0;
			}

			// Read the input file, if possible
			logger.Verbose("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return 0;
			}

			// Some formats only require the extension to know
			if (ext == "md5")
			{
				return OutputFormat.RedumpMD5;
			}
			if (ext == "sfv")
			{
				return OutputFormat.RedumpSFV;
			}
			if (ext == "sha1")
			{
				return OutputFormat.RedumpSHA1;
			}

			// For everything else, we need to read it
			try
			{
				// Get the first two lines to check
				StreamReader sr = File.OpenText(filename);
				string first = sr.ReadLine().ToLowerInvariant();
				string second = sr.ReadLine().ToLowerInvariant();
				sr.Dispose();

				// If we have an XML-based DAT
				if (first.Contains("<?xml") && first.Contains("?>"))
				{
					if (second.StartsWith("<!doctype datafile"))
					{
						return OutputFormat.Logiqx;
					}
					else if (second.StartsWith("<!doctype softwarelist"))
					{
						return OutputFormat.SoftwareList;
					}
					else if (second.StartsWith("<!doctype sabredat"))
					{
						return OutputFormat.SabreDat;
					}
					else if (second.StartsWith("<dat") && !second.StartsWith("<datafile"))
					{
						return OutputFormat.OfflineList;
					}
					// Older and non-compliant DATs
					else
					{
						return OutputFormat.Logiqx;
					}
				}

				// If we have an INI-based DAT
				else if (first.Contains("[") && first.Contains("]"))
				{
					return OutputFormat.RomCenter;
				}

				// If we have a CMP-based DAT
				else if (first.Contains("clrmamepro"))
				{
					return OutputFormat.ClrMamePro;
				}
				else if (first.Contains("romvault"))
				{
					return OutputFormat.ClrMamePro;
				}
				else if (first.Contains("doscenter"))
				{
					return OutputFormat.DOSCenter;
				}
				else
				{
					return OutputFormat.ClrMamePro;
				}
			}
			catch (Exception)
			{
				return 0;
			}
		}

		/// <summary>
		/// Retrieve file information for a single file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <param name="noMD5">True if MD5 hashes should not be calculated, false otherwise (default)</param>
		/// <param name="noSHA1">True if SHA-1 hashes should not be calcluated, false otherwise (default)</param>
		/// <param name="offset">Set a >0 number for getting hash for part of the file, 0 otherwise (default)</param>
		/// <param name="date">True if the file Date should be included, false otherwise (default)</param>
		/// <param name="header">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetFileInfo(string input, Logger logger, bool noMD5 = false, bool noSHA1 = false, long offset = 0, bool date = false, string header = null)
		{
			// Add safeguard if file doesn't exist
			if (!File.Exists(input))
			{
				return new Rom();
			}

			// Get the information from the file stream
			Rom rom = new Rom();
			if (header != null)
			{
				SkipperRule rule = Skipper.GetMatchingRule(input, Path.GetFileNameWithoutExtension(header), logger);

				// If there's a match, get the new information from the stream
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Create the input and output streams
					MemoryStream outputStream = new MemoryStream();
					FileStream inputStream = File.OpenRead(input);

					// Transform the stream and get the information from it
					rule.TransformStream(inputStream, outputStream, logger, keepReadOpen: false, keepWriteOpen: true);
					rom = GetStreamInfo(outputStream, outputStream.Length);

					// Dispose of the streams
					outputStream.Dispose();
					inputStream.Dispose();
				}
				// Otherwise, just get the info
				else
				{
					rom = GetStreamInfo(File.OpenRead(input), new FileInfo(input).Length, noMD5, noSHA1, offset, false);
				}
			}
			else
			{
				rom = GetStreamInfo(File.OpenRead(input), new FileInfo(input).Length, noMD5, noSHA1, offset, false);
			}

			// Add unique data from the file
			rom.Name = Path.GetFileName(input);
			rom.Date = (date ? new FileInfo(input).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") : "");

			return rom;
		}

		#endregion

		#region File Manipulation

		/// <summary>
		/// Get the XmlTextReader associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>The XmlTextReader representing the (possibly converted) file, null otherwise</returns>
		public static XmlReader GetXmlTextReader(string filename, Logger logger)
		{
			logger.Verbose("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return null;
			}

			XmlReader xtr = XmlReader.Create(filename, new XmlReaderSettings {
				CheckCharacters = false,
				DtdProcessing = DtdProcessing.Ignore,
				IgnoreComments = true,
				IgnoreWhitespace = true,
				ValidationFlags = XmlSchemaValidationFlags.None,
				ValidationType = ValidationType.None,
			});
			return xtr;
		}

		/// <summary>
		/// Remove an arbitrary number of bytes from the inputted file
		/// </summary>
		/// <param name="input">File to be cropped</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToRemoveFromHead">Bytes to be removed from head of file</param>
		/// <param name="bytesToRemoveFromTail">Bytes to be removed from tail of file</param>
		public static void RemoveBytesFromFile(string input, string output, long bytesToRemoveFromHead, long bytesToRemoveFromTail)
		{
			// If any of the inputs are invalid, skip
			if (!File.Exists(input) || new FileInfo(input).Length <= (bytesToRemoveFromHead + bytesToRemoveFromTail))
			{
				return;
			}

			// Get the streams
			FileStream fsr = File.OpenRead(input);
			FileStream fsw = File.OpenWrite(output);

			RemoveBytesFromStream(fsr, fsw, bytesToRemoveFromHead, bytesToRemoveFromTail);

			fsr.Dispose();
			fsw.Dispose();
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">String representing bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">String representing bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, string bytesToAddToHead, string bytesToAddToTail)
		{
			// Source: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
			byte[] bytesToAddToHeadArray = new byte[bytesToAddToHead.Length / 2];
			for (int i = 0; i < bytesToAddToHead.Length; i += 2)
			{
				bytesToAddToHeadArray[i / 2] = Convert.ToByte(bytesToAddToHead.Substring(i, 2), 16);
			}
			byte[] bytesToAddToTailArray = new byte[bytesToAddToTail.Length / 2];
			for (int i = 0; i < bytesToAddToTail.Length; i += 2)
			{
				bytesToAddToTailArray[i / 2] = Convert.ToByte(bytesToAddToTail.Substring(i, 2), 16);
			}

			AppendBytesToFile(input, output, bytesToAddToHeadArray, bytesToAddToTailArray);
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">Bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">Bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
		{
			// If any of the inputs are invalid, skip
			if (!File.Exists(input))
			{
				return;
			}

			FileStream fsr = File.OpenRead(input);
			FileStream fsw = File.OpenWrite(output);

			AppendBytesToStream(fsr, fsw, bytesToAddToHead, bytesToAddToTail);

			fsr.Dispose();
			fsw.Dispose();
		}

		/// <summary>
		/// Detect header skipper compliance and create an output file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		/// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>True if the output file was created, false otherwise</returns>
		public static bool DetectSkipperAndTransform(string file, string outDir, Logger logger)
		{
			// Create the output directory if it doesn't exist
			if (outDir != "" && !Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			logger.User("\nGetting skipper information for '" + file + "'");

			// Get the skipper rule that matches the file, if any
			SkipperRule rule = Skipper.GetMatchingRule(file, "", logger);

			// If we have an empty rule, return false
			if (rule.Tests == null || rule.Tests.Count == 0 || rule.Operation != HeaderSkipOperation.None)
			{
				return false;
			}

			logger.User("File has a valid copier header");

			// Get the header bytes from the file first
			string hstr = string.Empty;
			BinaryReader br = new BinaryReader(File.OpenRead(file));

			// Extract the header as a string for the database
			byte[] hbin = br.ReadBytes((int)rule.StartOffset);
			for (int i = 0; i < (int)rule.StartOffset; i++)
			{
				hstr += BitConverter.ToString(new byte[] { hbin[i] });
			}
			br.Dispose();

			// Apply the rule to the file
			string newfile = (outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file)));
			rule.TransformFile(file, newfile, logger);

			// If the output file doesn't exist, return false
			if (!File.Exists(newfile))
			{
				return false;
			}

			// Now add the information to the database if it's not already there
			Rom rom = GetFileInfo(newfile, logger);
			DatabaseTools.AddHeaderToDatabase(hstr, rom.SHA1, rule.SourceFile, logger);

			return true;
		}

		/// <summary>
		/// Detect and replace header(s) to the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		/// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
		/// <param name="logger">Logger object for console and file output</param>
		/// <returns>True if a header was found and appended, false otherwise</returns>
		public static bool RestoreHeader(string file, string outDir, Logger logger)
		{
			// Create the output directory if it doesn't exist
			if (outDir != "" && !Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			bool success = true;

			// First, get the SHA-1 hash of the file
			Rom rom = GetFileInfo(file, logger);

			// Then try to pull the corresponding headers from the database
			string header = "";

			// Open the database connection
			SqliteConnection dbc = new SqliteConnection(Constants.HeadererConnectionString);
			dbc.Open();

			string query = @"SELECT header, type FROM data WHERE sha1='" + rom.SHA1 + "'";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			SqliteDataReader sldr = slc.ExecuteReader();

			if (sldr.HasRows)
			{
				int sub = 0;
				while (sldr.Read())
				{
					logger.Verbose("Found match with rom type " + sldr.GetString(1));
					header = sldr.GetString(0);

					logger.User("Creating reheadered file: " +
						(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + sub);
					FileTools.AppendBytesToFile(file,
						(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + sub, header, string.Empty);
					logger.User("Reheadered file created!");
				}
			}
			else
			{
				logger.Warning("No matching header could be found!");
				success = false;
			}

			// Dispose of database objects
			slc.Dispose();
			sldr.Dispose();
			dbc.Dispose();

			return success;
		}

		/// <summary>
		/// Cleans out the temporary directory
		/// </summary>
		/// <param name="dirname">Name of the directory to clean out</param>
		public static void CleanDirectory(string dirname)
		{
			foreach (string file in Directory.EnumerateFiles(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					File.Delete(file);
				}
				catch { }
			}
			foreach (string dir in Directory.EnumerateDirectories(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					Directory.Delete(dir, true);
				}
				catch { }
			}
		}

		#endregion

		#region Rebuilding and Verifying

		/// <summary>
		/// Process inputs and convert to TorrentZip or TorrentGZ, optionally converting to Romba format
		/// </summary>
		/// <param name="datFile">DatFile to use as a filter in conversion, null otherwise</param>
		/// <param name="inputs">List of inputs to convert over to TorrentZip or TorrentGZ</param>
		/// <param name="outDir">Output folder to rebuild to, blank is the current directory</param>
		/// <param name="tempDir">Temporary directory to use in file extraction</param>
		/// <param name="tgz">True if files should be output in TorrentGZ format, false for TorrentZip</param>
		/// <param name="romba">True if TorrentGZ files should be output in romba depot format, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing how files should be treated</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if processing was a success, false otherwise</returns>
		public static bool ConvertFiles(DatFile datFile, List<string> inputs, string outDir, string tempDir, bool tgz,
			bool romba, bool delete, ArchiveScanLevel archiveScanLevel, Logger logger)
		{
			bool success = true;

			// First, check that the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				outDir = Path.GetFullPath(outDir);
			}

			// Then create or clean the temp directory
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			else
			{
				CleanDirectory(tempDir);
			}

			// Now process all of the inputs
			foreach (string input in inputs)
			{
				logger.User("Examining file " + input);

				// Get if the file should be scanned internally and externally
				bool shouldExternalProcess, shouldInternalProcess;
				ArchiveTools.GetInternalExternalProcess(input, archiveScanLevel, logger, out shouldExternalProcess, out shouldInternalProcess);

				// Do an external scan of the file, if necessary
				if (shouldExternalProcess)
				{
					// If a DAT is defined, we want to make sure that this file is not in there
					Rom rom = FileTools.GetFileInfo(input, logger);
					if (datFile != null && datFile.Files.Count > 0)
					{
						if (rom.HasDuplicates(datFile, logger))
						{
							logger.User("File '" + input + "' existed in the DAT, skipping...");
							continue;
						}
					}

					logger.User("Processing file " + input);

					if (tgz)
					{
						success &= ArchiveTools.WriteTorrentGZ(input, outDir, romba, logger);
					}
					else
					{
						success &= ArchiveTools.WriteToArchive(input, outDir, rom, logger);
					}
				}

				// Process the file as an archive, if necessary
				if (shouldInternalProcess)
				{
					// Now, if the file is a supported archive type, also run on all files within
					bool encounteredErrors = ArchiveTools.ExtractArchive(input, tempDir, archiveScanLevel, logger);

					// If no errors were encountered, we loop through the temp directory
					if (!encounteredErrors)
					{
						logger.Verbose("Archive found! Successfully extracted");
						foreach (string file in Directory.EnumerateFiles(tempDir, "*", SearchOption.AllDirectories))
						{
							// If a DAT is defined, we want to make sure that this file is not in there
							Rom rom = FileTools.GetFileInfo(file, logger);
							if (datFile != null && datFile.Files.Count > 0)
							{
								if (rom.HasDuplicates(datFile, logger))
								{
									logger.User("File '" + file + "' existed in the DAT, skipping...");
									continue;
								}
							}

							logger.User("Processing file " + input);

							if (tgz)
							{
								success &= ArchiveTools.WriteTorrentGZ(file, outDir, romba, logger);
							}
							else
							{
								success &= ArchiveTools.WriteToArchive(file, outDir, rom, logger);
							}
						}

						CleanDirectory(tempDir);
					}
				}

				// Delete the source file if we're supposed to
				if (delete)
				{
					try
					{
						logger.User("Attempting to delete " + input);
						File.Delete(input);
					}
					catch (Exception ex)
					{
						logger.Error(ex.ToString());
						success &= false;
					}
				}
			}

			// Now one final delete of the temp directory
			while (Directory.Exists(tempDir))
			{
				try
				{
					Directory.Delete(tempDir, true);
				}
				catch
				{
					continue;
				}
			}

			// If we're in romba mode and the size file doesn't exist, create it
			if (romba && !File.Exists(Path.Combine(outDir, ".romba_size")))
			{
				// Get the size of all of the files in the output folder
				long size = 0;
				foreach (string file in Directory.EnumerateFiles(outDir, "*", SearchOption.AllDirectories))
				{
					FileInfo tempinfo = new FileInfo(file);
					size += tempinfo.Length;
				}

				// Write out the value to each of the romba depot files
				StreamWriter tw = new StreamWriter(File.Open(Path.Combine(outDir, ".romba_size"), FileMode.Create, FileAccess.Write));
				StreamWriter twb = new StreamWriter(File.Open(Path.Combine(outDir, ".romba_size.backup"), FileMode.Create, FileAccess.Write));

				tw.Write(size);
				twb.Write(size);

				tw.Dispose();
				twb.Dispose();
			}

			return success;
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <param name="datFile">DAT to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="tgz">True if output files should be written to TorrentGZ instead of TorrentZip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		/// <remarks>
		/// This currently processes files as follows:
		/// 1) Get all file names from the input files/folders
		/// 2) Loop through and process each file individually
		///		a) Hash the file
		///		b) Check against the DAT for duplicates
		///		c) Check for headers
		///		d) Check headerless rom for duplicates
		/// 
		/// This is actually rather slow and inefficient. See below for more correct implemenation
		/// </remarks>
		public static bool RebuildToOutput(DatFile datFile, List<string> inputs, string outDir, string tempDir, bool quickScan, bool date,
			bool toFolder, bool delete, bool tgz, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat, string headerToCheckAgainst,
			int maxDegreeOfParallelism, Logger logger)
		{
			// First, check that the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				outDir = Path.GetFullPath(outDir);
			}

			// Then create or clean the temp directory
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			else
			{
				CleanDirectory(tempDir);
			}

			bool success = true;
			DatFile matched = new DatFile();

			logger.User("Retrieving list all files from input");
			DateTime start = DateTime.Now;

			// Create a list of just files from inputs
			List<string> files = new List<string>();
			Parallel.ForEach(inputs,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, },
				input => {
					if (File.Exists(input))
					{
						logger.Verbose("File found: '" + input + "'");
						files.Add(Path.GetFullPath(input));
					}
					else if (Directory.Exists(input))
					{
						logger.Verbose("Directory found: '" + input + "'");
						Parallel.ForEach(Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories),
							new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, },
							file =>
							{
								logger.Verbose("File found: '" + file + "'");
								files.Add(Path.GetFullPath(file));
							});
					}
					else
					{
						logger.Error("'" + input + "' is not a file or directory!");
					}
				});
			logger.User("Retrieving complete in: " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Then, loop through and check each of the inputs
			logger.User("Processing files:\n");
			int cursorTop = Console.CursorTop;
			for (int i = 0; i < files.Count; i++)
			{
				success &= RebuildToOutputHelper(datFile, matched, files[i], i, files.Count, cursorTop, outDir, tempDir, quickScan,
					date, toFolder, delete, tgz, romba, archiveScanLevel, headerToCheckAgainst, logger);
				if (tempDir != Path.GetTempPath())
				{
					CleanDirectory(tempDir);
				}
				if (success && delete)
				{
					try
					{
						File.Delete(files[i]);
					}
					catch { }
				}
			}

			// Now one final delete of the temp directory
			while (Directory.Exists(tempDir))
			{
				try
				{
					if (tempDir != Path.GetTempPath())
					{
						Directory.Delete(tempDir, true);
					}
				}
				catch
				{
					continue;
				}
			}

			// Now output the stats for the built files
			logger.ClearBeneath(Constants.HeaderHeight);
			Console.SetCursorPosition(0, Constants.HeaderHeight + 1);
			logger.User("Stats of the matched ROMs:");
			StreamWriter sw = new StreamWriter(new MemoryStream());
			matched.OutputStats(sw, StatOutputFormat.None, logger, recalculate: true, baddumpCol: true, nodumpCol: true);
			sw.Dispose();

			// Now output the fixdat based on the original input if asked
			if (updateDat)
			{
				datFile.FileName = "fixDat_" + datFile.FileName;
				datFile.Name = "fixDat_" + datFile.Name;
				datFile.Description = "fixDat_" + datFile.Description;
				datFile.OutputFormat = OutputFormat.Logiqx;
				datFile.WriteToFile("", logger);
			}

			return success;
		}

		/// <summary>
		/// Process an individual file against the DAT for rebuilding
		/// </summary>
		/// <param name="datFile">DAT to compare against</param>
		/// <param name="matched">List of files that were matched by the DAT</param>
		/// <param name="input">Name of the input file</param>
		/// <param name="index">Index of the current file</param>
		/// <param name="total">Total number of files</param>
		/// <param name="cursorTop">Top cursor position to use</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="tgz">True if output files should be written to TorrentGZ instead of TorrentZip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="recurse">True if this is in a recurse step and the file should be deleted, false otherwise (default)</param>
		/// <returns>True if it was processed properly, false otherwise</returns>
		private static bool RebuildToOutputHelper(DatFile datFile, DatFile matched, string input, int index, int total, int cursorTop,
			string outDir, string tempDir, bool quickScan, bool date, bool toFolder, bool delete, bool tgz, bool romba,
			ArchiveScanLevel archiveScanLevel, string headerToCheckAgainst, Logger logger, bool recurse = false)
		{
			bool success = true;

			// Get the full path of the input for movement purposes
			string percentage = (index == 0 ? "0.00" : Math.Round((100 * ((double)index / total)), 2, MidpointRounding.AwayFromZero).ToString());
			string statement = percentage + "% - " + input;
			logger.ClearBeneath(cursorTop + 1);
			logger.WriteExact(statement, cursorTop, 0);

			// Get if the file should be scanned internally and externally
			bool shouldExternalScan, shouldInternalScan;
			ArchiveTools.GetInternalExternalProcess(input, archiveScanLevel, logger, out shouldExternalScan, out shouldInternalScan);

			// Hash and match the external files
			if (shouldExternalScan)
			{
				Rom rom = FileTools.GetFileInfo(input, logger);

				// If we have a blank RomData, it's an error
				if (rom.Name == null)
				{
					return false;
				}

				// Try to find the matches to the file that was found
				List<DatItem> foundroms = rom.GetDuplicates(datFile, logger);
				logger.Verbose("File '" + input + "' had " + foundroms.Count + " matches in the DAT!");
				foreach (Rom found in foundroms)
				{
					logger.Verbose("Matched name: " + found.Name);

					// Add rom to the matched list
					string key = found.Size + "-" + found.CRC;
					if (matched.Files.ContainsKey(key))
					{
						matched.Files[key].Add(found);
					}
					else
					{
						List<DatItem> temp = new List<DatItem>();
						temp.Add(found);
						matched.Files.Add(key, temp);
					}

					if (toFolder)
					{
						// Copy file to output directory
						string gamedir = Path.Combine(outDir, found.MachineName);
						if (!Directory.Exists(gamedir))
						{
							Directory.CreateDirectory(gamedir);
						}

						logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (tgz ? found.SHA1 : found.Name) + "'");
						try
						{
							File.Copy(input, Path.Combine(gamedir, Path.GetFileName(found.Name)));
						}
						catch { }
					}
					else
					{
						if (tgz)
						{
							ArchiveTools.WriteTorrentGZ(input, outDir, romba, logger);
						}
						else
						{
							ArchiveTools.WriteToArchive(input, outDir, found, logger, date: date);
						}
					}
				}

				// Now get the transformed file if it exists
				SkipperRule rule = Skipper.GetMatchingRule(input, headerToCheckAgainst, logger);

				// If we have have a non-empty rule, apply it
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Otherwise, apply the rule to the file
					string newinput = input + ".new";
					rule.TransformFile(input, newinput, logger);
					Rom drom = FileTools.GetFileInfo(newinput, logger);

					// If we have a blank RomData, it's an error
					if (String.IsNullOrEmpty(drom.Name))
					{
						return false;
					}

					// Try to find the matches to the file that was found
					List<DatItem> founddroms = drom.GetDuplicates(datFile, logger);
					logger.Verbose("File '" + newinput + "' had " + founddroms.Count + " matches in the DAT!");
					foreach (Rom found in founddroms)
					{
						// Add rom to the matched list
						string key = found.Size + "-" + found.CRC;
						if (matched.Files.ContainsKey(key))
						{
							matched.Files[key].Add(found);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(found);
							matched.Files.Add(key, temp);
						}

						// First output the headerless rom
						logger.Verbose("Matched name: " + found.Name);

						if (toFolder)
						{
							// Copy file to output directory
							string gamedir = Path.Combine(outDir, found.MachineName);
							if (!Directory.Exists(gamedir))
							{
								Directory.CreateDirectory(gamedir);
							}

							logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (tgz ? found.SHA1 : found.Name) + "'");
							try
							{
								File.Copy(newinput, Path.Combine(gamedir, Path.GetFileName(found.Name)));
							}
							catch { }
						}
						else
						{
							if (tgz)
							{
								ArchiveTools.WriteTorrentGZ(newinput, outDir, romba, logger);
							}
							else
							{
								ArchiveTools.WriteToArchive(newinput, outDir, found, logger, date: date);
							}
						}

						// Then output the headered rom (renamed)
						Rom newfound = found;
						newfound.Name = Path.GetFileNameWithoutExtension(newfound.Name) + " (" + rom.CRC + ")" + Path.GetExtension(newfound.Name);
						newfound.Size = rom.Size;
						newfound.CRC = rom.CRC;
						newfound.MD5 = rom.MD5;
						newfound.SHA1 = rom.SHA1;

						// Add rom to the matched list
						key = newfound.Size + "-" + newfound.CRC;
						if (matched.Files.ContainsKey(key))
						{
							matched.Files[key].Add(newfound);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(newfound);
							matched.Files.Add(key, temp);
						}

						if (toFolder)
						{
							// Copy file to output directory
							string gamedir = Path.Combine(outDir, found.MachineName);
							if (!Directory.Exists(gamedir))
							{
								Directory.CreateDirectory(gamedir);
							}

							logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + newfound.Name + "'");
							try
							{
								File.Copy(input, Path.Combine(gamedir, Path.GetFileName(newfound.Name)));
							}
							catch { }
						}
						else
						{
							logger.Verbose("Matched name: " + newfound.Name);
							if (tgz)
							{
								ArchiveTools.WriteTorrentGZ(input, outDir, romba, logger);
							}
							else
							{
								ArchiveTools.WriteToArchive(input, outDir, newfound, logger, date: date);
							}
						}
					}

					// Now remove this temporary file
					try
					{
						File.Delete(newinput);
					}
					catch
					{
						// Don't log file deletion errors
					}
				}
			}

			// If we should scan the file as an archive
			if (shouldInternalScan)
			{
				// If external scanning is enabled, use that method instead
				if (quickScan)
				{
					logger.Verbose("Beginning quick scan of contents from '" + input + "'");
					List<Rom> internalRomData = ArchiveTools.GetArchiveFileInfo(input, logger);
					logger.Verbose(internalRomData.Count + " entries found in '" + input + "'");

					// If the list is populated, then the file was a filled archive
					if (internalRomData.Count > 0)
					{
						foreach (Rom rom in internalRomData)
						{
							// Try to find the matches to the file that was found
							List<DatItem> foundroms = rom.GetDuplicates(datFile, logger);
							logger.Verbose("File '" + rom.Name + "' had " + foundroms.Count + " matches in the DAT!");
							foreach (Rom found in foundroms)
							{
								// Add rom to the matched list
								string key = found.Size + "-" + found.CRC;
								if (matched.Files.ContainsKey(key))
								{
									matched.Files[key].Add(found);
								}
								else
								{
									List<DatItem> temp = new List<DatItem>();
									temp.Add(found);
									matched.Files.Add(key, temp);
								}

								if (toFolder)
								{
									// Copy file to output directory
									logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + found.Name + "'");
									string outfile = ArchiveTools.ExtractSingleItemFromArchive(input, rom.Name, tempDir, logger);
									if (File.Exists(outfile))
									{
										string gamedir = Path.Combine(outDir, found.MachineName);
										if (!Directory.Exists(gamedir))
										{
											Directory.CreateDirectory(gamedir);
										}

										try
										{
											File.Move(outfile, Path.Combine(gamedir, Path.GetFileName(found.Name)));
										}
										catch { }
									}
								}
								else
								{
									// Copy file between archives
									logger.Verbose("Rebuilding file '" + Path.GetFileName(rom.Name) + "' to '" + (tgz ? found.SHA1 : found.Name) + "'");

									if (Build.MonoEnvironment || tgz)
									{
										string outfile = ArchiveTools.ExtractSingleItemFromArchive(input, rom.Name, tempDir, logger);
										if (File.Exists(outfile))
										{
											if (tgz)
											{
												ArchiveTools.WriteTorrentGZ(outfile, outDir, romba, logger);
											}
											else
											{
												ArchiveTools.WriteToArchive(outfile, outDir, found, logger);
											}

											try
											{
												File.Delete(outfile);
											}
											catch { }
										}
									}
									else
									{
										ArchiveTools.CopyFileBetweenArchives(input, outDir, rom.Name, found, logger);
									}
								}
							}
						}
					}
				}
				else
				{
					// Now, if the file is a supported archive type, also run on all files within
					bool encounteredErrors = ArchiveTools.ExtractArchive(input, tempDir, archiveScanLevel, logger);

					// Remove the current file if we are in recursion so it's not picked up in the next step
					if (recurse)
					{
						try
						{
							File.Delete(input);
						}
						catch (Exception)
						{
							// Don't log file deletion errors
						}
					}

					// If no errors were encountered, we loop through the temp directory
					if (!encounteredErrors)
					{
						logger.Verbose("Archive found! Successfully extracted");
						foreach (string file in Directory.EnumerateFiles(tempDir, "*", SearchOption.AllDirectories))
						{
							success &= RebuildToOutputHelper(datFile, matched, file, index, total, cursorTop, outDir, tempDir, quickScan,
								date, toFolder, delete, tgz, romba, archiveScanLevel, headerToCheckAgainst, logger, recurse: true);
						}
					}
				}
			}

			return success;
		}

		/// <summary>
		/// Process the DAT and find all matches in input files and folders
		/// </summary>
		/// <param name="datFile">DAT to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="toFolder">True if files should be output to folder, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="tgz">True if output files should be written to TorrentGZ instead of TorrentZip</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if rebuilding was a success, false otherwise</returns>
		/// <remarks>
		/// This implemenation of the code should do the following:
		/// 1) Get all file names from the input files/folders (parallel)
		/// 2) Loop through and get the file info from every file (including headerless)
		/// 3) Find all duplicate files in the input DAT(s)
		/// 4) Order by output game
		/// 5) Rebuild all files
		/// </remarks>
		public static bool RebuiltToOutputAlternate(DatFile datFile, List<string> inputs, string outDir, string tempDir, bool quickScan, bool date,
			bool toFolder, bool delete, bool tgz, bool romba, ArchiveScanLevel archiveScanLevel, bool updateDat, string headerToCheckAgainst,
			int maxDegreeOfParallelism, Logger logger)
		{
			// First, check that the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				outDir = Path.GetFullPath(outDir);
			}

			// Then create or clean the temp directory
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			else
			{
				CleanDirectory(tempDir);
			}

			bool success = true;

			#region Find all files

			// Create a list of just files from inputs
			logger.User("Finding all files...");
			List<string> files = new List<string>();
			Parallel.ForEach(inputs,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				input =>
				{
					if (File.Exists(input))
					{
						logger.Verbose("File found: '" + input + "'");
						lock (files)
						{
							files.Add(Path.GetFullPath(input));
						}
					}
					else if (Directory.Exists(input))
					{
						logger.Verbose("Directory found: '" + input + "'");

						List<string> infiles = Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories).ToList();
						Parallel.ForEach(infiles,
							new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
							file =>
							{
								logger.Verbose("File found: '" + input + "'");
								lock (files)
								{
									files.Add(Path.GetFullPath(file));
								}
							});
					}
					else
					{
						logger.Error("'" + input + "' is not a file or directory!");
					}
				});
			logger.User("Finding files complete!");

			#endregion

			#region Get source file information

			// Now loop through all of the files and check them, DFD style
			logger.User("Getting source file information...");
			DatFile matchdat = new DatFile
			{
				Files = new SortedDictionary<string, List<DatItem>>(),
			};
			foreach (string file in files)
			{
				// Get if the file should be scanned internally and externally
				bool shouldExternalScan, shouldInternalScan;
				ArchiveTools.GetInternalExternalProcess(file, archiveScanLevel, logger, out shouldExternalScan, out shouldInternalScan);

				// Hash and match the external files
				if (shouldExternalScan)
				{
					RebuildToOutputAlternateParseRomHelper(file, ref matchdat, headerToCheckAgainst, logger);
				}

				// If we should scan the file as an archive
				if (shouldInternalScan)
				{
					// If external scanning is enabled, use that method instead
					if (quickScan)
					{
						logger.Verbose("Beginning quick scan of contents from '" + file + "'");
						List<Rom> internalRomData = ArchiveTools.GetArchiveFileInfo(file, logger);
						logger.Verbose(internalRomData.Count + " entries found in '" + file + "'");

						// Now add all of the roms to the DAT
						for (int i = 0; i < internalRomData.Count; i++)
						{
							RebuildToOutputAlternateParseRomHelper(file, ref matchdat, headerToCheckAgainst, logger);
						}
					}
					// Otherwise, try to extract the file to the temp folder
					else
					{
						// Now, if the file is a supported archive type, also run on all files within
						bool encounteredErrors = ArchiveTools.ExtractArchive(file, tempDir, archiveScanLevel, logger);

						// If we succeeded in extracting, loop through the files
						if (!encounteredErrors)
						{
							List<string> extractedFiles = Directory.EnumerateFiles(tempDir, "*", SearchOption.AllDirectories).ToList();
							foreach (string extractedFile in extractedFiles)
							{
								RebuildToOutputAlternateParseRomHelper(extractedFile, ref matchdat, headerToCheckAgainst, logger);
							}
						}
						// Otherwise, skip extracting and just get information on the file itself (if we didn't already)
						else if (!shouldExternalScan)
						{
							RebuildToOutputAlternateParseRomHelper(file, ref matchdat, headerToCheckAgainst, logger);
						}

						// Clean the temp directory for the next round
						if (Directory.Exists(tempDir))
						{
							CleanDirectory(tempDir);
						}
					}
				}
			}
			logger.User("Getting source file information complete!");

			#endregion

			#region Find all files to rebuild and bucket by game

			// Create a dictionary of from/to Rom mappings
			Dictionary<DatItem, DatItem> toFromMap = new Dictionary<DatItem, DatItem>();

			// Now populate it
			foreach (string key in matchdat.Files.Keys)
			{
				foreach (DatItem rom in matchdat.Files[key])
				{
					List<DatItem> matched = rom.GetDuplicates(datFile, logger, true);
					foreach (DatItem match in matched)
					{
						try
						{
							toFromMap.Add(match, rom);
						}
						catch { }
					}
				}
			}

			// Then bucket the keys by game for better output
			SortedDictionary<string, List<DatItem>> keysByGame = DatFile.BucketListByGame(toFromMap.Keys.ToList(), false, true, logger);

			#endregion

			#region Rebuild all files

			// At this point, we have "toFromMap" which maps output files to input files as well as
			// as SortedDictionary called keysByGame which is the output files sorted by game in
			// alphabetical order. We should be able to use these to do everything we need =)

			// Now write out each game sequentially
			foreach (string key in keysByGame.Keys)
			{

			}

			#endregion

			return success;
		}

		/// <summary>
		/// Wrap adding a file to the dictionary in custom DFD, files that matched a skipper a prefixed with "HEAD::"
		/// </summary>
		/// <param name="file">Name of the file to attempt to add</param>
		/// <param name="matchdat">Reference to the Dat to add to</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the file could be added, false otherwise</returns>
		private static bool RebuildToOutputAlternateParseRomHelper(string file, ref DatFile matchdat, string headerToCheckAgainst, Logger logger)
		{
			Rom rom = FileTools.GetFileInfo(file, logger);

			// If we have a blank RomData, it's an error
			if (rom.Name == null)
			{
				return false;
			}

			// Otherwise, set the machine name as the full path to the file
			rom.MachineName = Path.GetDirectoryName(Path.GetFullPath(file));

			// Add the rom information to the Dat
			string key = rom.Size + "-" + rom.CRC;
			if (matchdat.Files.ContainsKey(key))
			{
				matchdat.Files[key].Add(rom);
			}
			else
			{
				List<DatItem> temp = new List<DatItem>();
				temp.Add(rom);
				matchdat.Files.Add(key, temp);
			}

			// Now attempt to see if the file has a header
			FileStream input = File.OpenRead(file);
			SkipperRule rule = Skipper.GetMatchingRule(input, headerToCheckAgainst, logger);

			// If there's a match, get the new information from the stream
			if (rule.Tests != null && rule.Tests.Count != 0)
			{
				// Create the input and output streams
				MemoryStream output = new MemoryStream();

				// Transform the stream and get the information from it
				rule.TransformStream(input, output, logger, false, true);
				Rom romNH = FileTools.GetStreamInfo(output, output.Length);
				romNH.Name = "HEAD::" + rom.Name;
				romNH.MachineName = rom.MachineName;

				// Add the rom information to the Dat
				key = romNH.Size + "-" + romNH.CRC;
				if (matchdat.Files.ContainsKey(key))
				{
					matchdat.Files[key].Add(romNH);
				}
				else
				{
					List<DatItem> temp = new List<DatItem>();
					temp.Add(romNH);
					matchdat.Files.Add(key, temp);
				}

				// Dispose of the stream
				output.Dispose();
			}

			// Dispose of the stream
			input.Dispose();

			return true;
		}

		/// <summary>
		/// Process the DAT and verify the output directory
		/// </summary>
		/// <param name="datFile">DAT to use to verify the directory</param>
		/// <param name="inputs">List of input directories to compare against</param>
		/// <param name="tempDir">Temporary directory for archive extraction</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if verification was a success, false otherwise</returns>
		public static bool VerifyDirectory(DatFile datFile, List<string> inputs, string tempDir, string headerToCheckAgainst, Logger logger)
		{
			// First create or clean the temp directory
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			else
			{
				CleanDirectory(tempDir);
			}

			bool success = true;

			/*
			We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
			*/

			// Then, loop through and check each of the inputs
			logger.User("Processing files:\n");
			foreach (string input in inputs)
			{
				datFile.PopulateDatFromDir(input, false /* noMD5 */, false /* noSHA1 */, true /* bare */, false /* archivesAsFiles */,
					true /* enableGzip */, false /* addBlanks */, false /* addDate */, tempDir /* tempDir */, false /* copyFiles */,
					headerToCheckAgainst, 4 /* maxDegreeOfParallelism */, logger);
			}

			// Setup the fixdat
			DatFile matched = (DatFile)datFile.CloneHeader();
			matched.Files = new SortedDictionary<string, List<DatItem>>();
			matched.FileName = "fixDat_" + matched.FileName;
			matched.Name = "fixDat_" + matched.Name;
			matched.Description = "fixDat_" + matched.Description;
			matched.OutputFormat = OutputFormat.Logiqx;

			// Now that all files are parsed, get only files found in directory
			bool found = false;
			foreach (List<DatItem> roms in datFile.Files.Values)
			{
				List<DatItem> newroms = DatItem.Merge(roms, logger);
				foreach (Rom rom in newroms)
				{
					if (rom.SourceID == 99)
					{
						found = true;
						string key = rom.Size + "-" + rom.CRC;
						if (matched.Files.ContainsKey(key))
						{
							matched.Files[key].Add(rom);
						}
						else
						{
							List<DatItem> temp = new List<DatItem>();
							temp.Add(rom);
							matched.Files.Add(key, temp);
						}
					}
				}
			}

			// Now output the fixdat to the main folder
			if (found)
			{
				matched.WriteToFile("", logger, stats: true);
			}
			else
			{
				logger.User("No fixDat needed");
			}

			return success;
		}

		#endregion

		#region Stream Information

		/// <summary>
		/// Retrieve file information for a single file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="size">Size of the input stream</param>
		/// <param name="noMD5">True if MD5 hashes should not be calculated, false otherwise (default)</param>
		/// <param name="noSHA1">True if SHA-1 hashes should not be calcluated, false otherwise (default)</param>
		/// <param name="offset">Set a >0 number for getting hash for part of the file, 0 otherwise (default)</param>
		/// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetStreamInfo(Stream input, long size, bool noMD5 = false, bool noSHA1 = false, long offset = 0, bool keepReadOpen = false)
		{
			Rom rom = new Rom
			{
				Type = ItemType.Rom,
				Size = size,
				CRC = string.Empty,
				MD5 = string.Empty,
				SHA1 = string.Empty,
			};

			try
			{
				// Initialize the hashers
				OptimizedCRC crc = new OptimizedCRC();
				MD5 md5 = MD5.Create();
				SHA1 sha1 = SHA1.Create();

				// Seek to the starting position, if one is set
				if (offset < 0)
				{
					input.Seek(offset, SeekOrigin.End);
				}
				else
				{
					input.Seek(offset, SeekOrigin.Begin);
				}

				byte[] buffer = new byte[8 * 1024];
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					crc.Update(buffer, 0, read);
					if (!noMD5)
					{
						md5.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if (!noSHA1)
					{
						sha1.TransformBlock(buffer, 0, read, buffer, 0);
					}
				}

				crc.Update(buffer, 0, 0);
				rom.CRC = crc.Value.ToString("X8").ToLowerInvariant();

				if (!noMD5)
				{
					md5.TransformFinalBlock(buffer, 0, 0);
					rom.MD5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
				}
				if (!noSHA1)
				{
					sha1.TransformFinalBlock(buffer, 0, 0);
					rom.SHA1 = BitConverter.ToString(sha1.Hash).Replace("-", "").ToLowerInvariant();
				}

				// Dispose of the hashers
				crc.Dispose();
				md5.Dispose();
				sha1.Dispose();
			}
			catch (IOException)
			{
				return new Rom();
			}
			finally
			{
				if (!keepReadOpen)
				{
					input.Dispose();
				}
			}

			return rom;
		}

		#endregion

		#region Stream Manipulation

		// <summary>
		/// Remove an arbitrary number of bytes from the inputted stream
		/// </summary>
		/// <param name="input">Stream to be cropped</param>
		/// <param name="output">Stream to output to</param>
		/// <param name="bytesToRemoveFromHead">Bytes to be removed from head of stream</param>
		/// <param name="bytesToRemoveFromTail">Bytes to be removed from tail of stream</param>
		public static void RemoveBytesFromStream(Stream input, Stream output, long bytesToRemoveFromHead, long bytesToRemoveFromTail)
		{
			// Read the input file and write to the fail
			BinaryReader br = new BinaryReader(input);
			BinaryWriter bw = new BinaryWriter(output);

			int bufferSize = 1024;
			long adjustedLength = br.BaseStream.Length - bytesToRemoveFromTail;

			// Seek to the correct position
			br.BaseStream.Seek((bytesToRemoveFromHead < 0 ? 0 : bytesToRemoveFromHead), SeekOrigin.Begin);

			// Now read the file in chunks and write out
			byte[] buffer = new byte[bufferSize];
			while (br.BaseStream.Position <= (adjustedLength - bufferSize))
			{
				buffer = br.ReadBytes(bufferSize);
				bw.Write(buffer);
			}

			// For the final chunk, if any, write out only that number of bytes
			int length = (int)(adjustedLength - br.BaseStream.Position);
			buffer = new byte[length];
			buffer = br.ReadBytes(length);
			bw.Write(buffer);
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted stream
		/// </summary>
		/// <param name="input">Stream to be appended to</param>
		/// <param name="output">Outputted stream</param>
		/// <param name="bytesToAddToHead">String representing bytes to be added to head of stream</param>
		/// <param name="bytesToAddToTail">String representing bytes to be added to tail of stream</param>
		public static void AppendBytesToStream(Stream input, Stream output, string bytesToAddToHead, string bytesToAddToTail)
		{
			// Source: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
			byte[] bytesToAddToHeadArray = new byte[bytesToAddToHead.Length / 2];
			for (int i = 0; i < bytesToAddToHead.Length; i += 2)
			{
				bytesToAddToHeadArray[i / 2] = Convert.ToByte(bytesToAddToHead.Substring(i, 2), 16);
			}
			byte[] bytesToAddToTailArray = new byte[bytesToAddToTail.Length / 2];
			for (int i = 0; i < bytesToAddToTail.Length; i += 2)
			{
				bytesToAddToTailArray[i / 2] = Convert.ToByte(bytesToAddToTail.Substring(i, 2), 16);
			}

			AppendBytesToStream(input, output, bytesToAddToHeadArray, bytesToAddToTailArray);
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted stream
		/// </summary>
		/// <param name="input">Stream to be appended to</param>
		/// <param name="output">Outputted stream</param>
		/// <param name="bytesToAddToHead">Bytes to be added to head of stream</param>
		/// <param name="bytesToAddToTail">Bytes to be added to tail of stream</param>
		public static void AppendBytesToStream(Stream input, Stream output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
		{
			BinaryReader br = new BinaryReader(input);
			BinaryWriter bw = new BinaryWriter(output);

			if (bytesToAddToHead.Count() > 0)
			{
				bw.Write(bytesToAddToHead);
			}

			int bufferSize = 1024;

			// Now read the file in chunks and write out
			byte[] buffer = new byte[bufferSize];
			while (br.BaseStream.Position <= (br.BaseStream.Length - bufferSize))
			{
				buffer = br.ReadBytes(bufferSize);
				bw.Write(buffer);
			}

			// For the final chunk, if any, write out only that number of bytes
			int length = (int)(br.BaseStream.Length - br.BaseStream.Position);
			buffer = new byte[length];
			buffer = br.ReadBytes(length);
			bw.Write(buffer);

			if (bytesToAddToTail.Count() > 0)
			{
				bw.Write(bytesToAddToTail);
			}
		}

		#endregion
	}
}
