namespace Uccs.Net;

public class VaultException : Exception
{
	public VaultException()
	{
	}

	public VaultException(string m) : base(m)
	{
	}

	public VaultException(string m, Exception inner) : base(m, inner)
	{
	}
}
