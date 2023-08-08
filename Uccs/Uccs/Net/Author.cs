using System;
using System.Collections.Generic;
using System.IO;

namespace Uccs.Net
{
	public class Branch
	{
		public string				Path { get; set; }
		public AccountAddress[]		Publishers { get; set; }
		public ChainTime			Expiration;
	}

	public class Author : IBinarySerializable
	{
		public const int				ExclusiveLengthMax = 4;
		public const int				NameLengthMin = 2;

		public string					Name;
		public string					Title;
		public AccountAddress			Owner;
		public byte						Years;
		public ChainTime				RegistrationTime;
		public ChainTime				FirstBidTime = ChainTime.Empty;
		public AccountAddress			LastWinner;
		public Coin						LastBid;
		public ChainTime				LastBidTime;
		public Resource[]				Resources = {};
		public int						NextResourceId;

		public static bool				IsExclusive(string name) => name.Length <= ExclusiveLengthMax; 

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
					FirstBidTime = reader.ReadTime();
					LastWinner	 = reader.ReadAccount();
					LastBidTime	 = reader.ReadTime();
					LastBid		 = reader.ReadCoin();
				}
			}

			if(reader.ReadBoolean())
			{
				Owner				= reader.ReadAccount();
				RegistrationTime	= reader.ReadTime();
				Title				= reader.ReadUtf8();
				Years				= reader.ReadByte();
			}
		}
	}
}
