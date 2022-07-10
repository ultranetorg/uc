using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class PackageAddressGroup : IBinarySerializable
	{
		public IEnumerable<PackageAddress> Items;

		public PackageAddressGroup()
		{
		}

		public PackageAddressGroup(IEnumerable<PackageAddress> packages)
		{
			Items = packages;
		}

		public void Write(BinaryWriter writer)
		{
			var ds = Items.GroupBy(i => i.Distribution);
			writer.Write7BitEncodedInt(ds.Count());

			foreach(var d in ds)
			{
				writer.Write((byte)d.Key);

				var pfs = d.GroupBy(i => i.Platform);
				writer.Write7BitEncodedInt(pfs.Count());

				foreach(var pf in pfs)
				{
					writer.WriteUtf8(pf.Key);

					var aths = pf.GroupBy(i => i.Author);
					writer.Write7BitEncodedInt(aths.Count());

					foreach(var a in aths)
					{
						writer.WriteUtf8(a.Key);

						var prs = a.GroupBy(i => i.Product);
						writer.Write7BitEncodedInt(prs.Count());

						foreach(var pr in prs)
						{
							writer.WriteUtf8(pr.Key);

							writer.Write(pr, i => i.Version.Write(writer));
						}
					}
				}
			}
		}

		public void Read(BinaryReader reader)
		{
			var list = new List<PackageAddress>();

			var dn = reader.Read7BitEncodedInt();

			for(int i=0; i<dn; i++)
			{
				var d = (Distribution)reader.ReadByte();

				var pfn = reader.Read7BitEncodedInt();

				for(int j=0; j<pfn; j++)
				{
					var pf = reader.ReadUtf8();

					var an = reader.Read7BitEncodedInt();

					for(int k=0; k<an; k++)
					{
						var a = reader.ReadUtf8();

						var prn = reader.Read7BitEncodedInt();

						for(int l=0; l<prn; l++)
						{
							var pr = reader.ReadUtf8();

							list.AddRange(reader.ReadArray(() => new PackageAddress(a, pr, reader.Read<Version>(), pf, d)));
						}
					}
				}
			}

			Items = list;
		}
	}
}
