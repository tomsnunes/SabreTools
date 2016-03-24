using System;
using System.Data.SQLite;
using System.IO;
using System.Xml.Linq;

using DATabase.Helper;

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
			if (args.Length == 0)
			{
				Help();
				return;
			}

			// Determine which switches are enabled (with values if necessary)
			bool help = false, import = false, generate = false, convert = false,
				listsys = false, listsrc = false, norename = false, old = false; ;
			string systems = "", sources = "", input = "";
			foreach (string arg in args)
			{
				help = help || (arg == "-h" || arg == "-?" || arg == "--help");
				import = import || (arg == "-i" || arg == "--import");
				generate = generate || (arg == "-g" || arg == "--generate");
				convert = convert || (arg == "-c" || arg == "--convert");
				listsys = listsys || (arg == "-lso");
				listsrc = listsrc || (arg == "-lsy");
				norename = norename || (arg == "-nr" || arg == "--no-rename");
				old = old || (arg == "-old" || arg == "--old");
				systems = (arg.StartsWith("system=") && systems == "" ? arg.Split('=')[1] : systems);
				sources = (arg.StartsWith("source=") && sources == "" ? arg.Split('=')[1] : sources);

				// Take care of the two distinct input name possibilites; prioritize the input tag
				input = (arg.StartsWith("input=") && input == "" ? arg.Split('=')[1] : input);
				input = (!arg.StartsWith("-") && !arg.StartsWith("source=") && !arg.StartsWith("system=") && !arg.StartsWith("input=") && input == "" ? arg : input);
			}

			Console.WriteLine(input);

			// If more than one switch is enabled, show the help screen
			if (!(help ^ import ^ generate ^ listsys ^ listsrc) || help)
			{
				Help();
				return;
			}

			// Now take care of each mode in succesion

			// Import a file or folder
			if (import)
			{
				// Check to see if the second argument is a file that exists
				if (input != "" && File.Exists(input))
				{
					Console.WriteLine(input);
					Import imp = new Import(input, _connectionString);
					imp.ImportData();
				}
				// Check to see if the second argument is a directory that exists
				else if (input != "" && Directory.Exists(input))
				{
					foreach (string filename in Directory.GetFiles(input, "*", SearchOption.TopDirectoryOnly))
					{
						Console.WriteLine(filename);
						Import imp = new Import(filename, _connectionString);
						imp.ImportData();
					}
				}
				// Otherwise, show help
				else
				{
					Help();
					return;
				}
			}

			// Generate a DAT
			else if (generate)
			{
				int sysid = -1, srcid = -1;

				if (systems != "")
				{
					Int32.TryParse(systems, out sysid);
				}
				if (sources != "")
				{
					Int32.TryParse(sources, out srcid);
				}

				//Generate gen = new Generate(systems, sources, _connectionString, norename, old);
				Generate gen = new Generate(sysid, srcid, _connectionString, norename, old);
				gen.Export();
			}

			// List all available sources
			else if (listsrc)
			{
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
			}

			// List all available systems
			else if (listsys)
			{
				string query = @"
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
			}

			// Convert RV DAT to XML DAT
			else if (convert)
			{
				if (File.Exists(input))
				{
					Console.WriteLine("Converting " + input);
					XElement conv = Converters.RomVaultToXML(File.ReadAllLines(input));
					FileStream fs = File.OpenWrite(Path.GetFileNameWithoutExtension(input) + ".new.xml");
					StreamWriter sw = new StreamWriter(fs);
					sw.Write(conv);
					sw.Close();
					fs.Close();
					Console.WriteLine("Converted file: " + Path.GetFileNameWithoutExtension(input) + ".new.xml");
				}
				// Otherwise, show help
				else
				{
					Help();
					return;
				}
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
	-h, -?, --help		Show this help
	-i, --import		Start tool in import mode
	-g, --generate		Start tool in generate mode
	-lso			List all sources (id <= name)
	-lsy			List all systems (id <= name)
	-c, --convert		Convert a RV DAT to XML
	-nr, --no-rename	Do not rename games according to source/system
	-old			Use RV datfile format

If started in import or convert mode, either a filename
or directory name is required in order to run.
Filenames and directories can't start with '-', 'system=', or 'source='
unless prefixed by 'input='
");
		}
	}
}
