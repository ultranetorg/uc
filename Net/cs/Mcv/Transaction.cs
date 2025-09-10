namespace Uccs.Net;

public enum TransactionStatus : byte
{
	None, Pending, Accepted, Placed, FailedOrNotFound, Confirmed
}

public enum ActionOnResult : byte
{
	DoNotCare, CancelOnFailure, RetryUntilConfirmed, ExpectFailure
}

public class Transaction : IBinarySerializable
{
	public const int				PowLength = 16;
	public const int				TagLengthMax = 1024;

	public int						Nid { get; set; }
	public bool						Sponsored { get; set; }
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
	public AutoId					Member;
	public int						Expiration { get; set; }
	public byte[]					PoW;
	public byte[]					Tag;
	public long						Bonus;
	
	public long						EnergyConsumed;
	public byte[]					Signature { get; set; }

	private AccountAddress			_Signer;
	public AccountAddress			Signer { get => _Signer ??= Net.Cryptography.AccountFrom(Signature, Hashify()); set => _Signer = value; }
	public TransactionStatus		Status;
	public IPeer					Rdi;
	public Flow						Flow;
	public DateTime					Inquired;
	public ActionOnResult			ActionOnResult = ActionOnResult.DoNotCare;

	public bool Valid(Mcv mcv)
	{
		return	(Tag == null || Tag.Length <= TagLengthMax) &&
				Operations.Any() && Operations.All(i => i.IsValid(mcv.Net)) && Operations.Length <= mcv.Net.ExecutionCyclesPerTransactionLimit &&
				(!mcv.Net.PoW || PoW.Length == PowLength && Cryptography.Hash(mcv.FindRound(Expiration - Mcv.TransactionPlacingLifetime).Hash.Concat(PoW).ToArray()).Take(3).All(i => i == 0));
	}

 	public Transaction()
 	{
 	}

	public override string ToString()
	{
		return $"Id={Id}, Nid={Nid}, {Status}, Operations={{{Operations.Length}}}, Signer={Signer?.Bytes.ToHexPrefix()}, Expiration={Expiration}, Signature={Signature?.ToHexPrefix()}";
	}

	public byte[] Hashify(byte[] powhash)
	{
		if(!Net.PoW || powhash.SequenceEqual(Net.Cryptography.ZeroHash))
		{
			PoW = new byte[PowLength];
		}
		else
		{
			var r = new Random();
			var h = new byte[32];
			var x = new byte[32 + PowLength];

			Array.Copy(powhash, x, 32);

			do
			{
				r.NextBytes(new Span<byte>(x, 32, PowLength));

				h = Cryptography.Hash(x);

			}
			while(h[0] != 0 || h[1] != 0 || h[2] != 0);

			PoW = x.Skip(32).ToArray();
		}

		return Hashify();
	}

	public void Sign(AccountKey signer, byte[] powhash)
	{
		Signer = signer;
		Signature = Net.Cryptography.Sign(signer, Hashify(powhash));
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
		w.Write(Bonus);
		w.WriteBytes(PoW);
		w.WriteBytes(Tag);
		w.Write(Sponsored);
		w.Write(Operations, i => i.Write(w));

		return Cryptography.Hash(s.ToArray());
	}

 	public void	WriteConfirmed(BinaryWriter writer)
 	{
		writer.Write(Member);
		writer.Write(Signer);
		writer.Write7BitEncodedInt(Nid);
		writer.Write7BitEncodedInt64(Bonus);
		writer.WriteBytes(Tag);
		writer.Write(Sponsored);
		writer.Write(Operations, i =>{
										writer.Write(Net.Codes[i.GetType()]); 
										i.Write(writer); 
									 });
 	}
 		
 	public void	ReadConfirmed(BinaryReader reader)
 	{
		Status		= TransactionStatus.Confirmed;

		Member		= reader.Read<AutoId>();
		Signer		= reader.ReadAccount();
		Nid			= reader.Read7BitEncodedInt();
		Bonus		= reader.Read7BitEncodedInt64();
		Tag			= reader.ReadBytes();
		Sponsored	= reader.ReadBoolean();
 		Operations	= reader.ReadArray(() => {
 												var o = Net.Contructors[typeof(Operation)][reader.ReadUInt32()].Invoke(null) as Operation;
 												o.Transaction = this;
 												o.Read(reader); 
 												return o; 
 											});
 	}

	public void	WriteForVote(BinaryWriter writer)
	{
		writer.Write(ActionOnResult);

		writer.Write(Member);
		writer.Write(Signature);
		writer.Write7BitEncodedInt(Nid);
		writer.Write7BitEncodedInt(Expiration);
		//writer.Write(STFee);
		writer.Write7BitEncodedInt64(Bonus);
		writer.Write(PoW);
		writer.WriteBytes(Tag);
		writer.Write(Sponsored);
		writer.Write(Operations, i => {
										writer.Write(Net.Codes[i.GetType()]); 
										i.Write(writer); 
									  });
	}
 		
	public void	ReadForVote(BinaryReader reader)
	{
		ActionOnResult = reader.Read<ActionOnResult>();

		Member		= reader.Read<AutoId>();
		Signature	= reader.ReadSignature();
		Nid			= reader.Read7BitEncodedInt();
		Expiration	= reader.Read7BitEncodedInt();
		Bonus		= reader.Read7BitEncodedInt64();
		PoW			= reader.ReadBytes(PowLength);
		Tag			= reader.ReadBytes();
		Sponsored	= reader.ReadBoolean();
 		Operations	= reader.ReadArray(() => {
 												var o = Net.Contructors[typeof(Operation)][reader.ReadUInt32()].Invoke(null) as Operation;
 												o.Transaction	= this;
 												o.Read(reader); 
 												return o; 
 											});
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(ActionOnResult);
	
		writer.Write(Member);
		writer.Write(Signature);
		writer.Write7BitEncodedInt(Nid);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Bonus);
		writer.Write(PoW);
		writer.WriteBytes(Tag);
		writer.Write(Sponsored);
		writer.Write(Operations, i =>	{
											writer.Write(Net.Codes[i.GetType()]); 
											i.Write(writer); 
										});
	}

	public void Read(BinaryReader reader)
	{
		ActionOnResult = reader.Read<ActionOnResult>();
	
		Member		= reader.Read<AutoId>();
		Signature	= reader.ReadSignature();
		Nid			= reader.Read7BitEncodedInt();
		Expiration	= reader.Read7BitEncodedInt();
		Bonus		= reader.Read7BitEncodedInt64();
		PoW			= reader.ReadBytes(PowLength);
		Tag			= reader.ReadBytes();
		Sponsored	= reader.ReadBoolean();
		Operations	= reader.ReadArray(() => {
												var o = Net.Contructors[typeof(Operation)][reader.ReadUInt32()].Invoke(null) as Operation;
												o.Transaction = this;
												o.Read(reader); 
												return o; 
											});
	}
}
