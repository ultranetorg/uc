using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalysisResultUpdation : Operation
	{
		public ResourceAddress	Resource { get; set; }
		public int				Meta { get; set; }
		public AnalysisResult	Result { get; set; }
		
		public override string	Description => $"Resource={Resource}, Meta={Meta}, Result={Result}";
		public override bool	Valid => true;

		public AnalysisResultUpdation()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write7BitEncodedInt(Meta);
			writer.Write((byte)Result);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Meta	 = reader.Read7BitEncodedInt();
			Result	 = (AnalysisResult)reader.ReadByte();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(Require(round, Resource, out var a, out var r) == false)
				return;

			var rr = r.Metas.FirstOrDefault(i => i.Data.Interpretation is Analysis an && i.Id == Meta);

			if(rr == null)
			{
				Error = NotFound;
				return;
			}

			var c = mcv.Authors.FindResource((rr.Data.Interpretation as Analysis).Consil, round.Id)?.Data.Interpretation as Consil;

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
			rr = r.AffectMeta(rr.Owner, rr.Data);

			var an = rr.Data.Interpretation as Analysis;
			an.Results ??= [];
			 
			var j = Array.FindIndex(an.Results, i => i.Analyzer == aix);
			
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
