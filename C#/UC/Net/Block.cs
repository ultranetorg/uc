using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.BouncyCastle.Utilities.Encoders;
using System.Diagnostics;
using System.Reflection;

namespace UC.Net
{
	public enum BlockType : byte
	{
		GeneratorJoinRequest = 1, Vote = 2, Payload = 3
	}

	public abstract class Block : ITypedBinarySerializable, IBinarySerializable
	{
		public int					RoundId { get; set; }
		public byte[]				Signature;
		public Account				Generator { get; set; }

		public byte					TypeCode => (byte)Type;
		public int					ParentId  => RoundId - Roundchain.Pitch;
		public Round				Round;
		protected Roundchain		Chain;
		public virtual bool			Valid => RoundId > 0 && Cryptography.Current.Valid(Signature, Hash, Generator);
		public bool					Confirmed = false;
		public byte[]				Hash;

		protected abstract void		WriteForSigning(BinaryWriter w);

		public BlockType			Type => Enum.Parse<BlockType>(GetType().Name);

		public Block(Roundchain c)
		{
			Chain = c;
		}

		public override string ToString()
		{
			return	$"{Type}, Round={RoundId}";
					//$"Member={Member.ToString().Substring(0, 16)}, " +
					//$"Signature={(Signature != null ? Hex.ToHexString(Signature).Substring(0, 16) : "")}";
		}

		public static Block FromType(Roundchain chain, BlockType type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(Block).Namespace + "." + type).GetConstructor(new System.Type[]{typeof(Roundchain)}).Invoke(new object[]{chain}) as Block;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Block)} type", ex);
			}
		}

		public byte[] CalculateHash()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write((byte)Type);
			w.Write(Chain.Settings.Zone.Name);
			w.Write7BitEncodedInt(RoundId);

			WriteForSigning(w);
											
			return Cryptography.Current.Hash(s.ToArray());
		}

		public void Sign(PrivateAccount generator)
		{
			Hash = CalculateHash();
			Generator = generator;
			Signature = Cryptography.Current.Sign(generator, Hash);
		} 

		public virtual void Write(BinaryWriter w)
		{
			if(Signature.Length != Cryptography.SignatureSize)
				throw new IntegrityException("Wrong Signature length");

			w.Write7BitEncodedInt(RoundId);
			w.Write(Generator);						/// needed to hash transactions
			w.Write(Signature);
		}

		public virtual void Read(BinaryReader r)
		{
			RoundId		= r.Read7BitEncodedInt();
			Generator	= r.ReadAccount();	
			Signature	= r.ReadSignature();
		}
	}

	public class GeneratorJoinRequest : Block
	{
		public IPAddress	IP;
		public CandidacyDeclaration Declaration;

		public GeneratorJoinRequest(Roundchain c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + ", IP=" + IP;
		}

		protected override void WriteForSigning(BinaryWriter w)
		{
			w.Write(IP);
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(IP);
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			IP = r.ReadIPAddress();
		
			Hash = CalculateHash();
		}
	}
	
	public class Vote : Block
	{
		public int					Try; /// TODO: revote if consensus not reached
		public DateTime				Time;
		public long					TimeDelta;
		public RoundReference		Reference;
		public List<Account>		Violators = new();
		public List<Account>		Joiners = new();
		public List<Account>		Leavers = new();
		public List<Account>		FundJoiners = new();
		public List<Account>		FundLeavers = new();
		//public List<Proposition>	Propositions = new();

		public byte[]				Prefix => Hash.Take(RoundReference.PrefixLength).ToArray();
		//public byte[]				PropositionsHash;

		public Vote(Roundchain c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + $", Parents={{{Reference.Payloads.Count}}}, Violators={{{Violators.Count}}}, Joiners={{{Joiners.Count}}}, Leavers={{{Leavers.Count}}}, TimeDelta={TimeDelta}";
		}

		protected override void WriteForSigning(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			Reference.WriteHashable(writer);

			writer.Write(Violators);
			writer.Write(Joiners);
			writer.Write(Leavers);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);

			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			Reference.Write(writer);

			writer.Write(Violators);
			writer.Write(Joiners);
			writer.Write(Leavers);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);

			Try = reader.Read7BitEncodedInt();
			TimeDelta = reader.Read7BitEncodedInt64();
			Reference = new RoundReference();
			Reference.Read(reader);

			Violators	= reader.ReadAccounts();
			Joiners		= reader.ReadAccounts();
			Leavers		= reader.ReadAccounts();
			FundJoiners	= reader.ReadAccounts();
			FundLeavers	= reader.ReadAccounts();

			Hash = CalculateHash();
		}
	}

	public class Payload : Vote
	{
		public List<Transaction>		Transactions = new();
		public IEnumerable<Transaction> SuccessfulTransactions => Transactions.Where(i => i.SuccessfulOperations.Any());
		public byte[]					OrderingKey => Generator;

		public override bool Valid
		{
			get
			{
				if(!base.Valid)
					return false;

				if(Transactions.GroupBy(i => i.Signer).Any(g => g.Count() > 1)) /// only 1 tx per sender is allowed
					return false;

				foreach(var i in Transactions)
				{
					if(i.RoundMax < RoundId)
						return false;

					if(!i.Valid)
						return false;
				}

				return true;
			}
		}

		public Payload(Roundchain c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + $", Tx(n)={Transactions.Count}, Op(n)={Transactions.Sum(i => i.Operations.Count)}";
		}

		public void AddNext(Transaction t)
		{
			t.Payload = this;
			Transactions.Insert(0, t);
		}
				
		protected override void WriteForSigning(BinaryWriter w)
		{
			base.WriteForSigning(w);

			foreach(var i in Transactions) 
			{
				w.Write(i.Signature);
			}
		}

 		public void WriteConfirmed(BinaryWriter w)
 		{
			//w.Write7BitEncodedInt64(TimeDelta);
			w.Write(Generator);
 			w.Write(SuccessfulTransactions, i => i.WriteConfirmed(w));
 		}
 		
 		public void ReadConfirmed(BinaryReader r)
 		{
			//TimeDelta = r.Read7BitEncodedInt64();
			Generator = r.ReadAccount();
 			Transactions = r.ReadList(() =>	{
 												var t = new Transaction(Chain.Settings)
														{ 
															Payload = this, 
															Generator = Generator
														};
 												t.ReadConfirmed(r);
 												return t;
 											});
 		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);
			w.Write(Transactions);
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			Transactions = r.ReadList(() =>	{
												var t = new Transaction(Chain.Settings)
														{
															Payload	 = this,
															Generator	 = Generator
														};

												t.Read(r);
												return t;
											});
			Hash = CalculateHash();
		}
	}
}
