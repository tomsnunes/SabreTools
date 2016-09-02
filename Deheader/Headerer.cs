using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the Deheader application
	/// </summary>
	public class Headerer
	{
		// Private instance variables
		private string _input;
		private bool _flag;
		private Logger _logger;

		// Private required variables
		private static string _dbName = "Headerer.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";

		/// <summary>
		/// Create a new Headerer object
		/// </summary>
		/// <param name="input">Input file or folder name</param>
		/// <param name="flag">True if we're extracting headers (default), false if we're replacing them</param>
		/// <param name="logger">Logger object for file and console output</param>
		public Headerer(string input, bool flag, Logger logger)
		{
			_input = input;
			_flag = flag;
			_logger = logger;
		}

		/// <summary>
		/// Start deheader operation with supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Perform initial setup and verification
			Logger logger = new Logger(true, "headerer.log");
			logger.Start();
			DBTools.EnsureDatabase(_dbName, _connectionString);

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			// If we have no arguments, show the help
			if (args.Length == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("Headerer");

			// Get the filename (or foldername)
			string input = "";
			bool help = false,
				flag = true,
				headerer = true;
			foreach (string arg in args)
			{
				string temparg = arg.Replace("\"", "").Replace("file://", "");
				switch (temparg)
				{
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-e":
					case "--extract":
						flag = true;
						break;
					case "-r":
					case "--replace":
						flag = false;
						break;
					default:
						if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							input = temparg;
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
			if (String.IsNullOrEmpty(input) && (headerer))
			{
				logger.Error("This feature requires at exactly one input");
				Build.Help();
				logger.Close();
				return;
			}

			// If we're in headerer mode
			if (headerer)
			{
				InitHeaderer(input, flag, logger);
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
		/// Wrap extracting and replacing headers
		/// </summary>
		/// <param name="input">Input file or folder name</param>
		/// <param name="flag">True if we're extracting headers (default), false if we're replacing them</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitHeaderer(string input, bool flag, Logger logger)
		{
			Headerer headerer = new Headerer(input, flag, logger);
			headerer.Process();
		}

		/// <summary>
		/// Extract and remove or replace headers
		/// </summary>
		/// <returns>True if it succeeded, false otherwise</returns>
		public bool Process()
		{
			if (_flag)
			{
				// If it's a single file, just check it
				if (File.Exists(_input))
				{
					DetectSkipperAndTransform(_input);
				}
				// If it's a directory, recursively check all
				else if (Directory.Exists(_input))
				{
					foreach (string sub in Directory.EnumerateFiles(_input, "*", SearchOption.AllDirectories))
					{
						if (sub != ".." && sub != ".")
						{
							DetectSkipperAndTransform(sub);
						}
					}
				}
			}
			else
			{
				// If it's a single file, just check it
				if (File.Exists(_input))
				{
					ReplaceHeader(_input);
				}
				// If it's a directory, recursively check all
				else if (Directory.Exists(_input))
				{
					foreach (string sub in Directory.GetFiles(_input))
					{
						if (sub != ".." && sub != ".")
						{
							ReplaceHeader(sub);
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Detect header skipper compliance and create an output file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		/// <returns>True if the output file was created, false otherwise</returns>
		public bool DetectSkipperAndTransform(string file)
		{
			_logger.User("\nGetting skipper information for '" + file + "'");

			// Then, if the file was headered, store it to the database
			int headerSize = -1;
			HeaderType type = Skippers.GetFileHeaderType(file, out headerSize, _logger);

			// If we have a valid HeaderType, remove the correct byte count
			_logger.User("File has header: " + (type != HeaderType.None));
			if (type != HeaderType.None)
			{
				_logger.Log("Deteched header type: " + type);

				// Now take care of the header and new output file
				string hstr = string.Empty;
				using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
				{
					// Extract the header as a string for the database
					byte[] hbin = br.ReadBytes(headerSize);
					for (int i = 0; i < headerSize; i++)
					{
						hstr += BitConverter.ToString(new byte[] { hbin[i] });
					}
				}

				// Then find an apply the exact rule to the file
				SkipperRule rule = Skippers.MatchesSkipper(file, "", _logger);

				// If we have an empty rule, return false
				if (rule.Tests == null || rule.Tests.Count == 0)
				{
					return false;
				}

				// Otherwise, apply the rule ot the file
				string newfile = file + ".new";
				Skippers.TransformFile(file, newfile, rule, _logger);

				// If the output file doesn't exist, return false
				if (!File.Exists(newfile))
				{
					return false;
				}

				// Now add the information to the database if it's not already there
				Rom rom = FileTools.GetSingleFileInfo(newfile);
				AddHeaderToDatabase(hstr, rom.HashData.SHA1, type);
			}

			return true;
		}

		/// <summary>
		/// Add a header to the database
		/// </summary>
		/// <param name="header">String representing the header bytes</param>
		/// <param name="SHA1">SHA-1 of the deheadered file</param>
		/// <param name="type">HeaderType representing the detected header</param>
		public void AddHeaderToDatabase(string header, string SHA1, HeaderType type)
		{
			bool exists = false;

			string query = @"SELECT * FROM data WHERE sha1='" + SHA1 + "' AND header='" + header + "'";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						exists = sldr.HasRows;
					}
				}
			}

			if (!exists)
			{
				query = @"INSERT INTO data (sha1, header, type) VALUES ('" +
				SHA1 + "', " +
				"'" + header + "', " +
				"'" + type.ToString() + "')";
				using (SqliteConnection dbc = new SqliteConnection(_connectionString))
				{
					dbc.Open();
					using (SqliteCommand slc = new SqliteCommand(query, dbc))
					{
						_logger.Log("Result of inserting header: " + slc.ExecuteNonQuery());
					}
				}
			}
		}

		/// <summary>
		/// Detect and replace header(s) to the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		/// <returns>True if a header was found and appended, false otherwise</returns>
		public bool ReplaceHeader(string file)
		{
			// First, get the SHA-1 hash of the file
			Rom rom = FileTools.GetSingleFileInfo(file);

			// Then try to pull the corresponding headers from the database
			string header = "";

			string query = @"SELECT header, type FROM data WHERE sha1='" + rom.HashData.SHA1 + "'";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						if (sldr.HasRows)
						{
							int sub = 0;
							while (sldr.Read())
							{
								_logger.Log("Found match with rom type " + sldr.GetString(1));
								header = sldr.GetString(0);

								_logger.User("Creating reheadered file: " + file + ".new" + sub);
								FileTools.AppendBytesToFile(file, file + ".new" + sub, header, string.Empty);
								_logger.User("Reheadered file created!");
							}
						}
						else
						{
							_logger.Warning("No matching header could be found!");
							return false;
						}
					}
				}
			}

			return true;
		}
	}
}
