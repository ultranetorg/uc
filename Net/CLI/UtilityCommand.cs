using System.Reflection;

namespace Uccs.Net;

public class UtilityCommand : McvCommand
{
	public UtilityCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Transfer()
	{
		var from = "from";
		var to = "to";

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "t";

		a.Description = "Send utility from one account to another.";
		a.Arguments =  [new (from, EA, "Entity type and id where funds are credited from"),
						new (to, EA, "Entity type and id where funds are credited to"),
						new ("st", ST, "Amount of space-time to be transferred", Flag.Optional),
						new ("ec", EC, "Amount of Execution Cycles of the current year to be transferred", Flag.Optional),
						new ("ecnext", EC, "Amount of Execution Cycles of next year to be transferred", Flag.Optional),
						SignerArgument("Account public address where funds are debited from")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var info = Ppc(new InfoRequest {});
								
								var toe = GetString(to).Split('/')[1];

								AutoId toid;

								if(toe == "lastcreated")
									toid = AutoId.LastCreated;
								else
									toid = AutoId.Parse(toe);

								return new UtilityTransfer( info.Tables[GetString(from).Split('/')[0]], 
															AutoId.Parse(GetString(from).Split('/')[1]),
															info.Tables[GetString(to).Split('/')[0]], 
															toid,
															GetEC("e", 0), 
															GetEC("en", 0), 
															GetBD("st", 0));
							};

		return a;
	}
}
