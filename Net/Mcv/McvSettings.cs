namespace Uccs.Net;

public class ChainSettings : Settings
{
	public ChainSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}

public class GeneratorSettings : Settings
{
	public string			User { get; set; }
	public AutoId			Id;

	public GeneratorSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public override string ToString()
	{
		return $"{User}/{Id}";
	}
}

public class McvSettings : Settings
{
	public GeneratorSettings[]		Generators { get; set; } = [];
	public ChainSettings			Chain { get; set; }
	public virtual long				Roles => ((long)Role.Graph) |
											 (Chain != null ? (long)Role.Chain : 0);

	public McvSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}
