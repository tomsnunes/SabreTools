namespace SabreTools.Helper.Data
{
	public class Globals
	{
		#region Private implementations

		private static Logger _logger = null;
		private static int _maxDegreeOfParallelism = 4;

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
			get { return _maxDegreeOfParallelism; }
			set { _maxDegreeOfParallelism = value; }
		}

		#endregion
	}
}
