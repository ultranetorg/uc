using System;
using System.Linq;
using Nethereum.Hex.HexConvertors.Extensions;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class AnalysisCommand : Command
	{
		public const string Keyword = "analysis";

		public AnalysisCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "u" :
				case "update" :
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AnalysisResultUpdation {	Resource = GetResourceAddress("resource"), 
														Release = GetReleaseAddress("release"), 
														Consil = GetResourceAddress("consil"), 
														Result = Enum.Parse<AnalysisResult>(GetString("result"))};;
				}
// 
// 				case "e" : 
// 				case "entity" : 
// 				{	
// 					Workflow.CancelAfter(RdcQueryTimeout);
// 
// 					var rp = Rdc<AnalysisResponse>(new AnalysisRequest {Release = ReleaseAddress.Parse(Args.Nodes[1].Name)});
// 	
// 					Dump(rp.Analysis);
// 						
// 					return rp.Analysis;
// 				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
