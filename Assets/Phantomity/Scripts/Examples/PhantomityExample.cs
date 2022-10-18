using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Phantomity.Constants;
using Phantomity.DTO;
using Phantomity.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Phantomity.Examples
{
	public class PhantomityExample : MonoBehaviour
	{
		private const string UrlToBrowse = "https://phantom.app/";
		private const string MessageToSign = "Hello Phantom!";

		[SerializeField] private GameObject _connectScreen;
		[SerializeField] private GameObject _featuresScreen;
		[SerializeField] private Text _userAddress;
		[SerializeField] private Text _signature;
		
		private IPhantomBridge _phantomity;
		private string _address;

		public void Connect()
		{
			ConnectAsync().Forget();
		}

		public void SignMessage()
		{
			SignMessageAsync().Forget();
		}

		public void Browse()
		{
			_phantomity.Browse(UrlToBrowse);
		}
		
		public void Disconnect()
		{
			DisconnectAsync().Forget();
		}

		private void Start()
		{
			_phantomity = new PhantomBridge();
			ToggleConnection(false);
		}

		private void SetUpUniversalLink()
		{
			var linkConfig = new LinkConfig
			{
				Scheme = "https",
				Domain = "www.ankr.com",
				PathPrefix = "phantom",
				RedefinedMethods = new Dictionary<string, string>
				{
					{ PhantomMethods.Connect, "onPhantomConnected" },
					{ PhantomMethods.SignMessage, "onMessageSigned" }
				}
			};
			var _phantomity = new PhantomBridge(linkConfig, "https://www.ankr.com/");
		}

		private async UniTaskVoid ConnectAsync()
		{
			_address = await _phantomity.Connect();
			_userAddress.text = _address;
			ToggleConnection(true);
		}

		private void OnConnect()
		{
			_userAddress.text = _address;
			
			_connectScreen.SetActive(false);
			_featuresScreen.SetActive(true);
		}

		private async UniTask SignMessageAsync()
		{
			var signature = await _phantomity.SignMessage(MessageToSign);
			OnMessageSigned(signature);
		}

		private void OnMessageSigned(string signature)
		{
			_signature.text = signature;
		}
		
		private async UniTask DisconnectAsync()
		{
			await _phantomity.Disconnect();
			ToggleConnection(false);
			_signature.text = String.Empty;
			_userAddress.text = String.Empty;
		}

		private void ToggleConnection(bool status)
		{
			_connectScreen.SetActive(!status);
			_featuresScreen.SetActive(status);
		}
	}
}