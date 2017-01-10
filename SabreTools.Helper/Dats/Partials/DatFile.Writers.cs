using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using MemoryStream = System.IO.MemoryStream;
using StreamWriter = System.IO.StreamWriter;
#endif
using NaturalSort;

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Writing [MODULAR DONE]

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datdata">All information for creating the datfile header</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for file output:
		/// - Have the ability to strip special (non-ASCII) characters from rom information
		/// </remarks>
		public bool WriteToFile(string outDir, Logger logger, bool norename = true, bool stats = false, bool ignoreblanks = false, bool overwrite = true)
		{
			// If there's nothing there, abort
			if (Count == 0)
			{
				return false;
			}

			// If output directory is empty, use the current folder
			if (outDir.Trim() == "")
			{
				outDir = Environment.CurrentDirectory;
			}

			// Create the output directory if it doesn't already exist
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// If the DAT has no output format, default to XML
			if (DatFormat == 0)
			{
				DatFormat = DatFormat.Logiqx;
			}

			// Make sure that the three essential fields are filled in
			if (String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				FileName = Name = Description = "Default";
			}
			else if (String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				FileName = Name = Description;
			}
			else if (String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				FileName = Description = Name;
			}
			else if (String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				FileName = Description;
			}
			else if (!String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Name = Description = FileName;
			}
			else if (!String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				Name = Description;
			}
			else if (!String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Description = Name;
			}
			else if (!String.IsNullOrEmpty(FileName) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				// Nothing is needed
			}

			// Output initial statistics, for kicks
			if (stats)
			{
				OutputStats(new Dictionary<StatDatFormat, StreamWriter>(), StatDatFormat.None, logger, recalculate: (RomCount + DiskCount == 0), baddumpCol: true, nodumpCol: true);
			}

			// Bucket roms by game name and optionally dedupe
			BucketByGame(MergeRoms, norename, logger);

			// Get the outfile name
			Dictionary<DatFormat, string> outfiles = Style.CreateOutfileNames(outDir, this, overwrite);

			try
			{
				// Get a properly sorted set of keys
				List<string> keys = Keys.ToList();
				keys.Sort(new NaturalComparer());

				foreach (DatFormat datFormat in outfiles.Keys)
				{
					string outfile = outfiles[datFormat];

					logger.User("Opening file for writing: " + outfile);
					FileStream fs = File.Create(outfile);
					StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(true));

					// Write out the header
					WriteHeader(sw, datFormat, logger);

					// Write out each of the machines and roms
					int depth = 2, last = -1;
					string lastgame = null;
					List<string> splitpath = new List<string>();

					foreach (string key in keys)
					{
						List<DatItem> roms = this[key];

						for (int index = 0; index < roms.Count; index++)
						{
							DatItem rom = roms[index];

							// There are apparently times when a null rom can skip by, skip them
							if (rom.Name == null || rom.Machine.Name == null)
							{
								logger.Warning("Null rom found!");
								continue;
							}

							List<string> newsplit = rom.Machine.Name.Split('\\').ToList();

							// If we have a different game and we're not at the start of the list, output the end of last item
							if (lastgame != null && lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
							{
								depth = WriteEndGame(sw, datFormat, rom, splitpath, newsplit, lastgame, depth, out last, logger);
							}

							// If we have a new game, output the beginning of the new item
							if (lastgame == null || lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
							{
								depth = WriteStartGame(sw, datFormat, rom, newsplit, lastgame, depth, last, logger);
							}

							// If we have a "null" game (created by DATFromDir or something similar), log it to file
							if (rom.Type == ItemType.Rom
								&& ((Rom)rom).Size == -1
								&& ((Rom)rom).CRC == "null"
								&& ((Rom)rom).MD5 == "null"
								&& ((Rom)rom).SHA1 == "null")
							{
								logger.Verbose("Empty folder found: " + rom.Machine.Name);

								// If we're in a mode that doesn't allow for actual empty folders, add the blank info
								if (datFormat != DatFormat.CSV
									&& datFormat != DatFormat.MissFile
									&& datFormat != DatFormat.SabreDat
									&& datFormat != DatFormat.TSV)
								{
									rom.Name = (rom.Name == "null" ? "-" : rom.Name);
									((Rom)rom).Size = Constants.SizeZero;
									((Rom)rom).CRC = Constants.CRCZero;
									((Rom)rom).MD5 = Constants.MD5Zero;
									((Rom)rom).SHA1 = Constants.SHA1Zero;
								}

								// Otherwise, set the new path and such, write out, and continue
								else
								{
									splitpath = newsplit;
									lastgame = rom.Machine.Name;
									continue;
								}
							}

							// Now, output the rom data
							WriteRomData(sw, datFormat, rom, lastgame, depth, logger, ignoreblanks);

							// Set the new data to compare against
							splitpath = newsplit;
							lastgame = rom.Machine.Name;
						}
					}

					// Write the file footer out
					WriteFooter(sw, datFormat, depth, logger);

					logger.Verbose("File written!" + Environment.NewLine);
					sw.Dispose();
					fs.Dispose();
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT header using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datFormat">Output format to write to</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteHeader(StreamWriter sw, DatFormat datFormat, Logger logger)
		{
			try
			{
				string header = "";
				switch (datFormat)
				{
					case DatFormat.AttractMode:
						header = "#Title;Name;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra;Buttons\n";
						break;
					case DatFormat.ClrMamePro:
						header = "clrmamepro (\n" +
							"\tname \"" + Name + "\"\n" +
							"\tdescription \"" + Description + "\"\n" +
							(!String.IsNullOrEmpty(Category) ? "\tcategory \"" + Category + "\"\n" : "") +
							"\tversion \"" + Version + "\"\n" +
							(!String.IsNullOrEmpty(Date) ? "\tdate \"" + Date + "\"\n" : "") +
							"\tauthor \"" + Author + "\"\n" +
							(!String.IsNullOrEmpty(Email) ? "\temail \"" + Email + "\"\n" : "") +
							(!String.IsNullOrEmpty(Homepage) ? "\thomepage \"" + Homepage + "\"\n" : "") +
							(!String.IsNullOrEmpty(Url) ? "\turl \"" + Url + "\"\n" : "") +
							(!String.IsNullOrEmpty(Comment) ? "\tcomment \"" + Comment + "\"\n" : "") +
							(ForcePacking == ForcePacking.Unzip ? "\tforcezipping no\n" : "") +
							(ForcePacking == ForcePacking.Zip ? "\tforcezipping yes\n" : "") +
							(ForceMerging == ForceMerging.Full ? "\tforcemerging full\n" : "") +
							(ForceMerging == ForceMerging.Split ? "\tforcemerging split\n" : "") +
							")\n";
						break;
					case DatFormat.CSV:
						header = "\"File Name\",\"Internal Name\",\"Description\",\"Game Name\",\"Game Description\",\"Type\",\"" +
								"Rom Name\",\"Disk Name\",\"Size\",\"CRC\",\"MD5\",\"SHA1\",\"Nodump\"\n";
						break;
					case DatFormat.DOSCenter:
						header = "DOSCenter (\n" +
							"\tName: " + Name + "\n" +
							"\tDescription: " + Description + "\n" +
							"\tVersion: " + Version + "\n" +
							"\tDate: " + Date + "\n" +
							"\tAuthor: " + Author + "\n" +
							"\tHomepage: " + Homepage + "\n" +
							"\tComment: " + Comment + "\n" +
							")\n";
						break;
					case DatFormat.Logiqx:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(Description) + "</description>\n" +
							(!String.IsNullOrEmpty(RootDir) ? "\t\t<rootdir>" + HttpUtility.HtmlEncode(RootDir) + "</rootdir>\n" : "") +
							(!String.IsNullOrEmpty(Category) ? "\t\t<category>" + HttpUtility.HtmlEncode(Category) + "</category>\n" : "") +
							"\t\t<version>" + HttpUtility.HtmlEncode(Version) + "</version>\n" +
							(!String.IsNullOrEmpty(Date) ? "\t\t<date>" + HttpUtility.HtmlEncode(Date) + "</date>\n" : "") +
							"\t\t<author>" + HttpUtility.HtmlEncode(Author) + "</author>\n" +
							(!String.IsNullOrEmpty(Email) ? "\t\t<email>" + HttpUtility.HtmlEncode(Email) + "</email>\n" : "") +
							(!String.IsNullOrEmpty(Homepage) ? "\t\t<homepage>" + HttpUtility.HtmlEncode(Homepage) + "</homepage>\n" : "") +
							(!String.IsNullOrEmpty(Url) ? "\t\t<url>" + HttpUtility.HtmlEncode(Url) + "</url>\n" : "") +
							(!String.IsNullOrEmpty(Comment) ? "\t\t<comment>" + HttpUtility.HtmlEncode(Comment) + "</comment>\n" : "") +
							(!String.IsNullOrEmpty(Type) ? "\t\t<type>" + HttpUtility.HtmlEncode(Type) + "</type>\n" : "") +
							(ForcePacking != ForcePacking.None || ForceMerging != ForceMerging.None || ForceNodump != ForceNodump.None ?
								"\t\t<clrmamepro" +
									(ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : "") +
									(ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : "") +
									(ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : "") +
									(ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : "") +
									(ForceNodump == ForceNodump.Ignore ? " forceitemStatus=\"ignore\"" : "") +
									(ForceNodump == ForceNodump.Obsolete ? " forceitemStatus=\"obsolete\"" : "") +
									(ForceNodump == ForceNodump.Required ? " forceitemStatus=\"required\"" : "") +
									" />\n"
							: "") +
							"\t</header>\n";
						break;
					case DatFormat.TSV:
						header = "\"File Name\"\t\"Internal Name\"\t\"Description\"\t\"Game Name\"\t\"Game Description\"\t\"Type\"\t\"" +
								"Rom Name\"\t\"Disk Name\"\t\"Size\"\t\"CRC\"\t\"MD5\"\t\"SHA1\"\t\"Nodump\"\n";
						break;
					case DatFormat.OfflineList:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n"
							+ "<dat xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"datas.xsd\">\n"
							+ "\t<configuration>\n"
							+ "\t\t<datName>" + HttpUtility.HtmlEncode(Name) + "</datName>\n"
							+ "\t\t<datVersion>" + Count + "</datVersion>\n"
							+ "\t\t<system>none</system>\n"
							+ "\t\t<screenshotsWidth>240</screenshotsWidth>\n"
							+ "\t\t<screenshotsHeight>160</screenshotsHeight>\n"
							+ "\t\t<infos>\n"
							+ "\t\t\t<title visible=\"false\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<location visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<publisher visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<sourceRom visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<saveType visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<romSize visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t\t<releaseNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<languageNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<comment visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<romCRC visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
							+ "\t\t\t<im1CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
							+ "\t\t\t<im2CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
							+ "\t\t\t<languages visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
							+ "\t\t</infos>\n"
							+ "\t\t<canOpen>\n"
							+ "\t\t\t<extension>.bin</extension>\n"
							+ "\t\t</canOpen>\n"
							+ "\t\t<newDat>\n"
							+ "\t\t\t<datVersionURL>" + HttpUtility.HtmlEncode(Url) + "</datVersionURL>\n"
							+ "\t\t\t<datURL fileName=\"" + HttpUtility.HtmlEncode(FileName) + ".zip\">" + HttpUtility.HtmlEncode(Url) + "</datURL>\n"
							+ "\t\t\t<imURL>" + HttpUtility.HtmlEncode(Url) + "</imURL>\n"
							+ "\t\t</newDat>\n"
							+ "\t\t<search>\n"
							+ "\t\t\t<to value=\"location\" default=\"true\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"romSize\" default=\"true\" auto=\"false\"/>\n"
							+ "\t\t\t<to value=\"languages\" default=\"true\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"saveType\" default=\"false\" auto=\"false\"/>\n"
							+ "\t\t\t<to value=\"publisher\" default=\"false\" auto=\"true\"/>\n"
							+ "\t\t\t<to value=\"sourceRom\" default=\"false\" auto=\"true\"/>\n"
							+ "\t\t</search>\n"
							+ "\t\t<romTitle >%u - %n</romTitle>\n"
							+ "\t</configuration>\n"
							+ "\t<games>\n";
						break;
					case DatFormat.RomCenter:
						header = "[CREDITS]\n" +
							"author=" + Author + "\n" +
							"version=" + Version + "\n" +
							"comment=" + Comment + "\n" +
							"[DAT]\n" +
							"version=2.50\n" +
							"split=" + (ForceMerging == ForceMerging.Split ? "1" : "0") + "\n" +
							"merge=" + (ForceMerging == ForceMerging.Full ? "1" : "0") + "\n" +
							"[EMULATOR]\n" +
							"refname=" + Name + "\n" +
							"version=" + Description + "\n" +
							"[GAMES]\n";
						break;
					case DatFormat.SabreDat:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE sabredat SYSTEM \"newdat.xsd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(Description) + "</description>\n" +
							(!String.IsNullOrEmpty(RootDir) ? "\t\t<rootdir>" + HttpUtility.HtmlEncode(RootDir) + "</rootdir>\n" : "") +
							(!String.IsNullOrEmpty(Category) ? "\t\t<category>" + HttpUtility.HtmlEncode(Category) + "</category>\n" : "") +
							"\t\t<version>" + HttpUtility.HtmlEncode(Version) + "</version>\n" +
							(!String.IsNullOrEmpty(Date) ? "\t\t<date>" + HttpUtility.HtmlEncode(Date) + "</date>\n" : "") +
							"\t\t<author>" + HttpUtility.HtmlEncode(Author) + "</author>\n" +
							(!String.IsNullOrEmpty(Comment) ? "\t\t<comment>" + HttpUtility.HtmlEncode(Comment) + "</comment>\n" : "") +
							(!String.IsNullOrEmpty(Type) || ForcePacking != ForcePacking.None || ForceMerging != ForceMerging.None || ForceNodump != ForceNodump.None ?
								"\t\t<flags>\n" +
									(!String.IsNullOrEmpty(Type) ? "\t\t\t<flag name=\"type\" value=\"" + HttpUtility.HtmlEncode(Type) + "\"/>\n" : "") +
									(ForcePacking == ForcePacking.Unzip ? "\t\t\t<flag name=\"forcepacking\" value=\"unzip\"/>\n" : "") +
									(ForcePacking == ForcePacking.Zip ? "\t\t\t<flag name=\"forcepacking\" value=\"zip\"/>\n" : "") +
									(ForceMerging == ForceMerging.Full ? "\t\t\t<flag name=\"forcemerging\" value=\"full\"/>\n" : "") +
									(ForceMerging == ForceMerging.Split ? "\t\t\t<flag name=\"forcemerging\" value=\"split\"/>\n" : "") +
									(ForceNodump == ForceNodump.Ignore ? "\t\t\t<flag name=\"forceitemStatus\" value=\"ignore\"/>\n" : "") +
									(ForceNodump == ForceNodump.Obsolete ? "\t\t\t<flag name=\"forceitemStatus\" value=\"obsolete\"/>\n" : "") +
									(ForceNodump == ForceNodump.Required ? "\t\t\t<flag name=\"forceitemStatus\" value=\"required\"/>\n" : "") +
									"\t\t</flags>\n"
							: "") +
							"\t</header>\n" +
							"\t<data>\n";
						break;
					case DatFormat.SoftwareList:
						header = "<?xml version=\"1.0\"?>\n" +
							"<!DOCTYPE softwarelist SYSTEM \"softwarelist.dtd\">\n\n" +
							"<softwarelist name=\"" + HttpUtility.HtmlEncode(Name) + "\"" +
								" description=\"" + HttpUtility.HtmlEncode(Description) + "\"" +
								(ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : "") +
								(ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : "") +
								(ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : "") +
								(ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : "") +
								(ForceNodump == ForceNodump.Ignore ? " forceitemStatus=\"ignore\"" : "") +
								(ForceNodump == ForceNodump.Obsolete ? " forceitemStatus=\"obsolete\"" : "") +
								(ForceNodump == ForceNodump.Required ? " forceitemStatus=\"required\"" : "") +
								">\n\n";
						break;
				}

				// Write the header out
				sw.Write(header);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		private int WriteStartGame(StreamWriter sw, DatFormat datFormat, DatItem rom, List<string> newsplit, string lastgame, int depth, int last, Logger logger)
		{
			try
			{
				// No game should start with a path separator
				if (rom.Machine.Name.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.Machine.Name = rom.Machine.Name.Substring(1);
				}

				string state = "";
				switch (datFormat)
				{
					case DatFormat.AttractMode:
						state += rom.Machine.Name + ";"
							+ rom.Machine.Description + ";"
							+ FileName + ";"
							+ rom.Machine.CloneOf + ";"
							+ rom.Machine.Year + ";"
							+ rom.Machine.Manufacturer + ";"
							/* + rom.Machine.Category */ + ";"
							/* + rom.Machine.Players */ + ";"
							/* + rom.Machine.Rotation */ + ";"
							/* + rom.Machine.Control */ + ";"
							/* + rom.Machine.Status */ + ";"
							/* + rom.Machine.DisplayCount */ + ";"
							/* + rom.Machine.DisplayType */ + ";"
							/* + rom.Machine.AltRomname */ + ";"
							/* + rom.Machine.AltTitle */ + ";"
							+ rom.Machine.Comment + ";"
							/* + rom.Machine.Buttons */ + "\n";
						break;
					case DatFormat.ClrMamePro:
						state += "game (\n\tname \"" + rom.Machine.Name + "\"\n" +
							(ExcludeOf ? "" :
								(String.IsNullOrEmpty(rom.Machine.RomOf) ? "" : "\tromof \"" + rom.Machine.RomOf + "\"\n") +
								(String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : "\tcloneof \"" + rom.Machine.CloneOf + "\"\n") +
								(String.IsNullOrEmpty(rom.Machine.SampleOf) ? "" : "\tsampleof \"" + rom.Machine.SampleOf + "\"\n")
							) +
							"\tdescription \"" + (String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description) + "\"\n" +
							(String.IsNullOrEmpty(rom.Machine.Year) ? "" : "\tyear " + rom.Machine.Year + "\n") +
							(String.IsNullOrEmpty(rom.Machine.Manufacturer) ? "" : "\tmanufacturer \"" + rom.Machine.Manufacturer + "\"\n");
						break;
					case DatFormat.DOSCenter:
						state += "game (\n\tname \"" + rom.Machine.Name + ".zip\"\n";
						break;
					case DatFormat.Logiqx:
						state += "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Machine.Name) + "\"" +
								(ExcludeOf ? "" :
									(rom.Machine.MachineType == MachineType.Bios ? " isbios=\"yes\"" : "") +
									(rom.Machine.MachineType == MachineType.Device ? " isdevice=\"yes\"" : "") +
									(rom.Machine.MachineType == MachineType.Mechanical ? " ismechanical=\"yes\"" : "") +
									(rom.Machine.Runnable ? " runnable=\"yes\"" : "") +
									(String.IsNullOrEmpty(rom.Machine.CloneOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.CloneOf.ToLowerInvariant())
										? ""
										: " cloneof=\"" + HttpUtility.HtmlEncode(rom.Machine.CloneOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.RomOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.RomOf.ToLowerInvariant())
										? ""
										: " romof=\"" + HttpUtility.HtmlEncode(rom.Machine.RomOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.SampleOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.SampleOf.ToLowerInvariant())
										? ""
										: " sampleof=\"" + HttpUtility.HtmlEncode(rom.Machine.SampleOf) + "\"")
								) +
								">\n" +
							(String.IsNullOrEmpty(rom.Machine.Comment) ? "" : "\t\t<comment>" + HttpUtility.HtmlEncode(rom.Machine.Comment) + "</comment>\n") +
							"\t\t<description>" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) + "</description>\n" +
							(String.IsNullOrEmpty(rom.Machine.Year) ? "" : "\t\t<year>" + HttpUtility.HtmlEncode(rom.Machine.Year) + "</year>\n") +
							(String.IsNullOrEmpty(rom.Machine.Manufacturer) ? "" : "\t\t<manufacturer>" + HttpUtility.HtmlEncode(rom.Machine.Manufacturer) + "</manufacturer>\n");
						break;
					case DatFormat.SabreDat:
						for (int i = (last == -1 ? 0 : last); i < newsplit.Count; i++)
						{
							for (int j = 0; j < depth - last + i - (lastgame == null ? 1 : 0); j++)
							{
								state += "\t";
							}
							state += "<directory name=\"" + HttpUtility.HtmlEncode(newsplit[i]) + "\" description=\"" +
							HttpUtility.HtmlEncode(newsplit[i]) + "\">\n";
						}
						depth = depth - (last == -1 ? 0 : last) + newsplit.Count;
						break;
					case DatFormat.SoftwareList:
						state += "\t<software name=\"" + HttpUtility.HtmlEncode(rom.Machine.Name) + "\""
							+ (rom.Supported != null ? " supported=\"" + (rom.Supported == true ? "yes" : "no") + "\"" : "") +
							(ExcludeOf ? "" :
									(String.IsNullOrEmpty(rom.Machine.CloneOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.CloneOf.ToLowerInvariant())
										? ""
										: " cloneof=\"" + HttpUtility.HtmlEncode(rom.Machine.CloneOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.RomOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.RomOf.ToLowerInvariant())
										? ""
										: " romof=\"" + HttpUtility.HtmlEncode(rom.Machine.RomOf) + "\"") +
									(String.IsNullOrEmpty(rom.Machine.SampleOf) || (rom.Machine.Name.ToLowerInvariant() == rom.Machine.SampleOf.ToLowerInvariant())
										? ""
										: " sampleof=\"" + HttpUtility.HtmlEncode(rom.Machine.SampleOf) + "\"")
								) + ">\n"
							+ "\t\t<description>" + HttpUtility.HtmlEncode(rom.Machine.Description) + "</description>\n"
							+ (rom.Machine.Year != null ? "\t\t<year>" + HttpUtility.HtmlEncode(rom.Machine.Year) + "</year>\n" : "")
							+ (rom.Publisher != null ? "\t\t<publisher>" + HttpUtility.HtmlEncode(rom.Publisher) + "</publisher>\n" : "");

						foreach (Tuple<string, string> kvp in rom.Infos)
						{
							state += "\t\t<info name=\"" + HttpUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + HttpUtility.HtmlEncode(kvp.Item2) + "\" />\n";
						}
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out Game start using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		private int WriteEndGame(StreamWriter sw, DatFormat datFormat, DatItem rom, List<string> splitpath, List<string> newsplit, string lastgame, int depth, out int last, Logger logger)
		{
			last = 0;

			try
			{
				string state = "";

				switch (datFormat)
				{
					case DatFormat.ClrMamePro:
					case DatFormat.DOSCenter:
						state += (String.IsNullOrEmpty(rom.Machine.SampleOf) ? "" : "\tsampleof \"" + rom.Machine.SampleOf + "\"\n") + ")\n";
						break;
					case DatFormat.Logiqx:
						state += "\t</machine>\n";
						break;
					case DatFormat.OfflineList:
						state += "\t\t</game>\n";
						break;
					case DatFormat.SabreDat:
						if (splitpath != null)
						{
							for (int i = 0; i < newsplit.Count && i < splitpath.Count; i++)
							{
								// Always keep track of the last seen item
								last = i;

								// If we find a difference, break
								if (newsplit[i] != splitpath[i])
								{
									break;
								}
							}

							// Now that we have the last known position, take down all open folders
							for (int i = depth - 1; i > last + 1; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									state += "\t";
								}
								state += "</directory>\n";
							}

							// Reset the current depth
							depth = 2 + last;
						}
						break;
					case DatFormat.SoftwareList:
						state += "\t</software>\n\n";
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return depth;
			}

			return depth;
		}

		/// <summary>
		/// Write out RomData using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datFormat">Output format to write to</param>
		/// <param name="rom">RomData object to be output</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteRomData(StreamWriter sw, DatFormat datFormat, DatItem rom, string lastgame, int depth, Logger logger, bool ignoreblanks = false)
		{
			// If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
			if (ignoreblanks
				&& (rom.Type == ItemType.Rom
				&& (((Rom)rom).Size == 0 || ((Rom)rom).Size == -1)))
			{
				return true;
			}

			try
			{
				string state = "", name = "", pre = "", post = "";
				switch (datFormat)
				{
					case DatFormat.ClrMamePro:
						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\tarchive ( name\"" + rom.Name + "\""
									+ " )\n";
								break;
							case ItemType.BiosSet:
								state += "\tbiosset ( name\"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description \"" + ((BiosSet)rom).Description + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? "default " + ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ " )\n";
								break;
							case ItemType.Disk:
								state += "\tdisk ( name \"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5 " + ((Disk)rom).MD5.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1 " + ((Disk)rom).SHA1.ToLowerInvariant() : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? " flags " + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() : "")
									+ " )\n";
								break;
							case ItemType.Release:
								state += "\trelease ( name\"" + rom.Name + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region \"" + ((Release)rom).Region + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language \"" + ((Release)rom).Language + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date \"" + ((Release)rom).Date + "\"" : "")
									+ (((Release)rom).Default != null
										? "default " + ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ " )\n";
								break;
							case ItemType.Rom:
								state += "\trom ( name \"" + rom.Name + "\""
									+ (((Rom)rom).Size != -1 ? " size " + ((Rom)rom).Size : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc " + ((Rom)rom).CRC.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5 " + ((Rom)rom).MD5.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1 " + ((Rom)rom).SHA1.ToLowerInvariant() : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date \"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? " flags " + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() : "")
									+ " )\n";
								break;
							case ItemType.Sample:
								state += "\tsample ( name\"" + rom.Name + "\""
									+ " )\n";
								break;
						}

						break;
					case DatFormat.CSV:
						// CSV should only output Rom and Disk
						if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
						{
							return true;
						}

						pre = Prefix + (Quotes ? "\"" : "");
						post = (Quotes ? "\"" : "") + Postfix;

						if (rom.Type == ItemType.Rom)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
						}
						else if (rom.Type == ItemType.Disk)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
						}

						if (rom.Type == ItemType.Rom)
						{
							string inline = "\"" + FileName + "\""
								+ ",\"" + Name + "\""
								+ ",\"" + Description + "\""
								+ ",\"" + rom.Machine.Name + "\""
								+ ",\"" + rom.Machine.Description + "\""
								+ "," + "\"rom\""
								+ ",\"" + rom.Name + "\""
								+ "," + "\"\""
								+ ",\"" + ((Rom)rom).Size + "\""
								+ ",\"" + ((Rom)rom).CRC + "\""
								+ ",\"" + ((Rom)rom).MD5 + "\""
								+ ",\"" + ((Rom)rom).SHA1 + "\""
								+ "," + (((Rom)rom).ItemStatus != ItemStatus.None ? "\"" + ((Rom)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							string inline = "\"" + FileName + "\""
								+ ",\"" + Name + "\""
								+ ",\"" + Description + "\""
								+ ",\"" + rom.Machine.Name + "\""
								+ ",\"" + rom.Machine.Description + "\""
								+ "," + "\"disk\""
								+ "," + "\"\""
								+ ",\"" + rom.Name + "\""
								+ "," + "\"\""
								+ "," + "\"\""
								+ ",\"" + ((Disk)rom).MD5 + "\""
								+ ",\"" + ((Disk)rom).SHA1 + "\""
								+ "," + (((Disk)rom).ItemStatus != ItemStatus.None ? "\"" + ((Disk)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						break;
					case DatFormat.DOSCenter:
						switch (rom.Type)
						{
							case ItemType.Archive:
							case ItemType.BiosSet:
							case ItemType.Disk:
							case ItemType.Release:
							case ItemType.Sample:
								// We don't output these at all
								break;
							case ItemType.Rom:
								state += "\tfile ( name " + ((Rom)rom).Name
									+ (((Rom)rom).Size != -1 ? " size " + ((Rom)rom).Size : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date " + ((Rom)rom).Date : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc " + ((Rom)rom).CRC.ToLowerInvariant() : "")
									+ " )\n";
								break;
						}
						break;
					case DatFormat.Logiqx:
						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\t\t<archive name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
							case ItemType.BiosSet:
								state += "\t\t<biosset name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Disk:
								state += "\t\t<disk name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n";
								break;
							case ItemType.Release:
								state += "\t\t<release name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Rom:
								state += "\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n";
								break;
							case ItemType.Sample:
								state += "\t\t<file type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
						}
						break;
					case DatFormat.MissFile:
						// Missfile should only output Rom and Disk
						if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
						{
							return true;
						}

						pre = Prefix + (Quotes ? "\"" : "");
						post = (Quotes ? "\"" : "") + Postfix;

						if (rom.Type == ItemType.Rom)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
						}
						else if (rom.Type == ItemType.Disk)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
						}

						// If we're in Romba mode, the state is consistent
						if (Romba)
						{
							if (rom.Type == ItemType.Rom)
							{
								// We can only write out if there's a SHA-1
								if (((Rom)rom).SHA1 != "")
								{
									name = ((Rom)rom).SHA1.Substring(0, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(2, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(4, 2)
										+ "/" + ((Rom)rom).SHA1.Substring(6, 2)
										+ "/" + ((Rom)rom).SHA1 + ".gz";
									state += pre + name + post + "\n";
								}
							}
							else if (rom.Type == ItemType.Disk)
							{
								// We can only write out if there's a SHA-1
								if (((Disk)rom).SHA1 != "")
								{
									name = ((Disk)rom).SHA1.Substring(0, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(2, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(4, 2)
										+ "/" + ((Disk)rom).SHA1.Substring(6, 2)
										+ "/" + ((Disk)rom).SHA1 + ".gz";
									state += pre + name + post + "\n";
								}
							}
						}

						// Otherwise, use any flags
						name = (UseGame ? rom.Machine.Name : rom.Name);
						if (RepExt != "" || RemExt)
						{
							if (RemExt)
							{
								RepExt = "";
							}

							string dir = Path.GetDirectoryName(name);
							dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
							name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + RepExt);
						}
						if (AddExt != "")
						{
							name += AddExt;
						}
						if (!UseGame && GameName)
						{
							name = Path.Combine(rom.Machine.Name, name);
						}

						if (UseGame && rom.Machine.Name != lastgame)
						{
							state += pre + name + post + "\n";
							lastgame = rom.Machine.Name;
						}
						else if (!UseGame)
						{
							state += pre + name + post + "\n";
						}
						break;
					case DatFormat.OfflineList:
						state += "\t\t<game>\n"
							+ "\t\t\t<imageNumber>1</imageNumber>\n"
							+ "\t\t\t<releaseNumber>1</releaseNumber>\n"
							+ "\t\t\t<title>" + HttpUtility.HtmlEncode(rom.Name) + "</title>\n"
							+ "\t\t\t<saveType>None</saveType>\n";

						if (rom.Type == ItemType.Rom)
						{
							state += "\t\t\t<romSize>" + ((Rom)rom).Size + "</romSize>\n";
						}

						state += "\t\t\t<publisher>None</publisher>\n"
							+ "\t\t\t<location>0</location>\n"
							+ "\t\t\t<sourceRom>None</sourceRom>\n"
							+ "\t\t\t<language>0</language>\n";

						if (rom.Type == ItemType.Disk)
						{
							state += "\t\t\t<files>\n"
								+ (((Disk)rom).MD5 != null
									? "\t\t\t\t<romMD5 extension=\".chd\">" + ((Disk)rom).MD5.ToUpperInvariant() + "</romMD5>\n"
									: "\t\t\t\t<romSHA1 extension=\".chd\">" + ((Disk)rom).SHA1.ToUpperInvariant() + "</romSHA1>\n")
								+ "\t\t\t</files>\n";
						}
						else if (rom.Type == ItemType.Rom)
						{
							string tempext = Path.GetExtension(((Rom)rom).Name);
							if (!tempext.StartsWith("."))
							{
								tempext = "." + tempext;
							}

							state += "\t\t\t<files>\n"
								+ (((Rom)rom).CRC != null
									? "\t\t\t\t<romCRC extension=\"" + tempext + "\">" + ((Rom)rom).CRC.ToUpperInvariant() + "</romMD5>\n"
									: ((Rom)rom).MD5 != null
										? "\t\t\t\t<romMD5 extension=\"" + tempext + "\">" + ((Rom)rom).MD5.ToUpperInvariant() + "</romMD5>\n"
										: "\t\t\t\t<romSHA1 extension=\"" + tempext + "\">" + ((Rom)rom).SHA1.ToUpperInvariant() + "</romSHA1>\n")
								+ "\t\t\t</files>\n";
						}

						state += "\t\t\t<im1CRC>00000000</im1CRC>\n"
							+ "\t\t\t<im2CRC>00000000</im2CRC>\n"
							+ "\t\t\t<comment></comment>\n"
							+ "\t\t\t<duplicateID>0</duplicateID>\n"
							+ "\t\t</game>\n";
						break;
					case DatFormat.RedumpMD5:
						if (rom.Type == ItemType.Rom)
						{
							state += ((Rom)rom).MD5 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += ((Disk)rom).MD5 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						break;
					case DatFormat.RedumpSFV:
						if (rom.Type == ItemType.Rom)
						{
							state += (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + " " + ((Rom)rom).CRC + "\n";
						}
						break;
					case DatFormat.RedumpSHA1:
						if (rom.Type == ItemType.Rom)
						{
							state += ((Rom)rom).SHA1 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += ((Disk)rom).SHA1 + " *" + (GameName ? rom.Machine.Name + Path.DirectorySeparatorChar : "") + rom.Name + "\n";
						}
						break;
					case DatFormat.RomCenter:
						if (rom.Type == ItemType.Rom)
						{
							state += "¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + HttpUtility.HtmlEncode(rom.Machine.Name) +
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬" + ((Rom)rom).CRC.ToLowerInvariant() +
							"¬" + (((Rom)rom).Size != -1 ? ((Rom)rom).Size.ToString() : "") + "¬¬¬\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							state += "¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + (String.IsNullOrEmpty(rom.Machine.CloneOf) ? "" : HttpUtility.HtmlEncode(rom.Machine.CloneOf)) +
							"¬" + HttpUtility.HtmlEncode(rom.Machine.Name) +
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.Machine.Description) ? rom.Machine.Name : rom.Machine.Description)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬¬¬¬¬\n";
						}

						break;
					case DatFormat.SabreDat:
						string prefix = "";
						for (int i = 0; i < depth; i++)
						{
							prefix += "\t";
						}
						state += prefix;

						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "<file type=\"archive\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
							case ItemType.BiosSet:
								state += "<file type=\"biosset\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Disk:
								state += "<file type=\"disk\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? prefix + "/>\n" + prefix + "\t<flags>\n" +
										prefix + "\t\t<flag name=\"status\" value=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"/>\n" +
										prefix + "\t</flags>\n" +
										prefix + "</file>\n" : "/>\n");
								break;
							case ItemType.Release:
								state += "<file type=\"release\" name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n";
								break;
							case ItemType.Rom:
								state += "<file type=\"rom\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? prefix + "/>\n" + prefix + "\t<flags>\n" +
										prefix + "\t\t<flag name=\"status\" value=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"/>\n" +
										prefix + "\t</flags>\n" +
										prefix + "</file>\n" : "/>\n");
								break;
							case ItemType.Sample:
								state += "<file type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n";
								break;
						}
						break;
					case DatFormat.SoftwareList:
						state += "\t\t<part name=\"" + rom.PartName + "\" interface=\"" + rom.PartInterface + "\">\n";

						foreach (Tuple<string, string> kvp in rom.Features)
						{
							state += "\t\t\t<feature name=\"" + HttpUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + HttpUtility.HtmlEncode(kvp.Item2) + "\"/>\n";
						}

						switch (rom.Type)
						{
							case ItemType.Archive:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "archive" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<archive name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.BiosSet:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "biosset" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<biosset name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((BiosSet)rom).Description) ? " description=\"" + HttpUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : "")
									+ (((BiosSet)rom).Default != null
										? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.Disk:
								state += "\t\t\t<diskarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "cdrom" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<disk name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n"
									+ "\t\t\t</diskarea>\n";
								break;
							case ItemType.Release:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "release" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<release name\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (!String.IsNullOrEmpty(((Release)rom).Region) ? " region=\"" + HttpUtility.HtmlEncode(((Release)rom).Region) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Language) ? " language=\"" + HttpUtility.HtmlEncode(((Release)rom).Language) + "\"" : "")
									+ (!String.IsNullOrEmpty(((Release)rom).Date) ? " date=\"" + HttpUtility.HtmlEncode(((Release)rom).Date) + "\"" : "")
									+ (((Release)rom).Default != null
										? ((Release)rom).Default.ToString().ToLowerInvariant()
										: "")
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.Rom:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "rom" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<rom name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ (((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : "")
									+ (!String.IsNullOrEmpty(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : "")
									+ (((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : "")
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
							case ItemType.Sample:
								state += "\t\t\t<dataarea name=\"" + (String.IsNullOrEmpty(rom.AreaName) ? "sample" : rom.AreaName) + "\""
										+ (rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : "") + ">\n"
									+ "\t\t\t\t<sample type=\"sample\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\""
									+ "/>\n"
									+ "\t\t\t</dataarea>\n";
								break;
						}

						state += "\t\t</part>\n";
						break;
					case DatFormat.TSV:
						// TSV should only output Rom and Disk
						if (rom.Type != ItemType.Disk && rom.Type != ItemType.Rom)
						{
							return true;
						}

						pre = Prefix + (Quotes ? "\"" : "");
						post = (Quotes ? "\"" : "") + Postfix;

						if (rom.Type == ItemType.Rom)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%crc%", ((Rom)rom).CRC)
								.Replace("%md5%", ((Rom)rom).MD5)
								.Replace("%sha1%", ((Rom)rom).SHA1)
								.Replace("%size%", ((Rom)rom).Size.ToString());
						}
						else if (rom.Type == ItemType.Disk)
						{
							// Check for special strings in prefix and postfix
							pre = pre
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
							post = post
								.Replace("%game%", rom.Machine.Name)
								.Replace("%name%", rom.Name)
								.Replace("%md5%", ((Disk)rom).MD5)
								.Replace("%sha1%", ((Disk)rom).SHA1);
						}

						if (rom.Type == ItemType.Rom)
						{
							string inline = "\"" + FileName + "\""
								+ "\t\"" + Name + "\""
								+ "\t\"" + Description + "\""
								+ "\t\"" + rom.Machine.Name + "\""
								+ "\t\"" + rom.Machine.Description + "\""
								+ "\t" + "\"rom\""
								+ "\t\"" + rom.Name + "\""
								+ "\t" + "\"\""
								+ "\t\"" + ((Rom)rom).Size + "\""
								+ "\t\"" + ((Rom)rom).CRC + "\""
								+ "\t\"" + ((Rom)rom).MD5 + "\""
								+ "\t\"" + ((Rom)rom).SHA1 + "\""
								+ "\t" + (((Rom)rom).ItemStatus != ItemStatus.None ? "\"" + ((Rom)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						else if (rom.Type == ItemType.Disk)
						{
							string inline = "\"" + FileName + "\""
								+ "\t\"" + Name + "\""
								+ "\t\"" + Description + "\""
								+ "\t\"" + rom.Machine.Name + "\""
								+ "\t\"" + rom.Machine.Description + "\""
								+ "\t" + "\"disk\""
								+ "\t" + "\"\""
								+ "\t\"" + rom.Name + "\""
								+ "\t" + "\"\""
								+ "\t" + "\"\""
								+ "\t\"" + ((Disk)rom).MD5 + "\""
								+ "\t\"" + ((Disk)rom).SHA1 + "\""
								+ "\t" + (((Disk)rom).ItemStatus != ItemStatus.None ? "\"" + ((Disk)rom).ItemStatus.ToString() + "\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						break;
				}

				sw.Write(state);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write out DAT footer using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datFormat">Output format to write to</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		private bool WriteFooter(StreamWriter sw, DatFormat datFormat, int depth, Logger logger)
		{
			try
			{
				string footer = "";

				// If we have roms, output the full footer
				if (Count > 0)
				{
					switch (datFormat)
					{
						case DatFormat.ClrMamePro:
						case DatFormat.DOSCenter:
							footer = ")\n";
							break;
						case DatFormat.Logiqx:
							footer = "\t</machine>\n</datafile>\n";
							break;
						case DatFormat.OfflineList:
							footer = "\t\t</game>"
								+ "\t</games>\n"
								+ "\t<gui>\n"
								+ "\t\t<images width=\"487\" height=\"162\">\n"
								+ "\t\t\t<image x=\"0\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t\t<image x=\"245\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t</images>\n"
								+ "\t</gui>\n"
								+ "</dat>";
							break;
						case DatFormat.SabreDat:
							for (int i = depth - 1; i >= 2; i--)
							{
								// Print out the number of tabs and the end folder
								for (int j = 0; j < i; j++)
								{
									footer += "\t";
								}
								footer += "</directory>\n";
							}
							footer += "\t</data>\n</datafile>\n";
							break;
						case DatFormat.SoftwareList:
							footer = "\t</software>\n\n</softwarelist>\n";
							break;
					}
				}

				// Otherwise, output the abbreviated form
				else
				{
					switch (datFormat)
					{
						case DatFormat.Logiqx:
						case DatFormat.SabreDat:
							footer = "</datafile>\n";
							break;
						case DatFormat.OfflineList:
							footer = "\t</games>\n"
								+ "\t<gui>\n"
								+ "\t\t<images width=\"487\" height=\"162\">\n"
								+ "\t\t\t<image x=\"0\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t\t<image x=\"245\" y=\"0\" width=\"240\" height=\"160\"/>\n"
								+ "\t\t</images>\n"
								+ "\t</gui>\n"
								+ "</dat>";
							break;
						case DatFormat.SoftwareList:
							footer = "</softwarelist>\n";
							break;
					}
				}

				// Write the footer out
				sw.Write(footer);
				sw.Flush();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		#endregion
	}
}
