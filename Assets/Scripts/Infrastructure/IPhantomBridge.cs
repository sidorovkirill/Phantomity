using System.Threading.Tasks;
using Phantom.DTO;

namespace Phantom.Infrastructure
{
	public interface IPhantomBridge
	{
		Task<string> Connect();
		Task Disconnect();
		Task<string> SignMessage(string message);
		Task<string> SignAndSendTransaction(byte[] transaction, SendOptions sendOptions = null);
		Task<string> SignTransaction(byte[] transaction);
	}
}