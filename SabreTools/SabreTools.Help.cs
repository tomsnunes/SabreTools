using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;

namespace SabreTools
{
	public partial class SabreTools
	{
		// TODO: Add 1G1R to the flags for update
		public static Help RetrieveHelp()
		{
			// Create and add the header to the Help object
			string barrier = "-----------------------------------------";
			List<string> helpHeader = new List<string>()
			{
				"SabreTools - Manipulate, convert, and use DAT files",
				barrier,
				"Usage: SabreTools [option] [flags] [filename|dirname] ...",
				""
			};
			Help help = new Help(helpHeader);

			// Create the Help feature
			Feature helpFeature = new Feature(
				new List<string>() { "-?", "-h", "--help" },
				"Show this help",
				FeatureType.Flag,
				null);

			// Create the Script feature
			Feature script = new Feature(
				"--script",
				"Enable script mode (no clear screen)",
				FeatureType.Flag,
				null);

			// Create the DATFromDir feature
			Feature datFromDir = new Feature(
				new List<string>() { "-d", "--d2d", "--dfd" },
				"Create DAT(s) from an input directory",
				FeatureType.Flag,
				null);
			datFromDir.AddFeature("noMD5", new Feature(
				new List<string>() { "-nm", "--noMD5" },
				"Don't include MD5 in output",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("noSHA1", new Feature(
				new List<string>() { "-ns", "--noSHA1" },
				"Don't include SHA1 in output",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("noSHA256", new Feature(
				new List<string>() { "-ns256", "--noSHA256" },
				"Include SHA-256 in output", // TODO: Invert this later
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("noSHA384", new Feature(
				new List<string>() { "-ns384", "--noSHA384" },
				"Include SHA-384 in output", // TODO: Invert this later
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("noSHA512", new Feature(
				new List<string>() { "-ns512", "--noSHA512" },
				"Include SHA-512 in output", // TODO: Invert this later
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("bare", new Feature(
				new List<string>() { "-b", "--bare" },
				"Don't include date in file name",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("forcepack", new Feature(
				new List<string>() { "-fp", "--forcepack" },
				"Set force packing",
				FeatureType.String,
				new List<string>()
				{
					"Supported values are:",
					"	None, Zip, Unzip",
				}));
			datFromDir.AddFeature("files", new Feature(
				new List<string>() { "-f", "--files" },
				"Treat archives as files",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-all", new Feature(
				new List<string>() { "-oa", "--output-all" },
				"Output in all formats",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-am", new Feature(
				new List<string>() { "-oam", "--output-am" },
				"Output in AttractMode format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-cmp", new Feature(
				new List<string>() { "-oc", "--output-cmp" },
				"Output in CMP format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-csv", new Feature(
				new List<string>() { "-ocsv", "--output-csv" },
				"Output in CSV format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-dc", new Feature(
				new List<string>() { "-od", "--output-dc" },
				"Output in DOSCenter format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-lr", new Feature(
				new List<string>() { "-olr", "--output-lr" },
				"Output in MAME Listrom format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-miss", new Feature(
				new List<string>() { "-om", "--output-miss" },
				"Output in Missfile format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-md5", new Feature(
				new List<string>() { "-omd5", "--output-md5" },
				"Output in MD5 format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-ol", new Feature(
				new List<string>() { "-ool", "--output-ol" },
				"Output in OfflineList format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-rc", new Feature(
				new List<string>() { "-or", "--output-rc" },
				"Output in RomCenter format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-sd", new Feature(
				new List<string>() { "-os", "--output-sd" },
				"Output in SabreDat format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-sfv", new Feature(
				new List<string>() { "-osfv", "--output-sfv" },
				"Output in SFV format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-sha1", new Feature(
				new List<string>() { "-osha1", "--output-sha1" },
				"Output in SHA-1 format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-sha256", new Feature(
				new List<string>() { "-osha256", "--output-sha256" },
				"Output in SHA-256 format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-sha384", new Feature(
				new List<string>() { "-osha384", "--output-sha384" },
				"Output in SHA-256 format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-sha512", new Feature(
				new List<string>() { "-osha512", "--output-sha512" },
				"Output in SHA-256 format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-sl", new Feature(
				new List<string>() { "-osl", "--output-sl" },
				"Output in Softwarelist format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-tsv", new Feature(
				new List<string>() { "-otsv", "--output-tsv" },
				"Output in TSV format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-xml", new Feature(
				new List<string>() { "-ox", "--output-xml" },
				"Output in Logiqx XML format [default]",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("romba", new Feature(
				new List<string>() { "-ro", "--romba" },
				"Read files from a Romba input",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("skiparc", new Feature(
				new List<string>() { "-ska", "--skiparc" },
				"Skip any files that are treated like archives",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("skipfile", new Feature(
				new List<string>() { "-skf", "--skipfile" },
				"Skip any files that are not treated like archives",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("filename", new Feature(
				new List<string>() { "-f", "--filename" },
				"Set the external name of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("name", new Feature(
				new List<string>() { "-n", "--name" },
				"Set the internal name of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("desc", new Feature(
				new List<string>() { "-de", "--desc" },
				"Set the description of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("cat", new Feature(
				new List<string>() { "-c", "--cat" },
				"Set the category of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("version", new Feature(
				new List<string>() { "-v", "--version" },
				"Set the version of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("author", new Feature(
				new List<string>() { "-au", "--author" },
				"Set the author of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("email", new Feature(
				new List<string>() { "-em", "--email" },
				"Set a new email of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("homepage", new Feature(
				new List<string>() { "-hp", "--homepage" },
				"Set a new homepage of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("url", new Feature(
				new List<string>() { "-u", "--url" },
				"Set a new URL of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("comment", new Feature(
				new List<string>() { "-co", "--comment" },
				"Set a new comment of the DAT",
				FeatureType.String,
				null));
			datFromDir.AddFeature("superdat", new Feature(
				new List<string>() { "-sd", "--superdat" },
				"Enable SuperDAT creation",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("exclude-of", new Feature(
				new List<string>() { "-xof", "--exclude-of" },
				"Exclude romof, cloneof, sampleof tags",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("scene-date-strip", new Feature(
				new List<string>() { "-sds", "--scene-date-strip" },
				"Remove date from scene-named sets",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("add-blank", new Feature(
				new List<string>() { "-ab", "--add-blank" },
				"Output blank files for folders",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("add-date", new Feature(
				new List<string>() { "-ad", "--add-date" },
				"Output dates for each file parsed",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("copy-files", new Feature(
				new List<string>() { "-cf", "--copy-files" },
				"Copy files to the temp directory before parsing",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("header", new Feature(
				new List<string>() { "-h", "--header" },
				"Set a header skipper to use, blank means all",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("ignore-chd", new Feature(
				new List<string>() { "-ic", "--ignore-chd" },
				"Treat CHDs as regular files",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("temp", new Feature(
				new List<string>() { "-t", "--temp" },
				"Set the temporary directory to use",
				FeatureType.String,
				null));
			datFromDir.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));
			datFromDir.AddFeature("mt", new Feature(
				new List<string>() { "-mt", "--mt" },
				"Amount of threads to use (default 4, -1 unlimted)",
				FeatureType.String,
				null));

			// Create the Extract feature
			Feature extract = new Feature(
				new List<string>() { "-ex", "--extract" },
				"Extract and remove copier headers",
				FeatureType.Flag,
				null);
			extract.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));
			extract.AddFeature("no-store-header", new Feature(
				new List<string>() { "-nsh", "--no-store-header" },
				"Don't store the extracted header",
				FeatureType.Flag,
				null));

			// Create the Extension Split feature
			Feature extSplit = new Feature(
				new List<string>() { "-es", "--ext-split" },
				"Split DAT(s) by two file extensions",
				FeatureType.Flag,
				null);
			extSplit.AddFeature("exta", new Feature(
				new List<string>() { "-exta", "--exta" },
				"First extension (multiple allowed)",
				FeatureType.List,
				null));
			extSplit.AddFeature("extb", new Feature(
				new List<string>() { "-extb", "--extb" },
				"Second extension (multiple allowed)",
				FeatureType.List,
				null));
			extSplit.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));

			// Create the Hash Split feature
			Feature hashSplit = new Feature(
				new List<string>() { "-hs", "--hash-split" },
				"Split DAT(s) or folder by best-available hashes",
				FeatureType.Flag,
				null);
			hashSplit.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));

			// Create the Level Split feature
			Feature levelSplit = new Feature(
				new List<string>() { "-ls", "--lvl-split" },
				"Split a SuperDAT or folder by internal path",
				FeatureType.Flag,
				null);
			levelSplit.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));
			levelSplit.AddFeature("short", new Feature(
				new List<string>() { "-s", "--short" },
				"Use short output names",
				FeatureType.Flag,
				null));
			levelSplit.AddFeature("base", new Feature(
				new List<string>() { "-ba", "--base" },
				"Use source DAT as base name for outputs",
				FeatureType.Flag,
				null));

			// Create the Restore feature
			Feature restore = new Feature(
				new List<string>() { "-re", "--restore" },
				"Restore header to file based on SHA-1",
				FeatureType.Flag,
				null);
			restore.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));

			// Create the Sort feature
			Feature sort = new Feature(
				new List<string>() { "-ss", "--sort" },
				"Sort inputs by a set of DATs",
				FeatureType.Flag,
				new List<string>()
				{
					"",
					"Archive scanning levels:",
					"  0	Hash archive and contents",
					"  1	Only hash contents",
					"  2	Only hash archive",
				});
			sort.AddFeature("dat", new Feature(
				new List<string>() { "-dat", "--dat" },
				"Input DAT to rebuild against",
				FeatureType.List,
				null));
			sort.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));
			sort.AddFeature("depot", new Feature(
				new List<string>() { "-dep", "--depot" },
				"Assume directories are romba depots",
				FeatureType.Flag,
				null));
			sort.AddFeature("delete", new Feature(
				new List<string>() { "-del", "--delete" },
				"Delete fully rebuilt input files",
				FeatureType.Flag,
				null));
			sort.AddFeature("inverse", new Feature(
				new List<string>() { "-in", "--inverse" },
				"Rebuild only files not in DAT",
				FeatureType.Flag,
				null));
			sort.AddFeature("quick", new Feature(
				new List<string>() { "-qs", "--quick" },
				"Enable quick scanning of archives",
				FeatureType.Flag,
				null));
			sort.AddFeature("ignore-chd", new Feature(
				new List<string>() { "-ic", "--ignore-chd" },
				"Treat CHDs as regular files",
				FeatureType.Flag,
				null));
			sort.AddFeature("add-date", new Feature(
				new List<string>() { "-ad", "--add-date" },
				"Add original dates from DAT, if possible",
				FeatureType.Flag,
				null));
			sort.AddFeature("t7z", new Feature(
				new List<string>() { "-t7z", "--t7z" },
				"Enable Torrent7z output",
				FeatureType.Flag,
				null));
			sort.AddFeature("tar", new Feature(
				new List<string>() { "-tar", "--tar" },
				"Enable TAR output",
				FeatureType.Flag,
				null));
			sort.AddFeature("tgz", new Feature(
				new List<string>() { "-tgz", "--tgz" },
				"Enable TorrentGZ output",
				FeatureType.Flag,
				null));
			sort["tgz"].AddFeature("romba", new Feature(
				new List<string>() { "-r", "--romba" },
				"Enable Romba depot dir output",
				FeatureType.Flag,
				null));
			/*
			sort.AddFeature("tlrz", new Feature(
				new List<string>() { "-tlrz", "--tlrz" },
				"Enable TorrentLRZ output",
				FeatureType.Flag,
				null));
			*/
			/*
			sort.AddFeature("tlz4", new Feature(
				new List<string>() { "-tlz4", "--tlz4" },
				"Enable TorrentLZ4 output",
				FeatureType.Flag,
				null));
			*/
			/*
			sort.AddFeature("trar", new Feature(
				new List<string>() { "-trar", "--trar" },
				"Enable TorrentRAR output",
				FeatureType.Flag,
				null));
			*/
			/*
			sort.AddFeature("txz", new Feature(
				new List<string>() { "-txz", "--txz" },
				"Enable TorrentXZ output",
				FeatureType.Flag,
				null));
			*/
			sort.AddFeature("tzip", new Feature(
				new List<string>() { "-tzip", "--tzip" },
				"Enable TorrentZip output",
				FeatureType.Flag,
				null));
			/*
			sort.AddFeature("tzpaq", new Feature(
				new List<string>() { "-tzpaq", "--tzpaq" },
				"Enable TorrentZPAQ output",
				FeatureType.Flag,
				null));
			*/
			/*
			sort.AddFeature("tzstd", new Feature(
				new List<string>() { "-tzstd", "--tzstd" },
				"Enable TorrentZstd output",
				FeatureType.Flag,
				null));
			*/
			sort.AddFeature("header", new Feature(
				new List<string>() { "-h", "--header" },
				"Set a header skipper to use, blank means all",
				FeatureType.String,
				null));
			sort.AddFeature("7z", new Feature(
				new List<string>() { "-7z", "--7z" },
				"Set scanning level for 7z archives (default 1)",
				FeatureType.String,
				null));
			sort.AddFeature("gz", new Feature(
				new List<string>() { "-gz", "--gz" },
				"Set scanning level for GZip archives (default 1)",
				FeatureType.String,
				null));
			sort.AddFeature("rar", new Feature(
				new List<string>() { "-rar", "--rar" },
				"Set scanning level for RAR archives (default 1)",
				FeatureType.String,
				null));
			sort.AddFeature("zip", new Feature(
				new List<string>() { "-zip", "--zip" },
				"Set scanning level for ZIP archives (default 1)",
				FeatureType.String,
				null));
			sort.AddFeature("scan-all", new Feature(
				new List<string>() { "-sa", "--scan-all" },
				"Set scanning levels for all archives to 0",
				FeatureType.Flag,
				null));
			sort.AddFeature("dat-merged", new Feature(
				new List<string>() { "-dm", "--dat-merged" },
				"Force creating merged sets",
				FeatureType.Flag,
				null));
			sort.AddFeature("dat-split", new Feature(
				new List<string>() { "-ds", "--dat-split" },
				"Force creating split sets",
				FeatureType.Flag,
				null));
			sort.AddFeature("dat-nonmerged", new Feature(
				new List<string>() { "-dnm", "--dat-nonmerged" },
				"Force creating non-merged sets",
				FeatureType.Flag,
				null));
			sort.AddFeature("dat-fullnonmerged", new Feature(
				new List<string>() { "-df", "--dat-fullnonmerged" },
				"Force creating fully non-merged sets",
				FeatureType.Flag,
				null));
			sort.AddFeature("update-dat", new Feature(
				new List<string>() { "-ud", "--update-dat" },
				"Output updated DAT to output directory",
				FeatureType.Flag,
				null));
			sort.AddFeature("mt", new Feature(
				new List<string>() { "-mt", "--mt" },
				"Amount of threads to use (default 4, -1 unlimted)",
				FeatureType.String,
				null));	

			// Create the Stats feature
			Feature stats = new Feature(
				new List<string>() { "-st", "--stats" },
				"Get statistics on all input DATs",
				FeatureType.Flag,
				null);
			stats.AddFeature("all-stats", new Feature(
				new List<string>() { "-as", "--all-stats" },
				"Write all statistics to all available formats",
				FeatureType.Flag,
				null));
			stats.AddFeature("baddump-col", new Feature(
				new List<string>() { "-bc", "--baddump-col" },
				"Add baddump stats to output",
				FeatureType.Flag,
				null));
			stats.AddFeature("csv", new Feature(
				new List<string>() { "-csv", "--csv" },
				"Output in Comma-Separated Value format",
				FeatureType.Flag,
				null));
			stats.AddFeature("filename", new Feature(
				new List<string>() { "-f", "--filename" },
				"Set the filename for the output",
				FeatureType.String,
				null));
			stats.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));
			stats.AddFeature("html", new Feature(
				new List<string>() { "-html", "--html" },
				"Output in HTML format",
				FeatureType.Flag,
				null));
			stats.AddFeature("nodump-col", new Feature(
				new List<string>() { "-nc", "--nodump-col" },
				"Add nodump stats to output",
				FeatureType.Flag,
				null));
			stats.AddFeature("single", new Feature(
				new List<string>() { "-si", "--single" },
				"Show individual statistics",
				FeatureType.Flag,
				null));
			stats.AddFeature("tsv", new Feature(
				new List<string>() { "-tsv", "--tsv" },
				"Output in Tab-Separated Value format",
				FeatureType.Flag,
				null));
			stats.AddFeature("text", new Feature(
				new List<string>() { "-txt", "--text" },
				"Output in generic text format",
				FeatureType.Flag,
				null));

			// Create the Type Split feature
			Feature typeSplit = new Feature(
				new List<string>() { "-ts", "--type-split" },
				"Split DAT(s) or folder by file types (rom/disk)",
				FeatureType.Flag,
				null);
			typeSplit.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));

			// Create the Update feature
			Feature update = new Feature(
				new List<string>() { "-ud", "--update" },
				"Update and manipulate DAT file(s)",
				FeatureType.Flag,
				new List<string>()
				{
					"",
					"All -diX, --diff-XX flags can be used with each other",

					"",
					"Filter parameters game name, rom name, all hashes can",
					"be matched using full C#-style regex.",

					"",
					"Filter parameters for size can use postfixes for inputs:",
					"  e.g. 8kb => 8000 or 8kib => 8192",

					"",
					"Most of the filter parameters allow for multiple inputs:",
					"  e.g. --game-name=foo --game-name=bar",
				});
			update.AddFeature("output-all", new Feature(
				new List<string>() { "-oa", "--output-all" },
				"Output in all formats",
				FeatureType.Flag,
				null));
			update.AddFeature("output-am", new Feature(
				new List<string>() { "-oam", "--output-am" },
				"Output in AttractMode format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-cmp", new Feature(
				new List<string>() { "-oc", "--output-cmp" },
				"Output in CMP format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-csv", new Feature(
				new List<string>() { "-ocsv", "--output-csv" },
				"Output in CSV format",
				FeatureType.Flag,
				null));
				update["output-csv"].AddFeature("prefix", new Feature(
					new List<string>() { "-pre", "--prefix" },
					"Set prefix for all lines",
					FeatureType.String,
					null));
				update["output-csv"].AddFeature("postfix", new Feature(
					new List<string>() { "-post", "--postfix" },
					"Set postfix for all lines",
					FeatureType.String,
					null));
				update["output-csv"].AddFeature("quotes", new Feature(
					new List<string>() { "-q", "--quotes" },
					"Put double-quotes around each item",
					FeatureType.Flag,
					null));
			update.AddFeature("output-dc", new Feature(
				new List<string>() { "-od", "--output-dc" },
				"Output in DOSCenter format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-lr", new Feature(
				new List<string>() { "-olr", "--output-lr" },
				"Output in MAME Listrom format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-miss", new Feature(
				new List<string>() { "-om", "--output-miss" },
				"Output in Missfile format",
				FeatureType.Flag,
				new List<string>()
				{
					"",
					"Prefix and postfix can include certain fields from the",
					"items by including %blah% in the input.",
					"A list of features that can be used are:",
					"  game, name, crc, md5, sha1, sha256, sha384, sha512, size",
				}));
				update["output-miss"].AddFeature("roms", new Feature(
					new List<string>() { "-r", "--roms" },
					"Output roms to miss instead of sets",
					FeatureType.Flag,
					null));
				update["output-miss"].AddFeature("game-prefix", new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null));
				update["output-miss"].AddFeature("prefix", new Feature(
					new List<string>() { "-pre", "--prefix" },
					"Set prefix for all lines",
					FeatureType.String,
					null));
				update["output-miss"].AddFeature("postfix", new Feature(
					new List<string>() { "-post", "--postfix" },
					"Set postfix for all lines",
					FeatureType.String,
					null));
				update["output-miss"].AddFeature("quotes", new Feature(
					new List<string>() { "-q", "--quotes" },
					"Put double-quotes around each item",
					FeatureType.Flag,
					null));
				update["output-miss"].AddFeature("add-ext", new Feature(
					new List<string>() { "-ae", "--add-ext" },
					"Add an extension to each item",
					FeatureType.String,
					null));
				update["output-miss"].AddFeature("rep-ext", new Feature(
					new List<string>() { "-rep", "--rep-ext" },
					"Replace all extensions with specified",
					FeatureType.String,
					null));
				update["output-miss"].AddFeature("rem-ext", new Feature(
					new List<string>() { "-rme", "--rem-ext" },
					"Remove all extensions from each item",
					FeatureType.String,
					null));
				update["output-miss"].AddFeature("romba", new Feature(
					new List<string>() { "-ro", "--romba" },
					"Output in Romba format (requires SHA-1)",
					FeatureType.Flag,
					null));
				update["output-miss"].AddFeature("tsv", new Feature(
					new List<string>() { "-tsv", "--tsv" },
					"Output in Tab-Separated Value format",
					FeatureType.Flag,
					null));
				update["output-miss"].AddFeature("csv", new Feature(
					new List<string>() { "-csv", "--csv" },
					"Output in Comma-Separated Value format",
					FeatureType.Flag,
					null));
			update.AddFeature("output-md5", new Feature(
				new List<string>() { "-omd5", "--output-md5" },
				"Output in MD5 format",
				FeatureType.Flag,
				null));
				update["output-md5"].AddFeature("game-prefix", new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null));
			update.AddFeature("output-ol", new Feature(
				new List<string>() { "-ool", "--output-ol" },
				"Output in OfflineList format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-rc", new Feature(
				new List<string>() { "-or", "--output-rc" },
				"Output in RomCenter format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-sd", new Feature(
				new List<string>() { "-os", "--output-sd" },
				"Output in SabreDat format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-sfv", new Feature(
				new List<string>() { "-osfv", "--output-sfv" },
				"Output in SFV format",
				FeatureType.Flag,
				null));
				update["output-sfv"].AddFeature("game-prefix", new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null));
			update.AddFeature("output-sha1", new Feature(
				new List<string>() { "-osha1", "--output-sha1" },
				"Output in SHA-1 format",
				FeatureType.Flag,
				null));
				update["output-sha1"].AddFeature("game-prefix", new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null));
			update.AddFeature("output-sha256", new Feature(
				new List<string>() { "-osha256", "--output-sha256" },
				"Output in SHA-256 format",
				FeatureType.Flag,
				null));
				update["output-sha256"].AddFeature("game-prefix", new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null));
			update.AddFeature("output-sha384", new Feature(
				new List<string>() { "-osha384", "--output-sha384" },
				"Output in SHA-384 format",
				FeatureType.Flag,
				null));
				update["output-sha384"].AddFeature("game-prefix", new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null));
			update.AddFeature("output-sha512", new Feature(
				new List<string>() { "-osha512", "--output-sha512" },
				"Output in SHA-512 format",
				FeatureType.Flag,
				null));
				update["output-sha512"].AddFeature("game-prefix", new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null));
			update.AddFeature("output-sl", new Feature(
				new List<string>() { "-osl", "--output-sl" },
				"Output in Softwarelist format",
				FeatureType.Flag,
				null));
			update.AddFeature("output-tsv", new Feature(
				new List<string>() { "-otsv", "--output-tsv" },
				"Output in TSV format",
				FeatureType.Flag,
				null));
				update["output-tsv"].AddFeature("prefix", new Feature(
					new List<string>() { "-pre", "--prefix" },
					"Set prefix for all lines",
					FeatureType.String,
					null));
				update["output-tsv"].AddFeature("postfix", new Feature(
					new List<string>() { "-post", "--postfix" },
					"Set postfix for all lines",
					FeatureType.String,
					null));
				update["output-tsv"].AddFeature("quotes", new Feature(
					new List<string>() { "-q", "--quotes" },
					"Put double-quotes around each item",
					FeatureType.Flag,
					null));
			update.AddFeature("output-xml", new Feature(
				new List<string>() { "-ox", "--output-xml" },
				"Output in Logiqx XML format",
				FeatureType.Flag,
				null));
			update.AddFeature("filename", new Feature(
				new List<string>() { "-f", "--filename" },
				"Set a new filename",
				FeatureType.String,
				null));
			update.AddFeature("name", new Feature(
				new List<string>() { "-n", "--name" },
				"Set a new internal name",
				FeatureType.String,
				null));
			update.AddFeature("desc", new Feature(
				new List<string>() { "-de", "--desc" },
				"Set a new description",
				FeatureType.String,
				null));
			update.AddFeature("rootdir", new Feature(
				new List<string>() { "-r", "--root" },
				"Set a new rootdir",
				FeatureType.String,
				null));
			update.AddFeature("category", new Feature(
				new List<string>() { "-ca", "--category" },
				"Set a new category",
				FeatureType.String,
				null));
			update.AddFeature("version", new Feature(
				new List<string>() { "-v", "--version" },
				"Set a new version",
				FeatureType.String,
				null));
			update.AddFeature("date", new Feature(
				new List<string>() { "-da", "--date" },
				"Set a new date",
				FeatureType.String,
				null));
			update.AddFeature("author", new Feature(
				new List<string>() { "-au", "--author" },
				"Set a new author",
				FeatureType.String,
				null));
			update.AddFeature("email", new Feature(
				new List<string>() { "-em", "--email" },
				"Set a new email",
				FeatureType.String,
				null));
			update.AddFeature("homepage", new Feature(
				new List<string>() { "-hp", "--homepage" },
				"Set a new homepage",
				FeatureType.String,
				null));
			update.AddFeature("url", new Feature(
				new List<string>() { "-u", "--url" },
				"Set a new URL",
				FeatureType.String,
				null));
			update.AddFeature("comment", new Feature(
				new List<string>() { "-co", "--comment" },
				"Set a new comment",
				FeatureType.String,
				null));
			update.AddFeature("header", new Feature(
				new List<string>() { "-h", "--header" },
				"Set a new header skipper",
				FeatureType.String,
				null));
			update.AddFeature("superdat", new Feature(
				new List<string>() { "-sd", "--superdat" },
				"Set SuperDAT type",
				FeatureType.Flag,
				null));
			update.AddFeature("forcemerge", new Feature(
				new List<string>() { "-fm", "--forcemerge" },
				"Set force merging",
				FeatureType.String,
				new List<string>()
				{
					"			    Supported values are:",
					"			        None, Split, Full",
				}));
			update.AddFeature("forcend", new Feature(
				new List<string>() { "-fn", "--forcend" },
				"Set force nodump",
				FeatureType.String,
				new List<string>()
				{
					"			    Supported values are:",
					"			        None, Obsolete, Required, Ignore",
				}));
			update.AddFeature("forcepack", new Feature(
				new List<string>() { "-fp", "--forcepack" },
				"Set force packing",
				FeatureType.String,
				new List<string>()
				{
					"			    Supported values are:",
					"			        None, Zip, Unzip",
				}));
			update.AddFeature("exclude-of", new Feature(
				new List<string>() { "-xof", "--exclude-of" },
				"Exclude romof, cloneof, sampleof tags",
				FeatureType.Flag,
				null));
			update.AddFeature("scene-date-strip", new Feature(
				new List<string>() { "-sds", "--scene-date-strip" },
				"Remove date from scene-named sets",
				FeatureType.Flag,
				null));
			update.AddFeature("clean", new Feature(
				new List<string>() { "-clean", "--clean" },
				"Clean game names according to WoD standards",
				FeatureType.Flag,
				null));
			update.AddFeature("rem-uni", new Feature(
				new List<string>() { "-ru", "--rem-uni" },
				"Remove unicode characters from names",
				FeatureType.Flag,
				null));
			update.AddFeature("rem-md5", new Feature(
				new List<string>() { "-rmd5", "--rem-md5" },
				"Remove MD5 hashes from the output",
				FeatureType.Flag,
				null));
			update.AddFeature("rem-sha1", new Feature(
				new List<string>() { "-rsha1", "--rem-sha1" },
				"Remove SHA-1 hashes from the output",
				FeatureType.Flag,
				null));
			update.AddFeature("rem-sha256", new Feature(
				new List<string>() { "-rsha256", "--rem-sha256" },
				"Remove SHA-256 hashes from the output",
				FeatureType.Flag,
				null));
			update.AddFeature("rem-sha384", new Feature(
				new List<string>() { "-rsha384", "--rem-sha384" },
				"Remove SHA-384 hashes from the output",
				FeatureType.Flag,
				null));
			update.AddFeature("rem-sha512", new Feature(
				new List<string>() { "-rsha512", "--rem-sha512" },
				"Remove SHA-512 hashes from the output",
				FeatureType.Flag,
				null));
			update.AddFeature("desc-name", new Feature(
				new List<string>() { "-dan", "--desc-name" },
				"Use description instead of machine name",
				FeatureType.Flag,
				null));
			update.AddFeature("dat-merged", new Feature(
				new List<string>() { "-dm", "--dat-merged" },
				"Create merged sets",
				FeatureType.Flag,
				null));
			update.AddFeature("dat-split", new Feature(
				new List<string>() { "-ds", "--dat-split" },
				"Create split sets",
				FeatureType.Flag,
				null));
			update.AddFeature("dat-nonmerged", new Feature(
				new List<string>() { "-dnm", "--dat-nonmerged" },
				"Create non-merged sets",
				FeatureType.Flag,
				null));
			update.AddFeature("dat-devnonmerged", new Feature(
				new List<string>() { "-dnd", "--dat-devnonmerged" },
				"Create device non-merged sets",
				FeatureType.Flag,
				null));
			update.AddFeature("dat-fullnonmerged", new Feature(
				new List<string>() { "-df", "--dat-fullnonmerged" },
				"Create fully non-merged sets",
				FeatureType.Flag,
				null));
			update.AddFeature("trim", new Feature(
				new List<string>() { "-trim", "--trim" },
				"Trim file names to fit NTFS length",
				FeatureType.Flag,
				null));
				update["trim"].AddFeature("root-dir", new Feature(
					new List<string>() { "-rd", "--root-dir" },
					"Set the root directory for calc",
					FeatureType.String,
					null));
			update.AddFeature("single", new Feature(
				new List<string>() { "-si", "--single" },
				"All game names replaced by '!'",
				FeatureType.Flag,
				null));
			update.AddFeature("dedup", new Feature(
				new List<string>() { "-dd", "--dedup" },
				"Enable deduping in the created DAT",
				FeatureType.Flag,
				null));
			update.AddFeature("game-dedup", new Feature(
				new List<string>() { "-gdd", "--game-dedup" },
				"Enable in-game deduping in the created DAT",
				FeatureType.Flag,
				null));
			update.AddFeature("merge", new Feature(
				new List<string>() { "-m", "--merge" },
				"Merge the input DATs",
				FeatureType.Flag,
				null));
				update["merge"].AddFeature("bare", new Feature(
					new List<string>() { "-b", "--bare" },
					"Don't include the date in automatic name",
					FeatureType.Flag,
					null));
			update.AddFeature("diff", new Feature(
				new List<string>() { "-di", "--diff" },
				"Create diffdats from inputs (all outputs)",
				FeatureType.Flag,
				null));
				update["diff"].AddFeature("against", new Feature(
					new List<string>() { "-ag", "--against" },
					"Diff all inputs against a set of base DATs",
					FeatureType.Flag,
					null));
					update["diff"]["against"].AddFeature("base-dat", new Feature(
						new List<string>() { "-bd", "--base-dat" },
						"Add a base DAT for diffing",
						FeatureType.List,
						null));
				update["diff"].AddFeature("bare", new Feature(
					new List<string>() { "-b", "--bare" },
					"Don't include the date in automatic name",
					FeatureType.Flag,
					null));
				update["diff"].AddFeature("cascade", new Feature(
					new List<string>() { "-c", "--cascade" },
					"Enable cascaded diffing",
					FeatureType.Flag,
					null));
					update["diff"]["cascade"].AddFeature("skip", new Feature(
						new List<string>() { "-sf", "--skip" },
						"Skip output of first DAT",
						FeatureType.Flag,
						null));
				update["diff"].AddFeature("rev-cascade", new Feature(
						new List<string>() { "-rc", "--rev-cascade" },
						"Enable reverse cascaded diffing",
						FeatureType.Flag,
						null));
					update["diff"]["rev-cascade"].AddFeature("skip", new Feature(
						new List<string>() { "-sf", "--skip" },
						"Skip output of first DAT",
						FeatureType.Flag,
						null));
			update.AddFeature("diff-du", new Feature(
				new List<string>() { "-did", "--diff-du" },
				"Create diffdat containing just duplicates",
				FeatureType.Flag,
				null));
				update["diff-du"].AddFeature("bare", new Feature(
					new List<string>() { "-b", "--bare" },
					"Don't include the date in automatic name",
					FeatureType.Flag,
					null));
			update.AddFeature("diff-in", new Feature(
				new List<string>() { "-dii", "--diff-in" },
				"Create diffdats for individual DATs",
				FeatureType.Flag,
				null));
				update["diff-in"].AddFeature("bare", new Feature(
					new List<string>() { "-b", "--bare" },
					"Don't include the date in automatic name",
					FeatureType.Flag,
					null));
			update.AddFeature("diff-nd", new Feature(
				new List<string>() { "-din", "--diff-nd" },
				"Create diffdat containing no duplicates",
				FeatureType.Flag,
				null));
				update["diff-nd"].AddFeature("bare", new Feature(
					new List<string>() { "-b", "--bare" },
					"Don't include the date in automatic name",
					FeatureType.Flag,
					null));
			update.AddFeature("base-name", new Feature(
				new List<string>() { "-bn", "--base-name" },
				"Replace item names from base DATs in order",
				FeatureType.Flag,
				null));
				update["base-name"].AddFeature("base-dat", new Feature(
					new List<string>() { "-bd", "--base-dat" },
					"Add a base DAT for replacing",
					FeatureType.List,
					null));
			update.AddFeature("reverse-base-name", new Feature(
				new List<string>() { "-rbn", "--reverse-base-name" },
				"Replace item names from base DATs in reverse",
				FeatureType.Flag,
				null));
				update["reverse-base-name"].AddFeature("base-dat", new Feature(
					new List<string>() { "-bd", "--base-dat" },
					"Add a base DAT for replacing",
					FeatureType.List,
					null));
			update.AddFeature("game-name", new Feature(
				new List<string>() { "-gn", "--game-name" },
				"Filter by game name",
				FeatureType.List,
				null));
			update.AddFeature("not-game", new Feature(
				new List<string>() { "-ngn", "--not-game" },
				"Filter by not game name",
				FeatureType.List,
				null));
			update.AddFeature("of-as-game", new Feature(
				new List<string>() { "-ofg", "--of-as-game" },
				"Allow cloneof and romof tags to match game name filters",
				FeatureType.Flag,
				null));
			update.AddFeature("rom-name", new Feature(
				new List<string>() { "-rn", "--rom-name" },
				"Filter by rom name",
				FeatureType.List,
				null));
			update.AddFeature("not-rom", new Feature(
				new List<string>() { "-nrn", "--not-rom" },
				"Filter by not rom name",
				FeatureType.List,
				null));
			update.AddFeature("rom-type", new Feature(
				new List<string>() { "-rt", "--rom-type" },
				"Filter by rom type",
				FeatureType.List,
				null));
			update.AddFeature("not-type", new Feature(
				new List<string>() { "-nrt", "--not-type" },
				"Filter by not rom type",
				FeatureType.List,
				null));
			update.AddFeature("greater", new Feature(
				new List<string>() { "-sgt", "--greater" },
				"Filter by size >=",
				FeatureType.String,
				null));
			update.AddFeature("less", new Feature(
				new List<string>() { "-slt", "--less" },
				"Filter by size =<",
				FeatureType.String,
				null));
			update.AddFeature("equal", new Feature(
				new List<string>() { "-seq", "--equal" },
				"Filter by size ==",
				FeatureType.String,
				null));
			update.AddFeature("crc", new Feature(
				new List<string>() { "-crc", "--crc" },
				"Filter by CRC hash",
				FeatureType.List,
				null));
			update.AddFeature("not-crc", new Feature(
				new List<string>() { "-ncrc", "--not-crc" },
				"Filter by not CRC hash",
				FeatureType.List,
				null));
			update.AddFeature("md5", new Feature(
				new List<string>() { "-md5", "--md5" },
				"Filter by MD5 hash",
				FeatureType.List,
				null));
			update.AddFeature("not-md5", new Feature(
				new List<string>() { "-nmd5", "--not-md5" },
				"Filter by not MD5 hash",
				FeatureType.List,
				null));
			update.AddFeature("sha1", new Feature(
				new List<string>() { "-sha1", "--sha1" },
				"Filter by SHA-1 hash",
				FeatureType.List,
				null));
			update.AddFeature("not-sha1", new Feature(
				new List<string>() { "-nsha1", "--not-sha1" },
				"Filter by not SHA-1 hash",
				FeatureType.List,
				null));
			update.AddFeature("sha256", new Feature(
				new List<string>() { "-sha256", "--sha256" },
				"Filter by SHA-256 hash",
				FeatureType.List,
				null));
			update.AddFeature("not-sha256", new Feature(
				new List<string>() { "-nsha256", "--not-sha256" },
				"Filter by not SHA-256 hash",
				FeatureType.List,
				null));
			update.AddFeature("sha384", new Feature(
				new List<string>() { "-sha384", "--sha384" },
				"Filter by SHA-384 hash",
				FeatureType.List,
				null));
			update.AddFeature("not-sha384", new Feature(
				new List<string>() { "-nsha384", "--not-sha384" },
				"Filter by not SHA-384 hash",
				FeatureType.List,
				null));
			update.AddFeature("sha512", new Feature(
				new List<string>() { "-sha512", "--sha512" },
				"Filter by SHA-512 hash",
				FeatureType.List,
				null));
			update.AddFeature("not-sha512", new Feature(
				new List<string>() { "-nsha512", "--not-sha512" },
				"Filter by not SHA-512 hash",
				FeatureType.List,
				null));
			update.AddFeature("status", new Feature(
				new List<string>() { "-is", "--status" },
				"Include only items with a given status",
				FeatureType.List,
				new List<string>()
				{
					"			    Supported values are:",
					"			        None, Good, BadDump, Nodump, Verified",
				}));
			update.AddFeature("not-status", new Feature(
				new List<string>() { "-nis", "--not-status" },
				"Exclude only items with a given status",
				FeatureType.List,
				new List<string>()
				{
					"			    Supported values are:",
					"			        None, Good, BadDump, Nodump, Verified",
				}));
			update.AddFeature("game-type", new Feature(
				new List<string>() { "-gt", "--game-type" },
				"Include only games with a given type",
				FeatureType.List,
				new List<string>()
				{
					"			    Supported values are:",
					"			        None, Bios, Device, Mechanical",
				}));
			update.AddFeature("not-gtype", new Feature(
				new List<string>() { "-ngt", "--not-gtype" },
				"Exclude only games with a given type",
				FeatureType.List,
				new List<string>()
				{
					"			    Supported values are:",
					"			        None, Bios, Device, Mechanical",
				}));
			update.AddFeature("runnable", new Feature(
				new List<string>() { "-run", "--runnable" },
				"Include only items that are marked runnable",
				FeatureType.Flag,
				null));
			update.AddFeature("not-run", new Feature(
				new List<string>() { "-nrun", "--not-run" },
				"Include only items that are marked unrunnable",
				FeatureType.Flag,
				null));
			update.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory (overridden by --inplace)",
				FeatureType.String,
				null));
			update.AddFeature("inplace", new Feature(
				new List<string>() { "-ip", "--inplace" },
				"Enable overwriting of source files (update, cascade only)",
				FeatureType.Flag,
				null));
			update.AddFeature("mt", new Feature(
				new List<string>() { "-mt", "--mt" },
				"Amount of threads to use (default 4, -1 unlimited)",
				FeatureType.String,
				null));

			// Create the Verify feature
			Feature verify = new Feature(
				new List<string>() { "-ve", "--verify" },
				"Verify a folder against DATs",
				FeatureType.Flag,
				null);
			verify.AddFeature("dat", new Feature(
				new List<string>() { "-dat", "--dat" },
				"Input DAT to verify against",
				FeatureType.List,
				null));
			verify.AddFeature("depot", new Feature(
				new List<string>() { "-dep", "--depot" },
				"Assume directories are romba depots",
				FeatureType.Flag,
				null));
			verify.AddFeature("temp", new Feature(
				new List<string>() { "-t", "--temp" },
				"Set the temporary directory to use",
				FeatureType.String,
				null));
			verify.AddFeature("hash-only", new Feature(
				new List<string>() { "-ho", "--hash-only" },
				"Check files by hash only",
				FeatureType.Flag,
				null));
			verify.AddFeature("quick", new Feature(
				new List<string>() { "-qs", "--quick" },
				"Enable quick scanning of archives",
				FeatureType.Flag,
				null));
			verify.AddFeature("header", new Feature(
				new List<string>() { "-h", "--header" },
				"Set a header skipper to use, blank means all",
				FeatureType.String,
				null));
			verify.AddFeature("ignore-chd", new Feature(
				new List<string>() { "-ic", "--ignore-chd" },
				"Treat CHDs as regular files",
				FeatureType.Flag,
				null));
			verify.AddFeature("dat-merged", new Feature(
				new List<string>() { "-dm", "--dat-merged" },
				"Force checking merged sets",
				FeatureType.Flag,
				null));
			verify.AddFeature("dat-split", new Feature(
				new List<string>() { "-ds", "--dat-split" },
				"Force checking split sets",
				FeatureType.Flag,
				null));
			verify.AddFeature("dat-nonmerged", new Feature(
				new List<string>() { "-dnm", "--dat-nonmerged" },
				"Force checking non-merged sets",
				FeatureType.Flag,
				null));
			verify.AddFeature("dat-fullnonmerged", new Feature(
				new List<string>() { "-df", "--dat-fullnonmerged" },
				"Force checking fully non-merged sets",
				FeatureType.Flag,
				null));

			// Now, add all of the main features to the Help object
			help.Add("Help", helpFeature);
			help.Add("Script", script);
			help.Add("DATFromDir", datFromDir);
			help.Add("Extract", extract);
			help.Add("Extension Split", extSplit);
			help.Add("Hash Split", hashSplit);
			help.Add("Level Split", levelSplit);
			help.Add("Restore", restore);
			help.Add("Sort", sort);
			help.Add("Stats", stats);
			help.Add("Type Split", typeSplit);
			help.Add("Update", update);
			help.Add("Verify", verify);

			return help;
		}
	}
}
