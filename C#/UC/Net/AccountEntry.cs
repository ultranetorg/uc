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
		Null, OK, Siezed
	}

	public class AccountEntry : TableEntry<Account>
	{
		public Account					Account;
		public int						LastOperationId = -1;
		public Coin						Balance;
		public int						LastEmissionId = -1;
		public int						CandidacyDeclarationRound = -1;
		//public IPAddress[]				IPs = new IPAddress[]{};
		public Coin						Bail;
		public BailStatus				BailStatus;

		public HashSet<int>				Transactions = new();
// 		List<string>					_Authors;
// 		
// 		public List<string>	Authors
// 		{
// 			get
// 			{
// 				if(_Authors == null)
// 				{
// 					_Authors = new();
// 
// 					foreach(var c in Chain.Authors.Clusters)
// 						foreach(var e in c.Entries)
// 							if(e.Owner == Account)
// 								_Authors.Add(e.Name);
// 				}
// 				
// 				return _Authors;
// 			}
// 		}

		public override Account			Key => Account;
		public override byte[]			ClusterKey => ((byte[])Account).Take(ClusterKeyLength).ToArray();

		Database						Chain;

		public AccountEntry(Database chain)
		{
			Chain = chain;
		}

		public AccountEntry Clone()
		{
			return new AccountEntry(Chain){	Account = Account,
											LastOperationId = LastOperationId,
											LastEmissionId = LastEmissionId,
											Balance = Balance,
											CandidacyDeclarationRound = CandidacyDeclarationRound,
											Bail = Bail,
											BailStatus = BailStatus,
											//_Authors = new List<string>(Authors),
											Transactions = Chain.Settings.Database.Chain ? new HashSet<int>(Transactions) : null};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Account);
			w.Write7BitEncodedInt(LastOperationId);
			w.Write7BitEncodedInt(LastEmissionId);
			w.Write(Balance);
			w.Write7BitEncodedInt(CandidacyDeclarationRound);

			if(CandidacyDeclarationRound != -1)
			{
				w.Write(Bail);
				w.Write((byte)BailStatus);
			}
		}

		public override void Read(BinaryReader r)
		{
			Account						= r.ReadAccount();
			LastOperationId				= r.Read7BitEncodedInt();
			LastEmissionId				= r.Read7BitEncodedInt();
			Balance						= r.ReadCoin();
			CandidacyDeclarationRound	= r.Read7BitEncodedInt();

			if(CandidacyDeclarationRound != -1)
			{
				Bail		= r.ReadCoin();
				BailStatus	= (BailStatus)r.ReadByte();
			}
		}

		public override void WriteMore(BinaryWriter w)
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

		public override void ReadMore(BinaryReader r)
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
	}
}
