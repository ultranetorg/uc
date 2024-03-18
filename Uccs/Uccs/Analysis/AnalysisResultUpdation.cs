using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalysisResultUpdation : Operation
	{
		public ResourceAddress	Resource { get; set; }
		public ResourceAddress	Analysis { get; set; }
		public AnalysisResult	Result { get; set; }
		
		public override string	Description => $"Resource={Resource}, Analysis={Analysis}, Result={Result}";
		public override bool	Valid => true;

		public AnalysisResultUpdation()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write(Analysis);
			writer.Write((byte)Result);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Analysis = reader.Read<ResourceAddress>();
			Result	 = (AnalysisResult)reader.ReadByte();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(Require(round, null, Resource, out var a, out var r) == false)
				return;

			if(Require(round, null, Analysis, out var aa, out var ar) == false)
				return;

			var c = mcv.Authors.FindResource((ar.Data.Interpretation as Analysis).Consil, round.Id)?.Data.Interpretation as Consil;

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

			a = Affect(round, Resource.Author);
			r = a.AffectResource(Resource.Resource);

			aa = Affect(round, Analysis.Author);
			ar = a.AffectResource(Analysis.Resource);

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
