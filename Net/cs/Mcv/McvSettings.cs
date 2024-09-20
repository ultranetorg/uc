namespace Uccs.Net
{
	public class BaseSettings : Settings
	{
		public ChainSettings	Chain { get; set; }

		public BaseSettings() : base(NetXonTextValueSerializator.Default)
		{
		}
	}

	public class ChainSettings : Settings
	{
		public ChainSettings() : base(NetXonTextValueSerializator.Default)
		{
		}
	}

	public class McvSettings : NodeSettings
	{
		//public long						Pledge { get; set; }
		public AccountAddress[]			Generators { get; set; } = [];
		//public List<AccountAddress>	ProposedFundJoiners = new();
		//public List<AccountAddress>	ProposedFundLeavers = new();
		public BaseSettings				Base { get; set; }

		public virtual long				Roles =>	(Base != null ? (long)Role.Base : 0) |
													(Base?.Chain != null ? (long)Role.Chain : 0);

		public McvSettings()
		{
		}

		public McvSettings(string profile) : base(profile)
		{
		}
	}
}
