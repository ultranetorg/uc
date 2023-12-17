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
				case "o" :
				case "order" :
				{
					return new AnalysisOrder {Release = Args.Nodes[1].Name.HexToByteArray(), Fee = GetMoney("fee")};
				}

				case "rr" :
				case "registerresult" :
				{
					return new AnalysisResultRegistration {Release = Args.Nodes[1].Name.HexToByteArray(), Result = Enum.Parse<AnalysisResult>(GetString("result")) };;
				}

				case "e" : 
				case "entity" : 
				{	
					var rp = Program.Rdc<AnalysisResponse>(new AnalysisRequest {Release = Args.Nodes[1].Name.HexToByteArray()});
	
					Dump(rp.Analysis);
						
					return rp.Analysis;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
