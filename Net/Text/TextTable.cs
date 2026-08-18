using System.Text;

namespace Uccs.Net;


public class StringId : RawId
{
	public string	Text => _Text ??= Encoding.UTF8.GetString(Bytes);
	string			_Text;
	
	public StringId()
	{
	}

	public StringId(byte[] k) : base(k)
	{
	}

	public StringId(string t) : base(Encoding.UTF8.GetBytes(t))
	{
		_Text = t;
	}

	public override string ToString()
	{
		return Text;
	}
}

public abstract class TextTable<E> : Table<StringId, E> where E : class, ITableEntry<StringId>
{
	public TextTable(Mcv mcv, string name, bool index) : base(mcv, name, index)
	{
	}
 }

public class TextExecution<E, T> : TableExecution<StringId, E, T> where E : class, ITableEntry<StringId> where T : TextTable<E>
{
	public TextExecution(T table, Execution execution) : base(table, execution)
	{
	}
}