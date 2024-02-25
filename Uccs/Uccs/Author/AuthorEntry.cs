using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AuthorEntry : Author, ITableEntry<string>
	{
		public string		Key => Name;
		public bool			New { get; set; }
		
		Mcv					Chain;
		List<Resource>		AffectedResources = new();
		public Resource[]	Resources { get; set; } = {};

		public AuthorEntry()
		{
		}

		public AuthorEntry(Mcv chain)
		{
			Chain = chain;
		}

		public override string ToString()
		{
			return $"{Name}, {Owner}, {Expiration}, {FirstBidTime}, {LastWinner}, {LastBid}, {LastBidTime}";
		}

		public AuthorEntry Clone()
		{
			return new AuthorEntry(Chain){
											Id = Id,
											Name = Name,
											Owner = Owner,
											Expiration = Expiration,
											FirstBidTime = FirstBidTime,
											LastWinner = LastWinner,
											LastBid = LastBid,
											LastBidTime = LastBidTime,
											DomainOwnersOnly = DomainOwnersOnly,
											Resources = Resources,
											NextResourceId = NextResourceId,
											SpaceReserved = SpaceReserved,
											SpaceUsed = SpaceUsed
										};
		}

		public void WriteMain(BinaryWriter w)
		{
			Write(w);

			w.Write(Resources.Where(i => i.Address.Type == ResourceType.Variable), i =>	{
																							w.Write7BitEncodedInt(i.Id.Ri);
																							w.WriteUtf8(i.Address.Resource);
																							i.Write(w);
																						});

			w.Write(Resources.Where(i => i.Address.Type == ResourceType.Constant), i =>	{
																							w.Write7BitEncodedInt(i.Id.Ri);
																							w.WriteUtf8(i.Address.Resource);
																							i.Write(w);
																						});
		}

		public void ReadMain(BinaryReader reader)
		{
			Read(reader);

			Resources = reader.Read(() =>	{ 
												var a = new Resource();
												a.Id = new ResourceId(Id.Ci, Id.Ei, reader.Read7BitEncodedInt());
												a.Address = new ResourceAddress{Type = ResourceType.Variable,
																				Author = Name, 
																				Resource = reader.ReadUtf8()};
												a.Read(reader);
												return a;
											})
					.Concat(reader.Read(() =>{ 
												var a = new Resource();
												a.Id = new ResourceId(Id.Ci, Id.Ei, reader.Read7BitEncodedInt());
												a.Address = new ResourceAddress{Type = ResourceType.Constant,
																				Author = Name, 
																				Resource = reader.ReadUtf8()};
												a.Read(reader);
												return a;
											})).ToArray();
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}

		public Resource AffectResource(ResourceAddress resource)
		{
			var r = AffectedResources.Find(i => i.Address == resource);
			
			if(r != null)
				return r;

			var i = Array.FindIndex(Resources, i => i.Address == resource);

			if(i != -1)
			{
				Resources = Resources.ToArray();

				r = Resources[i].Clone();
				Resources[i] = r;
			} 
			else
			{
				r = new Resource{Address = resource, Id = new ResourceId(Id.Ci, Id.Ei, NextResourceId++), New = true};
				
				Resources = Resources.Append(r).ToArray();
			}

			AffectedResources.Add(r);

			return r;
		}
	}
}
