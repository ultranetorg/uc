using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nethereum.Model;

namespace UC.Net
{
	public enum BailStatus
	{
		Null, Active, Siezed
	}

	public class AccountEntry : Account, ITableEntry<AccountAddress>
	{
		public int						LastOperationId = -1;
		public int						LastEmissionId = -1;
		public int						CandidacyDeclarationRid = -1;

		public HashSet<int>				Transactions = new();

		public AccountAddress			Key => Address;
		public byte[]					GetClusterKey(int n) => ((byte[])Address).Take(n).ToArray();

		Database						Chain;

		public AccountEntry()
		{
		}

		public AccountEntry(Database chain)
		{
			Chain = chain;
		}

		public AccountEntry Clone()
		{
			return new AccountEntry(Chain){	Address = Address,
											LastOperationId = LastOperationId,
											LastEmissionId = LastEmissionId,
											Balance = Balance,
											CandidacyDeclarationRid = CandidacyDeclarationRid,
											Bail = Bail,
											BailStatus = BailStatus,
											Transactions = Chain.Settings.Database.Chain ? new HashSet<int>(Transactions) : null};
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);

			writer.Write7BitEncodedInt(LastOperationId);
			writer.Write7BitEncodedInt(LastEmissionId);
			writer.Write7BitEncodedInt(CandidacyDeclarationRid);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);

			LastOperationId				= reader.Read7BitEncodedInt();
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
			if(Chain.Settings.Database.Chain)
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
			if(Chain.Settings.Database.Chain)
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
			d.Add("LastOperationId").Value			= LastOperationId;
			d.Add("Balance").Value					= Balance;
			d.Add("LastEmissionId").Value			= LastEmissionId;
			d.Add("CandidacyDeclarationRid").Value	= CandidacyDeclarationRid;
			d.Add("Bail").Value						= Bail;
			d.Add("BailStatus").Value				= BailStatus;

			return d;
		}

	}
}
