using System.Text.Json.Serialization;

namespace Uccs.Net;

public enum Trust : byte
{
	None, AlwaysAllow, AskEveryTime
}

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum CryptographyType : byte
{
	No, Mcv, Iccp
}

public class AuthenticationResult
{
	public PublicKey	Signer { get; set; }
	public byte[]			Session { get; set; }
}

public class AuthorizationResult
{
	public PublicKey	Signer { get; set; }
	public byte[]		Signature { get; set; }
}

public class VaultApiClient : JsonApiClient
{
	public VaultApiClient(string address, HttpClient http = null, int timeout = 30) : base(address, http, timeout)
	{
	}
}

public class IsAuthenticatedApc : Fpc<bool>
{
	public string				Application { get; set; }
	public string				Net { get; set; }
	public string				User { get; set; }
	public byte[]				Session { get; set; }
}

public class AuthenticateApc : Fpc<AuthenticationResult>
{
	public string				Application { get; set; }
	public string				Net { get; set; } /// fair.rdn
	public string				User { get; set; } /// optional
	public byte[]				Logo { get; set; }
	public PublicKey			Key { get; set; }/// suggested
}

public class AuthorizeApc : Fpc<AuthorizationResult>
{
	public CryptographyType		Cryptography { get; set; }
	public string				Net { get; set; }
	public string				User { get; set; }
	public byte[]				Session { get; set; }
	public string				Operation { get; set; }
	public byte[]				Hash { get; set; }
	public bool					ManualApprovalRequired { get; set; }
}
