using Mono.Data.Sqlite;
using System;
using System.IO;

namespace SabreTools.Helper
{
	/// <summary>
	/// All general database operations
	/// </summary>
	public class DatabaseTools
	{
		/// <summary>
		/// Add a header to the database
		/// </summary>
		/// <param name="header">String representing the header bytes</param>
		/// <param name="SHA1">SHA-1 of the deheadered file</param>
		/// <param name="type">Name of the source skipper file</param>
		/// <param name="logger">Logger object for console and file output</param>
		public static void AddHeaderToDatabase(string header, string SHA1, string source, Logger logger)
		{
			bool exists = false;

			// Open the database connection
			SqliteConnection dbc = new SqliteConnection(Constants.HeadererConnectionString);
			dbc.Open();

			string query = @"SELECT * FROM data WHERE sha1='" + SHA1 + "' AND header='" + header + "'";
			SqliteCommand slc = new SqliteCommand(query, dbc);
			SqliteDataReader sldr = slc.ExecuteReader();
			exists = sldr.HasRows;

			if (!exists)
			{
				query = @"INSERT INTO data (sha1, header, type) VALUES ('" +
				SHA1 + "', " +
				"'" + header + "', " +
				"'" + source + "')";
				slc = new SqliteCommand(query, dbc);
				logger.Verbose("Result of inserting header: " + slc.ExecuteNonQuery());
			}

			// Dispose of database objects
			slc.Dispose();
			sldr.Dispose();
			dbc.Dispose();
		}

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
	'id'		INTEGER		NOT NULL
	'size'		INTEGER		NOT NULL
	'crc'		TEXT		NOT NULL
	'md5'		TEXT		NOT NULL
	'sha1'		TEXT		NOT NULL
	'exists'	INTEGER		NOT NULL
	PRIMARY KEY (id)
)";
					SqliteCommand slc = new SqliteCommand(query, dbc);
					slc.ExecuteNonQuery();

					query = @"
CREATE TABLE IF NOT EXISTS dats (
	'id'	INTEGER		NOT NULL
	'hash'	TEXT		NOT NULL
	PRIMARY KEY (id, hash)
)";
					slc = new SqliteCommand(query, dbc);
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
