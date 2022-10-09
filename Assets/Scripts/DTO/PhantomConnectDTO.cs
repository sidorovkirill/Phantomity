using System;
using Newtonsoft.Json;

namespace Phantom.DTO
{
	[Serializable]
	public class PhantomConnectDTO
	{
		[JsonProperty("public_key")]
		public string WalletPublicKey { get; private set; }
        
		[JsonProperty("session")]
		public string Session { get; private set; }
	}
}