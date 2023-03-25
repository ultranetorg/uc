using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class Release : IBinarySerializable
	{
		public ReleaseAddress	Address { get; set; }
		public byte[]			Manifest { get; set; }
		public string			Channel { get; set; } /// stable, beta, nightly, debug,...

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(Manifest);
			w.WriteUtf8(Channel);
		}

		public void Read(BinaryReader r)
		{
			Address	= r.Read<ReleaseAddress>();
			Manifest = r.ReadSha3();
			Channel = r.ReadUtf8();
		}
	}
}
