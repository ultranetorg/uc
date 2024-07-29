namespace Uccs.Rdn
{
	public class Consil : IBinarySerializable
 	{
 		public Unit				PerByteSTFee;
		public AccountAddress[]	Analyzers;


		public void Read(BinaryReader reader)
		{
			PerByteSTFee	= reader.Read<Unit>();
			Analyzers		= reader.ReadArray<AccountAddress>();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(PerByteSTFee);
			writer.Write(Analyzers);
		}

		public Consil Clone()
		{
			return new Consil {	PerByteSTFee= PerByteSTFee, 
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
		public Urr					Release { get; set; }
		public Unit					STPayment { get; set; }
		public Unit					EUPayment { get; set; }
		public Ura					Consil	{ get; set; }
		public AnalyzerResult[]		Results { get; set; }

		public override string ToString()
		{
			return $"{Release}, STPayment={STPayment}, EUPayment={EUPayment}, Consil={Consil}, Results={Results.Length}";
		}

		public void Read(BinaryReader reader)
		{
			Release		= reader.ReadVirtual<Urr>();
			Consil		= reader.Read<Ura>();
			STPayment	= reader.Read<Unit>();
			EUPayment	= reader.Read<Unit>();
			Results		= reader.ReadArray(() => new AnalyzerResult { Analyzer = reader.ReadByte(), 
																	  Result = (AnalysisResult)reader.ReadByte() });
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Consil);
			writer.Write(STPayment);
			writer.Write(EUPayment);
			writer.Write(Results, i => { writer.Write(i.Analyzer);
										 writer.Write((byte)i.Result); });
		}

		public Analysis Clone()
		{
			return new Analysis {Release	= Release, 
								 Consil		= Consil,	
								 STPayment	= STPayment, 
								 EUPayment	= EUPayment, 
								 Results	= Results.Clone() as AnalyzerResult[]};
		}
	}
}
