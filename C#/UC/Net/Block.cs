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
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;

namespace UC.Net
{
	public enum BlockType : byte
	{
		MembersJoinRequest = 1, Vote = 2, Payload = 3
	}

	public class BlockPiece : IBinarySerializable
	{
		public int		RoundId { get; set; }
		public int		Piece { get; set; }
		public int		PiecesTotal { get; set; }
		public byte[]	Signature { get; set; }
		public byte[]	Data { get; set; }

		public Account	Generator { get; protected set; }
		public DateTime	Time;

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write7BitEncodedInt(Piece);
			writer.Write7BitEncodedInt(PiecesTotal);
			writer.Write7BitEncodedInt(Data.Length);
			writer.Write(Data);
			writer.Write(Signature);
		}

		public void Read(BinaryReader reader)
		{
			RoundId		= reader.Read7BitEncodedInt();
			Piece		= reader.Read7BitEncodedInt();
			PiecesTotal	= reader.Read7BitEncodedInt();
			Data		= reader.ReadBytes(reader.Read7BitEncodedInt());
			Signature	= reader.ReadSignature();

			Generator = Cryptography.Current.AccountFrom(Signature, Hashify());
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(RoundId);
			w.Write7BitEncodedInt(Piece);
			w.Write7BitEncodedInt(PiecesTotal);
			w.Write(Data);

			return Cryptography.Current.Hash(s.ToArray());
		}

		public void Sign(PrivateAccount generator)
		{
			Generator = generator;
			Signature = Cryptography.Current.Sign(generator, Hashify());
		} 
	}

	public abstract class Block : ITypedBinarySerializable, IBinarySerializable
	{
		public const int			SizeMax = 65536;

		public virtual bool			Valid => true;

		public int					RoundId { get; set; }
		public Account				Generator { get; protected set; }
		public byte[]				Signature;
		public byte[]				Hash;

		public BlockType			Type => Enum.Parse<BlockType>(GetType().Name);
		public byte					TypeCode => (byte)Type;
		public int					ParentId => RoundId - Database.Pitch;
		protected Database			Database;
		public Round				Round;

		protected abstract void		HashWrite(BinaryWriter w);
		public abstract	void		Write(BinaryWriter w);
		public abstract	void		Read(BinaryReader r);

		public Block(Database database)
		{
			Database = database;
		}

		public override string ToString()
		{
			return	$"{Type}, Round={RoundId}";
					//$"Member={Member.ToString().Substring(0, 16)}, " +
					//$"Signature={(Signature != null ? Hex.ToHexString(Signature).Substring(0, 16) : "")}";
		}

		public static Block FromType(Database chain, BlockType type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(Block).Namespace + "." + type).GetConstructor(new System.Type[]{typeof(Database)}).Invoke(new object[]{chain}) as Block;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Block)} type", ex);
			}
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(TypeCode);
			w.Write(Database.Settings.Zone.Name);
			w.Write7BitEncodedInt(RoundId);

			HashWrite(w);
											
			return Cryptography.Current.Hash(s.ToArray());
		}
						
		public void Sign(PrivateAccount generator)
		{
			Hash = Hashify();
			Generator = generator;
			Signature = Cryptography.Current.Sign(generator, Hash);
		} 
	}

	public class MembersJoinRequest : Block
	{
		public IPAddress[]		IPs;
		
		//public Coin			Bail;

		public MembersJoinRequest(Database c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + ", IP=" + string.Join(',', IPs as IEnumerable<IPAddress>);
		}

		protected override void HashWrite(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write(IPs, i => writer.Write(i));
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write(IPs, i => writer.Write(i));
			writer.Write(Signature);
		}

		public override void Read(BinaryReader reader)
		{
			RoundId		= reader.Read7BitEncodedInt();
			IPs			= reader.ReadArray(() => reader.ReadIPAddress());
			Signature	= reader.ReadSignature();
		
			Hash		= Hashify();
			Generator	= Cryptography.Current.AccountFrom(Signature, Hash);
		}
	}

	public class Vote : Block
	{
		public override bool		Valid => RoundId > 0 && Cryptography.Current.Valid(Signature, Hash, Generator);
		public DateTime				Time;

		public int					Try; /// TODO: revote if consensus not reached
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

		public Vote(Database c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + $", Parents={{{Reference.Payloads.Count}}}, Violators={{{Violators.Count}}}, Joiners={{{Joiners.Count}}}, Leavers={{{Leavers.Count}}}, TimeDelta={TimeDelta}";
		}

		protected override void HashWrite(BinaryWriter writer)
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
			writer.Write7BitEncodedInt(RoundId);
			writer.Write(Generator);
			writer.Write(Signature);

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
			RoundId		= reader.Read7BitEncodedInt();
			Generator	= reader.ReadAccount();	
			Signature	= reader.ReadSignature();

			Try			= reader.Read7BitEncodedInt();
			TimeDelta	= reader.Read7BitEncodedInt64();
			Reference	= new RoundReference();
			Reference.Read(reader);

			Violators	= reader.ReadAccounts();
			Joiners		= reader.ReadAccounts();
			Leavers		= reader.ReadAccounts();
			FundJoiners	= reader.ReadAccounts();
			FundLeavers	= reader.ReadAccounts();

			Hash = Hashify();
		}
	}

	public class Payload : Vote
	{
		public List<Transaction>		Transactions = new();
		public IEnumerable<Transaction> SuccessfulTransactions => Transactions.Where(i => i.SuccessfulOperations.Any());
		public byte[]					OrderingKey => Hash;

		public bool						Confirmed = false;

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

		public Payload(Database c) : base(c)
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
				
		protected override void HashWrite(BinaryWriter w)
		{
			base.HashWrite(w);

			foreach(var i in Transactions) 
			{
				w.Write(i.Signature);
			}
		}

 		public void WriteConfirmed(BinaryWriter w)
 		{
			w.Write(Generator);
 			w.Write(SuccessfulTransactions, i => i.WriteConfirmed(w));
 		}
 		
 		public void ReadConfirmed(BinaryReader r)
 		{
			Generator = r.ReadAccount();
 			Transactions = r.ReadList(() =>	{
 												var t = new Transaction(Database.Settings)
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
			w.Write(Transactions, t => t.WriteForBlock(w));
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			Transactions = r.ReadList(() =>	{
												var t = new Transaction(Database.Settings)
														{
															Payload	= this,
															Generator = Generator
														};
												t.ReadForBlock(r);
												return t;
											});
			Hash = Hashify();
		}
	}
}
