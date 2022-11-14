using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Nethereum.Model;
using RocksDbSharp;

namespace UC.Net
{
	public class AuthorEntry : TableEntry<string>
	{
		public override string		Key => Name;
		public override byte[]		ClusterKey => Encoding.UTF8.GetBytes(Name).Take(ClusterKeyLength).ToArray();

		public string				Name;
		public string				Title;
		public Account				Owner;
		public byte					Years;
		public ChainTime			RegistrationTime;
		public ChainTime			FirstBidTime;
		public Account				LastWinner;
		public Coin					LastBid;
		public ChainTime			LastBidTime;

		public int					ObtainedRid;
		public List<string>			Products = new();

		Roundchain					Chain;

		public const int			LengthMaxForAuction = 4;

		public AuthorEntry(Roundchain chain)
		{
			Chain = chain;
		}

		public static bool IsExclusive(string name) => name.Length <= LengthMaxForAuction; 

		public AuthorEntry Clone()
		{
			return new AuthorEntry(Chain)
					{
						Name = Name,
						Title = Title,
						Owner = Owner,
						FirstBidTime = FirstBidTime,
						LastWinner = LastWinner,
						LastBid = LastBid,
						LastBidTime = LastBidTime,
						RegistrationTime = RegistrationTime,
						Years = Years,
						ObtainedRid = ObtainedRid,
						Products = new List<string>(Products)
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.WriteUtf8(Name);

			if(IsExclusive(Name))
			{
				w.Write(FirstBidTime);

				if(FirstBidTime != ChainTime.Zero)
				{
					w.Write(LastWinner);
					w.Write(LastBidTime);
					w.Write(LastBid);
				}
			}

			w.Write(RegistrationTime);

			if(RegistrationTime != ChainTime.Zero)
			{
				w.Write(Owner);
				w.Write(Title);
				w.Write(Years);
			}
		}

		public override void WriteMore(BinaryWriter w)
		{
			w.Write7BitEncodedInt(ObtainedRid);

			if(RegistrationTime != ChainTime.Zero)
			{
				w.Write(Products);
			}
		}

		public override void Read(BinaryReader r)
		{
			Name = r.ReadUtf8();

			if(IsExclusive(Name))
			{
				FirstBidTime = r.ReadTime();

				if(FirstBidTime != ChainTime.Zero)
				{
					LastWinner	 = r.ReadAccount();
					LastBidTime	 = r.ReadTime();
					LastBid		 = r.ReadCoin();
				}
			}

			RegistrationTime = r.ReadTime();

			if(RegistrationTime != ChainTime.Zero)
			{
				Owner	= r.ReadAccount();
				Title	= r.ReadString();
				Years	= r.ReadByte();
			}
		}

		public override void ReadMore(BinaryReader r)
		{
			ObtainedRid = r.Read7BitEncodedInt();

			if(RegistrationTime != ChainTime.Zero)
			{
				Products = r.ReadStings();
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
