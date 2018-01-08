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
					new List<string>() { "-ab", "--add-blank-files" },
					"Output blank files for folders",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature addDateFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ad", "--add-date" },
					"Add dates to items, where posible",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature againstFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ag", "--against" },
					"Diff all inputs against a set of base DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature allStatsFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-as", "--all-stats" },
					"Write all statistics to all available formats",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature archivesAsFilesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-f", "--archives-as-files" },
					"Treat archives as files",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature baddumpColumnFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-bc", "--baddump-column" },
					"Add baddump stats to output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature baseFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ba", "--base" },
					"Use source DAT as base name for outputs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature baseReplaceFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-br", "--base-replace" },
					"Replace from base DATs in order",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature cascadeFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-c", "--cascade" },
					"Enable cascaded diffing",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature chdsAsFilesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ic", "--chds-as-files" },
					"Treat CHDs as regular files",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature cleanFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-clean", "--clean" },
					"Clean game names according to WoD standards",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature copyFilesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-cf", "--copy-files" },
					"Copy files to the temp directory before parsing",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature csvFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-csv", "--csv" },
					"Output in Comma-Separated Value format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature datDeviceNonMergedFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-dnd", "--dat-device-non-merged" },
					"Create device non-merged sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature datFullNonMergedFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-df", "--dat-full-non-merged" },
					"Force creating fully non-merged sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature datMergedFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-dm", "--dat-merged" },
					"Force creating merged sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature datNonMergedFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-dnm", "--dat-non-merged" },
					"Force creating non-merged sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature datSplitFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ds", "--dat-split" },
					"Force creating split sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature dedupFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-dd", "--dedup" },
					"Enable deduping in the created DAT",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature deleteFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-del", "--delete" },
					"Delete fully rebuilt input files",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature depotFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-dep", "--depot" },
					"Assume directories are romba depots",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature descriptionAsNameFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-dan", "--description-as-name" },
					"Use description instead of machine name",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature diffFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-di", "--diff" },
					"Create diffdats from inputs (all outputs)",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature diffDuFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-did", "--diff-du" },
					"Create diffdat containing just duplicates",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature diffInFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-dii", "--diff-in" },
					"Create diffdats for individual DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature diffNdFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-din", "--diff-nd" },
					"Create diffdat containing no duplicates",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature excludeOfFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-xof", "--exclude-of" },
					"Exclude romof, cloneof, sampleof tags",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature extensionFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-es", "--extension" },
					"Split DAT(s) by two file extensions",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature gameDedupFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-gdd", "--game-dedup" },
					"Enable in-game deduping in the created DAT",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature gamePrefixFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-gp", "--game-prefix" },
					"Add game name as a prefix",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature hashFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-hs", "--hash" },
					"Split DAT(s) or folder by best-available hashes",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature hashOnlyFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ho", "--hash-only" },
					"Check files by hash only",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature htmlFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-html", "--html" },
					"Output in HTML format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature individualFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ind", "--individual" },
					"Show individual statistics",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature inplaceFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ip", "--inplace" },
					"Write to the input directories, where possible",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature inverseFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-in", "--inverse" },
					"Rebuild only files not in DAT",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature levelFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ls", "--level" },
					"Split a SuperDAT or folder by internal path",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature mergeFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-m", "--merge" },
					"Merge the input DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noAutomaticDateFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-b", "--no-automatic-date" },
					"Don't include date in automatic name",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature nodumpColumnFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-nc", "--nodump-column" },
					"Add nodump stats to output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noStoreHeaderFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-nsh", "--no-store-header" },
					"Don't store the extracted header",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature notRunnableFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-nrun", "--not-runnable" },
					"Include only items that are marked unrunnable",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature matchOfTagsFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ofg", "--match-of-tags" },
					"Allow cloneof and romof tags to match game name filters",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature onlySameFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ons", "--only-same" },
					"Only update description if machine name matches description",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputAllFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-oa", "--output-all" },
					"Output in all formats",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputAttractmodeFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-oam", "--output-attractmode" },
					"Output in AttractMode format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputCmpFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-oc", "--output-cmp" },
					"Output in CMP format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputCsvFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ocsv", "--output-csv" },
					"Output in CSV format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputDoscenterFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-od", "--output-doscenter" },
					"Output in DOSCenter format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputListromFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-olr", "--output-listrom" },
					"Output in MAME Listrom format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputMd5Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-omd5", "--output-md5" },
					"Output in MD5 format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputMissFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-om", "--output-miss" },
					"Output in Missfile format",
					FeatureType.Flag,
					new List<string>()
					{
						"",
						"Prefix and postfix can include certain fields from the",
						"items by including %blah% in the input.",
						"A list of features that can be used are:",
						"  game, name, crc, md5, sha1, sha256, sha384, sha512, size",
					});
			}
		}
		private static Feature outputOfflinelistFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ool", "--output-offlinelist" },
					"Output in OfflineList format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputRomcenterFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-or", "--output-romcenter" },
					"Output in RomCenter format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSabredatFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-os", "--output-sabredat" },
					"Output in SabreDat format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSfvFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-osfv", "--output-sfv" },
					"Output in SFV format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSha1Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-osha1", "--output-sha1" },
					"Output in SHA-1 format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSha256Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-osha256", "--output-sha256" },
					"Output in SHA-256 format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSha384Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-osha384", "--output-sha384" },
					"Output in SHA-256 format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSha512Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-osha512", "--output-sha512" },
					"Output in SHA-256 format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSoftwarelistFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-osl", "--output-softwarelist" },
					"Output in Softwarelist format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputTsvFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-otsv", "--output-tsv" },
					"Output in TSV format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputXmlFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ox", "--output-xml" },
					"Output in Logiqx XML format [default]",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature quickFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-qs", "--quick" },
					"Enable quick scanning of archives",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature quotesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-q", "--quotes" },
					"Put double-quotes around each item",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature removeExtensionsFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rme", "--remove-extensions" },
					"Remove all extensions from each item",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature removeMd5Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rmd5", "--remove-md5" },
					"Remove MD5 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature removeSha1Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha1", "--remove-sha1" },
					"Remove SHA-1 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature removeSha256Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha256", "--remove-sha256" },
					"Remove SHA-256 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature removeSha384Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha384", "--remove-sha384" },
					"Remove SHA-384 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature removeSha512Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha512", "--remove-sha512" },
					"Remove SHA-512 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature removeUnicodeFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ru", "--remove-unicode" },
					"Remove unicode characters from names",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature reverseBaseReplaceFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rbr", "--reverse-base-replace" },
					"Replace item names from base DATs in reverse",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature reverseCascadeFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-rc", "--reverse-cascade" },
					"Enable reverse cascaded diffing",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature rombaFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ro", "--romba" },
					"Treat like a Romba depot (requires SHA-1)",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature romsFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-r", "--roms" },
					"Output roms to miss instead of sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature runnableFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-run", "--runnable" },
					"Include only items that are marked runnable",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature scanAllFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-sa", "--scan-all" },
					"Set scanning levels for all archives to 0",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature sceneDateStripFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-sds", "--scene-date-strip" },
					"Remove date from scene-named sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature shortFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-s", "--short" },
					"Use short output names",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature singleSetFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-si", "--single-set" },
					"All game names replaced by '!'",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature sizeFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-szs", "--size" },
					"Split DAT(s) or folder by file sizes",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipFirstOutputFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-sf", "--skip-first-output" },
					"Skip output of first DAT",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipArchivesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ska", "--skip-archives" },
					"Skip any files that are treated like archives",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipFilesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-skf", "--skip-files" },
					"Skip any files that are not treated like archives",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipMd5Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-nm", "--skip-md5" },
					"Don't include MD5 in output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipSha1Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns", "--skip-sha1" },
					"Don't include SHA1 in output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipSha256Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns256", "--skip-sha256" },
					"Include SHA-256 in output", // TODO: Invert this later
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipSha384Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns384", "--skip-sha384" },
					"Include SHA-384 in output", // TODO: Invert this later
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipSha512Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns512", "--skip-sha512" },
					"Include SHA-512 in output", // TODO: Invert this later
					FeatureType.Flag,
					null);
			}
		}
		private static Feature superdatFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-sd", "--superdat" },
					"Enable SuperDAT creation",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature tarFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tar", "--tar" },
					"Enable TAR output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature textFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-txt", "--text" },
					"Output in generic text format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrent7zipFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-t7z", "--torrent-7zip" },
					"Enable Torrent7z output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentGzipFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tgz", "--torrent-gzip" },
					"Enable TorrentGZ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentLrzipFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tlrz", "--torrent-lrzip" },
					"Enable TorrentLRZ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentLz4Flag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tlz4", "--torrent-lz4" },
					"Enable TorrentLZ4 output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentRarFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-trar", "--torrent-rar" },
					"Enable TorrentRAR output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentXzFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-txz", "--torrent-xz" },
					"Enable TorrentXZ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentZipFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tzip", "--torrent-zip" },
					"Enable TorrentZip output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentZpaqFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tzpaq", "--torrent-zpaq" },
					"Enable TorrentZPAQ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature torrentZstdFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tzstd", "--torrent-zstd" },
					"Enable TorrentZstd output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature trimFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-trim", "--trim" },
					"Trim file names to fit NTFS length",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature tsvFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-tsv", "--tsv" },
					"Output in Tab-Separated Value format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature typeFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ts", "--type" },
					"Split DAT(s) or folder by file types (rom/disk)",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateDatFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-ud", "--update-dat" },
					"Output updated DAT to output directory",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateDescriptionFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-udd", "--update-description" },
					"Update machine descriptions from base DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateHashesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-uh", "--update-hashes" },
					"Update hashes from base DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateManufacturerFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-um", "--update-manufacturer" },
					"Update machine manufacturers from base DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateNamesFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-un", "--update-names" },
					"Update item names from base DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateYearFlag
		{
			get
			{
				return new Feature(
					new List<string>() { "-uy", "--update-year" },
					"Update machine years from base DATs",
					FeatureType.Flag,
					null);
			}
		}

		#endregion

		#region Private Int32 features

		private static Feature gzInt32Input
		{
			get
			{
				return new Feature(
					new List<string>() { "-gz", "--gz" },
					"Set scanning level for GZip archives (default 1)",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature rarInt32Input
		{
			get
			{
				return new Feature(
					new List<string>() { "-rar", "--rar" },
					"Set scanning level for RAR archives (default 1)",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature sevenZipInt32Input
		{
			get
			{
				return new Feature(
					new List<string>() { "-7z", "--7z" },
					"Set scanning level for 7z archives (default 1)",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature threadsInt32Input
		{
			get
			{
				return new Feature(
					new List<string>() { "-mt", "--threads" },
					"Amount of threads to use (default = # cores, -1 unlimted)",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature zipInt32Input
		{
			get
			{
				return new Feature(
					new List<string>() { "-zip", "--zip" },
					"Set scanning level for ZIP archives (default 1)",
					FeatureType.Int32,
					null);
			}
		}

		#endregion

		#region Private Int64 features

		private static Feature radixInt64Input
		{
			get
			{
				return new Feature(
					new List<string>() { "-rad", "--radix" },
					"Set the midpoint to split at",
					FeatureType.Int64,
					null);
			}
		}

		#endregion

		#region Private List<string> features

		private static Feature baseDatListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-bd", "--base-dat" },
					"Add a base DAT for processing",
					FeatureType.List,
					null);
			}
		}
		private static Feature crcListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-crc", "--crc" },
					"Filter by CRC hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature datListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-dat", "--dat" },
					"Input DAT",
					FeatureType.List,
					null);
			}
		}
		private static Feature extaListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-exta", "--exta" },
					"First extension (multiple allowed)",
					FeatureType.List,
					null);
			}
		}
		private static Feature extbListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-extb", "--extb" },
					"Second extension (multiple allowed)",
					FeatureType.List,
					null);
			}
		}
		private static Feature gameNameListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-gn", "--game-name" },
					"Filter by game name",
					FeatureType.List,
					null);
			}
		}
		private static Feature gameTypeListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-gt", "--game-type" },
					"Include only games with a given type",
					FeatureType.List,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Bios, Device, Mechanical",
					});
			}
		}
		private static Feature itemNameListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-rn", "--item-name" },
					"Filter by item name",
					FeatureType.List,
					null);
			}
		}
		private static Feature itemTypeListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-rt", "--item-type" },
					"Filter by item type",
					FeatureType.List,
					null);
			}
		}
		private static Feature md5ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-md5", "--md5" },
					"Filter by MD5 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature notCrcListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-ncrc", "--not-crc" },
					"Filter by not CRC hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature notGameNameListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-ngn", "--not-game-name" },
					"Filter by not game name",
					FeatureType.List,
					null);
			}
		}
		private static Feature notGameTypeListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-ngt", "--not-game-type" },
					"Exclude only games with a given type",
					FeatureType.List,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Bios, Device, Mechanical",
					});
			}
		}
		private static Feature notItemNameListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nrn", "--not-item-name" },
					"Filter by not rom name",
					FeatureType.List,
					null);
			}
		}
		private static Feature notItemTypeListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nrt", "--not-item-type" },
					"Filter by not item type",
					FeatureType.List,
					null);
			}
		}
		private static Feature notMd5ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nmd5", "--not-md5" },
					"Filter by not MD5 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature notSha1ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nsha1", "--not-sha1" },
					"Filter by not SHA-1 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature notSha256ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nsha256", "--not-sha256" },
					"Filter by not SHA-256 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature notSha384ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nsha384", "--not-sha384" },
					"Filter by not SHA-384 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature notSha512ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nsha512", "--not-sha512" },
					"Filter by not SHA-512 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature notStatusListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-nis", "--not-status" },
					"Exclude only items with a given status",
					FeatureType.List,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Good, BadDump, Nodump, Verified",
					});
			}
		}
		private static Feature sha1ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-sha1", "--sha1" },
					"Filter by SHA-1 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature sha256ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-sha256", "--sha256" },
					"Filter by SHA-256 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature sha384ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-sha384", "--sha384" },
					"Filter by SHA-384 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature sha512ListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-sha512", "--sha512" },
					"Filter by SHA-512 hash",
					FeatureType.List,
					null);
			}
		}
		private static Feature statusListInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-is", "--status" },
					"Include only items with a given status",
					FeatureType.List,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Good, BadDump, Nodump, Verified",
					});
			}
		}

		#endregion

		#region Private String features

		private static Feature addExtensionStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-ae", "--add-extension" },
					"Add an extension to each item",
					FeatureType.String,
					null);
			}
		}
		private static Feature authorStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-au", "--author" },
					"Set the author of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature categoryStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-c", "--category" },
					"Set the category of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature commentStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-co", "--comment" },
					"Set a new comment of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature dateStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-da", "--date" },
					"Set a new date",
					FeatureType.String,
					null);
			}
		}
		private static Feature descriptionStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-de", "--description" },
					"Set the description of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature emailStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-em", "--email" },
					"Set a new email of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature equalStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-seq", "--equal" },
					"Filter by size ==",
					FeatureType.String,
					null);
			}
		}
		private static Feature filenameStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-f", "--filename" },
					"Set the external name of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature forcemergingStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-fm", "--forcemerging" },
					"Set force merging",
					FeatureType.String,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Split, Merged, Nonmerged, Full",
					});
			}
		}
		private static Feature forcenodumpStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-fn", "--forcenodump" },
					"Set force nodump",
					FeatureType.String,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Obsolete, Required, Ignore",
					});
			}
		}
		private static Feature forcepackingStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-fp", "--forcepacking" },
					"Set force packing",
					FeatureType.String,
					new List<string>()
					{
						"Supported values are:",
						"	None, Zip, Unzip",
					});
			}
		}
		private static Feature greaterStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-sgt", "--greater" },
					"Filter by size >=",
					FeatureType.String,
					null);
			}
		}
		private static Feature headerStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-h", "--header" },
					"Set a header skipper to use, blank means all",
					FeatureType.String,
					null);

			}
		}
		private static Feature homepageStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-hp", "--homepage" },
					"Set a new homepage of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature lessStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-slt", "--less" },
					"Filter by size =<",
					FeatureType.String,
					null);
			}
		}
		private static Feature nameStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-n", "--name" },
					"Set the internal name of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature outputDirStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-out", "--output-dir" },
					"Output directory",
					FeatureType.String,
					null);
			}
		}
		private static Feature postfixStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-post", "--postfix" },
					"Set postfix for all lines",
					FeatureType.String,
					null);
			}
		}
		private static Feature prefixStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-pre", "--prefix" },
					"Set prefix for all lines",
					FeatureType.String,
					null);
			}
		}
		private static Feature replaceExtensionStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-rep", "--replace-extension" },
					"Replace all extensions with specified",
					FeatureType.String,
					null);
			}
		}
		private static Feature rootStringInput
		{
			get
			{
				return new Feature(
					new List<string>() { "-r", "--root" },
					"Set a new rootdir",
					FeatureType.String,
					null);
			}
		}
		private static Feature rootDirFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rd", "--root-dir" },
					"Set the root directory for calc",
					FeatureType.String,
					null);
			}
		}
		private static Feature tempFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-t", "--temp" },
					"Set the temporary directory to use",
					FeatureType.String,
					null);
			}
		}
		private static Feature urlFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-u", "--url" },
					"Set a new URL of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature versionFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-v", "--version" },
					"Set the version of the DAT",
					FeatureType.String,
					null);
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
				new List<string>() { "-?", "-h", "--help" },
				"Show this help",
				FeatureType.Flag,
				null);

			#endregion

			#region Script

			Feature script = new Feature(
				"--script",
				"Enable script mode (no clear screen)",
				FeatureType.Flag,
				null);

			#endregion

			#region DATFromDir

			Feature datFromDir = new Feature(
				new List<string>() { "-d", "--d2d", "--dfd" },
				"Create DAT(s) from an input directory",
				FeatureType.Flag,
				null);
			datFromDir.AddFeature("skip-md5", skipMd5Flag);
			datFromDir.AddFeature("skip-sha1", skipSha1Flag);
			datFromDir.AddFeature("skip-sha256", skipSha256Flag);
			datFromDir.AddFeature("skip-sha384", skipSha384Flag);
			datFromDir.AddFeature("skip-sha512", skipSha512Flag);
			datFromDir.AddFeature("no-automatic-date", noAutomaticDateFlag);
			datFromDir.AddFeature("forcepacking", forcepackingStringInput);
			datFromDir.AddFeature("archives-as-files", archivesAsFilesFlag);
			datFromDir.AddFeature("output-all", outputAllFlag);
			datFromDir.AddFeature("output-attractmode", outputAttractmodeFlag);
			datFromDir.AddFeature("output-cmp", outputCmpFlag);
			datFromDir.AddFeature("output-csv", outputCsvFlag);
			datFromDir.AddFeature("output-doscenter", outputDoscenterFlag);
			datFromDir.AddFeature("output-listrom", outputListromFlag);
			datFromDir.AddFeature("output-miss", outputMissFlag);
			datFromDir.AddFeature("output-md5", outputMd5Flag);
			datFromDir.AddFeature("output-offlinelist", outputOfflinelistFlag);
			datFromDir.AddFeature("output-romcenter", outputRomcenterFlag);
			datFromDir.AddFeature("output-sabredat", outputSabredatFlag);
			datFromDir.AddFeature("output-sfv", outputSfvFlag);
			datFromDir.AddFeature("output-sha1", outputSha1Flag);
			datFromDir.AddFeature("output-sha256", outputSha256Flag);
			datFromDir.AddFeature("output-sha384", outputSha384Flag);
			datFromDir.AddFeature("output-sha512", outputSha512Flag);
			datFromDir.AddFeature("output-softwarelist", outputSoftwarelistFlag);
			datFromDir.AddFeature("output-tsv", outputTsvFlag);
			datFromDir.AddFeature("output-xml", outputXmlFlag);
			datFromDir.AddFeature("romba", rombaFlag);
			datFromDir.AddFeature("skip-archives", skipArchivesFlag);
			datFromDir.AddFeature("skip-files", skipFilesFlag);
			datFromDir.AddFeature("filename", filenameStringInput);
			datFromDir.AddFeature("name", nameStringInput);
			datFromDir.AddFeature("description", descriptionStringInput);
			datFromDir.AddFeature("category", categoryStringInput);
			datFromDir.AddFeature("version", versionFeature);
			datFromDir.AddFeature("author", authorStringInput);
			datFromDir.AddFeature("email", emailStringInput);
			datFromDir.AddFeature("homepage", homepageStringInput);
			datFromDir.AddFeature("url", urlFeature);
			datFromDir.AddFeature("comment", commentStringInput);
			datFromDir.AddFeature("superdat", superdatFlag);
			datFromDir.AddFeature("exclude-of", excludeOfFlag);
			datFromDir.AddFeature("scene-date-strip", sceneDateStripFlag);
			datFromDir.AddFeature("add-blank-files", addBlankFilesFlag);
			datFromDir.AddFeature("add-date", addDateFlag);
			datFromDir.AddFeature("copy-files", copyFilesFlag);
			datFromDir.AddFeature("header", headerStringInput);
			datFromDir.AddFeature("chds-as-files", chdsAsFilesFlag);
			datFromDir.AddFeature("temp", tempFeature);
			datFromDir.AddFeature("output-dir", outputDirStringInput);
			datFromDir.AddFeature("threads", threadsInt32Input);

			#endregion

			#region Extract

			Feature extract = new Feature(
				new List<string>() { "-ex", "--extract" },
				"Extract and remove copier headers",
				FeatureType.Flag,
				null);
			extract.AddFeature("output-dir", outputDirStringInput);
			extract.AddFeature("no-store-header", noStoreHeaderFlag);

			#endregion

			#region Restore

			Feature restore = new Feature(
				new List<string>() { "-re", "--restore" },
				"Restore header to file based on SHA-1",
				FeatureType.Flag,
				null);
			restore.AddFeature("output-dir", outputDirStringInput);

			#endregion

			#region Sort

			Feature sort = new Feature(
				new List<string>() { "-ss", "--sort" },
				"Sort inputs by a set of DATs",
				FeatureType.Flag,
				new List<string>()
				{
					"",
					"Archive scanning levels:",
					"  0	Hash archive and contents",
					"  1	Only hash contents",
					"  2	Only hash archive",
				});
			sort.AddFeature("dat", datListInput);
			sort.AddFeature("output-dir", outputDirStringInput);
			sort.AddFeature("depot", depotFlag);
			sort.AddFeature("delete", deleteFlag);
			sort.AddFeature("inverse", inverseFlag);
			sort.AddFeature("quick", quickFlag);
			sort.AddFeature("chds-as-files", chdsAsFilesFlag);
			sort.AddFeature("add-date", addDateFlag);
			sort.AddFeature("torrent-7zip", torrent7zipFlag);
			sort.AddFeature("tar", tarFlag);
			sort.AddFeature("torrent-gzip", torrentGzipFlag);
				sort["tgz"].AddFeature("romba", rombaFlag);
			//sort.AddFeature("torrent-lrzip", tlrzFeature);
			//sort.AddFeature("torrent-lz4", tlz4Feature);
			//sort.AddFeature("torrent-rar", trarFeature);
			//sort.AddFeature("torrent-xz", txzFeature);
			sort.AddFeature("torrent-zip", torrentZipFlag);
			//sort.AddFeature("torrent-zpaq", tzpaqFeature);
			//sort.AddFeature("torrent-zstd", tzstdFeature);
			sort.AddFeature("header", headerStringInput);
			sort.AddFeature("7z", sevenZipInt32Input);
			sort.AddFeature("gz", gzInt32Input);
			sort.AddFeature("rar", rarInt32Input);
			sort.AddFeature("zip", zipInt32Input);
			sort.AddFeature("scan-all", scanAllFlag);
			sort.AddFeature("dat-merged", datMergedFlag);
			sort.AddFeature("dat-split", datSplitFlag);
			sort.AddFeature("dat-non-merged", datNonMergedFlag);
			sort.AddFeature("dat-device-non-merged", datDeviceNonMergedFlag);
			sort.AddFeature("dat-full-non-merged", datFullNonMergedFlag);
			sort.AddFeature("update-dat", updateDatFlag);
			sort.AddFeature("threads", threadsInt32Input);

			#endregion

			#region Split

			Feature split = new Feature(
				new List<string>() { "-sp", "--split" },
				"Split input DATs by a given criteria",
				FeatureType.Flag,
				null);
			split.AddFeature("output-all", outputAllFlag);
			split.AddFeature("output-attractmode", outputAttractmodeFlag);
			split.AddFeature("output-cmp", outputCmpFlag);
			split.AddFeature("output-csv", outputCsvFlag);
			split.AddFeature("output-doscenter", outputDoscenterFlag);
			split.AddFeature("output-listrom", outputListromFlag);
			split.AddFeature("output-miss", outputMissFlag);
			split.AddFeature("output-md5", outputMd5Flag);
			split.AddFeature("output-offlinelist", outputOfflinelistFlag);
			split.AddFeature("output-romcenter", outputRomcenterFlag);
			split.AddFeature("output-sabredat", outputSabredatFlag);
			split.AddFeature("output-sfv", outputSfvFlag);
			split.AddFeature("output-sha1", outputSha1Flag);
			split.AddFeature("output-sha256", outputSha256Flag);
			split.AddFeature("output-sha384", outputSha384Flag);
			split.AddFeature("output-sha512", outputSha512Flag);
			split.AddFeature("output-softwarelist", outputSoftwarelistFlag);
			split.AddFeature("output-tsv", outputTsvFlag);
			split.AddFeature("output-xml", outputXmlFlag);
			split.AddFeature("output-dir", outputDirStringInput);
			split.AddFeature("inplace", inplaceFlag);
			split.AddFeature("extension", extensionFlag);
				split["ext"].AddFeature("exta", extaListInput);
				split["ext"].AddFeature("extb", extbListInput);
			split.AddFeature("hash", hashFlag);
			split.AddFeature("level", levelFlag);
				split["level"].AddFeature("short", shortFlag);
				split["level"].AddFeature("base", baseFlag);
			split.AddFeature("size", sizeFlag);
				split["size"].AddFeature("radix", radixInt64Input);
			split.AddFeature("type", typeFlag);

			#endregion

			#region Stats

			Feature stats = new Feature(
				new List<string>() { "-st", "--stats" },
				"Get statistics on all input DATs",
				FeatureType.Flag,
				null);
			stats.AddFeature("all-stats", allStatsFlag);
			stats.AddFeature("baddump-column", baddumpColumnFlag);
			stats.AddFeature("csv", csvFlag);
			stats.AddFeature("filename", filenameStringInput);
			stats.AddFeature("output-dir", outputDirStringInput);
			stats.AddFeature("html", htmlFlag);
			stats.AddFeature("nodump-col", nodumpColumnFlag);
			stats.AddFeature("individual", individualFlag);
			stats.AddFeature("tsv", tsvFlag);
			stats.AddFeature("text", textFlag);

			#endregion

			#region Update

			Feature update = new Feature(
				new List<string>() { "-ud", "--update" },
				"Update and manipulate DAT file(s)",
				FeatureType.Flag,
				new List<string>()
				{
					"",
					"Filter parameters game name, rom name, all hashes can",
					"be matched using full C#-style regex.",

					"",
					"Filter parameters for size can use postfixes for inputs:",
					"  e.g. 8kb => 8000 or 8kib => 8192",

					"",
					"Most of the filter parameters allow for multiple inputs:",
					"  e.g. --game-name=foo --game-name=bar",
				});
			update.AddFeature("output-all", outputAllFlag);
			update.AddFeature("output-attractmode", outputAttractmodeFlag);
			update.AddFeature("output-cmp", outputCmpFlag);
			update.AddFeature("output-csv", outputCsvFlag);
				update["output-csv"].AddFeature("prefix", prefixStringInput);
				update["output-csv"].AddFeature("postfix", postfixStringInput);
				update["output-csv"].AddFeature("quotes", quotesFlag);
			update.AddFeature("output-doscenter", outputDoscenterFlag);
			update.AddFeature("output-listrom", outputListromFlag);
			update.AddFeature("output-miss", outputMissFlag);
				update["output-miss"].AddFeature("roms", romsFlag);
				update["output-miss"].AddFeature("game-prefix", gamePrefixFlag);
				update["output-miss"].AddFeature("prefix", prefixStringInput);
				update["output-miss"].AddFeature("postfix", postfixStringInput);
				update["output-miss"].AddFeature("quotes", quotesFlag);
				update["output-miss"].AddFeature("add-extension", addExtensionStringInput);
				update["output-miss"].AddFeature("replace-extension", replaceExtensionStringInput);
				update["output-miss"].AddFeature("remove-extensions", removeExtensionsFlag);
				update["output-miss"].AddFeature("romba", rombaFlag);
			update.AddFeature("output-md5", outputMd5Flag);
				update["output-md5"].AddFeature("game-prefix", gamePrefixFlag);
			update.AddFeature("output-offlinelist", outputOfflinelistFlag);
			update.AddFeature("output-romcenter", outputRomcenterFlag);
			update.AddFeature("output-sabredat", outputSabredatFlag);
			update.AddFeature("output-sfv", outputSfvFlag);
				update["output-sfv"].AddFeature("game-prefix", gamePrefixFlag);
			update.AddFeature("output-sha1", outputSha1Flag);
				update["output-sha1"].AddFeature("game-prefix", gamePrefixFlag);
			update.AddFeature("output-sha256", outputSha256Flag);
				update["output-sha256"].AddFeature("game-prefix", gamePrefixFlag);
			update.AddFeature("output-sha384", outputSha384Flag);
				update["output-sha384"].AddFeature("game-prefix", gamePrefixFlag);
			update.AddFeature("output-sha512", outputSha512Flag);
				update["output-sha512"].AddFeature("game-prefix", gamePrefixFlag);
			update.AddFeature("output-softwarelist", outputSoftwarelistFlag);
			update.AddFeature("output-tsv", outputTsvFlag);
				update["output-tsv"].AddFeature("prefix", prefixStringInput);
				update["output-tsv"].AddFeature("postfix", postfixStringInput);
				update["output-tsv"].AddFeature("quotes", quotesFlag);
			update.AddFeature("output-xml", outputXmlFlag);
			update.AddFeature("filename", filenameStringInput);
			update.AddFeature("name", nameStringInput);
			update.AddFeature("description", descriptionStringInput);
			update.AddFeature("rootdir", rootStringInput);
			update.AddFeature("category", categoryStringInput);
			update.AddFeature("version", versionFeature);
			update.AddFeature("date", dateStringInput);
			update.AddFeature("author", authorStringInput);
			update.AddFeature("email", emailStringInput);
			update.AddFeature("homepage", homepageStringInput);
			update.AddFeature("url", urlFeature);
			update.AddFeature("comment", commentStringInput);
			update.AddFeature("header", headerStringInput);
			update.AddFeature("superdat", superdatFlag);
			update.AddFeature("forcemerging", forcemergingStringInput);
			update.AddFeature("forcenodump", forcenodumpStringInput);
			update.AddFeature("forcepacking", forcepackingStringInput);
			update.AddFeature("exclude-of", excludeOfFlag);
			update.AddFeature("scene-date-strip", sceneDateStripFlag);
			update.AddFeature("clean", cleanFlag);
			update.AddFeature("remove-uni", removeUnicodeFlag);
			update.AddFeature("remove-md5", removeMd5Flag);
			update.AddFeature("remove-sha1", removeSha1Flag);
			update.AddFeature("remove-sha256", removeSha256Flag);
			update.AddFeature("remove-sha384", removeSha384Flag);
			update.AddFeature("remove-sha512", removeSha512Flag);
			update.AddFeature("description-as-name", descriptionAsNameFlag);
			update.AddFeature("dat-merged", datMergedFlag);
			update.AddFeature("dat-split", datSplitFlag);
			update.AddFeature("dat-non-merged", datNonMergedFlag);
			update.AddFeature("dat-device-non-merged", datDeviceNonMergedFlag);
			update.AddFeature("dat-full-non-merged", datFullNonMergedFlag);
			update.AddFeature("trim", trimFlag);
				update["trim"].AddFeature("root-dir", rootDirFeature);
			update.AddFeature("single-set", singleSetFlag);
			update.AddFeature("dedup", dedupFlag);
			update.AddFeature("game-dedup", gameDedupFlag);
			update.AddFeature("merge", mergeFlag);
				update["merge"].AddFeature("no-automatic-date", noAutomaticDateFlag);
			update.AddFeature("diff", diffFlag);
				update["diff"].AddFeature("against", againstFlag);
					update["diff"]["against"].AddFeature("base-dat", baseDatListInput);
				update["diff"].AddFeature("no-automatic-date", noAutomaticDateFlag);
				update["diff"].AddFeature("cascade", cascadeFlag);
					update["diff"]["cascade"].AddFeature("skip-first-output", skipFirstOutputFlag);
				update["diff"].AddFeature("reverse-cascade", reverseCascadeFlag);
					update["diff"]["rev-cascade"].AddFeature("skip-first-output", skipFirstOutputFlag);
			update.AddFeature("diff-du", diffDuFlag);
				update["diff-du"].AddFeature("no-automatic-date", noAutomaticDateFlag);
			update.AddFeature("diff-in", diffInFlag);
				update["diff-in"].AddFeature("no-automatic-date", noAutomaticDateFlag);
			update.AddFeature("diff-nd", diffNdFlag);
				update["diff-nd"].AddFeature("no-automatic-date", noAutomaticDateFlag);
			update.AddFeature("base-replace", baseReplaceFlag);
				update["base-replace"].AddFeature("base-dat", baseDatListInput);
				update["base-replace"].AddFeature("update-names", updateNamesFlag);
				update["base-replace"].AddFeature("update-hashes", updateHashesFlag);
				update["base-replace"].AddFeature("update-description", updateDescriptionFlag);
					update["base-replace"]["update-description"].AddFeature("only-same", onlySameFlag);
				update["base-replace"].AddFeature("update-year", updateYearFlag);
				update["base-replace"].AddFeature("update-manufacturer", updateManufacturerFlag);
			update.AddFeature("reverse-base-replace", reverseBaseReplaceFlag);
				update["reverse-base-replace"].AddFeature("base-dat", baseDatListInput);
				update["reverse-base-replace"].AddFeature("update-names", updateNamesFlag);
				update["reverse-base-replace"].AddFeature("update-hashes", updateHashesFlag);
				update["reverse-base-replace"].AddFeature("update-description", updateDescriptionFlag);
					update["reverse-base-replace"]["update-description"].AddFeature("only-same", onlySameFlag);
				update["reverse-base-replace"].AddFeature("update-year", updateYearFlag);
				update["reverse-base-replace"].AddFeature("update-manufacturer", updateManufacturerFlag);
			update.AddFeature("game-name", gameNameListInput);
			update.AddFeature("not-game-name", notGameNameListInput);
			update.AddFeature("match-of-tags", matchOfTagsFlag);
			update.AddFeature("rom-name", itemNameListInput);
			update.AddFeature("not-rom-name", notItemNameListInput);
			update.AddFeature("rom-type", itemTypeListInput);
			update.AddFeature("not-type", notItemTypeListInput);
			update.AddFeature("greater", greaterStringInput);
			update.AddFeature("less", lessStringInput);
			update.AddFeature("equal", equalStringInput);
			update.AddFeature("crc", crcListInput);
			update.AddFeature("not-crc", notCrcListInput);
			update.AddFeature("md5", md5ListInput);
			update.AddFeature("not-md5", notMd5ListInput);
			update.AddFeature("sha1", sha1ListInput);
			update.AddFeature("not-sha1", notSha1ListInput);
			update.AddFeature("sha256", sha256ListInput);
			update.AddFeature("not-sha256", notSha256ListInput);
			update.AddFeature("sha384", sha384ListInput);
			update.AddFeature("not-sha384", notSha384ListInput);
			update.AddFeature("sha512", sha512ListInput);
			update.AddFeature("not-sha512", notSha512ListInput);
			update.AddFeature("status", statusListInput);
			update.AddFeature("not-status", notStatusListInput);
			update.AddFeature("game-type", gameTypeListInput);
			update.AddFeature("not-game-type", notGameTypeListInput);
			update.AddFeature("runnable", runnableFlag);
			update.AddFeature("not-runnable", notRunnableFlag);
			update.AddFeature("output-dir", outputDirStringInput);
			update.AddFeature("inplace", inplaceFlag);
			update.AddFeature("threads", threadsInt32Input);

			#endregion

			#region Verify

			Feature verify = new Feature(
				new List<string>() { "-ve", "--verify" },
				"Verify a folder against DATs",
				FeatureType.Flag,
				null);
			verify.AddFeature("dat", datListInput);
			verify.AddFeature("depot", depotFlag);
			verify.AddFeature("temp", tempFeature);
			verify.AddFeature("hash-only", hashOnlyFlag);
			verify.AddFeature("quick", quickFlag);
			verify.AddFeature("header", headerStringInput);
			verify.AddFeature("chds-as-files", chdsAsFilesFlag);
			verify.AddFeature("dat-merged", datMergedFlag);
			verify.AddFeature("dat-split", datSplitFlag);
			verify.AddFeature("dat-device-non-merged", datDeviceNonMergedFlag);
			verify.AddFeature("dat-non-merged", datNonMergedFlag);
			verify.AddFeature("dat-full-non-merged", datFullNonMergedFlag);

			#endregion

			// Now, add all of the main features to the Help object
			help.Add("Help", helpFeature);
			help.Add("Script", script);
			help.Add("DATFromDir", datFromDir);
			help.Add("Extract", extract);
			help.Add("Restore", restore);
			help.Add("Sort", sort);
			help.Add("Split", split);
			help.Add("Stats", stats);
			help.Add("Update", update);
			help.Add("Verify", verify);

			return help;
		}
	}
}
