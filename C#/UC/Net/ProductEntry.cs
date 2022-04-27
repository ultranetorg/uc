using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class ProductEntry : Entry<ProductAddress>
	{
		public ProductAddress	Name;
		public string			Title;
		public int				LastRegistration = -1;
		public HashSet<int>		Releases = new();

		public override ProductAddress	Key => Name;

		public ProductEntry(ProductAddress name)
		{
			Name = name;
		}

		public ProductEntry Clone()
		{
			return	new ProductEntry(Name)
					{ 
						Title = Title, 
						Releases = new HashSet<int>(Releases),
						LastRegistration = LastRegistration,
					};
		}


		public override void Write(BinaryWriter w)
		{
			w.Write(Name);
			w.Write(Title);
			w.Write7BitEncodedInt(LastRegistration);
			w.Write(Releases);
		}

		public override void Read(BinaryReader r)
		{
			Name				= r.ReadProductAddress();
			Title				= r.ReadUtf8();
			LastRegistration	= r.Read7BitEncodedInt();
			Releases			= r.ReadHashSet(() => r.Read7BitEncodedInt());
		}
	}
}
