using System.IO;

namespace Uccs.Net
{
	public class Account : IBinarySerializable
	{
		public AccountAddress			Address { get; set; }
		public Money					Balance { get; set; }
		public Money					Bail { get; set; }
		//public BailStatus				BailStatus { get; set; }
		public int						LastTransactionNid { get; set; } = -1;
		public int						LastEmissionId  { get; set; } = -1;
		public int						CandidacyDeclarationRid  { get; set; } = -1;

		public virtual void Write(BinaryWriter writer)
		{
			writer.Write(Address);
			writer.Write(Balance);
			
			writer.Write7BitEncodedInt(CandidacyDeclarationRid);
			
			if(CandidacyDeclarationRid != -1)
			{
				writer.Write(Bail);
			}

			writer.Write7BitEncodedInt(LastTransactionNid);
			writer.Write7BitEncodedInt(LastEmissionId);
		}

		public virtual void Read(BinaryReader reader)
		{
			Address		= reader.ReadAccount();
			Balance		= reader.ReadMoney();

			CandidacyDeclarationRid	= reader.Read7BitEncodedInt();

			if(CandidacyDeclarationRid != -1)
			{
				Bail = reader.ReadMoney();
			}

			LastTransactionNid	= reader.Read7BitEncodedInt();
			LastEmissionId		= reader.Read7BitEncodedInt();
		}
	}
}
