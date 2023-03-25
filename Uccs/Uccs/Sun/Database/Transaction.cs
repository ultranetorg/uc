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
		
		public Payload					Payload;
		public AccountAddress					Generator;
		public int						RoundMax;
		public byte[]					Signature;
		
		public AccountAddress					Signer;
		public Settings					Settings;

		public byte[]					Prefix => Signature.Take(Consensus.PrefixLength).ToArray();

		public bool Valid
		{
			get
			{
				return Operations.All(i => i.Valid);
			}
		}

 		public Transaction(Settings settings)
 		{
 			Settings = settings;
 		}

		public Transaction(Settings settings, AccountKey signer)
		{
			Settings	= settings;
			Signer		= signer;
		}

		public void Sign(AccountAddress member, int rmax)
		{
			Generator	= member;
			RoundMax	= rmax;
			Signature	= Cryptography.Current.Sign(Signer as AccountKey, Hashify());
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

		public override string ToString()
		{
			return $"Operations={{{Operations.Count}}}, Signer={Signer}, RoundMax={RoundMax}";
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.WriteUtf8(Settings.Zone.Name); 
			w.Write(Generator);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => i.Write(w));

			return Cryptography.Current.Hash(s.ToArray());
		}

 		public void	WriteConfirmed(BinaryWriter w)
 		{
			w.Write(Signature);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
 		}
 		
 		public void	ReadConfirmed(BinaryReader r)
 		{
			Signature	= r.ReadSignature();
			RoundMax	= r.Read7BitEncodedInt();
 			Operations	= r.ReadList(() => {
 												var o = Operation.FromType((Operations)r.ReadByte());
 												o.Placing		= PlacingStage.Confirmed;
 												//o.Signer		= Signer;
 												o.Transaction	= this;
 												o.Read(r); 
 												return o; 
 											});
			
			Signer = Cryptography.Current.AccountFrom(Signature, Hashify());

			foreach(var i in Operations)
				i.Signer = Signer;
 		}

		public void Write(BinaryWriter w)
		{
			w.Write(Signature);
			w.Write(Generator);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
		}

		public void Read(BinaryReader r)
		{
			Signature	= r.ReadSignature();
			Generator	= r.ReadAccount();
			RoundMax	= r.Read7BitEncodedInt();
			Operations	= r.ReadList(() => {
												var o = Operation.FromType((Operations)r.ReadByte());
												//o.Signer = Signer;
												o.Transaction = this;
												o.Read(r); 
												return o; 
											});

			Signer = Cryptography.Current.AccountFrom(Signature, Hashify());

			foreach(var i in Operations)
				i.Signer = Signer;
		}

		public void WriteUnconfirmed(BinaryWriter w)
		{
			w.Write(Signature);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
		}

		public void ReadUnconfirmed(BinaryReader r)
		{

			Signature	= r.ReadSignature();
			RoundMax	= r.Read7BitEncodedInt();
			Operations	= r.ReadList(() => {
												var o = Operation.FromType((Operations)r.ReadByte());
												o.Signer = Signer;
												o.Transaction = this;
												o.Read(r); 
												return o; 
											});

			Signer = Cryptography.Current.AccountFrom(Signature, Hashify());

			foreach(var i in Operations)
				i.Signer = Signer;
		}
	}
}
