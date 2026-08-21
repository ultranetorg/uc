using System.Numerics;
using System.Text;

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
	public const int				TagLengthMax = 256;

	public TransactionId			Id;
	public Operation[]				Operations = {};
	public string					User { get; set; }
	public int						Nonce { get; set; }
	public int						Expiration { get; set; }
	public byte[]					Signature { get; set; }
	public byte[]					Tag { get; set; }
	public long						Boost { get; set; }
	public byte[]					Pow { get; set; }
	
	public int						EnergyConsumed;

	public TransactionStatus		Status;
	public Vote						Vote;
	public Round					Round;
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
		return	Uccs.Net.User.IsNameValid(User) &&
				(Tag == null || Tag.Length <= TagLengthMax) &&
				(Pow == null || Pow.Length == McvNet.PoWLength) &&
				Operations.Any() && Operations.All(i => i.IsValid(mcv.Net)) && Operations.Length <= mcv.Net.ExecutionCyclesPerTransactionLimit;
	}

 	public Transaction()
 	{
 	}

	public override string ToString()
	{
		return $"{nameof(User)}={User}, {nameof(Nonce)}={Nonce}, {Status}, {nameof(Operations)}={(Operations != null ? (Operations.Length == 1 ?  $"{Operations.First()}" : $"{{{Operations.Length}}}") : null)}, {nameof(Expiration)}={Expiration}, {nameof(Signature)}={Signature?.ToHexPrefix()}";
	}

	public void Sign(McvNet net, SecretKey signer, SigningFeatures deterministic)
	{
		//Signer = signer.Address;
		Signature = net.Cryptography.Sign(signer, Hashify(net), deterministic);
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
		w.WriteASCII(net.Address);
		w.WriteASCII(User);
		w.Write(Nonce);
		w.Write(Expiration);
		w.Write(Boost);
		w.WriteBytes(Pow);
		w.WriteBytes(Tag);
		w.WriteVirtual(Operations);

		return s.Hash;
	}

	internal void Dump(McvNet net, string tab, StringBuilder builder)
	{
		builder.Append(tab);		builder.AppendLine($"Zone: {net.Zone}" );
		builder.Append(tab);		builder.AppendLine($"Address: {net.Address}" );
		builder.Append(tab);		builder.AppendLine($"Nonce: {Nonce}" );
		builder.Append(tab);		builder.AppendLine($"Expiration: {Expiration}" );
		builder.Append(tab);		builder.AppendLine($"Boost: {Boost}" );
		builder.Append(tab);		builder.AppendLine($"Pow: {Pow?.ToHex()}" );
		builder.Append(tab);		builder.AppendLine($"Tag: {Tag?.ToHex()}" );

		builder.Append(tab);		builder.AppendLine($"{nameof(Operations)}: ");
		foreach(var i in Operations)
			builder.AppendLine($"{tab}\t{i}");
	}

	#if DEBUG
	static readonly long __Checker = 0x0123456789ABCDEF;
	#endif

	public void Write(Writer writer)
	{
		writer.Write(ActionOnResult);
	
		WriteConfirmed(writer);

		#if DEBUG
		writer.Write(__Checker);
		#endif
	}

	public void Read(Reader reader)
	{
		ActionOnResult		= reader.Read<ActionOnResult>();
	
		ReadConfirmed(reader);

		#if DEBUG
		if(reader.ReadInt64() != __Checker)
			throw new IntegrityException();
		#endif
	}

 	public void	WriteConfirmed(Writer writer)
 	{
		writer.WriteASCII(User);
		writer.Write7BitEncodedInt(Nonce);
		writer.Write7BitEncodedInt(Expiration);
		writer.Write7BitEncodedInt64(Boost);
		writer.WriteBytes(Tag);
		writer.WriteBytes(Pow);
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
		Pow			= reader.ReadBytes();
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

//	public void	WriteForVote(Writer writer)
//	{
//		writer.Write(ActionOnResult);
//
//		writer.WriteASCII(User);
//		//writer.Write(Member);
//		writer.Write7BitEncodedInt(Nonce);
//		writer.Write7BitEncodedInt(Expiration);
//		writer.Write7BitEncodedInt64(Boost);
//		writer.WriteBytes(Tag);
//		writer.WriteVirtual(Operations);
//		writer.Write(Signature);
//
//		#if DEBUG
//		writer.Write(__Checker);
//		#endif
//	}
// 		
//	public void	ReadForVote(Reader reader)
//	{
//		ActionOnResult		= reader.Read<ActionOnResult>();
//
//		User				= reader.ReadASCII();
//		//Member			= reader.Read<AutoId>();
//		Nonce				= reader.Read7BitEncodedInt();
//		Expiration			= reader.Read7BitEncodedInt();
//		Boost				= reader.Read7BitEncodedInt64();
//		Tag					= reader.ReadBytes();
// 		Operations			= reader.ReadArray(() => {
// 													 	var o = reader.ReadVirtual<Operation>();
// 													 	o.Transaction	= this;
// 													 	return o; 
// 													 });
//		Signature			= reader.ReadSignature();
//
//		#if DEBUG
//		if(reader.ReadInt64() != __Checker)
//			throw new IntegrityException();
//		#endif
//	}

	public void CreatePow(McvNode node)
	{
		Pow = null;
			
		if(node.Net.PoWDifficulity > 0)
		{
			var s = node.Peering.Call(new StampPpc {}, Flow);
			
			var ts = Enumerable	.Range(0, Environment.ProcessorCount)
								.Select(i => node.CreateThread(() => { 
																		var b = new byte[McvNet.PoWLength];
																		var a = Blake2Fast.Blake2b.CreateHashAlgorithm(McvNet.PoWLength);
																		var r = new Random();
																	 
																	 	while(Flow.Active && Pow == null)
																	 	{
																	 		r.NextBytes(b);
																	 		var h = a.ComputeHash([..s.GraphHash, ..b]);
																	 
																	 		var f = h.Sum(i => BitOperations.PopCount(i));
																	 
																	 		if(f >= node.Net.PoWDifficulity)
																	 		{
																	 			Pow = b;
																	 		}
																	 	}
																	 })).ToArray();
			foreach(var i in ts)
				i.Start();
			
			while(Pow == null)
				Thread.Sleep(100);
		} 
		else /// simulation
		{
			Pow = new byte[32];
		}
	}
}
