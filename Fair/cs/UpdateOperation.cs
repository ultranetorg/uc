namespace Uccs.Fair;

public abstract class UpdateOperation : FairOperation
{
	public object				Value { get; set; }

	protected string[]			Strings  => Value as string[];
	protected string			String	 => Value as string;
	protected EntityId			EntityId => Value as EntityId;
	protected AccountAddress	AccountAddress  => Value as AccountAddress;
	protected byte				Byte => (byte)Value;
	protected byte[]			Bytes => Value as byte[];
	protected long				Long => (long)Value;
	protected int				Int	=> (int)Value;
}