using System.Reflection;

namespace Uccs.Rdn.CLI;

public class SubnetCommand : RdnCommand
{
	public SubnetCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Attach()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "a";
		a.Description = "Requests subnet attachment voting";
		a.Arguments =	[
							new (null, SNN, "A name of a subnet to attach", ArgumentFlag.First),
							new ("peer", IP, "A list of ipadddress:port of peers of subnet", ArgumentFlag.Multi),
							new ("client", SNQ, "An address of default client software"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new FriendAttachment
										{
											Name = First, 
											Client = Snq.Parse(GetString("client")),
											Peers =	Args.Where(i => i.Name == "peer").Select(i => Endpoint.Parse(i.Get<string>())).ToArray()
										};
							};
		return a;
	}
}
