﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class Hello
	{
		public int[]						Versions;
		public Guid							PeerId;
		public long							Roles;
		public Guid							ZoneId;
		public IPAddress					IP;
		public bool							Permanent;
		public string						Name;

		public void Write(BinaryWriter w)
		{
			w.Write(Versions, i => w.Write7BitEncodedInt(i));
			w.Write(ZoneId);
			w.Write(PeerId);
			w.WriteUtf8(Name);
			w.Write7BitEncodedInt64(Roles);
			w.Write(IP);
			w.Write(Permanent);
		}

		public void Read(BinaryReader r)
		{
			Versions			= r.ReadArray(() => r.Read7BitEncodedInt());
			ZoneId				= r.ReadGuid();
			PeerId				= r.ReadGuid();
			Name				= r.ReadUtf8();
			Roles				= r.Read7BitEncodedInt64();
			IP					= r.ReadIPAddress();
			Permanent			= r.ReadBoolean();
		}
	}
}