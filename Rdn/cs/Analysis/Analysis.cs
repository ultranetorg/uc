﻿namespace Uccs.Rdn;

public class Consil : IBinarySerializable
{
	public long				PerByteBYFee;
	public AccountAddress[]	Analyzers;


	public void Read(BinaryReader reader)
	{
		PerByteBYFee	= reader.Read7BitEncodedInt64();
		Analyzers		= reader.ReadArray<AccountAddress>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt64(PerByteBYFee);
		writer.Write(Analyzers);
	}

	public Consil Clone()
	{
		return new Consil {	PerByteBYFee= PerByteBYFee, 
							Analyzers	= Analyzers.Clone() as AccountAddress[]};
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
	public long						ECPayment { get; set; }
	public long						BYPayment { get; set; }
	public EntityId					Consil	{ get; set; }
	public AnalyzerResult[]			Results { get; set; }

	public override string ToString()
	{
		return $"{Release}, STPayment={BYPayment}, EUPayment={ECPayment}, Consil={Consil}, Results={Results.Length}";
	}

	public void Read(BinaryReader reader)
	{
		Release		= Urr.ReadVirtual(reader);
		Consil		= reader.Read<EntityId>();
		BYPayment	= reader.Read7BitEncodedInt64();
		ECPayment	= reader.Read7BitEncodedInt64();
		Results		= reader.ReadArray(() => new AnalyzerResult { Analyzer = reader.ReadByte(), 
																  Result = reader.Read<AnalysisResult>() });
	}

	public void Write(BinaryWriter writer)
	{
		Release.WriteVirtual(writer);
		writer.Write(Consil);
		writer.Write7BitEncodedInt64(BYPayment);
		writer.Write7BitEncodedInt64(ECPayment);
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
