namespace Uccs.Net
{
	public interface IFeeAsker
	{
		bool Ask(Node sun, AccountAddress account, Operation operation);
	}

	public class SilentFeeAsker : IFeeAsker
	{
		public SilentFeeAsker()
		{
		}

		public bool Ask(Node sun, AccountAddress account, Operation operation)
		{
			return true;
		}
	}
}
