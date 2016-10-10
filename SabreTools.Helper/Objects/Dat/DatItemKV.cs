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
		private NameValueCollection _machineAttributes;
		private NameValueCollection _machineElements;

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
			_machineAttributes = new NameValueCollection();
			_machineElements = new NameValueCollection();
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

				// Special case for "rom"
				if (_name == "rom")
				{
					// If either is a nodump, it's never a match
					if (Get("status") == "nodump" || other.Get("status") == "nodump")
					{
						success = false;
					}

					// If the size is the same and any combination of metadata matches
					if ((Get("size") == other.Get("size")) &&
						((String.IsNullOrEmpty(Get("crc")) || String.IsNullOrEmpty(other.Get("crc"))) || Get("crc") == other.Get("crc")) &&
						((String.IsNullOrEmpty(Get("md5")) || String.IsNullOrEmpty(other.Get("md5"))) || Get("md5") == other.Get("md5")) &&
						((String.IsNullOrEmpty(Get("sha1")) || String.IsNullOrEmpty(other.Get("sha1"))) || Get("sha1") == other.Get("sha1")))
					{
						success = true;
					}
					else
					{
						success = false;
					}
				}
				// Special case for "disk"
				else if (_name == "disk")
				{
					// If either is a nodump, it's never a match
					if (Get("status") == "nodump" || other.Get("status") == "nodump")
					{
						success = false;
					}

					// If any combination of metadata matches
					if (((String.IsNullOrEmpty(Get("md5")) || String.IsNullOrEmpty(other.Get("md5"))) || Get("md5") == other.Get("md5")) &&
						((String.IsNullOrEmpty(Get("sha1")) || String.IsNullOrEmpty(other.Get("sha1"))) || Get("sha1") == other.Get("sha1")))
					{
						success = true;
					}
					else
					{
						success = false;
					}
				}
				// For everything else
				else
				{
					// http://stackoverflow.com/questions/649444/testing-equality-of-arrays-in-c-sharp
					var q = from a in vals
							join b in ovals on a equals b
							select a;

					success &= vals.Length == ovals.Length && q.Count() == vals.Length;
				}
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

		public void MachineAttributesAdd(string name, string value)
		{
			_machineAttributes.Add(name, value);
		}
		public string MachineAttributesGet(string name)
		{
			return _attributes.Get(name);
		}
		public string MachineAttributesGet(int index)
		{
			return _attributes.Get(index);
		}
		public string[] MachineAttributesGetValues(string name)
		{
			return _attributes.GetValues(name);
		}
		public string[] MachineAttributesGetValues(int index)
		{
			return _attributes.GetValues(index);
		}

		public void MachineElementsAdd(string name, string value)
		{
			_machineAttributes.Add(name, value);
		}
		public string MachineElementsGet(string name)
		{
			return _attributes.Get(name);
		}
		public string MachineElementsGet(int index)
		{
			return _attributes.Get(index);
		}
		public string[] MachineElementsGetValues(string name)
		{
			return _attributes.GetValues(name);
		}
		public string[] MachineElementsGetValues(int index)
		{
			return _attributes.GetValues(index);
		}
	}
}
