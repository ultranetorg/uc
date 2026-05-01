namespace Uccs.Net;

public abstract class IccTransaction : IBinarySerializable  
{
	public string			ToNet { get; set; }
	
	public abstract void	OutgoingExecute(Execution execution);
	public abstract void	IncomingExecute(Execution execution);

	public void Write(Writer writer)
	{
		writer.WriteASCII(ToNet);
	}

	public void Read(Reader reader)
	{
		ToNet = reader.ReadASCII();
	}
}
// 
// public class IccTransaction : IBinarySerializable
// {
// 	public IccOperation	Operations;
// 
// 	public void Write(Writer writer)
// 	{
// 	}
// 
// 	public void Read(Reader reader)
// 	{
// 	}
// }


public class IccTransfer : IBinarySerializable
{
	public string				From;
	Constructor					Constructor;

	public int					Id { get; set; }
	public IccTransaction[]		Transactions { get; set; }
	public byte[]				Hash => _Hash ??= Cryptography.Hash(Transactions);
	byte[]						_Hash;

	public IccTransfer(Constructor constructor)
	{
		Constructor = constructor;
	}

	public void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(Transactions, i =>	{
											writer.Write(Constructor.TypeToCode(i.GetType())); 
											i.Write(writer);
										});
	}

	public void Read(Reader reader)
	{
		Id = reader.Read7BitEncodedInt();
		Transactions = reader.ReadArray(() =>	{
 										 			var o = Constructor.Construct(typeof(IccTransaction), reader.ReadUInt32()) as IccTransaction;
 										 			o.Read(reader); 
 										 			return o; 
 												});
	}
}
