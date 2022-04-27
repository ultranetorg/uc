#pragma once

namespace uc
{
	struct CMarket
	{
		CString						Name;
		CMap<CString, CString>		Symbols;
	};

	struct CTradeProvider
	{
		CString											Name;
		CList<CMarket>									Markets;
		CMap<CString, CString>							Intervals;
		CMap<CString, CString>							Styles;

		CString FindSymbolName(CString const & name)
		{
			for(auto i : Markets)
			{
				if(i.Symbols.Contains(name))
					return i.Symbols(name);
			}

			return name;
		}

		virtual ~CTradeProvider(){}
	};
}