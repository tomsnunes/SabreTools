using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;

namespace RombaSharp
{
	public partial class RombaSharp
	{
		public static Help RetrieveHelp()
		{
			// Create and add the header to the Help object
			string barrier = "-----------------------------------------";
			List<string> helpHeader = new List<string>()
			{
				"RombaSharp - C# port of the Romba rom management tool",
				barrier,
				"Usage: RombaSharp [option] [filename|dirname] ...",
				""
			};
			Help help = new Help(helpHeader);

			// Create the Help feature
			Feature helpFeature = new Feature(
				"Help",
				new List<string>() { "-?", "-h", "--help" },
				"Show this help",
				FeatureType.Flag,
				null);

			// Create the Archive feature
			Feature archive = new Feature(
				"Archive",
				"archive",
				"Adds ROM files from the specified directories to depot",
				FeatureType.Flag,
				null);
			archive.AddFeature(new Feature(
				"only-needed",
				"-only-needed",
				"Only archive ROM files in database",
				FeatureType.Flag,
				null));

			// Create the Build feature
			Feature build = new Feature(
				"Build",
				"build",
				"For each specified DAT file it creates TZip files",
				FeatureType.Flag,
				null);
			build.AddFeature(new Feature(
				"copy",
				"-copy",
				"Copy files instead of rebuilding",
				FeatureType.Flag,
				null));

			// Create the Stats feature
			Feature stats = new Feature(
				"Stats",
				"dbstats",
				"Prints db stats",
				FeatureType.Flag,
				null);

			// Create the Rescan Depots feature
			Feature rescanDepots = new Feature(
				"Rescan Depots",
				"depot-rescan",
				"Rescan a specific depot to get new information",
				FeatureType.Flag,
				null);

			// Create the Diffdat feature
			Feature diffdat = new Feature(
				"Diffdat",
				"diffdat",
				"Creates a DAT file for entries found in the new DAT",
				FeatureType.Flag,
				null);
			diffdat.AddFeature(new Feature(
				"new",
				"-new",
				"DAT to compare to",
				FeatureType.String,
				null));

			// Create the Dir2Dat feature
			Feature dir2dat = new Feature(
				"Dir2Dat",
				"dir2dat",
				"Creates a DAT file for the specified input directory",
				FeatureType.Flag,
				null);
			dir2dat.AddFeature(new Feature(
				"out",
				"-out",
				"Filename to save out to",
				FeatureType.String,
				null));

			// Create the Export feature
			Feature export = new Feature(
				"Export",
				"export",
				"Exports db to export.csv",
				FeatureType.Flag,
				null);

			// Create the Fixdat feature
			Feature fixdat = new Feature(
				"Fixdat",
				"fixdat",
				"For each specified DAT file it creates a fix DAT",
				FeatureType.Flag,
				null);

			// Create the Import feature
			Feature import = new Feature(
				"Import",
				"import",
				"Import a database from a formatted CSV file",
				FeatureType.Flag,
				null);

			// Create the Lookup feature
			Feature lookup = new Feature(
				"Lookup",
				"lookup",
				"For each specified hash, look up available information",
				FeatureType.Flag,
				null);

			// Create the Mmmstats feature
			Feature memstats = new Feature(
				"Memstats",
				"memstats",
				"Prints memory stats",
				FeatureType.Flag,
				null);

			// Create the Merge feature
			Feature merge = new Feature(
				"Merge",
				"merge",
				"Merges a depot into the existing one",
				FeatureType.Flag,
				null);
			merge.AddFeature(new Feature(
				"depot",
				"-depot",
				"Depot path to merge into",
				FeatureType.String,
				null));
			merge.AddFeature(new Feature(
				"only-needed",
				"-only-needed",
				"Only merge files in if needed",
				FeatureType.Flag,
				null));

			// Create the Miss feature
			Feature miss = new Feature(
				"Miss",
				"miss",
				"For each specified DAT file, create miss and have file",
				FeatureType.Flag,
				null);

			// Create the Purge Backup feature
			Feature purgeBackup = new Feature(
				"Purge Backup",
				"purge-backup",
				"Moves DAT index entries for orphaned DATs",
				FeatureType.Flag,
				null);
			purgeBackup.AddFeature(new Feature(
				"log-only",
				"-log-only",
				"Only write out actions to log",
				FeatureType.Flag,
				null));

			// Create the Purge Delete feature
			Feature purgeDelete = new Feature(
				"Purge Delete",
				"purge-delete",
				"Deletes DAT index entries for orphaned DATs",
				FeatureType.Flag,
				null);
			purgeDelete.AddFeature(new Feature(
				"log-only",
				"-log-only",
				"Only write out actions to log",
				FeatureType.Flag,
				null));

			// Create the Refresh DATs feature
			Feature refreshDats = new Feature(
				"Refresh DATs",
				"refresh-dats",
				"Refreshes the DAT index from the files in the DAT root",
				FeatureType.Flag,
				null);

			// Create the Progress feature
			Feature progress = new Feature(
				"Progress",
				"progress",
				"Shows progress of currently running command [OBSOLETE]",
				FeatureType.Flag,
				null);

			// Create the Shutdown feature
			Feature shutdown = new Feature(
				"Shutdown",
				"shutdown",
				"Gracefully shuts down server [OBSOLETE]",
				FeatureType.Flag,
				null);

			// Now, add all of the main features to the Help object
			help.Add(helpFeature);
			help.Add(archive);
			help.Add(build);
			help.Add(stats);
			help.Add(rescanDepots);
			help.Add(diffdat);
			help.Add(dir2dat);
			help.Add(export);
			help.Add(fixdat);
			help.Add(import);
			help.Add(lookup);
			help.Add(memstats);
			help.Add(merge);
			help.Add(miss);
			help.Add(purgeBackup);
			help.Add(purgeDelete);
			help.Add(refreshDats);
			help.Add(progress);
			help.Add(shutdown);

			return help;
		}
	}
}
