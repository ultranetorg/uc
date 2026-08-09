namespace Uccs.Net;

public class ChainSettings : Settings
{
	public ChainSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}

public class MembershipSettings : Settings
{
	public string			Generator { get; set; }
	public string			Beneficiary { get; set; }
	public AutoId			GeneratorId;
	public AutoId			BeneficiaryId;

	public MembershipSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public override string ToString()
	{
		return $"{Generator}/{GeneratorId}, {Beneficiary}/{BeneficiaryId} ";
	}
}

public class McvSettings : Settings
{
	public List<MembershipSettings>		Memberships { get; set; } = [];
	public ChainSettings				Chain { get; set; }
	public virtual long					Roles => ((long)Role.Graph) |
												 (Chain != null ? (long)Role.Chain : 0);

	public McvSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}
