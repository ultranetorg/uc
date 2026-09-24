using System.Reflection;

namespace Uccs.Rdn.CLI;

public class RdnCli : McvCli
{
	public const string	ExeBaseFileName = "rdn";

	public RdnCli()
	{
		Boot = new NetBoot(ExeDirectory);

		Net				= Rdn.ByZone(Boot.Zone);
		NexusSettings	= new NexusSettings(Boot.Zone, Boot.Profile);
		Settings		= new RdnNodeSettings(NexusSettings);

		Api				= new RdnApiClient(Settings.Api.LocalNodeAddress(Net));

		Execute(Boot.Profile, Boot.Commnand);
	}

	public RdnCli(NexusSettings nexussettings, RdnNodeSettings settings, RdnApiClient api) : base(nexussettings, settings, api)
	{
		Net	= Rdn.ByZone(nexussettings.Zone);
	}

	public override void Collect()
	{
		base.Collect();

		Commands.Remove(typeof(Uccs.Net.UserCommand));

		Commands.Add(typeof(DevCommand));
		Commands.Add(typeof(DomainCommand));
		Commands.Add(typeof(DomainNameCommand));
		Commands.Add(typeof(EconomyCommand));
		Commands.Add(typeof(LinkCommand));
		Commands.Add(typeof(NodeCommand));
		Commands.Add(typeof(ReleaseCommand));
		Commands.Add(typeof(ResourceCommand));
		Commands.Add(typeof(SubnetCommand));
		Commands.Add(typeof(UserCommand));
	}
}
