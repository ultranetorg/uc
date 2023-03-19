using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class ReleaseEntry : TableEntry<VersionAddress>
	{
		public override VersionAddress	Key => Address;
		public override byte[]			ClusterKey =>  Encoding.UTF8.GetBytes(Address.Author).Take(ClusterKeyLength).ToArray();

		public VersionAddress			Address;
		public byte[]					Manifest;
		public string					Channel; /// stable, beta, nightly, debug,...

		public int						LastRegistration = -1;

		public ReleaseEntry Clone()
		{
			return	new() 
					{ 
						Address	= Address, 
						Manifest = Manifest.Clone() as byte[],
						Channel	= Channel,
						LastRegistration = LastRegistration
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
			Address	= r.Read<VersionAddress>();
			Manifest = r.ReadSha3();
			Channel = r.ReadUtf8();
		}

		public override void WriteMore(BinaryWriter w)
		{
			w.Write7BitEncodedInt(LastRegistration);
		}

		public override void ReadMore(BinaryReader r)
		{
			LastRegistration = r.Read7BitEncodedInt();
		}
	}
}
