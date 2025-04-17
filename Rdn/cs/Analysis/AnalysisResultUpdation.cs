namespace Uccs.Rdn;

public class AnalysisResultUpdation : RdnOperation
{
	//public ResourceId		Resource { get; set; }
	public EntityId			Analysis { get; set; }
	public AnalysisResult	Result { get; set; }
	
	public override string	Explanation => $"Analysis={Analysis}, Result={Result}";
	public override bool	IsValid(McvNet net) => true;

	public AnalysisResultUpdation()
	{
	}
	
	public override void Write(BinaryWriter writer)
	{
		//writer.Write(Resource);
		writer.Write(Analysis);
		writer.Write(Result);
	}
	
	public override void Read(BinaryReader reader)
	{
		//Resource = reader.Read<ResourceId>();
		Analysis = reader.Read<EntityId>();
		Result	 = reader.Read<AnalysisResult>();
	}

	public override void Execute(RdnExecution execution)
	{
		//if(Require(round, null, Resource, out var d, out var r) == false)
		//	return;

		if(RequireResource(execution, Analysis, out var ad, out var ar) == false)
			return;

		if(ar.Data == null)
		{
			Error = NoData;
			return;
		}

		var c = execution.FindResource(ar.Data.Read<Analysis>().Consil)?.Data?.Read<Consil>();

		if(c == null)
		{
			Error = NotFound;
			return;
		}

		var aix = Array.IndexOf(c.Analyzers, Signer.Address);

		if(aix == -1)
		{
			Error = NotFound;
			return;
		}

		ar = execution.AffectResource(ad, ar.Address.Resource);

		var an = ar.Data.Read<Analysis>();
		 
		var j = Array.FindIndex(an.Results, i => i.Analyzer == aix);
		an.Results ??= [];
		
 		if(j == -1)
		{
			an.Results = [..an.Results, new AnalyzerResult {Analyzer = (byte)aix, Result = Result}];

			Signer.Energy	  += an.EnergyReward / c.Analyzers.Length;
			Signer.Spacetime  += an.SpacetimeReward / c.Analyzers.Length;

			var o = execution.AffectAccount(ad.Owner);

			o.Energy	-= an.EnergyReward / c.Analyzers.Length;
			o.Spacetime -= an.SpacetimeReward / c.Analyzers.Length;

			SpacetimeSpenders.Add(o);
			EnergySpenders.Add(o);
		}
 		else
		{	
			an.Results = [..an.Results];
			an.Results[j].Result = Result;
		}
	}
}
