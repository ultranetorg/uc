using System;
using System.IO;
using System.Linq;
using System.Net;
using Uccs.Net;
using Uocs;

namespace Tests
{
	class Program
	{
		public static void Main(string[] args)
		{
			var sun = new Sun(Zone.Localnet, new Settings() {Profile = $"{G.Dev.Tmp}\\Tests" });

			var rq = new MembersResponse()
			{
				Id = 123,
				Error = new NodeException(NodeError.AllNodesFailed),
				Members = new MembersResponse.Member[] {new MembersResponse.Member{ Account = AccountAddress.Zero, 
																					BaseRdcIPs = new IPAddress[] {IPAddress.Parse("1.1.1.1") },
																					SeedHubRdcIPs = new IPAddress[] {IPAddress.Parse("1.1.1.1") },
																					CastingSince = 345,
																					Proxyable = true
																					}}
			};

			
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			var r = new BinaryReader(s);

			BinarySerializator.Serialize(w, rq);

			s.Position = 0;

			rq = BinarySerializator.Deserialize<MembersResponse>(r, sun.Constract);
		}
	}
}
