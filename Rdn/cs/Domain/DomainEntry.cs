namespace Uccs.Rdn;

public class DomainEntry : Domain, ITableEntry
{
	public bool				Deleted { get; set; }
	Mcv						Mcv;
	//bool					ResourcesCloned;
	
	//public Resource[]		Resources { get; set; } = [];

	public DomainEntry()
	{
	}

	public DomainEntry(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Address}, {Id}, Owner={Owner}, {Expiration}, {FirstBidTime}, {LastWinner}, {LastBid}, {LastBidTime}";
	}

	public DomainEntry Clone()
	{
		return new DomainEntry(Mcv){Id = Id,
									Address = Address,
									Owner = Owner,
									Expiration = Expiration,
									FirstBidTime = FirstBidTime,
									LastWinner = LastWinner,
									LastBid = LastBid,
									LastBidTime = LastBidTime,
									ComOwner = ComOwner,
									OrgOwner = OrgOwner,
									NetOwner = NetOwner,
									Space = Space,
									NtnChildNet = NtnChildNet,
									NtnSelfHash = NtnSelfHash,
									};
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

// 		public Resource AffectResource(string resource)
// 		{
// 			if(!Affected)
// 				Debugger.Break();
// 
// 			var i = Resources == null ? -1 : Array.FindIndex(Resources, i => i.Address.Resource == resource);
// 
// 			if(i != -1)
// 			{
// 				if(!ResourcesCloned && Resources[i].Affected)
// 					Debugger.Break();
// 
// 				if(!ResourcesCloned)
// 				{
// 					Resources = Resources.ToArray();
// 					ResourcesCloned = true;
// 				}
// 
// 				if(!Resources[i].Affected)
// 				{
// 					Resources[i] = Resources[i].Clone();
// 					Resources[i].Affected = true;
// 				}
// 				
// 				return Resources[i];
// 			} 
// 			else
// 			{
// 				var r = new Resource {	Affected = true,
// 										New = true,
// 										Address = new Ura(Address, resource),
// 										Id = new ResourceId(Id.Ci, Id.Ei, NextResourceId++) };
// 
// 				Resources = Resources == null ? [r] : Resources.Append(r).ToArray();
// 				ResourcesCloned = true;
// 
// 				return r;
// 			}
// 		}
// 
// 		public void DeleteResource(Resource resource)
// 		{
// 			Resources = Resources.Where(i => i != resource).ToArray();
// 			ResourcesCloned = true;
// 		}
}
