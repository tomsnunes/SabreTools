using System;
using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public class Machine
	{
		#region Protected instance variables

		private Guid _guid;

		#endregion

		#region Publicly facing variables

		// Machine information
		public string Name;
		public string Comment;
		public string Description;
		public string Year;
		public string Manufacturer;
		public string RomOf;
		public string CloneOf;
		public string SampleOf;
		public string SourceFile;
		public bool? Runnable;
		public string Board;
		public string RebuildTo;
		public List<string> Devices;
		public MachineType MachineType;

		#endregion

		#region Constructors

		/// <summary>
		/// Create a new Machine object
		/// </summary>
		public Machine()
		{
			Name = null;
			Comment = null;
			Description = null;
			Year = null;
			Manufacturer = null;
			RomOf = null;
			CloneOf = null;
			SampleOf = null;
			SourceFile = null;
			Runnable = null;
			Board = null;
			RebuildTo = null;
			Devices = null;
			MachineType = MachineType.NULL;
			_guid = new Guid();
		}

		/// <summary>
		/// Create a new Machine object with the included information
		/// </summary>
		/// <param name="name">Name of the machine</param>
		/// <param name="description">Description of the machine</param>
		public Machine(string name, string description)
		{
			Name = name;
			Comment = null;
			Description = description;
			Year = null;
			Manufacturer = null;
			RomOf = null;
			CloneOf = null;
			SampleOf = null;
			SourceFile = null;
			Runnable = null;
			Board = null;
			RebuildTo = null;
			Devices = null;
			MachineType = MachineType.NULL;
			_guid = new Guid();
		}

		#endregion

		#region Equality comparerers

		/// <summary>
		/// Override the Equals method
		/// </summary>
		public override bool Equals(object o)
		{
			if (this == null && o == null)
			{
				return true;
			}
			else if (this == null || o == null)
			{
				return false;
			}
			else if (o.GetType() != typeof(Machine))
			{
				return false;
			}

			Machine b = (Machine)o;

			return (this.Name == b.Name
				&& this.Comment == b.Comment
				&& this.Description == b.Description
				&& this.Year == b.Year
				&& this.Manufacturer == b.Manufacturer
				&& this.RomOf == b.RomOf
				&& this.CloneOf == b.CloneOf
				&& this.SampleOf == b.SampleOf
				&& this.SourceFile == b.SourceFile
				&& this.Runnable == b.Runnable
				&& this.Board == b.Board
				&& this.RebuildTo == b.RebuildTo
				&& this.Devices == b.Devices
				&& this.MachineType == b.MachineType);
		}

		#endregion

		#region Property updaters

		public void AppendName(string append)
		{
			Name += append;
		}

		public void AppendDescription(string append)
		{
			Description += append;
		}

		public void UpdateName(string update)
		{
			Name = update;
		}

		public void UpdateDescription(string update)
		{
			Description = update;
		}

		public void UpdateCloneOf(string update)
		{
			CloneOf = update;
		}

		public void UpdateRomOf(string update)
		{
			RomOf = update;
		}

		public void UpdateSampleOf(string update)
		{
			SampleOf = update;
		}

		#endregion
	}
}
