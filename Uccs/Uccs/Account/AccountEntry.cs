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
		public int						LastTransactionId = -1;
		public int						LastEmissionId = -1;
		public int						CandidacyDeclarationRid = -1;

		public HashSet<int>				Transactions = new();

		public AccountAddress			Key => Address;
		public byte[]					GetClusterKey(int n) => ((byte[])Address).Take(n).ToArray();

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

			writer.Write7BitEncodedInt(LastTransactionId);
			writer.Write7BitEncodedInt(LastEmissionId);
			writer.Write7BitEncodedInt(CandidacyDeclarationRid);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);

			LastTransactionId			= reader.Read7BitEncodedInt();
			LastEmissionId				= reader.Read7BitEncodedInt();
			CandidacyDeclarationRid		= reader.Read7BitEncodedInt();
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
