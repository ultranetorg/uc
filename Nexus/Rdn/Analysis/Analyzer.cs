namespace Uccs.Rdn;

public class Analyzer
{
	public const int				IdMax = 255;

	public AccountAddress			Account;
	public byte						Id;
	public int						JoinedAt;

  	public void WriteGraphState(Writer writer)
 	{
 		writer.Write(Account);
		writer.Write(Id); /// negative if inactive
		writer.Write7BitEncodedInt(JoinedAt); /// negative if inactive
 	}
 
 	public void ReadGraphState(Reader reader)
 	{
		Account		= reader.ReadAccount();
		Id			= reader.ReadByte();
		JoinedAt	= reader.Read7BitEncodedInt();
	}

	public override string ToString()
	{
		return $"{Id}, Account={Account}, JoinedAt={JoinedAt}";
	}
}
