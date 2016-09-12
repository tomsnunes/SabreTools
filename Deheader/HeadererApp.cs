using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the Deheader application
	/// </summary>
	public class HeadererApp
	{
		// Private required variables
		private static string _headererDbSchema = "Headerer";
		private static string _headererDbName = "Headerer.sqlite";
		private static string _headererConnectionString = "Data Source=" + _headererDbName + ";Version = 3;";

		/// <summary>
		/// Start deheader operation with supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Perform initial setup and verification
			Logger logger = new Logger(true, "headerer.log");
			logger.Start();
			DBTools.EnsureDatabase(_headererDbSchema, _headererDbName, _headererConnectionString);

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			// If we have no arguments, show the help
			if (args.Length == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("Headerer");

			// Get the filename (or foldername)
			string input = "";
			bool help = false,
				extract = true,
				headerer = true;
			foreach (string arg in args)
			{
				string temparg = arg.Replace("\"", "").Replace("file://", "");
				switch (temparg)
				{
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-e":
					case "--extract":
						extract = true;
						break;
					case "-r":
					case "-re":
					case "--restore":
						extract = false;
						break;
					default:
						if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							input = temparg;
						}
						else
						{
							logger.Error("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
							Console.WriteLine();
							logger.Error("Invalid input detected: " + arg);
							logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (String.IsNullOrEmpty(input) && (headerer))
			{
				logger.Error("This feature requires at least one input");
				Build.Help();
				logger.Close();
				return;
			}

			// If we're in headerer mode
			if (headerer)
			{
				InitHeaderer(input, extract, logger);
			}

			// If nothing is set, show the help
			else
			{
				Build.Help();
			}

			logger.Close();
			return;
		}

		/// <summary>
		/// Wrap extracting and replacing headers
		/// </summary>
		/// <param name="input">Input file or folder name</param>
		/// <param name="flag">True if we're extracting headers (default), false if we're replacing them</param>
		/// <param name="logger">Logger object for file and console output</param>
		private static void InitHeaderer(string input, bool flag, Logger logger)
		{
			Headerer headerer = new Headerer(input, flag, logger);
			headerer.Process();
		}
	}
}
