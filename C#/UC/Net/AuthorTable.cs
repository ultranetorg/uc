using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RocksDbSharp;

namespace UC.Net
{
	public class AuthorTable : Table<AuthorEntry, string>
	{
		public AuthorTable(Roundchain chain) : base(chain)
		{
		}
		
		protected override AuthorEntry Create()
		{
			return new AuthorEntry(Chain);
		}

		protected override byte[] KeyToBytes(string key)
		{
			return Encoding.UTF8.GetBytes(key);
		}
		
 		public AuthorEntry Find(string name, int ridmax)
 		{
 			foreach(var r in Chain.Rounds.Where(i => i.Id <= ridmax))
 				if(r.AffectedAuthors.ContainsKey(name))
 					return r.AffectedAuthors[name];
 		
 			var e = FindEntry(name);
 		
 			if(e != null && e.ObtainedRid > ridmax)
 				throw new IntegrityException("maxrid works inside pool only");
 		
 			return e;
 		}

		public IEnumerable<AuthorEntry> Find(Account account, int ridmax)
		{
			var o = new List<string>();

			foreach(var r in Chain.Rounds.Where(i => i.Id <= ridmax))
				foreach(var a in r.AffectedAccounts)
					if(a.Key == account)
					{	
						foreach(var i in a.Value.Authors)
							yield return Find(i, r.Id);

						yield break;
					}

			var e = Chain.Accounts.FindEntry(account);

			if(e != null)
				foreach(var a in e.Authors.Select(i => FindEntry(i)))
					if(a.ObtainedRid <= ridmax)
					{
						if(!o.Contains(a.Name))
						{	
							o.Add(a.Name);
							yield return a;
						}
					}
					else
						throw new IntegrityException("maxrid works inside pool only");
		}
	}
}
