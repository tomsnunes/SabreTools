using System;
using System.Collections.Generic;
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
		/// <param name="forceunzip">Force all sets to be unzipped</param>
		/// <param name="old">Set output mode to old-style DAT</param>
		/// <param name="outDir">Set the output directory</param>
		/// <param name="roms">List of RomData objects representing the games to be written out</param>
		/// <param name="logger">Logger object for console and/or file output</param>
		/// <returns>Tru if the DAT was written correctly, false otherwise</returns>
		public static bool WriteToDat(string name, string description, string version, string date, string category, string author, bool forceunzip, bool old, string outDir, List<RomData> roms, Logger logger)
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
					"\tversion \"" + HttpUtility.HtmlEncode(version) + "\"\n" +
					"\tcomment \"\"\n" +
					"\tauthor \"" + HttpUtility.HtmlEncode(author) + "\"\n" +
					(forceunzip ? "\tforcezipping no\n" : "") +
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
					(forceunzip ? "\t\t\t<clrmamepro forcepacking=\"unzip\" />\n" : "") +
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
		/// <returns>True if the file was written, false otherwise</returns>
		public static bool WriteToText(string textfile, string outdir, List<RomData> roms, Logger logger, bool useGame = true, string prefix = "", string postfix = "", bool quotes = false)
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

			logger.Log("Opening file for writing: " + outdir + textfile);

			try
			{
				FileStream fs = File.Create(outdir + textfile);
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

				string lastgame = "";
				foreach (RomData rom in roms)
				{
					if (useGame && rom.Game != lastgame)
					{
						sw.WriteLine(prefix + (quotes ? "\"" : "") + rom.Game + (quotes ? "\"" : "") + postfix);
						lastgame = rom.Game;
					}
					else if (!useGame)
					{
						sw.WriteLine(prefix + (quotes ? "\"" : "") + rom.Name + (quotes ? "\"" : "")+ postfix);
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
