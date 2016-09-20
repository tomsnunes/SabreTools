using System;
using System.Collections.Generic;

namespace SabreTools.Helper
{
	#region Dat-to-Hash structs

	/// <summary>
	/// Intermediate struct for holding and processing hash data
	/// </summary>
	public struct Hash : IEquatable<Hash>
	{
		public long Size;
		public string CRC;
		public string MD5;
		public string SHA1;

		public bool Equals(Hash other)
		{
			return this.Equals(other, false);
		}

		public bool Equals(Hash other, bool IsDisk)
		{
			bool equals = false;

			if (IsDisk &&
					((String.IsNullOrEmpty(this.MD5) || String.IsNullOrEmpty(other.MD5)) || this.MD5 == other.MD5) &&
					((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(other.SHA1)) || this.SHA1 == other.SHA1))
			{
				equals = true;
			}
			else if (!IsDisk &&
					(this.Size == other.Size) &&
					((String.IsNullOrEmpty(this.CRC) || String.IsNullOrEmpty(other.CRC)) || this.CRC == other.CRC) &&
					((String.IsNullOrEmpty(this.MD5) || String.IsNullOrEmpty(other.MD5)) || this.MD5 == other.MD5) &&
					((String.IsNullOrEmpty(this.SHA1) || String.IsNullOrEmpty(other.SHA1)) || this.SHA1 == other.SHA1))
			{
				equals = true;
			}

			return equals;
		}
	}

	#endregion

	#region Skipper structs

	/// <summary>
	/// Intermediate struct for holding header skipper information
	/// </summary>
	public struct Skipper
	{
		public string Name;
		public string Author;
		public string Version;
		public List<SkipperRule> Rules;
	}

	/// <summary>
	/// Intermediate struct for holding header skipper rule information
	/// </summary>
	public struct SkipperRule
	{
		public long? StartOffset; // null is EOF
		public long? EndOffset; // null if EOF
		public HeaderSkipOperation Operation;
		public List<SkipperTest> Tests;
	}

	/// <summary>
	/// Intermediate struct for holding header test information
	/// </summary>
	public struct SkipperTest
	{
		public HeaderSkipTest Type;
		public long? Offset; // null is EOF
		public byte[] Value;
		public bool Result;
		public byte[] Mask;
		public long? Size; // null is PO2, "power of 2" filesize
		public HeaderSkipTestFileOperator Operator;
	}

	#endregion
}
