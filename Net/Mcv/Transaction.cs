using System.Text.Json.Serialization;

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

	public string					User { get; set; }
	public int						Nonce { get; set; }
	//public bool						UserCreationRequest => User != Mcv.GodName && User[0] == '?';
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
	public AccountAddress			Signer { get => _Signer ??= Net.Cryptography.AccountFrom(Signature, Hashify()); protected set => _Signer = value; }
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
		return $"Id={Id}, Nid={Nonce}, {Status}, Operations={{{Operations.Length}}}, Signer={Signer?.Bytes.ToHexPrefix()}, Expiration={Expiration}, Signature={Signature?.ToHexPrefix()}";
	}

	public void Sign(AccountKey signer)
	{
		Signer = signer.Address;
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

		w.Write(Net.Zone);
		w.WriteUtf8(Net.Address);
		w.WriteUtf8(User);
		w.Write(Member);
		w.Write7BitEncodedInt(Nonce);
		w.Write7BitEncodedInt(Expiration);
		w.Write(Bonus);
		w.WriteBytes(Tag);
		w.Write(Operations, i => i.Write(w));

		return Cryptography.Hash(s.ToArray());
	}

 	public void	WriteConfirmed(BinaryWriter writer)
 	{
		writer.WriteUtf8(User);
		//writer.Write(Member);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt64(Bonus);
		writer.Write(Operations, i =>{
										writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
										i.Write(writer); 
									 });
 	}
 		
 	public void	ReadConfirmed(BinaryReader reader)
 	{
		Status		= TransactionStatus.Confirmed;

		User		= reader.ReadUtf8();
		//Member		= reader.Read<AutoId>();
		Nonce		= reader.Read7BitEncodedInt();
		Bonus		= reader.Read7BitEncodedInt64();
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
		writer.Write(Signature);

		writer.WriteUtf8(User);
		writer.Write(Member);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Bonus);
		writer.WriteBytes(Tag);
		writer.Write(Operations, i =>	{
											writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
											i.Write(writer); 
										});
	}
 		
	public void	ReadForVote(BinaryReader reader)
	{
		ActionOnResult		= reader.Read<ActionOnResult>();
		Signature			= reader.ReadSignature();

		User				= reader.ReadUtf8();
		Member				= reader.Read<AutoId>();
		Nonce				= reader.Read7BitEncodedInt();
		Expiration			= reader.Read7BitEncodedInt();
		Bonus				= reader.Read7BitEncodedInt64();
		Tag					= reader.ReadBytes();
 		Operations			= reader.ReadArray(() => {
 													 	var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
 													 	o.Transaction	= this;
 													 	o.Read(reader); 
 													 	return o; 
 													 });
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(ActionOnResult);
		writer.Write(Signature);
	
		writer.WriteUtf8(User);
		writer.Write(Member);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Bonus);
		writer.WriteBytes(Tag);
		writer.Write(Operations, i =>	{
											writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
											i.Write(writer); 
										});
	}

	public void Read(BinaryReader reader)
	{
		ActionOnResult		= reader.Read<ActionOnResult>();
		Signature			= reader.ReadSignature();
	
		User				= reader.ReadUtf8();
		Member				= reader.Read<AutoId>();
		Nonce				= reader.Read7BitEncodedInt();
		Expiration			= reader.Read7BitEncodedInt();
		Bonus				= reader.Read7BitEncodedInt64();
		Tag					= reader.ReadBytes();
		Operations			= reader.ReadArray(() => {
														var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
														o.Transaction = this;
														o.Read(reader); 
														return o; 
													});
	}

	public static byte[] Export(Net net, Operation[] operations, string user)
	{
		throw new NotImplementedException("USER AND SIGNATURE NEEDED");

		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write(operations, i => {
									w.Write(net.Constructor.TypeToCode(i.GetType()));
									i.Write(w); 
								 });
		w.WriteUtf8(user);

		return s.ToArray();
	}

	public static void Import(byte[] raw, Constructor constructor, out Operation[] operations, out string user)
	{
		throw new NotImplementedException("USER AND SIGNATURE NEEDED");

		var r = new BinaryReader(new MemoryStream(raw));

		operations = r.ReadArray(() =>	{
 											var o = constructor.Construct(typeof(Operation), r.ReadUInt32()) as Operation;
 											o.Read(r); 
 											return o;
										});
		user = r.ReadUtf8();
	}
}
