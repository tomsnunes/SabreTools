using System;
using System.IO;

namespace SabreTools.Helper
{
	/// <summary>
	/// Log either to file or to the console
	/// </summary>
	public class Logger
	{
		// Private instance variables
		private bool _tofile;
		private string _filename;
		private DateTime _start;
		private StreamWriter _log;

		// Public wrappers
		public bool ToFile
		{
			get { return _tofile; }
			set
			{
				if (!value)
				{
					Close();
				}
				_tofile = value;
				if (_tofile)
				{
					Start();
				}
			}
		}

		/// <summary>
		/// Initialize a Logger object with the given information
		/// </summary>
		/// <param name="tofile">True if file should be written to instead of console</param>
		/// <param name="filename">Optional filename representing log location</param>
		public Logger(bool tofile, string filename = "")
		{
			_tofile = tofile;
			_filename = filename;
		}

		/// <summary>
		/// Start logging by opening output file (if necessary)
		/// </summary>
		/// <returns>True if the logging was started correctly, false otherwise</returns>
		public bool Start()
		{
			if (!_tofile)
			{
				return true;
			}

			try
			{
				_log = new StreamWriter(File.Open(_filename, FileMode.OpenOrCreate | FileMode.Append));
				_log.WriteLine("Logging started " + DateTime.Now);
				_start = DateTime.Now;
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// End logging by closing output file (if necessary)
		/// </summary>
		/// <returns>True if the logging was ended correctly, false otherwise</returns>
		public bool Close()
		{
			TimeSpan elapsed = DateTime.Now - _start;
			if (!_tofile)
			{
				Console.WriteLine("Total runtime: " + elapsed);
				return true;
			}

			try
			{
				_log.WriteLine("Logging ended " + DateTime.Now);
				_log.WriteLine("Total runtime: " + elapsed.TotalMinutes);
				Console.WriteLine("Total runtime: " + elapsed);
				_log.Close();
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write the given string to the log output
		/// </summary>
		/// <param name="output">String to be written log</param>
		/// <param name="loglevel">Severity of the information being logged</param>
		/// <returns>True if the output could be written, false otherwise</returns>
		public bool Log(string output, LogLevel loglevel = LogLevel.VERBOSE)
		{
			// Everything writes to console
			Console.WriteLine(loglevel.ToString() + " " + output);

			// If we're writing to file, use the existing stream
			if (_tofile)
			{
				try
				{
					_log.WriteLine(loglevel.ToString() + " - " + DateTime.Now  + " - " + output);
					_log.Flush();
				}
				catch
				{
					Console.WriteLine("Could not write to log file!");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Write the given string as a warning to the log output
		/// </summary>
		/// <param name="output">String to be written log</param>
		/// <returns>True if the output could be written, false otherwise</returns>
		public bool Warning(string output)
		{
			return Log(output, LogLevel.WARNING);
		}

		/// <summary>
		/// Writes the given string as an error in the log
		/// </summary>
		/// <param name="output">String to be written log</param>
		/// <returns>True if the output could be written, false otherwise</returns>
		public bool Error(string output)
		{
			return Log(output, LogLevel.ERROR);
		}
	}
}
