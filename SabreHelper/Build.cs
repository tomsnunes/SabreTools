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
Usage: DATabase [option] [filename|dirname|<system=sy,...> <source=so,...>]

Options:
  -h, -?, --help	Show this help
  -i, --import		Start tool in import mode
			  A filename or folder is required to run
  -g, --generate	Start tool in generate mode
			  system=sy,...		List of system IDs
			  source=so,...		List of source IDs
			  out=dir			Output directory
			  -nr, --no-rename	Don't auto-rename games
			  -old, --romvault	Produce a DAT in RV format
  -ga, --generate-all	Start tool in generate all mode
  -lso, --list-sources	List all sources (id <= name)
  -lsy, --list-systems	List all systems (id <= name)
  -cr, --convert-rv	Convert an XML DAT to RV
  -cx, --convert-xml	Convert a RV DAT to XML
			  Both converters require a filename or folder
  -l, --log		Enable logging of program output
");

					Console.Write("\nPress any key to continue...");
					Console.ReadKey();
					Console.Write(@"
Database Options:
  -a, --add		Add a new system or source to the database
			  manu=mn		Manufacturer name (system only)
			  system=sy		System name (system only)
			  source=sr		Source name (source only)
			  url=ul		URL (source only)
  -r, --remove	Remove a system or source from the database
			  system=sy		System ID
			  source=so			Source ID

Filenames and directories can't start with '-', 'system=', or 'source='
unless prefixed by 'input='");
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
Usage: SingleGame.exe <filename> [-r=rootdir|-n]

Options:
    -r=rootdir		Set the directory name for path size
    -n			Disable single-game mode
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
  -a=, --author=	Set the author of the DAT");
					break;
				default:
					Console.Write("This is the default help output");
					break;
			}
		}
	}
}
