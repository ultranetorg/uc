using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Net
{
	public class Product : IBinarySerializable
	{

		public ProductAddress					Address;
		public string							Title;

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Title);
		}

		public void Read(BinaryReader r)
		{
			Address		= r.Read<ProductAddress>();
			Title		= r.ReadUtf8();
		}
	}
}
