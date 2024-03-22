using System.IO;

namespace Uccs.Net
{
 	public class Consil : IBinarySerializable
 	{
 		public Money			PerByteFee;
		public AccountAddress[]	Analyzers;

 		public byte[]			Raw {
										get
										{
											var s = new MemoryStream();
											var w = new BinaryWriter(s);
											
											Write(w);
											
											return s.ToArray();
										}
									}

		public void Read(BinaryReader reader)
		{
			PerByteFee	= reader.Read<Money>();
			Analyzers	= reader.ReadArray<AccountAddress>();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(PerByteFee);
			writer.Write(Analyzers);
		}

		public Consil Clone()
		{
			return new Consil {	PerByteFee	= PerByteFee, 
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
		public ReleaseAddress		Release { get; set; }
		public Money				Payment { get; set; }
		public ResourceAddress		Consil	{ get; set; }
		public AnalyzerResult[]		Results { get; set; }

 		public byte[]				Raw {
											get
											{
												var s = new MemoryStream();
												var w = new BinaryWriter(s);
												
												Write(w);
												
												return s.ToArray();
											}
										}

		public override string ToString()
		{
			return $"{Release}, Payment={Payment}, Consil={Consil}, Results={Results.Length}";
		}

		public void Read(BinaryReader reader)
		{
			Release = reader.Read<ReleaseAddress>(ReleaseAddress.FromType);
			Consil	= reader.Read<ResourceAddress>();
			Payment	= reader.Read<Money>();
			Results	= reader.ReadArray(() => new AnalyzerResult { Analyzer = reader.ReadByte(), 
																  Result = (AnalysisResult)reader.ReadByte() });
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Consil);
			writer.Write(Payment);
			writer.Write(Results, i => { writer.Write(i.Analyzer);
										 writer.Write((byte)i.Result); });
		}

		public Analysis Clone()
		{
			return new Analysis {Release	= Release, 
								 Payment	= Payment, 
								 Consil		= Consil,	
								 Results	= Results.Clone() as AnalyzerResult[]};
		}
	}
}
