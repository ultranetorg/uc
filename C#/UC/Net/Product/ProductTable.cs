using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RocksDbSharp;

namespace UC.Net
{
	public class ProductTable : Table<ProductEntry, ProductAddress>
	{
		public ProductTable(Database chain) : base(chain)
		{
		}
		
		protected override ProductEntry Create()
		{
			return new ProductEntry();
		}

		protected override byte[] KeyToBytes(ProductAddress key)
		{
			return Encoding.UTF8.GetBytes(key.ToString());
		}
		
		public ProductEntry Find(ProductAddress name, int ridmax)
		{
			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedProducts.ContainsKey(name))
					return r.AffectedProducts[name];

			var e = FindEntry(name);

			if(e != null && (e.LastRegistration > ridmax))
				throw new IntegrityException("maxrid works inside pool only");

			return e;
		}


//		protected override void Update(Round round, Operation o, HashSet<ProductEntry> affected)
//		{
//			if(o is ProductRegistration ar)
//			{
//				var e = GetEntry(ar.Product);
//				e.Registration = round.Id;
//				affected.Add(e);
//			}
//			else if(o is Release r)
//			{
//				var e = GetEntry(r.Product);
//				e.Releases.Add(round.Id);
//				affected.Add(e);
//			}
//		}

// 		public List<Release> FindLastReleases(string author, string product)
// 		{
// 			var list = new List<Release>();
// 
// 			var e = FindEntry(product);
// 			
// 			if(e != null && e.Releases != null)
// 			{
// 				list = e.Releases.OrderByDescending(i => i).SelectMany(i => Chain.FindRound(i).FindOperations<Release>(x => x.Author == author && 
// 																															x.Product == product && 
// 																															!list.Any(i => i.Product == x.Product))).ToList();
// 
// 				if(!list.Any())					
// 					throw new IntegrityException("Release operation(s) not found");
// 			}
// 
// 			return list;
// 		}

//  		public IEnumerable<Release> FindReleases(string author, string product, Func<Release, bool> f)
//  		{
//  			var e = FindEntry(product);
//  			
//  			if(e != null && e.Releases.Any())
//  			{
//  				return e.Releases.OrderByDescending(i => i).SelectMany(i => Chain.FindRound(i).FindOperations<Release>(i => f(i)));
//  			}
//  
//  			return new Release[0];
//  		}
// 
//  		public Release FindLastRelease(string author, string product, string platform)
//  		{
//  			var e = FindEntry(product);
//  			
//  			if(e != null && e.Releases != null)
//  			{
//  				var o = Chain.FindRound(e.Releases.Max()).FindOperation<Release>(x => x.Author == author && x.Product == product && x.Platform == platform);
//  
// 				if(o != null)
// 					return o;
// 				else
// 	 				throw new IntegrityException("Release transaction(s) not found");
//  			}
//  			
//  			return null;
//  		}
// 
// 		public ProductRegistration FindRegistration(ProductAddress product)
// 		{
// 			var e = FindEntry(product);
// 			
// 			if(e != null)
// 			{
// 				var pr = Chain.FindRound(e.LastRegistration).FindOperation<ProductRegistration>(x => x.Name == product);
// 
// 				if(pr != null)
// 					return pr;
// 				else
// 					throw new IntegrityException("ProductRegistration operation not found");
// 			}
// 			
// 			return null;
// 		}
// 
// 		public List<ProductRegistration> FindRegistrations(Func<ProductRegistration, bool> f)
// 		{
// 			var list = new List<ProductRegistration>();
// 		
// 			using(var it = Database.NewIterator(ColumnFamily))
// 			{
// 				for(it.SeekToFirst(); it.Valid(); it.Next())
// 				{
// 					var e = FindEntry(Encoding.UTF8.GetString(it.Key()));
// 
// 					if(e.LastRegistration != -1)
// 						foreach(var o in Chain.FindRound(e.LastRegistration).FindOperations<ProductRegistration>(f))
// 							list.Add(o);
// 				}
// 			}
// 		
// 			return list;
// 		}
	}
}
