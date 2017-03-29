using System.Reflection;
using System.Threading.Tasks;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

namespace SabreTools.Helper.Data
{
	public class Globals
	{
		#region Private implementations

		private static Logger _logger = null;
		private static int _maxDegreeOfParallelism = 4;
		private static string _exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");

		#endregion

		#region Public accessors

		public static Logger Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = new Logger();
				}
				return _logger;
			}
			set { _logger = value; }
		}
		public static int MaxDegreeOfParallelism
		{
			set { _maxDegreeOfParallelism = value; }
		}
		public static ParallelOptions ParallelOptions
		{
			get
			{
				return new ParallelOptions()
				{
					MaxDegreeOfParallelism = _maxDegreeOfParallelism
				};
			}
		}
		public static string ExeDir
		{
			get
			{
				return _exeDir;
			}
		}

		#endregion
	}
}
