﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Nethereum.Signer;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public class Transaction : IBinarySerializable
	{
		public int						Id;
		public List<Operation>			Operations = new ();
		//public IEnumerable<Operation>	SuccessfulOperations => Operations.Where(i => i.Error == null);
		public bool						Successful => Operations.Any() && Operations.All(i => i.Error == null);
		
		public Vote						Vote;
		public AccountAddress			Generator;
		public int						Expiration;
		public byte[]					PoW;
		public byte[]					Signature;
		
		public AccountAddress			Signer;
		public Zone						Zone;
		public PlacingStage				Placing;

		const int						PoWLength = 16;

		public bool Valid(Mcv mcv)
		{
			return	Operations.Any() && Operations.All(i => i.Valid) &&
					(!Zone.PoW || Zone.PoW && Zone.Cryptography.Hash(mcv.FindRound(Expiration - Mcv.Pitch * 2).Hash.Concat(PoW).ToArray()).Take(2).All(i => i == 0));
		}

 		public Transaction(Zone zone)
 		{
 			Zone = zone;
 		}

		public override string ToString()
		{
			return $"Id={Id}, {Placing}, Operations={{{Operations.Count}}}, Signer={Signer}, Generator={Generator}, Expiration={Expiration}, Signature={Hex.ToHexString(Signature)}";
		}

		public void Sign(AccountKey signer, AccountAddress generator, int expiration, byte[] powhash)
		{
			Signer		= signer;
			Generator	= generator;
			Expiration	= expiration;

            if(powhash.SequenceEqual(Zone.Cryptography.ZeroHash) || !Zone.PoW)
			{
				PoW = new byte[PoWLength];
			}
			else
            {
	            var r = new Random();
				var h = new byte[32];
	
				var x = new byte[32+PoWLength];
	
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
			Operations.Insert(0, operation);
			operation.Transaction = this;
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.WriteUtf8(Zone.Name); 
			w.Write7BitEncodedInt(Id);
			w.Write(Generator);
			w.Write7BitEncodedInt(Expiration);
			w.WriteBytes(PoW);
			w.Write(Operations, i => i.Write(w));

			return Zone.Cryptography.Hash(s.ToArray());
		}

 		public void	WriteConfirmed(BinaryWriter writer)
 		{
			writer.Write(Signer);
			writer.Write7BitEncodedInt(Id);
			writer.Write7BitEncodedInt(Expiration);
			writer.Write(PoW);
			writer.Write(Operations, i =>{
											writer.Write((byte)i.Class); 
											i.Write(writer); 
										 });
 		}
 		
 		public void	ReadConfirmed(BinaryReader reader)
 		{
			Signer		= reader.ReadAccount();
			Id			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
			PoW			= reader.ReadBytes(PoWLength);
 			Operations	= reader.ReadList(() => {
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
			writer.Write(Signature);
			writer.Write7BitEncodedInt(Id);
			writer.Write7BitEncodedInt(Expiration);
			writer.Write(PoW);
			writer.Write(Operations, i => {
											writer.Write((byte)i.Class); 
											i.Write(writer); 
										  });
 		}
 		
 		public void	ReadForVote(BinaryReader reader)
 		{
			Signature	= reader.ReadSignature();
			Id			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
			PoW			= reader.ReadBytes(PoWLength);
 			Operations	= reader.ReadList(() => {
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
			writer.Write(Generator);
			writer.Write(Signature);
			writer.Write7BitEncodedInt(Id);
			writer.Write7BitEncodedInt(Expiration);
			writer.Write(PoW);
			writer.Write(Operations, i =>	{
												writer.Write((byte)i.Class); 
												i.Write(writer); 
											});
		}

		public void Read(BinaryReader reader)
		{
			Generator	= reader.ReadAccount();
			Signature	= reader.ReadSignature();
			Id			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
			PoW			= reader.ReadBytes(PoWLength);
			Operations	= reader.ReadList(() => {
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