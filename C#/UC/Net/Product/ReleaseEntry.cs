using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class ReleaseEntry : TableEntry<ReleaseAddress>
	{
		public override ReleaseAddress	Key => Address;
		public override byte[]			ClusterKey =>  Encoding.UTF8.GetBytes(Address.Author).Take(ClusterKeyLength).ToArray();

		public ReleaseAddress			Address;
		public byte[]					Manifest;
		public string					Channel; /// stable, beta, nightly, debug,...

		public ReleaseEntry Clone()
		{
			return	new() 
					{ 
						Address	= Address, 
						Manifest = Manifest.Clone() as byte[],
						Channel	= Channel
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(Manifest);
			w.WriteUtf8(Channel);
		}

		public override void Read(BinaryReader r)
		{
			Address	= r.Read<ReleaseAddress>();
			Manifest = r.ReadSha3();
			Channel = r.ReadUtf8();
		}

		public override void WriteMore(BinaryWriter w)
		{
		}

		public override void ReadMore(BinaryReader r)
		{
		}
	}
}
