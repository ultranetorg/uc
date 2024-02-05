using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ReleaseEntry : Release, ITableEntry<ReleaseAddress>
	{
		public EntityId			Id { get; set; }
		public ReleaseAddress	Key => Address;

		Mcv						Chain;

		public ReleaseEntry()
		{
		}

		public ReleaseEntry(Mcv chain)
		{
			Chain = chain;
		}

		public ReleaseEntry Clone()
		{
			return new ReleaseEntry(Chain){	Id = Id, 
											Address = Address,
											Fee = Fee,
											StartedAt = StartedAt,
											Consil = Consil,
											Results = Results };
		}

		public void WriteMain(BinaryWriter writer)
		{
			Write(writer);
		}

		public void ReadMain(BinaryReader reader)
		{
			Read(reader);
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}
	}
}
