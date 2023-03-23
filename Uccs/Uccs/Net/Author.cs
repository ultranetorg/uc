using System;
using System.IO;

namespace UC.Net
{
	public class Author : IBinarySerializable
	{
		public const int			ExclusiveLengthMax = 4;
		public const int			NameLengthMin = 2;

		public static bool IsExclusive(string name) => name.Length <= ExclusiveLengthMax; 

		public string				Name { get; set; }
		public string				Title { get; set; }
		public AccountAddress		Owner { get; set; }
		public byte					Years { get; set; }
		public ChainTime			RegistrationTime { get; set; } = ChainTime.Empty;
		public ChainTime			FirstBidTime { get; set; } = ChainTime.Empty;
		public AccountAddress		LastWinner { get; set; }
		public Coin					LastBid { get; set; }
		public ChainTime			LastBidTime { get; set; }

		public void Write(BinaryWriter w)
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

		public void Read(BinaryReader r)
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
	}
}
