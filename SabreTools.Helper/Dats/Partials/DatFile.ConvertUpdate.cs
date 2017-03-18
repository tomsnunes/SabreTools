using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
#endif
using NaturalSort;

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Converting and Updating [MODULAR DONE]

		/// <summary>
		/// Determine if input files should be merged, diffed, or processed invidually
		/// </summary>
		/// <param name="inputPaths">Names of the input files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void DetermineUpdateType(List<string> inputPaths, string outDir, bool merge, DiffMode diff, bool inplace, bool skip,
			bool bare, bool clean, bool descAsName, Filter filter, SplitType splitType, bool trim, bool single, string root)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || diff != 0)
			{
				// Make sure there are no folders in inputs
				List<string> newInputFileNames = FileTools.GetOnlyFilesFromInputs(inputPaths, appendparent: true);

				// If we're in inverse cascade, reverse the list
				if ((diff & DiffMode.ReverseCascade) != 0)
				{
					newInputFileNames.Reverse();
				}

				// Create a dictionary of all ROMs from the input DATs
				List<DatFile> datHeaders = PopulateUserData(newInputFileNames, inplace, clean, descAsName,
					outDir, filter, splitType, trim, single, root);

				// Modify the Dictionary if necessary and output the results
				if (diff != 0 && diff < DiffMode.Cascade)
				{
					DiffNoCascade(diff, outDir, newInputFileNames);
				}
				// If we're in cascade and diff, output only cascaded diffs
				else if (diff != 0 && diff >= DiffMode.Cascade)
				{
					DiffCascade(outDir, inplace, newInputFileNames, datHeaders, skip);
				}
				// Output all entries with user-defined merge
				else
				{
					MergeNoDiff(outDir, newInputFileNames, datHeaders);
				}
			}
			// Otherwise, loop through all of the inputs individually
			else
			{
				Update(inputPaths, outDir, clean, descAsName, filter, splitType, trim, single, root);
			}
			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <returns>List of DatData objects representing headers</returns>
		private List<DatFile> PopulateUserData(List<string> inputs, bool inplace, bool clean, bool descAsName, string outDir,
			Filter filter, SplitType splitType, bool trim, bool single, string root)
		{
			DatFile[] datHeaders = new DatFile[inputs.Count];
			DateTime start = DateTime.Now;
			Globals.Logger.User("Processing individual DATs");

			// Parse all of the DATs into their own DatFiles in the array
			Parallel.For(0,
				inputs.Count,
				Globals.ParallelOptions,
				i =>
				{
					string input = inputs[i];
					Globals.Logger.User("Adding DAT: " + input.Split('¬')[0]);
					datHeaders[i] = new DatFile
					{
						DatFormat = (DatFormat != 0 ? DatFormat : 0),
						MergeRoms = MergeRoms,
					};

					datHeaders[i].Parse(input.Split('¬')[0], i, 0, splitType, true, clean, descAsName);
				});

			Globals.Logger.User("Processing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			Globals.Logger.User("Populating internal DAT");
			Parallel.For(0, inputs.Count, Globals.ParallelOptions, i =>
			{
				// Get the list of keys from the DAT
				List<string> keys = datHeaders[i].Keys.ToList();
				Parallel.ForEach(keys, Globals.ParallelOptions, key =>
				{
					// Add everything from the key to the internal DAT
					AddRange(key, datHeaders[i][key]);

					// Now remove the key from the source DAT
					lock (datHeaders)
					{
						datHeaders[i].Remove(key);
					}
				});

				// Now remove the file dictionary from the souce DAT to save memory
				datHeaders[i].Delete();
			});

			// Now that we have a merged DAT, filter it
			Filter(filter, single, trim, root);

			Globals.Logger.User("Processing and populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datHeaders.ToList();
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		public void DiffNoCascade(DiffMode diff, string outDir, List<string> inputs)
		{
			DateTime start = DateTime.Now;
			Globals.Logger.User("Initializing all output DATs");

			// Default vars for use
			string post = "";
			DatFile outerDiffData = new DatFile();
			DatFile dupeData = new DatFile();

			// Fill in any information not in the base DAT
			if (String.IsNullOrEmpty(_fileName))
			{
				_fileName = "All DATs";
			}
			if (String.IsNullOrEmpty(_name))
			{
				_name = "All DATs";
			}
			if (String.IsNullOrEmpty(_description))
			{
				_description = "All DATs";
			}

			// Don't have External dupes
			if ((diff & DiffMode.NoDupes) != 0)
			{
				post = " (No Duplicates)";
				outerDiffData = new DatFile(this);
				outerDiffData.FileName += post;
				outerDiffData.Name += post;
				outerDiffData.Description += post;
				outerDiffData.Reset();
			}

			// Have External dupes
			if ((diff & DiffMode.Dupes) != 0)
			{
				post = " (Duplicates)";
				dupeData = new DatFile(this);
				dupeData.FileName += post;
				dupeData.Name += post;
				dupeData.Description += post;
				dupeData.Reset();
			}

			// Create a list of DatData objects representing individual output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			if ((diff & DiffMode.Individuals) != 0)
			{
				DatFile[] outDatsArray = new DatFile[inputs.Count];

				Parallel.For(0, inputs.Count, j =>
				{
					string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
					DatFile diffData = new DatFile(this);
					diffData.FileName += innerpost;
					diffData.Name += innerpost;
					diffData.Description += innerpost;
					diffData.Reset();
					outDatsArray[j] = diffData;
				});

				outDats = outDatsArray.ToList();
			}
			Globals.Logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			Globals.Logger.User("Populating all output DATs");
			List<string> keys = Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(this[key]);

				if (roms != null && roms.Count > 0)
				{
					foreach (DatItem rom in roms)
					{
						// No duplicates
						if ((diff & DiffMode.NoDupes) != 0 || (diff & DiffMode.Individuals) != 0)
						{
							if ((rom.Dupe & DupeType.Internal) != 0)
							{
								// Individual DATs that are output
								if ((diff & DiffMode.Individuals) != 0)
								{
									outDats[rom.SystemID].Add(key, rom);
								}

								// Merged no-duplicates DAT
								if ((diff & DiffMode.NoDupes) != 0)
								{
									DatItem newrom = rom;
									newrom.Machine.Name += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

									outerDiffData.Add(key, newrom);
								}
							}
						}

						// Duplicates only
						if ((diff & DiffMode.Dupes) != 0)
						{
							if ((rom.Dupe & DupeType.External) != 0)
							{
								DatItem newrom = rom;
								newrom.Machine.Name += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

								dupeData.Add(key, newrom);
							}
						}
					}
				}
			}
			Globals.Logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			Globals.Logger.User("Outputting all created DATs");

			// Output the difflist (a-b)+(b-a) diff
			if ((diff & DiffMode.NoDupes) != 0)
			{
				outerDiffData.WriteToFile(outDir);
			}

			// Output the (ab) diff
			if ((diff & DiffMode.Dupes) != 0)
			{
				dupeData.WriteToFile(outDir);
			}

			// Output the individual (a-b) DATs
			if ((diff & DiffMode.Individuals) != 0)
			{
				for (int j = 0; j < inputs.Count; j++)
				{
					// If we have an output directory set, replace the path
					string[] split = inputs[j].Split('¬');
					string path = outDir + (split[0] == split[1]
						? Path.GetFileName(split[0])
						: (Path.GetDirectoryName(split[0]).Remove(0, split[1].Length)));

					// Try to output the file
					outDats[j].WriteToFile(path);
				}
			}
			Globals.Logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output cascading diffs
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		public void DiffCascade(string outDir, bool inplace, List<string> inputs, List<DatFile> datHeaders, bool skip)
		{
			string post = "";

			// Create a list of DatData objects representing output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			DateTime start = DateTime.Now;
			Globals.Logger.User("Initializing all output DATs");

			DatFile[] outDatsArray = new DatFile[inputs.Count];

			Parallel.For(0, inputs.Count, j =>
			{
				string innerpost = " (" + Path.GetFileNameWithoutExtension(inputs[j].Split('¬')[0]) + " Only)";
				DatFile diffData;

				// If we're in inplace mode, take the appropriate DatData object already stored
				if (inplace || !String.IsNullOrEmpty(outDir))
				{
					diffData = datHeaders[j];
				}
				else
				{
					diffData = new DatFile(this);
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
				}
				diffData.Reset();

				outDatsArray[j] = diffData;
			});

			outDats = outDatsArray.ToList();
			Globals.Logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			Globals.Logger.User("Populating all output DATs");
			List<string> keys = Keys.ToList();

			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(this[key]);

				if (roms != null && roms.Count > 0)
				{
					foreach (DatItem rom in roms)
					{
						// There's odd cases where there are items with System ID < 0. Skip them for now
						if (rom.SystemID < 0)
						{
							Globals.Logger.Warning("Item found with a <0 SystemID: " + rom.Name);
							continue;
						}

						outDats[rom.SystemID].Add(key, rom);
					}
				}
			}
			Globals.Logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			Globals.Logger.User("Outputting all created DATs");
			for (int j = (skip ? 1 : 0); j < inputs.Count; j++)
			{
				// If we have an output directory set, replace the path
				string path = "";
				if (inplace)
				{
					path = Path.GetDirectoryName(inputs[j].Split('¬')[0]);
				}
				else if (!String.IsNullOrEmpty(outDir))
				{
					string[] split = inputs[j].Split('¬');
					path = outDir + (split[0] == split[1]
						? Path.GetFileName(split[0])
						: (Path.GetDirectoryName(split[0]).Remove(0, split[1].Length))); ;
				}

				// Try to output the file
				outDats[j].WriteToFile(path);
			}
			Globals.Logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output user defined merge
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		public void MergeNoDiff(string outDir, List<string> inputs, List<DatFile> datHeaders)
		{
			// If we're in SuperDAT mode, prefix all games with their respective DATs
			if (Type == "SuperDAT")
			{
				List<string> keys = Keys.ToList();
				foreach (string key in keys)
				{
					List<DatItem> newroms = new List<DatItem>();
					foreach (DatItem rom in this[key])
					{
						DatItem newrom = rom;
						string filename = inputs[newrom.SystemID].Split('¬')[0];
						string rootpath = inputs[newrom.SystemID].Split('¬')[1];

						rootpath += (rootpath == "" ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newrom.Machine.Name = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
							+ Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
							+ newrom.Machine.Name;
						newroms.Add(newrom);
					}
					this[key] = newroms;
				}
			}

			// Try to output the file
			WriteToFile(outDir);
		}

		/// <summary>
		/// Strip the given hash types from the DAT
		/// </summary>
		private void StripHashesFromItems()
		{
			// Output the logging statement
			Globals.Logger.User("Stripping requested hashes");

			// Now process all of the roms
			List<string> keys = Keys.ToList();
			for (int i = 0; i < keys.Count; i++)
			{
				List<DatItem> items = this[keys[i]];
				for (int j = 0; j < items.Count; j++)
				{
					DatItem item = items[j];
					if (item.Type == ItemType.Rom)
					{
						Rom rom = (Rom)item;
						if ((StripHash & Hash.MD5) != 0)
						{
							rom.MD5 = null;
						}
						if ((StripHash & Hash.SHA1) != 0)
						{
							rom.SHA1 = null;
						}
						if ((StripHash & Hash.SHA256) != 0)
						{
							rom.SHA256 = null;
						}
						if ((StripHash & Hash.SHA384) != 0)
						{
							rom.SHA384 = null;
						}
						if ((StripHash & Hash.SHA512) != 0)
						{
							rom.SHA512 = null;
						}

						items[j] = rom;
					}
					else if (item.Type == ItemType.Disk)
					{
						Disk disk = (Disk)item;
						if ((StripHash & Hash.MD5) != 0)
						{
							disk.MD5 = null;
						}
						if ((StripHash & Hash.SHA1) != 0)
						{
							disk.SHA1 = null;
						}
						if ((StripHash & Hash.SHA256) != 0)
						{
							disk.SHA256 = null;
						}
						if ((StripHash & Hash.SHA384) != 0)
						{
							disk.SHA384 = null;
						}
						if ((StripHash & Hash.SHA512) != 0)
						{
							disk.SHA512 = null;
						}
						items[j] = disk;
					}
				}

				this[keys[i]] = items;
			}
		}

		/// <summary>
		/// Convert, update, and filter a DAT file or set of files using a base
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void Update(List<string> inputFileNames, string outDir, bool clean, bool descAsName, Filter filter,
			SplitType splitType, bool trim, bool single, string root)
		{
			Parallel.ForEach(inputFileNames, Globals.ParallelOptions, inputFileName =>
			{
				// Clean the input string
				if (inputFileName != "")
				{
					inputFileName = Path.GetFullPath(inputFileName);
				}

				if (File.Exists(inputFileName))
				{
					DatFile innerDatdata = new DatFile(this);
					Globals.Logger.User("Processing \"" + Path.GetFileName(inputFileName) + "\"");
					innerDatdata.Parse(inputFileName, 0, 0, splitType, true, clean, descAsName,
						keepext: ((innerDatdata.DatFormat & DatFormat.TSV) != 0 || (innerDatdata.DatFormat & DatFormat.CSV) != 0));
					innerDatdata.Filter(filter, trim, single, root);

					// Try to output the file
					innerDatdata.WriteToFile((outDir == "" ? Path.GetDirectoryName(inputFileName) : outDir), overwrite: (outDir != ""));
				}
				else if (Directory.Exists(inputFileName))
				{
					inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

					List<string> subFiles = Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(subFiles, Globals.ParallelOptions, file =>
					{
						Globals.Logger.User("Processing \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
						DatFile innerDatdata = new DatFile(this);
						innerDatdata.Parse(file, 0, 0, splitType, true, clean, descAsName,
							keepext: ((innerDatdata.DatFormat & DatFormat.TSV) != 0 || (innerDatdata.DatFormat & DatFormat.CSV) != 0));
						innerDatdata.Filter(filter, trim, single, root);

						// Try to output the file
						innerDatdata.WriteToFile((outDir == "" ? Path.GetDirectoryName(file) : outDir + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)),
							overwrite: (outDir != ""));
					});
				}
				else
				{
					Globals.Logger.Error("I'm sorry but " + inputFileName + " doesn't exist!");
					return;
				}
			});
		}

		#endregion
	}
}
