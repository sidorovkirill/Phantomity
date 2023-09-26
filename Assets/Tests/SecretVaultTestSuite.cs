using NUnit.Framework;
using Phantomity;
using Phantomity.Utils;

namespace Tests
{
    public class SecretVaultTestSuite
    {
        private SecretsVault _alice;
        private SecretsVault _bob;

        [SetUp]
        public void SetUp()
        {
            _alice = new SecretsVault();
            _bob = new SecretsVault();
        }
        
        [Test]
        public void EncryptAndDecryptData()
        {
            var originalMessage = "Hello World!";
            var bobsPublicKey = Cryptography.Decode(_bob.PublicKey);
            var alicePublicKey = Cryptography.Decode(_alice.PublicKey);
            
            _alice.GenerateSecret(bobsPublicKey);
            _bob.GenerateSecret(alicePublicKey);
            
            var nonce = SecretsVault.GetNonce();
            var encryptedMessage = _alice.EncryptPayload(originalMessage, nonce);
            var decryptedMessage = _bob.DecryptPayload(encryptedMessage, nonce);
            
            Assert.AreEqual(originalMessage, decryptedMessage);
        }
    }
}