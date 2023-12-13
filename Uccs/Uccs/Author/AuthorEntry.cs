using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Nethereum.Model;
using RocksDbSharp;

namespace Uccs.Net
{
	public class AuthorEntry : Author, ITableEntry<string>
	{
		public string		Key => Name;
		public Span<byte>	GetClusterKey(int n) => new Span<byte>(Encoding.UTF8.GetBytes(Name, 0, n));

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
			return new AuthorEntry(Chain)
					{
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

			w.Write(Resources, i =>	{
										w.WriteUtf8(i.Address.Resource);
										i.Write(w);
									});
		}

		public void ReadMain(BinaryReader reader)
		{
			Read(reader);

			Resources = reader.ReadArray(() => { 
													var a = new Resource();
													a.Address = new ResourceAddress(Name, reader.ReadUtf8());
													a.Read(reader);
													return a;
												});
		}

		public void WriteMore(BinaryWriter w)
		{
			//w.Write7BitEncodedInt(ObtainedRid);

			//if(RegistrationTime != ChainTime.Zero)
			//{
			//	w.Write(Products);
			//}
		}

		public void ReadMore(BinaryReader r)
		{
			//ObtainedRid = r.Read7BitEncodedInt();

			//if(RegistrationTime != ChainTime.Zero)
			//{
			//	Products = r.ReadStings();
			//}
		}

		//public Resource AffectResource(Resource release)
		//{
		//	var i = Array.FindIndex(Resources, i => i.Address == release)  
		//
		//	if(AffectedResources.ContainsKey(release.Address))
		//		return release;
		//	
		//	return AffectedResources[release.Address] = release.Clone();
		//}

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
				r = new Resource{Address = resource, Id = NextResourceId++};
				
				Resources = Resources.Append(r).ToArray();
			}

			AffectedResources.Add(r);

			return r;
		}
	}
}
