namespace Uccs.Net;

public abstract class IccTransaction : IBinarySerializable  
{
	public string			ToNet { get; set; }
	
	public abstract void	OutgoingExecute(Execution execution);
	public abstract void	IncomingExecute(Execution execution);

	public void Write(BinaryWriter writer)
	{
		writer.WriteASCII(ToNet);
	}

	public void Read(BinaryReader reader)
	{
		ToNet = reader.ReadASCII();
	}
}
// 
// public class IccTransaction : IBinarySerializable
// {
// 	public IccOperation	Operations;
// 
// 	public void Write(BinaryWriter writer)
// 	{
// 	}
// 
// 	public void Read(BinaryReader reader)
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

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(Transactions, i =>	{
											writer.Write(Constructor.TypeToCode(i.GetType())); 
											i.Write(writer);
										});
	}

	public void Read(BinaryReader reader)
	{
		Id = reader.Read7BitEncodedInt();
		Transactions = reader.ReadArray(() =>	{
 										 			var o = Constructor.Construct(typeof(IccTransaction), reader.ReadUInt32()) as IccTransaction;
 										 			o.Read(reader); 
 										 			return o; 
 												});
	}
}
