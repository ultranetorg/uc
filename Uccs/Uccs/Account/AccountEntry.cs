using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nethereum.Model;

namespace Uccs.Net
{
	public enum BailStatus
	{
		Null, Active, Siezed
	}

	public class AccountEntry : Account, ITableEntry<AccountAddress>
	{
		public HashSet<int>				Transactions = new();

		Mcv								Mcv;
		public AccountAddress			Key => Address;
		public byte[]					GetClusterKey(int n) => Address.Bytes.Take(n).ToArray();

		public AccountEntry()
		{
		}

		public AccountEntry(Mcv chain)
		{
			Mcv = chain;
		}

		public AccountEntry Clone()
		{
			return new AccountEntry(Mcv){	Address = Address,
											Balance = Balance,
											Bail = Bail,
											BailStatus = BailStatus,
											LastTransactionId = LastTransactionId,
											LastEmissionId = LastEmissionId,
											CandidacyDeclarationRid = CandidacyDeclarationRid,
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
			//w.Write(_Authors != null);
			//
			//if(_Authors != null)
			//{
			//	w.Write(_Authors, i => w.WriteUtf8(i));
			//}
		}

		public void ReadMore(BinaryReader r)
		{
			if(Mcv.Roles.HasFlag(Role.Chain))
			{
				Transactions = r.ReadHashSet(() => r.Read7BitEncodedInt());
			}
			//if(r.ReadBoolean())
			//{
			//	_Authors = r.ReadList(() => r.ReadUtf8());
			//}
		}

		public XonDocument ToXon()
		{
			var d = new XonDocument(new XonTextValueSerializator());

			d.Add("Address").Value					= Address;
			d.Add("LastTransactionId").Value		= LastTransactionId;
			d.Add("Balance").Value					= Balance;
			d.Add("LastEmissionId").Value			= LastEmissionId;
			d.Add("CandidacyDeclarationRid").Value	= CandidacyDeclarationRid;
			d.Add("Bail").Value						= Bail;
			d.Add("BailStatus").Value				= BailStatus;

			return d;
		}

	}
}
