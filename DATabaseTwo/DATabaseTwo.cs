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
	public class DATabaseTwo
	{
		// Private required variables
		private static string _datroot = "DATS";
		private static string _outroot = "Output";
		private static string _dbName = "dats.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";
		private static Logger _logger;

		// Regex File Name Patterns
		private static string _defaultPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.dat$";
		private static string _defaultSpecialPattern = @"^(.+?) - (.+?) \((.*) (.*)\)\.xml$";
		private static string _goodPattern = @"^(Good.*?)_.*\.dat";
		private static string _goodXmlPattern = @"^(Good.*?)_.*\.xml";
		private static string _mamePattern = @"^(.*)\.xml$";
		private static string _maybeIntroPattern = @"(.*?) \[T-En\].*\((\d{8})\)\.dat$";
		private static string _noIntroPattern = @"^(.*?) \((\d{8}-\d{6})_CM\)\.dat$";
		private static string _noIntroNumberedPattern = @"(.*? - .*?) \(\d.*?_CM\).dat";
		private static string _noIntroSpecialPattern = @"(.*? - .*?) \((\d{8})\)\.dat";
		private static string _nonGoodPattern = @"^(NonGood.*?)( .*?)?.xml";
		private static string _nonGoodSpecialPattern = @"^(NonGood.*?)( .*)?.dat";
		private static string _redumpPattern = @"^(.*?) \((\d{8} \d{2}-\d{2}-\d{2})\)\.dat$";
		private static string _redumpBiosPattern = @"^(.*?) \(\d+\) \((\d{4}-\d{2}-\d{2})\)\.dat$";
		private static string _tosecPattern = @"^(.*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\)\.dat$";
		private static string _tosecSpecialPatternA = @"^(.*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\)\.dat$";
		private static string _tosecSpecialPatternB = @"^(.*? - .*? - .*?) - .* \(TOSEC-v(\d{4}-\d{2}-\d{2})_CM\)\.dat$";
		private static string _truripPattern = @"^(.*) - .* \(trurip_XML\)\.dat$";
		private static string _zandroPattern = @"^SMW-.*.xml";

		public static void Main(string[] args)
		{
			Console.Clear();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			_logger = new Logger(true, "database2.log");
			_logger.Start();

			Remapping.CreateRemappings();
			Build.Start("DATabaseTwo");

			// Perform initial database and folder setup
			if (!Directory.Exists(_datroot))
			{
				Directory.CreateDirectory(_datroot);
			}
			if (!Directory.Exists(_outroot))
			{
				Directory.CreateDirectory(_outroot);
			}
			DBTools.EnsureDatabase(_dbName, _connectionString);

			// If there's no arguments, show the menu
			if (args.Length == 0)
			{
				_logger.ToFile = true;
				ShowMainMenu();
				_logger.Close();
				return;
			}

			// Set all default values
			bool help = false,
				gen = false,
				genall = false,
				ignore = false,
				listsys = false,
				norename = false,
				old = false;
			string system = "";

			// Determine which switches are enabled (with values if necessary)
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-g":
					case "--generate":
						gen = true;
						break;
					case "-ga":
					case "--generate-all":
						genall = true;
						break;
					case "-i":
					case "--ignore":
						ignore = true;
						break;
					case "-lsy":
					case "--list-systems":
						listsys = true;
						break;
					case "-nr":
					case "--no-rename":
						norename = true;
						break;
					case "-o":
					case "--old":
						old = true;
						break;
					default:
						if (arg.StartsWith("-sys=") || arg.StartsWith("--system="))
						{
							system = arg.Split('=')[1];
						}
						else
						{
							_logger.Warning("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							_logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set or system is blank, show the help screen
			if (help || (system == "" && !listsys))
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// If we want a list of systems
			if (listsys)
			{
				ListSystems();
			}

			// If we want to generate all DATs
			else if (genall)
			{
				Import(ignore);
				InitGenerateAll(norename, old);
			}

			// If we want to generate a DAT
			else if (gen)
			{
				Import(ignore);
				InitGenerate(system, norename, old);
			}


			_logger.Close();
		}

		#region Menus

		/// <summary>
		/// Show the text-based main menu
		/// </summary>
		private static void ShowMainMenu()
		{
			string selection = "";
			while (selection.ToLowerInvariant() != "x")
			{
				Console.Clear();
				Build.Start("DATabaseTwo");
				Console.WriteLine(@"MAIN MENU
===========================
Make a selection:

    1) Show command line usage
    2) Check for new or changed DATs
    3) Generate System DATs
    4) List all available systems
    5) " + (_logger.ToFile ? "Disable Logging" : "Enable Logging") + @"
    6) Show credits
    X) Exit Program
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();

				switch (selection)
				{
					case "1":
						Console.Clear();
						Build.Help();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "2":
						ImportMenu();
						break;
					case "3":
						GenerateMenu();
						break;
					case "4":
						Console.Clear();
						Build.Start("DATabaseTwo");
						ListSystems();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
					case "5":
						_logger.ToFile = !_logger.ToFile;
						break;
					case "6":
						Console.Clear();
						Build.Credits();
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						break;
				}
			}
		}

		/// <summary>
		/// Show the text-based import menu
		/// </summary>
		private static void ImportMenu()
		{
			string selection = "";
			bool ignore = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabaseTwo");
				Console.WriteLine(@"IMPORT MENU
===========================
Make a selection:

    1) " + (ignore ? "Enable new source prompt" : "Disable new source prompt") + @"
    2) Begin import process
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						ignore = !ignore;
						break;
					case "2":
						Console.Clear();
						Import(ignore);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						ignore = false;
						break;
				}
			}
			return;
		}

		/// <summary>
		/// Show the text-based generate menu
		/// </summary>
		private static void GenerateMenu()
		{
			string selection = "", system = "";
			bool norename = false, old = false;
			while (selection.ToLowerInvariant() != "b")
			{
				Console.Clear();
				Build.Start("DATabaseTwo");
				Console.WriteLine(@"GENERATE MENU
===========================
Make a selection:

    1) " + (norename ? "Enable game renaming" : "Disable game renaming") + @"
    2) " + (old ? "Enable XML output" : "Enable ClrMamePro output") + @"
    3) System ID to generate from" + (system != "" ? ": " + system : "") + @"
    4) Generate the DAT file for the specified system
    5) Generate all DAT files
    B) Go back to the previous menu
");
				Console.Write("Enter selection: ");
				selection = Console.ReadLine();
				switch (selection)
				{
					case "1":
						norename = !norename;
						break;
					case "2":
						old = !old;
						break;
					case "3":
						Console.Clear();
						ListSystems();
						Console.Write("Please enter the System ID: ");
						system = Console.ReadLine();
						break;
					case "4":
						Console.Clear();
						InitGenerate(system, norename, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						system = "";
						norename = false; old = false;
						break;
					case "5":
						Console.Clear();
						InitGenerateAll(norename, old);
						Console.Write("\nPress any key to continue...");
						Console.ReadKey();
						system = "";
						norename = false; old = false;
						break;
				}
			}
			return;
		}

		#endregion

		#region Function Methods

		/// <summary>
		/// Wrap generating a DAT from the library
		/// </summary>
		/// <param name="system">System ID to be used in the DAT (blank means all)</param>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in ClrMamePro format (default false)</param>
		private static void InitGenerate(string systemid, bool norename, bool old)
		{
			string name = "";
			string path = _datroot;

			// If the System ID isn't set, then we will merge everything
			if (systemid != "")
			{
				string system = "";

				// First get the name of the system, if possible
				string query = "SELECT manufacturer, name FROM system WHERE id=" + systemid;
				using (SqliteConnection dbc = new SqliteConnection(_connectionString))
				{
					dbc.Open();

					using (SqliteCommand slc = new SqliteCommand(query, dbc))
					{
						using (SqliteDataReader sldr = slc.ExecuteReader())
						{
							if (sldr.Read())
							{
								system = sldr.GetString(0) + " - " + sldr.GetString(1);
							}
						}
					}
				}

				// If we didn't find anything, then return
				if (system == "")
				{
					_logger.Warning("No system could be found with id " + systemid);
					return;
				}

				path += Path.DirectorySeparatorChar + system.Trim();
				name = system.Trim();
			}
			else
			{
				name = "ALL";
			}

			// Get the rest of the info as well
			string date = DateTime.Now.ToString("yyyyMMddHHmmss");
			string description = name + " (merged " + date + ")";
			name += " (merged)";

			// For good measure, get all sources
			Dictionary<int, string> sources = new Dictionary<int, string>();
			sources.Add(0, "Default");

			string squery = "SELECT id, name FROM source";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				using (SqliteCommand slc = new SqliteCommand(squery, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							sources.Add(sldr.GetInt32(0), sldr.GetString(1));
						}
					}
				}
			}

			// Get a list of files to sourceid mappings
			Dictionary<string, string> sourcemap = new Dictionary<string, string>();
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				string tquery = "SELECT DISTINCT dats.name, datsdata.value FROM dats JOIN datsdata ON dats.id=datsdata.id WHERE key='source'";
				using (SqliteCommand slc = new SqliteCommand(tquery, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							string tempname = sldr.GetString(0);
							string tempval = sldr.GetString(1);
							if (!sourcemap.ContainsKey(tempname))
							{
								sourcemap.Add(tempname, tempval);
							}
						}
					}
				}
			}

			// Now read in all of the files
			Dictionary<string, List<RomData>> roms = new Dictionary<string, List<RomData>>();
			foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
			{
				int tempSrcId = 0;
				if (sourcemap.ContainsKey(file))
				{
					Int32.TryParse(sourcemap[file], out tempSrcId);
				}
				roms = RomManipulation.ParseDict(file, 0, tempSrcId, roms, _logger);
			}

			// If the dictionary is empty for any reason, tell the user and exit
			if (roms.Keys.Count == 0 || roms.Count == 0)
			{
				_logger.Log("No roms found for system ID " + systemid);
				return;
			}

			// Now process all of the roms
			_logger.Log("Cleaning rom data");
			List<string> keys = roms.Keys.ToList();
			foreach (string key in keys)
			{
				List<RomData> temp = new List<RomData>();
				List<RomData> newroms = roms[key];
				for (int i = 0; i < newroms.Count; i++)
				{
					RomData rom = newroms[i];

					// In the case that the RomData is incomplete, skip it
					if (rom.Name == null || rom.Game == null)
					{
						continue;
					}

					// WOD origninally stripped out any subdirs from the imported files, we do the same
					rom.Name = Path.GetFileName(rom.Name);

					// Run the name through the filters to make sure that it's correct
					rom.Name = Style.NormalizeChars(rom.Name);
					rom.Name = Style.RussianToLatin(rom.Name);
					rom.Name = Regex.Replace(rom.Name, @"(.*) \.(.*)", "$1.$2");

					// WoD gets rid of anything past the first "(" or "[" as the name, we will do the same
					string stripPattern = @"(([[(].*[\)\]] )?([^([]+))";
					Regex stripRegex = new Regex(stripPattern);
					Match stripMatch = stripRegex.Match(rom.Game);
					rom.Game = stripMatch.Groups[1].Value;

					// Run the name through the filters to make sure that it's correct
					rom.Game = Style.NormalizeChars(rom.Game);
					rom.Game = Style.RussianToLatin(rom.Game);
					rom.Game = Style.SearchPattern(rom.Game);
					rom.Game = rom.Game.TrimEnd().TrimStart();

					if (!norename)
					{
						rom.Game += " [" + sources[rom.SourceID] + "]";
					}
					temp.Add(rom);
				}
				roms[key] = temp;
			}

			// Then write out the file
			Output.WriteToDatFromDict(name, description, "", date, "SabreTools", "SabreTools", false, old, true, _outroot, roms, _logger, norename);
		}

		/// <summary>
		/// Wrap generating all standard DATs from the library
		/// </summary>
		private static void InitGenerateAll(bool norename, bool old)
		{
			List<string> systems = new List<string>();
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				string query = "SELECT id FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							_logger.Warning("No systems found! Please add a system and then try again.");
							return;
						}

						while (sldr.Read())
						{
							systems.Add(sldr.GetInt32(0).ToString());
						}
					}
				}

				// Loop through the inputs
				foreach (string system in systems)
				{
					InitGenerate(system, norename, old);
				}
			}
		}

		/// <summary>
		/// List systems in the database
		/// </summary>
		private static void ListSystems()
		{
			string query = @"
SELECT DISTINCT system.id, system.manufacturer, system.name
FROM system
ORDER BY system.manufacturer, system.name";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							_logger.Warning("No systems found! Please add a system and then try again.");
							return;
						}

						Console.WriteLine("Available Systems (id <= name):\n");
						while (sldr.Read())
						{
							Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1) + " - " + sldr.GetString(2));
						}
					}
				}
			}
			return;
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Perform initial or incremental import of DATs in the root folder
		/// </summary>
		/// <param name="ignore">False if each DAT that has no defined source asks for user input (default), true otherwise</param>
		private static void Import(bool ignore = false)
		{
			_logger.Log("Beginning import/update process");
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				Dictionary<string, int> hashes = new Dictionary<string, int>();
				string query = "SELECT sha1, id FROM dats";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							hashes.Add(sldr.GetString(0), sldr.GetInt32(1));
						}
					}
				}

				SHA1 sha1 = SHA1.Create();
				query = "SELECT * FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							int systemid = sldr.GetInt32(0);
							string system = _datroot + Path.DirectorySeparatorChar + sldr.GetString(1) + " - " + sldr.GetString(2);
							system = system.Trim();

							_logger.Log("System: " + system.Remove(0, 5));

							if (!Directory.Exists(system))
							{
								Directory.CreateDirectory(system);
							}

							// Audit all DATs in the folder
							foreach (string file in Directory.GetFiles(system, "*", SearchOption.AllDirectories))
							{
								string hash = "";
								using (FileStream fs = File.Open(file, FileMode.Open))
								{
									hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
								}

								// If the hash isn't in add it and all required information
								int hashid = -1;
								if (!hashes.ContainsKey(hash))
								{
									_logger.Log("Adding file information for " + Path.GetFileName(file));

									string squery = @"BEGIN;
INSERT INTO dats (size, sha1, name)
VALUES (" + (new FileInfo(file)).Length + ", '" + hash + "', '" + file.Replace("'", "''") + @"');
SELECT last_insert_rowid();
COMMIT;";
									using (SqliteCommand sslc = new SqliteCommand(squery, dbc))
									{
										using (SqliteDataReader ssldr = sslc.ExecuteReader())
										{
											if (ssldr.Read())
											{
												hashid = ssldr.GetInt32(0);
											}
										}
									}

									// Add the hash to the temporary Dictionary
									hashes.Add(hash, hashid);

									// If we don't care about source, stop here
									if (ignore)
									{
										continue;
									}

									// Now try to determine the source for the file based on the name
									string source = GetSourceFromFileName(Path.GetFileName(file));
									int sourceid = 0;

									Dictionary<string, int> sources = new Dictionary<string, int>();
									query = "SELECT name, id FROM source";
									using (SqliteCommand sslc = new SqliteCommand(query, dbc))
									{
										using (SqliteDataReader ssldr = sslc.ExecuteReader())
										{
											while (ssldr.Read())
											{
												sources.Add(ssldr.GetString(0), ssldr.GetInt32(1));
											}
										}
									}

									// If the source is blank, ask the user to supply one
									while (source == "" && sourceid == 0)
									{
										Console.Clear();
										Build.Start("DATabaseTwo");

										Console.WriteLine("Sources:");
										foreach (KeyValuePair<string, int> pair in sources)
										{
											Console.WriteLine("    " + pair.Value + " - " + pair.Key);
										}
										Console.WriteLine("\nFor file name: " + Path.GetFileName(file));
										Console.Write("Select a source above or enter a new one: ");
										source = Console.ReadLine();

										Int32.TryParse(source, out sourceid);

										// If the value could be parsed, reset the source string
										if (sourceid != 0)
										{
											source = "";
										}

										// If the source ID is set check to see if it's valid
										if (sourceid != 0 && !sources.ContainsValue(sourceid))
										{
											Console.WriteLine("Invalid selection: " + sourceid);
											Console.ReadLine();
											sourceid = 0;
										}
									}

									// If the source isn't in, add it and get the insert id
									if (source != "" && sourceid == 0 && !sources.ContainsKey(source))
									{
										string tquery = @"BEGIN;
INSERT INTO source (name, url)
VALUES ('" + source + @"', '');
SELECT last_insert_rowid();
COMMIT;";
										using (SqliteCommand sslc = new SqliteCommand(tquery, dbc))
										{
											using (SqliteDataReader ssldr = sslc.ExecuteReader())
											{
												if (ssldr.Read())
												{
													sourceid = ssldr.GetInt32(0);
												}
											}
										}

										// Add the new source to the temporary Dictionary
										sources.Add(source, sourceid);
									}
									// Otherwise, get the ID
									else if (source != "" && sourceid == 0 && sources.ContainsKey(source))
									{
										sourceid = sources[source];
									}
									// Otherwise, we should already have an ID

									// Add the source and system link to the database
									string uquery = @"INSERT OR IGNORE INTO datsdata (id, key, value)
VALUES (" + hashid + ", 'source', '" + sourceid + @"'),
(" + hashid + ", 'system', '" + systemid + "')";
									using (SqliteCommand uslc = new SqliteCommand(uquery, dbc))
									{
										uslc.ExecuteNonQuery();
									}
								}
							}
						}
					}
				}
			}

			_logger.Log("Import/update process complete!");
		}

		/// <summary>
		/// Determine the source name from the file name, if possible
		/// </summary>
		/// <param name="filename">The name of the file to be checked</param>
		/// <returns>The name of the source if determined, blank otherwise</returns>
		private static string GetSourceFromFileName (string filename)
		{
			string source = "";

			// Determine which dattype we have
			GroupCollection fileinfo;

			if (Regex.IsMatch(filename, _nonGoodPattern))
			{
				fileinfo = Regex.Match(filename, _nonGoodPattern).Groups;
				if (!Remapping.NonGood.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as NonGood but could not be mapped.");
					return source;
				}
				source = "NonGood";
			}
			else if (Regex.IsMatch(filename, _nonGoodSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _nonGoodSpecialPattern).Groups;
				if (!Remapping.NonGood.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as NonGood but could not be mapped.");
					return source;
				}
				source = "NonGood";
			}
			else if (Regex.IsMatch(filename, _goodPattern))
			{
				fileinfo = Regex.Match(filename, _goodPattern).Groups;
				if (!Remapping.Good.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Good but could not be mapped.");
					return source;
				}
				source = "Good";
			}
			else if (Regex.IsMatch(filename, _goodXmlPattern))
			{
				fileinfo = Regex.Match(filename, _goodXmlPattern).Groups;
			}
			else if (Regex.IsMatch(filename, _maybeIntroPattern))
			{
				fileinfo = Regex.Match(filename, _maybeIntroPattern).Groups;
				if (!Remapping.MaybeIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Maybe-Intro but could not be mapped.");
					return source;
				}
				source = "Maybe-Intro";
			}
			else if (Regex.IsMatch(filename, _noIntroPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroPattern).Groups;
				if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			// For numbered DATs only
			else if (Regex.IsMatch(filename, _noIntroNumberedPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroNumberedPattern).Groups;
				if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			// For N-Gage and Gizmondo only
			else if (Regex.IsMatch(filename, _noIntroSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _noIntroSpecialPattern).Groups;
				if (!Remapping.NoIntro.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as No-Intro but could not be mapped.");
					return source;
				}
				source = "no-Intro";
			}
			else if (Regex.IsMatch(filename, _redumpPattern))
			{
				fileinfo = Regex.Match(filename, _redumpPattern).Groups;
				if (!Remapping.Redump.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Redump but could not be mapped.");
					return source;
				}
				source = "Redump";
			}
			// For special BIOSes only
			else if (Regex.IsMatch(filename, _redumpBiosPattern))
			{
				fileinfo = Regex.Match(filename, _redumpBiosPattern).Groups;
				if (!Remapping.Redump.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as Redump but could not be mapped.");
					return source;
				}
				source = "Redump";
			}
			else if (Regex.IsMatch(filename, _tosecPattern))
			{
				fileinfo = Regex.Match(filename, _tosecPattern).Groups;
				if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
				{
					// Handle special case mappings found only in TOSEC
					fileinfo = Regex.Match(filename, _tosecSpecialPatternA).Groups;

					if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
					{
						fileinfo = Regex.Match(filename, _tosecSpecialPatternB).Groups;

						if (!Remapping.TOSEC.ContainsKey(fileinfo[1].Value))
						{
							_logger.Warning("The filename " + fileinfo[1].Value + " was matched as TOSEC but could not be mapped.");
							return source;
						}
					}
				}
				source = "TOSEC";
			}
			else if (Regex.IsMatch(filename, _truripPattern))
			{
				fileinfo = Regex.Match(filename, _truripPattern).Groups;
				if (!Remapping.TruRip.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as TruRip but could not be mapped.");
					return source;
				}
				source = "trurip";
			}
			else if (Regex.IsMatch(filename, _zandroPattern))
			{
				source = "Zandro";
			}
			else if (Regex.IsMatch(filename, _defaultPattern))
			{
				fileinfo = Regex.Match(filename, _defaultPattern).Groups;
				source = fileinfo[3].Value;
			}
			else if (Regex.IsMatch(filename, _defaultSpecialPattern))
			{
				fileinfo = Regex.Match(filename, _defaultSpecialPattern).Groups;
				source = fileinfo[3].Value;
			}
			else if (Regex.IsMatch(filename, _mamePattern))
			{
				fileinfo = Regex.Match(filename, _mamePattern).Groups;
				if (!Remapping.MAME.ContainsKey(fileinfo[1].Value))
				{
					_logger.Warning("The filename " + fileinfo[1].Value + " was matched as MAME but could not be mapped.");
					return source;
				}
				source = "MAME";
			}

			return source;
		}

		#endregion
	}
}
