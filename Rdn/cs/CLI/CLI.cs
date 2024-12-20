using System.Diagnostics;
using System.Reflection;

namespace Uccs.Rdn.CLI
{
	public class Program : McvCli
	{
		public Program()
		{
		}

		public Program(RdnNodeSettings settings, RdnApiClient api, Flow workflow, IPasswordAsker passwordAsker) : base(settings, api, workflow, passwordAsker)
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
			var c = base.Create(commnad, flow);

			if(c != null)
				return c;

			var t = commnad.First().Name;

			var args = commnad.Skip(1).ToList();

			switch(t)
			{
				case AnalysisCommand.Keyword:	c = new AnalysisCommand(this, args, flow); break;
				case DevCommand.Keyword:		c = new DevCommand(this, args, flow); break;
				case EconomyCommand.Keyword:	c = new EconomyCommand(this, args, flow); break;
				case DomainCommand.Keyword:		c = new DomainCommand(this, args, flow); break;
				case ResourceCommand.Keyword:	c = new ResourceCommand(this, args, flow); break;
				case ReleaseCommand.Keyword:	c = new ReleaseCommand(this, args, flow); break;
				case LinkCommand.Keyword:		c = new LinkCommand(this, args, flow); break;
				default:
					throw new SyntaxException("Unknown command");
			}

			return c;
		}

		public override object Execute(Boot boot, Flow flow)
		{
			Settings = new RdnNodeSettings(boot.Profile);
			Net = Rdn.ByZone(boot.Zone);

			return base.Execute(boot, flow);
		}
	}
}
