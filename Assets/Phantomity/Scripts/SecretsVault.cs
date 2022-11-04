using System.Collections.Generic;
using System.Security.Cryptography;
using Chaos.NaCl;
using Newtonsoft.Json;
using Phantomity.Utils;

namespace Phantomity
{
	public class SecretsVault
	{
		private byte[] _publicKey;
		private byte[] _privateKey;
		private byte[] _sharedSecret;

		public string PublicKey
		{
			get { return Cryptography.Encode(_publicKey); }
		}

		public SecretsVault()
		{
			GenerateKeypair();
		}

		public void GenerateSecret(byte[] handshakePublicKey)
		{
			_sharedSecret = MontgomeryCurve25519.KeyExchange(handshakePublicKey, _privateKey);
		}

		public string DecryptPayload(string payload, byte[] nonce)
		{
			var encodedData = Cryptography.Decode(payload);
			var decryptedData = XSalsa20Poly1305.TryDecrypt(encodedData, _sharedSecret, nonce);
			string decryptedStr = System.Text.Encoding.UTF8.GetString(decryptedData);
			
			return decryptedStr;
		}

		public string EncryptPayload(string payload, byte[] nonce)
		{
			var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

			var encryptedBytes = XSalsa20Poly1305.Encrypt(payloadBytes, _sharedSecret, nonce);
			return Cryptography.Encode(encryptedBytes);
		}

		private void GenerateKeypair()
		{
			_privateKey = GetRandomBytes(32);
			_publicKey = MontgomeryCurve25519.GetPublicKey(_privateKey);
		}
		
		public static byte[] GetNonce()
		{
			return GetRandomBytes(24);
		}
		
		private static byte[] GetRandomBytes(int length)
		{
			var randomBytes = new byte[length];
			var rnd = new RNGCryptoServiceProvider();
			rnd.GetBytes(randomBytes);

			return randomBytes;
		}
	}
}