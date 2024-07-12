using System.Collections.Generic;
using System.IO;

namespace Uccs.Net
{
	public class AccountEntry : Account, ITableEntry<AccountAddress>
	{
		public AccountAddress			Key => Address;
		
		//[JsonIgnore]
		public bool						New;
		
		public HashSet<int>				Transactions = new();
		Mcv								Mcv;

		public AccountEntry()
		{
		}

		public AccountEntry(Mcv chain)
		{
			Mcv = chain;
		}

		public AccountEntry Clone()
		{
			return new AccountEntry(Mcv){	Id = Id,
											Address = Address,
											STBalance = STBalance,
											EUBalance = EUBalance,
											MRBalance = MRBalance,
											//Bail = Bail,
											LastTransactionNid = LastTransactionNid,
											LastEmissionId = LastEmissionId,
											//CandidacyDeclarationRid = CandidacyDeclarationRid,
											Transactions = Mcv.Settings.Base?.Chain != null  ? new HashSet<int>(Transactions) : null};
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);

		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
		}

		public void WriteMain(BinaryWriter w)
		{
			Write(w);

		}

		public void ReadMain(BinaryReader r)
		{
			Read(r);
		}

		public void WriteMore(BinaryWriter w)
		{
			if(Mcv.Settings.Base?.Chain != null)
			{
				w.Write(Transactions);
			}
		}

		public void ReadMore(BinaryReader r)
		{
			if(Mcv.Settings.Base?.Chain != null)
			{
				Transactions = r.ReadHashSet(() => r.Read7BitEncodedInt());
			}
		}
	}
}
