using System;
using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.Dats
{
	public struct Machine
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
