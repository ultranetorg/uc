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

	public string					Application { get; set; } /// for API

	public McvNet					Net;
	public Vote						Vote;
	public Round					Round;
	public AutoId					Member;
	public byte[]					Tag;
	public long						Boost;
	
	public long						EnergyConsumed;

	//AccountAddress					_Signer;
	//public AccountAddress			Signer { get => _Signer ??= Net.Cryptography.AccountFrom(Signature, Hashify()); set => _Signer = value; }
	//public bool						IsSignerSet => _Signer != null;
	public TransactionStatus		Status;
	public string					Error;
	public string					OverallError => Error ?? Operations.FirstOrDefault(i => i.Error != null)?.Error;
	public IHomoPeer				Ppi;
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

	public void Sign(AccountKey signer)
	{
		//Signer = signer.Address;
		Signature = Net.Cryptography.Sign(signer, Hashify());
	}

	public void AddOperation(Operation operation)
	{ 
		Operations = [..Operations, operation];
		operation.Transaction = this;
	}

	public byte[] Hashify()
	{
		var s = new Blake2Stream();
		var w = new Writer(s);

		w.Write(Net.Zone);
		w.WriteUtf8(Net.Address);
		w.WriteASCII(User);
		w.Write(Member);
		w.Write7BitEncodedInt(Nonce);
		w.Write7BitEncodedInt(Expiration);
		w.Write(Boost);
		w.WriteBytes(Tag);
		w.Write(Operations, i => i.Write(w));

		return s.Hash;
	}

 	public void	WriteConfirmed(Writer writer)
 	{
		writer.WriteASCII(User);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt64(Boost);
		writer.Write(Operations, i =>{
										writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
										i.Write(writer); 
									 });
		writer.Write(Member); /// Need  for migrations
		
		///if(Operations.Any(i => i is UserFreeCreation))
		//	writer.Write(Signer); /// and for DomainMigratation
 	}
 		
 	public void	ReadConfirmed(Reader reader)
 	{
		Status		= TransactionStatus.Confirmed;

		User		= reader.ReadASCII();
		Nonce		= reader.Read7BitEncodedInt();
		Boost		= reader.Read7BitEncodedInt64();
 		Operations	= reader.ReadArray(() => {
 												var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
 												o.Transaction = this;
 												o.Read(reader); 
 												return o; 
 											});
		Member		= reader.Read<AutoId>(); /// Need  for migrations

		///if(Operations.Any(i => i is UserFreeCreation)) 
		//	Signer = reader.Read<AccountAddress>(); /// and for DomainMigratation
 	}

	public void	WriteForVote(Writer writer)
	{
		writer.Write(ActionOnResult);
		writer.Write(Signature);

		writer.WriteUtf8(User);
		writer.Write(Member);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Boost);
		writer.WriteBytes(Tag);
		writer.Write(Operations, i =>	{
											writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
											i.Write(writer); 
										});
	}
 		
	public void	ReadForVote(Reader reader)
	{
		ActionOnResult		= reader.Read<ActionOnResult>();
		Signature			= reader.ReadSignature();

		User				= reader.ReadUtf8();
		Member				= reader.Read<AutoId>();
		Nonce				= reader.Read7BitEncodedInt();
		Expiration			= reader.Read7BitEncodedInt();
		Boost				= reader.Read7BitEncodedInt64();
		Tag					= reader.ReadBytes();
 		Operations			= reader.ReadArray(() => {
 													 	var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
 													 	o.Transaction	= this;
 													 	o.Read(reader); 
 													 	return o; 
 													 });
	}

	public void Write(Writer writer)
	{
		writer.Write(ActionOnResult);
		writer.Write(Signature);
	
		writer.WriteUtf8(User);
		writer.Write(Member);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Boost);
		writer.WriteBytes(Tag);
		writer.Write(Operations, i =>	{
											writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
											i.Write(writer); 
										});
	}

	public void Read(Reader reader)
	{
		ActionOnResult		= reader.Read<ActionOnResult>();
		Signature			= reader.ReadSignature();
	
		User				= reader.ReadUtf8();
		Member				= reader.Read<AutoId>();
		Nonce				= reader.Read7BitEncodedInt();
		Expiration			= reader.Read7BitEncodedInt();
		Boost				= reader.Read7BitEncodedInt64();
		Tag					= reader.ReadBytes();
		Operations			= reader.ReadArray(() => {
														var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
														o.Transaction = this;
														o.Read(reader); 
														return o; 
													});
	}

	public static byte[] Export(Net net, Operation[] operations, string user, Func<MemoryStream, Writer, byte[]> sign)
	{
		var s = new MemoryStream();
		var w = new Writer(s);

		w.Write(operations, i => {
									w.Write(net.Constructor.TypeToCode(i.GetType()));
									i.Write(w); 
								 });
		w.WriteUtf8(user);
		w.WriteBytes(sign(s, w));

		return s.ToArray();
	}

	public static void Import(McvNet net, byte[] raw, Constructor constructor, out Operation[] operations, out string user)
	{
		var r = new Reader(raw);

		operations = r.ReadArray(() =>	{
 											var o = constructor.Construct(typeof(Operation), r.ReadUInt32()) as Operation;
 											o.Read(r); 
 											return o;
										});
		user = r.ReadUtf8();
		//account = net.Cryptography.AccountFrom(r.ReadSignature(), Cryptography.Hash(raw.AsSpan(0, raw.Length - Cryptography.SignatureLength)));
	}
}
