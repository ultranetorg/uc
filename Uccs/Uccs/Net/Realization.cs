using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class Realization : IBinarySerializable
	{
		public RealizationAddress		Address;
		public Osbi[]					OSes;

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(OSes);
		}

		public void Read(BinaryReader r)
		{
			Address	= r.Read<RealizationAddress>();
			OSes	= r.ReadArray<Osbi>();
		}

	}
}
