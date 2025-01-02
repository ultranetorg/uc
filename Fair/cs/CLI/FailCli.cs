namespace Uccs.Fair;

public class FairCli : McvCli
{
	public FairCli()
	{
	}

	public FairCli(FairNodeSettings settings, FairApiClient api, Flow workflow, IPasswordAsker passwordAsker) : base(settings, api, workflow, passwordAsker)
	{
	}

	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

		new McvCli();
	}

	public override McvCommand Create(IEnumerable<Xon> commnad, Flow flow)
	{
		McvCommand c = null;
		var t = commnad.First().Name;

		var args = commnad.Skip(1).ToList();

		switch(t)
		{
			case AuthorCommand.Keyword:		c = new AuthorCommand(this, args, flow); break;
			case SiteCommand.Keyword :	c = new SiteCommand(this, args, flow); break;
			case ProductCommand.Keyword:	c = new ProductCommand(this, args, flow); break;
			case PageCommand.Keyword:		c = new PageCommand(this, args, flow); break;
			
			//case AnalysisCommand.Keyword:	c = new AnalysisCommand(this, args, flow); break;
			//case DevCommand.Keyword:		c = new DevCommand(this, args, flow); break;
			//case EconomyCommand.Keyword:	c = new EconomyCommand(this, args, flow); break;
			//case DomainCommand.Keyword:		c = new DomainCommand(this, args, flow); break;
			//case ResourceCommand.Keyword:	c = new ResourceCommand(this, args, flow); break;
			//case ReleaseCommand.Keyword:	c = new ReleaseCommand(this, args, flow); break;
			//case LinkCommand.Keyword:		c = new LinkCommand(this, args, flow); break;
		}

		return c ?? base.Create(commnad, flow);;
	}

	public override object Execute(Boot boot, Flow flow)
	{
		Settings = new FairNodeSettings(boot.Profile);
		Net = Fair.ByZone(boot.Zone);

		return base.Execute(boot, flow);
	}
}
