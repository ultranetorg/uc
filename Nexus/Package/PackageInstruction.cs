using System.Text;

namespace Uccs.Nexus;

public class Start : IBinarySerializable
{
	public string		Path { get; set; }
	public string		Arguments { get; set; }
	public Expression	Condition { get; set; }

	public override string ToString()
	{
		return $"{Path}, {Arguments}";
	}

	public void Read(Reader reader)
	{
		Path		= reader.ReadUtf8();
		Arguments	= reader.ReadUtf8();
		Condition	= reader.ReadNullable<Expression>();
	}
	
	public void Write(Writer writer)
	{
		writer.WriteUtf8(Path);
		writer.WriteUtf8(Arguments);
		writer.WriteNullable(Condition);
	}

	public static Start FromXon(Xon x)
	{
		var m = new Start ();
		m.Path		= x.Get<string>("Path", null);
		m.Arguments	= x.Get<string>("Arguments", null);
		m.Condition	= x.Has("Condition") ? Expression.FromXon(x.One("Condition").Nodes.First()) : null;

		return m;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{					
		var x = new Xon(serializator);
	
		x.Add("Path").Value = Path;

		if(Arguments != null)	x.Add(nameof(Arguments)).Value = Arguments;
		if(Condition != null)	x.Add(nameof(Condition)).Nodes.Add(Condition.ToXon(serializator));

		return x;
	}
}	

public class PackageInstruction : IBinarySerializable
{
	public const string				Extension = ".rdnpi";

	public Start[]					Start { get; set; }

	public Start					MatchExecution(Platform platform) => Start.FirstOrDefault(i => i.Condition.Match(platform));

	public PackageInstruction()
	{
  	}

	public void Write(Writer writer)
	{
		writer.Write(Start);
	}

	public void Read(Reader reader)
	{
		Start					= reader.ReadArray<Start>();
	}

	public Start MatchExecution(Family family)
	{ 
		var p = new Platform {Family = family};
		return Start.FirstOrDefault(i => i.Condition.Match(p)); 
	}

	public static PackageInstruction Parse(string text)
	{
		return FromXon(new Xon(text));
	}

	public static PackageInstruction Load(string filepath)
	{
		return FromXon(new Xon(File.ReadAllText(filepath, Encoding.UTF8)));
	}

	public void Save(string filepath)
	{
		ToXon(new NetXonTextValueSerializator()).Save(filepath);
	}

	public static PackageInstruction FromXon(Xon xon)
	{
		var m = new PackageInstruction();

		m.Start					= xon.Many(nameof(Start)).Select(Uccs.Nexus.Start.FromXon).ToArray();

		return m;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);

		if(Start != null && Start.Any())
			x.Nodes.AddRange(Start.Select(i => {
													var e = i.ToXon(serializator);
													e.Name = nameof(Start);
													return e;
												}));

		return x;
	}
}
