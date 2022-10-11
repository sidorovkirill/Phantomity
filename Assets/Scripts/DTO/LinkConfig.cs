using System.Collections.Generic;

namespace DTO
{
	public class LinkConfig
	{
		public string Scheme { get; set; }
		public string Domain { get; set; }
		public string PathPrefix { get; set; }
		
		public Dictionary<string, string> RedefinedMethods { get; set; }
	}
}