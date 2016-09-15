using SabreTools.Helper;

namespace System.IO.Compression
{
	public class ZipArchiveEntry
	{
		public ArchiveVersion VersionMadeBy;
		public ArchiveVersion VersionNeeded;
		public GeneralPurposeBitFlag GeneralPurposeBitFlag;
		public CompressionMethod CompressionMethod;
		public uint CRC;
		public byte[] ExtraField;
		public byte[] Comment;
		public InternalFileAttributes InternalFileAttributes;
		public int ExternalFileAttributes;
		public int RelativeOffset;
	}
}
