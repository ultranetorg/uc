namespace Uccs.Net;

public class BandwidthCommand : McvCommand
{
	public const string Keyword = "bandwith";

	public BandwidthCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[

						new ()
						{
							Names = ["a", "allocate"],

							Help = new Help	{ 
												Title = "Allocate",
												Description = "Allocate execution bandwidth",
												Syntax = $"{Keyword} ab|allocatebandwidth bandwidth=EC days=NUMBER signer=UAA",

												Arguments =	[
																new ("bandwidth", "Amount of EC allocated per day"),
																new ("days", "Number of days to allocate bandwidth for"),
															],

												Examples =	[
																new (null, $"{Keyword} allocatebandwidth bandwidth=100 days=2 signer=0x0000fffb3f90771533b1739480987cee9f08d754")
															]
											},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new BandwidthAllocation {Bandwidth = GetMoney("bandwidth"), Days = (short)GetInt("days")};
											}
						},

					];
	}
}
