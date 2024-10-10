using System.Reflection;

namespace Uccs.Net
{
	public enum NtnError : byte
	{
		None,
		NotHub
	}

	public class NtnException : NetException
	{
		public override int				ErrorCode { get => (int)Error; set => Error = (NtnError)value; }
		public NtnError				Error { get; protected set; }
		public override string			Message => Error.ToString();

		public NtnException()
		{
		}

		public NtnException(NtnError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
	}
}
