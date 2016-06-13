using System;
using Mono.Data.Sqlite;
using System.IO;

namespace SabreTools.Helper
{
	/// <summary>
	/// All general database operations
	/// </summary>
	public class Database
	{
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
				SqliteConnection.CreateFile(db);
			}

			//Get "type" from the filename
			string type = Path.GetFileNameWithoutExtension(db);

			// Connect to the file
			SqliteConnection dbc = new SqliteConnection(connectionString);
			dbc.Open();

			// Make sure the database has the correct schema
			try
			{
				if (type == "Headerer")
				{
					string query = @"
CREATE TABLE IF NOT EXISTS data (
	'sha1'		TEXT		NOT NULL,
	'header'	TEXT		NOT NULL,
	'type'		TEXT		NOT NULL,
	PRIMARY KEY (sha1, header, type)
)";
					SqliteCommand slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();
				}
				else if (type == "dats")
				{
					string query = @"
CREATE TABLE IF NOT EXISTS dats (
	'id'	INTEGER	PRIMARY KEY	NOT NULL,
	'size'	INTEGER				NOT NULL	DEFAULT -1,
	'sha1'	TEXT				NOT NULL,
	'name'	TEXT				NOT NULL
)";
					SqliteCommand slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS datsdata (
	'id'	INTEGER		NOT NULL,
	'key'		TEXT		NOT NULL,
	'value'		TEXT,
	PRIMARY KEY (id, key, value)
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS source (
	'id'	INTEGER PRIMARY KEY	NOT NULL,
	'name'	TEXT				NOT NULL	UNIQUE,
	'url'	TEXT				NOT NULL
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS system (
	'id'			INTEGER PRIMARY KEY	NOT NULL,
	'manufacturer'	TEXT				NOT NULL,
	'name'		TEXT				NOT NULL
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();
				}
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

		/// <summary>
		/// Add a new source to the database if it doesn't already exist
		/// </summary>
		/// <param name="name">Source name</param>
		/// <param name="url">Source URL(s)</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <returns>True if the source existed or could be added, false otherwise</returns>
		public static bool AddSource(string name, string url, string connectionString)
		{
			string query = "SELECT id, name, url FROM source WHERE name='" + name + "'";
			using (SqliteConnection dbc = new SqliteConnection(connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, add the source
						if (!sldr.HasRows)
						{
							string squery = "INSERT INTO source (name, url) VALUES ('" + name + "', '" + url + "')";
							using (SqliteCommand sslc = new SqliteCommand(squery, dbc))
							{
								return sslc.ExecuteNonQuery() >= 1;
							}
						}
						// Otherwise, update the source URL if it's different
						else
						{
							sldr.Read();
							if (url != sldr.GetString(2))
							{
								string squery = "UPDATE source SET url='" + url + "' WHERE id=" + sldr.GetInt32(0);
								using (SqliteCommand sslc = new SqliteCommand(squery, dbc))
								{
									return sslc.ExecuteNonQuery() >= 1;
								}
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Remove an existing source from the database
		/// </summary>
		/// <param name="id">Source ID to be removed from the database</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <returns>True if the source was removed, false otherwise</returns>
		public static bool RemoveSource(int id, string connectionString)
		{
			string query = "DELETE FROM source WHERE id=" + id;
			using (SqliteConnection dbc = new SqliteConnection(connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					return slc.ExecuteNonQuery() >= 1;
				}
			}
		}

		/// <summary>
		/// Add a new system to the database if it doesn't already exist
		/// </summary>
		/// <param name="manufacturer">Manufacturer name</param>
		/// <param name="system">System name</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <returns>True if the system existed or could be added, false otherwise</returns>
		public static bool AddSystem(string manufacturer, string system, string connectionString)
		{
			string query = "SELECT id, manufacturer, name FROM system WHERE manufacturer='" + manufacturer + "' AND system='" + system + "'";
			using (SqliteConnection dbc = new SqliteConnection(connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, add the system
						if (!sldr.HasRows)
						{
							string squery = "INSERT INTO name (manufacturer, system) VALUES ('" + manufacturer + "', '" + system + "')";
							using (SqliteCommand sslc = new SqliteCommand(squery, dbc))
							{
								return sslc.ExecuteNonQuery() >= 1;
							}
						}
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Remove an existing system from the database
		/// </summary>
		/// <param name="id">System ID to be removed from the database</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		/// <returns>True if the system was removed, false otherwise</returns>
		public static bool RemoveSystem(int id, string connectionString)
		{
			string query = "DELETE FROM system WHERE id=" + id;
			using (SqliteConnection dbc = new SqliteConnection(connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					return slc.ExecuteNonQuery() >= 1;
				}
			}
		}
	}
}
