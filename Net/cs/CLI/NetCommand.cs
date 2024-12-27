namespace Uccs.Net;

public abstract class NetCommand : Command
{
	public class ArgumentType
	{
		public string Name;
		public string Description;
		public string Example;

		public ArgumentType(string name, string description, string example)
		{
			Name = name;
			Description = description;
			Example = example;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public readonly ArgumentType AA			= new ArgumentType("AA",		@"Account address, in hexadecimal form prepended with '0x'",			@"0x0000fffb3f90771533b1739480987cee9f08d754");
	public readonly ArgumentType IP			= new ArgumentType("IP",		@"IP Address",															@"12.34.56.78");
	public readonly ArgumentType INT		= new ArgumentType("INT",		@"Positive integer number",												@"324552");
	public readonly ArgumentType PASSWORD	= new ArgumentType("PASSWORD",	@"Text string of any no. chars and longer than 1 char",					@"MyStrongSecret!@#$%^&*()");
	public readonly ArgumentType PATH		= new ArgumentType("PATH",		@"A text string of the local file system path in its native format",	@" C:\Users\file.txt");
	public readonly ArgumentType PORT		= new ArgumentType("PORT",		@"Port number",															@"3800");
	public readonly ArgumentType PRIVATEKEY	= new ArgumentType("PRIVATEKEY",@"Hexadecimal text string of account private key",						@"949d6fe0c479f8a152bb83935cd01d633540517e84f36f4c45fa17d3db3e4561");
	public readonly ArgumentType TITLE		= new ArgumentType("TITLE",		@"An arbitrary single-line text string longer than 1 char",				@"Ultranet Org ");
	public readonly ArgumentType BY			= new ArgumentType("BY",		@"Byte-Years in form of integer number",								@"123");
	public readonly ArgumentType EC			= new ArgumentType("EC",		@"Execution Cycles in form of integer number",							@"123");
	//public readonly ArgumentType NET		= new ArgumentType("NET",		@"Net name",															@"PublicTest");
	public readonly ArgumentType TEXT		= new ArgumentType("TEXT",		@"Arbitary text, can be multi-line",									@"Hello world!");

	
	protected NetCommand(List<Xon> args, Flow flow) : base(args, flow)
	{
	}

	public AccountAddress GetAccountAddress(string paramenter, bool mandatory = true)
	{
		if(Has(paramenter))
			return AccountAddress.Parse(GetString(paramenter));
		else
			if(mandatory)
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
			else
				return null;
	}

	protected E GetEnum<E>(string paramenter, E def) where E : struct
	{
		var p = One(paramenter);

		if(p != null)
			return Enum.Parse<E>(p.Get<string>());
		else
			return def;
	}

	protected E GetEnum<E>(string paramenter) where E : struct
	{
		var p = One(paramenter);

		if(p != null)
			return Enum.Parse<E>(p.Get<string>());
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}
}
