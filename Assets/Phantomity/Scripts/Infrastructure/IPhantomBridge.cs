using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Phantomity.DTO;

namespace Phantomity.Infrastructure
{
	public interface IPhantomBridge
	{
		UniTask<string> Connect();
		UniTask Disconnect();
		UniTask<string> SignMessage(string message, string display = null);
		UniTask<string> SignAndSendTransaction(byte[] transaction, SendOptions sendOptions = null);
		UniTask<string> SignTransaction(byte[] transaction);
		UniTask<string> SignAllTransaction(byte[][] transactions);
		void Browse(string url);
	}
}