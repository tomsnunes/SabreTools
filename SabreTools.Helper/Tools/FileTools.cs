using Mono.Data.Sqlite;
using OCRC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Schema;

namespace SabreTools.Helper
{
	public class FileTools
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
					else if (second.StartsWith("<dat"))
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
		/// <param name="removeHeader">True if headers should be removed from files if possible, false otherwise (default)</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetSingleFileInfo(string input, Logger logger, bool noMD5 = false, bool noSHA1 = false, long offset = 0, bool date = false, bool removeHeader = false)
		{
			// Add safeguard if file doesn't exist
			if (!File.Exists(input))
			{
				return new Rom();
			}

			// Get the information from the file stream
			Rom rom = new Rom();
			if (removeHeader)
			{
				SkipperRule rule = Skippers.MatchesSkipper(input, "", logger);

				// If there's a match, get the new information from the stream
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Create the input and output streams
					MemoryStream outputStream = new MemoryStream();
					FileStream inputStream = File.OpenRead(input);

					// Transform the stream and get the information from it
					Skippers.TransformStream(inputStream, outputStream, rule, logger, false, true);
					rom = GetSingleStreamInfo(outputStream, outputStream.Length);

					// Dispose of the streams
					outputStream.Dispose();
					inputStream.Dispose();
				}
				// Otherwise, just get the info
				else
				{
					rom = GetSingleStreamInfo(File.OpenRead(input), new FileInfo(input).Length, noMD5, noSHA1, offset, false);
				}
			}
			else
			{
				rom = GetSingleStreamInfo(File.OpenRead(input), new FileInfo(input).Length, noMD5, noSHA1, offset, false);
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
			SkipperRule rule = Skippers.MatchesSkipper(file, "", logger);

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
			Skippers.TransformFile(file, newfile, rule, logger);

			// If the output file doesn't exist, return false
			if (!File.Exists(newfile))
			{
				return false;
			}

			// Now add the information to the database if it's not already there
			Rom rom = GetSingleFileInfo(newfile, logger);
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
			Rom rom = GetSingleFileInfo(file, logger);

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
		public static Rom GetSingleStreamInfo(Stream input, long size, bool noMD5 = false, bool noSHA1 = false, long offset = 0, bool keepReadOpen = false)
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

				byte[] buffer = new byte[1024];
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
