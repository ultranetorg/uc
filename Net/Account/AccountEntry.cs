using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public class AccountEntry : Account, ITableEntry<AccountAddress>
	{
		public EntityId					Id { get; set; }
		public AccountAddress			Key => Address;
		
		[JsonIgnore]
		public bool						New { get; set; }
		
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
											Balance = Balance,
											//Bail = Bail,
											LastTransactionNid = LastTransactionNid,
											LastEmissionId = LastEmissionId,
											//CandidacyDeclarationRid = CandidacyDeclarationRid,
											Transactions = Mcv.Roles.HasFlag(Role.Chain) ? new HashSet<int>(Transactions) : null};
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
			if(Mcv.Roles.HasFlag(Role.Chain))
			{
				w.Write(Transactions);
			}
		}

		public void ReadMore(BinaryReader r)
		{
			if(Mcv.Roles.HasFlag(Role.Chain))
			{
				Transactions = r.ReadHashSet(() => r.Read7BitEncodedInt());
			}
		}

		public XonDocument ToXon()
		{
			var d = new XonDocument(new XonTextValueSerializator());

			d.Add("Address").Value					= Address;
			d.Add("LastTransactionId").Value		= LastTransactionNid;
			d.Add("Balance").Value					= Balance;
			d.Add("LastEmissionId").Value			= LastEmissionId;

			return d;
		}

	}
}
