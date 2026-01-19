using System.Reflection;

namespace Uccs.Net;

public class BandwidthCommand : McvCommand
{
	public BandwidthCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Allocate()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Name = "a";
		a.Description = "Allocate execution bandwidth";
		a.Arguments =	[
						new ("bandwidth", EC, "Amount of EC allocated per day"),
						new ("days", INT, "Number of days to allocate bandwidth for"),
						ByArgument()
					];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new BandwidthAllocation {Bandwidth = GetUInt16("bandwidth"), Days = GetUInt16("days")};
							};

		return a;
	}
}
