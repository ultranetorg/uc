using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public enum PlacingStage
	{
		None, 
		Pending,
		Accepted, Placed, FailedOrNotFound, Confirmed
	}

	public class Transaction : IBinarySerializable
	{
		public int						Nid;
		public TransactionId			Id => new (Round.Id, Array.IndexOf(Round.ConfirmedTransactions, this));
		public Operation[]				Operations = {};
		public bool						Successful => Operations.Any() && Operations.All(i => i.Error == null);
		
		public Vote						Vote;
		public Round					Round;
		public AccountAddress			Member;
		public int						Expiration;
		public byte[]					PoW;
		public Money					Fee;
		public byte[]					Signature;
				
		public AccountAddress			Signer;
		public Zone						Zone;
		public PlacingStage				Placing;
		public RdcInterface				Rdc;

		const int						PoWLength = 16;

		public PlacingStage				__ExpectedPlacing = PlacingStage.None;

		public bool Valid(Mcv mcv)
		{
			return	Operations.Any() && Operations.Length <= mcv.Zone.OperationsPerTransactionLimit && Operations.All(i => i.Valid) &&
					(!mcv.Zone.PoW || mcv.Zone.Cryptography.Hash(mcv.FindRound(Expiration - Mcv.Pitch * 2).Hash.Concat(PoW).ToArray()).Take(2).All(i => i == 0));
		}

 		public Transaction(Zone zone)
 		{
 			Zone = zone;
 		}

		public override string ToString()
		{
			return $"Nid={Nid}, {Placing}, Operations={{{Operations.Length}}}, Signer={Signer?.Bytes.ToHexPrefix()}, Member={Member?.Bytes.ToHexPrefix()}, Expiration={Expiration}, Signature={Signature?.ToHexPrefix()}";
		}

		public void Sign(AccountKey signer, byte[] powhash)
		{
			Signer = signer;

            if(powhash.SequenceEqual(Zone.Cryptography.ZeroHash) || !Zone.PoW)
			{
				PoW = new byte[PoWLength];
			}
			else
            {
	            var r = new Random();
				var h = new byte[32];
	
				var x = new byte[32 + PoWLength];
	
				Array.Copy(powhash, x, 32);
	
				do
				{
					r.NextBytes(new Span<byte>(x, 32, PoWLength));
					
					h = Zone.Cryptography.Hash(x);
				
				}
				while(h[0] != 0 || h[1] != 0);
				
				PoW = x.Skip(32).ToArray();
            }

			Signature = Zone.Cryptography.Sign(signer, Hashify());
		}

		public bool EqualBySignature(Transaction t)
		{
			return Signature != null && t.Signature != null && Signature.SequenceEqual(t.Signature);
		}

		public void AddOperation(Operation operation)
		{ 
			Operations = Operations.Prepend(operation).ToArray();
			operation.Transaction = this;
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.WriteUtf8(Zone.Name); 
			w.Write7BitEncodedInt(Nid);
			w.Write(Member);
			w.Write7BitEncodedInt(Expiration);
			w.Write(Fee);
			w.WriteBytes(PoW);
			w.Write(Operations, i => i.Write(w));

			return Zone.Cryptography.Hash(s.ToArray());
		}

 		public void	WriteConfirmed(BinaryWriter writer)
 		{
			writer.Write(Signer);
			writer.Write7BitEncodedInt(Nid);
			writer.Write(Fee);
			writer.Write(Operations, i =>{
											writer.Write((byte)i.Class); 
											i.Write(writer); 
										 });
 		}
 		
 		public void	ReadConfirmed(BinaryReader reader)
 		{
			Placing		= PlacingStage.Confirmed;

			Signer		= reader.ReadAccount();
			Nid			= reader.Read7BitEncodedInt();
			Fee			= reader.ReadMoney();
 			Operations	= reader.ReadArray(() => {
 													var o = Operation.FromType((OperationClass)reader.ReadByte());
 													o.Transaction = this;
 													o.Read(reader); 
 													return o; 
 												});
			
			foreach(var i in Operations)
				i.Signer = Signer;
 		}

 		public void	WriteForVote(BinaryWriter writer)
 		{
			writer.Write((byte)__ExpectedPlacing);

			writer.Write(Signature);
			writer.Write7BitEncodedInt(Nid);
			writer.Write7BitEncodedInt(Expiration);
			writer.Write(Fee);
			writer.Write(PoW);
			writer.Write(Operations, i => {
											writer.Write((byte)i.Class); 
											i.Write(writer); 
										  });
 		}
 		
 		public void	ReadForVote(BinaryReader reader)
 		{
			__ExpectedPlacing = (PlacingStage)reader.ReadByte();

			Signature	= reader.ReadSignature();
			Nid			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
			Fee			= reader.ReadMoney();
			PoW			= reader.ReadBytes(PoWLength);
 			Operations	= reader.ReadArray(() => {
 													var o = Operation.FromType((OperationClass)reader.ReadByte());
 													//o.Placing		= PlacingStage.Confirmed;
 													o.Transaction	= this;
 													o.Read(reader); 
 													return o; 
 												});
			
			Signer = Zone.Cryptography.AccountFrom(Signature, Hashify());

			foreach(var i in Operations)
				i.Signer = Signer;
 		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)__ExpectedPlacing);
		
			writer.Write(Member);
			writer.Write(Signature);
			writer.Write7BitEncodedInt(Nid);
			writer.Write7BitEncodedInt(Expiration);
			writer.Write(Fee);
			writer.Write(PoW);
			writer.Write(Operations, i =>	{
												writer.Write((byte)i.Class); 
												i.Write(writer); 
											});
		}

		public void Read(BinaryReader reader)
		{
			__ExpectedPlacing = (PlacingStage)reader.ReadByte();
		
			Member	= reader.ReadAccount();
			Signature	= reader.ReadSignature();
			Nid			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
			Fee			= reader.ReadMoney();
			PoW			= reader.ReadBytes(PoWLength);
			Operations	= reader.ReadArray(() => {
													var o = Operation.FromType((OperationClass)reader.ReadByte());
													o.Transaction = this;
													o.Read(reader); 
													return o; 
												});

			Signer = Zone.Cryptography.AccountFrom(Signature, Hashify());

			foreach(var i in Operations)
				i.Signer = Signer;
		}
	}
}
