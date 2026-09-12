//using System.Reflection;
//
//namespace Uccs.Rdn.CLI;
//
///// <summary>
///// Usage: 
/////		release publish 
///// </summary>
//public class AnalysisCommand : RdnCommand
//{
//	public AnalysisCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
//	{
//	}
//
//	public CommandAction Analysis_Update()
//	{
//		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//		
//		a.Name = "u";
//		a.Description = "Register an analysis result in Ultranet distributed database",
//						Arguments =	[
//										new (null, RA, "Address of analysis resource"), // Assuming "<first>" is a placeholder and needs a correct identifier.
//										new ("result", ANRESULT, "Negative, Positive, Vulnerable")
//									],
//						};
//
//		a.Execute = () =>	{
//								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
//
//								var r = Ppc(new ResourceRequest(Ura.Parse(Args[0].Name))).Resource;
//
//								return new AnalysisResultUpdation {	Analysis = r.Id, 
//																	Result = Enum.Parse<AnalysisResult>(GetString("result"))};;
//							};
//
//		return a;
//	}
//}
