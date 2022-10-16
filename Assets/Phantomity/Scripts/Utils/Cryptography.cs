using System.Collections.Generic;

namespace Phantomity.Utils
{
	public static class Cryptography
	{
		public static string Encode(byte[] content)
		{
			return Base58Encoding.Encode(content);
		}
		
		public static IEnumerable<string> Encode(byte[][] transactions)
		{
			foreach (var transaction in transactions)
			{
				yield return Encode(transaction);
			}
		}

		public static byte[] Decode(string content)
		{
			return Base58Encoding.Decode(content);
		}
	}
}