using System;
using System.IO;

namespace Uccs.Net
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
		public ChainTime			RegistrationTime { get; set; }
		public ChainTime			FirstBidTime { get; set; } = ChainTime.Empty;
		public AccountAddress		LastWinner { get; set; }
		public Coin					LastBid { get; set; }
		public ChainTime			LastBidTime { get; set; }

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Name);

			if(IsExclusive(Name))
			{
				w.Write(LastWinner != null);

				if(LastWinner != null)
				{
					w.Write(FirstBidTime);
					w.Write(LastWinner);
					w.Write(LastBidTime);
					w.Write(LastBid);
				}
			}

			w.Write(Owner != null);

			if(Owner != null)
			{
				w.Write(Owner);
				w.Write(RegistrationTime);
				w.WriteUtf8(Title);
				w.Write(Years);
			}
		}

		public void Read(BinaryReader r)
		{
			Name = r.ReadUtf8();

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
				Owner				= r.ReadAccount();
				RegistrationTime	= r.ReadTime();
				Title				= r.ReadUtf8();
				Years				= r.ReadByte();
			}
		}
	}
}
