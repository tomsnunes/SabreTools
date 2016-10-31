using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

using SabreTools.Helper.Data;
using SabreTools.Helper.Dats;
using SabreTools.Helper.Skippers;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = System.IO.BinaryWriter;
using FileStream = System.IO.FileStream;
using IOException = System.IO.IOException;
using MemoryStream = System.IO.MemoryStream;
using PathTooLongException = System.IO.PathTooLongException;
using SearchOption = System.IO.SearchOption;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
using StreamReader = System.IO.StreamReader;
#endif
using NaturalSort;
using OCRC;

namespace SabreTools.Helper.Tools
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
			dirs = Style.OrderByAlphaNumeric(dirs, s => s).ToList();
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
		/// <returns>The DatFormat corresponding to the DAT</returns>
		/// <remarks>There is currently no differentiation between XML and SabreDAT here</remarks>
		public static DatFormat GetDatFormat(string filename, Logger logger)
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
			logger.Verbose("Attempting to read file to get format: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				logger.Warning("File '" + filename + "' could not read from!");
				return 0;
			}

			// Some formats only require the extension to know
			if (ext == "md5")
			{
				return DatFormat.RedumpMD5;
			}
			if (ext == "sfv")
			{
				return DatFormat.RedumpSFV;
			}
			if (ext == "sha1")
			{
				return DatFormat.RedumpSHA1;
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
						return DatFormat.Logiqx;
					}
					else if (second.StartsWith("<!doctype softwarelist"))
					{
						return DatFormat.SoftwareList;
					}
					else if (second.StartsWith("<!doctype sabredat"))
					{
						return DatFormat.SabreDat;
					}
					else if (second.StartsWith("<dat") && !second.StartsWith("<datafile"))
					{
						return DatFormat.OfflineList;
					}
					// Older and non-compliant DATs
					else
					{
						return DatFormat.Logiqx;
					}
				}

				// If we have an INI-based DAT
				else if (first.Contains("[") && first.Contains("]"))
				{
					return DatFormat.RomCenter;
				}

				// If we have a CMP-based DAT
				else if (first.Contains("clrmamepro"))
				{
					return DatFormat.ClrMamePro;
				}
				else if (first.Contains("romvault"))
				{
					return DatFormat.ClrMamePro;
				}
				else if (first.Contains("doscenter"))
				{
					return DatFormat.DOSCenter;
				}
				else
				{
					return DatFormat.ClrMamePro;
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
		/// Retrieve a list of just files from inputs
		/// </summary>
		/// <param name="inputs">List of strings representing directories and files</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="appendparent">True if the parent name should be appended after the special character "¬", false otherwise</param>
		/// <returns>List of strings representing just files from the inputs</returns>
		public static List<string> GetOnlyFilesFromInputs(List<string> inputs, int maxDegreeOfParallelism, Logger logger, bool appendparent = false)
		{
			List<string> outputs = new List<string>();
			Parallel.ForEach(inputs,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, },
				input =>
				{
					if (Directory.Exists(input))
					{
						List<string> files = FileTools.RetrieveFiles(input, new List<string>());
						foreach (string file in files)
						{
							try
							{
								lock (outputs)
								{
									outputs.Add(Path.GetFullPath(file) + (appendparent ? "¬" + Path.GetFullPath(input) : ""));
								}
							}
							catch (PathTooLongException)
							{
								logger.Warning("The path for " + file + " was too long");
							}
							catch (Exception ex)
							{
								logger.Error(ex.ToString());
							}
						}
					}
					else if (File.Exists(input))
					{
						try
						{
							lock (outputs)
							{
								outputs.Add(Path.GetFullPath(input) + (appendparent ? "¬" + Path.GetFullPath(input) : ""));
							}
						}
						catch (PathTooLongException)
						{
							logger.Warning("The path for " + input + " was too long");
						}
						catch (Exception ex)
						{
							logger.Error(ex.ToString());
						}
					}
				});

			return outputs;
		}

		/// <summary>
		/// Get the romba path for a file based on the rom's SHA-1
		/// </summary>
		/// <param name="rom">Rom to get the sha1 from</param>
		/// <param name="baseOutDir">Base output folder</param>
		/// <returns>Formatted path string to use</returns>
		public static string GetRombaPath(Rom rom, string baseOutDir)
		{
			string subfolder = Path.Combine(rom.SHA1.Substring(0, 2), rom.SHA1.Substring(2, 2), rom.SHA1.Substring(4, 2), rom.SHA1.Substring(6, 2));
			return Path.Combine(baseOutDir, subfolder);
		}

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

			XmlReader xtr = XmlReader.Create(filename, new XmlReaderSettings
			{
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
		/// Move a file to a named, Romba-style subdirectory
		/// </summary>
		/// <param name="rom">Rom to get the sha1 from</param>
		/// <param name="baseOutDir">Base output folder</param>
		/// <param name="filename">Name of the file to be moved</param>
		/// <param name="logger">Logger object for file and console output</param>
		public static void MoveToRombaFolder(Rom rom, string baseOutDir, string filename, Logger logger)
		{
			string outDir = GetRombaPath(rom, baseOutDir);
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			try
			{
				File.Move(filename, Path.Combine(outDir, Path.GetFileName(filename)));
			}
			catch (Exception ex)
			{
				logger.Warning(ex.ToString());
				File.Delete(filename);
			}
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

			// First, get the SHA-1 hash of the file
			Rom rom = GetFileInfo(file, logger);

			// Retrieve a list of all related headers from the database
			List<string> headers = DatabaseTools.RetrieveHeadersFromDatabase(rom.SHA1, logger);

			// If we have nothing retrieved, we return false
			if (headers.Count == 0)
			{
				return false;
			}

			// Now loop through and create the reheadered files, if possible
			for (int i = 0; i < headers.Count; i++)
			{
				logger.User("Creating reheadered file: " +
						(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + i);
				AppendBytesToFile(file,
					(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + i, headers[i], string.Empty);
				logger.User("Reheadered file created!");
			}

			return true;
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
