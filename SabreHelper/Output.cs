using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SabreTools.Helper
{
	public class Output
	{
		/// <summary>
		/// Create and open an output file for writing
		/// </summary>
		/// <param name="name">Internal name of the DAT</param>
		/// <param name="description">Description and external name of the DAT</param>
		/// <param name="version">Version or iteration of the DAT</param>
		/// <param name="date">Usually the DAT creation date</param>
		/// <param name="category">Category of the DAT</param>
		/// <param name="author">DAT author</param>
		/// <param name="forceunpack">Force all sets to be unzipped</param>
		/// <param name="old">Set output mode to old-style DAT</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="roms">List of RomData objects representing the games to be written out</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>Tru if the DAT was written correctly, false otherwise</returns>
		public static bool WriteToDat(string name, string description, string version, string date, string category, string author,
			bool forceunpack, bool old, string outDir, List<RomData> roms, Logger logger)
		{
			// If it's empty, use the current folder
			if (outDir.Trim() == "")
			{
				outDir = Environment.CurrentDirectory;
			}

			// Double check the outdir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// (currently uses current time, change to "last updated time")
			logger.User("Opening file for writing: " + outDir + description + (old ? ".dat" : ".xml"));

			try
			{
				FileStream fs = File.Create(outDir + description + (old ? ".dat" : ".xml"));
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				string header_old = "clrmamepro (\n" +
					"\tname \"" + HttpUtility.HtmlEncode(name) + "\"\n" +
					"\tdescription \"" + HttpUtility.HtmlEncode(description) + "\"\n" +
					"\tcategory \"" + HttpUtility.HtmlEncode(category) + "\"\n" +
					"\tversion \"" + HttpUtility.HtmlEncode(version) + "\"\n" +
					"\tdate \"" + HttpUtility.HtmlEncode(date) + "\"\n" +
					"\tauthor \"" + HttpUtility.HtmlEncode(author) + "\"\n" +
					(forceunpack ? "\tforcezipping no\n" : "") +
					")\n";

				string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
					"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
					"<datafile>\n" +
					"\t<header>\n" +
					"\t\t<name>" + HttpUtility.HtmlEncode(name) + "</name>\n" +
					"\t\t<description>" + HttpUtility.HtmlEncode(description) + "</description>\n" +
					"\t\t<category>" + HttpUtility.HtmlEncode(category) + "</category>\n" +
					"\t\t<version>" + HttpUtility.HtmlEncode(version) + "</version>\n" +
					"\t\t<date>" + HttpUtility.HtmlEncode(date) + "</date>\n" +
					"\t\t<author>" + HttpUtility.HtmlEncode(author) + "</author>\n" +
					(forceunpack ? "\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
					"\t</header>\n";

				// Write the header out
				sw.Write((old ? header_old : header));

				// Write out each of the machines and roms
				string lastgame = null;
				foreach (RomData rom in roms)
				{
					string state = "";
					if (lastgame != null && lastgame != rom.Game)
					{
						state += (old ? ")\n" : "\t</machine>\n");
					}

					if (lastgame != rom.Game)
					{
						state += (old ? "game (\n\tname \"" + rom.Game + "\"\n" +
							"\tdescription \"" + rom.Game + "\"\n" :
							"\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Game) + "\">\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(rom.Game) + "</description>\n");
					}

					if (old)
					{
						state += "\t" + rom.Type + " ( name \"" + rom.Name + "\"" +
							(rom.Size != -1 &&
								rom.Size != 0 &&
								rom.CRC != RomManipulation.CRCZero &&
								rom.MD5 != RomManipulation.MD5Zero &&
								rom.SHA1 != RomManipulation.SHA1Zero ? " size " + rom.Size : "") +
							(rom.CRC != "" ? " crc " + rom.CRC.ToLowerInvariant() : "") +
							(rom.MD5 != "" ? " md5 " + rom.MD5.ToLowerInvariant() : "") +
							(rom.SHA1 != "" ? " sha1 " + rom.SHA1.ToLowerInvariant() : "") +
							" )\n";
					}
					else
					{
						state += "\t\t<" + rom.Type + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.Size != -1 &&
								rom.Size != 0 &&
								rom.CRC != RomManipulation.CRCZero &&
								rom.MD5 != RomManipulation.MD5Zero &&
								rom.SHA1 != RomManipulation.SHA1Zero ? " size=\"" + rom.Size + "\"" : "") +
							(rom.CRC != "" ? " crc=\"" + rom.CRC.ToLowerInvariant() + "\"" : "") +
							(rom.MD5 != "" ? " md5=\"" + rom.MD5.ToLowerInvariant() + "\"" : "") +
							(rom.SHA1 != "" ? " sha1=\"" + rom.SHA1.ToLowerInvariant() + "\"" : "") +
							"/>\n";
					}

					lastgame = rom.Game;

					sw.Write(state);
				}

				sw.Write((old ? ")" : "\t</machine>\n</datafile>"));
				logger.User("File written!" + Environment.NewLine);
				sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datdata">All information for creating the datfile header</param>
		/// <param name="merge">Enable output in merged mode (one game per hash)</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="dict">Dictionary containing all the roms to be written</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for file output:
		/// - Have the ability to strip special (non-ASCII) characters from rom information
		/// - Add a flag for ignoring roms with blank sizes
		/// </remarks>
		public static bool WriteToDatFromDict(DatData datdata, bool merge, string outDir, Dictionary<string, List<RomData>> dict, Logger logger, bool norename = true)
		{
			// Get all values in the dictionary and write out
			SortedDictionary<string, List<RomData>> sortable = new SortedDictionary<string, List<RomData>>();
			long count = 0;
			foreach (List<RomData> roms in dict.Values)
			{
				List<RomData> newroms = roms;
				if (merge)
				{
					newroms = RomManipulation.Merge(newroms);
				}

				foreach (RomData rom in newroms)
				{
					count++;
					string key = (norename ? "" : rom.SystemID.ToString().PadLeft(10, '0') + "-" + rom.SourceID.ToString().PadLeft(10, '0') + "-") + rom.Game.ToLowerInvariant();
					if (sortable.ContainsKey(key))
					{
						sortable[key].Add(rom);
					}
					else
					{
						List<RomData> temp = new List<RomData>();
						temp.Add(rom);
						sortable.Add(key, temp);
					}
				}
			}

			logger.Log("A total of " + count + " file hashes will be written out to file");

			// Now write out to file
			// If it's empty, use the current folder
			if (outDir.Trim() == "")
			{
				outDir = Environment.CurrentDirectory;
			}

			// Double check the outdir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// (currently uses current time, change to "last updated time")
			logger.User("Opening file for writing: " + outDir + datdata.Description + (datdata.OutputFormat == OutputFormat.Xml ? ".xml" : ".dat"));

			try
			{
				FileStream fs = File.Create(outDir + datdata.Description + (datdata.OutputFormat == OutputFormat.Xml ? ".xml" : ".dat"));
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				string header = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						header = "clrmamepro (\n" +
							"\tname \"" + HttpUtility.HtmlEncode(datdata.Name) + "\"\n" +
							"\tdescription \"" + HttpUtility.HtmlEncode(datdata.Description) + "\"\n" +
							"\tcategory \"" + HttpUtility.HtmlEncode(datdata.Category) + "\"\n" +
							"\tversion \"" + HttpUtility.HtmlEncode(datdata.Version) + "\"\n" +
							"\tdate \"" + HttpUtility.HtmlEncode(datdata.Date) + "\"\n" +
							"\tauthor \"" + HttpUtility.HtmlEncode(datdata.Author) + "\"\n" +
							"\tcomment \"" + HttpUtility.HtmlEncode(datdata.Comment) + "\"\n" +
							(datdata.ForcePacking == ForcePacking.Unzip ? "\tforcezipping no\n" : "") +
							")\n";
						break;
					case OutputFormat.RomCenter:
						header = "[CREDITS]\n" +
							"author=" + HttpUtility.HtmlEncode(datdata.Author) + "\n" +
							"version=" + HttpUtility.HtmlEncode(datdata.Version) + "\n" +
							"comment=" + HttpUtility.HtmlEncode(datdata.Comment) + "\n" +
							"[DAT]\n" +
							"version=2.50\n" +
							"split=" + (datdata.ForceMerging == ForceMerging.Split ? "1" : "0") + "\n" +
							"merge=" + (datdata.ForceMerging == ForceMerging.Full ? "1" : "0") + "\n" +
							"[EMULATOR]\n" +
							"refname=" + HttpUtility.HtmlEncode(datdata.Name) + "\n" +
							"version=" + HttpUtility.HtmlEncode(datdata.Description) + "\n" +
							"[GAMES]\n";
						break;
					case OutputFormat.Xml:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(datdata.Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(datdata.Description) + "</description>\n" +
							"\t\t<category>" + HttpUtility.HtmlEncode(datdata.Category) + "</category>\n" +
							"\t\t<version>" + HttpUtility.HtmlEncode(datdata.Version) + "</version>\n" +
							"\t\t<date>" + HttpUtility.HtmlEncode(datdata.Date) + "</date>\n" +
							"\t\t<author>" + HttpUtility.HtmlEncode(datdata.Author) + "</author>\n" +
							"\t\t<comment>" + HttpUtility.HtmlEncode(datdata.Comment) + "</comment>\n" +
							(datdata.ForcePacking == ForcePacking.Unzip ? "\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
							"\t</header>\n";
						break;
				}

				// Write the header out
				sw.Write(header);

				// Write out each of the machines and roms
				string lastgame = null;
				foreach (List<RomData> roms in sortable.Values)
				{
					foreach (RomData rom in roms)
					{
						string state = "";
						if (lastgame != null && lastgame.ToLowerInvariant() != rom.Game.ToLowerInvariant())
						{
							switch (datdata.OutputFormat)
							{
								case OutputFormat.ClrMamePro:
									state += ")\n";
									break;
								case OutputFormat.Xml:
									state += "\t </ machine >\n";
									break;
							}
						}

						if (lastgame == null || lastgame.ToLowerInvariant() != rom.Game.ToLowerInvariant())
						{
							switch (datdata.OutputFormat)
							{
								case OutputFormat.ClrMamePro:
									state += "game (\n\tname \"" + rom.Game + "\"\n" +
										"\tdescription \"" + rom.Game + "\"\n";
									break;
								case OutputFormat.Xml:
									state += "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Game) + "\">\n" +
										"\t\t<description>" + HttpUtility.HtmlEncode(rom.Game) + "</description>\n";
									break;
							}
						}

						// Now output the rom data
						switch (datdata.OutputFormat)
						{
							case OutputFormat.ClrMamePro:
								state += "\t" + rom.Type + " ( name \"" + rom.Name + "\"" +
									(rom.Size != 0 ? " size " + rom.Size : "") +
									(rom.CRC != "" ? " crc " + rom.CRC.ToLowerInvariant() : "") +
									(rom.MD5 != "" ? " md5 " + rom.MD5.ToLowerInvariant() : "") +
									(rom.SHA1 != "" ? " sha1 " + rom.SHA1.ToLowerInvariant() : "") +
									" )\n";
								break;
							case OutputFormat.RomCenter:
								state += "¬¬¬" + HttpUtility.HtmlEncode(rom.Game) +
									"¬" + HttpUtility.HtmlEncode(rom.Game) +
									"¬" + HttpUtility.HtmlEncode(rom.Name) +
									"¬" + rom.CRC.ToLowerInvariant() +
									"¬" + (rom.Size != -1 ? rom.Size.ToString() : "") + "¬¬¬";
								break;
							case OutputFormat.Xml:
								state += "\t\t<" + rom.Type + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
									(rom.Size != -1 ? " size=\"" + rom.Size + "\"" : "") +
									(rom.CRC != "" ? " crc=\"" + rom.CRC.ToLowerInvariant() + "\"" : "") +
									(rom.MD5 != "" ? " md5=\"" + rom.MD5.ToLowerInvariant() + "\"" : "") +
									(rom.SHA1 != "" ? " sha1=\"" + rom.SHA1.ToLowerInvariant() + "\"" : "") +
									"/>\n";
								break;
						}

						lastgame = rom.Game;

						sw.Write(state);
					}
				}

				string footer = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						footer = ")";
						break;
					case OutputFormat.Xml:
						footer = "\t</machine>\n</datafile>";
						break;
				}

				sw.Write(footer);
				logger.User("File written!" + Environment.NewLine);
				sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Output a list of roms as a text file with an arbitrary prefix and postfix
		/// </summary>
		/// <param name="textfile">Name of the output file</param>
		/// <param name="outdir">Output directory for the miss file</param>
		/// <param name="roms">List of RomData objects representing the roms to be output</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="useGame">True if only games are written to text file (default), false for files only</param>
		/// <param name="prefix">Arbitrary string to prefix each line</param>
		/// <param name="postfix">Arbitrary string to postfix each line</param>
		/// <param name="quotes">True if quotes should be put around the item, false otherwise (default)</param>
		/// <param name="addext">Arbitrary extension added to the end of each item</param>
		/// <param name="repext">Arbitrary extension to replace all extensions in the item</param>
		/// <param name="gamename">True if the game name is appended (only when !usegame), false otherwise</param>
		/// <returns>True if the file was written, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for this method:
		/// - Have switch for automatically outputting to Romba format:
		///		e.g. /aa/bb/cc/dd/aabbccddef770b06131a878b46d4302ac28dd126.gz
		///		Anything without a SHA-1 has to be skipped
		/// </remarks>
		public static bool WriteToText(string textfile, string outdir, List<RomData> roms, Logger logger, bool useGame = true, string prefix = "",
			string postfix = "", string addext = "", string repext = "", bool quotes = false, bool gamename = false)
		{
			// Normalize the output directory
			if (outdir == "")
			{
				outdir = Environment.CurrentDirectory;
			}
			if (!outdir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outdir += Path.DirectorySeparatorChar;
			}

			// Make the output directory if it doesn't exist
			if (!Directory.Exists(outdir))
			{
				Directory.CreateDirectory(outdir);
			}

			// Normalize the extensions
			addext = (addext == "" || addext.StartsWith(".") ? addext : "." + addext);
			repext = (repext == "" || repext.StartsWith(".") ? repext : "." + repext);

			logger.User("Opening file for writing: " + outdir + textfile);

			try
			{
				FileStream fs = File.Create(outdir + textfile);
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				string lastgame = "";
				foreach (RomData rom in roms)
				{
					string pre = prefix + (quotes ? "\"" : "");
					string post = (quotes ? "\"" : "") + postfix;
					string name = (useGame ? rom.Game : rom.Name);
					if (repext != "")
					{
						string dir = Path.GetDirectoryName(name);
						dir = (dir.EndsWith(Path.DirectorySeparatorChar.ToString()) ? dir : dir + Path.DirectorySeparatorChar);
						dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
						name = dir + Path.GetFileNameWithoutExtension(name) + repext;
					}
					if (addext != "")
					{
						name += addext;
					}
					if (!useGame && gamename)
					{
						name = (rom.Game.EndsWith(Path.DirectorySeparatorChar.ToString()) ? rom.Game : rom.Game + Path.DirectorySeparatorChar) + name;
					}

					if (useGame && rom.Game != lastgame)
					{
						sw.WriteLine(pre + name + post);
						lastgame = rom.Game;
					}
					else if (!useGame)
					{
						sw.WriteLine(pre + name + post);
					}
				}

				logger.User("File written!" + Environment.NewLine);
				sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Convert a List of RomData objects to a List of tab-deliminated strings
		/// </summary>
		/// <param name="roms">List of RomData objects representing the roms to be parsed</param>
		/// <returns>List of Strings representing the roms</returns>
		public static List<String> RomDataToString(List<RomData> roms)
		{
			List<String> outlist = new List<String>();
			foreach (RomData rom in roms)
			{
				outlist.Add(rom.Manufacturer + "\t" +
					rom.System + "\t" +
					rom.SystemID + "\t" +
					rom.Source + "\t" +
					rom.URL + "\t" +
					rom.SourceID + "\t" +
					rom.Game + "\t" +
					rom.Name + "\t" +
					rom.Type + "\t" +
					rom.Size + "\t" +
					rom.CRC + "\t" +
					rom.MD5 + "\t" +
					rom.SHA1);
			}
			return outlist;
		}

		/// <summary>
		/// Convert a List of RomData objects' hash information to a List of tab-deliminated strings
		/// </summary>
		/// <param name="roms">List of RomData objects representing the roms to be parsed</param>
		/// <returns>List of Strings representing the rom hashes</returns>
		public static List<String> HashDataToString(List<RomData> roms)
		{
			List<String> outlist = new List<String>();
			foreach (RomData rom in roms)
			{
				outlist.Add(rom.Size + "\t" +
					rom.CRC + "\t" +
					rom.MD5 + "\t" +
					rom.SHA1);
			}
			return outlist;
		}

		/// <summary>
		/// Convert a List of tab-deliminated strings objects to a List of RomData objects
		/// </summary>
		/// <param name="roms">List of Strings representing the roms to be parsed</param>
		/// <returns>List of RomData objects representing the roms</returns>
		public static List<RomData> StringToRomData(List<String> roms)
		{
			List<RomData> outlist = new List<RomData>();
			foreach (String rom in roms)
			{
				string[] temp = rom.Split('\t');
				try
				{
					outlist.Add(new RomData
					{
						Manufacturer = temp[0],
						System = temp[1],
						SystemID = Int32.Parse(temp[2]),
						Source = temp[3],
						URL = temp[4],
						SourceID = Int32.Parse(temp[5]),
						Game = temp[6],
						Name = temp[7],
						Type = temp[8],
						Size = Int64.Parse(temp[9]),
						CRC = temp[10],
						MD5 = temp[11],
						SHA1 = temp[12],
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			return outlist;
		}
	}
}
