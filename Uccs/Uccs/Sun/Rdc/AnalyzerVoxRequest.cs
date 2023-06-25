using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Uccs.Net
{
	public enum AnalysisResult
	{
		NotVerified	= 0b00, 
		Clean		= 0b01,
		Infected	= 0b10,
	}

	public class AnalyzerVoxRequest : RdcRequest
	{
		public int							RoundId { get; set; }
		public IEnumerable<IPAddress>		IPs { get; set; }
		public IEnumerable<ReleaseAddress>	Clean { get; set; }
		public IEnumerable<ReleaseAddress>	Infected { get; set; }
		public byte[]						Signature { get; set; }

		public AccountAddress				Account;
		public override bool				WaitResponse => false;

		public AnalyzerVoxRequest()
		{
		}

		public override string ToString()
		{
			return base.ToString() + ", Account=" + Account + ", IP=" + string.Join(',', IPs as IEnumerable<IPAddress>);
		}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				return null;
			}
		}
		
		public byte[] Hashify(Zone zone)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(RoundId);
			w.Write(IPs, i => w.Write(i));
			w.Write(Clean);
			w.Write(Infected);

			return zone.Cryptography.Hash(s.ToArray());
		}
		
		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write(IPs, i => writer.Write(i));
			writer.Write(Clean);
			writer.Write(Infected);
			writer.Write(Signature);
		}
		
		public void Read(BinaryReader reader, Zone zone)
		{
			RoundId		= reader.Read7BitEncodedInt();
			IPs			= reader.ReadArray(() => reader.ReadIPAddress());
			Clean		= reader.ReadArray<ReleaseAddress>();
			Infected	= reader.ReadArray<ReleaseAddress>();
			Signature	= reader.ReadSignature();
		
			Account = zone.Cryptography.AccountFrom(Signature, Hashify(zone));
		}
	}
}
