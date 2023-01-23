using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using Nethereum.Model;
using RocksDbSharp;
using Nethereum.Signer;
using System.Diagnostics;

namespace UC.Net
{
	public class AccountTable : Table<AccountEntry, Account>
	{
		public AccountTable(Database chain) : base(chain)
		{
		}

		protected override AccountEntry Create()
		{
			return new AccountEntry(Database);
		}

		protected override byte[] KeyToBytes(Account k)
		{
			return k;
		}

		public Transaction FindTransaction(Account account, Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
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
						if(payload_predicate != null && !payload_predicate(p))
							continue;

						foreach(var t in p.Transactions.Where(t => t.Signer == account))
						{
							if(transaction_predicate == null || transaction_predicate(t))
								return t;
						}
					}
				}
			}

			return null;
		}

		public IEnumerable<Transaction> FindTransactions(Account account, Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
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
						if(payload_predicate != null && !payload_predicate(p))
							continue;

						foreach(var t in p.Transactions.Where(t => t.Signer == account))
						{
							if(transaction_predicate == null || transaction_predicate(t))
								yield return t;
						}
					}
				}
			}
		}

		public Transaction FindLastTransaction(Account signer, Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			return	Database.FindLastTailTransaction(i => i.Signer == signer && (transaction_predicate == null || transaction_predicate(i)), payload_predicate, round_predicate)
					??
					FindTransaction(signer, transaction_predicate, payload_predicate, round_predicate);
		}

		public IEnumerable<Transaction> FindLastTransactions(Account signer, Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			return	Database.FindLastTailTransactions(i => i.Signer == signer && (transaction_predicate == null || transaction_predicate(i)), payload_predicate, round_predicate)
					.Union(FindTransactions(signer, transaction_predicate, payload_predicate, round_predicate));
		}

		public IEnumerable<Transaction> SearchTransactions(Account signer, int skip = 0, int count = int.MaxValue)
		{
			var o = Database.FindLastTailTransactions(i => i.Signer == signer);

			var e = FindEntry(signer);

			if(e != null && e.Transactions != null)
			{
				o = o.Union(e.Transactions.SelectMany(r => Database.FindRound(r).FindTransactions(t => t.Signer == signer))).Skip(skip).Take(count);
			}

			return o;
		}

		public Operation FindLastOperation(Account signer, Func<Operation, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null, Func<Round, bool> rp = null)
		{
			foreach(var t in FindLastTransactions(signer, tp, pp, rp))
				foreach(var o in t.Operations)
					if(op == null || op(o))
						return o;

			return null;
		}

		public O FindLastOperation<O>(Account signer, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null, Func<Round, bool> rp = null) where O : Operation
		{
			foreach(var t in FindLastTransactions(signer, tp, pp, rp))
				foreach(var o in t.Operations.OfType<O>())
					if(op == null || op(o))
						return o;

			return null;
		}

		public IEnumerable<O> FindLastOperations<O>(Account signer, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null, Func<Round, bool> rp = null) where O : Operation
		{
			foreach(var t in FindLastTransactions(signer, tp, pp, rp))
				foreach(var o in t.Operations.OfType<O>())
					if(op == null || op(o))
						yield return o;
		}

		public AccountEntry Find(Account account, int ridmax)
		{
			if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1) /// by -1 we treat the whole Base as a round before Last
				throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedAccounts.ContainsKey(account))
					return r.AffectedAccounts[account];

			return FindEntry(account);
		}
	}
}
