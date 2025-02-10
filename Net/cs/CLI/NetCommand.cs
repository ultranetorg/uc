namespace Uccs.Net;

public abstract class NetCommand : Command
{
	public class ArgumentType
	{
		public string Name;
		public string Description;
		public string[] Examples;

		public string Example => Examples[0];

		public ArgumentType(string name, string description, string[] example)
		{
			Name = name;
			Description = description;
			Examples = example;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public readonly ArgumentType AA			= new ArgumentType("AA",		@"Account address, in hexadecimal form prepended with '0x'",			["0x0000A5A0591B2BF5085C0DDA2C39C5E478300C68", "0x0001D9867473B229B5F13BF594E0DF9D4F61F8ED", "0x0002C6C15D653236A811A1115A6F08F0BC54D2B7", "0x0003924B07CE25C18667899EFFF15CBF5C75C1EE"]);
	public readonly ArgumentType AAID		= new ArgumentType("AAID",		@"Account address or Id",												["0x0000fffb3f90771533b1739480987cee9f08d754", "123456-789"]);
	public readonly ArgumentType IP			= new ArgumentType("IP",		@"IP Address",															["12.34.56.78"]);
	public readonly ArgumentType INT		= new ArgumentType("INT",		@"Positive integer number",												["324552"]);
	public readonly ArgumentType HEX		= new ArgumentType("HEX",		@"Array od bytes in form of hexadecimal string",						["0105BCE1C336874FBEBE40D2510EC035D0251FE855399EAD76E22BD18E2EBC6E37"]);
	public readonly ArgumentType PASSWORD	= new ArgumentType("PASSWORD",	@"Text string of any no. chars and longer than 1 char",					["MyStrongSecret!@#$%^&*()"]);
	public readonly ArgumentType PATH		= new ArgumentType("PATH",		@"A path to local file or directory in native format",					[@"C:\User\readme.txt", @"C:\User\Admin", @"D:\Documents"]);
	public readonly ArgumentType FILEPATH	= new ArgumentType("FILEPATH",	@"A text string of the local file path in its native format",			[@"C:\User\file", @"D:\image.jpg", @"E:\content.bin", ]);
	public readonly ArgumentType DIRPATH	= new ArgumentType("DIRPATH",	@"A text string of the local directory path in its native format",		[@"C:\Folder"]);
	public readonly ArgumentType PORT		= new ArgumentType("PORT",		@"Port number",															["3800"]);
	public readonly ArgumentType PRIVATEKEY	= new ArgumentType("PRIVATEKEY",@"Hexadecimal text string of account private key",						["f5eb914b0cdf95fb3df9bcf7e3686cb16d351edf772e577dd6658f841f51b848"]);
	public readonly ArgumentType NAME		= new ArgumentType("NAME",		@"An arbitrary single-line string",										["One"]);
	public readonly ArgumentType ST			= new ArgumentType("ST",		@"Space-time in form of byte-years(BY), byte-days(BY), eth.",			["300bd", "500by"]);
	public readonly ArgumentType EC			= new ArgumentType("EC",		@"Execution Cycles in form of integer number",							["1000"]);
	//public readonly ArgumentType NET		= new ArgumentType("NET",		@"Net name",															[@"PublicTest"]);
	public readonly ArgumentType TEXT		= new ArgumentType("TEXT",		@"Arbitary text, can be multi-line",									["Hello world!"]);

	
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

	protected E GetEnum<E>(int index) where E : struct
	{
		try
		{
			return Enum.Parse<E>(Args[index].Name);
		}
		catch(Exception)
		{
			throw new SyntaxException($"Parameter at {index} position is not provided or incorrect");
		}
	}
}
