using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using IOException = System.IO.IOException;
using SearchOption = System.IO.SearchOption;
#endif
using SharpCompress.Common;

namespace SabreTools.Library.Dats
{
	public partial class DatFile
	{
		#region Populate DAT from Directory [MODULAR DONE, FOR NOW]

		/// <summary>
		/// Create a new Dat from a directory
		/// </summary>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="skipFileType">Type of files that should be skipped</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="outDir">Output directory to </param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		public bool PopulateFromDir(string basePath, Hash omitFromScan, bool bare, bool archivesAsFiles, bool enableGzip,
			 SkipFileType skipFileType, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst)
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
				Globals.Logger.Verbose("Folder found: {0}", basePath);

				// Process the files in the main folder
				List<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.TopDirectoryOnly).ToList();
				Parallel.ForEach(files, Globals.ParallelOptions, item =>
				{
					PopulateFromDirCheckFile(item, basePath, omitFromScan, bare, archivesAsFiles, enableGzip, skipFileType,
						addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst);
				});

				// Find all top-level subfolders
				files = Directory.EnumerateDirectories(basePath, "*", SearchOption.TopDirectoryOnly).ToList();
				foreach (string item in files)
				{
					List<string> subfiles = Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories).ToList();
					Parallel.ForEach(subfiles, Globals.ParallelOptions, subitem =>
					{
						PopulateFromDirCheckFile(subitem, basePath, omitFromScan, bare, archivesAsFiles, enableGzip, skipFileType,
							addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst);
					});
				}

				// Now find all folders that are empty, if we are supposed to
				if (!Romba && addBlanks)
				{
					List<string> empties = FileTools.GetEmptyDirectories(basePath).ToList();
					Parallel.ForEach(empties, Globals.ParallelOptions, dir =>
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
							romname = "_";
						}

						// Otherwise, we want just the top level folder as the game, and the file as everything else
						else
						{
							gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
							romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "_");
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

						Globals.Logger.Verbose("Adding blank empty folder: {0}", gamename);
						this["null"].Add(new Rom(romname, gamename, omitFromScan));
					});
				}
			}
			else if (File.Exists(basePath))
			{
				PopulateFromDirCheckFile(basePath, Path.GetDirectoryName(Path.GetDirectoryName(basePath)), omitFromScan, bare, archivesAsFiles, enableGzip,
					skipFileType, addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst);
			}

			// Now that we're done, delete the temp folder (if it's not the default)
			Globals.Logger.User("Cleaning temp folder");
			if (tempDir != Path.GetTempPath())
			{
				FileTools.TryDeleteDirectory(tempDir);
			}

			return true;
		}

		/// <summary>
		/// Check a given file for hashes, based on current settings
		/// </summary>
		/// <param name="item">Filename of the item to be checked</param>
		/// <param name="basePath">Base folder to be used in creating the DAT</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="enableGzip">True if GZIP archives should be treated as files, false otherwise</param>
		/// <param name="skipFileType">Type of files that should be skipped</param>
		/// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		private void PopulateFromDirCheckFile(string item, string basePath, Hash omitFromScan, bool bare, bool archivesAsFiles,
			bool enableGzip, SkipFileType skipFileType, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst)
		{
			// Define the temporary directory
			string tempSubDir = Path.GetFullPath(Path.Combine(tempDir, Path.GetRandomFileName())) + Path.DirectorySeparatorChar;

			// Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
			if (Romba)
			{
				Rom rom = ArchiveTools.GetTorrentGZFileInfo(item);

				// If the rom is valid, write it out
				if (rom != null && rom.Name != null)
				{
					// Add the list if it doesn't exist already
					Add(rom.Size + "-" + rom.CRC, rom);
					Globals.Logger.User("File added: {0}", Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
				}
				else
				{
					Globals.Logger.User("File not added: {0}", Path.GetFileNameWithoutExtension(item) + Environment.NewLine);
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

			// Create a list for all found items
			List<Rom> extracted = null;

			// Temporarily set the archivesAsFiles if we have a GZip archive and we're not supposed to use it as one
			if (archivesAsFiles && !enableGzip && newItem.EndsWith(".gz"))
			{
				archivesAsFiles = false;
			}
			
			// If we don't have archives as files, try to scan the file as an archive
			if (!archivesAsFiles)
			{
				// If all deep hash skip flags are set, do a quickscan
				if (omitFromScan == Hash.SecureHashes)
				{
					extracted = ArchiveTools.GetArchiveFileInfo(newItem, date: addDate);
				}
				// Otherwise, get the list with whatever hashes are wanted
				else
				{
					extracted = ArchiveTools.GetExtendedArchiveFileInfo(newItem, omitFromScan: omitFromScan, date: addDate);
				}
			}

			// If the file should be skipped based on type, do so now
			if ((extracted != null && skipFileType == SkipFileType.Archive)
				|| (extracted == null && skipFileType == SkipFileType.File))
			{
				return;
			}

			// If the extracted list is null, just scan the item itself
			if (extracted == null || archivesAsFiles)
			{
				PopulateFromDirProcessFile(newItem, "", newBasePath, omitFromScan, addDate, headerToCheckAgainst);
			}
			// Otherwise, add all of the found items
			else
			{
				// First take care of the found items
				Parallel.ForEach(extracted, Globals.ParallelOptions, rom =>
				{
					PopulateFromDirProcessFileHelper(newItem,
						rom,
						basePath,
						(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
				});

				// Then, if we're looking for blanks, get all of the blank folders and add them
				if (addBlanks)
				{
					List<string> empties = ArchiveTools.GetEmptyFoldersInArchive(newItem);
					Parallel.ForEach(empties, Globals.ParallelOptions, empty =>
					{
						Rom emptyRom = new Rom(Path.Combine(empty, "_"), newItem, omitFromScan);
						PopulateFromDirProcessFileHelper(newItem,
							emptyRom,
							basePath,
							(Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
					});
				}
			}

			// Cue to delete the file if it's a copy
			if (copyFiles && item != newItem)
			{
				FileTools.TryDeleteDirectory(newBasePath);
			}

			// Delete the sub temp directory
			FileTools.TryDeleteDirectory(tempSubDir);
		}

		/// <summary>
		/// Process a single file as a file
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="parent">Parent game to be used</param>
		/// <param name="basePath">Path the represents the parent directory</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		private void PopulateFromDirProcessFile(string item, string parent, string basePath, Hash omitFromScan,
			bool addDate, string headerToCheckAgainst)
		{
			Globals.Logger.Verbose("'{0}' treated like a file", Path.GetFileName(item));
			Rom rom = FileTools.GetFileInfo(item, omitFromScan: omitFromScan, date: addDate, header: headerToCheckAgainst);

			PopulateFromDirProcessFileHelper(item, rom, basePath, parent);
		}

		/// <summary>
		/// Process a single file as a file (with found Rom data)
		/// </summary>
		/// <param name="item">File to be added</param>
		/// <param name="item">Rom data to be used to write to file</param>
		/// <param name="basepath">Path the represents the parent directory</param>
		/// <param name="parent">Parent game to be used</param>
		private void PopulateFromDirProcessFileHelper(string item, DatItem datItem, string basepath, string parent)
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
						romname = datItem.Name;
					}

					// Otherwise, we want the archive name as the game, and the file as everything else
					else
					{
						gamename = parent;
						romname = datItem.Name;
					}
				}

				// Sanitize the names
				if (romname == null)
				{
					romname = "";
				}
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
				datItem.MachineName = gamename;
				datItem.Description = gamename;

				// Add the file information to the DAT
				Add(key, datItem);

				Globals.Logger.User("File added: {0}", romname + Environment.NewLine);
			}
			catch (IOException ex)
			{
				Globals.Logger.Error(ex.ToString());
				return;
			}
		}

		#endregion
	}
}
