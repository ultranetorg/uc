using System.Text;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class Realization : IBinarySerializable
{
	public Ura					Latest;
	public string				Name;
	public string				Title;
	public Expression	Condition;
	public string				Channel;

	public Realization()
	{
	}

	public static Realization FromXon(Xon xon)
	{
		var r = new Realization();

		r.Name		= xon.Get<string>();
		r.Title		= xon.Get<string>("Title", null);
		r.Latest	= xon.One("Latest")?.Get<Ura>();
		r.Channel	= xon.Get<string>("Channel");
		r.Condition = xon.Has("Condition") ? Expression.FromXon(xon.One("Condition").Nodes.First()) : null;

		return r;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);

		x.Name = "Realization";
		x.Value = Name;
		x.Add("Title").Value = Title;
		x.Add("Latest").Value = Latest;
		x.Add("Channel").Value = Channel;
		x.Add("Condition").Nodes.Add(Condition.ToXon(serializator));

		return x;
	}

	public void Read(Reader reader)
	{
		Title		= reader.ReadUtf8();
		Latest		= reader.Read<Ura>();
		Channel		= reader.ReadUtf8();
		Condition	= reader.ReadNullable<Expression>();
	}				

	public void Write(Writer writer)
	{
		writer.WriteUtf8(Title);
		writer.Write(Latest);
		writer.WriteUtf8(Channel);
		writer.WriteNullable(Condition);
	}
}

public class ProductManifest : IBinarySerializable
{
	public const string		Extension = "rdnpm";

	public Realization[]	Realizations;
	public string			Title;

	public Realization MatchRealization(Platform platform) => Realizations.FirstOrDefault(i => i.Condition.Match(platform));

	public ProductManifest()
	{
	}

	public static ProductManifest Parse(string text)
	{
		return FromXon(new Xon(text));
	}

	public static ProductManifest Load(string path)
	{
		return FromXon(new Xon(File.ReadAllText(path)));
	}

	public static ProductManifest FromXon(Xon xon)
	{
		var m = new ProductManifest();

		m.Title = xon.Get<string>("Title");
		m.Realizations = xon.Many("Realization").Select(Realization.FromXon).ToArray();

		return m;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);

		x.Add("Title").Value = Title;
		x.Nodes.AddRange(Realizations.Select(i => i.ToXon(serializator)));

		return x;
	}

	public void Write(Writer writer)
	{
		writer.WriteUtf8(Title);
		writer.Write(Realizations);
	}

	public void Read(Reader reader)
	{
		Title = reader.ReadUtf8();
		Realizations = reader.ReadArray<Realization>();
	}
}
