using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SabreTools.Helper
{
	public class Build
	{
		/// <summary>
		/// Returns true if running in a Mono environment
		/// </summary>
		public static bool MonoEnvironment
		{
			get { return (Type.GetType("Mono.Runtime") != null); }
		}

		/// <summary>
		/// Readies the console and outputs the header
		/// </summary>
		/// <param name="name">The name to be displayed as the program</param>
		/// <remarks>Adapted from http://stackoverflow.com/questions/8200661/how-to-align-string-in-fixed-length-string</remarks>
		public static void Start(string name)
		{
			// Dynamically create the header string
			int width = Console.WindowWidth - 3;
			string border = "+" + new string('-', width) + "+";
			string mid = name + " " + Constants.Version;
			mid = "|" + mid.PadLeft(((width - mid.Length) / 2) + mid.Length).PadRight(width) + "|";

			// If we're outputting to console, do fancy things
			if (!Console.IsOutputRedirected)
			{
				// Set the console to ready state
				ConsoleColor formertext = ConsoleColor.White;
				ConsoleColor formerback = ConsoleColor.Black;
				if (!MonoEnvironment)
				{
					Console.SetBufferSize(Console.BufferWidth, 999);
					formertext = Console.ForegroundColor;
					formerback = Console.BackgroundColor;
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.BackgroundColor = ConsoleColor.Blue;
				}

				Console.Title = "SabreTools-" + name + " " + Constants.Version;

				// Output the header
				Console.WriteLine(border);
				Console.WriteLine(mid);
				Console.WriteLine(border);
				Console.WriteLine();

				// Return the console to the original text and background colors
				if (!MonoEnvironment)
				{
					Console.ForegroundColor = formertext;
					Console.BackgroundColor = formerback;
				}
			}
		}

		/// <summary>
		/// Show the help dialog for a given class
		/// </summary>
		public static void Help()
		{
			//http://stackoverflow.com/questions/14849367/how-to-determine-calling-method-and-class-name
			StackTrace st = new StackTrace();
			string className = st.GetFrame(1).GetMethod().ReflectedType.Name;
			string barrier = "-----------------------------------------";
			List<String> helptext = new List<string>();

			// Set the help text
			switch (className)
			{
				case "RombaSharp":
					helptext.Add(Resources.Resources.RombaSharp_Name + " - " + Resources.Resources.RombaSharp_Desc);
					helptext.Add(barrier);
					helptext.Add(Resources.Resources.Usage + ": " + Resources.Resources.RombaSharp_Name + " [option] [filename|dirname] ...");
					helptext.Add("");
					helptext.Add("Options:");
					helptext.Add("  -?, -h, --help	Show this help");
					helptext.Add("  archive		Adds ROM files from the specified directories to depot");
					helptext.Add("	-only-needed		Only archive ROM files in database");
					helptext.Add("  build			For each specified DAT file it creates TZip files");
					helptext.Add("	-copy				Copy files instead of rebuilding");
					helptext.Add("  dbstats		Prints db stats");
					helptext.Add("  depot-rescan	Rescan a specific depot to get new information");
					helptext.Add("  diffdat		Creates a DAT file for entries found in the new DAT");
					helptext.Add("	-new=			DAT to compare to");
					helptext.Add("  dir2dat		Creates a DAT file for the specified input directory");
					helptext.Add("	-out=			Filename to save out to");
					helptext.Add("  export		Exports db to export.csv");
					helptext.Add("  fixdat		For each specified DAT file it creates a fix DAT");
					helptext.Add("  lookup		For each specified hash, look up available information");
					helptext.Add("  memstats		Prints memory stats");
					helptext.Add("  miss			For each specified DAT file, create miss and have file");
					helptext.Add("  progress		Shows progress of currently running command [OBSOLETE]");
					helptext.Add("  purge-backup		Moves DAT index entries for orphaned DATs");
					helptext.Add("  purge-delete		Deletes DAT index entries for orphaned DATs");
					helptext.Add("  refresh-dats		Refreshes the DAT index from the files in the DAT root");
					helptext.Add("  shutdown		Gracefully shuts down server [OBSOLETE]");
					break;
				case "SabreTools":
					helptext.Add(Resources.Resources.SabreTools_Name + " - " + Resources.Resources.SabreTools_Desc);
					helptext.Add(barrier);
					helptext.Add(Resources.Resources.Usage + ": " + Resources.Resources.SabreTools_Name + " [option] [filename|dirname] ...");
					helptext.Add("");
					helptext.Add("Options:");
					helptext.Add("  -?, -h, --help	Show this help");
					helptext.Add("  -d, --dfd		Create a DAT from an input directory");
					helptext.Add("	-nm, --noMD5		Don't include MD5 in output");
					helptext.Add("	-ns, --noSHA1		Don't include SHA1 in output");
					helptext.Add("	-b, --bare		Don't include date in file name");
					helptext.Add("	-u, --unzip		Force unzipping in created DAT");
					helptext.Add("	-f, --files		Treat archives as files");
					helptext.Add("	-oa, --output-all	Output in all formats");
					helptext.Add("	-oc, --output-cmp	Output in CMP format");
					helptext.Add("	-ocsv, --output-csv	Output in CSV format");
					helptext.Add("	-od, --output-dc	Output in DOSCenter format");
					helptext.Add("	-om, --output-miss	Output in Missfile format");
					helptext.Add("	-omd5, --output-md5	Output in MD5 format");
					helptext.Add("	-ool, --output-ol	Output in OfflineList format");
					helptext.Add("	-or, --output-rc	Output in RomCenter format");
					helptext.Add("	-os, --output-sd	Output in SabreDAT format");
					helptext.Add("	-osfv, --ouput-sfv	Output in SFV format");
					helptext.Add("	-osha1, --output-sha1	Output in SHA-1 format");
					helptext.Add("	-osl, --output-sl	Output in Softwarelist format");
					helptext.Add("	-otsv, --output-tsv	Output in TSV format");
					helptext.Add("	-ox, --output-xml	Output in Logiqx XML format");
					helptext.Add("	-gz, --gz-files		Allow reading of GZIP files as archives");
					helptext.Add("	-ro, --romba		Read files from a Romba input");
					helptext.Add("	-f=, --filename=	Set the external name of the DAT");
					helptext.Add("	-n=, --name=		Set the internal name of the DAT");
					helptext.Add("	-de=, --desc=		Set the description of the DAT");
					helptext.Add("	-c=, --cat=		Set the category of the DAT");
					helptext.Add("	-v=, --version=		Set the version of the DAT");
					helptext.Add("	-au=, --author=		Set the author of the DAT");
					helptext.Add("	-sd, --superdat		Enable SuperDAT creation");
					helptext.Add("	-xof, --exclude-of	Exclude romof, cloneof, sampleof tags");
					helptext.Add("	-ab, --add-blank	Output blank files for folders");
					helptext.Add("	-ad, --add-date		Output dates for each file parsed");
					helptext.Add("	-cf, --copy-files	Copy files to the temp directory before parsing");
					helptext.Add("	-h=, --header=		Set a header skipper to use, blank means all");
					helptext.Add("	-t=, --temp=		Set the temporary directory to use");
					helptext.Add("	-mt={4}			Amount of threads to use (-1 unlimted)");
					helptext.Add("  -es, --ext-split	Split a DAT by two file extensions");
					helptext.Add("	-exta=			First set of extensions (comma-separated)");
					helptext.Add("	-extb=			Second set of extensions (comma-separated)");
					helptext.Add("	-out=			Output directory");
					helptext.Add("  -he, --headerer	Extract and remove copier headers");
					helptext.Add("	-r, --restore		Restore header to file based on SHA-1 instead");
					helptext.Add("	-out=				Output directory");
					helptext.Add("  -hs, --hash-split	Split a DAT or folder by best-available hashes");
					helptext.Add("	-out=			Output directory");
					helptext.Add("  -st, --stats		Get statistics on all input DATs");
					helptext.Add("	-bc, --baddump-col	Add baddump stats to output");
					helptext.Add("	-csv, --csv			Output in Comma-Separated Value format");
					helptext.Add("	-f=, --filename=	Set the filename for the output");
					helptext.Add("	-html, --html		Output in HTML format");
					helptext.Add("	-nc, --nodump-col	Add nodump stats to output");
					helptext.Add("	-si, --single		Show individual statistics");
					helptext.Add("	-tsv, --tsv			Output in Tab-Separated Value format");
					helptext.Add("  -ts, --type-split		Split a DAT or folder by file types (rom/disk)");
					helptext.Add("	-out=			Output directory");
					helptext.Add("  -ud, --update		Update a DAT file");
					helptext.Add("	-oa, --output-all	Output in all formats");
					helptext.Add("	-oc, --output-cmp	Output in CMP format");
					helptext.Add("	-ocsv, --output-csv	Output in CSV format");
					helptext.Add("	  -pre=, --prefix=		Set prefix for all lines");
					helptext.Add("	  -post=, --postfix=		Set postfix for all lines");
					helptext.Add("	  -q, --quotes			Put double-quotes around each item");
					helptext.Add("	-od, --output-dc	Output in DOSCenter format");
					helptext.Add("	-om, --output-miss	Output in Missfile format");
					helptext.Add("	  -r, --roms			Output roms to miss instead of sets");
					helptext.Add("	  -gp, --game-prefix		Add game name as a prefix");
					helptext.Add("	  -pre=, --prefix=		Set prefix for all lines");
					helptext.Add("	  -post=, --postfix=		Set postfix for all lines");
					helptext.Add("	  -q, --quotes			Put double-quotes around each item");
					helptext.Add("	  -ae=, --add-ext=		Add an extension to each item");
					helptext.Add("	  -re=, --rep-ext=		Replace all extensions with specified");
					helptext.Add("	  -rme, --rem-ext		Remove all extensions from each item");
					helptext.Add("	  -ro, --romba			Output in Romba format (requires SHA-1)");
					helptext.Add("	  -tsv, --tsv			Output in Tab-Separated Value format");
					helptext.Add("	  -csv, --csv			Output in Comma-Separated Value format");
					helptext.Add("	-omd5, --output-md5	Output in MD5 format");
					helptext.Add("	  -gp, --game-prefix		Add game name as a prefix");
					helptext.Add("	-ool, --output-ol	Output in OfflineList format");
					helptext.Add("	-or, --output-rc	Output in RomCenter format");
					helptext.Add("	-os, --output-sd	Output in SabreDAT format");
					helptext.Add("	-osfv, --ouput-sfv	Output in SFV format");
					helptext.Add("	  -gp, --game-prefix		Add game name as a prefix");
					helptext.Add("	-osha1, --output-sha1	Output in SHA-1 format");
					helptext.Add("	  -gp, --game-prefix		Add game name as a prefix");
					helptext.Add("	-osl, --output-sl	Output in Softwarelist format");
					helptext.Add("	-otsv, --output-tsv	Output in TSV format");
					helptext.Add("	  -pre=, --prefix=		Set prefix for all lines");
					helptext.Add("	  -post=, --postfix=		Set postfix for all lines");
					helptext.Add("	  -q, --quotes			Put double-quotes around each item");
					helptext.Add("	-ox, --output-xml	Output in Logiqx XML format");
					helptext.Add("	-f=, --filename=	Set a new filename");
					helptext.Add("	-n=, --name=		Set a new internal name");
					helptext.Add("	-de=, --desc=		Set a new description");
					helptext.Add("	-r=, --root=		Set a new rootdir");
					helptext.Add("	-ca=, --category=	Set a new category");
					helptext.Add("	-v=, --version=		Set a new version");
					helptext.Add("	-da=, --date=		Set a new date");
					helptext.Add("	-au=, --author=		Set a new author");
					helptext.Add("	-em=, --email=		Set a new email");
					helptext.Add("	-hp=, --homepage=	Set a new homepage");
					helptext.Add("	-u=, --url=		Set a new URL");
					helptext.Add("	-co=, --comment=	Set a new comment");
					helptext.Add("	-h=, --header=		Set a new header skipper");
					helptext.Add("	-sd=, --superdat	Set SuperDAT type");
					helptext.Add("	-fm=, --forcemerge=	Set force merging");
					helptext.Add("		Supported values are:");
					helptext.Add("		  None, Split, Full");
					helptext.Add("	-fn=, --forcend=	Set force nodump");
					helptext.Add("		Supported values are:");
					helptext.Add("		  None, Obsolete, Required, Ignore");
					helptext.Add("	-fp=, --forcepack=	Set force packing");
					helptext.Add("		Supported values are:");
					helptext.Add("		  None, Zip, Unzip");
					helptext.Add("	-xof, --exclude-of	Exclude romof, cloneof, sampleof tags");
					helptext.Add("	-clean			Clean game names according to WoD standards");
					helptext.Add("	-sl, --softlist	Use Software List name instead of description");
					helptext.Add("	-trim			Trim file names to fit NTFS length");
					helptext.Add("		-rd=, --root-dir=	Set the root directory for calc");
					helptext.Add("	-si, --single		All game names replaced by '!'");
					helptext.Add("	-dd, --dedup		Enable deduping in the created DAT");
					helptext.Add("	-m, --merge		Merge the input DATs");
					helptext.Add("		-b, --bare		Don't include date in automatic name");
					helptext.Add("	-di, --diff		Create diffdats from inputs (all outputs)");
					helptext.Add("		-b, --bare		Don't include date in automatic name");
					helptext.Add("		-c, --cascade		Enable cascaded diffing");
					helptext.Add("			-ip, --inplace		Enable inplace, cascaded diff");
					helptext.Add("			-sf, --skip		Skip output of first DAT");
					helptext.Add("		-rc, --rev-cascade	Enable reverse cascaded diffing");
					helptext.Add("			-ip, --inplace		Enable inplace, cascaded diff");
					helptext.Add("			-sf, --skip		Skip output of first DAT");
					helptext.Add("	-did, --diff-du	Create diffdat containing just duplicates");
					helptext.Add("		[Can be used with other -diX flags]");
					helptext.Add("		-b, --bare		Don't include date in automatic name");
					helptext.Add("	-dii, --diff-in	Create diffdats for individual DATs");
					helptext.Add("		[Can be used with other -diX flags]");
					helptext.Add("		-b, --bare		Don't include date in automatic name");
					helptext.Add("	-din, --diff-nd	Create diffdat containing no duplicates");
					helptext.Add("		[Can be used with other -diX flags]");
					helptext.Add("		-b, --bare		Don't include date in automatic name");
					helptext.Add("	-gn=, --game-name=	Filter by game name");
					helptext.Add("	-rn=, --rom-name=	Filter by rom name");
					helptext.Add("	-rt=, --rom-type=	Filter by rom type");
					helptext.Add("	-sgt=, --greater=	Filter by size >=");
					helptext.Add("	-slt=, --less=		Filter by size <=");
					helptext.Add("	-seq=, --equal=		Filter by size ==");
					helptext.Add("	-crc=, --crc=		Filter by CRC hash");
					helptext.Add("	-md5=, --md5=		Filter by MD5 hash");
					helptext.Add("	-sha1=, --sha1=		Filter by SHA-1 hash");
					helptext.Add("	-is=, --status=		Include only items with a given status");
					helptext.Add("		Supported values are:");
					helptext.Add("		  None, Good, BadDump, Nodump, Verified, NotNodump");
					helptext.Add("	-out=			Output directory (overridden by --inplace)");
					helptext.Add("");
					helptext.Add("Filenames and directories can't start with a reserved string");
					helptext.Add("unless prefixed by 'input='");
					helptext.Add("");
					helptext.Add("Filter parameters game name, rom name, CRC, MD5, SHA-1 can");
					helptext.Add("do partial matches using asterisks as follows (case insensitive):");
					helptext.Add("    *00 means ends with '00'");
					helptext.Add("    00* means starts with '00'");
					helptext.Add("    *00* means contains '00'");
					helptext.Add("    00 means exactly equals '00'");
					helptext.Add("");
					helptext.Add("Filter parameters for size can use postfixes for inputs:");
					helptext.Add("    e.g. 8kb => 8000 or 8kib => 8192");
					break;
				case "SimpleSortApp":
					helptext.Add(Resources.Resources.SimpleSort_Name + " - " + Resources.Resources.SimpleSort_Desc);
					helptext.Add(barrier);
					helptext.Add(Resources.Resources.Usage + ": " + Resources.Resources.SimpleSort_Name + " [options] [filename|dirname] ...");
					helptext.Add("");
					helptext.Add("Options:");
					helptext.Add("  -?, -h, --help	Show this help");
					helptext.Add("  -dat=			Input DAT to rebuild against (REQUIRED)");
					helptext.Add("  -out=			Output directory");
					helptext.Add("  -t=, --temp=		Set the temporary directory to use");
					helptext.Add("  -d, --delete	Delete input files");
					helptext.Add("  -qs, --quick		Enable quick scanning of archives");
					helptext.Add("  -ad, --add-date		Add original dates from DAT, if possible");
					helptext.Add("  -v, --verify		Enable verification of output directory");
					helptext.Add("  -c, --convert		Enable conversion of input files to TGZ");
					helptext.Add("	Note: If a DAT is used, only files NOT included will rebuild");
					helptext.Add("  -tgz			Enable TorrentGZ output");
					helptext.Add("	-r, --romba		Enable Romba depot dir output");
					helptext.Add("  -do, --directory	Output files as uncompressed");
					helptext.Add("	-h=, --header=		Set a header skipper to use, blank means all");
					helptext.Add("  -7z={0}		Set scanning level for 7z archives");
					helptext.Add("  -gz={2}		Set scanning level for GZip archives");
					helptext.Add("  -rar={2}		Set scanning level for RAR archives");
					helptext.Add("  -zip={0}		Set scanning level for ZIP archives");
					helptext.Add("  -ud, --update-dat	Output updated DAT");
					helptext.Add("");
					helptext.Add("Archive scanning levels:");
					helptext.Add("  0	Hash archive and contents");
					helptext.Add("  1	Only hash contents");
					helptext.Add("  2	Only hash archive");
					break;
				default:
					helptext.Add(Resources.Resources.Default_Desc);
					break;
			}

			// Now output based on the size of the screen
			int i = 0;
			foreach (string help in helptext)
			{
				Console.WriteLine(help);
				i++;

				// If we're not being redirected and we reached the size of the screen, pause
				if (i == Console.WindowHeight - 2)
				{
					i = 0;
					Pause();
				}
			}
			Pause();
		}

		public static void Credits()
		{
			Console.WriteLine(@"-----------------------------------------
Credits
-----------------------------------------

Programmer / Lead:	Matt Nadareski (darksabre76)
Additional code:	emuLOAD, @tractivo, motoschifo
Testing:		emuLOAD, @tractivo, Kludge, Obiwantje, edc
Suggestions:		edc, AcidX, Amiga12, EliUmniCk
Based on work by:	The Wizard of DATz");
			Pause();
		}

		private static void Pause()
		{
			if (!Console.IsOutputRedirected)
			{
				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();
			}
		}
	}
}
