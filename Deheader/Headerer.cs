using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the Deheader application
	/// </summary>
	class Headerer
	{
		private static string _dbName = "Headerer.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";
		private static Logger logger;

		/// <summary>
		/// Start deheader operation with supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		static void Main(string[] args)
		{
			// Perform initial setup and verification
			Console.Clear();
			logger = new Logger(false, "database.log");
			logger.Start();
			DBTools.EnsureDatabase(_dbName, _connectionString);
			Remapping.CreateHeaderSkips();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			if (args.Length == 0 || args.Length > 2)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("Headerer");

			// Get the filename (or foldername)
			string file = "";
			bool deheader = true;
			if (args.Length == 1)
			{
				file = args[0];
			}
			else
			{
				
				if (args[0] == "-e")
				{
					deheader = true;
				}
				else if (args[0] == "-r")
				{
					deheader = false;
				}

				file = args[1];
			}

			if (deheader)
			{
				// If it's a single file, just check it
				if (File.Exists(file))
				{
					DetectRemoveHeader(file);
				}
				// If it's a directory, recursively check all
				else if (Directory.Exists(file))
				{
					foreach (string sub in Directory.GetFiles(file))
					{
						if (sub != ".." && sub != ".")
						{
							DetectRemoveHeader(sub);
						}
					}
				}
				// Else, show that help text
				else
				{
					Build.Help();
				}
			}
			else
			{
				// If it's a single file, just check it
				if (File.Exists(file))
				{
					ReplaceHeader(file);
				}
				// If it's a directory, recursively check all
				else if (Directory.Exists(file))
				{
					foreach (string sub in Directory.GetFiles(file))
					{
						if (sub != ".." && sub != ".")
						{
							ReplaceHeader(sub);
						}
					}
				}
				// Else, show that help text
				else
				{
					Build.Help();
				}
			}
			logger.Close();
		}

		/// <summary>
		/// Detect and remove header from the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		private static void DetectRemoveHeader(string file)
		{
			// Open the file in read mode
			BinaryReader br = new BinaryReader(File.OpenRead(file));

			// Extract the first 1024 bytes of the file
			byte[] hbin = br.ReadBytes(1024);
			string header = BitConverter.ToString(hbin).Replace("-", string.Empty);

			// Determine the type of the file from the header, if possible
			HeaderType type = HeaderType.None;
			int headerSize = 0;

			// Loop over the header types and see if there's a match
			foreach (HeaderType test in Enum.GetValues(typeof(HeaderType)))
			{
				Dictionary<string, int> tempDict = new Dictionary<string, int>();
				switch (test)
				{
					case HeaderType.A7800:
						tempDict = Remapping.A7800;
						break;
					case HeaderType.FDS:
						tempDict = Remapping.FDS;
						break;
					case HeaderType.Lynx:
						tempDict = Remapping.Lynx;
						break;
					case HeaderType.PCE:
						tempDict = Remapping.PCE;
						break;
					/*
					case HeaderType.N64:
						tempDict = Remapping.N64;
						break;
					*/
					case HeaderType.NES:
						tempDict = Remapping.NES;
						break;
					case HeaderType.PSID:
						tempDict = Remapping.PSID;
						break;
					case HeaderType.SNES:
						tempDict = Remapping.SNES;
						break;
					case HeaderType.SPC:
						tempDict = Remapping.SPC;
						break;
				}

				// Loop over the dictionary and see if there are matches
				foreach (KeyValuePair<string, int> entry in tempDict)
				{
					if (Regex.IsMatch(header, entry.Key))
					{
						type = test;
						headerSize = entry.Value;
						break;
					}
				}

				// If we found something, break out
				if (type != HeaderType.None)
				{
					break;
				}
			}

			logger.Log("File has header: " + (type != HeaderType.None));

			if (type != HeaderType.None)
			{
				logger.Log("Deteched header type: " + type);
				int hs = headerSize;

				// Save header as string in the database
				string realhead = "";
				for (int i = 0; i < hs; i++)
				{
					realhead += BitConverter.ToString(new byte[] { hbin[i] });
				}

				// Get the bytes that aren't from the header from the extracted bit so they can be written before the rest of the file
				hbin = hbin.Skip(hs).ToArray();

				// Write out the new file
				logger.Log("Creating unheadered file: " + file + ".new");
				BinaryWriter bw = new BinaryWriter(File.OpenWrite(file + ".new"));
				FileInfo fi = new FileInfo(file);
				bw.Write(hbin);
				bw.Write(br.ReadBytes((int)fi.Length - hs));
				bw.Close();
				logger.Log("Unheadered file created!");

				// Now add the information to the database if it's not already there
				SHA1 sha1 = SHA1.Create();
				sha1.ComputeHash(File.ReadAllBytes(file + ".new"));
				bool exists = false;

				string query = @"SELECT * FROM data WHERE sha1='" + BitConverter.ToString(sha1.Hash) + "' AND header='" + realhead + "'";
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
					BitConverter.ToString(sha1.Hash) + "', " +
					"'" + realhead + "', " +
					"'" + type.ToString() + "')";
					using (SqliteConnection dbc = new SqliteConnection(_connectionString))
					{
						dbc.Open();
						using (SqliteCommand slc = new SqliteCommand(query, dbc))
						{
							logger.Log("Result of inserting header: " + slc.ExecuteNonQuery());
						}
					}
				}
			}
			br.Close();
		}

		/// <summary>
		/// Detect and replace header(s) to the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		private static void ReplaceHeader(string file)
		{
			// First, get the SHA-1 hash of the file
			SHA1 sha1 = SHA1.Create();
			sha1.ComputeHash(File.ReadAllBytes(file));
			string hash = BitConverter.ToString(sha1.Hash);

			// Then try to pull the corresponding headers from the database
			string header = "";

			string query = @"SELECT header, type FROM data WHERE sha1='" + hash + "'";
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
								logger.Log("Found match with rom type " + sldr.GetString(1));
								header = sldr.GetString(0);

								logger.Log("Creating reheadered file: " + file + ".new" + sub);
								BinaryWriter bw = new BinaryWriter(File.OpenWrite(file + ".new" + sub));

								// Source: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
								for (int i = 0; i < header.Length; i += 2)
								{
									bw.Write(Convert.ToByte(header.Substring(i, 2), 16));
								}
								bw.Write(File.ReadAllBytes(file));
								bw.Close();
								logger.Log("Reheadered file created!");
							}
						}
						else
						{
							logger.Warning("No matching header could be found!");
							return;
						}
					}
				}
			}
		}
	}
}
