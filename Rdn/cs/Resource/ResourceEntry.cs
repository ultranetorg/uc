namespace Uccs.Rdn;

public class ResourceEntry : Resource, ITableEntry
{
	public BaseId			BaseId => Id;
	public bool				Deleted { get; set; }
	
	//public new EntityId		Id { get => base.Id; set => base.Id = value as ResourceId; }
	//public Resource[]		Resources { get; set; } = [];

	//public bool				New;
	//public bool			Affected;
	RdnMcv					Mcv;
	//bool					ResourcesCloned;

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
						                Address	= Address, 
						                Flags = Flags,
						                Data = Data?.Clone(),
						                Updated = Updated,
						                Outbounds = Outbounds,
						                Inbounds = Inbounds};
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
