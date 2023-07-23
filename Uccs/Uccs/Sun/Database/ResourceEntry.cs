using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class ResourceEntry : Resource, ITableEntry<ResourceAddress>
	{
		public ResourceAddress	Key => Address;
		public byte[]			GetClusterKey(int n) =>  Encoding.UTF8.GetBytes(Address.Author).Take(n).ToArray();

		public ResourceEntry Clone()
		{
			return	new() 
					{ 
						Address	= Address, 
						Data = Data.Clone() as byte[],
						//Channel	= Channel,
						AnalysisStage = AnalysisStage,
						AnalysisFee = AnalysisFee,
						RoundId = RoundId,
						AnalysisQuorumRid = AnalysisQuorumRid,
						Good = Good,
						Bad = Bad,
					};
		}

		public void WriteMain(BinaryWriter w)
		{
			Write(w);
		}

		public void ReadMain(BinaryReader r)
		{
			Read(r);
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}
	}
}
