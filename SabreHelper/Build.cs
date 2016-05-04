using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SabreTools.Helper
{
	public class Build
	{
		/// <summary>
		/// The current toolset version to be used by all child applications
		/// </summary>
		public static string Version
		{
			get { return "0.7.0.0"; }
		}

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
			string mid = name + " " + Build.Version;
			mid = "|" + mid.PadLeft(((77 - mid.Length) / 2) + mid.Length).PadRight(77) + "|";

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

			Console.Title = "SabreTools-" + name + " " + Build.Version;

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
				case "DATabase":
					Console.Write(@"
DATabase - Import and Generate DAT files
-----------------------------------------
Usage: DATabase [option] [filename|dirname] ...

Options:
  -?, -h, --help	Show this help
  -a, --add		Add a new system or source to the database
	manu=			Manufacturer name (system only)
	system=			System name (system only)
	source=			Source name (source only)
	url=			URL (source only)
  -cc, --convert-cmp	Convert an XML DAT to CMP
	out=			Output directory
  -cm, --convert-miss	Convert from DAT to miss
	-r, --roms		Output roms to miss instead of sets
	-gp, --game-prefix	Add game name as a prefix to each item
	-pre=, --prefix=	Set prefix to be printed in front of all lines
	-post=, --postfix=	Set postfix to be printed behind all lines
	-q, --quotes		Put double-quotes around each item
	-ae=, --add-ext=	Add an extension to each item
	-re=, --rep-ext=	Replace all extensions with specified
  -cx, --convert-xml	Convert a CMP DAT to XML
	out=			Output directory
  -es, --ext-split		Split a DAT by two file extensions
	exta=			First extension to split by
	extb=			Second extension to split by
	out=			Output directory
  -g, --generate	Start tool in generate mode
	system=			Comma-separated list of system IDs
	source=			Comma-separated list of source IDs
	out=			Output directory
	-nr, --no-rename	Don't auto-rename games
	-o, --old		Output DAT in CMP format instead of XML
  -ga, --generate-all	Start tool in generate all mode
	out=			Output directory
	-nr, --no-rename	Don't auto-rename games
	-o, --old		Output DAT in CMP format instead of XML
  -i, --import		Start tool in import mode
  -l, --log		Enable logging of program output
  -lso, --list-sources	List all sources (id <= name)
  -lsy, --list-systems	List all systems (id <= name)
  -m, --merge		Merge one or more DATs
	-ad, --all-diff	Enable output of all diff variants
	-di, --diff		Switch to diffdat mode
	-dd, --dedup		Enable deduping in the created DAT
	-b, --bare		Don't include date in file name
	-u, --unzip		Force unzipping in created DAT
	-o, --old		Output DAT in CMP format instead of XML
	-n=, --name=		Set the name of the DAT
	-d=, --desc=		Set the description of the DAT
	-c=, --cat=		Set the category of the DAT
	-v=, --version=		Set the version of the DAT
	-au=, --author=		Set the author of the DAT
  -rm, --remove		Remove a system or source from the database
	system=			System ID
	source=			Source ID
  -tm, --trim-merge	Consolidate DAT into a single game and trim entries
	-rd=, --root-dir=	Set the root directory for trimming calculation
	-nr, --no-rename	Keep game names instead of using '!'
	-df, --disable-force	Disable forceunzipping

Filenames and directories can't start with a reserved string
unless prefixed by 'input='
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
				case "DATFromDir":
					Console.WriteLine(@"DATFromDir - Create a DAT file from a directory
-----------------------------------------
Usage: DATFromDir [options] [filename|dirname] <filename|dirname> ...

Options:
  -h, -?, --help	Show this help dialog
  -m, --noMD5		Don't include MD5 in output
  -s, --noSHA1		Don't include SHA1 in output
  -b, --bare		Don't include date in file name
  -u, --unzip		Force unzipping in created DAT
  -f, --files		Treat archives as files
  -o, --old		Output DAT in CMP format instead of XML
  -n=, --name=		Set the name of the DAT
  -d=, --desc=		Set the description of the DAT
  -c=, --cat=		Set the category of the DAT
  -v=, --version=	Set the version of the DAT
  -au=, --author=	Set the author of the DAT
  -l, --log		Enable log to file
  -sd, --superdat	Enable SuperDAT creation");
					break;
				case "OfflineMerge":
					Console.WriteLine(@"OfflineMerge - Update DATS for offline arrays
-----------------------------------------
Usage: OfflineMerge [options] [inputs]

Options:
  -h, -?, --help	Show this help dialog
  -f, --fake		Replace all hashes and sizes by the default

Inputs:
  com=			Complete current DAT
  fix=			Complete current Missing
  new=			New Complete DAT

This program will output the following DATs:
  (a) Net New - (NewComplete)-(Complete)
  (b) Unneeded - (Complete)-(NewComplete)
  (c) New Missing - (Net New)+(Missing-(Unneeded))
  (d) Have - (NewComplete)-(New Missing)
        OR (Complete or NewComplete)-(Missing) if one is missing");
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
Testing:		emuLOAD, @tractivo, Kludge, Obiwantje
Suggestions:	edc, AcidX
Based on work by:	The Wizard of DATz");
		}
	}
}
