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
		/// <param name="softlist">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void DetermineUpdateType(List<string> inputPaths, string outDir, bool merge, DiffMode diff, bool inplace, bool skip,
			bool bare, bool clean, bool softlist, Filter filter, bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			// If we're in merging or diffing mode, use the full list of inputs
			if (merge || diff != 0)
			{
				// Make sure there are no folders in inputs
				List<string> newInputFileNames = FileTools.GetOnlyFilesFromInputs(inputPaths, maxDegreeOfParallelism, logger, appendparent: true);

				// If we're in inverse cascade, reverse the list
				if ((diff & DiffMode.ReverseCascade) != 0)
				{
					newInputFileNames.Reverse();
				}

				// Create a dictionary of all ROMs from the input DATs
				List<DatFile> datHeaders = PopulateUserData(newInputFileNames, inplace, clean, softlist,
					outDir, filter, trim, single, root, maxDegreeOfParallelism, logger);

				// Modify the Dictionary if necessary and output the results
				if (diff != 0 && diff < DiffMode.Cascade)
				{
					DiffNoCascade(diff, outDir, newInputFileNames, logger);
				}
				// If we're in cascade and diff, output only cascaded diffs
				else if (diff != 0 && diff >= DiffMode.Cascade)
				{
					DiffCascade(outDir, inplace, newInputFileNames, datHeaders, skip, logger);
				}
				// Output all entries with user-defined merge
				else
				{
					MergeNoDiff(outDir, newInputFileNames, datHeaders, logger);
				}
			}
			// Otherwise, loop through all of the inputs individually
			else
			{
				Update(inputPaths, outDir, clean, softlist, filter, trim, single, root, maxDegreeOfParallelism, logger);
			}
			return;
		}

		/// <summary>
		/// Populate the user DatData object from the input files
		/// </summary>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		/// <returns>List of DatData objects representing headers</returns>
		private List<DatFile> PopulateUserData(List<string> inputs, bool inplace, bool clean, bool softlist, string outDir,
			Filter filter, bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			DatFile[] datHeaders = new DatFile[inputs.Count];
			DateTime start = DateTime.Now;
			logger.User("Processing individual DATs");

			/// BEGIN
			Parallel.For(0,
				inputs.Count,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				i =>
				{
					string input = inputs[i];
					logger.User("Adding DAT: " + input.Split('¬')[0]);
					datHeaders[i] = new DatFile
					{
						DatFormat = (DatFormat != 0 ? DatFormat : 0),
						Files = new SortedDictionary<string, List<DatItem>>(),
						MergeRoms = MergeRoms,
					};

					datHeaders[i].Parse(input.Split('¬')[0], i, 0, filter, trim, single, root, logger, true, clean, softlist);
				});

			logger.User("Processing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			logger.User("Populating internal DAT");
			Files = new SortedDictionary<string, List<DatItem>>();
			for (int i = 0; i < inputs.Count; i++)
			{
				List<string> keys = datHeaders[i].Files.Keys.ToList();
				foreach (string key in keys)
				{
					if (Files.ContainsKey(key))
					{
						Files[key].AddRange(datHeaders[i].Files[key]);
					}
					else
					{
						Files.Add(key, datHeaders[i].Files[key]);
					}
					datHeaders[i].Files.Remove(key);
				}
				datHeaders[i].Files = null;
			}
			/// END

			logger.User("Processing and populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			return datHeaders.ToList();
		}

		/// <summary>
		/// Output non-cascading diffs
		/// </summary>
		/// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void DiffNoCascade(DiffMode diff, string outDir, List<string> inputs, Logger logger)
		{
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

			// Default vars for use
			string post = "";
			DatFile outerDiffData = new DatFile();
			DatFile dupeData = new DatFile();

			// Don't have External dupes
			if ((diff & DiffMode.NoDupes) != 0)
			{
				post = " (No Duplicates)";
				outerDiffData = (DatFile)CloneHeader();
				outerDiffData.FileName += post;
				outerDiffData.Name += post;
				outerDiffData.Description += post;
				outerDiffData.Files = new SortedDictionary<string, List<DatItem>>();
			}

			// Have External dupes
			if ((diff & DiffMode.Dupes) != 0)
			{
				post = " (Duplicates)";
				dupeData = (DatFile)CloneHeader();
				dupeData.FileName += post;
				dupeData.Name += post;
				dupeData.Description += post;
				dupeData.Files = new SortedDictionary<string, List<DatItem>>();
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
					DatFile diffData = (DatFile)CloneHeader();
					diffData.FileName += innerpost;
					diffData.Name += innerpost;
					diffData.Description += innerpost;
					diffData.Files = new SortedDictionary<string, List<DatItem>>();
					outDatsArray[j] = diffData;
				});

				outDats = outDatsArray.ToList();
			}
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = Files.Keys.ToList();
			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(Files[key], logger);

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
									if (outDats[rom.SystemID].Files.ContainsKey(key))
									{
										outDats[rom.SystemID].Files[key].Add(rom);
									}
									else
									{
										List<DatItem> tl = new List<DatItem>();
										tl.Add(rom);
										outDats[rom.SystemID].Files.Add(key, tl);
									}
								}

								// Merged no-duplicates DAT
								if ((diff & DiffMode.NoDupes) != 0)
								{
									DatItem newrom = rom;
									newrom.Machine.Name += " (" + Path.GetFileNameWithoutExtension(inputs[newrom.SystemID].Split('¬')[0]) + ")";

									if (outerDiffData.Files.ContainsKey(key))
									{
										outerDiffData.Files[key].Add(newrom);
									}
									else
									{
										List<DatItem> tl = new List<DatItem>();
										tl.Add(rom);
										outerDiffData.Files.Add(key, tl);
									}
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

								if (dupeData.Files.ContainsKey(key))
								{
									dupeData.Files[key].Add(newrom);
								}
								else
								{
									List<DatItem> tl = new List<DatItem>();
									tl.Add(rom);
									dupeData.Files.Add(key, tl);
								}
							}
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");

			// Output the difflist (a-b)+(b-a) diff
			if ((diff & DiffMode.NoDupes) != 0)
			{
				outerDiffData.WriteToFile(outDir, logger);
			}

			// Output the (ab) diff
			if ((diff & DiffMode.Dupes) != 0)
			{
				dupeData.WriteToFile(outDir, logger);
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

					// If we have more than 0 roms, output
					if (outDats[j].Files.Count > 0)
					{
						outDats[j].WriteToFile(path, logger);
					}
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output cascading diffs
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void DiffCascade(string outDir, bool inplace, List<string> inputs, List<DatFile> datHeaders, bool skip, Logger logger)
		{
			string post = "";

			// Create a list of DatData objects representing output files
			List<DatFile> outDats = new List<DatFile>();

			// Loop through each of the inputs and get or create a new DatData object
			DateTime start = DateTime.Now;
			logger.User("Initializing all output DATs");

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
					diffData = (DatFile)CloneHeader();
					diffData.FileName += post;
					diffData.Name += post;
					diffData.Description += post;
				}
				diffData.Files = new SortedDictionary<string, List<DatItem>>();

				outDatsArray[j] = diffData;
			});

			outDats = outDatsArray.ToList();
			logger.User("Initializing complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Now, loop through the dictionary and populate the correct DATs
			start = DateTime.Now;
			logger.User("Populating all output DATs");
			List<string> keys = Files.Keys.ToList();

			foreach (string key in keys)
			{
				List<DatItem> roms = DatItem.Merge(Files[key], logger);

				if (roms != null && roms.Count > 0)
				{
					foreach (DatItem rom in roms)
					{
						// There's odd cases where there are items with System ID < 0. Skip them for now
						if (rom.SystemID < 0)
						{
							logger.Warning("Item found with a <0 SystemID: " + rom.Name);
							continue;
						}

						if (outDats[rom.SystemID].Files.ContainsKey(key))
						{
							outDats[rom.SystemID].Files[key].Add(rom);
						}
						else
						{
							List<DatItem> tl = new List<DatItem>();
							tl.Add(rom);
							outDats[rom.SystemID].Files.Add(key, tl);
						}
					}
				}
			}
			logger.User("Populating complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));

			// Finally, loop through and output each of the DATs
			start = DateTime.Now;
			logger.User("Outputting all created DATs");
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

				// If we have more than 0 roms, output
				if (outDats[j].Files.Count > 0)
				{
					outDats[j].WriteToFile(path, logger);
				}
			}
			logger.User("Outputting complete in " + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.fffff"));
		}

		/// <summary>
		/// Output user defined merge
		/// </summary>
		/// <param name="outDir">Output directory to write the DATs to</param>
		/// <param name="inputs">List of inputs to write out from</param>
		/// <param name="datHeaders">Dat headers used optionally</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void MergeNoDiff(string outDir, List<string> inputs, List<DatFile> datHeaders, Logger logger)
		{
			// If we're in SuperDAT mode, prefix all games with their respective DATs
			if (Type == "SuperDAT")
			{
				List<string> keys = Files.Keys.ToList();
				foreach (string key in keys)
				{
					List<DatItem> newroms = new List<DatItem>();
					foreach (DatItem rom in Files[key])
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
					Files[key] = newroms;
				}
			}

			// Output a DAT only if there are roms
			if (Files.Count != 0)
			{
				WriteToFile(outDir, logger);
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
		/// <param name="softlist">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
		/// <param name="filter">Filter object to be passed to the DatItem level</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logging object for console and file output</param>
		public void Update(List<string> inputFileNames, string outDir, bool clean, bool softlist, Filter filter,
			bool trim, bool single, string root, int maxDegreeOfParallelism, Logger logger)
		{
			Parallel.ForEach(inputFileNames,
				new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
				inputFileName =>
				{
					// Clean the input string
					if (inputFileName != "")
					{
						inputFileName = Path.GetFullPath(inputFileName);
					}

					if (File.Exists(inputFileName))
					{
						DatFile innerDatdata = (DatFile)CloneHeader();
						logger.User("Processing \"" + Path.GetFileName(inputFileName) + "\"");
						innerDatdata.Parse(inputFileName, 0, 0, filter, trim, single,
							root, logger, true, clean, softlist,
							keepext: ((innerDatdata.DatFormat & DatFormat.TSV) != 0 || (innerDatdata.DatFormat & DatFormat.CSV) != 0));

						// If we have roms, output them
						if (innerDatdata.Files.Count != 0)
						{
							innerDatdata.WriteToFile((outDir == "" ? Path.GetDirectoryName(inputFileName) : outDir), logger, overwrite: (outDir != ""));
						}
					}
					else if (Directory.Exists(inputFileName))
					{
						inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

						Parallel.ForEach(Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories),
							new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
							file =>
							{
								logger.User("Processing \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
								DatFile innerDatdata = (DatFile)Clone();
								innerDatdata.Files = null;
								innerDatdata.Parse(file, 0, 0, filter,
									trim, single, root, logger, true, clean, softlist,
									keepext: ((innerDatdata.DatFormat & DatFormat.TSV) != 0 || (innerDatdata.DatFormat & DatFormat.CSV) != 0));

							// If we have roms, output them
							if (innerDatdata.Files != null && innerDatdata.Files.Count != 0)
								{
									innerDatdata.WriteToFile((outDir == "" ? Path.GetDirectoryName(file) : outDir + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)), logger, overwrite: (outDir != ""));
								}
							});
					}
					else
					{
						logger.Error("I'm sorry but " + inputFileName + " doesn't exist!");
						return;
					}
				});
		}

		#endregion
	}
}
