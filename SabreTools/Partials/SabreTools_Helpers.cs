using System;

using SabreTools.Helper.Data;

namespace SabreTools
{
	public partial class SabreTools
	{
		#region Helper methods

		/// <summary>
		/// Get the multiplier to be used with the size given
		/// </summary>
		/// <param name="sizestring">String with possible size with extension</param>
		/// <returns>Tuple of multiplier to use on final size and fixed size string</returns>
		private static long GetSizeFromString(string sizestring)
		{
			long size = 0;

			// Make sure the string is in lower case
			sizestring = sizestring.ToLowerInvariant();

			// Get any trailing size identifiers
			long multiplier = 1;
			if (sizestring.EndsWith("k") || sizestring.EndsWith("kb"))
			{
				multiplier = Constants.KiloByte;
			}
			else if (sizestring.EndsWith("ki") || sizestring.EndsWith("kib"))
			{
				multiplier = Constants.KibiByte;
			}
			else if (sizestring.EndsWith("m") || sizestring.EndsWith("mb"))
			{
				multiplier = Constants.MegaByte;
			}
			else if (sizestring.EndsWith("mi") || sizestring.EndsWith("mib"))
			{
				multiplier = Constants.MibiByte;
			}
			else if (sizestring.EndsWith("g") || sizestring.EndsWith("gb"))
			{
				multiplier = Constants.GigaByte;
			}
			else if (sizestring.EndsWith("gi") || sizestring.EndsWith("gib"))
			{
				multiplier = Constants.GibiByte;
			}
			else if (sizestring.EndsWith("t") || sizestring.EndsWith("tb"))
			{
				multiplier = Constants.TeraByte;
			}
			else if (sizestring.EndsWith("ti") || sizestring.EndsWith("tib"))
			{
				multiplier = Constants.TibiByte;
			}
			else if (sizestring.EndsWith("p") || sizestring.EndsWith("pb"))
			{
				multiplier = Constants.PetaByte;
			}
			else if (sizestring.EndsWith("pi") || sizestring.EndsWith("pib"))
			{
				multiplier = Constants.PibiByte;
			}

			// Remove any trailing identifiers
			sizestring = sizestring.TrimEnd(new char[] { 'k', 'm', 'g', 't', 'p', 'i', 'b', ' ' });

			// Now try to get the size from the string
			if (!Int64.TryParse(sizestring, out size))
			{
				size = -1;
			}
			else
			{
				size *= multiplier;
			}

			return size;
		}

		#endregion
	}
}
