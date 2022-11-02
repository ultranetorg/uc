#include "StdAfx.h"
#include "ChartIcon.h"

using namespace uc;

CChartIcon::CChartIcon(CExperimentalLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;

	SetContentFromImage(Server->MapReleasePath(L"Chart-24x24.png"));
}

CChartIcon::~CChartIcon()
{
}
