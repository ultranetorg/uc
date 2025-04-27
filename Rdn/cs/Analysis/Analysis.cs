namespace Uccs.Rdn;

public class Consil : IBinarySerializable
{
	public long				SizeEnergyFeeMinimum;
	public long				ResultEnergyFeeMinimum;
	public long				ResultSpacetimeFeeMinimum;
	public AccountAddress[]	Analyzers;

	public const int		AnalyzersMaximum = 256;

	public void Read(BinaryReader reader)
	{
		ResultEnergyFeeMinimum		= reader.Read7BitEncodedInt64();
		SizeEnergyFeeMinimum		= reader.Read7BitEncodedInt64();
		ResultSpacetimeFeeMinimum	= reader.Read7BitEncodedInt64();
		Analyzers					= reader.ReadArray<AccountAddress>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt64(SizeEnergyFeeMinimum);
		writer.Write7BitEncodedInt64(ResultEnergyFeeMinimum);
		writer.Write7BitEncodedInt64(ResultSpacetimeFeeMinimum);
		writer.Write(Analyzers);
	}

	public Consil Clone()
	{
		return new Consil()	{	SizeEnergyFeeMinimum		= SizeEnergyFeeMinimum, 
								ResultSpacetimeFeeMinimum	= ResultSpacetimeFeeMinimum, 
								ResultEnergyFeeMinimum		= ResultEnergyFeeMinimum, 
								Analyzers					= Analyzers
							};
	}
}

public enum AnalysisResult : byte
{
	None,
	Negative,
	Positive,
	Vulnerable,
}

public struct AnalyzerResult
{
	public byte				Analyzer { get; set; }
	public AnalysisResult	Result { get; set; }

	public override string ToString()
	{
		return $"Analyzer={Analyzer}, Result={Result}";
	}
}

public class Analysis : IBinarySerializable
{
	public Urr						Release { get; set; }
	public long						EnergyReward { get; set; }
	public long						SpacetimeReward { get; set; }
	public EntityId					Consil	{ get; set; }
	public AnalyzerResult[]			Results { get; set; }

	public override string ToString()
	{
		return $"{Release}, EnergyReward={EnergyReward}, SpacetimeReward={SpacetimeReward}, Consil={Consil}, Results={Results.Length}";
	}

	public void Read(BinaryReader reader)
	{
		Release			= Urr.ReadVirtual(reader);
		Consil			= reader.Read<EntityId>();
		EnergyReward	= reader.Read7BitEncodedInt64();
		SpacetimeReward	= reader.Read7BitEncodedInt64();
		Results			= reader.ReadArray(() => new AnalyzerResult {Analyzer = reader.ReadByte(), 
																	 Result = reader.Read<AnalysisResult>() });
	}

	public void Write(BinaryWriter writer)
	{
		Release.WriteVirtual(writer);
		writer.Write(Consil);
		writer.Write7BitEncodedInt64(EnergyReward);
		writer.Write7BitEncodedInt64(SpacetimeReward);
		writer.Write(Results, i => { writer.Write(i.Analyzer);
									 writer.Write(i.Result); });
	}

//	public Analysis Clone()
//	{
//		return new Analysis {Release	= Release, 
//							 Consil		= Consil,	
//							 ECPayment	= ECPayment.ToArray(),
//							 BYPayment	= BYPayment, 
//							 Results	= Results.Clone() as AnalyzerResult[]};
//	}
}
