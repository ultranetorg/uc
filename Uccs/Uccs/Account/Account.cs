using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Net
{
	public class Account : IBinarySerializable
	{
		public AccountAddress			Address;
		public Coin						Balance;
		public Coin						Bail;
		public BailStatus				BailStatus;

		public virtual void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(Balance);
			w.Write((byte)BailStatus);
			
			if(BailStatus != BailStatus.Null)
			{
				w.Write(Bail);
			}
		}

		public virtual void Read(BinaryReader r)
		{
			Address		= r.ReadAccount();
			Balance		= r.ReadCoin();
			BailStatus	= (BailStatus)r.ReadByte();

			if(BailStatus != BailStatus.Null)
			{
				Bail = r.ReadCoin();
			}
		}
	}
}
