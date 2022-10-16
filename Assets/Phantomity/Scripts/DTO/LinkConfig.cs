using System.Collections.Generic;

namespace Phantomity.DTO
{
	/// <summary>
	/// Object to config universal link for redirect from Phantom<br/>
	/// It can be use to config deep link, but the easiest way to do that is to use <see cref="PhantomBridge(string, string, string, Dictionary{string, string})"/>
	/// </summary>
	public class LinkConfig
	{
		/// <summary>
		/// Scheme of link
		/// </summary>
		/// <value>
		/// A <see cref="string"/> that can have any values for deep link and <c>http</c> or <c>https</c> for universal link.
		/// </value>
		public string Scheme { get; set; }
		/// <summary>
		/// Domain name
		/// </summary>
		public string Domain { get; set; }
		/// <value>
		/// A <see cref="string"/> that represent part of request url precedes the Phantom method name.
		/// </value>
		public string PathPrefix { get; set; }
		/// <summary>
		/// Allow to redefine callback methods names.<br/>
		/// By default callback names that uses in redirect link the same as <see cref="PhantomMethods">Phantom provider methods</see>
		/// If you need to redefine them please populate this dictionary.
		/// </summary>
		/// <value>
		/// Entry with <see cref="PhantomMethods"/> and string with new callback method name
		/// </value>
		public Dictionary<string, string> RedefinedMethods { get; set; }
	}
}