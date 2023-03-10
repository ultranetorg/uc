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

		public string				Name { get; set; }
		public string				Title { get; set; }
		public Account				Owner { get; set; }
		public byte					Years { get; set; }
		public ChainTime			RegistrationTime { get; set; } = ChainTime.Empty;
		public ChainTime			FirstBidTime { get; set; } = ChainTime.Empty;
		public Account				LastWinner { get; set; }
		public Coin					LastBid { get; set; }
		public ChainTime			LastBidTime { get; set; }

		Database					Chain;

		public const int			ExclusiveLengthMax = 4;
		public const int			NameLengthMin = 2;

		public AuthorEntry()
		{
		}

		public AuthorEntry(Database chain)
		{
			Chain = chain;
		}

		public static bool IsValid(string name) => name.Length >= NameLengthMin; 
		public static bool IsExclusive(string name) => name.Length <= ExclusiveLengthMax; 

		public AuthorEntry Clone()
		{
			return new AuthorEntry(Chain)
					{
						Name = Name,
						Title = Title,
						Owner = Owner,
						Years = Years,
						RegistrationTime = RegistrationTime,
						FirstBidTime = FirstBidTime,
						LastWinner = LastWinner,
						LastBid = LastBid,
						LastBidTime = LastBidTime
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.WriteUtf8(Name);

			if(IsExclusive(Name))
			{
				w.Write(FirstBidTime);

				if(FirstBidTime != ChainTime.Empty)
				{
					w.Write(LastWinner);
					w.Write(LastBidTime);
					w.Write(LastBid);
				}
			}

			w.Write(RegistrationTime);

			if(RegistrationTime != ChainTime.Empty)
			{
				w.Write(Owner);
				w.WriteUtf8(Title);
				w.Write(Years);
			}
		}

		public override void Read(BinaryReader r)
		{
			Name = r.ReadUtf8();

			if(IsExclusive(Name))
			{
				FirstBidTime = r.ReadTime();

				if(FirstBidTime != ChainTime.Empty)
				{
					LastWinner	 = r.ReadAccount();
					LastBidTime	 = r.ReadTime();
					LastBid		 = r.ReadCoin();
				}
			}

			RegistrationTime = r.ReadTime();

			if(RegistrationTime != ChainTime.Empty)
			{
				Owner	= r.ReadAccount();
				Title	= r.ReadUtf8();
				Years	= r.ReadByte();
			}
		}

		public override void WriteMore(BinaryWriter w)
		{
			//w.Write7BitEncodedInt(ObtainedRid);

			//if(RegistrationTime != ChainTime.Zero)
			//{
			//	w.Write(Products);
			//}
		}

		public override void ReadMore(BinaryReader r)
		{
			//ObtainedRid = r.Read7BitEncodedInt();

			//if(RegistrationTime != ChainTime.Zero)
			//{
			//	Products = r.ReadStings();
			//}
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
			}

			return d;
		}

		public bool IsOngoingAuction(Round executing)
		{
			var sinceauction = executing.Time - FirstBidTime/* fb.Transaction.Payload.Round.Time*/;

			bool expired = (Owner == null && sinceauction > ChainTime.FromYears(2) ||																			/// winner has not registered during 2 year since auction start, restart the auction
							Owner != null && executing.Time - RegistrationTime > ChainTime.FromYears(Years));	/// winner has not renewed, restart the auction

 			if(!expired)
 			{
	 			return sinceauction < ChainTime.FromYears(1);
 			} 
 			else
 			{
				return true;
			}
		}
	}
}
