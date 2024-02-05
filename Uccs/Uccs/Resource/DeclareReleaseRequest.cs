namespace Uccs.Net
{
	public class DeclareReleaseRequest : RdcRequest//, IBinarySerializable
	{
		public ResourceDeclaration[]	Resources { get; set; }
		public override bool			WaitResponse => true;

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
				RequireMember(sun);

			lock(sun.SeedHub.Lock)
				return new DeclareReleaseResponse {Results = sun.SeedHub.ProcessIncoming(Peer.IP, Resources).ToArray() };
		}
	}

	public enum DeclarationResult
	{
		None, Accepted, ResourceNotFound, ReleaseNotFound, NotRelease, NotNearest, 
	}

	public class ReleaseDeclarationResult
	{
		public ReleaseAddress		Address { get; set; }
		public DeclarationResult	Result { get; set; }	
	}

	public class DeclareReleaseResponse : RdcResponse
	{
		public ReleaseDeclarationResult[]	Results { get; set; }
	}
// 		public void Write(BinaryWriter writer)
// 		{
// 			var aa = Releases.GroupBy(i => i.Key.Author);
// 
// 			writer.Write7BitEncodedInt(aa.Count());
// 			
// 			foreach(var a in aa)
// 			{
// 				writer.WriteUtf8(a.Key);
// 				writer.Write(a, i => {	writer.WriteUtf8(i.Key.Resource);
// 										writer.Write((byte)i.Value); });
// 			}
// 
// 			//var ds = Items.GroupBy(i => i.Value);
// 			//writer.Write7BitEncodedInt(ds.Count());
// 			//
// 			//foreach(var d in ds)
// 			//{
// 			//	writer.Write((byte)d.Key);
// 			//
// 			//	var aths = d.GroupBy(i => i.Key.Product.Author);
// 			//	writer.Write7BitEncodedInt(aths.Count());
// 			//
// 			//	foreach(var a in aths)
// 			//	{
// 			//		writer.WriteUtf8(a.Key);
// 			//
// 			//		var fs = a.GroupBy(i => i.Key.Realization.Name);
// 			//		writer.Write7BitEncodedInt(fs.Count());
// 			//
// 			//		foreach(var f in fs)
// 			//		{
// 			//			writer.WriteUtf8(f.Key);
// 			//
// 			//			var ps = f.GroupBy(i => i.Key.Product.Name);
// 			//			writer.Write7BitEncodedInt(ps.Count());
// 			//
// 			//			foreach(var p in ps)
// 			//			{
// 			//				writer.WriteUtf8(p.Key);
// 			//				writer.Write(p, i => i.Key.Version.Write(writer));
// 			//			}
// 			//		}
// 			//	}
// 			//}
// 		}
// 
// 		public void Read(BinaryReader reader)
// 		{
// 			Releases = new Dictionary<ResourceAddress, Availability>();
// 			
// 			var an = reader.Read7BitEncodedInt();
// 			
// 			for(int i=0; i<an; i++)
// 			{
// 				var a = reader.ReadUtf8();
// 
// 				var rn = reader.Read7BitEncodedInt();
// 			
// 				for(int j=0; j<rn; j++)
// 				{
// 					var r = reader.ReadUtf8();
// 
// 					Releases[new ResourceAddress(a, r)] = (Availability)reader.ReadByte();
// 				}
// 			}
// 
// 			//var list = new Dictionary<ReleaseAddress, Distributive>();
// 			//
// 			//var dn = reader.Read7BitEncodedInt();
// 			//
// 			//for(int i=0; i<dn; i++)
// 			//{
// 			//	var d = (Distributive)reader.ReadByte();
// 			//	var an = reader.Read7BitEncodedInt();
// 			//
// 			//	for(int j=0; j<an; j++)
// 			//	{
// 			//		var a = reader.ReadUtf8();
// 			//		var fn = reader.Read7BitEncodedInt();
// 			//
// 			//		for(int k=0; k<fn; k++)
// 			//		{
// 			//			var f = reader.ReadUtf8();
// 			//			var pn = reader.Read7BitEncodedInt();
// 			//
// 			//			for(int l=0; l<pn; l++)
// 			//			{
// 			//				var p = reader.ReadUtf8();
// 			//
// 			//				foreach(var v in reader.ReadArray<Version>())
// 			//				{
// 			//					list.Add(new ReleaseAddress(a, p, f, v), d);
// 			//				}
// 			//			}
// 			//		}
// 			//	}
// 			//}
// 			//
// 			//Items = list;
// 		}
}
