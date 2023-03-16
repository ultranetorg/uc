#include "StdAfx.h"
#include "LinkIcon.h"
#include "LinkProperties.h"

using namespace uc;

CLinkIcon::CLinkIcon(CShellLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;

	UseRectangle();
}
	
CLinkIcon::~CLinkIcon()
{
}

void CLinkIcon::SetEntity(CUol & e)
{
	__super::SetEntity(e);

}

void CLinkIcon::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	if(apply && W > 0 && H > 0 && !Visual->Material && !Level->ImageExtractor->IsRequested(this))
	{
		Level->ImageExtractor->GetIconMaterial(this, Entity->Target, int(W))->Done = [this](auto m){ SetContentFromMaterial(m); };
	}
}
