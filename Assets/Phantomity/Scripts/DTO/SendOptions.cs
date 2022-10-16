using System;
using Newtonsoft.Json;

namespace Phantomity.DTO
{
	/// <summary>
	/// Options for sending transactions
	/// </summary>
	[Serializable]
	public class SendOptions
	{
		/// <summary>
		/// Maximum number of times for the RPC node to retry sending the transaction to the leader.
		/// </summary>
		[JsonProperty(PropertyName = "maxRetries", NullValueHandling = NullValueHandling.Ignore)]
		public int? MaxRetries;
		
		/// <summary>
		/// The minimum slot that the request can be evaluated at.
		/// </summary>
		[JsonProperty(PropertyName = "minContextSlot", NullValueHandling = NullValueHandling.Ignore)]
		public int? MinContextSlot;
		
		/// <summary>
		/// Preflight commitment level.
		/// </summary>
		/// <value>
		/// Level of <see cref="Commitment"/> desired when querying state:
		/// <para>
		/// <c>Processed</c>: Query the most recent block which has reached 1 confirmation by the connected node.<br/>
		/// <c>Confirmed</c>: Query the most recent block which has reached 1 confirmation by the cluster.<br/>
		/// <c>Finalized</c>: Query the most recent block which has been finalized by the cluster.<br/>
		/// </para>
		/// </value>
		[JsonProperty(PropertyName = "preflightCommitment", NullValueHandling = NullValueHandling.Ignore)]
		public string PreflightCommitment;
		
		/// <summary>
		/// Disable transaction verification step.
		/// </summary>
		[JsonProperty(PropertyName = "skipPreflight", NullValueHandling = NullValueHandling.Ignore)]
		public bool? SkipPreflight;
	}
}