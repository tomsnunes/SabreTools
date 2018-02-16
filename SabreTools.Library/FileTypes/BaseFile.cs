using System.Collections.Generic;

using SabreTools.Library.Data;

namespace SabreTools.Library.FileTypes
{
	public abstract class BaseFile
	{
		#region Protected instance variables

		protected FileType _fileType;
		protected string _filename;
		protected List<BaseFile> _children;

		// External hash values for the file
		protected long? _size;
		protected byte[] _crc;
		protected byte[] _md5;
		protected byte[] _sha1;
		protected byte[] _sha256;
		protected byte[] _sha384;
		protected byte[] _sha512;

		#endregion

		#region Publicly facing variables

		// TODO: Get all of these values automatically so there is no public "set"
		public string Filename
		{
			get { return _filename; }
			set { _filename = value; }
		}
		public long? Size
		{
			get { return _size; }
			set { _size = value; }
		}
		public byte[] CRC
		{
			get { return _crc; }
			set { _crc = value; }
		}
		public byte[] MD5
		{
			get { return _md5; }
			set { _md5 = value; }
		}
		public byte[] SHA1
		{
			get { return _sha1; }
			set { _sha1 = value; }
		}
		public byte[] SHA256
		{
			get { return _sha256; }
			set { _sha256 = value; }
		}
		public byte[] SHA384
		{
			get { return _sha384; }
			set { _sha384 = value; }
		}
		public byte[] SHA512
		{
			get { return _sha512; }
			set { _sha512 = value; }
		}

		#endregion

		#region Construtors

		/// <summary>
		/// Create a new Archive with no base file
		/// </summary>
		public BaseFile()
		{
		}

		/// <summary>
		/// Create a new Archive from the given file
		/// </summary>
		/// <param name="filename">Name of the file to use as an archive</param>
		public BaseFile(string filename)
		{
			_filename = filename;
		}

		#endregion
	}
}
