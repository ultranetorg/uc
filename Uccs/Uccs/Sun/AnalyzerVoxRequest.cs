using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalyzerVoxRequest : RdcRequest
	{
		public int							RoundId { get; set; }
		public IEnumerable<Analysis>		Analyses { get; set; }
		public byte[]						Signature { get; set; }

		public AccountAddress				Account;
		public override bool				WaitResponse => false;

		public AnalyzerVoxRequest()
		{
		}

		public override string ToString()
		{
			return base.ToString() + $", Account={Account}, Analyses={{{Analyses.Count()}}}";
		}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				return null;
			}
		}
		
		public void Sign(Zone zone, AccountKey account)
		{
			Account = account;
			Signature = zone.Cryptography.Sign(account, Hashify(zone));
		}
		
		public byte[] Hashify(Zone zone)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(RoundId);
			w.Write(Analyses);

			return zone.Cryptography.Hash(s.ToArray());
		}
		
		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write(Analyses);
			writer.Write(Signature);
		}
		
		public void Read(BinaryReader reader, Zone zone)
		{
			RoundId		= reader.Read7BitEncodedInt();
			Analyses	= reader.ReadArray<Analysis>();
			Signature	= reader.ReadSignature();
		
			Account = zone.Cryptography.AccountFrom(Signature, Hashify(zone));
		}
	}
}
