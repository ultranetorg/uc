using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalysisRegistration : Operation
	{
		public IEnumerable<byte[]>		Negatives { get; set; }
		public IEnumerable<byte[]>		Positives { get; set; }
		//public IEnumerable<byte[]>		Rejections { get; set; }
		
		public override string			Description => $"Negatives={{{Negatives.Count()}}}, Positives={{{Positives.Count()}}}";
		public override bool			Valid => Negatives.Any() || Positives.Any();

		public AnalysisRegistration()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Negatives, i => writer.Write(i));
			writer.Write(Positives, i => writer.Write(i));
			//writer.Write(Rejections, i => writer.Write(i));
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Negatives = reader.ReadArray(() => reader.ReadHash());
			Positives = reader.ReadArray(() => reader.ReadHash());
			//Rejections = reader.ReadArray(() => reader.ReadHash());
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var az = round.Analyzers.Find(i => i.Account == Signer);

			if(az == null)
			{
				Error = NotFound;
				return;
			}

 			foreach(var i in Negatives)
 			{
 				var an = Affect(round, i);

				if(an == null)
				{
					Error = NotFound;
					return;
				}
 
				var r = an.Results.FirstOrDefault(i => i.AnalyzerId == az.Id);

 				if(r.Result == AnalysisResult.None)
 				{
					an.Results = an.Results.Append(new AnalyzerResult {AnalyzerId = az.Id, Result = AnalysisResult.Negative}).ToArray();
					Affect(round, Signer).Balance += an.Fee/an.Consil;
				}
 				else
					r.Result = AnalysisResult.Negative;
 			}

 			foreach(var i in Positives)
 			{
 				var an = Affect(round, i);

				if(an == null)
				{
					Error = NotFound;
					return;
				}
 
				var j = Array.FindIndex(an.Results, i => i.AnalyzerId == az.Id);

 				if(j == -1)
				{
					an.Results = an.Results.Append(new AnalyzerResult {AnalyzerId = az.Id, Result = AnalysisResult.Positive}).ToArray();
					Affect(round, Signer).Balance += an.Fee/an.Consil;
				}
 				else
					an.Results[j].Result = AnalysisResult.Positive;
 			}

 			//foreach(var i in Rejections)
 			//{
 			//	var an = Affect(round, i);
			//
			//	if(an == null)
			//	{
			//		Error = NotFound;
			//		return;
			//	}
 			//
			//	var r = an.Results.FirstOrDefault(i => i.AnalyzerId == az.Id);
			//
 			//	if(r.Result == AnalysisResult.None)
			//	{
			//		an.Results = an.Results.Append(new AnalyzerResult {AnalyzerId = az.Id, Result = AnalysisResult.NotEnoughPrepayment}).ToArray();
			//		Affect(round, Signer).Balance += an.Fee/an.Consil;
			//	}
 			//	else
			//		r.Result = AnalysisResult.NotEnoughPrepayment;
 			//}
		}
	}
}
