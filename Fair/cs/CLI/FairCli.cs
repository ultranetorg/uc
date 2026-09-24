using System.Reflection;

namespace Uccs.Fair;

public class FairCli : McvCli
{
	public FairCli()
	{
		Boot = new NetBoot(ExeDirectory);

		Net				= Fair.ByZone(Boot.Zone);
		NexusSettings	= new NexusSettings(Boot.Zone, Boot.Profile);
		Settings		= new FairNodeSettings(NexusSettings);

		Api				= new FairApiClient(Settings.Api.LocalNodeAddress(Net));

		Execute(Boot.Profile, Boot.Commnand);
	}

	public FairCli(NexusSettings nexussettings, FairNodeSettings settings, FairApiClient api) : base(nexussettings, settings, api)
	{
		Net	= Fair.ByZone(nexussettings.Zone);
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return	CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow)
				??
				base.Create(commnad, flow);
	}
}
