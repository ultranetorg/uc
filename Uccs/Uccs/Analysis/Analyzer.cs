using System;
using System.IO;

namespace Uccs.Net
{
	public class Analyzer
	{
		public const int				IdMax = 255;

		public AccountAddress			Account;
		public byte						Id;
		public int						JoinedAt;
	
  		public void WriteBaseState(BinaryWriter w)
 		{
 			w.Write(Account);
			w.Write(Id); /// negative if inactive
			w.Write7BitEncodedInt(JoinedAt); /// negative if inactive
 		}
 
 		public void ReadBaseState(BinaryReader r)
 		{
			Account		= r.ReadAccount();
 			Id			= r.ReadByte();
 			JoinedAt	= r.Read7BitEncodedInt();
		}

		public override string ToString()
		{
			return $"{Id}, Account={Account}, JoinedAt={JoinedAt}";
		}
	}
}
