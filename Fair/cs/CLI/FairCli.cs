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
		var b = new NetBoot(ExeDirectory);

		Net				= Fair.ByZone(b.Zone);
		NexusSettings	= new NexusSettings(b.Zone, b.Profile);
		Settings		= new FairNodeSettings(Path.Join(b.Profile, typeof(FairNode).FullName), b.Zone, NexusSettings);

		Execute(b);
	}

	public FairCli(NexusSettings nexussettings, FairNodeSettings settings, FairApiClient api) : base(nexussettings, settings, api)
	{
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;

		var args = commnad.Skip(1).ToList();

		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		var c = ct?.GetConstructor([typeof(FairCli), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as Command;
				
		if(c != null)
		{
			var a = c.Actions.FirstOrDefault(i => i.Name == null || i.Names.Contains(args.FirstOrDefault()?.Name));
			
			if(a != null)
				return c;
		}
		
		return base.Create(commnad, flow);
	}
}
