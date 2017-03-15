using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using xxHashSharp;

namespace SabreTools.Helper.Tools
{
	public static class FileTools
	{
		#region File Information

		/// <summary>
		/// Get what type of DAT the input file is
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The DatFormat corresponding to the DAT</returns>
		/// <remarks>There is currently no differentiation between XML and SabreDAT here</remarks>
		public static DatFormat GetDatFormat(string filename)
		{
			// Limit the output formats based on extension
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}
			if (ext != "csv" && ext != "dat" && ext != "md5" && ext != "sfv" && ext != "sha1"
				&& ext != "sha384" && ext != "sha512" && ext != "tsv" && ext != "txt" && ext != "xml")
			{
				return 0;
			}

			// Read the input file, if possible
			Globals.Logger.Verbose("Attempting to read file to get format: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				Globals.Logger.Warning("File '" + filename + "' could not read from!");
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
			if (ext == "sha256")
			{
				return DatFormat.RedumpSHA256;
			}
			if (ext == "sha384")
			{
				return DatFormat.RedumpSHA384;
			}
			if (ext == "sha512")
			{
				return DatFormat.RedumpSHA512;
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
				else if (first.Contains("#Name;Title;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra"))
				{
					return DatFormat.AttractMode;
				}
				else if (first.Contains("\"File Name\",\"Internal Name\",\"Description\""))
				{
					return DatFormat.CSV;
				}
				else if (first.Contains("\"File Name\"\t\"Internal Name\"\t\"Description\""))
				{
					return DatFormat.TSV;
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
		/// Get all empty folders within a root folder
		/// </summary>
		/// <param name="root">Root directory to parse</param>
		/// <returns>IEumerable containing all directories that are empty, an empty enumerable if the root is empty, null otherwise</returns>
		public static IEnumerable<string> GetEmptyDirectories(string root)
		{
			// Check if the root exists first
			if (!Directory.Exists(root))
			{
				return null;
			}

			// If it does and it is empty, return a blank enumerable
			if (Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories).Count() == 0)
			{
				return new List<string>();
			}

			// Otherwise, get the complete list
			return Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
				.Where(dir => Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.AllDirectories).Count() == 0);
		}

		/// <summary>
		/// Retrieve file information for a single file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
		/// <param name="offset">Set a >0 number for getting hash for part of the file, 0 otherwise (default)</param>
		/// <param name="date">True if the file Date should be included, false otherwise (default)</param>
		/// <param name="header">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetFileInfo(string input, Hash omitFromScan = 0x0,
			long offset = 0, bool date = false, string header = null)
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
				SkipperRule rule = Skipper.GetMatchingRule(input, Path.GetFileNameWithoutExtension(header));

				// If there's a match, get the new information from the stream
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Create the input and output streams
					MemoryStream outputStream = new MemoryStream();
					FileStream inputStream = File.OpenRead(input);

					// Transform the stream and get the information from it
					rule.TransformStream(inputStream, outputStream, keepReadOpen: false, keepWriteOpen: true);
					rom = GetStreamInfo(outputStream, outputStream.Length);

					// Dispose of the streams
					outputStream.Dispose();
					inputStream.Dispose();
				}
				// Otherwise, just get the info
				else
				{
					rom = GetStreamInfo(File.OpenRead(input), new FileInfo(input).Length, omitFromScan, offset, false);
				}
			}
			else
			{
				rom = GetStreamInfo(File.OpenRead(input), new FileInfo(input).Length, omitFromScan, offset, false);
			}

			// Add unique data from the file
			rom.Name = Path.GetFileName(input);
			rom.Date = (date ? new FileInfo(input).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") : "");

			return rom;
		}

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
		/// <returns>True if the output file was created, false otherwise</returns>
		public static bool DetectSkipperAndTransform(string file, string outDir)
		{
			// Create the output directory if it doesn't exist
			if (outDir != "" && !Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			Globals.Logger.User("\nGetting skipper information for '" + file + "'");

			// Get the skipper rule that matches the file, if any
			SkipperRule rule = Skipper.GetMatchingRule(file, "");

			// If we have an empty rule, return false
			if (rule.Tests == null || rule.Tests.Count == 0 || rule.Operation != HeaderSkipOperation.None)
			{
				return false;
			}

			Globals.Logger.User("File has a valid copier header");

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
			rule.TransformFile(file, newfile);

			// If the output file doesn't exist, return false
			if (!File.Exists(newfile))
			{
				return false;
			}

			// Now add the information to the database if it's not already there
			Rom rom = GetFileInfo(newfile);
			DatabaseTools.AddHeaderToDatabase(hstr, rom.SHA1, rule.SourceFile);

			return true;
		}

		/// <summary>
		/// Retrieve a list of just files from inputs
		/// </summary>
		/// <param name="inputs">List of strings representing directories and files</param>
		/// <param name="appendparent">True if the parent name should be appended after the special character "¬", false otherwise</param>
		/// <returns>List of strings representing just files from the inputs</returns>
		public static List<string> GetOnlyFilesFromInputs(List<string> inputs, bool appendparent = false)
		{
			List<string> outputs = new List<string>();
			foreach (string input in inputs)
			{
				if (Directory.Exists(input))
				{
					List<string> files = FileTools.RetrieveFiles(input, new List<string>());

					// Make sure the files in the directory are ordered correctly
					files = Style.OrderByAlphaNumeric(files, s => s).ToList();
					foreach (string file in files)
					{
						try
						{
							outputs.Add(Path.GetFullPath(file) + (appendparent ? "¬" + Path.GetFullPath(input) : ""));
						}
						catch (PathTooLongException)
						{
							Globals.Logger.Warning("The path for " + file + " was too long");
						}
						catch (Exception ex)
						{
							Globals.Logger.Error(ex.ToString());
						}
					}
				}
				else if (File.Exists(input))
				{
					try
					{
						outputs.Add(Path.GetFullPath(input) + (appendparent ? "¬" + Path.GetFullPath(input) : ""));
					}
					catch (PathTooLongException)
					{
						Globals.Logger.Warning("The path for " + input + " was too long");
					}
					catch (Exception ex)
					{
						Globals.Logger.Error(ex.ToString());
					}
				}
			}

			return outputs;
		}

		/// <summary>
		/// Get the XmlTextReader associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The XmlTextReader representing the (possibly converted) file, null otherwise</returns>
		public static XmlReader GetXmlTextReader(string filename)
		{
			Globals.Logger.Verbose("Attempting to read file: \"" + filename + "\"");

			// Check if file exists
			if (!File.Exists(filename))
			{
				Globals.Logger.Warning("File '" + filename + "' could not read from!");
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
		/// Detect and replace header(s) to the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		/// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
		/// <returns>True if a header was found and appended, false otherwise</returns>
		public static bool RestoreHeader(string file, string outDir)
		{
			// Create the output directory if it doesn't exist
			if (outDir != "" && !Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// First, get the SHA-1 hash of the file
			Rom rom = GetFileInfo(file);

			// Retrieve a list of all related headers from the database
			List<string> headers = DatabaseTools.RetrieveHeadersFromDatabase(rom.SHA1);

			// If we have nothing retrieved, we return false
			if (headers.Count == 0)
			{
				return false;
			}

			// Now loop through and create the reheadered files, if possible
			for (int i = 0; i < headers.Count; i++)
			{
				Globals.Logger.User("Creating reheadered file: " +
						(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + i);
				AppendBytesToFile(file,
					(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + i, headers[i], string.Empty);
				Globals.Logger.User("Reheadered file created!");
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
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
		/// <param name="offset">Set a >0 number for getting hash for part of the file, 0 otherwise (default)</param>
		/// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetStreamInfo(Stream input, long size, Hash omitFromScan = 0x0,
			long offset = 0, bool keepReadOpen = false)
		{
			Rom rom = new Rom
			{
				Type = ItemType.Rom,
				Size = size,
				CRC = string.Empty,
				MD5 = string.Empty,
				SHA1 = string.Empty,
				SHA256 = string.Empty,
			};

			try
			{
				// Initialize the hashers
				OptimizedCRC crc = new OptimizedCRC();
				MD5 md5 = MD5.Create();
				SHA1 sha1 = SHA1.Create();
				SHA256 sha256 = SHA256.Create();
				SHA384 sha384 = SHA384.Create();
				SHA512 sha512 = SHA512.Create();
				xxHash xxHash = new xxHash();
				xxHash.Init();

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
					if ((omitFromScan & Hash.MD5) == 0)
					{
						md5.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA1) == 0)
					{
						sha1.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA256) == 0)
					{
						sha256.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA384) == 0)
					{
						sha384.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA512) == 0)
					{
						sha512.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.xxHash) == 0)
					{
						xxHash.Update(buffer, read);
					}
				}

				crc.Update(buffer, 0, 0);
				rom.CRC = crc.Value.ToString("X8").ToLowerInvariant();

				if ((omitFromScan & Hash.MD5) == 0)
				{
					md5.TransformFinalBlock(buffer, 0, 0);
					rom.MD5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
				}
				if ((omitFromScan & Hash.SHA1) == 0)
				{
					sha1.TransformFinalBlock(buffer, 0, 0);
					rom.SHA1 = BitConverter.ToString(sha1.Hash).Replace("-", "").ToLowerInvariant();
				}
				if ((omitFromScan & Hash.SHA256) == 0)
				{
					sha256.TransformFinalBlock(buffer, 0, 0);
					rom.SHA256 = BitConverter.ToString(sha256.Hash).Replace("-", "").ToLowerInvariant();
				}
				if ((omitFromScan & Hash.SHA384) == 0)
				{
					sha384.TransformFinalBlock(buffer, 0, 0);
					rom.SHA384 = BitConverter.ToString(sha384.Hash).Replace("-", "").ToLowerInvariant();
				}
				if ((omitFromScan & Hash.SHA512) == 0)
				{
					sha512.TransformFinalBlock(buffer, 0, 0);
					rom.SHA512 = BitConverter.ToString(sha512.Hash).Replace("-", "").ToLowerInvariant();
				}
				if ((omitFromScan & Hash.xxHash) == 0)
				{
					//rom.xxHash = xxHash.Digest().ToString("X8").ToLowerInvariant();
				}

				// Dispose of the hashers
				crc.Dispose();
				md5.Dispose();
				sha1.Dispose();
				sha256.Dispose();
				sha384.Dispose();
				sha512.Dispose();
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
