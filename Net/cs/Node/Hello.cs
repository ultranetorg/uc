using System.Net;

namespace Uccs.Net
{
	public class Hello
	{
		public int[]						Versions;
		public long							Roles;
		public string						Net;
		public IPAddress					IP;
		public bool							Permanent;
		public string						Name;

		public void Write(BinaryWriter w)
		{
			w.Write(Versions, i => w.Write7BitEncodedInt(i));
			w.WriteUtf8(Net);
			w.WriteUtf8(Name);
			w.Write7BitEncodedInt64(Roles);
			w.Write(IP);
			w.Write(Permanent);
		}

		public void Read(BinaryReader r)
		{
			Versions			= r.ReadArray(() => r.Read7BitEncodedInt());
			Net					= r.ReadUtf8();
			Name				= r.ReadUtf8();
			Roles				= r.Read7BitEncodedInt64();
			IP					= r.ReadIPAddress();
			Permanent			= r.ReadBoolean();
		}
	}
}
