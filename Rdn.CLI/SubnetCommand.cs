using System.Reflection;

namespace Uccs.Rdn.CLI;

public class SubnetCommand : RdnCommand
{
	string First => Args[0].Name;

	public SubnetCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Attachment()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "a";
		a.Description = "Request subnet attachement voting";
		a.Arguments =	[
							new (null, SNN, "A name of a subnet to attach", Flag.First),
							new ("peer", IP, "A list of ipadddress:port of peers of subnet", Flag.Multi),
							new ("client", UNEL, "An address of default client software"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new SubnetAttachment
										{
											Name = First, 
											Client = Snp.Parse(GetString("client")),
											Peers =	Args.Where(i => i.Name == "peer").Select(i => Endpoint.Parse(i.Get<string>())).ToArray()
										};
							};
		return a;
	}
}
