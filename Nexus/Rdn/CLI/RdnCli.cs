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

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return	CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow)
				??
				base.Create(commnad, flow);
	}
}
