using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phantomity;
using Phantomity.Constants;
using Phantomity.DTO;
using Phantomity.Infrastructure;
using Phantomity.Utils;
using UnityEngine;

namespace DefaultNamespace
{
	public class DocExamples : MonoBehaviour
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
			var scheme = "unitydl";
			var appUrl = "https://example.com";
			var phantomBridge = new PhantomBridge(scheme, appUrl);

			await phantomBridge.Connect();

			phantomBridge.AutoConnect = true;

			try
			{
				var serializedTransaction = new byte[10];
				var signature = await phantomBridge.SignTransaction(serializedTransaction);
			}
			catch (PhantomException e)
			{
				Debug.Log(e.Message);
			}
			catch (InvalidOperationException e)
			{
				Debug.Log(e.Message);
			}
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