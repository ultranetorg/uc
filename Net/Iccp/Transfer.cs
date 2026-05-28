namespace Uccs.Net;

public class TransferRequestIcca : IccpArgumentation
{
	public byte[]			Hash { get; set; }
	public byte[]			Signature { get; set; }
	//public Endpoint[]		Peers { get; set; }
	//public IccOperation[]	Operations { get; set; }

	///public override string ToString()
	///{
	///	return $"Nonce={Nonce}, Operations={Operations.Length}, Peers={Peers.Length}";
	///}

	public override void Read(Reader reader)
	{
		Hash		= reader.ReadBytes(Cryptography.HashLength);
		Signature	= reader.ReadBytes(Cryptography.SignatureLength);
		//Peers		= reader.ReadArray<Endpoint>();
 		///Operations	= reader.ReadArray(() => {
 		///										var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
 		///										o.Transaction = this;
 		///										o.Read(reader); 
 		///										return o; 
 		///									 });
	}

	public override void Write(Writer writer)
	{
		writer.Write(Hash);
		writer.Write(Signature);
		//writer.Write(Peers);
		///writer.Write(Operations, i =>	{
		///									writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
		///									i.Write(writer); 
		///								});
	}
}

public class LastOutgoingTransferIcca : IccpArgumentation
{
	//public byte[]	Hash { get; set; }
	//public IccOperation[]	Operations { get; set; }

	public override void Read(Reader reader)
	{
		//Hash = reader.ReadBytes();
	}

	public override void Write(Writer writer)
	{
		//writer.WriteBytes(Hash);
	}
}

public class LastOutgoingTransferIccr : IccpResult
{
	public IccpTransfer		Transfer;

	public override void Read(Reader reader)
	{
		///Nonce		= reader.Read7BitEncodedInt();
		///Peers		= reader.ReadArray<Endpoint>();
 		///Operations	= reader.ReadArray(() => {
 		///										var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
 		///										o.Transaction = this;
 		///										o.Read(reader); 
 		///										return o; 
 		///									 });
	}

	public override void Write(Writer writer)
	{
		///writer.Write7BitEncodedInt(Nonce);
		///writer.Write(Peers);
		///writer.Write(Operations, i =>	{
		///									writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
		///									i.Write(writer); 
		///								});
	}
}

public class LastIncomingTransferIcca : IccpArgumentation
{
}

public class LastIncomingTransferIccr : IccpResult
{
	public int					Id { get; set; }
	public IccpTransferResult	Result { get; set; }

	public override void Read(Reader reader)
	{
		Id		= reader.Read7BitEncodedInt();
		Result	= reader.Read<IccpTransferResult>();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(Result);
	}
}
