using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Uccs.Net
{
	public class Hello
	{
		public int[]						Versions;
		public Guid							Nuid;
		public Role							Roles;
		public string						Zone;
		public IPAddress					IP;
		public Peer[]						Peers;
		public IEnumerable<Member>			Generators = new Member[]{};

		public void Write(BinaryWriter w)
		{
			w.Write((uint)Roles);
			w.Write(Versions, i => w.Write7BitEncodedInt(i));
			w.WriteUtf8(Zone);
			w.Write(IP.GetAddressBytes());
			w.Write(Nuid.ToByteArray());
			w.Write(Peers, i => i.WritePeer(w));
			w.Write(Generators, i => i.WriteForSharing(w));
		}

		public void Read(BinaryReader r)
		{
			Roles				= (Role)r.ReadUInt32();
			Versions			= r.ReadArray(() => r.Read7BitEncodedInt());
			Zone				= r.ReadUtf8();
			IP					= new IPAddress(r.ReadBytes(4));
			Nuid				= new Guid(r.ReadBytes(16));
			Peers				= r.Read<Peer>(i => {
														i.Fresh = true; 
														i.ReadPeer(r);
													}).ToArray();
			Generators			= r.Read<Member>(i => i.ReadForSharing(r));
		}
	}
}
