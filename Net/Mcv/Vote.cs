namespace Uccs.Net;

public class Vote : IBinarySerializable
{
	public int					ParentId => RoundId - Mcv.P;

	//public List<Peer>			Peers;
	public bool					BroadcastConfirmed;
	public Round				Round;
	public DateTime				Created;
	AccountAddress				_Generator;
	byte[]						_Hash;
	byte[]						_RawPayload;
	Mcv							Mcv;

	public int					RoundId;
	public int					Try; /// TODO: revote if consensus not reached
	public Time					Time;
	public byte[]				ParentHash;
	public AutoId[]				MemberLeavers = [];
	///public AccountAddress[]	FundJoiners = {};
	///public AccountAddress[]	FundLeavers = {};
	public AutoId[]				Violators = [];
	public byte[][]				NntBlocks = [];
	public Transaction[]		Transactions = [];
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
				_Hash = Hashify();
				_Generator = Mcv.Net.Cryptography.AccountFrom(Signature, _Hash);
			}

			return _Generator;
		}
		set
		{
			_Generator = value;
		}
	}

	public byte[] Hash
	{ 
		get
		{
			if(_Generator == null)
			{
				_Hash = Hashify();
				_Generator = Mcv.Net.Cryptography.AccountFrom(Signature, _Hash);
			}

			return _Hash;
		}
	}

	public byte[] RawPayload
	{
		get
		{ 
			if(_RawPayload == null)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				WritePayload(w);

				_RawPayload = s.ToArray();
			}
		
			return _RawPayload; 
		}

		set { _RawPayload = value; }
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

	public byte[] Hashify()
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write((byte)Mcv.Net.Zone);
		w.WriteUtf8(Mcv.Net.Address);
		//w.Write(_Generator);
		w.Write7BitEncodedInt(RoundId);
		w.WriteBytes(RawPayload);

		return Cryptography.Hash(s.ToArray());
	}

	protected virtual void WritePayload(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Try);
		writer.Write(Time);
		writer.Write(ParentHash);

		writer.Write(MemberLeavers);
		///writer.Write(FundJoiners);
		///writer.Write(FundLeavers);
		writer.Write(Violators);
		writer.Write(NntBlocks, writer.WriteBytes);

		writer.Write(Transactions, t => t.WriteForVote(writer));
	}

	protected virtual void ReadPayload(BinaryReader reader)
	{
		Try					= reader.Read7BitEncodedInt();
		Time				= reader.Read<Time>();
		ParentHash			= reader.ReadBytes(Cryptography.HashSize);

		MemberLeavers		= reader.ReadArray<AutoId>();
		///FundJoiners		= reader.ReadArray<AccountAddress>();
		///FundLeavers		= reader.ReadArray<AccountAddress>();
		Violators			= reader.ReadArray<AutoId>();
		NntBlocks			= reader.ReadArray(reader.ReadBytes);

		Transactions = reader.ReadArray(() =>	{
													var t = new Transaction {Net = Mcv.Net, Vote = this};
													t.ReadForVote(reader);
													return t;
												});
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(RoundId);
		writer.Write(Signature);
		writer.WriteBytes(RawPayload);
	}

	public void Read(BinaryReader reader)
	{
		RoundId		= reader.Read7BitEncodedInt();
		Signature	= reader.ReadSignature();
		_RawPayload	= reader.ReadBytes();
	}

	public void WriteForRoundUnconfirmed(BinaryWriter writer)
	{
		writer.Write(Signature);
		writer.Write(Generator);
		WritePayload(writer);
	}

	public void ReadForRoundUnconfirmed(BinaryReader reader)
	{
		Signature	= reader.ReadSignature();
		_Generator	= reader.ReadAccount();
		ReadPayload(reader);
	}

	public void Restore()
	{
		ReadPayload(new BinaryReader(new MemoryStream(RawPayload)));
	}

	public void Dump(Round round, Log log)
	{
		foreach(var m in round.VotersRound.Members)
			log.ReportWarning(this, $"Member {m}");

		foreach(var t in Transactions)
		{	
			log.ReportWarning(this, $"----Transaction {t}" );
			log.ReportWarning(this, $"----NearestBy {round.VotersRound.Members.NearestBy(m => m.Address, t.Signer).Address}");
			log.ReportWarning(this, $"----Signature {t.Signature.ToHex()}" );
			log.ReportWarning(this, $"----Hash {t.Hashify().ToHex()}" );
			log.ReportWarning(this, $"----Zone {t.Net.Zone}");
			log.ReportWarning(this, $"----Net {t.Net.Address}");
			log.ReportWarning(this, $"----Member {t.Member}");
			log.ReportWarning(this, $"----Nid {t.Nid}");
			log.ReportWarning(this, $"----Expiration {t.Expiration}");
			log.ReportWarning(this, $"----Bonus {t.Bonus}");
			log.ReportWarning(this, $"----Tag {t.Tag?.ToHex()}");
			log.ReportWarning(this, $"----Sponsored {t.Sponsored}");

			foreach(var i in t.Operations)
				log.ReportWarning(this, $"--------Operation {i}");
		}

		log.ReportWarning(this, $"RoundId {RoundId}");
		log.ReportWarning(this, $"Signature {Signature.ToHex()}");
		log.ReportWarning(this, $"Hash {Hashify().ToHex()}");
		log.ReportWarning(this, $"RawPayload {RawPayload.ToHex()}");
	}

}
