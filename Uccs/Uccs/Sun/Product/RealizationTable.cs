using System.Linq;
using System.Text;

namespace UC.Net
{
	public class RealizationTable : Table<RealizationEntry, RealizationAddress>
	{
		public RealizationTable(Database chain) : base(chain)
		{
		}
		
		protected override RealizationEntry Create()
		{
			return new RealizationEntry();
		}

		protected override byte[] KeyToBytes(RealizationAddress key)
		{
			return Encoding.UTF8.GetBytes(key.ToString());
		}
		
		public RealizationEntry Find(RealizationAddress name, int ridmax)
		{
			if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
				throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Database.Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedRealizations.ContainsKey(name))
					return r.AffectedRealizations[name];

			var e = FindEntry(name);

			return e;
		}
	}
}
