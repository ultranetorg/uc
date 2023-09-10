using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class Author : IBinarySerializable
	{
		public const int					ExclusiveLengthMax = 12;
		public const int					LengthMin = 1;

		public static readonly ChainTime	AuctionMinimalDuration = ChainTime.FromDays(365);
		public static readonly ChainTime	Prolongation = ChainTime.FromDays(30);
		public static readonly ChainTime	WinnerRegistrationPeriod = ChainTime.FromDays(30);
		public static readonly ChainTime	RenewaPeriod = ChainTime.FromDays(365);
		public ChainTime					AuctionEnd => ChainTime.Max(FirstBidTime + AuctionMinimalDuration, LastBidTime + Prolongation);

		public string						Name { get; set; }
		public string						Title { get; set; }
		public AccountAddress				Owner { get; set; }
		public ChainTime					Expiration { get; set; }
		public ChainTime					FirstBidTime { get; set; } = ChainTime.Empty;
		public AccountAddress				LastWinner { get; set; }
		public Coin							LastBid { get; set; }
		public ChainTime					LastBidTime { get; set; }
		public bool							DomainOwnersOnly  { get; set; }
		public Resource[]					Resources { get; set; } = {};
		public int							NextResourceId { get; set; }


		public static bool IsExclusive(string name) => name.Length <= ExclusiveLengthMax; 

		public static bool IsExpired(Author a, ChainTime time) 
		{
			return	a.LastWinner != null && a.Owner == null && time > a.AuctionEnd + WinnerRegistrationPeriod ||  /// winner has not registered since the end of auction, restart the auction
					a.Owner != null && time > a.Expiration;	 /// owner has not renewed, restart the auction
		}

		public static bool CanRegister(string name, Author author, ChainTime time, AccountAddress by)
		{
			return	author == null && !IsExclusive(name) || /// available
					author != null && !IsExclusive(name) && author.Owner != null && time > author.Expiration || /// not renewed by current owner
					author != null && IsExclusive(name) && author.Owner == null && author.LastWinner == by &&	
						time > author.FirstBidTime + AuctionMinimalDuration && /// auction lasts minimum specified period
						time > author.LastBidTime + Prolongation && /// wait until prolongation is over
						time < author.AuctionEnd + WinnerRegistrationPeriod || /// auction is over and a winner can register an author during special period
					author != null && author.Owner == by &&	time > author.Expiration - RenewaPeriod && /// renewal by owner: renewal is allowed during last year olny
															time <= author.Expiration
				  ;
		}

		public static bool CanBid(string name, Author author, ChainTime time)
		{
 			if(!IsExpired(author, time))
 			{
				if(author.LastWinner == null) /// first bid
				{
					return true;
				}
				//else if(time - author.FirstBidTime < AuctionMinimalDuration || time - author.LastBidTime < Prolongation)
				else if(time < author.AuctionEnd)
				{
					return true;
				}
 			} 
 			else
 			{
				return true;
			}

			return false;
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.Write7BitEncodedInt(NextResourceId);
			w.Write(Resources, i =>	{
										w.WriteUtf8(i.Address.Resource);
										i.Write(w);
									});

			if(IsExclusive(Name))
			{
				w.Write(LastWinner != null);

				if(LastWinner != null)
				{
					w.Write(FirstBidTime);
					w.Write(LastWinner);
					w.Write(LastBidTime);
					w.Write(LastBid);
					w.Write(DomainOwnersOnly);
				}
			}

			w.Write(Owner != null);

			if(Owner != null)
			{
				w.Write(Owner);
				w.Write(Expiration);
				w.WriteUtf8(Title);
			}

		}

		public void Read(BinaryReader reader)
		{
			Name			= reader.ReadUtf8();
			NextResourceId	= reader.Read7BitEncodedInt();
			Resources		= reader.ReadArray(() => { 
														var a = new Resource();
														a.Address = new ResourceAddress(Name, reader.ReadUtf8());
														a.Read(reader);
														return a;
													});

			if(IsExclusive(Name))
			{
				if(reader.ReadBoolean())
				{
					FirstBidTime		= reader.ReadTime();
					LastWinner			= reader.ReadAccount();
					LastBidTime			= reader.ReadTime();
					LastBid				= reader.ReadCoin();
					DomainOwnersOnly	= reader.ReadBoolean();
				}
			}

			if(reader.ReadBoolean())
			{
				Owner				= reader.ReadAccount();
				Expiration			= reader.ReadTime();
				Title				= reader.ReadUtf8();
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
 				p.Add("Expiration").Value = Expiration;
 			}
 
 			return d;
 		}

	}
}
