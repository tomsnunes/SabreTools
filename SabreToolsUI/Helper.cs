using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabreTools
{
	class UIHelper
	{
		private static string _dbName = "DATabase.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";

		public static object[] GetAllSystems()
		{
			List<object> objs = new List<object>();

			Process.Start("DATabase.exe", "--skip");

			string query = @"
SELECT DISTINCT systems.id, systems.manufacturer, systems.system
FROM systems JOIN games ON systems.id=games.system
ORDER BY systems.manufacturer, systems.system";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (sldr.HasRows)
						{
							while (sldr.Read())
							{
								objs.Add(sldr.GetString(1) + " - " + sldr.GetString(2) + " (" + sldr.GetInt32(0) + ")");
							}
						}
					}
				}
			}

			return objs.ToArray();
		}

		public static object[] GetAllSources()
		{
			List<object> objs = new List<object>();

			Process.Start("DATabase.exe", "--skip");

			string query = @"
SELECT DISTINCT sources.id, sources.name
FROM sources JOIN games on sources.id=games.source
ORDER BY sources.name COLLATE NOCASE";
			using (SQLiteConnection dbc = new SQLiteConnection(_connectionString))
			{
				dbc.Open();
				using (SQLiteCommand slc = new SQLiteCommand(query, dbc))
				{
					using (SQLiteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (sldr.HasRows)
						{
							while (sldr.Read())
							{
								objs.Add(sldr.GetString(1) + " (" + sldr.GetInt32(0) + ")");
							}
						}
					}
				}
			}

			return objs.ToArray();
		}
	}
}
