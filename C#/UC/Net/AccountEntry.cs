using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public Coin						Balance;
		public Coin						Bail;
		public BailStatus				BailStatus;

		public List<string>				Authors = new();
		public HashSet<int>				Transactions = new();

		public override Account			Key => Account;
		public override byte[]			ClusterKey => ((byte[])Account).Take(ClusterKeyLength).ToArray();

		Roundchain						Chain;

		public AccountEntry(Roundchain chain)
		{
			Chain = chain;
		}

		public AccountEntry Clone()
		{
			return new AccountEntry(Chain){	Account = Account,
											Authors = new List<string>(Authors),
											Transactions = new HashSet<int>(Transactions),
											Balance = Balance,
											Bail = Bail,
											BailStatus = BailStatus};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Account);
			w.Write(Balance);
			w.Write(Bail);
			w.Write((byte)BailStatus);
		}

		public override void WriteMore(BinaryWriter w)
		{
			w.Write(Transactions);
			w.Write(Authors, i => w.WriteUtf8(i));
		}

		public override void Read(BinaryReader r)
		{
			Account		= r.ReadAccount();
			Balance		= r.ReadCoin();
			Bail		= r.ReadCoin();
			BailStatus	= (BailStatus)r.ReadByte();
		}

		public override void ReadMore(BinaryReader r)
		{
			Transactions	= r.ReadHashSet(() => r.Read7BitEncodedInt());
			Authors			= r.ReadList(() => r.ReadUtf8());
		}

		public O ExeFindOperation<O>(Round executing, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null) where O : Operation
		{
			return	(	executing.ExecutedOperations.FirstOrDefault(i =>	i.Signer == Account && 
																			(i.Successful && i is O o && (op == null || op(o))) &&
																			(pp == null || pp(i.Transaction.Payload)) && 
																			(tp == null || tp(i.Transaction)))
						??
						Chain.Accounts.FindLastOperation<O>(Account,	o => o.Successful && (op == null || op(o)), 
																		tp, 
																		p => (!p.Round.Confirmed || p.Confirmed) && (pp == null || pp(p)), /// if round is confirmed then take confirmed blocks only
																		r => r.Id < executing.Id)
					)
					as O;
		}

	}
}
