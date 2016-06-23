using System;
using System.IO;

namespace SabreTools.Helper
{
	/// <summary>
	/// Log either to file or to the console
	/// </summary>
	/// <remarks>
	/// Things to do:
	/// - Allow for "triggerable" logging done on an interval (async)
	/// - Log filtering? (#if debug?)
	/// </remarks>
	public class Logger
	{
		// Private instance variables
		private bool _tofile;
		private string _filename;
		private DateTime _start;
		private StreamWriter _log;

		// Private required variables
		private string _basepath = "Logs" + Path.DirectorySeparatorChar;

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
		/// <param name="filename">Filename representing log location</param>
		public Logger(bool tofile, string filename)
		{
			_tofile = tofile;
			_filename = Path.GetFileNameWithoutExtension(filename) + " (" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ")" + Path.GetExtension(filename);

			if (!Directory.Exists(_basepath))
			{
				Directory.CreateDirectory(_basepath);
			}
		}

		/// <summary>
		/// Start logging by opening output file (if necessary)
		/// </summary>
		/// <returns>True if the logging was started correctly, false otherwise</returns>
		public bool Start()
		{
			_start = DateTime.Now;
			if (!_tofile)
			{
				return true;
			}

			try
			{
				_log = new StreamWriter(File.Open(_basepath + _filename, FileMode.OpenOrCreate | FileMode.Append));
				_log.WriteLine("Logging started " + DateTime.Now);
				_log.Flush();
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
		/// <param name="suppress">True if all ending output is to be suppressed, false otherwise (default)</param>
		/// <returns>True if the logging was ended correctly, false otherwise</returns>
		public bool Close(bool suppress = false)
		{
			if (!suppress)
			{
				TimeSpan span = DateTime.Now.Subtract(_start);
				string total = span.ToString(@"hh\:mm\:ss\.fffff");
				if (!_tofile)
				{
					Console.WriteLine("Total runtime: " + total);
					return true;
				}

				try
				{
					_log.WriteLine("Logging ended " + DateTime.Now);
					_log.WriteLine("Total runtime: " + total);
					Console.WriteLine("Total runtime: " + total);
					_log.Close();
				}
				catch
				{
					return false;
				}
			}
			else
			{
				try
				{
					_log.Close();
				}
				catch
				{
					return false;
				}
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
			// USER and ERROR writes to console
			if (loglevel == LogLevel.USER || loglevel == LogLevel.ERROR)
			{
				Console.WriteLine((loglevel == LogLevel.ERROR ? loglevel.ToString() + " " : "") + output);
			}

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
		/// Write the given exact string to the log output
		/// </summary>
		/// <param name="output">String to be written log</param>
		/// <param name="loglevel">Severity of the information being logged</param>
		/// <returns>True if the output could be written, false otherwise</returns>
		public bool LogExact(string output)
		{
			Console.Write(output);

			// If we're writing to file, use the existing stream
			if (_tofile)
			{
				try
				{
					_log.Write(DateTime.Now + " - " + output);
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
		/// Write the given string as a user message to the log output
		/// </summary>
		/// <param name="output">String to be written log</param>
		/// <returns>True if the output could be written, false otherwise</returns>
		public bool User(string output)
		{
			return Log(output, LogLevel.USER);
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
