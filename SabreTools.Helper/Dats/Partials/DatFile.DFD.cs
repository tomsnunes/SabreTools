﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using IOException = System.IO.IOException;
using SearchOption = System.IO.SearchOption;
#endif
using SharpCompress.Common;

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Populate DAT from Directory [MODULAR DONE, FOR NOW]

		/// <summary>
		/// Create a new Dat from a directory
		/// </summary>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="outDir">Output directory to </param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logger object for console and file output</param>
		public bool PopulateFromDir(string basePath, bool noMD5, bool noSHA1, bool bare, bool archivesAsFiles,
			bool enableGzip, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst,
			int maxDegreeOfParallelism, Logger logger)
		{
			// If the description is defined but not the name, set the name from the description
			if (String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description))
			{
				Name = Description;
			}

			// If the name is defined but not the description, set the description from the name
			else if (!String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Description = Name + (bare ? "" : " (" + Date + ")");
			}

			// If neither the name or description are defined, set them from the automatic values
			else if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Description))
			{
				Name = basePath.Split(Path.DirectorySeparatorChar).Last();
				Description = Name + (bare ? "" : " (" + Date + ")");
			}

			// Process the input
			if (Directory.Exists(basePath))
			{
				logger.Verbose("Folder found: " + basePath);

				// Process the files in the main folder
				List<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.TopDirectoryOnly).ToList();
				Parallel.ForEach(files,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
					item =>
					{
						PopulateFromDirCheckFile(item, basePath, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, addBlanks, addDate,
							tempDir, copyFiles, headerToCheckAgainst, maxDegreeOfParallelism, logger);
					});

				// Find all top-level subfolders
				files = Directory.EnumerateDirectories(basePath, "*", SearchOption.TopDirectoryOnly).ToList();
				Parallel.ForEach(files,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
					item =>
					{
						List<string> subfiles = Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories).ToList();
						Parallel.ForEach(subfiles,
							new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
							subitem =>
							{
								PopulateFromDirCheckFile(subitem, basePath, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, addBlanks, addDate,
								tempDir, copyFiles, headerToCheckAgainst, maxDegreeOfParallelism, logger);
							});
					});

				// Process the files in all subfolders
				files = Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories).ToList();
				Parallel.ForEach(files,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
					item =>
					{
						PopulateFromDirCheckFile(item, basePath, noMD5, noSHA1, bare, archivesAsFiles, enableGzip, addBlanks, addDate,
							tempDir, copyFiles, headerToCheckAgainst, maxDegreeOfParallelism, logger);
					});

				// Now find all folders that are empty, if we are supposed to
				if (!Romba && addBlanks)
				{
					List<string> empties = Directory.EnumerateDirectories(basePath, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(empties,
						new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
						dir =>
						{
							if (Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly).Count() == 0)
							{
								// Get the full path for the directory
								string fulldir = Path.GetFullPath(dir);

								// Set the temporary variables
								string gamename = "";
								string romname = "";

								// If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
								if (Type == "SuperDAT")
								{
									gamename = fulldir.Remove(0, basePath.Length + 1);
									romname = "-";
								}

								// Otherwise, we want just the top level folder as the game, and the file as everything else
								else
								{
									gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
									romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "-");
								}

								// Sanitize the names
								if (gamename.StartsWith(Path.DirectorySeparatorChar.ToString()))
								{
									gamename = gamename.Substring(1);
								}
								if (gamename.EndsWith(Path.DirectorySeparatorChar.ToString()))
								{
									gamename = gamename.Substring(0, gamename.Length - 1);
								}
								if (romname.StartsWith(Path.DirectorySeparatorChar.ToString()))
								{
									romname = romname.Substring(1);
								}
								if (romname.EndsWith(Path.DirectorySeparatorChar.ToString()))
								{
									romname = romname.Substring(0, romname.Length - 1);
								}

								logger.Verbose("Adding blank empty folder: " + gamename);
								this["null"].Add(new Rom(romname, gamename));
							}
						});
				}
			}
			else if (File.Exists(basePath))
			{
				PopulateFromDirCheckFile(basePath, Path.GetDirectoryName(Path.GetDirectoryName(basePath)), noMD5, noSHA1, bare, archivesAsFiles, enableGzip, addBlanks, addDate,
					tempDir, copyFiles, headerToCheckAgainst, maxDegreeOfParallelism, logger);
			}

			// Now that we're done, delete the temp folder (if it's not the default)
			logger.User("Cleaning temp folder");
			try
			{
				if (tempDir != Path.GetTempPath())
				{
					Directory.Delete(tempDir, true);
				}
			}
			catch
			{
				// Just absorb the error for now
			}

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="maxDegreeOfParallelism">Integer representing the maximum amount of parallelization to be used</param>
		/// <param name="logger">Logger object for console and file output</param>
		private void PopulateFromDirCheckFile(string item, string basePath, bool noMD5, bool noSHA1, bool bare, bool archivesAsFiles,
			bool enableGzip, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst,
			int maxDegreeOfParallelism, Logger logger)
		{
			// Define the temporary directory
			string tempSubDir = Path.GetFullPath(Path.Combine(tempDir, Path.GetRandomFileName())) + Path.DirectorySeparatorChar;

			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (Romba)
			{
				Rom rom = ArchiveTools.GetTorrentGZFileInfo(item, logger);

				// If the rom is valid, write it out
				if (rom != null && rom.Name != null)
				{
					// Add the list if it doesn't exist already
					Add(rom.Size + "-" + rom.CRC, rom);
					logger.User("File added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
				}
				else
				{
					logger.User("File not added: " + Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
					return;
				}

				return;
			}

			// If we're copying files, copy it first and get the new filename
			string newItem = item;
			string newBasePath = basePath;
			if (copyFiles)
			{
				newBasePath = Path.Combine(tempDir, Path.GetRandomFileName());
				newItem = Path.GetFullPath(Path.Combine(newBasePath, Path.GetFullPath(item).Remove(0, basePath.Length + 1)));
				Directory.CreateDirectory(Path.GetDirectoryName(newItem));
				File.Copy(item, newItem, true);
			}

			// If both deep hash skip flags are set, do a quickscan
			if (noMD5 && noSHA1)
			{
				ArchiveType? type = ArchiveTools.GetCurrentArchiveType(newItem, logger);

				// If we have an archive, scan it
				if (type != null && !archivesAsFiles)
				{
					List<Rom> extracted = ArchiveTools.GetArchiveFileInfo(newItem, logger);

					foreach (Rom rom in extracted)
					{
						PopulateFromDirProcessFileHelper(newItem,
							rom,
							basePath,
							(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item),
							logger);
					}
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(newItem))
				{
					PopulateFromDirProcessFile(newItem, "", newBasePath, noMD5, noSHA1, addDate, headerToCheckAgainst, logger);
				}
			}
			// Otherwise, attempt to extract the files to the temporary directory
			else
			{
				ArchiveScanLevel asl = (archivesAsFiles ? ArchiveScanLevel.SevenZipExternal : ArchiveScanLevel.SevenZipInternal)
					| (!archivesAsFiles && enableGzip ? ArchiveScanLevel.GZipInternal : ArchiveScanLevel.GZipExternal)
					| (archivesAsFiles ? ArchiveScanLevel.RarExternal : ArchiveScanLevel.RarInternal)
					| (archivesAsFiles ? ArchiveScanLevel.ZipExternal : ArchiveScanLevel.ZipInternal);

				bool encounteredErrors = ArchiveTools.ExtractArchive(newItem, tempSubDir, asl, logger);

				// If the file was an archive and was extracted successfully, check it
				if (!encounteredErrors)
				{
					logger.Verbose(Path.GetFileName(item) + " treated like an archive");
					List<string> extracted = Directory.EnumerateFiles(tempSubDir, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(extracted,
						new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
						entry =>
						{
							PopulateFromDirProcessFile(entry,
								Path.Combine((Type == "SuperDAT"
									? (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length)
									: ""),
								Path.GetFileNameWithoutExtension(item)),
								tempSubDir,
								noMD5,
								noSHA1,
								addDate,
								headerToCheckAgainst,
								logger);
						});
				}
				// Otherwise, just get the info on the file itself
				else if (File.Exists(newItem))
				{
					PopulateFromDirProcessFile(newItem, "", newBasePath, noMD5, noSHA1, addDate, headerToCheckAgainst, logger);
				}
			}

			// Cue to delete the file if it's a copy
			if (copyFiles && item != newItem)
			{
				try
				{
					Directory.Delete(newBasePath, true);
				}
				catch { }
			}

			// Delete the sub temp directory
			if (Directory.Exists(tempSubDir))
			{
				Directory.Delete(tempSubDir, true);
			}
		}

		/// <summary>
		/// Process a single file as a file
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="basePath">Path the represents the parent directory</param>
		/// <param name="noMD5">True if MD5 hashes should be skipped over, false otherwise</param>
		/// <param name="noSHA1">True if SHA-1 hashes should be skipped over, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="logger">Logger object for console and file output</param>
		private void PopulateFromDirProcessFile(string item, string parent, string basePath, bool noMD5, bool noSHA1, bool addDate, string headerToCheckAgainst, Logger logger)
		{
			logger.Verbose(Path.GetFileName(item) + " treated like a file");
			Rom rom = FileTools.GetFileInfo(item, logger, noMD5: noMD5, noSHA1: noSHA1, date: addDate, header: headerToCheckAgainst);

			PopulateFromDirProcessFileHelper(item, rom, basePath, parent, logger);
		}

		/// <summary>
		/// Process a single file as a file (with found Rom data)
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="item">Rom data to be used to write to file</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		private void PopulateFromDirProcessFileHelper(string item, DatItem datItem, string basepath, string parent, Logger logger)
		{
			// If the datItem isn't a Rom or Disk, return
			if (datItem.Type != ItemType.Rom && datItem.Type != ItemType.Disk)
			{
				return;
			}

			string key = "";
			if (datItem.Type == ItemType.Rom)
			{
				key = ((Rom)datItem).Size + "-" + ((Rom)datItem).CRC;
			}
			else
			{
				key = ((Disk)datItem).MD5;
			}

			// Add the list if it doesn't exist already
			Add(key);

			try
			{
				// If the basepath ends with a directory separator, remove it
				if (!basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					basepath += Path.DirectorySeparatorChar.ToString();
				}

				// Make sure we have the full item path
				item = Path.GetFullPath(item);

				// Get the data to be added as game and item names
				string gamename = "";
				string romname = "";

				// If the parent is blank, then we have a non-archive file
				if (parent == "")
				{
					// If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
					if (Type == "SuperDAT")
					{
						gamename = Path.GetDirectoryName(item.Remove(0, basepath.Length));
						romname = Path.GetFileName(item);
					}

					// Otherwise, we want just the top level folder as the game, and the file as everything else
					else
					{
						gamename = item.Remove(0, basepath.Length).Split(Path.DirectorySeparatorChar)[0];
						romname = item.Remove(0, (Path.Combine(basepath, gamename).Length));
					}
				}

				// Otherwise, we assume that we have an archive
				else
				{
					// If we have a SuperDAT, we want the archive name as the game, and the file as everything else (?)
					if (Type == "SuperDAT")
					{
						gamename = parent;
						romname = item.Remove(0, basepath.Length);
					}

					// Otherwise, we want the archive name as the game, and the file as everything else
					else
					{
						gamename = parent;
						romname = item.Remove(0, basepath.Length);
					}
				}

				// Sanitize the names
				if (gamename.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					gamename = gamename.Substring(1);
				}
				if (gamename.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					gamename = gamename.Substring(0, gamename.Length - 1);
				}
				if (romname.StartsWith(Path.DirectorySeparatorChar.ToString()))
				{
					romname = romname.Substring(1);
				}
				if (romname.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					romname = romname.Substring(0, romname.Length - 1);
				}
				if (!String.IsNullOrEmpty(gamename) && String.IsNullOrEmpty(romname))
				{
					romname = gamename;
					gamename = "Default";
				}

				// Update rom information
				datItem.Name = romname;
				if (datItem.Machine == null)
				{
					datItem.Machine = new Machine
					{
						Name = gamename,
						Description = gamename,
					};
				}
				else
				{
					datItem.Machine.Name = gamename;
					datItem.Machine.Description = gamename;
				}

				// Add the file information to the DAT
				Add(key, datItem);

				logger.User("File added: " + romname + Environment.NewLine);
			}
			catch (IOException ex)
			{
				logger.Error(ex.ToString());
				return;
			}
		}

		#endregion
	}
}