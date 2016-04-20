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
			get { return "0.6.0.0"; }
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
  -cr, --convert-rv	Convert an XML DAT to RV
	out=			Output directory
  -cx, --convert-xml	Convert a RV DAT to XML
	out=			Output directory
  -es, --ext-split		Split a DAT by two file extensions
	exta=			First extension to split by
	extb=			Second extension to split by
	out=			Output directory
  -g, --generate	Start tool in generate mode
  -ga, --generate-all	Start tool in generate all mode
	system=			Comma-separated list of system IDs
	source=			Comma-separated list of source IDs
	out=			Output directory
	-nr, --no-rename	Don't auto-rename games
	-old, --romvault	Produce a DAT in RV format
  -i, --import		Start tool in import mode
  -l, --log		Enable logging of program output
  -lso, --list-sources	List all sources (id <= name)
  -lsy, --list-systems	List all systems (id <= name)
  -r, --remove	Remove a system or source from the database
	system=			System ID
	source=			Source ID
  -tm, --trim-merge	Consolidate DAT into a single game and trim entries
	-rd=, --root-dir=	Set the directory name for path size
	-nr, --no-rename	Disable single-game mode
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
  -o, --old		Output DAT in RV format instead of XML
  -n=, --name=		Set the name of the DAT
  -d=, --desc=		Set the description of the DAT
  -c=, --cat=		Set the category of the DAT
  -v=, --version=	Set the version of the DAT
  -a=, --author=	Set the author of the DAT
  -l, --log		Enable log to file
  -sd, --superdat	Enable SuperDAT creation");
					break;
				case "MergeDAT":
					Console.WriteLine(@"MergeDAT - Merge two or more DATs
-----------------------------------------
Usage: MergeDAT [options] [filename] ...

Options:
  -h, -?, --help	Show this help dialog
  -l, --log		Enable log to file
  -d, --diff		Switch to diffdat mode
  -m, --merge	Enable deduping in the created DAT");
					break;
				case "DatToMiss":
					Console.WriteLine(@"DatToMiss - Generate a miss file from a DAT
-----------------------------------------
Usage: DatToMiss [options] [filename]

Options:
  -h, -?, --help	Show this help dialog
  -l, --log		Enable log to file
  -r, --roms		Output roms to miss instead of sets
  -pre=, --prefix=	Set prefix to be printed in front of all lines
  -post=, --postfix=	Set postfix to be printed behind all lines
  -q, --quotes		Put double-quotes around each outputted item (not prefix/postfix)");
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
Based on work by:	The Wizard of DATz");
		}
	}
}
