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
		public IEnumerable<byte[]>		Rejections { get; set; }
		
		public override string			Description => $"Negatives={{{Negatives.Count()}}}, Positives={{{Positives.Count()}}}";
		public override bool			Valid => Negatives.Any() || Positives.Any();

		public AnalysisRegistration()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Negatives, i => writer.Write(i));
			writer.Write(Positives, i => writer.Write(i));
			writer.Write(Rejections, i => writer.Write(i));
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Negatives = reader.ReadArray(() => reader.ReadHash());
			Positives = reader.ReadArray(() => reader.ReadHash());
			Rejections = reader.ReadArray(() => reader.ReadHash());
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
 				var a = Affect(round, i);

				if(a == null)
				{
					Error = NotFound;
					return;
				}
 
				var r = a.Results.FirstOrDefault(i => i.Analyzer == az.Id);

 				if(r.Result == AnalysisResult.None)
 					a.Results = a.Results.Append(new AnalyzerResult {Analyzer = az.Id, Result = AnalysisResult.Negative}).ToArray();
 				else
					r.Result = AnalysisResult.Negative;
 			}

 			foreach(var i in Positives)
 			{
 				var a = Affect(round, i);

				if(a == null)
				{
					Error = NotFound;
					return;
				}
 
				var j = Array.FindIndex(a.Results, i => i.Analyzer == az.Id);

 				if(j == -1)
 					a.Results = a.Results.Append(new AnalyzerResult {Analyzer = az.Id, Result = AnalysisResult.Positive}).ToArray();
 				else
					a.Results[j].Result = AnalysisResult.Positive;
 			}

 			foreach(var i in Rejections)
 			{
 				var a = Affect(round, i);

				if(a == null)
				{
					Error = NotFound;
					return;
				}
 
				var r = a.Results.FirstOrDefault(i => i.Analyzer == az.Id);

 				if(r.Result == AnalysisResult.None)
 					a.Results = a.Results.Append(new AnalyzerResult {Analyzer = az.Id, Result = AnalysisResult.NotEnoughPrepayment}).ToArray();
 				else
					r.Result = AnalysisResult.NotEnoughPrepayment;
 			}
		}
	}
}
