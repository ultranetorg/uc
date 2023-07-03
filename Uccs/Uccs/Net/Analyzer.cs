using System;
using System.IO;

namespace Uccs.Net
{
	public class Analyzer
	{
		public AccountAddress			Account;
		public int						JoinedAt;
		//public int						LeftAt;
	
  		public void WriteConfirmed(BinaryWriter w)
 		{
 			w.Write(Account);
 		}
 
 		public void ReadConfirmed(BinaryReader r)
 		{
			Account	= r.ReadAccount();
		}
	
  		public void WriteForBase(BinaryWriter w)
 		{
 			w.Write(Account);
			w.Write7BitEncodedInt(JoinedAt); /// negative if inactive
			//w.Write7BitEncodedInt(LeftAt); /// negative if inactive
 		}
 
 		public void ReadForBase(BinaryReader r)
 		{
			Account		= r.ReadAccount();
 			JoinedAt	= r.Read7BitEncodedInt();
 			//LeftAt		= r.Read7BitEncodedInt();
		}

		public override string ToString()
		{
			return $"Account={Account}, JoinedAt={JoinedAt}";
		}
	}
}
