using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Phantomity.Constants;
using Phantomity.DTO;
using Phantomity.Infrastructure;
using Phantomity.Utils;
using UnityEngine;

namespace Phantomity
{
	public class PhantomBridge : ConfigurableLink, IPhantomBridge
	{
		private const string DefaultPhantomUrl = "https://phantom.app/ul";
		private const string DefaultAppUrl = "https://unity.com";
		private const string DefaultLinkScheme = "unitydl";

		private readonly string _providerUrl = $"{DefaultPhantomUrl}/v1";
		private readonly string _browseUrl = $"{DefaultPhantomUrl}/browse";
		private readonly string _appUrl = DefaultAppUrl;
		private readonly string _cluster = Cluster.Devnet;

		private readonly DeepLinkProtocol _protocol;
		private readonly PhantomityVault _vault = new PhantomityVault();

		private PhantomConnectDTO _connectData;

		public bool IsConnected { get; private set; }
		public bool AutoConnect { get; set; } = false;

		private static LinkConfig CreateLinkConfig(string linkScheme = DefaultLinkScheme, Dictionary<string, string> redefinedMethods = null)
		{
			return new LinkConfig
			{
				Scheme = linkScheme,
				RedefinedMethods = redefinedMethods
			};
		}
		
		/// <summary>
		/// Constructor for test purposes.
		/// </summary>
		public PhantomBridge() : base(CreateLinkConfig())
		{
			_protocol = new DeepLinkProtocol(_providerUrl, LinkConfig);
		}

		/// <summary>
		/// Constructor for applications that uses deeplink technology. 
		/// </summary>
		/// <param name="linkScheme">A URL scheme that specifies a link structure that your application reacts to.</param>
		/// <param name="appUrl">A url used to fetch app metadata (i.e. title, icon)</param>
		/// <param name="cluster">The network that should be used for subsequent interactions. Please use values from <see cref="Cluster"/></param>
		/// <param name="redefinedMethods">Allow to redefine callback methods names.</param>
		public PhantomBridge(string linkScheme, string appUrl, string cluster = Cluster.Devnet, Dictionary<string, string> redefinedMethods = null) :
			base(CreateLinkConfig(linkScheme, redefinedMethods))
		{
			_appUrl = appUrl;
			_cluster = cluster;
			_protocol = new DeepLinkProtocol(_providerUrl, LinkConfig);
		}
		
		/// <summary>
		/// Constructor for applications that uses universal link technology. 
		/// </summary>
		/// <param name="linkConfig">Configuration of redirect link.</param>
		/// <param name="appUrl">A url used to fetch app metadata (i.e. title, icon)</param>
		/// <param name="cluster">The network that should be used for subsequent interactions. Please use values from <see cref="Cluster"/></param>
		public PhantomBridge(LinkConfig linkConfig, string appUrl, string cluster = Cluster.Devnet) : base(linkConfig)
		{
			_protocol = new DeepLinkProtocol(_providerUrl, linkConfig);
			_appUrl = appUrl;
			_cluster = cluster;
		}

		/// <summary>
		/// Establish a connection with the Phantom wallet.
		/// </summary>
		/// <returns>Returns a task that holds public key of the user.</returns>
		/// <exception cref="PhantomException">Throws exception when Phantom can't handle request.</exception>
		/// <exception cref="InvalidOperationException">Throws exception when Phantom already connected.</exception>
		/// <seealso href="https://docs.phantom.app/integrating/deeplinks-ios-and-android/provider-methods/connect">Connect</seealso>
		public async UniTask<string> Connect()
		{
			if (IsConnected)
			{
				throw new InvalidOperationException("Wallet already connected");
			}

			var data = new DeepLinkData
			{
				Method = PhantomMethods.Connect,
				Params = new Dictionary<string, string>
				{
					{ QueryParams.AppURL, Encode(_appUrl) },
					{ QueryParams.RedirectLink, Encode(GetLink(PhantomMethods.Connect)) },
					{ QueryParams.PubKey, _vault.PublicKey },
					{ QueryParams.Cluster, _cluster },
				}
			};

			var response = await _protocol.Send(data);

			CheckForError(response);

			return HandleConnect(response);
		}

		/// <summary>
		/// Disconnect from Phantom wallet.
		/// <remarks>
		/// Once disconnected, Phantom will reject all signature requests until another connection is established.
		/// </remarks>
		/// </summary>
		/// <returns>Returns a task that holds the asynchronous operation.</returns>
		/// <exception cref="PhantomException">Throws exception when Phantom can't handle request.</exception>
		/// <exception cref="InvalidOperationException">Throws exception when Phantom didn't connect and auto connect wasn't activate.</exception>
		/// <seealso href="https://docs.phantom.app/integrating/deeplinks-ios-and-android/provider-methods/disconnect"/>
		public async UniTask Disconnect()
		{
			if (!IsConnected)
			{
				throw new InvalidOperationException("Wallet not connected");
			}

			var payload = new Dictionary<string, string>
			{
				{ PayloadParams.Session, _connectData.Session }
			};

			var data = PrepareRequestData(PhantomMethods.Disconnect, payload);

			var response = await _protocol.Send(data);

			CheckForError(response);

			HandleDisconnect();
		}

		/// <summary>
		/// Sign given message with a user private key.
		/// </summary>
		/// <param name="message">The message that should be signed by the user. Phantom will display this message to the user when they are prompted to sign.</param>
		/// <param name="display">How message should display to the user. Defaults to utf8. Please use for this values from <see cref="DisplayMessageTypes"/></param>
		/// <returns>Returns a task that holds the message signature, encoded in base58.</returns>
		/// <exception cref="PhantomException">Throws exception when Phantom can't handle request.</exception>
		/// <exception cref="InvalidOperationException">Throws exception when Phantom didn't connect and auto connect wasn't activate.</exception>
		/// <seealso href="https://docs.phantom.app/integrating/deeplinks-ios-and-android/provider-methods/signmessage"/>
		public async UniTask<string> SignMessage(string message, string display = null)
		{
			await CheckConnection();
			
			var payload = new Dictionary<string, string>
			{
				{ PayloadParams.Session, _connectData.Session },
				{ PayloadParams.Message, Cryptography.Encode(System.Text.Encoding.UTF8.GetBytes(message)) }
			};
			if (display != null)
			{
				payload.Add(PayloadParams.Display, display);
			}
			
			var data = PrepareRequestData(PhantomMethods.SignMessage, payload);
			var response = await _protocol.Send(data);
			
			CheckForError(response);
			var respPayload = DecryptPayload<Dictionary<string, string>>(response);
			
			return respPayload[PayloadParams.Signature];
		}
		
		/// <summary>
		/// Sign given transaction with a user private key.
		/// <remarks>
		/// The easiest and most recommended way to send a transaction is via <see cref="SignAndSendTransaction"/>.
		/// </remarks>
		/// </summary>
		/// <param name="transaction">Serialized transaction that Phantom will sign.</param>
		/// <returns>Returns a task that holds the transaction signature, encoded in base58.</returns>
		/// <exception cref="PhantomException">Throws exception when Phantom can't handle request.</exception>
		/// <exception cref="InvalidOperationException">Throws exception when Phantom didn't connect and auto connect wasn't activate.</exception>
		/// <seealso href="https://docs.phantom.app/integrating/deeplinks-ios-and-android/provider-methods/signtransaction"/>
		public async UniTask<string> SignTransaction(byte[] transaction)
		{
			await CheckConnection();

			var payload = new Dictionary<string, string>
			{
				{ PayloadParams.Session, _connectData.Session },
				{ PayloadParams.Transaction, Cryptography.Encode(transaction) }
			};

			var data = PrepareRequestData(PhantomMethods.SignTx, payload);

			var response = await _protocol.Send(data);

			CheckForError(response);

			var respPayload = DecryptPayload<Dictionary<string, string>>(response);

			return respPayload[PayloadParams.Transaction];
		}
		
		/// <summary>
		/// Sign multiple transactions with a user private key.
		/// </summary>
		/// <param name="transactions">An array of serialized transactions that Phantom will sign.</param>
		/// <returns>Returns a task that holds an array of transaction signatures, encoded in base58.</returns>
		/// <exception cref="PhantomException">Throws exception when Phantom can't handle request.</exception>
		/// <exception cref="InvalidOperationException">Throws exception when Phantom didn't connect and auto connect wasn't activate.</exception>
		/// <seealso href="https://docs.phantom.app/integrating/deeplinks-ios-and-android/provider-methods/signalltransactions"/>
		public async UniTask<string> SignAllTransaction(byte[][] transactions)
		{
			await CheckConnection();

			var payload = new Dictionary<string, string>
			{
				{ PayloadParams.Session, _connectData.Session },
				{ PayloadParams.Transaction, JsonConvert.SerializeObject(Cryptography.Encode(transactions).ToArray()) }
			};

			var data = PrepareRequestData(PhantomMethods.SignAllTx, payload);

			var response = await _protocol.Send(data);

			CheckForError(response);

			var respPayload = DecryptPayload<Dictionary<string, string>>(response);

			return respPayload[PayloadParams.Transaction];
		}

		/// <summary>
		/// Prompt the user for permission to  send transactions on their behalf.
		/// </summary>
		/// <param name="transaction">Serialized transaction that Phantom will sign.</param>
		/// <param name="sendOptions">An optional <see cref="SendOptions">object</see> that specifies options for how Phantom should submit the transaction.</param>
		/// <returns>Returns a task that holds the first signature in the transaction, which can be used as its transaction id.</returns>
		/// <exception cref="PhantomException">Throws exception when Phantom can't handle request.</exception>
		/// <exception cref="InvalidOperationException">Throws exception when Phantom didn't connect and auto connect wasn't activate.</exception>
		/// <seealso href="https://docs.phantom.app/integrating/deeplinks-ios-and-android/provider-methods/signandsendtransaction"/>
		public async UniTask<string> SignAndSendTransaction(byte[] transaction, SendOptions sendOptions = null)
		{
			await CheckConnection();

			var payload = new Dictionary<string, string>
			{
				{ PayloadParams.Session, _connectData.Session },
				{ PayloadParams.Transaction, Cryptography.Encode(transaction) }
			};
			if (sendOptions != null)
			{
				var sendOptionsPayload = JsonConvert.SerializeObject(sendOptions);
				payload.Add(PayloadParams.SendOptions, sendOptionsPayload);
			}

			var data = PrepareRequestData(PhantomMethods.SignAndSendTx, payload);

			var response = await _protocol.Send(data);

			CheckForError(response);

			var respPayload = DecryptPayload<Dictionary<string, string>>(response);

			return respPayload[PayloadParams.Signature];
		}

		/// <summary>
		/// Open a page directly within Phantom’s in-app browser.
		/// </summary>
		/// <param name="url">The URL that should open within Phantom's in-app browser.</param>
		/// <seealso href="https://docs.phantom.app/integrating/deeplinks-ios-and-android/other-methods/browse"/>
		public void Browse(string url)
		{
			try
			{
				var browseUrl = $"{_browseUrl}/{Encode(url)}?ref={Encode(_appUrl)}";
				Application.OpenURL(browseUrl);
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				Debug.LogException(e);
			}
		}

		private DeepLinkData PrepareRequestData(string method, Dictionary<string, string> payload)
		{
			var nonce = PhantomityVault.GetNonce();
			var encryptedPayload = _vault.EncryptPayload(payload, nonce);

			return new DeepLinkData
			{
				Method = method,
				Params = new Dictionary<string, string>
				{
					{ QueryParams.RedirectLink, Encode(GetLink(method)) },
					{ QueryParams.PubKey, _vault.PublicKey },
					{ QueryParams.Nonce, Cryptography.Encode(nonce) },
					{ QueryParams.Payload, encryptedPayload },
				}
			};
		}

		private string HandleConnect(DeepLinkData data)
		{
			var phantomPublicKey = Cryptography.Decode(data.Params[QueryParams.ResponsePubKey]);
			_vault.GenerateSecret(phantomPublicKey);
			_connectData = DecryptPayload<PhantomConnectDTO>(data);

			IsConnected = true;

			return _connectData.WalletPublicKey;
		}

		private void HandleDisconnect()
		{
			IsConnected = false;
		}

		private T DecryptPayload<T>(DeepLinkData data)
		{
			var nonce = Cryptography.Decode(data.Params[QueryParams.Nonce]);
			var decryptedPayload = _vault.DecryptPayload(data.Params[QueryParams.Data], nonce);
			var payload = JsonConvert.DeserializeObject<T>(decryptedPayload);

			if (payload == null)
			{
				throw new Exception($"Payload can't be deserialized as ${nameof(T)}");
			}

			return payload;
		}

		private async UniTask CheckConnection()
		{
			if (!AutoConnect && !IsConnected)
			{
				throw new InvalidOperationException("Wallet not connected");
			}

			if (AutoConnect && !IsConnected)
			{
				await Connect();
			}
		}

		private static string Encode(string url)
		{
			return UnityWebRequest.EscapeURL(url);
		}

		private static void CheckForError(DeepLinkData data)
		{
			if (data.Params.ContainsKey(QueryParams.ErrorCode))
			{
				var code = int.Parse(data.Params[QueryParams.ErrorCode]);
				var message = data.Params[QueryParams.ErrorMessage];
				throw new PhantomException((PhantomErrorCode)code, message);
			}
		}
	}
}