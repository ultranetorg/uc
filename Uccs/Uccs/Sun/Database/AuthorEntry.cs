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

		Database			Chain;

		public AuthorEntry()
		{
		}

		public AuthorEntry(Database chain)
		{
			Chain = chain;
		}

		public static bool IsValid(string name) => name.Length >= NameLengthMin; 

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
