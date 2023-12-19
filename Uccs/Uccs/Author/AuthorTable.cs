using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class AuthorTable : Table<AuthorEntry, string>
	{
		public override bool		Equal(string a, string b) => a.Equals(b);
		public override Span<byte>	KeyToCluster(string author) => new Span<byte>(Encoding.UTF8.GetBytes(author, 0, Cluster.IdLength));

		public AuthorTable(Mcv chain) : base(chain)
		{
		}
		
		protected override AuthorEntry Create()
		{
			return new AuthorEntry(Mcv);
		}
		
 		public AuthorEntry Find(string name, int ridmax)
 		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
 				if(r.AffectedAuthors.TryGetValue(name, out AuthorEntry v))
 					return v;
 		
 			return FindEntry(name);
 		}
		
 		public Resource FindResource(ResourceAddress resource, int ridmax)
 		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
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
				yield return a.Resources.First(j => j.Id.Ri == i);
			}
 		}
	}
}
