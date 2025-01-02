using System.Reflection;

namespace Uccs.Net;

public class BandwidthCommand : McvCommand
{
	public BandwidthCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Allocate()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		a.Name = "a";
		a.Help = new Help	{ 
								Description = "Allocate execution bandwidth",
								Syntax = $"{Keyword} {a.NamesSyntax} bandwidth={EC} days={INT} {SignerArg}={AA}",

								Arguments =	[
												new ("bandwidth", "Amount of EC allocated per day"),
												new ("days", "Number of days to allocate bandwidth for"),
											],

								Examples =	[
												new (null, $"{Keyword} {a.Name} bandwidth=100 days=2 {SignerArg}={AA.Example}")
											]
							};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new BandwidthAllocation {Bandwidth = GetMoney("bandwidth"), Days = (short)GetInt("days")};
							};

		return a;
	}
}
