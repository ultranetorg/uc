using System.Text;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class PlatformExpression
{
	public string Operator;
	public PlatformExpression[] Operands;

	const string Greater = ">";
	const string GreaterOrEqual = ">=";
	const string Less = "<";
	const string LessOrEqual = "<=";
	const string Equal = "==";
	const string Not = "NOT";
	const string Or = "OR";
	const string And = "AND";

	static bool IsOperation(string name) => name == Greater ||
											name == GreaterOrEqual ||
											name == Less ||
											name == LessOrEqual ||
											name == Equal ||
											name == Not ||
											name == Or ||
											name == And;

	public bool Match(Platform platform) => (bool)Evaluate(new(){   {"F", platform.Family},
																	{"B", platform.Brand},
																	{"V", platform.Version},
																	{"A", platform.Architecture}});

	public PlatformExpression()
	{
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{
		var o = new Xon { Name = Operator };

		if(IsOperation(Operator))
			o.Nodes.AddRange(Operands.Select(i => i.ToXon(serializator)));

		return o;
	}

	public static PlatformExpression FromXon(Xon xon)
	{
		var e = new PlatformExpression();

		e.Operator = xon.Name;

		if(IsOperation(xon.Name))
		{
			e.Operands = xon.Nodes.Select(FromXon).ToArray();
		}

		return e;
		//if(Enum.TryParse<PlatfromOperator>(x.Name, out var o))
		//	e.Operator = o;
		//else
		//	e.Name = x.Name;
		//
		//e.Operands = x.Nodes.Select(FromXon).ToArray();

		//return e;
	}

	public object Evaluate(Dictionary<string, object> consts)
	{
		switch(Operator)
		{
			case Not:
				return !(bool)Operands[0].Evaluate(consts);

			case And:
				return Operands.All(i => (bool)i.Evaluate(consts));

			case Or:
				return Operands.Any(i => (bool)i.Evaluate(consts));

			case Equal:
			{
				var a = Operands[0].Evaluate(consts);
				return Operands.Skip(1).All(i => a.Equals(i.Evaluate(consts)));
			}

			case Greater:
				return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) > 0;

			case GreaterOrEqual:
				return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) >= 0;

			case Less:
				return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) < 0;

			case LessOrEqual:
				return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) <= 0;

			default:
				return consts.TryGetValue(Operator, out var v) ? v : Platform.ParseIdentifier(Operator);
		}
	}
}

public class Realization
{
	public Ura Latest;
	public string Name;
	public PlatformExpression Condition;
	public string Channel;

	public Realization()
	{
	}

	public static Realization FromXon(Xon xon)
	{
		var r = new Realization();

		r.Name = xon.Get<string>();
		r.Latest = xon.One("Latest")?.Get<Ura>();
		r.Channel = xon.Get<string>("Channel");
		r.Condition = xon.Has("Condition") ? PlatformExpression.FromXon(xon.One("Condition").Nodes.First()) : null;

		return r;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);

		x.Name = "Realization";
		x.Value = Name;
		x.Add("Latest").Value = Latest;
		x.Add("Channel").Value = Channel;
		x.Add("Condition").Nodes.Add(Condition.ToXon(serializator));

		return x;
	}
}

public class ProductManifest
{
	public const string Extension = "rdnpm";

	public Realization[] Realizations;
	public string Title;

	public Realization MatchRealization(Platform platform) => Realizations.FirstOrDefault(i => i.Condition.Match(platform));

	public byte[] Raw
	{
		get
		{
			var s = new MemoryStream();

			ToXon(new NetXonTextValueSerializator()).Save(new XonTextWriter(s, Encoding.UTF8));

			return s.ToArray();
		}
	}

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
}
