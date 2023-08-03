using System;
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
		
		public Vote						Block;
		public AccountAddress			Generator;
		public int						Expiration;
		public byte[]					Signature;
		
		public AccountAddress			Signer;
		public Zone						Zone;
		public PlacingStage				Placing;

		public bool Valid
		{
			get
			{
				return Operations.Any() && Operations.All(i => i.Valid);
			}
		}

 		public Transaction(Zone zone)
 		{
 			Zone = zone;
 		}

		public override string ToString()
		{
			return $"Id={Id}, {Placing}, Operations={{{Operations.Count}}}, Signer={Signer}, Generator={Generator}, Expiration={Expiration}, Signature={Hex.ToHexString(Signature)}";
		}

		public void Sign(AccountKey signer, AccountAddress generator, int expiration)
		{
			Signer		= signer;
			Generator	= generator;
			Expiration	= expiration;
			Signature	= Zone.Cryptography.Sign(signer, Hashify());
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
			w.Write(Operations, i => i.Write(w));

			return Zone.Cryptography.Hash(s.ToArray());
		}

 		public void	WriteConfirmed(BinaryWriter w)
 		{
			w.Write(Signer);
			w.Write7BitEncodedInt(Id);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
 		}
 		
 		public void	ReadConfirmed(BinaryReader reader)
 		{
			Signer		= reader.ReadAccount();
			Id			= reader.Read7BitEncodedInt();
 			Operations	= reader.ReadList(() => {
 												var o = Operation.FromType((Operations)reader.ReadByte());
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
			writer.Write(Operations, i => {
										writer.Write((byte)i.Type); 
										i.Write(writer); 
									 });
 		}
 		
 		public void	ReadForVote(BinaryReader reader)
 		{
			Signature	= reader.ReadSignature();
			Id			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
 			Operations	= reader.ReadList(() => {
 													var o = Operation.FromType((Operations)reader.ReadByte());
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
			writer.Write(Signature);
			writer.Write7BitEncodedInt(Id);
			writer.Write(Generator);
			writer.Write7BitEncodedInt(Expiration);
			writer.Write(Operations, i =>	{
												writer.Write((byte)i.Type); 
												i.Write(writer); 
											});
		}

		public void Read(BinaryReader reader)
		{
			Signature	= reader.ReadSignature();
			Id			= reader.Read7BitEncodedInt();
			Generator	= reader.ReadAccount();
			Expiration	= reader.Read7BitEncodedInt();
			Operations	= reader.ReadList(() => {
													var o = Operation.FromType((Operations)reader.ReadByte());
													o.Transaction = this;
													o.Read(reader); 
													return o; 
												});

			Signer = Zone.Cryptography.AccountFrom(Signature, Hashify());

			foreach(var i in Operations)
				i.Signer = Signer;
		}

// 		public void WriteUnconfirmed(BinaryWriter w)
// 		{
// 			w.Write(Signature);
// 			w.Write7BitEncodedInt(RoundMax);
// 			w.Write(Operations, i => {
// 										w.Write((byte)i.Type); 
// 										i.Write(w); 
// 									 });
// 		}
// 
// 		public void ReadUnconfirmed(BinaryReader r)
// 		{
// 			Signature	= r.ReadSignature();
// 			RoundMax	= r.Read7BitEncodedInt();
// 			Operations	= r.ReadList(() => {
// 												var o = Operation.FromType((Operations)r.ReadByte());
// 												o.Transaction = this;
// 												o.Read(r); 
// 												return o; 
// 											});
// 
// 			Signer = Zone.Cryptography.AccountFrom(Signature, Hashify());
// 
// 			foreach(var i in Operations)
// 				i.Signer = Signer;
// 		}
	}
}
