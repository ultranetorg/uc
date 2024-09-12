using System.Diagnostics;

namespace Uccs.Rdn
{
	public class DomainEntry : Domain, ITableEntry<string>
	{
		public string			Key => Address;
		
		//[JsonIgnore]
		public bool				New;
		public bool				Affected;
		Mcv						Mcv;
		bool					ResourcesCloned;
		
		public Resource[]		Resources { get; set; } = [];

		public DomainEntry()
		{
		}

		public DomainEntry(Mcv chain)
		{
			Mcv = chain;
		}

		public override string ToString()
		{
			return $"{Address}, {Owner}, {Expiration}, {FirstBidTime}, {LastWinner}, {LastBid}, {LastBidTime}";
		}

		public DomainEntry Clone()
		{
			return new DomainEntry(Mcv) {	Id = Id,
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
											Resources = Resources,
											NextResourceId = NextResourceId,
											SpaceReserved = SpaceReserved,
											SpaceUsed = SpaceUsed };
		}

		public void WriteMain(BinaryWriter writer)
		{
			var f = DomainFlag.None;
			
			if(LastWinner != null)	f |= DomainFlag.Auction;
			if(Owner != null)		f |= DomainFlag.Owned;
			if(ComOwner != null)	f |= DomainFlag.ComOwned;
			if(OrgOwner != null)	f |= DomainFlag.OrgOwned;
			if(NetOwner != null)	f |= DomainFlag.NetOwned;

			writer.Write((byte)f);
			writer.WriteUtf8(Address);
			writer.Write7BitEncodedInt(NextResourceId);
			writer.Write7BitEncodedInt(SpaceReserved);
			writer.Write7BitEncodedInt(SpaceUsed);

			if(IsWeb(Address))
			{
				if(f.HasFlag(DomainFlag.Auction))
				{
					writer.Write(FirstBidTime);
					writer.Write(LastWinner);
					writer.Write(LastBidTime);
					writer.Write7BitEncodedInt64(LastBid);
				}

				if(f.HasFlag(DomainFlag.ComOwned))	writer.Write(ComOwner);
				if(f.HasFlag(DomainFlag.OrgOwned))	writer.Write(OrgOwner);
				if(f.HasFlag(DomainFlag.NetOwned))	writer.Write(NetOwner);
			}

			if(f.HasFlag(DomainFlag.Owned))
			{
				writer.Write(Owner);
				writer.Write(Expiration);
			}

			if(IsChild(Address))
			{
				writer.Write((byte)ParentPolicy);
			}

			writer.Write(Resources, i =>{
											writer.Write7BitEncodedInt(i.Id.Ri);
											writer.WriteUtf8(i.Address.Resource);
											i.WriteMain(writer);
										});
		}

		public void ReadMain(BinaryReader reader)
		{
			var f			= (DomainFlag)reader.ReadByte();
			Address			= reader.ReadUtf8();
			NextResourceId	= reader.Read7BitEncodedInt();
			SpaceReserved	= (short)reader.Read7BitEncodedInt();
			SpaceUsed		= (short)reader.Read7BitEncodedInt();

			if(IsWeb(Address))
			{
				if(f.HasFlag(DomainFlag.Auction))
				{
					FirstBidTime	= reader.Read<Time>();
					LastWinner		= reader.Read<EntityId>();
					LastBidTime		= reader.Read<Time>();
					LastBid			= reader.Read7BitEncodedInt64();
				}

				if(f.HasFlag(DomainFlag.ComOwned))	ComOwner = reader.Read<EntityId>();
				if(f.HasFlag(DomainFlag.OrgOwned))	OrgOwner = reader.Read<EntityId>();
				if(f.HasFlag(DomainFlag.NetOwned))	NetOwner = reader.Read<EntityId>();
			}

			if(f.HasFlag(DomainFlag.Owned))
			{
				Owner		= reader.Read<EntityId>();
				Expiration	= reader.Read<Time>();
			}

			if(IsChild(Address))
			{
				ParentPolicy = (DomainChildPolicy)reader.ReadByte();
			}

			Resources = reader.Read(() =>	{ 
												var a = new Resource();
												a.Id = new ResourceId(Id.Ci, Id.Ei, reader.Read7BitEncodedInt());
												a.Address = new Ura{Domain = Address, 
																	Resource = reader.ReadUtf8()};
												a.ReadMain(reader);
												return a;
											}).ToArray();
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}

		public Resource AffectResource(string resource)
		{
			if(!Affected)
				Debugger.Break();

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
										Address = new Ura(Address, resource),
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
