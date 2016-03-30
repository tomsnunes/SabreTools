using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SabreTools
{
	public partial class SabreToolsUI : Form
	{
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

		private void generateButton_Click(object sender, EventArgs e)
		{
			string systems = "";
			string sources = "";

			CheckedListBox.CheckedItemCollection cil = this.systemsCheckedListBox.CheckedItems;
			foreach (object ci in cil)
			{
				string id = Regex.Match(ci.ToString(), @".*? \((.*?)\)").Groups[1].Value;

				systems += (systems == "" ? id : "," + id);
			}

			cil = this.sourcesCheckedListBox.CheckedItems;
			foreach (object ci in cil)
			{
				string id = Regex.Match(ci.ToString(), @".*? \((.*?)\)").Groups[1].Value;

				sources += (sources == "" ? id : "," + id);
			}

			bool old = this.oldCheckBox.Checked;
			bool norename = !this.renameCheckBox.Checked;

			string args = "-l -g" +
				(old ? " -old" : "") +
				(norename ? " -nr" : "") +
				(systems != "" ? " system=" + systems : "") +
				(sources != "" ? " source=" + sources : "");

			Process.Start("DATabase.exe", args);
		}

		private void generateAllButton_Click(object sender, EventArgs e)
		{
			Process.Start("DATabase.exe", "-l -ga");
		}

		private void exploreButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			// Set the proper starting folder
			if (importTextBox.Text != "")
			{
				ofd.InitialDirectory = Path.GetDirectoryName(importTextBox.Text);
			}
			else
			{
				ofd.InitialDirectory = Environment.CurrentDirectory;
			}

			// Set the new folder, if applicable
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				importTextBox.Text = ofd.FileName;
			}
		}
	}
}
