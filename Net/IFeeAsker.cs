namespace Uccs.Net;

public interface IFeeAsker
{
	bool Ask(HomoTcpPeering sun, AccountAddress account, Operation operation);
}

public class SilentFeeAsker : IFeeAsker
{
	public SilentFeeAsker()
	{
	}

	public bool Ask(HomoTcpPeering sun, AccountAddress account, Operation operation)
	{
		return true;
	}
}
