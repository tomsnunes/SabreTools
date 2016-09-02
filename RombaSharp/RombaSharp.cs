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
		private string _dbSchema = "rombasharp";
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
			DBTools.EnsureDatabase(_dbSchema, _db, _connectionString);
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
		/// Populate or refresh the database information
		/// </summary>
		/// <remarks>
		/// Even though I already wrote a bunch of code here, I think this needs improvement
		/// The process for figuring out what needs updating should be easy:
		///		- Get a list of DAT hashes from the database
		///		- Get a list of DAT hashes from the folder
		///		- Figure out what DATs are new and which are no longer around
		///		- Remove references in the keyvault to the no-longer-arounds
		///		- Add only new DATs
		/// </remarks>
		private void RefreshDatabase()
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
				DBTools.EnsureDatabase(_dbSchema, _db, _connectionString);
			}

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

			// Now create a new structure to replace the database
			Dictionary<Hash, List<string>> keyvault = new Dictionary<Hash, List<string>>();

			// Now parse the directory into an internal structure
			int i = 0; // Dat number
			foreach (string file in Directory.EnumerateFiles(_dats, "*", SearchOption.AllDirectories))
			{
				Dat datdata = new Dat();
				datdata = DatTools.Parse(file, i, i, datdata, _logger);
				Rom romdata = FileTools.GetSingleFileInfo(file);

				// Loop through the entire DAT and add to the structure
				foreach (List<Rom> roms in datdata.Files.Values)
				{
					List<Rom> newroms = RomTools.Merge(roms, _logger);
					foreach (Rom rom in roms)
					{
						if (keyvault.ContainsKey(rom.HashData))
						{
							keyvault[rom.HashData].Add(romdata.HashData.SHA1);
						}
						else
						{
							List<string> temp = new List<string>();
							temp.Add(romdata.HashData.SHA1);
							keyvault.Add(rom.HashData, temp);
						}
					}
				}

				// Increment the DAT number
				i++;
			}

			// Now that we have the structure, we can create a new database
		}
	}
}
