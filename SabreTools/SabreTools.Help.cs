using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;

namespace SabreTools
{
	public partial class SabreTools
	{
		// TODO: Split this by feature type
		#region Private flag and input features

		private static Feature addBlankFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ab", "--add-blank" },
					"Output blank files for folders",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature addDateFeature
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
		private static Feature addExtFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ae", "--add-ext" },
					"Add an extension to each item",
					FeatureType.String,
					null);
			}
		}
		private static Feature againstFeature
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
		private static Feature allStatsFeature
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
		private static Feature authorFeature
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
		private static Feature baddumpColFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-bc", "--baddump-col" },
					"Add baddump stats to output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature bareFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-b", "--bare" },
					"Don't include date in automatic name",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature baseFeature
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
		private static Feature baseDatFeature
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
		private static Feature baseReplaceFeature
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
		private static Feature cascadeFeature
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
		private static Feature categoryFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-c", "--cat", "--category" },
					"Set the category of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature cleanFeature
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
		private static Feature commentFeature
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
		private static Feature copyFilesFeature
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
		private static Feature crcFeature
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
		private static Feature csvFeature
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
		private static Feature datFeature
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
		private static Feature datDevnonmergedFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-dnd", "--dat-devnonmerged" },
					"Create device non-merged sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature dateFeature
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
		private static Feature datFullnonmergedFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-df", "--dat-fullnonmerged" },
					"Force creating fully non-merged sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature datMergedFeature
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
		private static Feature datNonmergedFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-dnm", "--dat-nonmerged" },
					"Force creating non-merged sets",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature datSplitFeature
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
		private static Feature dedupFeature
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
		private static Feature deleteFeature
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
		private static Feature depotFeature
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
		private static Feature descriptionFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-de", "--desc", "--description" },
					"Set the description of the DAT",
					FeatureType.String,
					null);
			}
		}
		private static Feature descNameFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-dan", "--desc-name" },
					"Use description instead of machine name",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature diffFeature
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
		private static Feature diffDuFeature
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
		private static Feature diffInFeature
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
		private static Feature diffNdFeature
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
		private static Feature emailFeature
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
		private static Feature equalFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-seq", "--equal" },
					"Filter by size ==",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature excludeOfFeature
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
		private static Feature extFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-es", "--ext" },
					"Split DAT(s) by two file extensions",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature extaFeature
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
		private static Feature extbFeature
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
		private static Feature filenameFeature
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
		private static Feature filesFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-f", "--files" },
					"Treat archives as files",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature forcemergeFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-fm", "--forcemerge" },
					"Set force merging",
					FeatureType.String,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Split, Merged, Nonmerged, Full",
					});
			}
		}
		private static Feature forcendFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-fn", "--forcend" },
					"Set force nodump",
					FeatureType.String,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Obsolete, Required, Ignore",
					});
			}
		}
		private static Feature forcepackFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-fp", "--forcepack" },
					"Set force packing",
					FeatureType.String,
					new List<string>()
					{
						"Supported values are:",
						"	None, Zip, Unzip",
					});
			}
		}
		private static Feature gameDedupFeature
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
		private static Feature gameNameFeature
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
		private static Feature gamePrefixFeature
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
		private static Feature gameTypeFeature
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
		private static Feature greaterFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-sgt", "--greater" },
					"Filter by size >=",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature gzFeature
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
		private static Feature hashFeature
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
		private static Feature hashOnlyFeature
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
		private static Feature headerFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-h", "--header" },
					"Set a header skipper to use, blank means all",
					FeatureType.Flag,
					null);

			}
		}
		private static Feature homepageFeature
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
		private static Feature htmlFeature
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
		private static Feature ignoreChdFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ic", "--ignore-chd" },
					"Treat CHDs as regular files",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature individualFeature
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
		private static Feature inplaceFeature
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
		private static Feature inverseFeature
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
		private static Feature lessFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-slt", "--less" },
					"Filter by size =<",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature levelFeature
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
		private static Feature md5Feature
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
		private static Feature mergeFeature
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
		private static Feature mtFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-mt", "--mt" },
					"Amount of threads to use (default 4, -1 unlimted)",
					FeatureType.Int32,
					null);
			}
		}
		private static Feature nameFeature
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
		private static Feature nodumpColFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-nc", "--nodump-col" },
					"Add nodump stats to output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noMD5Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-nm", "--noMD5" },
					"Don't include MD5 in output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noSHA1Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns", "--noSHA1" },
					"Don't include SHA1 in output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noSHA256Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns256", "--noSHA256" },
					"Include SHA-256 in output", // TODO: Invert this later
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noSHA384Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns384", "--noSHA384" },
					"Include SHA-384 in output", // TODO: Invert this later
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noSHA512Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ns512", "--noSHA512" },
					"Include SHA-512 in output", // TODO: Invert this later
					FeatureType.Flag,
					null);
			}
		}
		private static Feature noStoreHeaderFeature
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
		private static Feature notCrcFeature
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
		private static Feature notGameFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ngn", "--not-game" },
					"Filter by not game name",
					FeatureType.List,
					null);
			}
		}
		private static Feature notGtypeFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ngt", "--not-gtype" },
					"Exclude only games with a given type",
					FeatureType.List,
					new List<string>()
					{
						"			    Supported values are:",
						"			        None, Bios, Device, Mechanical",
					});
			}
		}
		private static Feature notMd5Feature
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
		private static Feature notRomFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-nrn", "--not-rom" },
					"Filter by not rom name",
					FeatureType.List,
					null);
			}
		}
		private static Feature notRunFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-nrun", "--not-run" },
					"Include only items that are marked unrunnable",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature notSha1Feature
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
		private static Feature notSha256Feature
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
		private static Feature notSha384Feature
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
		private static Feature notSha512Feature
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
		private static Feature notStatusFeature
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
		private static Feature notTypeFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-nrt", "--not-type" },
					"Filter by not rom type",
					FeatureType.List,
					null);
			}
		}
		private static Feature ofAsGameFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ofg", "--of-as-game" },
					"Allow cloneof and romof tags to match game name filters",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-out", "--out" },
					"Output directory",
					FeatureType.String,
					null);
			}
		}
		private static Feature outputAllFeature
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
		private static Feature outputAmFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-oam", "--output-am" },
					"Output in AttractMode format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputCmpFeature
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
		private static Feature outputCsvFeature
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
		private static Feature outputDcFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-od", "--output-dc" },
					"Output in DOSCenter format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputLrFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-olr", "--output-lr" },
					"Output in MAME Listrom format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputMd5Feature
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
		private static Feature outputMissFeature
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
		private static Feature outputOlFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ool", "--output-ol" },
					"Output in OfflineList format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputRcFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-or", "--output-rc" },
					"Output in RomCenter format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSdFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-os", "--output-sd" },
					"Output in SabreDat format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputSfvFeature
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
		private static Feature outputSha1Feature
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
		private static Feature outputSha256Feature
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
		private static Feature outputSha384Feature
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
		private static Feature outputSha512Feature
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
		private static Feature outputSlFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-osl", "--output-sl" },
					"Output in Softwarelist format",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature outputTsvFeature
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
		private static Feature outputXmlFeature
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
		private static Feature prefixFeature
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
		private static Feature postfixFeature
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
		private static Feature quickFeature
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
		private static Feature quotesFeature
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
		private static Feature rarFeature
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
		private static Feature remExtFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rme", "--rem-ext" },
					"Remove all extensions from each item",
					FeatureType.String,
					null);
			}
		}
		private static Feature remMd5Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rmd5", "--rem-md5" },
					"Remove MD5 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature remSha1Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha1", "--rem-sha1" },
					"Remove SHA-1 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature remSha256Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha256", "--rem-sha256" },
					"Remove SHA-256 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature remSha384Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha384", "--rem-sha384" },
					"Remove SHA-384 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature remSha512Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rsha512", "--rem-sha512" },
					"Remove SHA-512 hashes from the output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature remUniFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ru", "--rem-uni" },
					"Remove unicode characters from names",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature repExtFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rep", "--rep-ext" },
					"Replace all extensions with specified",
					FeatureType.String,
					null);
			}
		}
		private static Feature revCascadeFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rc", "--rev-cascade" },
					"Enable reverse cascaded diffing",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature reverseBaseReplaceFeature
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
		private static Feature rombaFeature
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
		private static Feature romNameFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rn", "--rom-name" },
					"Filter by rom name",
					FeatureType.List,
					null);
			}
		}
		private static Feature romsFeature
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
		private static Feature romTypeFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-rt", "--rom-type" },
					"Filter by rom type",
					FeatureType.List,
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
		private static Feature rootFeature
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
		private static Feature runnableFeature
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
		private static Feature scanAllFeature
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
		private static Feature sceneDateStripFeature
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
		private static Feature sevenZipFeature
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
		private static Feature sha1Feature
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
		private static Feature sha256Feature
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
		private static Feature sha384Feature
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
		private static Feature sha512Feature
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
		private static Feature shortFeature
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
		private static Feature singleFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-si", "--single" },
					"All game names replaced by '!'",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-sf", "--skip" },
					"Skip output of first DAT",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skiparcFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-ska", "--skiparc" },
					"Skip any files that are treated like archives",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature skipfileFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-skf", "--skipfile" },
					"Skip any files that are not treated like archives",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature statusFeature
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
		private static Feature superdatFeature
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
		private static Feature t7zFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-t7z", "--t7z" },
					"Enable Torrent7z output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature tarFeature
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
		private static Feature textFeature
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
		private static Feature tgzFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-tgz", "--tgz" },
					"Enable TorrentGZ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature tlrzFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-tlrz", "--tlrz" },
					"Enable TorrentLRZ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature tlz4Feature
		{
			get
			{
				return new Feature(
					new List<string>() { "-tlz4", "--tlz4" },
					"Enable TorrentLZ4 output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature trarFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-trar", "--trar" },
					"Enable TorrentRAR output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature trimFeature
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
		private static Feature tsvFeature
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
		private static Feature txzFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-txz", "--txz" },
					"Enable TorrentXZ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature typeFeature
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
		private static Feature tzipFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-tzip", "--tzip" },
					"Enable TorrentZip output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature tzpaqFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-tzpaq", "--tzpaq" },
					"Enable TorrentZPAQ output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature tzstdFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-tzstd", "--tzstd" },
					"Enable TorrentZstd output",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateDatFeature
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
		private static Feature updateDescFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-udd", "--update-desc" },
					"Update machine descriptions from base DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateHashesFeature
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
		private static Feature updateManuFeature
		{
			get
			{
				return new Feature(
					new List<string>() { "-um", "--update-manu" },
					"Update machine manufacturers from base DATs",
					FeatureType.Flag,
					null);
			}
		}
		private static Feature updateNamesFeature
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
		private static Feature updateYearFeature
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
		private static Feature zipFeature
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

		// TODO: Create features ahead of time so that features can be more easily maintained
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
			datFromDir.AddFeature("noMD5", noMD5Feature);
			datFromDir.AddFeature("noSHA1", noSHA1Feature);
			datFromDir.AddFeature("noSHA256", noSHA256Feature);
			datFromDir.AddFeature("noSHA384", noSHA384Feature);
			datFromDir.AddFeature("noSHA512", noSHA512Feature);
			datFromDir.AddFeature("bare", bareFeature);
			datFromDir.AddFeature("forcepack", forcepackFeature);
			datFromDir.AddFeature("files", filesFeature);
			datFromDir.AddFeature("output-all", outputAllFeature);
			datFromDir.AddFeature("output-am", outputAmFeature);
			datFromDir.AddFeature("output-cmp", outputCmpFeature);
			datFromDir.AddFeature("output-csv", outputCsvFeature);
			datFromDir.AddFeature("output-dc", outputDcFeature);
			datFromDir.AddFeature("output-lr", outputLrFeature);
			datFromDir.AddFeature("output-miss", outputMissFeature);
			datFromDir.AddFeature("output-md5", outputMd5Feature);
			datFromDir.AddFeature("output-ol", outputOlFeature);
			datFromDir.AddFeature("output-rc", outputRcFeature);
			datFromDir.AddFeature("output-sd", outputSdFeature);
			datFromDir.AddFeature("output-sfv", outputSfvFeature);
			datFromDir.AddFeature("output-sha1", outputSha1Feature);
			datFromDir.AddFeature("output-sha256", outputSha256Feature);
			datFromDir.AddFeature("output-sha384", outputSha384Feature);
			datFromDir.AddFeature("output-sha512", outputSha512Feature);
			datFromDir.AddFeature("output-sl", outputSlFeature);
			datFromDir.AddFeature("output-tsv", outputTsvFeature);
			datFromDir.AddFeature("output-xml", outputXmlFeature);
			datFromDir.AddFeature("romba", rombaFeature);
			datFromDir.AddFeature("skiparc", skiparcFeature);
			datFromDir.AddFeature("skipfile", skipfileFeature);
			datFromDir.AddFeature("filename", filenameFeature);
			datFromDir.AddFeature("name", nameFeature);
			datFromDir.AddFeature("desc", descriptionFeature);
			datFromDir.AddFeature("category", categoryFeature);
			datFromDir.AddFeature("version", versionFeature);
			datFromDir.AddFeature("author", authorFeature);
			datFromDir.AddFeature("email", emailFeature);
			datFromDir.AddFeature("homepage", homepageFeature);
			datFromDir.AddFeature("url", urlFeature);
			datFromDir.AddFeature("comment", commentFeature);
			datFromDir.AddFeature("superdat", superdatFeature);
			datFromDir.AddFeature("exclude-of", excludeOfFeature);
			datFromDir.AddFeature("scene-date-strip", sceneDateStripFeature);
			datFromDir.AddFeature("add-blank", addBlankFeature);
			datFromDir.AddFeature("add-date", addDateFeature);
			datFromDir.AddFeature("copy-files", copyFilesFeature);
			datFromDir.AddFeature("header", headerFeature);
			datFromDir.AddFeature("ignore-chd", ignoreChdFeature);
			datFromDir.AddFeature("temp", tempFeature);
			datFromDir.AddFeature("out", outFeature);
			datFromDir.AddFeature("mt", mtFeature);

			#endregion

			#region Extract

			Feature extract = new Feature(
				new List<string>() { "-ex", "--extract" },
				"Extract and remove copier headers",
				FeatureType.Flag,
				null);
			extract.AddFeature("out", outFeature);
			extract.AddFeature("no-store-header", noStoreHeaderFeature);

			#endregion

			#region Restore

			Feature restore = new Feature(
				new List<string>() { "-re", "--restore" },
				"Restore header to file based on SHA-1",
				FeatureType.Flag,
				null);
			restore.AddFeature("out", outFeature);

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
			sort.AddFeature("dat", datFeature);
			sort.AddFeature("out", outFeature);
			sort.AddFeature("depot", depotFeature);
			sort.AddFeature("delete", deleteFeature);
			sort.AddFeature("inverse", inverseFeature);
			sort.AddFeature("quick", quickFeature);
			sort.AddFeature("ignore-chd", ignoreChdFeature);
			sort.AddFeature("add-date", addDateFeature);
			sort.AddFeature("t7z", t7zFeature);
			sort.AddFeature("tar", tarFeature);
			sort.AddFeature("tgz", tgzFeature);
				sort["tgz"].AddFeature("romba", rombaFeature);
			//sort.AddFeature("tlrz", tlrzFeature);
			//sort.AddFeature("tlz4", tlz4Feature);
			//sort.AddFeature("trar", trarFeature);
			//sort.AddFeature("txz", txzFeature);
			sort.AddFeature("tzip", tzipFeature);
			//sort.AddFeature("tzpaq", tzpaqFeature);
			//sort.AddFeature("tzstd", tzstdFeature);
			sort.AddFeature("header", headerFeature);
			sort.AddFeature("7z", sevenZipFeature);
			sort.AddFeature("gz", gzFeature);
			sort.AddFeature("rar", rarFeature);
			sort.AddFeature("zip", zipFeature);
			sort.AddFeature("scan-all", scanAllFeature);
			sort.AddFeature("dat-merged", datMergedFeature);
			sort.AddFeature("dat-split", datSplitFeature);
			sort.AddFeature("dat-nonmerged", datNonmergedFeature);
			sort.AddFeature("dat-fullnonmerged", datFullnonmergedFeature);
			sort.AddFeature("update-dat", updateDatFeature);
			sort.AddFeature("mt", mtFeature);

			#endregion

			#region Split

			Feature split = new Feature(
				new List<string>() { "-sp", "--split" },
				"Split input DATs by a given criteria",
				FeatureType.Flag,
				null);
			split.AddFeature("output-all", outputAllFeature);
			split.AddFeature("output-am", outputAmFeature);
			split.AddFeature("output-cmp", outputCmpFeature);
			split.AddFeature("output-csv", outputCsvFeature);
			split.AddFeature("output-dc", outputDcFeature);
			split.AddFeature("output-lr", outputLrFeature);
			split.AddFeature("output-miss", outputMissFeature);
			split.AddFeature("output-md5", outputMd5Feature);
			split.AddFeature("output-ol", outputOlFeature);
			split.AddFeature("output-rc", outputRcFeature);
			split.AddFeature("output-sd", outputSdFeature);
			split.AddFeature("output-sfv", outputSfvFeature);
			split.AddFeature("output-sha1", outputSha1Feature);
			split.AddFeature("output-sha256", outputSha256Feature);
			split.AddFeature("output-sha384", outputSha384Feature);
			split.AddFeature("output-sha512", outputSha512Feature);
			split.AddFeature("output-sl", outputSlFeature);
			split.AddFeature("output-tsv", outputTsvFeature);
			split.AddFeature("output-xml", outputXmlFeature);
			split.AddFeature("out", outFeature);
			split.AddFeature("inplace", inplaceFeature);
			split.AddFeature("ext", extFeature);
				split["ext"].AddFeature("exta", extaFeature);
				split["ext"].AddFeature("extb", extbFeature);
			split.AddFeature("hash", hashFeature);
			split.AddFeature("level", levelFeature);
				split["level"].AddFeature("short", shortFeature);
				split["level"].AddFeature("base", baseFeature);
			split.AddFeature("type", typeFeature);

			#endregion

			#region Stats

			Feature stats = new Feature(
				new List<string>() { "-st", "--stats" },
				"Get statistics on all input DATs",
				FeatureType.Flag,
				null);
			stats.AddFeature("all-stats", allStatsFeature);
			stats.AddFeature("baddump-col", baddumpColFeature);
			stats.AddFeature("csv", csvFeature);
			stats.AddFeature("filename", filenameFeature);
			stats.AddFeature("out", outFeature);
			stats.AddFeature("html", htmlFeature);
			stats.AddFeature("nodump-col", nodumpColFeature);
			stats.AddFeature("individual", individualFeature);
			stats.AddFeature("tsv", tsvFeature);
			stats.AddFeature("text", textFeature);

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
			update.AddFeature("output-all", outputAllFeature);
			update.AddFeature("output-am", outputAmFeature);
			update.AddFeature("output-cmp", outputCmpFeature);
			update.AddFeature("output-csv", outputCsvFeature);
				update["output-csv"].AddFeature("prefix", prefixFeature);
				update["output-csv"].AddFeature("postfix", postfixFeature);
				update["output-csv"].AddFeature("quotes", quotesFeature);
			update.AddFeature("output-dc", outputDcFeature);
			update.AddFeature("output-lr", outputLrFeature);
			update.AddFeature("output-miss", outputMissFeature);
				update["output-miss"].AddFeature("roms", romsFeature);
				update["output-miss"].AddFeature("game-prefix", gamePrefixFeature);
				update["output-miss"].AddFeature("prefix", prefixFeature);
				update["output-miss"].AddFeature("postfix", postfixFeature);
				update["output-miss"].AddFeature("quotes", quotesFeature);
				update["output-miss"].AddFeature("add-ext", addExtFeature);
				update["output-miss"].AddFeature("rep-ext", repExtFeature);
				update["output-miss"].AddFeature("rem-ext", remExtFeature);
				update["output-miss"].AddFeature("romba", rombaFeature);
			update.AddFeature("output-md5", outputMd5Feature);
				update["output-md5"].AddFeature("game-prefix", gamePrefixFeature);
			update.AddFeature("output-ol", outputOlFeature);
			update.AddFeature("output-rc", outputRcFeature);
			update.AddFeature("output-sd", outputSdFeature);
			update.AddFeature("output-sfv", outputSfvFeature);
				update["output-sfv"].AddFeature("game-prefix", gamePrefixFeature);
			update.AddFeature("output-sha1", outputSha1Feature);
				update["output-sha1"].AddFeature("game-prefix", gamePrefixFeature);
			update.AddFeature("output-sha256", outputSha256Feature);
				update["output-sha256"].AddFeature("game-prefix", gamePrefixFeature);
			update.AddFeature("output-sha384", outputSha384Feature);
				update["output-sha384"].AddFeature("game-prefix", gamePrefixFeature);
			update.AddFeature("output-sha512", outputSha512Feature);
				update["output-sha512"].AddFeature("game-prefix", gamePrefixFeature);
			update.AddFeature("output-sl", outputSlFeature);
			update.AddFeature("output-tsv", outputTsvFeature);
				update["output-tsv"].AddFeature("prefix", prefixFeature);
				update["output-tsv"].AddFeature("postfix", postfixFeature);
				update["output-tsv"].AddFeature("quotes", quotesFeature);
			update.AddFeature("output-xml", outputXmlFeature);
			update.AddFeature("filename", filenameFeature);
			update.AddFeature("name", nameFeature);
			update.AddFeature("desc", descriptionFeature);
			update.AddFeature("rootdir", rootFeature);
			update.AddFeature("category", categoryFeature);
			update.AddFeature("version", versionFeature);
			update.AddFeature("date", dateFeature);
			update.AddFeature("author", authorFeature);
			update.AddFeature("email", emailFeature);
			update.AddFeature("homepage", homepageFeature);
			update.AddFeature("url", urlFeature);
			update.AddFeature("comment", commentFeature);
			update.AddFeature("header", headerFeature);
			update.AddFeature("superdat", superdatFeature);
			update.AddFeature("forcemerge", forcemergeFeature);
			update.AddFeature("forcend", forcendFeature);
			update.AddFeature("forcepack", forcepackFeature);
			update.AddFeature("exclude-of", excludeOfFeature);
			update.AddFeature("scene-date-strip", sceneDateStripFeature);
			update.AddFeature("clean", cleanFeature);
			update.AddFeature("rem-uni", remUniFeature);
			update.AddFeature("rem-md5", remMd5Feature);
			update.AddFeature("rem-sha1", remSha1Feature);
			update.AddFeature("rem-sha256", remSha256Feature);
			update.AddFeature("rem-sha384", remSha384Feature);
			update.AddFeature("rem-sha512", remSha512Feature);
			update.AddFeature("desc-name", descNameFeature);
			update.AddFeature("dat-merged", datMergedFeature);
			update.AddFeature("dat-split", datSplitFeature);
			update.AddFeature("dat-nonmerged", datNonmergedFeature);
			update.AddFeature("dat-devnonmerged", datDevnonmergedFeature);
			update.AddFeature("dat-fullnonmerged", datFullnonmergedFeature);
			update.AddFeature("trim", trimFeature);
				update["trim"].AddFeature("root-dir", rootDirFeature);
			update.AddFeature("single", singleFeature);
			update.AddFeature("dedup", dedupFeature);
			update.AddFeature("game-dedup", gameDedupFeature);
			update.AddFeature("merge", mergeFeature);
				update["merge"].AddFeature("bare", bareFeature);
			update.AddFeature("diff", diffFeature);
				update["diff"].AddFeature("against", againstFeature);
					update["diff"]["against"].AddFeature("base-dat", baseDatFeature);
				update["diff"].AddFeature("bare", bareFeature);
				update["diff"].AddFeature("cascade", cascadeFeature);
					update["diff"]["cascade"].AddFeature("skip", skipFeature);
				update["diff"].AddFeature("rev-cascade", revCascadeFeature);
					update["diff"]["rev-cascade"].AddFeature("skip", skipFeature);
			update.AddFeature("diff-du", diffDuFeature);
				update["diff-du"].AddFeature("bare", bareFeature);
			update.AddFeature("diff-in", diffInFeature);
				update["diff-in"].AddFeature("bare", bareFeature);
			update.AddFeature("diff-nd", diffNdFeature);
				update["diff-nd"].AddFeature("bare", bareFeature);
			update.AddFeature("base-replace", baseReplaceFeature);
				update["base-replace"].AddFeature("base-dat", baseDatFeature);
				update["base-replace"].AddFeature("update-names", updateNamesFeature);
				update["base-replace"].AddFeature("update-hashes", updateHashesFeature);
				update["base-replace"].AddFeature("update-desc", updateDescFeature);
				update["base-replace"].AddFeature("update-year", updateYearFeature);
				update["base-replace"].AddFeature("update-manu", updateManuFeature);
			update.AddFeature("reverse-base-replace", reverseBaseReplaceFeature);
				update["reverse-base-replace"].AddFeature("base-dat", baseDatFeature);
				update["reverse-base-replace"].AddFeature("update-names", updateNamesFeature);
				update["reverse-base-replace"].AddFeature("update-hashes", updateHashesFeature);
				update["reverse-base-replace"].AddFeature("update-desc", updateDescFeature);
				update["reverse-base-replace"].AddFeature("update-year", updateYearFeature);
				update["reverse-base-replace"].AddFeature("update-manu", updateManuFeature);
			update.AddFeature("game-name", gameNameFeature);
			update.AddFeature("not-game", notGameFeature);
			update.AddFeature("of-as-game", ofAsGameFeature);
			update.AddFeature("rom-name", romNameFeature);
			update.AddFeature("not-rom", notRomFeature);
			update.AddFeature("rom-type", romTypeFeature);
			update.AddFeature("not-type", notTypeFeature);
			update.AddFeature("greater", greaterFeature);
			update.AddFeature("less", lessFeature);
			update.AddFeature("equal", equalFeature);
			update.AddFeature("crc", crcFeature);
			update.AddFeature("not-crc", notCrcFeature);
			update.AddFeature("md5", md5Feature);
			update.AddFeature("not-md5", notMd5Feature);
			update.AddFeature("sha1", sha1Feature);
			update.AddFeature("not-sha1", notSha1Feature);
			update.AddFeature("sha256", sha256Feature);
			update.AddFeature("not-sha256", notSha256Feature);
			update.AddFeature("sha384", sha384Feature);
			update.AddFeature("not-sha384", notSha384Feature);
			update.AddFeature("sha512", sha512Feature);
			update.AddFeature("not-sha512", notSha512Feature);
			update.AddFeature("status", statusFeature);
			update.AddFeature("not-status", notStatusFeature);
			update.AddFeature("game-type", gameTypeFeature);
			update.AddFeature("not-gtype", notGtypeFeature);
			update.AddFeature("runnable", runnableFeature);
			update.AddFeature("not-run", notRunFeature);
			update.AddFeature("out", outFeature);
			update.AddFeature("inplace", inplaceFeature);
			update.AddFeature("mt", mtFeature);

			#endregion

			#region Verify

			Feature verify = new Feature(
				new List<string>() { "-ve", "--verify" },
				"Verify a folder against DATs",
				FeatureType.Flag,
				null);
			verify.AddFeature("dat", datFeature);
			verify.AddFeature("depot", depotFeature);
			verify.AddFeature("temp", tempFeature);
			verify.AddFeature("hash-only", hashOnlyFeature);
			verify.AddFeature("quick", quickFeature);
			verify.AddFeature("header", headerFeature);
			verify.AddFeature("ignore-chd", ignoreChdFeature);
			verify.AddFeature("dat-merged", datMergedFeature);
			verify.AddFeature("dat-split", datSplitFeature);
			verify.AddFeature("dat-nonmerged", datNonmergedFeature);
			verify.AddFeature("dat-fullnonmerged", datFullnonmergedFeature);

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
