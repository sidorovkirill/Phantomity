using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phantomity.Constants;
using Phantomity.DTO;
using Phantomity.Infrastructure;
using UnityEngine;

namespace Phantomity.Examples
{
	public class BridgeExample : MonoBehaviour
	{
		private void Awake()
		{
			
		}

		private void Start()
		{
			SignWithDeepLink().ConfigureAwait(false);
			// SignWithUniversalLink().ConfigureAwait(false);
		}

		private async Task SignWithDeepLink()
		{
			var phantomBridge = new PhantomityBridge();
			await SignMessage(phantomBridge);
			phantomBridge.Browse("https://www.ankr.com/");
		}

		private async Task SignWithUniversalLink()
		{
			var linkConfig = new LinkConfig
			{
				Scheme = "unitydl",
				Domain = "www.ankr.com",
				PathPrefix = "phantom",
				RedefinedMethods = new Dictionary<string, string>
				{
					{PhantomMethods.Connect, "onPhantomConnected"},
					{PhantomMethods.SignMessage, "onMessageSigned"}
				}
			};
			var phantomBridge = new PhantomityBridge(linkConfig, "https://www.ankr.com/");
			await SignMessage(phantomBridge);
		}

		private async Task SignMessage(IPhantomity phantomBridge)
		{
			var address = await phantomBridge.Connect();
			Debug.Log("address = " + address);
			try
			{
				var signature = await phantomBridge.SignMessage("HelloWorld");
				Debug.Log("signature = " + signature);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}