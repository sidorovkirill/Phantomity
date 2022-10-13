namespace Phantom
{
	public enum PhantomErrorCode
	{
		Disconnected = 4900,
		Unauthorized = 4100,
		UserRejectedRequest = 4001,
		InvalidInput = -32000,
		RequestedResourceNotAvailable = -32002,
		TransactionRejected = -32003,
		MethodNotFound = -32601,
		InternalError = -32603
	}
}