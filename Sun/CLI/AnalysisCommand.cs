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
				case "c" :
				case "create" :
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AnalysisOrder {Release = Args.Nodes[1].Name.FromHex(), Fee = GetMoney("fee")};
				}

				case "r" :
				case "update" :
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AnalysisResultRegistration {Release = Args.Nodes[1].Name.FromHex(), Result = Enum.Parse<AnalysisResult>(GetString("result")) };;
				}

				case "e" : 
				case "entity" : 
				{	
					Workflow.CancelAfter(RdcQueryTimeout);

					var rp = Rdc<AnalysisResponse>(new AnalysisRequest {Release = Args.Nodes[1].Name.FromHex()});
	
					Dump(rp.Analysis);
						
					return rp.Analysis;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
