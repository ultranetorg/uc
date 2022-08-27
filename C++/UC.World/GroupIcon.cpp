#include "stdafx.h"
#include "GroupIcon.h"

using namespace uc;

CGroupIcon::CGroupIcon(CWorld * l, CString const & name) : CIcon(l, l->Server, name)
{
	Level = l;
	SetContentFromImage(Server->MapReleasePath(L"Group-24x24.png"));
}

CGroupIcon::~CGroupIcon()
{
}
