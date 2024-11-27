using System.Diagnostics;

namespace Uccs.Rdn
{
	public class SiteEntry : ITableEntry
	{
		public EntityId			Id { get; set; }
		public int				NextResourceId { get; set; }
		public Resource[]		Resources { get; set; } = [];

		public bool				New;
		//public bool			Affected;
		RdnMcv					Mcv;
		bool					ResourcesCloned;

		public SiteEntry()
		{
		}

		public SiteEntry(RdnMcv rdn)
		{
			Mcv = rdn;
		}

		public override string ToString()
		{
			return $"{Id}, Resources={{{Resources.Length}}}";
		}

		public SiteEntry Clone()
		{
			return new SiteEntry(Mcv){	Id = Id,
										Resources = Resources,
										NextResourceId = NextResourceId,
										};
		}

		public void WriteMain(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(NextResourceId);

 			writer.Write(Resources, i =>{
 											writer.Write7BitEncodedInt(i.Id.Ri);
 											writer.WriteUtf8(i.Address.Resource);
 											i.WriteMain(writer);
 										});
		}

		public void ReadMain(BinaryReader reader)
		{
            var d = Mcv.Domains.FindEntry(Id);

			NextResourceId	= reader.Read7BitEncodedInt();

 			Resources = reader.Read(() =>	{ 
 												var a = new Resource();
 												a.Id = new ResourceId(Id.Ci, Id.Ei, reader.Read7BitEncodedInt());
 												a.Address = new Ura(d.Address, reader.ReadUtf8());
 												a.ReadMain(reader);

 												return a;
 											})
                                            .ToArray();
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}

		public void Cleanup(Round lastInCommit)
		{
		}

 		public Resource AffectResource(DomainEntry domain, string resource)
 		{
            if(resource == "app/win32/0.0.4")
                resource = resource;
 			//if(!Affected)
 			//	Debugger.Break();
 
 			var i = Resources == null ? -1 : Array.FindIndex(Resources, i => i.Address.Resource == resource);
 
 			if(i != -1)
 			{
 				if(!ResourcesCloned && Resources[i].Affected)
 					Debugger.Break();
 
 				if(!ResourcesCloned)
 				{
 					Resources = Resources.ToArray();
 					ResourcesCloned = true;
 				}
 
 				if(!Resources[i].Affected)
 				{
 					Resources[i] = Resources[i].Clone();
 					Resources[i].Affected = true;
 				}
 				
 				return Resources[i];
 			} 
 			else
 			{
 				var r = new Resource {	Affected = true,
 										New = true,
 										Address = new Ura(domain.Address, resource),
 										Id = new ResourceId(Id.Ci, Id.Ei, NextResourceId++) };
 
 				Resources = Resources == null ? [r] : Resources.Append(r).ToArray();
 				ResourcesCloned = true;
 
 				return r;
 			}
 		}
 
 		public void DeleteResource(Resource resource)
 		{
 			Resources = Resources.Where(i => i != resource).ToArray();
 			ResourcesCloned = true;
 		}
	}
}
