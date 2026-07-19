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

	public TransactionId			Id;
	public Operation[]				Operations = {};
	public string					User { get; set; }
	public int						Nonce { get; set; }
	public int						Expiration { get; set; }
	public byte[]					Signature { get; set; }

	public Vote						Vote;
	public Round					Round;
	//public AutoId					Member;
	public byte[]					Tag;
	public long						Boost;
	
	public int						EnergyConsumed;

	//AccountAddress					_Signer;
	//public AccountAddress			Signer { get => _Signer ??= Net.Cryptography.AccountFrom(Signature, Hashify()); set => _Signer = value; }
	//public bool						IsSignerSet => _Signer != null;
	public TransactionStatus		Status;
	public int						Length;
	public string					Error;
	public string					OverallError => Error ?? Operations.FirstOrDefault(i => i.Error != null)?.Error;
	public IHomoPeer				Peer;
	public Flow						Flow;
	public DateTime					Inquired;
	public byte[]					Session;
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
		return $"User={User}, Nonce={Nonce}, {Status}, Operations={Operations.FirstOrDefault()?.ToString() ?? $"{{{Operations.Length}}}"}, Expiration={Expiration}, Signature={Signature?.ToHexPrefix()}";
	}

	public void Sign(McvNet net, SecretKey signer)
	{
		//Signer = signer.Address;
		Signature = net.Cryptography.Sign(signer, Hashify(net));
	}

	public void AddOperation(Operation operation)
	{ 
		Operations = [..Operations, operation];
		operation.Transaction = this;
	}

	public byte[] Hashify(McvNet net)
	{
		var s = new Blake2Stream();
		var w = new Writer(s, net.Constructor);

		w.Write(net.Zone);
		w.WriteUtf8(net.Address);
		//w.Write(Member);
		w.WriteASCII(User);
		w.Write7BitEncodedInt(Nonce);
		w.Write7BitEncodedInt(Expiration);
		w.Write(Boost);
		w.WriteBytes(Tag);
		w.WriteVirtual(Operations);

		return s.Hash;
	}

	#if DEBUG
	static readonly long __Checker = 0x0123456789ABCDEF;
	#endif

 	public void	WriteConfirmed(Writer writer)
 	{
		writer.WriteASCII(User);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Boost);
		writer.WriteBytes(Tag);
		writer.WriteVirtual(Operations);
		writer.WriteBytes(Signature);

		#if DEBUG
		writer.Write(__Checker);
		#endif
 	}
 		
 	public void	ReadConfirmed(Reader reader)
 	{
		Status		= TransactionStatus.Confirmed;

		User		= reader.ReadASCII();
		Nonce		= reader.Read7BitEncodedInt();
		Expiration	= reader.Read7BitEncodedInt();
		Boost		= reader.Read7BitEncodedInt64();
		Tag			= reader.ReadBytes();
 		Operations	= reader.ReadArray(() => {
 												var o = reader.ReadVirtual<Operation>();
 												o.Transaction = this;
 												return o; 

 											});
		Signature	= reader.ReadBytes();

		#if DEBUG
		if(reader.ReadInt64() != __Checker)
			throw new IntegrityException();
		#endif
 	}

	public void	WriteForVote(Writer writer)
	{
		writer.Write(ActionOnResult);

		writer.WriteUtf8(User);
		//writer.Write(Member);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Boost);
		writer.WriteBytes(Tag);
		writer.WriteVirtual(Operations);
		writer.Write(Signature);

		#if DEBUG
		writer.Write(__Checker);
		#endif
	}
 		
	public void	ReadForVote(Reader reader)
	{
		ActionOnResult		= reader.Read<ActionOnResult>();

		User				= reader.ReadUtf8();
		//Member				= reader.Read<AutoId>();
		Nonce				= reader.Read7BitEncodedInt();
		Expiration			= reader.Read7BitEncodedInt();
		Boost				= reader.Read7BitEncodedInt64();
		Tag					= reader.ReadBytes();
 		Operations			= reader.ReadArray(() => {
 													 	var o = reader.ReadVirtual<Operation>();
 													 	o.Transaction	= this;
 													 	return o; 
 													 });
		Signature			= reader.ReadSignature();

		#if DEBUG
		if(reader.ReadInt64() != __Checker)
			throw new IntegrityException();
		#endif
	}

	public void Write(Writer writer)
	{
		writer.Write(ActionOnResult);
		writer.Write(Signature);
	
		writer.WriteUtf8(User);
		//writer.Write(Member);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Boost);
		writer.WriteBytes(Tag);
		writer.WriteVirtual(Operations);

		#if DEBUG
		writer.Write(__Checker);
		#endif
	}

	public void Read(Reader reader)
	{
		ActionOnResult		= reader.Read<ActionOnResult>();
		Signature			= reader.ReadSignature();
	
		User				= reader.ReadUtf8();
		//Member				= reader.Read<AutoId>();
		Nonce				= reader.Read7BitEncodedInt();
		Expiration			= reader.Read7BitEncodedInt();
		Boost				= reader.Read7BitEncodedInt64();
		Tag					= reader.ReadBytes();
		Operations			= reader.ReadArray(() => {
														var o = reader.ReadVirtual<Operation>();
														o.Transaction = this;
														return o; 
													});

		#if DEBUG
		if(reader.ReadInt64() != __Checker)
			throw new IntegrityException();
		#endif
	}

	//public static byte[] Export(Net net, Operation[] operations, string user, Func<MemoryStream, Writer, byte[]> sign)
	//{
	//	var s = new MemoryStream();
	//	var w = new Writer(s, net.Constructor);
	//
	//	w.WriteVirtual(operations);
	//	w.WriteUtf8(user);
	//	w.WriteBytes(sign(s, w));
	//
	//	return s.ToArray();
	//}
	//
	//public static void Import(McvNet net, byte[] raw, Constructor constructor, out Operation[] operations, out string user)
	//{
	//	var r = new Reader(raw, net.Constructor);
	//
	//	operations = r.ReadArrayVirtual<Operation>();
	//	user = r.ReadUtf8();
	//	signiture = r.ReadBytes()
	//	//account = net.Cryptography.AccountFrom(r.ReadSignature(), Cryptography.Hash(raw.AsSpan(0, raw.Length - Cryptography.SignatureLength)));
	//}
}
