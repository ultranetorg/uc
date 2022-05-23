using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Nethereum.Signer;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net
{
	public class Transaction : IBinarySerializable, IHashable
	{
		public List<Operation>			Operations = new ();
		public IEnumerable<Operation>	SuccessfulOperations => Operations.Where(i => i.Successful);
		
		public Payload					Payload;
		//public bool						Successful;
		public Account					Generator;
		public int						RoundMax;
		public byte[]					Signature;
		
		public Account					Signer;
		//public ProcessingStage			Stage;
		public Settings					Settings;

		public byte[]					Prefix => Signature.Take(RoundReference.PrefixLength).ToArray();

		public bool Valid
		{
			get
			{
				return Operations.All(i => i.Valid);
			}
		}

		public Transaction(Settings chain)
		{
			Settings = chain;
		}

		public Transaction(Settings settings, PrivateAccount signer)
		{
			Settings	= settings;
			Signer		= signer;
		}

		public void Sign(Account member, int rmax)
		{
			Generator		= member;
			RoundMax	= rmax;
			Signature	= Cryptography.Current.Sign(Signer as PrivateAccount, this);
		}

		public bool SignatureEquals(Transaction t)
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
			return $"Signature={(Signature != null ? Hex.ToHexString(Signature).Substring(0, 8) : "")}, RoundMax={RoundMax}";
		}

		public void HashWrite(BinaryWriter w)
		{
			w.WriteUtf8(Settings.Zone); 
			w.Write(Generator);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => {
										i.HashWrite(w);
									 });
		}

		public void	WriteConfirmed(BinaryWriter w)
		{
			w.Write(Signer);
			w.Write(SuccessfulOperations, i =>	{ 
													w.Write((byte)i.Type); 
													i.WriteConfirmed(w); 
												});
		}
		
		public void	ReadConfirmed(BinaryReader r)
		{
			Signer		= r.ReadAccount();
			Operations	= r.ReadList(() => {
												var o = Operation.FromType((Operations)r.ReadByte());
												o.Placing		= PlacingStage.Confirmed;
												o.Executed		= true;	
												o.Signer		= Signer;
												o.Transaction	= this;
												o.ReadConfirmed(r); 
												return o; 
											});
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Signature);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
		}

		public void Read(BinaryReader r)
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

			Signer = Cryptography.Current.AccountFrom(Signature, this);

			foreach(var i in Operations)
				i.Signer = Signer;
		}
	}
}
