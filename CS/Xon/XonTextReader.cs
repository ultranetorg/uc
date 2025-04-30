using System.Text;

namespace Uccs;

public class XonTextReader : IXonReader
{
	Stack<bool>				Type = new ();
	public XonToken			Current { get; set; }
	char					C;
	CharEnumerator			Enumerator;
	bool					R;

	public XonTextReader(string s)
	{
		Enumerator = s.GetEnumerator();
	}

	void Move()
	{
		R = Enumerator.MoveNext();
		
		if(R)
			C = Enumerator.Current;
	}

	public XonToken Read(IXonValueSerializator serializator)
	{
		Move();
		
		if(R)
		{
			Type.Push(false);
			Current = XonToken.NodeBegin;
		}
		else
			Current = XonToken.End;

		return Current;
	}

	public XonToken ReadNext()
	{
		Next();

		if(!R)
		{
			Current = XonToken.End;
			return Current;
		}

		switch(Current)
		{
			case XonToken.NodeEnd:
			{
				if(C == '}') // after last child
				{
					bool t = Type.Pop();

					Current = t ? XonToken.AttrValueEnd : XonToken.ChildrenEnd;
					Move();
				}
				else // next child
				{
					Current = XonToken.NodeBegin;
				}
				break;
			}
			case XonToken.NodeBegin:
				Current = XonToken.NameBegin;
				break;

			case XonToken.NameEnd:
				if(C == '=')
				{
					Move();
					Current = XonToken.ValueBegin;
				}
				else if(C == '{')
				{
					Move();
					Type.Push(false);
					Current = XonToken.ChildrenBegin;
				}
				else
					Current = XonToken.NodeEnd;
				break;

			case XonToken.ValueBegin:
				if(C == '{')
				{
					Move();
					Type.Push(true);
					Current = XonToken.AttrValueBegin;
				}
				else
					Current = XonToken.SimpleValueBegin;
				break;

			case XonToken.AttrValueBegin:
				if(C == '}')
				{
					Move();
					Current = XonToken.AttrValueEnd;
				}
				else
				{
					Current = XonToken.NodeBegin;
				}
				break;

			case XonToken.ChildrenBegin:
				if(C == '}')
				{
					Current = XonToken.ChildrenEnd;
					Move();
				}
				else
				{
					Current = XonToken.NodeBegin;
				}
				break;

			case XonToken.ChildrenEnd:
				Current = XonToken.NodeEnd;
				break;

			case XonToken.AttrValueEnd:
			case XonToken.SimpleValueEnd:
				Current = XonToken.ValueEnd;
				break;

			case XonToken.ValueEnd:
			{
				if(C == '{')
				{
					Move();
					Type.Push(false);
					Current = XonToken.ChildrenBegin;
				}
				else
					Current = XonToken.NodeEnd;
				break;
			}
		}

		return Current;
	}

	public object ParseValue()
	{
		StringBuilder value = null; 

		bool q = false;

		while(R)
		{
			if(!q)
			{
				if(C == ' ' || C == '\t' || C == '\r' || C == '\n' || C == '}' || C == '{')
				{
					if(value != null)
						break;
				}
				else if(C == '\"') /// opening "
				{
					value = new StringBuilder();
					q = true;
				}
				else
				{
					if(value == null)
					{
						//found = true;
						value = new StringBuilder();
					}
					value.Append(C);
				}
			}
			else
			{
				if(C == '"') /// closing " or escaping
				{
					Move();
					
					if(!R)
						break;
					else if(C == '"')
					{
						value.Append(C);
					}
					else
						break;
				}
				else
					value.Append(C);

			}
			Move();
		}

		Current = XonToken.SimpleValueEnd;

		return value?.ToString();
	}

	public string ParseName()
	{
		bool q = false;

		var name = new StringBuilder(); 

		while(true)
		{
			if(!q)
			{
				if(!R || C == ' ' || C == '\t' || C == '\r' || C == '\n' || C == '{' || C == '}' || C == '=')
				{
					Current = XonToken.NameEnd;
					return name.ToString();
				}
				else if(C == '\"') // opening '
				{
					q = true;
				}
				else
				{
					name.Append(C);
				}
			}
			else
			{
				if(C == '\"') /// closing ' or escaping
				{
					Move();
					if(C == '\"')
					{
						name.Append(C);
						//c++;
					}
					else
					{
						Current = XonToken.NameEnd;
						break;
					}
				}
				else
					name.Append(C);
			}
			Move();
		}

		return name.ToString();
	}

	public object ParseMeta()
	{
		throw new NotSupportedException();
	}

	private void Next()
	{
		while(R)
		{
			if(C == ' ' || C == '\t' || C == '\r' || C == '\n')
			{
				Move();
			}
			//else if(C == '/')
			//{
			//	Move();
			//	if(R && C == '/')
			//		while(R && C != '\r' && C != '\n')
			//			Move();
			//	else
			//	{
			//		c--;
			//		break;
			//	}
			//}
			else
				break;
		}
	}
}
