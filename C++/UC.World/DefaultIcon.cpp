#include "stdafx.h"
#include "DefaultIcon.h"

using namespace uc;

CDefaultIcon::CDefaultIcon(CWorldProtocol * l, CString const & name) : CIcon(l, l->Server, name)
{
	SetContentFromMaterial(Level->Materials->GetMaterial(L"1 1 1"));
}

CDefaultIcon::~CDefaultIcon()
{
}

void CDefaultIcon::SetEntity(CUol & e)
{
	Entity = null;
}
