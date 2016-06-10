using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.Sqlite;

using SabreTools.Helper;

namespace SabreTools
{
	public partial class DATabase
	{
		#region Init Methods

		/// <summary>
		/// Wrap importing and updating DATs
		/// </summary>
		/// <param name="ignore"></param>
		private static void InitImport(bool ignore)
		{
			IImport imp = new ImportTwo(_datroot, _connectionString, _logger, ignore);
			imp.UpdateDatabase();
		}

		/// <summary>
		/// Wrap generating a DAT from the library
		/// </summary>
		/// <param name="system">System ID to be used in the DAT (blank means all)</param>
		/// <param name="norename">True if files should not be renamed with system and/or source in merged mode (default false)</param>
		/// <param name="old">True if the output file should be in ClrMamePro format (default false)</param>
		private static void InitGenerate(string systemid, bool norename, bool old)
		{
			IGenerate gen = new GenerateTwo(systemid, "" /* sourceid */, _datroot, _outroot, _connectionString, _logger, norename, old);
			gen.Export();
		}

		/// <summary>
		/// Wrap generating all standard DATs from the library
		/// </summary>
		private static void InitGenerateAll(bool norename, bool old)
		{
			List<string> systems = new List<string>();
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();

				string query = "SELECT id FROM system";
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						// If nothing is found, tell the user and exit
						if (!sldr.HasRows)
						{
							_logger.Warning("No systems found! Please add a system and then try again.");
							return;
						}

						while (sldr.Read())
						{
							systems.Add(sldr.GetInt32(0).ToString());
						}
					}
				}

				// Loop through the inputs
				foreach (string system in systems)
				{
					_logger.User("Generating DAT for system id " + system);
					InitGenerate(system, norename, old);
				}
			}
		}

		/// <summary>
		/// Wrap converting and updating DAT file from any format to any format
		/// </summary>
		/// <param name="input">Input filename</param>
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="category">New category</param>
		/// <param name="version">New version</param>
		/// <param name="date">New date</param>
		/// <param name="author">New author</param>
		/// <param name="email">New email</param>
		/// <param name="homepage">New homepage</param>
		/// <param name="url">New URL</param>
		/// <param name="comment">New comment</param>
		/// <param name="header">New header</param>
		/// <param name="superdat">True to set SuperDAT type, false otherwise</param>
		/// <param name="forcemerge">None, Split, Full</param>
		/// <param name="forcend">None, Obsolete, Required, Ignore</param>
		/// <param name="forcepack">None, Zip, Unzip</param>
		/// <param name="outputCMP">True to output to ClrMamePro format</param>
		/// <param name="outputMiss">True to output to Missfile format</param>
		/// <param name="outputRC">True to output to RomCenter format</param>
		/// <param name="outputSD">True to output to SabreDAT format</param>
		/// <param name="outputXML">True to output to Logiqx XML format</param>
		/// <param name="usegame">True if games are to be used in output, false if roms are</param>
		/// <param name="prefix">Generic prefix to be added to each line</param>
		/// <param name="postfix">Generic postfix to be added to each line</param>
		/// <param name="quotes">Add quotes to each item</param>
		/// <param name="repext">Replace all extensions with another</param>
		/// <param name="addext">Add an extension to all items</param>
		/// <param name="gamename">Add the dat name as a directory prefix</param>
		/// <param name="romba">Output files in romba format</param>
		/// <param name="tsv">Output files in TSV format</param>
		/// <param name="outdir">Optional param for output directory</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		private static void InitUpdate(string input,
			string filename,
			string name,
			string description,
			string category,
			string version,
			string date,
			string author,
			string email,
			string homepage,
			string url,
			string comment,
			string header,
			bool superdat,
			string forcemerge,
			string forcend,
			string forcepack,
			bool outputCMP,
			bool outputMiss,
			bool outputRC,
			bool outputSD,
			bool outputXML,
			bool usegame,
			string prefix,
			string postfix,
			bool quotes,
			string repext,
			string addext,
			bool gamename,
			bool romba,
			bool tsv,
			string outdir,
			bool clean)
		{
			// Set the special flags
			ForceMerging fm = ForceMerging.None;
			switch (forcemerge.ToLowerInvariant())
			{
				case "none":
				default:
					fm = ForceMerging.None;
					break;
				case "split":
					fm = ForceMerging.Split;
					break;
				case "full":
					fm = ForceMerging.Full;
					break;
			}

			ForceNodump fn = ForceNodump.None;
			switch (forcend.ToLowerInvariant())
			{
				case "none":
				default:
					fn = ForceNodump.None;
					break;
				case "obsolete":
					fn = ForceNodump.Obsolete;
					break;
				case "required":
					fn = ForceNodump.Required;
					break;
				case "ignore":
					fn = ForceNodump.Ignore;
					break;
			}

			ForcePacking fp = ForcePacking.None;
			switch (forcepack.ToLowerInvariant())
			{
				case "none":
				default:
					fp = ForcePacking.None;
					break;
				case "zip":
					fp = ForcePacking.Zip;
					break;
				case "unzip":
					fp = ForcePacking.Unzip;
					break;
			}

			// Normalize the extensions
			addext = (addext == "" || addext.StartsWith(".") ? addext : "." + addext);
			repext = (repext == "" || repext.StartsWith(".") ? repext : "." + repext);

			// Populate the DatData object
			DatData userInputDat = new DatData
			{
				FileName = filename,
				Name = name,
				Description = description,
				Category = category,
				Version = version,
				Date = date,
				Author = author,
				Email = email,
				Homepage = homepage,
				Url = url,
				Comment = comment,
				Header = header,
				Type = (superdat ? "SuperDAT" : null),
				ForceMerging = fm,
				ForceNodump = fn,
				ForcePacking = fp,
				MergeRoms = false,

				UseGame = usegame,
				Prefix = prefix,
				Postfix = postfix,
				Quotes = quotes,
				RepExt = repext,
				AddExt = addext,
				GameName = gamename,
				Romba = romba,
				TSV = tsv,
			};

			if (outputCMP)
			{
				userInputDat.OutputFormat = OutputFormat.ClrMamePro;
				InitUpdate(input, userInputDat, outdir, clean);
			}
			if (outputMiss || romba)
			{
				userInputDat.OutputFormat = OutputFormat.MissFile;
				InitUpdate(input, userInputDat, outdir, clean);
			}
			if (outputRC)
			{
				userInputDat.OutputFormat = OutputFormat.RomCenter;
				InitUpdate(input, userInputDat, outdir, clean);
			}
			if (outputSD)
			{
				userInputDat.OutputFormat = OutputFormat.SabreDat;
				InitUpdate(input, userInputDat, outdir, clean);
			}
			if (outputXML)
			{
				userInputDat.OutputFormat = OutputFormat.Xml;
				InitUpdate(input, userInputDat, outdir, clean);
			}
			if (!outputCMP && !(outputMiss || romba) && !outputRC && !outputSD && !outputXML)
			{
				InitUpdate(input, userInputDat, outdir, clean);
			}
		}

		/// <summary>
		/// Wrap converting and updating DAT file from any format to any format
		/// </summary>
		/// <param name="inputFileName">Name of the input file or folder</param>
		/// <param name="datdata">User specified inputs contained in a DatData object</param>
		/// <param name="outputDirectory">Optional param for output directory</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		private static void InitUpdate(string inputFileName, DatData datdata, string outputDirectory, bool clean = false)
		{
			// Clean the input strings
			outputDirectory = outputDirectory.Replace("\"", "");
			if (outputDirectory != "")
			{
				outputDirectory = Path.GetFullPath(outputDirectory) + Path.DirectorySeparatorChar;
			}
			inputFileName = inputFileName.Replace("\"", "");

			if (File.Exists(inputFileName))
			{
				_logger.User("Converting \"" + Path.GetFileName(inputFileName) + "\"");
				datdata = RomManipulation.Parse(inputFileName, 0, 0, datdata, _logger, true, clean);

				// If the extension matches, append ".new" to the filename
				string extension = (datdata.OutputFormat == OutputFormat.Xml || datdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
				if (outputDirectory == "" && Path.GetExtension(inputFileName) == extension)
				{
					datdata.FileName += ".new";
				}

				Output.WriteDatfile(datdata, (outputDirectory == "" ? Path.GetDirectoryName(inputFileName) : outputDirectory), _logger);
			}
			else if (Directory.Exists(inputFileName))
			{
				inputFileName = Path.GetFullPath(inputFileName) + Path.DirectorySeparatorChar;

				foreach (string file in Directory.EnumerateFiles(inputFileName, "*", SearchOption.AllDirectories))
				{
					_logger.User("Converting \"" + Path.GetFullPath(file).Remove(0, inputFileName.Length) + "\"");
					DatData innerDatdata = (DatData)datdata.Clone();
					innerDatdata.Roms = null;
					innerDatdata = RomManipulation.Parse(file, 0, 0, innerDatdata, _logger, true, clean);

					// If the extension matches, append ".new" to the filename
					string extension = (innerDatdata.OutputFormat == OutputFormat.Xml || innerDatdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
					if (outputDirectory == "" && Path.GetExtension(file) == extension)
					{
						innerDatdata.FileName += ".new";
					}

					Output.WriteDatfile(innerDatdata, (outputDirectory == "" ? Path.GetDirectoryName(file) : outputDirectory + Path.GetDirectoryName(file).Remove(0, inputFileName.Length - 1)), _logger);
				}
			}
			else
			{
				_logger.Error("I'm sorry but " + inputFileName + " doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// Wrap trimming and merging a single DAT
		/// </summary>
		/// <param name="input">Input file or folder to be converted</param>
		/// <param name="root">Root directory to base path lengths on</param>
		/// <param name="rename">True is games should not be renamed</param>
		/// <param name="force">True if forcepacking="unzip" should be included</param>
		private static void InitTrimMerge(string input, string root, bool rename, bool force)
		{
			// Strip any quotations from the name
			input = input.Replace("\"", "");

			if (input != "" && (File.Exists(input) || Directory.Exists(input)))
			{
				TrimMerge sg = new TrimMerge(input, root, rename, force, _logger);
				sg.Process();
				return;
			}
		}

		/// <summary>
		/// Wrap merging, diffing, and deduping 2 or mor DATs
		/// </summary>
		/// <param name="inputs">A List of Strings representing the DATs or DAT folders to be merged</param>
		/// <param name="name">Internal name of the DAT</param>
		/// <param name="desc">Description and external name of the DAT</param>
		/// <param name="cat">Category for the DAT</param>
		/// <param name="version">Version of the DAT</param>
		/// <param name="author">Author of the DAT</param>
		/// <param name="diff">True if a DiffDat of all inputs is wanted, false otherwise</param>
		/// <param name="dedup">True if the outputted file should remove duplicates, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="forceunpack">True if the forcepacking="unzip" tag is to be added, false otherwise</param>
		/// <param name="old">True if a old-style DAT should be output, false otherwise</param>
		/// <param name="superdat">True if DATs should be merged in SuperDAT style, false otherwise</param>
		/// <param name="cascade">True if the outputted diffs should be cascaded, false otherwise</param>
		/// <param name="inplace">True if cascaded diffs overwrite the source files, false otherwise</param>
		/// <param name="outdir">Output directory for the files (blank is default)</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		private static void InitMergeDiff(List<string> inputs, string name, string desc, string cat, string version, string author,
			bool diff, bool dedup, bool bare, bool forceunpack, bool old, bool superdat, bool cascade, bool inplace, string outdir = "", bool clean = false)
		{
			// Make sure there are no folders in inputs
			List<string> newInputs = new List<string>();
			foreach (string input in inputs)
			{
				if (Directory.Exists(input.Replace("\"", "")))
				{
					foreach (string file in Directory.EnumerateFiles(input.Replace("\"", ""), "*", SearchOption.AllDirectories))
					{
						try
						{
							newInputs.Add(Path.GetFullPath(file) + "¬" + Path.GetFullPath(input.Replace("\"", "")));
						}
						catch (PathTooLongException)
						{
							_logger.Warning("The path for " + file + " was too long");
						}
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}
					}
				}
				else if (File.Exists(input.Replace("\"", "")))
				{
					try
					{
						newInputs.Add(Path.GetFullPath(input.Replace("\"", "")) + "¬" + Path.GetDirectoryName(Path.GetFullPath(input.Replace("\"", ""))));
					}
					catch (PathTooLongException)
					{
						_logger.Warning("The path for " + input.Replace("\"", "") + " was too long");
					}
					catch (Exception ex)
					{
						_logger.Error(ex.ToString());
					}
				}
			}

			MergeDiff md = new MergeDiff(newInputs, name, desc, cat, version, author, diff, dedup, bare, forceunpack, old, superdat, cascade, inplace, outdir, clean, _logger);
			md.Process();
		}

		/// <summary>
		/// Wrap splitting a DAT by 2 extensions
		/// </summary>
		/// <param name="input">Input file or folder to be split</param>
		/// <param name="exta">First extension to split on</param>
		/// <param name="extb">Second extension to split on</param>
		/// <param name="outdir">Output directory for the split files</param>
		private static void InitExtSplit(string input, string exta, string extb, string outdir)
		{
			// Strip any quotations from the names
			input = input.Replace("\"", "");
			exta = exta.Replace("\"", "");
			extb = extb.Replace("\"", "");
			outdir = outdir.Replace("\"", "");

			if (input != "" && File.Exists(input))
			{
				if (exta == "" || extb == "")
				{
					_logger.Warning("Two extensions are needed to split a DAT!");
					return;
				}
				ExtSplit es = new ExtSplit(input, exta, extb, outdir, _logger);
				es.Split();
				return;
			}
			else
			{
				_logger.Log("I'm sorry but " + input + "doesn't exist!");
			}
		}

		/// <summary>
		/// Wrap splitting a DAT by best available hashes
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outdir">Output directory for the split files</param>
		private static void InitHashSplit(List<string> inputs, string outdir)
		{
			// Strip any quotations from the names
			outdir = outdir.Replace("\"", "");

			// Verify the input files
			foreach (string input in inputs)
			{
				if (!File.Exists(input.Replace("\"", "")) && !Directory.Exists(input.Replace("\"", "")))
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// If so, run the program
			HashSplit hs = new HashSplit(inputs, outdir, _logger);
			hs.Split();
		}

		/// <summary>
		/// Wrap getting statistics on a DAT or folder of DATs
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="single">True to show individual DAT statistics, false otherwise</param>
		private static void InitStats(List<string> inputs, bool single)
		{
			List<string> newinputs = new List<string>();

			foreach (string input in inputs)
			{
				if (File.Exists(input.Replace("\"", "")))
				{
					newinputs.Add(input.Replace("\"", ""));
				}
				if (Directory.Exists(input.Replace("\"", "")))
				{
					foreach (string file in Directory.GetFiles(input.Replace("\"", ""), "*", SearchOption.AllDirectories))
					{
						newinputs.Add(file.Replace("\"", ""));
					}
				}
			}

			Logger statlog = new Logger(true, "stats.txt");
			statlog.Start();
			Stats stats = new Stats(newinputs, single, statlog);
			stats.Process();
			statlog.Close();
		}

		/// <summary>
		/// Wrap adding a new source to the database
		/// </summary>
		/// <param name="name">Source name</param>
		/// <param name="url">Source URL(s)</param>
		private static void InitAddSource(string name, string url)
		{
			if (DBTools.AddSource(name, url, _connectionString))
			{
				_logger.Log("Source " + name + " added!");
			}
			else
			{
				_logger.Error("Source " + name + " could not be added!");
			}
		}

		/// <summary>
		/// Wrap removing an existing source from the database
		/// </summary>
		/// <param name="id">Source ID to be removed from the database</param>
		private static void InitRemoveSource(string sourceid)
		{
			int srcid = -1;
			if (Int32.TryParse(sourceid, out srcid))
			{
				if (DBTools.RemoveSource(srcid, _connectionString))
				{
					_logger.Log("Source '" + srcid + "' removed!");
				}
				else
				{
					_logger.Error("Source with id '" + srcid + "' could not be removed.");
				}
			}
			else
			{
				_logger.Error("Invalid input");
			}
		}

		/// <summary>
		/// Wrap adding a new system to the database
		/// </summary>
		/// <param name="manufacturer">Manufacturer name</param>
		/// <param name="system">System name</param>
		private static void InitAddSystem(string manufacturer, string system)
		{
			if (DBTools.AddSystem(manufacturer, system, _connectionString))
			{
				_logger.Log("System " + manufacturer + " - " + system + " added!");
			}
			else
			{
				_logger.Error("System " + manufacturer + " - " + system + " could not be added!");
			}
		}

		/// <summary>
		/// Wrap removing an existing system from the database
		/// </summary>
		/// <param name="id">System ID to be removed from the database</param>
		private static void InitRemoveSystem(string systemid)
		{
			int sysid = -1;
			if (Int32.TryParse(systemid, out sysid))
			{
				if (DBTools.RemoveSystem(sysid, _connectionString))
				{
					_logger.Log("System '" + sysid + "' removed!");
				}
				else
				{
					_logger.Error("System with id '" + sysid + "' could not be removed.");
				}
			}
			else
			{
				_logger.Error("Invalid input");
			}
		}

		#endregion

		#region OBSOLETE

		/// <summary>
		/// Wrap converting DAT file from any format to any format
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="outputFormat"></param>
		/// <param name="outdir">Optional param for output directory</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		private static void InitConvert(string filename, OutputFormat outputFormat, string outdir, bool clean)
		{
			// Clean the input strings
			outdir = outdir.Replace("\"", "");
			if (outdir != "")
			{
				outdir = Path.GetFullPath(outdir) + Path.DirectorySeparatorChar;
			}
			filename = filename.Replace("\"", "");

			if (File.Exists(filename))
			{
				_logger.User("Converting \"" + Path.GetFileName(filename) + "\"");
				DatData datdata = new DatData
				{
					OutputFormat = outputFormat,
					MergeRoms = false,
				};
				datdata = RomManipulation.Parse(filename, 0, 0, datdata, _logger, true, clean);

				// If the extension matches, append ".new" to the filename
				string extension = (datdata.OutputFormat == OutputFormat.Xml || datdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
				if (outdir == "" && Path.GetExtension(filename) == extension)
				{
					datdata.FileName += ".new";
				}

				Output.WriteDatfile(datdata, (outdir == "" ? Path.GetDirectoryName(filename) : outdir), _logger);
			}
			else if (Directory.Exists(filename))
			{
				filename = Path.GetFullPath(filename) + Path.DirectorySeparatorChar;

				foreach (string file in Directory.EnumerateFiles(filename, "*", SearchOption.AllDirectories))
				{
					_logger.User("Converting \"" + Path.GetFullPath(file).Remove(0, filename.Length) + "\"");
					DatData datdata = new DatData
					{
						OutputFormat = outputFormat,
						MergeRoms = false,
					};
					datdata = RomManipulation.Parse(file, 0, 0, datdata, _logger, true, clean);

					// If the extension matches, append ".new" to the filename
					string extension = (datdata.OutputFormat == OutputFormat.Xml || datdata.OutputFormat == OutputFormat.SabreDat ? ".xml" : ".dat");
					if (outdir == "" && Path.GetExtension(file) == extension)
					{
						datdata.FileName += ".new";
					}

					Output.WriteDatfile(datdata, (outdir == "" ? Path.GetDirectoryName(file) : outdir + Path.GetDirectoryName(file).Remove(0, filename.Length - 1)), _logger);
				}
			}
			else
			{
				_logger.Error("I'm sorry but " + filename + " doesn't exist!");
			}
			return;
		}

		/// <summary>
		/// Wrap converting a DAT to missfile
		/// </summary>
		/// <param name="input">File to be converted</param>
		/// <param name="usegame">True if games are to be used in output, false if roms are</param>
		/// <param name="prefix">Generic prefix to be added to each line</param>
		/// <param name="postfix">Generic postfix to be added to each line</param>
		/// <param name="quotes">Add quotes to each item</param>
		/// <param name="repext">Replace all extensions with another</param>
		/// <param name="addext">Add an extension to all items</param>
		/// <param name="gamename">Add the dat name as a directory prefix</param>
		/// <param name="romba">Output files in romba format</param>
		/// <param name="tsv">Output files in TSV format</param>
		private static void InitConvertMiss(string input, bool usegame, string prefix, string postfix, bool quotes,
			string repext, string addext, bool gamename, bool romba, bool tsv)
		{
			// Strip any quotations from the name
			input = input.Replace("\"", "");

			if (input != "" && File.Exists(input))
			{
				// Get the full input name
				input = Path.GetFullPath(input);

				// Get the output name
				string name = Path.GetFileNameWithoutExtension(input) + "-miss";

				// Read in the roms from the DAT and then write them to the file
				_logger.User("Converting " + input);
				DatData datdata = new DatData
				{
					OutputFormat = OutputFormat.MissFile,

					UseGame = usegame,
					Prefix = prefix,
					Postfix = postfix,
					AddExt = addext,
					RepExt = repext,
					Quotes = quotes,
					GameName = gamename,
					Romba = romba,
					TSV = tsv,
				};
				datdata = RomManipulation.Parse(input, 0, 0, datdata, _logger);
				datdata.FileName += "-miss";
				datdata.Name += "-miss";
				datdata.Description += "-miss";

				// Normalize the extensions
				addext = (addext == "" || addext.StartsWith(".") ? addext : "." + addext);
				repext = (repext == "" || repext.StartsWith(".") ? repext : "." + repext);

				Output.WriteDatfile(datdata, Path.GetDirectoryName(input), _logger);
				_logger.User(input + " converted to: " + name);
				return;
			}
			else
			{
				_logger.Error("I'm sorry but " + input + "doesn't exist!");
			}
		}

		#endregion
	}
}
