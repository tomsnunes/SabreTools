using System;
using System.IO;

namespace WoD.Helper
{
	public class Logging
	{
		// Private instance variables
		private bool _tofile;
		private string _filename;
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

		public Logging(bool tofile, string filename = "")
		{
			_tofile = tofile;
			_filename = filename;
		}

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
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool Close()
		{
			if (!_tofile)
			{
				return true;
			}

			try
            {
				_log.WriteLine("Logging ended " + DateTime.Now);
				_log.Close();
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool Log(string output)
		{
			// If we're writing to console, just write the string
			if (!_tofile)
			{
				Console.WriteLine(output);
				return true;
			}
			// If we're writing to file, use the existing stream
			try
			{
				_log.WriteLine(output);
			}
			catch
			{
				Console.WriteLine("Could not write to log file!");
				return false;
			}

			return true;
		}
	}
}
