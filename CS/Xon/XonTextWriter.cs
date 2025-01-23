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
		string s = "";
	
		foreach(var i in root.Nodes)
		{
			Write(ref s, i, 0);
		}
		
		Write(s);
	}

	string Quotate(string v, bool isname)
	{
		string s = null;

		var q = v.IndexOfAny(new char[]{' ', '\t', '\r', '\n', '{', '}' }) != -1 || v.IndexOf("//") != -1 || (!isname || v.Contains('='));
		var qq = v.IndexOf('\"') != -1;

		if(q || qq)
		{
			s += '\"';
		}

		if(qq)
			s += v.Replace("\"", "\"\"");
		else
			s += v;

		if(q || qq)
		{
			s += '\"';
		}

		return s;
	}

	public void Write(ref string s, Xon n, int d)
	{
		string t = new string('\t', d);

		s += t + (n.IsDifferenceDeleted ? '-' : "") + Quotate(n.Name, true);
		
		///if(IsWriteTypes && n.GetValue() != null)
		///{
		///	s += ":" + n.GetValue().GetTypeName();
		///}
		if(n.Value != null)
		{
			string v = n.String;
			
			if(!string.IsNullOrEmpty(v))
			{

				s += " = " + Quotate(v, false);
			}
		}

		s += NewLine;

		if(n.Nodes.Count() > 0)
		{
			s += t + '{' + NewLine;
			foreach(var i in n.Nodes)
			{
				Write(ref s, i, d+1);
			}
			s += t + '}' + NewLine;
		}
	}
}
