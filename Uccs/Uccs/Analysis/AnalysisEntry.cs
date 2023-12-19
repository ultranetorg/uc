using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalysisEntry : Analysis, ITableEntry<byte[]>
	{
		public EntityId		Id { get; set; }
		public byte[]		Key => Release;

		Mcv					Chain;

		public AnalysisEntry()
		{
		}

		public AnalysisEntry(Mcv chain)
		{
			Chain = chain;
		}

		public AnalysisEntry Clone()
		{
			return new AnalysisEntry(Chain){ Id = Id, 
											 Release = Release,
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
