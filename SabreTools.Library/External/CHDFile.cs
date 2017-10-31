using System;

using SabreTools.Library.Data;
using SabreTools.Library.Items;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using BinaryReader = System.IO.BinaryReader;
using FileStream = System.IO.FileStream;
using SeekOrigin = System.IO.SeekOrigin;
#endif

namespace SabreTools.Library.External
{
	/// <summary>
	/// This is code adapted from chd.h and chd.cpp in MAME
	/// </summary>
	/// <remrks>
	/// ----------------------------------------------
	/// Common CHD Header:
	/// 0x00-0x07 - CHD signature
	/// 0x08-0x0B - Header size
	/// 0x0C-0x0F - CHD version
	/// ----------------------------------------------
	/// CHD v3 header layout:
	/// 0x10-0x13 - Flags (1: Has parent SHA-1, 2: Disallow writes)
	/// 0x14-0x17 - Compression
	/// 0x18-0x1B - Hunk count
	/// 0x1C-0x23 - Logical Bytes
	/// 0x24-0x2C - Metadata Offset
	/// ...
	/// 0x4C-0x4F - Hunk Bytes
	/// 0x50-0x63 - SHA-1
	/// 0x64-0x77 - Parent SHA-1
	/// 0x78-0x87 - Map
	/// ----------------------------------------------
	/// CHD v4 header layout:
	/// 0x10-0x13 - Flags (1: Has parent SHA-1, 2: Disallow writes)
	/// 0x14-0x17 - Compression
	/// 0x18-0x1B - Hunk count
	/// 0x1C-0x23 - Logical Bytes
	/// 0x24-0x2C - Metadata Offset
	/// ...
	/// 0x2C-0x2F - Hunk Bytes
	/// 0x30-0x43 - SHA-1
	/// 0x44-0x57 - Parent SHA-1
	/// 0x58-0x6b - Raw SHA-1
	/// 0x6c-0x7b - Map
	/// ----------------------------------------------
	/// CHD v5 header layout:
	/// 0x10-0x13 - Compression format 1
	/// 0x14-0x17 - Compression format 2
	/// 0x18-0x1B - Compression format 3
	/// 0x1C-0x1F - Compression format 4
	/// 0x20-0x27 - Logical Bytes
	/// 0x28-0x2F - Map Offset
	/// 0x30-0x37 - Metadata Offset
	/// 0x38-0x3B - Hunk Bytes
	/// 0x3C-0x3F - Unit Bytes
	/// 0x40-0x53 - Raw SHA-1
	/// 0x54-0x67 - SHA-1
	/// 0x68-0x7b - Parent SHA-1
	/// ----------------------------------------------
	/// </remrks>
	public class CHDFile
	{
		/// <summary>
		/// Information regarding the CHD, mostly unused
		/// </summary>
		private class CHD
		{
			// Core parameters from the header
			public ulong m_signature;       // signature
			public uint m_headersize;       // size of the header
			public uint m_version;          // version of the header
			public ulong m_logicalbytes;    // logical size of the raw CHD data in bytes
			public ulong m_mapoffset;       // offset of map
			public ulong m_metaoffset;      // offset to first metadata bit
			public uint m_hunkbytes;        // size of each raw hunk in bytes
			private ulong m_hunkcount;       // number of hunks represented
			public uint m_unitbytes;        // size of each unit in bytes
			public ulong m_unitcount;       // number of units represented
			public CHDCodecType[] m_compression = new CHDCodecType[4];   // array of compression types used

			// map information
			public uint m_mapentrybytes;            // length of each entry in a map

			/// <summary>
			/// Parse a CHD v3 header
			/// </summary>
			/// <param name="br">Binary reader representing the input stream</param>
			/// <returns>The extracted SHA-1 on success, null otherwise</returns>
			public byte[] ParseCHDv3Header(BinaryReader br)
			{
				// Set the blank SHA-1 hash
				byte[] sha1 = new byte[20];

				// Set offsets and defaults
				m_mapoffset = 120;
				m_mapentrybytes = 16;

				// Read the CHD flags
				uint flags = br.ReadUInt32();

				// Determine compression
				switch (br.ReadUInt32())
				{
					case 0: m_compression[0] = CHDCodecType.CHD_CODEC_NONE; break;
					case 1: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
					case 2: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
					case 3: m_compression[0] = CHDCodecType.CHD_CODEC_AVHUFF; break;
					default: /* throw CHDERR_UNKNOWN_COMPRESSION; */ return null;
				}

				m_compression[1] = m_compression[2] = m_compression[3] = CHDCodecType.CHD_CODEC_NONE;

				m_hunkcount = br.ReadUInt32();
				m_logicalbytes = br.ReadUInt64();
				m_metaoffset = br.ReadUInt64();

				br.BaseStream.Seek(76, SeekOrigin.Begin);
				m_hunkbytes = br.ReadUInt32();

				br.BaseStream.Seek(Constants.CHDv3SHA1Offset, SeekOrigin.Begin);
				sha1 = br.ReadBytes(20);

				// guess at the units based on snooping the metadata
				// m_unitbytes = guess_unitbytes();
				m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;

				return sha1;
			}

			/// <summary>
			/// Parse a CHD v4 header
			/// </summary>
			/// <param name="br">Binary reader representing the input stream</param>
			/// <returns>The extracted SHA-1 on success, null otherwise</returns>
			public byte[] ParseCHDv4Header(BinaryReader br)
			{
				// Set the blank SHA-1 hash
				byte[] sha1 = new byte[20];

				// Set offsets and defaults
				m_mapoffset = 108;
				m_mapentrybytes = 16;

				// Read the CHD flags
				uint flags = br.ReadUInt32();

				// Determine compression
				switch (br.ReadUInt32())
				{
					case 0: m_compression[0] = CHDCodecType.CHD_CODEC_NONE; break;
					case 1: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
					case 2: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
					case 3: m_compression[0] = CHDCodecType.CHD_CODEC_AVHUFF; break;
					default: /* throw CHDERR_UNKNOWN_COMPRESSION; */ return null;
				}

				m_compression[1] = m_compression[2] = m_compression[3] = CHDCodecType.CHD_CODEC_NONE;

				m_hunkcount = br.ReadUInt32();
				m_logicalbytes = br.ReadUInt64();
				m_metaoffset = br.ReadUInt64();

				br.BaseStream.Seek(44, SeekOrigin.Begin);
				m_hunkbytes = br.ReadUInt32();

				br.BaseStream.Seek(Constants.CHDv4SHA1Offset, SeekOrigin.Begin);
				sha1 = br.ReadBytes(20);

				// guess at the units based on snooping the metadata
				// m_unitbytes = guess_unitbytes();
				m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;
				return sha1;
			}

			/// <summary>
			/// Parse a CHD v5 header
			/// </summary>
			/// <param name="br">Binary reader representing the input stream</param>
			/// <returns>The extracted SHA-1 on success, null otherwise</returns>
			public byte[] ParseCHDv5Header(BinaryReader br)
			{
				// Set the blank SHA-1 hash
				byte[] sha1 = new byte[20];

				// Determine compression
				m_compression[0] = (CHDCodecType)br.ReadUInt32();
				m_compression[1] = (CHDCodecType)br.ReadUInt32();
				m_compression[2] = (CHDCodecType)br.ReadUInt32();
				m_compression[3] = (CHDCodecType)br.ReadUInt32();

				m_logicalbytes = br.ReadUInt64();
				m_mapoffset = br.ReadUInt64();
				m_metaoffset = br.ReadUInt64();
				m_hunkbytes = br.ReadUInt32();
				m_hunkcount = (m_logicalbytes + m_hunkbytes - 1) / m_hunkbytes;
				m_unitbytes = br.ReadUInt32();
				m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;

				// m_allow_writes = !compressed();

				// determine properties of map entries
				// m_mapentrybytes = compressed() ? 12 : 4;

				br.BaseStream.Seek(Constants.CHDv5SHA1Offset, SeekOrigin.Begin);
				sha1 = br.ReadBytes(20);
				return sha1;
			}
		}

		/// <summary>
		/// Get internal metadata from a CHD
		/// </summary>
		/// <param name="filename">Filename of possible CHD</param>
		/// <returns>A Disk object with internal SHA-1 on success, null on error, empty Disk otherwise</returns>
		/// <remarks>
		/// Original code had a "writable" param. This is not required for metadata checking
		/// </remarks>
		public static DatItem GetCHDInfo(string filename)
		{
			FileStream fs = FileTools.TryOpenRead(filename);
			DatItem datItem = GetCHDInfo(fs);
			fs.Dispose();
			return datItem;
		}

		/// <summary>
		/// Get internal metadata from a CHD
		/// </summary>
		/// <param name="fs">Stream of possible CHD</param>
		/// <returns>A Disk object with internal SHA-1 on success, null on error, empty Disk otherwise</returns>
		/// <remarks>
		/// Original code had a "writable" param. This is not required for metadata checking
		/// </remarks>
		public static DatItem GetCHDInfo(Stream fs)
		{
			// Create a blank Disk to populate and return
			Disk datItem = new Disk();

			// Get a CHD object to store the data
			CHD chd = new CHD();

			// Get a binary reader to make life easier
			BinaryReader br = new BinaryReader(fs);

			// Read and verify the CHD signature
			chd.m_signature = br.ReadUInt64(); 
			if (chd.m_signature != Constants.CHDSignature)
			{
				// throw CHDERR_INVALID_FILE;
				return null;
			}

			// Get the header size and version
			chd.m_headersize = br.ReadUInt32();
			chd.m_version = br.ReadUInt32();

			// Create a placeholder for the extracted SHA-1
			byte[] sha1 = new byte[20];

			// If we have a CHD v3 file, parse it accordingly
			if (chd.m_headersize == Constants.CHD_V3_HEADER_SIZE && chd.m_version == 3)
			{
				sha1 = chd.ParseCHDv3Header(br);
			}
			// If we have a CHD v4 file, parse it accordingly
			else if (chd.m_headersize == Constants.CHD_V4_HEADER_SIZE && chd.m_version == 4)
			{
				sha1 = chd.ParseCHDv4Header(br);
			}
			// If we have a CHD v5 file, parse it accordingly
			else if (chd.m_headersize == Constants.CHD_V5_HEADER_SIZE && chd.m_version == 5)
			{
				sha1 = chd.ParseCHDv5Header(br);
			}
			// If we don't have a valid combination, return null
			else
			{
				// throw CHDERR_UNSUPPORTED_VERSION;
				// throw CHDERR_INVALID_FILE;
				return null;
			}

			// Set the SHA-1 of the Disk to return
			datItem.SHA1 = BitConverter.ToString(sha1).Replace("-", string.Empty).ToLowerInvariant();

			return datItem;
		}

		/// <summary>
		/// Get if file is a valid CHD
		/// </summary>
		/// <param name="filename">Filename of possible CHD</param>
		/// <returns>True if a the file is a valid CHD, false otherwise</returns>
		public static bool IsValidCHD(string filename)
		{
			DatItem datItem = GetCHDInfo(filename);
			return datItem != null
				&& datItem.Type == ItemType.Disk 
				&& ((Disk)datItem).SHA1 != null;
		}

		/// <summary>
		/// Get if stream is a valid CHD
		/// </summary>
		/// <param name="fs">Stream of possible CHD</param>
		/// <returns>True if a the file is a valid CHD, false otherwise</returns>
		public static bool IsValidCHD(Stream fs)
		{
			DatItem datItem = GetCHDInfo(fs);
			return datItem != null
				&& datItem.Type == ItemType.Disk
				&& ((Disk)datItem).SHA1 != null;
		}
	}
}
