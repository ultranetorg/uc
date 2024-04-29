using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public class DomainEntry : Domain, ITableEntry<string>
	{
		public string			Key => Address;
		
		[JsonIgnore]
		public bool				New { get; set; }
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

		public void WriteMain(BinaryWriter w)
		{
			Write(w);

			w.Write(Resources, i =>	{
										w.Write7BitEncodedInt(i.Id.Ri);
										w.WriteUtf8(i.Address.Resource);
										i.WriteMain(w);
									});
		}

		public void ReadMain(BinaryReader reader)
		{
			Read(reader);

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
