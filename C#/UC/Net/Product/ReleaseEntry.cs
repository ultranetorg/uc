using System.Collections.Generic;
using System.IO;

namespace UC.Net
{
	public class ReleaseEntry : IBinarySerializable
	{
		public string				Platform;
		public Version				Version;
		public string				Channel; /// stable, beta, nightly, debug,...
		public int					Rid;
		//public List<ReleaseAddress>	MergedDependencies = new();

		public ReleaseEntry()
		{
		}

		public ReleaseEntry(string platform, Version version, string channel, int rid)
		{
			Platform = platform;
			Version = version;
			Channel = channel;
			Rid = rid;
		}

		public ReleaseEntry Clone()
		{
			return new ReleaseEntry(Platform, Version, Channel, Rid);
		}

		public void Read(BinaryReader r)
		{
			Platform = r.ReadUtf8();
			Version = r.Read<Version>();
			Channel = r.ReadUtf8();
			Rid = r.Read7BitEncodedInt();
			//MergedDependencies = r.ReadList<ReleaseAddress>();
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Platform);
			w.Write(Version);
			w.WriteUtf8(Channel);
			w.Write7BitEncodedInt(Rid);
			//w.Write(MergedDependencies);
		}
	}
}
