using System;
using System.Collections.Generic;
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
		/// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
		/// <returns>True if the DAT was written correctly, false otherwise</returns>
		/// <remarks>
		/// The following features have been requested for file output:
		/// - Have the ability to strip special (non-ASCII) characters from rom information
		/// - Add a flag for ignoring roms with blank sizes
		/// </remarks>
		public static bool WriteDatfile(Dat datdata, string outDir, Logger logger, bool norename = true, bool stats = false)
		{
			// Output initial statistics, for kicks
			if (stats)
			{
				Stats.OutputStats(datdata, logger, (datdata.RomCount + datdata.DiskCount == 0));
			}
			
			// Bucket roms by game name and optionally dedupe
			SortedDictionary<string, List<Rom>> sortable = DatTools.BucketByGame(datdata.Roms, datdata.MergeRoms, norename, logger);

			// Now write out to file
			// If it's empty, use the current folder
			if (outDir.Trim() == "")
			{
				outDir = Environment.CurrentDirectory;
			}

			// Create the output directory if it doesn't already exist
			Directory.CreateDirectory(outDir);

			// Make sure that the three essential fields are filled in
			if (String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Name = datdata.Description = "Default";
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Name = datdata.Description;
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Description = datdata.Name;
			}
			else if (String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.FileName = datdata.Description;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Name = datdata.Description = datdata.FileName;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Name = datdata.Description;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && String.IsNullOrEmpty(datdata.Description))
			{
				datdata.Description = datdata.Name;
			}
			else if (!String.IsNullOrEmpty(datdata.FileName) && !String.IsNullOrEmpty(datdata.Name) && !String.IsNullOrEmpty(datdata.Description))
			{
				// Nothing is needed
			}

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
				foreach (List<Rom> roms in sortable.Values)
				{
					for (int index = 0; index < roms.Count; index++)
					{
						Rom rom = roms[index];
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
						if (rom.Name == "null" && rom.HashData.Size == -1 && rom.HashData.CRC == "null" && rom.HashData.MD5 == "null" && rom.HashData.SHA1 == "null")
						{
							logger.Log("Empty folder found: " + rom.Game);

							// If we're in a mode that doesn't allow for actual empty folders, add the blank info
							if (datdata.OutputFormat != OutputFormat.SabreDat && datdata.OutputFormat != OutputFormat.MissFile)
							{
								rom.Name = "-";
								rom.HashData.Size = Constants.SizeZero;
								rom.HashData.CRC = Constants.CRCZero;
								rom.HashData.MD5 = Constants.MD5Zero;
								rom.HashData.SHA1 = Constants.SHA1Zero;
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
		public static bool WriteHeader(StreamWriter sw, Dat datdata, Logger logger)
		{
			try
			{
				string header = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						header = "clrmamepro (\n" +
							"\tname \"" + datdata.Name + "\"\n" +
							"\tdescription \"" + datdata.Description + "\"\n" +
							"\tcategory \"" + datdata.Category + "\"\n" +
							"\tversion \"" + datdata.Version + "\"\n" +
							"\tdate \"" + datdata.Date + "\"\n" +
							"\tauthor \"" + datdata.Author + "\"\n" +
							"\temail \"" + datdata.Email + "\"\n" +
							"\thomepage \"" + datdata.Homepage + "\"\n" +
							"\turl \"" + datdata.Url + "\"\n" +
							"\tcomment \"" + datdata.Comment + "\"\n" +
							(datdata.ForcePacking == ForcePacking.Unzip ? "\tforcezipping no\n" : "") +
							")\n";
						break;
					case OutputFormat.MissFile:
						if (datdata.TSV == true)
						{
							header = "\"File Name\"\t\"Internal Name\"\t\"Description\"\t\"Game Name\"\t\"Game Description\"\t\"Type\"\t\"" +
								"Rom Name\"\t\"Disk Name\"\t\"Size\"\t\"CRC\"\t\"MD5\"\t\"SHA1\"\t\"Nodump\"\n";
						}
						else if (datdata.TSV == false)
						{
							header = "\"File Name\",\"Internal Name\",\"Description\",\"Game Name\",\"Game Description\",\"Type\",\"" +
								"Rom Name\",\"Disk Name\",\"Size\",\"CRC\",\"MD5\",\"SHA1\",\"Nodump\"\n";
						}
						break;
					case OutputFormat.RomCenter:
						header = "[CREDITS]\n" +
							"author=" + datdata.Author + "\n" +
							"version=" + datdata.Version + "\n" +
							"comment=" + datdata.Comment + "\n" +
							"[DAT]\n" +
							"version=2.50\n" +
							"split=" + (datdata.ForceMerging == ForceMerging.Split ? "1" : "0") + "\n" +
							"merge=" + (datdata.ForceMerging == ForceMerging.Full ? "1" : "0") + "\n" +
							"[EMULATOR]\n" +
							"refname=" + datdata.Name + "\n" +
							"version=" + datdata.Description + "\n" +
							"[GAMES]\n";
						break;
					case OutputFormat.SabreDat:
						header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
							"<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
							"<datafile>\n" +
							"\t<header>\n" +
							"\t\t<name>" + HttpUtility.HtmlEncode(datdata.Name) + "</name>\n" +
							"\t\t<description>" + HttpUtility.HtmlEncode(datdata.Description) + "</description>\n" +
							"\t\t<rootdir>" + HttpUtility.HtmlEncode(datdata.RootDir) + "</rootdir>\n" +
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
							"\t\t<rootdir>" + HttpUtility.HtmlEncode(datdata.RootDir) + "</rootdir>\n" +
							"\t\t<category>" + HttpUtility.HtmlEncode(datdata.Category) + "</category>\n" +
							"\t\t<version>" + HttpUtility.HtmlEncode(datdata.Version) + "</version>\n" +
							"\t\t<date>" + HttpUtility.HtmlEncode(datdata.Date) + "</date>\n" +
							"\t\t<author>" + HttpUtility.HtmlEncode(datdata.Author) + "</author>\n" +
							"\t\t<email>" + HttpUtility.HtmlEncode(datdata.Email) + "</email>\n" +
							"\t\t<homepage>" + HttpUtility.HtmlEncode(datdata.Homepage) + "</homepage>\n" +
							"\t\t<url>" + HttpUtility.HtmlEncode(datdata.Url) + "</url>\n" +
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
		public static int WriteStartGame(StreamWriter sw, Rom rom, List<string> newsplit, string lastgame, Dat datdata, int depth, int last, Logger logger)
		{
			try
			{
				// No game should start with a path separator
				if (rom.Game.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					rom.Game = rom.Game.Substring(1);
				}

				string state = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "game (\n\tname \"" + rom.Game + "\"\n" +
							"\tdescription \"" + (String.IsNullOrEmpty(rom.GameDescription) ? rom.Game : rom.GameDescription) + "\"\n";
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
							"\t\t<description>" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.GameDescription) ? rom.Game : rom.GameDescription)) + "</description>\n";
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
		public static int WriteEndGame(StreamWriter sw, Rom rom, List<string> splitpath, List<string> newsplit, string lastgame, Dat datdata, int depth, out int last, Logger logger)
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
		public static bool WriteRomData(StreamWriter sw, Rom rom, string lastgame, Dat datdata, int depth, Logger logger)
		{
			try
			{
				string state = "";
				switch (datdata.OutputFormat)
				{
					case OutputFormat.ClrMamePro:
						state += "\t" + rom.Type + " ( name \"" + rom.Name + "\"" +
							(rom.HashData.Size != -1 ? " size " + rom.HashData.Size : "") +
							(!String.IsNullOrEmpty(rom.HashData.CRC) ? " crc " + rom.HashData.CRC.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.HashData.MD5) ? " md5 " + rom.HashData.MD5.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.HashData.SHA1) ? " sha1 " + rom.HashData.SHA1.ToLowerInvariant() : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date \"" + rom.Date + "\"" : "") +
							(rom.Nodump ? " flags nodump" : "") +
							" )\n";
						break;
					case OutputFormat.MissFile:
						string pre = datdata.Prefix + (datdata.Quotes ? "\"" : "");
						string post = (datdata.Quotes ? "\"" : "") + datdata.Postfix;

						// Check for special strings in prefix and postfix
						pre = pre.Replace("%crc%", rom.HashData.CRC).Replace("%md5%", rom.HashData.MD5).Replace("%sha1%", rom.HashData.SHA1).Replace("%size%", rom.HashData.Size.ToString());
						post = post.Replace("%crc%", rom.HashData.CRC).Replace("%md5%", rom.HashData.MD5).Replace("%sha1%", rom.HashData.SHA1).Replace("%size%", rom.HashData.Size.ToString());

						// If we're in Romba mode, the state is consistent
						if (datdata.Romba)
						{
							// We can only write out if there's a SHA-1
							if (rom.HashData.SHA1 != "")
							{
								string name = rom.HashData.SHA1.Substring(0, 2) + "/" + rom.HashData.SHA1.Substring(2, 2) + "/" + rom.HashData.SHA1.Substring(4, 2) + "/" +
									rom.HashData.SHA1.Substring(6, 2) + "/" + rom.HashData.SHA1 + ".gz";
								state += pre + name + post + "\n";
							}
						}
						// If we're in TSV mode, similarly the state is consistent
						else if (datdata.TSV == true)
						{
							string inline = "\"" + datdata.FileName + "\"\t\"" + datdata.Name + "\"\t\"" + datdata.Description + "\"\t\"" + rom.Game + "\"\t\"" + rom.Game + "\"\t\"" +
								rom.Type + "\"\t\"" + (rom.Type == "rom" ? rom.Name : "") + "\"\t\"" + (rom.Type == "disk" ? rom.Name : "") + "\"\t\"" + rom.HashData.Size + "\"\t\"" +
								rom.HashData.CRC + "\"\t\"" + rom.HashData.MD5 + "\"\t\"" + rom.HashData.SHA1 + "\"\t" + (rom.Nodump ? "\"Nodump\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						// If we're in CSV mode, similarly the state is consistent
						else if (datdata.TSV == false)
						{
							string inline = "\"" + datdata.FileName + "\",\"" + datdata.Name + "\",\"" + datdata.Description + "\",\"" + rom.Game + "\",\"" + rom.Game + "\",\"" +
								rom.Type + "\",\"" + (rom.Type == "rom" ? rom.Name : "") + "\",\"" + (rom.Type == "disk" ? rom.Name : "") + "\",\"" + rom.HashData.Size + "\",\"" +
								rom.HashData.CRC + "\",\"" + rom.HashData.MD5 + "\",\"" + rom.HashData.SHA1 + "\"," + (rom.Nodump ? "\"Nodump\"" : "\"\"");
							state += pre + inline + post + "\n";
						}
						// Otherwise, use any flags
						else
						{
							string name = (datdata.UseGame ? rom.Game : rom.Name);
							if (datdata.RepExt != "")
							{
								string dir = Path.GetDirectoryName(name);
								dir = (dir.StartsWith(Path.DirectorySeparatorChar.ToString()) ? dir.Remove(0, 1) : dir);
								name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + datdata.RepExt);
							}
							if (datdata.AddExt != "")
							{
								name += datdata.AddExt;
							}
							if (!datdata.UseGame && datdata.GameName)
							{
								name = Path.Combine(rom.Game, name);
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
							"¬" + HttpUtility.HtmlEncode((String.IsNullOrEmpty(rom.GameDescription) ? rom.Game : rom.GameDescription)) +
							"¬" + HttpUtility.HtmlEncode(rom.Name) +
							"¬" + rom.HashData.CRC.ToLowerInvariant() +
							"¬" + (rom.HashData.Size != -1 ? rom.HashData.Size.ToString() : "") + "¬¬¬\n";
						break;
					case OutputFormat.SabreDat:
						string prefix = "";
						for (int i = 0; i < depth; i++)
						{
							prefix += "\t";
						}

						state += prefix;
						state += "<file type=\"" + rom.Type + "\" name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.HashData.Size != -1 ? " size=\"" + rom.HashData.Size + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.CRC) ? " crc=\"" + rom.HashData.CRC.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.MD5) ? " md5=\"" + rom.HashData.MD5.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.SHA1) ? " sha1=\"" + rom.HashData.SHA1.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.Date) ? " date=\"" + rom.Date + "\"" : "") +
							(rom.Nodump ? prefix + "/>\n" + prefix + "\t<flags>\n" +
								prefix + "\t\t<flag name=\"status\" value=\"nodump\"/>\n" +
								prefix + "\t</flags>\n" +
								prefix + "</file>\n" :
							"/>\n");
						break;
					case OutputFormat.Xml:
						state += "\t\t<" + rom.Type + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.HashData.Size != -1 ? " size=\"" + rom.HashData.Size + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.CRC) ? " crc=\"" + rom.HashData.CRC.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.MD5) ? " md5=\"" + rom.HashData.MD5.ToLowerInvariant() + "\"" : "") +
							(!String.IsNullOrEmpty(rom.HashData.SHA1) ? " sha1=\"" + rom.HashData.SHA1.ToLowerInvariant() + "\"" : "") +
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
		public static bool WriteFooter(StreamWriter sw, Dat datdata, int depth, Logger logger)
		{
			try
			{
				string footer = "";

				// If we have roms, output the full footer
				if (datdata.Roms != null && datdata.Roms.Count > 0)
				{
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
				}
				
				// Otherwise, output the abbreviated form
				else
				{
					switch (datdata.OutputFormat)
					{
						case OutputFormat.SabreDat:
						case OutputFormat.Xml:
							footer = "</datafile>";
							break;
					}
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

		/// <summary>
		/// Cleans out the temporary directory
		/// </summary>
		/// <param name="dirname">Name of the directory to clean out</param>
		public static void CleanDirectory(string dirname)
		{
			foreach (string file in Directory.EnumerateFiles(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					File.Delete(file);
				}
				catch { }
			}
			foreach (string dir in Directory.EnumerateDirectories(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				try
				{
					Directory.Delete(dir, true);
				}
				catch { }
			}
		}

		/// <summary>
		/// Remove an arbitrary number of bytes from the inputted file
		/// </summary>
		/// <param name="input">File to be cropped</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToRemoveFromHead">Bytes to be removed from head of file</param>
		/// <param name="bytesToRemoveFromTail">Bytes to be removed from tail of file</param>
		public static void RemoveBytesFromFile(string input, string output, long bytesToRemoveFromHead, long bytesToRemoveFromTail)
		{
			// If any of the inputs are invalid, skip
			if (!File.Exists(input) || new FileInfo(input).Length <= (bytesToRemoveFromHead + bytesToRemoveFromTail))
			{
				return;
			}

			// Read the input file and write to the fail
			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(output)))
			{
				int bufferSize = 1024;
				long adjustedLength = br.BaseStream.Length - bytesToRemoveFromTail;

				// Seek to the correct position
				br.BaseStream.Seek((bytesToRemoveFromHead < 0 ? 0 : bytesToRemoveFromHead), SeekOrigin.Begin);

				// Now read the file in chunks and write out
				byte[] buffer = new byte[bufferSize];
				while (br.BaseStream.Position <= (adjustedLength - bufferSize))
				{
					buffer = br.ReadBytes(bufferSize);
					bw.Write(buffer);
				}

				// For the final chunk, if any, write out only that number of bytes
				int length = (int)(adjustedLength - br.BaseStream.Position);
				buffer = new byte[length];
				buffer = br.ReadBytes(length);
				bw.Write(buffer);
			}
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">String representing bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">String representing bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, string bytesToAddToHead, string bytesToAddToTail)
		{
			// Source: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
			byte[] bytesToAddToHeadArray = new byte[bytesToAddToHead.Length / 2];
			for (int i = 0; i < bytesToAddToHead.Length; i += 2)
			{
				bytesToAddToHeadArray[i/2] = Convert.ToByte(bytesToAddToHead.Substring(i, 2), 16);
			}
			byte[] bytesToAddToTailArray = new byte[bytesToAddToTail.Length / 2];
			for (int i = 0; i < bytesToAddToTail.Length; i += 2)
			{
				bytesToAddToTailArray[i / 2] = Convert.ToByte(bytesToAddToTail.Substring(i, 2), 16);
			}

			AppendBytesToFile(input, output, bytesToAddToHeadArray, bytesToAddToTailArray);
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">Bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">Bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
		{
			// If any of the inputs are invalid, skip
			if (!File.Exists(input))
			{
				return;
			}

			using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
			using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(output)))
			{
				if (bytesToAddToHead.Count() > 0)
				{
					bw.Write(bytesToAddToHead);
				}

				int bufferSize = 1024;

				// Now read the file in chunks and write out
				byte[] buffer = new byte[bufferSize];
				while (br.BaseStream.Position <= (br.BaseStream.Length - bufferSize))
				{
					buffer = br.ReadBytes(bufferSize);
					bw.Write(buffer);
				}

				// For the final chunk, if any, write out only that number of bytes
				int length = (int)(br.BaseStream.Length - br.BaseStream.Position);
				buffer = new byte[length];
				buffer = br.ReadBytes(length);
				bw.Write(buffer);

				if (bytesToAddToTail.Count() > 0)
				{
					bw.Write(bytesToAddToTail);
				}
			}
		}

		/// <summary>
		/// Copy a file to a new location, creating directories as needed
		/// </summary>
		/// <param name="input">Input filename</param>
		/// <param name="output">Output filename</param>
		public static void CopyFileToNewLocation(string input, string output)
		{
			if (File.Exists(input) && !File.Exists(output))
			{
				if (!Directory.Exists(Path.GetDirectoryName(output)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(output));
				}
				File.Copy(input, output);
			}
		}
		}
}
