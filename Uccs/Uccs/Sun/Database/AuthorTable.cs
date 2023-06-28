using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RocksDbSharp;

namespace Uccs.Net
{
	public class AuthorTable : Table<AuthorEntry, string>
	{
		public AuthorTable(Database chain) : base(chain)
		{
		}
		
		protected override AuthorEntry Create()
		{
			return new AuthorEntry(Database);
		}

		protected override byte[] KeyToBytes(string key)
		{
			return Encoding.UTF8.GetBytes(key);
		}
		
 		public AuthorEntry Find(string name, int ridmax)
 		{
			if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
				throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
 				if(r.AffectedAuthors.ContainsKey(name))
 					return r.AffectedAuthors[name];
 		
 			return FindEntry(name);
 		}

		// 		public IEnumerable<AuthorEntry> Find(Account account, int ridmax)
		// 		{
		// 			var o = new List<string>();
		// 		
		// 			foreach(var r in Database.Rounds.Where(i => i.Id <= ridmax))
		// 				foreach(var a in r.AffectedAccounts)
		// 					if(a.Key == account)
		// 					{
		// 						foreach(var i in a.Value.Authors)
		// 							yield return Find(i, r.Id);
		// 		
		// 						yield break;
		// 					}
		// 		
		// 			var e = Database.Accounts.FindEntry(account);
		// 		
		// 			if(e != null)
		// 				foreach(var a in e.Authors.Select(i => FindEntry(i)))
		// 					if(a.ObtainedRid <= ridmax)
		// 					{
		// 						if(!o.Contains(a.Name))
		// 						{	
		// 							o.Add(a.Name);
		// 							yield return a;
		// 						}
		// 					}
		// 					else
		// 						throw new IntegrityException("maxrid works inside pool only");
		// 		}
	}
}
