using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs
{
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

		public void Write(ref string s, Xon n, int d)
		{
			string t = new string('\t', d);

			s += t + (n.IsDifferenceDeleted ? "-" : "") + n.Name;
			
			///if(IsWriteTypes && n.GetValue() != null)
			///{
			///	s += ":" + n.GetValue().GetTypeName();
			///}
			if(n.Value != null)
			{
				string v = n.String;
				
				if(!string.IsNullOrEmpty(v))
				{
					var q = v.IndexOfAny(new char[]{' ', '\t', '\r', '\n', '{', '}' }) != -1 || v.IndexOf("//") != -1;
					var qq = v.IndexOf('\"') != -1;
	
					s += " = ";
					if(q || qq)
					{
						s += "\"";
					}
	
					if(qq)
						v = v.Replace("\"", "\"\"");
	
					s += v;
	
					if(q || qq)
					{
						s += "\"";
					}
				}
			}

			s += NewLine;

			if(n.Nodes.Count() > 0)
			{
				s += t + "{" + NewLine;
				foreach(var i in n.Nodes)
				{
					Write(ref s, i, d+1);
				}
				s += t + "}" + NewLine;
			}
		}
	}
}
