using System.Collections.Generic;

using SabreTools.Helper.Data;
using SabreTools.Helper.Help;
using SabreTools.Helper.Resources;

namespace SabreTools
{
	public partial class SabreTools
	{
		public static Help RetrieveHelp()
		{
			// Create and add the header to the Help object
			string barrier = "-----------------------------------------";
			List<string> helpHeader = new List<string>();
			helpHeader.Add(Resources.SabreTools_Name + " - " + Resources.SabreTools_Desc);
			helpHeader.Add(barrier);
			helpHeader.Add(Resources.Usage + ": " + Resources.SabreTools_Name + " [option] [flags] [filename|dirname] ...");
			helpHeader.Add("");
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
				"Create a DAT from an input directory",
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
			datFromDir.AddFeature("output-miss", new Feature(
				new List<string>() { "-om", "--output-miss" },
				"Output in Missfile format",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("output-md5", new Feature(
				new List<string>() { "-oa", "--output-md5" },
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
				new List<string>() { "-osfv", "--output-sha1" },
				"Output in SHA-1 format",
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
			datFromDir.AddFeature("gz-files", new Feature(
				new List<string>() { "-gzf", "--gz-files" },
				"Allow reading of GZIP files as archives",
				FeatureType.Flag,
				null));
			datFromDir.AddFeature("romba", new Feature(
				new List<string>() { "-ro", "--romba" },
				"Read files from a Romba input",
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
				new List<string>() { "-d", "--desc" },
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
			datFromDir.AddFeature("temp", new Feature(
				new List<string>() { "-t", "--temp" },
				"Set the temporary directory to use",
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

			// Create the Extension Split feature
			Feature extSplit = new Feature(
				new List<string>() { "-es", "--ext-split" },
				"Split a DAT by two file extensions",
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
				"Split a DAT or folder by best-available hashes",
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
			sort.AddFeature("temp", new Feature(
				new List<string>() { "-temp", "--temp" },
				"Set the temporary directory to use",
				FeatureType.String,
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
			sort.AddFeature("add-date", new Feature(
				new List<string>() { "-ad", "--add-date" },
				"Add original dates from DAT, if possible",
				FeatureType.Flag,
				null));
			/*
			sort.AddFeature("t7z", new Feature(
				new List<string>() { "-t7z", "--t7z" },
				"Enable Torrent7z output",
				FeatureType.Flag,
				null));
			*/
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
				"Set scanning level for GZip archives (default 2)",
				FeatureType.String,
				null));
			sort.AddFeature("rar", new Feature(
				new List<string>() { "-rar", "--rar" },
				"Set scanning level for RAR archives (default 2)",
				FeatureType.String,
				null));
			sort.AddFeature("zip", new Feature(
				new List<string>() { "-zip", "--zip" },
				"Set scanning level for ZIP archives (default 1)",
				FeatureType.String,
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

			// Create the Sort Depot feature
			Feature sortDepot = new Feature(
				new List<string>() { "-ssd", "--sort-depot" },
				"Sort romba depots by a set of DATs",
				FeatureType.Flag,
				null);
			sortDepot.AddFeature("dat", new Feature(
				new List<string>() { "-dat", "--dat" },
				"Input DAT to rebuild against",
				FeatureType.List,
				null));
			sortDepot.AddFeature("out", new Feature(
				new List<string>() { "-out", "--out" },
				"Output directory",
				FeatureType.String,
				null));
			sortDepot.AddFeature("temp", new Feature(
				new List<string>() { "-temp", "--temp" },
				"Set the temporary directory to use",
				FeatureType.String,
				null));
			sortDepot.AddFeature("delete", new Feature(
				new List<string>() { "-del", "--delete" },
				"Delete fully rebuilt input files",
				FeatureType.Flag,
				null));
			sortDepot.AddFeature("inverse", new Feature(
				new List<string>() { "-in", "--inverse" },
				"Rebuild only files not in DAT",
				FeatureType.Flag,
				null));
			sortDepot.AddFeature("add-date", new Feature(
				new List<string>() { "-ad", "--add-date" },
				"Add original dates from DAT, if possible",
				FeatureType.Flag,
				null));
			/*
			sortDepot.AddFeature("t7z", new Feature(
				new List<string>() { "-t7z", "--t7z" },
				"Enable Torrent7z output",
				FeatureType.Flag,
				null));
			*/
			sortDepot.AddFeature("tar", new Feature(
				new List<string>() { "-tar", "--tar" },
				"Enable TAR output",
				FeatureType.Flag,
				null));
			sortDepot.AddFeature("tgz", new Feature(
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
			sortDepot.AddFeature("tlrz", new Feature(
				new List<string>() { "-tlrz", "--tlrz" },
				"Enable TorrentLRZ output",
				FeatureType.Flag,
				null));
			*/
			/*
			sortDepot.AddFeature("trar", new Feature(
				new List<string>() { "-trar", "--trar" },
				"Enable TorrentRAR output",
				FeatureType.Flag,
				null));
			*/
			/*
			sortDepot.AddFeature("txz", new Feature(
				new List<string>() { "-txz", "--txz" },
				"Enable TorrentXZ output",
				FeatureType.Flag,
				null));
			*/
			sortDepot.AddFeature("tzip", new Feature(
				new List<string>() { "-tzip", "--tzip" },
				"Enable TorrentZip output",
				FeatureType.Flag,
				null));
			sortDepot.AddFeature("header", new Feature(
				new List<string>() { "-h", "--header" },
				"Set a header skipper to use, blank means all",
				FeatureType.String,
				null));
			sortDepot.AddFeature("update-dat", new Feature(
				new List<string>() { "-ud", "--update-dat" },
				"Output updated DAT to output directory",
				FeatureType.Flag,
				null));
			sortDepot.AddFeature("mt", new Feature(
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
				new List<string>() { "-tsv", "--csv" },
				"Output in Tab-Separated Value format",
				FeatureType.Flag,
				null));

			/*

			// Create the Type Split feature
			Feature typeSplit = new Feature(
				new List<string>() { "-ts", "--type-split" },
				"Split a DAT or folder by file types (rom/disk)",
				FeatureType.Flag,
				null);
			elptext.Add("  -out=			Output directory");

			// Create the Update feature
			Feature update = new Feature(
				new List<string>() { "-ud", "--update" },
				"Update a DAT file",
				FeatureType.Flag,
				null);
			helptext.Add("  -oa, --output-all	    Output in all formats");
			helptext.Add("  -oam, --output-am	    Output in AttractMode format");
			helptext.Add("  -oc, --output-cmp	    Output in CMP format");
			helptext.Add("  -ocsv, --output-csv	    Output in CSV format");
			helptext.Add("    -pre=, --prefix=		Set prefix for all lines");
			helptext.Add("    -post=, --postfix=		Set postfix for all lines");
			helptext.Add("    -q, --quotes		Put double-quotes around each item");
			helptext.Add("  -od, --output-dc	    Output in DOSCenter format");
			helptext.Add("  -om, --output-miss	    Output in Missfile format");
			helptext.Add("    -r, --roms			Output roms to miss instead of sets");
			helptext.Add("    -gp, --game-prefix		Add game name as a prefix");
			helptext.Add("    -pre=, --prefix=		Set prefix for all lines");
			helptext.Add("    -post=, --postfix=		Set postfix for all lines");
			helptext.Add("    -q, --quotes		Put double-quotes around each item");
			helptext.Add("    -ae=, --add-ext=		Add an extension to each item");
			helptext.Add("    -rep=, --rep-ext=		Replace all extensions with specified");
			helptext.Add("    -rme, --rem-ext		Remove all extensions from each item");
			helptext.Add("    -ro, --romba		Output in Romba format (requires SHA-1)");
			helptext.Add("    -tsv, --tsv			Output in Tab-Separated Value format");
			helptext.Add("    -csv, --csv			Output in Comma-Separated Value format");
			helptext.Add("  -omd5, --output-md5	    Output in MD5 format");
			helptext.Add("    -gp, --game-prefix		Add game name as a prefix");
			helptext.Add("  -ool, --output-ol	    Output in OfflineList format");
			helptext.Add("  -or, --output-rc	    Output in RomCenter format");
			helptext.Add("  -os, --output-sd	    Output in SabreDAT format");
			helptext.Add("  -osfv, --ouput-sfv	    Output in SFV format");
			helptext.Add("    -gp, --game-prefix		Add game name as a prefix");
			helptext.Add("  -osha1, --output-sha1	    Output in SHA-1 format");
			helptext.Add("    -gp, --game-prefix		Add game name as a prefix");
			helptext.Add("  -osl, --output-sl	    Output in Softwarelist format");
			helptext.Add("  -otsv, --output-tsv	    Output in TSV format");
			helptext.Add("    -pre=, --prefix=		Set prefix for all lines");
			helptext.Add("    -post=, --postfix=		Set postfix for all lines");
			helptext.Add("    -q, --quotes		Put double-quotes around each item");
			helptext.Add("  -ox, --output-xml	    Output in Logiqx XML format");
			helptext.Add("  -f=, --filename=	    Set a new filename");
			helptext.Add("  -n=, --name=		    Set a new internal name");
			helptext.Add("  -de=, --desc=		    Set a new description");
			helptext.Add("  -r=, --root=		    Set a new rootdir");
			helptext.Add("  -ca=, --category=	    Set a new category");
			helptext.Add("  -v=, --version=	    Set a new version");
			helptext.Add("  -da=, --date=		    Set a new date");
			helptext.Add("  -au=, --author=	    Set a new author");
			helptext.Add("  -em=, --email=	    Set a new email");
			helptext.Add("  -hp=, --homepage=	    Set a new homepage");
			helptext.Add("  -u=, --url=		    Set a new URL");
			helptext.Add("  -co=, --comment=	    Set a new comment");
			helptext.Add("  -h=, --header=	    Set a new header skipper");
			helptext.Add("  -sd=, --superdat	    Set SuperDAT type");
			helptext.Add("  -fm=, --forcemerge=	    Set force merging");
			helptext.Add("			    Supported values are:");
			helptext.Add("			        None, Split, Full");
			helptext.Add("  -fn=, --forcend=	    Set force nodump");
			helptext.Add("			    Supported values are:");
			helptext.Add("			        None, Obsolete, Required, Ignore");
			helptext.Add("  -fp=, --forcepack=	    Set force packing");
			helptext.Add("			    Supported values are:");
			helptext.Add("			        None, Zip, Unzip");
			helptext.Add("  -xof, --exclude-of	    Exclude romof, cloneof, sampleof tags");
			helptext.Add("  -clean		    Clean game names according to WoD standards");
			helptext.Add("  -sl, --softlist	    Use Software List name instead of description");
			helptext.Add("  -dm, --dat-merged	    Create merged sets");
			helptext.Add("  -ds, --dat-split	    Create split sets");
			helptext.Add("  -dnm, --dat-nonmerged     Create non-merged sets");
			helptext.Add("  -df, --dat-fullnonmerged  Create fully non-merged sets");
			helptext.Add("  -trim			    Trim file names to fit NTFS length");
			helptext.Add("	-rd=, --root-dir=	Set the root directory for calc");
			helptext.Add("  -si, --single		    All game names replaced by '!'");
			helptext.Add("  -dd, --dedup		    Enable deduping in the created DAT");
			helptext.Add("  -m, --merge		    Merge the input DATs");
			helptext.Add("	-b, --bare		Don't include date in automatic name");
			helptext.Add("  -di, --diff		    Create diffdats from inputs (all outputs)");
			helptext.Add("	-b, --bare		Don't include date in automatic name");
			helptext.Add("	-c, --cascade		Enable cascaded diffing");
			helptext.Add("		-ip, --inplace		Enable inplace, cascaded diff");
			helptext.Add("		-sf, --skip		Skip output of first DAT");
			helptext.Add("	-rc, --rev-cascade	Enable reverse cascaded diffing");
			helptext.Add("		-ip, --inplace		Enable inplace, cascaded diff");
			helptext.Add("		-sf, --skip		Skip output of first DAT");
			helptext.Add("  -did, --diff-du	    Create diffdat containing just duplicates");
			helptext.Add("	-b, --bare		Don't include date in automatic name");
			helptext.Add("  -dii, --diff-in	    Create diffdats for individual DATs");
			helptext.Add("	-b, --bare		Don't include date in automatic name");
			helptext.Add("  -din, --diff-nd	    Create diffdat containing no duplicates");
			helptext.Add("	-b, --bare		Don't include date in automatic name");
			helptext.Add("  -gn=, --game-name=	    Filter by game name");
			helptext.Add("  -ngn=, --not-game=	    Filter by not game name");
			helptext.Add("  -rn=, --rom-name=	    Filter by rom name");
			helptext.Add("  -nrn=, --not-rom=	    Filter by not rom name");
			helptext.Add("  -rt=, --rom-type=	    Filter by rom type");
			helptext.Add("  -nrt=, --not-type=	    Filter by not rom type");
			helptext.Add("  -sgt=, --greater=	    Filter by size >=");
			helptext.Add("  -slt=, --less=	    Filter by size <=");
			helptext.Add("  -seq=, --equal=	    Filter by size ==");
			helptext.Add("  -crc=, --crc=		    Filter by CRC hash");
			helptext.Add("  -ncrc=, --not-crc=	    Filter by not CRC hash");
			helptext.Add("  -md5=, --md5=		    Filter by MD5 hash");
			helptext.Add("  -nmd5=, --not-md5=	    Filter by not MD5 hash");
			helptext.Add("  -sha1=, --sha1=	    Filter by SHA-1 hash");
			helptext.Add("  -nsha1=, --not-sha1=	    Filter by not SHA-1 hash");
			helptext.Add("  -is=, --status=	    Include only items with a given status");
			helptext.Add("			    Supported values are:");
			helptext.Add("			        None, Good, BadDump, Nodump, Verified");
			helptext.Add("  -nis=, --not-status=	    Exclude items with a given status");
			helptext.Add("			    Supported values are:");
			helptext.Add("			        None, Good, BadDump, Nodump, Verified");
			helptext.Add("  -gt=, --game-type=	    Include only games with a given type");
			helptext.Add("			    Supported values are:");
			helptext.Add("			        None, Bios, Device, Mechanical");
			helptext.Add("  -ngt=, --not-gtype=	    Exclude only games with a given type");
			helptext.Add("			    Supported values are:");
			helptext.Add("			        None, Bios, Device, Mechanical");
			helptext.Add("  -run, --runnable	    Include only items that are marked runnable");
			helptext.Add("  -nrun, --not-run	    Include only items that are marked unrunnable");
			helptext.Add("  -out=			    Output directory (overridden by --inplace)");
			helptext.Add("  -mt={4}		    Amount of threads to use (-1 unlimted)");


			helptext.Add("");
			helptext.Add(barrier);
			helptext.Add("Additional Notes:");

			helptext.Add("");
			helptext.Add("All -diX, --diff-XX flags can be used with each other");

			helptext.Add("");
			helptext.Add("Filter parameters game name, rom name, CRC, MD5, SHA-1 can");
			helptext.Add("do partial matches using asterisks as follows (case insensitive):");
			helptext.Add("  *00 means ends with '00'");
			helptext.Add("  00* means starts with '00'");
			helptext.Add("  *00* means contains '00'");
			helptext.Add("  00 means exactly equals '00'");

			helptext.Add("");
			helptext.Add("Filter parameters for size can use postfixes for inputs:");
			helptext.Add("  e.g. 8kb => 8000 or 8kib => 8192");

			helptext.Add("");
			helptext.Add("Most of the filter parameters allow for multiple inputs:");
			helptext.Add("  e.g. --game-name=foo --game-name=bar");

			// Create the Verify feature
			Feature verify = new Feature(
				new List<string>() { "-ve", "--verify" },
				"Verify a folder against DATs",
				FeatureType.Flag,
				null);
			helptext.Add("  -dat=			Input DAT to verify against");
			helptext.Add("  -t=, --temp=		Set the temporary directory to use");
			helptext.Add("  -ho, --hash-only	Check files by hash only");
			helptext.Add("  -qs, --quick		Enable quick scanning of archives");
			helptext.Add("  -h=, --header=	Set a header skipper to use, blank means all");

			*/

			// Create the Verify Depot feature
			Feature verifyDepot = new Feature(
				new List<string>() { "-ved", "--verify-depot" },
				"Verify a depot against DATs",
				FeatureType.Flag,
				null);
			verifyDepot.AddFeature("dat", new Feature(
				new List<string>() { "-dat", "--dat" },
				"Input DAT to verify against",
				FeatureType.List,
				null));
			verifyDepot.AddFeature("temp", new Feature(
				new List<string>() { "-t", "--temp" },
				"Set the temporary directory to use",
				FeatureType.String,
				null));
			verifyDepot.AddFeature("header", new Feature(
				new List<string>() { "-h", "--header" },
				"Set a header skipper to use, blank means all",
				FeatureType.String,
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
			help.Add("Sort Depot", sortDepot);
			help.Add("Stats", stats);
			//help.Add("Type Split", typeSplit);
			//help.Add("Update", update);
			//help.Add("Verify", verify);
			help.Add("Verify Depot", verifyDepot);

			return help;
		}
	}
}
