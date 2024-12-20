﻿namespace Uccs.Rdn.CLI;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class AnalysisCommand : RdnCommand
{
	public const string Keyword = "analysis";


	public AnalysisCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =
		[
			new ()
			{
				Names = ["u", "update"],
				Help = new Help	{ 
									Title = "ANALYSIS UPDATE",
									Description = "Register an analysis result in Ultranet distributed database",
									Syntax = "analysis u|update URA result=RESULT",

									Arguments = 
									[
										new ("<first>", "Address of analysis resource"), // Assuming "<first>" is a placeholder and needs a correct identifier.
										new ("result", "Negative, Positive, Vulnerable")
									],

									Examples = 
									[
										new (null, "analysis update companyonc/application/1.3.5/analysis result=Negative")
									]
								},

				Execute = () =>	{
									Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

									var r = Rdc(new ResourceRequest(Ura.Parse(Args[0].Name))).Resource;


									return new AnalysisResultUpdation {	Analysis = r.Id, 
																		Result = Enum.Parse<AnalysisResult>(GetString("result"))};;
								}
			}
		];	
	}
}
