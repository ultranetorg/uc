using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class ReleaseEntry : Release, ITableEntry<ReleaseAddress>
	{
		public ReleaseAddress	Key => Address;
		public byte[]			GetClusterKey(int n) =>  Encoding.UTF8.GetBytes(Address.Author).Take(n).ToArray();

		public ReleaseEntry Clone()
		{
			return	new() 
					{ 
						Address	= Address, 
						Manifest = Manifest.Clone() as byte[],
						Channel	= Channel
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
