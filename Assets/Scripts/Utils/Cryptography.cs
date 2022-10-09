namespace Phantom
{
	public static class Cryptography
	{
		public static string Encode(byte[] content)
		{
			return Base58Encoding.Encode(content);
		}

		public static byte[] Decode(string content)
		{
			return Base58Encoding.Decode(content);
		}
	}
}