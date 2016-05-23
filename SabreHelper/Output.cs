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
		/// Create and open an output file for writing direct from a dictionary
		/// </summary>
		/// <param name="datdata">All information for creating the datfile header</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for file output:
		/// - Have the ability to strip special (non-ASCII) characters from rom information
		/// - Add a flag for ignoring roms with blank sizes
		/// </remarks>
		public static bool WriteDatfile(DatData datdata, string outDir, Logger logger, bool norename = true)
		{
			// Get all values in the dictionary and write out
			SortedDictionary<string, List<RomData>> sortable = new SortedDictionary<string, List<RomData>>();
			long count = 0;
			foreach (List<RomData> roms in datdata.Roms.Values)
			{
				List<RomData> newroms = roms;
				if (datdata.MergeRoms)
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

			logger.User("A total of " + count + " file hashes will be written out to file");

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

			// Create the output directory if it doesn't already exist
			Directory.CreateDirectory(outDir);

			// Get the outfile name
			string outfile = Style.CreateOutfileName(outDir, datdata);

			logger.User("Opening file for writing: " + outfile);

			try
			{
				FileStream fs = File.Create(outfile);
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				// Write out the header
				WriteHeader(sw, datdata, logger);

				// Write out each of the machines and roms
				int depth = 2, last = -1;
				string lastgame = null;
				List<string> splitpath = new List<string>();
				foreach (List<RomData> roms in sortable.Values)
				{
					for (int index = 0; index < roms.Count; index++)
					{
						RomData rom = roms[index];

						string state = "";
						List<string> newsplit = rom.Game.Split('\\').ToList();

						// If we have a different game and we're not at the start of the list, output the end of last item
						if (lastgame != null && lastgame.ToLowerInvariant() != rom.Game.ToLowerInvariant())
						{
							depth = WriteEndGame(sw, rom, splitpath, newsplit, lastgame, datdata, depth, out last, logger);
						}

						// If we have a new game, output the beginning of the new item
						if (lastgame == null || lastgame.ToLowerInvariant() != rom.Game.ToLowerInvariant())
						{
							depth = WriteStartGame(sw, rom, newsplit, lastgame, datdata, depth, last, logger);
						}

						// If we have a "null" game (created by DATFromDir or something similar), log it to file
						if (rom.Name == "null" && rom.Size == -1 && rom.CRC == "null" && rom.MD5 == "null" && rom.SHA1 == "null")
						{
							logger.Log("Empty folder found: " + rom.Game);

							// If we're in a mode that doesn't allow for actual empty folders, add the blank info
							if (datdata.OutputFormat != OutputFormat.SabreDat && datdata.OutputFormat != OutputFormat.MissFile)
							{
								rom.Name = "-";
								rom.Size = Constants.SizeZero;
								rom.CRC = Constants.CRCZero;
								rom.MD5 = Constants.MD5Zero;
								rom.SHA1 = Constants.SHA1Zero;
							}

							// Otherwise, set the new path and such, write out, and continue
							else
							{
								splitpath = newsplit;
								lastgame = rom.Game;
								continue;
							}
						}

						// Now, output the rom data
						WriteRomData(sw, rom, lastgame, datdata, depth, logger);

						// Set the new data to compare against
						splitpath = newsplit;
						lastgame = rom.Game;
					}
				}

				// Write the file footer out
				WriteFooter(sw, datdata, depth, logger);
				
				logger.Log("File written!" + Environment.NewLine);
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
		/// Write out DAT header using the supplied StreamWriter
		/// </summary>
		/// <param name="sw">StreamWriter to output to</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteHeader(StreamWriter sw, DatData datdata, Logger logger)
		{
			try
			{
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
					case OutputFormat.SabreDat:
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
							(!String.IsNullOrEmpty(datdata.Type) && datdata.ForcePacking != ForcePacking.Unzip ?
								"\t\t<flags>\n" +
								(!String.IsNullOrEmpty(datdata.Type) ? "\t\t\t<flag name=\"type\" value=\"" + datdata.Type + "\"/>\n" : "") +
								(datdata.ForcePacking == ForcePacking.Unzip ? "\t\t\t<flag name=\"forcepacking\" value=\"unzip\"/>\n" : "") +
								"\t\t</flags>\n" : "") +
							"\t</header>\n" +
							"\t<data>\n";
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
							(!String.IsNullOrEmpty(datdata.Type) ? "\t\t<type>" + datdata.Type + "</type>\n" : "") +
							(datdata.ForcePacking == ForcePacking.Unzip ? "\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
							"\t</header>\n";
						break;
				}

				// Write the header out
				sw.Write(header);
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
		/// <param name="rom">RomData object to be output</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		public static int WriteStartGame(StreamWriter sw, RomData rom, List<string> newsplit, string lastgame, DatData datdata, int depth, int last, Logger logger)
		{
			try
			{
				string state = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "game (\n\tname \"" + rom.Game + "\"\n" +
							"\tdescription \"" + rom.Game + "\"\n";
						break;
					case OutputFormat.SabreDat:
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
					case OutputFormat.Xml:
						state += "\t<machine name=\"" + HttpUtility.HtmlEncode(rom.Game) + "\">\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(rom.Game) + "</description>\n";
						break;
				}

				sw.Write(state);
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
		/// <param name="rom">RomData object to be output</param>
		/// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
		/// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>The new depth of the tag</returns>
		public static int WriteEndGame(StreamWriter sw, RomData rom, List<string> splitpath, List<string> newsplit, string lastgame, DatData datdata, int depth, out int last, Logger logger)
		{
			last = 0;

			try
			{
				string state = "";

				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += ")\n";
						break;
					case OutputFormat.SabreDat:
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
					case OutputFormat.Xml:
						state += "\t</machine>\n";
						break;
				}

				sw.Write(state);
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
		/// <param name="rom">RomData object to be output</param>
		/// <param name="lastgame">The name of the last game to be output</param>
		/// <param name="datdata">DatData object representing DAT information</param>
		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteRomData(StreamWriter sw, RomData rom, string lastgame, DatData datdata, int depth, Logger logger)
		{
			try
			{
				string state = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "\t" + rom.Type + " ( name \"" + rom.Name + "\"" +
							(rom.Size != -1 ? " size " + rom.Size : "") +
							(!String.IsNullOrEmpty(rom.CRC) ? " crc " + rom.CRC.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.MD5) ? " md5 " + rom.MD5.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.SHA1) ? " sha1 " + rom.SHA1.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date \"" + rom.Date + "\"" : "") +
							(rom.Nodump ? " flags nodump" : "") +
							" )\n";
						break;
					case OutputFormat.MissFile:
						string pre = datdata.Prefix + (datdata.Quotes ? "\"" : "");
						string post = (datdata.Quotes ? "\"" : "") + datdata.Postfix;

						// Check for special strings in prefix and postfix
						pre = pre.Replace("%crc%", rom.CRC).Replace("%md5%", rom.MD5).Replace("%sha1%", rom.SHA1).Replace("%size%", rom.Size.ToString());
						post = post.Replace("%crc%", rom.CRC).Replace("%md5%", rom.MD5).Replace("%sha1%", rom.SHA1).Replace("%size%", rom.Size.ToString());

						// If we're in Romba mode, the state is consistent
						if (datdata.Romba)
						{
							// We can only write out if there's a SHA-1
							if (rom.SHA1 != "")
							{
								string name = "/" + rom.SHA1.Substring(0, 2) + "/" + rom.SHA1.Substring(2, 2) + "/" + rom.SHA1.Substring(4, 2) + "/" +
									rom.SHA1.Substring(6, 2) + "/" + rom.SHA1 + ".gz\n";
								state += pre + name + post;
							}
						}
						// Otherwise, use any flags
						else
						{
							string name = (datdata.UseGame ? rom.Game : rom.Name);
							if (datdata.RepExt != "")
							{
								string dir = Path.GetDirectoryName(name);
								dir = (dir.EndsWith(Path.DirectorySeparatorChar.ToString()) ? dir : dir + Path.DirectorySeparatorChar);
								dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
								name = dir + Path.GetFileNameWithoutExtension(name) + datdata.RepExt;
							}
							if (datdata.AddExt != "")
							{
								name += datdata.AddExt;
							}
							if (!datdata.UseGame && datdata.GameName)
							{
								name = (rom.Game.EndsWith(Path.DirectorySeparatorChar.ToString()) ? rom.Game : rom.Game + Path.DirectorySeparatorChar) + name;
							}

							if (datdata.UseGame && rom.Game != lastgame)
							{
								state += pre + name + post + "\n";
								lastgame = rom.Game;
							}
							else if (!datdata.UseGame)
							{
								state += pre + name + post + "\n";
							}
						}
						break;
					case OutputFormat.RomCenter:
						state += "¬¬¬" + HttpUtility.HtmlEncode(rom.Game) +
							"¬" + HttpUtility.HtmlEncode(rom.Game) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬" + rom.CRC.ToLowerInvariant() +
							"¬" + (rom.Size != -1 ? rom.Size.ToString() : "") + "¬¬¬\n";
						break;
					case OutputFormat.SabreDat:
						string prefix = "";
						for (int i = 0; i < depth; i++)
						{
							prefix += "\t";
						}

						state += prefix;
						state += "<file type=\"" + rom.Type + "\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.Size != -1 ? " size=\"" + rom.Size + "\"" : "") +
							(!String.IsNullOrEmpty(rom.CRC) ? " crc=\"" + rom.CRC.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.MD5) ? " md5=\"" + rom.MD5.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.SHA1) ? " sha1=\"" + rom.SHA1.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date=\"" + rom.Date + "\"" : "") +
							(rom.Nodump ? prefix + "/>\n" + prefix + "\t<flags>\n" +
								prefix + "\t\t<flag name=\"status\" value=\"nodump\"/>\n" +
								prefix + "\t</flags>\n" +
								prefix + "</file>\n" :
							"/>\n");
						break;
					case OutputFormat.Xml:
						state += "\t\t<" + rom.Type + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.Size != -1 ? " size=\"" + rom.Size + "\"" : "") +
							(!String.IsNullOrEmpty(rom.CRC) ? " crc=\"" + rom.CRC.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.MD5) ? " md5=\"" + rom.MD5.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.SHA1) ? " sha1=\"" + rom.SHA1.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date=\"" + rom.Date + "\"" : "") +
							(rom.Nodump ? " status=\"nodump\"" : "") +
							"/>\n";
						break;
				}

				sw.Write(state);
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
		/// <param name="datdata">DatData object representing DAT information</param>
		/// 		/// <param name="depth">Current depth to output file at (SabreDAT only)</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the data was written, false on error</returns>
		public static bool WriteFooter(StreamWriter sw, DatData datdata, int depth, Logger logger)
		{
			try
			{
				string footer = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						footer = ")";
						break;
					case OutputFormat.SabreDat:
						for (int i = depth - 1; i >= 2; i--)
						{
							// Print out the number of tabs and the end folder
							for (int j = 0; j < i; j++)
							{
								footer += "\t";
							}
							footer += "</directory>\n";
						}
						footer += "\t</data>\n</datafile>";
						break;
					case OutputFormat.Xml:
						footer = "\t</machine>\n</datafile>";
						break;
				}

				// Write the footer out
				sw.Write(footer);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				return false;
			}

			return true;
		}
	}
}
