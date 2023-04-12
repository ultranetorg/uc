using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class PlatformTable : Table<PlatformEntry, PlatformAddress>
	{
		public PlatformTable(Database chain) : base(chain)
		{
		}
		
		protected override PlatformEntry Create()
		{
			return new PlatformEntry();
		}

		protected override byte[] KeyToBytes(PlatformAddress key)
		{
			return Encoding.UTF8.GetBytes(key.ToString());
		}
		
		public PlatformEntry Find(PlatformAddress name, int ridmax)
		{
			if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
				throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedPlatforms.ContainsKey(name))
					return r.AffectedPlatforms[name];

			var e = FindEntry(name);

			return e;
		}
	}
}
