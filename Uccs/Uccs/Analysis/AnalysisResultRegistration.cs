using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalysisResultRegistration : Operation
	{
		public ResourceAddress	Resource { get; set; }
		public ReleaseAddress	Release { get; set; }
		public AnalysisResult	Result { get; set; }
		
		public override string	Description => $"Resource={Resource}, Release={Release}, Result={Result}";
		public override bool	Valid => true;

		public AnalysisResultRegistration()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write(Release);
			writer.Write((byte)Result);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Release = reader.Read<ReleaseAddress>(ReleaseAddress.FromType);
			Result = (AnalysisResult)reader.ReadByte();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var az = round.Analyzers.Find(i => i.Account == Signer);

			if(az == null)
			{
				Error = NotFound;
				return;
			}

			var a = mcv.Authors.Find(Resource.Author, round.Id);

			if(a == null)
			{
				Error = NotFound;
				return;
			}

			if(Author.IsExpired(a, round.ConsensusTime))
			{
				Error = Expired;
				return;
			}

			var e = a.Resources.FirstOrDefault(i => i.Address == Resource);
					
			if(e == null)
			{
				Error = NotFound;
				return;
			}

			a = Affect(round, Resource.Author);
			var r = a.AffectResource(Resource.Resource);
			
			r.AnalysisResults ??= [];
 
			var j = Array.FindIndex(r.AnalysisResults, i => i.AnalyzerId == az.Id);
			
 			if(j == -1)
			{
				r.AnalysisResults = r.AnalysisResults.Append(new AnalyzerResult {AnalyzerId = az.Id, Result = Result}).ToArray();
				Affect(round, Signer).Balance += r.AnalysisPayment/r.AnalysisConsil;
			}
 			else
				r.AnalysisResults[j].Result = Result;
 			

 			//foreach(var i in Positives)
 			//{
 			//	var an = Affect(round, i);
			//
			//	if(an == null)
			//	{
			//		Error = NotFound;
			//		return;
			//	}
 			//
			//	var j = Array.FindIndex(an.Results, i => i.AnalyzerId == az.Id);
			//
 			//	if(j == -1)
			//	{
			//		an.Results = an.Results.Append(new AnalyzerResult {AnalyzerId = az.Id, Result = AnalysisResult.Positive}).ToArray();
			//		Affect(round, Signer).Balance += an.Fee/an.Consil;
			//	}
 			//	else
			//		an.Results[j].Result = AnalysisResult.Positive;
 			//}

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
