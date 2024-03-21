#include "StdAfx.h"
#include "CommanderIcon.h"

using namespace uc;

CCommanderIcon::CCommanderIcon(CExperimentalLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;

	SetContentFromImage(Server->MapReleasePath(L"Commander-24x24.png"));
}

CCommanderIcon::~CCommanderIcon()
{
}
