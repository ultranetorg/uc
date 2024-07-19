using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Rdn
{
	public class ProductManifest
	{
		public enum Operator : byte
		{
			None, Argument, Greater, GreaterOrEqual, Less, LessOrEqual, Equal, Not, Or, And
		}

		public class Expression
		{
			public Operator			Operator;
			public Expression[]		Operands;
			public string			Name;

			public Expression()
			{
			}

			public Expression(Xon xon)
			{
				Parse(xon);
			}

			Expression Parse(Xon xon)
			{
				if(xon.Name == "==")
				{
					var e = new Expression();
					e.Operator = Operator.Equal;
					e.Operands = [Parse(xon.Nodes[0]), Parse(xon.Nodes[1])];
					return e;
				}
				else if(xon.Name == ">")
				{
					var e = new Expression();
					e.Operator = Operator.Greater;
					e.Operands = [Parse(xon.Nodes[0]), Parse(xon.Nodes[1])];
					return e;
				}
				else if(xon.Name == ">=")
				{
					var e = new Expression();
					e.Operator = Operator.GreaterOrEqual;
					e.Operands = [Parse(xon.Nodes[0]), Parse(xon.Nodes[1])];
					return e;
				}
				else if(xon.Name == "<")
				{
					var e = new Expression();
					e.Operator = Operator.Less;
					e.Operands = [Parse(xon.Nodes[0]), Parse(xon.Nodes[1])];
					return e;
				}
				else if(xon.Name == "<=")
				{
					var e = new Expression();
					e.Operator = Operator.LessOrEqual;
					e.Operands = [Parse(xon.Nodes[0]), Parse(xon.Nodes[1])];
					return e;
				}
				else if(xon.Name == "NOT")
				{
					var e = new Expression();
					e.Operator = Operator.Not;
					e.Operands = [Parse(xon.Nodes[0])];
					return e;
				}
				else if(xon.Name == "OR")
				{
					var e = new Expression();
					e.Operator = Operator.Or;
					e.Operands = xon.Nodes.Select(i => new Expression(i)).ToArray();
					return e;
				}
				else if(xon.Name == "AND")
				{
					var e = new Expression();
					e.Operator = Operator.And;
					e.Operands = xon.Nodes.Select(i => new Expression()).ToArray();
					return e;
				}
				else
				{
					var e = new Expression();
					e.Operator = Operator.Argument;
					e.Name = xon.Name;
					return e;
				}
			}

			public object Evaluate(Dictionary<string, object> consts)
			{
				switch(Operator)
				{
					case Operator.Argument:
						return consts[Name];

					case Operator.Not:
						return !(bool)Operands[0].Evaluate(consts);

					case Operator.And:
						return Operands.All(i => (bool)i.Evaluate(consts));

					case Operator.Or:
						return Operands.Any(i => (bool)i.Evaluate(consts));

					case Operator.Equal:
					{	
						var a = Operands[0].Evaluate(consts);
						return Operands.Skip(1).All(i => a.Equals(i.Evaluate(consts)));
					}

					case Operator.Greater:
						return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) > 0;

					case Operator.GreaterOrEqual:
						return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) >= 0;

					case Operator.Less:
						return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) < 0;

					case Operator.LessOrEqual:
						return (Operands[0].Evaluate(consts) as IComparable).CompareTo(Operands[1].Evaluate(consts)) <= 0;
				}

				return false;
			}
		}

		public class Realization
		{
			public string			Name;
			public Expression		Requirement;

			public Realization()
			{
			}

			public Realization(Xon xon)
			{
				Name = xon.Get<string>();
				Requirement = new Expression(xon);
			}
		}

		public Realization[]		Realizations;

		public void Load(string path)
		{
			var x = new XonDocument(File.ReadAllText(path));

			Realizations = x.Many("Realization").Select(i => new Realization(i)).ToArray();
		}
	}
}
