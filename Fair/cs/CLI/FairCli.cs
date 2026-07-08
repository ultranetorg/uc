using System.Reflection;

namespace Uccs.Fair;

public class FairCli : McvCli
{
	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new FairCli();
	}

	public FairCli()
	{
		Boot = new NetBoot(ExeDirectory);

		Net				= Fair.ByZone(Boot.Zone);
		NexusSettings	= new NexusSettings(Boot.Zone, Boot.Profile);
		Settings		= new FairNodeSettings(Path.Join(Boot.Profile, typeof(FairNode).FullName), Boot.Zone, NexusSettings);

		Execute(Boot.Profile, Boot.Commnand);
	}

	public FairCli(NexusSettings nexussettings, FairNodeSettings settings, FairApiClient api) : base(nexussettings, settings, api)
	{
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return	CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow)
				??
				base.Create(commnad, flow);
	}
}
