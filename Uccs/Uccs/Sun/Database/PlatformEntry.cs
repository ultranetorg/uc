using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class PlatformEntry : Platform, ITableEntry<PlatformAddress>
	{
		public PlatformAddress		Key => Address;
		public byte[]				GetClusterKey(int n) => Encoding.UTF8.GetBytes(Address.Author).Take(n).ToArray();

		public PlatformEntry Clone()
		{
			return	new()
					{ 
						Address			= Address, 
						OSes			= OSes.ToArray(),
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
