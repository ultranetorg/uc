using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class Hello
	{
		public int[]						Versions;
		public Guid							Nuid;
		public Dictionary<Guid, Role>		Roles;
		public string						Zone;
		public IPAddress					IP;
		public bool							Permanent;
		public Peer[]						Peers;
		//public IEnumerable<Member>			Generators = new Member[]{};

		public void Write(BinaryWriter w)
		{
			w.Write(Versions, i => w.Write7BitEncodedInt(i));
			w.Write(Nuid);
			w.Write(Roles, i => {	w.Write(i.Key);
									w.Write((uint)i.Value); });
			w.WriteUtf8(Zone);
			w.Write(IP);
			w.Write(Permanent);
			w.Write(Peers, i => i.WritePeer(w));
			//w.Write(Generators, i => i.WriteForSharing(w));
		}

		public void Read(BinaryReader r)
		{
			Versions			= r.ReadArray(() => r.Read7BitEncodedInt());
			Nuid				= r.ReadGuid();
			Roles				= r.ReadDictionary(	() => r.ReadGuid(), 
													() => (Role)r.ReadUInt32());
			Zone				= r.ReadUtf8();
			IP					= r.ReadIPAddress();
			Permanent			= r.ReadBoolean();
			Peers				= r.Read<Peer>(i => {
														i.Fresh = true; 
														i.ReadPeer(r);
													}).ToArray();
			//Generators			= r.Read<Member>(i => i.ReadForSharing(r));
		}
	}
}
