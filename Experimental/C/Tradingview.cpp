#include "stdafx.h"
#include "Tradingview.h"
#include "TradingviewProvider.h"

using namespace uc;

CTradingview::CTradingview(CExperimentalLevel * l, CString const & name) : CTrade(l, name)
{
	Level = l;

	Provider = l->Tradingview;

	SetDirectories(MapRelative(L""));
}

CTradingview::~CTradingview()
{
	Save();
}
