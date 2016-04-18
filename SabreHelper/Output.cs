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
		/// <returns></returns>
		public static bool WriteToDat(string name, string description, string version, string date, string category, string author, bool forceunzip, bool old, string outDir, List<RomData> roms, Logger logger)
		{
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
					"\t\t\t<category>" + HttpUtility.HtmlEncode(category) + "/category>\n" +
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
							" />\n";
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
	}
}
