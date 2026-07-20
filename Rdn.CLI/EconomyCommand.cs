using System.Reflection;

namespace Uccs.Rdn.CLI;

public class EconomyCommand : RdnCommand
{
	public EconomyCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Cost_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Gets information about current cost of various ULTRANET resources.";

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var c = new CostApc{Years = [1, 5, 10], 
													DomainLengths = [1, 5, 10, 15], 
													Rate = Has("rate") ? Unit.Parse(GetString("rate")) : 1};

								var r = Api<CostApc.Return>(c);

								Report($"");

								Flow.Log.Dump(	r.RentDomain,
												["Domains Rent |>", .. c.DomainLengths.Select(i => $"{i} chars>")],
												[(o, i) => $"{c.Years[i]} year(s) |", .. c.DomainLengths.Select((x, li) => new Func<Unit[], int, object>((j, i) => j[li].ToString()))]);

								Report($"");

								Flow.Log.Dump(	r.RentResource.Append(r.RentResourceForever),
												["Resource Rent>", "Cost>"],
												[(o, i) => i < r.RentResource.Length ? $"{c.Years[i]} year(s)" : "Forever", (o, i) => o.ToString()]);

								Report($"");

								Flow.Log.Dump(	r.RentResourceData.Append(r.RentResourceDataForever),
												["Resource Data Per Byte Rent>", "Cost>"],
												[(o, i) => i < r.RentResourceData.Length ? $"{c.Years[i]} year(s)" : "Forever", (o, i) => o.ToString()]);

								return r;
							};
		return a;
	}
}
