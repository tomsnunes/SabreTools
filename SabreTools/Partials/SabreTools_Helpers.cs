using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
using System.IO;

namespace SabreTools
{
	public partial class SabreTools
	{
		#region Helper methods

		/// <summary>
		/// Perform initial setup for the program
		/// </summary>
		private static void Setup()
		{
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
			DBTools.EnsureDatabase(_databaseDbSchema, _databaseDbName, _databaseConnectionString);

			using (SqliteConnection dbc = new SqliteConnection(_databaseConnectionString))
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
							string system = Path.Combine(_datroot, sldr.GetString(1).Trim() + " - " + sldr.GetString(2).Trim());

							if (!Directory.Exists(system))
							{
								Directory.CreateDirectory(system);
							}
						}
					}
				}
			}

			DBTools.EnsureDatabase(_headererDbSchema, _headererDbName, _headererConnectionString);
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
			using (SqliteConnection dbc = new SqliteConnection(_databaseConnectionString))
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
			using (SqliteConnection dbc = new SqliteConnection(_databaseConnectionString))
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

		/// <summary>
		/// Get the multiplier to be used with the size given
		/// </summary>
		/// <param name="sizestring">String with possible size with extension</param>
		/// <returns>Tuple of multiplier to use on final size and fixed size string</returns>
		private static long GetSizeFromString(string sizestring)
		{
			long size = 0;

			// Make sure the string is in lower case
			sizestring = sizestring.ToLowerInvariant();

			// Get any trailing size identifiers
			long multiplier = 1;
			if (sizestring.EndsWith("k") || sizestring.EndsWith("kb"))
			{
				multiplier = Constants.KiloByte;
			}
			else if (sizestring.EndsWith("ki") || sizestring.EndsWith("kib"))
			{
				multiplier = Constants.KibiByte;
			}
			else if (sizestring.EndsWith("m") || sizestring.EndsWith("mb"))
			{
				multiplier = Constants.MegaByte;
			}
			else if (sizestring.EndsWith("mi") || sizestring.EndsWith("mib"))
			{
				multiplier = Constants.MibiByte;
			}
			else if (sizestring.EndsWith("g") || sizestring.EndsWith("gb"))
			{
				multiplier = Constants.GigaByte;
			}
			else if (sizestring.EndsWith("gi") || sizestring.EndsWith("gib"))
			{
				multiplier = Constants.GibiByte;
			}
			else if (sizestring.EndsWith("t") || sizestring.EndsWith("tb"))
			{
				multiplier = Constants.TeraByte;
			}
			else if (sizestring.EndsWith("ti") || sizestring.EndsWith("tib"))
			{
				multiplier = Constants.TibiByte;
			}
			else if (sizestring.EndsWith("p") || sizestring.EndsWith("pb"))
			{
				multiplier = Constants.PetaByte;
			}
			else if (sizestring.EndsWith("pi") || sizestring.EndsWith("pib"))
			{
				multiplier = Constants.PibiByte;
			}

			// Remove any trailing identifiers
			sizestring = sizestring.TrimEnd(new char[] { 'k', 'm', 'g', 't', 'p', 'i', 'b', ' ' });

			// Now try to get the size from the string
			if (!Int64.TryParse(sizestring, out size))
			{
				size = -1;
			}
			else
			{
				size *= multiplier;
			}

			return size;
		}

		#endregion
	}
}
