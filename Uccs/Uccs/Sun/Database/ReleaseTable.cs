using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Uccs.Net
{
	public class ReleaseTable : Table<ReleaseEntry, ReleaseAddress>
	{
		public ReleaseTable(Database chain) : base(chain)
		{
		}
		
		protected override ReleaseEntry Create()
		{
			return new ReleaseEntry();
		}

		protected override byte[] KeyToBytes(ReleaseAddress key)
		{
			return Encoding.UTF8.GetBytes(key.ToString());
		}
		
		public ReleaseEntry Find(ReleaseAddress name, int ridmax)
		{
			if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
				throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedReleases.ContainsKey(name))
					return r.AffectedReleases[name];

			var e = FindEntry(name);

			return e;
		}

		public IEnumerable<ReleaseEntry> Where(string author, string product, Func<ReleaseEntry, bool> predicate, int ridmax)
		{
			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				foreach(var i in r.AffectedReleases)
					if(i.Key.Product.Author == author && i.Key.Product.Name == product && predicate(i.Value))
						yield return i.Value;

			var bcid = KeyToBytes(new ReleaseAddress(author, product, null, Version.Zero)).Take(ClustersKeyLength).ToArray();
			var cid = Cluster.ToId(bcid);
		
			var c = Clusters.Find(i => i.Id == cid);
		
			if(c == null)
				yield break;
		
			foreach(var i in c.Entries)
				if(i.Key.Product.Author == author && i.Key.Product.Name == product && predicate(i))
					yield return i;
		}
	}
}
