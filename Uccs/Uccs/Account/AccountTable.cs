using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using Nethereum.Model;
using RocksDbSharp;
using Nethereum.Signer;
using System.Diagnostics;

namespace Uccs.Net
{
	public class AccountTable : Table<AccountEntry, AccountAddress>
	{
		protected override bool Equal(AccountAddress a, AccountAddress b) => a.Equals(b);

		public AccountTable(Mcv chain) : base(chain)
		{
		}

		protected override AccountEntry Create()
		{
			return new AccountEntry(Database);
		}

		protected override byte[] KeyToBytes(AccountAddress k)
		{
			return k.Bytes;
		}

		public Transaction FindTransaction(AccountAddress account, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
		{
			var e = FindEntry(account);

			if(e != null)
			{
				foreach(var i in e.Transactions.OrderByDescending(i => i))
				{
					var r = Database.FindRound(i);

					if(round_predicate != null && !round_predicate(r))
						continue;

					foreach(var t in r.ConfirmedTransactions.Where(t => t.Signer == account))
					{
						if(transaction_predicate == null || transaction_predicate(t))
							return t;
					}
				}
			}

			return null;
		}

		public IEnumerable<Transaction> FindTransactions(AccountAddress account, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
		{
			var e = FindEntry(account);

			if(e != null)
			{
				foreach(var i in e.Transactions.OrderByDescending(i => i))
				{
					var r = Database.FindRound(i);

					if(round_predicate != null && !round_predicate(r))
						continue;

					foreach(var p in r.Payloads)
					{
						foreach(var t in r.ConfirmedTransactions.Where(t => t.Signer == account))
						{
							if(transaction_predicate == null || transaction_predicate(t))
								yield return t;
						}
					}
				}
			}
		}

		public Transaction FindLastTransaction(AccountAddress signer, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
		{
			return	Database.FindLastTailTransaction(i => i.Signer == signer && (transaction_predicate == null || transaction_predicate(i)), round_predicate)
					??
					FindTransaction(signer, transaction_predicate, round_predicate);
		}

		public IEnumerable<Transaction> FindLastTransactions(AccountAddress signer, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
		{
			return	Database.FindLastTailTransactions(i => i.Signer == signer && (transaction_predicate == null || transaction_predicate(i)), round_predicate)
					.Union(FindTransactions(signer, transaction_predicate, round_predicate));
		}

		public IEnumerable<Transaction> SearchTransactions(AccountAddress signer, int skip = 0, int count = int.MaxValue)
		{
			var o = Database.FindLastTailTransactions(i => i.Signer == signer);

			var e = FindEntry(signer);

			if(e != null && e.Transactions != null)
			{
				o = o.Union(e.Transactions.SelectMany(r => Database.FindRound(r).Transactions.Where(t => t.Signer == signer))).Skip(skip).Take(count);
			}

			return o;
		}

		public Operation FindLastOperation(AccountAddress signer, Func<Operation, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
		{
			foreach(var t in FindLastTransactions(signer, tp, rp))
				foreach(var o in t.Operations)
					if(op == null || op(o))
						return o;

			return null;
		}

		public O FindLastOperation<O>(AccountAddress signer, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null) where O : Operation
		{
			foreach(var t in FindLastTransactions(signer, tp, rp))
				foreach(var o in t.Operations.OfType<O>())
					if(op == null || op(o))
						return o;

			return null;
		}

		public IEnumerable<O> FindLastOperations<O>(AccountAddress signer, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null) where O : Operation
		{
			foreach(var t in FindLastTransactions(signer, tp, rp))
				foreach(var o in t.Operations.OfType<O>())
					if(op == null || op(o))
						yield return o;
		}

		public AccountEntry Find(AccountAddress account, int ridmax)
		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedAccounts.TryGetValue(account, out var e))
					return e;

			return FindEntry(account);
		}
	}
}
