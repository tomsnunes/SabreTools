using System;
using System.Collections.Specialized;
using System.Linq;

namespace SabreTools.Helper
{
	public class DatItemKV : IEquatable<DatItemKV>
	{
		// Private instance variables
		private string _name;
		private NameValueCollection _attributes;

		// Public instance variables
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		public string Type
		{
			get { return _name; }
			set { _name = value; }
		}
		public string[] this[string s]
		{
			get
			{
				if (_attributes == null)
				{
					_attributes = new NameValueCollection();
				}

				return _attributes.GetValues(s);
			}
		}

		// Constructors
		public DatItemKV(string name)
		{
			_name = name;
			_attributes = new NameValueCollection();
		}

		// Comparison methods
		public bool Equals(DatItemKV other)
		{
			// If the types don't match, then it's not the same
			if (_name != other.Type)
			{
				return false;
			}

			// Otherwise, loop through and compare against what you can
			bool success = true;
			foreach (string key in _attributes.Keys)
			{
				string[] vals = _attributes.GetValues(key);
				string[] ovals = other.GetValues(key);

				// TODO: This does a flat check on all items. This needs to have some finesse when it comes to comparing hashes and sizes for roms,
				// disks, and files since they are all separate now

				// http://stackoverflow.com/questions/649444/testing-equality-of-arrays-in-c-sharp
				var q = from a in vals
						join b in ovals on a equals b
						select a;

				success &= vals.Length == ovals.Length && q.Count() == vals.Length;
			}

			return success;
		}

		// Instance methods
		public void Add(string name, string value)
		{
			_attributes.Add(name, value);
		}
		public string Get(string name)
		{
			return _attributes.Get(name);
		}
		public string Get(int index)
		{
			return _attributes.Get(index);
		}
		public string[] GetValues(string name)
		{
			return _attributes.GetValues(name);
		}
		public string[] GetValues(int index)
		{
			return _attributes.GetValues(index);
		}
	}
}
