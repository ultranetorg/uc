using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UC.Net
{
	public enum BailStatus
	{
		Null, OK, Siezed
	}

	public class AccountEntry : Entry<Account>
	{
		public Account					Account;
		public List<string>				Authors = new();
		///public Dictionary<string, int>	Products = new();
		public HashSet<int>				Transactions = new();
		public Coin						Balance;
		public Coin						Bail;
		public BailStatus				BailStatus;

		public override Account			Key => Account;
		Roundchain						Chain;

		public AccountEntry(Roundchain chain, Account a)
		{
			Chain = chain;
			Account = a;
		}

		public AccountEntry Clone()
		{
			return new AccountEntry(Chain, Account){	Authors = new List<string>(Authors),
														Transactions = new HashSet<int>(Transactions),
														Balance = Balance,
														Bail = Bail,
														BailStatus = BailStatus,
													};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Transactions);
			w.Write(Authors, i => w.WriteUtf8(i));
			w.Write(Balance);
			w.Write(Bail);
			w.Write((byte)BailStatus);
		}

		public override void Read(BinaryReader r)
		{
			Transactions	= r.ReadHashSet(() => r.Read7BitEncodedInt());
			Authors			= r.ReadList(() => r.ReadUtf8());
			Balance			= r.ReadCoin();
			Bail			= r.ReadCoin();
			BailStatus		= (BailStatus)r.ReadByte();
		}


		public O FindOperation<O>(Round executing, Func<Operation, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null) where O : Operation
		{
			return	(	executing.ExecutedOperations.FirstOrDefault(i =>	i.Signer == Account && 
																			i is O &&
																			(pp == null || pp(i.Transaction.Payload)) && 
																			(tp == null || tp(i.Transaction)) && 
																			(op == null || op(i)))
						??
						Chain.Accounts.FindLastOperation<O>(Account,o => o.Successful && (op == null || op(o)), 
																	tp, 
																	p => (!p.Round.Confirmed || p.Confirmed) && (pp == null || pp(p)), /// if round is confirmed than take confirmed blocks only
																	r => r.Id < executing.Id)
					)
					as O;
		}

	}
}
