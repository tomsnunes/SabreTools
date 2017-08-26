using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
#endif
using NaturalSort;

namespace SabreTools.Library.Dats
{
	public partial class DatFile
	{
		#region Converting and Updating

		/// <summary>
		/// Determine if input files should be merged, diffed, or processed invidually
		/// </summary>
		/// <param name="inputPaths">Names of the input files and/or folders</param>
		/// <param name="basePaths">Names of base files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void DetermineUpdateType(List<string> inputPaths, List<string> basePaths, string outDir, bool merge, DiffMode diff, bool inplace, bool skip,
			bool bare, bool clean, bool remUnicode, bool descAsName, Filter filter, SplitType splitType, bool trim, bool single, string root)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || (diff != 0 && (diff & DiffMode.Against) == 0))
			{
				// Make sure there are no folders in inputs
				List<string> newInputFileNames = FileTools.GetOnlyFilesFromInputs(inputPaths, appendparent: true);

				// Reverse if we have to
				if ((diff & DiffMode.ReverseCascade) != 0)
				{
					newInputFileNames.Reverse();
				}

				// Create a dictionary of all ROMs from the input DATs
				List<DatFile> datHeaders = PopulateUserData(newInputFileNames, inplace, clean,
					remUnicode, descAsName, outDir, filter, splitType, trim, single, root);

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
			// If we're in "diff against" mode, we treat the inputs differently
			else if ((diff & DiffMode.Against) != 0)
			{
				DiffAgainst(inputPaths, basePaths, outDir, inplace, clean, remUnicode, descAsName, filter, splitType, trim, single, root);
			}
			// Otherwise, loop through all of the inputs individually
			else
			{
				Update(inputPaths, outDir, inplace, clean, remUnicode, descAsName, filter, splitType, trim, single, root);
			}
			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="inputs">Paths to DATs to parse</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <returns>List of DatData objects representing headers</returns>
		private List<DatFile> PopulateUserData(List<string> inputs, bool inplace, bool clean, bool remUnicode, bool descAsName,
			string outDir, Filter filter, SplitType splitType, bool trim, bool single, string root)
		{
			DatFile[] datHeaders = new DatFile[inputs.Count];
			DateTime start = DateTime.Now;
			Globals.Logger.User("Processing individual DATs");

			// Parse all of the DATs into their own DatFiles in the array
			Parallel.For(0, inputs.Count, i =>
			{
				string input = inputs[i];
				Globals.Logger.User("Adding DAT: {0}", input.Split('¬')[0]);
				datHeaders[i] = new DatFile
				{
					DatFormat = (DatFormat != 0 ? DatFormat : 0),
					MergeRoms = MergeRoms,
				};

				datHeaders[i].Parse(input.Split('¬')[0], i, 0, splitType, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);
			});

			Globals.Logger.User("Processing complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			Globals.Logger.User("Populating internal DAT");
			Parallel.For(0, inputs.Count, i =>
			{
				// Get the list of keys from the DAT
				List<string> keys = datHeaders[i].Keys.ToList();
				Parallel.ForEach(keys, key =>
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

			Globals.Logger.User("Processing and populating complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datHeaders.ToList();
		}

		/// <summary>
		/// Output diffs against a base set
		/// </summary>
		/// <param name="inputPaths">Names of the input files and/or folders</param>
		/// <param name="basePaths">Names of base files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void DiffAgainst(List<string> inputPaths, List<string> basePaths, string outDir, bool inplace, bool clean, bool remUnicode,
			bool descAsName, Filter filter, SplitType splitType, bool trim, bool single, string root)
		{
			// First we want to parse all of the base DATs into the input
			DateTime start = DateTime.Now;
			Globals.Logger.User("Populating base DAT for comparison...");

			List<string> baseFileNames = FileTools.GetOnlyFilesFromInputs(basePaths);
			Parallel.ForEach(baseFileNames, path =>
			{
				Parse(path, 0, 0, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);
			});

			Globals.Logger.User("Populating base DAT complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// For comparison's sake, we want to use CRC as the base ordering
			BucketBy(SortedBy.CRC, true);

			// Now we want to compare each input DAT against the base
			List<string> inputFileNames = FileTools.GetOnlyFilesFromInputs(inputPaths, appendparent: true);
			Parallel.ForEach(inputFileNames, path =>
			{
				// Get the two halves of the path
				string[] splitpath = path.Split('¬');

				Globals.Logger.User("Comparing '{0}'' to base DAT", splitpath[0]);

				// First we parse in the DAT internally
				DatFile intDat = new DatFile();
				intDat.Parse(splitpath[0], 1, 1, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);

				// For comparison's sake, we want to use CRC as the base ordering
				intDat.BucketBy(SortedBy.CRC, true);

				// Then we do a hashwise comparison against the base DAT
				List<string> keys = intDat.Keys.ToList();
				Parallel.ForEach(keys, key =>
				{
					List<DatItem> datItems = intDat[key];
					List<DatItem> keepDatItems = new List<DatItem>();
					foreach (DatItem datItem in datItems)
					{
						if (!datItem.HasDuplicates(this, true))
						{
							keepDatItems.Add(datItem);
						}
					}

					// Now add the new list to the key
					intDat.Remove(key);
					intDat.AddRange(key, keepDatItems);
				});

				// Determine the output path for the DAT
				string interOutDir = outDir;
				if (inplace)
				{
					interOutDir = Path.GetDirectoryName(path);
				}
				else if (!String.IsNullOrEmpty(interOutDir))
				{
					interOutDir = Path.GetDirectoryName(Path.Combine(interOutDir, splitpath[0].Remove(0, splitpath[1].Length + 1)));
				}
				else
				{
					interOutDir = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, splitpath[0].Remove(0, splitpath[1].Length + 1)));
				}

				// Once we're done, we check to see if there's anything to write out
				if (intDat.Count > 0)
				{
					intDat.WriteToFile(interOutDir);
				}

				// Due to possible memory requirements, we force a garbage collection
				GC.Collect();
			});
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
				if (inplace || outDir != Environment.CurrentDirectory)
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
			Globals.Logger.User("Initializing complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			Globals.Logger.User("Populating all output DATs");
			List<string> keys = Keys.ToList();

			Parallel.ForEach(keys, key =>
			{
				List<DatItem> items = DatItem.Merge(this[key]);

				// If the rom list is empty or null, just skip it
				if (items == null || items.Count == 0)
				{
					return;
				}

				foreach (DatItem item in items)
				{
					// There's odd cases where there are items with System ID < 0. Skip them for now
					if (item.SystemID < 0)
					{
						Globals.Logger.Warning("Item found with a <0 SystemID: {0}", item.Name);
						continue;
					}

					outDats[item.SystemID].Add(key, item);
				}
			});

			Globals.Logger.User("Populating complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			Globals.Logger.User("Outputting all created DATs");

			Parallel.For((skip ? 1 : 0), inputs.Count, j =>
			{
				// If we have an output directory set, replace the path
				string path = "";
				if (inplace)
				{
					path = Path.GetDirectoryName(inputs[j].Split('¬')[0]);
				}
				else if (outDir != Environment.CurrentDirectory)
				{
					string[] split = inputs[j].Split('¬');
					path = outDir + (split[0] == split[1]
						? Path.GetFileName(split[0])
						: (Path.GetDirectoryName(split[0]).Remove(0, split[1].Length))); ;
				}

				// Try to output the file
				outDats[j].WriteToFile(path);
			});

			Globals.Logger.User("Outputting complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
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
			Globals.Logger.User("Initializing complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			Globals.Logger.User("Populating all output DATs");

			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, key =>
			{
				List<DatItem> items = DatItem.Merge(this[key]);

				// If the rom list is empty or null, just skip it
				if (items == null || items.Count == 0)
				{
					return;
				}

				// Loop through and add the items correctly
				foreach(DatItem item in items)
				{
					// No duplicates
					if ((diff & DiffMode.NoDupes) != 0 || (diff & DiffMode.Individuals) != 0)
					{
						if ((item.Dupe & DupeType.Internal) != 0)
						{
							// Individual DATs that are output
							if ((diff & DiffMode.Individuals) != 0)
							{
								outDats[item.SystemID].Add(key, item);
							}

							// Merged no-duplicates DAT
							if ((diff & DiffMode.NoDupes) != 0)
							{
								DatItem newrom = item.Clone() as DatItem;
								newrom.Machine.AppendName(" (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")");

								outerDiffData.Add(key, newrom);
							}
						}
					}

					// Duplicates only
					if ((diff & DiffMode.Dupes) != 0)
					{
						if ((item.Dupe & DupeType.External) != 0)
						{
							DatItem newrom = item.Clone() as DatItem;
							newrom.Machine.AppendName(" (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")");

							dupeData.Add(key, newrom);
						}
					}
				}
			});

			Globals.Logger.User("Populating complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

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
				Parallel.For(0, inputs.Count, j =>
				{
					// If we have an output directory set, replace the path
					string[] split = inputs[j].Split('¬');
					string path = Path.Combine(outDir,
						(split[0] == split[1]
							? Path.GetFileName(split[0])
							: (Path.GetDirectoryName(split[0]).Remove(0, split[1].Length))));

					// Try to output the file
					outDats[j].WriteToFile(path);
				});
			}
			Globals.Logger.User("Outputting complete in {0}", DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
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
				Parallel.ForEach(keys, key =>
				{
					List<DatItem> items = this[key].ToList();
					List<DatItem> newItems = new List<DatItem>();
					foreach (DatItem item in items)
					{
						DatItem newItem = item;
						string filename = inputs[newItem.SystemID].Split('¬')[0];
						string rootpath = inputs[newItem.SystemID].Split('¬')[1];

						rootpath += (rootpath == "" ? "" : Path.DirectorySeparatorChar.ToString());
						filename = filename.Remove(0, rootpath.Length);
						newItem.Machine.UpdateName(Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
							+ Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
							+ newItem.Machine.Name);

						newItems.Add(newItem);
					}

					Remove(key);
					AddRange(key, newItems);
				});
			}

			// Try to output the file
			WriteToFile(outDir);
		}

		/// <summary>
		/// Convert, update, and filter a DAT file or set of files using a base
		/// </summary>
		/// <param name="inputFileNames">Names of the input files and/or folders</param>
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		public void Update(List<string> inputFileNames, string outDir, bool inplace, bool clean, bool remUnicode, bool descAsName,
			Filter filter, SplitType splitType, bool trim, bool single, string root)
		{
			Parallel.ForEach(inputFileNames, inputFileName =>
			{
				// Clean the input string
				if (inputFileName != "")
				{
					inputFileName = Path.GetFullPath(inputFileName);
				}

				if (File.Exists(inputFileName))
				{
					// If inplace is set, override the output dir
					string realOutDir = outDir;
					if (inplace)
					{
						realOutDir = Path.GetDirectoryName(inputFileName);
					}

					DatFile innerDatdata = new DatFile(this);
					Globals.Logger.User("Processing '{0}'", Path.GetFileName(inputFileName));
					innerDatdata.Parse(inputFileName, 0, 0, splitType, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName,
						keepext: ((innerDatdata.DatFormat & DatFormat.TSV) != 0 || (innerDatdata.DatFormat & DatFormat.CSV) != 0));
					innerDatdata.Filter(filter, trim, single, root);

					// Try to output the file
					innerDatdata.WriteToFile((realOutDir == Environment.CurrentDirectory ? Path.GetDirectoryName(inputFileName) : realOutDir), overwrite: (realOutDir != Environment.CurrentDirectory));
				}
				else if (Directory.Exists(inputFileName))
				{
					inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

					// If inplace is set, override the output dir
					string realOutDir = outDir;
					if (inplace)
					{
						realOutDir = Path.GetDirectoryName(inputFileName);
					}

					List<string> subFiles = Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(subFiles, file =>
					{
						Globals.Logger.User("Processing '{0}'", Path.GetFullPath(file).Remove(0, inputFileName.Length));
						DatFile innerDatdata = new DatFile(this);
						innerDatdata.Parse(file, 0, 0, splitType, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName,
							keepext: ((innerDatdata.DatFormat & DatFormat.TSV) != 0 || (innerDatdata.DatFormat & DatFormat.CSV) != 0));
						innerDatdata.Filter(filter, trim, single, root);

						// Try to output the file
						innerDatdata.WriteToFile((realOutDir == Environment.CurrentDirectory ? Path.GetDirectoryName(file) : realOutDir + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)),
							overwrite: (realOutDir != Environment.CurrentDirectory));
					});
				}
				else
				{
					Globals.Logger.Error("I'm sorry but '{0}' doesn't exist!", inputFileName);
					return;
				}
			});
		}

		#endregion
	}
}
