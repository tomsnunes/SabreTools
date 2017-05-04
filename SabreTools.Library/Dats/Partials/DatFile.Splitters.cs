using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using SabreTools.Helper.Data;
using SabreTools.Helper.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif
using NaturalSort;

namespace SabreTools.Helper.Dats
{
	public partial class DatFile
	{
		#region Splitting

		/// <summary>
		/// Split a DAT by input extensions
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="extA">List of extensions to split on (first DAT)</param>
		/// <param name="extB">List of extensions to split on (second DAT)</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByExt(string outDir, string basepath, List<string> extA, List<string> extB)
		{
			// Make sure all of the extensions have a dot at the beginning
			List<string> newExtA = new List<string>();
			Parallel.ForEach(extA, Globals.ParallelOptions, s =>
			{
				lock (newExtA)
				{
					newExtA.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
				}
			});
			string newExtAString = string.Join(",", newExtA);

			List<string> newExtB = new List<string>();
			Parallel.ForEach(extB, Globals.ParallelOptions, s =>
			{
				lock (newExtB)
				{
					newExtB.Add((s.StartsWith(".") ? s : "." + s).ToUpperInvariant());
				}
			});
			string newExtBString = string.Join(",", newExtB);

			// Set all of the appropriate outputs for each of the subsets
			DatFile datdataA = new DatFile
			{
				FileName = this.FileName + " (" + newExtAString + ")",
				Name = this.Name + " (" + newExtAString + ")",
				Description = this.Description + " (" + newExtAString + ")",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				DatFormat = this.DatFormat,
			};
			DatFile datdataB = new DatFile
			{
				FileName = this.FileName + " (" + newExtBString + ")",
				Name = this.Name + " (" + newExtBString + ")",
				Description = this.Description + " (" + newExtBString + ")",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				DatFormat = this.DatFormat,
			};

			// If roms is empty, return false
			if (Count == 0)
			{
				return false;
			}

			// Now separate the roms accordingly
			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				Parallel.ForEach(items, Globals.ParallelOptions, item =>
				{
					if (newExtA.Contains(Path.GetExtension(item.Name.ToUpperInvariant())))
					{
						datdataA.Add(key, item);
					}
					else if (newExtB.Contains(Path.GetExtension(item.Name.ToUpperInvariant())))
					{
						datdataB.Add(key, item);
					}
					else
					{
						datdataA.Add(key, item);
						datdataB.Add(key, item);
					}
				});
			});

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(this.FileName).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(this.FileName);
			}

			// Then write out both files
			bool success = datdataA.WriteToFile(outDir);
			success &= datdataB.WriteToFile(outDir);

			return success;
		}

		/// <summary>
		/// Split a DAT by best available hashes
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByHash(string outDir, string basepath)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Create each of the respective output DATs
			Globals.Logger.User("Creating and populating new DATs");
			DatFile nodump = new DatFile
			{
				FileName = this.FileName + " (Nodump)",
				Name = this.Name + " (Nodump)",
				Description = this.Description + " (Nodump)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile sha512 = new DatFile
			{
				FileName = this.FileName + " (SHA-512)",
				Name = this.Name + " (SHA-512)",
				Description = this.Description + " (SHA-512)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile sha384 = new DatFile
			{
				FileName = this.FileName + " (SHA-384)",
				Name = this.Name + " (SHA-384)",
				Description = this.Description + " (SHA-384)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile sha256 = new DatFile
			{
				FileName = this.FileName + " (SHA-256)",
				Name = this.Name + " (SHA-256)",
				Description = this.Description + " (SHA-256)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile sha1 = new DatFile
			{
				FileName = this.FileName + " (SHA-1)",
				Name = this.Name + " (SHA-1)",
				Description = this.Description + " (SHA-1)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile md5 = new DatFile
			{
				FileName = this.FileName + " (MD5)",
				Name = this.Name + " (MD5)",
				Description = this.Description + " (MD5)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile crc = new DatFile
			{
				FileName = this.FileName + " (CRC)",
				Name = this.Name + " (CRC)",
				Description = this.Description + " (CRC)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile other = new DatFile
			{
				FileName = this.FileName + " (Other)",
				Name = this.Name + " (Other)",
				Description = this.Description + " (Other)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				Parallel.ForEach(items, Globals.ParallelOptions, item =>
				{
					// If the file is not a Rom or Disk, continue
					if (item.Type != ItemType.Disk && item.Type != ItemType.Rom)
					{
						return;
					}

					// If the file is a nodump
					if ((item.Type == ItemType.Rom && ((Rom)item).ItemStatus == ItemStatus.Nodump)
						|| (item.Type == ItemType.Disk && ((Disk)item).ItemStatus == ItemStatus.Nodump))
					{
						nodump.Add(key, item);
					}
					// If the file has a SHA-512
					else if ((item.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)item).SHA512))
						|| (item.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)item).SHA512)))
					{
						sha512.Add(key, item);
					}
					// If the file has a SHA-384
					else if ((item.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)item).SHA384))
						|| (item.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)item).SHA384)))
					{
						sha384.Add(key, item);
					}
					// If the file has a SHA-256
					else if ((item.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)item).SHA256))
						|| (item.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)item).SHA256)))
					{
						sha256.Add(key, item);
					}
					// If the file has a SHA-1
					else if ((item.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)item).SHA1))
						|| (item.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)item).SHA1)))
					{
						sha1.Add(key, item);
					}
					// If the file has no SHA-1 but has an MD5
					else if ((item.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)item).MD5))
						|| (item.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)item).MD5)))
					{
						md5.Add(key, item);
					}
					// If the file has no MD5 but a CRC
					else if ((item.Type == ItemType.Rom && !String.IsNullOrEmpty(((Rom)item).SHA1))
						|| (item.Type == ItemType.Disk && !String.IsNullOrEmpty(((Disk)item).SHA1)))
					{
						crc.Add(key, item);
					}
					else
					{
						other.Add(key, item);
					}
				});
			});

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(this.FileName).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(this.FileName);
			}

			// Now, output all of the files to the output directory
			Globals.Logger.User("DAT information created, outputting new files");
			bool success = true;
			success &= nodump.WriteToFile(outDir);
			success &= sha512.WriteToFile(outDir);
			success &= sha384.WriteToFile(outDir);
			success &= sha256.WriteToFile(outDir);
			success &= sha1.WriteToFile(outDir);
			success &= md5.WriteToFile(outDir);
			success &= crc.WriteToFile(outDir);

			return success;
		}

		/// <summary>
		/// Split a SuperDAT by lowest available directory level
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <param name="shortname">True if short names should be used, false otherwise</param>
		/// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByLevel(string outDir, string basepath, bool shortname, bool basedat)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// First, organize by games so that we can do the right thing
			BucketBy(SortedBy.Game, false /* mergeroms */, lower: false, norename: true);

			// Create a temporary DAT to add things to
			DatFile tempDat = new DatFile(this)
			{
				Name = null,
			};

			// Sort the input keys
			List<string> keys = Keys.ToList();
			keys.Sort(SplitByLevelSort);

			// Then, we loop over the games
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				// Here, the key is the name of the game to be used for comparison
				if (tempDat.Name != null && tempDat.Name != Style.GetDirectoryName(key))
				{
					// Process and output the DAT
					SplitByLevelHelper(tempDat, outDir, shortname, basedat);

					// Reset the DAT for the next items
					tempDat = new DatFile(this)
					{
						Name = null,
					};
				}

				// Clean the input list and set all games to be pathless
				List<DatItem> items = this[key];
				items.ForEach(item => item.Machine.Name = Style.GetFileName(item.Machine.Name));
				items.ForEach(item => item.Machine.Description = Style.GetFileName(item.Machine.Description));

				// Now add the game to the output DAT
				tempDat.AddRange(key, items);

				// Then set the DAT name to be the parent directory name
				tempDat.Name = Style.GetDirectoryName(key);
			});

			// Then we write the last DAT out since it would be skipped otherwise
			SplitByLevelHelper(tempDat, outDir, shortname, basedat);

			return true;
		}

		/// <summary>
		/// Helper function for SplitByLevel to sort the input game names
		/// </summary>
		/// <param name="a">First string to compare</param>
		/// <param name="b">Second string to compare</param>
		/// <returns>-1 for a coming before b, 0 for a == b, 1 for a coming after b</returns>
		private int SplitByLevelSort(string a, string b)
		{
			NaturalComparer nc = new NaturalComparer();
			int adeep = a.Count(c => c == '/' || c == '\\');
			int bdeep = b.Count(c => c == '/' || c == '\\');

			if (adeep == bdeep)
			{
				return nc.Compare(a, b);
			}
			return adeep - bdeep;
		}

		/// <summary>
		/// Helper function for SplitByLevel to clean and write out a DAT
		/// </summary>
		/// <param name="datFile">DAT to clean and write out</param>
		/// <param name="outDir">Directory to write out to</param>
		/// <param name="shortname">True if short naming scheme should be used, false otherwise</param>
		/// <param name="restore">True if original filenames should be used as the base for output filename, false otherwise</param>
		private void SplitByLevelHelper(DatFile datFile, string outDir, bool shortname, bool restore)
		{
			// Get the name from the DAT to use separately
			string name = datFile.Name;
			string expName = name.Replace("/", " - ").Replace("\\", " - ");

			// Get the path that the file will be written out to
			string path = HttpUtility.HtmlDecode(String.IsNullOrEmpty(name)
				? outDir
				: Path.Combine(outDir, name));

			// Now set the new output values
			datFile.FileName = HttpUtility.HtmlDecode(String.IsNullOrEmpty(name)
				? FileName
				: (shortname
					? Style.GetFileName(name)
					: expName
					)
				);
			datFile.FileName = (restore ? FileName + " (" + datFile.FileName + ")" : datFile.FileName);
			datFile.Name = Name + " (" + expName + ")";
			datFile.Description = (String.IsNullOrEmpty(Description) ? datFile.Name : Description + " (" + expName + ")");
			datFile.Type = null;

			// Write out the temporary DAT to the proper directory
			datFile.WriteToFile(path);
		}

		/// <summary>
		/// Split a DAT by type of Rom
		/// </summary>
		/// <param name="outDir">Name of the directory to write the DATs out to</param>
		/// <param name="basepath">Parent path for replacement</param>
		/// <returns>True if split succeeded, false otherwise</returns>
		public bool SplitByType(string outDir, string basepath)
		{
			// Sanitize the basepath to be more predictable
			basepath = (basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath : basepath + Path.DirectorySeparatorChar);

			// Create each of the respective output DATs
			Globals.Logger.User("Creating and populating new DATs");
			DatFile romdat = new DatFile
			{
				FileName = this.FileName + " (ROM)",
				Name = this.Name + " (ROM)",
				Description = this.Description + " (ROM)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile diskdat = new DatFile
			{
				FileName = this.FileName + " (Disk)",
				Name = this.Name + " (Disk)",
				Description = this.Description + " (Disk)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};
			DatFile sampledat = new DatFile
			{
				FileName = this.FileName + " (Sample)",
				Name = this.Name + " (Sample)",
				Description = this.Description + " (Sample)",
				Category = this.Category,
				Version = this.Version,
				Date = this.Date,
				Author = this.Author,
				Email = this.Email,
				Homepage = this.Homepage,
				Url = this.Url,
				Comment = this.Comment,
				Header = this.Header,
				Type = this.Type,
				ForceMerging = this.ForceMerging,
				ForceNodump = this.ForceNodump,
				ForcePacking = this.ForcePacking,
				DatFormat = this.DatFormat,
				MergeRoms = this.MergeRoms,
			};

			// Now populate each of the DAT objects in turn
			List<string> keys = Keys.ToList();
			Parallel.ForEach(keys, Globals.ParallelOptions, key =>
			{
				List<DatItem> items = this[key];
				Parallel.ForEach(items, Globals.ParallelOptions, item =>
				{
					// If the file is a Rom
					if (item.Type == ItemType.Rom)
					{
						romdat.Add(key, item);
					}
					// If the file is a Disk
					else if (item.Type == ItemType.Disk)
					{
						diskdat.Add(key, item);
					}
					// If the file is a Sample
					else if (item.Type == ItemType.Sample)
					{
						sampledat.Add(key, item);
					}
				});
			});

			// Get the output directory
			if (outDir != "")
			{
				outDir = outDir + Path.GetDirectoryName(this.FileName).Remove(0, basepath.Length - 1);
			}
			else
			{
				outDir = Path.GetDirectoryName(this.FileName);
			}

			// Now, output all of the files to the output directory
			Globals.Logger.User("DAT information created, outputting new files");
			bool success = true;
			success &= romdat.WriteToFile(outDir);
			success &= diskdat.WriteToFile(outDir);
			success &= sampledat.WriteToFile(outDir);

			return success;
		}

		#endregion
	}
}
