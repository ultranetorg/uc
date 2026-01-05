using System.Text;

namespace Uccs.Net;

public class UserTable : Table<AutoId, User>
{
	public override string Name => McvTable.User.ToString();

	public int	KeyToBucket(string name) => EntityId.BytesToBucket(Encoding.UTF8.GetBytes(name.PadRight(3, '\0'), 0, 3));

	public UserTable(Mcv chain) : base(chain)
	{
	}

	public override User Create()
	{
		return new User(Mcv);
	}

	public User FindEntry(string nickname)
	{
		var bid = KeyToBucket(nickname);

		return FindBucket(bid)?.Entries.FirstOrDefault(i => i.Name == nickname);
	}

	public User Find(string nickname, int ridmax)
	{
		foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
			if(r.AffectedAccounts.Values.FirstOrDefault(i => i.Name == nickname) is User e && !e.Deleted)
				return e;

		return FindEntry(nickname);
	}

	public User Latest(string nickname)
	{
		return Find(nickname, Mcv.LastConfirmedRound.Id);
	}

// 		public Transaction FindTransaction(AccountAddress account, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
// 		{
// 			var e = FindEntry(account);
// 
// 			if(e != null)
// 			{
// 				foreach(var i in e.Transactions.OrderByDescending(i => i))
// 				{
// 					var r = Mcv.FindRound(i);
// 
// 					if(round_predicate != null && !round_predicate(r))
// 						continue;
// 
// 					foreach(var t in r.ConsensusTransactions.Where(t => t.Signer == account))
// 					{
// 						if(transaction_predicate == null || transaction_predicate(t))
// 							return t;
// 					}
// 				}
// 			}
// 
// 			return null;
// 		}
// 
// 		public IEnumerable<Transaction> FindTransactions(AccountAddress account, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
// 		{
// 			var e = FindEntry(account);
// 
// 			if(e != null)
// 			{
// 				foreach(var i in e.Transactions.OrderByDescending(i => i))
// 				{
// 					var r = Mcv.FindRound(i);
// 
// 					if(round_predicate != null && !round_predicate(r))
// 						continue;
// 
// 					foreach(var p in r.Payloads)
// 					{
// 						foreach(var t in r.ConsensusTransactions.Where(t => t.Signer == account))
// 						{
// 							if(transaction_predicate == null || transaction_predicate(t))
// 								yield return t;
// 						}
// 					}
// 				}
// 			}
// 		}

// 		public Transaction FindLastTransaction(AccountAddress signer, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
// 		{
// 			return	Mcv.FindLastTailTransaction(i => i.Signer == signer && (transaction_predicate == null || transaction_predicate(i)), round_predicate)
// 					??
// 					FindTransaction(signer, transaction_predicate, round_predicate);
// 		}
// 
// 		public IEnumerable<Transaction> FindLastTransactions(AccountAddress signer, Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
// 		{
// 			return	Mcv.FindLastTailTransactions(i => i.Signer == signer && (transaction_predicate == null || transaction_predicate(i)), round_predicate)
// 					.Concat(FindTransactions(signer, transaction_predicate, round_predicate));
// 		}

// 		public IEnumerable<Transaction> SearchTransactions(AccountAddress signer, int skip = 0, int count = int.MaxValue)
// 		{
// 			var o = Mcv.FindLastTailTransactions(i => i.Signer == signer);
// 
// 			var e = FindEntry(signer);
// 
// 			if(e != null && e.Transactions != null)
// 			{
// 				o = o.Concat(e.Transactions.SelectMany(r => Mcv.FindRound(r).Transactions.Where(t => t.Signer == signer))).Skip(skip).Take(count);
// 			}
// 
// 			return o;
// 		}
// 
// 		public Operation FindLastOperation(AccountAddress signer, Func<Operation, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
// 		{
// 			foreach(var t in FindLastTransactions(signer, tp, rp))
// 				foreach(var o in t.Operations)
// 					if(op == null || op(o))
// 						return o;
// 
// 			return null;
// 		}
// 
// 		public O FindLastOperation<O>(AccountAddress signer, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null) where O : Operation
// 		{
// 			foreach(var t in FindLastTransactions(signer, tp, rp))
// 				foreach(var o in t.Operations.OfType<O>())
// 					if(op == null || op(o))
// 						return o;
// 
// 			return null;
// 		}

// 		public IEnumerable<O> FindLastOperations<O>(AccountAddress signer, Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null) where O : Operation
// 		{
// 			foreach(var t in FindLastTransactions(signer, tp, rp))
// 				foreach(var o in t.Operations.OfType<O>())
// 					if(op == null || op(o))
// 						yield return o;
// 		}
}
