using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalysisResultUpdation : RdsOperation
	{
		//public ResourceId		Resource { get; set; }
		public ResourceId		Analysis { get; set; }
		public AnalysisResult	Result { get; set; }
		
		public override string	Description => $"Analysis={Analysis}, Result={Result}";
		public override bool	IsValid(Mcv mcv) => true;

		public AnalysisResultUpdation()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			//writer.Write(Resource);
			writer.Write(Analysis);
			writer.Write((byte)Result);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			//Resource = reader.Read<ResourceId>();
			Analysis = reader.Read<ResourceId>();
			Result	 = (AnalysisResult)reader.ReadByte();
		}

		public override void Execute(Rds mcv, RdsRound round)
		{
			//if(Require(round, null, Resource, out var d, out var r) == false)
			//	return;

			if(Require(round, null, Analysis, out var ad, out var ar) == false)
				return;

			var c = mcv.Domains.FindResource((ar.Data.Interpretation as Analysis).Consil, round.Id)?.Data.Interpretation as Consil;

			if(c == null)
			{
				Error = NotFound;
				return;
			}

			var aix = Array.IndexOf(c.Analyzers, Signer);

			if(aix == -1)
			{
				Error = NotFound;
				return;
			}

			//d = round.AffectDomain(d.Id);
			//r = d.AffectResource(r.Address.Resource);

			ad = round.AffectDomain(ad.Id);
			ar = ad.AffectResource(ar.Address.Resource);

			var an = ar.Data.Interpretation as Analysis;
			 
			var j = Array.FindIndex(an.Results, i => i.Analyzer == aix);
			an.Results ??= [];
			
 			if(j == -1)
			{
				an.Results = an.Results.Append(new AnalyzerResult {Analyzer = (byte)aix, Result = Result}).ToArray();
				Affect(round, Signer).Balance += an.Payment/c.Analyzers.Length;
			}
 			else
				an.Results[j].Result = Result;
		}
	}
}
