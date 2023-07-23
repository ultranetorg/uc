using System;
using System.Collections.Generic;
using System.IO;

namespace Uccs.Net
{
	public class ResourceDirectory
	{
		public string				Path { get; set; }
		public AccountAddress[]		Publishers { get; set; }
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
		public List<ResourceDirectory>	Directories;

		public static bool				IsExclusive(string name) => name.Length <= ExclusiveLengthMax; 

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

			w.Write(Directories, i => {	w.WriteUtf8(i.Path);
										w.Write(i.Publishers); });
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

			Directories = r.ReadList<ResourceDirectory>(() => new ResourceDirectory {	Path = r.ReadUtf8(), 
																						Publishers = r.ReadArray<AccountAddress>() });
		}
	}
}
