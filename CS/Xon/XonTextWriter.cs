using System.Text;

namespace Uccs;

public class XonTextWriter : StreamWriter, IXonWriter 
{
	public XonTextWriter(Stream s, Encoding encoding) : base(s, encoding)
	{
	}

	public void Start()
	{
		//BaseStream.Write(Encoding.GetPreamble());
	}

	public void Finish()
	{
		Flush();
	}

	public void Write(Xon root)
	{
		var s = new StringBuilder();
	
		foreach(var i in root.Nodes)
		{
			Write(s, i, 0);
		}
		
		Write(s);
	}

	string Quotate(string v, bool isname)
	{
		var s = new StringBuilder();

		var q = v.IndexOfAny(new char[]{' ', '\t', '\r', '\n', '{', '}' }) != -1 || v.IndexOf("//") != -1 || (isname && v.Contains('='));
		var qq = v.IndexOf('\"') != -1;

		if(q || qq)
		{
			s.Append('\"');
		}

		if(qq)
			s.Append(v.Replace("\"", "\"\""));
		else
			s.Append(v);

		if(q || qq)
		{
			s.Append('\"');
		}

		return s.ToString();
	}

	public void Write(StringBuilder s, Xon n, int d)
	{
		string t = new string('\t', d);

		s.Append(t);
		s.Append((n.IsDifferenceDeleted ? '-' : ""));
		s.Append(Quotate(n.Name, true));
		
		///if(IsWriteTypes && n.GetValue() != null)
		///{
		///	s += ":" + n.GetValue().GetTypeName();
		///}
		if(n.Value != null)
		{
			string v = n.String;
			
			if(!string.IsNullOrEmpty(v))
			{
				s.Append(" = "); 
				s.Append(Quotate(v, false));
			}
		}

		s.AppendLine();

		if(n.Nodes.Count() > 0)
		{
			s.Append(t);
			s.AppendLine("{");
			
			foreach(var i in n.Nodes)
			{
				Write(s, i, d+1);
			}
			
			s.Append(t);
			s.AppendLine("}");
		}
	}
}
