using System.Reflection;

namespace Uccs.Rdn.CLI;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class AnalysisCommand : RdnCommand
{
	public AnalysisCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Analysis_Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		a.Name = "u";
		a.Help = new() {Description = "Register an analysis result in Ultranet distributed database",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA} result=RESULT",

						Arguments =	[
										new ("<first>", "Address of analysis resource"), // Assuming "<first>" is a placeholder and needs a correct identifier.
										new ("result", "Negative, Positive, Vulnerable")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} companyonc/application/1.3.5/analysis result=Negative")
									]
						};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var r = Ppc(new ResourceRequest(Ura.Parse(Args[0].Name))).Resource;


								return new AnalysisResultUpdation {	Analysis = r.Id, 
																	Result = Enum.Parse<AnalysisResult>(GetString("result"))};;
							};

		return a;
	}
}
