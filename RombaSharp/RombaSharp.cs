using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SabreTools.Helper;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the RombaSharp application
	/// </summary>
	public class RombaSharp
	{
		// General settings
		private int _workers;		//Number of parallel threads
		private string _logdir;		//Log folder location
		private string _tmpdir;	//Temp folder location
		private string _webdir;		// Web frontend location
		private string _baddir;		// Fail-to-unpack file folder location
		private int _verbosity;		// Verbosity of the output
		private int _cores;			// Forced CPU cores

		// DatRoot settings
		private string _dats;		// DatRoot folder location
		private string _db;			// Database name

		// Depot settings
		private Dictionary<string, Tuple<long, bool>> _depots; // Folder location, Max size

		// Server settings
		private int _port;			// Web server port

		// Other private variables
		private string _config = "config.xml";
		private string _connectionString;
		private Logger _logger;

		/// <summary>
		/// Create a new RombaSharp object
		/// </summary>
		/// <param name="logger">Logger object for file and console output</param>
		public RombaSharp(Logger logger)
		{
			_logger = logger;

			InitializeConfiguration();
			InitializeDatabase();
		}

		public static void Main(string[] args)
		{
		}

		/// <summary>
		/// Initialize the Romba application from XML config
		/// </summary>
		private void InitializeConfiguration()
		{
			// Get default values if they're not written
			int workers = 4,
				verbosity = 1,
				cores = 4,
				port = 4003;
			string logdir = "logs",
				tmpdir = "tmp",
				webdir = "web",
				baddir = "bad",
				dats = "dats",
				db = "db",
				connectionString = "";
			Dictionary<string, Tuple<long, bool>> depots = new Dictionary<string, Tuple<long, bool>>();

			// Get the XML text reader for the configuration file, if possible
			XmlTextReader xtr = DatTools.GetXmlTextReader(_config, _logger);

			/* XML file structure

			<romba>
				<general>
					<workers>4</workers>
					<logdir>logs</logdir>
					<tmpdir>tmp</tmpdir>
					<webdir>web</web>
					<baddir>bad</baddir>
					<verbosity>1</verbosity>
					<cores>4</cores>
				</general>
				<index>
					<dats>dats</dats>
					<db>db</db>
				</index>
				<depots>
					<depot>
						<root>depot</root>
						<maxsize>40000</maxsize>
						<online>true</online>
					</depot>
				</depots>
				<server>
					<port>4003</port>
				</server>
			</romba>

			*/

			// Now parse the XML file for settings
			if (xtr != null)
			{
				xtr.MoveToContent();
				while (!xtr.EOF)
				{
					// We only want elements
					if (xtr.NodeType != XmlNodeType.Element)
					{
						xtr.Read();
						continue;
					}

					switch (xtr.Name)
					{
						case "workers":
							workers = xtr.ReadElementContentAsInt();
							break;
						case "logdir":
							logdir = xtr.ReadElementContentAsString();
							break;
						case "tmpdir":
							tmpdir = xtr.ReadElementContentAsString();
							break;
						case "webdir":
							webdir = xtr.ReadElementContentAsString();
							break;
						case "baddir":
							baddir = xtr.ReadElementContentAsString();
							break;
						case "verbosity":
							verbosity = xtr.ReadElementContentAsInt();
							break;
						case "cores":
							cores = xtr.ReadElementContentAsInt();
							break;
						case "dats":
							dats = xtr.ReadElementContentAsString();
							break;
						case "db":
							db = xtr.ReadElementContentAsString();
							break;
						case "depot":
							XmlReader subreader = xtr.ReadSubtree();
							if (subreader != null)
							{
								string root = "";
								long maxsize = -1;
								bool online = true;

								while (!subreader.EOF)
								{
									// We only want elements
									if (subreader.NodeType != XmlNodeType.Element)
									{
										subreader.Read();
										continue;
									}

									switch (subreader.Name)
									{
										case "root":
											root = subreader.ReadElementContentAsString();
											break;
										case "maxsize":
											maxsize = subreader.ReadElementContentAsLong();
											break;
										case "online":
											online = subreader.ReadElementContentAsBoolean();
											break;
										default:
											subreader.Read();
											break;
									}
								}

								try
								{
									depots.Add(root, new Tuple<long, bool>(maxsize, online));
								}
								catch
								{
									// Ignore add errors
								}
							}

							xtr.Skip();
							break;
						case "port":
							port = xtr.ReadElementContentAsInt();
							break;
						default:
							xtr.Read();
							break;
					}
				}
			}

			// Now validate the values given
			if (workers < 1)
			{
				workers = 1;
			}
			if (workers > 8)
			{
				workers = 8;
			}
			if (!Directory.Exists(logdir))
			{
				Directory.CreateDirectory(logdir);
			}
			if (!Directory.Exists(tmpdir))
			{
				Directory.CreateDirectory(tmpdir);
			}
			if (!Directory.Exists(webdir))
			{
				Directory.CreateDirectory(webdir);
			}
			if (!Directory.Exists(baddir))
			{
				Directory.CreateDirectory(baddir);
			}
			if (verbosity < 0)
			{
				verbosity = 0;
			}
			if (verbosity > 3)
			{
				verbosity = 3;
			}
			if (cores < 1)
			{
				cores = 1;
			}
			if (cores > 16)
			{
				cores = 16;
			}
			if (!Directory.Exists(dats))
			{
				Directory.CreateDirectory(dats);
			}
			db = Path.GetFileNameWithoutExtension(db) + ".sqlite";
			connectionString = "Data Source=" + _db + ";Version = 3;";
			foreach (string key in depots.Keys)
			{
				if (!Directory.Exists(key))
				{
					Directory.CreateDirectory(key);
				}
			}
			if (port < 0)
			{
				port = 0;
			}
			if (port > 65535)
			{
				port = 65535;
			}

			// Finally set all of the fields
			_workers = workers;
			_logdir = logdir;
			_tmpdir = tmpdir;
			_webdir = webdir;
			_baddir = baddir;
			_verbosity = verbosity;
			_cores = cores;
			_dats = dats;
			_db = db;
			_connectionString = connectionString;
			_depots = depots;
			_port = port;
		}

		/// <summary>
		/// Initialize the Romba database
		/// </summary>
		private void InitializeDatabase()
		{
			// Make sure the db is set
			if (String.IsNullOrEmpty(_db))
			{
				_db = "db.sqlite";
				_connectionString = "Data Source=" + _db + ";Version = 3;";
			}

			// Make sure the file exists
			if (!File.Exists(_db))
			{
				SqliteConnection.CreateFile(_db);
			}

			// Connect to the file
			SqliteConnection dbc = new SqliteConnection(_connectionString);
			dbc.Open();

			// Initialize the database schema
			try
			{
				string query = @"
CREATE TABLE IF NOT EXISTS data (
'id'	INTEGER		NOT NULL
'key'	TEXT		NOT NULL
'value'	TEXT		NOT NULL
)";
				SqliteCommand slc = new SqliteCommand(query, dbc);
				slc.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				// Close the database connection
				dbc.Close();
			}
		}

		/// <summary>
		/// Populate or refresh the database information
		/// </summary>
		private void RefreshDatabase()
		{
			// Make sure the dats dir is set
			if (String.IsNullOrEmpty(_dats))
			{
				_dats = "dats";
			}

			// Make sure the folder exists
			if (!Directory.Exists(_dats))
			{
				Directory.CreateDirectory(_dats);
			}

			// Now parse the directory into an internal Dat then insert
			foreach (string file in Directory.EnumerateFiles(_dats, "*", SearchOption.AllDirectories))
			{
				Dat datdata = new Dat();
				datdata = DatTools.Parse(file, 0, 0, datdata, _logger);
				Rom romdata = FileTools.GetSingleFileInfo(file);
			}
		}

		/// <summary>
		/// Process a datfile and insert it into the database
		/// </summary>
		/// <param name="datdata">Dat object representing the data to insert</param>
		/// <param name="romdata">Rom object representing the Dat file itself</param>
		private void InsertDatIntoDatabase(Dat datdata, Rom romdata)
		{
			// Make sure the db is set
			if (String.IsNullOrEmpty(_db))
			{
				_db = "db.sqlite";
				_connectionString = "Data Source=" + _db + ";Version = 3;";
			}

			// Make sure the file exists
			if (!File.Exists(_db))
			{
				SqliteConnection.CreateFile(_db);
				InitializeDatabase();
			}

			// Open a connection to the database
			using (SqliteConnection slc = new SqliteConnection(_connectionString))
			{
				// For each key
				foreach (string key in datdata.Files.Keys)
				{
					// For each Rom in the list
					foreach (Rom file in datdata.Files[key])
					{
						// Try to find the hash in the set

						// If it exists, see if there's any missing information

						// If it doesn't exist, insert it completely
					}
				}
			}
		}
	}
}
