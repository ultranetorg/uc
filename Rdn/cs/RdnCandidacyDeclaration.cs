﻿using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Rdn
{
	public class RdnCandidacyDeclaration : CandidacyDeclaration
	{
		public IPAddress[]		SeedHubRdcIPs;
		
		public RdnCandidacyDeclaration()
		{
		}

		public RdnCandidacyDeclaration(Unit bail, IPAddress[] baseRdcIPs, IPAddress[] seedHubRdcIPs)
		{
			Bail = bail;
			BaseRdcIPs = baseRdcIPs;
			SeedHubRdcIPs = seedHubRdcIPs;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			base.ReadConfirmed(reader);
			SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			base.WriteConfirmed(writer);
			writer.Write(SeedHubRdcIPs, i => writer.Write(i));
		}
	}
}