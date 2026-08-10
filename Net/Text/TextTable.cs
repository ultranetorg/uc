using System.Text;

namespace Uccs.Net;


public class StringId : RawId
{
	public StringId()
	{
	}

	public StringId(byte[] k) : base(k)
	{
	}

	public override string ToString()
	{
		return Encoding.UTF8.GetString(Bytes);
	}
}

public abstract class TextTable<E> : Table<StringId, E> where E : class, ITableEntry<StringId>
{
	public TextTable(Mcv mcv, string name, bool index) : base(mcv, name, index)
	{
	}

	public static StringId GetId(string t)
	{
		var b = Encoding.UTF8.GetBytes(t);

		return new StringId(b);
	}
 }

public class TextExecution<E> : TableExecution<StringId, E> where E : class, ITableEntry<StringId>
{
	public TextExecution(TextTable<E> table, Execution execution) : base(table, execution)
	{
	}
}