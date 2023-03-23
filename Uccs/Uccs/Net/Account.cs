using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UC.Net;

namespace UC.Net
{
	public class Account : IBinarySerializable
	{
		public AccountAddress			Address;
		public int						LastOperationId = -1;
		public Coin						Balance;
		public int						LastEmissionId = -1;
		public int						CandidacyDeclarationRid = -1;
		public Coin						Bail;
		public BailStatus				BailStatus;

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write7BitEncodedInt(LastOperationId);
			w.Write7BitEncodedInt(LastEmissionId);
			w.Write(Balance);
			w.Write7BitEncodedInt(CandidacyDeclarationRid);

			if(CandidacyDeclarationRid != -1)
			{
				w.Write(Bail);
				w.Write((byte)BailStatus);
			}
		}

		public void Read(BinaryReader r)
		{
			Address						= r.ReadAccount();
			LastOperationId				= r.Read7BitEncodedInt();
			LastEmissionId				= r.Read7BitEncodedInt();
			Balance						= r.ReadCoin();
			CandidacyDeclarationRid		= r.Read7BitEncodedInt();

			if(CandidacyDeclarationRid != -1)
			{
				Bail		= r.ReadCoin();
				BailStatus	= (BailStatus)r.ReadByte();
			}
		}
	}
}
