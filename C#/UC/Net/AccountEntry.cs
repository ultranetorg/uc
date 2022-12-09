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
		public int						CandidacyDeclarationRound = -1;
		public IPAddress[]				IPs = new IPAddress[]{};
		public Coin						Bail;
		public BailStatus				BailStatus;

		public List<string>				Authors = new();
		public HashSet<int>				Transactions = new();

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
											Balance = Balance,
											CandidacyDeclarationRound = CandidacyDeclarationRound,
											IPs = IPs.ToArray(),
											Bail = Bail,
											BailStatus = BailStatus,
											Authors = new List<string>(Authors),
											Transactions = new HashSet<int>(Transactions)
											};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Account);
			w.Write7BitEncodedInt(LastOperationId);
			w.Write(Balance);
			w.Write7BitEncodedInt(CandidacyDeclarationRound);

			if(CandidacyDeclarationRound != -1)
			{
				w.Write(IPs, i => w.Write(i));
				w.Write(Bail);
				w.Write((byte)BailStatus);
			}
		}

		public override void Read(BinaryReader r)
		{
			Account						= r.ReadAccount();
			LastOperationId				= r.Read7BitEncodedInt();
			Balance						= r.ReadCoin();
			CandidacyDeclarationRound	= r.Read7BitEncodedInt();

			if(CandidacyDeclarationRound != -1)
			{
				IPs			= r.ReadArray(() => r.ReadIPAddress());
				Bail		= r.ReadCoin();
				BailStatus	= (BailStatus)r.ReadByte();
			}
		}

		public override void WriteMore(BinaryWriter w)
		{
			w.Write(Transactions);
			w.Write(Authors, i => w.WriteUtf8(i));
		}

		public override void ReadMore(BinaryReader r)
		{
			Transactions	= r.ReadHashSet(() => r.Read7BitEncodedInt());
			Authors			= r.ReadList(() => r.ReadUtf8());
		}

// 		public O ExeFindOperation<O>(Round executing, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null) where O : Operation
// 		{
// 			return	(	executing.ExecutedOperations.FirstOrDefault(i =>	i.Signer == Account && 
// 																			(i.Successful && i is O o && (op == null || op(o))) &&
// 																			(pp == null || pp(i.Transaction.Payload)) && 
// 																			(tp == null || tp(i.Transaction)))
// 						??
// 						Chain.Accounts.FindLastOperation<O>(Account,	o => o.Successful && (op == null || op(o)), 
// 																		tp, 
// 																		p => (!p.Round.Confirmed || p.Confirmed) && (pp == null || pp(p)), /// if round is confirmed then take confirmed blocks only
// 																		r => r.Id < executing.Id)
// 					)
// 					as O;
// 		}

	}
}
