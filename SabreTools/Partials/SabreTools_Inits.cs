using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;

using SabreTools.Helper;

namespace SabreTools
{
	public partial class SabreTools
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
		/// /* Normal DAT header info */
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
		/// /* Missfile-specific DAT info */
		/// <param name="usegame">True if games are to be used in output, false if roms are</param>
		/// <param name="prefix">Generic prefix to be added to each line</param>
		/// <param name="postfix">Generic postfix to be added to each line</param>
		/// <param name="quotes">Add quotes to each item</param>
		/// <param name="repext">Replace all extensions with another</param>
		/// <param name="addext">Add an extension to all items</param>
		/// <param name="datprefix">Add the dat name as a directory prefix</param>
		/// <param name="romba">Output files in romba format</param>
		/// <param name="tsv">Output files in TSV format</param>
		/// /* Filtering info */
		/// <param name="gamename">Name of the game to match (can use asterisk-partials)</param>
		/// <param name="romname">Name of the rom to match (can use asterisk-partials)</param>
		/// <param name="romtype">Type of the rom to match</param>
		/// <param name="sgt">Find roms greater than or equal to this size</param>
		/// <param name="slt">Find roms less than or equal to this size</param>
		/// <param name="seq">Find roms equal to this size</param>
		/// <param name="crc">CRC of the rom to match (can use asterisk-partials)</param>
		/// <param name="md5">MD5 of the rom to match (can use asterisk-partials)</param>
		/// <param name="sha1">SHA-1 of the rom to match (can use asterisk-partials)</param>
		/// <param name="nodump">Select roms with nodump status as follows: null (match all), true (match Nodump only), false (exclude Nodump)</param>
		/// /* Output DAT info */
		/// <param name="outdir">Optional param for output directory</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="dedup">True to dedupe the roms in the DAT, false otherwise (default)</param>
		private static void InitUpdate(string input,
			/* Normal DAT header info */
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

			/* Missfile-specific DAT info */
			bool usegame,
			string prefix,
			string postfix,
			bool quotes,
			string repext,
			string addext,
			bool datprefix,
			bool romba,
			bool tsv,

			/* Filtering info */
			string gamename,
			string romname,
			string romtype,
			long sgt,
			long slt,
			long seq,
			string crc,
			string md5,
			string sha1,
			bool? nodump,

			/* Output DAT info */
			string outdir,
			bool clean,
			bool dedup)
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
				MergeRoms = dedup,

				UseGame = usegame,
				Prefix = prefix,
				Postfix = postfix,
				Quotes = quotes,
				RepExt = repext,
				AddExt = addext,
				GameName = datprefix,
				Romba = romba,
				TSV = tsv,
			};

			if (outputCMP)
			{
				userInputDat.OutputFormat = OutputFormat.ClrMamePro;
				DatTools.Update(input, userInputDat, outdir, clean, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, _logger);
			}
			if (outputMiss || romba)
			{
				userInputDat.OutputFormat = OutputFormat.MissFile;
				DatTools.Update(input, userInputDat, outdir, clean, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, _logger);
			}
			if (outputRC)
			{
				userInputDat.OutputFormat = OutputFormat.RomCenter;
				DatTools.Update(input, userInputDat, outdir, clean, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, _logger);
			}
			if (outputSD)
			{
				userInputDat.OutputFormat = OutputFormat.SabreDat;
				DatTools.Update(input, userInputDat, outdir, clean, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, _logger);
			}
			if (outputXML)
			{
				userInputDat.OutputFormat = OutputFormat.Xml;
				DatTools.Update(input, userInputDat, outdir, clean, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, _logger);
			}
			if (!outputCMP && !(outputMiss || romba) && !outputRC && !outputSD && !outputXML)
			{
				DatTools.Update(input, userInputDat, outdir, clean, gamename, romname, romtype, sgt, slt, seq, crc, md5, sha1, nodump, _logger);
			}
		}

		/// <summary>
		/// Wrap creating a DAT file from files or a directory
		/// </summary>
		/// <param name="input">List of innput filenames</param>
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="category">New category</param>
		/// <param name="version">New version</param>
		/// <param name="author">New author</param>
		/// <param name="forceunpack">True to set forcepacking="unzip" on the created file, false otherwise</param>
		/// <param name="old">True to output in CMP format, false to output in Logiqx XML</param>
		/// <param name="romba">True to enable reading a directory like a Romba depot, false otherwise</param>
		/// <param name="superdat">True to enable SuperDAT-style reading, false otherwise</param>
		/// <param name="noMD5">True to disable getting MD5 hash, false otherwise</param>
		/// <param name="noSHA1">True to disable getting SHA-1 hash, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory</param>
		private static void InitDatFromDir(List<string> inputs,
			string filename,
			string name,
			string description,
			string category,
			string version,
			string author,
			bool forceunpack,
			bool old,
			bool romba,
			bool superdat,
			bool noMD5,
			bool noSHA1,
			bool bare,
			bool archivesAsFiles,
			bool enableGzip,
			string tempDir)
		{
			// Create a new DATFromDir object and process the inputs
			DatData datdata = new DatData
			{
				FileName = description,
				Name = name,
				Description = description,
				Category = category,
				Version = version,
				Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Author = author,
				ForcePacking = (forceunpack ? ForcePacking.Unzip : ForcePacking.None),
				OutputFormat = (old ? OutputFormat.ClrMamePro : OutputFormat.Xml),
				Romba = romba,
				Type = (superdat ? "SuperDAT" : ""),
				Roms = new Dictionary<string, List<RomData>>(),
			};
			DATFromDir dfd = new DATFromDir(inputs, datdata, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, tempDir, _logger);
			bool success = dfd.Start();

			// If we failed, show the help
			if (!success)
			{
				Console.WriteLine();
				Build.Help();
			}
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
				if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						try
						{
							newInputs.Add(Path.GetFullPath(file) + "¬" + Path.GetFullPath(input));
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
				else if (File.Exists(input))
				{
					try
					{
						newInputs.Add(Path.GetFullPath(input) + "¬" + Path.GetDirectoryName(Path.GetFullPath(input)));
					}
					catch (PathTooLongException)
					{
						_logger.Warning("The path for " + input + " was too long");
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
		/// <param name="inputs">Input files or folders to be split</param>
		/// <param name="exta">First extension to split on</param>
		/// <param name="extb">Second extension to split on</param>
		/// <param name="outdir">Output directory for the split files</param>
		private static void InitExtSplit(List<string> inputs, string exta, string extb, string outdir)
		{
			// Verify the input files
			foreach (string input in inputs)
			{
				if (!File.Exists(input) && !Directory.Exists(input))
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// Convert comma-separated strings to list
			List<string> extaList = exta.Split(',').ToList();
			List<string> extbList = extb.Split(',').ToList();

			Split es = new Split(inputs, extaList, extbList, outdir, _logger);
			es.Process();
			return;
		}

		/// <summary>
		/// Wrap splitting a DAT by best available hashes
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outdir">Output directory for the split files</param>
		private static void InitHashSplit(List<string> inputs, string outdir)
		{
			// Verify the input files
			foreach (string input in inputs)
			{
				if (!File.Exists(input) && !Directory.Exists(input))
				{
					_logger.Error(input + " is not a valid file or folder!");
					Console.WriteLine();
					Build.Help();
					return;
				}
			}

			// If so, run the program
			Split hs = new Split(inputs, outdir, _logger);
			hs.Process();
		}

		/// <summary>
		/// Wrap creating an Offline merged DAT set
		/// </summary>
		/// <param name="currentAllMerged">Old-current DAT with merged and deduped values</param>
		/// <param name="currentMissingMerged">Old-current missing DAT with merged and deduped values</param>
		/// <param name="currentNewMerged">New-current DAT with merged and deduped values</param>
		/// <param name="fake">True if all values should be replaced with default 0-byte values, false otherwise</param>
		private static void InitOfflineMerge(string currentAllMerged, string currentMissingMerged, string currentNewMerged, bool fake)
		{
			OfflineMerge om = new OfflineMerge(currentAllMerged, currentMissingMerged, currentNewMerged, fake, _logger);
			bool success = om.Process();
			if (!success)
			{
				_logger.Warning("At least one complete DAT and the fixdat is needed to run!");
			}
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
				if (File.Exists(input))
				{
					newinputs.Add(input);
				}
				if (Directory.Exists(input))
				{
					foreach (string file in Directory.GetFiles(input, "*", SearchOption.AllDirectories))
					{
						newinputs.Add(file);
					}
				}
			}

			Logger statlog = new Logger(true, "stats.txt");
			statlog.Start();
			Stats stats = new Stats(newinputs, single, statlog);
			stats.Process();
			statlog.Close(true);
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
	}
}
