using System;
using System.Collections.Generic;

using SabreTools.Helper.Data;

namespace SabreTools.Helper.Help
{
	public class Feature
	{
		#region Private variables

		private List<string> _flags;
		private string _description;
		private FeatureType _featureType;
		private Dictionary<string, Feature> _features;
		private List<string> _additionalNotes;
		private bool _foundOnce = false;

		#endregion

		#region Constructors

		public Feature()
		{
			_flags = new List<string>();
			_description = null;
			_featureType = FeatureType.Flag;
			_features = new Dictionary<string, Feature>();
			_additionalNotes = new List<string>();
		}

		public Feature(string flag, string description, FeatureType featureType, List<string> additionalNotes)
		{
			List<string> flags = new List<string>();
			flags.Add(flag);
			_flags = flags;
			_description = description;
			_featureType = featureType;
			_features = new Dictionary<string, Feature>();
			_additionalNotes = additionalNotes;
		}

		public Feature(List<string> flags, string description, FeatureType featureType, List<string> additionalNotes)
		{
			_flags = flags;
			_description = description;
			_featureType = featureType;
			_features = new Dictionary<string, Feature>();
			_additionalNotes = additionalNotes;
		}

		#endregion

		#region Accessors

		/// <summary>
		/// Directly address a given subfeature
		/// </summary>
		public Feature this[string name]
		{
			get { return _features[name]; }
			set { _features[name] = value; }
		}

		/// <summary>
		/// Add a new feature for this feature
		/// </summary>
		/// <param name="name">Name of the feature to add</param>
		/// <param name="feature"></param>
		public void AddFeature(string name, Feature feature)
		{
			if (_features == null)
			{
				_features = new Dictionary<string, Feature>();
			}

			lock(_features)
			{
				if (!_features.ContainsKey(name))
				{
					_features.Add(name, feature);
				}
				else
				{
					_features[name] = feature;
				}
			}
		}

		/// <summary>
		/// Add a new flag for this feature
		/// </summary>
		/// <param name="flag">Flag to add for this feature</param>
		public void AddFlag(string flag)
		{
			if (_flags == null)
			{
				_flags = new List<string>();
			}

			lock (_flags)
			{
				_flags.Add(flag);
			}
		}

		/// <summary>
		/// Add a set of new flags for this feature
		/// </summary>
		/// <param name="flags">List of flags to add to this feature</param>
		public void AddFlags(List<string> flags)
		{
			if (_flags == null)
			{
				_flags = new List<string>();
			}

			lock (_flags)
			{
				_flags.AddRange(flags);
			}
		}

		/// <summary>
		/// Add a new additional note to this feature
		/// </summary>
		/// <param name="note">Note to add for this feature</param>
		public void AddNote(string note)
		{
			if (_additionalNotes == null)
			{
				_additionalNotes = new List<string>();
			}

			lock (_additionalNotes)
			{
				_additionalNotes.Add(note);
			}
		}

		/// <summary>
		/// Add a set of new notes for this feature
		/// </summary>
		/// <param name="notes">List of notes to add to this feature</param>
		public void AddNotes(List<string> notes)
		{
			if (_additionalNotes == null)
			{
				_additionalNotes = new List<string>();
			}

			lock (_additionalNotes)
			{
				_additionalNotes.AddRange(notes);
			}
		}

		/// <summary>
		/// Returns if a flag exists for the current feature
		/// </summary>
		/// <param name="name">Name of the flag to check</param>
		/// <returns>True if the flag was found, false otherwise</returns>
		public bool ContainsFlag(string name)
		{
			bool success = false;

			// Loop through the flags
			foreach (string flag in _flags)
			{
				if (flag == name)
				{
					success = true;
					break;
				}
				else if (flag.TrimStart('-') == name)
				{
					success = true;
					break;
				}
			}

			return success;
		}

		/// <summary>
		/// Returns if the feature contains a flag that starts with the given character
		/// </summary>
		/// <param name="c">Character to check against</param>
		/// <returns>True if the flag was found, false otherwise</returns>
		public bool StartsWith(char c)
		{
			bool success = false;

			// Loop through the flags
			foreach (string flag in _flags)
			{
				if (flag.TrimStart('-').ToLowerInvariant()[0] == c)
				{
					success = true;
					break;
				}
			}

			return success;
		}

		#endregion

		#region Instance Methods

		/// <summary>
		/// Output this feature only
		/// </summary>
		/// <param name="pre">Positive number representing number of spaces to put in front of the feature</param>
		/// <param name="midpoint">Positive number representing the column where the description should start</param>
		public List<string> Output(int pre = 0, int midpoint = 0)
		{
			// Create the output list
			List<string> outputList = new List<string>();

			// Build the output string first
			string output = "";

			// Add the pre-space first
			for (int i = 0; i < pre; i++)
			{
				output += " ";
			}

			// Now add all flags
			output += String.Join(", ", _flags);

			// If we have a midpoint set, check to see if the string needs padding
			if (midpoint > 0 && output.Length < midpoint)
			{
				while (output.Length < midpoint)
				{
					output += " ";
				}
			}
			else
			{
				output += " ";
			}

			// Append the description
			output += _description;

			// Now append it to the list
			outputList.Add(output);

			return outputList;
		}

		/// <summary>
		/// Output this feature and all subfeatures
		/// </summary>
		/// <param name="tabLevel">Level of indentation for this feature</param>
		/// <param name="pre">Positive number representing number of spaces to put in front of the feature</param>
		/// <param name="midpoint">Positive number representing the column where the description should start</param>
		public List<string> OutputRecursive(int tabLevel, int pre = 0, int midpoint = 0)
		{
			// Create the output list
			List<string> outputList = new List<string>();

			// Build the output string first
			string output = "";

			// Normalize based on the tab level
			int preAdjusted = pre;
			int midpointAdjusted = midpoint;
			if (tabLevel > 0)
			{
				preAdjusted += 4 * tabLevel;
				midpointAdjusted += 4 * tabLevel;
			}

			// Add the pre-space first
			for (int i = 0; i < preAdjusted; i++)
			{
				output += " ";
			}

			// Now add all flags
			output += String.Join(", ", _flags);

			// If we have a midpoint set, check to see if the string needs padding
			if (midpoint > 0 && output.Length < midpointAdjusted)
			{
				while (output.Length < midpointAdjusted)
				{
					output += " ";
				}
			}
			else
			{
				output += " ";
			}

			// Append the description
			output += _description;

			// Now append it to the list
			outputList.Add(output);

			// Now let's append all subfeatures
			foreach (string feature in _features.Keys)
			{
				outputList.AddRange(_features[feature].OutputRecursive(tabLevel + 1, pre, midpoint));
			}

			return outputList;
		}

		/// <summary>
		/// Validate whether a flag is valid for this feature or not
		/// </summary>
		/// <param name="input">Input to check against</param>
		/// <param name="exact">True if just this feature should be checked, false if all subfeatures are checked as well</param>
		/// <returns>True if the flag was valid, false otherwise</returns>
		public bool ValidateInput(string input, bool exact = false)
		{
			bool valid = false;

			// Determine what we should be looking for
			switch (_featureType)
			{
				// If we have a flag, make sure it doesn't have an equal sign in it 
				case FeatureType.Flag:
					valid = !input.Contains("=") && _flags.Contains(input);
					break;

				// If we have an input, make sure it has an equals sign in it
				case FeatureType.List:
				case FeatureType.String:
					valid = input.Contains("=") && _flags.Contains(input.Split('=')[0]);
					break;
			}

			// If we haven't found a valid flag and we're not looking for just this feature, check to see if any of the subfeatures are valid
			if (!valid && !exact)
			{
				foreach (string feature in _features.Keys)
				{
					valid = _features[feature].ValidateInput(input);

					// If we've found a valid feature, we break out
					if (valid)
					{
						break;
					}
				}
			}

			// If we've already found this flag before and we don't allow duplicates, set valid to false
			if (valid && _foundOnce && _featureType != FeatureType.List)
			{
				valid = false;
			}

			return valid;
		}

		#endregion
	}
}
