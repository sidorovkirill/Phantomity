namespace Phantom
{
	public static class UrlRegex
	{
		public static string Build()
		{
			return
				$@"^((?<{UrlRegexVariables.Scheme}>[^:/?#]+):)" +
				$@"?//(?<{UrlRegexVariables.Domain}>[^/?#]*\.[^/?#]*)"+
				$@"?([/]*(?<{UrlRegexVariables.Action}>[^?#]*))" +
				$@"?(\?(?<{UrlRegexVariables.Params}>[^#]*))" +
				$@"?(#(?<{UrlRegexVariables.Anchor}>.*))?";
		}
	}
}