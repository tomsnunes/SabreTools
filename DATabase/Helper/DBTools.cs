using System;
using System.Data.SQLite;
using System.IO;

namespace DATabase.Helper
{
	class DBTools
	{
		public static void EnsureDatabase(string db, string connectionString)
		{
			// Make sure the file exists
			if (!File.Exists(db))
			{
				SQLiteConnection.CreateFile(db);
			}

			// Connect to the file
			SQLiteConnection dbc = new SQLiteConnection(connectionString);
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
	}
}
