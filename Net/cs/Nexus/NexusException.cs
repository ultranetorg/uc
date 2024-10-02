using System.Reflection;

namespace Uccs.Net
{
	public enum NexusError : byte
	{
		None,
		NotHub
	}

	public class NexusException : NetException
	{
		public override int				ErrorCode { get => (int)Error; set => Error = (NexusError)value; }
		public NexusError				Error { get; protected set; }
		public override string			Message => Error.ToString();

		public NexusException()
		{
		}

		public NexusException(NexusError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
	}
}
