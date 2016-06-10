using System.IO;
using SabreTools.Helper;

namespace SabreTools
{
	public partial class DATabase
	{
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
		public static void InitUpdate(string input,
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
		public static void InitUpdate(string inputFileName, DatData datdata, string outputDirectory, bool clean = false)
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
