using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public enum BlockType : byte
	{
		MembersJoinRequest = 1, Vote = 2, Payload = 3
	}

	public class BlockPiece : IBinarySerializable, IEquatable<BlockPiece>
	{
		public byte[]			Guid { get; set; }
		public int				RoundId { get; set; }
		public int				Index { get; set; }
		public int				Total { get; set; }
		public byte[]			Signature { get; set; }
		public byte[]			Data { get; set; }

		public List<Peer>		Peers;
		public bool				Broadcasted;
		public AccountAddress	Generator { get; protected set; }
		public const int		GuidLength = 8;

		Zone					Zone;

		public BlockPiece(Zone zone)
		{
			Zone = zone;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Guid);
			writer.Write7BitEncodedInt(RoundId);
			writer.Write7BitEncodedInt(Index);
			writer.Write7BitEncodedInt(Total);
			writer.Write7BitEncodedInt(Data.Length);
			writer.Write(Data);
			writer.Write(Signature);
		}

		public void Read(BinaryReader reader)
		{
			Guid		= reader.ReadBytes(GuidLength);
			RoundId		= reader.Read7BitEncodedInt();
			Index		= reader.Read7BitEncodedInt();
			Total		= reader.Read7BitEncodedInt();
			Data		= reader.ReadBytes(reader.Read7BitEncodedInt());
			Signature	= reader.ReadSignature();

			Generator = Zone.Cryptography.AccountFrom(Signature, Hashify());
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(Guid);
			w.Write7BitEncodedInt(RoundId);
			w.Write7BitEncodedInt(Index);
			w.Write7BitEncodedInt(Total);
			w.Write(Data);

			return Zone.Cryptography.Hash(s.ToArray());
		}

		public void Sign(AccountKey generator)
		{
			Generator = generator;
			Signature = Zone.Cryptography.Sign(generator, Hashify());
		}

		public override string ToString()
		{
			return $"{RoundId}, {Index}/{Total}, {Hex.ToHexString(Generator)}, Broadcasted={Broadcasted}";
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as BlockPiece);
		}

		public bool Equals(BlockPiece other)
		{
			return other is not null && Signature.SequenceEqual(other.Signature);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Signature);
		}
	}

	public abstract class Block : ITypedBinarySerializable, IBinarySerializable
	{
		public const int			SizeMax = 65536;

		public virtual bool			Valid => true;

		public int					RoundId { get; set; }
		public AccountAddress		Generator { get; protected set; }
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
			w.Write(Database.Zone.Name);
			w.Write7BitEncodedInt(RoundId);

			HashWrite(w);
											
			return Database.Zone.Cryptography.Hash(s.ToArray());
		}
						
		public void Sign(AccountKey generator)
		{
			Generator = generator;
			Hash = Hashify();
			Signature = Database.Zone.Cryptography.Sign(generator, Hash);
		} 
	}

	public class MembersJoinRequest : Block
	{
		public IPAddress[]		IPs;

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
			Generator	= Database.Zone.Cryptography.AccountFrom(Signature, Hash);
		}
	}

	public class Vote : Block
	{
		public override bool			Valid => RoundId > 0 && Database.Zone.Cryptography.Valid(Signature, Hash, Generator);
		public DateTime					Time;

		public int						Try; /// TODO: revote if consensus not reached
		public long						TimeDelta;
		public Consensus				Consensus;
		public List<AccountAddress>		Violators = new();
		public List<AccountAddress>		Joiners = new();
		public List<AccountAddress>		Leavers = new();
		public List<AccountAddress>		FundJoiners = new();
		public List<AccountAddress>		FundLeavers = new();
		//public List<Proposition>		Propositions = new();

		public byte[]					Prefix => Hash.Take(Consensus.PrefixLength).ToArray();
		//public byte[]					PropositionsHash;

		public Vote(Database c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + $", Parents={{{Consensus.Payloads.Count}}}, Violators={{{Violators.Count}}}, Joiners={{{Joiners.Count}}}, Leavers={{{Leavers.Count}}}, TimeDelta={TimeDelta}";
		}

		protected override void HashWrite(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			writer.Write(Consensus);

			writer.Write(Joiners);
			writer.Write(Leavers);
			writer.Write(Violators);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
		}

		protected void WriteVote(BinaryWriter writer)
		{
			writer.Write(Signature);

			writer.Write7BitEncodedInt(RoundId);
			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			writer.Write(Consensus);

			writer.Write(Joiners);
			writer.Write(Leavers);
			writer.Write(Violators);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
		}

		public override void Write(BinaryWriter writer)
		{
			WriteVote(writer);
		}

		protected void ReadVote(BinaryReader reader)
		{
			Signature	= reader.ReadSignature();

			RoundId		= reader.Read7BitEncodedInt();
			Try			= reader.Read7BitEncodedInt();
			TimeDelta	= reader.Read7BitEncodedInt64();
			Consensus	= reader.Read<Consensus>();

			Joiners		= reader.ReadAccounts();
			Leavers		= reader.ReadAccounts();
			Violators	= reader.ReadAccounts();
			FundJoiners	= reader.ReadAccounts();
			FundLeavers	= reader.ReadAccounts();
		}

		public override void Read(BinaryReader reader)
		{
			ReadVote(reader);

			Hash = Hashify();
			Generator = Database.Zone.Cryptography.AccountFrom(Signature, Hash);
		}
	}

	public class Payload : Vote
	{
		public List<Transaction>		Transactions = new();
		public IEnumerable<Transaction> SuccessfulTransactions => Transactions.Where(i => i.SuccessfulOperations.Count() == i.Operations.Count);
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
				
		protected override void HashWrite(BinaryWriter writer)
		{
			writer.Write(Generator); /// needed to read check transactions' signatures in Payload

			base.HashWrite(writer);

			foreach(var i in Transactions) 
			{
				writer.Write(i.Signature);
			}
		}

		public override void Write(BinaryWriter writer)
		{
			WriteVote(writer);

			writer.Write(Generator); /// needed to read check transactions' signatures in Payload
			writer.Write(Transactions, t => t.WriteAsPartOfBlock(writer));
		}

		public override void Read(BinaryReader reader)
		{
			ReadVote(reader);

			Generator = reader.ReadAccount();	
			Transactions = reader.ReadList(() =>	{
														var t = new Transaction(Database.Zone)
																{
																	Payload		= this,
																	Generator	= Generator
																};
														t.ReadAsPartOfBlock(reader);
														return t;
													});
			Hash = Hashify();
		}

 		public void WriteConfirmed(BinaryWriter w)
 		{
			w.Write(Generator);
 			w.Write(SuccessfulTransactions, i => i.WriteAsPartOfBlock(w));
 		}
 		
 		public void ReadConfirmed(BinaryReader r)
 		{
			Generator = r.ReadAccount();
 			Transactions = r.ReadList(() =>	{
 												var t = new Transaction(Database.Zone)
														{ 
															Payload = this, 
															Generator = Generator
														};
 												t.ReadAsPartOfBlock(r);
 												return t;
 											});
 		}
	}
}
