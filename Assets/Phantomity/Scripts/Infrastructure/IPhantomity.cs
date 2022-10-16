using System.Threading.Tasks;
using Phantomity.DTO;

namespace Phantomity.Infrastructure
{
	public interface IPhantomity
	{
		Task<string> Connect();
		Task Disconnect();
		Task<string> SignMessage(string message, string display = null);
		Task<string> SignAndSendTransaction(byte[] transaction, SendOptions sendOptions = null);
		Task<string> SignTransaction(byte[] transaction);
		Task<string> SignAllTransaction(byte[][] transactions);
	}
}