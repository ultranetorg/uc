namespace Uccs.Net;

public interface IFeeAsker
{
	bool Ask(HomoPeering sun, AccountAddress account, Operation operation);
}

public class SilentFeeAsker : IFeeAsker
{
	public SilentFeeAsker()
	{
	}

	public bool Ask(HomoPeering sun, AccountAddress account, Operation operation)
	{
		return true;
	}
}
