using System;
using System.Collections.Generic;
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


		public AnalysisCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =
			[
				new ()
				{
					Names = ["u", "update"],
					Help = new Help	{ 
										Title = "ANALYSIS UPDATE",
										Description = "Register an analysis result in Ultranet distributed database",
										Syntax = "analysis u|update a=URA|id=RID result=RESULT",

										Arguments = 
										[
											new ("a/id", "Address/Id of analysis resource"), // Assuming "<first>" is a placeholder and needs a correct identifier.
											new ("result", "Negative, Positive, Vulnerable")
										],

										Examples = 
										[
											new (null, "analysis update F371BC4A311F2B009EEF952DD83CA80E2B60026C8E935592D0F9C308453C813E result=Negative")
										]
									},

					Execute = () =>	{
										Flow.CancelAfter(RdcTransactingTimeout);

										var r = Rdc(new ResourceRequest(ResourceIdentifier)).Resource;


										return new AnalysisResultUpdation {	Analysis = r.Id, 
																			Result = Enum.Parse<AnalysisResult>(GetString("result"))};;
									}
				}
			];	
		}
	}
}
