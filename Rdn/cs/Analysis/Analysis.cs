namespace Uccs.Rdn
{
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
		public ExecutionReservation[]	ECPayment { get; set; }
		public long						BYPayment { get; set; }
		public Ura						Consil	{ get; set; }
		public AnalyzerResult[]			Results { get; set; }

		public override string ToString()
		{
			return $"{Release}, STPayment={BYPayment}, EUPayment={ECPayment}, Consil={Consil}, Results={Results.Length}";
		}

		public void Read(BinaryReader reader)
		{
			Release		= reader.ReadVirtual<Urr>();
			Consil		= reader.Read<Ura>();
			BYPayment	= reader.Read7BitEncodedInt64();
			ECPayment	= reader.ReadArray<ExecutionReservation>();
			Results		= reader.ReadArray(() => new AnalyzerResult { Analyzer = reader.ReadByte(), 
																	  Result = (AnalysisResult)reader.ReadByte() });
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Consil);
			writer.Write7BitEncodedInt64(BYPayment);
			writer.Write(ECPayment);
			writer.Write(Results, i => { writer.Write(i.Analyzer);
										 writer.Write((byte)i.Result); });
		}

		public Analysis Clone()
		{
			return new Analysis {Release	= Release, 
								 Consil		= Consil,	
								 ECPayment	= ECPayment.ToArray(),
								 BYPayment	= BYPayment, 
								 Results	= Results.Clone() as AnalyzerResult[]};
		}
	}
}
