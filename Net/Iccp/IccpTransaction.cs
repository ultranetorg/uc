using System.Numerics;

namespace Uccs.Net;

public abstract class IccpTransaction : IBinarySerializable, ITypeCode
{
	public abstract bool	IncomingExecute(Execution execution);
	public abstract void	OutgoingPrelock(Execution execution);
	public abstract void	OutgoingConfirm(Execution execution);
	public abstract void	OutgoingRollback(Execution execution);

	public virtual void Write(Writer writer)
	{
	}

	public virtual void Read(Reader reader)
	{
	}
}

public class AssetTransfer : IccpTransaction
{
	public byte[]		SourceHolder { get; set; }
	public byte[]		SourceAsset { get; set; } /// May define not only a type but also other properties like Expiration for Energy
	public byte[]		DestinationHolder { get; set; }
	public BigInteger	Amount { get; set; }

	public override void Write(Writer writer)
	{
		writer.Write(SourceHolder);
		writer.Write(SourceAsset);
		writer.Write(DestinationHolder);
		writer.Write(Amount);
	}

	public override void Read(Reader reader)
	{
		SourceHolder			= reader.ReadBytes();
		SourceAsset				= reader.ReadBytes();
		DestinationHolder		= reader.ReadBytes();
		Amount					= reader.ReadBigInteger();
	}

	public override bool IncomingExecute(Execution execution)
	{
		return true;
	}

	public override void OutgoingPrelock(Execution execution)
	{
	}

	public override void OutgoingConfirm(Execution execution)
	{
	}

	public override void OutgoingRollback(Execution execution)
	{
	}
}

public class IccpTransfer : IBinarySerializable
{
	public string				From;

	public int					Id { get; set; }
	public IccpTransaction[]	Transactions { get; set; }
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
		Transactions = reader.ReadArrayVirtual<IccpTransaction>();
	}
}
