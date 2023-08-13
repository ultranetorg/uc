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
		public byte[]		GetClusterKey(int n) => Encoding.UTF8.GetBytes(Name).Take(n).ToArray();

		Chainbase			Chain;
		List<Resource>		AffectedResources = new();

		public AuthorEntry()
		{
		}

		public AuthorEntry(Chainbase chain)
		{
			Chain = chain;
		}

		public override string ToString()
		{
			return $"{Name}, {Title}, {Owner}, {Expiration}, {FirstBidTime}, {LastWinner}, {LastBid}, {LastBidTime}";
		}

		public AuthorEntry Clone()
		{
			return new AuthorEntry(Chain)
					{
						Name = Name,
						Title = Title,
						Owner = Owner,
						Expiration = Expiration,
						FirstBidTime = FirstBidTime,
						LastWinner = LastWinner,
						LastBid = LastBid,
						LastBidTime = LastBidTime,
						DomainOwnersOnly = DomainOwnersOnly,
						Resources = Resources,
						NextResourceId = NextResourceId,
					};
		}

		public void WriteMain(BinaryWriter w)
		{
			Write(w);
		}

		public void ReadMain(BinaryReader r)
		{
			Read(r);
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
