using System.Linq;
using System.Text;

namespace UC.Net
{
	public class ReleaseTable : Table<ReleaseEntry, VersionAddress>
	{
		public ReleaseTable(Database chain) : base(chain)
		{
		}
		
		protected override ReleaseEntry Create()
		{
			return new ReleaseEntry();
		}

		protected override byte[] KeyToBytes(VersionAddress key)
		{
			return Encoding.UTF8.GetBytes(key.ToString());
		}
		
		public ReleaseEntry Find(VersionAddress name, int ridmax)
		{
			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedReleases.ContainsKey(name))
					return r.AffectedReleases[name];

			var e = FindEntry(name);

			if(e != null && e.LastRegistration > ridmax)
				throw new IntegrityException("maxrid works inside pool only");

			return e;
		}
	}
}
