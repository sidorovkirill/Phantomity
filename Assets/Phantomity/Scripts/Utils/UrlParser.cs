using System;
using System.Text.RegularExpressions;
using Phantomity.Constants;

namespace Phantomity.Utils
{
	public class UrlParser
	{
		private readonly string _urlRegex = Build();
		private GroupCollection _parsedUrl;
		public static string Build()
		{
			return
				$@"^((?<{UrlRegexVariables.Scheme}>[^:/?#]+):)" +
				$@"?//(?<{UrlRegexVariables.Domain}>[^/?#]*\.[^/?#]*)"+
				$@"?([/]*(?<{UrlRegexVariables.Method}>[^?#]*))" +
				$@"?(\?(?<{UrlRegexVariables.Params}>[^#]*))" +
				$@"?(#(?<{UrlRegexVariables.Anchor}>.*))?";
		}
		
		public UrlParser(string url)
		{
			Regex rx = new Regex(_urlRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
			MatchCollection matches = rx.Matches(url);
            
			if (matches.Count == 0)
			{
				throw new Exception("Received message doesn't match to url rules");
			}
            
			_parsedUrl = matches[0].Groups;
		}

		public string GetUrlPart(string partName)
		{
			var part = _parsedUrl[partName];
			return !String.IsNullOrEmpty(part.Value) ? part.Value : null;
		}

		public string GetMethodName(string prefix = null)
		{
			var methodName = GetUrlPart(UrlRegexVariables.Method);
			if (prefix != null)
			{
				methodName = methodName.Replace(prefix + "/", "");
			}

			return methodName;
		}
	}
}