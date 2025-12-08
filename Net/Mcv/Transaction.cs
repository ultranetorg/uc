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
	public bool						Successful => Error == null && Operations.Any() && Operations.All(i => i.Error == null);

#if IMMISSION
	public bool						EmissionOnly => Operations.All(i => i is Immission);
#endif

	public McvNet					Net;
	public Vote						Vote;
	public Round					Round;
	public AutoId					Member;
	public int						Expiration { get; set; }
	public byte[]					Tag;
	public long						Bonus;
	
	public long						EnergyConsumed;
	public byte[]					Signature { get; set; }

	AccountAddress					_Signer;
	public AccountAddress			Signer { get => _Signer ??= Net.Cryptography.AccountFrom(Signature, Hashify()); set => _Signer = value; }
	public TransactionStatus		Status;
	public IHomoPeer				Ppi;
	public Flow						Flow;
	public DateTime					Inquired;
	public string					Error;
	public ActionOnResult			ActionOnResult = ActionOnResult.DoNotCare;

	public bool Valid(Mcv mcv)
	{
		return	(Tag == null || Tag.Length <= TagLengthMax) &&
				Operations.Any() && Operations.All(i => i.IsValid(mcv.Net)) && Operations.Length <= mcv.Net.ExecutionCyclesPerTransactionLimit;
	}

 	public Transaction()
 	{
 	}

	public override string ToString()
	{
		return $"Id={Id}, Nid={Nid}, {Status}, Operations={{{Operations.Length}}}, Signer={Signer?.Bytes.ToHexPrefix()}, Expiration={Expiration}, Signature={Signature?.ToHexPrefix()}";
	}

	public void Sign(AccountKey signer)
	{
		Signer = signer;
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
		w.Write(Bonus);
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
										writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
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
 												var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
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
		writer.Write7BitEncodedInt64(Bonus);
		writer.WriteBytes(Tag);
		writer.Write(Sponsored);
		writer.Write(Operations, i => {
										writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
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
		Tag			= reader.ReadBytes();
		Sponsored	= reader.ReadBoolean();
 		Operations	= reader.ReadArray(() => {
 												var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
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
		writer.WriteBytes(Tag);
		writer.Write(Sponsored);
		writer.Write(Operations, i =>	{
											writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
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
		Tag			= reader.ReadBytes();
		Sponsored	= reader.ReadBoolean();
		Operations	= reader.ReadArray(() => {
												var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
												o.Transaction = this;
												o.Read(reader); 
												return o; 
											});
	}
}
