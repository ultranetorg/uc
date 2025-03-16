namespace Uccs.Rdn;

public class ResourceEntry : Resource, ITableEntry
{
	public bool				Deleted { get; set; }
	RdnMcv					Mcv;

	public ResourceEntry()
	{
	}

	public ResourceEntry(RdnMcv rdn)
	{
		Mcv = rdn;
	}

	public ResourceEntry Clone()
	{
		return new ResourceEntry(Mcv)  {Id = Id,
										Domain = Domain,
						                Address	= Address, 
						                Flags = Flags,
						                Data = Data?.Clone(),
						                Updated = Updated,
						                Outbounds = Outbounds,
						                Inbounds = Inbounds};
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write7BitEncodedInt(Domain.E);
		writer.WriteUtf8(Address.Resource);
		writer.Write(Flags);
		writer.Write(Updated);
		
		if(Flags.HasFlag(ResourceFlags.Data))
			writer.Write(Data);
	
		writer.Write(Outbounds);
		writer.Write(Inbounds);
	}

	public void ReadMain(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Domain	= new (Id.B, reader.Read7BitEncodedInt());
		Address = new Ura(null, reader.ReadUtf8());
		Flags	= reader.Read<ResourceFlags>();
		Updated	= reader.Read<Time>();

		if(Flags.HasFlag(ResourceFlags.Data))
			Data = reader.Read<ResourceData>();

		Outbounds	= reader.ReadArray<ResourceLink>();
		Inbounds	= reader.ReadArray<EntityId>();
	}
	
// 		public void WriteMain(BinaryWriter writer)
// 		{
// 			writer.Write7BitEncodedInt(NextResourceId);
//  			writer.Write(Resources, i =>{
//  											writer.Write7BitEncodedInt(i.Id.Ri);
//  											writer.WriteUtf8(i.Address.Resource);
//  											i.WriteMain(writer);
//  										});
// 		}
// 
// 		public void ReadMain(BinaryReader reader)
// 		{
//             var d = Mcv.Domains.FindEntry(Id);
// 
// 			NextResourceId	= reader.Read7BitEncodedInt();
//  			Resources = reader.ReadArray(() =>	{ 
// 													var a = new Resource();
// 													a.Id = new ResourceId(Id.Ci, Id.Ei, reader.Read7BitEncodedInt());
// 													a.Address = new Ura(d.Address, reader.ReadUtf8());
// 													a.ReadMain(reader);
// 
// 													return a;
//  											    });
// 		}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

	public void Cleanup(Round lastInCommit)
	{
	}

//  		public Resource AffectResource(DomainEntry domain, string resource)
//  		{
//  			//if(!Affected)
//  			//	Debugger.Break();
//  
//  			var i = Resources == null ? -1 : Array.FindIndex(Resources, i => i.Address.Resource == resource);
//  
//  			if(i != -1)
//  			{
//  				if(!ResourcesCloned && Resources[i].Affected)
//  					Debugger.Break();
//  
//  				if(!ResourcesCloned)
//  				{
//  					Resources = Resources.ToArray();
//  					ResourcesCloned = true;
//  				}
//  
//  				if(!Resources[i].Affected)
//  				{
//  					Resources[i] = Resources[i].Clone();
//  					Resources[i].Affected = true;
//  				}
//  				
//  				return Resources[i];
//  			} 
//  			else
//  			{
//  				var r = new Resource {	Affected = true,
//  										New = true,
//  										Address = new Ura(domain.Address, resource),
//  										Id = new ResourceId(Id.Ci, Id.Ei, NextResourceId++) };
//  
//  				Resources = Resources == null ? [r] : [..Resources, r];
//  				ResourcesCloned = true;
//  
//  				return r;
//  			}
//  		}
 
// 		public void DeleteResource(Resource resource)
// 		{
// 			Resources = Resources.Where(i => i != resource).ToArray();
// 			ResourcesCloned = true;
// 		}
}
