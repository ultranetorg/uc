﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs
{
	public interface IXonWriter
	{
		public const string BonHeader = "BON02";

		void Start();
		void Write(Xon s);
		void Finish();
		//void Write(IEnumerable<Xon> items);
	};
}
