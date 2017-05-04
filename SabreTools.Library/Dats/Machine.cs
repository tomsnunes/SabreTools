using System;
using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public class Machine : ICloneable
	{
		#region Protected instance variables

		// Machine information
		protected string _name;
		protected string _comment;
		protected string _description;
		protected string _year;
		protected string _manufacturer;
		protected string _romOf;
		protected string _cloneOf;
		protected string _sampleOf;
		protected string _sourceFile;
		protected bool? _runnable;
		protected string _board;
		protected string _rebuildTo;
		protected List<string> _devices;
		protected MachineType _machineType;

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
		/// Create a default, empty Machine object
		/// </summary>
		public Machine()
		{
			_name = "";
			_description = "";
			_runnable = null;
		}

		/// <summary>
		/// Create a new Machine object with the included information
		/// </summary>
		/// <param name="name">Name of the machine</param>
		/// <param name="description">Description of the machine</param>
		public Machine(string name, string description)
		{
			_name = name;
			_description = description;
			_runnable = null;
		}

		#endregion

		#region Cloneing

		public object Clone()
		{
			return new Machine()
			{
				Name = _name,
				Comment = _comment,
				Description = _description,
				Year = _year,
				Manufacturer = _manufacturer,
				RomOf = _romOf,
				CloneOf = _cloneOf,
				SampleOf = _sampleOf,
				SourceFile = _sourceFile,
				Runnable = _runnable,
				Board = _board,
				RebuildTo = _rebuildTo,
				Devices = _devices,
				MachineType = _machineType,
			};
		}

		#endregion
	}
}
