using System;
using System.Threading.Tasks;
using Phantom;
using Phantom.Infrastructure;
using UnityEngine;

namespace DefaultNamespace
{
	public class BridgeExample : MonoBehaviour
	{
		private IPhantomBridge _phantomBridge;

		private void Awake()
		{
			_phantomBridge = new PhantomBridge();
		}

		private void Start()
		{
			Initialize().ConfigureAwait(false);
		}

		private async Task Initialize()
		{
			var address = await _phantomBridge.Connect();
			Debug.Log("address = " + address);
			try
			{
				var signature = await _phantomBridge.SignMessage("HelloWorld");
				Debug.Log("signature = " + signature);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		private async Task OnDestroy()
		{
			await _phantomBridge.Disconnect();
		}
	}
}