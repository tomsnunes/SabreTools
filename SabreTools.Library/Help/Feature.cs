using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Library.Data;

namespace SabreTools.Library.Help
{
	public class Feature
	{
		#region Private instance variables

		private List<string> _flags;
		private string _description;
		private FeatureType _featureType;
		private Dictionary<string, Feature> _features;
		private List<string> _additionalNotes;
		private bool _foundOnce = false;

		// Specific value types
		private bool _valueBool = false;
		private int _valueInt32 = Int32.MinValue;
		private long _valueInt64 = Int64.MinValue;
		private string _valueString = null;
		private List<string> _valueList = null;

		#endregion

		#region Publicly facing variables

		public string Description
		{
			get { return _description; }
		}
		public Dictionary<string, Feature> Features
		{
			get { return _features; }
		}

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
			string prespace = "";
			for (int i = 0; i < pre; i++)
			{
				prespace += " ";
			}
			output += prespace;

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
			string prespace = "";
			for (int i = 0; i < preAdjusted; i++)
			{
				prespace += " ";
			}
			output += prespace;

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

			// Finally, let's append all additional notes
			if (_additionalNotes != null && _additionalNotes.Count > 0)
			{
				foreach (string note in _additionalNotes)
				{
					outputList.Add(prespace + note);
				}
			}

			return outputList;
		}

		/// <summary>
		/// Validate whether a flag is valid for this feature or not
		/// </summary>
		/// <param name="input">Input to check against</param>
		/// <param name="exact">True if just this feature should be checked, false if all subfeatures are checked as well</param>
		/// <param name="ignore">True if the existing flag should be ignored, false otherwise</param>
		/// <returns>True if the flag was valid, false otherwise</returns>
		public bool ValidateInput(string input, bool exact = false, bool ignore = false)
		{
			bool valid = false;

			// Determine what we should be looking for
			switch (_featureType)
			{
				// If we have a flag, make sure it doesn't have an equal sign in it 
				case FeatureType.Flag:
					valid = !input.Contains("=") && _flags.Contains(input);
					if (valid)
					{
						_valueBool = true;

						// If we've already found this feature before
						if (_foundOnce && !ignore)
						{
							valid = false;
						}

						_foundOnce = true;
					}
					break;
				// If we have an Int32, try to parse it if at all possible
				case FeatureType.Int32:
					valid = input.Contains("=") && _flags.Contains(input.Split('=')[0]);
					if (valid)
					{
						if (!Int32.TryParse(input.Split('=')[1], out int value))
						{
							value = Int32.MinValue;
						}
						_valueInt32 = value;

						// If we've already found this feature before
						if (_foundOnce && !ignore)
						{
							valid = false;
						}

						_foundOnce = true;
					}
					break;
				// If we have an Int32, try to parse it if at all possible
				case FeatureType.Int64:
					valid = input.Contains("=") && _flags.Contains(input.Split('=')[0]);
					if (valid)
					{
						if (!Int64.TryParse(input.Split('=')[1], out long value))
						{
							value = Int64.MinValue;
						}
						_valueInt64 = value;

						// If we've already found this feature before
						if (_foundOnce && !ignore)
						{
							valid = false;
						}

						_foundOnce = true;
					}
					break;
				// If we have an input, make sure it has an equals sign in it
				case FeatureType.List:
					valid = input.Contains("=") && _flags.Contains(input.Split('=')[0]);
					if (valid)
					{
						if (_valueList == null)
						{
							_valueList = new List<string>();
						}

						_valueList.Add(input.Split('=')[1]);
					}
					break;
				case FeatureType.String:
					valid = input.Contains("=") && _flags.Contains(input.Split('=')[0]);
					if (valid)
					{
						_valueString = input.Split('=')[1];

						// If we've already found this feature before
						if (_foundOnce && !ignore)
						{
							valid = false;
						}

						_foundOnce = true;
					}
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

			// If we're not valid at this point, we want to check if this flag is a file or a folder
			if (!valid)
			{
				valid = File.Exists(input) || Directory.Exists(input);
			}

			return valid;
		}

		/// <summary>
		/// Get the proper value associated with this feature
		/// </summary>
		/// <returns>Value associated with this feature</returns>
		public object GetValue()
		{
			switch (_featureType)
			{
				case FeatureType.Flag:
					return _valueBool;
				case FeatureType.Int32:
					return _valueInt32;
				case FeatureType.Int64:
					return _valueInt64;
				case FeatureType.List:
					return _valueList;
				case FeatureType.String:
					return _valueString;
			}

			return null;
		}

		/// <summary>
		/// Returns if this feature has a valid value or not
		/// </summary>
		/// <returns>True if the feature is enabled, false otherwise</returns>
		public bool IsEnabled()
		{
			object obj = GetValue();

			switch (_featureType)
			{
				case FeatureType.Flag:
					return (bool)obj;
				case FeatureType.Int32:
					return (int)obj != Int32.MinValue;
				case FeatureType.Int64:
					return (long)obj != Int64.MinValue;
				case FeatureType.List:
					return obj != null;
				case FeatureType.String:
					return obj != null;
			}

			return false;
		}

		#endregion
	}
}
