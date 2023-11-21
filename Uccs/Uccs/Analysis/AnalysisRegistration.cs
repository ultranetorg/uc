using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public class AnalysisRegistration : Operation
	{
		public byte[]			Release { get; set; }
		public AnalysisResult	Result { get; set; }
		
		public override string			Description => $"Release={Release.ToHex()}, Result={Result}";
		public override bool			Valid => true;

		public AnalysisRegistration()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write((byte)Result);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Release = reader.ReadHash();
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

 			var an = Affect(round, Release);

			if(an == null)
			{
				Error = NotFound;
				return;
			}
 
			var j = Array.FindIndex(an.Results, i => i.AnalyzerId == az.Id);
			
 			if(j == -1)
			{
				an.Results = an.Results.Append(new AnalyzerResult {AnalyzerId = az.Id, Result = Result}).ToArray();
				Affect(round, Signer).Balance += an.Fee/an.Consil;
			}
 			else
				an.Results[j].Result = Result;
 			

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
