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
					helptext.Add("  dbstats		Prints db stats");
					helptext.Add("  diffdat		Creates a DAT file for entries found in the new DAT");
					helptext.Add("	-new=			DAT to compare to");
					helptext.Add("  dir2dat		Creates a DAT file for the specified input directory");
					helptext.Add("	-out=			Filename to save out to");
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
					helptext.Add("  -a, --add		Add a new system or source to the database");
					helptext.Add("	-manu=			Manufacturer name (system only)");
					helptext.Add("	-system=		System name (system only)");
					helptext.Add("	-source=		Source name (source only)");
					helptext.Add("	-url=			URL (source only)");
					helptext.Add("  -d, --dfd		Create a DAT from each input directory");
					helptext.Add("	-nm, --noMD5		Don't include MD5 in output");
					helptext.Add("	-ns, --noSHA1		Don't include SHA1 in output");
					helptext.Add("	-b, --bare		Don't include date in file name");
					helptext.Add("	-u, --unzip		Force unzipping in created DAT");
					helptext.Add("	-f, --files		Treat archives as files");
					helptext.Add("	-oc, --output-cmp	Output in CMP format");
					helptext.Add("	-om, --output-miss	Output in Missfile format");
					helptext.Add("	-omd5, --output-md5	Output in MD5 format");
					helptext.Add("	-or, --output-rc	Output in RomCenter format");
					helptext.Add("	-os, --output-sd	Output in SabreDAT format");
					helptext.Add("	-osfv, --ouput-sfv	Output in SFV format");
					helptext.Add("	-osha1, --output-sha1	Output in SHA-1 format");
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
					helptext.Add("	-t=, --temp=		Set the temporary directory to use");
					helptext.Add("  -dp, --dfdp		Create a DAT using multithreading");
					helptext.Add("	-nm, --noMD5		Don't include MD5 in output");
					helptext.Add("	-ns, --noSHA1		Don't include SHA1 in output");
					helptext.Add("	-b, --bare		Don't include date in file name");
					helptext.Add("	-u, --unzip		Force unzipping in created DAT");
					helptext.Add("	-f, --files		Treat archives as files");
					helptext.Add("	-oc, --output-cmp	Output in CMP format");
					helptext.Add("	-om, --output-miss	Output in Missfile format");
					helptext.Add("	-omd5, --output-md5	Output in MD5 format");
					helptext.Add("	-or, --output-rc	Output in RomCenter format");
					helptext.Add("	-os, --output-sd	Output in SabreDAT format");
					helptext.Add("	-osfv, --ouput-sfv	Output in SFV format");
					helptext.Add("	-osha1, --output-sha1	Output in SHA-1 format");
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
					helptext.Add("	-t=, --temp=		Set the temporary directory to use");
					helptext.Add("	-mt={4}			Amount of threads to use (-1 unlimted)");
					helptext.Add("  -es, --ext-split	Split a DAT by two file extensions");
					helptext.Add("	-exta=			First set of extensions (comma-separated)");
					helptext.Add("	-extb=			Second set of extensions (comma-separated)");
					helptext.Add("	-out=			Output directory");
					helptext.Add("  -g, --generate	Start tool in generate mode");
					helptext.Add("	-system=		System ID to generate from");
					helptext.Add("	-nr, --no-rename	Don't auto-rename games");
					helptext.Add("	-o, --old		Output DAT in CMP format instead of XML");
					helptext.Add("  -ga, --generate-all	Start tool in generate all mode");
					helptext.Add("	-nr, --no-rename	Don't auto-rename games");
					helptext.Add("	-o, --old		Output DAT in CMP format instead of XML");
					helptext.Add("  -hs, --hash-split		Split a DAT or folder by best-available hashes");
					helptext.Add("	-out=			Output directory");
					helptext.Add("  -i, --import		Start tool in import mode");
					helptext.Add("	-ig, --ignore		Don't prompt for new sources");
					helptext.Add("  -lso, --list-sources	List all sources (id <= name)");
					helptext.Add("  -lsy, --list-systems	List all systems (id <= name)");
					helptext.Add("  -ol, --offmerge	Update DATS for offline arrays (see notes)");
					helptext.Add("	-com=			Complete current DAT");
					helptext.Add("	-fix=			Complete current Missing");
					helptext.Add("	-new=			New Complete DAT");
					helptext.Add("	-fk, --fake		Replace all hashes and sizes by the default");
					helptext.Add("  -rm, --remove		Remove a system or source from the database");
					helptext.Add("	-system=		System ID");
					helptext.Add("	-source=		Source ID");
					helptext.Add("  -st, --stats		Get statistics on all input DATs");
					helptext.Add("	-si, --single		Show individual statistics");
					helptext.Add("  -ts, --type-split		Split a DAT or folder by file types (rom/disk)");
					helptext.Add("	-out=			Output directory");
					helptext.Add("  -ud, --update		Update a DAT file");
					helptext.Add("	-oc, --output-cmp	Output in CMP format");
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
					helptext.Add("	-or, --output-rc	Output in RomCenter format");
					helptext.Add("	-os, --output-sd	Output in SabreDAT format");
					helptext.Add("	-osfv, --ouput-sfv	Output in SFV format");
					helptext.Add("	-osha1, --output-sha1	Output in SHA-1 format");
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
					helptext.Add("	-nd, --nodump		Include only match nodump roms");
					helptext.Add("	-nnd, --not-nodump	Exclude all nodump roms");
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
					helptext.Add("");
					helptext.Add("Offline merge mode notes:");
					helptext.Add("  This program will output the following DATs:");
					helptext.Add("    (a) Net New - (NewComplete)-(Complete)");
					helptext.Add("    (b) Unneeded - (Complete)-(NewComplete)");
					helptext.Add("    (c) New Missing - (Net New)+(Missing-(Unneeded))");
					helptext.Add("    (d) Have - (NewComplete)-(New Missing)");
					helptext.Add("      OR(Complete or NewComplete) - (Missing) if one is missing");
					break;
				case "Headerer":
					helptext.Add(Resources.Resources.Headerer_Name + " - " + Resources.Resources.Headerer_Desc);
					helptext.Add(barrier);
					helptext.Add(Resources.Resources.Usage + ": " + Resources.Resources.Headerer_Name + " [option] [filename|dirname]");
					helptext.Add("");
					helptext.Add("Options:");
					helptext.Add("  -?, -h, --help	Show this help");
					helptext.Add("  -e				Detect and remove mode");
					helptext.Add("  -r				Restore header to file based on SHA-1");
					break;
				case "SimpleSort":
					helptext.Add(Resources.Resources.SimpleSort_Name + " - " + Resources.Resources.SimpleSort_Desc);
					helptext.Add(barrier);
					helptext.Add(Resources.Resources.Usage + ": " + Resources.Resources.SimpleSort_Name + " [options] [filename|dirname] ...");
					helptext.Add("");
					helptext.Add("Options:");
					helptext.Add("  -?, -h, --help	Show this help");
					helptext.Add("  -dat=			Input DAT to rebuild against (REQUIRED)");
					helptext.Add("  -out=			Output directory");
					helptext.Add("  -t=, --temp=		Set the temporary directory to use");
					helptext.Add("  -qs, --quick		Enable quick scanning of archives");
					helptext.Add("  -v, --verify		Enable verification of output directory");
					helptext.Add("  -tgz, --tgz		Enable TorrentGZ output");
					helptext.Add("	-r, --romba		Enable Romba depot dir output");
					helptext.Add("  -do, --directory	Output files as uncompressed");
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
				case "TGZConvert":
					helptext.Add(Resources.Resources.TGZTest_Name + " - " + Resources.Resources.TGZTest_Desc);
					helptext.Add(barrier);
					helptext.Add(Resources.Resources.Usage + ": " + Resources.Resources.TGZTest_Name + " [options] [filename|dirname] ...");
					helptext.Add("");
					helptext.Add("Options:");
					helptext.Add("  -?, -h, --help	Show this help");
					helptext.Add("  -out=			Output directory");
					helptext.Add("  -t=, --temp=		Set the temporary directory to use");
					helptext.Add("  -d, --delete	Delete input files");
					helptext.Add("  -r, --romba	Enable Romba depot dir output");
					helptext.Add("  -7z={1}		Set scanning level for 7z archives");
					helptext.Add("  -gz={2}		Set scanning level for GZip archives");
					helptext.Add("  -rar={2}		Set scanning level for RAR archives");
					helptext.Add("  -zip={1}		Set scanning level for ZIP archives");
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
				if (!Console.IsOutputRedirected && i == Console.WindowHeight - 2)
				{
					i = 0;
					Console.WriteLine("Press enter to continue...");
					Console.ReadLine();
				}
			}
		}

		public static void Credits()
		{
			Console.WriteLine(@"-----------------------------------------
Credits
-----------------------------------------

Programmer / Lead:	Matt Nadareski (darksabre76)
Additional code:	emuLOAD, @tractivo
Testing:		emuLOAD, @tractivo, Kludge, Obiwantje, edc
Suggestions:		edc, AcidX, Amiga12, EliUmniCk
Based on work by:	The Wizard of DATz");
		}
	}
}
