using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public class Vote
	{
		public const int				SizeMax = 65536;
		public int						ParentId => RoundId - Database.Pitch;

		public List<Peer>				Peers;
		public bool						BroadcastConfirmed;
		public byte[]					Hash;
		Database						Database;
		public Round					Round;
		public DateTime					Created;
		AccountAddress					_Generator;
		byte[]							_RawForBroadcast;
		public int						RoundId;
		public IEnumerable<IPAddress>	BaseIPs;
		public IEnumerable<IPAddress>	HubIPs;
		public int						Try; /// TODO: revote if consensus not reached
		public long						TimeDelta;
		public byte[]					ParentSummary;
		public List<AccountAddress>		GeneratorJoiners = new();
		public List<AccountAddress>		GeneratorLeavers = new();
		//public List<AccountAddress>		HubJoiners = new();
		//public List<AccountAddress>		HubLeavers = new();
		public List<AccountAddress>		AnalyzerJoiners = new();
		public List<AccountAddress>		AnalyzerLeavers = new();
		public List<AccountAddress>		FundJoiners = new();
		public List<AccountAddress>		FundLeavers = new();
		public List<AccountAddress>		Violators = new();
		public List<ResourceAddress>	CleanReleases = new();
		public List<ResourceAddress>	InfectedReleases = new();
		public List<Transaction>		Transactions = new();
	
		public byte[]					Signature { get; set; }

		public bool Valid
		{
			get
			{
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

		public AccountAddress Generator
		{ 
			get
			{
				if(_Generator == null)
				{
					_Generator = Database.Zone.Cryptography.AccountFrom(Signature, Hashify());
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

		public Vote(Database c)
		{
			Database = c;
		}

		public override string ToString()
		{
			return $"{RoundId}, BroadcastConfirmed={BroadcastConfirmed}, {Hex.ToHexString(Generator)}, Parent={{{Hex.ToHexString(ParentSummary)}}}, Violators={{{Violators.Count}}}, GJoiners={{{GeneratorJoiners.Count}}}, GLeavers={{{GeneratorLeavers.Count}}}, TimeDelta={TimeDelta}, Tx(n)={Transactions.Count}, Op(n)={Transactions.Sum(i => i.Operations.Count)}";
		}

		public void AddNext(Transaction t)
		{
			t.Block = this;
			Transactions.Insert(0, t);
		}
		
		public void Sign(AccountKey generator)
		{
			_Generator = generator;
			Hash = Hashify();
			Signature = Database.Zone.Cryptography.Sign(generator, Hash);
		}

		protected byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.WriteUtf8(Database.Zone.Name);
			w.Write(Generator);
			w.Write7BitEncodedInt(RoundId);

			WriteVote(w);

			return Database.Zone.Cryptography.Hash(s.ToArray());
		}

		void WriteVote(BinaryWriter writer)
		{
			writer.Write(BaseIPs, i => writer.Write(i));
			writer.Write(HubIPs, i => writer.Write(i));

			writer.Write7BitEncodedInt(Try);
			writer.Write7BitEncodedInt64(TimeDelta);
			writer.Write(ParentSummary);

			writer.Write(GeneratorJoiners);
			writer.Write(GeneratorLeavers);
			//writer.Write(HubJoiners);
			//writer.Write(HubLeavers);
			writer.Write(AnalyzerJoiners);
			writer.Write(AnalyzerLeavers);
			writer.Write(FundJoiners);
			writer.Write(FundLeavers);
			writer.Write(Violators);
			writer.Write(CleanReleases);
			writer.Write(InfectedReleases);

			writer.Write(Transactions, t => t.WriteForVote(writer));
		}

		void ReadVote(BinaryReader reader)
		{
			BaseIPs			= reader.ReadArray(() => reader.ReadIPAddress());
			HubIPs			= reader.ReadArray(() => reader.ReadIPAddress());

			Try				= reader.Read7BitEncodedInt();
			TimeDelta		= reader.Read7BitEncodedInt64();
			ParentSummary	= reader.ReadBytes(Cryptography.HashSize);

			GeneratorJoiners	= reader.ReadAccounts();
			GeneratorLeavers	= reader.ReadAccounts();
			//HubJoiners			= reader.ReadAccounts();
			//HubLeavers			= reader.ReadAccounts();
			AnalyzerJoiners		= reader.ReadAccounts();
			AnalyzerLeavers		= reader.ReadAccounts();
			FundJoiners			= reader.ReadAccounts();
			FundLeavers			= reader.ReadAccounts();
			Violators			= reader.ReadAccounts();
			CleanReleases		= reader.ReadList<ResourceAddress>();
			InfectedReleases	= reader.ReadList<ResourceAddress>();

			Transactions = reader.ReadList(() =>	{
														var t = new Transaction(Database.Zone)
																{
																	Block		= this,
																	Generator	= Generator
																};
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
		
			Hash = Hashify();
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
			
			Hash = Hashify();
		}
	}

}
