using System.Numerics;

namespace Uccs.Net;

public class Asset : IBinarySerializable
{
	public string	Name { get; set; }
	public byte[]	Id { get; set; }
	public string	Units { get; set; }

	public static Asset Energy(byte year)
	{
		var a = new Asset();
		a.Name = $"Energy for {Time.FirstYear + year} year";
		a.Id = [0, year];
		a.Units = "Execution Cycles (EC)";

		return a;
	}

	public static Asset Spacetime()
	{
		var a = new Asset();
		a.Name = $"Space-time";
		a.Id = [1];
		a.Units = "Byte-days (BD)";

		return a;
	}

	public override string ToString()
	{
		return $"{Name} ({Units})";
	}

	public void Read(Reader reader)
	{
		Name	= reader.ReadASCII();
		Id		= reader.ReadBytes();
		Units	= reader.ReadASCII();
	}

	public void Write(Writer writer)
	{
		writer.WriteASCII(Name);
		writer.WriteBytes(Id);
		writer.WriteASCII(Units);
	}
}

public class HolderClassesIcca : IccpArgumentation
{
}

public class HolderClassesIccr : IccpResult
{
	public string[]			Classes { get; set; }

	public override void	Read(Reader reader) => Classes = reader.ReadArray(reader.ReadASCII);
	public override void	Write(Writer writer) => writer.Write(Classes, writer.WriteASCII);
}

public class HolderAssetsIcca : IccpArgumentation
{
	public byte[]	Entity { get; set; }

	public override void Read(Reader reader)
	{
		Entity = reader.ReadBytes();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(Entity);
	}
}

public class HolderAssetsIccr : IccpResult
{
	public Asset[]	Assets { get; set; }

	public override void Read(Reader reader) => Assets = reader.ReadArray<Asset>();
	public override void Write(Writer writer) => writer.Write(Assets);
}

public class AssetBalanceIcca : IccpArgumentation
{
	public byte[]	Entity { get; set; }
	public byte[]	Asset { get; set; }

	public override void Read(Reader reader)
	{
		Entity	= reader.ReadBytes();
		Asset	= reader.ReadBytes();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(Entity);
		writer.WriteBytes(Asset);
	}
}

public class AssetBalanceIccr : IccpResult
{
	public BigInteger		Balance {get; set;}

	public override void	Read(Reader reader) => Balance = reader.ReadBigInteger();
	public override void	Write(Writer writer) => writer.Write(Balance);
}

public class AssetTransferIcca : IccpArgumentation
{
	public byte[]		FromEntity { get; set; }
	public string		ToNet { get; set; }
	public byte[]		ToEntity { get; set; }
	public byte[]		Asset { get; set; }
	public BigInteger	Amount { get; set; }

	public override void Read(Reader reader)
	{
		FromEntity	= reader.ReadBytes();
		ToNet		= reader.ReadUtf8();
		ToEntity	= reader.ReadBytes();
		Asset		= reader.ReadBytes();
		Amount		= reader.ReadBigInteger();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(FromEntity);
		writer.WriteUtf8(ToNet);
		writer.WriteBytes(ToEntity);
		writer.WriteBytes(Asset);
		writer.Write(Amount);
	}
}

public class AssetTransferIccr : IccpResult
{
	public byte[]			TransactionId { get; set; }

	public override void	Read(Reader reader) => TransactionId = reader.ReadBytes();
	public override void	Write(Writer writer) => writer.WriteBytes(TransactionId);
}

