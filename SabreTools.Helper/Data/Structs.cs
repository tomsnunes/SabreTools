using System;
using System.Collections.Generic;

namespace SabreTools.Helper
{
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
