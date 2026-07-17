using System.Reflection;

namespace Uccs.Rdn.CLI;

public class SubnetCommand : RdnCommand
{
	public SubnetCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public SubnetCommand()
	{
	}

	public CommandAction Attach_A()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Requests subnet attachment voting";
		a.Arguments =	[
							new (NameKeyword, NETNAME, "Name of a subnet to attach"),
							new ("peer", IP, "List of IPAdddress:Port of peers of subnet", ArgumentFlag.Multi),
							new ("client", SNQ, "Address of default client software"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new FriendAttachment
										{
											Name = Name, 
											Client = Snq.Parse(GetString("client")),
											Peers =	Args.Where(i => i.Name == "peer").Select(i => Endpoint.Parse(i.Get<string>())).ToArray()
										};
							};
		return a;
	}
}
