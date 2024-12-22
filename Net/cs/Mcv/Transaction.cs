namespace Uccs.Net;

public enum TransactionStatus
{
	None, Pending, Accepted, Placed, FailedOrNotFound, Confirmed
}

public class Transaction : IBinarySerializable
{
	const int						PoWLength = 16;
	const int						TagLengthMax = 1024;

	public int						Nid { get; set; }
	TransactionId					_Id;
	public TransactionId			Id
									{ 
										set => _Id = value; 
										get
										{
											if(_Id != default)
												return _Id;

											_Id = Round != null && Round.Confirmed ? new (Round.Id, Array.IndexOf(Round.ConsensusTransactions, this)) : default; 

											return _Id;
										}
									 }
	public Operation[]				Operations = {};
	public bool						Successful => Operations.Any() && Operations.All(i => i.Error == null);

#if IMMISSION
	public bool						EmissionOnly => Operations.All(i => i is Immission);
#endif

	public McvNet					Net;
	public Vote						Vote;
	public Round					Round;
	public EntityId					Member;
	public int						Expiration { get; set; }
	public byte[]					PoW;
	public byte[]					Tag;
	//public Money					STFee;
	public long						ECFee;
	
	public long						ECSpent;
	public long						BYReward;
	//public long					ECReward;
	public byte[]					Signature { get; set; }

	private AccountAddress			_Signer;
	public AccountAddress			Signer { get => _Signer ??= Net.Cryptography.AccountFrom(Signature, Hashify()); set => _Signer = value; }
	public TransactionStatus		Status;
	public IPeer					Rdi;
	public Flow						Flow;
	public TransactionStatus		__ExpectedStatus = TransactionStatus.None;

	public bool Valid(Mcv mcv)
	{
		return	(Tag == null || Tag.Length <= TagLengthMax) &&
				Operations.Any() && Operations.All(i => i.IsValid(mcv)) && Operations.Length <= mcv.Net.ExecutionCyclesPerTransactionLimit &&
				(!mcv.Net.PoW || PoW.Length == PoWLength && Cryptography.Hash(mcv.FindRound(Expiration - Mcv.TransactionPlacingLifetime).Hash.Concat(PoW).ToArray()).Take(2).All(i => i == 0));
	}

 		public Transaction()
 		{
 		}

	public override string ToString()
	{
		return $"Id={Id}, Nid={Nid}, {Status}, Operations={{{Operations.Length}}}, Signer={Signer?.Bytes.ToHexPrefix()}, Expiration={Expiration}, Signature={Signature?.ToHexPrefix()}";
	}

	public void Sign(AccountKey signer, byte[] powhash)
	{
		Signer = signer;

		if(!Net.PoW || powhash.SequenceEqual(Net.Cryptography.ZeroHash))
		{
			PoW = new byte[PoWLength];
		}
		else
            {
            var r = new Random();
			var h = new byte[32];

			var x = new byte[32 + PoWLength];

			Array.Copy(powhash, x, 32);

			do
			{
				r.NextBytes(new Span<byte>(x, 32, PoWLength));
				
				h = Cryptography.Hash(x);
			
			}
			while(h[0] != 0 || h[1] != 0);
			
			PoW = x.Skip(32).ToArray();
            }

		Signature = Net.Cryptography.Sign(signer, Hashify());
	}

	public bool EqualBySignature(Transaction t)
	{
		return Signature != null && t.Signature != null && Signature.SequenceEqual(t.Signature);
	}

	public void AddOperation(Operation operation)
	{ 
		Operations = Operations.Append(operation).ToArray();
		operation.Transaction = this;
	}

	public byte[] Hashify()
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write((byte)Net.Zone);
		w.WriteUtf8(Net.Address);
		w.Write(Member);
		w.Write7BitEncodedInt(Nid);
		w.Write7BitEncodedInt(Expiration);
		//w.Write(STFee);
		w.Write(ECFee);
		w.WriteBytes(PoW);
		w.WriteBytes(Tag);
		w.Write(Operations, i => i.Write(w));

		return Cryptography.Hash(s.ToArray());
	}

 		public void	WriteConfirmed(BinaryWriter writer)
 		{
		writer.Write(Member);
		writer.Write(Signer);
		writer.Write7BitEncodedInt(Nid);
		writer.Write7BitEncodedInt64(ECFee);
		writer.WriteBytes(Tag);
		writer.Write(Operations, i =>{
										writer.Write(Net.Codes[i.GetType()]); 
										i.Write(writer); 
									 });
 		}
 		
 		public void	ReadConfirmed(BinaryReader reader)
 		{
		Status		= TransactionStatus.Confirmed;

		Member	= reader.Read<EntityId>();
		Signer		= reader.ReadAccount();
		Nid			= reader.Read7BitEncodedInt();
		ECFee		= reader.Read7BitEncodedInt64();
		Tag			= reader.ReadBytes();
 			Operations	= reader.ReadArray(() => {
 													var o = Net.Contructors[typeof(Operation)][reader.ReadByte()].Invoke(null) as Operation;
 													o.Transaction = this;
 													o.Read(reader); 
 													return o; 
 												});
 		}

 		public void	WriteForVote(BinaryWriter writer)
 		{
		writer.Write((byte)__ExpectedStatus);

		writer.Write(Member);
		writer.Write(Signature);
		writer.Write7BitEncodedInt(Nid);
		writer.Write7BitEncodedInt(Expiration);
		//writer.Write(STFee);
		writer.Write7BitEncodedInt64(ECFee);
		writer.Write(PoW);
		writer.WriteBytes(Tag);
		writer.Write(Operations, i => {
										writer.Write(Net.Codes[i.GetType()]); 
										i.Write(writer); 
									  });
 		}
 		
 		public void	ReadForVote(BinaryReader reader)
 		{
		__ExpectedStatus = (TransactionStatus)reader.ReadByte();

		Member	= reader.Read<EntityId>();
		Signature	= reader.ReadSignature();
		Nid			= reader.Read7BitEncodedInt();
		Expiration	= reader.Read7BitEncodedInt();
		//STFee		= reader.Read<Money>();
		ECFee		= reader.Read7BitEncodedInt64();
		PoW			= reader.ReadBytes(PoWLength);
		Tag			= reader.ReadBytes();
 			Operations	= reader.ReadArray(() => {
 													var o = Net.Contructors[typeof(Operation)][reader.ReadByte()].Invoke(null) as Operation;
 													//o.Placing		= PlacingStage.Confirmed;
 													o.Transaction	= this;
 													o.Read(reader); 
 													return o; 
 												});
 		}

	public void Write(BinaryWriter writer)
	{
		writer.Write((byte)__ExpectedStatus);
	
		writer.Write(Member);
		writer.Write(Signature);
		writer.Write7BitEncodedInt(Nid);
		writer.Write7BitEncodedInt(Expiration);
		//writer.Write(STFee);
		writer.Write7BitEncodedInt64(ECFee);
		writer.Write(PoW);
		writer.WriteBytes(Tag);
		writer.Write(Operations, i =>	{
											writer.Write(Net.Codes[i.GetType()]); 
											i.Write(writer); 
										});
	}

	public void Read(BinaryReader reader)
	{
		__ExpectedStatus = (TransactionStatus)reader.ReadByte();
	
		Member	= reader.Read<EntityId>();
		Signature	= reader.ReadSignature();
		Nid			= reader.Read7BitEncodedInt();
		Expiration	= reader.Read7BitEncodedInt();
		//STFee		= reader.Read<Money>();
		ECFee		= reader.Read7BitEncodedInt64();
		PoW			= reader.ReadBytes(PoWLength);
		Tag			= reader.ReadBytes();
		Operations	= reader.ReadArray(() => {
												var o = Net.Contructors[typeof(Operation)][reader.ReadByte()].Invoke(null) as Operation;
												o.Transaction = this;
												o.Read(reader); 
												return o; 
											});
	}
}
