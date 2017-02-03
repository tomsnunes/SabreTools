using System.Collections.Generic;

using SabreTools.Helper.Data;
using SabreTools.Helper.Help;
using SabreTools.Helper.Resources;

namespace RombaSharp
{
	public partial class RombaSharp
	{
		public static Help RetrieveHelp()
		{
			// Create and add the header to the Help object
			string barrier = "-----------------------------------------";
			List<string> helpHeader = new List<string>();
			helpHeader.Add(Resources.RombaSharp_Name + " - " + Resources.RombaSharp_Desc);
			helpHeader.Add(barrier);
			helpHeader.Add(Resources.Usage + ": " + Resources.RombaSharp_Name + " [option] [filename|dirname] ...");
			helpHeader.Add("");
			Help help = new Help(helpHeader);

			// Create the Help feature
			Feature helpFeature = new Feature(
				new List<string>() { "-?", "-h", "--help" },
				"Show this help",
				FeatureType.Flag,
				null);

			// Create the Archive feature
			Feature archive = new Feature(
				"archive",
				"Adds ROM files from the specified directories to depot",
				FeatureType.Flag,
				null);
			archive.AddFeature("only-needed", new Feature(
				"-only-needed",
				"Only archive ROM files in database",
				FeatureType.Flag,
				null));

			// Create the Build feature
			Feature build = new Feature(
				"build",
				"For each specified DAT file it creates TZip files",
				FeatureType.Flag,
				null);
			build.AddFeature("copy", new Feature(
				"-copy",
				"Copy files instead of rebuilding",
				FeatureType.Flag,
				null));

			// Create the Stats feature
			Feature stats = new Feature(
				"dbstats",
				"Prints db stats",
				FeatureType.Flag,
				null);

			// Create the Rescan Depots feature
			Feature rescanDepots = new Feature(
				"depot-rescan",
				"Rescan a specific depot to get new information",
				FeatureType.Flag,
				null);

			// Create the Diffdat feature
			Feature diffdat = new Feature(
				"diffdat",
				"Creates a DAT file for entries found in the new DAT",
				FeatureType.Flag,
				null);
			diffdat.AddFeature("new", new Feature(
				"-new",
				"DAT to compare to",
				FeatureType.String,
				null));

			// Create the Dir2Dat feature
			Feature dir2dat = new Feature(
				"dir2dat",
				"Creates a DAT file for the specified input directory",
				FeatureType.Flag,
				null);
			dir2dat.AddFeature("out", new Feature(
				"-out",
				"Filename to save out to",
				FeatureType.String,
				null));

			// Create the Export feature
			Feature export = new Feature(
				"export",
				"Exports db to export.csv",
				FeatureType.Flag,
				null);

			// Create the Fixdat feature
			Feature fixdat = new Feature(
				"fixdat",
				"For each specified DAT file it creates a fix DAT",
				FeatureType.Flag,
				null);

			// Create the Lookup feature
			Feature lookup = new Feature(
				"lookup",
				"For each specified hash, look up available information",
				FeatureType.Flag,
				null);

			// Create the Mmmstats feature
			Feature memstats = new Feature(
				"memstats",
				"Prints memory stats",
				FeatureType.Flag,
				null);

			// Create the Merge feature
			Feature merge = new Feature(
				"merge",
				"Merges a depot into the existing one",
				FeatureType.Flag,
				null);
			merge.AddFeature("depot", new Feature(
				"-depot",
				"Depot path to merge into",
				FeatureType.String,
				null));
			merge.AddFeature("only-needed", new Feature(
				"-only-needed",
				"Only merge files in if needed",
				FeatureType.Flag,
				null));

			// Create the Miss feature
			Feature miss = new Feature(
				"miss",
				"For each specified DAT file, create miss and have file",
				FeatureType.Flag,
				null);

			// Create the Purge Backup feature
			Feature purgeBackup = new Feature(
				"purge-backup",
				"Moves DAT index entries for orphaned DATs",
				FeatureType.Flag,
				null);

			// Create the Purge Delete feature
			Feature purgeDelete = new Feature(
				"purge-delete",
				"Deletes DAT index entries for orphaned DATs",
				FeatureType.Flag,
				null);

			// Create the Refresh DATs feature
			Feature refreshDats = new Feature(
				"refresh-dats",
				"Refreshes the DAT index from the files in the DAT root",
				FeatureType.Flag,
				null);

			// Create the Progress feature
			Feature progress = new Feature(
				"progress",
				"Shows progress of currently running command [OBSOLETE]",
				FeatureType.Flag,
				null);

			// Create the Shutdown feature
			Feature shutdown = new Feature(
				"shutdown",
				"Gracefully shuts down server [OBSOLETE]",
				FeatureType.Flag,
				null);

			// Now, add all of the main features to the Help object
			help.Add("Help", helpFeature);
			help.Add("Archive", archive);
			help.Add("Build", build);
			help.Add("Stats", stats);
			help.Add("Rescan Depots", rescanDepots);
			help.Add("Diffdat", diffdat);
			help.Add("Dir2Dat", dir2dat);
			help.Add("Export", export);
			help.Add("Fixdat", fixdat);
			help.Add("Lookup", lookup);
			help.Add("Memstats", memstats);
			help.Add("Merge", merge);
			help.Add("Miss", miss);
			help.Add("Purge Backup", purgeBackup);
			help.Add("Purge Delete", purgeDelete);
			help.Add("Refresh DATs", refreshDats);
			help.Add("Progress", progress);
			help.Add("Shutdown", shutdown);

			return help;
		}
	}
}
