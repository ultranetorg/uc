namespace Uccs.Net;

public abstract class IccTransaction : IBinarySerializable, ITypeCode
{
	public string			ToNet { get; set; }
	
	public abstract bool	IncomingExecute(Execution execution);
	public abstract void	OutgoingPrelock(Execution execution);
	public abstract void	OutgoingConfirm(Execution execution);
	public abstract void	OutgoingRollback(Execution execution);

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

	public int					Id { get; set; }
	public IccTransaction[]		Transactions { get; set; }
	public byte[]				Hash => _Hash ??= Cryptography.Hash(Transactions);
	byte[]						_Hash;

	public void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.WriteVirtual(Transactions);
	}

	public void Read(Reader reader)
	{
		Id = reader.Read7BitEncodedInt();
		Transactions = reader.ReadArrayVirtual<IccTransaction>();
	}
}
