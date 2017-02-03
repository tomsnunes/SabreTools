using System;
using System.Collections.Generic;

namespace SabreTools.Helper.Help
{
	public class Help
	{
		#region Private variables

		private List<string> _header;
		private Dictionary<string, Feature> _features;

		#endregion

		#region Constructors

		public Help()
		{
			_header = new List<string>();
			_features = new Dictionary<string, Feature>();
		}

		public Help(List<string> header)
		{
			_header = header;
			_features = new Dictionary<string, Feature>();
		}

		#endregion

		#region Accessors

		public Feature this[string name]
		{
			get { return _features[name]; }
			set { _features[name] = value; }
		}

		/// <summary>
		/// Add a new feature to the help
		/// </summary>
		/// <param name="name">Name of the feature to add</param>
		/// <param name="feature">Feature object to map to</param>
		public void Add(string name, Feature feature)
		{
			if (_features == null)
			{
				_features = new Dictionary<string, Feature>();
			}

			lock (_features)
			{
				_features.Add(name, feature);
			}
		}

		#endregion

		#region Instance Methods

		/// <summary>
		/// Check if a flag is a top-level (main application) flag
		/// </summary>
		/// <param name="flag">Name of the flag to check</param>
		/// <returns>True if the feature was found, false otherwise</returns>
		public bool TopLevelFlag(string flag)
		{
			bool success = false;

			// Loop through the features and check
			foreach (string feature in _features.Keys)
			{
				if (_features[feature].ValidateInput(flag, exact: true))
				{
					success = true;
					break;
				}
			}

			return success;
		}

		/// <summary>
		/// Output top-level features only
		/// </summary>
		public void OutputGenericHelp()
		{
			// Start building the output list
			List<string> output = new List<string>();

			// Append the header first
			output.AddRange(_header);

			// Now append all available top-level flags
			output.Add("Available options:");
			foreach (string feature in _features.Keys)
			{
				output.AddRange(_features[feature].Output(pre: 2, midpoint: 25));
			}

			// And append the generic ending
			output.Add("");
			output.Add("For information on available flags, put the option name after help");

			// Now write out everything in a staged manner
			WriteOutWithPauses(output);
		}

		/// <summary>
		/// Output all features recursively
		/// </summary>
		public void OutputAllHelp()
		{
			// Start building the output list
			List<string> output = new List<string>();

			// Append the header first
			output.AddRange(_header);

			// Now append all available flags recursively
			output.Add("Available options:");
			foreach (string feature in _features.Keys)
			{
				output.AddRange(_features[feature].OutputRecursive(0, pre: 2, midpoint: 25));
			}

			// Now write out everything in a staged manner
			WriteOutWithPauses(output);
		}

		/// <summary>
		/// Output a single feature recursively
		/// </summary>
		public void OutputIndividualFeature(string featurename)
		{
			// Start building the output list
			List<string> output = new List<string>();

			// Now try to find the feature that has the name included
			string realname = null;
			List<string> startsWith = new List<string>();
			foreach (string feature in _features.Keys)
			{
				// If we have a match to the feature name somehow
				if (feature == featurename)
				{
					realname = feature;
					break;
				}

				// If we have a match within the flags
				else if (_features[feature].ContainsFlag(featurename))
				{
					realname = feature;
					break;
				}

				// Otherwise, we want to get features with the same start
				else if (_features[feature].StartsWith(featurename[0]))
				{
					startsWith.Add(feature);
				}
			}

			// If we have a real name found, append all available subflags recursively
			if (realname != null)
			{
				output.Add("Available options for " + realname + ":");
				output.AddRange(_features[realname].OutputRecursive(0, pre: 2, midpoint: 25));
			}

			// If no name was found but we have possible matches, show them
			else if (startsWith.Count > 0)
			{
				output.Add("\"" + featurename + "\" not found. Did you mean:");
				foreach (string possible in startsWith)
				{
					output.AddRange(_features[possible].Output(pre: 2, midpoint: 25));
				}
			}

			// Now write out everything in a staged manner
			WriteOutWithPauses(output);
		}

		/// <summary>
		/// Write out the help text with pauses, if needed
		/// </summary>
		/// <param name="helptext"></param>
		private void WriteOutWithPauses(List<string> helptext)
		{
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

		/*
		 * Here is a non-comprehensive list of things we want a help method to do:
		 * - Parse and return flags from arguments
		 * - Perform partial matching to find potentially similar features
		 */

		#endregion
	}
}
