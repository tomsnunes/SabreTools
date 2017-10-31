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
	public class CHDFile
	{
		// core parameters from the header
		private uint m_version;          // version of the header
		private ulong m_logicalbytes;    // logical size of the raw CHD data in bytes
		private ulong m_mapoffset;       // offset of map
		private ulong m_metaoffset;      // offset to first metadata bit
		private uint m_hunkbytes;        // size of each raw hunk in bytes
		private ulong m_hunkcount;       // number of hunks represented
		private uint m_unitbytes;        // size of each unit in bytes
		private ulong m_unitcount;       // number of units represented
		private CHDCodecType[] m_compression = new CHDCodecType[4];   // array of compression types used

		// key offsets within the header
		private long m_mapoffset_offset; // offset of map offset field
		private long m_metaoffset_offset;// offset of metaoffset field
		private long m_sha1_offset;      // offset of SHA1 field
		private long m_rawsha1_offset;   // offset of raw SHA1 field
		private long m_parentsha1_offset;// offset of paren SHA1 field

		// map information
		uint m_mapentrybytes;            // length of each entry in a map

		/// <summary>
		/// Get internal metadata from a CHD
		/// </summary>
		/// <param name="filename">Filename of possible CHD</param>
		/// <returns>A Disk object with internal SHA-1 on success, null on error, empty Disk otherwise</returns>
		/// <remarks>
		/// Original code had a "writable" param. This is not required for metadata checking
		/// </remarks>
		public DatItem GetCHDInfo(string filename)
		{
			FileStream fs = FileTools.TryOpenRead(filename);
			DatItem datItem = GetCHDInfo(fs);
			fs.Dispose();
			return datItem;
		}

		/// <summary>
		/// Get internal metadata from a CHD
		/// </summary>
		/// <param name="fs">FileStreams of possible CHD</param>
		/// <returns>A Disk object with internal SHA-1 on success, null on error, empty Disk otherwise</returns>
		/// <remarks>
		/// Original code had a "writable" param. This is not required for metadata checking
		/// </remarks>
		public DatItem GetCHDInfo(FileStream fs)
		{
			// Create a blank Disk to populate and return
			Disk datItem = new Disk();

			// Get a binary reader to make life easier
			BinaryReader br = new BinaryReader(fs);

			// Read and verify the CHD signature
			// read the raw header
			byte[] signature = br.ReadBytes(8); 

			// verify the signature
			bool correct = true;
			for (int i = 0; i < signature.Length; i++)
			{
				correct &= (signature[i] == Constants.CHDSignature[i]);
			}
			if (!correct)
			{
				// throw CHDERR_INVALID_FILE;
				return null;
			}

			// only allow writes to the most recent version
			br.BaseStream.Seek(12, SeekOrigin.Begin);
			m_version = br.ReadUInt32();

			// read the header if we support it
			byte[] sha1 = new byte[20];
			switch (m_version)
			{
				case 3:
					ParseV3Header(br, out sha1);
					break;
				case 4:
					ParseV4Header(br, out sha1);
					break;
				case 5:
					ParseV5Header(br, out sha1);
					break;
				default:
					// throw CHDERR_UNSUPPORTED_VERSION;
					return null;
			}

			datItem.SHA1 = BitConverter.ToString(sha1).Replace("-", string.Empty).ToLowerInvariant();

			return datItem;
		}

		/// <summary>
		/// Parse a CHD v3 header, populate the fields, and return the SHA-1
		/// </summary>
		/// <param name="br">BinaryReader representing the file to read</param>
		/// <param name="sha1">Out parameter representing the SHA-1</param>
		/// <returns>True if the header was parsed properly, false otherwise</returns>
		/// <remarks>
		/// CHD v3 header layout:
		/// 0x00-0x07 - CHD signature
		/// 0x08-0x0B - Header size
		/// 0x0C-0x0F - [UNUSED]
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
		/// </remarks>
		private bool ParseV3Header(BinaryReader br, out byte[] sha1)
		{
			// Set the blank SHA-1 hash
			sha1 = null;

			// Set offsets and defaults
			m_mapoffset_offset = 0;
			m_rawsha1_offset = 0;
			m_mapoffset = 120;
			m_metaoffset_offset = 36;
			m_sha1_offset = 80;
			m_parentsha1_offset = 100;
			m_mapentrybytes = 16;

			// Ensure the proper starting position
			br.BaseStream.Seek(8, SeekOrigin.Begin);

			// Verify header length
			if (br.ReadInt32() != Constants.CHD_V3_HEADER_SIZE)
			{
				// throw CHDERR_INVALID_FILE;
				return false;
			}

			// Skip over the 0x0C-0x0F block
			br.ReadUInt32(); 

			// Read the CHD flags
			uint flags = br.ReadUInt32();

			// Determine compression
			switch (br.ReadUInt32())
			{
				case 0: m_compression[0] = CHDCodecType.CHD_CODEC_NONE; break;
				case 1: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 2: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 3: m_compression[0] = CHDCodecType.CHD_CODEC_AVHUFF; break;
				default: /* throw CHDERR_UNKNOWN_COMPRESSION; */ return false;
			}

			m_compression[1] = m_compression[2] = m_compression[3] = CHDCodecType.CHD_CODEC_NONE;

			m_hunkcount = br.ReadUInt32();
			m_logicalbytes = br.ReadUInt64();
			m_metaoffset = br.ReadUInt64();

			br.BaseStream.Seek(76, SeekOrigin.Begin);
			m_hunkbytes = br.ReadUInt32();

			br.BaseStream.Seek(m_sha1_offset, SeekOrigin.Begin);
			sha1 = br.ReadBytes(20);

			// guess at the units based on snooping the metadata
			// m_unitbytes = guess_unitbytes();
			m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;

			return true;
		}

		/// <summary>
		/// Parse a CHD v4 header, populate the fields, and return the SHA-1
		/// </summary>
		/// <param name="br">BinaryReader representing the file to read</param>
		/// <param name="sha1">Out parameter representing the SHA-1</param>
		/// <returns>True if the header was parsed properly, false otherwise</returns>
		/// <remarks>
		/// CHD v4 header layout:
		/// 0x00-0x07 - CHD signature
		/// 0x08-0x0B - Header size
		/// 0x0C-0x0F - [UNUSED]
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
		/// </remarks>
		private bool ParseV4Header(BinaryReader br, out byte[] sha1)
		{
			// Set the blank SHA-1 hash
			sha1 = null;

			// Set offsets and defaults
			m_mapoffset_offset = 0;
			m_metaoffset_offset = 36;
			m_sha1_offset = 48;
			m_parentsha1_offset = 68;
			m_rawsha1_offset = 88;
			m_mapoffset = 108;
			m_mapentrybytes = 16;

			// Ensure the proper starting position
			br.BaseStream.Seek(8, SeekOrigin.Begin);

			// Verify header length
			if (br.ReadUInt32() != Constants.CHD_V4_HEADER_SIZE)
			{
				// throw CHDERR_INVALID_FILE;
				return false;
			}

			// Read the CHD flags
			uint flags = br.ReadUInt32();

			// Determine compression
			switch (br.ReadUInt32())
			{
				case 0: m_compression[0] = CHDCodecType.CHD_CODEC_NONE; break;
				case 1: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 2: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 3: m_compression[0] = CHDCodecType.CHD_CODEC_AVHUFF; break;
				default: /* throw CHDERR_UNKNOWN_COMPRESSION; */ return false;
			}

			m_compression[1] = m_compression[2] = m_compression[3] = CHDCodecType.CHD_CODEC_NONE;

			m_hunkcount = br.ReadUInt32();
			m_logicalbytes = br.ReadUInt64();
			m_metaoffset = br.ReadUInt64();

			br.BaseStream.Seek(44, SeekOrigin.Begin);
			m_hunkbytes = br.ReadUInt32();
			
			br.BaseStream.Seek(m_sha1_offset, SeekOrigin.Begin);
			sha1 = br.ReadBytes(20);

			// guess at the units based on snooping the metadata
			// m_unitbytes = guess_unitbytes();
			m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;

			return true;
		}

		/// <summary>
		/// Parse a CHD v5 header, populate the fields, and return the SHA-1
		/// </summary>
		/// <param name="br">BinaryReader representing the file to read</param>
		/// <param name="sha1">Out parameter representing the SHA-1</param>
		/// <returns>True if the header was parsed properly, false otherwise</returns>
		/// <remarks>
		/// CHD v5 header layout:
		/// 0x00-0x07 - CHD signature
		/// 0x08-0x0B - Header size
		/// 0x0C-0x0F - [UNUSED]
		/// 0x10-0x13 - Compression format 1
		/// 0x14-0x17 - Compression format 2
		/// 0x18-0x1B - Compression format 3
		/// 0x1C-0x1F - Compression format 4
		/// 0x18-0x1B - Hunk count
		/// 0x20-0x27 - Logical Bytes
		/// 0x28-0x2F - Map Offset
		/// 0x30-0x37 - Metadata Offset
		/// 0x38-0x3B - Hunk Bytes
		/// 0x3C-0x3F - Unit Bytes
		/// 0x40-0x53 - Raw SHA-1
		/// 0x54-0x67 - SHA-1
		/// 0x68-0x7b - Parent SHA-1
		/// </remarks>
		private bool ParseV5Header(BinaryReader br, out byte[] sha1)
		{
			// Set the blank SHA-1 hash
			sha1 = null;

			// Set offsets and defaults
			m_mapoffset_offset = 40;
			m_metaoffset_offset = 48;
			m_rawsha1_offset = 64;
			m_sha1_offset = 84;
			m_parentsha1_offset = 104;

			// Ensure the proper starting position
			br.BaseStream.Seek(8, SeekOrigin.Begin);

			// Verify header length
			if (br.ReadUInt32() != Constants.CHD_V5_HEADER_SIZE)
			{
				// throw CHDERR_INVALID_FILE
				return false;
			}

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

			br.BaseStream.Seek(m_sha1_offset, SeekOrigin.Begin);
			sha1 = br.ReadBytes(20);

			return true;
		}
	}
}
