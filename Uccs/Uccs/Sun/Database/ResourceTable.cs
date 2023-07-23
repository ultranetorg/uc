using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Uccs.Net
{
	public class ResourceTable : Table<ResourceEntry, ResourceAddress>
	{
		public ResourceTable(Database chain) : base(chain)
		{
		}
		
		protected override ResourceEntry Create()
		{
			return new ResourceEntry();
		}

		protected override byte[] KeyToBytes(ResourceAddress key)
		{
			return Encoding.UTF8.GetBytes(key.Author);
		}
		
		public ResourceEntry Find(ResourceAddress name, int ridmax)
		{
			if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
				throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedReleases.ContainsKey(name))
					return r.AffectedReleases[name];

			var e = FindEntry(name);

			return e;
		}

		public IEnumerable<ResourceEntry> Where(string author, Func<ResourceEntry, bool> predicate, int ridmax)
		{
			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				foreach(var i in r.AffectedReleases)
					if(i.Key.Author == author && predicate(i.Value))
						yield return i.Value;

			var bcid = KeyToBytes(new ResourceAddress(author, null)).Take(ClustersKeyLength).ToArray();
			var cid = Cluster.ToId(bcid);
		
			var c = Clusters.Find(i => i.Id == cid);
		
			if(c == null)
				yield break;
		
			foreach(var i in c.Entries)
				if(i.Key.Author == author && predicate(i))
					yield return i;
		}
	}
}
