using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phantom;
using Phantom.DTO;
using Phantom.Infrastructure;
using UnityEngine;

namespace DefaultNamespace
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
			var phantomBridge = new PhantomBridge();
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
			var phantomBridge = new PhantomBridge(linkConfig, "https://www.ankr.com/");
			await SignMessage(phantomBridge);
		}

		private async Task SignMessage(IPhantomBridge phantomBridge)
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