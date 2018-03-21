using System;
using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.DatItems
{
	/// <summary>
	/// Represents the information specific to a set/game/machine
	/// </summary>
	public class Machine : ICloneable
	{
		#region Publicly facing variables

		// Machine information
		public string Name;
		public string Comment;
		public string Description;
		public string Year;
		public string Manufacturer;
		public string Publisher;
		public string RomOf;
		public string CloneOf;
		public string SampleOf;
		public bool? Supported;
		public string SourceFile;
		public bool? Runnable;
		public string Board;
		public string RebuildTo;
		public List<string> Devices;
		public List<string> SlotOptions;
		public List<Tuple<string, string>> Infos;
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
			Publisher = null;
			RomOf = null;
			CloneOf = null;
			SampleOf = null;
			Supported = true;
			SourceFile = null;
			Runnable = null;
			Board = null;
			RebuildTo = null;
			Devices = null;
			SlotOptions = null;
			Infos = null;
			MachineType = MachineType.NULL;
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
			Publisher = null;
			RomOf = null;
			CloneOf = null;
			SampleOf = null;
			Supported = true;
			SourceFile = null;
			Runnable = null;
			Board = null;
			RebuildTo = null;
			Devices = null;
			SlotOptions = null;
			Infos = null;
			MachineType = MachineType.NULL;
		}

		#endregion

		#region Cloning methods

		/// <summary>
		/// Create a clone of the current machine
		/// </summary>
		/// <returns>New machine with the same values as the current one</returns>
		public object Clone()
		{
			return new Machine()
			{
				Name = this.Name,
				Comment = this.Comment,
				Description = this.Description,
				Year = this.Year,
				Manufacturer = this.Manufacturer,
				Publisher = this.Publisher,
				RomOf = this.RomOf,
				CloneOf = this.CloneOf,
				SampleOf = this.SampleOf,
				Supported = this.Supported,
				SourceFile = this.SourceFile,
				Runnable = this.Runnable,
				Board = this.Board,
				RebuildTo = this.RebuildTo,
				Devices = this.Devices,
				SlotOptions = this.SlotOptions,
				Infos = this.Infos,
				MachineType = this.MachineType,
			};
		}

		#endregion
	}
}
