using Mono.Data.Sqlite;
using System;
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
		/// <param name="type">Schema type to use</param>
		/// <param name="db">Name of the databse</param>
		/// <param name="connectionString">Connection string for SQLite</param>
		public static void EnsureDatabase(string type, string db, string connectionString)
		{
			// Set the type to lowercase
			type = type.ToLowerInvariant();

			// Make sure the file exists
			if (!File.Exists(db))
			{
				SqliteConnection.CreateFile(db);
			}

			// Open the database connection
			SqliteConnection dbc = new SqliteConnection(connectionString);
			dbc.Open();

			// Make sure the database has the correct schema
			try
			{
				if (type == "rombasharp")
				{
					string query = @"
CREATE TABLE IF NOT EXISTS data (
	'id'	INTEGER		NOT NULL
	'key'	TEXT		NOT NULL
	'value'	TEXT		NOT NULL
	PRIMARY KEY (id, key, value)
)";
					SqliteCommand slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();
					slc.Dispose();
				}
				else if (type == "headerer")
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
					slc.Dispose();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				dbc.Dispose();
			}
		}
	}
}
