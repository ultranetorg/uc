using System.Reflection;

namespace Uccs.Smp;

public class FairCli : McvCli
{
	public FairCli()
	{
	}

	public FairCli(SmpNodeSettings settings, SmpApiClient api, Flow workflow, IPasswordAsker passwordAsker) : base(settings, api, workflow, passwordAsker)
	{
	}

	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

		new FairCli();
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

	public override object Execute(Boot boot, Flow flow)
	{
		Settings = new SmpNodeSettings(boot.Profile);
		Net = Fair.ByZone(boot.Zone);

		return base.Execute(boot, flow);
	}
}
