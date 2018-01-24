using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;

namespace SabreTools
{
	public partial class SabreTools
	{
		#region Private Flag features

		private static Feature addBlankFilesFlag
		{
			get
			{
				return new Feature(
					"add-blank-files",
					new List<string>() { "-ab", "--add-blank-files" },
					"Output blank files for folders",
					FeatureType.Flag,
					longDescription: "If this flag is set, then blank entries will be created for each of the empty directories in the source. This is useful for tools that require all folders be accounted for in the output DAT.");
			}
		}
		private static Feature addDateFlag
		{
			get
			{
				return new Feature(
					"add-date",
					new List<string>() { "-ad", "--add-date" },
					"Add dates to items, where posible",
					FeatureType.Flag,
					longDescription: "If this flag is set, then the Date will be appended to each file information in the output DAT. The output format is standardized as \"yyyy/MM/dd HH:mm:ss\".");
			}
		}
		private static Feature againstFlag
		{
			get
			{
				return new Feature(
					"against",
					new List<string>() { "-ag", "--against" },
					"Diff all inputs against a set of base DATs",
					FeatureType.Flag,
					"This flag will enable a special type of diffing in which a set of base DATs are used as a comparison point for each of the input DATs. This allows users to get a slightly different output to cascaded diffing, which may be more useful in some cases. This is heavily influenced by the diffing model used by Romba.");
			}
		}
		private static Feature allStatsFlag
		{
			get
			{
				return new Feature(
					"all-stats",
					new List<string>() { "-as", "--all-stats" },
					"Write all statistics to all available formats",
					FeatureType.Flag,
					longDescription: "Output all rom information to all available formats.");
			}
		}
		private static Feature archivesAsFilesFlag
		{
			get
			{
				return new Feature(
					"archives-as-files",
					new List<string>() { "-f", "--archives-as-files" },
					"Treat archives as files",
					FeatureType.Flag,
					longDescription: "Instead of trying to enumerate the files within archives, treat the archives as files themselves. This is good for uncompressed sets that include archives that should be read as-is.");
			}
		}
		private static Feature baddumpColumnFlag
		{
			get
			{
				return new Feature(
					"baddump-column",
					new List<string>() { "-bc", "--baddump-column" },
					"Add baddump stats to output",
					FeatureType.Flag,
					longDescription: "Add a new column or field for counting the number of baddumps in the DAT.");
			}
		}
		private static Feature baseFlag
		{
			get
			{
				return new Feature(
					"base",
					new List<string>() { "-ba", "--base" },
					"Use source DAT as base name for outputs",
					FeatureType.Flag,
					longDescription: "If splitting an entire folder of DATs, some output files may be normally overwritten since the names would be the same. With this flag, the original DAT name is used in the output name, in the format of \"Original Name(Dir - Name)\". This can be used in conjunction with --short to output in the format of \"Original Name (Name)\" instead.");
			}
		}
		private static Feature baseReplaceFlag
		{
			get
			{
				return new Feature(
					"base-replace",
					new List<string>() { "-br", "--base-replace" },
					"Replace from base DATs in order",
					FeatureType.Flag,
					longDescription: "By default, no item names are changed except when there is a merge occurring. This flag enables users to define a DAT or set of base DATs to use as \"replacements\" for all input DATs. Note that the first found instance of an item in the base DAT(s) will be used and all others will be discarded. If no additional flag is given, it will default to updating names.");
			}
		}
		private static Feature cascadeFlag
		{
			get
			{
				return new Feature(
					"cascade",
					new List<string>() { "-c", "--cascade" },
					"Enable cascaded diffing",
					FeatureType.Flag,
					longDescription: "This flag allows for a special type of diffing in which the first DAT is considered a base, and for each additional input DAT, it only leaves the files that are not in one of the previous DATs. This can allow for the creation of rollback sets or even just reduce the amount of duplicates across multiple sets.");
			}
		}
		private static Feature chdsAsFilesFlag
		{
			get
			{
				return new Feature(
					"chds-as-files",
					new List<string>() { "-ic", "--chds-as-files" },
					"Treat CHDs as regular files",
					FeatureType.Flag,
					longDescription: "Normally, CHDs would be processed using their internal hash to compare against the input DATs. This flag forces all CHDs to be treated like regular files.");
			}
		}
		private static Feature cleanFlag
		{
			get
			{
				return new Feature(
					"clean",
					new List<string>() { "-clean", "--clean" },
					"Clean game names according to WoD standards",
					FeatureType.Flag,
					longDescription: "Game names will be sanitized to remove what the original WoD standards deemed as unneeded information, such as parenthesized or bracketed strings.");
			}
		}
		private static Feature copyFilesFlag
		{
			get
			{
				return new Feature(
					"copy-files",
					new List<string>() { "-cf", "--copy-files" },
					"Copy files to the temp directory before parsing",
					FeatureType.Flag,
					longDescription: "If this flag is set, then all files that are going to be parsed are moved to the temporary directory before being hashed. This can be helpful in cases where the temp folder is located on an SSD and the user wants to take advantage of this.");
			}
		}
		private static Feature csvFlag
		{
			get
			{
				return new Feature(
					"csv",
					new List<string>() { "-csv", "--csv" },
					"Output in Comma-Separated Value format",
					FeatureType.Flag,
					longDescription: "Output all statistical information in standardized CSV format.");
			}
		}
		private static Feature datDeviceNonMergedFlag
		{
			get
			{
				return new Feature(
					"dat-device-non-merged",
					new List<string>() { "-dnd", "--dat-device-non-merged" },
					"Create device non-merged sets",
					FeatureType.Flag,
					longDescription: "Preprocess the DAT to have child sets contain all items from the device references. This is incompatible with the other --dat-X flags.");
			}
		}
		private static Feature datFullNonMergedFlag
		{
			get
			{
				return new Feature(
					"dat-full-non-merged",
					new List<string>() { "-df", "--dat-full-non-merged" },
					"Create fully non-merged sets",
					FeatureType.Flag,
					longDescription: "Preprocess the DAT to have child sets contain all items from the parent sets based on the cloneof and romof tags as well as device references. This is incompatible with the other --dat-X flags.");
			}
		}
		private static Feature datMergedFlag
		{
			get
			{
				return new Feature(
					"dat-merged",
					new List<string>() { "-dm", "--dat-merged" },
					"Force creating merged sets",
					FeatureType.Flag,
					longDescription: "Preprocess the DAT to have parent sets contain all items from the children based on the cloneof tag. This is incompatible with the other --dat-X flags.");
			}
		}
		private static Feature datNonMergedFlag
		{
			get
			{
				return new Feature(
					"dat-non-merged",
					new List<string>() { "-dnm", "--dat-non-merged" },
					"Force creating non-merged sets",
					FeatureType.Flag,
					longDescription: "Preprocess the DAT to have child sets contain all items from the parent set based on the romof and cloneof tags. This is incompatible with the other --dat-X flags.");
			}
		}
		private static Feature datSplitFlag
		{
			get
			{
				return new Feature(
					"dat-split",
					new List<string>() { "-ds", "--dat-split" },
					"Force creating split sets",
					FeatureType.Flag,
					longDescription: "Preprocess the DAT to remove redundant files between parents and children based on the romof and cloneof tags. This is incompatible with the other --dat-X flags.");
			}
		}
		private static Feature dedupFlag
		{
			get
			{
				return new Feature(
					"dedup",
					new List<string>() { "-dd", "--dedup" },
					"Enable deduping in the created DAT",
					FeatureType.Flag,
					longDescription: "For all outputted DATs, allow for hash deduping. This makes sure that there are effectively no duplicates in the output files. Cannot be used with game dedup.");
			}
		}
		private static Feature deleteFlag
		{
			get
			{
				return new Feature(
					"delete",
					new List<string>() { "-del", "--delete" },
					"Delete fully rebuilt input files",
					FeatureType.Flag,
					longDescription: "Optionally, the input files, once processed and fully matched, can be deleted. This can be useful when the original file structure is no longer needed or if there is limited space on the source drive.");
			}
		}
		private static Feature depotFlag
		{
			get
			{
				return new Feature(
					"depot",
					new List<string>() { "-dep", "--depot" },
					"Assume directories are romba depots",
					FeatureType.Flag,
					longDescription: "Normally, input directories will be treated with no special format. If this flag is used, all input directories will be assumed to be romba-style depots.");
			}
		}
		private static Feature depreciatedFlag
		{
			get
			{
				return new Feature(
					"depreciated",
					new List<string>() { "-dpc", "--depreciated" },
					"Output 'game' instead of 'machine'",
					FeatureType.Flag,
					longDescription: "By default, Logiqx XML DATs output with the more modern \"machine\" tag for each set. This flag allows users to output the older \"game\" tag instead, for compatibility reasons. [Logiqx only]");
			}
		}
		private static Feature descriptionAsNameFlag
		{
			get
			{
				return new Feature(
					"description-as-name",
					new List<string>() { "-dan", "--description-as-name" },
					"Use description instead of machine name",
					FeatureType.Flag,
					longDescription: "By default, all DATs are converted exactly as they are input. Enabling this flag allows for the machine names in the DAT to be replaced by the machine description instead. In most cases, this will result in no change in the output DAT, but a notable example would be a software list DAT where the machine names are generally DOS-friendly while the description is more complete.");
			}
		}
		private static Feature diffFlag
		{
			get
			{
				return new Feature(
					"diff",
					new List<string>() { "-di", "--diff" },
					"Create diffdats from inputs (all outputs)",
					FeatureType.Flag,
					longDescription: "By default, all DATs are processed individually with the user-specified flags. With this flag enabled, input DATs are diffed against each other in all ways specified by the --diff-X flags.");
			}
		}
		private static Feature diffDuFlag
		{
			get
			{
				return new Feature(
					"diff-du",
					new List<string>() { "-did", "--diff-du" },
					"Create diffdat containing just duplicates",
					FeatureType.Flag,
					longDescription: "All files that have duplicates outside of the original DAT are included.");
			}
		}
		private static Feature diffInFlag
		{
			get
			{
				return new Feature(
					"diff-in",
					new List<string>() { "-dii", "--diff-in" },
					"Create diffdats for individual DATs",
					FeatureType.Flag,
					longDescription: "All files that have no duplicates outside of the original DATs are put into DATs that are named after the source DAT.");
			}
		}
		private static Feature diffNdFlag
		{
			get
			{
				return new Feature(
					"diff-nd",
					new List<string>() { "-din", "--diff-nd" },
					"Create diffdat containing no duplicates",
					FeatureType.Flag,
					longDescription: "All files that have no duplicates outside of the original DATs are included.");
			}
		}
		private static Feature excludeOfFlag
		{
			get
			{
				return new Feature(
					"exclude-of",
					new List<string>() { "-xof", "--exclude-of" },
					"Exclude romof, cloneof, sampleof tags",
					FeatureType.Flag,
					longDescription: "If this flag is enabled, then the romof, cloneof, and sampleof tags will be omitted from the outputted DAT.");
			}
		}
		private static Feature extensionFlag
		{
			get
			{
				return new Feature(
					"extension",
					new List<string>() { "-es", "--extension" },
					"Split DAT(s) by two file extensions",
					FeatureType.Flag,
					longDescription: "For a DAT, or set of DATs, allow for splitting based on a list of input extensions. This can allow for combined DAT files, such as those combining two separate systems, to be split. Files with any extensions not listed in the input lists will be included in both outputted DAT files.");
			}
		}
		private static Feature gameDedupFlag
		{
			get
			{
				return new Feature(
					"game-dedup",
					new List<string>() { "-gdd", "--game-dedup" },
					"Enable deduping within games in the created DAT",
					FeatureType.Flag,
					longDescription: "For all outputted DATs, allow for hash deduping but only within the games, and not across the entire DAT. This makes sure that there are effectively no duplicates within each of the output sets. Cannot be used with standard dedup.");
			}
		}
		private static Feature gamePrefixFlag
		{
			get
			{
				return new Feature(
					"game-prefix",
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					longDescription: "This flag allows for the name of the game to be used as a prefix to each file. [Missfile, MD5, SFV, SHA* only]");
			}
		}
		private static Feature hashFlag
		{
			get
			{
				return new Feature(
					"hash",
					new List<string>() { "-hs", "--hash" },
					"Split DAT(s) or folder by best-available hashes",
					FeatureType.Flag,
					longDescription: "For a DAT, or set of DATs, allow for splitting based on the best available hash for each file within. The order of preference for the outputted DATs is as follows: Nodump, SHA-512, SHA-384, SHA-256, SHA-1, MD5, CRC (or worse).");
			}
		}
		private static Feature hashOnlyFlag
		{
			get
			{
				return new Feature(
					"hash-only",
					new List<string>() { "-ho", "--hash-only" },
					"Check files by hash only",
					FeatureType.Flag,
					longDescription: "This sets a mode where files are not checked based on name but rather hash alone. This allows verification of (possibly) incorrectly named folders and sets to be verified without worrying about the proper set structure to be there.");
			}
		}
		private static Feature htmlFlag
		{
			get
			{
				return new Feature(
					"html",
					new List<string>() { "-html", "--html" },
					"Output in HTML format",
					FeatureType.Flag,
					longDescription: "Output all statistical information in standardized HTML format.");
			}
		}
		private static Feature individualFlag
		{
			get
			{
				return new Feature(
					"individual",
					new List<string>() { "-ind", "--individual" },
					"Show individual statistics",
					FeatureType.Flag,
					longDescription: "Optionally, the statistics for each of the individual input DATs can be output as well.");
			}
		}
		private static Feature inplaceFlag
		{
			get
			{
				return new Feature(
					"inplace",
					new List<string>() { "-ip", "--inplace" },
					"Write to the input directories, where possible",
					FeatureType.Flag,
					longDescription: "By default, files are written to the runtime directory (or the output directory, if set). This flag enables users to write out to the directory that the DATs originated from.");
			}
		}
		private static Feature inverseFlag
		{
			get
			{
				return new Feature(
					"inverse",
					new List<string>() { "-in", "--inverse" },
					"Rebuild only files not in DAT",
					FeatureType.Flag,
					longDescription: "Instead of the normal behavior of rebuilding using a DAT, this flag allows the user to use the DAT as a filter instead. All files that are found in the DAT will be skipped and everything else will be output in the selected format.");
			}
		}
		private static Feature levelFlag
		{
			get
			{
				return new Feature(
					"level",
					new List<string>() { "-ls", "--level" },
					"Split a SuperDAT or folder by lowest available level",
					FeatureType.Flag,
					longDescription: "For a DAT, or set of DATs, allow for splitting based on the lowest available level of game name. That is, if a game name is top/mid/last, then it will create an output DAT for the parent directory \"mid\" in a folder called \"top\" with a game called \"last\".");
			}
		}
		private static Feature mergeFlag
		{
			get
			{
				return new Feature(
					"merge",
					new List<string>() { "-m", "--merge" },
					"Merge the input DATs",
					FeatureType.Flag,
					longDescription: "By default, all DATs are processed individually with the user-specified flags. With this flag enabled, all of the input DATs are merged into a single output. This is best used with the dedup flag.");
			}
		}
		private static Feature noAutomaticDateFlag
		{
			get
			{
				return new Feature(
					"no-automatic-date",
					new List<string>() { "-b", "--no-automatic-date" },
					"Don't include date in file name",
					FeatureType.Flag,
					longDescription: "Normally, the DAT will be created with the date in the file name in brackets. This flag removes that instead of the default.");
			}
		}
		private static Feature nodumpColumnFlag
		{
			get
			{
				return new Feature(
					"nodump-column",
					new List<string>() { "-nc", "--nodump-column" },
					"Add statistics for nodumps to output",
					FeatureType.Flag,
					longDescription: "Add a new column or field for counting the number of nodumps in the DAT.");
			}
		}
		private static Feature noStoreHeaderFlag
		{
			get
			{
				return new Feature(
					"no-store-header",
					new List<string>() { "-nsh", "--no-store-header" },
					"Don't store the extracted header",
					FeatureType.Flag,
					longDescription: "By default, all headers that are removed from files are backed up in the database. This flag allows users to skip that step entirely, avoiding caching the headers at all.");
			}
		}
		private static Feature notRunnableFlag
		{
			get
			{
				return new Feature(
					"not-runnable",
					new List<string>() { "-nrun", "--not-runnable" },
					"Include only items that are not marked runnable",
					FeatureType.Flag,
					longDescription: "This allows users to include only unrunnable games.");
			}
		}
		private static Feature matchOfTagsFlag
		{
			get
			{
				return new Feature(
					"match-of-tags",
					new List<string>() { "-ofg", "--match-of-tags" },
					"Allow cloneof and romof tags to match game name filters",
					FeatureType.Flag,
					longDescription: "If filter or exclude by game name is used, this flag will allow those filters to be checked against the romof and cloneof tags as well. This can allow for more advanced set-building, especially in arcade-based sets.");
			}
		}
		private static Feature onlySameFlag
		{
			get
			{
				return new Feature(
					"only-same",
					new List<string>() { "-ons", "--only-same" },
					"Only update description if machine name matches description",
					FeatureType.Flag,
					longDescription: "Normally, updating the description will always overwrite if the machine names are the same. With this flag, descriptions will only be overwritten if they are the same as the machine names.");
			}
		}
		private static Feature outputAllFlag
		{
			get
			{
				return new Feature(
					"output-all",
					new List<string>() { "-oa", "--output-all" },
					"Output in all formats [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputAttractmodeFlag
		{
			get
			{
				return new Feature(
					"output-attractmode",
					new List<string>() { "-oam", "--output-attractmode" },
					"Output in AttractMode format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputCmpFlag
		{
			get
			{
				return new Feature(
					"output-cmp",
					new List<string>() { "-oc", "--output-cmp" },
					"Output in CMP format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputCsvFlag
		{
			get
			{
				return new Feature(
					"output-csv",
					new List<string>() { "-ocsv", "--output-csv" },
					"Output in CSV format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputDoscenterFlag
		{
			get
			{
				return new Feature(
					"output-doscenter",
					new List<string>() { "-od", "--output-doscenter" },
					"Output in DOSCenter format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputListromFlag
		{
			get
			{
				return new Feature(
					"output-listrom",
					new List<string>() { "-olr", "--output-listrom" },
					"Output in MAME Listrom format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputListxmlFlag
		{
			get
			{
				return new Feature(
					"output-listxml",
					new List<string>() { "-olx", "--output-listxml" },
					"Output in MAME Listxml format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputMd5Flag
		{
			get
			{
				return new Feature(
					"output-md5",
					new List<string>() { "-omd5", "--output-md5" },
					"Output in MD5 format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputMissFlag
		{
			get
			{
				return new Feature(
					"output-miss",
					new List<string>() { "-om", "--output-miss" },
					"Output in Missfile format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputOfflinelistFlag
		{
			get
			{
				return new Feature(
					"output-offlinelist",
					new List<string>() { "-ool", "--output-offlinelist" },
					"Output in OfflineList format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputRomcenterFlag
		{
			get
			{
				return new Feature(
					"output-romcenter",
					new List<string>() { "-or", "--output-romcenter" },
					"Output in RomCenter format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSabredatFlag
		{
			get
			{
				return new Feature(
					"output-sabredat",
					new List<string>() { "-os", "--output-sabredat" },
					"Output in SabreDat format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSfvFlag
		{
			get
			{
				return new Feature(
					"output-sfv",
					new List<string>() { "-osfv", "--output-sfv" },
					"Output in SFV format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSha1Flag
		{
			get
			{
				return new Feature(
					"output-sha1",
					new List<string>() { "-osha1", "--output-sha1" },
					"Output in SHA-1 format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSha256Flag
		{
			get
			{
				return new Feature(
					"output-sha256",
					new List<string>() { "-osha256", "--output-sha256" },
					"Output in SHA-256 format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSha384Flag
		{
			get
			{
				return new Feature(
					"output-sha384",
					new List<string>() { "-osha384", "--output-sha384" },
					"Output in SHA-256 format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSha512Flag
		{
			get
			{
				return new Feature(
					"output-sha512",
					new List<string>() { "-osha512", "--output-sha512" },
					"Output in SHA-256 format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSoftwarelistFlag
		{
			get
			{
				return new Feature(
					"output-softwarelist",
					new List<string>() { "-osl", "--output-softwarelist" },
					"Output in Softwarelist format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputSsvFlag
		{
			get
			{
				return new Feature(
					"output-ssv",
					new List<string>() { "-ossv", "--output-ssv" },
					"Output in SSV format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputTsvFlag
		{
			get
			{
				return new Feature(
					"output-tsv",
					new List<string>() { "-otsv", "--output-tsv" },
					"Output in TSV format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature outputXmlFlag
		{
			get
			{
				return new Feature(
					"output-xml",
					new List<string>() { "-ox", "--output-xml" },
					"Output in Logiqx XML format [DEPRECIATED]",
					FeatureType.Flag,
					longDescription: "");
			}
		} // TODO: Remove
		private static Feature quickFlag
		{
			get
			{
				return new Feature(
					"quick",
					new List<string>() { "-qs", "--quick" },
					"Enable quick scanning of archives",
					FeatureType.Flag,
					longDescription: "For all archives, if this flag is enabled, it will only use the header information to get the archive entries' file information. The upside to this is that it is the fastest option. On the downside, it can only get the CRC and size from most archive formats, leading to possible issues.");
			}
		}
		private static Feature quotesFlag
		{
			get
			{
				return new Feature(
					"quotes",
					new List<string>() { "-q", "--quotes" },
					"Double-quote each item",
					FeatureType.Flag,
					longDescription: "This flag surrounds the item by double-quotes, not including the prefix or postfix. [*SV, Missfile only]");
			}
		}
		private static Feature removeExtensionsFlag
		{
			get
			{
				return new Feature(
					"remove-extensions",
					new List<string>() { "-rme", "--remove-extensions" },
					"Remove all extensions from all items",
					FeatureType.Flag,
					longDescription: "For each item, remove the extension. [Missfile only]");
			}
		}
		private static Feature removeMd5Flag
		{
			get
			{
				return new Feature(
					"remove-md5",
					new List<string>() { "-rmd5", "--remove-md5" },
					"Remove MD5 hashes from the output",
					FeatureType.Flag,
					longDescription: "By default, all available hashes will be written out to the DAT. This will remove all MD5 hashes from the output file(s).");
			}
		}
		private static Feature removeSha1Flag
		{
			get
			{
				return new Feature(
					"remove-sha1",
					new List<string>() { "-rsha1", "--remove-sha1" },
					"Remove SHA-1 hashes from the output",
					FeatureType.Flag,
					longDescription: "By default, all available hashes will be written out to the DAT. This will remove all SHA-1 hashes from the output file(s).");
			}
		}
		private static Feature removeSha256Flag
		{
			get
			{
				return new Feature(
					"remove-sha256",
					new List<string>() { "-rsha256", "--remove-sha256" },
					"Remove SHA-256 hashes from the output",
					FeatureType.Flag,
					longDescription: "By default, all available hashes will be written out to the DAT. This will remove all SHA-256 hashes from the output file(s).");
			}
		}
		private static Feature removeSha384Flag
		{
			get
			{
				return new Feature(
					"remove-sha384",
					new List<string>() { "-rsha384", "--remove-sha384" },
					"Remove SHA-384 hashes from the output",
					FeatureType.Flag,
					longDescription: "By default, all available hashes will be written out to the DAT. This will remove all SHA-384 hashes from the output file(s).");
			}
		}
		private static Feature removeSha512Flag
		{
			get
			{
				return new Feature(
					"remove-sha512",
					new List<string>() { "-rsha512", "--remove-sha512" },
					"Remove SHA-512 hashes from the output",
					FeatureType.Flag,
					longDescription: "By default, all available hashes will be written out to the DAT. This will remove all SHA-512 hashes from the output file(s).");
			}
		}
		private static Feature removeUnicodeFlag
		{
			get
			{
				return new Feature(
					"remove-unicode",
					new List<string>() { "-ru", "--remove-unicode" },
					"Remove unicode characters from names",
					FeatureType.Flag,
					longDescription: "By default, the character set from the original file(s) will be used for item naming. This flag removes all Unicode characters from the item names, machine names, and machine descriptions.");
			}
		}
		private static Feature reverseBaseReplaceFlag
		{
			get
			{
				return new Feature(
					"reverse-base-replace",
					new List<string>() { "-rbr", "--reverse-base-replace" },
					"Replace item names from base DATs in reverse",
					FeatureType.Flag,
					longDescription: "By default, no item names are changed except when there is a merge occurring. This flag enables users to define a DAT or set of base DATs to use as \"replacements\" for all input DATs. Note that the first found instance of an item in the last base DAT(s) will be used and all others will be discarded. If no additional flag is given, it will default to updating names.");
			}
		}
		private static Feature reverseCascadeFlag
		{
			get
			{
				return new Feature(
					"reverse-cascade",
					new List<string>() { "-rc", "--reverse-cascade" },
					"Enable reverse cascaded diffing",
					FeatureType.Flag,
					longDescription: "This flag allows for a special type of diffing in which the last DAT is considered a base, and for each additional input DAT, it only leaves the files that are not in one of the previous DATs. This can allow for the creation of rollback sets or even just reduce the amount of duplicates across multiple sets.");
			}
		}
		private static Feature rombaFlag
		{
			get
			{
				return new Feature(
					"romba",
					new List<string>() { "-ro", "--romba" },
					"Treat like a Romba depot (requires SHA-1)",
					FeatureType.Flag,
					longDescription: "This flag allows reading and writing of DATs and output files to and from a Romba-style depot. This also implies TorrentGZ input and output for physical files. Where appropriate, Romba depot files will be created as well.");
			}
		}
		private static Feature romsFlag
		{
			get
			{
				return new Feature(
					"roms",
					new List<string>() { "-r", "--roms" },
					"Output roms to miss instead of sets",
					FeatureType.Flag,
					longDescription: "By default, the outputted file will include the name of the game so this flag allows for the name of the rom to be output instead. [Missfile only]");
			}
		}
		private static Feature runnableFlag
		{
			get
			{
				return new Feature(
					"runnable",
					new List<string>() { "-run", "--runnable" },
					"Include only items that are marked runnable",
					FeatureType.Flag,
					longDescription: "This allows users to include only verified runnable games.");
			}
		}
		private static Feature scanAllFlag
		{
			get
			{
				return new Feature(
					"scan-all",
					new List<string>() { "-sa", "--scan-all" },
					"Set scanning levels for all archives to 0",
					FeatureType.Flag,
					longDescription: "This flag is the short equivalent to -7z=0 -gz=0 -rar=0 -zip=0 wrapped up. Generally this will be helpful in all cases where the content of the rebuild folder is not entirely known or is known to be mixed.");
			}
		}
		private static Feature sceneDateStripFlag
		{
			get
			{
				return new Feature(
					"scene-date-strip",
					new List<string>() { "-sds", "--scene-date-strip" },
					"Remove date from scene-named sets",
					FeatureType.Flag,
					longDescription: "If this flag is enabled, sets with \"scene\" names will have the date removed from the beginning. For example \"01.01.01-Game_Name-GROUP\" would become \"Game_Name-Group\".");
			}
		}
		private static Feature shortFlag
		{
			get
			{
				return new Feature(
					"short",
					new List<string>() { "-s", "--short" },
					"Use short output names",
					FeatureType.Flag,
					longDescription: "Instead of using ClrMamePro-style long names for DATs, use just the name of the folder as the name of the DAT. This can be used in conjunction with --base to output in the format of \"Original Name (Name)\" instead.");
			}
		}
		private static Feature singleSetFlag
		{
			get
			{
				return new Feature(
					"single-set",
					new List<string>() { "-si", "--single-set" },
					"All game names replaced by '!'",
					FeatureType.Flag,
					longDescription: "This is useful for keeping all roms in a DAT in the same archive or folder.");
			}
		}
		private static Feature sizeFlag
		{
			get
			{
				return new Feature(
					"size",
					new List<string>() { "-szs", "--size" },
					"Split DAT(s) or folder by file sizes",
					FeatureType.Flag,
					longDescription: "For a DAT, or set of DATs, allow for splitting based on the sizes of the files, specifically if the type is a Rom (most item types don't have sizes).");
			}
		}
		private static Feature skipFirstOutputFlag
		{
			get
			{
				return new Feature(
					"skip-first-output",
					new List<string>() { "-sf", "--skip-first-output" },
					"Skip output of first DAT",
					FeatureType.Flag,
					longDescription: "In times where the first DAT does not need to be written out a second time, this will skip writing it. This can often speed up the output process.");
			}
		}
		private static Feature skipArchivesFlag
		{
			get
			{
				return new Feature(
					"skip-archives",
					new List<string>() { "-ska", "--skip-archives" },
					"Skip all archives",
					FeatureType.Flag,
					longDescription: "Skip any files that are treated like archives");
			}
		}
		private static Feature skipFilesFlag
		{
			get
			{
				return new Feature(
					"skip-files",
					new List<string>() { "-skf", "--skip-files" },
					"Skip all non-archives",
					FeatureType.Flag,
					longDescription: "Skip any files that are not treated like archives");
			}
		}
		private static Feature skipMd5Flag
		{
			get
			{
				return new Feature(
					"skip-md5",
					new List<string>() { "-nm", "--skip-md5" },
					"Don't include MD5 in output",
					FeatureType.Flag,
					longDescription: "This allows the user to skip calculating the MD5 for each of the files which will speed up the creation of the DAT.");
			}
		}
		private static Feature skipSha1Flag
		{
			get
			{
				return new Feature(
					"skip-sha1",
					new List<string>() { "-ns", "--skip-sha1" },
					"Don't include SHA-1 in output",
					FeatureType.Flag,
					longDescription: "This allows the user to skip calculating the SHA-1 for each of the files which will speed up the creation of the DAT.");
			}
		}
		private static Feature skipSha256Flag
		{
			get
			{
				return new Feature(
					"skip-sha256",
					new List<string>() { "-ns256", "--skip-sha256" },
					"Include SHA-256 in output", // TODO: Invert this later
					FeatureType.Flag,
					longDescription: "This allows the user to skip calculating the SHA-256 for each of the files which will speed up the creation of the DAT.");
			}
		}
		private static Feature skipSha384Flag
		{
			get
			{
				return new Feature(
					"skip-sha384",
					new List<string>() { "-ns384", "--skip-sha384" },
					"Include SHA-384 in output", // TODO: Invert this later
					FeatureType.Flag,
					longDescription: "This allows the user to skip calculating the SHA-384 for each of the files which will speed up the creation of the DAT.");
			}
		}
		private static Feature skipSha512Flag
		{
			get
			{
				return new Feature(
					"skip-sha512",
					new List<string>() { "-ns512", "--skip-sha512" },
					"Include SHA-512 in output", // TODO: Invert this later
					FeatureType.Flag,
					longDescription: "This allows the user to skip calculating the SHA-512 for each of the files which will speed up the creation of the DAT.");
			}
		}
		private static Feature superdatFlag
		{
			get
			{
				return new Feature(
					"superdat",
					new List<string>() { "-sd", "--superdat" },
					"Enable SuperDAT creation",
					FeatureType.Flag,
					longDescription: "Set the type flag to \"SuperDAT\" for the output DAT as well as preserving the directory structure of the inputted folder, if applicable.");
			}
		}
		private static Feature tarFlag
		{
			get
			{
				return new Feature(
					"tar",
					new List<string>() { "-tar", "--tar" },
					"Enable Tape ARchive output",
					FeatureType.Flag,
					longDescription: "Instead of outputting the files to folder, files will be rebuilt to Tape ARchive (TAR) files. This format is a standardized storage archive without any compression, usually used with other compression formats around it. It is widely used in backup applications and source code archives.");
			}
		}
		private static Feature textFlag
		{
			get
			{
				return new Feature(
					"text",
					new List<string>() { "-txt", "--text" },
					"Output in generic text format",
					FeatureType.Flag,
					longDescription: "Output all statistical information in generic text format. If no other format flags are enabled, this is the default output.");
			}
		}
		private static Feature torrent7zipFlag
		{
			get
			{
				return new Feature(
					"torrent-7zip",
					new List<string>() { "-t7z", "--torrent-7zip" },
					"Enable Torrent7Zip output",
					FeatureType.Flag,
					longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent7Zip (T7Z) files. This format is based on the LZMA container format 7Zip, but with custom header information. This is currently unused by any major application. Currently does not produce proper Torrent-compatible outputs.");
			}
		}
		private static Feature torrentGzipFlag
		{
			get
			{
				return new Feature(
					"torrent-gzip",
					new List<string>() { "-tgz", "--torrent-gzip" },
					"Enable Torrent GZip output",
					FeatureType.Flag,
					longDescription: "Instead of outputting the files to folder, files will be rebuilt to TorrentGZ (TGZ) files. This format is based on the GZip archive format, but with custom header information and a file name replaced by the SHA-1 of the file inside. This is primarily used by external tool Romba (https://github.com/uwedeportivo/romba), but may be used more widely in the future.");
			}
		}
		private static Feature torrentLrzipFlag
		{
			get
			{
				return new Feature(
					"torrent-lrzip",
					new List<string>() { "-tlrz", "--torrent-lrzip" },
					"Enable Torrent Long-Range Zip output [UNIMPLEMENTED]",
					FeatureType.Flag,
					longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent Long-Range Zip (TLRZ) files. This format is based on the LRZip file format as defined at https://github.com/ckolivas/lrzip but with custom header information. This is currently unused by any major application.");
			}
		}
		private static Feature torrentLz4Flag
		{
			get
			{
				return new Feature(
					"torrent-lz4",
					new List<string>() { "-tlz4", "--torrent-lz4" },
					"Enable Torrent LZ4 output [UNIMPLEMENTED]",
					FeatureType.Flag,
					longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent LZ4 (TLZ4) files. This format is based on the LZ4 file format as defined at https://github.com/lz4/lz4 but with custom header information. This is currently unused by any major application.");
			}
		}
		private static Feature torrentRarFlag
		{
			get
			{
				return new Feature(
					"torrent-rar",
					new List<string>() { "-trar", "--torrent-rar" },
					"Enable Torrent RAR output [UNIMPLEMENTED]",
					FeatureType.Flag,
					longDescription: "Instead of outputting files to folder, files will be rebuilt to Torrent RAR (TRAR) files. This format is based on the RAR propietary format but with custom header information. This is currently unused by any major application.");
			}
		}
		private static Feature torrentXzFlag
		{
			get
			{
				return new Feature(
					"torrent-xz",
					new List<string>() { "-txz", "--torrent-xz" },
					"Enable Torrent XZ output [UNSUPPORTED]",
					FeatureType.Flag,
					longDescription: "Instead of outputting files to folder, files will be rebuilt to Torrent XZ (TXZ) files. This format is based on the LZMA container format XZ, but with custom header information. This is currently unused by any major application. Currently does not produce proper Torrent-compatible outputs.");
			}
		}
		private static Feature torrentZipFlag
		{
			get
			{
				return new Feature(
					"torrent-zip",
					new List<string>() { "-tzip", "--torrent-zip" },
					"Enable Torrent Zip output",
					FeatureType.Flag,
					longDescription: "Instead of outputting files to folder, files will be rebuilt to TorrentZip (TZip) files. This format is based on the ZIP archive format, but with custom header information. This is primarily used by external tool RomVault (http://www.romvault.com/) and is already widely used.");
			}
		}
		private static Feature torrentZpaqFlag
		{
			get
			{
				return new Feature(
					"torrent-zpaq",
					new List<string>() { "-tzpaq", "--torrent-zpaq" },
					"Enable Torrent ZPAQ output [UNIMPLEMENTED]",
					FeatureType.Flag,
					longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent ZPAQ (TZPAQ) files. This format is based on the ZPAQ file format as defined at https://github.com/zpaq/zpaq but with custom header information. This is currently unused by any major application.");
			}
		}
		private static Feature torrentZstdFlag
		{
			get
			{
				return new Feature(
					"torrent-zstd",
					new List<string>() { "-tzstd", "--torrent-zstd" },
					"Enable Torrent Zstd output [UNIMPLEMENTED]",
					FeatureType.Flag,
					longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent Zstd (TZstd) files. This format is based on the Zstd file format as defined at https://github.com/skbkontur/ZstdNet but with custom header information. This is currently unused by any major application.");
			}
		}
		private static Feature trimFlag
		{
			get
			{
				return new Feature(
					"trim",
					new List<string>() { "-trim", "--trim" },
					"Trim file names to fit NTFS length",
					FeatureType.Flag,
					longDescription: "In the cases where files will have too long a name, this allows for trimming the name of the files to the NTFS maximum length at most.");
			}
		}
		private static Feature tsvFlag
		{
			get
			{
				return new Feature(
					"tsv",
					new List<string>() { "-tsv", "--tsv" },
					"Output in Tab-Separated Value format",
					FeatureType.Flag,
					longDescription: "Output all statistical information in standardized TSV format.");
			}
		}
		private static Feature typeFlag
		{
			get
			{
				return new Feature(
					"type",
					new List<string>() { "-ts", "--type" },
					"Split DAT(s) or folder by file types (rom/disk)",
					FeatureType.Flag,
					longDescription: "For a DAT, or set of DATs, allow for splitting based on the types of the files, specifically if the type is a rom or a disk.");
			}
		}
		private static Feature updateDatFlag
		{
			get
			{
				return new Feature(
					"update-dat",
					new List<string>() { "-ud", "--update-dat" },
					"Output updated DAT to output directory",
					FeatureType.Flag,
					longDescription: "Once the files that were able to rebuilt are taken care of, a DAT of the files that could not be matched will be output to the output directory.");
			}
		}
		private static Feature updateDescriptionFlag
		{
			get
			{
				return new Feature(
					"update-description",
					new List<string>() { "-udd", "--update-description" },
					"Update machine descriptions from base DATs",
					FeatureType.Flag,
					longDescription: "This flag enables updating of machine descriptions from base DATs.");
			}
		}
		private static Feature updateHashesFlag
		{
			get
			{
				return new Feature(
					"update-hashes",
					new List<string>() { "-uh", "--update-hashes" },
					"Update hashes from base DATs",
					FeatureType.Flag,
					longDescription: "This flag enables updating of hashes from base DATs.");
			}
		}
		private static Feature updateManufacturerFlag
		{
			get
			{
				return new Feature(
					"update-manufacturer",
					new List<string>() { "-um", "--update-manufacturer" },
					"Update machine manufacturers from base DATs",
					FeatureType.Flag,
					longDescription: "This flag enables updating of machine manufacturers from base DATs.");
			}
		}
		private static Feature updateNamesFlag
		{
			get
			{
				return new Feature(
					"update-names",
					new List<string>() { "-un", "--update-names" },
					"Update item names from base DATs",
					FeatureType.Flag,
					longDescription: "This flag enables updating of item names from base DATs.");
			}
		}
		private static Feature updateYearFlag
		{
			get
			{
				return new Feature(
					"update-year",
					new List<string>() { "-uy", "--update-year" },
					"Update machine years from base DATs",
					FeatureType.Flag,
					longDescription: "This flag enables updating of machine years from base DATs.");
			}
		}

		#endregion

		#region Private Int32 features

		private static Feature gzInt32Input
		{
			get
			{
				return new Feature(
					"gz",
					new List<string>() { "-gz", "--gz" },
					"Set scanning level for GZip archives (default 1)",
					FeatureType.Int32,
					longDescription: "Scan GZip archives in one of the following ways: 0 - Hash both archive and its contents; 1 - Only hash contents of the archive; 2 - Only hash archive itself (treat like a regular file).");
			}
		}
		private static Feature rarInt32Input
		{
			get
			{
				return new Feature(
					"rar",
					new List<string>() { "-rar", "--rar" },
					"Set scanning level for RAR archives (default 1)",
					FeatureType.Int32,
					longDescription: "Scan RAR archives in one of the following ways: 0 - Hash both archive and its contents; 1 - Only hash contents of the archive; 2 - Only hash archive itself (treat like a regular file).");
			}
		}
		private static Feature sevenZipInt32Input
		{
			get
			{
				return new Feature(
					"7z",
					new List<string>() { "-7z", "--7z" },
					"Set scanning level for 7zip archives (default 1)",
					FeatureType.Int32,
					longDescription: "Scan 7Zip archives in one of the following ways: 0 - Hash both archive and its contents; 1 - Only hash contents of the archive; 2 - Only hash archive itself (treat like a regular file).");
			}
		}
		private static Feature threadsInt32Input
		{
			get
			{
				return new Feature(
					"threads",
					new List<string>() { "-mt", "--threads" },
					"Amount of threads to use (default = # cores, -1 unlimted)",
					FeatureType.Int32,
					longDescription: "Optionally, set the number of threads to use for the multithreaded operations. The default is the number of available machine threads; -1 means unlimited threads created.");
			}
		}
		private static Feature zipInt32Input
		{
			get
			{
				return new Feature(
					"zip",
					new List<string>() { "-zip", "--zip" },
					"Set scanning level for Zip archives (default 1)",
					FeatureType.Int32,
					longDescription: "Scan Zip archives in one of the following ways: 0 - Hash both archive and its contents; 1 - Only hash contents of the archive; 2 - Only hash archive itself (treat like a regular file).");
			}
		}

		#endregion

		#region Private Int64 features

		private static Feature radixInt64Input
		{
			get
			{
				return new Feature(
					"radix",
					new List<string>() { "-rad", "--radix" },
					"Set the midpoint to split at",
					FeatureType.Int64,
					longDescription: "Set the size at which all roms less than the size are put in the first DAT, and everything greater than or equal goes in the second.");
			}
		}

		#endregion

		#region Private List<string> features

		private static Feature baseDatListInput
		{
			get
			{
				return new Feature(
					"base-dat",
					new List<string>() { "-bd", "--base-dat" },
					"Add a base DAT for processing",
					FeatureType.List,
					longDescription: "Add a DAT or folder of DATs to the base set to be used for all operations. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature crcListInput
		{
			get
			{
				return new Feature(
					"crc",
					new List<string>() { "-crc", "--crc" },
					"Filter by CRC hash",
					FeatureType.List,
					longDescription: "Include only items with this CRC hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature datListInput
		{
			get
			{
				return new Feature(
					"dat",
					new List<string>() { "-dat", "--dat" },
					"Input DAT to be used",
					FeatureType.List,
					longDescription: "User-supplied DAT for use in all operations. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature extaListInput
		{
			get
			{
				return new Feature(
					"exta",
					new List<string>() { "-exta", "--exta" },
					"Set extension to be included in first DAT",
					FeatureType.List,
					longDescription: "Set the extension to be used to populate the first DAT. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature extbListInput
		{
			get
			{
				return new Feature(
					"extb",
					new List<string>() { "-extb", "--extb" },
					"Set extension to be included in second DAT",
					FeatureType.List,
					longDescription: "Set the extension to be used to populate the second DAT. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature gameDescriptionListInput
		{
			get
			{
				return new Feature(
					"game-description",
					new List<string>() { "-gd", "--game-description" },
					"Filter by game description",
					FeatureType.List,
					longDescription: "Include only items with this game description in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature gameNameListInput
		{
			get
			{
				return new Feature(
					"game-name",
					new List<string>() { "-gn", "--game-name" },
					"Filter by game name",
					FeatureType.List,
					longDescription: "Include only items with this game name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature gameTypeListInput
		{
			get
			{
				return new Feature(
					"game-type",
					new List<string>() { "-gt", "--game-type" },
					"Include only games with a given type [None, Bios, Device, Mechanical]",
					FeatureType.List,
					longDescription: "Include only items with this game type in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature itemNameListInput
		{
			get
			{
				return new Feature(
					"item-name",
					new List<string>() { "-rn", "--item-name" },
					"Filter by item name",
					FeatureType.List,
					longDescription: "Include only items with this item name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature itemTypeListInput
		{
			get
			{
				return new Feature(
					"item-type",
					new List<string>() { "-rt", "--item-type" },
					"Filter by item type",
					FeatureType.List,
					longDescription: "Include only items with this item type in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature md5ListInput
		{
			get
			{
				return new Feature(
					"md5",
					new List<string>() { "-md5", "--md5" },
					"Filter by MD5 hash",
					FeatureType.List,
					longDescription: "Include only items with this MD5 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notCrcListInput
		{
			get
			{
				return new Feature(
					"not-crc",
					new List<string>() { "-ncrc", "--not-crc" },
					"Filter by not CRC hash",
					FeatureType.List,
					longDescription: "Include only items without this CRC hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notGameDescriptionListInput
		{
			get
			{
				return new Feature(
					"not-game-description",
					new List<string>() { "-ngd", "--not-game-description" },
					"Filter by not game description",
					FeatureType.List,
					longDescription: "Include only items without this game description in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notGameNameListInput
		{
			get
			{
				return new Feature(
					"not-game-name",
					new List<string>() { "-ngn", "--not-game-name" },
					"Filter by not game name",
					FeatureType.List,
					longDescription: "Include only items without this game name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notGameTypeListInput
		{
			get
			{
				return new Feature(
					"not-game-type",
					new List<string>() { "-ngt", "--not-game-type" },
					"Exclude only games with a given type [None, Bios, Device, Mechanical]",
					FeatureType.List,
					longDescription: "Include only items without this game type in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notItemNameListInput
		{
			get
			{
				return new Feature(
					"not-item-name",
					new List<string>() { "-nrn", "--not-item-name" },
					"Filter by not item name",
					FeatureType.List,
					longDescription: "Include only items without this item name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notItemTypeListInput
		{
			get
			{
				return new Feature(
					"not-item-type",
					new List<string>() { "-nrt", "--not-item-type" },
					"Filter by not item type",
					FeatureType.List,
					longDescription: "Include only items without this item type in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notMd5ListInput
		{
			get
			{
				return new Feature(
					"not-md5",
					new List<string>() { "-nmd5", "--not-md5" },
					"Filter by not MD5 hash",
					FeatureType.List,
					longDescription: "Include only items without this MD5 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notSha1ListInput
		{
			get
			{
				return new Feature(
					"not-sha1",
					new List<string>() { "-nsha1", "--not-sha1" },
					"Filter by not SHA-1 hash",
					FeatureType.List,
					longDescription: "Include only items without this SHA-1 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notSha256ListInput
		{
			get
			{
				return new Feature(
					"not-sha256",
					new List<string>() { "-nsha256", "--not-sha256" },
					"Filter by not SHA-256 hash",
					FeatureType.List,
					longDescription: "Include only items without this SHA-256 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notSha384ListInput
		{
			get
			{
				return new Feature(
					"not-sha384",
					new List<string>() { "-nsha384", "--not-sha384" },
					"Filter by not SHA-384 hash",
					FeatureType.List,
					longDescription: "Include only items without this SHA-384 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notSha512ListInput
		{
			get
			{
				return new Feature(
					"not-sha512",
					new List<string>() { "-nsha512", "--not-sha512" },
					"Filter by not SHA-512 hash",
					FeatureType.List,
					longDescription: "Include only items without this SHA-512 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature notStatusListInput
		{
			get
			{
				return new Feature(
					"not-status",
					new List<string>() { "-nis", "--not-status" },
					"Exclude only items with a given status [None, Good, BadDump, Nodump, Verified]",
					FeatureType.List,
					longDescription: "Include only items without this item status in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature outputTypeListInput
		{
			get
			{
				return new Feature(
					"output-type",
					new List<string>() { "-ot", "--output-type" },
					"Output DATs to a given format [all, am/attractmode, cmp/clrmamepro, csv, dc/doscenter, lr/listrom, lx/listxml,  miss/missfile, md5, ol/offlinelist, rc/romcenter, sd/sabredat, sfv, sha1, sha256, sha384, sha512, sl/softwarelist, ssv, tsv, xml/logiqx",
					FeatureType.List,
					longDescription: @"Add outputting the created DAT to one of the following formats: all - All available DAT types; am, attractmode - AttractMode XML; cmp, clrmamepro - ClrMamePro; csv - Standardized Comma-Separated Value; dc, doscenter - DOSCenter; lr, listrom - MAME Listrom; lx, listxml - MAME Listxml; miss, missfile - GoodTools Missfile; md5 - MD5; ol, offlinelist - OfflineList XML; rc, romcenter - RomCenter; sd, sabredat - SabreDat XML; sfv - SFV; sha1 - SHA-1; sha256 - SHA-256; sha384 - SHA-384; sha512 - SHA-512; sl, softwarelist - MAME Software List XML; ssv - Standardized Semicolon - Separated Value; tsv - Standardized Tab - Separated Value; xml, logiqx - Logiqx XML. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature sha1ListInput
		{
			get
			{
				return new Feature(
					"sha1",
					new List<string>() { "-sha1", "--sha1" },
					"Filter by SHA-1 hash",
					FeatureType.List,
					longDescription: "Include only items with this SHA-1 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature sha256ListInput
		{
			get
			{
				return new Feature(
					"sha256",
					new List<string>() { "-sha256", "--sha256" },
					"Filter by SHA-256 hash",
					FeatureType.List,
					longDescription: "Include only items with this SHA-256 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature sha384ListInput
		{
			get
			{
				return new Feature(
					"sha384",
					new List<string>() { "-sha384", "--sha384" },
					"Filter by SHA-384 hash",
					FeatureType.List,
					longDescription: "Include only items with this SHA-384 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature sha512ListInput
		{
			get
			{
				return new Feature(
					"sha512",
					new List<string>() { "-sha512", "--sha512" },
					"Filter by SHA-512 hash",
					FeatureType.List,
					longDescription: "Include only items with this SHA-512 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}
		private static Feature statusListInput
		{
			get
			{
				return new Feature(
					"status",
					new List<string>() { "-is", "--status" },
					"Include only items with a given status [None, Good, BadDump, Nodump, Verified]",
					FeatureType.List,
					longDescription: "Include only items with this item status in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
			}
		}

		#endregion

		#region Private String features

		private static Feature addExtensionStringInput
		{
			get
			{
				return new Feature(
					"add-extension",
					new List<string>() { "-ae", "--add-extension" },
					"Add an extension to each item",
					FeatureType.String,
					longDescription: "Add a postfix extension to each full item name. [Missfile only]");
			}
		}
		private static Feature authorStringInput
		{
			get
			{
				return new Feature(
					"author",
					new List<string>() { "-au", "--author" },
					"Set the author of the DAT",
					FeatureType.String,
					longDescription: "Set the author header field for the output DAT(s)");
			}
		}
		private static Feature categoryStringInput
		{
			get
			{
				return new Feature(
					"category",
					new List<string>() { "-c", "--category" },
					"Set the category of the DAT",
					FeatureType.String,
					longDescription: "Set the category header field for the output DAT(s)");
			}
		}
		private static Feature commentStringInput
		{
			get
			{
				return new Feature(
					"comment",
					new List<string>() { "-co", "--comment" },
					"Set a new comment of the DAT",
					FeatureType.String,
					longDescription: "Set the comment header field for the output DAT(s)");
			}
		}
		private static Feature dateStringInput
		{
			get
			{
				return new Feature(
					"date",
					new List<string>() { "-da", "--date" },
					"Set a new date",
					FeatureType.String,
					longDescription: "Set the date header field for the output DAT(s)");
			}
		}
		private static Feature descriptionStringInput
		{
			get
			{
				return new Feature(
					"description",
					new List<string>() { "-de", "--description" },
					"Set the description of the DAT",
					FeatureType.String,
					longDescription: "Set the description header field for the output DAT(s)");
			}
		}
		private static Feature emailStringInput
		{
			get
			{
				return new Feature(
					"email",
					new List<string>() { "-em", "--email" },
					"Set a new email of the DAT",
					FeatureType.String,
					longDescription: "Set the email header field for the output DAT(s)");
			}
		}
		private static Feature equalStringInput
		{
			get
			{
				return new Feature(
					"equal",
					new List<string>() { "-seq", "--equal" },
					"Filter by size ==",
					FeatureType.String,
					longDescription: "Only include items of this exact size in the output DAT. Users can specify either a regular integer number or a number with a standard postfix. e.g. 8kb => 8000 or 8kib => 8192");
			}
		}
		private static Feature filenameStringInput
		{
			get
			{
				return new Feature(
					"filename",
					new List<string>() { "-f", "--filename" },
					"Set the external name of the DAT",
					FeatureType.String,
					longDescription: "Set the external filename for the output DAT(s)");
			}
		}
		private static Feature forcemergingStringInput
		{
			get
			{
				return new Feature(
					"forcemerging",
					new List<string>() { "-fm", "--forcemerging" },
					"Set force merging [None, Split, Merged, Nonmerged, Full]",
					FeatureType.String,
					longDescription: "Set the forcemerging tag to the given value.");
			}
		}
		private static Feature forcenodumpStringInput
		{
			get
			{
				return new Feature(
					"forcenodump",
					new List<string>() { "-fn", "--forcenodump" },
					"Set force nodump [None, Obsolete, Required, Ignore]",
					FeatureType.String,
					longDescription: "Set the forcenodump tag to the given value.");
			}
		}
		private static Feature forcepackingStringInput
		{
			get
			{
				return new Feature(
					"forcepacking",
					new List<string>() { "-fp", "--forcepacking" },
					"Set force packing [None, Zip, Unzip]",
					FeatureType.String,
					longDescription: "Set the forcepacking tag to the given value.");
			}
		}
		private static Feature greaterStringInput
		{
			get
			{
				return new Feature(
					"greater",
					new List<string>() { "-sgt", "--greater" },
					"Filter by size >=",
					FeatureType.String,
					longDescription: "Only include items whose size is greater than or equal to this value in the output DAT. Users can specify either a regular integer number or a number with a standard postfix. e.g. 8kb => 8000 or 8kib => 8192");
			}
		}
		private static Feature headerStringInput
		{
			get
			{
				return new Feature(
					"header",
					new List<string>() { "-h", "--header" },
					"Set a header skipper to use, blank means all",
					FeatureType.String,
					longDescription: "Set the header special field for the output DAT(s)");

			}
		}
		private static Feature homepageStringInput
		{
			get
			{
				return new Feature(
					"homepage",
					new List<string>() { "-hp", "--homepage" },
					"Set a new homepage of the DAT",
					FeatureType.String,
					longDescription: "Set the homepage header field for the output DAT(s)");
			}
		}
		private static Feature lessStringInput
		{
			get
			{
				return new Feature(
					"less",
					new List<string>() { "-slt", "--less" },
					"Filter by size =<",
					FeatureType.String,
					longDescription: "Only include items whose size is less than or equal to this value in the output DAT. Users can specify either a regular integer number or a number with a standard postfix. e.g. 8kb => 8000 or 8kib => 8192");
			}
		}
		private static Feature nameStringInput
		{
			get
			{
				return new Feature(
					"name",
					new List<string>() { "-n", "--name" },
					"Set the internal name of the DAT",
					FeatureType.String,
					longDescription: "Set the name header field for the output DAT(s)");
			}
		}
		private static Feature outputDirStringInput
		{
			get
			{
				return new Feature(
					"output-dir",
					new List<string>() { "-out", "--output-dir" },
					"Output directory",
					FeatureType.String,
					longDescription: "This sets an output folder to be used when the files are created. If a path is not defined, the runtime directory is used instead.");
			}
		}
		private static Feature postfixStringInput
		{
			get
			{
				return new Feature(
					"postfix",
					new List<string>() { "-post", "--postfix" },
					"Set postfix for all lines",
					FeatureType.String,
					longDescription: "Set a generic postfix to be appended to all outputted lines. [*SV, Missfile only]");
			}
		}
		private static Feature prefixStringInput
		{
			get
			{
				return new Feature(
					"prefix",
					new List<string>() { "-pre", "--prefix" },
					"Set prefix for all lines",
					FeatureType.String,
					longDescription: "Set a generic prefix to be prepended to all outputted lines. [*SV, Missfile only]");
			}
		}
		private static Feature replaceExtensionStringInput
		{
			get
			{
				return new Feature(
					"replace-extension",
					new List<string>() { "-rep", "--replace-extension" },
					"Replace all extensions with specified",
					FeatureType.String,
					longDescription: "When an extension exists, replace it with the provided instead. [Missfile only]");
			}
		}
		private static Feature rootStringInput
		{
			get
			{
				return new Feature(
					"root",
					new List<string>() { "-r", "--root" },
					"Set a new rootdir",
					FeatureType.String,
					longDescription: "Set the rootdir (as used by SuperDAT mode) for the output DAT(s).");
			}
		}
		private static Feature rootDirStringInput
		{
			get
			{
				return new Feature(
					"root-dir",
					new List<string>() { "-rd", "--root-dir" },
					"Set the root directory for calc",
					FeatureType.String,
					longDescription: "In the case that the files will not be stored from the root directory, a new root can be set for path length calculations.");
			}
		}
		private static Feature tempStringInput
		{
			get
			{
				return new Feature(
					"temp",
					new List<string>() { "-t", "--temp" },
					"Set the temporary directory to use",
					FeatureType.String,
					longDescription: "Optionally, a temp folder can be supplied in the case the default temp directory is not preferred.");
			}
		}
		private static Feature urlStringInput
		{
			get
			{
				return new Feature(
					"url",
					new List<string>() { "-u", "--url" },
					"Set a new URL of the DAT",
					FeatureType.String,
					longDescription: "Set the URL header field for the output DAT(s)");
			}
		}
		private static Feature versionStringInput
		{
			get
			{
				return new Feature(
					"version",
					new List<string>() { "-v", "--version" },
					"Set the version of the DAT",
					FeatureType.String,
					longDescription: "Set the version header field for the output DAT(s)");
			}
		}

		#endregion

		public static Help RetrieveHelp()
		{
			// Create and add the header to the Help object
			string barrier = "-----------------------------------------";
			List<string> helpHeader = new List<string>()
			{
				"SabreTools - Manipulate, convert, and use DAT files",
				barrier,
				"Usage: SabreTools [option] [flags] [filename|dirname] ...",
				""
			};
			Help help = new Help(helpHeader);

			#region Help

			Feature helpFeature = new Feature(
				"Help",
				new List<string>() { "-?", "-h", "--help" },
				"Show this help",
				FeatureType.Flag,
				longDescription: "Built-in to most of the programs is a basic help text.");

			#endregion

			#region Script

			Feature script = new Feature(
				"Script",
				"--script",
				"Enable script mode (no clear screen)",
				FeatureType.Flag,
				longDescription: "For times when SabreTools is being used in a scripted environment, the user may not want the screen to be cleared every time that it is called. This flag allows the user to skip clearing the screen on run just like if the console was being redirected.");

			#endregion

			#region DATFromDir

			Feature datFromDir = new Feature(
				"DATFromDir",
				new List<string>() { "-d", "--d2d", "--dfd" },
				"Create DAT(s) from an input directory",
				FeatureType.Flag,
				longDescription: "Create a DAT file from an input directory or set of files. By default, this will output a DAT named based on the input directory and the current date. It will also treat all archives as possible games and add all three hashes (CRC, MD5, SHA-1) for each file.");
			datFromDir.AddFeature(skipMd5Flag);
			datFromDir.AddFeature(skipSha1Flag);
			datFromDir.AddFeature(skipSha256Flag);
			datFromDir.AddFeature(skipSha384Flag);
			datFromDir.AddFeature(skipSha512Flag);
			datFromDir.AddFeature(noAutomaticDateFlag);
			datFromDir.AddFeature(forcepackingStringInput);
			datFromDir.AddFeature(archivesAsFilesFlag);
			// NEW
			datFromDir.AddFeature(outputTypeListInput);
				datFromDir[outputTypeListInput.Name].AddFeature(depreciatedFlag);
			// OLD
			datFromDir.AddFeature(outputAllFlag);
			datFromDir.AddFeature(outputAttractmodeFlag);
			datFromDir.AddFeature(outputCmpFlag);
			datFromDir.AddFeature(outputCsvFlag);
			datFromDir.AddFeature(outputDoscenterFlag);
			datFromDir.AddFeature(outputListromFlag);
			datFromDir.AddFeature(outputListxmlFlag);
			datFromDir.AddFeature(outputMissFlag);
			datFromDir.AddFeature(outputMd5Flag);
			datFromDir.AddFeature(outputOfflinelistFlag);
			datFromDir.AddFeature(outputRomcenterFlag);
			datFromDir.AddFeature(outputSabredatFlag);
			datFromDir.AddFeature(outputSfvFlag);
			datFromDir.AddFeature(outputSha1Flag);
			datFromDir.AddFeature(outputSha256Flag);
			datFromDir.AddFeature(outputSha384Flag);
			datFromDir.AddFeature(outputSha512Flag);
			datFromDir.AddFeature(outputSoftwarelistFlag);
			datFromDir.AddFeature(outputSsvFlag);
			datFromDir.AddFeature(outputTsvFlag);
			datFromDir.AddFeature(outputXmlFlag);
				datFromDir[outputXmlFlag].AddFeature(depreciatedFlag);
			datFromDir.AddFeature(rombaFlag);
			datFromDir.AddFeature(skipArchivesFlag);
			datFromDir.AddFeature(skipFilesFlag);
			datFromDir.AddFeature(filenameStringInput);
			datFromDir.AddFeature(nameStringInput);
			datFromDir.AddFeature(descriptionStringInput);
			datFromDir.AddFeature(categoryStringInput);
			datFromDir.AddFeature(versionStringInput);
			datFromDir.AddFeature(authorStringInput);
			datFromDir.AddFeature(emailStringInput);
			datFromDir.AddFeature(homepageStringInput);
			datFromDir.AddFeature(urlStringInput);
			datFromDir.AddFeature(commentStringInput);
			datFromDir.AddFeature(superdatFlag);
			datFromDir.AddFeature(excludeOfFlag);
			datFromDir.AddFeature(sceneDateStripFlag);
			datFromDir.AddFeature(addBlankFilesFlag);
			datFromDir.AddFeature(addDateFlag);
			datFromDir.AddFeature(copyFilesFlag);
			datFromDir.AddFeature(headerStringInput);
			datFromDir.AddFeature(chdsAsFilesFlag);
			datFromDir.AddFeature(tempStringInput);
			datFromDir.AddFeature(outputDirStringInput);
			datFromDir.AddFeature(threadsInt32Input);

			#endregion

			#region Extract

			Feature extract = new Feature(
				"Extract",
				new List<string>() { "-ex", "--extract" },
				"Extract and remove copier headers",
				FeatureType.Flag,
				longDescription: "This will detect, store, and remove copier headers from a file or folder of files. The headers are backed up and collated by the hash of the unheadered file. Files are then output without the detected copier header alongside the originals with the suffix .new. No input files are altered in the process. The following systems have headers that this program can work with: Atari 7800; Atari Lynx; Commodore PSID Music; NEC PC - Engine / TurboGrafx 16; Nintendo Famicom / Nintendo Entertainment System; Nintendo Famicom Disk System; Nintendo Super Famicom / Super Nintendo Entertainment System; Nintendo Super Famicom / Super Nintendo Entertainment System SPC.");
			extract.AddFeature(outputDirStringInput);
			extract.AddFeature(noStoreHeaderFlag);

			#endregion

			#region Restore

			Feature restore = new Feature(
				"Restore",
				new List<string>() { "-re", "--restore" },
				"Restore header to file based on SHA-1",
				FeatureType.Flag,
				longDescription: "This will make use of stored copier headers and reapply them to files if they match the included hash. More than one header can be applied to a file, so they will be output to new files, suffixed with .newX, where X is a number. No input files are altered in the process. The following systems have headers that this program can work with: Atari 7800; Atari Lynx; Commodore PSID Music; NEC PC - Engine / TurboGrafx 16; Nintendo Famicom / Nintendo Entertainment System; Nintendo Famicom Disk System; Nintendo Super Famicom / Super Nintendo Entertainment System; Nintendo Super Famicom / Super Nintendo Entertainment System SPC.");
			restore.AddFeature(outputDirStringInput);

			#endregion

			#region Sort

			Feature sort = new Feature(
				"Sort",
				new List<string>() { "-ss", "--sort" },
				"Sort inputs by a set of DATs",
				FeatureType.Flag,
				longDescription: "This feature allows the user to quickly rebuild based on a supplied DAT file(s). By default all files will be rebuilt to uncompressed folders in the output directory.");
			sort.AddFeature(datListInput);
			sort.AddFeature(outputDirStringInput);
			sort.AddFeature(depotFlag);
			sort.AddFeature(deleteFlag);
			sort.AddFeature(inverseFlag);
			sort.AddFeature(quickFlag);
			sort.AddFeature(chdsAsFilesFlag);
			sort.AddFeature(addDateFlag);
			sort.AddFeature(torrent7zipFlag);
			sort.AddFeature(tarFlag);
			sort.AddFeature(torrentGzipFlag);
				sort[torrentGzipFlag].AddFeature(rombaFlag);
			//sort.AddFeature(tlrzFeature);
			//sort.AddFeature(tlz4Feature);
			//sort.AddFeature(trarFeature);
			//sort.AddFeature(txzFeature);
			sort.AddFeature(torrentZipFlag);
			//sort.AddFeature(tzpaqFeature);
			//sort.AddFeature(tzstdFeature);
			sort.AddFeature(headerStringInput);
			sort.AddFeature(sevenZipInt32Input);
			sort.AddFeature(gzInt32Input);
			sort.AddFeature(rarInt32Input);
			sort.AddFeature(zipInt32Input);
			sort.AddFeature(scanAllFlag);
			sort.AddFeature(datMergedFlag);
			sort.AddFeature(datSplitFlag);
			sort.AddFeature(datNonMergedFlag);
			sort.AddFeature(datDeviceNonMergedFlag);
			sort.AddFeature(datFullNonMergedFlag);
			sort.AddFeature(updateDatFlag);
			sort.AddFeature(threadsInt32Input);

			#endregion

			#region Split

			Feature split = new Feature(
				"Split",
				new List<string>() { "-sp", "--split" },
				"Split input DATs by a given criteria",
				FeatureType.Flag,
				longDescription: "This feature allows the user to split input DATs by a number of different possible criteria. See the individual input information for details. More than one split type is allowed at a time.");
			// NEW
			split.AddFeature(outputTypeListInput);
				split[outputTypeListInput.Name].AddFeature(depreciatedFlag);
			// OLD
			split.AddFeature(outputAllFlag);
			split.AddFeature(outputAttractmodeFlag);
			split.AddFeature(outputCmpFlag);
			split.AddFeature(outputCsvFlag);
			split.AddFeature(outputDoscenterFlag);
			split.AddFeature(outputListromFlag);
			split.AddFeature(outputListxmlFlag);
			split.AddFeature(outputMissFlag);
			split.AddFeature(outputMd5Flag);
			split.AddFeature(outputOfflinelistFlag);
			split.AddFeature(outputRomcenterFlag);
			split.AddFeature(outputSabredatFlag);
			split.AddFeature(outputSfvFlag);
			split.AddFeature(outputSha1Flag);
			split.AddFeature(outputSha256Flag);
			split.AddFeature(outputSha384Flag);
			split.AddFeature(outputSha512Flag);
			split.AddFeature(outputSoftwarelistFlag);
			split.AddFeature(outputSsvFlag);
			split.AddFeature(outputTsvFlag);
			split.AddFeature(outputXmlFlag);
				split[outputXmlFlag].AddFeature(depreciatedFlag);
			split.AddFeature(outputDirStringInput);
			split.AddFeature(inplaceFlag);
			split.AddFeature(extensionFlag);
				split[extensionFlag].AddFeature(extaListInput);
				split[extensionFlag].AddFeature(extbListInput);
			split.AddFeature(hashFlag);
			split.AddFeature(levelFlag);
				split[levelFlag].AddFeature(shortFlag);
				split[levelFlag].AddFeature(baseFlag);
			split.AddFeature(sizeFlag);
				split[sizeFlag].AddFeature(radixInt64Input);
			split.AddFeature(typeFlag);

			#endregion

			#region Stats

			Feature stats = new Feature(
				"Stats",
				new List<string>() { "-st", "--stats" },
				"Get statistics on all input DATs",
				FeatureType.Flag,
				longDescription: "This will output by default the combined statistics for all input DAT files. The stats that are outputted are as follows: Total uncompressed size; Number of games found; Number of roms found; Number of disks found; Roms that include a CRC/MD5/SHA-1/SHA-256/SHA-384/SHA-512; Roms with Nodump status.");
			stats.AddFeature(allStatsFlag);
			stats.AddFeature(baddumpColumnFlag);
			stats.AddFeature(csvFlag);
			stats.AddFeature(filenameStringInput);
			stats.AddFeature(outputDirStringInput);
			stats.AddFeature(htmlFlag);
			stats.AddFeature(nodumpColumnFlag);
			stats.AddFeature(individualFlag);
			stats.AddFeature(tsvFlag);
			stats.AddFeature(textFlag);

			#endregion

			#region Update

			Feature update = new Feature(
				"Update",
				new List<string>() { "-ud", "--update" },
				"Update and manipulate DAT(s)",
				FeatureType.Flag,
				longDescription: "This is the multitool part of the program, allowing for almost every manipulation to a DAT, or set of DATs. This is also a combination of many different programs that performed DAT manipulation that work better together.");
			// NEW
			update.AddFeature(outputTypeListInput);
				update[outputTypeListInput].AddFeature(prefixStringInput);
				update[outputTypeListInput].AddFeature(postfixStringInput);
				update[outputTypeListInput].AddFeature(quotesFlag);
				update[outputTypeListInput].AddFeature(romsFlag);
				update[outputTypeListInput].AddFeature(gamePrefixFlag);
				update[outputTypeListInput].AddFeature(addExtensionStringInput);
				update[outputTypeListInput].AddFeature(replaceExtensionStringInput);
				update[outputTypeListInput].AddFeature(removeExtensionsFlag);
				update[outputTypeListInput].AddFeature(rombaFlag);
				update[outputTypeListInput].AddFeature(depreciatedFlag);
			// OLD
			update.AddFeature(outputAllFlag);
			update.AddFeature(outputAttractmodeFlag);
			update.AddFeature(outputCmpFlag);
			update.AddFeature(outputCsvFlag);
				update[outputCsvFlag].AddFeature(prefixStringInput);
				update[outputCsvFlag].AddFeature(postfixStringInput);
				update[outputCsvFlag].AddFeature(quotesFlag);
			update.AddFeature(outputDoscenterFlag);
			update.AddFeature(outputListromFlag);
			update.AddFeature(outputListxmlFlag);
			update.AddFeature(outputMissFlag);
				update[outputMissFlag].AddFeature(romsFlag);
				update[outputMissFlag].AddFeature(gamePrefixFlag);
				update[outputMissFlag].AddFeature(prefixStringInput);
				update[outputMissFlag].AddFeature(postfixStringInput);
				update[outputMissFlag].AddFeature(quotesFlag);
				update[outputMissFlag].AddFeature(addExtensionStringInput);
				update[outputMissFlag].AddFeature(replaceExtensionStringInput);
				update[outputMissFlag].AddFeature(removeExtensionsFlag);
				update[outputMissFlag].AddFeature(rombaFlag);
			update.AddFeature(outputMd5Flag);
				update[outputMd5Flag].AddFeature(gamePrefixFlag);
			update.AddFeature(outputOfflinelistFlag);
			update.AddFeature(outputRomcenterFlag);
			update.AddFeature(outputSabredatFlag);
			update.AddFeature(outputSfvFlag);
				update[outputSfvFlag].AddFeature(gamePrefixFlag);
			update.AddFeature(outputSha1Flag);
				update[outputSha1Flag].AddFeature(gamePrefixFlag);
			update.AddFeature(outputSha256Flag);
				update[outputSha256Flag].AddFeature(gamePrefixFlag);
			update.AddFeature(outputSha384Flag);
				update[outputSha384Flag].AddFeature(gamePrefixFlag);
			update.AddFeature(outputSha512Flag);
				update[outputSha512Flag].AddFeature(gamePrefixFlag);
			update.AddFeature(outputSoftwarelistFlag);
			update.AddFeature(outputSsvFlag);
				update[outputSsvFlag].AddFeature(prefixStringInput);
				update[outputSsvFlag].AddFeature(postfixStringInput);
				update[outputSsvFlag].AddFeature(quotesFlag);
			update.AddFeature(outputTsvFlag);
				update[outputTsvFlag].AddFeature(prefixStringInput);
				update[outputTsvFlag].AddFeature(postfixStringInput);
				update[outputTsvFlag].AddFeature(quotesFlag);
			update.AddFeature(outputXmlFlag);
				update[outputXmlFlag].AddFeature(depreciatedFlag);
			update.AddFeature(filenameStringInput);
			update.AddFeature(nameStringInput);
			update.AddFeature(descriptionStringInput);
			update.AddFeature(rootStringInput);
			update.AddFeature(categoryStringInput);
			update.AddFeature(versionStringInput);
			update.AddFeature(dateStringInput);
			update.AddFeature(authorStringInput);
			update.AddFeature(emailStringInput);
			update.AddFeature(homepageStringInput);
			update.AddFeature(urlStringInput);
			update.AddFeature(commentStringInput);
			update.AddFeature(headerStringInput);
			update.AddFeature(superdatFlag);
			update.AddFeature(forcemergingStringInput);
			update.AddFeature(forcenodumpStringInput);
			update.AddFeature(forcepackingStringInput);
			update.AddFeature(excludeOfFlag);
			update.AddFeature(sceneDateStripFlag);
			update.AddFeature(cleanFlag);
			update.AddFeature(removeUnicodeFlag);
			update.AddFeature(removeMd5Flag);
			update.AddFeature(removeSha1Flag);
			update.AddFeature(removeSha256Flag);
			update.AddFeature(removeSha384Flag);
			update.AddFeature(removeSha512Flag);
			update.AddFeature(descriptionAsNameFlag);
			update.AddFeature(datMergedFlag);
			update.AddFeature(datSplitFlag);
			update.AddFeature(datNonMergedFlag);
			update.AddFeature(datDeviceNonMergedFlag);
			update.AddFeature(datFullNonMergedFlag);
			update.AddFeature(trimFlag);
				update[trimFlag].AddFeature(rootDirStringInput);
			update.AddFeature(singleSetFlag);
			update.AddFeature(dedupFlag);
			update.AddFeature(gameDedupFlag);
			update.AddFeature(mergeFlag);
				update[mergeFlag].AddFeature(noAutomaticDateFlag);
			update.AddFeature(diffFlag);
				update[diffFlag].AddFeature(againstFlag);
					update[diffFlag][againstFlag].AddFeature(baseDatListInput);
				update[diffFlag].AddFeature(noAutomaticDateFlag);
				update[diffFlag].AddFeature(cascadeFlag);
					update[diffFlag][cascadeFlag].AddFeature(skipFirstOutputFlag);
				update[diffFlag].AddFeature(reverseCascadeFlag);
					update[diffFlag][reverseCascadeFlag].AddFeature(skipFirstOutputFlag);
			update.AddFeature(diffDuFlag);
				update[diffDuFlag].AddFeature(noAutomaticDateFlag);
			update.AddFeature(diffInFlag);
				update[diffInFlag].AddFeature(noAutomaticDateFlag);
			update.AddFeature(diffNdFlag);
				update[diffNdFlag].AddFeature(noAutomaticDateFlag);
			update.AddFeature(baseReplaceFlag);
				update[baseReplaceFlag].AddFeature(baseDatListInput);
				update[baseReplaceFlag].AddFeature(updateNamesFlag);
				update[baseReplaceFlag].AddFeature(updateHashesFlag);
				update[baseReplaceFlag].AddFeature(updateDescriptionFlag);
					update[baseReplaceFlag][updateDescriptionFlag].AddFeature(onlySameFlag);
				update[baseReplaceFlag].AddFeature(updateYearFlag);
				update[baseReplaceFlag].AddFeature(updateManufacturerFlag);
			update.AddFeature(reverseBaseReplaceFlag);
				update[reverseBaseReplaceFlag].AddFeature(baseDatListInput);
				update[reverseBaseReplaceFlag].AddFeature(updateNamesFlag);
				update[reverseBaseReplaceFlag].AddFeature(updateHashesFlag);
				update[reverseBaseReplaceFlag].AddFeature(updateDescriptionFlag);
					update[reverseBaseReplaceFlag][updateDescriptionFlag].AddFeature(onlySameFlag);
				update[reverseBaseReplaceFlag].AddFeature(updateYearFlag);
				update[reverseBaseReplaceFlag].AddFeature(updateManufacturerFlag);
			update.AddFeature(gameNameListInput);
			update.AddFeature(notGameNameListInput);
			update.AddFeature(gameDescriptionListInput);
			update.AddFeature(notGameDescriptionListInput);
			update.AddFeature(matchOfTagsFlag);
			update.AddFeature(itemNameListInput);
			update.AddFeature(notItemNameListInput);
			update.AddFeature(itemTypeListInput);
			update.AddFeature(notItemTypeListInput);
			update.AddFeature(greaterStringInput);
			update.AddFeature(lessStringInput);
			update.AddFeature(equalStringInput);
			update.AddFeature(crcListInput);
			update.AddFeature(notCrcListInput);
			update.AddFeature(md5ListInput);
			update.AddFeature(notMd5ListInput);
			update.AddFeature(sha1ListInput);
			update.AddFeature(notSha1ListInput);
			update.AddFeature(sha256ListInput);
			update.AddFeature(notSha256ListInput);
			update.AddFeature(sha384ListInput);
			update.AddFeature(notSha384ListInput);
			update.AddFeature(sha512ListInput);
			update.AddFeature(notSha512ListInput);
			update.AddFeature(statusListInput);
			update.AddFeature(notStatusListInput);
			update.AddFeature(gameTypeListInput);
			update.AddFeature(notGameTypeListInput);
			update.AddFeature(runnableFlag);
			update.AddFeature(notRunnableFlag);
			update.AddFeature(outputDirStringInput);
			update.AddFeature(inplaceFlag);
			update.AddFeature(threadsInt32Input);

			#endregion

			#region Verify

			Feature verify = new Feature(
				"Verify",
				new List<string>() { "-ve", "--verify" },
				"Verify a folder against DATs",
				FeatureType.Flag,
				longDescription: "When used, this will use an input DAT or set of DATs to blindly check against an input folder. The base of the folder is considered the base for the combined DATs and games are either the directories or archives within. This will only do a direct verification of the items within and will create a fixdat afterwards for missing files.");
			verify.AddFeature(datListInput);
			verify.AddFeature(depotFlag);
			verify.AddFeature(tempStringInput);
			verify.AddFeature(hashOnlyFlag);
			verify.AddFeature(quickFlag);
			verify.AddFeature(headerStringInput);
			verify.AddFeature(chdsAsFilesFlag);
			verify.AddFeature(datMergedFlag);
			verify.AddFeature(datSplitFlag);
			verify.AddFeature(datDeviceNonMergedFlag);
			verify.AddFeature(datNonMergedFlag);
			verify.AddFeature(datFullNonMergedFlag);

			#endregion

			// Now, add all of the main features to the Help object
			help.Add(helpFeature);
			help.Add(script);
			help.Add(datFromDir);
			help.Add(extract);
			help.Add(restore);
			help.Add(sort);
			help.Add(split);
			help.Add(stats);
			help.Add(update);
			help.Add(verify);

			return help;
		}
	}
}
