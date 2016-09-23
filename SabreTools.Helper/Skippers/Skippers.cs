using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace SabreTools.Helper
{
	public class Skippers
	{
		// Local paths
		public const string LocalPath = "Skippers";

		// Header skippers represented by a list of skipper objects
		private static List<Skipper> _list;
		public static List<Skipper> List
		{
			get
			{
				if (_list == null || _list.Count == 0)
				{
					PopulateSkippers();
				}
				return _list;
			}
		}

		/// <summary>
		/// Populate the entire list of header Skippers
		/// </summary>
		/// <remarks>
		/// http://mamedev.emulab.it/clrmamepro/docs/xmlheaders.txt
		/// http://www.emulab.it/forum/index.php?topic=127.0
		/// </remarks>
		private static void PopulateSkippers()
		{
			if (_list == null)
			{
				_list = new List<Skipper>();
			}

			foreach (string skipperFile in Directory.EnumerateFiles(LocalPath, "*", SearchOption.AllDirectories))
			{
				_list.Add(PopulateSkippersHelper(Path.GetFullPath(skipperFile)));
			}
		}

		/// <summary>
		/// Populate an individual Skipper from file
		/// </summary>
		/// <param name="filename">Name of the file to be read from</param>
		/// <returns>The Skipper object associated with the file</returns>
		private static Skipper PopulateSkippersHelper(string filename)
		{
			Skipper skipper = new Skipper
			{
				Rules = new List<SkipperRule>(),
				SourceFile = filename,
			};

			if (!File.Exists(filename))
			{
				return skipper;
			}

			Logger logger = new Logger(false, "");
			XmlTextReader xtr = FileTools.GetXmlTextReader(filename, logger);

			if (xtr == null)
			{
				return skipper;
			}

			bool valid = false;
			xtr.MoveToContent();
			while (!xtr.EOF)
			{
				if (xtr.NodeType != XmlNodeType.Element)
				{
					xtr.Read();
				}

				switch (xtr.Name.ToLowerInvariant())
				{
					case "detector":
						valid = true;
						xtr.Read();
						break;
					case "name":
						skipper.Name = xtr.ReadElementContentAsString();
						break;
					case "author":
						skipper.Author = xtr.ReadElementContentAsString();
						break;
					case "version":
						skipper.Version = xtr.ReadElementContentAsString();
						break;
					case "rule":
						// Get the information from the rule first
						SkipperRule rule = new SkipperRule
						{
							StartOffset = 0,
							EndOffset = 0,
							Operation = HeaderSkipOperation.None,
							Tests = new List<SkipperTest>(),
							SourceFile = filename,
						};

						if (xtr.GetAttribute("start_offset") != null)
						{
							string offset = xtr.GetAttribute("start_offset");
							if (offset.ToLowerInvariant() == "eof")
							{
								rule.StartOffset = null;
							}
							else
							{
								rule.StartOffset = Convert.ToInt64(offset, 16);
							}
						}
						if (xtr.GetAttribute("end_offset") != null)
						{
							string offset = xtr.GetAttribute("end_offset");
							if (offset.ToLowerInvariant() == "eof")
							{
								rule.EndOffset = null;
							}
							else
							{
								rule.EndOffset = Convert.ToInt64(offset, 16);
							}
						}
						if (xtr.GetAttribute("operation") != null)
						{
							string operation = xtr.GetAttribute("operation");
							switch (operation.ToLowerInvariant())
							{
								case "bitswap":
									rule.Operation = HeaderSkipOperation.Bitswap;
									break;
								case "byteswap":
									rule.Operation = HeaderSkipOperation.Byteswap;
									break;
								case "wordswap":
									rule.Operation = HeaderSkipOperation.Wordswap;
									break;
							}
						}

						// Now read the individual tests into the Rule
						XmlReader subreader = xtr.ReadSubtree();

						if (subreader != null)
						{
							while (!subreader.EOF)
							{
								if (subreader.NodeType != XmlNodeType.Element)
								{
									subreader.Read();
								}

								// Get the test type
								SkipperTest test = new SkipperTest
								{
									Offset = 0,
									Value = new byte[0],
									Result = true,
									Mask = new byte[0],
									Size = 0,
									Operator = HeaderSkipTestFileOperator.Equal,
								};
								switch (subreader.Name.ToLowerInvariant())
								{
									case "data":
										test.Type = HeaderSkipTest.Data;
										break;
									case "or":
										test.Type = HeaderSkipTest.Or;
										break;
									case "xor":
										test.Type = HeaderSkipTest.Xor;
										break;
									case "and":
										test.Type = HeaderSkipTest.And;
										break;
									case "file":
										test.Type = HeaderSkipTest.File;
										break;
									default:
										subreader.Read();
										break;
								}

								// Now populate all the parts that we can
								if (subreader.GetAttribute("offset") != null)
								{
									string offset = subreader.GetAttribute("offset");
									if (offset.ToLowerInvariant() == "eof")
									{
										test.Offset = null;
									}
									else
									{
										test.Offset = Convert.ToInt64(offset, 16);
									}
								}
								if (subreader.GetAttribute("value") != null)
								{
									string value = subreader.GetAttribute("value");

									// http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
									test.Value = new byte[value.Length / 2];
									for (int index = 0; index < test.Value.Length; index++)
									{
										string byteValue = value.Substring(index * 2, 2);
										test.Value[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
									}
								}
								if (subreader.GetAttribute("result") != null)
								{
									string result = subreader.GetAttribute("result");
									switch (result.ToLowerInvariant())
									{
										case "false":
											test.Result = false;
											break;
										case "true":
										default:
											test.Result = true;
											break;
									}
								}
								if (subreader.GetAttribute("mask") != null)
								{
									string mask = subreader.GetAttribute("mask");

									// http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
									test.Mask = new byte[mask.Length / 2];
									for (int index = 0; index < test.Mask.Length; index++)
									{
										string byteValue = mask.Substring(index * 2, 2);
										test.Mask[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
									}
								}
								if (subreader.GetAttribute("size") != null)
								{
									string size = subreader.GetAttribute("size");
									if (size.ToLowerInvariant() == "po2")
									{
										test.Size = null;
									}
									else
									{
										test.Size = Convert.ToInt64(size, 16);
									}
								}
								if (subreader.GetAttribute("operator") != null)
								{
									string oper = subreader.GetAttribute("operator");
									switch (oper.ToLowerInvariant())
									{
										case "less":
											test.Operator = HeaderSkipTestFileOperator.Less;
											break;
										case "greater":
											test.Operator = HeaderSkipTestFileOperator.Greater;
											break;
										case "equal":
										default:
											test.Operator = HeaderSkipTestFileOperator.Equal;
											break;
									}
								}

								// Add the created test to the rule
								rule.Tests.Add(test);
								subreader.Read();
							}
						}

						// Add the created rule to the skipper
						skipper.Rules.Add(rule);
						xtr.Skip();
						break;
					default:
						xtr.Read();
						break;
				}
			}

			return (valid ? skipper : new Skipper());
		}

		/// <summary>
		/// Get the SkipperRule associated with a given file
		/// </summary>
		/// <param name="input">Name of the file to be checked</param>
		/// <param name="skipperName">Name of the skipper to be used</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The SkipperRule that matched the file</returns>
		public static SkipperRule MatchesSkipper(string input, string skipperName, Logger logger)
		{
			// If the file doesn't exist, return a blank skipper rule
			if (!File.Exists(input))
			{
				logger.Error("The file '" + input + "' does not exist so it cannot be tested");
				return new SkipperRule();
			}

			return MatchesSkipper(File.OpenRead(input), skipperName, logger);
		}

		/// <summary>
		/// Get the SkipperRule associated with a given stream
		/// </summary>
		/// <param name="input">Name of the file to be checked</param>
		/// <param name="skipperName">Name of the skipper to be used</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="keepOpen">True if the underlying stream should be kept open, false otherwise</param>
		/// <returns>The SkipperRule that matched the file</returns>
		public static SkipperRule MatchesSkipper(Stream input, string skipperName, Logger logger, bool keepOpen = false)
		{
			SkipperRule skipperRule = new SkipperRule();

			// Loop through and find a Skipper that has the right name
			logger.Log("Beginning search for matching header skip rules");
			foreach (Skipper skipper in List)
			{
				if (String.IsNullOrEmpty(skipperName) || (!String.IsNullOrEmpty(skipper.Name) && skipperName.ToLowerInvariant() == skipper.Name.ToLowerInvariant()))
				{
					// Loop through the rules until one is found that works
					BinaryReader br = new BinaryReader(input);

					foreach (SkipperRule rule in skipper.Rules)
					{
						// Always reset the stream back to the original place
						input.Seek(0, SeekOrigin.Begin);

						// For each rule, make sure it passes each test
						bool success = true;
						foreach (SkipperTest test in rule.Tests)
						{
							bool result = true;
							switch (test.Type)
							{
								case HeaderSkipTest.Data:
									// First seek to the correct position
									if (test.Offset == null)
									{
										input.Seek(0, SeekOrigin.End);
									}
									else if (test.Offset > 0 && test.Offset <= input.Length)
									{
										input.Seek((long)test.Offset, SeekOrigin.Begin);
									}
									else if (test.Offset < 0 && Math.Abs((long)test.Offset) <= input.Length)
									{
										input.Seek((long)test.Offset, SeekOrigin.End);
									}

									// Then read and compare bytewise
									result = true;
									for (int i = 0; i < test.Value.Length; i++)
									{
										try
										{
											if (br.ReadByte() != test.Value[i])
											{
												result = false;
												break;
											}
										}
										catch
										{
											result = false;
											break;
										}
									}

									// Return if the expected and actual results match
									success &= (result == test.Result);
									break;
								case HeaderSkipTest.Or:
								case HeaderSkipTest.Xor:
								case HeaderSkipTest.And:
									// First seek to the correct position
									if (test.Offset == null)
									{
										input.Seek(0, SeekOrigin.End);
									}
									else if (test.Offset > 0 && test.Offset <= input.Length)
									{
										input.Seek((long)test.Offset, SeekOrigin.Begin);
									}
									else if (test.Offset < 0 && Math.Abs((long)test.Offset) <= input.Length)
									{
										input.Seek((long)test.Offset, SeekOrigin.End);
									}

									result = true;
									try
									{
										// Then apply the mask if it exists
										byte[] read = br.ReadBytes(test.Mask.Length);
										byte[] masked = new byte[test.Mask.Length];
										for (int i = 0; i < read.Length; i++)
										{
											masked[i] = (byte)(test.Type == HeaderSkipTest.And ? read[i] & test.Mask[i] :
												(test.Type == HeaderSkipTest.Or ? read[i] | test.Mask[i] : read[i] ^ test.Mask[i])
											);
										}

										// Finally, compare it against the value
										for (int i = 0; i < test.Value.Length; i++)
										{
											if (masked[i] != test.Value[i])
											{
												result = false;
												break;
											}
										}
									}
									catch
									{
										result = false;
									}

									// Return if the expected and actual results match
									success &= (result == test.Result);
									break;
								case HeaderSkipTest.File:
									// First get the file size from stream
									long size = input.Length;

									// If we have a null size, check that the size is a power of 2
									result = true;
									if (test.Size == null)
									{
										// http://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
										result = (((ulong)size & ((ulong)size - 1)) == 0);
									}
									else if (test.Operator == HeaderSkipTestFileOperator.Less)
									{
										result = (size < test.Size);
									}
									else if (test.Operator == HeaderSkipTestFileOperator.Greater)
									{
										result = (size > test.Size);
									}
									else if (test.Operator == HeaderSkipTestFileOperator.Equal)
									{
										result = (size == test.Size);
									}

									// Return if the expected and actual results match
									success &= (result == test.Result);
									break;
							}
						}

						// If we still have a success, then return this rule
						if (success)
						{
							// If we're not keeping the stream open, dispose of the binary reader
							if (!keepOpen)
							{
								input.Dispose();
							}

							logger.User(" Matching rule found!");
							return rule;
						}
					}
				}
			}

			// If we're not keeping the stream open, dispose of the binary reader
			if (!keepOpen)
			{
				input.Dispose();
			}

			// If we have a blank rule, inform the user
			if (skipperRule.Tests == null)
			{
				logger.Log("No matching rule found!");
			}

			return skipperRule;
		}

		/// <summary>
		/// Transform an input file using the given rule
		/// </summary>
		/// <param name="input">Input file name</param>
		/// <param name="output">Output file name</param>
		/// <param name="rule">SkipperRule to apply to the file</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the file was transformed properly, false otherwise</returns>
		public static bool TransformFile(string input, string output, SkipperRule rule, Logger logger)
		{
			bool success = true;

			// If the input file doesn't exist, fail
			if (!File.Exists(input))
			{
				logger.Error("I'm sorry but '" + input + "' doesn't exist!");
				return false;
			}

			// Create the output directory if it doesn't already
			if (!Directory.Exists(Path.GetDirectoryName(output)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(output));
			}

			logger.User("Attempting to apply rule to '" + input + "'");
			success = TransformStream(File.Open(input, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), File.OpenWrite(output), rule, logger);

			// If the output file has size 0, delete it
			if (new FileInfo(output).Length == 0)
			{
				try
				{
					File.Delete(output);
					success = false;
				}
				catch
				{
					// Don't log this file deletion error
				}
			}

			return success;
		}

		/// <summary>
		/// Transform an input stream using the given rule
		/// </summary>
		/// <param name="input">Input stream</param>
		/// <param name="output">Output stream</param>
		/// <param name="rule">SkipperRule to apply to the stream</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
		/// <param name="keepWriteOpen">True if the underlying write stream should be kept open, false otherwise</param>
		/// <returns>True if the file was transformed properly, false otherwise</returns>
		public static bool TransformStream(Stream input, Stream output, SkipperRule rule, Logger logger, bool keepReadOpen = false, bool keepWriteOpen = false)
		{
			bool success = true;

			// If the sizes are wrong for the values, fail
			long extsize = input.Length;
			if ((rule.Operation > HeaderSkipOperation.Bitswap && (extsize % 2) != 0)
				|| (rule.Operation > HeaderSkipOperation.Byteswap && (extsize % 4) != 0)
				|| (rule.Operation > HeaderSkipOperation.Bitswap && (rule.StartOffset == null || rule.StartOffset % 2 == 0)))
			{
				logger.Error("The stream did not have the correct size to be transformed!");
				return false;
			}

			// Now read the proper part of the file and apply the rule
			BinaryWriter bw = null;
			BinaryReader br = null;
			try
			{
				logger.User("Applying found rule to input stream");
				bw = new BinaryWriter(output);
				br = new BinaryReader(input);

				// Seek to the beginning offset
				if (rule.StartOffset == null)
				{
					success = false;
				}
				else if (Math.Abs((long)rule.StartOffset) > input.Length)
				{
					success = false;
				}
				else if (rule.StartOffset > 0)
				{
					input.Seek((long)rule.StartOffset, SeekOrigin.Begin);
				}
				else if (rule.StartOffset < 0)
				{
					input.Seek((long)rule.StartOffset, SeekOrigin.End);
				}

				// Then read and apply the operation as you go
				if (success)
				{
					byte[] buffer = new byte[4];
					int pos = 0;
					while (input.Position < (rule.EndOffset != null ? rule.EndOffset : input.Length)
						&& input.Position < input.Length)
					{
						byte b = br.ReadByte();
						switch (rule.Operation)
						{
							case HeaderSkipOperation.Bitswap:
								// http://stackoverflow.com/questions/3587826/is-there-a-built-in-function-to-reverse-bit-order
								uint r = b;
								int s = 7;
								for (b >>= 1; b != 0; b >>= 1)
								{
									r <<= 1;
									r |= (byte)(b & 1);
									s--;
								}
								r <<= s;
								buffer[pos] = (byte)r;
								break;
							case HeaderSkipOperation.Byteswap:
								if (pos % 2 == 1)
								{
									buffer[pos - 1] = b;
								}
								if (pos % 2 == 0)
								{
									buffer[pos + 1] = b;
								}
								break;
							case HeaderSkipOperation.Wordswap:
								buffer[3 - pos] = b;
								break;
							case HeaderSkipOperation.WordByteswap:
								buffer[(pos + 2) % 4] = b;
								break;
							case HeaderSkipOperation.None:
							default:
								buffer[pos] = b;
								break;
						}

						// Set the buffer position to default write to
						pos = (pos + 1) % 4;

						// If we filled a buffer, flush to the stream
						if (pos == 0)
						{
							bw.Write(buffer);
							bw.Flush();
							buffer = new byte[4];
						}
					}
					// If there's anything more in the buffer, write only the left bits
					for (int i = 0; i < pos; i++)
					{
						bw.Write(buffer[i]);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}
			finally
			{
				// If we're not keeping the read stream open, dispose of the binary reader
				if (!keepReadOpen)
				{
					br?.Dispose();
				}

				// If we're not keeping the write stream open, dispose of the binary reader
				if (!keepWriteOpen)
				{
					bw?.Dispose();
				}
			}

			return success;
		}
	}
}
