using System.Numerics;

namespace Uccs.Net;

public class NnApiClient : ApiClient
{
	public NnApiClient(string address, string accesskey = null, HttpClient http = null, int timeout = 30) : base(address, accesskey, http, timeout)
	{
	}
}

public class AssetHolder
{
	public string	Class { get; set; }
	public string	Id { get; set; }
}

public class Asset
{
	public string	Name { get; set; }
	public string	Units { get; set; }
}

	//public BigInteger	Amount { get; set; }