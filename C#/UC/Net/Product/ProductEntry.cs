using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class ProductEntryRealization : IBinarySerializable
	{
		public string		Name;
		public Osbi[]		OSes;

		public void Read(BinaryReader r)
		{
			Name	= r.ReadUtf8();
			OSes	= r.ReadArray<Osbi>();
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.Write(OSes);
		}

		public ProductEntryRealization Clone()
		{
			return new ProductEntryRealization{	Name = Name, 
												OSes = OSes.Clone() as Osbi[]};
		}
	}

	public class ProductEntryRelease : IBinarySerializable
	{
		public string		Platform;
		public Version		Version;
		public string		Channel; /// stable, beta, nightly, debug,...
		public int			Rid;

		public ProductEntryRelease()
		{
		}

		public ProductEntryRelease(string platform, Version version, string channel, int rid)
		{
			Platform = platform;
			Version = version;
			Channel = channel;
			Rid = rid;
		}

		public ProductEntryRelease Clone()
		{
			return new ProductEntryRelease(Platform, Version, Channel, Rid);
		}

		public void Read(BinaryReader r)
		{
			Platform = r.ReadUtf8();
			Version = r.Read<Version>();
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

	public class ProductEntry : TableEntry<ProductAddress>
	{
		public override ProductAddress			Key => Address;
		public override byte[]					ClusterKey => Encoding.UTF8.GetBytes(Address.Author).Take(ClusterKeyLength).ToArray();

		public ProductAddress					Address;
		public string							Title;

		public int								LastRegistration = -1;
		public List<ProductEntryRelease>		Releases = new();
		public List<ProductEntryRealization>	Realizations = new();

		public ProductEntry()
		{
		}

		public ProductEntry Clone()
		{
			return	new ProductEntry()
					{ 
						Address				= Address,
						Title				= Title, 
						LastRegistration	= LastRegistration,
						Releases			= Releases.Select(i => i.Clone()).ToList(),
						Realizations		= Realizations.Select(i => i.Clone()).ToList()
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Title);
		}

		public override void Read(BinaryReader r)
		{
			Address		= r.Read<ProductAddress>();
			Title		= r.ReadUtf8();
		}

		public override void WriteMore(BinaryWriter w)
		{
			w.Write7BitEncodedInt(LastRegistration);
			w.Write(Releases);
			w.Write(Realizations);
		}

		public override void ReadMore(BinaryReader r)
		{
			LastRegistration	= r.Read7BitEncodedInt();
			Releases			= r.ReadList<ProductEntryRelease>();
			Realizations		= r.ReadList<ProductEntryRealization>();	
		}
	}
}
