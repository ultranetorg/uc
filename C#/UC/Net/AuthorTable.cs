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
		public AuthorTable(Roundchain chain, ColumnFamilyHandle cfh) : base(chain, cfh)
		{
		}
		
		protected override AuthorEntry Create(string author)
		{
			return new AuthorEntry(Chain, author);
		}

		protected override byte[] KeyToBytes(string key)
		{
			return Encoding.UTF8.GetBytes(key);
		}
		
		public void Save(WriteBatch batch, Round round)
		{
			///foreach(var i in releases)
			///{
			///	var e =	GetEntry(i.Address.Author);
			///
			///	var p = e.Products.Find(j => j.Name == i.Address.Product);
			///
			///	if(p == null)
			///	{
			///		p = new Product {Name = i.Address.Product};
			///		e.Products.Add(p);
			///	}
			///
			///	var r = p.Releases.Find(j => j.Platform == i.Address.Platform && j.Channel == i.Channel);
			///
			///	if(r == null)
			///		p.Releases.Add(new Release(i.Address.Platform, i.Address.Version, i.Channel, round.Id));
			///	else
			///		r.Version = i.Address.Version;
			///}

			
			Save(batch, round.AffectedAuthors.Values);
		}

 		public AuthorEntry Find(string name, int ridmax)
 		{
 			foreach(var r in Chain.Rounds.Where(i => i.Id <= ridmax))
 				if(r.AffectedAuthors.ContainsKey(name))
 					return r.AffectedAuthors[name];
 		
 			var e = FindEntry(name);
 		
 			if(e != null && (e.LastRegistration > ridmax || e.LastTransfer > ridmax))
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
					if((a.LastTransfer != -1 ? a.LastTransfer : a.LastRegistration) <= ridmax)
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
