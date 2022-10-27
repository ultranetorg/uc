using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace UC.Net
{
	public class Hello
	{
		public int[]			Versions;
		public Guid				Nuid;
		public Role				Roles;
		public string			Zone;
		public IPAddress		IP;
		public Peer[]			Peers;
		public int				LastRound;
		public int				LastConfirmedRound;

		public void Write(BinaryWriter w)
		{
			w.Write((uint)Roles);
			w.Write(Versions, i => w.Write7BitEncodedInt(i));
			w.WriteUtf8(Zone);
			w.Write(IP.GetAddressBytes());
			w.Write(Nuid.ToByteArray());
			w.Write(Peers, i => i.WritePeer(w));
			w.Write7BitEncodedInt(LastRound);
			w.Write7BitEncodedInt(LastConfirmedRound);
		}

		public void Read(BinaryReader r)
		{
			Roles				= (Role)r.ReadUInt32();
			Versions			= r.ReadArray(() => r.Read7BitEncodedInt());
			Zone				= r.ReadUtf8();
			IP					= new IPAddress(r.ReadBytes(4));
			Nuid				= new Guid(r.ReadBytes(16));
			Peers				= r.ReadArray<Peer>(() => {var p = new Peer(); p.ReadPeer(r); return p;});
			LastRound			= r.Read7BitEncodedInt();
			LastConfirmedRound	= r.Read7BitEncodedInt();
		}
	}
}
