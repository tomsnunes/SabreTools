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
			Console.SetBufferSize(Console.BufferWidth, 999);
			Console.Title = "SabreTools-" + name + " " + Build.Version;
			ConsoleColor formertext = Console.ForegroundColor;
			ConsoleColor formerback = Console.BackgroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Blue;

			// Output the header
			Console.WriteLine(border);
			Console.WriteLine(mid);
			Console.WriteLine(border);
			Console.WriteLine();

			// Return the console to the original text and background colors
			Console.ForegroundColor = formertext;
			Console.BackgroundColor = formerback;
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
					Console.Clear();
					Console.Write(@"
DATabase - Import and Generate DAT files
-----------------------------------------
Usage: DATabase [option] [filename|dirname] <filename|dirname> ...

Options:
  -?, -h, --help	Show this help
  -a, --add		Add a new system or source to the database
			  manu=mn		Manufacturer name (system only)
			  system=sy		System name (system only)
			  source=sr		Source name (source only)
			  url=ul		URL (source only)
  -cr, --convert-rv	Convert an XML DAT to RV
  -cx, --convert-xml	Convert a RV DAT to XML
			  Both converters require a filename or folder
  -g, --generate	Start tool in generate mode
  -ga, --generate-all	Start tool in generate all mode
			  system=sy,...		List of system IDs
			  source=so,...		List of source IDs
			  out=dir			Output directory
			  -nr, --no-rename	Don't auto-rename games
			  -old, --romvault	Produce a DAT in RV format
  -i, --import		Start tool in import mode
			  A filename or folder is required to run
  -l, --log		Enable logging of program output
  -lso, --list-sources	List all sources (id <= name)
  -lsy, --list-systems	List all systems (id <= name)
  -r, --remove	Remove a system or source from the database
			  system=sy		System ID
			  source=so			Source ID

Filenames and directories can't start with '-', 'system=', or 'source='
unless prefixed by 'input='
");
					Console.Write("\nPress any key to continue...");
					Console.ReadKey();
					break;
				case "DatSplit":
					Console.WriteLine(@"DatSplit - Split DAT files by file extension
-----------------------------------------
Usage: DatSplit.exe <filename> <ext> <ext>");
					break;
				case "Headerer":
					Console.WriteLine(@"Headerer - Remove and restore rom headers
-----------------------------------------
Usage: Headerer [option] [filename|dirname]

Options:
  -e			Detect and remove mode
  -r			Restore header to file based on SHA-1");
					break;
				case "SingleGame":
					Console.WriteLine(@"SingleGame - Process DATs for use in server environments
-----------------------------------------
Usage: SingleGame.exe [option] [filename|dirname]

Options:
    -r=rootdir		Set the directory name for path size
    -l, --log		Enable logging to file
    -n			Disable single-game mode
    -z			Disable forceunzipping
");
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
					Console.WriteLine(@"MergeDAT - Merge or diff two or more files
-----------------------------------------
Usage: DiffDat [options] [filename] [filename] ...

Options:
  -h, -?, --help	Show this help dialog
  -l, --log		Enable log to file
  -d, --diff		Enable diff creation
  -m, --merge	Enable merging in the created DAT");
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
  -post=, --postfix=	Set postfix to be printed behind all lines");
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
