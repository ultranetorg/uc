using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class Platform : IBinarySerializable
	{
		public PlatformAddress	Address;
		public Osbi[]			OSes;

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(OSes);
		}

		public void Read(BinaryReader r)
		{
			Address	= r.Read<PlatformAddress>();
			OSes	= r.ReadArray<Osbi>();
		}
	}
}
