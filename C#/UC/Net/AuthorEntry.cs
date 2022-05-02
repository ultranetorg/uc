using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RocksDbSharp;

namespace UC.Net
{
	public class Release : IBinarySerializable
	{
		public string			Platform;
		public Version			Version;
		public string			Channel;		/// stable, beta, nightly, debug,...
		public int				Rid;

		public Release()
		{
		}

		public Release(string platform, Version version, string channel, int rid)
		{
			Platform = platform;
			Version = version;
			Channel = channel;
			Rid = rid;
		}

		public Release Clone()
		{
			return new Release(Platform, Version, Channel, Rid);
		}

		public void Read(BinaryReader r)
		{
			Platform = r.ReadUtf8();
			Version = r.ReadVersion();
			Channel = r.ReadUtf8();
			Rid = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Platform);
			w.Write(Version);
			w.WriteUtf8(Channel);
			w.Write7BitEncodedInt(Rid);
		}
	}

	public class Product : IBinarySerializable
	{
		public string			Name;
		public List<Release>	Releases;

		public Product Clone()
		{
			return new Product {Name = Name, Releases = Releases.Select(i => i.Clone()).ToList()};
		}

		public void Read(BinaryReader r)
		{
			Name		= r.ReadUtf8();
			Releases	= r.ReadList<Release>();
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.Write(Releases);
		}
	}

	public class AuthorEntry : Entry<string>
	{
		public string				Name;
		public int					FirstBid = -1;
		public int					LastBid = -1;
		public int					LastRegistration = -1;
		public int					LastTransfer = -1;
		public List<Product>		Products = new();

		public override string		Key => Name;
		Roundchain					Chain;

		public AuthorEntry(Roundchain chain, string name)
		{
			Chain = chain;
			Name = name;
		}

		public AuthorEntry Clone()
		{
			return new AuthorEntry(Chain, Name)
					{ 
						FirstBid = FirstBid,
						LastBid = LastBid,
						LastRegistration = LastRegistration,
						LastTransfer = LastTransfer,
						Products = Products.Select(i => i.Clone()).ToList()
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(FirstBid);
			w.Write7BitEncodedInt(LastBid);
			w.Write7BitEncodedInt(LastRegistration);
			w.Write7BitEncodedInt(LastTransfer);
			w.Write(Products);
		}

		public override void Read(BinaryReader r)
		{
			FirstBid		= r.Read7BitEncodedInt();
			LastBid			= r.Read7BitEncodedInt();
			LastRegistration= r.Read7BitEncodedInt();
			LastTransfer	= r.Read7BitEncodedInt();
			Products		= r.ReadList<Product>();
		}

		public AuthorBid FindFirstBid(Round executing)
		{
			if(FirstBid != -1)
			{
				foreach(var b in Chain.FindRound(FirstBid).Payloads.AsEnumerable().Reverse())
					foreach(var t in b.SuccessfulTransactions.AsEnumerable().Reverse())
						foreach(var o in t.SuccessfulOperations.OfType<AuthorBid>().Reverse())
							if(o.Author == Name)
								return o;

				throw new IntegrityException("AuthorBid operation not found");
			}

			foreach(var r in Chain.Rounds.Where(i => i.Id < executing.Id).Reverse())
				foreach(var b in (r.Confirmed ? r.ConfirmedPayloads : r.Payloads).AsEnumerable().Reverse())
					foreach(var t in b.SuccessfulTransactions.AsEnumerable().Reverse())
						foreach(var o in t.SuccessfulOperations.AsEnumerable().Reverse())
							if(o is AuthorBid ab && ab.Author == Name)
								return ab;


			return executing.EffectiveOperations.Reverse().OfType<AuthorBid>().FirstOrDefault(i => i.Author == Name);
		}

		public AuthorBid FindLastBid(Round executing)
		{
			return	executing.EffectiveOperations.OfType<AuthorBid>().FirstOrDefault(i => i.Author == Name)
					??
					Chain.FindLastPoolOperation<AuthorBid>(o => o.Author == Name && o.Result == OperationResult.OK, 
																t => t.Successful, 
																p => !p.Round.Confirmed || p.Confirmed, 
																r => r.Id < executing.Id)
					??
					(LastBid != -1 ? Chain.FindRound(LastBid).FindOperation<AuthorBid>(i => i.Author == Name) : null);
		}

		public AuthorRegistration FindRegistration(Round executing)
		{
			return	executing.EffectiveOperations.OfType<AuthorRegistration>().FirstOrDefault(i => i.Author == Name)
					??
					Chain.FindLastPoolOperation<AuthorRegistration>(o => o.Author == Name && o.Result == OperationResult.OK, 
																	t => t.Successful, 
																	p => !p.Round.Confirmed || p.Confirmed, 
																	r => r.Id < executing.Id)
					??
					(LastRegistration != -1 ? Chain.FindRound(LastRegistration).FindOperation<AuthorRegistration>(i => i.Author == Name) : null);
		}

		public AuthorTransfer FindTransfer(Round executing)
		{
			return	executing.EffectiveOperations.OfType<AuthorTransfer>().FirstOrDefault(i => i.Author == Name)
					??
					Chain.FindLastPoolOperation<AuthorTransfer>(o => o.Author == Name && o.Result == OperationResult.OK, 
																t => t.Successful, 
																p => !p.Round.Confirmed || p.Confirmed, 
																r => r.Id < executing.Id)
					??
					(LastTransfer != -1 ? Chain.FindRound(LastTransfer).FindOperation<AuthorTransfer>(i => i.Author == Name) : null);
		}

		public Account FindOwner(Round executing)
		{
			var r = FindRegistration(executing);
			var t = FindTransfer(executing);
		
			return t == null ? r?.Signer : t?.To;
		}

		public bool IsOngoingAuction(Round executing)
		{
			ChainTime sinceauction() => executing.Time - Chain.FindRound(FirstBid).Time/* fb.Transaction.Payload.Round.Time*/;

			bool expired = (LastRegistration == -1 && sinceauction() > ChainTime.FromYears(2) ||																			/// winner has not registered during 2 year since auction start, restart the auction
							LastRegistration != -1 && executing.Time - Chain.FindRound(LastRegistration).Time > ChainTime.FromYears(FindRegistration(executing).Years));	/// winner has not renewed, restart the auction

 			if(!expired)
 			{
	 			return sinceauction() < ChainTime.FromYears(1);
 			} 
 			else
 			{
				return true;
			}
		}
	}
}
