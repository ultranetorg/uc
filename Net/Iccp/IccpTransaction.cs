using System.Numerics;

namespace Uccs.Net;

public enum IccpTransactionClass : uint
{
	None, 
	AssetTransfer,
}

public abstract class IccpTransaction : IBinarySerializable, ITypeCode
{
	public abstract bool	IncomingExecute(Execution execution);
	public abstract void	OutgoingPrelock(Execution execution);
	public abstract void	OutgoingConfirm(Execution execution);
	public abstract void	OutgoingRollback(Execution execution);

	public byte[]			Signature { get; set; }

	public virtual void Write(Writer writer)
	{
		writer.Write(Signature);
	}

	public virtual void Read(Reader reader)
	{
		Signature = reader.ReadBytes(Cryptography.SignatureLength);
	}
}

public class AssetTransfer : IccpTransaction
{
	public string		FromNet { get; set; }
	public byte[]		FromEntity { get; set; }
	public byte[]		Asset { get; set; } /// May define not only a type but also other properties like Expiration for Energy
	public BigInteger	Amount { get; set; }
	public string		ToNet { get; set; }
	public byte[]		ToEntity { get; set; }

	public override void Write(Writer writer)
	{
		base.Write(writer);

		writer.WriteASCII(FromNet);
		writer.WriteBytes(FromEntity);
		writer.WriteBytes(Asset);
		writer.Write(Amount);
		writer.WriteASCII(ToNet);
		writer.WriteBytes(ToEntity);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);

		FromNet			= reader.ReadASCII();
		FromEntity		= reader.ReadBytes();
		Asset			= reader.ReadBytes();
		Amount			= reader.ReadBigInteger();
		ToNet			= reader.ReadASCII();
		ToEntity		= reader.ReadBytes();
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

	public byte[] Hashify()
	{
		var s = new Blake2Stream();
		var w = new Writer(s);

		w.WriteASCII(FromNet);
		w.Write(FromEntity);
		w.Write(Asset);
		w.Write(Amount);
		w.WriteASCII(ToNet);
		w.Write(ToEntity);

		return s.Hash;
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
