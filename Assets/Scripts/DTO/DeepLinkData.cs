using System.Collections.Generic;

namespace Phantom
{
	public class DeepLinkData
	{
		public string Method { get; set; }
		public Dictionary<string, string> Params = new Dictionary<string, string>();
	}
}