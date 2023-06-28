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
		public List<Operation>			Operations = new ();
		public IEnumerable<Operation>	SuccessfulOperations => Operations.Where(i => i.Error == null);
		
		public Vote						Block;
		public AccountAddress			Generator;
		public int						Expiration;
		public byte[]					Signature;
		
		public AccountAddress			Signer;
		public Zone						Zone;

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
			return $"Operations={{{Operations.Count}}}, Signer={Signer}, Generator={Generator}, Expiration={Expiration}, Signature={Hex.ToHexString(Signature)}";
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
			w.Write(Generator);
			w.Write7BitEncodedInt(Expiration);
			w.Write(Operations, i => i.Write(w));

			return Zone.Cryptography.Hash(s.ToArray());
		}

 		public void	WriteAsPartOfBlock(BinaryWriter w)
 		{
			w.Write(Signature);
			w.Write7BitEncodedInt(Expiration);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
 		}
 		
 		public void	ReadAsPartOfBlock(BinaryReader r)
 		{
			Signature	= r.ReadSignature();
			Expiration	= r.Read7BitEncodedInt();
 			Operations	= r.ReadList(() => {
 												var o = Operation.FromType((Operations)r.ReadByte());
 												//o.Placing		= PlacingStage.Confirmed;
 												o.Transaction	= this;
 												o.Read(r); 
 												return o; 
 											});
			
			Signer = Zone.Cryptography.AccountFrom(Signature, Hashify());

			foreach(var i in Operations)
				i.Signer = Signer;
 		}

		public void Write(BinaryWriter w)
		{
			w.Write(Signature);
			w.Write(Generator);
			w.Write7BitEncodedInt(Expiration);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
		}

		public void Read(BinaryReader r)
		{
			Signature	= r.ReadSignature();
			Generator	= r.ReadAccount();
			Expiration	= r.Read7BitEncodedInt();
			Operations	= r.ReadList(() => {
												var o = Operation.FromType((Operations)r.ReadByte());
												o.Transaction = this;
												o.Read(r); 
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
