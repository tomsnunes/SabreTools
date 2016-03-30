using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SabreTools
{
	public partial class SabreToolsUI : Form
	{
		private static string _dbName = "DATabase.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";

		public SabreToolsUI()
		{
			AllocConsole();
			InitializeComponent();
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("SabreTools designed and coded by: Matt Nadareski (darksabre76)\nTested by: @tractivo", "About");
		}

		private static object[] GetAllSystems()
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

		private static object[] GetAllSources()
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

		private void button1_Click(object sender, EventArgs e)
		{
			string systems = "";
			string sources = "";

			CheckedListBox.CheckedItemCollection cil = this.checkedListBox1.CheckedItems;
			Console.WriteLine(cil.Count);
			foreach (object ci in cil)
			{
				string id = Regex.Match(ci.ToString(), @".*? \((.*?)\)").Groups[1].Value;

				systems += (systems == "" ? id : ", " + id);
				Console.WriteLine(systems);
			}

			cil = this.checkedListBox2.CheckedItems;
			Console.WriteLine(cil.Count);
			foreach (object ci in cil)
			{
				string id = Regex.Match(ci.ToString(), @".*? \((.*?)\)").Groups[1].Value;

				sources += (sources == "" ? id : ", " + id);
				Console.WriteLine(systems);
			}

			Process.Start("DATabase.exe", "-g" + (systems != "" ? " systems=" + systems : "") + (sources != "" ? " sources=" + sources : ""));
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Process.Start("DATabase.exe", "-ga");
		}
	}
}
