using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UC.Net
{
	public class Hello
	{
		public int[]		Versions;
		public Guid			Session;
		public string		Zone;
		public IPAddress	IP;
		public Peer[]		Peers;
		public int			LastRound;
		public int			LastConfirmedRound;
	}
}
