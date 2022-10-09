using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Phantom.DTO;
using Phantom.Infrastructure;
using UnityEngine;

namespace Phantom
{
	public class PhantomBridge : IPhantomBridge
	{
		private const string DefaultAppUrl = "https://unity.com";

		private string _appUrl = DefaultAppUrl;
		private string _appUrlScheme = "unitydl";
		private string _cluster = Cluster.Devnet;

		private readonly DeepLinkProtocol _protocol;
		private readonly PhantomBridgeVault _vault;

		private PhantomConnectDTO _connectData;

		public bool IsConnected { get; private set; }
		public bool AutoConnect { get; set; } = false;

		public PhantomBridge()
		{
			_vault = new PhantomBridgeVault();
			_protocol = new DeepLinkProtocol();
		}

		public PhantomBridge(string appUrlScheme, string appUrl = DefaultAppUrl, string cluster = Cluster.Devnet) :
			this()
		{
			_appUrlScheme = appUrlScheme;
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
				Method = Requests.ConnectMethod,
				Params = new Dictionary<string, string>
				{
					{ QueryParams.AppURL, Encode(_appUrl) },
					{ QueryParams.RedirectLink, GetCallbackUrl(Requests.ConnectMethod) },
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

			var data = PrepareRequestData(Requests.DisconnectMethod, payload);

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
			var data = PrepareRequestData(Requests.SignMessageMethod, payload);
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

			var data = PrepareRequestData(Requests.SignAndSendTxMethod, payload);

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

			var data = PrepareRequestData(Requests.SignTxMethod, payload);

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
					{ QueryParams.RedirectLink, GetCallbackUrl(method) },
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

		private string GetCallbackUrl(string method)
		{
			return Encode($"{_appUrlScheme}://{method}");
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