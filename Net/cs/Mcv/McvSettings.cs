namespace Uccs.Net;

public class ChainSettings : Settings
{
	public ChainSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}

public class McvSettings : Settings
{
	public AccountKey[]				Generators { get; set; } = [];
	public ChainSettings			Chain { get; set; }
	public virtual long				Roles => ((long)Role.Graph) |
											 (Chain != null ? (long)Role.Chain : 0);

	//public long					Pledge { get; set; }
	//public List<AccountAddress>	ProposedFundJoiners = new();
	//public List<AccountAddress>	ProposedFundLeavers = new();


	public McvSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}
