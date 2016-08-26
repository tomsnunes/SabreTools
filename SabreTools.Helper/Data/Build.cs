using System;
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
			string border = "+-----------------------------------------------------------------------------+";
			string mid = name + " " + Constants.Version;
			mid = "|" + mid.PadLeft(((77 - mid.Length) / 2) + mid.Length).PadRight(77) + "|";

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

			switch (className)
			{
				case "SabreTools":
					Console.Write(@"
SabreTools - Import, generate, manipulate DAT files
-----------------------------------------
Usage: SabreTools [option] [filename|dirname] ...

Options:
  -?, -h, --help	Show this help
  -a, --add		Add a new system or source to the database
	-manu=			Manufacturer name (system only)
	-system=		System name (system only)
	-source=		Source name (source only)
	-url=			URL (source only)
  -d, --dfd		Enable Dir2DAT mode
	-nm, --noMD5		Don't include MD5 in output
	-ns, --noSHA1		Don't include SHA1 in output
	-b, --bare		Don't include date in file name
	-u, --unzip		Force unzipping in created DAT
	-f, --files		Treat archives as files
	-o, --old		Output DAT in CMP format instead of XML
	-gz, --gz-files		Allow reading of GZIP files as archives
	-ro, --romba		Read files from a Romba input
	-f=, --filename=	Set the external name of the DAT
	-n=, --name=		Set the internal name of the DAT
	-de=, --desc=		Set the description of the DAT
	-c=, --cat=		Set the category of the DAT
	-v=, --version=		Set the version of the DAT
	-au=, --author=		Set the author of the DAT
	-sd, --superdat		Enable SuperDAT creation
	-t=, --temp=		Set the temporary directory to use
  -es, --ext-split	Split a DAT by two file extensions
	-exta=			First set of extensions (comma-separated)
	-extb=			Second set of extensions (comma-separated)
	-out=			Output directory
  -g, --generate	Start tool in generate mode
	-system=		System ID to generate from
	-nr, --no-rename	Don't auto-rename games
	-o, --old		Output DAT in CMP format instead of XML
  -ga, --generate-all	Start tool in generate all mode
	-nr, --no-rename	Don't auto-rename games
	-o, --old		Output DAT in CMP format instead of XML
  -hs, --hash-split		Split a DAT or folder by best-available hashes
	-out=			Output directory
  -i, --import		Start tool in import mode
	-ig, --ignore		Don't prompt for new sources
  -lso, --list-sources	List all sources (id <= name)
  -lsy, --list-systems	List all systems (id <= name)
  -ol, --offmerge	Update DATS for offline arrays (see notes)
	-com=			Complete current DAT
	-fix=			Complete current Missing
	-new=			New Complete DAT
	-fk, --fake		Replace all hashes and sizes by the default
  -rm, --remove		Remove a system or source from the database
	-system=		System ID
	-source=		Source ID
  -st, --stats		Get statistics on all input DATs
	-si, --single		Show individual statistics
  -ud, --update		Update a DAT file
	-oc, --output-cmp	Output in CMP format
	-om, --output-miss	Output in Missfile format
	  -r, --roms			Output roms to miss instead of sets
	  -gp, --game-prefix		Add game name as a prefix
	  -pre=, --prefix=		Set prefix for all lines
	  -post=, --postfix=		Set postfix for all lines
	  -q, --quotes			Put double-quotes around each item
	  -ae=, --add-ext=		Add an extension to each item
	  -re=, --rep-ext=		Replace all extensions with specified
	  -ro, --romba			Output in Romba format (requires SHA-1)
	  -tsv, --tsv			Output in Tab-Separated Value format
	  -csv, --csv			Output in Comma-Separated Value format
	-or, --output-rc	Output in RomCenter format
	-os, --output-sd	Output in SabreDAT format
	-ox, --output-xml	Output in Logiqx XML format
	-f=, --filename=	Set a new filename
	-n=, --name=		Set a new internal name
	-de=, --desc=		Set a new description
	-r=, --root=		Set a new rootdir
	-ca=, --category=	Set a new category
	-v=, --version=		Set a new version
	-da=, --date=		Set a new date
	-au=, --author=		Set a new author
	-em=, --email=		Set a new email
	-hp=, --homepage=	Set a new homepage
	-u=, --url=		Set a new URL
	-co=, --comment=	Set a new comment
	-h=, --header=		Set a new header skipper
	-sd=, --superdat	Set SuperDAT type
	-fm=, --forcemerge=	Set force merging
		Supported values are:
		  None, Split, Full
	-fn=, --forcend=	Set force nodump
		Supported values are:
		  None, Obsolete, Required, Ignore
	-fp=, --forcepack=	Set force packing
		Supported values are:
		  None, Zip, Unzip
	-clean			Clean game names according to WoD standards
	-sl, --softlist	Use Software List name instead of description
	-trim			Trim file names to fit NTFS length
		-rd=, --root-dir=	Set the root directory for calc
	-si, --single		All game names replaced by '!'
	-dd, --dedup		Enable deduping in the created DAT
	-m, --merge		Merge the input DATs
		-b, --bare		Don't include date in automatic name
	-di, --diff		Create diffdats from inputs (all outputs)
		-b, --bare		Don't include date in automatic name
		-c, --cascade		Enable cascaded diffing
			-ip, --inplace		Enable inplace, cascaded diff
			-sf, --skip		Skip output of first DAT
		-rc, --rev-cascade	Enable reverse cascaded diffing
			-ip, --inplace		Enable inplace, cascaded diff
			-sf, --skip		Skip output of first DAT
	-did, --diff-du	Create diffdat containing just duplicates
		[Can be used with other -diX flags]
		-b, --bare		Don't include date in automatic name
	-dii, --diff-in	Create diffdats for individual DATs
		[Can be used with other -diX flags]
		-b, --bare		Don't include date in automatic name
	-din, --diff-nd	Create diffdat containing no duplicates
		[Can be used with other -diX flags]
		-b, --bare		Don't include date in automatic name
	-gn=, --game-name=	Filter by game name
	-rn=, --rom-name=	Filter by rom name
	-rt=, --rom-type=	Filter by rom type
	-sgt=, --greater=	Filter by size >=
	-slt=, --less=		Filter by size <=
	-seq=, --equal=		Filter by size ==
	-crc=, --crc=		Filter by CRC hash
	-md5=, --md5=		Filter by MD5 hash
	-sha1=, --sha1=		Filter by SHA-1 hash
	-nd, --nodump		Include only match nodump roms
	-nnd, --not-nodump	Exclude all nodump roms
	-out=			Output directory (overridden by --inplace)

Filenames and directories can't start with a reserved string
unless prefixed by 'input='

Filter parameters game name, rom name, CRC, MD5, SHA-1 can
do partial matches using asterisks as follows (case insensitive):
    *00 means ends with '00'
    00* means starts with '00'
    *00* means contains '00'
    00 means exactly equals '00'

Filter parameters for size can use postfixes for inputs:
    e.g. 8kb => 8000 or 8kib => 8192

Offline merge mode notes:
  This program will output the following DATs:
    (a) Net New - (NewComplete)-(Complete)
    (b) Unneeded - (Complete)-(NewComplete)
    (c) New Missing - (Net New)+(Missing-(Unneeded))
    (d) Have - (NewComplete)-(New Missing)
      OR (Complete or NewComplete)-(Missing) if one is missing
");
					break;
				case "Headerer":
					Console.WriteLine(@"Headerer - Remove and restore rom headers
-----------------------------------------
Usage: Headerer [option] [filename|dirname]

Options:
  -e			Detect and remove mode
  -r			Restore header to file based on SHA-1");
					break;
				case "SimpleSort":
					Console.WriteLine(@"SimpleSort - Basic rebuild using a DAT
-----------------------------------------
Usage: SimpleSort [options] [filename|dirname] ...

Options:
  -?, -h, --help	Show this help
  -dat=			Input DAT to rebuild against (REQUIRED)
  -out=			Output directory
  -t=, --temp=		Set the temporary directory to use
  -qs, --quick		Enable quick scanning of archives
  -v, --verify		Enable verification of output directory
  -tgz, --tgz		Enable TorrentGZ output
	-r, --romba			Enable Romba depot dir output
  -do, --directory	Output files as uncompressed
  -7z={0}		Set scanning level for 7z archives
  -gz={2}		Set scanning level for GZip archives
  -rar={2}		Set scanning level for RAR archives
  -zip={0}		Set scanning level for ZIP archives

Archive scanning levels:
  0	Hash archive and contents
  1	Only hash contents
  2	Only hash archive
");
					break;
				case "TGZTest":
					Console.WriteLine(@"TGZTest - Test TorrentGZ output
-----------------------------------------
Usage: TGZTest [options] [filename|dirname] ...

Options:
  -?, -h, --help	Show this help
  -out=			Output directory
  -t=, --temp=		Set the temporary directory to use
  -d, --delete	Delete input files
  -r, --romba	Enable Romba depot dir output
  -7z={1}		Set scanning level for 7z archives
  -gz={2}		Set scanning level for GZip archives
  -rar={2}		Set scanning level for RAR archives
  -zip={1}		Set scanning level for ZIP archives

Archive scanning levels:
  0	Hash archive and contents
  1	Only hash contents
  2	Only hash archive");
                    break;
				default:
					Console.Write("This is the default help output");
					break;
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
