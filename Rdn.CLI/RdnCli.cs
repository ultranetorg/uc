using System.Reflection;

namespace Uccs.Rdn.CLI;

public class RdnCli : McvCli
{
	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new RdnCli();
	}

	public RdnCli()
	{
		Boot = new NetBoot(ExeDirectory);

		Net				= Rdn.ByZone(Boot.Zone);
		NexusSettings	= new NexusSettings(Boot.Zone, Boot.Profile);
		Settings		= new RdnNodeSettings(Path.Join(Boot.Profile, typeof(RdnNode).FullName), Boot.Zone, NexusSettings);

		Execute(Boot.Profile, Boot.Commnand);
	}

	public RdnCli(NexusSettings nexussettings, RdnNodeSettings settings, RdnApiClient api) : base(nexussettings, settings, api)
	{
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return	CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow)
				??
				base.Create(commnad, flow);
	}
}
