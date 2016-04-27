using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
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
		public static bool WriteToDat(string name, string description, string version, string date, string category, string author, bool forceunpack, bool old, string outDir, List<RomData> roms, Logger logger)
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
			logger.Log("Opening file for writing: " + outDir + description + (old ? ".dat" : ".xml"));

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
					"\t<datafile>\n" +
					"\t\t<header>\n" +
					"\t\t\t<name>" + HttpUtility.HtmlEncode(name) + "</name>\n" +
					"\t\t\t<description>" + HttpUtility.HtmlEncode(description) + "</description>\n" +
					"\t\t\t<category>" + HttpUtility.HtmlEncode(category) + "</category>\n" +
					"\t\t\t<version>" + HttpUtility.HtmlEncode(version) + "</version>\n" +
					"\t\t\t<date>" + HttpUtility.HtmlEncode(date) + "</date>\n" +
					"\t\t\t<author>" + HttpUtility.HtmlEncode(author) + "</author>\n" +
					(forceunpack ? "\t\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
					"\t\t</header>\n";

				// Write the header out
				sw.Write((old ? header_old : header));

				// Write out each of the machines and roms
				string lastgame = "";
				foreach (RomData rom in roms)
				{
					string state = "";
					if (lastgame != "" && lastgame != rom.Game)
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
							(rom.Size != 0 ? " size " + rom.Size : "") +
							(rom.CRC != "" ? " crc " + rom.CRC.ToLowerInvariant() : "") +
							(rom.MD5 != "" ? " md5 " + rom.MD5.ToLowerInvariant() : "") +
							(rom.SHA1 != "" ? " sha1 " + rom.SHA1.ToLowerInvariant() : "") +
							" )\n";
					}
					else
					{
						state += "\t\t<" + rom.Type + " name=\"" + HttpUtility.HtmlEncode(rom.Name) + "\"" +
							(rom.Size != -1 ? " size=\"" + rom.Size + "\"" : "") +
							(rom.CRC != "" ? " crc=\"" + rom.CRC.ToLowerInvariant() + "\"" : "") +
							(rom.MD5 != "" ? " md5=\"" + rom.MD5.ToLowerInvariant() + "\"" : "") +
							(rom.SHA1 != "" ? " sha1=\"" + rom.SHA1.ToLowerInvariant() + "\"" : "") +
							"/>\n";
					}

					lastgame = rom.Game;

					sw.Write(state);
				}

				sw.Write((old ? ")" : "\t</machine>\n</datafile>"));
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
		/// <param name="diff">Only output files that don't have dupes</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="dbc">Database connection representing the roms to be written</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>Tru if the DAT was written correctly, false otherwise</returns>
		public static bool WriteToDat2(string name, string description, string version, string date, string category,
			string author, bool forceunpack, bool old, bool diff, string outDir, SqliteConnection dbc, Logger logger)
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
			logger.Log("Opening file for writing: " + outDir + description + (old ? ".dat" : ".xml"));

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
					"\t<datafile>\n" +
					"\t\t<header>\n" +
					"\t\t\t<name>" + HttpUtility.HtmlEncode(name) + "</name>\n" +
					"\t\t\t<description>" + HttpUtility.HtmlEncode(description) + "</description>\n" +
					"\t\t\t<category>" + HttpUtility.HtmlEncode(category) + "</category>\n" +
					"\t\t\t<version>" + HttpUtility.HtmlEncode(version) + "</version>\n" +
					"\t\t\t<date>" + HttpUtility.HtmlEncode(date) + "</date>\n" +
					"\t\t\t<author>" + HttpUtility.HtmlEncode(author) + "</author>\n" +
					(forceunpack ? "\t\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
					"\t\t</header>\n";

				// Write the header out
				sw.Write((old ? header_old : header));

				// Write out each of the machines and roms
				string lastgame = "";
				string query = "SELECT * FROM roms" + (diff ? " WHERE dupe='false'" : "") + " ORDER BY game, name";
				using (SqliteDataReader sldr = (new SqliteCommand(query, dbc).ExecuteReader()))
				{
					while (sldr.Read())
					{
						string state = "";
						if (lastgame != "" && lastgame != sldr.GetString(1))
						{
							state += (old ? ")\n" : "\t</machine>\n");
						}

						if (lastgame != sldr.GetString(1))
						{
							state += (old ? "game (\n\tname \"" + sldr.GetString(1) + "\"\n" +
								"\tdescription \"" + sldr.GetString(1) + "\"\n" :
								"\t<machine name=\"" + HttpUtility.HtmlEncode(sldr.GetString(1)) + "\">\n" +
								"\t\t<description>" + HttpUtility.HtmlEncode(sldr.GetString(1)) + "</description>\n");
						}

						if (old)
						{
							state += "\t" + sldr.GetString(3) + " ( name \"" + sldr.GetString(2) + "\"" +
								(sldr.GetInt64(6) != 0 ? " size " + sldr.GetInt64(6) : "") +
								(sldr.GetString(7) != "" ? " crc " + sldr.GetString(7).ToLowerInvariant() : "") +
								(sldr.GetString(8) != "" ? " md5 " + sldr.GetString(8).ToLowerInvariant() : "") +
								(sldr.GetString(9) != "" ? " sha1 " + sldr.GetString(9).ToLowerInvariant() : "") +
								" )\n";
						}
						else
						{
							state += "\t\t<" + sldr.GetString(3) + " name=\"" + HttpUtility.HtmlEncode(sldr.GetString(2)) + "\"" +
								(sldr.GetInt64(6) != -1 ? " size=\"" + sldr.GetInt64(6) + "\"" : "") +
								(sldr.GetString(7) != "" ? " crc=\"" + sldr.GetString(7).ToLowerInvariant() + "\"" : "") +
								(sldr.GetString(8) != "" ? " md5=\"" + sldr.GetString(8).ToLowerInvariant() + "\"" : "") +
								(sldr.GetString(9) != "" ? " sha1=\"" + sldr.GetString(9).ToLowerInvariant() + "\"" : "") +
								"/>\n";
						}

						lastgame = sldr.GetString(1);

						sw.Write(state);
					}
				}

				sw.Write((old ? ")" : "\t</machine>\n</datafile>"));
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

			logger.Log("Opening file for writing: " + outdir + textfile);

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
