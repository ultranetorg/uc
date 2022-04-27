using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Nethereum.Signer;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net
{
	public class Transaction : IBinarySerializable
	{
		public int						Id;			/// makes same tx to have different hashes and for a member to determine when next tx can be added
		public int						RoundMax;
		public byte[]					Signature;
		public List<Operation>			Operations = new ();
		public IEnumerable<Operation>	SuccessfulOperations => Operations.Where(i => i.Result == OperationResult.OK);
		
		public Account					Member;
		public Account					Signer;
		public Payload					Payload;
		public bool						Successful;
		public ProcessingStage			Stage;
		public Settings					Settings;

		public bool Valid
		{
			get
			{
 				if(!Cryptography.Current.Valid(Signature, Hash(), Signer))
 					return false;

				return Operations.All(i => i.IsValid());
			}
		}

		public Transaction(Settings chain)
		{
			Settings = chain;
		}

		public Transaction(Settings chain, PrivateAccount signer, int id)
		{
			Settings	= chain;
			Id			= id;
			Signer		= signer;
		}

		public void AddOperation(Operation operation)
		{ 
			Operations.Add(operation);
			operation.Transaction = this;
		}

		public override string ToString()
		{
			return $"Id={Id}, Signature={(Signature != null ? Hex.ToHexString(Signature).Substring(0, 8) : "")}, RoundMax={RoundMax}";
		}

		public byte[] Hash()
		{
			if(Member == null)	throw new IntegrityException("Member can not be null");
			//if(Signer == null)	throw new IntegrityException("Signer can not be null");

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.WriteUtf8(Settings.Zone); 
			w.Write(Member);
			//w.Write(Signer);
			w.Write7BitEncodedInt(Id);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.HashWrite(w);
									 });

			return Cryptography.Current.Hash(s.ToArray());
		}

		public void Sign(Account member, int rmax)
		{
			Member		= member;
			RoundMax	= rmax;

			Signature = Cryptography.Current.Sign(Signer as PrivateAccount, Hash());
		}

		public int CalculateSize()
		{
			var s = new MemoryStream(); /// mock it
			var w = new BinaryWriter(s);

			Write(w);

			return (int)s.Length;
		}

		public void Write(BinaryWriter w)
		{
			var p = w.BaseStream.Position;

			//w.Write(Signer);
			w.Write(Signature);
			w.Write7BitEncodedInt(Id);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Operations, i => {
										w.Write((byte)i.Type); 
										i.Write(w); 
									 });
		}

		public void Read(BinaryReader r)
		{
			//Signer	= r.ReadAccount();
			Signature	= r.ReadSignature();
			Id			= r.Read7BitEncodedInt();
			RoundMax	= r.Read7BitEncodedInt();
			Operations	= r.ReadList(() => {
												var o = Operation.FromType((Operations)r.ReadByte());
												o.Signer = Signer;
												o.Transaction = this;
												o.Read(r); 
												return o; 
											});

			Signer = Cryptography.Current.AccountFrom(Signature, Hash());

			foreach(var i in Operations)
				i.Signer = Signer;
		}

		public void	Save(BinaryWriter w)
		{
			//w.Write(Signer);
			w.Write(Signature);
			w.Write7BitEncodedInt(Id);
			w.Write7BitEncodedInt(RoundMax);
			w.Write(Successful);
			w.Write(Operations, i => { 
										w.Write((byte)i.Type); 
										w.Write((byte)i.Result); 
										i.Write(w); 
									 });
		}

		public void	Load(BinaryReader r)
		{
			//Signer		= r.ReadAccount();
			Signature	= r.ReadSignature();
			Id			= r.Read7BitEncodedInt();
			RoundMax	= r.Read7BitEncodedInt();
			Successful	= r.ReadBoolean();
			Operations	= r.ReadList(() => {
												var o = Operation.FromType((Operations)r.ReadByte());
												o.Result = (OperationResult)r.ReadByte();
												//o.Signer = Signer;
												o.Transaction = this;
												o.Read(r); 
												return o; 
											});

			Signer = Cryptography.Current.AccountFrom(Signature, Hash());

			foreach(var i in Operations)
				i.Signer = Signer;
		}

		public bool SignatureEquals(Transaction t)
		{
			return Signature != null && t.Signature != null && Signature.SequenceEqual(t.Signature);
		}
	}

}
