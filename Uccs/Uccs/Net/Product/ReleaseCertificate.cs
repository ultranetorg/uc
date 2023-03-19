using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class ReleaseCertificate
	{
		public VersionAddress		Release;
		public Distributive	Distribution;
		public List<byte[]>			Approvals; /// Signed Release + Distribution by Analyzer
	}
}
