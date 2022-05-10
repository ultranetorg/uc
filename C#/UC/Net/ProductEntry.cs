using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class Release : IBinarySerializable
	{
		public string			Platform;
		public Version			Version;
		public string			Channel;		/// stable, beta, nightly, debug,...
		public int				Rid;

		public Release()
		{
		}

		public Release(string platform, Version version, string channel, int rid)
		{
			Platform = platform;
			Version = version;
			Channel = channel;
			Rid = rid;
		}

		public Release Clone()
		{
			return new Release(Platform, Version, Channel, Rid);
		}

		public void Read(BinaryReader r)
		{
			Platform = r.ReadUtf8();
			Version = r.ReadVersion();
			Channel = r.ReadUtf8();
			Rid = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Platform);
			w.Write(Version);
			w.WriteUtf8(Channel);
			w.Write7BitEncodedInt(Rid);
		}
	}

	public class ProductEntry : Entry<ProductAddress>
	{
		public ProductAddress			Address;
		public string					Title;
		public int						LastRegistration = -1;
		public List<Release>			Releases = new();

		public override ProductAddress	Key => Address;

		public ProductEntry(ProductAddress address)
		{
			Address = address;
		}

		public ProductEntry Clone()
		{
			return	new ProductEntry(Address)
					{ 
						Title = Title, 
						LastRegistration = LastRegistration,
						Releases = Releases.Select(i => i.Clone()).ToList(),
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(Title);
			w.Write7BitEncodedInt(LastRegistration);
			w.Write(Releases);
		}

		public override void Read(BinaryReader r)
		{
			Address				= r.Read<ProductAddress>();
			Title				= r.ReadUtf8();
			LastRegistration	= r.Read7BitEncodedInt();
			Releases			= r.ReadList<Release>();
		}
	}
}
