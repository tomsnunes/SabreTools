using System;
using Mono.Data.Sqlite;
using System.IO;

namespace SabreTools.Helper
{
	/// <summary>
	/// All general database operations
	/// </summary>
	public class DBTools
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
				if (type == "DATabase")
				{
					string query = @"
CREATE TABLE IF NOT EXISTS checksums (
	'file'	INTEGER		NOT NULL,
	'size'	INTEGER		NOT NULL	DEFAULT -1,
	'crc'	TEXT		NOT NULL,
	'md5'	TEXT		NOT NULL,
	'sha1'	TEXT		NOT NULL,
	PRIMARY KEY (file, size, crc, md5, sha1)
)";
					SqliteCommand slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS files (
	'id'			INTEGER	PRIMARY KEY	NOT NULL,
	'setid'			INTEGER				NOT NULL,
	'name'			TEXT				NOT NULL,
	'type'			TEXT				NOT NULL	DEFAULT 'rom',
	'lastupdated'	TEXT				NOT NULL
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS games (
	'id'		INTEGER PRIMARY KEY	NOT NULL,
	'system'	INTEGER				NOT NULL,
	'name'		TEXT				NOT NULL,
	'parent'	INTEGER				NOT NULL	DEFAULT '0',
	'source'	INTEGER				NOT NULL	DEFAULT '0'
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS parent (
	'id'	INTEGER PRIMARY KEY	NOT NULL,
	'name'	TEXT				NOT NULL
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS sources (
	'id'	INTEGER PRIMARY KEY	NOT NULL,
	'name'	TEXT				NOT NULL	UNIQUE,
	'url'	TEXT				NOT NULL
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS systems (
	'id'			INTEGER PRIMARY KEY	NOT NULL,
	'manufacturer'	TEXT				NOT NULL,
	'system'		TEXT				NOT NULL
)";
					slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();
				}
				else if (type == "Headerer")
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
			string query = "SELECT id, name, url FROM sources WHERE name='" + name + "'";
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
							string squery = "INSERT INTO sources (name, url) VALUES ('" + name + "', '" + url + "')";
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
								string squery = "UPDATE sources SET url='" + url + "' WHERE id=" + sldr.GetInt32(0);
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
			string query = "DELETE FROM sources WHERE id=" + id;
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
			string query = "SELECT id, manufacturer, system FROM systems WHERE manufacturer='" + manufacturer + "' AND system='" + system + "'";
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
							string squery = "INSERT INTO systems (manufacturer, system) VALUES ('" + manufacturer + "', '" + system + "')";
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
			string query = "DELETE FROM systems WHERE id=" + id;
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
