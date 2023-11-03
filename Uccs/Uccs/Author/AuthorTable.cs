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
		public AuthorTable(Mcv chain) : base(chain)
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
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
 				if(r.AffectedAuthors.TryGetValue(name, out AuthorEntry v))
 					return v;
 		
 			return FindEntry(name);
 		}
		
 		public Resource FindResource(ResourceAddress resource, int ridmax)
 		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedAuthors.TryGetValue(resource.Author, out AuthorEntry a))
				{	
					var x = a.Resources.FirstOrDefault(i => i.Address.Resource == resource.Resource);
					
					if(x != null)
 						return x;
				}
 		
 			return FindEntry(resource.Author)?.Resources.FirstOrDefault(i => i.Address.Resource == resource.Resource);
 		}

 		public IEnumerable<Resource> EnumerateSubresources(ResourceAddress resource, int ridmax)
 		{
			var a = Find(resource.Author, ridmax);
			var r = FindResource(resource, ridmax);

			foreach(var i in r.Resources)
			{
				yield return a.Resources.First(j => j.Id == i);
			}
 		}
	}
}
