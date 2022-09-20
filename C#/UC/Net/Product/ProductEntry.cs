using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class ProductEntry : Entry<ProductAddress>
	{
		public ProductAddress			Address;
		public string					Title;
		public int						LastRegistration = -1;
		public List<ReleaseEntry>		Releases = new();
		public List<RealizationEntry>	Realizations = new();

		public override ProductAddress	Key => Address;

		public ProductEntry(ProductAddress address)
		{
			Address = address;
		}

		public ProductEntry Clone()
		{
			return	new ProductEntry(Address)
					{ 
						Title				= Title, 
						LastRegistration	= LastRegistration,
						Releases			= Releases.Select(i => i.Clone()).ToList(),
						Realizations		= Realizations.Select(i => i.Clone()).ToList(),
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(Title);
			w.Write7BitEncodedInt(LastRegistration);
			w.Write(Releases);
			w.Write(Realizations);
		}

		public override void Read(BinaryReader r)
		{
			Address				= r.Read<ProductAddress>();
			Title				= r.ReadString();
			LastRegistration	= r.Read7BitEncodedInt();
			Releases			= r.ReadList<ReleaseEntry>();
			Realizations		= r.ReadList<RealizationEntry>();	
		}
	}
}
