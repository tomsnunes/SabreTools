using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the Deheader application
	/// </summary>
	class Headerer
	{
		private static string _version = "0.2.6.1";
		private static string _dbName = "Headerer.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";
		private static Dictionary<string, int> types;
		private static string help = @"Deheader - Remove headers from roms
-----------------------------------------
Usage: Deheader [option] [filename|dirname]

Options:
  -e			Detect and remove mode
  -r			Restore header to file based on SHA-1";

		/// <summary>
		/// Start deheader operation with supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		static void Main(string[] args)
		{
			// Type mapped to header size (in decimal bytes)
			types = new Dictionary<string, int>();
			types.Add("a7800", 128);
			types.Add("fds", 16);
			types.Add("lynx", 64);
			types.Add("nes", 16);
			types.Add("snes", 512);

			// Ensure that the header database is set up
			EnsureDatabase(_dbName, _connectionString);

			if (args.Length == 0 || args.Length > 2)
			{
				Console.WriteLine(help);
				return;
			}

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
					Console.WriteLine(help);
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
					Console.WriteLine(help);
				}
			}
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
			string type = "";
			if (Regex.IsMatch(header, "^.{2}415441524937383030") || Regex.IsMatch(header, "^.{200}41435455414C20434152542044415441205354415254532048455245"))
			{
				type = "a7800";
			}
			else if (Regex.IsMatch(header, "^4644531A0[1-4]0000000000000000000000"))
			{
				type = "fds";
			}
			else if (Regex.IsMatch(header, "^4C594E58") || Regex.IsMatch(header, "^425339"))
			{
				type = "lynx";
			}
			else if (Regex.IsMatch(header, "^4E45531A"))
			{
				type = "nes";
			}
			else if (Regex.IsMatch(header, "^.{16}0000000000000000") || Regex.IsMatch(header, "^.{16}AABB040000000000") || Regex.IsMatch(header, "^.{16}535550455255464F")) // fig, smc, ufo
			{
				type = "snes";
			}

			Console.WriteLine("File has header: " + (type != ""));

			if (type != "")
			{
				Console.WriteLine("Deteched header type: " + type);
				int hs = types[type];

				// Save header as string in the database
				string realhead = "";
				for (int i = 0; i < hs; i++)
				{
					realhead += BitConverter.ToString(new byte[] { hbin[i] });
				}

				// Get the bytes that aren't from the header from the extracted bit so they can be written before the rest of the file
				hbin = hbin.Skip(hs).ToArray();

				// Write out the new file
				Console.WriteLine("Creating unheadered file: " + file + ".new");
				BinaryWriter bw = new BinaryWriter(File.OpenWrite(file + ".new"));
				FileInfo fi = new FileInfo(file);
				bw.Write(hbin);
				bw.Write(br.ReadBytes((int)fi.Length - hs));
				bw.Close();
				Console.WriteLine("Unheadered file created!");

				// Now add the information to the database if it's not already there
				SHA1 sha1 = SHA1.Create();
				sha1.ComputeHash(File.ReadAllBytes(file + ".new"));
				bool exists = false;

				string query = @"SELECT * FROM data WHERE sha1='" + BitConverter.ToString(sha1.Hash) + "' AND header='" + realhead + "'";
				using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
				{
					dbc.Open();
					using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
					{
						using (SQLiteDataReader sldr = slc.ExecuteReader())
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
					"'" + type + "')";
					using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
					{
						dbc.Open();
						using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
						{
							Console.WriteLine("Result of inserting header: " + slc.ExecuteNonQuery());
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
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						if (sldr.HasRows)
						{
							int sub = 0;
							while (sldr.Read())
							{
								Console.WriteLine("Found match with rom type " + sldr.GetString(1));
								header = sldr.GetString(0);

								Console.WriteLine("Creating reheadered file: " + file + ".new" + sub);
								BinaryWriter bw = new BinaryWriter(File.OpenWrite(file + ".new" + sub));

								// Source: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
								for (int i = 0; i < header.Length; i += 2)
								{
									bw.Write(Convert.ToByte(header.Substring(i, 2), 16));
								}
								bw.Write(File.ReadAllBytes(file));
								bw.Close();
								Console.WriteLine("Reheadered file created!");
							}
						}
						else
						{
							Console.WriteLine("No matching header could be found!");
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// Ensure that the databse exists and has the proper schema
		/// </summary>
		/// <param name="db">Name of the databse</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		public static void EnsureDatabase(string db, string connectionString)
		{
			// Make sure the file exists
			if (!File.Exists(db))
			{
				SQLiteConnection.CreateFile(db);
			}

			// Connect to the file
			SQLiteConnection dbc = new SQLiteConnection(connectionString);
			dbc.Open();
			try
			{
				// Make sure the database has the correct schema
				string query = @"
CREATE TABLE IF NOT EXISTS data (
	'sha1'		TEXT		NOT NULL,
	'header'	TEXT		NOT NULL,
	'type'		TEXT		NOT NULL,
	PRIMARY KEY (sha1, header, type)
)";
				SQLiteCommand slc = new SQLiteCommand(query, dbc);
				slc.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				// Close and return the database connection
				dbc.Close();
			}
		}
	}
}
