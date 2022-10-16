using Phantomity.DTO;

namespace Phantomity
{
	public class ConfigurableLink
	{
		protected readonly LinkConfig LinkConfig;

		protected ConfigurableLink(LinkConfig linkConfig)
		{
			LinkConfig = linkConfig;
		}
		
		protected string GetMethodName(string method)
		{
			if (LinkConfig.RedefinedMethods != null && LinkConfig.RedefinedMethods.ContainsKey(method))
			{
				return LinkConfig.RedefinedMethods[method];
			}
			else
			{
				return method;
			}
		}
		
		protected string GetLink(string method)
		{
			var redirectLink = LinkConfig.Scheme + "://";
			
			if (LinkConfig.Domain != null)
			{
				redirectLink += LinkConfig.Domain + "/";
			}

			if (LinkConfig.PathPrefix != null)
			{
				redirectLink += LinkConfig.PathPrefix + "/";
			}
			
			return redirectLink + GetMethodName(method);
		}
	}
}