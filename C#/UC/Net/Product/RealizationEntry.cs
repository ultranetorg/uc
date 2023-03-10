using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class RealizationEntry : TableEntry<RealizationAddress>
	{
		public override RealizationAddress	Key => Address;
		public override byte[]				ClusterKey => Encoding.UTF8.GetBytes(Address.Author).Take(ClusterKeyLength).ToArray();

		public RealizationAddress			Address;
		public Osbi[]						OSes;

		public int							LastRegistrationRid = -1;

		public RealizationEntry Clone()
		{
			return	new()
					{ 
						Address			= Address, 
						OSes			= OSes.ToArray(),
						LastRegistrationRid = LastRegistrationRid
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(OSes);
		}

		public override void Read(BinaryReader r)
		{
			Address	= r.Read<RealizationAddress>();
			OSes	= r.ReadArray<Osbi>();
		}

		public override void WriteMore(BinaryWriter w)
		{
			w.Write7BitEncodedInt(LastRegistrationRid);
		}

		public override void ReadMore(BinaryReader r)
		{
			LastRegistrationRid = r.Read7BitEncodedInt();
		}
	}
}
