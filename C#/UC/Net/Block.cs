using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.BouncyCastle.Utilities.Encoders;
using System.Diagnostics;

namespace UC.Net
{
	public enum BlockType : byte
	{
		JoinRequest = 1, Vote = 2, Payload = 3
	}

	public abstract class Block : ITypedBinarySerializable
	{
		public int					RoundId { get; set; }
		public byte[]				Signature;
		public Account				Member { get; set; }

		public int					ParentId  => RoundId - Roundchain.Pitch;
		public Round				Round;
		protected Roundchain		Chain;
		public virtual bool			Valid => RoundId > 0 && Cryptography.Current.Valid(Signature, Hash, Member);
		public bool					Confirmed = false;

		protected abstract void		WriteForSigning(BinaryWriter w);

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

		public byte[] Hash
		{
			get
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				w.Write((byte)Type);
				w.Write(Chain.Settings.Zone);
				w.Write7BitEncodedInt(RoundId);

				WriteForSigning(w);
											
				return Cryptography.Current.Hash(s.ToArray());
			}
		}

		public void Sign(PrivateAccount member)
		{
			Member = member;
			Signature = Cryptography.Current.Sign(member, Hash);
		} 

		public static Block FromType(Roundchain chaim, BlockType type)
		{
			return type switch
						{
							BlockType.Vote					=> new Vote(chaim),
							BlockType.Payload				=> new Payload(chaim),
							BlockType.JoinRequest			=> new JoinRequest(chaim),
							_								=> throw new IntegrityException("Wrong BlockType"),
						};
		}

		public BlockType Type
		{
			get
			{
				return this switch
							{
								Payload					=> BlockType.Payload,
								Vote 					=> BlockType.Vote,
								JoinRequest				=> BlockType.JoinRequest,
								_						=> throw new IntegrityException("Wrong Block class"),
							};
			}
		}

		public byte BinaryType => (byte)Type;

		public virtual void Write(BinaryWriter w)
		{
			if(Signature.Length != Cryptography.SignatureSize)
				throw new IntegrityException("Wrong Signature length");

			w.Write7BitEncodedInt(RoundId);
			w.Write(Member);						/// needed to hash transactions
			w.Write(Signature);
		}

		public virtual void Read(BinaryReader r)
		{
			RoundId		= r.Read7BitEncodedInt();
			Member		= r.ReadAccount();	
			Signature	= r.ReadSignature();
		}
	}

	public class JoinRequest : Block
	{
		public IPAddress	IP;

		public CandidacyDeclaration Declaration;

		public JoinRequest(Roundchain c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + ", IP=" + IP;
		}

		protected override void WriteForSigning(BinaryWriter w)
		{
			w.Write((IP ?? IPAddress.None).GetAddressBytes());
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write((IP ?? IPAddress.None).GetAddressBytes());
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			IP = new IPAddress(r.ReadBytes(4));
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
		public List<Account>		FundableAssignments = new();
		public List<Account>		FundableRevocations = new();
		//public List<Proposition>	Propositions = new();

		public byte[]				Prefix => Hash.Take(RoundReference.PrefixLength).ToArray();
		public byte[]				PropositionsHash;

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

			PropositionsHash = HashProposals();
			writer.Write(PropositionsHash);
		}

		byte[] HashProposals()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			foreach(var i in Violators)
				w.Write(i);

			foreach(var i in Joiners)
				w.Write(i);

			foreach(var i in Leavers)
				w.Write(i);

			foreach(var i in FundableAssignments)
				w.Write(i);

			foreach(var i in FundableRevocations)
				w.Write(i);

			//foreach(var i in Propositions)
			//	i.Write(w);

			return Cryptography.Current.Hash(s.ToArray());
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write7BitEncodedInt(Try);
			w.Write7BitEncodedInt64(TimeDelta);
			Reference.Write(w);

			w.Write(Violators);
			w.Write(Joiners);
			w.Write(Leavers);
			w.Write(FundableAssignments);
			w.Write(FundableRevocations);
			//w.Write(Propositions);
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Try = r.Read7BitEncodedInt();
			TimeDelta = r.Read7BitEncodedInt64();
			Reference = new RoundReference();
			Reference.Read(r);

			Violators			= r.ReadAccounts();
			Joiners				= r.ReadAccounts();
			Leavers				= r.ReadAccounts();
			FundableAssignments	= r.ReadAccounts();
			FundableRevocations	= r.ReadAccounts();
			//Propositions		= r.ReadList<Proposition>();
			
			PropositionsHash = HashProposals();
		}
	}

	public class Payload : Vote
	{
		public List<Transaction>		Transactions = new();
		public IEnumerable<Transaction> SuccessfulTransactions => Transactions.Where(i => i.SuccessfulOperations.Any());
		public byte[]					OrderingKey => Member;

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
			w.Write(Member);
 			w.Write(SuccessfulTransactions, i => i.WriteConfirmed(w));
 		}
 		
 		public void ReadConfirmed(BinaryReader r)
 		{
			Member = r.ReadAccount();
 			Transactions = r.ReadList(() =>	{
 												var t = new Transaction(Chain.Settings){ Payload = this };
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
															Member	 = Member
														};

												t.Read(r);
												return t;
											});
		}
	}
}
