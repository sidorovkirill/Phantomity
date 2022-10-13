using System;
using System.Runtime.Serialization;

namespace Phantom
{
	[Serializable]
	public class PhantomException : InvalidOperationException, ISerializable
	{
		public PhantomErrorCode ErrorCode { get; private set; }

		private static string CreateMessage(PhantomErrorCode code, string message)
		{
			return $"Phantom error {code}: {message}";
		}
		
		public PhantomException(PhantomErrorCode errorCode, string message) : base(CreateMessage(errorCode, message))
		{
			ErrorCode = errorCode;
		}
		
		protected PhantomException(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
		}
		
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
		
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}