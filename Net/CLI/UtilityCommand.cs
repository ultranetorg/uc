using System.Reflection;

namespace Uccs.Net;

public class UtilityCommand : McvCommand
{
	public UtilityCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Transfer()
	{
		const string from = nameof(from);
		const string to = nameof(to);
		const string e = nameof(e);
		const string en = nameof(en);
		const string st = nameof(st);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "t";

		a.Description = "Send utility from one account to another.";
		a.Arguments =  [new (from,		EA, "Source entity type and id where utility are credited from"),
						new (to,		EA, "Destination entity type and id where utility are credited to"),
						new (e,		EC, "Amount of energy of the current year to be transferred", Flag.Optional),
						new (en,	EC, "Amount of energy of the next year to be transferred", Flag.Optional),
						new (st,		ST, "Amount of space-time to be transferred", Flag.Optional),
						ByArgument("Name of the user eligible to withdraw utility(s) from the source entity")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var f = EntityAddress.Parse(Cli.Net.Tables, GetString(from));
								var t = EntityAddress.Parse(Cli.Net.Tables, GetString(to));

								return new UtilityTransfer( f.Table, 
															f.Id,
															t.Table, 
															t.Id,
															GetLong(e, 0), 
															GetLong(en, 0), 
															GetSpacetime(st, 0));
							};

		return a;
	}
}
