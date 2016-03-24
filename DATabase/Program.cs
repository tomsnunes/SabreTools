using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

using System.Xml.Linq;

namespace DATabase
{
	class Program
	{
		private static string _connectionString = "Data Source=DATabase.sqlite;Version = 3;";

		static void Main(string[] args)
		{
			// Ensure the database is set up properly
			EnsureDatabase();

			// Make sure all mappings are created and ready to be used
			Remapping.CreateRemappings();

			// If there's not enough arguments, show the help screen
			if (args.Length == 0 || args[0] == "-h" || args[0] == "-?" || args[0] == "--help")
			{
				Help();
				return;
			}

			// Determine what mode we are in from the first argument
			switch (args[0])
			{
				// Import a file or folder
				case "-i":
				case "--import":
					// Check if there are enough arguments
					if (args.Length > 1)
					{
						// Check to see if the second argument is a file that exists
						if (args.Length > 1 && File.Exists(args[1]))
						{
							Console.WriteLine(args[1]);
							Import imp = new Import(args[1], _connectionString);
							imp.ImportData();
						}
						// Check to see if the second argument is a directory that exists
						else if (args.Length > 1 && Directory.Exists(args[1]))
						{
							foreach (string filename in Directory.GetFiles(args[1], "*", SearchOption.TopDirectoryOnly))
							{
								Console.WriteLine(filename);
								Import imp = new Import(filename, _connectionString);
								imp.ImportData();
							}
						}
						// If it's invalid for either, show the help
						else
						{
							Help();
						}
					}
					// If there aren't enough arguments
					else
					{
						Help();
					}
					break;
				// Generate a DAT
				case "-g":
				case "--generate":
					Generate gen;
					if (args.Length > 1)
					{
						int system = -1, source = -1;

						for (int i = 1; i < args.Length; i++)
						{
							if (args[i].StartsWith("system=") && system == -1)
							{
								Int32.TryParse(args[i].Split('=')[1], out system);
							}
							else if (args[i].StartsWith("source=") && source == -1)
							{
								Int32.TryParse(args[i].Split('=')[1], out source);
							}
							else
							{
								Help();
								return;
							}
						}

						gen = new Generate(system, source, _connectionString);
					}
					else
					{
						gen = new Generate(-1, -1, _connectionString);
					}
					gen.Export();
					break;

				// List all available sources
				case "-lso":
					string query = @"
SELECT DISTINCT sources.id, sources.name
FROM sources JOIN games on sources.id=games.source
ORDER BY sources.name COLLATE NOCASE";
					using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
					{
						dbc.Open();
						using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
						{
							using (SQLiteDataReader sldr = slc.ExecuteReader())
							{
								// If nothing is found, tell the user and exit
								if (!sldr.HasRows)
								{
									Console.WriteLine("Error: No sources found! Please add a source and then try again.");
									return;
								}

								Console.WriteLine("Available Sources (id <= name):");
								while (sldr.Read())
								{
									Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1));
								}
							}
						}
					}

					break;

				// List all available systems
				case "-lsy":
					query = @"
SELECT DISTINCT systems.id, systems.manufacturer, systems.system
FROM systems JOIN games ON systems.id=games.system
ORDER BY systems.manufacturer, systems.system";
					using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
					{
						dbc.Open();
						using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
						{
							using (SQLiteDataReader sldr = slc.ExecuteReader())
							{
								// If nothing is found, tell the user and exit
								if (!sldr.HasRows)
								{
									Console.WriteLine("Error: No systems found! Please add a system and then try again.");
									return;
								}

								Console.WriteLine("Available Systems (id <= name):");
								while (sldr.Read())
								{
									Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1) + " - " + sldr.GetString(2));
								}
							}
						}
					}

					break;

				case "-c":
				case "--convert":
					// Check if there are enough arguments
					if (args.Length > 1)
					{
						// Check to see if the second argument is a file that exists
						if (args.Length > 1 && File.Exists(args[1]))
						{
							Console.WriteLine("Converting " + args[1]);
							XElement conv = Converters.RomVaultToXML(File.ReadAllLines(args[1]));
							FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(args[1]) + ".new.xml");
							StreamWriter sw = new StreamWriter(fs);
							sw.Write(conv);
							sw.Close();
							fs.Close();
							Console.WriteLine("Converted file: " + Path.GetFileNameWithoutExtension(args[1]) + ".new.xml");
						}
						// If it's invalid, show the help
						else
						{
							Help();
						}
					}
					// If there aren't enough arguments
					else
					{
						Help();
					}
					break;

				// Invalid argument
				default:
					Help();
					break;
			}
			return;
		}

		private static void EnsureDatabase()
		{
			// Make sure the file exists
			if (!File.Exists("DATabase.sqlite"))
			{
				SQLiteConnection.CreateFile("DATabase.sqlite");
			}

			// Connect to the file
			SQLiteConnection dbc = new SQLiteConnection(_connectionString);
			dbc.Open();
            try
			{
				// Make sure the database has the correct schema
				string query = @"
CREATE TABLE IF NOT EXISTS checksums (
	'file'	INTEGER		NOT NULL,
	'size'	INTEGER		NOT NULL	DEFAULT -1,
	'crc'	TEXT		NOT NULL,
	'md5'	TEXT		NOT NULL,
	'sha1'	TEXT		NOT NULL,
	PRIMARY KEY (file, size, crc, md5, sha1)
)";
				SQLiteCommand slc = new SQLiteCommand(query, dbc);
				slc.ExecuteNonQuery();

				query = @"
CREATE TABLE IF NOT EXISTS files (
	'id'			INTEGER	PRIMARY KEY	NOT NULL,
	'setid'			INTEGER				NOT NULL,
	'name'			TEXT				NOT NULL,
	'type'			TEXT				NOT NULL	DEFAULT 'rom',
	'lastupdated'	TEXT				NOT NULL
)";
				slc = new SQLiteCommand(query, dbc);
				slc.ExecuteNonQuery();

				query = @"
CREATE TABLE IF NOT EXISTS games (
	'id'		INTEGER PRIMARY KEY	NOT NULL,
	'system'	INTEGER				NOT NULL,
	'name'		TEXT				NOT NULL,
	'parent'	INTEGER				NOT NULL	DEFAULT '0',
	'source'	INTEGER				NOT NULL	DEFAULT '0'
)";
				slc = new SQLiteCommand(query, dbc);
				slc.ExecuteNonQuery();

				query = @"
CREATE TABLE IF NOT EXISTS parent (
	'id'	INTEGER PRIMARY KEY	NOT NULL,
	'name'	TEXT				NOT NULL
)";
				slc = new SQLiteCommand(query, dbc);
				slc.ExecuteNonQuery();

				query = @"
CREATE TABLE IF NOT EXISTS sources (
	'id'	INTEGER PRIMARY KEY	NOT NULL,
	'name'	TEXT				NOT NULL	UNIQUE,
	'url'	TEXT				NOT NULL
)";
				slc = new SQLiteCommand(query, dbc);
				slc.ExecuteNonQuery();

				query = @"
CREATE TABLE IF NOT EXISTS systems (
	'id'			INTEGER PRIMARY KEY	NOT NULL,
	'manufacturer'	TEXT				NOT NULL,
	'system'		TEXT				NOT NULL
)";
				slc = new SQLiteCommand(query, dbc);
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

		private static void Help ()
		{
			Console.Write(@"
DATabase - Import and Generate DAT files
-----------------------------------------
Usage: DATabase <option> (<filename>|<dirname>) | (system=sy) (source=so)

<option> can be one of the following:
	-h, -?, --help	Show this help
	-i, --import	Start tool in import mode
	-g, --generate	Start tool in generate mode
	-lso		List all sources (id <= name)
	-lsy		List all systems (id <= name)
	-c, --convert	Convert a RV DAT to XML

If started in import or convert mode, either a filename
or directory name is required  in order to run.

If started in generate mode, here are the possible states:
	system blank,	source blank	Create MEGAMERGED
	system blank,	source		Create source-merged
	system,		source blank	Create system-merged
	system,		source		Create custom
");
		}
	}
}
