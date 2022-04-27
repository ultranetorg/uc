#include "StdAfx.h"
#include "EarthIcon.h"

using namespace uc;

CEarthIcon::CEarthIcon(CExperimentalLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;

	SetContentFromImage(Server->MapPath(L"Earth-24x24.png"));
}

CEarthIcon::~CEarthIcon()
{
}
