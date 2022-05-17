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

// 		public AuthorBid FindLastSuccessfulAuthorBid(string author, int maxrid)
// 		{
// 			var o = Chain.FindLastPoolOperation<AuthorBid>(o => o.Author == author && o.Result == OperationResult.OK, t => t.Successful, p => !p.Round.Confirmed || p.Confirmed, r => r.Id <= maxrid);
// 
// 			if(o != null)
// 				return o;
// 
// 			var e = FindEntry(author);
// 
// 			if(e != null)
// 			{
// 				foreach(var b in Chain.FindRound(e.LastBid).Payloads)
// 					foreach(var t in b.Transactions)
// 						foreach(var i in t.Operations.OfType<AuthorBid>())
// 							if(i.Author == author)
// 								return i;
// 							
// 				throw new IntegrityException("AuthorBid operation not found");
// 			}
// 
// 			return null;
// 		}

// 		public AuthorTransfer FindLastTransfer(string author, int maxrid)
// 		{
// 			var o = Chain.FindLastPoolOperation<AuthorTransfer>(o => o.Author == author && o.Result == OperationResult.OK, t => t.Successful, p => !p.Round.Confirmed || p.Confirmed, r => r.Id <= maxrid);
// 
// 			if(o != null)
// 				return o;
// 
// 			var e = FindEntry(author);
// 
// 			if(e != null)
// 			{
// 				foreach(var b in Chain.FindRound(e.LastRegistration).Payloads)
// 					foreach(var t in b.Transactions)
// 						foreach(var i in t.Operations.OfType<AuthorTransfer>())
// 							if(i.Author == author)
// 								return i;
// 							
// 				throw new IntegrityException("AuthorTransfer operation not found");
// 			}
// 
// 			return null;
// 		}

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

// 		public AuthorBid FindFirstBid(Round executing, string author)
// 		{
// 			return	executing.EffectiveOperations.Reverse().OfType<AuthorBid>().FirstOrDefault(i => i.Author == author)
// 					??
// 					FindFirstSuccessfulAuthorBid(author, executing.Id - 1);
// 		}

// 		public AuthorBid FindLastBid(Round executing, string author)
// 		{
// 			return	executing.EffectiveOperations.OfType<AuthorBid>().FirstOrDefault(i => i.Author == author)
// 					??
// 					FindLastSuccessfulAuthorBid(author, executing.Id - 1);
// 		}
	}
}
