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
		StringBuilder b = null; 

		bool q = false;

		while(R)
		{
			if(!q)
			{
				if(C == ' ' || C == '\t' || C == '\r' || C == '\n' || C == '}' || C == '{')
				{
					if(b != null)
						break;
				}
				else if(b == null && C == '\"') /// opening "
				{
					b = new StringBuilder();
					q = true;
				}
				else
				{
					b ??= new StringBuilder();
					b.Append(C);
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
						b.Append(C);
					else
						break;
				}
				else
					b.Append(C);

			}
			Move();
		}

		Current = XonToken.SimpleValueEnd;

		return b?.ToString();
	}

	public string ParseName()
	{
		bool q = false;
		StringBuilder b = null; 

		while(true)
		{
			if(!q)
			{
				if(!R || C == ' ' || C == '\t' || C == '\r' || C == '\n' || C == '{' || C == '}' || C == '=')
				{
					Current = XonToken.NameEnd;
					return b.ToString();
				}
				else if(b == null && C == '\"') /// opening quotation mark
				{
					q = true;
				}
				else
				{
					b ??= new StringBuilder();
					b.Append(C);
				}
			}
			else
			{
				if(C == '\"') /// closing ' or escaping
				{
					Move();

					if(!R)
						break;
					else if(C == '\"')
						b.Append(C);
					else
					{
						Current = XonToken.NameEnd;
						break;
					}
				}
				else
				{	
					b ??= new StringBuilder();
					b.Append(C);
				}
			}
			Move();
		}

		return b?.ToString();
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
