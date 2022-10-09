using System;
using Newtonsoft.Json;

namespace Phantom.DTO
{
	[Serializable]
	public class SendOptions
	{
		[JsonProperty(PropertyName = "maxRetries", NullValueHandling = NullValueHandling.Ignore)]
		public int? MaxRetries;
		
		[JsonProperty(PropertyName = "minContextSlot", NullValueHandling = NullValueHandling.Ignore)]
		public int? MinContextSlot;
		
		[JsonProperty(PropertyName = "preflightCommitment", NullValueHandling = NullValueHandling.Ignore)]
		public string PreflightCommitment;
		
		[JsonProperty(PropertyName = "skipPreflight", NullValueHandling = NullValueHandling.Ignore)]
		public bool? SkipPreflight;
	}
}