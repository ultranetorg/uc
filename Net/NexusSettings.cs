using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;

namespace Uccs.Net;


public class NexusSessionSettings
{
	public string			Net { get; set; }
	public AccountAddress	Signer { get; set; }
	public byte[]			Session { get; set; }
}

public class DeployedNode
{
	public string			Net;
	public string			Package;
	public Process			Process;
}

public class NexusSettings : SavableSettings
{
	public Zone							Zone;
	public string						Name { get; set; }
	public IPAddress					Host { get; set; }
	public IpApiSettings				Api { get; set; }
	public string						Packages { get; set; }
	public NexusSessionSettings[]		Sessions { get; set; }
	public PeeringSettings				IccpPeering { get; set; }
	public List<DeployedNode>			Nodes { get; set; } = [];

	public static readonly IPAddress	StandardHost = new ([127, 1, 0, 0]);

	public NexusSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public NexusSettings(Zone zone, string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
		Zone = zone;

		if(!File.Exists(Path))
		{
			Name		= GenerateRandomString();
			Host		= StandardHost;
			IccpPeering	= new PeeringSettings {Endpoint = new (IPAddress.Any, Port.Map(zone, KnownProtocol.Iccp))};
			Api			= new () {LocalIP = StandardHost};
			Packages	= System.IO.Path.Join(profile, "Packages");
			
			Save();
		}
	}

	public static string GenerateRandomString(int length = 16)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    
		return string.Create(length, chars, (span, alphabet) => RandomNumberGenerator.GetItems(alphabet, span));
	}
}
