using System.IO;

namespace Uccs.Net
{
	public class Account : IBinarySerializable
	{
		public EntityId					Id { get; set; }
		public AccountAddress			Address { get; set; }
		public Money					STBalance { get; set; }
		public Money					EUBalance { get; set; }
		public Money					MRBalance { get; set; }
		//public Money					Bail { get; set; }
		//public BailStatus				BailStatus { get; set; }
		public int						LastTransactionNid { get; set; } = -1;
		public int						LastEmissionId  { get; set; } = -1;
		//public int						CandidacyDeclarationRid  { get; set; } = -1;

		public Money					AverageUptime { get; set; }

		public virtual void Write(BinaryWriter writer)
		{
			writer.Write(Address);
			writer.Write(STBalance);
			writer.Write(EUBalance);
			writer.Write(MRBalance);
			writer.Write7BitEncodedInt(LastTransactionNid);
			writer.Write7BitEncodedInt(LastEmissionId);
			writer.Write(AverageUptime);
			
			//writer.Write7BitEncodedInt(CandidacyDeclarationRid);
			//
			//if(CandidacyDeclarationRid != -1)
			//{
			//	writer.Write(Bail);
			//}
		}

		public virtual void Read(BinaryReader reader)
		{
			Address				= reader.ReadAccount();
			STBalance 			= reader.Read<Money>();
			EUBalance 			= reader.Read<Money>();
			MRBalance 			= reader.Read<Money>();
			LastTransactionNid	= reader.Read7BitEncodedInt();
			LastEmissionId		= reader.Read7BitEncodedInt();
			AverageUptime		= reader.Read<Money>();

			//CandidacyDeclarationRid	= reader.Read7BitEncodedInt();
			//
			//if(CandidacyDeclarationRid != -1)
			//{
			//	Bail = reader.Read<Money>();
			//}
		}
	}
}
