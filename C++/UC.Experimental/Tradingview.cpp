#include "stdafx.h"
#include "Tradingview.h"

using namespace uc;

CTradingview::CTradingview(CExperimentalLevel * l, CString const & name) : CTrade(l, name)
{
	Level = l;

	MarketProvider = l->Tradingview;

	SetDirectories(MapRelative(L""));
}

CTradingview::~CTradingview()
{
	Save();
}
