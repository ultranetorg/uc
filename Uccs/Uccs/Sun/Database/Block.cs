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
		Nnull = 0, Vote, Payload 
	}

	public class BlockPiece : IBinarySerializable, IEquatable<BlockPiece>
	{
		public BlockType		Type { get; set; }
		public int				Try { get; set; }
		public int				RoundId { get; set; }
		public int				Index { get; set; }
		public int				Total { get; set; }
		public byte[]			Signature { get; set; }
		public byte[]			Data { get; set; }

		public List<Peer>		Peers;
		public bool				BroadcastConfirmed;
		Zone					Zone;
		AccountAddress			_Generator;

		public AccountAddress	Generator
		{ 
			get
			{
				if(_Generator == null)
				{
					_Generator = Zone.Cryptography.AccountFrom(Signature, Hashify());
				}

				return _Generator;
			}
		}

		public BlockPiece(Zone zone)
		{
			Zone = zone;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Type);
			writer.Write7BitEncodedInt(RoundId);
			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt(Index);
			writer.Write7BitEncodedInt(Total);
			writer.Write7BitEncodedInt(Data.Length);
			writer.Write(Data);
			writer.Write(Signature);
		}

		public void Read(BinaryReader reader)
		{
			Type		= (BlockType)reader.ReadByte();
			RoundId		= reader.Read7BitEncodedInt();
			Try			= reader.Read7BitEncodedInt();
			Index		= reader.Read7BitEncodedInt();
			Total		= reader.Read7BitEncodedInt();
			Data		= reader.ReadBytes(reader.Read7BitEncodedInt());
			Signature	= reader.ReadSignature();
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write((byte)Type);
			w.Write7BitEncodedInt(RoundId);
			w.Write7BitEncodedInt(Try);
			w.Write7BitEncodedInt(Index);
			w.Write7BitEncodedInt(Total);
			w.Write(Data);

			return Zone.Cryptography.Hash(s.ToArray());
		}

		public void Sign(AccountKey generator)
		{
			_Generator = generator;
			Signature = Zone.Cryptography.Sign(generator, Hashify());
		}

		public override string ToString()
		{
			return $"{RoundId}, {Index}/{Total}, {Hex.ToHexString(Generator)}, BroadcastConfirmed={BroadcastConfirmed}";
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

	public abstract class Block : ITypedBinarySerializable//, IBinarySerializable
	{
		public const int				SizeMax = 65536;

		public virtual bool				Valid => true;

		public int						RoundId { get; set; }
		public AccountAddress			Generator { get; set; }
		public IEnumerable<IPAddress>	IPs { get; set; } 
		//public byte[]					Signature;
		public byte[]					Hash;

		public BlockType				Type => Enum.Parse<BlockType>(GetType().Name);
		public byte						TypeCode => (byte)Type;
		public int						ParentId => RoundId - Database.Pitch;
		protected Database				Database;
		public Round					Round;


		protected abstract void			Hashify(BinaryWriter w);
		public abstract	void			WriteForRound(BinaryWriter w);
		public abstract	void			ReadForRound(BinaryReader r);

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
		
		public virtual void WriteForPiece(BinaryWriter writer)
		{
			writer.Write(IPs, i => writer.Write(i));
		}
		
		public virtual void ReadForPiece(BinaryReader reader)
		{
			IPs	= reader.ReadArray(() => reader.ReadIPAddress());
		}

		public byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(TypeCode);
			w.WriteUtf8(Database.Zone.Name);
			w.Write7BitEncodedInt(RoundId);
			w.Write(IPs, i => w.Write(i));
			
			Hashify(w);
											
			return Database.Zone.Cryptography.Hash(s.ToArray());
		}
						
		public void Sign(AccountKey generator)
		{
			Generator = generator;
			Hash = Hashify();
			//Signature = Database.Zone.Cryptography.Sign(generator, Hash);
		} 
	}

	public class Vote : Block
	{
		public DateTime					Time;

		public int						Try; /// TODO: revote if consensus not reached
		public long						TimeDelta;
		public byte[]					ParentSummary;
		public List<AccountAddress>		GeneratorJoiners = new();
		public List<AccountAddress>		GeneratorLeavers = new();
		public List<AccountAddress>		HubJoiners = new();
		public List<AccountAddress>		HubLeavers = new();
		public List<AccountAddress>		AnalyzerJoiners = new();
		public List<AccountAddress>		AnalyzerLeavers = new();
		public List<AccountAddress>		FundJoiners = new();
		public List<AccountAddress>		FundLeavers = new();
		public List<AccountAddress>		Violators = new();
		public List<ReleaseAddress>		CleanReleases = new();
		public List<ReleaseAddress>		InfectedReleases = new();

		//public byte[]					Prefix => Hash.Take(Consensus.PrefixLength).ToArray();

		public List<Transaction>		Transactions = new();
		public IEnumerable<Transaction> SuccessfulTransactions => Transactions.Where(i => i.SuccessfulOperations.Count() == i.Operations.Count);
		public byte[]					OrderingKey => Generator;

		//public bool						Confirmed = false;

		public override bool Valid
		{
			get
			{
				if(!base.Valid)
					return false;

				//if(Transactions.GroupBy(i => i.Signer).Any(g => g.Count() > 1)) /// only 1 tx per sender is allowed
				//	return false;

				foreach(var i in Transactions)
				{
					if(i.Expiration < RoundId)
						return false;

					if(!i.Valid)
						return false;
				}

				return true;
			}
		}

		public Vote(Database c) : base(c)
		{
		}

		public override string ToString()
		{
			return base.ToString() + $", Parent={{{Hex.ToHexString(ParentSummary)}}}, Violators={{{Violators.Count}}}, GJoiners={{{GeneratorJoiners.Count}}}, GLeavers={{{GeneratorLeavers.Count}}}, TimeDelta={TimeDelta}, Tx(n)={Transactions.Count}, Op(n)={Transactions.Sum(i => i.Operations.Count)}";
		}

		public void AddNext(Transaction t)
		{
			t.Block = this;
			Transactions.Insert(0, t);
		}

		protected override void Hashify(BinaryWriter writer)
		{
			writer.Write(Generator);
			
			writer.Write7BitEncodedInt(RoundId);
			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			writer.Write(ParentSummary);

			writer.Write(GeneratorJoiners);
			writer.Write(GeneratorLeavers);
			writer.Write(HubJoiners);
			writer.Write(HubLeavers);
			writer.Write(AnalyzerJoiners);
			writer.Write(AnalyzerLeavers);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
			writer.Write(Violators);
			writer.Write(CleanReleases);
			writer.Write(InfectedReleases);

			foreach(var i in Transactions) 
			{
				writer.Write(i.Signature);
			}
		}

		protected void WriteVote(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			writer.Write(ParentSummary);

			writer.Write(GeneratorJoiners);
			writer.Write(GeneratorLeavers);
			writer.Write(HubJoiners);
			writer.Write(HubLeavers);
			writer.Write(AnalyzerJoiners);
			writer.Write(AnalyzerLeavers);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
			writer.Write(Violators);
			writer.Write(CleanReleases);
			writer.Write(InfectedReleases);

			writer.Write(Transactions, t => t.WriteAsPartOfBlock(writer));
		}

		protected void ReadVote(BinaryReader reader)
		{
			RoundId		= reader.Read7BitEncodedInt();
			Try			= reader.Read7BitEncodedInt();
			TimeDelta	= reader.Read7BitEncodedInt64();
			ParentSummary	= reader.ReadBytes(Cryptography.HashSize);

			GeneratorJoiners	= reader.ReadAccounts();
			GeneratorLeavers	= reader.ReadAccounts();
			HubJoiners			= reader.ReadAccounts();
			HubLeavers			= reader.ReadAccounts();
			AnalyzerJoiners		= reader.ReadAccounts();
			AnalyzerLeavers		= reader.ReadAccounts();
			FundJoiners			= reader.ReadAccounts();
			FundLeavers			= reader.ReadAccounts();
			Violators			= reader.ReadAccounts();
			CleanReleases		= reader.ReadList<ReleaseAddress>();
			InfectedReleases	= reader.ReadList<ReleaseAddress>();

			Transactions = reader.ReadList(() =>	{
														var t = new Transaction(Database.Zone)
																{
																	Block		= this,
																	Generator	= Generator
																};
														t.ReadAsPartOfBlock(reader);
														return t;
													});
		}

		public override void WriteForPiece(BinaryWriter writer)
		{
			base.WriteForPiece(writer);
			WriteVote(writer);
		}

		public override void ReadForPiece(BinaryReader reader)
		{
			base.ReadForPiece(reader);
			ReadVote(reader);
			Hash = Hashify();
		}

		public override void WriteForRound(BinaryWriter writer)
		{
			writer.Write(Generator);
			WriteVote(writer);
		}

		public override void ReadForRound(BinaryReader reader)
		{
			Generator = reader.ReadAccount();
			ReadVote(reader);
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
															Block = this, 
															Generator = Generator
														};
 												t.ReadAsPartOfBlock(r);
 												return t;
 											});
 		}
	}

/*
	public class Payload : Vote
	{

		public Payload(Database c) : base(c)
		{
		}
				
		protected override void Hashify(BinaryWriter writer)
		{
			//writer.Write(Generator); /// needed to read check transactions' signatures in Payload

			base.Hashify(writer);

		}

		public override void WriteForPiece(BinaryWriter writer)
		{
			base.WriteForPiece(writer);

			WriteVote(writer);

		}

		public override void ReadForPiece(BinaryReader reader)
		{
			base.ReadForPiece(reader);

			ReadVote(reader);

		}

		public override void WriteForRound(BinaryWriter writer)
		{
			writer.Write(Generator);
			
			WriteVote(writer);

		}

		public override void ReadForRound(BinaryReader reader)
		{
			Generator = reader.ReadAccount();

			ReadVote(reader);

		}
	}*/

}
