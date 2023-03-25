using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs
{
	public class XonTextReader : IXonReader
	{
		//public IXonValueSerializator Serializator;
		public XonToken Current { get; set; }

		string Text;
		Stack<bool> Type = new ();

		private int c;
		private char C { get => Text[c]; }

		public XonTextReader(string s)
		{
			Text = s;
		}

		public XonToken Read(IXonValueSerializator serializator)
		{
			if(Text.Length > 0)
			{
				Type.Push(false);
				Current = XonToken.NodeBegin;
				c = 0;
			}
			else
				Current = XonToken.End;
			return Current;
		}

		public XonToken ReadNext()
		{
			Next(ref c);

			if(c < Text.Length)
			{
				switch(Current)
				{
					case XonToken.NodeEnd:
					{
						if(C == '}') // after last child
						{
							bool t = Type.Pop();

							Current = t ? XonToken.AttrValueEnd : XonToken.ChildrenEnd;
							c++;
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
							c++;
							Current = XonToken.ValueBegin;
						}
						else if(C == '{')
						{
							c++;
							Type.Push(false);
							Current = XonToken.ChildrenBegin;
						}
						else
							Current = XonToken.NodeEnd;
						break;

					case XonToken.ValueBegin:
						if(C == '{')
						{
							c++;
							Type.Push(true);
							Current = XonToken.AttrValueBegin;
						}
						else
							Current = XonToken.SimpleValueBegin;
						break;

					case XonToken.AttrValueBegin:
						if(C == '}')
						{
							c++;
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
							c++;
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
							c++;
							Type.Push(false);
							Current = XonToken.ChildrenBegin;
						}
						else
							Current = XonToken.NodeEnd;
						break;
					}
				}
			}
			else
				Current = XonToken.End;

			return Current;
		}

		public object ParseValue()
		{
			string value = null;

			//bool found = false;
			bool q = false;

			while(c < Text.Length)
			{
				if(!q)
				{
					if(C == ' ' || C == '\t' || C == '\r' || C == '\n' || C == '}' || C == '{')
					{
						if(value != null)
							break;
					}
					else if(C == '\"') // opening "
					{
						value = "";
						//if(!foundsemicolon)
						q = true;
					}
					else
					{
						if(value == null)
						{
							//found = true;
							value = "";
						}
						value += C;
					}
				}
				else
				{
					if(C == '\"') // closing " or escaping
					{
						c++;
						if(c == Text.Length)
							break;
						else if(C == '\"')
						{
							value += C;
							//c++;
						}
						else
							break;
					}
					else
						value += C;

				}
				c++;
			}


			Current = XonToken.SimpleValueEnd;

			return value;
		}

		public string ParseName()
		{
			//bool typefound = false;
			bool q = false;

			string name = null; 

			while(true)
			{
				if(!q)
				{
					if(c == Text.Length || C == ' ' || C == '\t' || C == '\r' || C == '\n' || C == '{' || C == '}' || C == '=')
					{
						Current = XonToken.NameEnd;
						return name;
					}
					else if(C == '\"') // opening '
					{
						//if(!foundsemicolon)
						q = true;
					}
					else
					{
						name += C;
					}
				}
				else
				{
					if(C == '\"') // closing ' or escaping
					{
						c++;
						if(C == '\"')
						{
							name += C;
							//c++;
						}
						else
						{
							Current = XonToken.NameEnd;
							break;
						}
					}
					else
						name += C;
				}
				c++;
			}

			return name;
		}

		public object ParseMeta()
		{
			throw new NotSupportedException();
		}

		private void Next(ref int c)
		{
			while(c < Text.Length)
			{
				if(C == ' ' || C == '\t' || C == '\r' || C == '\n')
				{
					c++;
				}
				else if(C == '/')
				{
					c++;
					if(c < Text.Length && C == '/')
						while(c < Text.Length && C != '\r' && C != '\n')
							c++;
					else
					{
						c--;
						break;
					}
				}
				else
					break;
			}
		}
	}
}
