using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

				case "r" :
				case "register" :
				{
					return new AnalysisRegistration { Negatives = Args.Get<string>("negatives", null)?.Split(' ')?.Select(i => i.HexToByteArray()),
													  Positives = Args.Get<string>("positives", null)?.Split(' ').Select(i => i.HexToByteArray()),
													  Rejections = Args.Get<string>("rejections", null)?.Split(' ').Select(i => i.HexToByteArray()) };
				}

				case "i" : 
				case "info" : 
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
