using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class PackageAddressPack : IBinarySerializable
	{
		public Dictionary<ReleaseAddress, Distributive> Items;

		public PackageAddressPack()
		{
		}

		public PackageAddressPack(Dictionary<ReleaseAddress, Distributive> packages)
		{
			Items = packages;
		}

		public void Write(BinaryWriter writer)
		{
			var ds = Items.GroupBy(i => i.Value);
			writer.Write7BitEncodedInt(ds.Count());

			foreach(var d in ds)
			{
				writer.Write((byte)d.Key);

				var aths = d.GroupBy(i => i.Key.Author);
				writer.Write7BitEncodedInt(aths.Count());

				foreach(var a in aths)
				{
					writer.WriteUtf8(a.Key);

					var fs = a.GroupBy(i => i.Key.Realization);
					writer.Write7BitEncodedInt(fs.Count());

					foreach(var f in fs)
					{
						writer.WriteUtf8(f.Key);

						var ps = f.GroupBy(i => i.Key.Product);
						writer.Write7BitEncodedInt(ps.Count());

						foreach(var p in ps)
						{
							writer.WriteUtf8(p.Key);
							writer.Write(p, i => i.Key.Version.Write(writer));
						}
					}
				}
			}
		}

		public void Read(BinaryReader reader)
		{
			var list = new Dictionary<ReleaseAddress, Distributive>();

			var dn = reader.Read7BitEncodedInt();

			for(int i=0; i<dn; i++)
			{
				var d = (Distributive)reader.ReadByte();
				var an = reader.Read7BitEncodedInt();

				for(int j=0; j<an; j++)
				{
					var a = reader.ReadUtf8();
					var fn = reader.Read7BitEncodedInt();

					for(int k=0; k<fn; k++)
					{
						var f = reader.ReadUtf8();
						var pn = reader.Read7BitEncodedInt();

						for(int l=0; l<pn; l++)
						{
							var p = reader.ReadUtf8();

							foreach(var v in reader.ReadArray<Version>())
							{
								list.Add(new ReleaseAddress(a, p, f, v), d);
							}
						}
					}
				}
			}

			Items = list;
		}
	}
}
