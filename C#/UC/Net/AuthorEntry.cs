using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RocksDbSharp;

namespace UC.Net
{

// 	public class Product : IBinarySerializable
// 	{
// 		public string			Name;
// 		public string			Title;
// 		public List<Release>	Releases;
// 		public int				LastRegistration = -1;
// 
// 		public Product(string name)
// 		{
// 			Name = name;
// 		}
// 
// 		public Product()
// 		{
// 		}
// 
// 		public Product Clone()
// 		{
// 			return	new Product(Name)
// 					{ 
// 						Name = Name,
// 						Title = Title, 
// 						Releases = Releases.Select(i => i.Clone()).ToList(),
// 						LastRegistration = LastRegistration,
// 					};
// 		}
// 
// 		public void Write(BinaryWriter w)
// 		{
// 			w.Write(Name);
// 			w.Write(Title);
// 			w.Write7BitEncodedInt(LastRegistration);
// 			w.Write(Releases);
// 		}
// 
// 		public void Read(BinaryReader r)
// 		{
// 			Name				= r.ReadUtf8();
// 			Title				= r.ReadUtf8();
// 			LastRegistration	= r.Read7BitEncodedInt();
// 			Releases			= r.ReadList<Release>();
// 		}
// 	}

	public class AuthorEntry : Entry<string>
	{
		public string				Name;
		public string				Title;
		//public int					FirstBid = -1;
		//public int					LastBid = -1;
		//public int					LastRegistration = -1;
		//public int					LastTransfer = -1;
		public List<string>			Products = new();
		public ChainTime			FirstBidTime;
		public Account				LastWinner;
		public Coin					LastBid;
		public ChainTime			LastBidTime;
		public Account				Owner;
		public ChainTime			RegistrationTime;
		public byte					Years;
		public int					Obtained;

		public override string		Key => Name;
		Roundchain					Chain;

		public const int			LengthMaxForAuction = 4;

		public static bool			IsExclusive(string name) => name.Length <= LengthMaxForAuction; 

		public AuthorEntry(Roundchain chain, string name)
		{
			Chain = chain;
			Name = name;
		}

		public AuthorEntry Clone()
		{
			return new AuthorEntry(Chain, Name)
					{
						Title = Title,
						FirstBidTime = FirstBidTime,
						LastWinner = LastWinner,
						LastBid = LastBid,
						LastBidTime = LastBidTime,
						Owner = Owner,
						RegistrationTime = RegistrationTime,
						Years = Years,
						Obtained = Obtained,
						Products = new List<string>(Products)
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Obtained);

			if(IsExclusive(Name))
			{
				w.Write(LastWinner != null);

				if(LastWinner != null)
				{
					w.Write(FirstBidTime);
					w.Write(LastWinner);
					w.Write(LastBid);
					w.Write(LastBidTime);
				}
			}

			w.Write(Owner != null);

			if(Owner != null)
			{
				w.Write(Owner);
				w.Write(Title);
				w.Write(RegistrationTime);
				w.Write(Years);
				w.Write(Products);
			}
		}

		public override void Read(BinaryReader r)
		{
			Obtained = r.Read7BitEncodedInt();

			if(IsExclusive(Name))
			{
				if(r.ReadBoolean())
				{
					FirstBidTime = r.ReadTime();
					LastWinner	 = r.ReadAccount();
					LastBidTime	 = r.ReadTime();
					LastBid		 = r.ReadCoin();
				}
			}

			if(r.ReadBoolean())
			{
				Owner			 = r.ReadAccount();
				Title			 = r.ReadString();
				RegistrationTime = r.ReadTime();
				Years			 = r.ReadByte();
				Products		 = r.ReadStings();
			}

		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			if(IsExclusive(Name))
			{
				if(LastWinner != null)
				{
					var p = d.Add("Auction");

					p.Add("FirstBidTime").Value = FirstBidTime;
					p.Add("LastWinner").Value = LastWinner;
					p.Add("LastBid").Value = LastBid;
					p.Add("LastBidTime").Value = LastBidTime;
				}
			}

			if(Owner != null)
			{
				var p = d.Add("Registration");

				p.Add("Owner").Value = Owner;
				p.Add("Title").Value = Title;
				p.Add("RegistrationTime").Value = RegistrationTime;
				p.Add("Years").Value = Years;
				p.Add("Products").Value = string.Join(", ", Products);
			}

			return d;
		}

// 		public AuthorBid FindFirstBid(Round executing)
// 		{
// 			if(FirstBid != -1)
// 			{
// 				foreach(var b in Chain.FindRound(FirstBid).Payloads.AsEnumerable().Reverse())
// 					foreach(var t in b.SuccessfulTransactions.AsEnumerable().Reverse())
// 						foreach(var o in t.SuccessfulOperations.OfType<AuthorBid>().Reverse())
// 							if(o.Author == Name)
// 								return o;
// 
// 				throw new IntegrityException("AuthorBid operation not found");
// 			}
// 
// 			foreach(var r in Chain.Rounds.Where(i => i.Id < executing.Id).Reverse())
// 				foreach(var b in (r.Confirmed ? r.ConfirmedPayloads : r.Payloads).AsEnumerable().Reverse())
// 					foreach(var t in b.SuccessfulTransactions.AsEnumerable().Reverse())
// 						foreach(var o in t.SuccessfulOperations.AsEnumerable().Reverse())
// 							if(o is AuthorBid ab && ab.Author == Name)
// 								return ab;
// 
// 			return executing.ExecutedOperations.Reverse().OfType<AuthorBid>().FirstOrDefault(i => i.Author == Name);
// 		}
// 
// 		public AuthorBid FindLastBid(Round executing)
// 		{
// 			return	executing.ExecutedOperations.OfType<AuthorBid>().FirstOrDefault(i => i.Author == Name)
// 					??
// 					Chain.FindLastPoolOperation<AuthorBid>(o => o.Author == Name && o.Successful, 
// 																null, 
// 																p => !p.Round.Confirmed || p.Confirmed, 
// 																r => r.Id < executing.Id)
// 					??
// 					(LastBid != -1 ? Chain.FindRound(LastBid).FindOperation<AuthorBid>(i => i.Author == Name) : null);
// 		}
// 
// 		public AuthorRegistration FindRegistration(Round executing)
// 		{
// 			return	executing.ExecutedOperations.OfType<AuthorRegistration>().FirstOrDefault(i => i.Author == Name)
// 					??
// 					Chain.FindLastPoolOperation<AuthorRegistration>(o => o.Author == Name && o.Successful, 
// 																	null, 
// 																	p => !p.Round.Confirmed || p.Confirmed, 
// 																	r => r.Id < executing.Id)
// 					??
// 					(LastRegistration != -1 ? Chain.FindRound(LastRegistration).FindOperation<AuthorRegistration>(i => i.Author == Name) : null);
// 		}

// 		public AuthorTransfer FindTransfer(Round executing)
// 		{
// 			return	executing.ExecutedOperations.OfType<AuthorTransfer>().FirstOrDefault(i => i.Author == Name)
// 					??
// 					Chain.FindLastPoolOperation<AuthorTransfer>(o => o.Author == Name && o.Successful, 
// 																null, 
// 																p => !p.Round.Confirmed || p.Confirmed, 
// 																r => r.Id < executing.Id)
// 					??
// 					(LastTransfer != -1 ? Chain.FindRound(LastTransfer).FindOperation<AuthorTransfer>(i => i.Author == Name) : null);
// 		}

// 		public Account FindOwner(Round executing)
// 		{
// 			var r = FindRegistration(executing);
// 			var t = FindTransfer(executing);
// 		
// 			return t == null ? r?.Signer : t?.To;
// 		}

		public bool IsOngoingAuction(Round executing)
		{
			ChainTime sinceauction() => executing.Time - FirstBidTime/* fb.Transaction.Payload.Round.Time*/;

			bool expired = (Owner == null && sinceauction() > ChainTime.FromYears(2) ||																			/// winner has not registered during 2 year since auction start, restart the auction
							Owner != null && executing.Time - RegistrationTime > ChainTime.FromYears(Years));	/// winner has not renewed, restart the auction

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
