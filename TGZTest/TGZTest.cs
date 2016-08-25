using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Helper;

namespace SabreTools
{
	public class TGZTest
	{
		// User-defined variables
		private List<string> _inputs;
		private string _outdir;
		private bool _delete;
		private Logger _logger;

		/// <summary>
		/// Create a new TGZTest object
		/// </summary>
		/// <param name="inputs">List of all inputted files and folders</param>
		/// <param name="outdir">Output directory (empty for default directory)</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		public TGZTest(List<string> inputs, string outdir, bool delete, Logger logger)
		{
			_inputs = inputs;
			_outdir = (String.IsNullOrEmpty(outdir) ? "tgz" : outdir);
			_delete = delete;
			_logger = logger;
		}

		/// <summary>
		/// Main entry point for the program
		/// </summary>
		/// <param name="args">List of arguments to be parsed</param>
		public static void Main(string[] args)
		{
			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Perform initial setup and verification
			Logger logger = new Logger(true, "tgztest.log");
			logger.Start();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("TGZTest");

			// Set all default values
			bool help = false,
				delete = false,
				tgz = true;
			string outdir = "";
			List<string> inputs = new List<string>();

			// Determine which switches are enabled (with values if necessary)
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
					case "-d":
					case "--delete":
						delete = true;
						break;
					default:
						if (temparg.StartsWith("-out=") || temparg.StartsWith("--out="))
						{
							outdir = temparg.Split('=')[1];
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
						}
						else
						{
							logger.Error("Invalid input detected: " + arg);
							Console.WriteLine();
							Build.Help();
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
			if (inputs.Count == 0 && tgz)
			{
				logger.Error("This feature requires at least one input");
				Build.Help();
				logger.Close();
				return;
			}

			// If we are doing a simple sort
			if (tgz)
			{
				InitTGZTest(inputs, outdir, delete, logger);
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
		/// Wrap sorting files using an input DAT
		/// </summary>
		/// <param name="inputs">List of all inputted files and folders</param>
		/// <param name="outdir">Output directory (empty for default directory)</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		public static bool InitTGZTest(List<string> inputs, string outdir, bool delete, Logger logger)
		{
			// Get all individual files from the inputs
			List<string> newinputs = new List<string>();
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					newinputs.Add(Path.GetFullPath(input));
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						newinputs.Add(Path.GetFullPath(input));
					}
				}
			}

			TGZTest tgztest = new TGZTest(newinputs, outdir, delete, logger);
			return tgztest.Process();
		}

		/// <summary>
		/// Process all input files
		/// </summary>
		/// <returns>True if processing was a success, false otherwise</returns>
		public bool Process()
		{
			foreach (string input in _inputs)
			{
				ArchiveTools.WriteTorrentGZ(input, _outdir, _logger);
				if (_delete)
				{
					try
					{
						File.Delete(input);
					}
					catch (Exception ex)
					{
						_logger.Error(ex.ToString());
					}
				}
			}

			return true;
		}
	}
}
