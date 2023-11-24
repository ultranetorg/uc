using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public class Vote
	{
		//public const int			SizeMax = 65536;
		public int					ParentId => RoundId - Mcv.P;

		public List<Peer>			Peers;
		public bool					BroadcastConfirmed;
		//public byte[]				Hash;
		public Round				Round;
		public DateTime				Created;
		AccountAddress				_Generator;
		byte[]						_RawForBroadcast;
		Mcv							Mcv;

		public int					RoundId;
		//public IPAddress[]			BaseRdcIPs;
		//public IPAddress[]			SeedHubRdcIPs;
		public int					Try; /// TODO: revote if consensus not reached
		public long					TimeDelta;
		public byte[]				ParentHash;
		//public AccountAddress[]		MemberJoiners = {};
		public AccountAddress[]		MemberLeavers = {};
		public AccountAddress[]		AnalyzerJoiners = {};
		public AccountAddress[]		AnalyzerLeavers = {};
		public AccountAddress[]		FundJoiners = {};
		public AccountAddress[]		FundLeavers = {};
		public AccountAddress[]		Violators = {};
		public ResourceAddress[]	CleanReleases = {};
		public ResourceAddress[]	InfectedReleases = {};
		public OperationId[]		Emissions = {};
		public OperationId[]		DomainBids = {};
		public Transaction[]		Transactions = {};
		public byte[]				Signature { get; set; }

		public int					TransactionCountExcess;

		public bool Valid
		{
			get
			{
				foreach(var i in Transactions)
				{
					if(i.Expiration < RoundId)
						return false;
	
					if(!i.Valid(Mcv))
						return false;
				}
	
				return true;
			}
		}

		public AccountAddress Generator
		{ 
			get
			{
				if(_Generator == null)
				{
					_Generator = Mcv.Zone.Cryptography.AccountFrom(Signature, Hashify());
				}

				return _Generator;
			}
		}

		public byte[] RawForBroadcast
		{
			get
			{ 
				if(_RawForBroadcast == null)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);

					WriteForBroadcast(w);

					_RawForBroadcast = s.ToArray();

				}
			
				return _RawForBroadcast; 
			}

			set { _RawForBroadcast = value; }
		}

		public Vote(Mcv c)
		{
			Mcv = c;
		}

		public override string ToString()
		{
			return $"{RoundId}, {_Generator?.Bytes.ToHex()}, ParentSummary={ParentHash?.ToHex()}, Violators={{{Violators.Length}}}, Leavers={{{MemberLeavers.Length}}}, TimeDelta={TimeDelta}, Tx(n)={Transactions.Length}, Op(n)={Transactions.Sum(i => i.Operations.Length)}, BroadcastConfirmed={BroadcastConfirmed}";
		}
		
		public void AddTransaction(Transaction t)
		{
			t.Vote = this;
			Transactions = Transactions.Prepend(t).ToArray();
		}
		
		public void Sign(AccountKey generator)
		{
			_Generator = generator;
			Signature = Mcv.Zone.Cryptography.Sign(generator, Hashify());
		}

		protected byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.WriteUtf8(Mcv.Zone.Name);
			w.Write(_Generator);
			w.Write7BitEncodedInt(RoundId);

			WriteVote(w);

			return Mcv.Zone.Cryptography.Hash(s.ToArray());
		}

		void WriteVote(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			writer.Write(ParentHash);

			writer.Write(MemberLeavers);
			writer.Write(AnalyzerJoiners);
			writer.Write(AnalyzerLeavers);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
			writer.Write(Violators);
			writer.Write(Emissions);
			writer.Write(DomainBids);
			writer.Write(CleanReleases);
			writer.Write(InfectedReleases);

			writer.Write(Transactions, t => t.WriteForVote(writer));
		}

		void ReadVote(BinaryReader reader)
		{
			Try					= reader.Read7BitEncodedInt();
			TimeDelta			= reader.Read7BitEncodedInt64();
			ParentHash			= reader.ReadBytes(Cryptography.HashSize);

			MemberLeavers		= reader.ReadArray<AccountAddress>();
			AnalyzerJoiners		= reader.ReadArray<AccountAddress>();
			AnalyzerLeavers		= reader.ReadArray<AccountAddress>();
			FundJoiners			= reader.ReadArray<AccountAddress>();
			FundLeavers			= reader.ReadArray<AccountAddress>();
			Violators			= reader.ReadArray<AccountAddress>();
			Emissions			= reader.ReadArray<OperationId>();
			DomainBids			= reader.ReadArray<OperationId>();
			CleanReleases		= reader.ReadArray<ResourceAddress>();
			InfectedReleases	= reader.ReadArray<ResourceAddress>();

			Transactions = reader.ReadArray(() =>	{
														var t = new Transaction {Zone = Mcv.Zone, Vote = this};
														t.ReadForVote(reader);
														return t;
													});
		}

		public void WriteForBroadcast(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write(Signature);
			writer.Write(Generator);
			WriteVote(writer);
		}

		public void ReadForBroadcast(BinaryReader reader)
		{
			RoundId		= reader.Read7BitEncodedInt();
			Signature	= reader.ReadSignature();
			_Generator	= reader.ReadAccount();
			ReadVote(reader);
		}

		public void WriteForRoundUnconfirmed(BinaryWriter writer)
		{
			writer.Write(Signature);
			writer.Write(Generator);
			WriteVote(writer);
		}

		public void ReadForRoundUnconfirmed(BinaryReader reader)
		{
			Signature	= reader.ReadSignature();
			_Generator	= reader.ReadAccount();
			ReadVote(reader);
		}
	}
}
