using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
// 	public class ProductEntryRealization : IBinarySerializable
// 	{
// 		public string		Name;
// 		public Osbi[]		OSes;
// 
// 		public void Read(BinaryReader r)
// 		{
// 			Name	= r.ReadUtf8();
// 			OSes	= r.ReadArray<Osbi>();
// 		}
// 
// 		public void Write(BinaryWriter w)
// 		{
// 			w.WriteUtf8(Name);
// 			w.Write(OSes);
// 		}
// 
// 		public ProductEntryRealization Clone()
// 		{
// 			return new ProductEntryRealization{	Name = Name, 
// 												OSes = OSes.Clone() as Osbi[]};
// 		}
// 	}

	public class ProductEntryRelease : IBinarySerializable
	{
		public string		Realization;
		public Version		Version;
		public string		Channel; /// stable, beta, nightly, debug,...
		public int			Rid;

		public ProductEntryRelease()
		{
		}

		public ProductEntryRelease(string platform, Version version, string channel, int rid)
		{
			Realization = platform;
			Version = version;
			Channel = channel;
			Rid = rid;
		}

		public ProductEntryRelease Clone()
		{
			return new ProductEntryRelease(Realization, Version, Channel, Rid);
		}

		public void Read(BinaryReader r)
		{
			Realization = r.ReadUtf8();
			Version = r.Read<Version>();
			Channel = r.ReadUtf8();
			Rid = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Realization);
			w.Write(Version);
			w.WriteUtf8(Channel);
			w.Write7BitEncodedInt(Rid);
		}
	}

	public class ProductEntry : Product, ITableEntry<ProductAddress>
	{
		public ProductAddress					Key => Address;
		public byte[]							GetClusterKey(int n) => Encoding.UTF8.GetBytes(Address.Author).Take(n).ToArray();

		public ProductEntry()
		{
		}

		public ProductEntry Clone()
		{
			return	new ProductEntry()
					{ 
						Address				= Address,
						Title				= Title, 
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
