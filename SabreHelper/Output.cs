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
									state += "\t</machine>\n";
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
							case OutputFormat.MissFile:
								string pre = datdata.Prefix + (datdata.Quotes ? "\"" : "");
								string post = (datdata.Quotes ? "\"" : "") + datdata.Postfix;

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
	}
}
