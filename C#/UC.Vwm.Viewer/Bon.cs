using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UC.Vwm.Viewer
{
	enum EBonHeader : byte
	{
		Null		= 0b00000000,
		Parent		= 0b10000000,
		Last		= 0b01000000,
		HasType		= 0b00100000,
		HasValue	= 0b00010000,
		BigValue	= 0b00001000
	};
}
