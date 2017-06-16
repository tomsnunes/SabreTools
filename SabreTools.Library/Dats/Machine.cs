using System;
using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public struct Machine
	{
		#region Protected instance variables

		// Machine information
		private string _name;
		private string _comment;
		private string _description;
		private string _year;
		private string _manufacturer;
		private string _romOf;
		private string _cloneOf;
		private string _sampleOf;
		private string _sourceFile;
		private bool? _runnable;
		private string _board;
		private string _rebuildTo;
		private List<string> _devices;
		private MachineType _machineType;
		private Guid _guid;

		#endregion

		#region Publicly facing variables

		// Machine information
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		public string Comment
		{
			get { return _comment; }
			set { _comment = value; }
		}
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}
		public string Year
		{
			get { return _year; }
			set { _year = value; }
		}
		public string Manufacturer
		{
			get { return _manufacturer; }
			set { _manufacturer = value; }
		}
		public string RomOf
		{
			get { return _romOf; }
			set { _romOf = value; }
		}
		public string CloneOf
		{
			get { return _cloneOf; }
			set { _cloneOf = value; }
		}
		public string SampleOf
		{
			get { return _sampleOf; }
			set { _sampleOf = value; }
		}
		public string SourceFile
		{
			get { return _sourceFile; }
			set { _sourceFile = value; }
		}
		public bool? Runnable
		{
			get { return _runnable; }
			set { _runnable = value; }
		}
		public string Board
		{
			get { return _board; }
			set { _board = value; }
		}
		public string RebuildTo
		{
			get { return _rebuildTo; }
			set { _rebuildTo = value; }
		}
		public List<string> Devices
		{
			get { return _devices; }
			set { _devices = value; }
		}
		public MachineType MachineType
		{
			get { return _machineType; }
			set { _machineType = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a new Machine object with the included information
		/// </summary>
		/// <param name="name">Name of the machine</param>
		/// <param name="description">Description of the machine</param>
		public Machine(string name, string description)
		{
			_name = name;
			_comment = null;
			_description = description;
			_year = null;
			_manufacturer = null;
			_romOf = null;
			_cloneOf = null;
			_sampleOf = null;
			_sourceFile = null;
			_runnable = null;
			_board = null;
			_rebuildTo = null;
			_devices = null;
			_machineType = MachineType.NULL;
			_guid = new Guid();
		}

		#endregion

		#region Equality comparerers

		/// <summary>
		/// Override the equality comparer
		/// </summary>
		public static bool operator ==(Machine a, Machine b)
		{
			return (a.Name == b.Name
				&& a.Comment == b.Comment
				&& a.Description == b.Description
				&& a.Year == b.Year
				&& a.Manufacturer == b.Manufacturer
				&& a.RomOf == b.RomOf
				&& a.CloneOf == b.CloneOf
				&& a.SampleOf == b.SampleOf
				&& a.SourceFile == b.SourceFile
				&& a.Runnable == b.Runnable
				&& a.Board == b.Board
				&& a.RebuildTo == b.RebuildTo
				&& a.Devices == b.Devices
				&& a.MachineType == b.MachineType);
		}

		/// <summary>
		/// Override the inequality comparer
		/// </summary>
		public static bool operator !=(Machine a, Machine b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Override the Equals method
		/// </summary>
		public override bool Equals(object o)
		{
			if (o.GetType() != typeof(Machine))
			{
				return false;
			}

			return this == (Machine)o;
		}

		/// <summary>
		/// Override the GetHashCode method
		/// </summary>
		public override int GetHashCode()
		{
			return OCRC.OptimizedCRC.Compute(_guid.ToByteArray());
		}

		#endregion

		#region Update fields

		/// <summary>
		/// Append a string to the description
		/// </summary>
		public void AppendDescription(string append)
		{
			UpdateDescription(this.Description + append);
		}

		/// <summary>
		/// Append a string to the name
		/// </summary>
		public void AppendName(string append)
		{
			UpdateName(this.Name + append);
		}

		/// <summary>
		/// Update the cloneof
		/// </summary>
		public void UpdateCloneOf(string update)
		{
			_cloneOf = update;
		}

		/// <summary>
		/// Update the description
		/// </summary>
		public void UpdateDescription(string update)
		{
			_description = update;
		}

		/// <summary>
		/// Update the romof
		/// </summary>
		public void UpdateRomOf(string update)
		{
			_romOf = update;
		}

		/// <summary>
		/// Update the sampleof
		/// </summary>
		public void UpdateSampleOf(string update)
		{
			_sampleOf = update;
		}

		/// <summary>
		/// Update the name
		/// </summary>
		public void UpdateName(string update)
		{
			_name = update;
		}

		#endregion
	}
}
