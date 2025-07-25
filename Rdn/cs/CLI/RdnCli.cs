using System.Reflection;

namespace Uccs.Rdn.CLI;

public class RdnCli : McvCli
{
	public RdnCli()
	{
	}

	public RdnCli(RdnNodeSettings settings, RdnApiClient api) : base(settings, api)
	{
	}

	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

		new RdnCli();
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;

		var args = commnad.Skip(1).ToList();

		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return	ct?.GetConstructor([typeof(RdnCli), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as Command 
				??
				base.Create(commnad, flow);
	}

	public override object Execute(Boot boot, Flow flow)
	{
		Settings = new RdnNodeSettings(boot.Profile);
		Net = Rdn.ByZone(boot.Zone);

		return base.Execute(boot, flow);
	}
}
