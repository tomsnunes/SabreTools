using Ionic.Crc;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SabreTools.Helper
{
	/// <remarks>
	/// Based on work by GordonJ for RomVault
	/// https://github.com/gjefferyes/RomVault/blob/master/ROMVault2/SupportedFiles/Zip/zipFile.cs
	/// </remarks>
	public class ZipFileEntry
	{
		#region Private instance variables

		private readonly Stream _zipstream;
		private string _fileName;
		private CompressionMethod _compressionMethod;
		private ArchiveVersion _versionMadeBy;
		private ArchiveVersion _versionNeeded;
		private GeneralPurposeBitFlag _generalPurposeBitFlag;
		private ushort _lastModFileTime;
		private ushort _lastModFileDate;
		private uint _crc;
		private ulong _compressedSize;
		private ulong _uncompressedSize;
		private byte[] _extraField;
		private byte[] _comment;
		private InternalFileAttributes _internalFileAttributes;
		private uint _externalFileAttributes;
		private ulong _relativeOffset;
		private ulong _crc32Location;
		private ulong _extraLocation;
		private ulong _dataLocation;
		private bool _zip64;
		private bool _torrentZip;
		private byte[] _md5;
		private byte[] _sha1;

		#endregion

		#region Public facing variables

		public string FileName
		{
			get { return _fileName; }
			private set { _fileName = value; }
		}
		public GeneralPurposeBitFlag GeneralPurposeBitFlag
		{
			get { return _generalPurposeBitFlag; }
			private set { _generalPurposeBitFlag = value; }
		}
		public ushort LastModFileTime
		{
			get { return _lastModFileTime; }
			set { _lastModFileTime = value; }
		}
		public ushort LastModFileDate
		{
			get { return _lastModFileDate; }
			set { _lastModFileDate = value; }
		}
		public byte[] CRC
		{
			get { return BitConverter.GetBytes(_crc); }
			private set { _crc = BitConverter.ToUInt32(value, 0); }
		}
		public ulong UncompressedSize
		{
			get { return _uncompressedSize; }
			private set { _uncompressedSize = value; }
		}
		public string ExtraField
		{
			get { return Encoding.ASCII.GetString(_extraField); }
			set { _extraField = Style.StringToByteArray(Style.ConvertAsciiToHex(value)); }
		}
		public string Comment
		{
			get { return Encoding.ASCII.GetString(_comment); }
			set { _comment = Style.StringToByteArray(Style.ConvertAsciiToHex(value)); }
		}
		public ulong RelativeOffset
		{
			get { return _relativeOffset; }
			set { _relativeOffset = value; }
		}
		public bool Zip64
		{
			get { return _zip64; }
			private set { _zip64 = value; }
		}
		public bool TorrentZip
		{
			get { return _torrentZip; }
			private set { _torrentZip = value; }
		}
		public byte[] MD5
		{
			get { return _md5; }
			private set { _md5 = value; }
		}
		public byte[] SHA1
		{
			get { return _sha1; }
			private set { _sha1 = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a new ZipFileEntry using just a stream
		/// </summary>
		/// <param name="zipstream">Stream representing the entry</param>
		public ZipFileEntry(Stream zipstream)
		{
			_zipstream = zipstream;
		}

		/// <summary>
		/// Create a new ZipFileEntry from a stream and a filename
		/// </summary>
		/// <param name="zipstream">Stream representing the entry</param>
		/// <param name="filename">Internal filename to use</param>
		/// <param name="torrentZip">True if the file should be set with TorrentZip defaults, false otherwise (default)</param>
		public ZipFileEntry(Stream zipstream, string filename, bool torrentZip = false)
		{
			_zip64 = false;
			_zipstream = zipstream;
			_generalPurposeBitFlag = GeneralPurposeBitFlag.DeflatingMaximumCompression;
			_compressionMethod = CompressionMethod.Deflated;
			FileName = filename;

			if (torrentZip)
			{
				_lastModFileTime = 48128;
				_lastModFileDate = 8600;
			}
		}

		/// <summary>
		/// Read the central directory entry from the input stream
		/// </summary>
		/// <returns>True if the central directory was read correctly, false otherwise</returns>
		public bool ReadCentralDirectory()
		{
			try
			{
				// Open the stream for reading
				using (BinaryReader br = new BinaryReader(_zipstream))
				{
					// If the first bytes aren't a central directory header, log and return
					if (br.ReadUInt32() != Constants.CentralDirectoryHeaderSignature)
					{
						Console.Write("Error: Central directory entry malformed");
						return false;
					}

					// Now read in available information, skipping the unnecessary
					_versionMadeBy = (ArchiveVersion)br.ReadUInt16();
					_versionNeeded = (ArchiveVersion)br.ReadUInt16();
					_generalPurposeBitFlag = (GeneralPurposeBitFlag)br.ReadUInt16();
					_compressionMethod = (CompressionMethod)br.ReadUInt16();

					// If we have an unsupported compression method, log and return
					if (_compressionMethod != CompressionMethod.Stored && _compressionMethod != CompressionMethod.Deflated)
					{
						Console.Write("Error: Unsupported compression method; requires store or deflate");
						return false;
					}

					// Keep reading available information, skipping the unnecessary
					_lastModFileTime = br.ReadUInt16();
					_lastModFileDate = br.ReadUInt16();
					_crc = br.ReadUInt32();
					_compressedSize = br.ReadUInt32();
					_uncompressedSize = br.ReadUInt32();

					// Now store some temp vars to find the filename, extra field, and comment
					ushort fileNameLength = br.ReadUInt16();
					ushort extraFieldLength = br.ReadUInt16();
					ushort fileCommentLength = br.ReadUInt16();

					// Even more reading available information, skipping the unnecessary
					br.ReadUInt16(); // Disk number start
					_internalFileAttributes = (InternalFileAttributes)br.ReadUInt16();
					_externalFileAttributes = br.ReadUInt16();
					_relativeOffset = br.ReadUInt32();
					byte[] fileNameBytes = br.ReadBytes(fileNameLength);
					_fileName = ((_generalPurposeBitFlag & GeneralPurposeBitFlag.LanguageEncodingFlag) == 0
						? Encoding.ASCII.GetString(fileNameBytes)
						: Encoding.UTF8.GetString(fileNameBytes, 0, fileNameLength));
					_extraField = br.ReadBytes(extraFieldLength);
					_comment = br.ReadBytes(fileCommentLength);

					/*
					Full disclosure: this next section is in GordonJ's work but I honestly
					have no idea everything that it does. It seems to do something to figure
					out if it's Zip64, or possibly check for random things but it uses the
					extra field for this, which I do not fully understand. It's copied in
					its entirety below in the hope that it makes things better...
					*/

					int pos = 0;
					while (extraFieldLength > pos)
					{
						ushort type = BitConverter.ToUInt16(_extraField, pos);
						pos += 2;
						ushort blockLength = BitConverter.ToUInt16(_extraField, pos);
						pos += 2;
						switch (type)
						{
							case 0x0001:
								Zip64 = true;
								if (UncompressedSize == 0xffffffff)
								{
									UncompressedSize = BitConverter.ToUInt64(_extraField, pos);
									pos += 8;
								}
								if (_compressedSize == 0xffffffff)
								{
									_compressedSize = BitConverter.ToUInt64(_extraField, pos);
									pos += 8;
								}
								if (_relativeOffset == 0xffffffff)
								{
									_relativeOffset = BitConverter.ToUInt64(_extraField, pos);
									pos += 8;
								}
								break;
							case 0x7075:
								//byte version = extraField[pos];
								pos += 1;
								uint nameCRC32 = BitConverter.ToUInt32(_extraField, pos);
								pos += 4;

								Ionic.Crc.CRC32 crcTest = new Ionic.Crc.CRC32();
								crcTest.SlurpBlock(fileNameBytes, 0, fileNameLength);
								uint fCRC = (uint)crcTest.Crc32Result;

								if (nameCRC32 != fCRC)
								{
									Console.Write("Error: Central directory entry malformed");
									return false;
								}

								int charLen = blockLength - 5;

								_fileName = Encoding.UTF8.GetString(_extraField, pos, charLen);
								pos += charLen;

								break;
							default:
								pos += blockLength;
								break;
						}
					}
				}
			}
			catch
			{
				Console.Write("Error: Central directory entry malformed");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write the central directory entry from the included stream
		/// </summary>
		/// <param name="output">Write out the data from the internal stream to the output stream</param>
		public void WriteCentralDirectory(Stream output)
		{
			// Open the crcStream for writing
			using (BinaryWriter bw = new BinaryWriter(output))
			{
				// Create an empty extra field to start out with
				List<byte> extraField = new List<byte>();

				// Now get the uncompressed size (for Zip64 compatibility)
				uint uncompressedSize32;
				if (_uncompressedSize >= 0xffffffff)
				{
					_zip64 = true;
					uncompressedSize32 = 0xffffffff;
					extraField.AddRange(BitConverter.GetBytes(_uncompressedSize));
				}
				else
				{
					uncompressedSize32 = (uint)_uncompressedSize;
				}

				// Now get the compressed size (for Zip64 compatibility)
				uint compressedSize32;
				if (_compressedSize >= 0xffffffff)
				{
					_zip64 = true;
					compressedSize32 = 0xffffffff;
					extraField.AddRange(BitConverter.GetBytes(_compressedSize));
				}
				else
				{
					compressedSize32 = (uint)_compressedSize;
				}

				// Now get the relative offset (for Zip64 compatibility)
				uint relativeOffset32;
				if (_relativeOffset >= 0xffffffff)
				{
					_zip64 = true;
					relativeOffset32 = 0xffffffff;
					extraField.AddRange(BitConverter.GetBytes(_relativeOffset));
				}
				else
				{
					relativeOffset32 = (uint)_relativeOffset;
				}

				// If we wrote anything to the extra field, set the flag and size
				if (extraField.Count > 0)
				{
					ushort extraFieldLengthInternal = (ushort)extraField.Count;
					extraField.InsertRange(0, BitConverter.GetBytes((ushort)0x0001));
					extraField.InsertRange(2, BitConverter.GetBytes(extraFieldLengthInternal));
				}
				ushort extraFieldLength = (ushort)extraField.Count;

				// Now check for a unicode filename and set the flag accordingly
				byte[] fileNameBytes;
				if (Style.IsUnicode(_fileName))
				{
					_generalPurposeBitFlag |= GeneralPurposeBitFlag.LanguageEncodingFlag;
					fileNameBytes = Encoding.UTF8.GetBytes(_fileName);
				}
				else
				{
					fileNameBytes = Encoding.ASCII.GetBytes(_fileName);
				}
				ushort fileNameLength = (ushort)fileNameBytes.Length;

				// Set the version needed to extract according to if it's Zip64
				ushort versionNeededToExtract = (ushort)(_zip64 ? 45 : 20);

				// Now, write all of the data to the stream
				bw.Write(Constants.CentralDirectoryHeaderSignature);
				bw.Write((ushort)ArchiveVersion.MSDOSandOS2);
				bw.Write(versionNeededToExtract);
				bw.Write((ushort)_generalPurposeBitFlag);
				bw.Write((ushort)_compressionMethod);
				bw.Write(_lastModFileTime);
				bw.Write(_lastModFileDate);
				bw.Write(_crc);
				bw.Write(compressedSize32);
				bw.Write(uncompressedSize32);
				bw.Write(fileNameLength);
				bw.Write(extraFieldLength);
				bw.Write((ushort)0); // File comment length
				bw.Write((ushort)0); // Disk number start
				bw.Write((ushort)0); // Internal file attributes
				bw.Write((uint)0); // External file attributes
				bw.Write(relativeOffset32);
				bw.Write(fileNameBytes, 0, fileNameLength); // Only write first bytes if longer than allowed
				bw.Write(extraField.ToArray(), 0, extraFieldLength); // Only write the first bytes if longer than allowed
				// We have no file comment, so we don't have to write more
			}
		}

		/// <summary>
		/// Read the local file header from the input stream
		/// </summary>
		/// <returns>True if the local file header was read correctly, false otherwise</returns>
		public bool ReadLocalFileHeader()
		{
			try
			{
				// We assume that the file is torrentzip until proven otherwise
				_torrentZip = true;

				// Open the stream for reading
				using (BinaryReader br = new BinaryReader(_zipstream))
				{
					// Set the position of the writer based on the entry information
					br.BaseStream.Seek((long)_relativeOffset, SeekOrigin.Begin);

					// If the first bytes aren't a local file header, log and return
					if (br.ReadUInt32() != Constants.LocalFileHeaderSignature)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}

					// Now read in available information, comparing to the known data
					if (br.ReadUInt16() != (ushort)_versionNeeded)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					if (br.ReadUInt16() != (ushort)_generalPurposeBitFlag)
					{
						_torrentZip = false;
					}
					if (br.ReadUInt16() != (ushort)_compressionMethod)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					if (br.ReadUInt16() != _lastModFileTime)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					if (br.ReadUInt16() != _lastModFileDate)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					if ((_generalPurposeBitFlag & GeneralPurposeBitFlag.ZeroedCRCAndSize) == 0 && br.ReadUInt32() != _crc)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}

					uint readCompressedSize = br.ReadUInt32();
					// If we have Zip64, the compressed size should be 0xffffffff
					if (_zip64 && readCompressedSize != 0xffffffff && readCompressedSize != _compressedSize)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					// If we have the zeroed flag set, then no size should be included
					if ((_generalPurposeBitFlag & GeneralPurposeBitFlag.ZeroedCRCAndSize) == GeneralPurposeBitFlag.ZeroedCRCAndSize && readCompressedSize != 0)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					// If we don't have the zeroed flag set, then the size should match
					if ((_generalPurposeBitFlag & GeneralPurposeBitFlag.ZeroedCRCAndSize) == 0 && readCompressedSize != _compressedSize)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}

					uint readUncompressedSize = br.ReadUInt32();
					// If we have Zip64, the uncompressed size should be 0xffffffff
					if (_zip64 && readUncompressedSize != 0xffffffff && readUncompressedSize != _compressedSize)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					// If we have the zeroed flag set, then no size should be included
					if ((_generalPurposeBitFlag & GeneralPurposeBitFlag.ZeroedCRCAndSize) == GeneralPurposeBitFlag.ZeroedCRCAndSize && readUncompressedSize != 0)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					// If we don't have the zeroed flag set, then the size should match
					if ((_generalPurposeBitFlag & GeneralPurposeBitFlag.ZeroedCRCAndSize) == 0 && readUncompressedSize != _uncompressedSize)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}

					ushort fileNameLength = br.ReadUInt16();
					ushort extraFieldLength = br.ReadUInt16();

					byte[] fileNameBytes = br.ReadBytes(fileNameLength);
					string tempFileName = ((_generalPurposeBitFlag & GeneralPurposeBitFlag.LanguageEncodingFlag) == 0
						? Encoding.ASCII.GetString(fileNameBytes)
						: Encoding.UTF8.GetString(fileNameBytes, 0, fileNameLength));

					byte[] extraField = br.ReadBytes(extraFieldLength);

					/*
					Full disclosure: this next section is in GordonJ's work but I honestly
					have no idea everything that it does. It seems to do something to figure
					out if it's Zip64, or possibly check for random things but it uses the
					extra field for this, which I do not fully understand. It's copied in
					its entirety below in the hope that it makes things better...
					*/

					_zip64 = false;
					int pos = 0;
					while (extraFieldLength > pos)
					{
						ushort type = BitConverter.ToUInt16(extraField, pos);
						pos += 2;
						ushort blockLength = BitConverter.ToUInt16(extraField, pos);
						pos += 2;
						switch (type)
						{
							case 0x0001:
								Zip64 = true;
								if (readUncompressedSize == 0xffffffff)
								{
									ulong tLong = BitConverter.ToUInt64(extraField, pos);
									if (tLong != UncompressedSize)
									{
										Console.Write("Error: Local file header malformed");
										return false;
									}
									pos += 8;
								}
								if (readCompressedSize == 0xffffffff)
								{
									ulong tLong = BitConverter.ToUInt64(extraField, pos);
									if (tLong != _compressedSize)
									{
										Console.Write("Error: Local file header malformed");
										return false;
									}
									pos += 8;
								}
								break;
							case 0x7075:
								//byte version = extraField[pos];
								pos += 1;
								uint nameCRC32 = BitConverter.ToUInt32(extraField, pos);
								pos += 4;

								Ionic.Crc.CRC32 crcTest = new Ionic.Crc.CRC32();
								crcTest.SlurpBlock(fileNameBytes, 0, fileNameLength);
								uint fCRC = (uint)crcTest.Crc32Result;

								if (nameCRC32 != fCRC)
								{
									Console.Write("Error: Local file header malformed");
									return false;
								}

								int charLen = blockLength - 5;

								tempFileName = Encoding.UTF8.GetString(extraField, pos, charLen);
								pos += charLen;

								break;
							default:
								pos += blockLength;
								break;
						}
					}

					// Back to code I understand
					if (String.Equals(_fileName, tempFileName, StringComparison.InvariantCulture))
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}

					// Set the position of the data
					_dataLocation = (ulong)_zipstream.Position;

					// Now if no other data should be after the data, return
					if((_generalPurposeBitFlag & GeneralPurposeBitFlag.ZeroedCRCAndSize) == GeneralPurposeBitFlag.ZeroedCRCAndSize)
					{
						return true;
					}

					// Otherwise, compare the data after the file too
					_zipstream.Seek((long)_compressedSize, SeekOrigin.Current);

					// If there's no subheader, read the next thing as crc
					uint tempCrc = br.ReadUInt32();
					if (tempCrc != Constants.EndOfLocalFileHeaderSignature)
					{
						tempCrc = br.ReadUInt32();
					}

					if (tempCrc != _crc)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					if (br.ReadUInt32() != _compressedSize)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
					if (br.ReadUInt32() != _uncompressedSize)
					{
						Console.Write("Error: Local file header malformed");
						return false;
					}
				}
			}
			catch
			{
				Console.Write("Error: Local file header malformed");
				return false;
			}

			return true;
		}

		#endregion
	}
}
