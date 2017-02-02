using System;
using System.Collections.Generic;

namespace SabreTools.Helper.Data
{
	public static class Build
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
		/// <param name="name">The name to be displayed as the program</param>B
		public static void Start(string name)
		{
			// Dynamically create the header string, adapted from http://stackoverflow.com/questions/8200661/how-to-align-string-in-fixed-length-string
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

				Console.Title = name + " " + Constants.Version;

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
		/// <param name="className">Name of the class to get help for, "Credits" for developer credits</param>
		public static void Help(string className, string subset = null)
		{
			//http://stackoverflow.com/questions/14849367/how-to-determine-calling-method-and-class-name
			string barrier = "-----------------------------------------";
			List<string> helptext = new List<string>();

			// Normalize the subset text
			if (subset != null)
			{
				subset = subset.ToLowerInvariant().TrimStart('-');
			}

			// Set the help text
			switch (className)
			{
				case "Credits":
					helptext.Add(barrier);
					helptext.Add("Credits");
					helptext.Add(barrier);
					helptext.Add("");
					helptext.Add("Programmer / Lead:	Matt Nadareski (darksabre76)");
					helptext.Add("Additional code:	emuLOAD, @tractivo, motoschifo");
					helptext.Add("Testing:		emuLOAD, @tractivo, Kludge, Obiwantje, edc");
					helptext.Add("Suggestions:		edc, AcidX, Amiga12, EliUmniCk");
					helptext.Add("Based on work by:	The Wizard of DATz");
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
				if (i == Console.WindowHeight - 3)
				{
					i = 0;
					Pause();
				}
			}
			Pause();
		}

		/// <summary>
		/// Pause on console output
		/// </summary>
		private static void Pause()
		{
			if (!Console.IsOutputRedirected)
			{
				Console.WriteLine();
				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();
			}
		}
	}
}
