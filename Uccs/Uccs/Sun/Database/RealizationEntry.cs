using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class RealizationEntry : Realization, ITableEntry<RealizationAddress>
	{
		public RealizationAddress	Key => Address;
		public byte[]				GetClusterKey(int n) => Encoding.UTF8.GetBytes(Address.Product.Author).Take(n).ToArray();

		public RealizationEntry Clone()
		{
			return	new()
					{ 
						Address			= Address, 
						//OSes			= OSes.ToArray(),
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
