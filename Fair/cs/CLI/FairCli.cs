using System.Reflection;

namespace Uccs.Fair;

public class FairCli : McvCli
{
	public override JsonApiClient	Api => _Api ??= new FairApiClient(Settings.Api.LocalNodeAddress(Net));
	JsonApiClient					_Api;

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
		Net	= Fair.ByZone(nexussettings.Zone);
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return	CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow)
				??
				base.Create(commnad, flow);
	}
}
