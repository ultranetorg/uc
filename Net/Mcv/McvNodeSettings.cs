using System.Diagnostics;
using System.Net;

namespace Uccs.Net;

public class AccountSessionSettings
{
	public string			User { get; set; }
	public AccountAddress	Signer { get; set; }
	public byte[]			Session { get; set; }
}

public class McvNodeSettings : SavableSettings
{
	public IpApiSettings			Api { get; set; }
	public McvSettings				Mcv { get; set; }
	public PeeringSettings			Peering { get; set; } = new();
	//public PeeringSettings			NnPeering { get; set; }
	public AccountSessionSettings[]	Sessions { get; set; }
	public bool						Log { get; set; }
	public int						PoolMaximum { get; set; } = 100_000;
	public int						PpcTimeout { get; set; } = 5000;
	public int						TransactingTimeout { get; set; } = 5*60*1000;
	public int						TransactionNoInquireKeepPeriod { get; set; } = 60; /// In seconds

	public virtual long				Roles => (Mcv?.Roles ?? 0);

	public McvNodeSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public McvNodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
		if(Debugger.IsAttached)
		{
			PpcTimeout = int.MaxValue;
			TransactingTimeout = int.MaxValue;
		}
	}
}
