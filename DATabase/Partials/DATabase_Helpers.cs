using System;
using System.IO;
using Mono.Data.Sqlite;

using SabreTools.Helper;

namespace SabreTools
{
	public partial class DATabase
	{
		#region Helper methods

		/// <summary>
		/// Perform initial setup for the program
		/// </summary>
		private static void Setup()
		{
			Remapping.CreateRemappings();
			Build.Start("DATabase");

			// Perform initial database and folder setup
			if (!Directory.Exists(_datroot))
			{
				Directory.CreateDirectory(_datroot);
			}
			if (!Directory.Exists(_outroot))
			{
				Directory.CreateDirectory(_outroot);
			}
			DatabaseTools.EnsureDatabase(_dbName, _connectionString);

			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				string query = "SELECT * FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						while (sldr.Read())
						{
							int systemid = sldr.GetInt32(0);
							string system = _datroot + Path.DirectorySeparatorChar + sldr.GetString(1) + " - " + sldr.GetString(2);
							system = system.Trim();

							if (!Directory.Exists(system))
							{
								Directory.CreateDirectory(system);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// List sources in the database
		/// </summary>
		/// <remarks>This does not have an analogue in DATabaseTwo</remarks>
		private static void ListSources()
		{
			string query = @"
SELECT DISTINCT source.id, source.name, source.url
FROM source
ORDER BY source.name";
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
							_logger.Warning("No sources found! Please add a system and then try again.");
							return;
						}

						Console.WriteLine("Available Sources (id <= name):\n");
						while (sldr.Read())
						{
							Console.WriteLine(sldr.GetInt32(0) + "\t<=\t" + sldr.GetString(1) + (!String.IsNullOrEmpty(sldr.GetString(2)) ? " (" + sldr.GetString(2) + ")" : ""));
						}
					}
				}
			}
			return;
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
	}
}
