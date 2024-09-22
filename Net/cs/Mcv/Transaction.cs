namespace Uccs.Net
{
	public enum TransactionStatus
	{
		None, Pending, Accepted, Placed, FailedOrNotFound, Confirmed
	}

	public class Transaction : IBinarySerializable
	{
		const int						PoWLength = 16;
		const int						TagLengthMax = 1024;

		public int						Nid;
		public TransactionId			Id => Round != null && Round.Confirmed ? new (Round.Id, Array.IndexOf(Round.ConsensusTransactions, this)) : default;
		public Operation[]				Operations = {};
		public bool						Successful => Operations.Any() && Operations.All(i => i.Error == null);

#if IMMISSION
		public bool						EmissionOnly => Operations.All(i => i is Immission);
#endif

		public Mcv						Mcv;
		public Vote						Vote;
		public Round					Round;
		public EntityId					Generator;
		public int						Expiration;
		public byte[]					PoW;
		public byte[]					Tag;
		//public Money					STFee;
		public long						ECFee;
		
		public long						ECSpent;
		public long						BYReward;
		//public long					ECReward;
		public byte[]					Signature;

		private AccountAddress			_Signer;
		public AccountAddress			Signer { get => _Signer ??= Zone.Cryptography.AccountFrom(Signature, Hashify()); set => _Signer = value; }
		public McvZone					Zone;
		public TransactionStatus		Status;
		public IPeer					Rdi;
		public Flow						Flow;
		public TransactionStatus		__ExpectedStatus = TransactionStatus.None;

		public bool Valid(Mcv mcv)
		{
			return	(Tag == null || Tag.Length <= TagLengthMax) &&
					Operations.Any() && Operations.All(i => i.IsValid(mcv)) && Operations.Length <= mcv.Zone.ExecutionCyclesPerTransactionLimit &&
					(!mcv.Zone.PoW || PoW.Length == PoWLength && Cryptography.Hash(mcv.FindRound(Expiration - Mcv.TransactionPlacingLifetime).Hash.Concat(PoW).ToArray()).Take(2).All(i => i == 0));
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

			if(!Zone.PoW || powhash.SequenceEqual(Zone.Cryptography.ZeroHash))
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

			Signature = Zone.Cryptography.Sign(signer, Hashify());
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

			w.Write(Zone.Id);
			w.Write(Generator);
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
			writer.Write(Generator);
			writer.Write(Signer);
			writer.Write7BitEncodedInt(Nid);
			writer.Write7BitEncodedInt64(ECFee);
			writer.WriteBytes(Tag);
			writer.Write(Operations, i =>{
											writer.Write(ITypeCode.Codes[i.GetType()]); 
											i.Write(writer); 
										 });
 		}
 		
 		public void	ReadConfirmed(BinaryReader reader)
 		{
			Status		= TransactionStatus.Confirmed;

			Generator	= reader.Read<EntityId>();
			Signer		= reader.ReadAccount();
			Nid			= reader.Read7BitEncodedInt();
			ECFee		= reader.Read7BitEncodedInt64();
			Tag			= reader.ReadBytes();
 			Operations	= reader.ReadArray(() => {
 													var o = Mcv.CreateOperation(reader.ReadByte());
 													o.Transaction = this;
 													o.Read(reader); 
 													return o; 
 												});
 		}

 		public void	WriteForVote(BinaryWriter writer)
 		{
			writer.Write((byte)__ExpectedStatus);

			writer.Write(Generator);
			writer.Write(Signature);
			writer.Write7BitEncodedInt(Nid);
			writer.Write7BitEncodedInt(Expiration);
			//writer.Write(STFee);
			writer.Write7BitEncodedInt64(ECFee);
			writer.Write(PoW);
			writer.WriteBytes(Tag);
			writer.Write(Operations, i => {
											writer.Write(ITypeCode.Codes[i.GetType()]); 
											i.Write(writer); 
										  });
 		}
 		
 		public void	ReadForVote(BinaryReader reader)
 		{
			__ExpectedStatus = (TransactionStatus)reader.ReadByte();

			Generator	= reader.Read<EntityId>();
			Signature	= reader.ReadSignature();
			Nid			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
			//STFee		= reader.Read<Money>();
			ECFee		= reader.Read7BitEncodedInt64();
			PoW			= reader.ReadBytes(PoWLength);
			Tag			= reader.ReadBytes();
 			Operations	= reader.ReadArray(() => {
 													var o = Mcv.CreateOperation(reader.ReadByte());
 													//o.Placing		= PlacingStage.Confirmed;
 													o.Transaction	= this;
 													o.Read(reader); 
 													return o; 
 												});
 		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)__ExpectedStatus);
		
			writer.Write(Generator);
			writer.Write(Signature);
			writer.Write7BitEncodedInt(Nid);
			writer.Write7BitEncodedInt(Expiration);
			//writer.Write(STFee);
			writer.Write7BitEncodedInt64(ECFee);
			writer.Write(PoW);
			writer.WriteBytes(Tag);
			writer.Write(Operations, i =>	{
												writer.Write(ITypeCode.Codes[i.GetType()]); 
												i.Write(writer); 
											});
		}

		public void Read(BinaryReader reader)
		{
			__ExpectedStatus = (TransactionStatus)reader.ReadByte();
		
			Generator	= reader.Read<EntityId>();
			Signature	= reader.ReadSignature();
			Nid			= reader.Read7BitEncodedInt();
			Expiration	= reader.Read7BitEncodedInt();
			//STFee		= reader.Read<Money>();
			ECFee		= reader.Read7BitEncodedInt64();
			PoW			= reader.ReadBytes(PoWLength);
			Tag			= reader.ReadBytes();
			Operations	= reader.ReadArray(() => {
													var o = Mcv.CreateOperation(reader.ReadByte());
													o.Transaction = this;
													o.Read(reader); 
													return o; 
												});
		}
	}
}
