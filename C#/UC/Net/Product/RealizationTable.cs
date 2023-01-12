using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RocksDbSharp;

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
			foreach(var r in Database.Rounds.Where(i => i.Id <= ridmax))
				if(r.AffectedRealizations.ContainsKey(name))
					return r.AffectedRealizations[name];

			var e = FindEntry(name);

			if(e != null && e.LastRegistrationRid > ridmax)
				throw new IntegrityException("maxrid works inside pool only");

			return e;
		}
	}
}
