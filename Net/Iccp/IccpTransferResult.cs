namespace Uccs.Net;

public class IccpTransferResult : IBinarySerializable, IEquatable<IccpTransferResult>, IComparable<IccpTransferResult>
{
	public byte[] Hash { get; set; }
	public bool[] Results { get; set; }

	public int CompareTo(IccpTransferResult other)
	{
		var r = Bytes.Comparer.Compare(Hash, other.Hash);
		
		if(r != 0)
			return r;

		for(int i = 0; i < Results.Length; i++)
		{
			if(Results[i] && !other.Results[i])
				return 1;

			if(!Results[i] && other.Results[i])
				return -1;
		}

		return 0;
	}

	public override bool Equals(object obj)
	{
		return obj is IccpTransferResult c && Equals(obj);
	}

	public bool Equals(IccpTransferResult c)
	{
		return Bytes.Equals(Hash, c.Hash) && Results.SequenceEqual(c.Results);
		
	}

	public override int GetHashCode()
	{
		return Hash[0] << 24 | Hash[1] << 16 | Hash[2] << 8 | (Results[0] ? 1 : 0);
	}

	public void Read(Reader reader)
	{
		Hash	= reader.ReadHash();
		Results	= reader.ReadArray(reader.ReadBoolean);
		
	}

	public void Write(Writer writer)
	{
		writer.Write(Hash);
		writer.Write(Results, writer.Write);
	}
}
