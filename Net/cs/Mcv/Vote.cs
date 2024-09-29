namespace Uccs.Net
{
	public class Vote
	{
		public int					ParentId => RoundId - Mcv.P;

		public List<Peer>			Peers;
		public bool					BroadcastConfirmed;
		public Round				Round;
		public DateTime				Created;
		AccountAddress				_Generator;
		byte[]						_RawForBroadcast;
		Mcv							Mcv;

		public int					RoundId;
		public int					Try; /// TODO: revote if consensus not reached
		public Time					Time;
		public byte[]				ParentHash;
		public EntityId[]			MemberLeavers = {};
		///public AccountAddress[]		FundJoiners = {};
		///public AccountAddress[]		FundLeavers = {};
		public EntityId[]			Violators = {};
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
					_Generator = Mcv.Net.Cryptography.AccountFrom(Signature, Hashify());
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
			return $"{RoundId}, {_Generator?.Bytes.ToHex()}, ParentSummary={ParentHash?.ToHex()}, Violators={{{Violators.Length}}}, Leavers={{{MemberLeavers.Length}}}, Time={Time}, Tx(n)={Transactions.Length}, Op(n)={Transactions.Sum(i => i.Operations.Length)}, BroadcastConfirmed={BroadcastConfirmed}";
		}
		
		public void AddTransaction(Transaction t)
		{
			t.Vote = this;
			Transactions = Transactions.Prepend(t).ToArray();
		}
		
		public void Sign(AccountKey generator)
		{
			_Generator = generator;
			Signature = Mcv.Net.Cryptography.Sign(generator, Hashify());
		}

		protected byte[] Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.WriteUtf8(Mcv.Net.Name);
			w.Write(_Generator);
			w.Write7BitEncodedInt(RoundId);

			WriteVote(w);

			return Cryptography.Hash(s.ToArray());
		}

		protected virtual void WriteVote(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Try);
			writer.Write(Time);
			writer.Write(ParentHash);

			writer.Write(MemberLeavers);
			///writer.Write(FundJoiners);
			///writer.Write(FundLeavers);
			writer.Write(Violators);

			writer.Write(Transactions, t => t.WriteForVote(writer));
		}

		protected virtual void ReadVote(BinaryReader reader)
		{
			Try					= reader.Read7BitEncodedInt();
			Time				= reader.Read<Time>();
			ParentHash			= reader.ReadBytes(Cryptography.HashSize);

			MemberLeavers		= reader.ReadArray<EntityId>();
			///FundJoiners			= reader.ReadArray<AccountAddress>();
			///FundLeavers			= reader.ReadArray<AccountAddress>();
			Violators			= reader.ReadArray<EntityId>();

			Transactions = reader.ReadArray(() =>	{
														var t = new Transaction {Net = Mcv.Net, Mcv = Mcv, Vote = this};
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
