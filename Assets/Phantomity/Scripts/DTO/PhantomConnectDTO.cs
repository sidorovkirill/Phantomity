using System;
using Newtonsoft.Json;

namespace Phantomity.DTO
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