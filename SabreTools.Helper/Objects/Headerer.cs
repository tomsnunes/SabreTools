using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
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
		private bool _restore;
		private Logger _logger;

		// Private required variables
		private static string _headererDbSchema = "Headerer";
		private static string _headererDbName = "Headerer.sqlite";
		private static string _headererConnectionString = "Data Source=" + _headererDbName + ";Version = 3;";

		/// <summary>
		/// Create a new Headerer object
		/// </summary>
		/// <param name="input">Input file or folder name</param>
		/// <param name="restore">False if we're extracting headers (default), true if we're restoring them</param>
		/// <param name="logger">Logger object for file and console output</param>
		public Headerer(string input, bool restore, Logger logger)
		{
			_input = input;
			_restore = restore;
			_logger = logger;
		}

		/// <summary>
		/// Extract and remove or replace headers
		/// </summary>
		/// <returns>True if it succeeded, false otherwise</returns>
		public bool Process()
		{
			if (_restore)
			{
				// If it's a single file, just check it
				if (File.Exists(_input))
				{
					RestoreHeader(_input);
				}
				// If it's a directory, recursively check all
				else if (Directory.Exists(_input))
				{
					foreach (string sub in Directory.EnumerateFiles(_input, "*", SearchOption.AllDirectories))
					{
						if (sub != ".." && sub != ".")
						{
							RestoreHeader(sub);
						}
					}
				}
			}
			else
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

				// Otherwise, apply the rule to the file
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
			using (SqliteConnection dbc = new SqliteConnection(_headererConnectionString))
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
				using (SqliteConnection dbc = new SqliteConnection(_headererConnectionString))
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
		public bool RestoreHeader(string file)
		{
			// First, get the SHA-1 hash of the file
			Rom rom = FileTools.GetSingleFileInfo(file);

			// Then try to pull the corresponding headers from the database
			string header = "";

			string query = @"SELECT header, type FROM data WHERE sha1='" + rom.HashData.SHA1 + "'";
			using (SqliteConnection dbc = new SqliteConnection(_headererConnectionString))
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
