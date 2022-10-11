using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;
using DTO;
using Newtonsoft.Json;
using Phantom.DTO;
using Phantom.Infrastructure;

namespace Phantom
{
	public class PhantomBridge : ConfigurableLink, IPhantomBridge
	{
		private const string DefaultAppUrl = "https://unity.com";
		private const string DefaultLinkScheme = "unitydl";

		private readonly string _appUrl = DefaultAppUrl;
		private readonly string _cluster = Cluster.Devnet;

		private readonly DeepLinkProtocol _protocol;
		private readonly PhantomBridgeVault _vault = new PhantomBridgeVault();

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
		
		public PhantomBridge() : base(CreateLinkConfig())
		{
			_protocol = new DeepLinkProtocol(LinkConfig);
		}

		public PhantomBridge(string linkScheme, string appUrl, string cluster = Cluster.Devnet, Dictionary<string, string> redefinedMethods = null) :
			base(CreateLinkConfig(linkScheme, redefinedMethods))
		{
			_appUrl = appUrl;
			_cluster = cluster;
			_protocol = new DeepLinkProtocol(LinkConfig);
		}
		
		public PhantomBridge(LinkConfig linkConfig, string appUrl, string cluster = Cluster.Devnet) : base(linkConfig)
		{
			_protocol = new DeepLinkProtocol(linkConfig);
			_appUrl = appUrl;
			_cluster = cluster;
		}

		public async Task<string> Connect()
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

		public async Task Disconnect()
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

		public async Task<string> SignMessage(string message)
		{
			await CheckConnection();
			
			var payload = new Dictionary<string, string>
			{
				{ PayloadParams.Session, _connectData.Session },
				{ PayloadParams.Message, Cryptography.Encode(System.Text.Encoding.UTF8.GetBytes(message)) }
			};
			var data = PrepareRequestData(PhantomMethods.SignMessage, payload);
			var response = await _protocol.Send(data);
			
			CheckForError(response);
			var respPayload = DecryptPayload<Dictionary<string, string>>(response);
			
			return respPayload[PayloadParams.Signature];
		}

		public async Task<string> SignAndSendTransaction(byte[] transaction, SendOptions sendOptions = null)
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

		public async Task<string> SignTransaction(byte[] transaction)
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
		
		public async Task<string> SignAllTransaction(byte[][] transactions)
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

		private DeepLinkData PrepareRequestData(string method, Dictionary<string, string> payload)
		{
			var nonce = PhantomBridgeVault.GetNonce();
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

		private async Task CheckConnection()
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
				throw new Exception(
					$"Error {data.Params[QueryParams.ErrorCode]}: {data.Params[QueryParams.ErrorMessage]}");
			}
		}
	}
}